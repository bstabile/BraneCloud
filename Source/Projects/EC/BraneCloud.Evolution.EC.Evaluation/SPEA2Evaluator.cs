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
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.MultiObjective.SPEA2
{
    /// <summary> 
    /// The SPEA2Evaluator is a simple, non-coevolved generational evaluator which
    /// evaluates every single member of every subpop individually in its
    /// own problem space.  One Problem instance is cloned from p_problem for
    /// each evaluating thread.
    /// 
    /// The evaluator is also responsible for calculating the SPEA2Fitness
    /// function.  This function depends on the entire population and so
    /// cannot be calculated in the Problem class.
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.multiobjective.spea2.SPEA2Evaluator")]
    public class SPEA2Evaluator : SimpleEvaluator, ISPEA2Evaluator
    {
        #region Static

        /// <summary>
        /// Returns the kth smallest element in the array.
        /// Note that here k=1 means the smallest element in the array (not k=0).
        /// Uses a randomized sorting technique, hence the need for the random number generator.
        /// </summary>
        static double OrderStatistics(double[] array, int kth, IMersenneTwister rng)
        {
            return RandomizedSelect(array, 0, array.Length - 1, kth, rng);
        }

        /// <summary>
        /// OrderStatistics [Cormen, p187]:
        /// find the ith smallest element of the array between indices p and r
        /// </summary>
        static double RandomizedSelect(double[] array, int p, int r, int i, IMersenneTwister rng)
        {
            if (p == r) return array[p];
            var q = RandomizedPartition(array, p, r, rng);
            var k = q - p + 1;
            if (i <= k)
                return RandomizedSelect(array, p, q, i, rng);

            return RandomizedSelect(array, q + 1, r, i - k, rng);
        }


        /// <summary>
        /// [Cormen, p162]
        /// </summary>
        static int RandomizedPartition(double[] array, int p, int r, IMersenneTwister rng)
        {
            var i = rng.NextInt(r - p + 1) + p;

            //exchange array[p]<->array[i]
            var tmp = array[i];
            array[i] = array[p];
            array[p] = tmp;
            return Partition(array, p, r);
        }

        /// <summary>
        /// [Cormen p 154]
        /// </summary>
        static int Partition(double[] array, int p, int r)
        {
            var x = array[p];
            var i = p - 1;
            var j = r + 1;
            while (true)
            {
                do j--; while (array[j] > x);
                do i++; while (array[i] < x);
                if (i < j)
                {
                    //exchange array[i]<->array[j]
                    var tmp = array[i];
                    array[i] = array[j];
                    array[j] = tmp;
                }
                else
                    return j;
            }
        }

        #endregion // Static
        #region Operations

        /// <summary>
        /// A simple evaluator that doesn't do any coevolutionary
        /// evaluation.  Basically it applies evaluation pipelines,
        /// one per thread, to various subchunks of a new population. 
        /// </summary>
        public override void EvaluatePopulation(IEvolutionState state)
        {
            base.EvaluatePopulation(state);

            // build SPEA2 fitness values
            foreach (var t in state.Population.Subpops)
            {
                var inds = t.Individuals;
                ComputeAuxiliaryData(state, inds);
            }
        }

        /// <summary>
        /// Computes the strength of individuals, then the raw fitness (wimpiness) and kth-closest sparsity
        /// measure.  Finally, computes the final fitness of the individuals.
        /// </summary>
        public void ComputeAuxiliaryData(IEvolutionState state, IList<Individual> inds)
        {
            var distances = CalculateDistances(state, inds);

            // For each individual calculate the strength
            foreach (var t in inds)
            {
                // Calculate the node strengths
                var myStrength = 0;
                for (var z = 0; z < inds.Count; z++)
                    if (((SPEA2MultiObjectiveFitness)t.Fitness).ParetoDominates((MultiObjectiveFitness)inds[z].Fitness))
                        myStrength++;
                ((SPEA2MultiObjectiveFitness)t.Fitness).Strength = myStrength;
            }

            // calculate k value
            var kTH = (int)Math.Sqrt(inds.Count);  // note that the first element is k=1, not k=0 

            // For each individual calculate the Raw fitness and kth-distance
            for (var y = 0; y < inds.Count; y++)
            {
                double fitness = 0;
                for (var z = 0; z < inds.Count; z++)
                {
                    // Raw fitness 
                    if (((SPEA2MultiObjectiveFitness)inds[z].Fitness).ParetoDominates((MultiObjectiveFitness)inds[y].Fitness))
                    {
                        fitness += ((SPEA2MultiObjectiveFitness)inds[z].Fitness).Strength;
                    }
                } // For each individual z calculate RAW fitness distances
                // Set SPEA2 raw fitness value for each individual

                var indYFitness = ((SPEA2MultiObjectiveFitness)inds[y].Fitness);

                // Density component

                // calc k-th nearest neighbor distance.
                // distances are squared, so we need to take the square root.
                var kthDistance = Math.Sqrt(OrderStatistics(distances[y], kTH, state.Random[0]));

                // Set SPEA2 k-th NN distance value for each individual
                indYFitness.KthNNDistance = 1.0 / (2 + kthDistance);

                // Set SPEA2 fitness value for each individual
                indYFitness.Fitness = fitness + indYFitness.KthNNDistance;
            }
        }

        /// <summary>
        /// Returns a matrix of sum squared distances from each individual to each other individual.
        /// </summary>
        public double[][] CalculateDistances(IEvolutionState state, IList<Individual> inds)
        {
            var distances = new double[inds.Count][];
            for (var i = 0; i < inds.Count; i++)
                distances[i] = new double[inds.Count];

            for (var y = 0; y < inds.Count; y++)
            {
                distances[y][y] = 0;
                for (var z = y + 1; z < inds.Count; z++)
                {
                    distances[z][y] = distances[y][z] =
                                      ((SPEA2MultiObjectiveFitness)inds[y].Fitness).
                                          SumSquaredObjectiveDistance((SPEA2MultiObjectiveFitness)inds[z].Fitness);
                }
            }
            return distances;
        }

        #endregion // Operations
    }
}