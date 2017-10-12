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
using System.Collections;
using System.Threading;

using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.ES
{
    /// <summary> 
    /// MuCommaLambdaBreeder is a Breeder which, together with
    /// ESSelection, implements the (mu,Lambda) breeding strategy and gathers
    /// the comparison data you can use to implement a 1/5-rule mutation mechanism.
    /// 
    /// <p/>Evolution strategies breeders require a "mu" parameter and a "Lambda"
    /// parameter for each subpop.  "mu" refers to the number of parents
    /// from which the new population will be built.  "Lambda" refers to the
    /// number of children generated by the mu parents.  Subpopulation sizes
    /// will change as necessary to accommodate this fact in later generations.
    /// The only rule for initial subpop sizes is that they must be
    /// greater than or equal to the mu parameter for that subpop.
    /// 
    /// <p/>You can now set your initial subpop
    /// size to whatever you like, totally independent of Lambda and mu,
    /// as long as it is &gt;= mu.
    /// 
    /// <p/>MuCommaLambdaBreeder stores mu and Lambda values for each subpop
    /// in the population, as well as comparisons.  A comparison tells you
    /// if &gt;1/5, &lt;1/5 or =1/5 of the new population was better than its
    /// parents (the so-called evolution strategies "one-fifth rule".
    /// Although the comparisons are gathered, no mutation objects are provided
    /// which actually <i>use</i> them -- you're free to use them in any mutation
    /// objects you care to devise which requires them.
    /// 
    /// <p/>To do evolution strategies evolution, the
    /// breeding pipelines should contain at least one ESSelection selection method.
    /// While a child is being generated by the pipeline, the ESSelection object will return a parent
    /// from the pool of mu parents.  The particular parent is chosen round-robin, so all the parents
    /// will have an equal number of children.  It's perfectly fine to have more than one ESSelection
    /// object in the tree, or to call the same one repeatedly during the course of generating a child;
    /// all such objects will consistently return the same parent.  They only increment to the next
    /// parent in the pool of mu parents after the child has been created from the pipeline.  You can
    /// also mix ESSelection operators with other operators (like Tournament Selection).  But you ought
    /// to have <b>at least one</b> ESSelection operator in the pipeline -- else it wouldn't be Evolution
    /// Strategies, would it?
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top">es.Lambda.<i>subpop-num</i><br/>
    /// <font size="-1">int >= 0</font></td><td>Specifies the 'Lambda' parameter for the subpop.</td>
    /// </tr>
    /// <tr><td valign="top">es.mu.<i>subpop-num</i><br/>
    /// <font size="-1">int:  a multiple of "Lambda"</font></td><td>Specifies the 'mu' parameter for the subpop.</td>
    /// </tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.es.MuCommaLambdaBreeder")]
    public class MuCommaLambdaBreeder : Breeder
    {
        /// <summary>
        /// The following private class does NOT have an equivalent in ECJ!
        /// </summary>
        private class AnonymousClassComparator : IComparer
        {
            public AnonymousClassComparator(MuCommaLambdaBreeder enclosingInstance)
            {
                InitBlock(enclosingInstance);
            }
            private void  InitBlock(MuCommaLambdaBreeder enclosingInstance)
            {
                Enclosing_Instance = enclosingInstance;
            }

            public MuCommaLambdaBreeder Enclosing_Instance { get; private set; }

            public virtual int Compare(object o1, object o2)
            {
                var a = (Individual) o1;
                var b = (Individual) o2;
                // return 1 if should appear after object b in the array.
                // This is the case if a has WORSE fitness.
                if (b.Fitness.BetterThan(a.Fitness))
                    return 1;
                // return -1 if a should appear before object b in the array.
                // This is the case if b has WORSE fitness.
                if (a.Fitness.BetterThan(b.Fitness))
                    return - 1;
                // else return 0
                return 0;
            }
        }

        #region Constants

        public const string P_MU = "mu";
        public const string P_LAMBDA = "lambda";

        public const sbyte C_OVER_ONE_FIFTH_BETTER = 1;
        public const sbyte C_UNDER_ONE_FIFTH_BETTER = -1;
        public const sbyte C_EXACTLY_ONE_FIFTH_BETTER = 0;

        #endregion // Constants
        #region Properties

        public int[] Mu { get; set; }
        public int[] Lambda { get; set; }

        public Population ParentPopulation { get; set; }

        public sbyte[] Comparison { get; set; }

        /// <summary>
        /// Modified by multiple threads, don't fool with this. 
        /// </summary>
        public int[] Count { get; set; }

        public int[] Children { get; set; }
        public int[] Parents { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // we're not using the paramBase
            var p = new Parameter(Initializer.P_POP).Push(Population.P_SIZE);
            // if size is wrong, we'll let Population complain about it -- for us, we'll just make 0-sized arrays and drop out.
            var size = state.Parameters.GetInt(p, null, 1);

            Mu = new int[size];
            Lambda = new int[size];
            Comparison = new sbyte[size];

            // load mu and Lambda data
            for (var x = 0; x < size; x++)
            {
                Lambda[x] = state.Parameters.GetInt(ESDefaults.ParamBase.Push(P_LAMBDA).Push("" + x), null, 1);
                if (Lambda[x] == 0)
                    state.Output.Error("Lambda must be an integer >= 1", ESDefaults.ParamBase.Push(P_LAMBDA).Push("" + x));
                Mu[x] = state.Parameters.GetInt(ESDefaults.ParamBase.Push(P_MU).Push("" + x), null, 1);
                if (Mu[x] == 0)
                    state.Output.Error("mu must be an integer >= 1", ESDefaults.ParamBase.Push(P_MU).Push("" + x));
                else if ((Lambda[x] / Mu[x]) * Mu[x] != Lambda[x]) // note integer division
                    state.Output.Error("mu must be a multiple of Lambda", ESDefaults.ParamBase.Push(P_MU).Push("" + x));
            }
            state.Output.ExitIfErrors();
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Sets all subpops in pop to the expected Lambda size.  Does not fill new slots with individuals. 
        /// </summary>
        public virtual Population SetToLambda(Population pop, IEvolutionState state)
        {
            for (var x = 0; x < pop.Subpops.Length; x++)
            {
                var s = Lambda[x];

                // check to see if the array's not the right size
                if (pop.Subpops[x].Individuals.Length != s)
                // need to increase
                {
                    var newinds = new Individual[s];
                    Array.Copy(pop.Subpops[x].Individuals, 0, newinds, 0,
                        s < pop.Subpops[x].Individuals.Length ? s : pop.Subpops[x].Individuals.Length);

                    pop.Subpops[x].Individuals = newinds;
                }
            }
            return pop;
        }

        public override Population BreedPopulation(IEvolutionState state)
        {
            // Complete 1/5 statistics for last population

            if (ParentPopulation != null)
            {
                // Only go from 0 to Lambda-1, as the remaining individuals may be parents.
                // A child C's parent's index I is equal to C / Mu[subpop].
                for (var x = 0; x < state.Population.Subpops.Length; x++)
                {
                    var numChildrenBetter = 0;
                    for (var i = 0; i < Lambda[x]; i++)
                    {
                        var parent = i / (Lambda[x] / Mu[x]); // note integer division
                        if (state.Population.Subpops[x].Individuals[i].Fitness.BetterThan(ParentPopulation.Subpops[x].Individuals[parent].Fitness))
                            numChildrenBetter++;
                    }
                    if (numChildrenBetter > Lambda[x] / 5.0) // note float division
                        Comparison[x] = C_OVER_ONE_FIFTH_BETTER;
                    else if (numChildrenBetter < Lambda[x] / 5.0) // note float division
                        Comparison[x] = C_UNDER_ONE_FIFTH_BETTER;
                    else
                        Comparison[x] = C_EXACTLY_ONE_FIFTH_BETTER;
                }
            }

            // load the parent population
            ParentPopulation = state.Population;

            // MU COMPUTATION

            // At this point we need to do load our population info
            // and make sure it jibes with our mu info

            // the first issue is: is the number of subpops
            // equal to the number of mu's?

            if (Mu.Length != state.Population.Subpops.Length) // uh oh
                state.Output.Fatal("For some reason the number of subpops is different than was specified in the file (conflicting with Mu and Lambda storage).", null);

            // next, load our population, make sure there are no subpops smaller than the mu's
            for (var x = 0; x < state.Population.Subpops.Length; x++)
            {
                if (state.Population.Subpops[0].Individuals.Length < Mu[x])
                    state.Output.Error("Subpopulation " + x + " must be a multiple of the equivalent mu (that is, " + Mu[x] + ").");
            }
            state.Output.ExitIfErrors();




            var numinds = new int[state.BreedThreads][];
            for (var i = 0; i < state.BreedThreads; i++)
            {
                numinds[i] = new int[state.Population.Subpops.Length];
            }
            var from = new int[state.BreedThreads][];
            for (var i2 = 0; i2 < state.BreedThreads; i2++)
            {
                from[i2] = new int[state.Population.Subpops.Length];
            }

            // sort evaluation to get the Mu best of each subpop

            for (var x = 0; x < state.Population.Subpops.Length; x++)
            {
                var i = state.Population.Subpops[x].Individuals;

                Array.Sort(i, new AnonymousClassComparator(this));
            }

            // now the subpops are sorted so that the best individuals
            // appear in the lowest indexes.

            var newpop = SetToLambda((Population)state.Population.EmptyClone(), state);

            // create the count array
            Count = new int[state.BreedThreads];

            // divvy up the Lambda individuals to create

            for (var y = 0; y < state.BreedThreads; y++)
                for (var x = 0; x < state.Population.Subpops.Length; x++)
                {
                    // figure numinds
                    if (y < state.BreedThreads - 1)
                        // not last one
                        numinds[y][x] = Lambda[x] / state.BreedThreads;
                    // in case we're slightly off in division
                    else
                        numinds[y][x] = Lambda[x] / state.BreedThreads + (Lambda[x] - (Lambda[x] / state.BreedThreads) * state.BreedThreads);

                    // figure from
                    from[y][x] = (Lambda[x] / state.BreedThreads) * y;
                }

            if (state.BreedThreads == 1)
            {
                BreedPopChunk(newpop, state, numinds[0], from[0], 0);
            }
            else
            {
                var t = new ThreadClass[state.BreedThreads];

                // start up the threads
                for (var y = 0; y < state.BreedThreads; y++)
                {
                    var r = new MuLambdaBreederThread
                    {
                        ThreadNum = y,
                        NewPop = newpop,
                        NumInds = numinds[y],
                        From = from[y],
                        Me = this,
                        State = state
                    };
                    t[y] = new ThreadClass(new ThreadStart(r.Run));
                    t[y].Start();
                }

                // gather the threads
                for (var y = 0; y < state.BreedThreads; y++)
                {
                    try
                    {
                        t[y].Join();
                    }
                    catch (ThreadInterruptedException)
                    {
                        state.Output.Fatal("Whoa! The main breeding thread got interrupted!  Dying...");
                    }
                }
            }

            return PostProcess(newpop, state.Population, state);
        }

        /// <summary>
        /// A hook for Mu+Lambda, not used in Mu,Lambda. 
        /// </summary>        
        public virtual Population PostProcess(Population newpop, Population oldpop, IEvolutionState state)
        {
            return newpop;
        }

        /// <summary>
        /// A private helper function for breedPopulation which breeds a chunk
        /// of individuals in a subpop for a given thread.
        /// Although this method is declared
        /// public (for the benefit of a private helper class in this file),
        /// you should not call it. 
        /// </summary>        
        public virtual void BreedPopChunk(Population newpop, IEvolutionState state, int[] numinds, int[] from, int threadnum)
        {
            for (var subpop = 0; subpop < newpop.Subpops.Length; subpop++)
            {
                // reset the appropriate count slot  -- this used to be outside the for-loop, a bug
                // I believe
                Count[threadnum] = 0;

                var bp = (BreedingPipeline)newpop.Subpops[subpop].Species.Pipe_Prototype.Clone();

                // check to make sure that the breeding pipeline produces
                // the right kind of individuals.  Don't want a mistake there! :-)
                if (!bp.Produces(state, newpop, subpop, threadnum))
                {
                    state.Output.Fatal("The Breeding Pipeline of subpop " + subpop
                        + " does not produce individuals of the expected species "
                        + newpop.Subpops[subpop].Species.GetType().FullName + " or fitness "
                        + newpop.Subpops[subpop].Species.F_Prototype);
                }
                bp.PrepareToProduce(state, subpop, threadnum);
                if (Count[threadnum] == 0)
                    // the ESSelection didn't set it to nonzero to inform us of his existence
                    state.Output.Warning("Whoa!  Breeding Pipeline for subpop " + subpop
                        + " doesn't have an ESSelection, but is being used by MuCommaLambdaBreeder or MuPlusLambdaBreeder."
                        + "  That's probably not right.");
                // reset again
                Count[threadnum] = 0;

                // start breedin'!

                var upperbound = from[subpop] + numinds[subpop];
                for (var x = from[subpop]; x < upperbound; x++)
                {
                    if (bp.Produce(1, 1, x, subpop, newpop.Subpops[subpop].Individuals, state, threadnum) != 1)
                        state.Output.Fatal("Whoa! Breeding Pipeline for subpop " + subpop
                            + " is not producing one individual at a time, as is required by the MuLambda strategies.");

                    // increment the count
                    Count[threadnum]++;
                }
                bp.FinishProducing(state, subpop, threadnum);
            }
        }
        
        #endregion // Operations
     }
        
    /// <summary>
    /// A private helper class for implementing multithreaded breeding. 
    /// </summary>
    class MuLambdaBreederThread : IThreadRunnable
    {
        internal Population NewPop;
        public int[] NumInds;
        public int[] From;
        public MuCommaLambdaBreeder Me;
        public IEvolutionState State;
        public int ThreadNum;
        public virtual void  Run()
        {
            Me.BreedPopChunk(NewPop, State, NumInds, From, ThreadNum);
        }
    }
}