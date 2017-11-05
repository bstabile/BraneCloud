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
    /// The Exchanger is a singleton object whose job is to (optionally)
    /// perform individual exchanges between subpops in the run,
    /// or exchange individuals with other concurrent evolutionary run processes,
    /// using sockets or whatever.  Keep in mind that other processes may go down,
    /// or be started up from checkpoints, etc.
    /// </summary>    
    [Serializable]
    [ECConfiguration("ec.Exchanger")]
    public abstract class Exchanger : ISingleton
    {
        #region Setup

        public abstract void Setup(IEvolutionState param1, IParameter param2);

        #endregion // Setup
        #region Operations

        #region Remote Contacts

        /// <summary>
        /// Initializes contacts with other processes, if that's what you're doing. 
        /// Called at the beginning of an evolutionary run, before a population is set up. 
        /// </summary>
        public virtual void InitializeContacts(IEvolutionState state) { }

        /// <summary>
        /// Initializes contacts with other processes, if that's what you're doing.  
        /// Called after restarting from a checkpoint. 
        /// </summary>
        public virtual void ReinitializeContacts(IEvolutionState state) { }

        /// <summary>
        /// Closes contacts with other processes, if that's what you're doing.  
        /// Called at the end of an evolutionary run. result is either 
        /// ec.EvolutionState.R_SUCCESS or ec.EvolutionState.R_FAILURE, 
        /// indicating whether or not an ideal individual was found. 
        /// </summary>
        public virtual void CloseContacts(IEvolutionState state, int result) { }

        #endregion // Remote Contacts
        #region Exchange

        /// <summary>
        /// Performs exchanges after the population has been evaluated but before it has been bred,
        /// once every generation (or pseudo-generation). 
        /// </summary>
        public abstract Population PreBreedingExchangePopulation(IEvolutionState state);
        
        /// <summary>
        /// Performs exchanges after the population has been bred but before it has been evaluated,
        /// once every generation (or pseudo-generation). 
        /// </summary>
        public abstract Population PostBreedingExchangePopulation(IEvolutionState state);

        /** Typically called by preBreedingExchangePopulation prior to migrating an individual.
            Override this method to process the migrant, or provide a different Individual to migrate.
            The default simply returns the individual.  "island" refers to the island id of the
            destination island for this individual, or null if there is no island (as is the case
            in InterPopulationExchange).  "subpop" refers the expected subpopulation of the individual
            in the destination island, or the subpopulation the indivdiual is migrating to in InterPopulationExchange.
            Hint: if you are using IslandExchange and your island has access to the server exchange parameters 
            ("exch.num-islands" and all parameters starting with "exch.island."), you can can call
            IslandExchange.getIslandIndex(state, island) to retrieve the island number in the parameters,
            from which you can then determine additional useful information about the destination island. */
        protected Individual Process(IEvolutionState state, int thread, string island, int subpop, Individual ind)
        {
            return ind;
        }

        /// <summary>
        /// Called after PreBreedingExchangePopulation(...) to evaluate whether or not
        /// the exchanger wishes the run to shut down (with ec.EvolutionState.R_FAILURE) --
        /// returns a String (which will be printed out as a message) if the exchanger
        /// wants to shut down, else returns null if the exchanger does NOT want to shut down.
        /// Why would you want to shut down?
        /// This would happen for two reasons.  First, another process might have found
        /// an ideal individual and the global run is now over.  Second, some network
        /// or operating system error may have occurred and the system needs to be shut
        /// down gracefully.  Note that if the exchanger wants to shut down, the system
        /// will shut down REGARDLESS of whether or not the user stated 
        /// ec.EvolutionState.quitOnRunComplete. 
        /// </summary>
        /// 
        public abstract string RunComplete(IEvolutionState state);
        
        #endregion // Exchange

        #endregion // Operations
    }
}