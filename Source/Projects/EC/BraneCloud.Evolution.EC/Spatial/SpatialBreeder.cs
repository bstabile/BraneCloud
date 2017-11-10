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
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.Spatial
{
    /// <summary> 
    /// A slight modification of the simple breeder for spatially-embedded EAs.
    /// 
    /// Breeds each subpop separately, with no inter-population exchange,
    /// and using a generational approach.  A SpatialBreeder may have multiple
    /// threads; it divvys up a subpop into chunks and hands one chunk
    /// to each thread to populate.  One array of BreedingPipelines is obtained
    /// from a population's Species for each operating breeding thread.
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.spatial.SpatialBreeder")]
    public class SpatialBreeder : SimpleBreeder
    {
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            // check for elitism and warn about it
            for (var i = 0; i < Elite.Length; i++)
                if (UsingElitism(i))
                {
                    state.Output.Warning("You're using elitism with SpatialBreeder.  This is unwise as elitism is done by moving individuals around in the population, thus messing up the spatial nature of breeding.",
                        paramBase.Push(P_ELITE).Push("" + i));
                    break;
                }

            if (SequentialBreeding) // uh oh, untested
                state.Output.Warning("SpatialBreeder hasn't been well tested with sequential evaluation, though it should probably work fine.  You're on your own.",
                    paramBase.Push(P_SEQUENTIAL_BREEDING));

            if (!ClonePipelineAndPopulation)
                state.Output.Fatal("clonePipelineAndPopulation must be true for SpatialBreeder.");
        }

        #endregion // Setup
        #region Operations

        public override void BreedPopChunk(Population newpop, IEvolutionState state, int[] numinds, int[] from, int threadnum)
        {
            for (var subpop = 0; subpop < newpop.Subpops.Count; subpop++)
            {
                IList<Individual> putHere = NewIndividuals[subpop][threadnum];

                // if it's subpop's turn and we're doing sequential breeding...
                if (!ShouldBreedSubpop(state, subpop, threadnum))
                {
                    // instead of breeding, we should just copy forward this subpopulation.  We'll copy the part we're assigned
                    for (var ind = from[subpop]; ind < numinds[subpop] - from[subpop]; ind++)
                        newpop.Subpops[subpop].Individuals[ind] =
                            (Individual)state.Population.Subpops[subpop].Individuals[ind].Clone();
                }
                else
                {
                    var bp = (BreedingSource)newpop.Subpops[subpop].Species.Pipe_Prototype.Clone();

                    if (!(state.Population.Subpops[subpop] is ISpace))
                        state.Output.Fatal("Subpopulation " + subpop + " does not implement the Space interface.");
                    var space = (ISpace)state.Population.Subpops[subpop];

                    // check to make sure that the breeding source produces
                    // the right kind of individuals.  Don't want a mistake there! :-)
                    if (!bp.Produces(state, newpop, subpop, threadnum))
                        state.Output.Fatal("The Breeding Source of subpopulation " + subpop +
                                           " does not produce individuals of the expected species " +
                                           newpop.Subpops[subpop].Species.GetType().Name + " or fitness " +
                                           newpop.Subpops[subpop].Species.F_Prototype);
                    bp.PrepareToProduce(state, subpop, threadnum);

                    // start breedin'!
                    for (var x = from[subpop]; x < from[subpop] + numinds[subpop]; x++)
                    {
                        space.SetIndex(threadnum, x);
                        var newMisc = newpop.Subpops[subpop].Species.BuildMisc(state, subpop, threadnum);
                        if (bp.Produce(1, 1, subpop, putHere, state, threadnum, newMisc) != 1)
                            state.Output.Fatal("The sources should produce one individual at a time!");
                    }

                    bp.FinishProducing(state, subpop, threadnum);
                }
            }
        }

        #endregion // Operations
    }
}