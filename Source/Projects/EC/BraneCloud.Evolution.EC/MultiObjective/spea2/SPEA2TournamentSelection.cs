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
using BraneCloud.Evolution.EC.Select;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.MultiObjective.SPEA2
{
    /// <summary>
    /// Replaces earlier class by: Robert Hubley, with revisions by Gabriel Balan and Keith Sullivan
    /// 
    /// This is a special version of TournamentSelection which restricts the selection to only
    /// the archive region (the top 'archiveSize' elements in the subpopulation).
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.multiobjective.spea2.SPEA2TournamentSelection")]
    public class SPEA2TournamentSelection : TournamentSelection
    {
        #region Operations

        public int GetRandomIndividual(int number, int subpopulation, EvolutionState state, int thread)
        {
            var oldinds = state.Population.Subpops[subpopulation].Individuals;
            var archiveSize = ((SimpleBreeder)state.Breeder).NumElites(state, subpopulation);
            var archiveStart = state.Population.Subpops[subpopulation].Individuals.Length - archiveSize;

            return archiveStart + state.Random[thread].NextInt(archiveSize);
        }

        #endregion // Operations
    }
}