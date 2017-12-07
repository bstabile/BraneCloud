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
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.EDA.DOvS
{
    /**
     * HyperboxSpecies is a DOVSSpecies which contains method for updating promising
     * sample area and also sample from that area.
     *
     * @author Ermo Wei and David Freelan
     */
    [ECConfiguration("ec.eda.dovs.HyperboxSpecies")]
    public class HyperboxSpecies : DOVSSpecies
    {
        /** boxA and boxB contain the current constraint hyperbox. */
        public IList<double[]> boxA;

        /** boxA and boxB contain the current constraint hyperbox. */
        public IList<Double> boxB;

        public static double UPPER_BOUND = 1e31;
        public static double EPSILON_STABILITY = 1e-20;
        public static double LARGE_NUMBER = 1e32;

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            boxA = new List<double[]>();
            boxB = new List<Double>();
        }

        /** Constructing a hyperbox, which defines the next search area. */
        public override void UpdateMostPromisingArea(IEvolutionState state)
        {
            int dimension = this.GenomeSize;
            // Each time we construct a hyperbox, the previous one,
            // defined by boxA, boxB are no longer useful.
            boxA = new List<double[]>();
            boxB = new List<double>();

            ActiveSolutions.Clear();
            // First the original problem formulation constraints
            // copy the contents of A into boxA and b to boxB
            foreach (double[] arr in A)
            {
                boxA.Add(arr);
            }
            foreach (double arr in B)
            {
                boxB.Add(arr);
            }

            // for each coordinate d, find xup_d and xlow_d that are closest to
            // xstar_d.
            // If one or both of xup_d and xlow_d do not exist. It is still ok
            // because
            // we have original problem constraints to bound the search region.
            for (int i = 0; i < dimension; ++i)
            {
                int key = ((IntegerVectorIndividual) Visited[OptimalIndex]).genome[i];

                // lowerBound() returns the iterator to the smallest element whose
                // key is
                // equal to or BIGGER than the argument "key". Decreasing it will
                // give the largest element
                // with a key smaller than the argument "key", if such an element
                // exists.
                CornerMap.Pair pair = Corners[i].LowerBound(key);
                if (pair == null)
                    state.Output.Fatal("Error. Cannot find coordnation in coordinate position map.");
                if (pair.Key == key)
                {
                    if (Corners[i].HasSmaller(pair))
                    {
                        // So we fetch the previous item and use its key to do a
                        // search for all
                        // solutions with this key, if there is such a key smaller
                        // than
                        // the key of xstar
                        pair = Corners[i].Smaller(pair);
                        ActiveSolutions.Add(pair.Value);
                        double[] atemp = new double[dimension];
                        Array.Clear(atemp, 0, atemp.Length);
                        atemp[i] = 1;
                        // The key is the coordinate position.
                        // So it is the rhs of the constraint
                        double btemp = pair.Key;
                        boxA.Add(atemp);
                        boxB.Add(btemp);
                    }
                }
                else
                {
                    // This should never happen.
                    state.Output.Fatal("Problem in constructing hyperbox");
                }

                // upper_bound returns the smallest element whose key is bigger than
                // (excluding equal to) "key",
                // if such an element exists
                pair = Corners[i].UpperBound(key);
                if (pair != null)
                {
                    ActiveSolutions.Add(pair.Value);

                    double[] atemp = new double[dimension];
                    Array.Clear(atemp, 0, atemp.Length);
                    atemp[i] = -1;

                    // The key is the coordinate position.
                    // So it is the rhs of the constraint
                    double btemp = pair.Key;
                    boxA.Add(atemp);
                    boxB.Add(btemp);
                }
            }
        }

        /** Sample from the hyperbox to get new samples for evaluation. */
        public override IList<Individual> MostPromisingAreaSamples(IEvolutionState state, int popSize)
        {
            var bestIndividual = (IntegerVectorIndividual) Visited[OptimalIndex];
            int dimension = bestIndividual.GenomeLength;
            int numOfConstraints = boxA.Count;

            IList<Individual> newSolutions = new List<Individual>();
            IList<Individual> candidates = new List<Individual>();
            // TODO : do we need implement clone function here?
            var newInd = (IntegerVectorIndividual) bestIndividual.Clone();
            ((DOVSFitness) newInd.Fitness).Reset();
            newSolutions.Add(newInd);

            for (int i = 0; i < popSize; ++i)
            {
                // Whenever a new solution is pushed into the vector candidate, a
                // new solution is created and
                // initially it has the same content as the solution just pushed
                // into the vector.
                if (i > 0)
                {
                    newInd = (IntegerVectorIndividual) ((IntegerVectorIndividual) newSolutions[i - 1]).Clone();
                    ((DOVSFitness) newInd.Fitness).Reset();
                    newSolutions.Add(newInd);
                }
                for (int j = 0; j < WarmUp; ++j)
                {
                    newInd = (IntegerVectorIndividual) newSolutions[i];
                    // To warm up: Randomly pick up a dimension to move along
                    int directionToMove = state.Random[0].NextInt(dimension);
                    double[] b1 = new double[numOfConstraints];
                    for (int k = 0; k < numOfConstraints; k++)
                    {
                        // For each constraint
                        double sum = 0;
                        for (int l = 0; l < dimension; l++)
                        {
                            // Do a matrix multiplication
                            if (l != directionToMove)
                            {
                                sum += boxA[k][l] * newInd.genome[l];
                            }
                        }
                        b1[k] = boxB[k] - sum;
                    }
                    // Now check which constraint is tight
                    double upper = UPPER_BOUND, lower = UPPER_BOUND;
                    for (int k = 0; k < numOfConstraints; ++k)
                    {
                        double temp = 0;
                        // temp is the temporary value of x_i to make the jth
                        // constraint tight
                        if (Math.Abs(boxA[k][directionToMove]) > EPSILON_STABILITY)
                            temp = b1[k] / boxA[k][directionToMove];
                        else
                            temp = LARGE_NUMBER;

                        if (temp > newInd.genome[directionToMove] + EPSILON_STABILITY)
                        {
                            // If the value to make the constraint tight is greater
                            // than the value of the current point,
                            // it means that there is space "above" the current
                            // point, and the upper bound could be shrinked, until
                            // the upper bound becomes the current point itself or
                            // cannot be smaller than 1.
                            if (temp - newInd.genome[directionToMove] < upper)
                                upper = temp - newInd.genome[directionToMove];
                        }
                        else if (temp < newInd.genome[directionToMove] - EPSILON_STABILITY)
                        {
                            if (newInd.genome[directionToMove] - temp < lower)
                                lower = newInd.genome[directionToMove] - temp;
                        }
                        else
                        {
                            // The constraint is already tight at current value,
                            // i.e., the point is now on the boundary. !!!!!!!!!!!
                            // If the coefficient is positive, then increasing,
                            // i.e., moving "up" will reenter feasible region
                            // because the
                            // inequalitys are Ax>=b
                            if (boxA[k][directionToMove] > 0)
                            {
                                lower = 0;
                            }
                            else
                            {
                                // Don't need to worry about
                                // boxA[k][directionToMove] = 0, because in that
                                // case temp will be a large number
                                upper = 0;
                            }
                        }
                    }
                    int maxXDirectionToMove = (int) Math.Floor(upper) + newInd.genome[directionToMove];
                    int minXDirectionToMove = newInd.genome[directionToMove] - (int) Math.Floor(lower);
                    int length = maxXDirectionToMove - minXDirectionToMove;
                    int step = state.Random[0].NextInt(length + 1);
                    newInd.genome[directionToMove] = minXDirectionToMove + step;
                }
                candidates.Add(newSolutions[i]);
            }

            return candidates;
        }
    }
}