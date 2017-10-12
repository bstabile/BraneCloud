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
using System.Threading;
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Configuration;

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
        public const string P_CLONE_PROBLEM = "clone-problem";
        public bool CloneProblem { get; set; }

        #region Setup

        /// <summary>
        /// Checks to make sure that the Problem implements ISimpleProblem.
        /// </summary>
        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            if (!(p_problem is ISimpleProblem))
                state.Output.Fatal("" + GetType() + " used, but the Problem is not of ISimpleProblem", paramBase.Push(P_PROBLEM));

            CloneProblem = state.Parameters.GetBoolean(paramBase.Push(P_CLONE_PROBLEM), null, true);
            if (!CloneProblem && (state.BreedThreads > 1)) // uh oh, this can't be right
                state.Output.Fatal("The Evaluator is not cloning its Problem, but you have more than one thread.", paramBase.Push(P_CLONE_PROBLEM));
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// A simple evaluator that doesn't do any coevolutionary
        /// evaluation.  Basically it applies evaluation pipelines,
        /// one per thread, to various subchunks of a new population. 
        /// </summary>
        public override void EvaluatePopulation(IEvolutionState state)
        {
            if (state.EvalThreads == 1)
            {
            // a minor bit of optimization
            var numinds = new int[state.Population.Subpops.Length];
            var from = new int[state.Population.Subpops.Length];
            for(var i = 0; i < state.Population.Subpops.Length; i++)
                { numinds[i] = state.Population.Subpops[i].Individuals.Length; from[i] = 0; }
            if (CloneProblem)
                EvalPopChunk(state,numinds,from,0,(ISimpleProblem)(p_problem.Clone()));
            else
                EvalPopChunk(state,numinds,from,0,(ISimpleProblem)(p_problem));            
            }
            else
            {

                var numinds = new int[state.EvalThreads][];
                for (var i = 0; i < state.EvalThreads; i++)
                {
                    numinds[i] = new int[state.Population.Subpops.Length];
                }
                var from = new int[state.EvalThreads][];
                for (var i2 = 0; i2 < state.EvalThreads; i2++)
                {
                    from[i2] = new int[state.Population.Subpops.Length];
                }

                for (var y = 0; y < state.EvalThreads; y++)
                    for (var x = 0; x < state.Population.Subpops.Length; x++)
                    {
                        // figure numinds
                        if (y < state.EvalThreads - 1)
                            // not last one
                            numinds[y][x] = state.Population.Subpops[x].Individuals.Length / state.EvalThreads;
                        // in case we're slightly off in division
                        else
                            numinds[y][x] = state.Population.Subpops[x].Individuals.Length / state.EvalThreads
                                            + (state.Population.Subpops[x].Individuals.Length
                                               -
                                               (state.Population.Subpops[x].Individuals.Length / state.EvalThreads) *
                                               state.EvalThreads);

                        // figure from
                        from[y][x] = (state.Population.Subpops[x].Individuals.Length / state.EvalThreads) * y;
                    }

                var t = new ThreadClass[state.EvalThreads];

                // start up the threads
                for (var y = 0; y < state.EvalThreads; y++)
                {
                    var r = new SimpleEvaluatorThread
                    {
                        ThreadNum = y,
                        NumInds = numinds[y],
                        From = from[y],
                        Me = this,
                        State = state,
                        p = (ISimpleProblem)p_problem.Clone() // should ignore cloneProblem parameter here
                    };
                    t[y] = new ThreadClass(new ThreadStart(r.Run));
                    t[y].Start();
                }

                // gather the threads
                for (var y = 0; y < state.EvalThreads; y++)
                {
                    try
                    {
                        t[y].Join();
                    }
                    catch (ThreadInterruptedException)
                    {
                        state.Output.Fatal("Whoa! The main evaluation thread got interrupted!  Dying...");
                    }
                }
            }
        }

        /// <summary>
        /// A private helper function for evaluatePopulation which evaluates a chunk
        /// of individuals in a subpop for a given thread.
        /// Although this method is declared
        /// public (for the benefit of a private helper class in this file),
        /// you should not call it. 
        /// </summary>		
        protected internal virtual void EvalPopChunk(IEvolutionState state, int[] numinds, int[] from, int threadnum, ISimpleProblem p)
        {
            ((IProblem)p).PrepareToEvaluate(state, threadnum);

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
            ((IProblem)p).FinishEvaluating(state, threadnum);
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
    }
    
    /// <summary>
    /// A private helper class for implementing multithreaded evaluation. 
    /// </summary>
    class SimpleEvaluatorThread : IThreadRunnable
    {
        public int[] NumInds;
        public int[] From;
        public SimpleEvaluator Me;
        public IEvolutionState State;
        public int ThreadNum;
        public ISimpleProblem p;
        public virtual void  Run()
        {
            lock (this)
            {
                Me.EvalPopChunk(State, NumInds, From, ThreadNum, p);
            }
        }
    }
}