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
using System.Threading.Tasks.Dataflow;
using BraneCloud.Evolution.EC.Breed;
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Simple
{
    /// <summary> 
    /// Breeds each subpop separately, with no inter-population exchange,
    /// and using a generational approach.  A SimpleBreeder may have multiple
    /// threads; it divvys up a subpop into chunks and hands one chunk
    /// to each thread to populate.  One array of BreedingPipelines is obtained
    /// from a population's Species for each operating breeding thread.
    /// 
    /// <p/>Prior to breeding a subpop, a SimpleBreeder may first fill part of the new
    /// subpop up with the best <i>n</i> individuals from the old subpop.
    /// By default, <i>n</i> is 0 for each subpop (that is, this "elitism"
    /// is not done).  The elitist step is performed by a single thread.
    /// 
    /// <p/>If the <i>sequential</i> parameter below is true, then breeding is done specially:
    /// instead of breeding all Subpopulations each generation, we only breed one each generation.
    /// The subpopulation index to breed is determined by taking the generation number, modulo the
    /// total number of subpopulations.  Use of this parameter outside of a coevolutionary context
    /// (see ec.coevolve.MultiPopCoevolutionaryEvaluator) is very rare indeed.
    /// 
    /// <p/>SimpleBreeder adheres to the default-subpop parameter in Population: if either an 'elite'
    /// or 'reevaluate-elites' parameter is missing, it will use the default subpopulation's value
    /// and signal a warning.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><tt><i>base</i>.Elite.<i>i</i></tt><br/>
    /// <font size="-1">int >= 0 (default=0)</font></td>
    /// <td valign="top">(the number of elitist individuals for subpop <i>i</i>)</td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.reduce - by.<i>i</i></tt><br/>
    /// <font size="-1">int >= 0 (default=0)</font></td>
    /// <td valign="top">(how many to reduce subpopulation<i>i</i> by each generation)</td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.minimum - size.<i>i</i></tt><br/>
    /// <font size="-1">int >= 2 (default=2)</font></td>
    /// <td valign="top">(the minimum size for subpopulation <i>i</i> regardless of reduction)</td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.reevaluate-elites.<i>i</i></tt><br/>
    /// <font size="-1">boolean (default = false)</font></td>    
    /// <td valign="top">(should we reevaluate the elites of subpopulation <i>i</i> each generation?)</td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.sequential</tt><br/>
    /// <font size="-1">boolean (default = false)</font></td>
    /// <td valign="top">(should we breed just one subpopulation each generation (as opposed to all of them)?)</td></tr>    
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.simple.SimpleBreeder")]
    public class SimpleBreeder : Breeder
    {
        #region Constants

        public const string P_ELITE = "elite";
        public const string P_ELITE_FRAC = "elite-fraction";
        public const string P_REEVALUATE_ELITES = "reevaluate-elites";
        public const string P_SEQUENTIAL_BREEDING = "sequential";
        public const string P_CLONE_PIPELINE_AND_POPULATION = "clone-pipeline-and-population";
        public const string P_REDUCE_BY = "reduce-by";
        public const string P_MINIMUM_SIZE = "minimum-size";

        public const int NOT_SET = -1;

        #endregion // Constants
        #region Properties

        /// <summary>
        /// An array[subpop] of the number of elites to keep for that subpop 
        /// </summary>
        public int[] Elite { get; set; }

        public double[] EliteFrac { get; set; }

        public int[] ReduceBy { get; set; }
        public int[] MinimumSize { get; set; }

        public bool[] ReevaluateElites { get; set; }

        public bool SequentialBreeding { get; set; }

        public bool ClonePipelineAndPopulation { get; set; }

        public Population BackupPopulation { get; set; }
        
        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            var p = new Parameter(Initializer.P_POP).Push(Population.P_SIZE);

            // if size is wrong, we'll let Population complain about it -- for us, we'll just make 0-sized arrays and drop out.
            var size = state.Parameters.GetInt(p, null, 1);

            Elite = new int[size];
            EliteFrac = new double[size];
            for (var i = 0; i < size; i++)
                EliteFrac[i] = Elite[i] = NOT_SET;
            ReevaluateElites = new bool[size];
            ReduceBy = new int[size];  // all zero
            MinimumSize = new int[size];
            for (int i = 0; i < size; i++)
                MinimumSize[i] = 2;

            SequentialBreeding = state.Parameters.GetBoolean(paramBase.Push(P_SEQUENTIAL_BREEDING), null, false);
            if (SequentialBreeding && size == 1) // uh oh, this can't be right
                state.Output.Fatal("The Breeder is breeding sequentially, but you have only one population.", 
                    paramBase.Push(P_SEQUENTIAL_BREEDING));

            ClonePipelineAndPopulation = state.Parameters.GetBoolean(paramBase.Push(P_CLONE_PIPELINE_AND_POPULATION), null, true);
            if (!ClonePipelineAndPopulation && (state.BreedThreads > 1)) // uh oh, this can't be right
                state.Output.Fatal("The Breeder is not cloning its pipeline and population, but you have more than one thread.", 
                    paramBase.Push(P_CLONE_PIPELINE_AND_POPULATION));

            var defaultSubpop = state.Parameters.GetInt(new Parameter(Initializer.P_POP).Push(Population.P_DEFAULT_SUBPOP), null, 0);
            for (var x = 0; x < size; x++)
            {
                ReduceBy[x] = state.Parameters.GetIntWithDefault(paramBase.Push(P_REDUCE_BY).Push("" + x), null, 0);
                if (ReduceBy[x] < 0)
                    state.Output.Fatal("reduce-by must be set to an integer >= 0.", paramBase.Push(P_REDUCE_BY).Push("" + x));

                MinimumSize[x] = state.Parameters.GetIntWithDefault(paramBase.Push(P_MINIMUM_SIZE).Push("" + x), null, 2);
                if (MinimumSize[x] < 2)
                    state.Output.Fatal("minimum-size must be set to an integer >= 2.", paramBase.Push(P_MINIMUM_SIZE).Push("" + x));

                // get elites
                if (state.Parameters.ParameterExists(paramBase.Push(P_ELITE).Push("" + x), null))
                {
                    if (state.Parameters.ParameterExists(paramBase.Push(P_ELITE_FRAC).Push("" + x), null))
                        state.Output.Error("Both elite and elite-frac specified for subpouplation " + x + ".", paramBase.Push(P_ELITE_FRAC).Push("" + x), paramBase.Push(P_ELITE_FRAC).Push("" + x));
                    else
                    {
                        Elite[x] = state.Parameters.GetIntWithDefault(paramBase.Push(P_ELITE).Push("" + x), null, 0);
                        if (Elite[x] < 0)
                            state.Output.Error("Elites for subpopulation " + x + " must be an integer >= 0", paramBase.Push(P_ELITE).Push("" + x));
                    }
                }
                else if (state.Parameters.ParameterExists(paramBase.Push(P_ELITE_FRAC).Push("" + x), null))
                {
                    EliteFrac[x] = state.Parameters.GetDoubleWithMax(paramBase.Push(P_ELITE_FRAC).Push("" + x), null, 0.0, 1.0);
                    if (EliteFrac[x] < 0.0)
                        state.Output.Error("Elite Fraction of subpopulation " + x + " must be a real value between 0.0 and 1.0 inclusive", paramBase.Push(P_ELITE_FRAC).Push("" + x));
                }
                else if (defaultSubpop >= 0)
                {
                    if (state.Parameters.ParameterExists(paramBase.Push(P_ELITE).Push("" + defaultSubpop), null))
                    {
                        Elite[x] = state.Parameters.GetIntWithDefault(paramBase.Push(P_ELITE).Push("" + defaultSubpop), null, 0);
                        if (Elite[x] < 0)
                            state.Output.Warning("Invalid default subpopulation elite value.");  // we'll fail later
                    }
                    else if (state.Parameters.ParameterExists(paramBase.Push(P_ELITE_FRAC).Push("" + defaultSubpop), null))
                    {
                        EliteFrac[x] = state.Parameters.GetDoubleWithMax(paramBase.Push(P_ELITE_FRAC).Push("" + defaultSubpop), null, 0.0, 1.0);
                        if (EliteFrac[x] < 0.0)
                            state.Output.Warning("Invalid default subpopulation elite-frac value.");  // we'll fail later
                    }
                    else  // elitism is 0
                    {
                        Elite[x] = 0;
                    }
                }
                else // elitism is 0
                {
                    Elite[x] = 0;
                }

                // get reevaluation
                if (defaultSubpop >= 0 && !state.Parameters.ParameterExists(paramBase.Push(P_REEVALUATE_ELITES).Push("" + x), null))
                {
                    ReevaluateElites[x] = state.Parameters.GetBoolean(paramBase.Push(P_REEVALUATE_ELITES).Push("" + defaultSubpop), null, false);
                    if (ReevaluateElites[x])
                        state.Output.Warning("Elite reevaluation not specified for subpopulation " + x
                            + ".  Using values for default subpopulation " + defaultSubpop + ": " + ReevaluateElites[x]);
                }
                else
                {
                    ReevaluateElites[x] = state.Parameters.GetBoolean(paramBase.Push(P_REEVALUATE_ELITES).Push("" + x), null, false);
                }
            }

            state.Output.ExitIfErrors();
        }

        #endregion // Setup
        #region Operations

        #region Elites

        public bool UsingElitism(int subpopulation)
        {
            return Elite[subpopulation] > 0 || EliteFrac[subpopulation] > 0;
        }

        public int NumElites(IEvolutionState state, int subpopulation)
        {
            if (Elite[subpopulation] != NOT_SET)
            {
                return Elite[subpopulation];
            }
            if (!EliteFrac[subpopulation].Equals(0)) // Floating point comparison!
            {
                return 0; // no elites
            }
            if (!EliteFrac[subpopulation].Equals(NOT_SET)) // Floating point comparison!
            {
                return (int)Math.Max(Math.Floor(state.Population.Subpops[subpopulation].Individuals.Length * EliteFrac[subpopulation]), 1.0);  // AT LEAST 1 ELITE
            }
            state.Output.WarnOnce("Elitism error (SimpleBreeder).  This shouldn't be able to happen.  Please report.");
            return 0;  // this shouldn't happen
        }

        protected void UnmarkElitesEvaluated(IEvolutionState state, Population newpop)
        {
            for (var sub = 0; sub < newpop.Subpops.Length; sub++)
            {
                if (!ShouldBreedSubpop(state, sub, 0))
                    continue;
                for (var e = 0; e < NumElites(state, sub); e++)
                {
                    var len = newpop.Subpops[sub].Individuals.Length;
                    if (ReevaluateElites[sub])
                        newpop.Subpops[sub].Individuals[len - e - 1].Evaluated = false;
                }
            }
        }

        /// <summary>
        /// A private helper function for breedPopulation which loads Elites into a subpop. 
        /// </summary>		
        protected virtual void LoadElites(IEvolutionState state, Population newpop)
        {
            // are our elites small enough?
            for (int x = 0; x < state.Population.Subpops.Length; x++)
            {
                if (NumElites(state, x) > state.Population.Subpops[x].Individuals.Length)
                    state.Output.Error("The number of elites for subpopulation " + x + " exceeds the actual size of the subpopulation",
                        new Parameter(EvolutionState.P_BREEDER).Push(P_ELITE).Push("" + x));
                if (NumElites(state, x) == state.Population.Subpops[x].Individuals.Length)
                    state.Output.Warning("The number of elites for subpopulation " + x + " is the actual size of the subpopulation",
                        new Parameter(EvolutionState.P_BREEDER).Push(P_ELITE).Push("" + x));
            }

            state.Output.ExitIfErrors();

            // we assume that we're only grabbing a small number (say <10%), so
            // it's not being done multithreaded
            for (var sub = 0; sub < state.Population.Subpops.Length; sub++)
            {
                if (!ShouldBreedSubpop(state, sub, 0))  // don't load the elites for this one, we're not doing breeding of it
                {
                    continue;
                }

                // if the number of Elites is 1, then we handle this by just finding the best one.
                if (NumElites(state, sub) == 1)
                {
                    var best = 0;
                    var oldinds = state.Population.Subpops[sub].Individuals;

                    for (var x = 1; x < oldinds.Length; x++)
                        if (oldinds[x].Fitness.BetterThan(oldinds[best].Fitness))
                            best = x;

                    var inds = newpop.Subpops[sub].Individuals;
                    inds[inds.Length - 1] = (Individual)oldinds[best].Clone();
                }
                else if (NumElites(state, sub) > 0) // we'll need to sort
                {
                    var orderedPop = new int[state.Population.Subpops[sub].Individuals.Length];
                    for (var x = 0; x < state.Population.Subpops[sub].Individuals.Length; x++)
                        orderedPop[x] = x;

                    // sort the best so far where "<" means "not as fit as"
                    QuickSort.QSort(orderedPop, new EliteComparator(state.Population.Subpops[sub].Individuals));
                    // load the top N individuals

                    var inds = newpop.Subpops[sub].Individuals;
                    var oldinds = state.Population.Subpops[sub].Individuals;
                    for (var x = inds.Length - NumElites(state, sub); x < inds.Length; x++)
                        inds[x] = (Individual)oldinds[orderedPop[x]].Clone();
                }
                // optionally force reevaluation
                UnmarkElitesEvaluated(state, newpop);
            }
        }

        #endregion // Elites

        /// <summary>
        /// Elites are often stored in the top part of the subpop; this function returns what
        /// part of the subpop contains individuals to replace with newly-bred ones
        /// (up to but not including the Elites). 
        /// </summary>
        public virtual int ComputeSubpopulationLength(IEvolutionState state, Population newpop, int subpop, int threadnum)
        {
            if (!ShouldBreedSubpop(state, subpop, threadnum))
                return newpop.Subpops[subpop].Individuals.Length;  // we're not breeding the population, just copy over the whole thing
            return newpop.Subpops[subpop].Individuals.Length - NumElites(state, subpop);	// we're breeding population, so elitism may have happened 
        }

        /// <summary>
        /// A simple breeder that doesn't attempt to do any cross-
        /// population breeding.  Basically it applies pipelines,
        /// one per thread, to various subchunks of a new population. 
        /// </summary>
        public override Population BreedPopulation(IEvolutionState state)
        {
            Population newpop;
            if (ClonePipelineAndPopulation)
                newpop = (Population)state.Population.EmptyClone();
            else
            {
                if (BackupPopulation == null)
                    BackupPopulation = (Population)state.Population.EmptyClone();
                newpop = BackupPopulation;
                newpop.Clear();
                BackupPopulation = state.Population;  // swap in
            }

            // maybe resize?
            for (int i = 0; i < state.Population.Subpops.Length; i++)
            {
                if (ReduceBy[i] > 0)
                {
                    int prospectiveSize = Math.Max(
                        Math.Max(state.Population.Subpops[i].Individuals.Length - ReduceBy[i], MinimumSize[i]),
                        NumElites(state, i));
                    if (prospectiveSize < state.Population.Subpops[i].Individuals.Length)  // let's resize!
                    {
                        state.Output.Message("Subpop " + i + " reduced " + state.Population.Subpops[i].Individuals.Length + " -> " + prospectiveSize);
                        newpop.Subpops[i].Resize(prospectiveSize);
                    }
                }
            }

            // load Elites into top of newpop
            LoadElites(state, newpop);

            // how many threads do we really need?  No more than the maximum number of individuals in any subpopulation
            int numThreads = 0;
            for (int x = 0; x < state.Population.Subpops.Length; x++)
                numThreads = Math.Max(numThreads, state.Population.Subpops[x].Individuals.Length);
            numThreads = Math.Min(numThreads, state.BreedThreads);
            if (numThreads < state.BreedThreads)
                state.Output.WarnOnce("Largest subpopulation size (" + numThreads + ") is smaller than number of breedthreads (" + state.BreedThreads +
                                      "), so fewer breedthreads will be created.");

            int[][] numinds = TensorFactory.Create<int>(state.BreedThreads, state.Population.Subpops.Length);
            //var numinds = new int[state.BreedThreads][];
            //for (var i = 0; i < state.BreedThreads; i++)
            //{
            //    numinds[i] = new int[state.Population.Subpops.Length];
            //}
            int[][] from = TensorFactory.Create<int>(state.BreedThreads, state.Population.Subpops.Length);
            //var from = new int[state.BreedThreads][];
            //for (var i2 = 0; i2 < state.BreedThreads; i2++)
            //{
            //    from[i2] = new int[state.Population.Subpops.Length];
            //}

            for (int x = 0; x < state.Population.Subpops.Length; x++)
            {
                int length = ComputeSubpopulationLength(state, newpop, x, 0);

                // we will have some extra individuals.  We distribute these among the early subpopulations
                int individualsPerThread = length / numThreads;  // integer division
                int slop = length - numThreads * individualsPerThread;
                int currentFrom = 0;

                for (int y = 0; y < numThreads; y++)
                {
                    if (slop > 0)
                    {
                        numinds[y][x] = individualsPerThread + 1;
                        slop--;
                    }
                    else
                        numinds[y][x] = individualsPerThread;

                    if (numinds[y][x] == 0)
                    {
                        state.Output.WarnOnce("More threads exist than can be used to breed some subpopulations (first example: subpopulation " + x + ")");
                    }

                    from[y][x] = currentFrom;
                    currentFrom += numinds[y][x];
                }
            }


            if (numThreads == 1)
            {
                BreedPopChunk(newpop, state, numinds[0], from[0], 0);
            }
            else
            {
                ParallelBreeding(state, newpop, from, numinds, this);
            }

            return newpop;
        }

        void ParallelBreeding(IEvolutionState state, Population newpop, int[][] origin, int[][] numinds, SimpleBreeder breeder)
        {
            // BRS: TPL DataFlow is cleaner and safer than using raw threads.

            // Limit the concurrency in case the user has gone overboard!
            var maxDegree = Math.Min(Environment.ProcessorCount, state.BreedThreads);
            var options = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegree };

            Action<BreederThread> act = t => t.Run();
            var actionBlock = new ActionBlock<BreederThread>(act, options);

            for (var i = 0; i < state.BreedThreads; i++)
            {
                var runnable = new BreederThread
                {
                    ThreadNum = i,
                    State = state,
                    NewPop = newpop,
                    NumInds = numinds[i],
                    From = origin[i],
                    Breeder = breeder,
                };
                actionBlock.Post(runnable);
            }
            actionBlock.Complete();
            actionBlock.Completion.Wait();
        }

        /// <summary>
        /// Returns true if we're doing sequential breeding and it's the subpopulation's turn (round robin, one subpopulation per generation).
        /// </summary>
        public bool ShouldBreedSubpop(IEvolutionState state, int subpop, int threadnum)
        {
            return !SequentialBreeding 
                || state.Generation % state.Population.Subpops.Length == subpop;
        }

        /// <summary>
        /// A private helper function for breedPopulation which breeds a chunk
        /// of individuals in a subpop for a given thread.
        /// Although this method is declared
        /// public (for the benefit of a private helper class in this file),
        /// you should not call it. 
        /// </summary>
        public override void BreedPopChunk(Population newpop, IEvolutionState state, int[] numinds, int[] from, int threadnum)
        {
            for (var subpop = 0; subpop < newpop.Subpops.Length; subpop++)
            {
                // if it's subpop's turn and we're doing sequential breeding...
                if (!ShouldBreedSubpop(state, subpop, threadnum))
                {
                    // instead of breeding, we should just copy forward this subpopulation.  We'll copy the part we're assigned
                    for (var ind = from[subpop]; ind < numinds[subpop] - from[subpop]; ind++)
                        // newpop.subpops[subpop].individuals[ind] = (Individual)(state.population.subpops[subpop].individuals[ind].clone());
                        // this could get dangerous
                        newpop.Subpops[subpop].Individuals[ind] = state.Population.Subpops[subpop].Individuals[ind];
                }
                else
                {
                    // do regular breeding of this subpopulation
                    BreedingPipeline bp = null;
                    if (ClonePipelineAndPopulation)
                        bp = (BreedingPipeline)newpop.Subpops[subpop].Species.Pipe_Prototype.Clone();
                    else
                        bp = newpop.Subpops[subpop].Species.Pipe_Prototype;


                    // check to make sure that the breeding pipeline produces
                    // the right kind of individuals.  Don't want a mistake there! :-)
                    if (!bp.Produces(state, newpop, subpop, threadnum))
                        state.Output.Fatal("The Breeding Pipeline of subpopulation " + subpop +
                                           " does not produce individuals of the expected species " +
                                           newpop.Subpops[subpop].Species.GetType().Name + " or fitness " +
                                           newpop.Subpops[subpop].Species.F_Prototype);
                    bp.PrepareToProduce(state, subpop, threadnum);

                    // start breedin'!

                    var x = from[subpop];
                    var upperbound = from[subpop] + numinds[subpop];
                    while (x < upperbound)
                        x += bp.Produce(1, upperbound - x, x, subpop,
                                        newpop.Subpops[subpop].Individuals,
                                        state, threadnum);
                    if (x > upperbound) // uh oh!  Someone blew it!
                        state.Output.Fatal(
                            "Whoa!  A breeding pipeline overwrote the space of another pipeline in subpopulation " +
                            subpop + ".  You need to check your breeding pipeline code (in produce() ).");

                    bp.FinishProducing(state, subpop, threadnum);
                }
            }
        }

        #endregion // Operations

        private class EliteComparator : ISortComparatorL
        {
            internal Individual[] Inds;
            public EliteComparator(Individual[] inds)
            {
                Inds = inds;
            }
            public virtual bool lt(long a, long b)
            {
                return Inds[(int)b].Fitness.BetterThan(Inds[(int)a].Fitness);
            }
            public virtual bool gt(long a, long b)
            {
                return Inds[(int)a].Fitness.BetterThan(Inds[(int)b].Fitness);
            }
        }

    }
}