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

namespace BraneCloud.Evolution.EC.GP.GE.Breed
{
    [ECConfiguration("ec.gp.ge.breed.GETruncationPipeline")]
    public class GETruncationPipeline : BreedingPipeline
    {
        #region Constants

        public const string P_TRUNCATION = "truncate";
        public const int NUM_SOURCES = 1;

        #endregion // Constants
        #region Properties

        public override int NumSources => NUM_SOURCES;

        public override IParameter DefaultBase => GEDefaults.ParamBase.Push(P_TRUNCATION);

        #endregion // Properties
        #region Operations

        public override int Produce(
            int min,
            int max,
            int start,
            int subpopulation,
            Individual[] inds,
            IEvolutionState state,
            int thread)
        {
            // grab individuals from our source and stick 'em right into inds.
            // we'll modify them from there
            var n = Sources[0].Produce(min, max, start, subpopulation, inds, state, thread);


            // should we bother?
            if (!state.Random[thread].NextBoolean(Likelihood))
                return Reproduce(n, start, subpopulation, inds, state, thread, false);  // DON'T produce children from source -- we already did



            // now let's mutate 'em
            for (var q = start; q < n + start; q++)
            {
                if (Sources[0] is SelectionMethod)
                    inds[q] = (Individual)(inds[q].Clone());

                var ind = (GEIndividual)(inds[q]);
                var species = (GESpecies)(ind.Species);

                var consumed = species.Consumed(state, ind, thread);
                if (consumed <= 1) continue;
                var pieces = new Object[2];
                ind.Split(new [] { consumed }, pieces);
                ind.Join(new [] { pieces[0] });
            }
            return n;
        }

        #endregion // Operations
    }
}