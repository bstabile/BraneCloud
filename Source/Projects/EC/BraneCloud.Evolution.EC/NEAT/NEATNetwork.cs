/*
 * BraneCloud.Evolution.EC (Evolutionary Computation)
 * Copyright 2011 Bennett R. Stabile (BraneCloud.Evolution.net|com)
 * Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
 *
 * This is an independent conversion from Java to .NET of ...
 *
 * Sean Luke's ECJ project at GMU 
 * (Academic Free License v3.0): 
 * http://www.cs.gmu.edu/~eclab/projects/ecj
 *
 * Radical alteration was required throughout (including structural).
 * The author of ECJ cannot and MUST not be expected to support this fork.
 *
 * If you wish to create yet another fork, please use a different root namespace.
 * BraneCloud is a registered domain that will be used for name/schema resolution.
 */

using System;
using System.Collections.Generic;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.NEAT
{
    /**
     * NEATNetwork is the phenotype of NEATIndividual. It share the same copy of
     * nodes and genes (links) with its corresponding NEATIndividual. This class
     * handles all operations that is critical in evaluation of the individuals.
     * 
     * @author Ermo Wei and David Freelan
     *
     */
    public class NEATNetwork : IPrototype
    {
        public const string P_NETWORK = "network";

        /** constant used for the sigmoid function */
        public const double SIGMOID_SLOPE = 4.924273;

        /** The neat individual we belong to */
        public NEATIndividual Individual { get; set; }

        /** A list of all nodes for this network. */
        public IList<NEATNode> Nodes { get; set; }

        /** A list of input nodes for this network. */
        public IList<NEATNode> Inputs { get; set; }

        /** A list of output nodes for this network. */
        public IList<NEATNode> Outputs { get; set; }

        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            // create the arraylist
            Nodes = new List<NEATNode>();
            Inputs = new List<NEATNode>();
            Outputs = new List<NEATNode>();
        }

        public virtual IParameter DefaultBase => NEATDefaults.ParamBase.Push(P_NETWORK);

        public virtual object Clone()
        {
            NEATNetwork myobj;
            try
            {
                myobj = (NEATNetwork) MemberwiseClone();
                myobj.Nodes = new List<NEATNode>();
                for (int i = 0; i < Nodes.Count; i++)
                    myobj.Nodes.Add((NEATNode) Nodes[i].Clone());
                myobj.Inputs = new List<NEATNode>();
                for (int i = 0; i < Inputs.Count; i++)
                    myobj.Inputs.Add((NEATNode) Inputs[i].Clone());
                myobj.Outputs = new List<NEATNode>();
                for (int i = 0; i < Outputs.Count; i++)
                    myobj.Outputs.Add((NEATNode) Outputs[i].Clone());
            }
            catch (CloneNotSupportedException e)
            {
                throw new InvalidOperationException();
            } // never happens
            return myobj;
        }


        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (this == obj)
                return true;

            NEATNetwork ind = (NEATNetwork) obj;
            //if the nodes or incoming and outgoing are different, they are different networks
            if (ind.Nodes.Count != Nodes.Count || ind.Inputs.Count != Inputs.Count ||
                ind.Outputs.Count != Outputs.Count)
                return false;
            for (int i = 0; i < ind.Nodes.Count; i++)
            {
                if (!ind.Nodes[i].Equals(Nodes[i]))
                    return false;
            }

            for (int i = 0; i < ind.Inputs.Count; i++)
            {
                if (!ind.Inputs[i].Equals(Inputs[i]))
                    return false;
            }

            for (int i = 0; i < ind.Outputs.Count; i++)
            {
                if (!ind.Outputs[i].Equals(Outputs[i]))
                    return false;
            }

            return true;

        }

        //public override int GetHashCode()
        //{
        //    // TODO Make this do something different since Equals in overridden!
        //    return base.GetHashCode();
        //}

        public void Flush()
        {
            foreach (NEATNode node in Nodes)
            {
                node.Flush();
            }
        }

        /**
         * Activates the net such that all outputs are active.
         */
        public void Activate(EvolutionState state)
        {
            // Keep activating until all the outputs have become active
            // (This only happens on the first activation, because after that they
            // are always active)

            bool oneTime = false; // Make sure we at least activate once
            int abortCounter = 0; // Used in case the output is somehow truncated
            // from the network

            // make sure all the output are activated, abortCounter make sure it
            // won't go into infinite loop
            while (OutputOff() || !oneTime)
            {
                abortCounter++;

                if (abortCounter >= ((NEATSpecies) (Individual.Species)).MaxNetworkDepth)
                {
                    state.Output.Fatal("Inputs disconnected from output!");
                }

                // For each node, compute the sum of its incoming activation
                foreach (NEATNode node in Nodes)
                {
                    if (node.Type != NEATNode.NodeType.SENSOR)
                    {
                        node.ActiveSum = 0.0;

                        node.ActiveFlag = false; // This will tell us if it has any
                        // active inputs
                        // For each incoming connection, add the activity from the
                        // connection to the activeSum
                        IList<NEATGene> incomingLinks = node.IncomingGenes;
                        foreach (NEATGene link in incomingLinks)
                        {
                            // Handle possible time delays
                            if (!link.TimeDelay)
                            {
                                double amount = link.Weight * link.InNode.GetActivation();
                                // NOTE: why only set activeFlag to true in here?
                                // need better explanation

                                if (link.InNode.ActiveFlag || link.InNode.Type == NEATNode.NodeType.SENSOR)
                                    node.ActiveFlag = true;
                                node.ActiveSum += amount;
                            }
                            else
                            {
                                double amount = link.Weight * link.InNode.GetTimeDelayActivation();
                                node.ActiveSum += amount;
                            }
                        }
                    }
                }

                // Now activate all the non-sensor nodes off their incoming
                // activation
                foreach (NEATNode node in Nodes)
                {
                    if (node.Type != NEATNode.NodeType.SENSOR)
                    {
                        // Only activate if some active input came in
                        if (node.ActiveFlag)
                        {
                            // Keep a memory of activations for potential time
                            // delayed connections
                            node.PreviousLastActivation = node.LastActivation;
                            node.LastActivation = node.Activation;

                            // Now run the net activation through an activation
                            // function
                            if (node.functionType == NEATNode.FunctionType.SIGMOID)
                            {
                                node.Sigmoid(SIGMOID_SLOPE);
                            }

                            // Increment the activationCount
                            // First activation cannot be from nothing!!
                            node.ActivationCount++;
                        }
                    }
                }

                oneTime = true;
            }

            // NOTE: there is adaptation code here in original code, however, for
            // default settings, it should not be used
            // since it have traits
            // see bool Network::activate()
        }



        /** Add a new input node. */
        public void AddInput(NEATNode node)
        {
            Inputs.Add(node);
        }

        /** Add a new output node. */
        public void AddOutput(NEATNode node)
        {
            Outputs.Add(node);
        }

        /** Takes an array of sensor values and loads it into SENSOR inputs ONLY. */
        public void LoadSensors(double[] vals)
        {
            int counter = 0;
            foreach (NEATNode n in Inputs)
            {
                // only load values into SENSORS (not BIASes)
                if (n.Type == NEATNode.NodeType.SENSOR)
                {
                    n.SensorLoad(vals[counter++]);
                }
            }
        }

        /** Produces an array of activation results, one per output node. */
        public double[] GetOutputResults()
        {
            var results = new double[Outputs.Count];
            for (int i = 0; i < results.Length; i++)
                results[i] = Outputs[i].Activation;
            return results;
        }

        /**
         * This checks a POTENTIAL link between start from fromNode to toNode to use
         * count and threshold to jump out in the case of an infinite loop.
         */
        public static bool[] HasPath(IEvolutionState state, NEATNode toNode, NEATNode fromNode, int threshold)
        {
            var results = new bool[2];
            int level = 0;
            var set = new HashSet<NEATNode>(); // for keeping track of the visiting nodes
            HasPath(state, toNode, fromNode, set, level, threshold, results);
            return results;
        }

        /** The helper function to check if there is a path from fromNode to toNode. */
        public static void HasPath(IEvolutionState state, NEATNode toNode, NEATNode fromNode, HashSet<NEATNode> set,
            int level,
            int threshold, bool[] results)
        {
            if (level > threshold)
            {
                // caught in infinite loop
                results[0] = false;
                results[1] = false;
                return;
            }

            if (toNode.NodeId == fromNode.NodeId)
            {
                results[0] = true;
                results[1] = true;
            }
            else
            {
                // Check back on all links...
                // But skip links that are already recurrent
                // (We want to check back through the forward flow of signals only
                foreach (NEATGene link in toNode.IncomingGenes)
                {
                    if (!link.IsRecurrent)
                    {
                        if (!set.Contains(link.InNode))
                        {
                            set.Add(link.InNode);
                            HasPath(state, link.InNode, fromNode, set, level + 1, threshold, results);
                            if (results[0] && results[1])
                            {
                                return;
                            }
                        }
                    }
                }
                set.Add(toNode);
                results[0] = true;
                results[1] = false;
            }
        }

        /** Check if not all output are active. */
        public bool OutputOff()
        {
            foreach (NEATNode node in Outputs)
            {
                if (node.ActivationCount == 0)
                    return true;
            }
            return false;
        }

        /** Find the maximum number of neurons between an output and an input. */
        public int MaxDepth()
        {
            int maxDepth = 0; // The max depth

            foreach (NEATNode node in Nodes)
            {
                node.InnerLevel = 0;
                node.IsTraversed = false;
            }

            foreach (NEATNode node in Outputs)
            {
                int curDepth = node.Depth(0, this, maxDepth);
                if (curDepth > maxDepth)
                    maxDepth = curDepth;
            }
            return maxDepth;
        }



        /**
         * Create the phenotype (network) from the genotype (genome). One main task
         * of method is to link the incomingGenes for each nodes.
         */
        public void BuildNetwork(NEATIndividual ind)
        {
            Individual = ind;

            ((List<NEATNode>) Nodes).AddRange(Individual.Nodes);

            List<NEATNode> inputList = new List<NEATNode>();
            List<NEATNode> outputList = new List<NEATNode>();

            // NOTE: original code clone the node, thus organism and network each
            // have a node instance
            // but we do not clone it here
            foreach (NEATNode node in Individual.Nodes)
            {
                // we are rebuild the network, we clear all the node incomingGenes
                // as we will rebuild it later
                node.ClearIncoming();
                // Check for input or output designation of node
                if (node.GeneticNodeLabel == NEATNode.NodePlace.INPUT)
                    inputList.Add(node);
                else if (node.GeneticNodeLabel == NEATNode.NodePlace.BIAS)
                    inputList.Add(node);
                else if (node.GeneticNodeLabel == NEATNode.NodePlace.OUTPUT)
                    outputList.Add(node);
            }
            ((List<NEATNode>) Inputs).AddRange(inputList);
            ((List<NEATNode>) Outputs).AddRange(outputList);

            // prepare the incomingGenes for each node
            foreach (Gene g in Individual.genome)
            {
                // only deal with enabled nodes
                NEATGene link = (NEATGene) g;

                if (link.Enable)
                {
                    NEATNode outNode = link.OutNode;

                    outNode.IncomingGenes.Add(link);
                }
            }
        }


    }
}