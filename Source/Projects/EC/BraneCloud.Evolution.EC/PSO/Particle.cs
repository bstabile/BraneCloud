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
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Randomization;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.PSO
{
    /**
     * Particle is a DoubleVectorIndividual with additional statistical information
     * necessary to perform Particle Swarm Optimization.  Specifically, it has a 
     * VELOCITY, a NEIGHBORHOOD of indexes of individuals, a NEIGHBORHOOD BEST genome
     * and fitness, and a PERSONAL BEST genome and fitness.  These elements, plus the
     * GLOBAL BEST genome and fitness found in PSOBreeder, are used to collectively
     * update the particle's location in space.
     *
     * <p> Particle updates its location in two steps.  First, it gathers current
     * neighborhood and personal best statistics via the update(...) method.  Then
     * it updates the particle's velocity and location (genome) according to these
     * statistics in the tweak(...) method.  Notice that neither of these methods is
     * the defaultMutate(...) method used in DoubleVectorIndividual: this means that
     * in *theory* you could rig up Particles to also be mutated if you thought that
     * was a good reason.
     * 
     * <p> Many of the parameters passed into the tweak(...) method are based on
     * weights determined by the PSOBreeder.
     *
     * @author Khaled Ahsan Talukder
     */


    [ECConfiguration("ec.pso.Particle")]
    public class Particle : DoubleVectorIndividual
    {
        // my velocity
        public double[] Velocity { get; set; }

        // the individuals in my neighborhood
        public int[] Neighborhood { get; set; } = null;

        // the best genome and fitness members of my neighborhood ever achieved
        public double[] NeighborhoodBestGenome { get; set; }

        public IFitness NeighborhoodBestFitness { get; set; }

        // the best genome and fitness *I* personally ever achieved
        public double[] PersonalBestGenome { get; set; }

        public IFitness PersonalBestFitness { get; set; }
        

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            Velocity = new double[genome.Length];
        }


        public override Object Clone()
        {
            Particle myobj = (Particle) base.Clone();
            // must clone the velocity and neighborhood pattern if they exist
            if (Velocity != null) Velocity = (double[]) Velocity.Clone();
            if (Neighborhood != null) Neighborhood = (int[]) Neighborhood.Clone();
            return myobj;
        }

        public void Update(IEvolutionState state, int subpop, int myindex, int thread)
        {
            // update personal best
            if (PersonalBestFitness == null || Fitness.BetterThan(PersonalBestFitness))
            {
                PersonalBestFitness = (Fitness) Fitness.Clone();
                PersonalBestGenome = (double[]) genome.Clone();
            }

            // initialize neighborhood if it's not been created yet
            PSOBreeder psob = (PSOBreeder) state.Breeder;
            if (Neighborhood == null || psob.Neighborhood == PSOBreeder.C_NEIGHBORHOOD_RANDOM_EACH_TIME)
            {
                if (psob.Neighborhood == PSOBreeder.C_NEIGHBORHOOD_RANDOM
                ) // "random" scheme is the only thing that is available for now
                    Neighborhood = CreateRandomPattern(myindex, psob.IncludeSelf,
                        state.Population.Subpops[subpop].Individuals.Length, psob.NeighborhoodSize, state, thread);
                else if (psob.Neighborhood == PSOBreeder.C_NEIGHBORHOOD_TOROIDAL ||
                         psob.Neighborhood == PSOBreeder.C_NEIGHBORHOOD_RANDOM_EACH_TIME)
                    Neighborhood = CreateToroidalPattern(myindex, psob.IncludeSelf,
                        state.Population.Subpops[subpop].Individuals.Length, psob.NeighborhoodSize);
                else // huh?
                    state.Output.Fatal("internal error: invalid PSO Neighborhood style: " + psob.Neighborhood);
            }

            // identify Neighborhood best
            NeighborhoodBestFitness = Fitness; // initially me
            NeighborhoodBestGenome = genome;
            for (int i = 0; i < Neighborhood.Length; i++)
            {
                int ind = Neighborhood[i];
                if (state.Population.Subpops[subpop].Individuals[ind].Fitness.BetterThan(Fitness))
                {
                    NeighborhoodBestFitness = state.Population.Subpops[subpop].Individuals[ind].Fitness;
                    NeighborhoodBestGenome =
                        ((DoubleVectorIndividual) (state.Population.Subpops[subpop].Individuals[ind]))
                        .genome;
                }
            }

            // clone Neighborhood best
            NeighborhoodBestFitness = (Fitness) (NeighborhoodBestFitness.Clone());
            NeighborhoodBestGenome = (double[]) (NeighborhoodBestGenome.Clone());
        }

        // velocityCoeff:       cognitive/confidence coefficient for the velocity
        // personalCoeff:       cognitive/confidence coefficient for self
        // informantCoeff:      cognitive/confidence coefficient for informants/neighbours
        // globalCoeff:         cognitive/confidence coefficient for global best, this is not done in the standard PSO
        public void Tweak(
            IEvolutionState state, double[] globalBest,
            double velocityCoeff, double personalCoeff,
            double informantCoeff, double globalCoeff,
            int thread)
        {
            for (int x = 0; x < GenomeLength; x++)
            {
                double xCurrent = genome[x];
                double xPersonal = PersonalBestGenome[x];
                double xNeighbour = NeighborhoodBestGenome[x];
                double xGlobal = globalBest[x];
                double beta = state.Random[thread].NextDouble() * personalCoeff;
                double gamma = state.Random[thread].NextDouble() * informantCoeff;
                double delta = state.Random[thread].NextDouble() * globalCoeff;

                double newVelocity = (velocityCoeff * Velocity[x]) + (beta * (xPersonal - xCurrent)) +
                                     (gamma * (xNeighbour - xCurrent)) + (delta * (xGlobal - xCurrent));
                Velocity[x] = newVelocity;
                genome[x] += newVelocity;
            }

            Evaluated = false;
        }

        // Creates a toroidal neighborhood pattern for the individual
        int[] CreateRandomPattern(int myIndex, bool includeSelf, int popsize, int neighborhoodSize,
            IEvolutionState state,
            int threadnum)
        {
            IMersenneTwister mtf = state.Random[threadnum];
            HashSet<int> already = new HashSet<int>();
            int[] neighbors = null;

            if (includeSelf)
            {
                neighbors = new int[neighborhoodSize + 1];
                neighbors[neighborhoodSize] = myIndex; // put me at the top
                already.Add(myIndex);
            }
            else
                neighbors = new int[neighborhoodSize];

            Int32 n;
            for (int i = 0; i < neighborhoodSize; i++)
            {
                do
                {
                    neighbors[i] = mtf.NextInt(popsize);
                    n = neighbors[i];
                } while (already.Contains(n));
                already.Add(n);
            }
            return neighbors;

        }

        // Creates a toroidal neighborhood pattern for the individual indexed by 'myindex'
        int[] CreateToroidalPattern(int myindex, bool includeSelf, int popsize, int neighborhoodSize)
        {
            int[] neighbors = null;

            if (includeSelf)
            {
                neighbors = new int[neighborhoodSize + 1];
                neighbors[neighborhoodSize] = myindex; // put me at the top
            }
            else
                neighbors = new int[neighborhoodSize];

            int pos = 0;
            for (int i = myindex - neighborhoodSize / 2; i < myindex; i++)
            {
                neighbors[pos++] = ((i % popsize) + popsize) % popsize;
            }

            for (int i = myindex + 1; i < neighborhoodSize - (neighborhoodSize / 2) + 1; i++)
            {
                neighbors[pos++] = ((i % popsize) + popsize) % popsize;
            }

            return neighbors;
        }
    }
}