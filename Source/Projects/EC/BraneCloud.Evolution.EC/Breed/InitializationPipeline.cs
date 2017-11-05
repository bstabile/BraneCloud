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
    /**
     * InitializationPipeline is a BreedingPipeline which simply generates a new
     * random inidividual.  It has no sources at all.
     *
     <p><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br>
     ...the maximum requested by the parent.

     <p><b>Number of Sources</b><br>
     0

     </table>
     <p><b>Default Base</b><br>
     breed.init
     */

    public class InitializationPipeline : BreedingPipeline
    {
        public const string P_INIT = "init";
        public const int NUM_SOURCES = 0;

        public override IParameter DefaultBase => BreedDefaults.ParamBase.Push(P_INIT);

        public override int NumSources => NUM_SOURCES;

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            IParameter def = DefaultBase;

            if (!Likelihood.Equals(1.0))
                state.Output.Warning(
                    "InitializationPipeline given a likelihood other than 1.0.  This is nonsensical and will be ignored.",
                    paramBase.Push(P_LIKELIHOOD),
                    def.Push(P_LIKELIHOOD));
        }

        public override int Produce(int min,
            int max,
            int start,
            int subpopulation,
            Individual[] inds,
            IEvolutionState state,
            int thread)
        {
            Species s = state.Population.Subpops[subpopulation].Species;
            for (int q = start; q < start + max; q++)
            {
                inds[q] = s.NewIndividual(state, thread);
            }
            return max;
        }
    }
}