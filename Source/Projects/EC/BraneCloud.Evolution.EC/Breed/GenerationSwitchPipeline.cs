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
    /// GenerationSwitchPipeline is a simple BreedingPipeline which switches its source depending
    /// on the generation.  If the generation number is &lt; n, then GenerationSwitchPipeline uses
    /// source.0.  If the generation number if >= n, then GenerationSwitchPipeline uses source.1.
    /// 
    /// <p/><b>Important Note:</b> Because GenerationSwitchPipeline gets the generation number
    /// from the EvolutionState, and this number is not protected by a mutex, if you create
    /// an EvolutionState or Breeder which uses multiple threads that can update the generation
    /// number as they like, you could cause a race condition.  This doesn't occur with the
    /// present EvolutionState objects provided with ECJ, but you should be aware of the
    /// possibility.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// Defined as the max of its children's responses. 
    /// <p/><b>Number of Sources</b><br/>
    /// 2
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>generate-max</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> (default) or <tt>false</tt></font></td>
    /// <td valign="top">(Each time Produce(...) is called, should the GenerationSwitchPipeline
    /// force all its Sources to produce exactly the same number of individuals as the largest
    /// typical number of individuals produced by any source in the group?)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>switch-at</tt><br/>
    /// <font size="-1"><tt>int &gt;= 0</tt></font></td>
    /// <td valign="top">(The generation we will switch at)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// breed.Generation-switch
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.breed.GenerationSwitchPipeline")]
    public class GenerationSwitchPipeline : BreedingPipeline
    {
        #region Constants

        public const string P_SWITCHAT = "switch-at";
        public const string P_MULTIBREED = "generation-switch";
        public const string P_GEN_MAX = "generate-max";
        public const int NUM_SOURCES = 2;

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return BreedDefaults.ParamBase.Push(P_MULTIBREED); }
        }

        public override int NumSources
        {
            get { return NUM_SOURCES; }
        }

        public int MaxGeneratable { get; set; }
        public bool GenerateMax { get; set; }
        public int GenerationSwitch { get; set; }

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

            state.Output.ExitIfErrors();

            GenerationSwitch = state.Parameters.GetInt(paramBase.Push(P_SWITCHAT), def.Push(P_SWITCHAT), 0);
            if (GenerationSwitch < 0)
                state.Output.Fatal("GenerationSwitchPipeline must have a switch-at >= 0", paramBase.Push(P_SWITCHAT), def.Push(P_SWITCHAT));

            GenerateMax = state.Parameters.GetBoolean(paramBase.Push(P_GEN_MAX), def.Push(P_GEN_MAX), true);
            MaxGeneratable = 0; // indicates that I don't know what it is yet.  

            // declare that likelihood isn't used
            if (Likelihood < 1.0)
                state.Output.Warning("GenerationSwitchPipeline does not respond to the 'likelihood' parameter.",
                    paramBase.Push(P_LIKELIHOOD), def.Push(P_LIKELIHOOD));
        }

        #endregion // Setup
        #region Operations

        public override int Produce(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            var s = (state.Generation < GenerationSwitch ? Sources[0] : Sources[1]);
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

                total = s.Produce(n, n, start, subpop, inds, state, thread);
            }
            else
            {
                total = s.Produce(min, max, start, subpop, inds, state, thread);
            }

            // clone if necessary
            if (s is SelectionMethod)
                for (var q = start; q < total + start; q++)
                    inds[q] = (Individual)(inds[q].Clone());

            return total;
        }

        #endregion // Operations
    }
}