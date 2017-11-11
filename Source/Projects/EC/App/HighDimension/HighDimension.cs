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
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.App.HighDimension
{
    public class HighDimension : Problem , ISimpleProblem
    {
    public const string P_HIGHDIMENSION = "high-dimension";

    public override IParameter DefaultBase => base.DefaultBase.Push(P_HIGHDIMENSION);

    public virtual void Evaluate(IEvolutionState state, Individual ind, int subpopulation, int threadnum)
    {

        if (!(ind is IntegerVectorIndividual))
        // TODO : the output text may need to change
        state.Output.Fatal("Whoa!  It's not an IntegerVectorIndividual!!!", null);

        int[] genome = ((IntegerVectorIndividual) ind).genome;
        if (genome.Length != 5)
            // TODO : the output text may need to change
            state.Output.Fatal("Whoa! The size of the genome is not right!!!", null);

        double gamma = 1e-3;
        long xi = 0;
        double beta = 1e4;
        double g = 0;

        double sum = 0;
        for (int j = 0; j < genome.Length; ++j)
        {
            sum += (genome[j] - xi) * (genome[j] - xi) * gamma;
        }

        g = beta * Math.Exp(-sum);

        double variance = g * 0.09;
        // TODO: how should we use noise?
        double noise = variance < 1e-30 ? 0 : state.Random[0].NextGaussian() * variance;

        // We return g as the fitness, as opposed in original code, where -g is returned.
        // Since we are try to maximize our fitness value, not find a min -g solution
        throw new NotImplementedException("DOvS is still under construction!");
        //((DOVSFitness) ind.Fitness).recordObservation(state, g);
        //ind.Evaluated = true;
    }

    }
}
