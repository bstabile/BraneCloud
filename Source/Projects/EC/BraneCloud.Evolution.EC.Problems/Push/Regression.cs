using System;
using System.IO;
using System.Text;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.GP.Koza;
using BraneCloud.Evolution.EC.GP.Push;
using BraneCloud.Evolution.EC.Psh;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.Problems.Push
{
    [ECConfiguration("ec.problems.push.Regression")]
    public class Regression : PushProblem, ISimpleProblem
    {
        private long SerialVersionUID = 1;

        public string P_SIZE = "size";
        public string P_FILE = "file";
        public string P_USE_FUNCTION = "use-function";
        public string P_MAX_STEPS = "max-steps";

        public int TrainingSetSize { get; set; }

        // if we have a file, should we use the function to compute the output values?  Or are they also contained?
        public bool UseFunction { get; set; } 

        // these are read-only during evaluation-time, so
        // they can be just light-cloned and not deep cloned.
        // cool, huh?

        public double[] Inputs { get; set; }
        public double[] Outputs { get; set; }

        public double Func(double x)
        {
            return x * x * x * x + x * x * x + x * x + x;
        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // very important, remember this
            base.Setup(state, paramBase);

            TrainingSetSize = state.Parameters.GetInt(paramBase.Push(P_SIZE), null, 1);
            if (TrainingSetSize < 1)
                state.Output.Fatal("Training Set Size must be an integer greater than 0", paramBase.Push(P_SIZE));

            // should we load our x parameters from a file, or generate them randomly?
            //file = state.Parameters.GetFile(paramBase.Push(P_FILE), null);
            Stream inputfile = state.Parameters.GetResource(paramBase.Push(P_FILE), null);

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
                    Scanner scan = new Scanner(inputfile);
                    for (int x = 0; x < TrainingSetSize; x++)
                    {
                        if (scan.HasNextDouble())
                            Inputs[x] = scan.NextDouble();
                        else
                            state.Output.Fatal("Not enough data points in file: expected " +
                                               TrainingSetSize * (UseFunction ? 1 : 2));
                        if (!UseFunction)
                        {
                            if (scan.HasNextDouble())
                                Outputs[x] = scan.NextDouble();
                            else
                                state.Output.Fatal("Not enough data points in file: expected " +
                                                   TrainingSetSize * (UseFunction ? 1 : 2));
                        }
                    }
                }
                catch (FormatException e)
                {
                    state.Output.Fatal("Some tokens in the file were not numbers.");
                }
            }
            else
                for (int x = 0; x < TrainingSetSize; x++)
                {
                    // On p. 242 of Koza-I, he claims that the points are chosen from the
                    // fully-closed interval [-1, 1].  This is likely not true as Koza's lisp
                    // code usually selected stuff from half-open intervals.  But just to be
                    // absurdly exact here, we're allowing 1 as a valid number.
                    Inputs[x] = state.Random[0].NextDouble(true, true) * 2.0 - 1.0; // fully closed interval.
                }

            for (int x = 0; x < TrainingSetSize; x++)
            {
                if (UseFunction)
                    Outputs[x] = Func(Inputs[x]);
                state.Output.Message("{" + Inputs[x] + "," + Outputs[x] + "},");
            }

            MaxSteps = state.Parameters.GetInt(paramBase.Push(P_MAX_STEPS), null, 0);
            if (MaxSteps < 0)
                state.Output.Fatal(
                    "Maximum Steps not specified, must be 1 or greater, or 0 to indicate no maximum number of steps.");
            if (MaxSteps == 0)
                state.Output.Warning("No maximum number of steps:. Push interpreter may get into an infinite loop.");
        }


        public int MaxSteps { get; set; }

    public void Evaluate(IEvolutionState state,
            Individual ind,
            int subpopulation,
            int threadnum)
        {
            if (!ind.Evaluated) // don't bother reevaluating
            {
                int hits = 0;
                double sum = 0.0;

                Interpreter interpreter = GetInterpreter(state, (GPIndividual) ind, threadnum);
                Psh.Program program = GetProgram(state, (GPIndividual) ind);

                for (int y = 0; y < TrainingSetSize; y++)
                {
                    if (y > 0) // need to reset first
                        ResetInterpreter(interpreter);

                    // load it up and run it
                    PushOntoFloatStack(interpreter, (float) (Inputs[y]));
                    ExecuteProgram(program, interpreter, MaxSteps);

                    // It's possible to get NaN because cos(infinity) and
                    // sin(infinity) are undefined (hence cos(exp(3000)) zings ya!)
                    // So since NaN is NOT =,<,>,etc. any other number, including
                    // NaN, we're CAREFULLY wording our cutoff to include NaN.
                    // Interesting that this has never been reported before to
                    // my knowledge.

                    double HIT_LEVEL = 0.01;
                    double PROBABLY_ZERO = 1E-6; // The Psh interpreter seems less accurate, not sure why
                    double BIG_NUMBER = 1.0e15; // the same as lilgp uses

                    var result = Math.Abs(Outputs[y] - TopOfFloatStack(interpreter));

                    if (!(result < BIG_NUMBER)) // *NOT* (input.x >= BIG_NUMBER)
                        result = BIG_NUMBER;

                    if (IsFloatStackEmpty(interpreter)) // uh oh, invalid value
                        result = BIG_NUMBER;

                    // very slight math errors can creep in when evaluating
                    // two equivalent by differently-ordered functions, like
                    // x * (x*x*x + x*x)  vs. x*x*x*x + x*x

                    else if (result < PROBABLY_ZERO) // slightly off
                        result = 0.0;

                    if (result <= HIT_LEVEL) hits++; // whatever!

                    sum += result;
                }

                // the fitness better be KozaFitness!
                var f = (KozaFitness) ind.Fitness;
                f.SetStandardizedFitness(state, (float) sum);
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