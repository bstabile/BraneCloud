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
using System.Linq;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.NEAT
{
    /**
     * NEATSpecies is a GeneVectorSpecies which implements NEAT algorithm. The class
     * has several important methods. The breedNewPopulation(...) will first use the
     * methods in this class to determined the expected offsprings for each of the
     * subspecies, then call the reproduce of each subspecies to reproduce new
     * individuals. After one individual is created, we call speciate(...) in this
     * class to assign it to a subspecies, this could lead to creation of new
     * subspecies.
     * 
     * <p>
     * NEATSpecies must be used in combination with NEATBreeder, which will call it
     * at appropriate times to reproduce new individuals for next generations. It
     * must also be used in combination with NEATInitializer, which will use it to
     * generate the initial population.
     *
     *
     * 
     * 
     * <p>
     * <b>Parameters</b><br>
     * <table>
     * <tr>
     * <td valign=top><tt><i>base</i>.weight-mut-power</tt><br>
     * <font size=-1>Floating-point value (default is 2.5)</font></td>
     * <td valign=top>Mutation power of the link weights</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.disjoint-coeff</tt><br>
     * <font size=-1>Floating-point value (default is 1.0)</font></td>
     * <td valign=top>Coefficient for disjoint gene in compatibility computation
     * </td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.excess-coeff</tt><br>
     * <font size=-1>Floating-point value (default is 1.0)</font></td>
     * <td valign=top>Coefficient for excess genes in compatibility computation</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.mutdiff-coeff</tt><br>
     * <font size=-1>Floating-point value (default is 0.4)</font></td>
     * <td valign=top>Coefficient for mutational difference genes in compatibility
     * computation</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.compat-thresh</tt><br>
     * <font size=-1>Floating-point value (default is 3.0)</font></td>
     * <td valign=top>Compatible threshold to determine if two individual are
     * compatible</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.age-significance</tt><br>
     * <font size=-1>Floating-point value (default is 1.0)</font></td>
     * <td valign=top>How much does age matter?</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.survival-thresh</tt><br>
     * <font size=-1>Floating-point value (default is 0.2)</font></td>
     * <td valign=top>Percent of ave fitness for survival</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.mutate-only-prob</tt><br>
     * <font size=-1>Floating-point value (default is 0.25)</font></td>
     * <td valign=top>Probability of a non-mating reproduction</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.mutate-link-weight-prob</tt><br>
     * <font size=-1>Floating-point value (default is 0.9)</font></td>
     * <td valign=top>Probability of doing link weight mutate</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.mutate-toggle-enable-prob</tt><br>
     * <font size=-1>Floating-point value (default is 0.0)</font></td>
     * <td valign=top>Probability of changing the enable status of gene</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.mutate-gene-reenable-prob</tt><br>
     * <font size=-1>Floating-point value (default is 0.0)</font></td>
     * <td valign=top>Probability of reenable a disabled gene</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.mutate-add-node-prob</tt><br>
     * <font size=-1>Floating-point value (default is 0.03)</font></td>
     * <td valign=top>Probability of doing add-node mutation</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.mutate-add-link-prob</tt><br>
     * <font size=-1>Floating-point value (default is 0.05)</font></td>
     * <td valign=top>Probability of doing add-link mutation</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.interspecies-mate-prob</tt><br>
     * <font size=-1>Floating-point value (default is 0.001)</font></td>
     r <td valign=top>Probability of doing interspecies crossover</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.mate-multipoint-prob</tt><br>
     * <font size=-1>Floating-point value (default is 0.6)</font></td>
     * <td valign=top>Probability of doing multipoint crossover</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.mate-multipoint-avg-prob</tt><br>
     * <font size=-1>Floating-point value (default is 0.4)</font></td>
     * <td valign=top>Probability of doing multipoint crossover with averaging two
     * genes</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.mate-singlepoint-prob</tt><br>
     * <font size=-1>Floating-point value (default is 0.0)</font></td>
     * <td valign=top>Probability of doing single point crossover (not in used in
     * this implementation, always set to 0)</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.mate-only-prob</tt><br>
     * <font size=-1>Floating-point value (default is 0.2)</font></td>
     * <td valign=top>Probability of mating without mutation</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.recur-only-prob</tt><br>
     * <font size=-1>Floating-point value (default is 0.2)</font></td>
     * <td valign=top>Probability of forcing selection of ONLY links that are
     * naturally recurrent</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.dropoff-age</tt><br>
     * <font size=-1>Integer (default is 15)</font></td>
     * <td valign=top>Age where Species starts to be penalized</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.new-link-tries</tt><br>
     * <font size=-1>Integer (default is 20)</font></td>
     * <td valign=top>Number of tries mutateAddLink will attempt to find an open
     * link</td>
     * </tr>
     * <td valign=top><tt><i>base</i>.new-node-tries</tt><br>
     * <font size=-1>Integer (default is 20)</font></td>
     * <td valign=top>Number of tries mutateAddNode will attempt to build a valid node.
     * </td>
     * </tr>
     * <td valign=top><tt><i>base</i>.add-node-max-genome-length</tt><br>
     * <font size=-1>Integer (default is 15)</font></td>
     * <td valign=top>For genomes this size or larger, mutateAddNode will do a pure random split when adding the node.
     * </td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.babies-stolen</tt><br>
     * <font size=-1>Integer (default is 0)</font></td>
     * <td valign=top>The number of babies to siphen off to the champions</td>
     * </tr>
     * <tr>
     * <td valign=top><i>base</i>.<tt>node</tt><br>
     * <font size=-1>Classname, = ec.neat.NEATNode</font></td>
     * <td valign=top>Class of node in a network</td>
     * </tr>
     * <tr>
     * <td valign=top><i>base</i>.<tt>subspecies</tt><br>
     * <font size=-1>Classname, = ec.neat.NEATSubspecies</font></td>
     * <td valign=top>Class of subspecies in the species</td>
     * </tr>
     * <tr>
     * <td valign=top><i>base</i>.<tt>innovation</tt><br>
     * <font size=-1>Classname, = ec.neat.NEATInnovation</font></td>
     * <td valign=top>Class of innovation in the species</td>
     * </tr>
     * </table>
     * 
     * 
     * 
     * <p>
     * <b>Default Base</b><br>
     * neat.species
     * 
     * <p>
     * <b>Parameter bases</b><br>
     * <table>
     * <tr>
     * <td valign=top><i>base</i>.<tt>species</tt></td>
     * <td>species (the subpopulations' species)</td>
     * </tr>
     *
     *
     * 
     * @author Ermo Wei and David Freelan
     * 
     */
    public class NEATSpecies : GeneVectorSpecies
    {

        public enum MutationType
        {
            GAUSSIAN,
            COLDGAUSSIAN
        }

        // parameters
        public const string P_SPECIES = "species";

        public const string P_NODE = "node";
        public const string P_NETWORK = "network";
        public const string P_SUBSPECIES = "subspecies";
        public const string P_INNOVATION = "innovation";
        public const string P_WEIGHT_MUT_POWER = "weight-mut-power";
        public const string P_DISJOINT_COEFF = "disjoint-coeff";
        public const string P_EXCESS_COEFF = "excess-coeff";
        public const string P_MUT_DIFF_COEFF = "mutdiff-coeff";
        public const string P_COMPAT_THRESH = "compat-thresh";
        public const string P_AGE_SIGNIFICANCE = "age-significance";
        public const string P_SURVIVIAL_THRESH = "survival-thresh";
        public const string P_MUTATE_ONLY_PROB = "mutate-only-prob";
        public const string P_MUTATE_LINK_WEIGHT_PROB = "mutate-link-weight-prob";
        public const string P_MUTATE_TOGGLE_ENABLE_PROB = "mutate-toggle-enable-prob";
        public const string P_MUTATE_GENE_REENABLE_PROB = "mutate-gene-reenable-prob";
        public const string P_MUTATE_ADD_NODE_PROB = "mutate-add-node-prob";
        public const string P_MUTATE_ADD_LINK_PROB = "mutate-add-link-prob";
        public const string P_INTERSPECIES_MATE_PROB = "interspecies-mate-prob";
        public const string P_MATE_MULTIPOINT_PROB = "mate-multipoint-prob";
        public const string P_MATE_MULTIPOINT_AVG_PROB = "mate-multipoint-avg-prob";
        public const string P_MATE_SINGLE_POINT_PROB = "mate-singlepoint-prob";
        public const string P_MATE_ONLY_PROB = "mate-only-prob";
        public const string P_RECUR_ONLY_PROB = "recur-only-prob";
        public const string P_DROPOFF_AGE = "dropoff-age";
        public const string P_NEW_LINK_TRIES = "new-link-tries";
        public const string P_NEW_NODE_TRIES = "new-node-tries";
        public const string P_BABIES_STOLEN = "babies-stolen";
        public const string P_MAX_NETWORK_DEPTH = "max-network-depth";
        public const string P_ADD_NODE_MAX_GENOME_LENGTH = "add-node-max-genome-length";

        /** Current innovation number that is available. */
        private int _currInnovNum;

        /** The prototypical node for individuals in this species. */
        public NEATNode NodePrototype { get; set; }

        /** The prototypical network. */
        public NEATNetwork NetworkPrototype { get; set; }

        /** The prototypical subspecies for individuals in this species. */
        public NEATSubspecies SubspeciesPrototype { get; set; }

        /** The prototypical innovation for individuals in this species. */
        public NEATInnovation InnovationPrototype { get; set; }

        /** Current node id that is available. */
        public int CurrNodeId { get; set; }

        /** Used for delta coding, stagnation detector. */
        public double HighestFitness { get; set; }

        /** Used for delta coding, If too high, leads to delta coding. */
        public int HighestLastChanged { get; set; }


        /** The Mutation power of the link's weights. */
        public double WeightMutationPower { get; set; }

        /** Coefficient for disjoint gene in compatibility computation. */
        public double DisjointCoeff { get; set; }

        /** Coefficient for excess genes in compatibility computation. */
        public double ExcessCoeff { get; set; }

        /**
         * Coefficient for mutational difference genes in compatibility computation.
         */
        public double MutDiffCoeff { get; set; }

        /** Compatible threshold to determine if two individual are compatible. */
        public double CompatThreshold { get; set; }

        /** How much does age matter? */
        public double AgeSignificance { get; set; }

        /** Percent of ave fitness for survival. */
        public double SurvivalThreshold { get; set; }

        /** Probility of a non-mating reproduction. */
        public double MutateOnlyProb { get; set; }

        /** Probability of doing link weight mutate. */
        public double MutateLinkWeightsProb { get; set; }

        /** Probability of changing the enable status of gene. */
        public double MutateToggleEnableProb { get; set; }

        /** Probability of reenable a disabled gene. */
        public double MutateGeneReenableProb { get; set; }

        /** Probability of doing add-node mutation. */
        public double MutateAddNodeProb { get; set; }

        /** Probability of doing add-link mutation. */
        public double MutateAddLinkProb { get; set; }

        /** Probability of doing interspecies crossover. */
        public double InterspeciesMateRate { get; set; }

        /** Probability of doing multipoint crossover. */
        public double MateMultipointProb { get; set; }

        /** Probability of doing multipoint crossover with averaging two genes. */
        public double MateMultipointAvgProb { get; set; }

        /**
         * Probability of doing single point crossover (not in used in this
         * implementation, always set to 0).
         */
        public double MateSinglepointProb { get; set; }

        /** Probability of mating without mutation. */
        public double MateOnlyProb { get; set; }

        /**
         * Probability of forcing selection of ONLY links that are naturally
         * recurrent.
         */
        public double RecurOnlyProb { get; set; }

        /** Age where Species starts to be penalized. */
        public int DropoffAge { get; set; }

        /** Number of tries mutateAddLink will attempt to find an open link. */
        public int NewLinkTries { get; set; }

        /** Number of tries mutateAddNode will attempt to build a new node. */
        public int NewNodeTries { get; set; }

        /** The number of babies to siphen off to the champions. */
        public int BabiesStolen { get; set; }

        /** how deep a node can be in the network, measured by number of parents */
        public int MaxNetworkDepth { get; set; }

        /** Beyond this genome length, mutateAddNode does a pure random split rather than a bias. */
        public int AddNodeMaxGenomeLength { get; set; }

        public IParameter ParamBase { get; set; }

        /** A list of the all the subspecies. */
        public IList<NEATSubspecies> Subspecies { get; set; }

        /** A Hashmap for easy tracking the innovation within species. */
        public IDictionary<NEATInnovation, NEATInnovation> Innovations { get; set; }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            IParameter def = DefaultBase;

            NodePrototype = (NEATNode) state.Parameters.GetInstanceForParameterEq(paramBase.Push(P_NODE),
                def.Push(P_NODE),
                typeof(NEATNode));
            NodePrototype.Setup(state, paramBase.Push(P_NODE));

            SubspeciesPrototype = (NEATSubspecies) state.Parameters.GetInstanceForParameterEq(
                paramBase.Push(P_SUBSPECIES),
                def.Push(P_SUBSPECIES), typeof(NEATSubspecies));
            SubspeciesPrototype.Setup(state, paramBase.Push(P_SUBSPECIES));

            InnovationPrototype = (NEATInnovation) state.Parameters.GetInstanceForParameterEq(
                paramBase.Push(P_INNOVATION),
                def.Push(P_INNOVATION), typeof(NEATInnovation));
            SubspeciesPrototype.Setup(state, paramBase.Push(P_INNOVATION));

            NetworkPrototype = (NEATNetwork) state.Parameters.GetInstanceForParameterEq(paramBase.Push(P_NETWORK),
                def.Push(P_NETWORK), typeof(NEATNetwork));
            NetworkPrototype.Setup(state, paramBase.Push(P_NETWORK));

            // make sure that super.setup is done AFTER we've loaded our gene
            // prototype.
            base.Setup(state, paramBase);

            Subspecies = new List<NEATSubspecies>();
            Innovations = new Dictionary<NEATInnovation, NEATInnovation>();
            HighestFitness = 0;
            HighestLastChanged = 0;



            // Load parameters from the parameter file
            // Load parameters from the parameter file
            WeightMutationPower =
                state.Parameters.GetDouble(paramBase.Push(P_WEIGHT_MUT_POWER), def.Push(P_WEIGHT_MUT_POWER), 2.5);
            DisjointCoeff =
                state.Parameters.GetDouble(paramBase.Push(P_DISJOINT_COEFF), def.Push(P_DISJOINT_COEFF), 1.0);
            ExcessCoeff = state.Parameters.GetDouble(paramBase.Push(P_EXCESS_COEFF), def.Push(P_EXCESS_COEFF), 1.0);
            MutDiffCoeff =
                state.Parameters.GetDouble(paramBase.Push(P_MUT_DIFF_COEFF), def.Push(P_MUT_DIFF_COEFF), 0.4);
            CompatThreshold =
                state.Parameters.GetDouble(paramBase.Push(P_COMPAT_THRESH), def.Push(P_COMPAT_THRESH), 3.0);
            AgeSignificance =
                state.Parameters.GetDouble(paramBase.Push(P_AGE_SIGNIFICANCE), def.Push(P_AGE_SIGNIFICANCE), 1.0);
            SurvivalThreshold =
                state.Parameters.GetDouble(paramBase.Push(P_SURVIVIAL_THRESH), def.Push(P_SURVIVIAL_THRESH));
            MutateOnlyProb = BoundProbabilityParameter(state, paramBase, P_MUTATE_ONLY_PROB, "Mutate only probability");
            MutateLinkWeightsProb =
                BoundProbabilityParameter(state, paramBase, P_MUTATE_LINK_WEIGHT_PROB,
                    "Mutate Link Weight probability");
            MutateToggleEnableProb =
                BoundProbabilityParameter(state, paramBase, P_MUTATE_TOGGLE_ENABLE_PROB,
                    "Mutate Toggle Enable probability");
            MutateGeneReenableProb =
                BoundProbabilityParameter(state, paramBase, P_MUTATE_GENE_REENABLE_PROB, "Mutate Gene Reenable");
            MutateAddNodeProb =
                BoundProbabilityParameter(state, paramBase, P_MUTATE_ADD_NODE_PROB, "Mutate Add Node probability");
            MutateAddLinkProb =
                BoundProbabilityParameter(state, paramBase, P_MUTATE_ADD_LINK_PROB, "Mutate Add Link probability");
            InterspeciesMateRate =
                BoundProbabilityParameter(state, paramBase, P_INTERSPECIES_MATE_PROB, "Interspecies Mate probability");
            MateMultipointProb =
                BoundProbabilityParameter(state, paramBase, P_MATE_MULTIPOINT_PROB, "Mate Multipoint probability");
            MateMultipointAvgProb = BoundProbabilityParameter(state, paramBase, P_MATE_MULTIPOINT_AVG_PROB,
                "Mate Multipoint Average probability");
            MateSinglepointProb =
                BoundProbabilityParameter(state, paramBase, P_MATE_SINGLE_POINT_PROB, "Single Point probability");
            MateOnlyProb = BoundProbabilityParameter(state, paramBase, P_MATE_ONLY_PROB, "Mate Only probability");
            RecurOnlyProb =
                BoundProbabilityParameter(state, paramBase, P_RECUR_ONLY_PROB, "Recurrent Only probability");
            DropoffAge = state.Parameters.GetInt(paramBase.Push(P_DROPOFF_AGE), def.Push(P_DROPOFF_AGE), 0);
            NewLinkTries = state.Parameters.GetInt(paramBase.Push(P_NEW_LINK_TRIES), def.Push(P_NEW_LINK_TRIES), 1);
            NewNodeTries = state.Parameters.GetInt(paramBase.Push(P_NEW_NODE_TRIES), def.Push(P_NEW_NODE_TRIES), 1);
            BabiesStolen = state.Parameters.GetInt(paramBase.Push(P_BABIES_STOLEN), def.Push(P_BABIES_STOLEN), 0);
            MaxNetworkDepth = state.Parameters.GetInt(paramBase.Push(P_MAX_NETWORK_DEPTH),
                paramBase.Push(P_MAX_NETWORK_DEPTH), 30);
            AddNodeMaxGenomeLength = state.Parameters.GetInt(paramBase.Push(P_ADD_NODE_MAX_GENOME_LENGTH),
                paramBase.Push(P_ADD_NODE_MAX_GENOME_LENGTH), 15);
        }

        double BoundProbabilityParameter(IEvolutionState state, IParameter paramBase, string param, string description)
        {
            IParameter def = DefaultBase;
            double probability = state.Parameters.GetDoubleWithMax(paramBase.Push(param), def.Push(param), 0.0, 1.0);
            if (probability < 0.0)
                state.Output.Fatal(description + " is a probability, and must be a value between 0.0 and 1.0.");
            return probability;
        }

        public override IParameter DefaultBase => NEATDefaults.ParamBase.Push(P_SPECIES);



        private readonly object _innoLock = new object();

        public int NextInnovationNumber()
        {
            lock (_innoLock)
            {
                return _currInnovNum++;
            }
        }

        public void SetInnovationNumber(int num)
        {
            lock (_innoLock)
            {
                _currInnovNum = num;
            }
        }

        /** Assign the individual into a species, if not found, create a new one */
        public void Speciate(IEvolutionState state, Individual ind)
        {

            NEATIndividual neatInd = (NEATIndividual) ind;
            // For each individual, search for a subspecies it is compatible to
            if (Subspecies.Count == 0) // not subspecies available, create the
                // first species
            {
                NEATSubspecies newSubspecies = (NEATSubspecies) SubspeciesPrototype.EmptyClone();
                newSubspecies.Reset();
                Subspecies.Add(newSubspecies);
                newSubspecies.AddNewGenIndividual(neatInd);
            }
            else
            {
                bool found = false;
                foreach (NEATSubspecies s in Subspecies)
                {
                    NEATIndividual represent = (NEATIndividual) s.NewGenerationFirst();
                    if (represent == null)
                        represent = (NEATIndividual) s.First();

                    // found compatible subspecies, add this individual to it
                    if (Compatibility(neatInd, represent) < CompatThreshold)
                    {

                        s.AddNewGenIndividual(neatInd);
                        found = true; // change flag
                        break; // search is over, quit loop
                    }
                }
                // if we didn't find a match, create a new subspecies
                if (!found)
                {
                    NEATSubspecies newSubspecies = (NEATSubspecies) SubspeciesPrototype.EmptyClone();
                    newSubspecies.Reset();
                    Subspecies.Add(newSubspecies);
                    newSubspecies.AddNewGenIndividual(neatInd);
                }
            }


        }

        /** Spawn a new individual with given individual as template. */
        public NEATIndividual SpawnWithTemplate(IEvolutionState state, NEATSpecies species, int thread, NEATIndividual ind)
        {
            // we clone but do not reset the individual, since these individuals are
            // made from template
            NEATIndividual newInd = (NEATIndividual) ind.Clone();
            // for first generation of population, we do not use the weight mutation
            // power from the file
            newInd.MutateLinkWeights(state, thread, species, 1.0, 1.0, NEATSpecies.MutationType.GAUSSIAN);
            newInd.SetGeneration(state);
            newInd.CreateNetwork(); // we create the network after we have the
            // complete genome
            return newInd;
        }

        /**
         * This function gives a measure of compatibility between two Genomes by
         * computing a linear combination of 3 characterizing variables of their
         * compatibilty. The 3 variables represent PERCENT DISJOINT GENES, PERCENT
         * EXCESS GENES, MUTATIONAL DIFFERENCE WITHIN MATCHING GENES. So the formula
         * for compatibility is:
         * disjointCoeff*numDisjoint+excessCoeff*numExcess+mutdiffCoeff*numMatching.
         */
        public double Compatibility(NEATIndividual a, NEATIndividual b)
        {


            int numExcess = 0;
            int numMatching = 0;
            int numDisjoint = 0;
            double mutTotalDiff = 0.0;
            // pointer for two genome
            int i = 0, j = 0;
            while (!(i == a.genome.Length && j == b.genome.Length))
            {
                // if genome a is already finished, move b's pointer
                if (i == a.genome.Length)
                {
                    j++;
                    numExcess++;
                }
                // if genome b is already finished, move a's pointer
                else if (j == b.genome.Length)
                {
                    i++;
                    numExcess++;
                }
                else
                {
                    int aInno = ((NEATGene) a.genome[i]).InnovationNumber;
                    int bInno = ((NEATGene) b.genome[j]).InnovationNumber;
                    if (aInno == bInno)
                    {
                        numMatching++;
                        double mutDiff = Math.Abs(((NEATGene) a.genome[i]).MutationNumber -
                                                  ((NEATGene) b.genome[j]).MutationNumber);
                        mutTotalDiff += mutDiff;
                        i++;
                        j++;
                    }
                    // innovation number do not match, skip this one
                    else if (aInno < bInno)
                    {
                        i++;
                        numDisjoint++;
                    }
                    else if (bInno < aInno)
                    {
                        j++;
                        numDisjoint++;
                    }
                }
            }

            // Return the compatibility number using compatibility formula
            // Note that mutTotalDiff/numMatching gives the AVERAGE
            // difference between mutationNums for any two matching Genes
            // in the Genome

            // We do not normalize the terms in here due to the following reason

            // If you decide to use the species compatibility coefficients and
            // thresholds from my own .ne settings files (provided with my NEAT
            // release), then do not normalize the terms in the compatibility
            // function, because I did not do this with my .ne files. In other
            // words, even though my papers suggest normalizing (dividing my number
            // of genes), since I didn't do that the coefficients that I used will
            // not work the same for you if you normalize. If you strongly desire to
            // normalize, you will need to find your own appropriate coefficients
            // and threshold.

            // see the comments above on NEAT page
            // https://www.cs.ucf.edu/~kstanley/neat.html

            // Normalizing for genome size
            // return (disjointCoeff*(numDisjoint/maxGenomeSize)+
            // excessCoeff*(numExcess/maxGenomeSize)+
            // mutDiffCoeff*(mutTotalDiff/numMatching));

            double compatibility = DisjointCoeff * (((double) numDisjoint) / 1.0);
            compatibility += ExcessCoeff * (((double) numExcess) / 1.0);
            compatibility += MutDiffCoeff * (mutTotalDiff / ((double) numMatching));



            return compatibility;
        }

        /** Determine the offsprings for all the subspecies. */
        public void CountOffspring(IEvolutionState state, int subpop)
        {
            // Go through the organisms and add up their adjusted fitnesses to
            // compute the overall average
            double total = 0.0;
            IList<Individual> inds = state.Population.Subpops[subpop].Individuals;
            foreach (Individual s in inds)
            {
                total += ((NEATIndividual) s).AdjustedFitness;
            }

            double overallAverage = total / inds.Count;

            // Now compute expected number of offspring for each individual organism
            foreach (Individual i in inds)
            {
                ((NEATIndividual) i).ExpectedOffspring = ((NEATIndividual) i).AdjustedFitness
                                                         / overallAverage;
            }

            // Now add those offsprings up within each Subspecies to get the number
            // of
            // offspring per subspecies
            double skim = 0.0;
            int totalExpected = 0;
            foreach (NEATSubspecies s in Subspecies)
            {
                skim = s.CountOffspring(skim);
                totalExpected += s.ExpectedOffspring;
            }



            // Need to make up for lost floating point precision in offspring
            // assignment. If we lost precision, give an extra baby to the best
            // subpecies
            if (totalExpected < inds.Count)
            {
                // Find the subspecies expecting the most
                int maxExpected = 0;
                int finalExpected = 0;
                NEATSubspecies best = null;
                foreach (NEATSubspecies s in Subspecies)
                {
                    if (s.ExpectedOffspring >= maxExpected)
                    {
                        maxExpected = s.ExpectedOffspring;
                        best = s;
                    }
                    finalExpected += s.ExpectedOffspring;
                }

                // Give the extra offspring to the best subspecies
                best.ExpectedOffspring++;
                finalExpected++;

                // If we still aren't at total, there is a problem
                // Note that this can happen if a stagnant subpecies
                // dominates the population and then gets killed off by its age
                // Then the whole population plummets in fitness
                // If the average fitness is allowed to hit 0, then we no longer
                // have an average we can use to assign offspring.
                if (finalExpected < inds.Count)
                {
                    state.Output.WarnOnce("Population has died");
                    foreach (NEATSubspecies s in Subspecies)
                    {
                        s.ExpectedOffspring = 0;
                    }
                    best.ExpectedOffspring = inds.Count;
                }
            }
        }

        /**
         * Breed a new generation of population, this is done by first figure the
         * expected offsprings for each subspecies, and then calls each subspecies
         * to reproduce.
         */
        public void BreedNewPopulation(IEvolutionState state, int subpop, int thread)
        {
            // see epoch method in Population
            IList<Individual> inds = state.Population.Subpops[subpop].Individuals;

            ClearEvaluationFlag(inds);

            // clear the innovation information of last generation
            Innovations.Clear();

            // we also ignore the code for competitive coevolution stagnation
            // detection

            // Use Species' ages to modify the objective fitness of organisms
            // in other words, make it more fair for younger species
            // so they have a chance to take hold
            // Also penalize stagnant species
            // Then adjust the fitness using the species size to "share" fitness
            // within a species.
            // Then, within each Species, mark for death
            // those below survivalThresh * average
            foreach (NEATSubspecies s in Subspecies)
            {
                s.AdjustFitness(state, DropoffAge, AgeSignificance);
                s.SortIndividuals();
                s.UpdateSubspeciesMaxFitness();
                s.MarkReproducableIndividuals(SurvivalThreshold);
            }

            // count the offspring for each subspecies
            CountOffspring(state, subpop);

            // sort the subspecies use extra list based on the max fitness
            // these need to use original fitness, descending order

            // BRS: Using extension methods.
            IList<NEATSubspecies> sortedSubspecies = Subspecies.ToList();
            sortedSubspecies.SortByFitnessDescending();


            // Check for population-level stagnation code
            PopulationStagnation(state, subpop, sortedSubspecies);

            // Check for stagnation if there is stagnation, perform delta-coding
            // TODO: fix weird constant
            if (HighestLastChanged >= DropoffAge + 5)
            {
                DeltaCoding(state, subpop, sortedSubspecies);
            }
            // STOLEN BABIES: The system can take expected offspring away from
            // worse species and give them to superior species depending on
            // the system parameter babies_stolen (when babies_stolen > 0)
            else if (BabiesStolen > 0)
            {
                StealBabies(state, thread, subpop, sortedSubspecies);
            }

            // Kill off all Individual marked for death. The remainder
            // will be allowed to reproduce.
            // NOTE this result the size change of individuals in each subspecies
            // however, it doesn't effect the individuals for the whole neat
            // population
            foreach (NEATSubspecies s in sortedSubspecies)
            {
                s.RemovePoorFitnessIndividuals();
            }

            // Reproduction
            // Perform reproduction. Reproduction is done on a per-Species
            // basis. (So this could be paralellized potentially.)
            // we do this with sortedSubspecies instead of subspecies
            // this is due to the fact that new subspecies could be created during
            // the reproduction period
            // thus, the sortedSubspecies are guarantee to contain all the old
            // subspecies
            foreach (NEATSubspecies s in sortedSubspecies)
            {
                s.NewGenIndividuals.Clear();
            }

            foreach (NEATSubspecies s in sortedSubspecies)
            {
                s.Reproduce(state, thread, subpop, sortedSubspecies);
            }

            // Remove all empty subspecies and age ones that survive
            // As this happens, create master individuals list for the new
            // generation

            // first age the old subspecies
            foreach (NEATSubspecies s in sortedSubspecies)
            {
                s.Age++;
            }
            IList<NEATSubspecies> remainSubspecies = new List<NEATSubspecies>();
            IList<Individual> newGenIndividuals = new List<Individual>();
            foreach (NEATSubspecies s in Subspecies)
            {
                if (s.HasNewGeneration())
                {
                    // add to the remaining subspecies
                    remainSubspecies.Add(s);
                    s.ToNewGeneration();
                    // add to the new generation population
                    ((List<Individual>)newGenIndividuals).AddRange(s.Individuals);
                }
            }
            // replace the old stuff
            Subspecies = remainSubspecies;

            state.Population.Subpops[subpop].Individuals = newGenIndividuals;
        }

        /** Perform a delta coding. */
        public void DeltaCoding(IEvolutionState state, int subpop, IList<NEATSubspecies> sortedSubspecies)
        {
            HighestLastChanged = 0;

            int popSize = state.Population.Subpops[subpop].InitialSize;
            int halfPop = popSize / 2;

            NEATSubspecies bestFitnessSubspecies = sortedSubspecies[0];
            // the first individual of the first subspecies can have 1/2 pop size
            // offsprings
            ((NEATIndividual) bestFitnessSubspecies.First()).SuperChampionOffspring = halfPop;
            // the first subspecies can have 1/2 pop size offspring
            bestFitnessSubspecies.ExpectedOffspring = halfPop;
            bestFitnessSubspecies.AgeOfLastImprovement = bestFitnessSubspecies.Age;

            if (sortedSubspecies.Count >= 2)
            {
                // the second subspecies can have the other half pop size
                ((NEATIndividual) sortedSubspecies[1].First()).SuperChampionOffspring = popSize - halfPop;
                sortedSubspecies[1].ExpectedOffspring = popSize - halfPop;
                sortedSubspecies[1].AgeOfLastImprovement = sortedSubspecies[1].Age;
                // the remainder subspecies has 0 offsprings
                for (int i = 2; i < sortedSubspecies.Count; ++i)
                {
                    sortedSubspecies[i].ExpectedOffspring = 0;
                }
            }
            else
            {
                ((NEATIndividual) bestFitnessSubspecies.First()).SuperChampionOffspring += popSize - halfPop;
                bestFitnessSubspecies.ExpectedOffspring = popSize - halfPop;
            }
        }

        /** Determine if the whole subpopulation get into stagnation. */
        public void PopulationStagnation(IEvolutionState state, int subpop, IList<NEATSubspecies> sortedSubspecies)
        {
            NEATIndividual bestFitnessIndividual = (NEATIndividual) sortedSubspecies[0].Individuals[0];
            bestFitnessIndividual.PopChampion = true;
            if (bestFitnessIndividual.Fitness.Value > HighestFitness)
            {
                HighestFitness = bestFitnessIndividual.Fitness.Value;
                HighestLastChanged = 0;
                //state.output.message("Population has reached a new RECORD FITNESS " + highestFitness);
            }
            else
            {
                HighestLastChanged++;
                //state.output.message(
                //    highestLastChanged + " generations since last population fitness record " + highestFitness);
            }
        }

        /** Steal the babies from champion subspecies. */
        public void StealBabies(IEvolutionState state, int thread, int subpop, IList<NEATSubspecies> sortedSubspecies)
        {
            // Take away a constant number of expected offspring from the worst few
            // species
            int babiesAlreadyStolen = 0;

            for (int i = sortedSubspecies.Count - 1; i >= 0 && babiesAlreadyStolen < BabiesStolen; i--)
            {
                NEATSubspecies subs = sortedSubspecies[i];

                if (subs.Age > 5 && subs.ExpectedOffspring > 2)
                {
                    // This subspecies has enough to finish off the stolen pool
                    int babiesNeeded = BabiesStolen - babiesAlreadyStolen;
                    if (subs.ExpectedOffspring - 1 >= babiesNeeded)
                    {
                        subs.ExpectedOffspring -= babiesNeeded;
                        babiesAlreadyStolen = BabiesStolen;
                    }
                    // Not enough here to complete the pool of stolen, then leave
                    // one individual
                    // for that subspecies
                    else
                    {
                        babiesAlreadyStolen += subs.ExpectedOffspring - 1;
                        subs.ExpectedOffspring = 1;
                    }
                }
            }

            // Mark the best champions of the top subspecies to be the super
            // champions
            // who will take on the extra offspring for cloning or mutant cloning
            // Determine the exact number that will be given to the top three
            // They get, in order, 1/5 1/5 and 1/10 of the already stolen babies
            int[] quote = new int[3];
            quote[0] = quote[1] = BabiesStolen / 5;
            quote[2] = BabiesStolen / 10;

            int quoteIndex = 0;

            foreach (var subs in sortedSubspecies)
            {
                // Don't give to dying species even if they are champions
                if (subs.TimeSinceLastImproved() <= DropoffAge)
                {
                    if (quoteIndex < quote.Length)
                    {
                        if (babiesAlreadyStolen > quote[quoteIndex])
                        {
                            ((NEATIndividual)subs.First()).SuperChampionOffspring = quote[quoteIndex];
                            subs.ExpectedOffspring += quote[quoteIndex];
                            babiesAlreadyStolen -= quote[quoteIndex];
                        }
                        quoteIndex++;
                    }
                    else if (quoteIndex >= quote.Length)
                    {
                        // Randomize a little which species get boosted by a super
                        // champion
                        if (state.Random[thread].NextBoolean(.9))
                        {
                            if (babiesAlreadyStolen > 3)
                            {
                                ((NEATIndividual)subs.First()).SuperChampionOffspring = 3;
                                subs.ExpectedOffspring += 3;
                                babiesAlreadyStolen -= 3;
                            }
                            else
                            {
                                ((NEATIndividual)subs.First()).SuperChampionOffspring = babiesAlreadyStolen;
                                subs.ExpectedOffspring += babiesAlreadyStolen;
                                babiesAlreadyStolen = 0;
                            }
                        }
                    }
                    // assiged all the stolen babies
                    if (babiesAlreadyStolen == 0)
                        break;
                }
            }


            // If any stolen babies aren't taken, give them to species #1's champion
            if (babiesAlreadyStolen > 0)
            {
                state.Output.Message("Not all stolen babies assigned, giving to the best subspecies");
                NEATSubspecies subs = Subspecies[0];
                ((NEATIndividual) subs.First()).SuperChampionOffspring += babiesAlreadyStolen;
                subs.ExpectedOffspring += babiesAlreadyStolen;
                babiesAlreadyStolen = 0;
            }

        }



        /** Create a new individual with given nodes and genes */
        public Individual NewIndividual(IEvolutionState state, int thread, IList<NEATNode> nodes, IList<Gene> genes)
        {
            NEATIndividual newind = (NEATIndividual) NewIndividual(state, thread);
            newind.Reset(nodes, genes);
            return newind;
        }

        public bool HasInnovation(NEATInnovation inno)
        {
            return Innovations.ContainsKey(inno);
        }

        public NEATInnovation GetInnovation(NEATInnovation inno)
        {
            return Innovations[inno];
        }

        public void AddInnovation(NEATInnovation inno)
        {
            Innovations[inno] = inno;
        }

        /**
         * Clear the evaluation flag in each individual. This is important if a
         * evaluation individual mutated.
         */
        public void ClearEvaluationFlag(IList<Individual> individuals)
        {
            foreach (Individual i in individuals)
            {
                i.Evaluated = false;
            }
        }

    }
}