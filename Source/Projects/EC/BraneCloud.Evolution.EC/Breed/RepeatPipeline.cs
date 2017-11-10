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
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.Breed
{
    /**
     * RepeatPipeline is a BreedingPipeline which, after prepareToProduce() is called,
     * produces a single individual from its single source, then repeatedly clones that
     * child to fulfill requests to produce().

     <p><b>Number of Sources</b><br>
     1

     <p><b>Default Base</b><br>
     breed.repeat

     * @author Sean Luke
     * @version 1.0 
     */

    public class RepeatPipeline : BreedingPipeline
    {
        public const string P_REPEAT = "repeat";
        public const int NUM_SOURCES = 1;

        public Individual Individual { get; set; }
        public IntBag Parents { get; set; }

        public override IParameter DefaultBase => BreedDefaults.ParamBase.Push(P_REPEAT);

        public override int NumSources => NUM_SOURCES;

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            IParameter def = DefaultBase;

            if (!Likelihood.Equals(1.0))
                state.Output.Warning(
                    "RepeatPipeline given a likelihood other than 1.0.  This is nonsensical and will be ignored.",
                    paramBase.Push(P_LIKELIHOOD),
                    def.Push(P_LIKELIHOOD));
        }

        public override void PrepareToProduce(IEvolutionState state, int subpop, int thread)
        {
            base.PrepareToProduce(state, subpop, thread);
            Individual = null;
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

            // First things first: build our individual and his parents array
            if (Individual == null)
            {
                IDictionary<string, object> misc1 = null;
                if (misc != null && misc[SelectionMethod.KEY_PARENTS] != null)
                {
                    // the user is providing a parents array.  We'll need to make our own.
                    var parentsArray = new IntBag[1];
                    misc1 = new Dictionary<string, object>
                    {
                        [SelectionMethod.KEY_PARENTS] = parentsArray
                    };
                }
                IList<Individual> temp = new List<Individual>();
                Sources[0].Produce(1, 1, subpop, temp, state, thread, misc1);
                Individual = temp[0];

                // Now we extract from misc1 if we have to
                if (misc1?[SelectionMethod.KEY_PARENTS] != null) // we already know this second fact unless it was somehow removed
                {
                    Parents = ((IntBag[]) misc[SelectionMethod.KEY_PARENTS])[0];
                }
                else Parents = null;
            }

            int start = inds.Count;

            // Now we can copy the individual in
            for (int i = 0; i < min; i++)
            {
                inds.Add((Individual) Individual.Clone());
            }

            // add in the parents if we need to
            if (Parents != null && misc != null && misc[SelectionMethod.KEY_PARENTS] != null)
            {
                var parentsArray = (IntBag[]) misc[SelectionMethod.KEY_PARENTS];
                for (int i = 0; i < min; i++)
                {
                    parentsArray[start + i] = new IntBag(Parents);
                }
            }

            return min;
        }
    }
}