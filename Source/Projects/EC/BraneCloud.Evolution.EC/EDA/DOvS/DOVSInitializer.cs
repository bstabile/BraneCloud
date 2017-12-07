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
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.EDA.DOvS
{
    /**
     * DOVSInitializer is a SimpleInitializer which ensures that the subpopulations
     * are create from an existing individual read from file. This individual will
     * be serve as the start search point for our algorithm.
     *
     * @author Ermo Wei and David Freelan
     */
    [ECConfiguration("ec.eda.dovs.DOVSInitializer")]
    public class DOVSInitializer : SimpleInitializer
    {
        private const long serialVersionUID = 1;

        /**
         * In DOVS, we provide the algorithm with a start individual from file, this
         * start individual is the start search point of the DOVS algorithm. We use
         * this start point to construct a hyperbox contains promising solutions,
         * and sample from this region, the number of sample is equal to parameter
         * "pop.subpop.X.size" in parameter files.
         * 
         * However, due to redundant samples, we the individuals size may be
         * smaller than what have been specified in pop.subpop.X.size.
         */
        public override Population InitialPopulation(IEvolutionState state, int thread)
        {
            Population p = base.InitialPopulation(state, thread);
            // make sure the each subpop only have one individual
            for (int i = 0; i < p.Subpops.Count; i++)
            {
                if (p.Subpops[i].Species is DOVSSpecies)
                {
                    DOVSSpecies species = (DOVSSpecies) p.Subpops[i].Species;

                    if (p.Subpops[i].Individuals.Count != 1)
                        state.Output.Fatal("contain more than one start point");

                    // add the start point to the visited ArrayList
                    species.Visited.Clear();
                    species.Visited.Add(p.Subpops[i].Individuals[0]);
                    species.VisitedIndexMap[p.Subpops[i].Individuals[0]] =  0;
                    species.OptimalIndex = 0;

                    IntegerVectorIndividual ind = (IntegerVectorIndividual) species.Visited[species.OptimalIndex];
                    // For the visited solution, record its coordinate
                    // positions in the multimap
                    for (int j = 0; j < species.GenomeSize; ++j)
                    {
                        // The individual is the content. The key is its
                        // coordinate position
                        species.Corners[j].Insert(ind.genome[j], ind);
                    }

                    // update MPA
                    species.UpdateMostPromisingArea(state);

                    // sample from MPA
                    int initialSize = p.Subpops[i].InitialSize;
                    IList<Individual> candidates = species.MostPromisingAreaSamples(state, initialSize);

                    // get unique candidates for evaluation, this is Sk in paper
                    IList<Individual> uniqueCandidates = species.UniqueSamples(state, candidates);

                    // update the individuals
                    p.Subpops[i].Individuals = uniqueCandidates;

                }

            }
            return p;
        }

    }
}