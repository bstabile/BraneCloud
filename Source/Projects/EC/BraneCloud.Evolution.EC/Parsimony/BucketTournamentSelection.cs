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

namespace BraneCloud.Evolution.EC.Parsimony
{
    /// <summary> 
    /// Does a tournament selection, limited to the subpop it's working in at the time.
    /// 
    /// <p/>Bucket Lexicographic Tournament selection works like as follows. There is a 
    /// number of buckets (<i>num-buckets</i>) specified beforehand, and each is
    /// assigned a rank from 1 to <i>num-buckets</i>.  The population, of size <i>pop-size</i>,
    /// is sorted by fitness.  The bottom <i>pop-size</i>/<i>num-buckets</i> individuals are
    /// placed in the worst ranked bucket, plus any individuals remaining in the population with
    /// the same fitness as the best individual in the bucket.  Then the second worst
    /// <i>pop-size</i>/<i>num-buckets</i> individuals are placed in the second worst ranked bucket,
    /// plus any individuals in the population equal in fitness to the best individual in that bucket.
    /// This continues until there are no individuals in the population.  Note that the topmost bucket
    /// with individuals can hold fewer than <i>pop-size</i>/<i>num-buckets</i> individuals, if
    /// <i>pop-size</i> is not a multiple of <i>num-buckets</i>. Depending on the number of
    /// equal-fitness individuals in the population, there can be some top buckets that are never
    /// filled. The fitness of each individual in a bucket is set to the rank of the bucket holding
    /// it.  Direct bucketing has the effect of trading off fitness differences for size. Thus the
    /// larger the bucket, the stronger the emphasis on size as a secondary objective.
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
    /// <tr><td valign="top"><i>base.</i><tt>num-buckets</tt><br/>
    /// <font size="-1">int &gt;= 1 (default 10)</font></td>
    /// <td valign="top">(the number of buckets)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// select.bucket-tournament
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.parsimony.BucketTournamentSelection")]
    public class BucketTournamentSelection : SelectionMethod, ISteadyStateBSource
    {
        private class AnonymousClassComparator : IComparer
        {
            public virtual int Compare(object o1, object o2)
            {
                var a = (Individual) o1;
                var b = (Individual) o2;
                if (a.Fitness.BetterThan(b.Fitness))
                    return 1;
                if (b.Fitness.BetterThan(a.Fitness))
                    return - 1;
                return 0;
            }
        }

        #region Constants

        /// <summary>
        /// Default base 
        /// </summary>
        public const string P_TOURNAMENT = "bucket-tournament";

        /// <summary>
        /// If the worst individual should be picked in the tournament 
        /// </summary>
        public const string P_PICKWORST = "pick-worst";

        /// <summary>
        /// Tournament size parameter 
        /// </summary>
        public const string P_SIZE = "size";

        /// <summary>
        /// The number of buckets 
        /// </summary>
        public const string P_BUCKETS = "num-buckets";

        /// <summary>
        /// Default number of buckets 
        /// </summary>
        public const int N_BUCKETS_DEFAULT = 10;

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return SelectDefaults.ParamBase.Push(P_TOURNAMENT); }
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
        /// The number of buckets
        /// </summary>
        public int NumBuckets { get; set; }

        /// <summary>
        /// The indexes of the buckets where the individuals should go (will be used instead of fitness)
        /// </summary>
        public int[] BucketValues { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;

            Size = state.Parameters.GetInt(paramBase.Push(P_SIZE), def.Push(P_SIZE), 1);
            if (Size < 1)
                state.Output.Fatal("Tournament size must be >= 1.", paramBase.Push(P_SIZE), def.Push(P_SIZE));

            if (state.Parameters.ParameterExists(paramBase.Push(P_BUCKETS), def.Push(P_BUCKETS)))
            {
                NumBuckets = state.Parameters.GetInt(paramBase.Push(P_BUCKETS), def.Push(P_BUCKETS), 1);
                if (NumBuckets < 1)
                {
                    state.Output.Fatal("The number of buckets size must be >= 1.", paramBase.Push(P_BUCKETS), def.Push(P_BUCKETS));
                }
            }
            else
            {
                NumBuckets = N_BUCKETS_DEFAULT;
            }

            PickWorst = state.Parameters.GetBoolean(paramBase.Push(P_PICKWORST), def.Push(P_PICKWORST), false);
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Prepare to produce: create the buckets!!!! 
        /// </summary>
        public override void  PrepareToProduce(IEvolutionState state, int subpop, int thread)
        {
            BucketValues = new int[state.Population.Subpops[subpop].Individuals.Length];
            
            // correct?
            Array.Sort(state.Population.Subpops[subpop].Individuals, new AnonymousClassComparator());
                        
            // how many individuals in current bucket

            var averageBuck = state.Population.Subpops[subpop].Individuals.Length / ((float) NumBuckets);
            
            // first individual goes into first bucket
            BucketValues[0] = 0;
            
            // now there is one individual in the first bucket
            var nInd = 1;
            
            for (var i = 1; i < state.Population.Subpops[subpop].Individuals.Length; i++)
            {
                // if there is still some place left in the current bucket, throw the current individual there too
                if (nInd < averageBuck)
                {
                    BucketValues[i] = BucketValues[i - 1];
                    nInd++;
                }
                // check if it has the same fitness as last individual
                else
                {
                    if (state.Population.Subpops[subpop].Individuals[i].Fitness
                        .EquivalentTo(state.Population.Subpops[subpop].Individuals[i - 1].Fitness))
                    {
                        // now the individual has exactly the same fitness as previous one,
                        // so we just put it in the same bucket as the previous one(s)
                        BucketValues[i] = BucketValues[i - 1];
                        nInd++;
                    }
                    else
                    {
                        // if there are buckets left
                        if (BucketValues[i - 1] + 1 < NumBuckets)
                        {
                            // new bucket!!!!
                            BucketValues[i] = BucketValues[i - 1] - 1;
                            // with only one individual
                            nInd = 1;
                        }
                        // no more buckets left, just stick everything in the last bucket
                        else
                        {
                            BucketValues[i] = BucketValues[i - 1];
                            nInd++;
                        }
                    }
                }
            }
        }
        
        public override int Produce(int subpop, IEvolutionState state, int thread)
        {
            // pick size random individuals, then pick the best.
            var oldinds = state.Population.Subpops[subpop].Individuals;
            var i = state.Random[thread].NextInt(oldinds.Length);
            long si = 0;
            
            for (var x = 1; x < Size; x++)
            {
                var j = state.Random[thread].NextInt(oldinds.Length);
                if (PickWorst)
                {
                    if (BucketValues[j] > BucketValues[i])
                    {
                        i = j; 
                        si = 0;
                    }
                    else if (BucketValues[i] > BucketValues[j])
                    { /* do nothing */ }	
                    else
                    {
                        if (si == 0)
                            si = oldinds[i].Size;
                        var sj = oldinds[j].Size;
                        
                        if (sj >= si) // sj's got worse lookin' trees
                        {
                            i = j; 
                            si = sj;
                        }
                    }
                }
                else
                {
                    if (BucketValues[j] < BucketValues[i])
                    {
                        i = j; 
                        si = 0;
                    }
                    else if (BucketValues[i] < BucketValues[j])
                    { /* do nothing */ }
                    else
                    {
                        if (si == 0)
                            si = oldinds[i].Size;
                        var sj = oldinds[j].Size;
                        
                        if (sj < si) // sj's got better lookin' trees
                        {
                            i = j; 
                            si = sj;
                        }
                    }
                }
            }
            return i;
        }
        
        public virtual void  IndividualReplaced(SteadyStateEvolutionState state, int subpop, int thread, int individual)
        {
            return ;
        }
        
        public virtual void SourcesAreProperForm(SteadyStateEvolutionState state)
        {
            return ;
        }

        #endregion // Operations
    }
}