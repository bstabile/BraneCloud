using System;
using System.Threading;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.Randomization;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.App.Majority
{

    /**
       MajorityGP.cs
            
       Implements a GP-style vector rule for the one-dimensional Majority-Ones cellular automaton problem.
       This code is in the spirit of Das, Crutchfield, Mitchel, and Hanson, "Evolving Globally Synchronized Cellular AUtomata",
       http://web.cecs.pdx.edu/~mm/EGSCA.pdf
       
       The primary difference is in the trials mechanism, in which we're using 25/25/50 rather than 50/50/0.
    */

    [ECConfiguration("ec.app.majority.MajorityGP")]
    public class MajorityGP : GPProblem, ISimpleProblem
    {

        // How many trials in our training set
        public static int NUM_TRIALS = 128;

        // CA description
        public static int CA_WIDTH = 149;

        public static int NEIGHBORHOOD = 7;

        // How long can I run the CA if it's not converging?
        public static int STEPS = 200;


        public static int NUM_TESTS = 10000;

        // kinds of trial types
        static int MAJORITY_ZERO = 0;
        static int MAJORITY_ONE = 1;
        static int RANDOM = -1;

        double _density = 0.0;
        CA _ca;

        private readonly int[][] _trials = TensorFactory.Create<int>(NUM_TRIALS, CA_WIDTH);
        readonly int[] _majorities = new int[NUM_TRIALS];


        bool MakeTrial(IEvolutionState state, int thread, int[] trial, int trialType)
        {
            if (trialType == RANDOM)
            {
                int count = 0;
                for (var i = 0; i < CA_WIDTH; i++)
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

            for (var i = 0; i < NUM_TRIALS / 4; i++)
            {
                _majorities[i] = MakeTrial(state, thread, _trials[i], MAJORITY_ZERO) ? 1 : 0;
            }
            for (var i = NUM_TRIALS / 4; i < NUM_TRIALS / 2; i++)
            {
                _majorities[i] = MakeTrial(state, thread, _trials[i], MAJORITY_ONE) ? 1 : 0;
            }
            for (var i = NUM_TRIALS / 2; i < NUM_TRIALS; i++)
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

        int _lockCount;
        private readonly object _syncLock = new object();

        public override void PrepareToEvaluate(IEvolutionState state, int threadnum)
        {
            if (threadnum == 0) return;

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
                    {
                        try
                        {
                            Monitor.Wait(_syncLock);
                        }
                        catch (ThreadInterruptedException) { }
                    }
                }

                // at this point I'm all alone!
                GenerateTrials(state, threadnum);
            }
        }

        public static bool All(int[] vals, int val)
        {
            foreach (int v in vals)
                if (v != val) return false;
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
                MajorityData input = (MajorityData) (this.Input);

                int sum = 0;

                // extract the rule
                ((GPIndividual) ind).Trees[0].Child.Eval(
                    state, threadnum, input, Stack, (GPIndividual) ind, this);

                int[] rule = _ca.GetRule();
                for (int i = 0; i < 64; i++)
                    rule[i] = (int) (((input.Data0) >> i) & 0x1);
                for (int i = 64; i < 128; i++)
                    rule[i] = (int) (((input.Data1) >> (i - 64)) & 0x1);
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
                f.SetFitness(state, (float) sum / (float) NUM_TRIALS, sum == NUM_TRIALS);
                ind.Evaluated = true;
            }
        }


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

            MajorityData input = (MajorityData) Input;

            // extract the rule
            ((GPIndividual) ind).Trees[0].Child.Eval(
                state, threadnum, input, Stack, (GPIndividual) ind, this);

            int[] rule = _ca.GetRule();
            for (int i = 0; i < 64; i++)
                rule[i] = (int) ((input.Data0 >> i) & 0x1);
            for (int i = 64; i < 128; i++)
                rule[i] = (int) ((input.Data1 >> (i - 64)) & 0x1);
            _ca.SetRule(rule); // for good measure though it doesn't matter

            // print rule                
            var s = "Rule: ";
            foreach (int r in rule)
                s += r;
            state.Output.PrintLn(s, log);

            double sum = 0;
            for (var i = 0; i < NUM_TESTS; i++)
            {
                // set up and run the CA
                int result = MakeTrial(state, threadnum, trial, RANDOM) ? 1 : 0;
                _ca.SetVals(trial);
                _ca.Step(STEPS, true);

                // extract the fitness
                if (All(_ca.GetVals(), result)) sum++;
            }

            _density = (sum / NUM_TESTS);
            state.Output.PrintLn("Generalization Accuracy: " + _density, 1); // stderr
            state.Output.PrintLn("Generalization Accuracy: " + _density, log);
        }

    }
}