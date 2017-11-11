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
using System.IO;
using System.Text;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.NEAT
{
    /**
     * NEATNode is the class to represent node in network, it stores status of the
     * node in that network. A Node is either a NEURON or a SENSOR. If it's a
     * sensor, it can be loaded with a value for output. If it's a neuron, it has a
     * list of its incoming input signals. Based on the position of the node in
     * network, we have output, input, bias and hidden nodes. We use INPUT nodes to
     * load inputs, and get output from OUTPUT nodes.
     * 
     * @author Ermo Wei and David Freelan
     */

    [ECConfiguration("ec.neat.NEATNode")]
    public class NEATNode : IPrototype
    {

        /**
         * The type of a node. A node could be a sensor node, where the input get
         * loaded in, or a neuron node, where activation is triggered.
         */
        public enum NodeType
        {
            NEURON,
            SENSOR
        }

        /** The place this node could be. */
        public enum NodePlace
        {
            HIDDEN,
            INPUT,
            OUTPUT,
            BIAS
        }

        /** The activation function is used in for hidden node. */
        public enum FunctionType
        {
            SIGMOID
        }

        public const string P_NODE = "node";

        /** Keeps track of which activation the node is currently in. */
        public int ActivationCount;

        /** Holds the previous step's activation for recurrence. */
        public double LastActivation;

        /**
         * Holds the activation BEFORE the previous step's This is necessary for a
         * special recurrent case when the inNode of a recurrent link is one time
         * step ahead of the outNode. The innode then needs to send from TWO time
         * steps ago.
         */
        public double PreviousLastActivation;

        /**
         * Indicates if the value of current node has been override by method other
         * than network's activation.
         */
        public bool Override;

        /**
         * Contains the activation value that will override this node's activation.
         */
        public double OverrideValue;

        /** When it's true, the node cannot be mutated. */
        public bool Frozen;

        /**
         * The activation function, use sigmoid for default, but can use some other
         * choice, like ReLU.
         */
        public FunctionType functionType;

        /** Distinguish the Sensor node or other neuron node. */
        public NodeType Type;

        /** Distinguish the input node, hidden or output node. */
        public NodePlace GeneticNodeLabel;

        /** The incoming activity before being processed. */
        public double ActiveSum;

        /** The total activation entering the node. */
        public double Activation;

        /** To make sure outputs are active. */
        public bool ActiveFlag;

        /**
         * A list of incoming links, it is used to get activation status of the
         * nodes on the other ends.
         */
        public IList<NEATGene> IncomingGenes;

        /** Node id for this node. */
        public int NodeId;

        /**
         * The depth of current node in current network, this field is used in
         * counting max depth in a network.
         */
        public int InnerLevel;

        /** Indicate if this node has been traversed in max depth counting. */
        public bool IsTraversed;

        public void Setup(IEvolutionState state, IParameter paramBase)
        {
            ActivationCount = 0;
            LastActivation = 0;
            PreviousLastActivation = 0;
            Override = false;
            OverrideValue = 0;
            Frozen = false;
            // TODO : could be extend to use some other activation function
            functionType = FunctionType.SIGMOID;
            Type = NodeType.NEURON;
            GeneticNodeLabel = NodePlace.HIDDEN;
            ActiveSum = 0;
            Activation = 0;
            ActiveFlag = false;
            IncomingGenes = new List<NEATGene>();
            NodeId = 0;
            InnerLevel = 0;
            IsTraversed = false;
        }

        public virtual IParameter DefaultBase => NEATDefaults.ParamBase.Push(P_NODE);


        /** Reset the node to initial status. */
        public void Reset(NodeType nodeType, int id, NodePlace placement)
        {
            // NNode::NNode(nodetype ntype,int nodeid, nodeplace placement)
            NodeId = id;
            ActiveFlag = false;
            ActiveSum = 0;
            Activation = 0;
            LastActivation = 0;
            PreviousLastActivation = 0;
            Type = nodeType; // NEURON or SENSOR type
            ActivationCount = 0; // Inactive upon creation
            functionType = FunctionType.SIGMOID;
            GeneticNodeLabel = placement;
            Frozen = false;
            Override = false;
            OverrideValue = 0;
            InnerLevel = 0;
            IsTraversed = false;
        }

        /**
         * Return a clone of this node, but with a empty incomingGenes list.
         */
        public object EmptyClone()
        {
            NEATNode myobj = (NEATNode) Clone();
            myobj.IncomingGenes = new List<NEATGene>();

            return myobj;
        }

        public virtual object Clone()
        {
            // NNode::NNode(NNode *n,Trait *t)
            NEATNode myobj = null;
            try
            {
                myobj = (NEATNode) MemberwiseClone();

                myobj.NodeId = NodeId;
                myobj.Type = Type;
                myobj.GeneticNodeLabel = GeneticNodeLabel;
                myobj.ActivationCount = 0;
                myobj.LastActivation = 0;
                myobj.PreviousLastActivation = 0;
                myobj.Override = false;
                myobj.OverrideValue = 0;
                myobj.Frozen = false;
                myobj.functionType = FunctionType.SIGMOID;
                myobj.ActiveSum = 0;
                myobj.Activation = 0;
                myobj.ActiveFlag = false;
                myobj.IsTraversed = false;
                myobj.InnerLevel = 0;
            }
            catch (CloneNotSupportedException e) // never happens
            {
                throw new InvalidOperationException();
            }
            return myobj;
        }

        public override bool Equals(Object obj)
        {
            NEATNode n = (NEATNode) obj;
            if (NodeId != n.NodeId)
                return false;

            for (int i = 0; i < IncomingGenes.Count; i++)
            {
                if (!n.IncomingGenes[i].Equals(IncomingGenes[i]))
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int result = NodeId;
            for (int i = 0; i < IncomingGenes.Count; i++)
            {
                // this is probably sufficient
                result = result * 31 + 17 + IncomingGenes[i].GetHashCode();
            }
            return result;
        }

        /**
         * Old flush code, used in C++ version. Put all the field into initial
         * status, this is useful in flushing the whole network.
         */
        public void FlushBack()
        {
            if (Type != NodeType.SENSOR)
            {
                // SENSOR Node do not need to flush recursively
                if (ActivationCount > 0)
                {
                    ActivationCount = 0;
                    Activation = 0;
                    LastActivation = 0;
                    PreviousLastActivation = 0;
                }
                for (int i = 0; i < IncomingGenes.Count; ++i)
                {
                    NEATGene link = IncomingGenes[i];
                    if (link.InNode.ActivationCount > 0)
                    {
                        // NOTE : in here we have the add_weight field clear code
                        // for hebbian learning,
                        // we ignore it here since we are not using it
                        link.InNode.FlushBack();
                    }
                }
            }
            else
            {
                // Flush the SENSOR
                ActivationCount = 0;
                Activation = 0;
                LastActivation = 0;
                PreviousLastActivation = 0;
            }
        }

        /**
         * Put all the field into initial status, this is useful in flushing the
         * whole network.
         */
        public void Flush()
        {
            ActivationCount = 0;
            Activation = 0;
            LastActivation = 0;
            PreviousLastActivation = 0;

            // FIXME: jneat code seems have a lot of redundant here
        }

        /** Return the activation status of this node. */
        public double GetActivation()
        {
            if (ActivationCount > 0)
                return Activation;
            return 0.0;
        }

        /** Return the last step activation if this node is active at last step. */
        public double GetTimeDelayActivation()
        {
            if (ActivationCount > 1)
                return LastActivation;
            return 0.0;
        }

        /** Set activation to the override value and turn off override. */
        public void ActivateWithOverride()
        {
            Activation = OverrideValue;
            Override = false;
        }

        /** Force an output value on the node. */
        public void OverrideOutput(double newOutput)
        {
            OverrideValue = newOutput;
            Override = true;
        }

        /**
         * Clear in incomgin links of this node, this is useful in create a new
         * network from current genotype.
         */
        public void ClearIncoming()
        {
            IncomingGenes.Clear();
        }

        /** Return the depth of this node in the network. */
        public int Depth(int d, NEATNetwork network, int maxDepth)
        {
            if (d > 100)
            {
                // original code use these number in code, need to find a good way
                // to justify these
                return 10;
            }

            // Base case
            if (this.Type == NodeType.SENSOR)
            {
                return d;
            }

            d++;

            // recursion
            int curDepth = 0; // The depth of current node
            for (int i = 0; i < IncomingGenes.Count; ++i)
            {
                NEATNode node = IncomingGenes[i].InNode;
                if (!node.IsTraversed)
                {
                    node.IsTraversed = true;
                    curDepth = node.Depth(d, network, maxDepth);
                    node.InnerLevel = curDepth - d;
                }
                else
                    curDepth = d + node.InnerLevel;

                maxDepth = Math.Max(curDepth, maxDepth);
            }
            return maxDepth;

        }

        /**
         * Reads a Node printed by printNode(...). The default form simply reads a
         * line into a string, and then calls readNodeFromString() on that line.
         */
        public void ReadNode(IEvolutionState state, StreamReader reader)
        {
            // NNode::NNode (const char *argline, std::vector<Trait*> &traits)
            ReadNodeFromString(reader.ReadLine(), state);
        }

        /**
         * This method is used to read a node in start genome from file.
         */
        public void ReadNodeFromString(string str, IEvolutionState state)
        {
            DecodeReturn dr = new DecodeReturn(str);
            Code.Decode(dr);
            NodeId = (int) dr.L;
            Code.Decode(dr);
            int nType = (int) dr.L;
            Code.Decode(dr);
            int nPlace = (int) dr.L;

            var vals = Enum.GetValues(typeof(NodeType));
            Type = (NodeType) vals.GetValue(nType);
            GeneticNodeLabel = (NodePlace) Enum.GetValues(typeof(NodePlace)).GetValue(nPlace);

            Override = false;
            ActiveSum = 0;
            Frozen = false;
        }

        /**
         * This method convert the gene in to human readable format. It can be
         * useful in debugging.
         */
        public override string ToString()
        {
            var sb = new StringBuilder();
            string maskf = " #,##0";
            string mask5 = " #,##0.000";

            if (Type == NEATNode.NodeType.SENSOR)
                sb.Append("\n (Sensor)");
            if (Type == NEATNode.NodeType.NEURON)
                sb.Append("\n (Neuron)");

            sb.Append(NodeId.ToString(maskf));

            sb.Append(" activation count " + ActivationCount.ToString(maskf) + " activation="
                                + Activation.ToString(mask5) + ")");

            return sb.ToString();
        }

        /**
         * This method is used to output a gene that is same as the format in start
         * genome file.
         */
        public string PrintNodeToString()
        {
            var sb = new StringBuilder();

            sb.Append(Code.Encode(NodeId));
            sb.Append(Code.Encode((int) Type));
            sb.Append(Code.Encode((int) GeneticNodeLabel));

            return sb.ToString();
        }

        /** The Sigmoid function. */
        public void Sigmoid(double slope)
        {

            // constant is not used for non shifted steepened
            Activation = 1.0 / (1.0 + Math.Exp(-(slope * ActiveSum)));
        }

        /** If this node is a sensor node, load this node with the given input */
        public bool SensorLoad(double val)
        {
            if (Type == NodeType.SENSOR)
            {
                // Time delay memory
                PreviousLastActivation = LastActivation;
                LastActivation = Activation;

                ActivationCount++;
                Activation = val;
                return true;
            }

            return false;
        }

    }
}