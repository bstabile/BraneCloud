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

namespace BraneCloud.Evolution.EC.Simple
{
    /// <summary> 
    /// SimpleInitializer is a default Initializer which initializes a Population
    /// by calling the Population's populate(...) method.  For most applications,
    /// this should suffice.
    /// </summary>    
    [Serializable]
    [ECConfiguration("ec.simple.SimpleInitializer")]
    public class SimpleInitializer : Initializer
    {
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
        }

        public override Population SetupPopulation(IEvolutionState state, int thread)
        {
            IParameter paramBase = new Parameter(P_POP);
            var p = (Population)state.Parameters.GetInstanceForParameterEq(paramBase, null, typeof(Population)); // Population.class is fine
            p.Setup(state, paramBase);
            return p;
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Creates, populates, and returns a new population by making a new
        /// population, calling Setup(...) on it, and calling populate(...)
        /// on it, assuming an unthreaded environment (thread 0).
        /// Obviously, this is an expensive method.  It should only
        /// be called once typically in a run. 
        /// </summary>        
        public override Population InitialPopulation(IEvolutionState state, int thread)
        {
            var p = SetupPopulation(state, thread);
            p.Populate(state, thread);
            return p;
        }

        #endregion // Operations
    }
}