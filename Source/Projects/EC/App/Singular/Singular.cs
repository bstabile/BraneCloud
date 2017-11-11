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
using System.IO;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.App.Singular
{
    public class Singular : Problem, ISimpleProblem
    {
        public static String P_SINGULAR = "singular";

        public override IParameter DefaultBase => base.DefaultBase.Push(P_SINGULAR);

        public virtual void Evaluate(IEvolutionState state, Individual ind, int subpopulation, int
            threadnum)
        {

            if (!(ind is IntegerVectorIndividual))
            // TODO : the output text may need to change
            state.Output.Fatal("Whoa!  It's not an IntegerVectorIndividual!!!", null);

            int[] genome = ((IntegerVectorIndividual) ind).genome;
            if (genome.Length != 4)
                // TODO : the output text may need to change
                state.Output.Fatal("Whoa! The size of the genome is not right!!!", null);

            double sum = 1 + (genome[0] + 10 * genome[1]) * (genome[0] + 10 * genome[1])
                         + 5 * (genome[2] - genome[3]) * (genome[2] - genome[3])
                         + Math.Pow(((double) (genome[1] - 2 * genome[2])), 4.0)
                         + 10 * Math.Pow(((double) (genome[0] - genome[3])), 4.0);


            throw new NotImplementedException("DOvS is still under construction!");

            //// We return g as the fitness, as opposed in original code, where -g is return.
            //// Since we are try to maximize our fitness value, not find a min -g solution
            //((DOVSFitness) ind.Fitness).recordObservation(state, -sum);

            //ind.Evaluated = true;
        }

    }
}
