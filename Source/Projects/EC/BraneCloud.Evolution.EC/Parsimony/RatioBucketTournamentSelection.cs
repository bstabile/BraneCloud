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

using BraneCloud.Evolution.EC.Select;
using BraneCloud.Evolution.EC.SteadyState;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.Parsimony
{
    /// <summary> 
    /// Does a tournament selection, limited to the subpop it's working in at the time.
    /// 
    /// <p/>Ratio Bucket Lexicographic Tournament selection works like as follows. The sizes of buckets are
    /// proportioned so that low-fitness individuals are placed into much larger buckets than high-fitness
    /// individuals.  A bucket ratio <i>1/ratio</i> is specified beforehand.  The bottom <i>1/ratio</i> individuals
    /// of the population are placed into the bottom bucket. If any individuals remain in the population
    /// with the same fitness as the best individual in the bottom bucket, they too are placed in that bucket.
    /// Of the remaining population, the next <i>1/ratio</i> individuals are placed into the next bucket, plus any
    /// individuals remaining in the population with the same fitness as the best individual now in that bucket,
    /// and so on.  This continues until every member of the population has been placed in a bucket. Once again,
    /// the fitness of every individual in a bucket is set to the rank of the bucket relative to other buckets.
    /// Ratio bucketing thus allows parsimony to have more of an effect on average when two similar low-fitness
    /// individuals are considered than when two high-fitness individuals are considered.
    /// 
    /// After ranking the individuals, <i>size</i> individuals are chosen at random from the
    /// population. Of those individuals, the one with the highest rank is selected. If the two
    /// individuals are in the same rank, meaning that they have similar fitness, the one
    /// with the smallest size is selected.
    /// 
    /// <p/>Bucket Lexicographic Tournament selection is so simple that it doesn't
    /// need to maintain a cache of any form, so many of the SelectionMethod methods
    /// just don't do anything at all.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// Always 1.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>size</tt><br/>
    /// <font size="-1">int &gt;= 1 (default 7)</font></td>
    /// <td valign="top">(the tournament size)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>pick-worst</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">(should we pick the <i>worst</i> individual in the tournament instead of the <i>best</i>?)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>ratio</tt><br/>
    /// <font size="-1">float &gt;= 2 (default)</font></td>
    /// <td valign="top">(the ratio of worst out of remaining individuals that go in the next bucket)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// select.ratio-bucket-tournament
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.parsimony.RatioBucketTournamentSelection")]
    public class RatioBucketTournamentSelection : SelectionMethod, ISteadyStateBSource
    {
        #region Constants

        /// <summary>
        /// Default base
        /// </summary>
        public const string P_RATIO_BUCKET_TOURNAMENT = "ratio-bucket-tournament";

        /// <summary>
        /// Size parameter
        /// </summary>
        public const string P_SIZE = "size";

        /// <summary>
        /// Default size
        /// </summary>
        public const int DEFAULT_SIZE = 7;

        /// <summary>
        /// If the worst individual should be picked in the tournament
        /// </summary>
        public const string P_PICKWORST = "pick-worst";

        /// <summary>
        /// The value of RATIO: each step, the worse 1/RATIO individuals are assigned the same fitness.
        /// </summary>
        public const string P_RATIO = "ratio";

        /// <summary>
        /// The default value for RATIO
        /// </summary>
        const float DefaultRatio = 2;

        #endregion // Constants
        #region Fields

        /// <summary>
        /// The indexes of the buckets where the individuals should go (will be used instead of fitness)
        /// </summary>
        int[] _bucketValues;

        #endregion // Fields
        #region Properties

        public override IParameter DefaultBase
        {
            get { return SelectDefaults.ParamBase.Push(P_RATIO_BUCKET_TOURNAMENT); }
        }

        /// <summary>
        /// Size of the tournament
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Do we pick the worst instead of the best?
        /// </summary>
        public bool PickWorst { get; set; }

        /// <summary>
        /// The value of RATIO
        /// </summary>
        public float Ratio { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;

            Size = state.Parameters.GetInt(paramBase.Push(P_SIZE), def.Push(P_SIZE), 1);
            if (Size < 1)
                state.Output.Fatal("Tournament size must be >= 1.", paramBase.Push(P_SIZE), def.Push(P_SIZE));

            if (state.Parameters.ParameterExists(paramBase.Push(P_RATIO), def.Push(P_RATIO)))
            {
                Ratio = state.Parameters.GetFloat(paramBase.Push(P_RATIO), def.Push(P_RATIO), 2.0f);
                if (Ratio < 2)
                {
                    state.Output.Fatal("The value of b must be >= 2.", paramBase.Push(P_RATIO), def.Push(P_RATIO));
                }
            }
            else
            {
                Ratio = DefaultRatio;
            }

            PickWorst = state.Parameters.GetBoolean(paramBase.Push(P_PICKWORST), def.Push(P_PICKWORST), false);
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Prepare to produce: create the buckets!!!!
        /// </summary>
        public override void PrepareToProduce(IEvolutionState state, int subpop, int thread)
        {
            _bucketValues = new int[state.Population.Subpops[subpop].Individuals.Length];

            // Default sort on fitness?
            Array.Sort(state.Population.Subpops[subpop].Individuals);

            // how many individuals in current bucket

            float totalInds = state.Population.Subpops[subpop].Individuals.Length;
            var averageBuck = Math.Max(totalInds / Ratio, 1);

            // first individual goes into first bucket
            _bucketValues[0] = 0;

            // now there is one individual in the first bucket
            var nInd = 1;
            totalInds--;

            for (var i = 1; i < state.Population.Subpops[subpop].Individuals.Length; i++)
            {
                // if there is still some place left in the current bucket, throw the current individual there too
                if (nInd < averageBuck)
                {
                    _bucketValues[i] = _bucketValues[i - 1];
                    nInd++;
                }
                else // check if it has the same fitness as last individual
                {
                    if ((state.Population.Subpops[subpop].Individuals[i]).Fitness
                        .EquivalentTo(state.Population.Subpops[subpop].Individuals[i - 1].Fitness))
                    {
                        // now the individual has exactly the same fitness as previous one,
                        // so we just put it in the same bucket as the previous one(s)
                        _bucketValues[i] = _bucketValues[i - 1];
                        nInd++;
                    }
                    else
                    {
                        // new bucket!!!!
                        averageBuck = Math.Max(totalInds / Ratio, 1);
                        _bucketValues[i] = _bucketValues[i - 1] - 1; // decrease the fitness, so that high fit individuals have lower bucket values
                        // with only one individual
                        nInd = 1;
                    }
                }
                totalInds--;
            }
        }

        public override int Produce(int subpop, IEvolutionState state, int thread)
        {
            // pick size random individuals, then pick the best.
            var oldinds = (state.Population.Subpops[subpop].Individuals);
            var i = state.Random[thread].NextInt(oldinds.Length);
            long si = 0;

            for (var x = 1; x < Size; x++)
            {
                var j = state.Random[thread].NextInt(oldinds.Length);
                if (PickWorst)
                {
                    if (_bucketValues[j] > _bucketValues[i]) { i = j; si = 0; }
                    else if (_bucketValues[i] > _bucketValues[j]) { } // do nothing
                    else
                    {
                        if (si == 0)
                            si = oldinds[i].Size;
                        var sj = oldinds[j].Size;

                        if (sj >= si) // sj's got worse lookin' trees
                        { i = j; si = sj; }
                    }
                }
                else
                {
                    if (_bucketValues[j] < _bucketValues[i]) { i = j; si = 0; }
                    else if (_bucketValues[i] < _bucketValues[j]) { } // do nothing
                    else
                    {
                        if (si == 0)
                            si = oldinds[i].Size;
                        var sj = oldinds[j].Size;

                        if (sj < si) // sj's got better lookin' trees
                        { i = j; si = sj; }
                    }
                }
            }
            return i;
        }

        /// <summary>
        /// Included for SteadyState
        /// </summary>
        public void IndividualReplaced(SteadyStateEvolutionState state, int subpop, int thread, int individual)
        { return; }

        public void SourcesAreProperForm(SteadyStateEvolutionState state)
        { return; }

        #endregion // Operations
    }
}