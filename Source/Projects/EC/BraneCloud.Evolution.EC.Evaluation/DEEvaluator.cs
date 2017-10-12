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
using BraneCloud.Evolution.EC.DE;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Evaluation
{
    /// <summary>
    /// DEEvaluator is a simple subclass of SimpleEvaluator which first evaluates the population, then
    /// compares each population member against the parent which had created it in Differential Evolution.
    /// The parents are stored in DEBreeder.previousPopulation.  If the parent is superior to the child,
    /// then the parent replaces the child in the population and the child is discarded.  This does not
    /// happen in the first generation, as there are of course no parents yet.
    /// 
    /// <p/>This code could have been moved into the Breeder of course.  But then the better of the parents
    /// and children would not appear in standard Statistics objects.  So we've broken it out here.
    /// 
    /// <p/>The full description of Differential Evolution may be found in the book
    /// "Differential Evolution: A Practical Approach to Global Optimization"
    /// by Kenneth Price, Rainer Storn, and Jouni Lampinen.
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.de.DEEvaluator")]
    public class DEEvaluator : SimpleEvaluator, IDEEvaluator
    {
        #region Operations

        public override void EvaluatePopulation(IEvolutionState state)
        {
            base.EvaluatePopulation(state);

            if (state.Breeder is DEBreeder)
            {
                var previousPopulation = ((DEBreeder)(state.Breeder)).PreviousPopulation; // for faster access
                if (previousPopulation != null)
                {
                    if (previousPopulation.Subpops.Length != state.Population.Subpops.Length)
                        state.Output.Fatal("DEEvaluator requires that the population have the same number of subpopulations every generation.");
                    for (var i = 0; i < previousPopulation.Subpops.Length; i++)
                    {
                        if (state.Population.Subpops[i].Individuals.Length != previousPopulation.Subpops[i].Individuals.Length)
                            state.Output.Fatal("DEEvaluator requires that subpopulation " + i + " should have the same number of individuals in all generations.");
                        for (var j = 0; j < state.Population.Subpops[i].Individuals.Length; j++)
                            if (previousPopulation.Subpops[i].Individuals[j].Fitness.BetterThan(state.Population.Subpops[i].Individuals[j].Fitness))
                                state.Population.Subpops[i].Individuals[j] = previousPopulation.Subpops[i].Individuals[j];
                    }
                }
            }
            else state.Output.Fatal("DEEvaluator requires DEBreeder to be the breeder.");
        }

        #endregion // Operations
    }
}