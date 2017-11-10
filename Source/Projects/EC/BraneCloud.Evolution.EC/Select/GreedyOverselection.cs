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
using System.Collections.Generic;
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.Select
{
    /// <summary> 
    /// GreedyOverselection is a SelectionMethod which implements Koza-style
    /// fitness-proportionate greedy overselection.  Not appropriate for
    /// multiobjective fitnesses.
    /// 
    /// <p/> This selection method first 
    /// divides individuals in a population into two groups: the "good" 
    /// ("top") group, and the "bad" ("bottom") group.  The best <i>top</i>
    /// percent of individuals in the population go into the good group.
    /// The rest go into the "bad" group.  With a certain probability (determined
    /// by the <i>gets</i> setting), an individual will be picked out of the
    /// "good" group.  Once we have determined which group the individual
    /// will be selected from, the individual is picked using fitness proportionate
    /// selection in that group, that is, the likelihood he is picked is 
    /// proportionate to his fitness relative to the fitnesses of others in his
    /// group.
    /// 
    /// <p/> All this is expensive to
    /// set up and bring down, so it's not appropriate for steady-state evolution.
    /// If you're not familiar with the relative advantages of 
    /// selection methods and just want a good one,
    /// use TournamentSelection instead. 
    /// 
    /// <p/><b><font color="red">
    /// Note: Fitnesses must be non-negative.  0 is assumed to be the worst fitness.
    /// </font></b>
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// Always 1.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>top</tt><br/>
    /// <font size="-1">0.0 &lt;= double &lt;= 1.0</font></td>
    /// <td valign="top">(the percentage of the population going into the "good" (top) group)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>gets</tt><br/>
    /// <font size="-1">0.0 &lt;= double &lt;= 1.0</font></td>
    /// <td valign="top">(the likelihood that an individual will be picked from the "good" group)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// select.greedy
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.select.GreedyOverselection")]
    public class GreedyOverselection : SelectionMethod
    {
        #region Constants

        public const string P_GREEDY = "greedy";
        public const string P_TOP = "top";
        public const string P_GETS = "gets";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase => SelectDefaults.ParamBase.Push(P_GREEDY);

        public double[] SortedFitOver { get; set; }
        public double[] SortedFitUnder { get; set; }

        /// <summary>
        /// Sorted population -- since I *have* to use an int-sized individual (short gives me only 16K), 
        /// I might as well just have pointers to the population itself.  :-( 
        /// </summary>
        public int[] SortedPop { get; set; }

        public double Top_N_Percent { get; set; }
        public double Gets_N_Percent { get; set; }

        #endregion // Properties
        #region Setup

        public override void  Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            
            var def = DefaultBase;
            
            Top_N_Percent = state.Parameters.GetDoubleWithMax(paramBase.Push(P_TOP), def.Push(P_TOP), 0.0, 1.0);
            if (Top_N_Percent < 0.0)
                state.Output.Fatal("Top-N-Percent must be between 0.0 and 1.0", paramBase.Push(P_TOP), def.Push(P_TOP));
            
            Gets_N_Percent = state.Parameters.GetDoubleWithMax(paramBase.Push(P_GETS), def.Push(P_GETS), 0.0, 1.0);
            if (Gets_N_Percent < 0.0)
                state.Output.Fatal("Gets-n-percent must be between 0.0 and 1.0", paramBase.Push(P_GETS), def.Push(P_GETS));
        }

        #endregion // Setup

        // don't need clone etc. -- I'll never clone with my arrays intact

        #region Operations

        public override void PrepareToProduce(IEvolutionState state, int subpop, int thread)
        {
            base.PrepareToProduce(state, subpop, thread);

            // load SortedPop integers
            var i = state.Population.Subpops[subpop].Individuals;

            SortedPop = new int[i.Count];
            for (var x = 0; x < SortedPop.Length; x++)
                SortedPop[x] = x;

            // sort SortedPop in increasing fitness order
            QuickSort.QSort(SortedPop, new AnonymousClassSortComparatorL(i));

            // determine my boundary -- must be at least 1 and must leave 1 over
            var boundary = (int)(SortedPop.Length * Top_N_Percent);
            if (boundary == 0)
                boundary = 1;
            if (boundary == SortedPop.Length)
                boundary = SortedPop.Length - 1;
            if (boundary == 0)
                // uh oh
                state.Output.Fatal("Greedy Overselection can only be done with a population of size 2 or more (offending subpop #" + subpop + ")");

            // load SortedFitOver
            SortedFitOver = new double[boundary];
            var y = 0;
            for (var x = SortedPop.Length - boundary; x < SortedPop.Length; x++)
            {
                SortedFitOver[y] = i[SortedPop[x]].Fitness.Value;
                if (SortedFitOver[y] < 0)
                    // uh oh
                    state.Output.Fatal("Discovered a negative fitness value."
                        + "  Greedy Overselection requires that all fitness values be non-negative (offending subpop #" + subpop + ")");
                y++;
            }

            // load SortedFitUnder
            SortedFitUnder = new double[SortedPop.Length - boundary];
            y = 0;
            for (var x = 0; x < SortedPop.Length - boundary; x++)
            {
                SortedFitUnder[y] = i[SortedPop[x]].Fitness.Value;
                if (SortedFitUnder[y] < 0)
                    // uh oh
                    state.Output.Fatal("Discovered a negative fitness value."
                        + "  Greedy Overselection requires that all fitness values be non-negative (offending subpop #" + subpop + ")");
                y++;
            }

            // organize the distributions.  All zeros in fitness is fine
            RandomChoice.OrganizeDistribution(SortedFitUnder, true);
            RandomChoice.OrganizeDistribution(SortedFitOver, true);
        }

        public override int Produce(int subpop, IEvolutionState state, int thread)
        {
            // pick a coin toss
            if (state.Random[thread].NextBoolean(Gets_N_Percent))
                // over -- SortedFitUnder.length to SortedPop.length
                return SortedPop[SortedFitUnder.Length
                    + RandomChoice.PickFromDistribution(SortedFitOver, state.Random[thread].NextDouble())];

            // under -- 0 to SortedFitUnder.length
            return SortedPop[RandomChoice.PickFromDistribution(SortedFitUnder, state.Random[thread].NextDouble())];
        }

        public override void FinishProducing(IEvolutionState s, int subpop, int thread)
        {
            base.FinishProducing(s, subpop, thread);

            // release the distributions so we can quickly 
            // garbage-collect them if necessary
            SortedFitUnder = null;
            SortedFitOver = null;
            SortedPop = null;
        }

        #endregion // Operations
        #region SortComparator

        private class AnonymousClassSortComparatorL : ISortComparatorL
        {
            public AnonymousClassSortComparatorL(IList<Individual> i)
            {
                _inds = i;
            }

            private readonly IList<Individual> _inds;

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