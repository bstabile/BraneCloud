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

using BraneCloud.Evolution.EC.CoEvolve;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Spatial
{
    /// <summary> 
    /// SpatialMultiPopCoevolutionaryEvaluator implements a coevolutionary evaluator involving multiple
    /// spatially-embedded subpopulations.  You ought to use it in conjuction with SpatialTournamentSelection
    /// (for selecting current-generation individuals, set the tournament selection size to 1, which will
    /// pick randomly from the space).
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.spatial.SpatialMultiPopCoevolutionaryEvaluator")]
    public class SpatialMultiPopCoevolutionaryEvaluator : MultiPopCoevolutionaryEvaluator
    {
        protected Individual Produce(SelectionMethod method, int subpop, int individual, IEvolutionState state, int thread)
        {
            if (!(state.Population.Subpops[subpop] is ISpace))
                state.Output.Fatal("Subpopulation " + subpop + " is not a Space.");

            var space = (ISpace)(state.Population.Subpops[subpop]);
            space.SetIndex(thread, individual);

            return state.Population.Subpops[subpop].Individuals[method.Produce(subpop, state, thread)];
        }
    }
}