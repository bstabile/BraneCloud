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

namespace BraneCloud.Evolution.EC.App.Tutorial3.Test
{
    [ECConfiguration("ec.app.tutorial3.OddRosenbrock")]
    public class OddRosenbrock : Problem, ISimpleProblem
    {
        public override void Setup(IEvolutionState state, IParameter paramBase) { }

        public void Evaluate(IEvolutionState state,
            Individual ind,
            int subpop,
            int threadnum)
        {
            if (!(ind is DoubleVectorIndividual))
                state.Output.Fatal("The individuals for this problem should be DoubleVectorIndividuals.");

            var genome = ((DoubleVectorIndividual)ind).genome;
            var len = genome.Length;
            double value = 0;

            // Compute the Rosenbrock function for our genome
            for (var i = 1; i < len; i++)
                value += 100 * (genome[i - 1] * genome[i - 1] - genome[i]) *
                    (genome[i - 1] * genome[i - 1] - genome[i]) +
                    (1 - genome[i - 1]) * (1 - genome[i - 1]);

            // Rosenbrock is a minimizing function which does not drop below 0. 
            // But SimpleFitness requires a maximizing function -- where 0 is worst
            // and 1 is best.  To use SimpleFitness, we must convert the function.
            // This is the Koza style of doing it:

            value = 1.0 / (1.0 + value);
            ((SimpleFitness)(ind.Fitness)).SetFitness(state, (float)value, value == 1.0);

            ind.Evaluated = true;
        }
    }
}