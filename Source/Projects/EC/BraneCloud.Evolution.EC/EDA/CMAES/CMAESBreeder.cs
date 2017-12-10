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

namespace BraneCloud.Evolution.EC.EDA.CMAES
{
    /**
     * CMAESBreeder is a Breeder which overrides the breedPopulation method
     * to first update CMA-ES's internal distribution, then replace all the
     * individuals in the population with new samples generated from the
     * distribution.  All the heavy lifting is done in CMAESSpecies, not here.
     *
     * @author Sam McKay and Sean Luke
     * @version 1.0 
     */

    [ECConfiguration("ec.eda.cmaes.CMAESBreeder")]
    public class CMAESBreeder : Breeder
    {
        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // nothing to setup
        }

        /** Updates the CMA-ES distribution given the current population, then 
            replaces the population with new samples generated from the distribution.
            Returns the revised population. */

        public override Population BreedPopulation(IEvolutionState state)
        {
            Population pop = state.Population;
            for (int i = 0; i < pop.Subpops.Count; i++)
            {
                Subpopulation subpop = pop.Subpops[i];
                if (!(subpop.Species is CMAESSpecies)) // uh oh
                    state.Output.Fatal("To use CMAESBreeder, subpopulation " + i +
                                       " must contain a CMAESSpecies.  But it contains a " + subpop.Species);

                CMAESSpecies species = (CMAESSpecies) subpop.Species;

                // update distribution[i] for subpop
                species.UpdateDistribution(state, subpop);

                // overwrite individuals
                IList<Individual> inds = subpop.Individuals;
                for (int j = 0; j < inds.Count; j++)
                    inds[j] = species.NewIndividual(state, 0);
            }

            return pop;
        }
    }
}