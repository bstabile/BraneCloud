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
using BraneCloud.Evolution.EC.CoEvolve;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.Problems.Coevolve2
{
    [ECConfiguration("ec.problems.coevolve2.CoevolutionaryECSuite")]
    public class CoevolutionaryECSuite : ECSuite.ECSuite, IGroupedProblem
    {
        #region Constants

        public const string P_SHOULD_SET_CONTEXT = "set-context";

        #endregion // Constants
        #region Fields

        bool _shouldSetContext;

        #endregion // Fields
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            // load whether we should set context or not
            _shouldSetContext = state.Parameters.GetBoolean(paramBase.Push(P_SHOULD_SET_CONTEXT), null, true);
        }

        #endregion // Setup
        #region Operations

        public void PreprocessPopulation(IEvolutionState state, Population pop, bool[] prepareForAssessment, bool countVictoriesOnly)
        {
            for (var i = 0; i < pop.Subpops.Count; i++)
            {
                if (prepareForAssessment[i])
                {
                    foreach (var t in pop.Subpops[i].Individuals)
                        ((SimpleFitness)t.Fitness).Trials = new List<double>();
                }
            }
        }

        public int PostprocessPopulation(IEvolutionState state, Population pop, bool[] assessFitness, bool countVictoriesOnly)
        {
            int total = 0;
            for (var i = 0; i < pop.Subpops.Count; i++)
            {
                if (!assessFitness[i]) continue;

                foreach (var ind in pop.Subpops[i].Individuals)
                {
                    var fit = (SimpleFitness)ind.Fitness;

                    // we take the max over the trials
                    var max = Double.NegativeInfinity;
                    var len = fit.Trials.Count;
                    for (var l = 0; l < len; l++)
                        max = Math.Max(fit.Trials[l], max); // it'll be the first one, but whatever

                    fit.SetFitness(state, (float)max, IsOptimal(ProblemType, (float)max));
                    ind.Evaluated = true;
                    total++;
                }
            }
            return total;
        }

        public void Evaluate(IEvolutionState state,
            Individual[] ind,  // the individuals to evaluate together
            bool[] updateFitness,  // should this individuals' fitness be updated?
            bool countVictoriesOnly, // can be neglected in cooperative coevolution
            int[] subpops,
            int threadnum)
        {
            if (ind.Length == 0)
                state.Output.Fatal("Number of individuals provided to CoevolutionaryECSuite is 0!");
            if (ind.Length == 1)
                state.Output.WarnOnce("Coevolution used, but number of individuals provided to CoevolutionaryECSuite is 1.");

            var size = 0;
            for (var i = 0; i < ind.Length; i++)
            {
                if (!(ind[i] is DoubleVectorIndividual))
                    state.Output.Error("Individual " + i + "in coevolution is not a DoubleVectorIndividual.");
                else
                {
                    var coind = (DoubleVectorIndividual)ind[i];
                    size += coind.genome.Length;
                }
            }
            state.Output.ExitIfErrors();

            // concatenate all the arrays
            var vals = new double[size];
            var pos = 0;
            foreach (var t in ind)
            {
                var coind = (DoubleVectorIndividual)t;
                Array.Copy(coind.genome, 0, vals, pos, coind.genome.Length);
                pos += coind.genome.Length;
            }

            var trial = Function(state, ProblemType, vals, threadnum);

            // update individuals to reflect the trial
            for (var i = 0; i < ind.Length; i++)
            {
                var coind = (DoubleVectorIndividual)ind[i];
                if (updateFitness[i])
                {
                    // Update the context if this is the best trial.  We're going to assume that the best
                    // trial is trial #0 so we don't have to search through them.
                    var len = coind.Fitness.Trials.Count;

                    if (len == 0)  // easy
                    {
                        if (_shouldSetContext) coind.Fitness.SetContext(ind, i);
                        coind.Fitness.Trials.Add(trial);
                    }
                    else if (coind.Fitness.Trials[0] < trial)  // best trial is presently #0
                    {
                        if (_shouldSetContext) coind.Fitness.SetContext(ind, i);
                        coind.Fitness.Trials.Add(trial);
                    }
                    else if (coind.Fitness.Trials[0] < trial)  // best trial is presently #0
                    {
                        if (_shouldSetContext) coind.Fitness.SetContext(ind, i);
                        // put me at position 0
                        Double t = coind.Fitness.Trials[0];
                        coind.Fitness.Trials[0] = trial;  // put me at 0
                        coind.Fitness.Trials.Add(t);  // move him to the end
                    }

                    // finally set the fitness for good measure
                    ((SimpleFitness)coind.Fitness).SetFitness(state, (float)trial, false);
                }
            }
        }

        #endregion // Operations
    }
}