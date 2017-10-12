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
    /// VectorMutationPipeline is a BreedingPipeline which implements a simple default Mutation
    /// for VectorIndividuals.  Normally it takes an individual and returns a mutated 
    /// child individual. VectorMutationPipeline works by calling defaultMutate(...) on the 
    /// parent individual.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// (however many its source produces)
    /// <p/><b>Number of Sources</b><br/>
    /// 1
    /// <p/><b>Default Base</b><br/>
    /// vector.mutate (not that it matters)
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.vector.breed.VectorMutationPipeline")]
    public class VectorMutationPipeline : BreedingPipeline
    {
        #region Constants

        public const string P_MUTATION = "mutate";
        public const int NUM_SOURCES = 1;

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return VectorDefaults.ParamBase.Push(P_MUTATION); }
        }

        /// <summary>
        /// Returns 1. 
        /// </summary>
        public override int NumSources
        {
            get { return NUM_SOURCES; }
        }

        #endregion // Properties
        #region Operations

        public override int Produce(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            // grab individuals from our source and stick 'em right into inds.
            // we'll modify them from there
            var n = Sources[0].Produce(min, max, start, subpop, inds, state, thread);

            // should we bother?
            if (!state.Random[thread].NextBoolean(Likelihood))
                return Reproduce(n, start, subpop, inds, state, thread, false);  // DON'T produce children from source -- we already did

            // clone the individuals if necessary
            if (!(Sources[0] is BreedingPipeline))
                for (var q = start; q < n + start; q++)
                    inds[q] = (Individual)inds[q].Clone();

            // mutate 'em
            for (var q = start; q < n + start; q++)
            {
                ((VectorIndividual)inds[q]).DefaultMutate(state, thread);
                ((VectorIndividual)inds[q]).Evaluated = false;
            }

            return n;
        }

        #endregion // Operations
    }
}