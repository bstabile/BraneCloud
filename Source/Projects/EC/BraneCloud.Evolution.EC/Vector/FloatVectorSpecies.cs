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
     * FloatVectorSpecies is a subclass of VectorSpecies with special
     * constraints for floating-point vectors, namely FloatVectorIndividual and
     * DoubleVectorIndividual.
     *
     * <p>FloatVectorSpecies can specify a number of parameters globally, per-segment, and per-gene.
     * See <a href="VectorSpecies.html">VectorSpecies</a> for information on how to this works.
     *
     * <p>FloatVectorSpecies defines a minimum and maximum gene value.  These values
     * are used during initialization and, depending on whether <tt>mutation-bounded</tt>
     * is true, also during various mutation algorithms to guarantee that the gene value
     * will not exceed these minimum and maximum bounds.
     *
     * <p>
     * FloatVectorSpecies provides support for five ways of mutating a gene.
     * <ul>
     * <li><b>reset</b> Replacing the gene's value with a value uniformly drawn from the gene's
     * range (the default behavior).</li>
     * <li><b>gauss</b>Perturbing the gene's value with gaussian noise; if the gene-by-gene range 
     * is used, than the standard deviation is scaled to reflect each gene's range. 
     * If the gaussian mutation's standard deviation is too large for the range,
     * than there's a large probability the mutated value will land outside range.
     * We will try again a number of times (100) before giving up and using the 
     * previous mutation method.</li>
     * <li><b>polynomial</b> Perturbing the gene's value with noise chosen from a <i>polynomial distribution</i>,
     * similar to the gaussian distribution.  The polynomial distribution was popularized
     * by Kalyanmoy Deb and is found in many of his publications (see http://www.iitk.ac.in/kangal/deb.shtml).
     * The polynomial distribution has two options.  First, there is the <i>index</i>.  This
     * variable defines the shape of the distribution and is in some sense the equivalent of the
     * standard deviation in the gaussian distribution.  The index is an integer.  If it is zero,
     * the polynomial distribution is simply the uniform distribution from [1,-1].  If it is 1, the
     * polynomial distribution is basically a triangular distribution from [1,-1] peaking at 0.  If
     * it is 2, the polynomial distribution follows a squared function, again peaking at 0.  Larger
     * values result in even more peaking and narrowness.  The default values used in nearly all of
     * the NSGA-II and Deb work is 20.  Second, there is whether or not the value is intended for
     * <i>bounded</i> genes.  The default polynomial distribution is used when we assume the gene can
     * take on literally any value, even beyond the min and max values.  For genes which are restricted
     * to be between min and max, there is an alternative version of the polynomial distribution, used by
     * Deb's team but not discussed much in the literature, desiged for that situation.  We assume boundedness
     * by default, and have found it to be somewhat better for NSGA-II and SPEA2 problems.  For a description
     * of this alternative version, see "A Niched-Penalty Approach for Constraint Handling in Genetic Algorithms"
     * by Kalyanmoy Deb and Samir Agrawal.  Deb's default implementation bounds the result to min or max;
     * instead ECJ's implementation of the polynomial distribution retries until it finds a legal value.  This
     * will be just fine for ranges like [0,1], but for smaller ranges you may be waiting a long time.
     * <li><b>integer-reset</b> Replacing the gene's value with a value uniformly drawn from the gene's range
     * but restricted to only integers.
     * <li><b>integer-random-walk</b> Replacing the gene's value by performing a random walk starting at the gene
     * value.  The random walk either adds 1 or subtracts 1 (chosen at random), then does a coin-flip
     * to see whether to continue the random walk.  When the coin-flip finally comes up false, the gene value
     * is set to the current random walk position.
     * </ul>
     *
     * <p>
     * FloatVectorSpecies provides support for two ways of initializing a gene.  The initialization procedure
     * is determined by the choice of mutation procedure as described above.  If the mutation is floating-point
     * (<tt>reset, gauss, polynomial</tt>), then initialization will be done by resetting the gene
     * to uniformly chosen floating-point value between the minimum and maximum legal gene values, inclusive.
     * If the mutation is integer (<tt>integer-reset, integer-random-walk</tt>), then initialization will be done
     * by performing the same kind of reset, but restricting values to integers only.
     * 
     * 
     * <p>
     * <b>Parameters</b><br>
     * <table>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>min-gene</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>segment</tt>.<i>segment-number</i>.<tt>min-gene</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>min-gene</tt>.<i>gene-number</i><br>
     <font size=-1>0.0 &lt;= float &lt;= 1.0 </font></td>
     <td valign=top>(probability that a gene will get mutated over default mutation)</td></tr>
     * <font size=-1>double (default=0.0)</font></td>
     * <td valign=top>(the minimum gene value)</td>
     * </tr>
     * 
     <tr><td>&nbsp;
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>max-gene</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>segment</tt>.<i>segment-number</i>.<tt>max-gene</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>max-gene</tt>.<i>gene-number</i><br>
     <font size=-1>0.0 &lt;= float &lt;= 1.0 </font></td>
     <td valign=top>(probability that a gene will get mutated over default mutation)</td></tr>
     * <font size=-1>double &gt;= <i>base</i>.min-gene</font></td>
     * <td valign=top>(the maximum gene value)</td>
     * </tr>
     * 
     <tr><td>&nbsp;
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>mutation-type</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>segment</tt>.<i>segment-number</i>.<tt>mutation-type</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>mutation-prob</tt>.<i>gene-number</i><br>
     * <font size=-1><tt>reset</tt>, <tt>gauss</tt>, <tt>polynomial</tt>, <tt>integer-reset</tt>, or <tt>integer-random-walk</tt> (default=<tt>reset</tt>)</font></td>
     * <td valign=top>(the mutation type)</td>
     * </tr>
     * 
     <tr><td>&nbsp;
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>mutation-stdev</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>segment</tt>.<i>segment-number</i>.<tt>mutation-stdev</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>mutation-stdev</tt>.<i>gene-number</i><br>
     * <font size=-1>double &ge; 0</font></td>
     * <td valign=top>(the standard deviation or the gauss perturbation)</td>
     * </tr>
     * 
     * <tr>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>distribution-index</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>segment</tt>.<i>segment-number</i>.<tt>distribution-index</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>distribution-index</tt>.<i>gene-number</i><br>
     * <font size=-1>int &ge; 0</font></td>
     * <td valign=top>(the mutation distribution index for the polynomial mutation distribution)</td>
     * </tr>
     * 
     <tr><td>&nbsp;
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>alternative-polynomial-version</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>segment</tt>.<i>segment-number</i>.<tt>alternative-polynomial-version</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>alternative-polynomial-version</tt>.<i>gene-number</i><br>
     *  <font size=-1>boolean (default=true)</font></td>
     *  <td valign=top>(whether to use the "bounded" variation of the polynomial mutation or the standard ("unbounded") version)</td>
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
     * <tr>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>mutation-bounded</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>segment</tt>.<i>segment-number</i>.<tt>mutation-bounded</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>mutation-bounded</tt>.<i>gene-number</i><br>
     *  <font size=-1>boolean (default=true)</font></td>
     *  <td valign=top>(whether mutation is restricted to only being within the min/max gene values.  Does not apply to SimulatedBinaryCrossover (which is always bounded))</td>
     * </tr>
     * 
     <tr><td>&nbsp;
     * <td valign=top><i>base</i>.<tt>out-of-bounds-retries</tt><br>
     *  <font size=-1>int &ge; 0 (default=100)</font></td>
     *  <td valign=top>(number of times the gaussian mutation got the gene out of range 
     *  before we give up and reset the gene's value; 0 means "never give up")</td>
     * </tr>
     *
     * </table>
     * @author Sean Luke, Gabriel Balan, Rafal Kicinger
     * @version 2.0
     */

    [Serializable]
    [ECConfiguration("ec.vector.FloatVectorSpecies")]
    public class FloatVectorSpecies : VectorSpecies
    {
        #region Constants

        public const string P_MINGENE = "min-gene";
        public const string P_MAXGENE = "max-gene";
        public const string P_MUTATIONTYPE = "mutation-type";
        public const string P_STDEV = "mutation-stdev";
        public const string P_MUTATION_DISTRIBUTION_INDEX = "mutation-distribution-index";
        public const string P_POLYNOMIAL_ALTERNATIVE = "alternative-polynomial-version";
        public const string P_OUTOFBOUNDS_RETRIES = "out-of-bounds-retries";
        public const string P_RANDOM_WALK_PROBABILITY = "random-walk-probability";

        // DEFINED ON VECTOR SPECIES
        //public const string P_NUM_SEGMENTS = "num-segments";
        //public const string P_SEGMENT_TYPE = "segment-type";
        //public const string P_SEGMENT_START = "start";
        //public const string P_SEGMENT_END = "end";
        //public const string P_SEGMENT = "segment";

        public const string P_MUTATION_BOUNDED = "mutation-bounded";

        public const int C_RESET_MUTATION = 0;
        public const int C_GAUSS_MUTATION = 1;
        public const int C_POLYNOMIAL_MUTATION = 2;
        public const int C_INTEGER_RESET_MUTATION = 3;
        public const int C_INTEGER_RANDOM_WALK_MUTATION = 4;

        public const string V_RESET_MUTATION = "reset";
        public const string V_GAUSS_MUTATION = "gauss";
        public const string V_POLYNOMIAL_MUTATION = "polynomial";
        public const string V_INTEGER_RANDOM_WALK_MUTATION = "integer-random-walk";
        public const string V_INTEGER_RESET_MUTATION = "integer-reset";

        public const int DEFAULT_OUT_OF_BOUNDS_RETRIES = 100;

        public const double SIMULATED_BINARY_CROSSOVER_EPS = 1.0e-14;

        #endregion // Constants

        #region Fields

        private bool _outOfBoundsRetriesWarningPrinted;

        /** Whether the mutationIsBounded value was defined, per gene.
            Used internally only.
            This array is one longer than the standard genome length.
            The top element in the array represents the parameters for genes in
            genomes which have extended beyond the genome length.  */
        private bool _mutationIsBoundedDefined;

        /** Whether the polymialIsAlternative value was defined, per gene.
            Used internally only.
            This array is one longer than the standard genome length.
            The top element in the array represents the parameters for genes in
            genomes which have extended beyond the genome length.  */
        private bool _polynomialIsAlternativeDefined;

        #endregion // Fields

        #region Properties

        public double[] MinGenes { get; set; }
        public double[] MaxGenes { get; set; }

        /** Mutation type, per gene.
            This array is one longer than the standard genome length.
            The top element in the array represents the parameters for genes in
            genomes which have extended beyond the genome length.  */
        public int[] MutationType { get; set; }

        /** Standard deviation for Gaussian Mutation, per gene.
            This array is one longer than the standard genome length.
            The top element in the array represents the parameters for genes in
            genomes which have extended beyond the genome length.  */
        public double[] GaussMutationStdev { get; set; }

        /** Whether mutation is bounded to the min- and max-gene values, per gene.
           This array is one longer than the standard genome length.
           The top element in the array represents the parameters for genes in
           genomes which have extended beyond the genome length.  */
        public bool[] MutationIsBounded { get; set; }

        public int OutOfBoundsRetries { get; set; } = 100;

        /** The distribution index for Polynomial Mutation, per gene.
            This array is one longer than the standard genome length.
            The top element in the array represents the parameters for genes in
            genomes which have extended beyond the genome length.  */
        public int[] MutationDistributionIndex { get; set; }

        /** Whether the Polynomial Mutation method is the "alternative" method, per gene.
            This array is one longer than the standard genome length.
            The top element in the array represents the parameters for genes in
            genomes which have extended beyond the genome length.  */
        public bool[] PolynomialIsAlternative { get; set; }

        /** The continuation probability for Integer Random Walk Mutation, per gene.
            This array is one longer than the standard genome length.
            The top element in the array represents the parameters for genes in
            genomes which have extended beyond the genome length.  */
        protected double[] RandomWalkProbability;

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            var def = DefaultBase;

            SetupGenome(state, paramBase);

            // OUT OF BOUNDS RETRIES

            OutOfBoundsRetries = state.Parameters.GetIntWithDefault(paramBase.Push(P_OUTOFBOUNDS_RETRIES), def.Push(P_OUTOFBOUNDS_RETRIES), DEFAULT_OUT_OF_BOUNDS_RETRIES);
            if (OutOfBoundsRetries < 0)
                state.Output.Fatal("Out of bounds retries must be >= 0.", paramBase.Push(P_OUTOFBOUNDS_RETRIES), def.Push(P_OUTOFBOUNDS_RETRIES));


            // CREATE THE ARRAYS

            MinGenes = new double[GenomeSize + 1];
            MaxGenes = new double[GenomeSize + 1];
            MutationType = Fill(new int[GenomeSize + 1], -1);
            GaussMutationStdev = Fill(new double[GenomeSize + 1], Double.NaN);
            MutationDistributionIndex = Fill(new int[GenomeSize + 1], -1);
            PolynomialIsAlternative = new bool[GenomeSize + 1];
            MutationIsBounded = new bool[GenomeSize + 1];
            RandomWalkProbability = Fill(new double[GenomeSize + 1], Double.NaN);


            // GLOBAL MIN/MAX GENES

            double _minGene = state.Parameters.GetDoubleWithDefault(paramBase.Push(P_MINGENE), def.Push(P_MINGENE), 0);
            double _maxGene = state.Parameters.GetDouble(paramBase.Push(P_MAXGENE), def.Push(P_MAXGENE), _minGene);
            if (_maxGene < _minGene)
                state.Output.Fatal("FloatVectorSpecies must have a default min-gene which is <= the default max-gene",
                    paramBase.Push(P_MAXGENE), def.Push(P_MAXGENE));
            Fill(MinGenes, _minGene);
            Fill(MaxGenes, _maxGene);



            /// MUTATION

            String mtype = state.Parameters.GetStringWithDefault(paramBase.Push(P_MUTATIONTYPE), def.Push(P_MUTATIONTYPE), null);
            int mutType = C_RESET_MUTATION;
            if (mtype == null)
                state.Output.Warning("No global mutation type given for FloatVectorSpecies, assuming 'reset' mutation",
                    paramBase.Push(P_MUTATIONTYPE), def.Push(P_MUTATIONTYPE));
            else if (mtype.Equals(V_RESET_MUTATION, StringComparison.InvariantCultureIgnoreCase))
                mutType = C_RESET_MUTATION; // redundant
            else if (mtype.Equals(V_POLYNOMIAL_MUTATION, StringComparison.InvariantCultureIgnoreCase))
                mutType = C_POLYNOMIAL_MUTATION; // redundant
            else if (mtype.Equals(V_GAUSS_MUTATION, StringComparison.InvariantCultureIgnoreCase))
                mutType = C_GAUSS_MUTATION;
            else if (mtype.Equals(V_INTEGER_RESET_MUTATION, StringComparison.InvariantCultureIgnoreCase))
            {
                mutType = C_INTEGER_RESET_MUTATION;
                state.Output.WarnOnce("Integer Reset Mutation used in FloatVectorSpecies.  Be advised that during initialization these genes will only be set to integer values.");
            }
            else if (mtype.Equals(V_INTEGER_RANDOM_WALK_MUTATION, StringComparison.InvariantCultureIgnoreCase))
            {
                mutType = C_INTEGER_RANDOM_WALK_MUTATION;
                state.Output.WarnOnce("Integer Random Walk Mutation used in FloatVectorSpecies.  Be advised that during initialization these genes will only be set to integer values.");
            }
            else
                state.Output.Fatal("FloatVectorSpecies given a bad mutation type: "
                    + mtype, paramBase.Push(P_MUTATIONTYPE), def.Push(P_MUTATIONTYPE));
            Fill(MutationType, mutType);


            if (mutType == C_POLYNOMIAL_MUTATION)
            {
                int mutDistIndex = state.Parameters.GetInt(paramBase.Push(P_MUTATION_DISTRIBUTION_INDEX), def.Push(P_MUTATION_DISTRIBUTION_INDEX), 0);
                if (mutDistIndex < 0)
                    state.Output.Fatal("If FloatVectorSpecies is going to use polynomial mutation as its global mutation type, the global distribution index must be defined and >= 0.",
                        paramBase.Push(P_MUTATION_DISTRIBUTION_INDEX), def.Push(P_MUTATION_DISTRIBUTION_INDEX));
                Fill(MutationDistributionIndex, mutDistIndex);

                if (!state.Parameters.ParameterExists(paramBase.Push(P_POLYNOMIAL_ALTERNATIVE), def.Push(P_POLYNOMIAL_ALTERNATIVE)))
                    state.Output.Warning("FloatVectorSpecies is using polynomial mutation as its global mutation type, but " + P_POLYNOMIAL_ALTERNATIVE + " is not defined.  Assuming 'true'");
                bool polyIsAlt = state.Parameters.GetBoolean(paramBase.Push(P_POLYNOMIAL_ALTERNATIVE), def.Push(P_POLYNOMIAL_ALTERNATIVE), true);
                Fill(PolynomialIsAlternative, polyIsAlt);
                _polynomialIsAlternativeDefined = true;
            }
            if (mutType == C_GAUSS_MUTATION)
            {
                double gaussMutStdev = state.Parameters.GetDouble(paramBase.Push(P_STDEV), def.Push(P_STDEV), 0);
                if (gaussMutStdev <= 0)
                    state.Output.Fatal("If it's going to use gaussian mutation as its global mutation type, FloatvectorSpecies must have a strictly positive standard deviation",
                        paramBase.Push(P_STDEV), def.Push(P_STDEV));
                Fill(GaussMutationStdev, gaussMutStdev);
            }
            if (mutType == C_INTEGER_RANDOM_WALK_MUTATION)
            {
                double randWalkProb = state.Parameters.GetDoubleWithMax(paramBase.Push(P_RANDOM_WALK_PROBABILITY), def.Push(P_RANDOM_WALK_PROBABILITY), 0.0, 1.0);
                if (randWalkProb <= 0)
                    state.Output.Fatal("If it's going to use random walk mutation as its global mutation type, FloatvectorSpecies must a random walk mutation probability between 0.0 and 1.0.",
                        paramBase.Push(P_RANDOM_WALK_PROBABILITY), def.Push(P_RANDOM_WALK_PROBABILITY));
                Fill(RandomWalkProbability, randWalkProb);
            }

            if (mutType == C_POLYNOMIAL_MUTATION ||
                mutType == C_GAUSS_MUTATION ||
                mutType == C_INTEGER_RANDOM_WALK_MUTATION)
            {
                if (!state.Parameters.ParameterExists(paramBase.Push(P_MUTATION_BOUNDED), def.Push(P_MUTATION_BOUNDED)))
                    state.Output.Warning("FloatVectorSpecies is using gaussian, polynomial, or integer random walk mutation as its global mutation type, but " + P_MUTATION_BOUNDED + " is not defined.  Assuming 'true'");
                bool mutIsBounded = state.Parameters.GetBoolean(paramBase.Push(P_MUTATION_BOUNDED), def.Push(P_MUTATION_BOUNDED), true);
                Fill(MutationIsBounded, mutIsBounded);
                _mutationIsBoundedDefined = true;
            }



            // CALLING SUPER

            // This will cause the remaining parameters to get set up, and
            // all per-gene and per-segment parameters to get set up as well.
            // We need to do this at this point because the global params need
            // to get set up first, and also prior to the prototypical individual
            // getting setup at the end of super.setup(...).

            base.Setup(state, paramBase);





            // VERIFY

            for (int x = 0; x < GenomeSize + 1; x++)
            {
                if (MaxGenes[x] != MaxGenes[x])  // uh oh, NaN
                    state.Output.Fatal("FloatVectorSpecies found that max-gene[" + x + "] is NaN");

                if (MinGenes[x] != MinGenes[x])  // uh oh, NaN
                    state.Output.Fatal("FloatVectorSpecies found that min-gene[" + x + "] is NaN");

                if (MaxGenes[x] < MinGenes[x])
                    state.Output.Fatal("FloatVectorSpecies must have a min-gene[" + x + "] which is <= the max-gene[" + x + "]");

                // check to see if these longs are within the data type of the particular individual
                if (!InNumericalTypeRange(MinGenes[x]))
                    state.Output.Fatal("This FloatvectorSpecies has a prototype of the kind: "
                        + I_Prototype.GetType().Name
                        + ", but doesn't have a min-gene["
                        + x
                        + "] value within the range of this prototype's genome's data types");
                if (!InNumericalTypeRange(MaxGenes[x]))
                    state.Output.Fatal("This FloatvectorSpecies has a prototype of the kind: "
                        + I_Prototype.GetType().Name
                        + ", but doesn't have a max-gene["
                        + x
                        + "] value within the range of this prototype's genome's data types");

                if (((MutationType[x] == FloatVectorSpecies.C_INTEGER_RESET_MUTATION ||
                            MutationType[x] == FloatVectorSpecies.C_INTEGER_RANDOM_WALK_MUTATION))  // integer type
                    && (MaxGenes[x] != Math.Floor(MaxGenes[x])))
                    state.Output.Fatal("Gene " + x + " is using an integer mutation method, but the max gene is not an integer (" + MaxGenes[x] + ").");

                if (((MutationType[x] == FloatVectorSpecies.C_INTEGER_RESET_MUTATION ||
                            MutationType[x] == FloatVectorSpecies.C_INTEGER_RANDOM_WALK_MUTATION))  // integer type
                    && (MinGenes[x] != Math.Floor(MinGenes[x])))
                    state.Output.Fatal("Gene " + x + " is using an integer mutation method, but the min gene is not an integer (" + MinGenes[x] + ").");
            }

            /*
            //Debugging
            for(int i = 0; i < minGene.length; i++)
            System.out.println("Min: " + MinGenes[i] + ", Max: " + MaxGenes[i]);
            */
        }

        protected void LoadParametersForGene(IEvolutionState state, int index, Parameter paramBase, Parameter def, String postfix)
        {
            base.LoadParametersForGene(state, index, paramBase, def, postfix);

            double minVal = state.Parameters.GetDoubleWithDefault(paramBase.Push(P_MINGENE).Push(postfix), def.Push(P_MINGENE).Push(postfix), Double.NaN);
            if (minVal == minVal)  // it's not NaN
            {
                //check if the value is in range
                if (!InNumericalTypeRange(minVal))
                    state.Output.Fatal("Min Gene Value out of range for data type " + I_Prototype.GetType().Name,
                        paramBase.Push(P_MINGENE).Push(postfix),
                        paramBase.Push(P_MINGENE).Push(postfix));
                else MinGenes[index] = minVal;

                if (DynamicInitialSize)
                    state.Output.WarnOnce("Using dynamic initial sizing, but per-gene or per-segment min-gene declarations.  This is probably wrong.  You probably want to use global min/max declarations.",
                        paramBase.Push(P_MINGENE).Push(postfix),
                        paramBase.Push(P_MINGENE).Push(postfix));
            }

            double maxVal = state.Parameters.GetDoubleWithDefault(paramBase.Push(P_MAXGENE).Push(postfix), def.Push(P_MAXGENE).Push(postfix), Double.NaN);
            if (maxVal == maxVal)  // it's not NaN
            {
                //check if the value is in range
                if (!InNumericalTypeRange(maxVal))
                    state.Output.Fatal("Max Gene Value out of range for data type " + I_Prototype.GetType().Name,
                        paramBase.Push(P_MAXGENE).Push(postfix),
                        paramBase.Push(P_MAXGENE).Push(postfix));
                else MaxGenes[index] = maxVal;

                if (DynamicInitialSize)
                    state.Output.WarnOnce("Using dynamic initial sizing, but per-gene or per-segment max-gene declarations.  This is probably wrong.  You probably want to use global min/max declarations.",
                        paramBase.Push(P_MAXGENE).Push(postfix),
                        paramBase.Push(P_MAXGENE).Push(postfix));
            }

            if ((maxVal == maxVal && !(minVal == minVal)))
                state.Output.Warning("Max Gene specified but not Min Gene", paramBase.Push(P_MINGENE).Push(postfix), def.Push(P_MINGENE).Push(postfix));

            if ((minVal == minVal && !(maxVal == maxVal)))
                state.Output.Warning("Min Gene specified but not Max Gene", paramBase.Push(P_MAXGENE).Push(postfix), def.Push(P_MINGENE).Push(postfix));


            /// MUTATION

            String mtype = state.Parameters.GetStringWithDefault(paramBase.Push(P_MUTATIONTYPE).Push(postfix), def.Push(P_MUTATIONTYPE).Push(postfix), null);
            int mutType = -1;
            if (mtype == null) { }  // we're cool
            else if (mtype.Equals(V_RESET_MUTATION, StringComparison.InvariantCultureIgnoreCase))
                mutType = MutationType[index] = C_RESET_MUTATION;
            else if (mtype.Equals(V_POLYNOMIAL_MUTATION, StringComparison.InvariantCultureIgnoreCase))
                mutType = MutationType[index] = C_POLYNOMIAL_MUTATION;
            else if (mtype.Equals(V_GAUSS_MUTATION, StringComparison.InvariantCultureIgnoreCase))
                mutType = MutationType[index] = C_GAUSS_MUTATION;
            else if (mtype.Equals(V_INTEGER_RESET_MUTATION, StringComparison.InvariantCultureIgnoreCase))
            {
                mutType = MutationType[index] = C_INTEGER_RESET_MUTATION;
                state.Output.WarnOnce("Integer Reset Mutation used in FloatVectorSpecies.  Be advised that during initialization these genes will only be set to integer values.");
            }
            else if (mtype.Equals(V_INTEGER_RANDOM_WALK_MUTATION, StringComparison.InvariantCultureIgnoreCase))
            {
                mutType = MutationType[index] = C_INTEGER_RANDOM_WALK_MUTATION;
                state.Output.WarnOnce("Integer Random Walk Mutation used in FloatVectorSpecies.  Be advised that during initialization these genes will only be set to integer values.");
            }
            else
                state.Output.Fatal("FloatVectorSpecies given a bad mutation type: " + mtype,
                    paramBase.Push(P_MUTATIONTYPE).Push(postfix), def.Push(P_MUTATIONTYPE).Push(postfix));


            if (mutType == C_POLYNOMIAL_MUTATION)
            {
                if (state.Parameters.ParameterExists(paramBase.Push(P_MUTATION_DISTRIBUTION_INDEX).Push(postfix), def.Push(P_MUTATION_DISTRIBUTION_INDEX).Push(postfix)))
                {
                    MutationDistributionIndex[index] = state.Parameters.GetInt(paramBase.Push(P_MUTATION_DISTRIBUTION_INDEX).Push(postfix), def.Push(P_MUTATION_DISTRIBUTION_INDEX).Push(postfix), 0);
                    if (MutationDistributionIndex[index] < 0)
                        state.Output.Fatal("If FloatVectorSpecies is going to use polynomial mutation as a per-gene or per-segment type, the global distribution index must be defined and >= 0.",
                            paramBase.Push(P_MUTATION_DISTRIBUTION_INDEX).Push(postfix), def.Push(P_MUTATION_DISTRIBUTION_INDEX).Push(postfix));
                }
                else if (MutationDistributionIndex[index] != MutationDistributionIndex[index])  // it's NaN
                    state.Output.Fatal("If FloatVectorSpecies is going to use polynomial mutation as a per-gene or per-segment type, either the global or per-gene/per-segment distribution index must be defined and >= 0.",
                        paramBase.Push(P_MUTATION_DISTRIBUTION_INDEX).Push(postfix), def.Push(P_MUTATION_DISTRIBUTION_INDEX).Push(postfix));

                if (state.Parameters.ParameterExists(paramBase.Push(P_POLYNOMIAL_ALTERNATIVE).Push(postfix), def.Push(P_POLYNOMIAL_ALTERNATIVE).Push(postfix)))
                {
                    PolynomialIsAlternative[index] = state.Parameters.GetBoolean(paramBase.Push(P_POLYNOMIAL_ALTERNATIVE).Push(postfix), def.Push(P_POLYNOMIAL_ALTERNATIVE).Push(postfix), true);
                }
            }
            if (mutType == C_GAUSS_MUTATION)
            {
                if (state.Parameters.ParameterExists(paramBase.Push(P_STDEV).Push(postfix), def.Push(P_STDEV).Push(postfix)))
                {
                    GaussMutationStdev[index] = state.Parameters.GetDouble(paramBase.Push(P_STDEV).Push(postfix), def.Push(P_STDEV).Push(postfix), 0);
                    if (GaussMutationStdev[index] <= 0)
                        state.Output.Fatal("If it's going to use gaussian mutation as a per-gene or per-segment type, it must have a strictly positive standard deviation",
                            paramBase.Push(P_STDEV).Push(postfix), def.Push(P_STDEV).Push(postfix));
                }
                else if (GaussMutationStdev[index] != GaussMutationStdev[index])
                    state.Output.Fatal("If FloatVectorSpecies is going to use polynomial mutation as a per-gene or per-segment type, either the global or per-gene/per-segment standard deviation must be defined.",
                        paramBase.Push(P_STDEV).Push(postfix), def.Push(P_STDEV).Push(postfix));
            }
            if (mutType == C_INTEGER_RANDOM_WALK_MUTATION)
            {
                if (state.Parameters.ParameterExists(paramBase.Push(P_RANDOM_WALK_PROBABILITY).Push(postfix), def.Push(P_RANDOM_WALK_PROBABILITY).Push(postfix)))
                {
                    RandomWalkProbability[index] = state.Parameters.GetDoubleWithMax(paramBase.Push(P_RANDOM_WALK_PROBABILITY).Push(postfix), def.Push(P_RANDOM_WALK_PROBABILITY).Push(postfix), 0.0, 1.0);
                    if (RandomWalkProbability[index] <= 0)
                        state.Output.Fatal("If it's going to use random walk mutation as a per-gene or per-segment type, FloatVectorSpecies must a random walk mutation probability between 0.0 and 1.0.",
                            paramBase.Push(P_RANDOM_WALK_PROBABILITY).Push(postfix), def.Push(P_RANDOM_WALK_PROBABILITY).Push(postfix));
                }
                else
                    state.Output.Fatal("If FloatVectorSpecies is going to use polynomial mutation as a per-gene or per-segment type, either the global or per-gene/per-segment random walk mutation probability must be defined.",
                        paramBase.Push(P_RANDOM_WALK_PROBABILITY).Push(postfix), def.Push(P_RANDOM_WALK_PROBABILITY).Push(postfix));
            }

            if (mutType == C_POLYNOMIAL_MUTATION ||
                mutType == C_GAUSS_MUTATION ||
                mutType == C_INTEGER_RANDOM_WALK_MUTATION)
            {
                if (state.Parameters.ParameterExists(paramBase.Push(P_MUTATION_BOUNDED).Push(postfix), def.Push(P_MUTATION_BOUNDED).Push(postfix)))
                {
                    MutationIsBounded[index] = state.Parameters.GetBoolean(paramBase.Push(P_MUTATION_BOUNDED).Push(postfix), def.Push(P_MUTATION_BOUNDED).Push(postfix), true);
                }
                else if (!_mutationIsBoundedDefined)
                    state.Output.Fatal("If FloatVectorSpecies is going to use gaussian, polynomial, or integer random walk mutation as a per-gene or per-segment type, the mutation bounding must be defined.",
                        paramBase.Push(P_MUTATION_BOUNDED).Push(postfix), def.Push(P_MUTATION_BOUNDED).Push(postfix));
            }

        }

        #endregion // Setup
        #region Operations

        #region Genome

        public void OutOfRangeRetryLimitReached(EvolutionState state)
        {
            state.Output.WarnOnce(
                "The limit of 'out-of-range' retries for gaussian or polynomial mutation (" 
                + DEFAULT_OUT_OF_BOUNDS_RETRIES + ") was reached.");
        }

        private void InitializeGenomeSegmentsByStartIndices(IEvolutionState state,
            IParameter paramBase, IParameter def, int numSegments, double minGene, double maxGene)
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
                    state.Output.Fatal("Invalid start index value for segment " + i + ": "
                        + currentSegmentEnd + "\nThe value must be smaller than "
                        + previousSegmentEnd + " and greater than or equal to  " + 0);

                //check if the index of the first segment is equal to 0
                if (i == 0 && currentSegmentEnd != 0)
                    state.Output.Fatal("Invalid start index value for the first segment " + i + ": "
                                            + currentSegmentEnd + "\nThe value must be equal to " + 0);


                //get min and max values of genes in this segment
                var currentSegmentMinGeneValue = Double.MaxValue;
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
                    currentSegmentMinGeneValue = state.Parameters.GetDoubleWithDefault(
                        paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE),
                        paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE), minGene);

                    //check if the value is in range
                    if (!InNumericalTypeRange(currentSegmentMinGeneValue))
                        state.Output.Error("This IntegerVectorSpecies has a prototype of the kind: "
                            + I_Prototype.GetType().FullName + ", but doesn't have a min-gene " + " value for segment "
                            + i + " within the range of this prototype's genome's data types",
                            paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE), paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE));
                }

                var currentSegmentMaxGeneValue = Double.MinValue;
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
                    currentSegmentMaxGeneValue = state.Parameters.GetDoubleWithDefault(
                        paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
                        paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE), maxGene);

                    //check if the value is in range
                    if (!InNumericalTypeRange(currentSegmentMaxGeneValue))
                        state.Output.Fatal("This IntegerVectorSpecies has a prototype of the kind: "
                            + I_Prototype.GetType().FullName + ", but doesn't have a max-gene "
                            + " value for segment " + i + " within the range of this prototype's genome's data types",
                            paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
                            paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE));
                }

                //check is min is smaller than or equal to max
                if (currentSegmentMaxGeneValue < currentSegmentMinGeneValue)
                    state.Output.Fatal("IntegerVectorSpecies must have a min-gene value for segment "
                                                              + i + " which is <= the max-gene value",
                                               paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
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
                                                                      int numSegments, double minGene, double maxGene)
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

                //check if the index of the final segment is equal to the GenomeSize
                if (i == numSegments - 1 && currentSegmentEnd != (GenomeSize - 1))
                    state.Output.Fatal("Invalid end index value for the last segment " + i + ": " + currentSegmentEnd
                        + "\nThe value must be equal to the index of the last gene in the genome:  " + (GenomeSize - 1));


                //get min and max values of genes in this segment
                var currentSegmentMinGeneValue = Double.MaxValue;
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
                    currentSegmentMinGeneValue = state.Parameters.GetDoubleWithDefault(
                                    paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE),
                                    paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE), minGene);

                    //check if the value is in range
                    if (!InNumericalTypeRange(currentSegmentMinGeneValue))
                        state.Output.Error("This IntegerVectorSpecies has a prototype of the kind: " + I_Prototype.GetType().FullName
                            + ", but doesn't have a min-gene " + " value for segment "
                            + i + " within the range of this prototype's genome's data types",
                            paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE),
                            paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE));
                }

                var currentSegmentMaxGeneValue = Double.MinValue;
                if (!state.Parameters.ParameterExists(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
                                             paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE)))
                {
                    if (!warnedMax)
                    {
                        state.Output.Warning("IntegerVectorSpecies has missing max-gene values for some segments.\n"
                            + "The first segment is #" + i + ".",
                            paramBase.Push(P_SEGMENT).Push("" + i),
                            paramBase.Push(P_SEGMENT).Push("" + i));

                        warnedMax = true;
                    }

                    //the max-gen value has not been defined for this segment so assume the global max value
                    currentSegmentMaxGeneValue = maxGene;
                }
                //get the max value for this segment
                else
                {
                    currentSegmentMaxGeneValue = state.Parameters.GetDoubleWithDefault(
                                    paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
                                    paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE), maxGene);

                    //check if the value is in range
                    if (!InNumericalTypeRange(currentSegmentMaxGeneValue))
                        state.Output.Fatal("This IntegerVectorSpecies has a prototype of the kind: "
                            + I_Prototype.GetType().FullName + ", but doesn't have a max-gene "
                            + " value for segment " + i + " within the range of this prototype's genome's data types",
                                                               paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
                                                               paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE));
                }

                //check is min is smaller than or equal to max
                if (currentSegmentMaxGeneValue < currentSegmentMinGeneValue)
                    state.Output.Fatal("IntegerVectorSpecies must have a min-gene value for segment "
                        + i + " which is <= the max-gene value",
                        paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
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
        public double GetMaxGene(int gene)
        {
            double[] m = MaxGenes;
            if (m.Length <= gene)
                gene = m.Length - 1;
            return m[gene];
        }

        public double GetMinGene(int gene)
        {
            double[] m = MinGenes;
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

        public double GetGaussMutationStdev(int gene)
        {
            double[] m = GaussMutationStdev;
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

        public int GetMutationDistributionIndex(int gene)
        {
            int[] m = MutationDistributionIndex;
            if (m.Length <= gene)
                gene = m.Length - 1;
            return m[gene];
        }

        public bool GetPolynomialIsAlternative(int gene)
        {
            bool[] m = PolynomialIsAlternative;
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

        #endregion // Genome
        #region Breeding

        public virtual void OutOfRangeRetryLimitReached(IEvolutionState state)
        {
            state.Output.WarnOnce(
                "The limit of 'out-of-range' retries for gaussian or polynomial mutation (" 
                + DEFAULT_OUT_OF_BOUNDS_RETRIES + ") was reached.");
        }

        public virtual bool InNumericalTypeRange(double geneVal)
        {
            if (I_Prototype is FloatVectorIndividual)
                return (geneVal <= Single.MaxValue && geneVal >= -Single.MaxValue);
            if (I_Prototype is DoubleVectorIndividual)
                return true;

            // geneVal is valid for all double
            return false; // dunno what the individual is...
        }

        #endregion // Breeding

        #endregion // Operations
    }
}