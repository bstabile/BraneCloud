using System;
using System.Threading;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.Randomization;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.App.Majority
{

    /**
       MajorityGA.cs
            
       Implements a GA-style vector rule for the one-dimensional Majority-Ones cellular automaton problem.
       This code is in the spirit of Das, Crutchfield, Mitchel, and Hanson, "Evolving Globally Synchronized Cellular Automata",
       http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.55.7754&rep=rep1&type=pdf
       
       The primary difference is in the trials mechanism, in which we're using 25/25/50 rather than 50/50/0.
       
       If you run java ec.app.majority.MajorityGA, it'll test using the ABK rule instead (0.8342 using 10000 tests, 
       0.82528 if you use 100000 tests, 0.823961 if you use 1000000 tests)
    */

    [ECConfiguration("ec.app.majority.MajorityGA")]
    public class MajorityGA : Problem, ISimpleProblem
    {
        // kinds of trial types
        static int MAJORITY_ZERO = 0;

        static int MAJORITY_ONE = 1;
        static int RANDOM = -1;

        // How many trials in our training set
        public static int NUM_TRIALS = 128;

        // CA description
        public static int CA_WIDTH = 149;

        public static int NEIGHBORHOOD = 7;

        // How long can I run the CA if it's not converging?
        public static int STEPS = 200;

        readonly int[][] _trials = TensorFactory.Create<int>(NUM_TRIALS, CA_WIDTH);
        readonly int[] _majorities = new int[NUM_TRIALS];

        CA _ca;


        bool MakeTrial(IEvolutionState state, int thread, int[] trial, int trialType)
        {
            if (trialType == RANDOM)
            {
                int count = 0;
                for (int i = 0; i < CA_WIDTH; i++)
                {
                    trial[i] = state.Random[thread].NextInt(2);
                    count += trial[i];
                }
                return (count > CA_WIDTH / 2.0); // > 74
            }
            if (trialType == MAJORITY_ONE)
            {
                while (!MakeTrial(state, thread, trial, RANDOM)) ;
                return true;
            }
            if (trialType == MAJORITY_ZERO) // uniform selection
            {
                while (MakeTrial(state, thread, trial, RANDOM)) ;
                return false;
            }
            state.Output.Fatal("This should never happen");
            return false;
        }


        public void GenerateTrials(IEvolutionState state, int thread)
        {
            // the trials strategy here is: 25% ones, 25% zeros, and 50%.Random choice

            //IMersenneTwister mtf = state.Random[thread];

            for (int i = 0; i < NUM_TRIALS / 4; i++)
            {
                _majorities[i] = MakeTrial(state, thread, _trials[i], MAJORITY_ZERO) ? 1 : 0;
            }

            for (int i = NUM_TRIALS / 4; i < NUM_TRIALS / 2; i++)
            {
                _majorities[i] = MakeTrial(state, thread, _trials[i], MAJORITY_ONE) ? 1 : 0;
            }
            for (int i = NUM_TRIALS / 2; i < NUM_TRIALS; i++)
            {
                _majorities[i] = MakeTrial(state, thread, _trials[i], RANDOM) ? 1 : 0;
            }

        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // very important, remember this
            base.Setup(state, paramBase);
            GenerateTrials(state, 0);
        }



        // the purpose of this code is to guarantee that I regenerate trials each generation
        // and make sure that nobody is using them at the moment.

        int _lockCount = 0;
        private readonly object _syncLock = new object();

        public override void PrepareToEvaluate(IEvolutionState state, int threadnum)
        {
            if (threadnum != 0)
                lock (_syncLock)
                {
                    _lockCount++;
                }
        }

        public override void FinishEvaluating(IEvolutionState state, int threadnum)
        {
            if (threadnum != 0)
            {
                lock (_syncLock)
                {
                    _lockCount--;
                    Monitor.PulseAll(_syncLock);
                }
            }
            else // I'm thread 0
            {
                lock (_syncLock)
                {
                    while (_lockCount > 0)
                        try
                        {
                            Monitor.Wait(_syncLock);
                        }
                        catch (ThreadInterruptedException e)
                        {
                        }
                }

                // at this point I'm all alone!
                GenerateTrials(state, threadnum);
            }
        }


        public static bool All(int[] vals, int val)
        {
            for (int i = 0; i < vals.Length; i++)
                if (vals[i] != val) return false;
            return true;
        }


        public void Evaluate(IEvolutionState state,
            Individual ind,
            int subpopulation,
            int threadnum)
        {
            if (_ca == null)
                _ca = new CA(CA_WIDTH, NEIGHBORHOOD);

            // we always reevaluate         
            //if (!ind.evaluated)  // don't bother reevaluating
            {
                int sum = 0;

                bool[] genome = ((BitVectorIndividual) ind).genome;

                // extract the rule
                int[] rule = _ca.GetRule();
                for (int i = 0; i < 128; i++)
                    rule[i] = (genome[i] ? 1 : 0);
                _ca.SetRule(rule); // for good measure though it doesn't matter

                for (int i = 0; i < NUM_TRIALS; i++)
                {
                    // set up and run the CA
                    _ca.SetVals(_trials[i]);
                    _ca.Step(STEPS, true);

                    // extract the fitness
                    if (All(_ca.GetVals(), _majorities[i]))
                        sum++;
                }
                SimpleFitness f = (SimpleFitness) ind.Fitness;
                f.SetFitness(state, (sum / (float) NUM_TRIALS), false);
                ind.Evaluated = true;
            }
        }



        public static int NUM_TESTS = 10000;

        double _density = 0.0;

        public override void Describe(
            IEvolutionState state,
            Individual ind,
            int subpopulation,
            int threadnum,
            int log)
        {
            if (_ca == null)
                _ca = new CA(CA_WIDTH, NEIGHBORHOOD);

            int[] trial = new int[CA_WIDTH];

            bool[] genome = ((BitVectorIndividual) ind).genome;

            // extract the rule
            int[] rule = _ca.GetRule();
            for (int i = 0; i < 128; i++)
                rule[i] = (genome[i] ? 1 : 0);
            _ca.SetRule(rule); // for good measure though it doesn't matter

            double sum = 0;
            for (int i = 0; i < NUM_TESTS; i++)
            {
                // set up and run the CA
                int result = MakeTrial(state, threadnum, trial, RANDOM) ? 1 : 0;
                _ca.SetVals(trial);
                _ca.Step(STEPS, true);

                // extract the fitness
                if (All(_ca.GetVals(), result)) sum++;
            }

            _density = sum / NUM_TESTS;

            if (state.Output == null) // can happen if we call from main() below
                Console.Error.WriteLine("Generalization Accuracy: " + _density);
            else
            {
                state.Output.PrintLn("Generalization Accuracy: " + _density, 1); // stderr
                state.Output.PrintLn("Generalization Accuracy: " + _density, log);
            }
        }


        public static void main(String[] args)
        {
            // tests the ABK rule 

            int[] ABK =
            {
                0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0,
                0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1
            };
            var state = new EvolutionState {Random = new IMersenneTwister[] {new MersenneTwisterFast(500)}};
            var ga = new MajorityGA();
            ga.Setup(state, new Parameter(""));
            var bvi = new BitVectorIndividual
            {
                Fitness = new SimpleFitness(),
                genome = new bool[128]
            };
            for (int i = 0; i < 128; i++)
                bvi.genome[i] = ABK[i] != 0;
            ga.Evaluate(state, bvi, 0, 0);
            Console.Error.WriteLine("ABK Rule");
            ga.Describe(state, bvi, 0, 0, 1);
        }
    }
}