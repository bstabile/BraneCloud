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
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.GP.Koza;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.App.TwoBox
{
    /// <summary>
    /// TwoBox implements the TwoBox problem, with or without ADFs,
    /// as discussed in Koza-II.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>data</tt><br/>
    /// <font size="-1">classname, inherits or == ec.app.twobox.TwoBoxData</font></td>
    /// <td valign="top">(the class for the prototypical GPData object for the TwoBox problem)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>size</tt><br/>
    /// <font size="-1">int >= 1</font></td>
    /// <td valign="top">(the size of the training set)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>range</tt><br/>
    /// <font size="-1">int >= 1</font></td>
    /// <td valign="top">(the range of dimensional values in the training set -- they'll be integers 1...range inclusive)</td></tr>
    /// </table>
    /// 
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>data</tt></td>
    /// <td>species (the GPData object)</td></tr>
    /// </table>
    /// </summary>
    [ECConfiguration("ec.app.twobox.TwoBox")]
    public class TwoBox : GPProblem, ISimpleProblem
    {
        public const string P_SIZE = "size";
        public const string P_RANGE = "range";

        public int currentIndex;
        public int trainingSetSize;
        public int range;

        // these are read-only during evaluation-time, so
        // they can be just light-cloned and not deep cloned.
        // cool, huh?

        public double[] inputsl0;
        public double[] inputsw0;
        public double[] inputsh0;
        public double[] inputsl1;
        public double[] inputsw1;
        public double[] inputsh1;
        public double[] outputs;

        // we'll need to deep clone this one though.
        public TwoBoxData input;

        public double func(double l0, double w0,
            double h0, double l1,
            double w1, double h1)
        { return l0 * w0 * h0 - l1 * w1 * h1; }

        public override object Clone()
        {
            // don't bother copying the inputs and outputs; they're read-only :-)
            // don't bother copying the currentIndex; it's transitory
            // but we need to copy our twobox data
            var myobj = (TwoBox)(base.Clone());

            myobj.input = (TwoBoxData)(input.Clone());
            return myobj;
        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // very important, remember this
            base.Setup(state, paramBase);

            trainingSetSize = state.Parameters.GetInt(paramBase.Push(P_SIZE), null, 1);
            if (trainingSetSize < 1) state.Output.Fatal("Training Set Size must be an integer greater than 0");

            range = state.Parameters.GetInt(paramBase.Push(P_RANGE), null, 1);
            if (trainingSetSize < 1) state.Output.Fatal("Range must be an integer greater than 0");

            // Compute our inputs so they
            // can be copied with clone later

            inputsl0 = new double[trainingSetSize];
            inputsw0 = new double[trainingSetSize];
            inputsh0 = new double[trainingSetSize];
            inputsl1 = new double[trainingSetSize];
            inputsw1 = new double[trainingSetSize];
            inputsh1 = new double[trainingSetSize];
            outputs = new double[trainingSetSize];

            for (int x = 0; x < trainingSetSize; x++)
            {
                inputsl0[x] = state.Random[0].NextInt(range) + 1;
                inputsw0[x] = state.Random[0].NextInt(range) + 1;
                inputsh0[x] = state.Random[0].NextInt(range) + 1;
                inputsl1[x] = state.Random[0].NextInt(range) + 1;
                inputsw1[x] = state.Random[0].NextInt(range) + 1;
                inputsh1[x] = state.Random[0].NextInt(range) + 1;
                outputs[x] = func(inputsl0[x], inputsw0[x], inputsh0[x],
                    inputsl1[x], inputsw1[x], inputsh1[x]);
                state.Output.PrintLn("{" + inputsl0[x] + "," + inputsw0[x] + "," +
                    inputsh0[x] + "," + inputsl1[x] + "," +
                    inputsw1[x] + "," + inputsh1[x] + "," +
                    outputs[x] + "},", 0);
            }

            // set up our input -- don't want to use the default base, it's unsafe
            input = (TwoBoxData)state.Parameters.GetInstanceForParameterEq(
                paramBase.Push(P_DATA), null, typeof(TwoBoxData));
            input.Setup(state, paramBase.Push(P_DATA));
        }

        public void Evaluate(IEvolutionState state,
            Individual ind,
            int subpop,
            int threadnum)
        {
            if (!ind.Evaluated)  // don't bother reevaluating
            {
                var hits = 0;
                var sum = 0.0;
                for (var y = 0; y < trainingSetSize; y++)
                {
                    currentIndex = y;
                    ((GPIndividual)ind).Trees[0].Child.Eval(
                        state, threadnum, input, Stack, ((GPIndividual)ind), this);

                    const double HIT_LEVEL = 0.01;
                    const double PROBABLY_ZERO = 1.11E-15;
                    const double BIG_NUMBER = 1.0e15;  // the same as lilgp uses

                    var result = Math.Abs(outputs[y] - input.x);

                    // very slight math errors can creep in when evaluating
                    // two equivalent by differently-ordered functions, like
                    // x * (x*x*x + x*x)  vs. x*x*x*x + x*x

                    if (result < PROBABLY_ZERO)  // slightly off
                        result = 0.0;

                    if (result > BIG_NUMBER)
                        result = BIG_NUMBER;

                    if (result <= HIT_LEVEL) hits++;  // whatever!

                    sum += result;
                }

                // the fitness better be KozaFitness!
                var f = ((KozaFitness)ind.Fitness);
                f.SetStandardizedFitness(state, (float)sum);
                f.Hits = hits;
                ind.Evaluated = true;
            }
        }
    }
}