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
using System.Linq;
using System.Text;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.MultiObjective.NSGA2
{
    /// <summary>
    /// This SimpleBreeder subclass breeds a set of children from the Population, then
    /// joins the original Population with the children in a (mu+mu) fashion.   An NSGA2Breeder
    /// may have multiple threads for breeding.
    /// 
    /// <p/>NSGA-II has fixed archive size (the population size), and so ignores the 'elites'
    /// declaration.  However it will adhere to the 'reevaluate-elites' parameter in SimpleBreeder
    /// to determine whether to force fitness reevaluation.
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.multiobjective.nsga2.NSGA2Breeder")]
    public class NSGA2Breeder : SimpleBreeder
    {
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            // make sure SimpleBreeder's elites facility isn't being used
            //foreach (var t in Elite.Where(t => t != 0))
            //{
            //    state.Output.Warning("Elites may not be used with NSGA2Breeder, and will be ignored.");
            //}
            for (int i = 0; i < Elite.Length; i++)  // we use elite.length here instead of pop.subpops.length because the population hasn't been made yet.
                if (UsingElitism(i))
                    state.Output.Warning("You're using elitism with NSGA2Breeder, which is not permitted and will be ignored.  However the reevaluate-elites parameter *will* bre recognized by NSGAEvaluator.",
                        paramBase.Push(P_ELITE).Push("" + i));

            for (int i = 0; i < state.Population.Subpops.Length; i++)
                if (ReduceBy[i] != 0)
                    state.Output.Fatal("NSGA2Breeder does not support population reduction.", paramBase.Push(P_REDUCE_BY).Push("" + i), null);


            if (SequentialBreeding) // uh oh, haven't tested with this
                state.Output.Fatal("NSGA2Breeder does not support sequential evaluation.",
                    paramBase.Push(P_SEQUENTIAL_BREEDING));

            if (!ClonePipelineAndPopulation)
                state.Output.Fatal("ClonePipelineAndPopulation must be true for NSGA2Breeder.");
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Override breedPopulation(). We take the result from the super method in
        /// SimpleBreeder and append it to the old population. Hence, after
        /// generation 0, every subsequent call to
        /// <code>NSGA2Evaluator.evaluatePopulation()</code> will be passed a
        /// population of 2x<code>originalPopSize</code> individuals.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public override Population BreedPopulation(IEvolutionState state)
        {
            var oldPop = state.Population;
            var newPop = base.BreedPopulation(state);
            var subpops = oldPop.Subpops;
            Subpopulation oldSubpop;
            Subpopulation newSubpop;
            var subpopsLength = subpops.Length;

            for (var i = 0; i < subpopsLength; i++)
            {
                oldSubpop = oldPop.Subpops[i];
                newSubpop = newPop.Subpops[i];
                var combinedInds = new Individual[oldSubpop.Individuals.Length + newSubpop.Individuals.Length];
                Array.Copy(newSubpop.Individuals, 0, combinedInds, 0, newSubpop.Individuals.Length);
                Array.Copy(oldSubpop.Individuals, 0, combinedInds, newSubpop.Individuals.Length, oldSubpop.Individuals.Length);
                newSubpop.Individuals = combinedInds;
            }
            return newPop;
        }

        #endregion // Operations
    }
}