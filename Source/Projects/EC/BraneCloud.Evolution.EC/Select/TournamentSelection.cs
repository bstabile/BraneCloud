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

using BraneCloud.Evolution.EC.SteadyState;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.Select
{	
    /// <summary> 
    /// Does a simple tournament selection, limited to the subpop it's working in at the time.
    /// 
    /// <p/>Tournament selection works like this: first, <i>size</i> individuals
    /// are chosen at random from the population.  Then of those individuals,
    /// the one with the best fitness is selected.  
    /// 
    /// <p/><i>size</i> can also be a floating-point value between 1.0 and 2.0,
    /// exclusive of them. In this situation, two individuals are chosen at random, and
    /// the better one is selected with a probability of <i>size/2</i> 
    /// 
    /// <p/>Common sizes for <i>size</i> include: 2, popular in Genetic Algorithms
    /// circles, and 7, popularized in Genetic Programming by John Koza.
    /// If the size is 1, then individuals are picked entirely at random.
    /// 
    /// <p/>Tournament selection is so simple that it doesn't need to maintain
    /// a cache of any form, so many of the SelectionMethod methods just
    /// don't do anything at all.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// Always 1.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>size</tt><br/>
    /// <font size="-1">int &gt;= 1 <b>or</b> 1.0 &lt; float &lt; 2.0</font></td>
    /// <td valign="top">(the tournament size)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>pick-worst</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">(should we pick the <i>worst</i> individual in the tournament instead of the <i>best</i>?)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// select.tournament
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.select.TournamentSelection")]
    public class TournamentSelection : SelectionMethod, ISteadyStateBSource
    {
        #region Constants

        /// <summary>
        /// Default base. 
        /// </summary>
        public const string P_TOURNAMENT = "tournament";
        
        public const string P_PICKWORST = "pick-worst";

        /// <summary>
        /// Size parameter. 
        /// </summary>
        public const string P_SIZE = "size";

        /// <summary>
        /// Default size.
        /// </summary>
        public const int DEFAULT_SIZE = 7;

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return SelectDefaults.ParamBase.Push(P_TOURNAMENT); }
        }

        /// <summary>
        /// Size of the tournament.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Probablity of picking the size plus one. 
        /// </summary>
        public double ProbabilityOfPickingSizePlusOne { get; set; }

        /// <summary>
        /// Do we pick the worst instead of the best? 
        /// </summary>
        public bool PickWorst { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;

            var val = state.Parameters.GetDouble(paramBase.Push(P_SIZE), def.Push(P_SIZE), 1.0);
            if (val < 1.0)
                state.Output.Fatal("Tournament size must be >= 1.", paramBase.Push(P_SIZE), def.Push(P_SIZE));
            else if (val == (int)val)
            // pick with probability
            {
                Size = (int)val;
                ProbabilityOfPickingSizePlusOne = 0.0;
            }
            else
            {
                Size = (int)Math.Floor(val);
                ProbabilityOfPickingSizePlusOne = val - Size;  // for example, if we have 5.4, then the probability of picking *6* is 0.4
            }

            PickWorst = state.Parameters.GetBoolean(paramBase.Push(P_PICKWORST), def.Push(P_PICKWORST), false);
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Returns a tournament size to use, at random, based on base size and probability of picking the size plus one.
        /// </summary>
        public int GetTournamentSizeToUse(IMersenneTwister random)
        {
            var p = ProbabilityOfPickingSizePlusOne;   // pulls us to under 35 bytes
            if (p == 0.0) return Size;
            return Size + (random.NextBoolean(p) ? 1 : 0);
        }


        /// <summary>
        /// Produces the index of a (typically uniformly distributed) randomly chosen individual
        /// to fill the tournament.  <i>number</i> is the position of the individual in the tournament.
        /// </summary>
        public int GetRandomIndividual(int number, int subpop, IEvolutionState state, int thread)
        {
            var oldinds = state.Population.Subpops[subpop].Individuals;
            return state.Random[thread].NextInt(oldinds.Length);
        }

        /// <summary>
        /// Returns true if *first* is a better (fitter, whatever) individual than *second*.
        /// </summary>
        public virtual bool BetterThan(Individual first, Individual second, int subpopulation, IEvolutionState state, int thread)
        {
            return first.Fitness.BetterThan(second.Fitness);
        }


        /// <summary>
        /// I hard-code both Produce(...) methods for efficiency's sake
        /// </summary>
        public override int Produce(int subpop, IEvolutionState state, int thread)
        {
            // pick size random individuals, then pick the best.
            var oldinds = state.Population.Subpops[subpop].Individuals;
            var best = GetRandomIndividual(0, subpop, state, thread);

            var s = GetTournamentSizeToUse(state.Random[thread]);

            if (PickWorst)
                for (var x = 1; x < s; x++)
                {
                    int j = GetRandomIndividual(x, subpop, state, thread);
                    if (!BetterThan(oldinds[j], oldinds[best], subpop, state, thread))  // j is at least as bad as best
                        best = j;
                }
            else
                for (var x = 1; x < s; x++)
                {
                    var j = GetRandomIndividual(x, subpop, state, thread);
                    if (BetterThan(oldinds[j], oldinds[best], subpop, state, thread))  // j is better than best
                        best = j;
                }

            return best;
        }


        /// <summary>
        /// I hard-code both Produce(...) methods for efficiency's sake
        /// </summary>
        public virtual void IndividualReplaced(SteadyStateEvolutionState state, int subpop, int thread, int individual)
        {
            return;
        }

        public virtual void SourcesAreProperForm(SteadyStateEvolutionState state)
        {
            return;
        }

        #endregion // Operations
    }
}