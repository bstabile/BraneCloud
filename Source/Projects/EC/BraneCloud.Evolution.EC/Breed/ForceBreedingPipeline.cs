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

namespace BraneCloud.Evolution.EC.Breed
{
    /// <summary> 
    /// ForceBreedingPipeline has one source.  To fill its quo for Produce(...),
    /// ForceBreedingPipeline repeatedly forces its source to produce exactly NumInds
    /// individuals at a time, except possibly the last time, where the number of
    /// individuals its source produces may be as low as 1.  This is useful for forcing
    /// Crossover to produce only one individual, or mutation to produce 2 individuals
    /// always, etc.
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// Determined by <i>base</i>.<tt>num-inds</tt>
    /// <p/><b>Number of Sources</b><br/>
    /// 1
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>num-inds</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(The number of individuals this breeding pipeline will force its
    /// source to produce each tim in order to fill the quo for Produce(...).)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// breed.force
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.breed.ForceBreedingPipeline")]
    public class ForceBreedingPipeline : BreedingPipeline
    {
        #region Constants

        public const string P_NUMINDS = "num-inds";
        public const string P_FORCE = "force";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return BreedDefaults.ParamBase.Push(P_FORCE); }
        }

        public override int NumSources
        {
            get { return 1; }
        }

        public int NumInds { get; set; }

        /// <summary>
        /// Returns the max of TypicalIndsProduced of all its children. 
        /// </summary>
        public override int TypicalIndsProduced
        {
            get { return NumInds; }
        }

        #endregion// Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            var def = DefaultBase;

            NumInds = state.Parameters.GetInt(paramBase.Push(P_NUMINDS), def.Push(P_NUMINDS), 1);
            if (NumInds == 0)
                state.Output.Fatal("ForceBreedingPipeline must produce at least 1 child at a time", paramBase.Push(P_NUMINDS), def.Push(P_NUMINDS));

            // declare that likelihood isn't used
            if (Likelihood < 1.0f)
                state.Output.Warning("ForceBreedingPipeline does not respond to the 'likelihood' parameter.",
                    paramBase.Push(P_LIKELIHOOD), def.Push(P_LIKELIHOOD));
        }

        #endregion // Setup
        #region Operations

        public override int Produce(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            var n = NumInds;
            if (n < min)
                n = min;
            if (n > max)
                n = max;

            int total;
            for (total = 0; total < n; )
            {
                var numToProduce = n - total;
                if (numToProduce > NumInds)
                    numToProduce = NumInds;

                total += Sources[0].Produce(numToProduce, numToProduce, start + total, subpop, inds, state, thread);
            }

            // clone if necessary
            if (Sources[0] is SelectionMethod)
                for (var q = start; q < total + start; q++)
                    inds[q] = (Individual)(inds[q].Clone());

            return total;
        }

        #endregion // Operations
    }
}