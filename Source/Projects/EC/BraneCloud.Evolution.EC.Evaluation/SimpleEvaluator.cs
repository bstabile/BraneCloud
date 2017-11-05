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
using System.Linq;
using System.Threading.Tasks.Dataflow;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Support;

namespace BraneCloud.Evolution.EC.Simple
{
    /// <summary> 
    /// The SimpleEvaluator is a simple, non-coevolved generational evaluator which
    /// evaluates every single member of every subpop individually in its
    /// own problem space.  One Problem instance is cloned from p_problem for
    /// each evaluating thread.  The Problem must implement ISimpleProblem.
    /// </summary>   
    [Serializable]
    [ECConfiguration("ec.simple.SimpleEvaluator")]
    public class SimpleEvaluator : Evaluator
    {
        #region Constants

        public const string P_CLONE_PROBLEM = "clone-problem";
        public const string P_NUM_TESTS = "num-tests";
        public const string P_MERGE = "merge";

        public const string V_MEAN = "mean";
        public const string V_MEDIAN = "median";
        public const string V_BEST = "best";
        
        public const string P_CHUNK_SIZE = "chunk-size";
        public const string V_AUTO = "auto";

        public const int MERGE_MEAN = 0;
        public const int MERGE_MEDIAN = 1;
        public const int MERGE_BEST = 2;

        public const int C_AUTO = 0;

        #endregion

        #region Private Fields

        object[] _locks = new object[0]; // Arrays are serializable
        int individualCounter = 0;
        int subPopCounter = 0;
        int chunkSize;  // a value >= 1, or C_AUTO

        Population oldpop = null;

        #endregion

        #region Properties

        public int numTests { get; set; } = 1;
        public int mergeForm { get; set; } = MERGE_MEAN;
        public bool CloneProblem { get; set; }

        #endregion

        #region Setup

        /// <summary>
        /// Checks to make sure that the Problem implements ISimpleProblem.
        /// </summary>
        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            if (!(p_problem is ISimpleProblem))
                state.Output.Fatal(GetType().Name + " used, but the Problem is not of ISimpleProblem", paramBase.Push(P_PROBLEM));

            CloneProblem = state.Parameters.GetBoolean(paramBase.Push(P_CLONE_PROBLEM), null, true);
            if (!CloneProblem && state.BreedThreads > 1) // uh oh, this can't be right
                state.Output.Fatal("The Evaluator is not cloning its Problem, but you have more than one thread.", paramBase.Push(P_CLONE_PROBLEM));

            numTests = state.Parameters.GetInt(paramBase.Push(P_NUM_TESTS), null, 1);
            if (numTests < 1) numTests = 1;
            else if (numTests > 1)
            {
                String m = state.Parameters.GetString(paramBase.Push(P_MERGE), null);
                if (m == null)
                    state.Output.Warning("Merge method not provided to SimpleEvaluator.  Assuming 'mean'");
                else if (m.Equals(V_MEAN))
                    mergeForm = MERGE_MEAN;
                else if (m.Equals(V_MEDIAN))
                    mergeForm = MERGE_MEDIAN;
                else if (m.Equals(V_BEST))
                    mergeForm = MERGE_BEST;
                else
                    state.Output.Fatal("Bad merge method: " + m, paramBase.Push(P_NUM_TESTS), null);
            }

            if (!state.Parameters.ParameterExists(paramBase.Push(P_CHUNK_SIZE), null))
            {
                chunkSize = C_AUTO;
            }
            else if (state.Parameters.GetString(paramBase.Push(P_CHUNK_SIZE), null).Equals(V_AUTO, StringComparison.InvariantCultureIgnoreCase))
            {
                chunkSize = C_AUTO;
            }
            else
            {
                chunkSize = (state.Parameters.GetInt(paramBase.Push(P_CHUNK_SIZE), null, 1));
                if (chunkSize == 0)  // uh oh
                    state.Output.Fatal("Chunk Size must be either an integer >= 1 or 'auto'", paramBase.Push(P_CHUNK_SIZE), null);
            }
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// replace the population with one that has some N copies of the original individuals
        /// </summary>
        /// <param name="state"></param>
        void Expand(IEvolutionState state)
        {
            Population pop = (Population)state.Population.EmptyClone();

            // populate with clones
            for (int i = 0; i < pop.Subpops.Length; i++)
            {
                pop.Subpops[i].Individuals = new Individual[numTests * state.Population.Subpops[i].Individuals.Length];
                for (int j = 0; j < state.Population.Subpops[i].Individuals.Length; j++)
                {
                    for (int k = 0; k < numTests; k++)
                    {
                        pop.Subpops[i].Individuals[numTests * j + k] =
                            (Individual)state.Population.Subpops[i].Individuals[j].Clone();
                    }
                }
            }

            // swap
            oldpop = state.Population;
            state.Population = pop;
        }

        // Take the N copies of the original individuals and fold their fitnesses back into the original
        // individuals, replacing them with the original individuals in the process.  See expand(...)
        void Contract(IEvolutionState state)
        {
            // swap back
            Population pop = state.Population;
            state.Population = oldpop;

            // merge fitnesses again
            for (int i = 0; i < pop.Subpops.Length; i++)
            {
                IFitness[] fits = new IFitness[numTests];
                for (int j = 0; j < state.Population.Subpops[i].Individuals.Length; j++)
                {
                    for (int k = 0; k < numTests; k++)
                    {
                        fits[k] = pop.Subpops[i].Individuals[numTests * j + k].Fitness;
                    }

                    if (mergeForm == MERGE_MEAN)
                    {
                        state.Population.Subpops[i].Individuals[j].Fitness.SetToMeanOf(state, fits);
                    }
                    else if (mergeForm == MERGE_MEDIAN)
                    {
                        state.Population.Subpops[i].Individuals[j].Fitness.SetToMedianOf(state, fits);
                    }
                    else  // MERGE_BEST
                    {
                        state.Population.Subpops[i].Individuals[j].Fitness.SetToBestOf(state, fits);
                    }

                    state.Population.Subpops[i].Individuals[j].Evaluated = true;
                }
            }
        }
        /// <summary>
        /// A simple evaluator that doesn't do any coevolutionary
        /// evaluation.  Basically it applies evaluation pipelines,
        /// one per thread, to various subchunks of a new population. 
        /// </summary>
        public override void EvaluatePopulation(IEvolutionState state)
        {
            if (numTests > 1)
                Expand(state);

            // reset counters.  Only used in multithreading
            individualCounter = 0;
            subPopCounter = 0;

            // start up if single-threaded?
            if (state.EvalThreads == 1)
            {
                int[] numinds = new int[state.Population.Subpops.Length];
                int[] from = new int[numinds.Length];

                for (int i = 0; i < numinds.Length; i++)
                {
                    numinds[i] = state.Population.Subpops[i].Individuals.Length;
                    from[i] = 0;
                }

                ISimpleProblem prob;
                if (CloneProblem)
                    prob = (ISimpleProblem) p_problem.Clone();
                else
                    prob = (ISimpleProblem) p_problem;  // just use the prototype
                EvalPopChunk(state, numinds, from, 0, prob);
            }
            else
            {
                ParallelEvaluation(state, (ISimpleProblem) p_problem, this);
            }

            if (numTests > 1)
                Contract(state);
        }

        void ParallelEvaluation(IEvolutionState state, ISimpleProblem prob, SimpleEvaluator evaluator)
        {
            // BRS: TPL DataFlow is cleaner and safer than using raw threads.

            // Limit the concurrency in case the user has gone overboard!
            var maxDegree = Math.Min(Environment.ProcessorCount, state.EvalThreads);
            var options = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegree };

            Action<SimpleEvaluatorThread> act = t => t.Run();
            var actionBlock = new ActionBlock<SimpleEvaluatorThread>(act, options);

            for (var i = 0; i < state.EvalThreads; i++)
            {
                var runnable = new SimpleEvaluatorThread
                {
                    ThreadNum = i,
                    State = state,
                    Problem = (ISimpleProblem) prob.Clone(),
                    Evaluator = evaluator,
                };
                actionBlock.Post(runnable);
            }
            actionBlock.Complete();
            actionBlock.Completion.Wait();
        }

        /// <summary>
        /// A private helper function for evaluatePopulation which evaluates a chunk
        /// of individuals in a subpop for a given thread.
        /// Although this method is declared
        /// protected, you should not call it. 
        /// </summary>		
        protected virtual void EvalPopChunk(IEvolutionState state, int[] numinds, int[] from, int threadnum, ISimpleProblem p)
        {
            p.PrepareToEvaluate(state, threadnum);

            var subpops = state.Population.Subpops;
            var len = subpops.Length;
            for (var pop = 0; pop < len; pop++)
            {
                // start evaluatin'!
                var fp = from[pop];
                var upperbound = fp + numinds[pop];
                var inds = subpops[pop].Individuals;
                for (var x = fp; x < upperbound; x++)
                    p.Evaluate(state, inds[x], pop, threadnum);
            }
            p.FinishEvaluating(state, threadnum);
        }

        // computes the chunk size if 'auto' is set.  This may be different depending on the subpopulation,
        // which is backward-compatible with previous ECJ approaches.
        int ComputeChunkSizeForSubpopulation(IEvolutionState state, int subpop, int threadnum)
        {
            int numThreads = state.EvalThreads;

            // we will have some extra individuals.  We distribute these among the early subpopulations
            int individualsPerThread = state.Population.Subpops[subpop].Individuals.Length / numThreads;  // integer division
            int slop = state.Population.Subpops[subpop].Individuals.Length - numThreads * individualsPerThread;

            if (threadnum >= slop) // beyond the slop
                return individualsPerThread;
            return individualsPerThread + 1;
        }

        /// <summary>
        /// The SimpleEvaluator determines that a run is complete by asking
        /// each individual in each population if he's optimal; if he 
        /// finds an individual somewhere that's optimal,
        /// he signals that the run is complete. 
        /// </summary>
        public override bool RunComplete(IEvolutionState state)
        {
            return state.Population.Subpops.Any(t1 => t1.Individuals.Any(t => t.Fitness.IsIdeal));
        }

        #endregion // Operations
        /// <summary>
        /// A private helper class for implementing multithreaded evaluation. 
        /// </summary>
        public class SimpleEvaluatorThread : IThreadRunnable
        {
            public int[] NumInds;
            public int[] From;
            public SimpleEvaluator Evaluator;
            public IEvolutionState State;
            public int ThreadNum;
            public ISimpleProblem Problem;

            private object _lock = new object();

            public virtual void Run()
            {
                Subpopulation[] subpops = State.Population.Subpops;

                int[] numinds = new int[subpops.Length];
                int[] from = new int[subpops.Length];

                int count = 1;
                int start = 0;
                int subpop = 0;

                while (true)
                {
                    // We need to grab the information about the next chunk we're responsible for.  This stays longer
                    // in the lock than I'd like :-(
                    lock (Evaluator._locks)
                    {
                        // has everyone done all the jobs?
                        if (Evaluator.subPopCounter >= subpops.Length) // all done
                            return;

                        // has everyone finished the jobs for this subpopulation?
                        if (Evaluator.individualCounter >= subpops[Evaluator.subPopCounter].Individuals.Length)  // try again, jump to next subpop
                        {
                            Evaluator.individualCounter = 0;
                            Evaluator.subPopCounter++;

                            // has everyone done all the jobs?  Check again.
                            if (Evaluator.subPopCounter >= subpops.Length) // all done
                                return;
                        }

                        start = Evaluator.individualCounter;
                        subpop = Evaluator.subPopCounter;
                        count = Evaluator.chunkSize;
                        if (count == C_AUTO)  // compute automatically for subpopulations
                            count = Evaluator.ComputeChunkSizeForSubpopulation(State, subpop, ThreadNum);

                        Evaluator.individualCounter += count;  // it can be way more than we'll actually do, that's fine                    
                    }

                    // Modify the true count
                    if (count >= subpops[subpop].Individuals.Length - start)
                        count = subpops[subpop].Individuals.Length - start;

                    // Load into arrays to reuse evalPopChunk
                    for (int i = 0; i < from.Length; i++)
                        numinds[i] = 0;

                    numinds[subpop] = count;
                    from[subpop] = start;
                    Evaluator.EvalPopChunk(State, numinds, from, ThreadNum, Problem);
                }
            }
        }
    }

}
