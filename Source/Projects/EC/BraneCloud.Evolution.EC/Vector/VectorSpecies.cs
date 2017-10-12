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
    /// <summary> 
    /// VectorSpecies is a species which can create VectorIndividuals.  
    /// Different VectorSpecies are used for different kinds of VectorIndividuals: 
    /// a plain VectorSpecies is probably only applicable for BitVectorIndividuals.
    /// 
    /// <p/>VectorSpecies contains a number of parameters guiding how the individual
    /// crosses over and mutates.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.n</i>.<tt>genome-size</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(size of the genome)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>chunk-size</tt><br/>
    /// <font size="-1">1 &lt;= int &lt;= genome-size (default=1)</font></td>
    /// <td valign="top">(the chunk size for crossover (crossover will only occur on chunk boundaries))</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>crossover-type</tt><br/>
    /// <font size="-1">string, one of: one, two, any</font></td>
    /// <td valign="top">(default crossover type (one-point, two-point, or any-point (uniform) crossover)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>crossover-prob</tt><br/>
    /// <font size="-1">0.0 &gt;= float &gt;= 1.0 </font></td>
    /// <td valign="top">(probability that a gene will get crossed over during any-point crossover)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>mutation-prob</tt><br/>
    /// <font size="-1">0.0 &lt;= float &lt;= 1.0 </font></td>
    /// <td valign="top">(probability that a gene will get mutated over default mutation)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// vector.Species
    /// </summary>	
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

        /// <summary>
        /// We use warned here because it's quite a bit faster than calling warnOnce.
        /// </summary>
        protected bool Warned;

        #endregion // Fields
        #region Properties

        public override IParameter DefaultBase
        {
            get { return VectorDefaults.ParamBase.Push(P_VECTORSPECIES); }
        }

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
        /// Probability that a gene will mutate. 
        /// </summary>
        public float MutationProbability { get; set; }

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

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // Setup constraints  FIRST so the individuals can see them when they're
            // set up.

            var def = DefaultBase;

            _state = state;

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
                    state.Output.Error("VectorSpecies must have a genome size > 0",
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

            MutationProbability = state.Parameters.GetFloatWithMax(
                paramBase.Push(P_MUTATIONPROB), def.Push(P_MUTATIONPROB), 0.0, 1.0);
            if (MutationProbability == -1.0)
                state.Output.Error("VectorSpecies must have a mutation probability between 0.0 and 1.0 inclusive",
                                   paramBase.Push(P_MUTATIONPROB), def.Push(P_MUTATIONPROB));

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
                state.Output.Error("VectorSpecies given a bad crossover type: " + ctype,
                                   paramBase.Push(P_CROSSOVERTYPE), def.Push(P_CROSSOVERTYPE));

            if (CrossoverType == C_LINE_RECOMB || CrossoverType == C_INTERMED_RECOMB)
            {
                if (!(this is IntegerVectorSpecies) && !(this is FloatVectorSpecies))
                    state.Output.Error(
                        "Line and intermediate recombinations are only supported by IntegerVectorSpecies and FloatVectorSpecies",
                        paramBase.Push(P_CROSSOVERTYPE), def.Push(P_CROSSOVERTYPE));
                LineDistance = state.Parameters.GetDouble(
                    paramBase.Push(P_LINEDISTANCE), def.Push(P_LINEDISTANCE), 0.0);
                if (LineDistance == -1.0)
                    state.Output.Error(
                        "If it's going to use line or intermediate recombination, VectorSpecies needs a line extension >= 0.0  (0.25 is common)",
                        paramBase.Push(P_LINEDISTANCE), def.Push(P_LINEDISTANCE));
            }
            else LineDistance = 0.0;

            if (CrossoverType == C_ANY_POINT)
            {
                CrossoverProbability = state.Parameters.GetFloatWithMax(
                    paramBase.Push(P_CROSSOVERPROB), def.Push(P_CROSSOVERPROB), 0.0, 0.5);
                if (CrossoverProbability == -1.0)
                    state.Output.Error(
                        "If it's going to use any-point crossover, VectorSpecies must have a crossover probability between 0.0 and 0.5 inclusive",
                        paramBase.Push(P_CROSSOVERPROB), def.Push(P_CROSSOVERPROB));
            }
            else CrossoverProbability = 0.0f;

            if (CrossoverType == C_SIMULATED_BINARY)
            {
                if (!(this is FloatVectorSpecies))
                    state.Output.Error("Simulated binary crossover (SBX) is only supported by FloatVectorSpecies",
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
                    "The 'crossover-prob' parameter may only be used with any-point crossover.  It states the probability that a particular gene will be crossed over.  If you were looking for the probability of crossover happening at *all*, look at the 'likelihood' parameter.",
                    paramBase.Push(P_CROSSOVERPROB), def.Push(P_CROSSOVERPROB));

            // NOW call super.Setup(...), which will in turn set up the prototypical individual
            base.Setup(state, paramBase);
        }

        #endregion // Setup
        #region Operations

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
    }
}