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
    /**
     * Performs a tournament selection restricted to only the best, or worst, <i>n</i>
     * indivdiuals in the population.  If the best individuals, then tournament selection
     * will prefer the better among them; if the worst individuals, then tournament selection
     * will prefer the worse among them.  The procedure for performing restriction is expensive to
     * set up and bring down, so it's not appropriate for steady-state evolution.  Like
     * TournamentSelection, the size of the tournament can be any 
     * If you're not familiar with the relative advantages of 
     * selection methods and just want a good one,
     * use TournamentSelection instead.   Not appropriate for
     * multiobjective fitnesses.
     *
     * <p>The tournament <i>size</i> can be any floating point value >= 1.0.  If it is a non-
     * integer value <i>x</i> then either a tournament of size ceil(x) is used
     * (with probability x - floor(x)), else a tournament of size floor(x) is used.
     *
     <p><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br>
     Always 1.

     <p><b>Parameters</b><br>
     <table>
     <tr><td valign=top><i>base.</i><tt>pick-worst</tt><br>
     <font size=-1> bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
     <td valign=top>(should we pick from among the <i>worst n</i> individuals in the tournament instead of the <i>best n</i>?)</td></tr>
     <tr><td valign=top><i>base.</i><tt>size</tt><br>
     <font size=-1>double &gt;= 1</font></td>
     <td valign=top>(the tournament size)</td></tr>
     <tr><td valign=top><i>base.</i><tt>n</tt><br>
     <font size=-1> int > 0 </font></td>
     <td valign=top>(the number of best-individuals to select from)</td></tr>
     <tr><td valign=top><i>base.</i><tt>n-fraction</tt><br>
     <font size=-1> 0.0 <= double < 1.0 (default is 1)</font></td>
     <td valign=top>(the number of best-individuals to select from, as a fraction of the total population)</td></tr>
     </table>

     <p><b>Default Base</b><br>
     select.best

     * @author Sean Luke
     * @version 1.0 
     */

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
        public const String P_N_FRACTION = "n-fraction";
        public const string P_PICKWORST = "pick-worst";
        public const String P_SIZE = "size";

        public const int NOT_SET = -1;

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase => SelectDefaults.ParamBase.Push(P_BEST);
        

        /** Base size of the tournament; this may change.  */
        public int Size { get; set; }

        /** Probablity of picking the size plus one. */
        public double ProbabilityOfPickingSizePlusOne { get; set; }

        /// <summary>
        /// Do we pick the worst instead of the best? 
        /// </summary>
        public bool PickWorst { get; set; }

        public int BestN { get; set; } = NOT_SET;

        public double BestNFrac = NOT_SET;

        /// <summary>
        /// Sorted, normalized, totalized fitnesses for the population. 
        /// </summary>
        public double[] SortedFit { get; set; }

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

            if (state.Parameters.ParameterExists(paramBase.Push(P_N), def.Push(P_N)))
            {
                BestN = state.Parameters.GetInt(paramBase.Push(P_N), def.Push(P_N), 1);
                if (BestN == 0)
                    state.Output.Fatal("n must be an integer greater than 0", paramBase.Push(P_N), def.Push(P_N));
            }
            else if (state.Parameters.ParameterExists(paramBase.Push(P_N_FRACTION), def.Push(P_N_FRACTION)))
            {
                if (state.Parameters.ParameterExists(paramBase.Push(P_N), def.Push(P_N)))
                    state.Output.Fatal("Both n and n-fraction specified for BestSelection.", paramBase.Push(P_N), def.Push(P_N));
                BestNFrac =
                    state.Parameters.GetDoubleWithMax(paramBase.Push(P_N_FRACTION), def.Push(P_N_FRACTION), 0.0, 1.0);
                if (BestNFrac <= 0.0)
                    state.Output.Fatal("n-fraction must be a double floating-point value greater than 0.0 and <= 1.0", paramBase.Push(P_N_FRACTION), def.Push(P_N_FRACTION));
            }
            else state.Output.Fatal("Either n or n-fraction must be defined for BestSelection.", paramBase.Push(P_N), def.Push(P_N));


            PickWorst = state.Parameters.GetBoolean(paramBase.Push(P_PICKWORST), def.Push(P_PICKWORST), false);

            double val = state.Parameters.GetDouble(paramBase.Push(P_SIZE), def.Push(P_SIZE), 1.0);
            if (val < 1.0)
                state.Output.Fatal("Tournament size must be >= 1.", paramBase.Push(P_SIZE), def.Push(P_SIZE));
            else if (val.Equals((int)val))  // easy, it's just an integer
            {
                Size = (int)val;
                ProbabilityOfPickingSizePlusOne = 0.0;
            }
            else
            {
                Size = (int)Math.Floor(val);
                ProbabilityOfPickingSizePlusOne = val - Size;  // for example, if we have 5.4, then the probability of picking *6* is 0.4
            }
        }

        #endregion // Setup
        #region Operations

        public override void PrepareToProduce(IEvolutionState s, int subpop, int thread)
        {
            // load SortedPop integers
            var i = s.Population.Subpops[subpop].Individuals;

            SortedPop = new int[i.Count];
            for (var x = 0; x < SortedPop.Length; x++)
                SortedPop[x] = x;

            // sort SortedPop in increasing fitness order
            // BRS: Using extension methods in Util.CollectionExtensions
            i.SortByFitnessAscending();

            if (!PickWorst)  // gotta reverse it
                for (int x = 0; x < SortedPop.Length / 2; x++)
                {
                    int p = SortedPop[x];
                    SortedPop[x] = SortedPop[SortedPop.Length - x - 1];
                    SortedPop[SortedPop.Length - x - 1] = p;
                }

            // figure out bestn
            if (!BestNFrac.Equals(NOT_SET))
            {
                BestN = (int)Math.Max(Math.Floor(s.Population.Subpops[subpop].Individuals.Count * BestNFrac), 1);
            }
        }

        /** Returns a tournament size to use, at random, based on base size and probability of picking the size plus one. */
        int GetTournamentSizeToUse(IMersenneTwister random)
        {
            double p = ProbabilityOfPickingSizePlusOne;   // pulls us to under 35 bytes
            if (p.Equals(0.0)) return Size;
            return Size + (random.NextBoolean(p) ? 1 : 0);
        }

        public override int Produce(int subpop, IEvolutionState state, int thread)
        {
            // pick size random individuals, then pick the best.
            IList<Individual> oldinds = state.Population.Subpops[subpop].Individuals;
            int best = state.Random[thread].NextInt(BestN);  // only among the first N

            int s = GetTournamentSizeToUse(state.Random[thread]);

            if (PickWorst)
                for (int x = 1; x < s; x++)
                {
                    int j = state.Random[thread].NextInt(BestN);  // only among the first N
                    if (!(oldinds[SortedPop[j]].Fitness.BetterThan(oldinds[SortedPop[best]].Fitness)))  // j isn't better than best
                        best = j;
                }
            else
                for (int x = 1; x < s; x++)
                {
                    int j = state.Random[thread].NextInt(BestN);  // only among the first N
                    if (oldinds[SortedPop[j]].Fitness.BetterThan(oldinds[SortedPop[best]].Fitness))  // j is better than best
                        best = j;
                }

            return SortedPop[best];
        }

        public override void FinishProducing(IEvolutionState s, int subpop, int thread)
        {
            // release the distributions so we can quickly 
            // garbage-collect them if necessary
            SortedPop = null;
        }

        #endregion // Operations
        #region SortComparator

        // BRS: Using extension methods in Util.CollectionExtensions instead
        //private class AnonymousClassSortComparatorL : ISortComparatorL
        //{
        //    public AnonymousClassSortComparatorL(Individual[] i)
        //    {
        //        InitBlock(i);
        //    }
        //    private void InitBlock(Individual[] i)
        //    {
        //        _inds = i;
        //    }
        //    private Individual[] _inds;

        //    public virtual bool lt(long a, long b)
        //    {
        //        return _inds[(int)b].Fitness.BetterThan(_inds[(int)a].Fitness);
        //    }

        //    public virtual bool gt(long a, long b)
        //    {
        //        return _inds[(int)a].Fitness.BetterThan(_inds[(int)b].Fitness);
        //    }
        //}
        
        #endregion // SortComparator
    }
}