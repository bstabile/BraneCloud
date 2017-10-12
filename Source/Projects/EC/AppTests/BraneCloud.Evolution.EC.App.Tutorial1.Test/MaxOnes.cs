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

using System.Linq;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.App.Tutorial1.Test
{
    [ECConfiguration("ec.app.tutorial1.MaxOnes")]
    public class MaxOnes : Problem, ISimpleProblem
    {
        public void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            if (ind.Evaluated) return;

            if (!(ind is BitVectorIndividual))
                state.Output.Fatal("Whoa!  It's not a BitVectorIndividual!!!", null);

            var ind2 = (BitVectorIndividual)ind;

            var sum = ind2.genome.Sum(t => (t ? 1 : 0));

            if (!(ind2.Fitness is SimpleFitness))
                state.Output.Fatal("Whoa!  It's not a SimpleFitness!!!", null);
            ((SimpleFitness)ind2.Fitness).SetFitness(state,
                // ...the fitness...
                (float)(((double)sum) / ind2.genome.Length),
                //... is the individual ideal?  Indicate here...
                sum == ind2.genome.Length);
            ind2.Evaluated = true;
        }
    }
}