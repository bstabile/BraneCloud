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
using System.Text;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.GP.Koza;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.App.Regression
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
        private const long SerialVersionUID = 1;

        public const string P_SIZE = "size";
        public const string P_FILE = "file";
        public const string P_USE_FUNCTION = "use-function";

        public double CurrentValue;
        public int TrainingSetSize;
        //public File file;
        public bool UseFunction;  // if we have a file, should we use the function to compute the output values?  Or are they also contained?

        // these are read-only during evaluation-time, so
        // they can be just light-cloned and not deep cloned.
        // cool, huh?

        public double[] Inputs;
        public double[] Outputs;

        public virtual double Func(double x)
        {
            return x * x * x * x + x * x * x + x * x + x;
        }

        public override object Clone()
        {
            // don't bother copying the inputs and outputs; they're read-only :-)
            // don't bother copying the currentValue; it's transitory
            // but we need to copy our regression data
            var myobj = (Regression)base.Clone();

            myobj.Input = (RegressionData)Input.Clone();
            return myobj;
        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // very important, remember this
            base.Setup(state, paramBase);

            // verify our input is the right class (or subclasses from it)
            if (!(Input is RegressionData))
            state.Output.Fatal("GPData class must subclass from " + typeof(RegressionData).Name,
                paramBase.Push(P_DATA), null);

            TrainingSetSize = state.Parameters.GetInt(paramBase.Push(P_SIZE), null, 1);
            if (TrainingSetSize < 1) state.Output.Fatal("Training Set Size must be an integer greater than 0", paramBase.Push(P_SIZE));

            // should we load our x parameters from a file, or generate them randomly?
            //file = state.parameters.getFile(base.push(P_FILE), null);
            var inputfile = state.Parameters.GetResource(paramBase.Push(P_FILE), null);

            // *IF* we load from a file, should we generate the output through the function, or load the output as well?
            UseFunction = state.Parameters.GetBoolean(paramBase.Push(P_USE_FUNCTION), null, true);

            // Compute our inputs so they can be copied with clone later

            Inputs = new double[TrainingSetSize];
            Outputs = new double[TrainingSetSize];

            //if (file != null)  // use the file
            if (inputfile != null)
            {
                try
                {
                    var scan = new Scanner(inputfile);
                    for (var x = 0; x < TrainingSetSize; x++)
                    {
                        if (scan.HasNextDouble())
                            Inputs[x] = scan.NextDouble();
                        else state.Output.Fatal("Not enough data points in file: expected " + TrainingSetSize * (UseFunction ? 1 : 2));
                        if (!UseFunction)
                        {
                            if (scan.HasNextDouble())
                                Outputs[x] = scan.NextDouble();
                            else state.Output.Fatal("Not enough data points in file: expected " + TrainingSetSize * (UseFunction ? 1 : 2));
                        }
                    }
                }
                catch (FormatException e)
                {
                    state.Output.Fatal("Some tokens in the file were not numbers.");
                }
                //catch (IOException e)
                //      {
                //      state.output.fatal("The file could not be read due to an IOException:\n" + e);
                //      }
            }
            else for (var x = 0; x < TrainingSetSize; x++)
                {
                    // On p. 242 of Koza-I, he claims that the points are chosen from the
                    // fully-closed interval [-1, 1].  This is likely not true as Koza's lisp
                    // code usually selected stuff from half-open intervals.  But just to be
                    // absurdly exact here, we're allowing 1 as a valid number.
                    Inputs[x] = state.Random[0].NextDouble(true, true) * 2.0 - 1.0;     // fully closed interval.
                }
            for (var x = 0; x < TrainingSetSize; x++)
            {
                if (UseFunction)
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
                    ((GPIndividual)ind).Trees[0].Child.Eval(state, threadnum, Input, Stack, (GPIndividual)ind, this);

                    // It's possible to get NaN because cos(infinity) and
                    // sin(infinity) are undefined (hence cos(exp(3000)) zings ya!)
                    // So since NaN is NOT =,<,>,etc. any other number, including
                    // NaN, we're CAREFULLY wording our cutoff to include NaN.
                    // Interesting that this has never been reported before to
                    // my knowledge.

                    const double HIT_LEVEL = 0.01;
                    const double PROBABLY_ZERO = 1.11E-15;
                    const double BIG_NUMBER = 1.0e15;

                    var result = Math.Abs(Outputs[y] - ((RegressionData)Input).x);

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
                var f = (KozaFitness)ind.Fitness;
                f.SetStandardizedFitness(state, sum);
                f.Hits = hits;
                ind.Evaluated = true;
            }
        }
    }

    class Scanner : StreamReader
    {
        string _currentWord;

        public Scanner(Stream source)
            : base(source)
        {
            ReadNextWord();
        }

        private void ReadNextWord()
        {
            var sb = new StringBuilder();
            do
            {
                var next = Read();
                if (next < 0)
                    break;
                var nextChar = (char)next;
                if (char.IsWhiteSpace(nextChar))
                    break;
                sb.Append(nextChar);
            } while (true);
            while ((Peek() >= 0) && (char.IsWhiteSpace((char)Peek())))
                Read();
            if (sb.Length > 0)
                _currentWord = sb.ToString();
            else
                _currentWord = null;
        }

        public bool HasNextInt()
        {
            if (_currentWord == null)
                return false;
            int dummy;
            return int.TryParse(_currentWord, out dummy);
        }

        public int NextInt()
        {
            try
            {
                return int.Parse(_currentWord);
            }
            finally
            {
                ReadNextWord();
            }
        }

        public bool HasNextDouble()
        {
            if (_currentWord == null)
                return false;
            double dummy;
            return double.TryParse(_currentWord, out dummy);
        }

        public double NextDouble()
        {
            try
            {
                return double.Parse(_currentWord);
            }
            finally
            {
                ReadNextWord();
            }
        }

        public bool HasNext()
        {
            return _currentWord != null;
        }
    } 

}