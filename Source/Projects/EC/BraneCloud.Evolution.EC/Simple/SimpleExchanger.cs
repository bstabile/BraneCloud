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
using BraneCloud.Evolution.EC.SteadyState;

namespace BraneCloud.Evolution.EC.Simple
{
    /// <summary> 
    /// A SimpleExchanger is a default Exchanger which, well, doesn't do anything.
    /// Most applications don't need Exchanger facilities; this simple version will suffice.
    /// 
    /// <p/>The SimpleExchanger implements the ISteadyStateExchanger, mostly
    /// because it does nothing with individuals.  For this reason, it is final;
    /// implement your own Exchanger if you need to do something more advanced.
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.simple.SimpleExchanger")]
    public sealed class SimpleExchanger : Exchanger, ISteadyStateExchanger
    {
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Doesn't do anything. 
        /// </summary>
        public override void InitializeContacts(IEvolutionState state)
        {
            // don't care
            return;
        }

        /// <summary>
        /// Doesn't do anything. 
        /// </summary>
        public override void ReinitializeContacts(IEvolutionState state)
        {
            // don't care
            return;
        }

        /// <summary>
        /// Doesn't do anything. 
        /// </summary>
        public override void CloseContacts(IEvolutionState state, int result)
        {
            // don't care
            return;
        }

        /// <summary>
        /// Simply returns state.Population. 
        /// </summary>
        public override Population PreBreedingExchangePopulation(IEvolutionState state)
        {
            // don't care
            return state.Population;
        }

        /// <summary>
        /// Simply returns state.Population. 
        /// </summary>
        public override Population PostBreedingExchangePopulation(IEvolutionState state)
        {
            // don't care
            return state.Population;
        }

        /// <summary>
        /// Always returns null 
        /// </summary>
        public override string RunComplete(IEvolutionState state)
        {
            return null;
        }

        #endregion // Operations
    }
}