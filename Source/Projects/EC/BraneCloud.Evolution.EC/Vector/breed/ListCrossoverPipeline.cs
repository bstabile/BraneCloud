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
using System.Linq;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Support;

namespace BraneCloud.Evolution.EC.Vector.Breed
{
    /// <summary>
    /// ListCrossoverPipeline is a crossover pipeline for vector individuals whose length
    /// may be lengthened or shortened.  There are two crossover options available: one-point
    /// and two-point.  One-point crossover picks a crossover point for each of the vectors
    /// (the crossover point can be different), and then does one-point crossover using those
    /// points.  Two-point crossover picks TWO crossover points for each of the vectors (again,
    /// the points can be different among the vectors), and swaps the middle regions between
    /// the respective crossover points.
    /// 
    /// <p/>ListCrossoverPipeline will try tries times to meet certain constraints: first,
    /// the resulting children must be no smaller than min-child-size.  Second, the amount
    /// of material removed from a parent must be no less than mix-crossover-percent and no 
    /// more than max-crossover-percent.
    ///   
    /// <p/>If toss is true, then only one child is generated, else at most two are generated.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// 2 * minimum typical number of individuals produced by each source, unless toss
    /// is set, in which case it's simply the minimum typical number.
    /// 
    /// <p/><b>Number of Sources</b><br/>
    /// 2
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"/><i>base</i>.<tt>toss</tt><br/>
    /// <font size="-1">bool = <tt>true</tt> or <tt>false</tt> (default)</font>/td>
    /// <td valign="top">(after crossing over with the first new individual, 
    /// should its second sibling individual be thrown away instead of adding it to 
    /// the population?)</td></tr>
    /// 
    /// <tr><td valign="top"><i>base</i>.<tt>tries</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(number of times to try finding valid crossover points)</td></tr>
    /// 
    /// <tr><td valign="top"><i>base</i>.<tt>min-child-size</tt><br/>
    /// <font size="-1">int &gt;= 0 (default)</font></td>
    /// <td valign="top">(the minimum allowed size of a child)</td></tr>
    /// 
    /// <tr><td valign="top"><i>base</i>.<tt>min-crossover-percent</tt><br/>
    /// <font size="-1">0 (default) &lt;= float &lt;= 1</font></td>
    /// <td valign="top">(the minimum percentage of an individual that may be removed during crossover)</td></tr>
    /// 
    /// <tr><td valign="top"><i>base</i>.<tt>max-crossover-percent</tt><br/>
    /// <font size="-1">0 &lt;= float &lt;= 1 (default)</font></td>
    /// <td valign="top">(the maximum percentage of an individual that may be removed during crossover)</td></tr>
    /// 
    /// </table>
    /// 
    /// <p/><b>Default Base</b><br/>
    /// vector.list-xover
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.vector.breed.ListCrossoverPipeline")]
    public class ListCrossoverPipeline : BreedingPipeline
    {
        #region Constants

        public const string P_TOSS = "toss";
        public const string P_LIST_CROSSOVER = "list-xover";
        public const string P_MIN_CHILD_SIZE = "min-child-size";
        public const string P_NUM_TRIES = "tries";
        public const string P_MIN_CROSSOVER_PERCENT = "min-crossover-percent";
        public const string P_MAX_CROSSOVER_PERCENT = "max-crossover-percent";
        public const int NUM_SOURCES = 2;
        public const string KEY_PARENTS = "parents";

        #endregion // Constants

        #region Fields

        protected IList<Individual> Parents { get; set; } = new List<Individual>();

        #endregion // Fields

        #region Properties

        public override IParameter DefaultBase => VectorDefaults.ParamBase.Push(P_LIST_CROSSOVER);

        public override int NumSources => NUM_SOURCES;

        public override int TypicalIndsProduced => TossSecondParent ? MinChildProduction : MinChildProduction * 2;

        public bool TossSecondParent { get; set; }
        public int CrossoverType { get; set; }
        public int MinChildSize { get; set; }
        public int NumTries { get; set; }
        public float MinCrossoverPercentage { get; set; }
        public float MaxCrossoverPercentage { get; set; }

        #endregion // Properties

        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            var def = DefaultBase;

            TossSecondParent = state.Parameters.GetBoolean(paramBase.Push(P_TOSS),
                def.Push(P_TOSS), false);

            MinChildSize = state.Parameters.GetIntWithDefault(paramBase.Push(P_MIN_CHILD_SIZE),
                def.Push(P_MIN_CHILD_SIZE), 0);

            NumTries = state.Parameters.GetIntWithDefault(paramBase.Push(P_NUM_TRIES),
                def.Push(P_NUM_TRIES), 1);

            MinCrossoverPercentage = state.Parameters.GetFloatWithDefault(paramBase.Push(P_MIN_CROSSOVER_PERCENT),
                def.Push(P_MIN_CROSSOVER_PERCENT), 0.0);
            MaxCrossoverPercentage = state.Parameters.GetFloatWithDefault(paramBase.Push(P_MAX_CROSSOVER_PERCENT),
                def.Push(P_MAX_CROSSOVER_PERCENT), 1.0);


            var crossoverTypeString = state.Parameters.GetStringWithDefault(
                paramBase.Push(VectorSpecies.P_CROSSOVERTYPE),
                def.Push(VectorSpecies.P_CROSSOVERTYPE),
                VectorSpecies.V_TWO_POINT);

            // determine the crossover method to use (only 1-point & 2-point currently supported)
            if (crossoverTypeString.ToUpper().Equals(VectorSpecies.V_ONE_POINT.ToUpper()))
            {
                CrossoverType = VectorSpecies.C_ONE_POINT;
            }
            else if (crossoverTypeString.ToUpper().Equals(VectorSpecies.V_TWO_POINT.ToUpper()))
            {
                CrossoverType = VectorSpecies.C_TWO_POINT;
            }
            else
            {
                state.Output.Error("ListCrossoverPipeline:\n:" +
                                   "   Parameter crossover-type is currently set to: " + crossoverTypeString + "\n" +
                                   "   Currently supported crossover types are \"one\" and \"two\" point.\n");
            }

            // sanity check for crossover parameters
            if (MinChildSize < 0)
            {
                state.Output.Error("ListCrossoverPipeline:\n" +
                                   "   Parameter min-child-size is currently equal to: " + MinChildSize + "\n" +
                                   "   min-child-size must be a positive integer\n");
            }

            if (NumTries < 1)
            {
                state.Output.Error("ListCrossoverPipeline:\n" +
                                   "   Parameter tries is currently equal to: " + NumTries + "\n" +
                                   "   tries must be greater than or equal to 1\n");
            }


            if (MinCrossoverPercentage < 0.0 || MinCrossoverPercentage > 1.0)
            {
                state.Output.Error("ListCrossoverPipeline:\n" +
                                   "   Parameter min-crossover-percent is currently equal to: " +
                                   MinCrossoverPercentage + "\n" +
                                   "   min-crossover-percent must be either a real-value float between [0.0, 1.0] or left unspecified\n");
            }
            if (MaxCrossoverPercentage < 0.0 || MaxCrossoverPercentage > 1.0)
            {
                state.Output.Error("ListCrossoverPipeline:\n" +
                                   "   Parameter max-crossover-percent is currently equal to: " +
                                   MaxCrossoverPercentage + "\n" +
                                   "   max-crossover-percent must be either a real-value float between [0.0, 1.0] or left unspecified\n");
            }
            if (MinCrossoverPercentage > MaxCrossoverPercentage)
            {
                state.Output.Error("ListCrossoverPipeline:\n" +
                                   "   Parameter min-crossover-percent must be less than max-crossover-percent\n");
            }
            if (MinCrossoverPercentage == MaxCrossoverPercentage)
            {
                state.Output.Warning("ListCrossoverPipeline:\n" +
                                     "   Parameter min-crossover-percent and max-crossover-percent are currently equal to: " +
                                     MinCrossoverPercentage + "\n" +
                                     "   This effectively prevents any crossover from occurring\n");
            }
        }

        #endregion // Setup

        #region Operations

        public override int Produce(
            int min,
            int max,
            int subpop,
            IList<Individual> inds,
            IEvolutionState state,
            int thread,
            IDictionary<string, object> misc)
        {
            int start = inds.Count;

            // how many individuals should we make?
            var n = TypicalIndsProduced;
            if (n < min) n = min;
            if (n > max) n = max;

            // should we bother?
            if (!state.Random[thread].NextBoolean(Likelihood))
            {
                // just load from source 0 and clone 'em
                Sources[0].Produce(n, n, subpop, inds, state, thread, misc);
                return n;
            }

            IntBag[] parentparents = null;
            IntBag[] preserveParents = null;
            if (misc != null && misc.ContainsKey(KEY_PARENTS))
            {
                preserveParents = (IntBag[]) misc[KEY_PARENTS];
                parentparents = new IntBag[2];
                misc[KEY_PARENTS] = parentparents;
            }

            for (var q = start; q < n + start; /* no increment */) // keep on going until we're filled up
            {
                Parents.Clear();

                // grab two individuals from our sources
                if (Sources[0] == Sources[1]) // grab from the same source
                {
                    Sources[0].Produce(2, 2, subpop, Parents, state, thread, misc);
                }
                else // grab from different sources
                {
                    Sources[0].Produce(1, 1, subpop, Parents, state, thread, misc);
                    Sources[1].Produce(1, 1, subpop, Parents, state, thread, misc);
                }


                // determines size of parents, in terms of chunks
                var chunkSize = ((VectorSpecies) Parents[0].Species).ChunkSize;
                var size = new int[2];
                size[0] = ((VectorIndividual) Parents[0]).GenomeLength;
                size[1] = ((VectorIndividual) Parents[1]).GenomeLength;
                var sizeInChunks = new int[2];
                sizeInChunks[0] = size[0] / chunkSize;
                sizeInChunks[1] = size[1] / chunkSize;

                // variables used to split & join the children
                var minChunks = new int[2];
                var maxChunks = new int[2];

                // BRS : TODO : Change to rectangular arrays?
                var split = new int[2][];
                for (var x = 0; x < 2; x++) split[x] = new int[2];
                var pieces = new Object[2][];
                for (var x = 0; x < 2; x++) pieces[x] = new object[2];

                // determine min and max crossover segment lengths, in terms of chunks
                for (var i = 0; i < 2; i++)
                {
                    minChunks[i] = (int) (sizeInChunks[i] * MinCrossoverPercentage);
                    // round minCrossoverPercentage up to nearest chunk boundary
                    if (size[i] % chunkSize != 0 && minChunks[i] < sizeInChunks[i])
                    {
                        minChunks[i]++;
                    }
                    maxChunks[i] = (int) (sizeInChunks[i] * MaxCrossoverPercentage);
                }

                // attempt 'num-tries' times to produce valid children (which are bigger than min-child-size)
                var validChildren = false;
                var attempts = 0;
                while (validChildren == false && attempts < NumTries)
                {
                    // generate split indices for one-point (tail end used as end of segment)
                    if (CrossoverType == VectorSpecies.C_ONE_POINT)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            // select first index at most 'max_chunks' away from tail end of vector
                            split[i][0] = sizeInChunks[i] - maxChunks[i];
                            // shift back towards tail end with random value based on min/max parameters
                            split[i][0] += state.Random[thread].NextInt(maxChunks[i] - minChunks[i]);
                            // convert split from chunk numbers to array indices
                            split[i][0] *= chunkSize;
                            // select tail end chunk boundary as second split index
                            split[i][1] = sizeInChunks[i] * chunkSize;
                        }
                    }

                    // generate split indices for two-point (both indicies have randomized positions)
                    else if (CrossoverType == VectorSpecies.C_TWO_POINT)
                    {
                        for (var i = 0; i < 2; i++)
                        {
                            // select first split index randomly
                            split[i][0] = state.Random[thread].NextInt(sizeInChunks[i] - minChunks[i]);
                            // second index must be at least 'min_chunks' after the first index
                            split[i][1] = split[i][0] + minChunks[i];
                            // add a random value up to max crossover size, without exceeding size of the parent
                            split[i][1] += state.Random[thread].NextInt(Math.Min(maxChunks[i] - minChunks[i],
                                sizeInChunks[i] - split[i][0]));
                            // convert split from chunk numbers to array indices
                            split[i][0] *= chunkSize;
                            split[i][1] *= chunkSize;
                        }
                    }

                    // use the split indices generated above to split the parents into pieces
                    ((VectorIndividual) Parents[0]).Split(split[0], pieces[0]);
                    ((VectorIndividual) Parents[1]).Split(split[1], pieces[1]);

                    // create copies of the parents, swap the middle segment, and then rejoin the pieces
                    // - this is done to test whether or not the resulting children are of a valid size,
                    // - because we are using Object references to an undetermined array type, there is no way 
                    //   to cast it to the appropriate array type (i.e. short[] or double[]) to figure out the
                    //   length of the pieces
                    // - instead, we use the join method on copies, and let each vector type figure out its own
                    //   length with the genomeLength() method
                    var children = new VectorIndividual[2];
                    children[0] = (VectorIndividual) Parents[0].Clone();
                    children[1] = (VectorIndividual) Parents[1].Clone();

                    var swap = pieces[0][1];
                    pieces[0][1] = pieces[1][1];
                    pieces[1][1] = swap;

                    children[0].Join(pieces[0]);
                    children[1].Join(pieces[1]);
                    if (children[0].GenomeLength > MinChildSize && children[1].GenomeLength > MinChildSize)
                    {
                        validChildren = true;
                    }
                    attempts++;
                }

                // if the children produced were valid, updates the parents
                if (validChildren)
                {
                    ((VectorIndividual) Parents[0]).Join(pieces[0]);
                    ((VectorIndividual) Parents[1]).Join(pieces[1]);
                    Parents[0].Evaluated = false;
                    Parents[1].Evaluated = false;
                }

                // add parents to the population
                // by Ermo. is this wrong?
                // -- Okay Sean
                inds.Add(Parents[0]);
                if (preserveParents != null)
                {
                    parentparents[0].AddAll(parentparents[1]);
                    preserveParents[q] = parentparents[0];
                }
                q++;
                if (q < n + start && TossSecondParent == false)
                {
                    // by Ermo. also this is wrong?
                    inds.Add(Parents[1]);
                    if (preserveParents != null)
                    {
                        parentparents[0].AddAll(parentparents[1]);
                        preserveParents[q] = parentparents[0];
                    }
                    q++;
                }
            }
            return n;
        }

        /** A hook called by ListCrossoverPipeline to allow subclasses to prepare for additional validation testing. 
            Primarily used by GECrossoverPipeline.  */
        public virtual object ComputeValidationData(IEvolutionState state, IList<Individual> parents, int thread)
        {
            return null;
        }

        #endregion // Operations

        #region Cloning

        public override object Clone()
        {
            var c = (ListCrossoverPipeline) base.Clone();
            c.Parents = Parents.ToList();
            return c;
        }

        #endregion // Cloning
    }
}