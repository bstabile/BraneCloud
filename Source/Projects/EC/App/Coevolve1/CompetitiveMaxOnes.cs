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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BraneCloud.Evolution.EC.CoEvolve;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.App.Coevolve1
{
    [ECConfiguration("ec.app.coevolve1.CompetitiveMaxOnes")]
    public class CompetitiveMaxOnes : Problem, IGroupedProblem
    {
        public void PreprocessPopulation(IEvolutionState state, Population pop, bool[] updateFitness, bool countVictoriesOnly)
        {
            for (var i = 0; i < pop.Subpops.Length; i++)
                if (updateFitness[i])
                    foreach (var t in pop.Subpops[i].Individuals)
                        ((SimpleFitness)(t.Fitness)).Trials = new List<double>();
        }

        public void PostprocessPopulation(IEvolutionState state, Population pop, bool[] updateFitness, bool countVictoriesOnly)
        {
            for (var i = 0; i < pop.Subpops.Length; i++)
                if (updateFitness[i])
                    foreach (var t in pop.Subpops[i].Individuals)
                    {
                        var fit = ((SimpleFitness)(t.Fitness));

                        // average of the trials we got
                        var len = fit.Trials.Count;
                        double sum = 0;
                        for (var l = 0; l < len; l++)
                            sum += (Double)fit.Trials[l];
                        sum /= len;

                        // we'll not bother declaring the ideal
                        fit.SetFitness(state, (float)(sum), false);
                        t.Evaluated = true;
                    }
        }

        public void Evaluate(IEvolutionState state,
            Individual[] ind,  // the individuals to evaluate together
            bool[] updateFitness,  // should this individuals' fitness be updated?
            bool countVictoriesOnly,
            int[] subpops,
            int threadnum)
        {
            if (ind.Length != 2 || updateFitness.Length != 2)
                state.Output.Fatal("The InternalSumProblem evaluates only two individuals at a time.");

            if (!(ind[0] is BitVectorIndividual))
                state.Output.Fatal("The individuals in the InternalSumProblem should be FloatVectorIndividuals.");

            if (!(ind[1] is BitVectorIndividual))
                state.Output.Fatal("The individuals in the InternalSumProblem should be FloatVectorIndividuals.");

            // calculate the function value for the first individual
            var temp = (BitVectorIndividual)ind[0];
            var value1 = temp.genome.Count(t => t);

            // calculate the function value for the second individual
            temp = (BitVectorIndividual)ind[1];
            var value2 = temp.genome.Count(t => t);

            double score = value1 - value2;

            if (updateFitness[0])
            {
                var fit = ((SimpleFitness)(ind[0].Fitness));
                fit.Trials.Add(score);

                // set the fitness because if we're doing Single Elimination Tournament, the tournament
                // needs to know who won this time around.  Don't bother declaring the ideal here.
                fit.SetFitness(state, (float)score, false);
            }

            if (updateFitness[1])
            {
                var fit = ((SimpleFitness)(ind[1].Fitness));
                fit.Trials.Add(-score);

                // set the fitness because if we're doing Single Elimination Tournament, the tournament
                // needs to know who won this time around.
                fit.SetFitness(state, (float)-score, false);
            }
        }
    }
}