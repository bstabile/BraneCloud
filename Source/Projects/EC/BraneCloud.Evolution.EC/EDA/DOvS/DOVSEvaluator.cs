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
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.EDA.DOvS
{
    /**
     * The DOVSEvaluator is a SimpleEvaluator to evaluate the Individual. Due to
     * the stochastic property of the problem. An individual may not to be evaluate
     * several times so that we can have a good assessment of it. This evaluator
     * will make use of the statistics of fitness of each individual and determine
     * how many evaluation are needed for a individual where we can have high
     * confidence about its fitness value.
     *
     * @author Ermo Wei and David Freelan
     */

    public class DOVSEvaluator : SimpleEvaluator
    {
        /**
         * For each of the iteration, we are not just evaluate the individuals in
         * current population but also current best individual and individuals in
         * activeSolutions. Their number of evaluation is determined by there
         * fitness statistics.
         */
        protected void EvalPopChunk(IEvolutionState state, int[] numinds, int[] from, int threadnum, ISimpleProblem p)
        {
            // so far the evaluator only support when evalthread is 1
            ((Problem) p).PrepareToEvaluate(state, threadnum);

            IList<Subpopulation> subpops = state.Population.Subpops;
            int len = subpops.Count;

            for (int pop = 0; pop < len; pop++)
            {
                // start evaluatin'!
                int fp = from[pop];
                int upperbound = fp + numinds[pop];
                IList<Individual> inds = subpops[pop].Individuals;
                if (subpops[pop].Species is DOVSSpecies)
                {
                    DOVSSpecies species = (DOVSSpecies) subpops[pop].Species;

                    // Evaluator need to evaluate individual from two set: Sk
                    // (individuals) and activeSolution
                    // Original comment: To avoid unnecessary complication with
                    // stopping test
                    // procedure, require that Sk has at least 2 reps.
                    // Although we do not have stopping test here, we still do 2
                    // reps
                    for (int i = 0; i < inds.Count; ++i)
                    {
                        DOVSFitness fit = (DOVSFitness) (inds[i].Fitness);
                        int addrep = 2 - fit.NumOfObservations;
                        for (int rep = 0; rep < addrep; ++rep)
                        {
                            p.Evaluate(state, inds[i], pop, threadnum);
                            species.NumOfTotalSamples++;
                        }
                    }

                    // This is a special treat for activeSolutions when
                    // certain criteria have met
                    if ( //species.ocba && 
                        species.Stochastic)
                    {
                        // ocba only makes sense when it is a stoc simulation
                        // allocate some reps to active solutions and sample
                        // best according to an ocba like heuristic
                        // if ocba option is turned on.
                        // There are deltan more reps to allocate, where deltan
                        // = sizeof(activesolutions).
                        int deltan = species.ActiveSolutions.size();
                        // Always add two more reps to current sample best
                        for (int i = 0; i < 2; i++)
                            p.Evaluate(state, species.Visited.Get(species.OptimalIndex), pop, threadnum);
                        species.NumOfTotalSamples += 2;
                        deltan -= 2;
                        if (deltan > 0)
                        {
                            // get R
                            double R = 0;
                            for (int i = 0; i < species.ActiveSolutions.Size(); ++i)
                            {
                                Individual ind = species.activeSolutions.get(i);
                                DOVSFitness fit = (DOVSFitness) (ind.Fitness);
                                Individual bestInd = species.Visited.get(species.OptimalIndex);
                                DOVSFitness bestFit = (DOVSFitness) (bestInd.Fitness);
                                R += (fit.Variance
                                      / Math.Max(1e-10, Math.Abs(fit.Mean - bestFit.Mean)));
                            }
                            for (int i = 0; i < species.ActiveSolutions.size(); ++i)
                            {
                                Individual ind = (Individual) species.ActiveSolutions.get(i);
                                DOVSFitness fit = (DOVSFitness) (ind.Fitness);
                                Individual bestInd = (Individual) species.Visited.Get(species.OptimalIndex);
                                DOVSFitness bestFit = (DOVSFitness) (bestInd.Fitness);

                                double fraction = fit.Variance
                                                  / Math.Max(1e-10, Math.Abs(fit.Mean - bestFit.Mean)) / R;
                                double tempDeltan = fraction * deltan;
                                if (tempDeltan > 1)
                                {
                                    long roundedDeltan = (long) tempDeltan;
                                    for (int j = 0; j < roundedDeltan; ++j)
                                        p.Evaluate(state, ind, pop, threadnum);
                                    species.NumOfTotalSamples += roundedDeltan;
                                }
                            }

                        }
                    }

                    // If it is a deterministic simulation, only one rep

                    // origial code start generation at 1, we start at 0
                    // thus, we add 1 to computation of base of log
                    int baseGen = state.Generation + 1;

                    int newReps = (int) Math
                        .Ceiling(species.InitialReps * Math.Max(1, Math.Pow(Math.Log((double) baseGen / 2), 1.01)));
                    if (species.Stochastic)
                        species.Repetition = (species.Repetition >= newReps) ? species.Repetition : newReps;
                    else
                        species.Repetition = 1;

                    // Now do the simulations for activeSolutions
                    for (int count = 0; count < species.ActiveSolutions.Size(); ++count)
                    {
                        Individual individual = (Individual) species.activeSolutions.get(count);
                        DOVSFitness fit = (DOVSFitness) (individual.Fitness);
                        if (fit.NumOfObservations < species.Repetition)
                        {
                            int newrep = species.Repetition - fit.NumOfObservations;
                            for (int rep = 0; rep < newrep; ++rep)
                            {
                                p.Evaluate(state, individual, pop, threadnum);
                            }
                            species.NumOfTotalSamples += newrep;
                        }
                    }

                    // Simulate current sample best
                    {
                        Individual bestIndividual = (Individual) species.Visited.Get(species.OptimalIndex);
                        DOVSFitness fit = (DOVSFitness) (bestIndividual.Fitness);
                        if (fit.NumOfObservations < species.Repetition)
                        {
                            int newrep = species.Repetition - fit.NumOfObservations;
                            for (int rep = 0; rep < newrep; ++rep)
                            {
                                p.Evaluate(state, bestIndividual, pop, threadnum);
                            }
                            species.NumOfTotalSamples += newrep;
                        }
                    }

                    // Simulate current individuals
                    // Since backtracking flag is always false, we always do this
                    for (int i = 0; i < inds.Count; ++i)
                    {
                        DOVSFitness fit = (DOVSFitness) (inds[i].Fitness);
                        if (fit.NumOfObservations < species.Repetition)
                        {
                            int newRep = species.Repetition - fit.NumOfObservations;
                            for (int rep = 0; rep < newRep; ++rep)
                            {
                                p.Evaluate(state, inds[i], pop, threadnum);
                            }
                            species.NumOfTotalSamples += newRep;
                        }
                    }
                }
                else
                {
                    for (int x = fp; x < upperbound; x++)
                        p.Evaluate(state, inds[x], pop, threadnum);
                }
            }

            ((Problem) p).FinishEvaluating(state, threadnum);
        }

    }
}
