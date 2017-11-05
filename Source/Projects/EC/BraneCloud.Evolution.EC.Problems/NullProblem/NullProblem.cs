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
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.Problems.NullProblem
{
    /**
     * @author dfreelan Temporary class intended to be used to measure ECJ overhead
     *         doing non-evaluation tasks (statistics and such)
     */
    [ECConfiguration("ec.problems.nullproblem.NullProblem")]
    public class NullProblem : Problem, ISimpleProblem
    {
        public void Evaluate(IEvolutionState state, Individual ind, int subpopulation, int threadnum)
        {
            double fit = 10.0 - ((DoubleVectorIndividual) ind).genome[0] * ((DoubleVectorIndividual) ind).genome[0];

            ((SimpleFitness) ind.Fitness).SetFitness(state, fit, false);

        }

    }
}
