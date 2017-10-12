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
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Vector;
using BraneCloud.Evolution.EC.Configuration;

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

        #endregion // Constants
        #region Fields

        /// <summary>
        /// Temporary holding place for Parents. 
        /// </summary>
        internal VectorIndividual[] Parents = new VectorIndividual[2];

        #endregion // Fields
        #region Properties

        public override IParameter DefaultBase
        {
            get { return VectorDefaults.ParamBase.Push(P_CROSSOVER); }
        }

        /// <summary>
        /// Returns 2 * minimum number of typical individuals produced by any Sources, else
        /// 1* minimum number if TossSecondParent is true. 
        /// </summary>
        public override int TypicalIndsProduced
        {
            get { return (TossSecondParent ? MinChildProduction : MinChildProduction * 2); }
        }

        /// <summary>
        /// Returns 2. 
        /// </summary>
        public override int NumSources
        {
            get { return NUM_SOURCES; }
        }

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

        public override int Produce(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            // how many individuals should we make?
            var n = TypicalIndsProduced;
            if (n < min) n = min;
            if (n > max) n = max;

            // should we bother?
            if (!state.Random[thread].NextBoolean(Likelihood))
                return Reproduce(n, start, subpop, inds, state, thread, true);  // DO produce children from source -- we've not done so already

            for (var q = start; q < n + start; )
            // keep on going until we're filled up
            {
                // grab two individuals from our Sources
                if (Sources[0] == Sources[1])
                // grab from the same source
                {
                    Sources[0].Produce(2, 2, 0, subpop, Parents, state, thread);
                    if (!(Sources[0] is BreedingPipeline))
                    // it's a selection method probably
                    {
                        Parents[0] = (VectorIndividual)(Parents[0].Clone());
                        Parents[1] = (VectorIndividual)(Parents[1].Clone());
                    }
                }
                // grab from different Sources
                else
                {
                    Sources[0].Produce(1, 1, 0, subpop, Parents, state, thread);
                    Sources[1].Produce(1, 1, 1, subpop, Parents, state, thread);
                    if (!(Sources[0] is BreedingPipeline)) // it's a selection method probably
                        Parents[0] = (VectorIndividual)(Parents[0].Clone());
                    if (!(Sources[1] is BreedingPipeline)) // it's a selection method probably
                        Parents[1] = (VectorIndividual)(Parents[1].Clone());
                }

                // at this point, Parents[] contains our two selected individuals,
                // AND they're copied so we own them and can make whatever modifications
                // we like on them.

                // so we'll cross them over now.  Since this is the default pipeline,
                // we'll just do it by calling defaultCrossover on the first child

                Parents[0].DefaultCrossover(state, thread, Parents[1]);
                Parents[0].Evaluated = false;
                Parents[1].Evaluated = false;

                // add 'em to the population
                inds[q] = Parents[0];
                q++;
                if (q < n + start && !TossSecondParent)
                {
                    inds[q] = Parents[1];
                    q++;
                }
            }
            return n;
        }

        #endregion // Operations
        #region Cloning

        public override object Clone()
        {
            var c = (VectorCrossoverPipeline)base.Clone();

            // deep-cloned stuff
            c.Parents = (VectorIndividual[])Parents.Clone();

            return c;
        }

        #endregion // Cloning
    }
}