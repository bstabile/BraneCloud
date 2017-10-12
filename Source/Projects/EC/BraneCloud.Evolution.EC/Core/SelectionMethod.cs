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
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC
{
    /// <summary> 
    /// A SelectionMethod is a BreedingSource which provides direct IMMUTABLE pointers
    /// to original individuals in an old population, not fresh mutable copies.
    /// If you use a SelectionMethod as your BreedingSource, you must 
    /// SelectionMethods might include Tournament Selection, Fitness Proportional Selection, etc.
    /// SelectionMethods don't have parent sources.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// Always 1.
    /// </summary>    
    [Serializable]
    [ECConfiguration("ec.SelectionMethod")]
    public abstract class SelectionMethod : BreedingSource
    {
        #region Constants

        public const int INDS_PRODUCED = 1;

        #endregion // Constants
        #region Properties

        /// <summary>
        /// Returns 1 (the typical default value). 
        /// </summary>
        public override int TypicalIndsProduced
        {
            get { return INDS_PRODUCED; }
        }

        #endregion // Properties
        #region Operations

        /// <summary>
        /// A default version of produces -- this method always returns
        /// true under the assumption that the selection method works
        /// with all Fitnesses.  If this isn't the case, you should override
        /// this to return your own assessment. 
        /// </summary>
        public override bool Produces(IEvolutionState state, Population newpop, int subpop, int thread)
        {
            return true;
        }


        public override void PreparePipeline(object hook)
        {
            // default does nothing
        }

        /// <summary>
        /// A default version of PrepareToProduce which does nothing.  
        /// </summary>
        public override void PrepareToProduce(IEvolutionState s, int subpop, int thread)
        {
            return;
        }

        /// <summary>
        /// A default version of FinishProducing, which does nothing. 
        /// </summary>
        public override void FinishProducing(IEvolutionState s, int subpop, int thread)
        {
            return;
        }

        public override int Produce(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            var n = INDS_PRODUCED;
            if (n < min)
                n = min;
            if (n > max)
                n = max;

            for (var q = 0; q < n; q++)
                inds[start + q] = state.Population.Subpops[subpop].Individuals[Produce(subpop, state, thread)];
            return n;
        }

        /// <summary>
        /// An alternative form of "produce" special to Selection Methods;
        /// selects an individual from the given subpop and returns its position in that subpop. 
        /// </summary>
        public abstract int Produce(int subpop, IEvolutionState state, int thread);

        #endregion // Operations
    }
}