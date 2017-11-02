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
     * VectorSpecies is a species which can create VectorIndividuals.  Different
     * VectorSpecies are used for different kinds of VectorIndividuals: a plain
     * VectorSpecies is probably only applicable for BitVectorIndividuals.
     * 
     * <p>VectorSpecies supports the following recombination methods:</p>
     * <ul>
     * <li><b>One-point crossover</b>.</li>
     * <li><b>Two-point crossover</b>.</li>
     * <li><b>Uniform crossover</b> - inaccurately called "any-point".</li>
     * <li><b>Line recombination</b> - children are random points on a line between
     *      the two parents.</li>
     * <li><b>Intermediate recombination</b> - the value of each component of the
     *      vector is between the values of that component of the parent vectors.
     *      </li>
     * </ul>
     * 
     * <P>Note that BitVectorIndividuals (which use VectorSpecies) and GeneVectorIndividuals
     * (which use GeneVectorSpecies, a subclass of VectorSpecies) do not support
     * Line or Intermediate Recombination.
     *
     * <p>Also note that for LongVectorIndividuals, there are certain values that will
     * never be created by line and intermediate recombination, because the
     * recombination is calculated using doubles and then rounded to the nearest
     * long. For large enough values (but still smaller than the maximum long), the
     * difference between one double and the next is greater than one.</p>
     *
     * <p>VectorSpecies has three wasy to determine the initial size of the individual:</p>
     * <ul>
     * <li><b>A fixed size</b>.</li>
     * <li><b>Geometric distribution</b>.</li>
     * <li><b>Uniform distribution</b></li>
     * </ul>
     *
     * <p>If the algorithm used is the geometric distribution, the VectorSpecies starts at a
     * minimum size and continues flipping a coin with a certain "resize probability",
     * increasing the size each time, until the coin comes up tails (fails).  The chunk size
     * must be 1 in this case.
     *
     * <p> If the algorithm used is the uniform distribution, the VectorSpecies picks a random
     * size between a provided minimum and maximum size, inclusive.  The chunk size
     * must be 1 in this case.
     *
     * <p>If the size is fixed, then you can also provide a "chunk size" which constrains the
     * locations in which crossover can be performed (only along chunk boundaries).  The genome
     * size must be a multiple of the chunk size in this case.
     *
     * <p>VectorSpecies also contains a number of parameters guiding how the individual
     * crosses over and mutates.
     *
     * <p><b>Per-Gene and Per-Segment Specification.</b>  VectorSpecies and its subclasses
     * specify a lot of parameters, notably mutation and initialization parameters, in one
     * of three ways.  We will use the <b><tt>mutation-probability</tt></b>
     * parameter as an example.
     *
     * <ol>
     * <li> Globally for all genes in the genome.
     *      This is done by specifying:
     *      <p><i>base</i>.<tt>mutation-probability</tt>
     *      <br><i>base</i>.<tt>max-gene</tt>
     *      <p><i>Note:</i> you <b>must</b> provide these values even if you don't use them,
     *      as they're used as defaults by #2 and #3 below.
     *<p>
     * <li> You may provide parameters for genes in segments (regions) along
     *      the genome.  The idea is to allow you to specify large chunks of genes
     *      all having the same parameter features.  
     *      To do this you must first specify how many segments there are:
     *      <p><i>base</i>.<tt>num-segments</tt>
     *      <p>The segments then may be defined by either start or end indices of genes. 
     *      This is controlled by specifying the value of:
     *      <p><i>base</i>.<tt>segment-type</tt>
     *      <p>...which can assume the value of start or end, with start being the default.
     *      The indices are defined using Java array style, i.e. the first gene has the index of 0, 
     *      and the last gene has the index of genome-size - 1.
     *      <p>Using this method, each segment is specified by<i>j</i>...
     *      <p><i>base</i>.<tt>segment.</tt><i>j</i><tt>.start</tt>
     *      <br><i>base</i>.<tt>segment.</tt><i>j</i><tt>.mutation-probability</tt>
     *      if segment-type value was chosen as start or by:
     *      <p><i>base</i>.<tt>segment.</tt><i>j</i><tt>.end</tt>
     *      <br><i>base</i>.<tt>segment.</tt><i>j</i><tt>.mutation-probability</tt>
     *      if segment-type value is equal to end.
     *<p>
     * <li> You may parameters for each separate gene.  
     *      This is done by specifying (for each gene location <i>i</i> you wish to specify)
     *      <p><i>base</i>.<tt>mutation-probability</tt>.<i>i</i>
     * </ol>
     * 
     * <p>Any settings for #3 override #2, and both override #1. 
     *
     * <p>The only parameter which can be specified this way in VectorSpecies is at present
     * <tt>mutation-probability</tt>.  However a number of parameters are specified this way
     * in subclasses. 
     
     <p><b>Parameters</b><br>
     <table>
     <tr><td valign=top><i>base</i>.<tt>genome-size</tt><br>
     <font size=-1>int &gt;= 1 or one of: geometric, uniform</font></td>
     <td valign=top>(size of the genome, or if 'geometric' or 'uniform', the algorithm used to size the initial genome)</td></tr>

     <tr><td valign=top><i>base</i>.<tt>chunk-size</tt><br>
     <font size=-1>1 &lt;= int &lt;= genome-size (default=1)</font></td>
     <td valign=top>(the chunk size for crossover (crossover will only occur on chunk boundaries))</td></tr>

     <tr><td valign=top><i>base</i>.<tt>geometric-prob</tt><br>
     <font size=-1>0.0 &lt;= float &lt; 1.0</font></td>
     <td valign=top>(the coin-flip probability for increasing the initial size using the geometric distribution)</td></tr>

     <tr><td valign=top><i>base</i>.<tt>min-initial-size</tt><br>
     <font size=-1>int &gt;= 0</font></td>
     <td valign=top>(the minimum initial size of the genome)</td></tr>

     <tr><td valign=top><i>base</i>.<tt>max-initial-size</tt><br>
     <font size=-1>int &gt;= min-initial-size</font></td>
     <td valign=top>(the maximum initial size of the genome)</td></tr>

     <tr><td valign=top><i>base</i>.<tt>crossover-type</tt><br>
     <font size=-1>string, one of: one, two, any</font></td>
     <td valign=top>(default crossover type (one-point, two-point, any-point (uniform), line, or intermediate)</td></tr>

     <tr><td valign=top><i>base</i>.<tt>crossover-prob</tt><br>
     <font size=-1>0.0 &gt;= float &gt;= 1.0 </font></td>
     <td valign=top>(probability that a gene will get crossed over during any-point or simulated binary crossover)</td></tr>

     <tr><td valign=top><i>base</i>.<tt>line-extension</tt><br>
     <font size=-1>float &gt;= 0.0 </font></td>
     <td valign=top>(for line and intermediate recombination, how far along the line or outside of the hypercube children can be. If this value is zero, all children must be within the hypercube.)


     <tr><td>&nbsp;
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>mutation-prob</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>segment</tt>.<i>segment-number</i>.<tt>mutation-prob</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>mutation-prob</tt>.<i>gene-number</i><br>
     <font size=-1>0.0 &lt;= float &lt;= 1.0 </font></td>
     <td valign=top>(probability that a gene will get mutated over default mutation)</td></tr>

     </table>

     <p><b>Default Base</b><br>
     vector.species

     * @author Sean Luke and Liviu Panait
     * @version 1.0 
     */

    [Serializable]
    [ECConfiguration("ec.vector.VectorSpecies")]
    public class VectorSpecies : Species
    {
        #region Constants

        public const string P_VECTORSPECIES = "species";
        
        public const string P_CROSSOVERTYPE = "crossover-type";
        public const string P_CHUNKSIZE = "chunk-size";
        public const string V_ONE_POINT = "one";
        public const string V_TWO_POINT = "two";
        public const string V_ANY_POINT = "any";
        public const string V_LINE_RECOMB = "line";
        public const string V_INTERMED_RECOMB = "intermediate";
        public const string V_SIMULATED_BINARY = "sbx";
        public const string P_CROSSOVER_DISTRIBUTION_INDEX = "crossover-distribution-index";
        public const string P_MUTATIONPROB = "mutation-prob";
        public const string P_CROSSOVERPROB = "crossover-prob";
        public const string P_GENOMESIZE = "genome-size";
        public const string P_LINEDISTANCE = "line-extension";
        public const string V_GEOMETRIC = "geometric";
        public const string P_GEOMETRIC_PROBABILITY = "geometric-prob";
        public const string V_UNIFORM = "uniform";
        public const string P_UNIFORM_MIN = "min-initial-size";
        public const string P_UNIFORM_MAX = "max-initial-size";
        public const string P_NUM_SEGMENTS = "num-segments";
        public const string P_SEGMENT_TYPE = "segment-type";
        public const string P_SEGMENT_START = "start";
        public const string P_SEGMENT_END = "end";
        public const string P_SEGMENT = "segment";
        public const string P_DUPLICATE_RETRIES = "duplicate-retries";


        public const int C_ONE_POINT = 0;
        public const int C_TWO_POINT = 1;
        public const int C_ANY_POINT = 128;
        public const int C_LINE_RECOMB = 256;
        public const int C_INTERMED_RECOMB = 512;
        public const int C_SIMULATED_BINARY = 1024;
        public const int C_NONE = 0;
        public const int C_GEOMETRIC = 1;
        public const int C_UNIFORM = 2;

        #endregion // Constants
        #region Fields

        IEvolutionState _state;

        #endregion // Fields
        #region Properties

        public override IParameter DefaultBase => VectorDefaults.ParamBase.Push(P_VECTORSPECIES);

        /// <summary>
        /// We use warned here because it's quite a bit faster than calling warnOnce.
        /// </summary>
        protected bool Warned { get; set; }

        /// <summary>
        /// How often do we retry until we get a non-duplicate gene?
        /// </summary>
        protected int[] DuplicateRetries { get; set; }

        /// <summary>
        /// How big of a genome should we create on initialization? 
        /// </summary>
        public int GenomeSize { get; set; }

        /// <summary>
        /// How should we reset the genome?
        /// </summary>
        public int GenomeResizeAlgorithm { get; set; }

        /// <summary>
        /// What's the smallest legal genome?
        /// </summary>
        public int MinInitialSize { get; set; }

        /// <summary>
        /// What's the largest legal genome?
        /// </summary>
        public int MaxInitialSize { get; set; }

        /// <summary>
        ///  With what probability would our genome be at least 1 larger than it is now during initialization?
        /// </summary>
        public float GenomeIncreaseProbability { get; set; }

        /// <summary>
        /// Was the initial size determined dynamically?
        /// </summary>
        public bool DynamicInitialSize { get; set; }

        /// <summary>
        /// Probability that a gene will mutate, per gene.
        /// This array is one longer than the standard genome length.
        /// The top element in the array represents the parameters for genes in
        /// genomes which have extended beyond the genome length.
        /// </summary>
        public double[] MutationProbability { get; set; }

        /// <summary>
        /// Probability that a gene will cross over -- ONLY used in V_ANY_POINT crossover. 
        /// </summary>
        public float CrossoverProbability { get; set; }

        /// <summary>
        /// What kind of crossover do we have? 
        /// </summary>
        public int CrossoverType { get; set; }

        /// <summary>
        /// What should the SBX distribution index be?
        /// </summary>
        public int CrossoverDistributionIndex { get; set; }

        /// <summary>
        /// How big of chunks should we define for crossover?
        /// </summary>
        public int ChunkSize { get; set; }

        /// <summary>
        /// How far along the long a child can be located for line or intermediate recombination.
        /// </summary>
        public double LineDistance { get; set; }

        #endregion // Properties
        #region Setup

        public void SetupGenome(IEvolutionState state, IParameter paramBase)
        {
            // Setup constraints  FIRST so the individuals can see them when they're
            // set up.

            var def = DefaultBase;

            var genomeSizeForm = state.Parameters.GetString(paramBase.Push(P_GENOMESIZE), def.Push(P_GENOMESIZE));
            if (genomeSizeForm == null) // clearly an error
            {
                state.Output.Fatal("No genome size specified.", paramBase.Push(P_GENOMESIZE), def.Push(P_GENOMESIZE));
            }
            else if (genomeSizeForm.Equals(V_GEOMETRIC))
            {
                DynamicInitialSize = true;
                GenomeSize = 1;
                GenomeResizeAlgorithm = C_GEOMETRIC;
                ChunkSize = state.Parameters.GetIntWithDefault(paramBase.Push(P_CHUNKSIZE), def.Push(P_CHUNKSIZE), 1);
                if (ChunkSize != 1)
                    state.Output.Fatal("To use Geometric size initialization, VectorSpecies must have a chunksize of 1",
                                       paramBase.Push(P_CHUNKSIZE), def.Push(P_CHUNKSIZE));
                MinInitialSize = state.Parameters.GetInt(paramBase.Push(P_UNIFORM_MIN), def.Push(P_UNIFORM_MIN), 0);
                if (MinInitialSize < 0)
                {
                    state.Output.Warning(
                        "Gemoetric size initialization used, but no minimum initial size provided.  Assuming minimum is 0.");
                    MinInitialSize = 0;
                }
                GenomeIncreaseProbability = state.Parameters.GetFloatWithMax(paramBase.Push(P_GEOMETRIC_PROBABILITY),
                                                                             def.Push(P_GEOMETRIC_PROBABILITY), 0.0, 1.0);
                if (GenomeIncreaseProbability < 0.0 || GenomeIncreaseProbability >= 1.0) // note >=
                    state.Output.Fatal(
                        "To use Gemoetric size initialization, the genome increase probability must be >= 0.0 and < 1.0",
                        paramBase.Push(P_GEOMETRIC_PROBABILITY), def.Push(P_GEOMETRIC_PROBABILITY));
            }
            else if (genomeSizeForm.Equals(V_UNIFORM))
            {
                DynamicInitialSize = true;
                GenomeSize = 1;
                GenomeResizeAlgorithm = C_UNIFORM;
                ChunkSize = state.Parameters.GetIntWithDefault(paramBase.Push(P_CHUNKSIZE), def.Push(P_CHUNKSIZE), 1);
                if (ChunkSize != 1)
                    state.Output.Fatal("To use Uniform size initialization, VectorSpecies must have a chunksize of 1",
                                       paramBase.Push(P_CHUNKSIZE), def.Push(P_CHUNKSIZE));
                MinInitialSize = state.Parameters.GetInt(paramBase.Push(P_UNIFORM_MIN), def.Push(P_UNIFORM_MIN), 0);
                if (MinInitialSize < 0)
                    state.Output.Fatal("To use Uniform size initialization, you must set a minimum initial size >= 0",
                                       paramBase.Push(P_UNIFORM_MIN), def.Push(P_UNIFORM_MIN));
                MaxInitialSize = state.Parameters.GetInt(paramBase.Push(P_UNIFORM_MAX), def.Push(P_UNIFORM_MAX), 0);
                if (MaxInitialSize < 0)
                    state.Output.Fatal("To use Uniform size initialization, you must set a maximum initial size >= 0",
                                       paramBase.Push(P_UNIFORM_MAX), def.Push(P_UNIFORM_MAX));
                if (MaxInitialSize < MinInitialSize)
                    state.Output.Fatal(
                        "To use Uniform size initialization, you must set a maximum initial size >= the minimum initial size",
                        paramBase.Push(P_UNIFORM_MAX), def.Push(P_UNIFORM_MAX));
            }
            else // it's a number
            {
                GenomeSize = state.Parameters.GetInt(paramBase.Push(P_GENOMESIZE), def.Push(P_GENOMESIZE), 1);
                if (GenomeSize == 0)
                    state.Output.Fatal("VectorSpecies must have a genome size > 0",
                                       paramBase.Push(P_GENOMESIZE), def.Push(P_GENOMESIZE));

                GenomeResizeAlgorithm = C_NONE;

                ChunkSize = state.Parameters.GetIntWithDefault(paramBase.Push(P_CHUNKSIZE), def.Push(P_CHUNKSIZE), 1);
                if (ChunkSize <= 0 || ChunkSize > GenomeSize)
                    state.Output.Fatal("VectorSpecies must have a chunksize which is > 0 and < genomeSize",
                                       paramBase.Push(P_CHUNKSIZE), def.Push(P_CHUNKSIZE));
                if (GenomeSize % ChunkSize != 0)
                    state.Output.Fatal("VectorSpecies must have a genomeSize which is a multiple of chunksize",
                                       paramBase.Push(P_CHUNKSIZE), def.Push(P_CHUNKSIZE));
            }
        }


        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            _state = state;

            IParameter def = DefaultBase;

            base.Setup(state, paramBase);

            // this might get called twice, I don't think it's a big deal
            SetupGenome(state, paramBase);

            // MUTATION

            double mutProb = state.Parameters.GetDoubleWithMax(paramBase.Push(P_MUTATIONPROB), def.Push(P_MUTATIONPROB), 0.0, 1.0);
            if (mutProb == -1.0)
                state.Output.Fatal("Global mutation probability must be between 0.0 and 1.0 inclusive",
                    paramBase.Push(P_MUTATIONPROB), def.Push(P_MUTATIONPROB));
            MutationProbability = Fill(new double[GenomeSize + 1], mutProb);

            int dupRetries = state.Parameters.GetIntWithDefault(paramBase.Push(P_DUPLICATE_RETRIES), def.Push(P_DUPLICATE_RETRIES), 0);
            if (dupRetries < 0)
            {
                state.Output.Fatal("Duplicate Retries, if defined, must be a value >= 0", paramBase.Push(P_DUPLICATE_RETRIES), def.Push(P_DUPLICATE_RETRIES));
            }
            DuplicateRetries = Fill(new int[GenomeSize + 1], dupRetries);

            // CROSSOVER

            var ctype = state.Parameters.GetStringWithDefault(paramBase.Push(P_CROSSOVERTYPE), def.Push(P_CROSSOVERTYPE), null);
            CrossoverType = C_ONE_POINT;
            if (ctype == null)
                state.Output.Warning("No crossover type given for VectorSpecies, assuming one-point crossover",
                                     paramBase.Push(P_CROSSOVERTYPE), def.Push(P_CROSSOVERTYPE));
            else if (ctype.ToUpper().Equals(V_ONE_POINT.ToUpper()))
                CrossoverType = C_ONE_POINT; // redundant
            else if (ctype.ToUpper().Equals(V_TWO_POINT.ToUpper()))
                CrossoverType = C_TWO_POINT;
            else if (ctype.ToUpper().Equals(V_ANY_POINT.ToUpper()))
                CrossoverType = C_ANY_POINT;
            else if (ctype.ToUpper().Equals(V_LINE_RECOMB.ToUpper()))
                CrossoverType = C_LINE_RECOMB;
            else if (ctype.ToUpper().Equals(V_INTERMED_RECOMB.ToUpper()))
                CrossoverType = C_INTERMED_RECOMB;
            else if (ctype.ToUpper().Equals(V_SIMULATED_BINARY.ToUpper()))
                CrossoverType = C_SIMULATED_BINARY;
            else
                state.Output.Fatal("VectorSpecies given a bad crossover type: " + ctype,
                                   paramBase.Push(P_CROSSOVERTYPE), def.Push(P_CROSSOVERTYPE));

            if (CrossoverType == C_LINE_RECOMB || CrossoverType == C_INTERMED_RECOMB)
            {
                if (!(this is IntegerVectorSpecies) && !(this is FloatVectorSpecies))
                    state.Output.Fatal(
                        "Line and intermediate recombinations are only supported by IntegerVectorSpecies and FloatVectorSpecies",
                        paramBase.Push(P_CROSSOVERTYPE), def.Push(P_CROSSOVERTYPE));
                LineDistance = state.Parameters.GetDouble(
                    paramBase.Push(P_LINEDISTANCE), def.Push(P_LINEDISTANCE), 0.0);
                if (LineDistance == -1.0)
                    state.Output.Fatal(
                        "If it's going to use line or intermediate recombination, VectorSpecies needs a line extension >= 0.0  (0.25 is common)",
                        paramBase.Push(P_LINEDISTANCE), def.Push(P_LINEDISTANCE));
            }
            else LineDistance = 0.0;

            if (CrossoverType == C_ANY_POINT)
            {
                CrossoverProbability = state.Parameters.GetFloatWithMax(
                    paramBase.Push(P_CROSSOVERPROB), def.Push(P_CROSSOVERPROB), 0.0, 0.5);
                if (CrossoverProbability == -1.0)
                    state.Output.Fatal(
                        "If it's going to use any-point crossover, VectorSpecies must have a crossover probability between 0.0 and 0.5 inclusive",
                        paramBase.Push(P_CROSSOVERPROB), def.Push(P_CROSSOVERPROB));
            }
            else if (CrossoverType == C_SIMULATED_BINARY)
            {
                if (!(this is FloatVectorSpecies))
                    state.Output.Fatal("Simulated binary crossover (SBX) is only supported by FloatVectorSpecies",
                                       paramBase.Push(P_CROSSOVERTYPE), def.Push(P_CROSSOVERTYPE));
                CrossoverDistributionIndex = state.Parameters.GetInt(paramBase.Push(P_CROSSOVER_DISTRIBUTION_INDEX),
                                                                     def.Push(P_CROSSOVER_DISTRIBUTION_INDEX), 0);
                if (CrossoverDistributionIndex < 0)
                    state.Output.Fatal(
                        "If FloatVectorSpecies is going to use simulated binary crossover (SBX), the distribution index must be defined and >= 0.",
                        paramBase.Push(P_CROSSOVER_DISTRIBUTION_INDEX), def.Push(P_CROSSOVER_DISTRIBUTION_INDEX));
            }
            else CrossoverProbability = 0.0f;

            state.Output.ExitIfErrors();

            if (CrossoverType != C_ANY_POINT &&
                state.Parameters.ParameterExists(paramBase.Push(P_CROSSOVERPROB), def.Push(P_CROSSOVERPROB)))
                state.Output.Warning(
                    "The 'crossover-prob' parameter may only be used with any-point crossover.  It states the probability that a particular gene will be crossed over.  If you were looking for the probability of crossover happening at ///all///, look at the 'likelihood' parameter.",
                    paramBase.Push(P_CROSSOVERPROB), def.Push(P_CROSSOVERPROB));

            // SEGMENTS

            // Set number of segments to 0 by default
            // Now check to see if segments of genes (genes having the same min and
            // max values) exist
            if (state.Parameters.ParameterExists(paramBase.Push(P_NUM_SEGMENTS), def.Push(P_NUM_SEGMENTS)))
            {
                if (DynamicInitialSize)
                    state.Output.WarnOnce("Using dynamic initial sizing, but per-segment min/max gene declarations.  This is probably wrong.  You probably want to use global min/max declarations.",
                        paramBase.Push(P_NUM_SEGMENTS), def.Push(P_NUM_SEGMENTS));

                var numSegments = state.Parameters.GetIntWithDefault(paramBase.Push(P_NUM_SEGMENTS),
                    def.Push(P_NUM_SEGMENTS), 0);

                if (numSegments == 0)
                    state.Output.Warning(
                        "The number of genome segments has been defined to be equal to 0.\n"
                        + "Hence, no genome segments will be defined.",
                        paramBase.Push(P_NUM_SEGMENTS),
                        def.Push(P_NUM_SEGMENTS));
                else if (numSegments < 0)
                    state.Output.Fatal(
                        "Invalid number of genome segments: " + numSegments
                        + "\nIt must be a nonnegative value.",
                        paramBase.Push(P_NUM_SEGMENTS),
                        def.Push(P_NUM_SEGMENTS));

                //read the type of segment definition using the default start value
                String segmentType = state.Parameters.GetStringWithDefault(paramBase.Push(P_SEGMENT_TYPE),
                    def.Push(P_SEGMENT_TYPE), P_SEGMENT_START);

                if (segmentType.Equals(P_SEGMENT_START, StringComparison.InvariantCultureIgnoreCase))
                    InitializeGenomeSegmentsByStartIndices(state, paramBase, def, numSegments);
                else if (segmentType.Equals(P_SEGMENT_END, StringComparison.InvariantCultureIgnoreCase))
                    InitializeGenomeSegmentsByEndIndices(state, paramBase, def, numSegments);
                else
                    state.Output.Fatal(
                        "Invalid specification of genome segment type: " + segmentType
                        + "\nThe " + P_SEGMENT_TYPE + " parameter must have the value of " + P_SEGMENT_START + " or " + P_SEGMENT_END,
                        paramBase.Push(P_SEGMENT_TYPE),
                        def.Push(P_SEGMENT_TYPE));
            }
            state.Output.ExitIfErrors();

            // PER-GENE VALUES
            for (int x = 0; x < GenomeSize; x++)
            {
                LoadParametersForGene(state, x, paramBase, def, "" + x);
            }
            state.Output.ExitIfErrors();

            // NOW call base.setup(...), which will in turn set up the prototypical individual
            base.Setup(state, paramBase);
        }

        /** Called when VectorSpecies is setting up per-gene and per-segment parameters.  The index
            is the current gene whose parameter is getting set up.  The Parameters in question are the
            bases for the gene.  The postfix should be appended to the end of any parameter looked up
            (it often contains a number indicating the gene in question), such as
            state.Parameters.ParameterExists(paramBase.Push(P_PARAM).Push(postfix), def.Push(P_PARAM).Push(postfix)
                            
            <p>If you override this method, be sure to call super(...) at some point, ideally first.
        */
        protected virtual void LoadParametersForGene(IEvolutionState state, int index, IParameter paramBase, IParameter def, String postfix)
        {
            // our only per-gene parameter is mutation probablity.

            if (state.Parameters.ParameterExists(paramBase.Push(P_MUTATIONPROB).Push(postfix), def.Push(P_MUTATIONPROB).Push(postfix)))
            {
                MutationProbability[index] = state.Parameters.GetDoubleWithMax(paramBase.Push(P_MUTATIONPROB).Push(postfix), def.Push(P_MUTATIONPROB).Push(postfix), 0.0, 1.0);
                if (MutationProbability[index] == -1.0)
                    state.Output.Fatal("Per-gene or per-segment mutation probability must be between 0.0 and 1.0 inclusive",
                        paramBase.Push(P_MUTATIONPROB).Push(postfix), def.Push(P_MUTATIONPROB).Push(postfix));
            }

            if (state.Parameters.ParameterExists(paramBase.Push(P_DUPLICATE_RETRIES).Push(postfix), def.Push(P_DUPLICATE_RETRIES).Push(postfix)))
            {
                DuplicateRetries[index] = state.Parameters.GetInt(paramBase.Push(P_DUPLICATE_RETRIES).Push(postfix), def.Push(P_DUPLICATE_RETRIES).Push(postfix));
                if (DuplicateRetries[index] < 0)
                    state.Output.Fatal("Duplicate Retries for gene " + index + ", if defined must be a value >= 0",
                        paramBase.Push(P_DUPLICATE_RETRIES).Push(postfix), def.Push(P_DUPLICATE_RETRIES).Push(postfix));
            }

        }

        /// <summary>
        /// Looks up genome segments using start indices.  Segments run up to the next declared start index.
        /// </summary>
        protected void InitializeGenomeSegmentsByStartIndices(IEvolutionState state,
            IParameter paramBase,
            IParameter def,
            int numSegments)
        {
            //loop in reverse order 
            int previousSegmentEnd = GenomeSize;
            int currentSegmentEnd = 0;

            for (int i = numSegments - 1; i >= 0; i--)
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
                    state.Output.Fatal("Genome segment " + i + " has not been defined!" +
                        "\nYou must specify start indices for " + numSegments + " segment(s)",
                        paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_START),
                        paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_START));
                }

                //check if the start index is valid
                if (currentSegmentEnd >= previousSegmentEnd || currentSegmentEnd < 0)
                    state.Output.Fatal(
                        "Invalid start index value for segment " + i + ": " + currentSegmentEnd
                        + "\nThe value must be smaller than " + previousSegmentEnd +
                        " and greater than or equal to  " + 0);

                //check if the index of the first segment is equal to 0
                if (i == 0 && currentSegmentEnd != 0)
                    state.Output.Fatal(
                        "Invalid start index value for the first segment " + i + ": " + currentSegmentEnd
                        + "\nThe value must be equal to " + 0);

                //and assign min and max values for all genes in this segment
                for (int j = previousSegmentEnd - 1; j >= currentSegmentEnd; j--)
                {
                    LoadParametersForGene(state, j, paramBase.Push(P_SEGMENT).Push("" + i), def.Push(P_SEGMENT).Push("" + i), "");
                }

                previousSegmentEnd = currentSegmentEnd;
            }
        }

        /** Looks up genome segments using end indices.  Segments run from the previously declared end index. */
        protected void InitializeGenomeSegmentsByEndIndices(IEvolutionState state,
            IParameter paramBase,
            IParameter def,
            int numSegments)
        {
            int previousSegmentEnd = -1;
            int currentSegmentEnd = 0;
            // iterate over segments and set genes values for each segment
            for (int i = 0; i < numSegments; i++)
            {
                //check if the segment data exist
                if (state.Parameters.ParameterExists(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END), def.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END)))
                {
                    //Read the index of the end gene specifying current segment
                    currentSegmentEnd = state.Parameters.GetInt(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END),
                        def.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END));

                }
                else
                {
                    state.Output.Fatal("Genome segment " + i + " has not been defined!" +
                        "\nYou must specify end indices for " + numSegments + " segment(s)",
                        paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END),
                        paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END));
                }

                //check if the end index is valid
                if (currentSegmentEnd <= previousSegmentEnd || currentSegmentEnd >= GenomeSize)
                    state.Output.Fatal(
                        "Invalid end index value for segment " + i + ": " + currentSegmentEnd
                        + "\nThe value must be greater than " + previousSegmentEnd +
                        " and smaller than " + GenomeSize);

                //check if the index of the segment is equal to the GenomeSize
                if (i == numSegments - 1 && currentSegmentEnd != (GenomeSize - 1))
                    state.Output.Fatal(
                        "Invalid end index value for the last segment " + i + ": " + currentSegmentEnd
                        + "\nThe value must be equal to the index of the last gene in the genome:  " + (GenomeSize - 1));


                //and assign min and max values for all genes in this segment
                for (int j = previousSegmentEnd + 1; j <= currentSegmentEnd; j++)
                {
                    LoadParametersForGene(state, j, paramBase.Push(P_SEGMENT).Push("" + i), def.Push(P_SEGMENT).Push("" + i), "");
                }

                previousSegmentEnd = currentSegmentEnd;
            }
        }

        #endregion // Setup
        #region Operations

        public double GetMutationProbability(int gene)
        {
            double[] m = MutationProbability;
            if (m.Length <= gene)
                gene = m.Length - 1;
            return m[gene];
        }

        public int GetDuplicateRetries(int gene)
        {
            int[] m = DuplicateRetries;
            if (m.Length <= gene)
                gene = m.Length - 1;
            return m[gene];
        }

        public override Individual NewIndividual(IEvolutionState state, int thread)
        {
            var newind = (VectorIndividual)(base.NewIndividual(state, thread));

            if (GenomeResizeAlgorithm == C_NONE)
                newind.Reset(state, thread);
            else if (GenomeResizeAlgorithm == C_UNIFORM)
            {
                var size = state.Random[thread].NextInt(MaxInitialSize - MinInitialSize + 1) + MinInitialSize;
                newind.Reset(state, thread, size);
            }
            else if (GenomeResizeAlgorithm == C_GEOMETRIC)
            {
                var size = MinInitialSize;
                while (state.Random[thread].NextBoolean(GenomeIncreaseProbability)) size++;
                newind.Reset(state, thread, size);
            }

            return newind;
        }

        protected void WarnAboutGene(int gene)
        {
            _state.Output.WarnOnce("Attempt to access MaxGene or MinGene from IntegerVectorSpecies beyond initial GenomeSize.\n" +
                "From now on, MaxGene(a) = MaxGene(MaxGeneIndex) for a >= MaxGeneIndex.  Likewise for MinGene(...)");
            Warned = true;
        }

        #endregion // Operations

        #region Utility Methods

        // These convenience methods are used by subclasses to fill arrays and check to see if
        // arrays contain certain values.

        /** Utility method: fills the array with the given value and returns it. */
        protected long[] Fill(long[] array, long val)
        {
            for (int i = 0; i < array.Length; i++) array[i] = val;
            return array;
        }

        /** Utility method: fills the array with the given value and returns it. */
        protected int[] Fill(int[] array, int val)
        {
            for (int i = 0; i < array.Length; i++) array[i] = val;
            return array;
        }

        /** Utility method: fills the array with the given value and returns it. */
        protected bool[] Fill(bool[] array, bool val)
        {
            for (int i = 0; i < array.Length; i++) array[i] = val;
            return array;
        }

        /** Utility method: fills the array with the given value and returns it. */
        protected double[] Fill(double[] array, double val)
        {
            for (int i = 0; i < array.Length; i++) array[i] = val;
            return array;
        }

        /** Utility method: returns the first array slot which contains the given value, else -1. */
        protected int Contains(bool[] array, bool val)
        {
            for (int i = 0; i < array.Length; i++)
                if (array[i] == val) return i;
            return -1;
        }

        /** Utility method: returns the first array slot which contains the given value, else -1. */
        protected int Contains(long[] array, long val)
        {
            for (int i = 0; i < array.Length; i++)
                if (array[i] == val) return i;
            return -1;
        }

        /** Utility method: returns the first array slot which contains the given value, else -1. */
        protected int Contains(int[] array, int val)
        {
            for (int i = 0; i < array.Length; i++)
                if (array[i] == val) return i;
            return -1;
        }

        /** Utility method: returns the first array slot which contains the given value, else -1. */
        protected int Contains(double[] array, double val)
        {
            for (int i = 0; i < array.Length; i++)
                if (array[i] == val) return i;
            return -1;
        }

        #endregion
    }
}