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

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.NEAT
{
    /**
     * NEATBreeder is a Breeder which overrides the breedPopulation method to first
     * mark the individuals in each subspecies that are allow to reproduce, and
     * replace the population with new individuals in each subspecies. All the heavy
     * lifting is done in NEATSpecies and NEATSubspecies, not here.
     * 
     * @author Ermo Wei and David Freelan
     */

    public class NEATBreeder : Breeder
    {

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // nothing to setup here
        }

        /**
         * This method simply call breedNewPopulation method in NEATSpecies，where
         * all the critical work in done.
         */
        public override Population BreedPopulation(IEvolutionState state)
        {

            Population pop = state.Population;
            for (int i = 0; i < pop.Subpops.Count; i++)
            {
                Subpopulation subpop = pop.Subpops[i];
                if (!(subpop.Species is NEATSpecies)) // uh oh
                state.Output.Fatal("To use NEATSpecies, subpopulation " + i
                                   + " must contain a NEATSpecies.  But it contains a " + subpop.Species);

                NEATSpecies species = (NEATSpecies) subpop.Species;




                species.BreedNewPopulation(state, i, 0);


            }

            return pop;
        }

    }
}