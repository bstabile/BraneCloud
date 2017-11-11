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
    /**
     * FirstCopyPipeline is a BreedingPipeline similar to ReproductionPipeline, except
     * that after a call to prepareToProduce(...), the immediate next child produced
     * is produced from source 0, and all the remaining children in that produce()
     * call and in subsequent produce() calls are produced from source 1.  This allows
     * a simple approach to doing a one-child elitism by loading the elitist child from
     * source 0 and the rest from source 1.  See ec/app/ecsuite/sa.params for an example.
     
     
     <p><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br>
     ...as many as the child produces

     <p><b>Number of Sources</b><br>
     2

     <p><b>Default Base</b><br>
     breed.reproduce

     * @author Sean Luke
     * @version 1.0 
     */

    [ECConfiguration("ec.breed.FirstCopyPipeline")]
    public class FirstCopyPipeline : BreedingPipeline
    {
        public const string P_FIRST_COPY = "first-copy";
        public const int NUM_SOURCES = 2;

        public override IParameter DefaultBase => BreedDefaults.ParamBase.Push(P_FIRST_COPY);

        public override int NumSources => NUM_SOURCES;

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            IParameter def = DefaultBase;

            if (!Likelihood.Equals(1.0))
                state.Output.Warning(
                    "FirstCopyPipeline given a likelihood other than 1.0.  This is nonsensical and will be ignored.",
                    paramBase.Push(P_LIKELIHOOD),
                    def.Push(P_LIKELIHOOD));
        }

        public bool FirstTime { get; set; } = true;

        public override void PrepareToProduce(IEvolutionState state, int subpop, int thread)
        {
            base.PrepareToProduce(state, subpop, thread);

            // reset
            FirstTime = true;
        }

        public override int Produce(
            int min,
            int max,
            int subpop,
            IList<Individual> inds,
            IEvolutionState state,
            int thread, 
            IDictionary<string, object> misc)
        {
            int start = inds.Count;

            if (FirstTime)
            {
                // Load our very first child from source 0
                int n = Sources[0].Produce(1, 1, subpop, inds, state, thread, misc);

                // Were we asked to make more kids than this?  If so, make the rest from source 1
                if (min > 1)
                {
                    n += Sources[1].Produce(min - 1, max - 1, subpop, inds, state, thread, misc);
                }

                FirstTime = false;
                return n;
            }
            else
            {
                // take all kids from source 1
                int n = Sources[1].Produce(min, max, subpop, inds, state, thread, misc);
                return n;
            }
        }
    }
}