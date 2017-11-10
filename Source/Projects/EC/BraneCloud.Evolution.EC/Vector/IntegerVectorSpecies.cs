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

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Vector
{
    /**
     * IntegerVectorSpecies is a subclass of VectorSpecies with special constraints
     * for integral vectors, namely ByteVectorIndividual, ShortVectorIndividual,
     * IntegerVectorIndividual, and LongVectorIndividual.
     *
     * <p>IntegerVectorSpecies can specify a number of parameters globally, per-segment, and per-gene.
     * See <a href="VectorSpecies.html">VectorSpecies</a> for information on how to this works.
     *
     * <p>IntegerVectorSpecies defines a minimum and maximum gene value.  These values
     * are used during initialization and, depending on whether <tt>mutation-bounded</tt>
     * is true, also during various mutation algorithms to guarantee that the gene value
     * will not exceed these minimum and maximum bounds.
     *
     * <p>
     * IntegerVectorSpecies provides support for two ways of mutating a gene.
     * <ul>
     * <li><b>reset</b> Replacing the gene's value with a value uniformly drawn from the gene's
     * range (the default behavior).</li>
     * <li><b>random-walk</b>Replacing the gene's value by performing a random walk starting at the gene
     * value.  The random walk either adds 1 or subtracts 1 (chosen at random), then does a coin-flip
     * to see whether to continue the random walk.  When the coin-flip finally comes up false, the gene value
     * is set to the current random walk position.
     * </ul>
     *
     * <p>IntegerVectorSpecies performs gene initialization by resetting the gene.
     *
     *
     * <p><b>Parameters</b><br>
     * <table>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>min-gene</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>segment</tt>.<i>segment-number</i>.<tt>min-gene</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>min-gene</tt>.<i>gene-number</i><br>
     * <font size=-1>long (default=0)</font></td>
     * <td valign=top>(the minimum gene value)</td></tr>
     *
     <tr><td>&nbsp;
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>max-gene</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>segment</tt>.<i>segment-number</i>.<tt>max-gene</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>max-gene</tt>.<i>gene-number</i><br>
     * <font size=-1>long &gt;= <i>base</i>.min-gene</font></td>
     * <td valign=top>(the maximum gene value)</td></tr>
     *
     <tr><td>&nbsp;
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>mutation-type</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>segment</tt>.<i>segment-number</i>.<tt>mutation-type</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>mutation-prob</tt>.<i>gene-number</i><br>
     * <font size=-1><tt>reset</tt> or <tt>random-walk</tt> (default=<tt>reset</tt>)</font></td>
     * <td valign=top>(the mutation type)</td>
     * </tr>
     *
     <tr><td>&nbsp;
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>random-walk-probability</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>segment</tt>.<i>segment-number</i>.<tt>random-walk-probability</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>random-walk-probability</tt>.<i>gene-number</i><br>
     <font size=-1>0.0 &lt;= float &lt;= 1.0 </font></td>
     *  <td valign=top>(the probability that a random walk will continue.  Random walks go up or down by 1.0 until the coin flip comes up false.)</td>
     * </tr>
     * 
     <tr><td>&nbsp;
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>mutation-bounded</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>segment</tt>.<i>segment-number</i>.<tt>mutation-bounded</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>mutation-bounded</tt>.<i>gene-number</i><br>
     *  <font size=-1>boolean (default=true)</font></td>
     *  <td valign=top>(whether mutation is restricted to only being within the min/max gene values.  Does not apply to SimulatedBinaryCrossover (which is always bounded))</td>
     * </tr>
     * </table>
     * @author Sean Luke, Rafal Kicinger
     * @version 1.0 
     */

    [Serializable]
    [ECConfiguration("ec.vector.IntegerVectorSpecies")]
    public class IntegerVectorSpecies : VectorSpecies
    {
        #region Constants

        public const string P_MINGENE = "min-gene";
        public const string P_MAXGENE = "max-gene";
        
        // DEFINED ON VECTOR SPECIES
        //public const string P_NUM_SEGMENTS = "num-segments";        
        //public const string P_SEGMENT_TYPE = "segment-type";       
        //public const string P_SEGMENT_START = "start";        
        //public const string P_SEGMENT_END = "end";        
        //public const string P_SEGMENT = "segment";

        public const string P_MUTATIONTYPE = "mutation-type";
        public const string P_RANDOM_WALK_PROBABILITY = "random-walk-probability";
        public const string P_MUTATION_BOUNDED = "mutation-bounded";
        public const string V_RESET_MUTATION = "reset";
        public const string V_RANDOM_WALK_MUTATION = "random-walk";
        public const int C_RESET_MUTATION = 0;
        public const int C_RANDOM_WALK_MUTATION = 1;

        #endregion // Constants
        #region Properties

        protected long[] MinGenes;
        protected long[] MaxGenes;

        /** Mutation type, per gene.
           This array is one longer than the standard genome length.
           The top element in the array represents the parameters for genes in
           genomes which have extended beyond the genome length.  */
        protected int[] MutationType { get; set; }

        /** The continuation probability for Integer Random Walk Mutation, per gene.
            This array is one longer than the standard genome length.
            The top element in the array represents the parameters for genes in
            genomes which have extended beyond the genome length.  */
        protected double[] RandomWalkProbability { get; set; }

        /** Whether mutation is bounded to the min- and max-gene values, per gene.
            This array is one longer than the standard genome length.
            The top element in the array represents the parameters for genes in
            genomes which have extended beyond the genome length.  */
        protected bool[] MutationIsBounded { get; set; }

        /** Whether the mutationIsBounded value was defined, per gene.
            Used internally only.
            This array is one longer than the standard genome length.
            The top element in the array represents the parameters for genes in
            genomes which have extended beyond the genome length.  */
        bool _mutationIsBoundedDefined;

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            IParameter def = DefaultBase;

            SetupGenome(state, paramBase);

            // create the arrays
            MinGenes = new long[GenomeSize + 1];
            MaxGenes = new long[GenomeSize + 1];
            MutationType = Fill(new int[GenomeSize + 1], -1);
            MutationIsBounded = new bool[GenomeSize + 1];
            RandomWalkProbability = new double[GenomeSize + 1];


            // LOADING GLOBAL MIN/MAX GENES
            long minGene = state.Parameters.GetLongWithDefault(paramBase.Push(P_MINGENE), def.Push(P_MINGENE), 0);
            long maxGene = state.Parameters.GetLong(paramBase.Push(P_MAXGENE), def.Push(P_MAXGENE), minGene);
            if (maxGene < minGene)
                state.Output.Fatal("IntegerVectorSpecies must have a default min-gene which is <= the default max-gene",
                    paramBase.Push(P_MAXGENE), def.Push(P_MAXGENE));
            Fill(MinGenes, minGene);
            Fill(MaxGenes, maxGene);


            /// MUTATION


            String mtype = state.Parameters.GetStringWithDefault(paramBase.Push(P_MUTATIONTYPE), def.Push(P_MUTATIONTYPE), null);
            int mutType = C_RESET_MUTATION;
            if (mtype == null)
                state.Output.Warning("No global mutation type given for IntegerVectorSpecies, assuming 'reset' mutation",
                    paramBase.Push(P_MUTATIONTYPE), def.Push(P_MUTATIONTYPE));
            else if (mtype.Equals(V_RESET_MUTATION, StringComparison.InvariantCultureIgnoreCase))
                mutType = C_RESET_MUTATION; // redundant
            else if (mtype.Equals(V_RANDOM_WALK_MUTATION, StringComparison.InvariantCultureIgnoreCase))
                mutType = C_RANDOM_WALK_MUTATION;
            else
                state.Output.Fatal("IntegerVectorSpecies given a bad mutation type: "
                    + mtype, paramBase.Push(P_MUTATIONTYPE), def.Push(P_MUTATIONTYPE));
            Fill(MutationType, mutType);

            if (mutType == C_RANDOM_WALK_MUTATION)
            {
                double randWalkProb = state.Parameters.GetDoubleWithMax(paramBase.Push(P_RANDOM_WALK_PROBABILITY), def.Push(P_RANDOM_WALK_PROBABILITY), 0.0, 1.0);
                if (randWalkProb <= 0)
                    state.Output.Fatal("If it's going to use random walk mutation as its global mutation type, IntegerVectorSpecies must a random walk mutation probability between 0.0 and 1.0.",
                        paramBase.Push(P_RANDOM_WALK_PROBABILITY), def.Push(P_RANDOM_WALK_PROBABILITY));
                Fill(RandomWalkProbability, randWalkProb);

                if (!state.Parameters.ParameterExists(paramBase.Push(P_MUTATION_BOUNDED), def.Push(P_MUTATION_BOUNDED)))
                    state.Output.Warning("IntegerVectorSpecies is using gaussian, polynomial, or integer randomwalk mutation as its global mutation type, but " + P_MUTATION_BOUNDED + " is not defined.  Assuming 'true'");
                bool mutIsBounded = state.Parameters.GetBoolean(paramBase.Push(P_MUTATION_BOUNDED), def.Push(P_MUTATION_BOUNDED), true);
                Fill(MutationIsBounded, mutIsBounded);
                _mutationIsBoundedDefined = true;
            }


            base.Setup(state, paramBase);

            // VERIFY
            for (var x = 0; x < GenomeSize; x++)
            {
                if (MaxGenes[x] < MinGenes[x])
                    state.Output.Fatal("IntegerVectorSpecies must have a min-gene[" + x + "] which is <= the max-gene[" + x + "]");

                // check to see if these longs are within the data type of the particular individual
                if (!InNumericalTypeRange(MinGenes[x]))
                    state.Output.Fatal("This IntegerVectorSpecies has a prototype of the kind: "
                                       + I_Prototype.GetType().FullName + ", but doesn't have a min-gene[" + x
                                       + "] value within the range of this prototype's genome's data types");

                if (!InNumericalTypeRange(MaxGenes[x]))
                    state.Output.Fatal("This IntegerVectorSpecies has a prototype of the kind: "
                                       + I_Prototype.GetType().FullName + ", but doesn't have a max-gene[" + x
                                       + "] value within the range of this prototype's genome's data types");
            }
            /*
            //Debugging
            for(int i = 0; i < minGenes.length; i++)
            System.out.PrintLn("Min: " + minGenes[i] + ", Max: " + maxGenes[i]);
            */
        }

        protected override void LoadParametersForGene(IEvolutionState state, int index, IParameter paramBase, IParameter def, String postfix)
        {
            base.LoadParametersForGene(state, index, paramBase, def, postfix);

            bool minValExists = state.Parameters.ParameterExists(paramBase.Push(P_MINGENE).Push(postfix), def.Push(P_MINGENE).Push(postfix));
            bool maxValExists = state.Parameters.ParameterExists(paramBase.Push(P_MAXGENE).Push(postfix), def.Push(P_MAXGENE).Push(postfix));

            if ((maxValExists && !minValExists))
                state.Output.Warning("Max Gene specified but not Min Gene", paramBase.Push(P_MINGENE).Push(postfix), def.Push(P_MINGENE).Push(postfix));

            if (minValExists && !maxValExists)
                state.Output.Warning("Min Gene specified but not Max Gene", paramBase.Push(P_MAXGENE).Push(postfix), def.Push(P_MINGENE).Push(postfix));

            if (minValExists)
            {
                long minVal = state.Parameters.GetLongWithDefault(paramBase.Push(P_MINGENE).Push(postfix), def.Push(P_MINGENE).Push(postfix), 0);

                //check if the value is in range
                if (!InNumericalTypeRange(minVal))
                    state.Output.Error("Min Gene Value out of range for data type " + I_Prototype.GetType().Name,
                        paramBase.Push(P_MINGENE).Push(postfix),
                        paramBase.Push(P_MINGENE).Push(postfix));
                else MinGenes[index] = minVal;

                if (DynamicInitialSize)
                    state.Output.WarnOnce("Using dynamic initial sizing, but per-gene or per-segment min-gene declarations.  This is probably wrong.  You probably want to use global min/max declarations.",
                        paramBase.Push(P_MINGENE).Push(postfix),
                        paramBase.Push(P_MINGENE).Push(postfix));
            }

            if (minValExists)
            {
                long maxVal = state.Parameters.GetLongWithDefault(paramBase.Push(P_MAXGENE).Push(postfix), def.Push(P_MAXGENE).Push(postfix), 0);

                //check if the value is in range
                if (!InNumericalTypeRange(maxVal))
                    state.Output.Error("Max Gene Value out of range for data type " + I_Prototype.GetType().Name,
                        paramBase.Push(P_MAXGENE).Push(postfix),
                        paramBase.Push(P_MAXGENE).Push(postfix));
                else MaxGenes[index] = maxVal;

                if (DynamicInitialSize)
                    state.Output.WarnOnce("Using dynamic initial sizing, but per-gene or per-segment max-gene declarations.  This is probably wrong.  You probably want to use global min/max declarations.",
                        paramBase.Push(P_MAXGENE).Push(postfix),
                        paramBase.Push(P_MAXGENE).Push(postfix));
            }

            // MUTATION

            String mtype = state.Parameters.GetStringWithDefault(paramBase.Push(P_MUTATIONTYPE).Push(postfix), def.Push(P_MUTATIONTYPE).Push(postfix), null);
            int mutType = -1;
            if (mtype == null) { }  // we're cool
            else if (mtype.Equals(V_RESET_MUTATION, StringComparison.InvariantCultureIgnoreCase))
                mutType = MutationType[index] = C_RESET_MUTATION;
            else if (mtype.Equals(V_RANDOM_WALK_MUTATION, StringComparison.InvariantCultureIgnoreCase))
            {
                mutType = MutationType[index] = C_RANDOM_WALK_MUTATION;
                state.Output.WarnOnce("Integer Random Walk Mutation used in IntegerVectorSpecies.  Be advised that during initialization these genes will only be set to integer values.");
            }
            else
                state.Output.Error("IntegerVectorSpecies given a bad mutation type: " + mtype,
                    paramBase.Push(P_MUTATIONTYPE).Push(postfix), def.Push(P_MUTATIONTYPE).Push(postfix));


            if (mutType == C_RANDOM_WALK_MUTATION)
            {
                if (state.Parameters.ParameterExists(paramBase.Push(P_RANDOM_WALK_PROBABILITY).Push(postfix), def.Push(P_RANDOM_WALK_PROBABILITY).Push(postfix)))
                {
                    RandomWalkProbability[index] = state.Parameters.GetDoubleWithMax(paramBase.Push(P_RANDOM_WALK_PROBABILITY).Push(postfix), def.Push(P_RANDOM_WALK_PROBABILITY).Push(postfix), 0.0, 1.0);
                    if (RandomWalkProbability[index] <= 0)
                        state.Output.Error("If it's going to use random walk mutation as a per-gene or per-segment type, IntegerVectorSpecies must a random walk mutation probability between 0.0 and 1.0.",
                            paramBase.Push(P_RANDOM_WALK_PROBABILITY).Push(postfix), def.Push(P_RANDOM_WALK_PROBABILITY).Push(postfix));
                }
                else
                    state.Output.Error("If IntegerVectorSpecies is going to use polynomial mutation as a per-gene or per-segment type, either the global or per-gene/per-segment random walk mutation probability must be defined.",
                        paramBase.Push(P_RANDOM_WALK_PROBABILITY).Push(postfix), def.Push(P_RANDOM_WALK_PROBABILITY).Push(postfix));

                if (state.Parameters.ParameterExists(paramBase.Push(P_MUTATION_BOUNDED).Push(postfix), def.Push(P_MUTATION_BOUNDED).Push(postfix)))
                {
                    MutationIsBounded[index] = state.Parameters.GetBoolean(paramBase.Push(P_MUTATION_BOUNDED).Push(postfix), def.Push(P_MUTATION_BOUNDED).Push(postfix), true);
                }
                else if (!_mutationIsBoundedDefined)
                    state.Output.Fatal("If IntegerVectorSpecies is going to use gaussian, polynomial, or integer random walk mutation as a per-gene or per-segment type, the mutation bounding must be defined.",
                        paramBase.Push(P_MUTATION_BOUNDED).Push(postfix), def.Push(P_MUTATION_BOUNDED).Push(postfix));

            }
        }
        #endregion // Setup
        #region Operations

        public virtual long GetMaxGene(int gene)
        {
            long[] m = MaxGenes;
            if (m.Length <= gene)
                gene = m.Length - 1;
            return m[gene];
        }

        public virtual long GetMinGene(int gene)
        {
            long[] m = MinGenes;
            if (m.Length <= gene)
                gene = m.Length - 1;
            return m[gene];
        }

        public int GetMutationType(int gene)
        {
            int[] m = MutationType;
            if (m.Length <= gene)
                gene = m.Length - 1;
            return m[gene];
        }

        public double GetRandomWalkProbability(int gene)
        {
            double[] m = RandomWalkProbability;
            if (m.Length <= gene)
                gene = m.Length - 1;
            return m[gene];
        }

        public bool GetMutationIsBounded(int gene)
        {
            bool[] m = MutationIsBounded;
            if (m.Length <= gene)
                gene = m.Length - 1;
            return m[gene];
        }

        public bool InNumericalTypeRange(double geneVal)
        {
            if (I_Prototype is ByteVectorIndividual)
            return (geneVal <= Byte.MaxValue && geneVal >= Byte.MinValue);
            if (I_Prototype is ShortVectorIndividual)
                return (geneVal <= Int16.MaxValue && geneVal >= Int16.MinValue);
            if (I_Prototype is IntegerVectorIndividual)
                return (geneVal <= Int32.MaxValue && geneVal >= Int32.MinValue);
            if (I_Prototype is LongVectorIndividual)
                return true;  // geneVal is valid for all longs
            return false;  // dunno what the individual is...
        }

        public virtual bool InNumericalTypeRange(long geneVal)
        {
            if (I_Prototype is ByteVectorIndividual)
                return (geneVal <= Byte.MaxValue && geneVal >= Byte.MinValue);
            if (I_Prototype is ShortVectorIndividual)
                return (geneVal <= Int16.MaxValue && geneVal >= Int16.MinValue);
            if (I_Prototype is IntegerVectorIndividual)
                return (geneVal <= Int32.MaxValue && geneVal >= Int32.MinValue);
            if (I_Prototype is LongVectorIndividual)
                return true; // geneVal is valid for all longs
            return false; // dunno what the individual is...
        }

        private void InitializeGenomeSegmentsByStartIndices(IEvolutionState state, IParameter paramBase, IParameter def,
                                                                            int numSegments, long minGene, long maxGene)
        {
            var warnedMin = false;
            var warnedMax = false;

            //loop in reverse order 
            var previousSegmentEnd = GenomeSize;
            var currentSegmentEnd = 0;

            for (var i = numSegments - 1; i >= 0; i--)
            {
                //check if the segment data exist
                if (state.Parameters.ParameterExists(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_START),
                                                  def.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_START)))
                {
                    //Read the index of the end gene specifying current segment
                    currentSegmentEnd = state.Parameters.GetInt(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_START),
                                                                      def.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_START));
                }
                else
                {
                    state.Output.Fatal("Genome segment " + i + " has not been defined!" + "\nYou must specify start indices for "
                        + numSegments + " segment(s)", paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_START),
                                                       paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_START));
                }

                //check if the start index is valid
                if (currentSegmentEnd >= previousSegmentEnd || currentSegmentEnd < 0)
                    state.Output.Fatal("Invalid start index value for segment " + i + ": " + currentSegmentEnd
                        + "\nThe value must be smaller than " + previousSegmentEnd + " and greater than or equal to  " + 0);

                //check if the index of the first segment is equal to 0
                if (i == 0 && currentSegmentEnd != 0)
                    state.Output.Fatal("Invalid start index value for the first segment "
                        + i + ": " + currentSegmentEnd + "\nThe value must be equal to " + 0);


                //get min and max values of genes in this segment
                var currentSegmentMinGeneValue = Int64.MaxValue;
                if (!state.Parameters.ParameterExists(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE),
                                             paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE)))
                {
                    if (!warnedMin)
                    {
                        state.Output.Warning("IntegerVectorSpecies has missing min-gene values for some segments.\n"
                            + "The first segment is #" + i + ".", paramBase.Push(P_SEGMENT).Push("" + i),
                                                                  paramBase.Push(P_SEGMENT).Push("" + i));
                        warnedMin = true;
                    }

                    //the min-gene value has not been defined for this segment so assume the global min value
                    currentSegmentMinGeneValue = minGene;
                }
                //get the min value for this segment
                else
                {
                    currentSegmentMinGeneValue = state.Parameters.GetLongWithDefault(
                                paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE),
                                paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE), minGene);
                }

                var currentSegmentMaxGeneValue = Int64.MinValue;
                if (!state.Parameters.ParameterExists(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
                                             paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE)))
                {
                    if (!warnedMax)
                    {
                        state.Output.Warning("IntegerVectorSpecies has missing max-gene values for some segments.\n"
                            + "The first segment is #" + i + ".", paramBase.Push(P_SEGMENT).Push("" + i),
                                                                  paramBase.Push(P_SEGMENT).Push("" + i));
                        warnedMax = true;
                    }

                    //the max-gen value has not been defined for this segment so assume the global max value
                    currentSegmentMaxGeneValue = maxGene;
                }
                //get the max value for this segment
                else
                {
                    currentSegmentMaxGeneValue = state.Parameters.GetLongWithDefault(
                                    paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
                                    paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE), maxGene);
                }

                //check is min is smaller than or equal to max
                if (currentSegmentMaxGeneValue < currentSegmentMinGeneValue)
                    state.Output.Fatal("IntegerVectorSpecies must have a min-gene value for segment " + i
                        + " which is <= the max-gene value", paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
                                                             paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE));


                //and assign min and max values for all genes in this segment
                for (var j = previousSegmentEnd - 1; j >= currentSegmentEnd; j--)
                {
                    MinGenes[j] = currentSegmentMinGeneValue;
                    MaxGenes[j] = currentSegmentMaxGeneValue;
                }

                previousSegmentEnd = currentSegmentEnd;
            }
        }

        private void InitializeGenomeSegmentsByEndIndices(IEvolutionState state, IParameter paramBase, IParameter def,
                                                                            int numSegments, long minGene, long maxGene)
        {
            var warnedMin = false;
            var warnedMax = false;

            var previousSegmentEnd = -1;
            var currentSegmentEnd = 0;
            // iterate over segments and set genes values for each segment
            for (var i = 0; i < numSegments; i++)
            {
                //check if the segment data exist
                if (state.Parameters.ParameterExists(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END),
                                                  def.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END)))
                {
                    //Read the index of the end gene specifying current segment
                    currentSegmentEnd = state.Parameters.GetInt(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END),
                                                                      def.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END));
                }
                else
                {
                    state.Output.Fatal("Genome segment " + i + " has not been defined!" + "\nYou must specify end indices for "
                        + numSegments + " segment(s)", paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END),
                                                       paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END));
                }

                //check if the end index is valid
                if (currentSegmentEnd <= previousSegmentEnd || currentSegmentEnd >= GenomeSize)
                    state.Output.Fatal("Invalid end index value for segment " + i + ": " + currentSegmentEnd
                            + "\nThe value must be greater than " + previousSegmentEnd + " and smaller than " + GenomeSize);

                //check if the index of the segment is equal to the GenomeSize
                if (i == numSegments - 1 && currentSegmentEnd != (GenomeSize - 1))
                    state.Output.Fatal("Invalid end index value for the last segment " + i + ": " + currentSegmentEnd
                        + "\nThe value must be equal to the index of the last gene in the genome:  " + (GenomeSize - 1));


                //get min and max values of genes in this segment
                var currentSegmentMinGeneValue = Int64.MaxValue;
                if (!state.Parameters.ParameterExists(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE),
                                             paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE)))
                {
                    if (!warnedMin)
                    {
                        state.Output.Warning("IntegerVectorSpecies has missing min-gene values for some segments.\n"
                            + "The first segment is #" + i + ".", paramBase.Push(P_SEGMENT).Push("" + i),
                                                                  paramBase.Push(P_SEGMENT).Push("" + i));
                        warnedMin = true;
                    }

                    //the min-gene value has not been defined for this segment so assume the global min value
                    currentSegmentMinGeneValue = minGene;
                }
                //get the min value for this segment
                else
                {
                    currentSegmentMinGeneValue = state.Parameters.GetLongWithDefault(
                                    paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE),
                                    paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE), minGene);
                }

                var currentSegmentMaxGeneValue = Int64.MinValue;
                if (!state.Parameters.ParameterExists(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
                                             paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE)))
                {
                    if (!warnedMax)
                    {
                        state.Output.Warning("IntegerVectorSpecies has missing max-gene values for some segments.\n"
                            + "The first segment is #" + i + ".", paramBase.Push(P_SEGMENT).Push("" + i),
                                                                  paramBase.Push(P_SEGMENT).Push("" + i));
                        warnedMax = true;
                    }

                    //the max-gen value has not been defined for this segment so assume the global max value
                    currentSegmentMaxGeneValue = maxGene;
                }
                //get the max value for this segment
                else
                {
                    currentSegmentMaxGeneValue = state.Parameters.GetLongWithDefault(
                                    paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
                                    paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE), maxGene);
                }

                //check is min is smaller than or equal to max
                if (currentSegmentMaxGeneValue < currentSegmentMinGeneValue)
                    state.Output.Fatal("IntegerVectorSpecies must have a min-gene value for segment " + i
                        + " which is <= the max-gene value", paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
                                                             paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE));

                //and assign min and max values for all genes in this segment
                for (var j = previousSegmentEnd + 1; j <= currentSegmentEnd; j++)
                {
                    MinGenes[j] = currentSegmentMinGeneValue;
                    MaxGenes[j] = currentSegmentMaxGeneValue;
                }

                previousSegmentEnd = currentSegmentEnd;
            }
        }

        #endregion // Operations
    }
}