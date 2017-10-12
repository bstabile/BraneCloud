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

namespace BraneCloud.Evolution.EC.App.Tutorial2.Test
{
    [ECConfiguration("ec.app.tutorial2.AddSubtract")]
    public class AddSubtract : Problem, ISimpleProblem
    {
        public void Evaluate(IEvolutionState state,
            Individual ind,
            int subpop,
            int threadnum)
        {
            if (ind.Evaluated) return;

            if (!(ind is IntegerVectorIndividual))
                state.Output.Fatal("Whoa!  It's not a IntegerVectorIndividual!!!", null);

            var ind2 = (IntegerVectorIndividual)ind;

            var rawfitness = 0;
            for (var x = 0; x < ind2.genome.Length; x++)
                if (x % 2 == 0) rawfitness += ind2.genome[x];
                else rawfitness -= ind2.genome[x];

            // We finish by taking the ABS of rawfitness.  By the way,
            // in SimpleFitness, fitness values must be set up so that 0 is <= the worst
            // fitness and +infinity is >= the ideal possible fitness.  Our raw fitness
            // value here satisfies this. 
            if (rawfitness < 0) rawfitness = -rawfitness;
            if (!(ind2.Fitness is SimpleFitness))
                state.Output.Fatal("Whoa!  It's not a SimpleFitness!!!", null);
            ((SimpleFitness)ind2.Fitness).SetFitness(state,
                // what the heck, lets normalize the fitness for genome length
                // so it's within float range
                (float)(((double)rawfitness) / ind2.genome.Length),
                //... is the individual ideal?  Indicate here...
                false);
            ind2.Evaluated = true;
        }
    }
}