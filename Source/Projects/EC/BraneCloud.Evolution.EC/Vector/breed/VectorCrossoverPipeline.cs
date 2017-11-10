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
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Vector;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Support;

namespace BraneCloud.Evolution.EC.Vector.Breed
{			
    /// <summary> 
    /// VectorCrossoverPipeline is a BreedingPipeline which implements a simple default crossover
    /// for VectorIndividuals.  Normally it takes two individuals and returns two crossed-over 
    /// child individuals.  Optionally, it can take two individuals, cross them over, but throw
    /// away the second child (a one-child crossover).  VectorCrossoverPipeline works by calling
    /// defaultCrossover(...) on the first parent individual.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// 2 * minimum typical number of individuals produced by each source, unless TossSecondParent
    /// is set, in which case it's simply the minimum typical number.
    /// <p/><b>Number of Sources</b><br/>
    /// 2
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"/><i>base</i>.<tt>toss</tt><br/>
    /// <font size="-1">bool = <tt>true</tt> or <tt>false</tt> (default)</font>/td>
    /// <td valign="top">(after crossing over with the first new individual, 
    /// should its second sibling individual be thrown away instead 
    /// of adding it to the population?)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// vector.xover
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.vector.breed.VectorCrossoverPipeline")]
    public class VectorCrossoverPipeline : BreedingPipeline
    {
        #region Constants

        public const string P_TOSS = "toss";
        public const string P_CROSSOVER = "xover";
        public const int NUM_SOURCES = 2;
        public const string KEY_PARENTS = "parents";

        #endregion // Constants
        #region Fields

        /// <summary>
        /// Temporary holding place for Parents. 
        /// </summary>
        protected IList<Individual> Parents { get; set; } = new List<Individual>();

        #endregion // Fields
        #region Properties

        public override IParameter DefaultBase => VectorDefaults.ParamBase.Push(P_CROSSOVER);

        /// <summary>
        /// Returns 2 * minimum number of typical individuals produced by any Sources, else
        /// 1* minimum number if TossSecondParent is true. 
        /// </summary>
        public override int TypicalIndsProduced => TossSecondParent ? MinChildProduction : MinChildProduction * 2;

        /// <summary>
        /// Returns 2. 
        /// </summary>
        public override int NumSources => NUM_SOURCES;

        /// <summary>
        /// Should the pipeline discard the second parent after crossing over? 
        /// </summary>
        public bool TossSecondParent { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            var def = DefaultBase;
            TossSecondParent = state.Parameters.GetBoolean(paramBase.Push(P_TOSS), def.Push(P_TOSS), false);
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

            IntBag[] parentparents = null;
            IntBag[] preserveParents = null;

            if (misc != null && misc.ContainsKey(KEY_PARENTS))
            {
                preserveParents = (IntBag[])misc[KEY_PARENTS];
                parentparents = new IntBag[2];
                misc[KEY_PARENTS] = parentparents;
            }

            // should we bother?
            // should we use them straight?
            if (!state.Random[thread].NextBoolean(Likelihood))
            {
                // just load from source 0 and clone 'em
                Sources[0].Produce(n, n, subpop, inds, state, thread, misc);
                return n;
            }

            for (var q = start; q < n + start; )
            // keep on going until we're filled up
            {
                Parents.Clear();

                // grab two individuals from our Sources
                if (Sources[0] == Sources[1])
                // grab from the same source
                {
                    Sources[0].Produce(2, 2, subpop, Parents, state, thread, misc);
                }
                // grab from different Sources
                else
                {
                    Sources[0].Produce(1, 1, subpop, Parents, state, thread, misc);
                    Sources[1].Produce(1, 1, subpop, Parents, state, thread, misc);
                }

                // at this point, Parents[] contains our two selected individuals,
                // AND they're copied so we own them and can make whatever modifications
                // we like on them.

                // so we'll cross them over now.  Since this is the default pipeline,
                // we'll just do it by calling defaultCrossover on the first child

                ((VectorIndividual)Parents[0]).DefaultCrossover(state, thread, (VectorIndividual)Parents[1]);
                Parents[0].Evaluated = false;
                Parents[1].Evaluated = false;

                // add 'em to the population
                // by Ermo. this should use add instead of set, because the inds is empty, so will throw index out of bounds
                // okay -- Sean
                inds.Add(Parents[0]);
                if (preserveParents != null)
                {
                    parentparents[0].AddAll(parentparents[1]);
                    preserveParents[q] = parentparents[0];
                }
                q++;
                if (q < n + start && !TossSecondParent)
                {
                    // by Ermo. as as here, see the comments above
                    inds.Add(Parents[1]);
                    if (preserveParents != null)
                    {
                        preserveParents[q] = new IntBag(parentparents[0]);
                    }
                    q++;
                }
            }
            if (preserveParents != null)
            {
                misc[KEY_PARENTS] = preserveParents;
            }
            return n;
        }

        #endregion // Operations
        #region Cloning

        public override object Clone()
        {
            var c = (VectorCrossoverPipeline)base.Clone();

            // deep-cloned stuff
            c.Parents = Parents.ToList();

            return c;
        }

        #endregion // Cloning
    }
}