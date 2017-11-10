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

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.SteadyState
{
    /// <summary> 
    /// This interface defines the hooks for SteadyStateEvolutionState objects
    /// to update themselves on.  Note that the the only methods in common
    /// with the standard statistics are initialization and final. This is an
    /// optional interface: SteadyStateEvolutionState will complain, but
    /// will permit Statistics objects that don't adhere to it, though they will
    /// only have their initialization and statistics methods called!
    /// 
    /// <p/>See SteadyStateEvolutionState for how regular Statistics objects'
    /// hook methods are called in steady state evolution.
    /// </summary>
    [ECConfiguration("ec.steadystate.ISteadyStateStatistics")]
    public interface ISteadyStateStatistics
    {
        /// <summary>
        /// Called when we created an empty initial Population.
        /// </summary>
        void EnteringInitialPopulationStatistics(SteadyStateEvolutionState state);

        /// <summary>
        /// Called when we have filled the initial population and are entering the steady state.
        /// </summary>
        void EnteringSteadyStateStatistics(int subpop, SteadyStateEvolutionState state);

        /// <summary>
        /// Called each time new individuals are bred during the steady-state process.
        /// </summary>
        void IndividualsBredStatistics(SteadyStateEvolutionState state, Individual[] individuals);

        /// <summary>
        /// Called each time new individuals are evaluated during the steady-state
        /// process, NOT including the initial generation's individuals.
        /// </summary>
        void IndividualsEvaluatedStatistics(SteadyStateEvolutionState state, Individual[] newIndividuals,
            Individual[] oldIndividuals, int[] subpops, int[] indices);

        /// <summary>
        /// Called when the generation count increments.
        /// </summary>
        void GenerationBoundaryStatistics(IEvolutionState state);

        /// <summary>
        /// Called immediately before checkpointing occurs.
        /// </summary>
        void PreCheckpointStatistics(IEvolutionState state);

        /// <summary>
        /// Called immediately after checkpointing occurs.
        /// </summary>
        void PostCheckpointStatistics(IEvolutionState state);

        /// <summary>
        /// Called immediately before the pre-breeding exchange occurs.
        /// </summary>
        void PrePreBreedingExchangeStatistics(IEvolutionState state);

        /// <summary>
        /// Called immediately after the pre-breeding exchange occurs.
        /// </summary>
        void PostPreBreedingExchangeStatistics(IEvolutionState state);

        /// <summary>
        /// Called immediately before the post-breeding exchange occurs.
        /// </summary>
        void PrePostBreedingExchangeStatistics(IEvolutionState state);

        /// <summary>
        /// Called immediately after the post-breeding exchange occurs.
        /// </summary>
        void PostPostBreedingExchangeStatistics(IEvolutionState state);

        /// <summary>
        /// Called immediately after the run has completed.  <i>result</i>
        /// is either <tt>state.R_FAILURE</tt>, indicating that an ideal individual
        /// was not found, or <tt>state.R_SUCCESS</tt>, indicating that an ideal
        /// individual <i>was</i> found.
        /// </summary>
        void FinalStatistics(IEvolutionState state, int result);
    }
}