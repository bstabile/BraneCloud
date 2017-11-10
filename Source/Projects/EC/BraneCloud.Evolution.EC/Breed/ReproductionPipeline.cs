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
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Breed
{
    /// <summary> 
    /// ReproductionPipeline is a BreedingPipeline which simply makes a copy
    /// of the individuals it recieves from its source.  
    ///  
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// ...as many as the child produces
    /// <p/><b>Number of Sources</b><br/>
    /// 1
    /// 
    /// <p/><b>Default Base</b><br/>
    /// breed.reproduce
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.breed.ReproductionPipeline")]
    public class ReproductionPipeline : BreedingPipeline
    {
        #region Constants

        public const string P_REPRODUCE = "reproduce";
        public const string P_MUSTCLONE = "must-clone";
        public const int NUM_SOURCES = 1;

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return BreedDefaults.ParamBase.Push(P_REPRODUCE); }
        }

        public override int NumSources
        {
            get { return NUM_SOURCES; }
        }

        public bool MustClone { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            IParameter def = DefaultBase;

            if (!Likelihood.Equals(1.0))
                state.Output.Warning("ReproductionPipeline given a likelihood other than 1.0.  This is nonsensical and will be ignored.",
                    paramBase.Push(P_LIKELIHOOD),
                    def.Push(P_LIKELIHOOD));
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

            // grab individuals from our source and stick 'em right into inds.
            // we'll modify them from there
            int n = Sources[0].Produce(min, max, subpop, inds, state, thread, misc);

            return n;
        }

        #endregion // Operations
    }
}