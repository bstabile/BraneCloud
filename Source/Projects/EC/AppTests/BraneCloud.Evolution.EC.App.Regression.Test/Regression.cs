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

namespace BraneCloud.Evolution.EC.App.Regression.Test
{
    /// <summary>
    /// Regression implements the Koza (quartic) Symbolic Regression problem.
    /// 
    /// <p/>The equation to be regressed is y = x^4 + x^3 + x^2 + x, {x in [-1,1]}
    /// <p/>This equation was introduced in J. R. Koza, GP II, 1994.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>data</tt><br/>
    /// <font size="-1">classname, inherits or == ec.app.regression.RegressionData</font></td>
    /// <td valign="top">(the class for the prototypical GPData object for the Regression problem)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>size</tt><br/>
    /// <font size="-1">int >= 1</font></td>
    /// <td valign="top">(the size of the training set)</td></tr>
    /// </table>
    /// 
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>data</tt></td>
    /// <td>species (the GPData object)</td></tr>
    /// </table>
    /// </summary>
    [ECConfiguration("ec.app.regression.Regression")]
    public class Regression : GPProblem, ISimpleProblem
    {
        public const string P_SIZE = "size";

        public double CurrentValue;
        public int TrainingSetSize;

        // these are read-only during evaluation-time, so
        // they can be just light-cloned and not deep cloned.
        // cool, huh?

        public double[] Inputs;
        public double[] Outputs;

        /// <summary>
        /// we'll need to deep clone this one though.
        /// </summary>
        public new RegressionData Input;

        public virtual double Func(double x)
        {
            return x * x * x * x + x * x * x + x * x + x;
        }

        public override object Clone()
        {
            // don't bother copying the inputs and outputs; they're read-only :-)
            // don't bother copying the currentValue; it's transitory
            // but we need to copy our regression data
            var myobj = (Regression)(base.Clone());

            myobj.Input = (RegressionData)(Input.Clone());
            return myobj;
        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // very important, remember this
            base.Setup(state, paramBase);

            TrainingSetSize = state.Parameters.GetInt(paramBase.Push(P_SIZE), null, 1);
            if (TrainingSetSize < 1) state.Output.Fatal("Training Set Size must be an integer greater than 0", paramBase.Push(P_SIZE));

            // Compute our inputs so they can be copied with clone later

            Inputs = new double[TrainingSetSize];
            Outputs = new double[TrainingSetSize];

            for (var x = 0; x < TrainingSetSize; x++)
            {
                Inputs[x] = state.Random[0].NextDouble() * 2.0 - 1.0;
                Outputs[x] = Func(Inputs[x]);
                state.Output.Message("{" + Inputs[x] + "," + Outputs[x] + "},");
            }

            // set up our input -- don't want to use the default base, it's unsafe
            Input = (RegressionData)state.Parameters.GetInstanceForParameterEq(
                paramBase.Push(P_DATA), null, typeof(RegressionData));
            Input.Setup(state, paramBase.Push(P_DATA));
        }

        public void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            if (!ind.Evaluated)  // don't bother reevaluating
            {
                var hits = 0;
                var sum = 0.0;
                for (var y = 0; y < TrainingSetSize; y++)
                {
                    CurrentValue = Inputs[y];
                    ((GPIndividual)ind).Trees[0].Child.Eval(state, threadnum, Input, Stack, ((GPIndividual)ind), this);

                    // It's possible to get NaN because cos(infinity) and
                    // sin(infinity) are undefined (hence cos(exp(3000)) zings ya!)
                    // So since NaN is NOT =,<,>,etc. any other number, including
                    // NaN, we're CAREFULLY wording our cutoff to include NaN.
                    // Interesting that this has never been reported before to
                    // my knowledge.

                    const double HIT_LEVEL = 0.01;
                    const double PROBABLY_ZERO = 1.11E-15;
                    const double BIG_NUMBER = 1.0e15;

                    var result = Math.Abs(Outputs[y] - Input.x);

                    if (!(result < BIG_NUMBER))   // *NOT* (input.x >= BIG_NUMBER)
                        result = BIG_NUMBER;

                    // very slight math errors can creep in when evaluating
                    // two equivalent by differently-ordered functions, like
                    // x * (x*x*x + x*x)  vs. x*x*x*x + x*x

                    else if (result < PROBABLY_ZERO)  // slightly off
                        result = 0.0;

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