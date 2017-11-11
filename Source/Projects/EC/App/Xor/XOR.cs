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
using BraneCloud.Evolution.EC.NEAT;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.App.Xor
{
    [ECConfiguration("ec.app.xor.XOR")]
    public class XOR : Problem, ISimpleProblem
    {
        public virtual void Evaluate(IEvolutionState state, Individual ind, int subpopulation, int threadnum)
        {
            if (ind.Evaluated) return;

            if (!(ind is NEATIndividual))
                state.Output.Fatal("Whoa! It's not a NEATIndividual!!!", null);

            NEATIndividual neatInd = (NEATIndividual) ind;

            if (!(neatInd.Fitness is SimpleFitness))
                state.Output.Fatal("Whoa! It's not a SimpleFitness!!!", null);

            //The four possible input combinations to xor
            //The first number is for biasing
            double[][] input =
            {
                new [] {1.0, 0.0, 0.0}, // output 0
                new [] {1.0, 0.0, 1.0}, //        1
                new [] {1.0, 1.0, 0.0}, //        1
                new [] {1.0, 1.0, 1.0}, //        0
            };

            double[] output = new double[4];
            double[] expectedOut = {0.0, 1.0, 1.0, 0.0};

            NEATNetwork net = neatInd.CreateNetwork();

            int netDepth = net.MaxDepth();

            // Load and activate the network on each input
            for (int i = 0; i < input.Length; i++)
            {
                net.LoadSensors(input[i]);

                for (int relax = 0; relax < netDepth; relax++)
                {
                    net.Activate(state);
                }

                // only have one output, so let's get it
                output[i] = net.GetOutputResults()[0];

                net.Flush();
            }


            // calculate fitness

            double errorSum = 0;
            for (int i = 0; i < output.Length; i++)
                errorSum += Math.Abs(output[i] - expectedOut[i]);

            double fitness = (4.0 - errorSum) * (4.0 - errorSum);

            // this is from the original code for counting as ideal
            bool ideal = true;
            for (int i = 0; i < output.Length; i++)
                if (Math.Abs(output[i] - expectedOut[i]) > 0.5)
                {
                    ideal = false;
                    break;
                }

            ((SimpleFitness) neatInd.Fitness).SetFitness(state, fitness, ideal);
            neatInd.Evaluated = true;
        }

    }
}