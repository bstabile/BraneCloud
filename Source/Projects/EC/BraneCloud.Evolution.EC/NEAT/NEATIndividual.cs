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
using System.Linq;
using System.Text;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.NEAT;
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.NEAT
{
    /**
     * NEATIndividual is GeneVectorIndividual with NEATNetwork as pheotype. It
     * contains the genome of the individual and also the nodes (NEATNode) for
     * pheotype. It's the combination of Organism class and Genome class in original
     * code. Most of the mutation and crossover happen in this class.
     * 
     * 
     * <p>
     * <b>Parameters</b><br>
     * <table>
     * <tr>
     * <td valign=top><i>base</i>.<tt>network</tt><br>
     * <font size=-1>Classname, = ec.neat.NEATNetwork</font></td>
     * <td valign=top>Class of network in the individual</td>
     * </tr>
     * </table>
     * 
     * <p>
     * <b>Parameter bases</b><br>
     * <table>
     * <tr>
     * <td valign=top><i>base</i>.<tt>network</tt><br>
     * <td>network in the individual</td>
     * </tr>
     * </table>
     * 
     * <p>
     * <b>Default Base</b><br>
     * neat.individual
     * 
     * @author Ermo Wei and David Freelan
     * 
     */
    public class NEATIndividual : GeneVectorIndividual
    {
        /**
         * Fitness after the adjustment.
         */
        public double AdjustedFitness { get; set; }

        /** The individual's subpecies */
        public NEATSubspecies Subspecies { get; set; }

        /** Number of children this individual may have for next generation. */
        public double ExpectedOffspring { get; set; }

        /** Tells which generation this individual is from. */
        public int Generation { get; set; }

        /** Marker for destruction of inferior individual. */
        public bool Eliminate { get; set; }

        /**
         * Marks the subspecies champion, which is the individual who has the
         * highest fitness with the subspecies.
         */
        public bool Champion { get; set; }

        /**
         * Number of reserved offspring for a population leader. This is used for
         * delta coding.
         */
        public int SuperChampionOffspring { get; set; }

        /** Marks the best individual in current generation of the population. */
        public bool PopChampion { get; set; }

        /** Marks the duplicate child of a champion (for tracking purposes). */
        public bool PopChampionChild { get; set; }

        /** debug variable, highest fitness of champion */
        public double HighFit { get; set; }

        /**
         * When playing in real-time allows knowing the maturity of an individual
         */
        public int TimeAlive { get; set; }

        /**
         * All the node of this individual. Nodes are arranged so that the first
         * part of the nodes are SENSOR nodes.
         */
        public IList<NEATNode> Nodes { get; set; }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            IParameter def = DefaultBase;

            Eliminate = false;
            ExpectedOffspring = 0;
            Generation = 0;
            Subspecies = null;
            Champion = false;
            SuperChampionOffspring = 0;
            Nodes = new List<NEATNode>();
        }

        public override IParameter DefaultBase => NEATDefaults.ParamBase.Push(P_INDIVIDUAL);

        /** Initializes an individual with minimal structure. */
        public override void Reset(IEvolutionState state, int thread)
        {
            base.Reset(state, thread);
        }

        /** Reset the individual with given nodes and genome */
        public void Reset(IList<NEATNode> nodeList, IList<Gene> genes)
        {
            // clone the genome
            genome = new Gene[genes.Count];
            genome = genes.ToArray();

            // must clone the nodes
            Nodes = nodeList;

            // link the genes in new individual with nodes
            for (int i = 0; i < genome.Length; ++i)
            {
                NEATGene gene = (NEATGene) genome[i];
                for (int j = 0; j < Nodes.Count; ++j)
                {
                    if (Nodes[j].NodeId == gene.InNodeId)
                        gene.InNode = Nodes[j];
                    if (Nodes[j].NodeId == gene.OutNodeId)
                        gene.OutNode = Nodes[j];
                }
            }
        }

        public override int GetHashCode()
        {
            int hash = base.GetHashCode();

            // fold in the nodes
            for (int i = 0; i < Nodes.Count; i++)
            {
                hash = hash * 31 + 17 + Nodes[i].GetHashCode();
            }

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (!base.Equals(obj))
                return false;
            NEATIndividual ind = (NEATIndividual) obj;
            int len1 = Nodes.Count;
            int len2 = ind.Nodes.Count;
            if (len1 != len2) return false;
            for (int i = 0; i < len1; i++)
                if (!Nodes[i].Equals(ind.Nodes[i]))
                    return false;
            return true;
        }

        /** Set the born generation of this individual. */
        public void SetGeneration(IEvolutionState state)
        {
            Generation = state.Generation;
        }

        /** Get the upperbound for the node id, used in Initializer. */
        public int GetNodeIdSup()
        {
            return Nodes[Nodes.Count - 1].NodeId + 1;
        }

        /** Get the upperbound for the innovation number, used in Initializer. */
        public int GetGeneInnovationNumberSup()
        {
            return ((NEATGene) genome[genome.Length - 1]).InnovationNumber + 1;
        }

        public override object Clone()
        {
            // Genome *Genome::duplicate(int new_id)
            // we ignore the new_id here as it can be assigned later
            // this clones the genes
            NEATIndividual myobj = (NEATIndividual) base.Clone();

            // must clone the nodes
            myobj.Nodes = new List<NEATNode>();
            for (int i = 0; i < Nodes.Count; ++i)
            {
                NEATNode newNode = (NEATNode) Nodes[i].EmptyClone();
                myobj.Nodes.Add(newNode);
            }

            // link the genes in new individual with nodes
            for (int i = 0; i < myobj.genome.Length; ++i)
            {
                NEATGene gene = (NEATGene) myobj.genome[i];
                for (int j = 0; j < myobj.Nodes.Count; ++j)
                {
                    if (myobj.Nodes[j].NodeId == gene.InNodeId)
                        gene.InNode = myobj.Nodes[j];
                    if (myobj.Nodes[j].NodeId == gene.OutNodeId)
                        gene.OutNode = myobj.Nodes[j];
                }
            }
            return myobj;
        }


        /**
         * This method is used to output a individual that is same as the format in
         * start genome file.
         */
        public override string GenotypeToString()
        {
            StringBuilder s = new StringBuilder();
            int size = genome.Length;
            s.Append(Code.Encode(size));
            for (int i = 0; i < genome.Length; i++)
            {
                s.Append("\n");
                s.Append(genome[i].PrintGeneToString());
            }
            s.Append("\n" + Code.Encode(Nodes.Count));
            for (int i = 0; i < Nodes.Count; ++i)
            {
                s.Append("\n");
                s.Append(Nodes[i].PrintNodeToString());
            }
            return s.ToString();
        }

        /**
         * This method is used to read a gene in start genome from file. It will
         * then calls the corresponding methods to parse genome and nodes
         * respectively.
         */
        protected override void ParseGenotype(IEvolutionState state, StreamReader reader)
        {
            // first parse the genotype, the genes of NEAT
            base.ParseGenotype(state, reader);

            // after read the gene, start to read nodes for network
            ParseNodes(state, reader);

            // go through all the gene (represent edge in network), link with
            for (int i = 0; i < genome.Length; ++i)
            {
                NEATGene neatGene = (NEATGene) genome[i];
                for (int j = 0; j < Nodes.Count; ++j)
                {
                    if (Nodes[j].NodeId == neatGene.InNodeId)
                        neatGene.InNode = Nodes[j];
                    else if (Nodes[j].NodeId == neatGene.OutNodeId)
                        neatGene.OutNode = Nodes[j];
                }
            }
        }

        /**
         * Create the nodes from the reader, and calls readNode method on each node.
         */
        public void ParseNodes(IEvolutionState state, StreamReader reader)
        {
            // read in the next line. The first item is the number of genes
            string str = reader.ReadLine();

            DecodeReturn d = new DecodeReturn(str);
            Code.Decode(d);
            if (d.Type != DecodeReturn.T_INTEGER) // uh oh
                state.Output.Fatal("Individual with nodes:\n" + str
                                   + "\n... does not have an integer at the beginning indicating the node count.");
            int lll = (int) d.L;

            NEATSpecies s = (NEATSpecies) Species;
            for (int i = 0; i < lll; ++i)
            {
                NEATNode node = (NEATNode) s.NodePrototype.EmptyClone();
                node.ReadNode(state, reader);
                Nodes.Add(node);
            }
        }

        /** We append new gene(s) to the current genome */
        public void AddGene(NEATGene[] appendGenes)
        {
            Gene[] newGenome = new Gene[genome.Length + appendGenes.Length];
            Array.Copy(genome, 0, newGenome, 0, genome.Length);
            Array.Copy(appendGenes, 0, newGenome, genome.Length, appendGenes.Length);
           
            Genome = newGenome;
        }

        /** Mutate the weights of the genes */
        public void MutateLinkWeights(IEvolutionState state, int thread, NEATSpecies species, double power, double rate,
            NEATSpecies.MutationType mutationType)
        {
            // Go through all the Genes and perturb their link's weights
            // Signifies the last part of the genome
            double endPart = ((double) genome.Length) * 0.8;

            // Modified power by gene number
            // The power of mutation will rise farther into the genome
            // on the theory that the older genes are more fit since
            // they have stood the test of time
            double powerMod = 1.0;

            double gaussPoint, coldGaussPoint;
            bool severe = state.Random[thread].NextBoolean();



            // Loop on all genes
            for (int i = 0; i < genome.Length; ++i)
            {
                // The following if determines the probabilities of doing cold
                // gaussian
                // mutation, meaning the probability of replacing a link weight with
                // another, entirely random weight. It is meant to bias such
                // mutations
                // to the tail of a genome, because that is where less time-tested
                // genes
                // reside. The gaussPoint and coldGaussPoint represent values above
                // which a random double will signify that kind of mutation.

                NEATGene gene = (NEATGene) genome[i];

                if (severe)
                {
                    gaussPoint = 0.3;
                    coldGaussPoint = 0.1;
                }
                else if (genome.Length >= 10 && i > endPart)
                {
                    // Mutate by modification % of connections
                    gaussPoint = 0.5;
                    // Mutate the rest by replacement % of the time
                    coldGaussPoint = 0.3;
                }
                else
                {
                    // Half the time don't do any cold mutations
                    if (state.Random[thread].NextBoolean())
                    {
                        gaussPoint = 1.0 - rate;
                        coldGaussPoint = 1.0 - rate - 0.1;
                    }
                    else
                    {
                        gaussPoint = 1.0 - rate;
                        coldGaussPoint = 1.0 - rate;
                    }
                }

                double value = (state.Random[thread].NextBoolean() ? 1 : -1) * state.Random[thread].NextDouble() *
                               power * powerMod;

                if (mutationType == NEATSpecies.MutationType.GAUSSIAN)
                {
                    double randomChoice = state.Random[thread].NextDouble();
                    if (randomChoice > gaussPoint)
                        gene.Weight += value;
                    else if (randomChoice > coldGaussPoint)
                        gene.Weight = value;
                }
                else if (mutationType == NEATSpecies.MutationType.COLDGAUSSIAN)
                {
                    gene.Weight = value;
                }

                // Clip the weight at 8.0 (experimental)
                // FIXME : this code only exist in C++ version
                /*
                 * if (gene.weight > 8.0) gene.weight = 8.0; else if (gene.weight <
                 * -8.0) gene.weight = -8.0;
                 */

                // Record the innovation
                gene.MutationNumber = gene.Weight;

            }

        }

        /** Try to add a new gene (link) into the current genome. */
        public void MutateAddLink(IEvolutionState state, int thread)
        {
            // Make attempts to find an unconnected pair
            int tryCount = 0;
            NEATSpecies neatSpecies = (NEATSpecies) Species;
            int newLinkTries = neatSpecies.NewLinkTries;
            // Decide whether to make this recurrent
            bool doRecur = state.Random[thread].NextBoolean(neatSpecies.RecurOnlyProb);

            NEATNode firstNode = null, secondNode = null;

            // Find the first non-sensor so that the to-node won't look at sensor as
            // possible destinations
            int firstNonSensor = -1;
            for (int i = 0; i < Nodes.Count; ++i)
            {
                if (Nodes[i].Type == NEATNode.NodeType.SENSOR)
                {
                    firstNonSensor = i;
                    break;
                }
            }

            // Here is the recurrent finder loop- it is done separately
            bool loopRecur = false;
            int firstNodeIndex = -1;
            int secondNodeIndex = -1;
            bool found = false;
            while (tryCount < newLinkTries)
            {
                if (doRecur)
                {
                    // at this point :
                    // 50% of prob to decide a loop recurrency (node X to node X)
                    // 50% a normal recurrency (node X to node Y)
                    loopRecur = state.Random[thread].NextBoolean();
                    if (loopRecur)
                    {
                        firstNodeIndex = firstNonSensor + state.Random[thread].NextInt(Nodes.Count - firstNonSensor);
                        secondNodeIndex = firstNodeIndex;
                    }
                    else
                    {
                        firstNodeIndex = state.Random[thread].NextInt(Nodes.Count);
                        secondNodeIndex = firstNonSensor + state.Random[thread].NextInt(Nodes.Count - firstNonSensor);
                    }
                }
                else // No recurrency case
                {
                    firstNodeIndex = state.Random[thread].NextInt(Nodes.Count);
                    secondNodeIndex = firstNonSensor + state.Random[thread].NextInt(Nodes.Count - firstNonSensor);
                }

                // grab the nodes
                firstNode = Nodes[firstNodeIndex];
                secondNode = Nodes[secondNodeIndex];

                // Verify is the possible new gene (link) already exist
                bool bypass = false;
                for (int i = 0; i < genome.Length; ++i)
                {
                    NEATGene gene = (NEATGene) genome[i];
                    if (secondNode.Type == NEATNode.NodeType.SENSOR)
                    {
                        bypass = true;
                        break;
                    }
                    if (gene.InNodeId == firstNode.NodeId && gene.OutNodeId == secondNode.NodeId && gene.IsRecurrent
                        && doRecur)
                    {
// already have a recurrent link between these nodes in
                        // recurrent case
                        bypass = true;
                        break;
                    }
                    if (gene.InNodeId == firstNode.NodeId && gene.OutNodeId == secondNode.NodeId && !gene.IsRecurrent
                        && !doRecur)
                    {
// already have a normal link between these nodes in normal
                        // case
                        bypass = true;
                        break;
                    }
                }

                if (!bypass)
                {
                    int threshold = Nodes.Count * Nodes.Count;
                    // we want to add a link from firstNode to secondNode,
                    // we first check if there is a potential link from secondNodde
                    // to firstNode
                    bool[] result = NEATNetwork.HasPath(state, firstNode, secondNode, threshold);

                    // the network contains a
                    if (!result[0])
                    {
                        state.Output.Error("network has infinite loop");
                        return;
                    }
                    // if we want a recur link but added link will not add recur
                    // or if we do not want a recur link but added link will cause a
                    // recur,
                    // we keep trying
                    if ((!result[1] && doRecur) || (result[1] && !doRecur))
                        tryCount++;
                    else
                    {
                        found = true;
                        break;
                    }
                }
                else
                {
                    // if bypass is true, this gene is not good
                    // and skip to next cycle
                    tryCount++;
                }
            }

            if (!found)
                return;

            NEATInnovation testInno = (NEATInnovation) neatSpecies.InnovationPrototype.Clone();
            testInno.Reset(firstNode.NodeId, secondNode.NodeId, doRecur);

            NEATGene[] newGenes = new NEATGene[1];
            if (neatSpecies.HasInnovation(testInno))
            {
                // Grab the existing innovation info
                NEATInnovation innovation = neatSpecies.GetInnovation(testInno);

                // create the gene
                newGenes[0] = (NEATGene) neatSpecies.GenePrototype.Clone();
                newGenes[0].Reset(innovation.NewWeight, firstNode.NodeId, secondNode.NodeId, doRecur,
                    innovation.InnovationNum1, 0);
                newGenes[0].InNode = firstNode;
                newGenes[0].OutNode = secondNode;
            }
            else
            {
                // The innovation is totally novel
                double weight = state.Random[thread].NextBoolean() ? 1 : -1;
                weight *= state.Random[thread].NextDouble();

                newGenes[0] = (NEATGene) neatSpecies.GenePrototype.Clone();
                int currInnovNum = neatSpecies.NextInnovationNumber();
                newGenes[0].Reset(weight, firstNode.NodeId, secondNode.NodeId, doRecur, currInnovNum, weight);
                newGenes[0].InNode = firstNode;
                newGenes[0].OutNode = secondNode;

                // create innovation information
                NEATInnovation newInno = (NEATInnovation) neatSpecies.InnovationPrototype.Clone();
                newInno.Reset(firstNode.NodeId, secondNode.NodeId, currInnovNum, weight, doRecur);
                neatSpecies.AddInnovation(newInno);


            }

            // Now add the new Genes to the genome
            AddGene(newGenes);
        }

        /** Add a new node into this individual. */
        public void MutateAddNode(IEvolutionState state, int thread)
        {
            NEATSpecies neatSpecies = (NEATSpecies) Species;
            NEATGene gene = null;
            int newNodeTries = neatSpecies.NewNodeTries;
            int tryCount = 0;
            bool found = false;
            int i = 0;
            // split next link with a bias towards older links

            if (GenomeLength < neatSpecies.AddNodeMaxGenomeLength)
            {

                bool step2 = false;

                // find the first non enable link whose input is not a bias node
                for (i = 0; i < genome.Length; ++i)
                {
                    gene = (NEATGene) genome[i];
                    if (gene.Enable && gene.InNode.GeneticNodeLabel != NEATNode.NodePlace.BIAS)
                        break;
                }
                // Now randomize which node is chosen at this point
                // We bias the search towards older genes because
                // this encourages splitting to distribute evenly
                for (; i < genome.Length; ++i)
                {
                    gene = (NEATGene) genome[i];
                    if (state.Random[thread].NextBoolean(.7) &&
                        (gene.InNode.GeneticNodeLabel != NEATNode.NodePlace.BIAS))
                    {
                        step2 = true;
                        break;
                    }
                }

                if (step2 && gene.Enable)
                    found = true;
            }
            else
            {
                while ((tryCount < newNodeTries) && (!found))
                {
                    // Pure random split
                    int index = state.Random[thread].NextInt(GenomeLength);
                    gene = (NEATGene) genome[index];
                    if (gene.Enable && gene.InNode.GeneticNodeLabel != NEATNode.NodePlace.BIAS)
                    {
                        found = true;
                    }
                    tryCount++;
                }
            }

            // If we couldn't find anything so say goodbye
            if (!found)
                return;

            // Disable the old gene (link)
            gene.Enable = false;

            // Extract the link
            double oldWeight = gene.Weight;

            // Extract the nodes
            NEATNode inNode = gene.InNode;
            NEATNode outNode = gene.OutNode;

            // Check to see if this innovation has already been done
            // in another genome
            // Innovations are used to make sure the same innovation in
            // two separate genomes in the same generation receives
            // the same innovation number.

            // We check to see if an innovation already occured that was:
            // -A new node
            // -Stuck between the same nodes as were chosen for this mutation
            // -Splitting the same gene as chosen for this mutation
            // If so, we know this mutation is not a novel innovation
            // in this generation
            // so we make it match the original, identical mutation which occured
            // elsewhere in the population by coincidence

            NEATInnovation testInno = (NEATInnovation) neatSpecies.InnovationPrototype.Clone();
            testInno.Reset(inNode.NodeId, outNode.NodeId, gene.InnovationNumber);

            NEATNode newNode = null;
            NEATGene[] newGenes = new NEATGene[2];
            if (neatSpecies.HasInnovation(testInno))
            {

                // Grab the existing innovation info
                NEATInnovation innovation = neatSpecies.GetInnovation(testInno);
                newNode = (NEATNode) neatSpecies.NodePrototype.EmptyClone();
                newNode.Reset(NEATNode.NodeType.NEURON, innovation.NewNodeId, NEATNode.NodePlace.HIDDEN);

                // create the gene
                newGenes[0] = (NEATGene) neatSpecies.GenePrototype.Clone();
                newGenes[0].Reset(1, inNode.NodeId, newNode.NodeId, gene.IsRecurrent, innovation.InnovationNum1, 0);
                newGenes[0].InNode = inNode;
                newGenes[0].OutNode = newNode;

                newGenes[1] = (NEATGene) neatSpecies.GenePrototype.Clone();
                newGenes[1].Reset(oldWeight, newNode.NodeId, outNode.NodeId, false, innovation.InnovationNum2, 0);
                newGenes[1].InNode = newNode;
                newGenes[1].OutNode = outNode;
            }
            else
            {
                // The innovation is totally novel
                // create the new Node

                newNode = (NEATNode) neatSpecies.NodePrototype.EmptyClone();
                newNode.Reset(NEATNode.NodeType.NEURON, neatSpecies.CurrNodeId++, NEATNode.NodePlace.HIDDEN);

                // create the new Gene
                newGenes[0] = (NEATGene) neatSpecies.GenePrototype.Clone();
                int currInnovNum = neatSpecies.NextInnovationNumber();
                int currInnovNum2 = neatSpecies.NextInnovationNumber();
                newGenes[0].Reset(1, inNode.NodeId, newNode.NodeId, gene.IsRecurrent, currInnovNum, 0);
                // link the new gene to node
                newGenes[0].InNode = inNode;
                newGenes[0].OutNode = newNode;


                newGenes[1] = (NEATGene) neatSpecies.GenePrototype.Clone();
                newGenes[1].Reset(oldWeight, newNode.NodeId, outNode.NodeId, false, currInnovNum2, 0);
                newGenes[1].InNode = newNode;
                newGenes[1].OutNode = outNode;



                // create innovation information
                NEATInnovation newInno = (NEATInnovation) neatSpecies.InnovationPrototype.Clone();
                newInno.Reset(inNode.NodeId, outNode.NodeId, currInnovNum, currInnovNum2,
                    newNode.NodeId, gene.InnovationNumber);
                neatSpecies.AddInnovation(newInno);
            }

            // Now add the new Node and New Genes to the genome
            Nodes.Add(newNode);
            AddGene(newGenes);

        }

        /** Randomly enable or disable a gene. */
        public void MutateToggleEnable(IEvolutionState state, int thread, int times)
        {
            for (int i = 0; i < times; ++i)
            {
                // Choose a random gene
                int index = state.Random[thread].NextInt(genome.Length);
                NEATGene gene = (NEATGene) genome[index];

                if (gene.Enable)
                {
                    // We need to make sure that another gene connects out of the
                    // in-node
                    // Because if not a section of network will break off and become
                    // isolated
                    bool found = false;
                    for (int j = 0; j < genome.Length; ++j)
                    {
                        NEATGene anotherGene = (NEATGene) genome[j];
                        if (anotherGene.InNodeId == gene.InNodeId && anotherGene.Enable
                            && anotherGene.InnovationNumber != gene.InnovationNumber)
                        {
                            found = true;
                            break;
                        }
                    }

                    // Disable the gene is it's safe to do so
                    if (found)
                        gene.Enable = false;
                }
                else // Turn on the enable if it's not enable
                    gene.Enable = true;
            }

        }

        /** Reenable a gene if it's disabled. */
        public void MutateGeneReenable()
        {
            for (int i = 0; i < genome.Length; ++i)
            {
                NEATGene gene = (NEATGene) genome[i];
                if (!gene.Enable)
                {
                    gene.Enable = true;
                    break;
                }
            }
        }



        /**
         * Mutation function, determine which mutation is going to proceed with
         * certain probabilities parameters.
         */
        public override void DefaultMutate(IEvolutionState state, int thread)
        {
            NEATSpecies neatSpecies = (NEATSpecies) Species;
            // do the mutation depending on the probabilities of various mutations
            if (state.Random[thread].NextBoolean(neatSpecies.MutateAddNodeProb))
            {

                MutateAddNode(state, thread);
            }
            else if (state.Random[thread].NextBoolean(neatSpecies.MutateAddLinkProb))
            {

                CreateNetwork(); // Make sure we have the network
                MutateAddLink(state, thread);
            }
            else
            {

                if (state.Random[thread].NextBoolean(neatSpecies.MutateLinkWeightsProb))
                {

                    MutateLinkWeights(state, thread, neatSpecies, neatSpecies.WeightMutationPower, 1.0,
                        NEATSpecies.MutationType.GAUSSIAN);
                }
                if (state.Random[thread].NextBoolean(neatSpecies.MutateToggleEnableProb))
                {

                    MutateToggleEnable(state, thread, 1);
                }
                if (state.Random[thread].NextBoolean(neatSpecies.MutateGeneReenableProb))
                {

                    MutateGeneReenable();
                }
            }
        }

        /**
         * Crossover function. Unlike defaultCrossover, this does not do destructive
         * crossover. It will create a new individual as the crossover of the new
         * parents.
         */
        public NEATIndividual Crossover(IEvolutionState state, int thread, NEATIndividual secondParent)
        {

            NEATSpecies neatSpecies = (NEATSpecies) Species;
            NEATIndividual newInd = null;
            if (state.Random[thread].NextBoolean(neatSpecies.MateMultipointProb))
            {
                // mate multipoint

                newInd = MateMultipoint(state, thread, secondParent, false);
            }
            else if (state.Random[thread].NextBoolean((neatSpecies.MateMultipointAvgProb
                                                       / (neatSpecies.MateMultipointAvgProb +
                                                          neatSpecies.MateSinglepointProb))))
            {

                // mate multipoint average
                newInd = MateMultipoint(state, thread, secondParent, true);
            }
            else
            {
                // mate single point

                newInd = MateSinglepoint(state, thread, secondParent);
            }
            // mate_baby = true;
            return newInd;
        }

        //@Deprecated
        /**
         * Crossover a single point from two parents, it's not used in original
         * code, as pop.subpop.X.species.mate-singlepoint-prob will always be 0.
         */
        public NEATIndividual MateSinglepoint(IEvolutionState state, int thread, NEATIndividual secondParent)
        {
            // NOTE : unfinished code, seems the NEAT is not using this mate method
            NEATSpecies neatSpecies = (NEATSpecies) Species;
            IList<NEATNode> newNodes = new List<NEATNode>();
            IList<Gene> newGenes = new List<Gene>();

            int sizeA = GenomeLength, sizeB = secondParent.GenomeLength;
            // make sure genomeA point to the shorter genome
            Gene[] genomeA = sizeA < sizeB ? genome : secondParent.genome;
            Gene[] genomeB = sizeA < sizeB ? secondParent.genome : genome;

            int lengthA = genomeA.Length, lengthB = genomeB.Length;
            int crossPoint = state.Random[thread].NextInt(lengthA);

            NEATGene geneA = null, geneB = null;
            int indexA = 0, indexB = 0;

            bool skip = false; // Default to not skip a Gene
            // Note that we skip when we are on the wrong Genome before
            // crossing

            int geneCounter = 0; // Ready to count to crosspoint

            NEATGene chosenGene = null;

            // Now move through the Genes of each parent until both genomes end
            while (indexA < lengthA || indexB < lengthB)
            {
                // if genomeA is ended, we move pointer of genomeB
                // select genes from genomeB
                if (indexA == lengthA)
                {
                    chosenGene = (NEATGene) genomeB[indexB];
                    indexB++;
                }
                // if genomeB is ended, we move pointer of genomeA
                // select genes from genomeA
                else if (indexB == lengthB)
                {
                    chosenGene = (NEATGene) genomeA[indexA];
                    indexA++;
                }
                else
                {
                    // extract current innovation number
                    int innovA = ((NEATGene) genomeA[indexA]).InnovationNumber;
                    int innovB = ((NEATGene) genomeB[indexB]).InnovationNumber;

                    if (innovA == innovB)
                    {
                        // Pick the chosenGene depending on whether we've crossed
                        // yet
                        if (geneCounter < crossPoint)
                        {
                            chosenGene = (NEATGene) genomeA[indexA];
                        }
                        else if (geneCounter > crossPoint)
                        {
                            chosenGene = (NEATGene) genomeB[indexB];
                        }
                        // We are at the cross point here
                        else
                        {
                            geneA = (NEATGene) genomeA[indexA];
                            geneB = (NEATGene) genomeB[indexB];

                            // set up the average gene
                            NEATGene avgGene = (NEATGene) neatSpecies.GenePrototype.Clone();

                            double weight = (geneA.Weight + geneB.Weight) / 2.0;

                            // Average them into the avgGene
                            int inNodeId = (state.Random[thread].NextBoolean()) ? geneA.InNodeId : geneB.InNodeId;
                            int outNodeId = (state.Random[thread].NextBoolean()) ? geneA.OutNodeId : geneB.OutNodeId;
                            bool isRecurrent = (state.Random[thread].NextBoolean())
                                ? geneA.IsRecurrent
                                : geneB.IsRecurrent;

                            int innovationNumber = geneA.InnovationNumber;
                            double mutationNumber = (geneA.MutationNumber + geneB.MutationNumber) / 2.0;

                            avgGene.Enable = !(!geneA.Enable || !geneB.Enable);

                            avgGene.Reset(weight, inNodeId, outNodeId, isRecurrent, innovationNumber, mutationNumber);

                            chosenGene = avgGene;
                        }

                        indexA++;
                        indexB++;
                        geneCounter++;
                    }
                    else if (innovA < innovB)
                    {
                        if (geneCounter < crossPoint)
                        {
                            chosenGene = (NEATGene) genomeA[indexA];
                            indexA++;
                            geneCounter++;
                        }
                        else
                        {
                            chosenGene = (NEATGene) genomeB[indexB];
                            indexB++;
                        }
                    }
                    else if (innovA > innovB)
                    {
                        indexB++;
                        skip = true; // Special case: we need to skip to the next
                        // iteration
                        // because this Gene is before the crosspoint on the wrong
                        // Genome
                    }
                }

                if (HasGene(newGenes, chosenGene))
                    skip = true;

                // Check to see if the chosenGene conflicts with an already chosen
                // gene
                // i.e. do they represent the same link
                // if the link is not duplicate, add it to the new individual
                if (!skip)
                {
                    // Check for the nodes, add them if not in the baby Genome
                    // already
                    CreateNodeCopyIfMissing(newNodes, chosenGene.InNode);
                    CreateNodeCopyIfMissing(newNodes, chosenGene.OutNode);

                    // clone the chosenGene and add to newGenes
                    NEATGene newGene = (NEATGene) chosenGene.Clone();
                    newGenes.Add(newGene);
                }

                skip = false;
            }

            return null;
        }

        /** Test if a genome has certain gene. */
        public bool HasGene(IList<Gene> genomeList, Gene gene)
        {
            NEATGene neatGene = (NEATGene) gene;
            for (int i = 0; i < genomeList.Count; ++i)
            {
                // original code seems redundant
                NEATGene g = (NEATGene) genomeList[i];
                if (g.InNodeId == neatGene.InNodeId && g.OutNodeId == neatGene.OutNodeId &&
                    g.IsRecurrent == neatGene.IsRecurrent)
                {
                    return true;
                }

            }
            return false;
        }

        /**
         * Create the node if the nodeList do not have that node.The nodes in the
         * nodeList is guarantee in ascending order according to node's nodeId.
         */
        public void CreateNodeCopyIfMissing(IList<NEATNode> nodeList, NEATNode node)
        {
            for (int i = 0; i < nodeList.Count; ++i)
            {
                NEATNode n = nodeList[i];
                // if we find the node with the same node id, we simply return it
                if (node.NodeId == n.NodeId)
                    return;
                // if we find a node with larger node id, we insert the new node
                // into current position
                else if (node.NodeId < n.NodeId)
                {
                    NEATNode newN = (NEATNode) node.EmptyClone();
                    nodeList.Add(newN);
                    return;
                }
            }

            // if we didn't find node in nodeList and it has the highest nodeId
            // we add it to the end of list
            NEATNode newNode = (NEATNode) node.EmptyClone();
            nodeList.Add(newNode);
        }

        /** Doing crossover from two parent at multiple points in the genome. */
        public NEATIndividual MateMultipoint(IEvolutionState state, int thread, NEATIndividual secondParent,
            bool averageFlag)
        {
            NEATSpecies neatSpecies = (NEATSpecies) Species;
            IList<NEATNode> newNodes = new List<NEATNode>();
            IList<Gene> newGenes = new List<Gene>();

            int indexA = 0;
            int indexB = 0;
            Gene[] genomeA = genome;
            Gene[] genomeB = secondParent.genome;
            int lengthA = genomeA.Length;
            int lengthB = genomeB.Length;

            // Figure out which genome is better
            // The worse genome should not be allowed to add extra structural
            // baggage
            // If they are the same, use the smaller one's disjoint and excess genes
            // only


            bool firstFitter = IsSuperiorTo(this, secondParent);

            // Make sure all sensors and outputs are included
            for (int i = 0; i < secondParent.Nodes.Count; ++i)
            {
                NEATNode node = secondParent.Nodes[i];
                if (node.GeneticNodeLabel == NEATNode.NodePlace.INPUT ||
                    node.GeneticNodeLabel == NEATNode.NodePlace.BIAS ||
                    node.GeneticNodeLabel == NEATNode.NodePlace.OUTPUT)
                {
                    CreateNodeCopyIfMissing(newNodes, node);
                }
            }

            NEATGene chosenGene = null;
            while (indexA < lengthA || indexB < lengthB)
            {
                bool skip = false;

                if (indexA >= lengthA)
                {
                    chosenGene = (NEATGene) genomeB[indexB];
                    indexB++;
                    // Skip excess from the worse genome
                    if (firstFitter)
                        skip = true;
                }
                else if (indexB >= lengthB)
                {
                    chosenGene = (NEATGene) genomeA[indexA];
                    indexA++;
                    // Skip excess from worse genome
                    if (!firstFitter)
                        skip = true;
                }
                else
                {
                    NEATGene geneA = (NEATGene) genomeA[indexA];
                    NEATGene geneB = (NEATGene) genomeB[indexB];

                    int innovA = geneA.InnovationNumber;
                    int innovB = geneB.InnovationNumber;

                    if (innovA == innovB)
                    {
                        if (!averageFlag)
                        {
                            chosenGene = state.Random[thread].NextBoolean() ? geneA : geneB;
                            // If one is disabled, the corresponding gene in the
                            // offspring will likely be disabled
                            if (!geneA.Enable || !geneB.Enable)
                            {
                                if (state.Random[thread].NextBoolean(0.75))
                                    chosenGene.Enable = false;
                            }
                        }
                        else
                        {
                            // weight averaged here
                            double weight = (geneA.Weight + geneB.Weight) / 2.0;

                            // Average them into the avgGene
                            int inNodeId = -1, outNodeId = -1;
                            NEATNode inNode = null, outNode = null;
                            if (state.Random[thread].NextBoolean())
                            {
                                inNodeId = geneA.InNodeId;
                                // we direct set the inNode here without clone
                                // this is because we will clone this node
                                // eventually
                                inNode = geneA.InNode;
                            }
                            else
                            {
                                inNodeId = geneB.InNodeId;
                                inNode = geneB.InNode;
                            }
                            if (state.Random[thread].NextBoolean())
                            {
                                outNodeId = geneA.OutNodeId;
                                // we direct set the inNode here without clone
                                // this is because we will clone this node
                                // eventually
                                outNode = geneA.OutNode;
                            }
                            else
                            {
                                outNodeId = geneB.OutNodeId;
                                outNode = geneB.OutNode;
                            }
                            bool isRecurrent = (state.Random[thread].NextBoolean())
                                ? geneA.IsRecurrent
                                : geneB.IsRecurrent;

                            int innovationNumber = geneA.InnovationNumber;
                            double mutationNumber = (geneA.MutationNumber + geneB.MutationNumber) / 2.0;

                            bool enable = true;
                            if (!geneA.Enable || !geneB.Enable)
                                if (state.Random[thread].NextBoolean(0.75))
                                    enable = false;

                            chosenGene = (NEATGene) neatSpecies.GenePrototype.Clone();
                            chosenGene.Reset(weight, inNodeId, outNodeId, isRecurrent, innovationNumber,
                                mutationNumber);
                            chosenGene.Enable = enable;
                            chosenGene.InNode = inNode;
                            chosenGene.OutNode = outNode;
                        }

                        indexA++;
                        indexB++;
                    }
                    else if (innovA < innovB)
                    {
                        chosenGene = (NEATGene) genomeA[indexA];
                        indexA++;
                        // Skip disjoint from worse genome
                        if (!firstFitter)
                            skip = true;
                    }
                    else if (innovA > innovB)
                    {
                        chosenGene = (NEATGene) genomeB[indexB];
                        indexB++;
                        // Skip disjoint from worse genome
                        if (firstFitter)
                            skip = true;
                    }
                }

                // Check to see if the chosengene conflicts with an already chosen
                // gene
                // i.e. do they represent the same link
                if (!skip && HasGene(newGenes, chosenGene))
                    skip = true;

                if (!skip)
                {
                    // Now add the chosenGene to the baby individual
                    CreateNodeCopyIfMissing(newNodes, chosenGene.InNode);
                    CreateNodeCopyIfMissing(newNodes, chosenGene.OutNode);

                    // clone the chosenGene and add to newGenes
                    NEATGene newGene = (NEATGene) chosenGene.Clone();
                    newGenes.Add(newGene);
                }

            }

            // NEATIndividual newInd = (NEATIndividual)
            // neatSpecies.i_prototype.clone();
            // newInd.reset(newNodes, newGenes);

            return (NEATIndividual) neatSpecies.NewIndividual(state, thread, newNodes, newGenes);
        }

        /**
         * Return true if first individual has better fitness than the second
         * individual. If they have the same fitness, look at their genome length
         * whoever is shorter is superior.
         */
        private bool IsSuperiorTo(NEATIndividual first, NEATIndividual second)
        {
            bool firstIsBetter = false;

            if (first.Fitness.BetterThan(second.Fitness))
                firstIsBetter = true;
            else if (second.Fitness.BetterThan(first.Fitness))
                firstIsBetter = false;
            else
            {
                // we compare their genome length if their fitness is equal
                // in this case, the individual with more compact representation
                // are considered as more fit
                firstIsBetter = first.genome.Length < second.genome.Length;
            }

            return firstIsBetter;
        }

        public NEATNetwork CreateNetwork()
        {
            NEATNetwork net = (NEATNetwork) ((NEATSpecies) Species).NetworkPrototype.Clone();
            net.BuildNetwork(this);
            return net;
        }

        /**
         * This method convert the individual in to human readable format. It can be
         * useful in debugging.
         */
        public override string ToString()
        {
            StringBuilder stringBuffer = new StringBuilder();
            stringBuffer.Append("\n GENOME START  ");
            stringBuffer.Append("\n  genes are :" + genome.Length);
            stringBuffer.Append("\n  nodes are :" + Nodes.Count);

            for (int i = 0; i < Nodes.Count; ++i)
            {
                NEATNode node = Nodes[i];
                if (node.GeneticNodeLabel == NEATNode.NodePlace.INPUT)
                    stringBuffer.Append("\n Input ");
                if (node.GeneticNodeLabel == NEATNode.NodePlace.OUTPUT)
                    stringBuffer.Append("\n Output");
                if (node.GeneticNodeLabel == NEATNode.NodePlace.HIDDEN)
                    stringBuffer.Append("\n Hidden");
                if (node.GeneticNodeLabel == NEATNode.NodePlace.BIAS)
                    stringBuffer.Append("\n Bias  ");
                stringBuffer.Append(node.ToString());
            }

            for (int i = 0; i < genome.Length; ++i)
            {
                NEATGene gene = (NEATGene) genome[i];
                stringBuffer.Append(gene.ToString());
            }


            stringBuffer.Append("\n");
            stringBuffer.Append(" GENOME END");
            return stringBuffer.ToString();
        }

    }
}