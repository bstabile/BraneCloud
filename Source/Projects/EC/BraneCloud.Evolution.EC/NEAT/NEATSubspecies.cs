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
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.NEAT
{
    /**
     * NEATSubspecies is the actual Species in original code. However, since we
     * already have Species in ECJ, we name Species in original code as Subspecies
     * in our implementation. The creation of the Subspecies is done in the speciate
     * method.
     * 
     * @author Ermo Wei and David Freelan
     *
     */
    [ECConfiguration("ec.neat.NEATSubspecies")]
    public class NEATSubspecies : IPrototype
    {
        public const string P_SUBSPECIES = "subspecies";



        /** Age of the current subspecies. */
        public int Age { get; set; }

        /**
         * Record the last time the best fitness improved within the individuals of
         * this subspecies If this is too long ago, the subspecies will goes extinct
         */
        public int AgeOfLastImprovement { get; set; }

        /** The max fitness the an individual in this subspecies ever achieved. */
        public double MaxFitnessEver { get; set; }

        /** The individuals within this species */
        public IList<Individual> Individuals { get; set; }

        /** The next generation individuals within this species */
        public IList<Individual> NewGenIndividuals { get; set; }

        /** Expected Offspring for next generation for this subspecies */
        public int ExpectedOffspring { get; set; }


        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            Age = 1;
            AgeOfLastImprovement = 0;
            MaxFitnessEver = 0;
            Individuals = new List<Individual>();
            NewGenIndividuals = new List<Individual>();
        }

        /**
         * Return a clone of this subspecies, but with a empty individuals and
         * newGenIndividuals list.
         */
        public virtual object EmptyClone()
        {
            NEATSubspecies myobj = (NEATSubspecies) Clone();
            Individuals = new List<Individual>();
            NewGenIndividuals = new List<Individual>();
            return myobj;
        }

        public virtual object Clone()
        {
            NEATSubspecies myobj = null;
            try
            {
                myobj = (NEATSubspecies) MemberwiseClone();
                myobj.Age = Age;
                myobj.AgeOfLastImprovement = AgeOfLastImprovement;
                myobj.MaxFitnessEver = MaxFitnessEver;
                myobj.ExpectedOffspring = ExpectedOffspring;


            }
            catch (CloneNotSupportedException e) // never happens
            {
                throw new InvalidOperationException();
            }
            return myobj;
        }

        /** Reset the status of the current subspecies. */
        public void Reset()
        {
            Age = 1;
            ExpectedOffspring = 0;
            AgeOfLastImprovement = 0;
            MaxFitnessEver = 0;
        }

        /** Return the first individual in this subspecies */
        public Individual First()
        {
            if (Individuals.Count > 0)
                return Individuals[0];
            return null;
        }

        /** Return the first individual in newGenIndividuals list. */
        public Individual NewGenerationFirst()
        {
            if (NewGenIndividuals.Count > 0)
                return NewGenIndividuals[0];
            return null;
        }

        public virtual IParameter DefaultBase => NEATDefaults.ParamBase.Push(P_SUBSPECIES);

        /**
         * Adjust the fitness of the individuals within this subspecies. We will use
         * the adjusted fitness to determine the expected offsprings within each
         * subspecies.
         */
        public void AdjustFitness(IEvolutionState state, int dropoffAge, double ageSignificance)
        {
            int ageDebt = (Age - AgeOfLastImprovement + 1) - dropoffAge;
            if (ageDebt == 0)
                ageDebt = 1;

            foreach (NEATIndividual ind in Individuals.Cast<NEATIndividual>())
            {
                // start to adjust the fitness with age information
                ind.AdjustedFitness = ind.Fitness.Value;

                // Make fitness decrease after a stagnation point dropoffAge
                // Added an if to keep species pristine until the dropoff point
                if (ageDebt >= 1)
                {
                    ind.AdjustedFitness = ind.AdjustedFitness * 0.01;
                }

                // Give a fitness boost up to some young age (niching)
                // The age-significance parameter is a system parameter
                // If it is 1, then young species get no fitness boost
                if (Age <= 10)
                    ind.AdjustedFitness = ind.AdjustedFitness * ageSignificance;

                // Do not allow negative fitness
                if (ind.AdjustedFitness < 0.0)
                    ind.AdjustedFitness = 0.0001;

                // Share fitness with the species
                // This is the explicit fitness sharing, where the the original
                // fitness
                // are dividing by the number of individuals in the species.
                // By using this, a species cannot afford to become too big even if
                // many of its
                // individual perform well
                ind.AdjustedFitness = ind.AdjustedFitness / Individuals.Count;
            }
        }

        /**
         * Sort the individuals in this subspecies, the one with highest fitness
         * comes first.
         */
        public void SortIndividuals()
        {
            Individuals = Individuals
                .OrderByDescending(x => ((NEATIndividual)x).AdjustedFitness)
                .ToList();
        }

        /** Update the maxFitnessEver variable. */
        public void UpdateSubspeciesMaxFitness()
        {
            // Update ageOfLastImprovement here, assume the individuals are
            // already sorted
            // (the first Individual has the best fitness)
            if (Individuals[0].Fitness.Value > MaxFitnessEver)
            {
                AgeOfLastImprovement = Age;
                MaxFitnessEver = Individuals[0].Fitness.Value;
            }

        }

        /** Mark the individual who can reproduce for this generation. */
        public void MarkReproducableIndividuals(double survivalThreshold)
        {
            // Decide how many get to reproduce based on survivalThreshold *
            // individuals.Count
            // mark for death those after survivalThreshold * individuals.Count
            // Adding 1.0 ensures that at least one will survive
            // floor is the largest (closest to positive infinity) double value that
            // is not greater than the argument and is equal to a mathematical
            // integer

            int numParents = (int) Math.Floor(survivalThreshold * Individuals.Count + 1.0);

            // Mark the champion as such
            ((NEATIndividual) First()).Champion = true;

            // Mark for death those who are ranked too low to be parents
            for (int i = 0; i < Individuals.Count; ++i)
            {
                NEATIndividual ind = (NEATIndividual) Individuals[i];
                if (i >= numParents)
                {
                    ind.Eliminate = true;
                }
            }
        }

        /** Test if newGenIndividuals list is empty. */
        public bool HasNewGeneration()
        {
            return NewGenIndividuals.Count != 0;
        }

        /**
         * Compute the collective offspring the entire species (the sum of all
         * individual's offspring) is assigned skim is fractional offspring left
         * over from a previous subspecies that was counted. These fractional parts
         * are kept until they add up to 1
         */


        public double CountOffspring(double skim)
        {
            ExpectedOffspring = 0;
            const double y1 = 1.0;
            double r2 = skim;
            int n2 = 0;

            foreach (var t in Individuals.Cast<NEATIndividual>())
            {
                double x1 = t.ExpectedOffspring;
                int n1 = (int) (x1 / y1);
                double r1 = x1 - ((int) (x1 / y1) * y1);
                n2 = n2 + n1;
                r2 = r2 + r1;

                if (r2 >= 1.0)
                {
                    n2 = n2 + 1;
                    r2 = r2 - 1.0;
                }
            }

            ExpectedOffspring = n2;
            return r2;
        }

        /**
         * Where the actual reproduce is happening, it will grab the candidate
         * parents, and calls the crossover or mutation method on these parents
         * individuals.
         */
        public bool Reproduce(IEvolutionState state, int thread, int subpop, IList<NEATSubspecies> sortedSubspecies)
        {
            if (ExpectedOffspring > 0 && Individuals.Count == 0)
            {
                state.Output.Fatal("Attempt to reproduce out of empty subspecies");
                return false;
            }

            if (ExpectedOffspring > state.Population.Subpops[subpop].InitialSize)
            {
                state.Output.Fatal("Attempt to reproduce too many individuals");
                return false;
            }

            NEATSpecies species = (NEATSpecies) state.Population.Subpops[subpop].Species;

            // bestIndividual of the 'this' specie is the first element of the
            // species
            // note, we already sort the individuals based on the fitness (not sure
            // if this is still correct to say)
            NEATIndividual bestIndividual = (NEATIndividual) First();




            // create the designated number of offspring for the Species one at a
            // time
            bool bestIndividualDone = false;

            for (int i = 0; i < ExpectedOffspring; ++i)
            {

                NEATIndividual newInd;

                if (bestIndividual.SuperChampionOffspring > 0)
                {

                    newInd = (NEATIndividual) bestIndividual.Clone();

                    // Most super champion offspring will have their connection
                    // weights mutated only
                    // The last offspring will be an exact duplicate of this super
                    // champion
                    // Note: Super champion offspring only occur with stolen babies!
                    // Settings used for published experiments did not use this

                    if (bestIndividual.SuperChampionOffspring > 1)
                    {
                        if (state.Random[thread].NextBoolean(0.8) || species.MutateAddLinkProb.Equals(0.0))
                        {
                            newInd.MutateLinkWeights(state, thread, species, species.WeightMutationPower, 1.0,
                                NEATSpecies.MutationType.GAUSSIAN);
                        }
                        else
                        {
                            // Sometime we add a link to a superchamp
                            newInd.CreateNetwork(); // make sure we have the network
                            newInd.MutateAddLink(state, thread);
                        }
                    }
                    if (bestIndividual.SuperChampionOffspring == 1)
                    {
                        if (bestIndividual.PopChampion)
                        {
                            newInd.PopChampionChild = true;
                            newInd.HighFit = bestIndividual.Fitness.Value;
                        }
                    }

                    bestIndividual.SuperChampionOffspring--;
                }
                else if (!bestIndividualDone && ExpectedOffspring > 5)
                {

                    newInd = (NEATIndividual) bestIndividual.Clone();
                    bestIndividualDone = true;
                }
                // Decide whether to mate or mutate
                // If there is only one individual, then always mutate
                else if (state.Random[thread].NextBoolean(species.MutateOnlyProb) || Individuals.Count == 1)
                {
                    // Choose the random parent
                    int parentIndex = state.Random[thread].NextInt(Individuals.Count);
                    Individual parent = Individuals[parentIndex];
                    newInd = (NEATIndividual) parent.Clone();


                    newInd.DefaultMutate((EvolutionState) state, thread);


                }
                else // Otherwise we should mate
                {

                    // random choose the first parent
                    int parentIndex = state.Random[thread].NextInt(Individuals.Count);
                    NEATIndividual firstParent = (NEATIndividual) Individuals[parentIndex];
                    NEATIndividual secondParent;
                    // Mate within subspecies, choose random second parent
                    if (state.Random[thread].NextBoolean(1.0 - species.InterspeciesMateRate))
                    {
                        parentIndex = state.Random[thread].NextInt(Individuals.Count);
                        secondParent = (NEATIndividual) Individuals[parentIndex];


                    }
                    else // Mate outside subspecies
                    {

                        // Select a random species
                        NEATSubspecies randomSubspecies = this;
                        // Give up if you cant find a different Species
                        int giveUp = 0;
                        while (randomSubspecies == this && giveUp < 5)
                        {
                            // Choose a random species tending towards better
                            // species
                            double value = state.Random[thread].NextGaussian() / 4;
                            if (value > 1.0)
                                value = 1.0;
                            // This tends to select better species

                            int upperBound = (int) Math.Floor(value * (sortedSubspecies.Count - 1.0) + 0.5);
                            int index = 0;
                            while (index < upperBound)
                                index++;
                            randomSubspecies = sortedSubspecies[index];
                            giveUp++;
                        }

                        secondParent = (NEATIndividual) randomSubspecies.First();

                    }

                    newInd = firstParent.Crossover(state, thread, secondParent);


                    // Determine whether to mutate the baby's Genome
                    // This is done randomly or if the parents are the same
                    // individual
                    if (state.Random[thread].NextBoolean(1.0 - species.MateOnlyProb) || firstParent == secondParent
                        || species.Compatibility(firstParent, secondParent).Equals(0.0))
                    {
                        newInd.DefaultMutate((EvolutionState) state, thread);
                    }
                }



                newInd.SetGeneration(state);
                newInd.CreateNetwork();

                // Add the new individual to its proper subspecies
                // this could create new subspecies
                species.Speciate(state, newInd);
            }



            return true;

        }

        /**
         * Compute generations gap since last improvement
         */
        public int TimeSinceLastImproved()
        {
            return Age - AgeOfLastImprovement;
        }

        /** Add the individual to the next generation of this subspecies */
        public void AddNewGenIndividual(NEATIndividual neatInd)
        {
            NewGenIndividuals.Add(neatInd);
            neatInd.Subspecies = this;
        }

        /**
         * Remove the individuals from current subspecies who have been mark as
         * eliminate the remain individuals will be allow to reproduce
         */
        public void RemovePoorFitnessIndividuals()
        {
            // create a new list, contain the non eliminate individuals
            IList<Individual> remainIndividuals = new List<Individual>();
            foreach (var ind in Individuals.Cast<NEATIndividual>())
            {
                if (!ind.Eliminate)
                {
                    remainIndividuals.Add(ind);
                }
            }
            Individuals = remainIndividuals;
        }

        /**
         * After we finish the reproduce, the newGenIndividual list has the all the
         * individuals that is ready for evalution in next generation. Let's switch
         * to it.
         */
        public void ToNewGeneration()
        {
            Individuals = NewGenIndividuals;
            // create a new ArrayList
            NewGenIndividuals = new List<Individual>();
        }

    }
}