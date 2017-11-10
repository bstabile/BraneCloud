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

namespace BraneCloud.Evolution.EC.Breed
{
    /// <summary>
    /// MultiBreedingPipeline is a BreedingPipeline stores some <i>n</i> child sources; 
    /// each time it must produce an individual or two, 
    /// it picks one of these sources at random and has it do the production.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// If by <i>base</i>.<tt>generate-max</tt> is <tt>true</tt>, then always the maximum
    /// number of the typical numbers of any child source.  If <tt>false</tt>, then varies
    /// depending on the child source picked.
    /// <p/><b>Number of Sources</b><br/>
    /// Dynamic.  As many as the user specifies.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>generate-max</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> (default) or <tt>false</tt></font></td>
    /// <td valign="top">(Each time Produce(...) is called, should the MultiBreedingPipeline
    /// force all its sources to produce exactly the same number of individuals as the largest
    /// typical number of individuals produced by any source in the group?)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// breed.multibreed
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.breed.MultiBreedingPipeline")]
    public class MultiBreedingPipeline : BreedingPipeline
    {
        #region Constants

        public const string P_GEN_MAX = "generate-max";
        public const string P_MULTIBREED = "multibreed";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase => BreedDefaults.ParamBase.Push(P_MULTIBREED); 

        public override int NumSources => DYNAMIC_SOURCES;

        public int MaxGeneratable { get; set; }
        public bool GenerateMax { get; set; }

        /// <summary>
        /// Returns the max of TypicalIndsProduced of all its children. 
        /// </summary>
        public override int TypicalIndsProduced
        {
            get
            {
                if (MaxGeneratable == 0)
                    // not determined yet
                    MaxGeneratable = MaxChildProduction;
                return MaxGeneratable;
            }
        }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;

            var total = 0.0;

            if (Sources.Length == 0)  // uh oh
                state.Output.Fatal("num-sources must be provided and > 0 for MultiBreedingPipeline",
                    paramBase.Push(P_NUMSOURCES), def.Push(P_NUMSOURCES));

            for (var x = 0; x < Sources.Length; x++)
            {
                if (Sources[x].Probability < 0.0) // null checked from state.Output.error above
                    state.Output.Error("Pipe #" + x + " must have a probability >= 0.0", paramBase); // convenient that NO_PROBABILITY is -1...		
                else
                    total += Sources[x].Probability;
            }

            state.Output.ExitIfErrors();

            // Now check for nonzero probability (we know it's positive)
            if (total.Equals(0.0))
                state.Output.Warning("MultiBreedingPipeline's children have all zero probabilities -- this will be treated as a uniform distribution.  This could be an error.", paramBase);

            // allow all zero probabilities
            SetupProbabilities(Sources);

            GenerateMax = state.Parameters.GetBoolean(paramBase.Push(P_GEN_MAX), def.Push(P_GEN_MAX), true);
            MaxGeneratable = 0; // indicates that I don't know what it is yet.  

            // declare that likelihood isn't used
            if (Likelihood < 1.0)
                state.Output.Warning("MultiBreedingPipeline does not respond to the 'likelihood' parameter.",
                    paramBase.Push(P_LIKELIHOOD), def.Push(P_LIKELIHOOD));
        }

        #endregion // Setup
        #region Operations

        public override int Produce(
            int min, 
            int max, 
            int subpop, 
            IList<Individual> inds, 
            IEvolutionState state, int 
            thread,
            IDictionary<string, object> misc)
        {
            int start = inds.Count;

            var s = Sources[PickRandom(Sources, state.Random[thread].NextDouble())];
            int total;

            if (GenerateMax)
            {
                if (MaxGeneratable == 0)
                    MaxGeneratable = MaxChildProduction;
                var n = MaxGeneratable;
                if (n < min)
                    n = min;
                if (n > max)
                    n = max;

                total = s.Produce(n, n, subpop, inds, state, thread, misc);
            }
            else
            {
                total = s.Produce(min, max, subpop, inds, state, thread, misc);
            }

            return total;
        }

        #endregion // Operations
    }
}