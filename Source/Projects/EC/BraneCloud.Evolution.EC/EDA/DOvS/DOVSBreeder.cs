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

using System.Collections.Generic;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.EDA.DOvS
{
    /**
     * DOVSBreeder is a Breeder which overrides the breedPopulation method to first
     * construct hyperbox around current best individual and replace the population
     * with new individuals sampled from this hyperbox. All the heavy lifting is
     * done in DOVSSpecies and its descendant, not here.
     * 
     * @author Ermo Wei and David Freelan
     */

    [ECConfiguration("ec.eda.dovs.DOVSBreeder")]
    public class DOVSBreeder : Breeder
    {
        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // nothing to setup
        }

        /**
         * This method have three major part, first identify the best indiviudal,
         * and then call updateMostPromisingArea(...) to construct a hyperbox around
         * this individual. At last, sampled a new population from the hyperbox and
         * take the none redundant samples and return it.
         */

        public override Population BreedPopulation(IEvolutionState state)
        {
            Population pop = state.Population;
            for (int i = 0; i < pop.Subpops.Count; i++)
            {
                Subpopulation subpop = pop.Subpops[i];
                if (!(subpop.Species is DOVSSpecies)) // uh oh
                    state.Output.Fatal("To use DOVSBreeder, subpopulation " + i
                                       + " must contain a DOVSSpecies.  But it contains a " + subpop.Species);

                DOVSSpecies species = (DOVSSpecies) (subpop.Species);

                // we assume backTrackingTest is always false.
                // Thus we combine activeSolution and Sk (individuals) to
                // identify the optimal
                species.FindBestSample(state, subpop);

                // Right now activeSolutions only has A_{k-1}, need to combine S_k
                for (int j = 0; j < subpop.Individuals.Count; j++)
                    species.activeSolutions.add(subpop.Individuals[i]);
                // Ak and bk will have all the constraints, including original
                // problem formulation and MPR
                // A b are original problem formulation constraints
                // activeSolutions will then have the indices for those solutions
                // already visited and define MPR
                // excluding current best solution

                // update MPA
                species.UpdateMostPromisingArea(state);

                // sample from MPA
                IList<Individual> candidates = species.MostPromisingAreaSamples(state, subpop.InitialSize);
                // get Sk for evaluation
                IList<Individual> Sk = species.UniqueSamples(state, candidates);

                // update the individuals
                subpop.Individuals = Sk;
            }
            return pop;
        }
    }
}