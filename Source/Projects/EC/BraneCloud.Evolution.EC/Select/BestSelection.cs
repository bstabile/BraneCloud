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

using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.Select
{
    /// <summary> 
    /// Picks among the best <i>n</i> individuals in a population in 
    /// direct proportion to their absolute
    /// fitnesses as returned by their fitness() methods relative to the
    /// fitnesses of the other "best" individuals in that <i>n</i>.  This is expensive to
    /// set up and bring down, so it's not appropriate for steady-state evolution.
    /// If you're not familiar with the relative advantages of 
    /// selection methods and just want a good one,
    /// use TournamentSelection instead.   Not appropriate for
    /// multiobjective fitnesses.
    /// 
    /// <p/><b><font color="red">
    /// Note: Fitnesses must be non-negative.  0 is assumed to be the worst fitness.
    /// </font></b>
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// Always 1.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>pick-worst</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">(should we pick from among the <i>worst n</i> individuals in the tournament instead of the <i>best n</i>?)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>n</tt><br/>
    /// <font size="-1"> int > 0 (default is 1)</font></td>
    /// <td valign="top">(the number of best-individuals to select from)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// select.best
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.select.BestSelection")]
    public class BestSelection : SelectionMethod
    {
        #region Constants

        /// <summary>
        /// Default base 
        /// </summary>
        public const string P_BEST = "best";
        public const string P_N = "n";
        public const string P_PICKWORST = "pick-worst";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return SelectDefaults.ParamBase.Push(P_BEST); }
        }

        /// <summary>
        /// Do we pick the worst instead of the best? 
        /// </summary>
        public bool PickWorst { get; set; }

        public int BestN { get; set; }

        /// <summary>
        /// Sorted, normalized, totalized fitnesses for the population. 
        /// </summary>
        public float[] SortedFit { get; set; }

        /// <summary>
        /// Sorted population -- since I *have* to use an int-sized
        /// individual (short gives me only 16K), 
        /// I might as well just have pointers to the
        /// population itself.  :-( 
        /// </summary>
        public int[] SortedPop { get; set; }

        #endregion // Properties
        #region Setup

        // don't need clone etc. 

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;

            BestN = state.Parameters.GetInt(paramBase.Push(P_N), def.Push(P_N), 1);
            if (BestN == 0)
                state.Output.Fatal("n must be an integer greater than 0", paramBase.Push(P_N), def.Push(P_N));

            PickWorst = state.Parameters.GetBoolean(paramBase.Push(P_PICKWORST), def.Push(P_PICKWORST), false);
        }

        #endregion // Setup
        #region Operations

        public override void PrepareToProduce(IEvolutionState s, int subpop, int thread)
        {
            // load SortedPop integers
            var i = s.Population.Subpops[subpop].Individuals;

            SortedPop = new int[i.Length];
            for (var x = 0; x < SortedPop.Length; x++)
                SortedPop[x] = x;

            // sort SortedPop in increasing fitness order
            QuickSort.QSort(SortedPop, new AnonymousClassSortComparatorL(i));

            // load SortedFit
            SortedFit = new float[Math.Min(SortedPop.Length, BestN)];
            if (PickWorst)
                for (var x = 0; x < SortedFit.Length; x++)
                    SortedFit[x] = i[SortedPop[x]].Fitness.Value;
            else
                for (var x = 0; x < SortedFit.Length; x++)
                    SortedFit[x] = i[SortedPop[SortedPop.Length - x - 1]].Fitness.Value;

            foreach (var t in SortedFit)
            {
                if (t < 0)
                    // uh oh
                    s.Output.Fatal("Discovered a negative fitness value."
                                   + "  BestSelection requires that all fitness values be non-negative(offending subpop #" + subpop + ")");
            }

            // organize the distributions.  All zeros in fitness is fine
            RandomChoice.OrganizeDistribution(SortedFit, true);
        }

        public override int Produce(int subpop, IEvolutionState state, int thread)
        {
            // Pick and return an individual from the population
            if (PickWorst)
                return SortedPop[RandomChoice.PickFromDistribution(SortedFit, state.Random[thread].NextFloat())];

            return SortedPop[SortedPop.Length - RandomChoice.PickFromDistribution(SortedFit, state.Random[thread].NextFloat()) - 1];
        }

        public override void FinishProducing(IEvolutionState s, int subpop, int thread)
        {
            // release the distributions so we can quickly 
            // garbage-collect them if necessary
            SortedFit = null;
            SortedPop = null;
        }

        #endregion // Operations
        #region SortComparator

        private class AnonymousClassSortComparatorL : ISortComparatorL
        {
            public AnonymousClassSortComparatorL(Individual[] i)
            {
                InitBlock(i);
            }
            private void InitBlock(Individual[] i)
            {
                _inds = i;
            }
            private Individual[] _inds;

            public virtual bool lt(long a, long b)
            {
                return _inds[(int)b].Fitness.BetterThan(_inds[(int)a].Fitness);
            }

            public virtual bool gt(long a, long b)
            {
                return _inds[(int)a].Fitness.BetterThan(_inds[(int)b].Fitness);
            }
        }
        
        #endregion // SortComparator
    }
}