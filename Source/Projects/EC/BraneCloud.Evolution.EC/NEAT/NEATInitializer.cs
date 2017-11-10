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
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.NEAT
{
    /**
     * NEATInitializer is a SimpleInitializer which ensures that the subpopulations
     * are all create from an existing template individual read from file.
     *
     * @author Ermo Wei and David Freelan
     */

    public class NEATInitializer : SimpleInitializer
    {
        private const long SerialVersionUID = 1;

        /**
         * In NEAT, we provide the algorithm with a start individual from file,
         * after read the start individual from file, we populate the subpopulation with
         * mutated version of that template individual. The number of individual we create is
         * determined by the "pop.subpop.X.size" parameter.
         */
        public Population InitialPopulation(EvolutionState state, int thread)
        {
            // read in the start genome as the template
            Population p = SetupPopulation(state, thread);
            p.Populate(state, thread);

            // go through all the population and populate the NEAT subpop
            foreach (Subpopulation subpop in p.Subpops)
            {
// NEAT uses a template to populate the population
                // we first read it in to form the population, then mutate the links
                if (subpop.Species is NEATSpecies)
                {
                    NEATSpecies species = (NEATSpecies) subpop.Species;

                    IList<Individual> inds = subpop.Individuals;
                    // get the template
                    NEATIndividual templateInd = (NEATIndividual) inds[0];
                    // clear the individuals
                    inds.Clear();

                    // spawn the individuals with template
                    int initialSize = subpop.InitialSize;
                    for (int j = 0; j < initialSize; ++j)
                    {
                        NEATIndividual newInd = species.SpawnWithTemplate(state, species, thread, templateInd);
                        inds.Add(newInd);
                    }

                    // state.output.warnOnce("Template genome found, populate the subpopulation with template individual");
                    // templateInd.printIndividual(state, 0);

                    // set the next available innovation number and node id
                    species.SetInnovationNumber(templateInd.GetGeneInnovationNumberSup());
                    species.CurrNodeId = templateInd.GetNodeIdSup();

                    // speciate
                    foreach (Individual ind in inds)
                    {
                        species.Speciate(state, ind);
                    }

                    // switch to the new generation
                    foreach (NEATSubspecies s in species.Subspecies)
                    {
                        s.ToNewGeneration();
                    }

                }
            }

            return p;
        }

    }
}