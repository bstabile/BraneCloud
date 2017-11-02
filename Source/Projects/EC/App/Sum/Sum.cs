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

namespace BraneCloud.Evolution.EC.App.Sum
{
    [ECConfiguration("ec.app.sum.Sum")]
    public class Sum : Problem, ISimpleProblem
    {
        public const string P_SUM = "sum";

        public override IParameter DefaultBase => base.DefaultBase.Push(P_SUM);
       

        public void Evaluate(IEvolutionState state,
            Individual ind,
            int subpop,
            int threadnum)
        {
            if (ind.Evaluated) return;

            if (!(ind is IntegerVectorIndividual))
                state.Output.Fatal("Whoa!  It's not an IntegerVectorIndividual!!!", null);

            var ind2 = (IntegerVectorIndividual)ind;
            var s = (IntegerVectorSpecies)ind2.Species;

            long sum = 0;
            long max = 0;
            for (var x = 0; x < ind2.genome.Length; x++)
            {
                sum += ind2.genome[x];
                max += (int)s.GetMaxGene(x);  // perhaps this neededn't be computed over and over again
            }

            // Now we know that max is the maximum possible value, and sum is the fitness.

            // assume we're using SimpleFitness
            ((SimpleFitness)ind2.Fitness).SetFitness(state,
                // ...the fitness...
                sum,
                //... our definition of the ideal individual
                sum == max);

            ind2.Evaluated = true;
        }
    }
}