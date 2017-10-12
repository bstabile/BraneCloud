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

using BraneCloud.Evolution.EC.SteadyState;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC
{
    /// <summary> 
    /// Statistics and its subclasses are ICliques which generate statistics
    /// during the run.  Statistics are arranged in a tree hierarchy -- 
    /// The root statistics object may have "Children", and when the root is
    /// called, it calls its Children with the same message.  You can override
    /// this behavior however you see fit.
    /// 
    /// <p/>There are lots of places where statistics might be nice to print out.
    /// These places are implemented as hooks in the Statistics object which you
    /// can override if you like; otherwise they call the default behavior.  If
    /// you plan on allowing your Statistics subclass to contain Children, you
    /// should remember to call the appropriate super.foo() if you 
    /// override any foo() method.
    /// 
    /// <p/>While there are lots of hooks, various EvolutionState objects only
    /// implement a subset of them.   You'll need to look at the EvolutionState
    /// code to see which ones are implemented to make sure that your statistics
    /// hooks are called appropriately!
    /// 
    /// <p/>Statistics objects should set up their statistics logs etc. during 
    /// <tt>Setup(...)</tt>.  Remember to make the log restartable in
    /// case of restarting from a checkpoint.
    /// 
    /// <p/><b>Steady-State Statistics</b>.  For convenience, Statistics contains
    /// default implementations of the ISteadyStateStatistics methods but
    /// does not implement ISteadyStateStatistics.  This mostly is intended
    /// to keep SteadyStateStatistcsForm implementations from having to call
    /// functions on all their Children: instead they can just call foo.super();
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>num-Children</tt><br/>
    /// <font size="-1">int &gt;= 0</font></td>
    /// <td valign="top">(number of child statistics objects)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>child</tt>.<i>n</i><br/>
    /// <font size="-1">classname, inherits or equals ec.Statistics</font></td>
    /// <td valign="top">(the class of child statistics object number <i>n</i>)</td></tr>
    /// </table>
    /// 
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>child</tt>.<i>n</i></td>
    /// <td>species (child statistics object number <i>n</i>)</td></tr>
    /// </table>
    /// </summary>   
    [Serializable]
    [ECConfiguration("ec.Statistics")]
    public class Statistics : ISingleton
    {
        #region Constants

        public const string P_NUMCHILDREN = "num-children";
        public const string P_CHILD = "child";

        #endregion // Constants
        #region Properties

        public Statistics[] Children { get; set; }

        #endregion // Properties
        #region Setup

        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            var t = state.Parameters.GetIntWithDefault(paramBase.Push(P_NUMCHILDREN), null, 0);
            if (t < 0)
                state.Output.Fatal("A Statistics object cannot have negative number of Children", paramBase.Push(P_NUMCHILDREN));

            // load the trees
            Children = new Statistics[t];

            for (var x = 0; x < t; x++)
            {
                var p = paramBase.Push(P_CHILD).Push("" + x);
                Children[x] = (Statistics)(state.Parameters.GetInstanceForParameterEq(p, null, typeof(Statistics)));
                Children[x].Setup(state, p);
            }
        }

        #endregion // Setup
        #region Operations

        #region On Initialization

        /// <summary>
        /// Called immediately before population initialization occurs. 
        /// </summary>
        public virtual void PreInitializationStatistics(IEvolutionState state)
        {
            foreach (var t in Children)
                t.PreInitializationStatistics(state);
        }

        /// <summary>
        /// GENERATIONAL: Called immediately after population initialization occurs. 
        /// </summary>
        public virtual void PostInitializationStatistics(IEvolutionState state)
        {
            foreach (var t in Children)
                t.PostInitializationStatistics(state);
        }

        #endregion // Pre-Post Initialization
        #region On Checkpoint

        /// <summary>
        /// Called immediately before checkpointing occurs. 
        /// </summary>
        public virtual void PreCheckpointStatistics(IEvolutionState state)
        {
            foreach (var t in Children)
                t.PreCheckpointStatistics(state);
        }

        /// <summary>
        /// Called immediately after checkpointing occurs. 
        /// </summary>
        public virtual void PostCheckpointStatistics(IEvolutionState state)
        {
            foreach (var t in Children)
                t.PostCheckpointStatistics(state);
        }

        #endregion // On Checkpoint
        #region On Evaluation

        /// <summary>
        /// GENERATIONAL: Called immediately before evaluation occurs. 
        /// </summary>
        public virtual void PreEvaluationStatistics(IEvolutionState state)
        {
            foreach (var t in Children)
                t.PreEvaluationStatistics(state);
        }

        /// <summary>
        /// GENERATIONAL: Called immediately after evaluation occurs. 
        /// </summary>
        public virtual void PostEvaluationStatistics(IEvolutionState state)
        {
            foreach (var t in Children)
                t.PostEvaluationStatistics(state);
        }

        #endregion // On Evaluation
        #region On PreBreeding

        /// <summary>
        /// Called immediately before the pre-breeding exchange occurs. 
        /// </summary>
        public virtual void PrePreBreedingExchangeStatistics(IEvolutionState state)
        {
            foreach (var t in Children)
                t.PrePreBreedingExchangeStatistics(state);
        }

        /// <summary>
        /// Called immediately after the pre-breeding exchange occurs. 
        /// </summary>
        public virtual void PostPreBreedingExchangeStatistics(IEvolutionState state)
        {
            foreach (var t in Children)
                t.PostPreBreedingExchangeStatistics(state);
        }

        #endregion // On PreBreeding
        #region On Breeding

        /// <summary>
        /// GENERATIONAL: Called immediately before breeding occurs. 
        /// </summary>
        public virtual void PreBreedingStatistics(IEvolutionState state)
        {
            foreach (var t in Children)
                t.PreBreedingStatistics(state);
        }

        /// <summary>
        /// GENERATIONAL: Called immediately after breeding occurs. 
        /// </summary>
        public virtual void PostBreedingStatistics(IEvolutionState state)
        {
            foreach (var t in Children)
                t.PostBreedingStatistics(state);
        }

        #endregion // On Breeding
        #region On PostBreeding

        /// <summary>
        /// Called immediately before the post-breeding exchange occurs. 
        /// </summary>
        public virtual void PrePostBreedingExchangeStatistics(IEvolutionState state)
        {
            foreach (var t in Children)
                t.PrePostBreedingExchangeStatistics(state);
        }

        /// <summary>
        /// Called immediately after the post-breeding exchange occurs. 
        /// </summary>
        public virtual void PostPostBreedingExchangeStatistics(IEvolutionState state)
        {
            foreach (var t in Children)
                t.PostPostBreedingExchangeStatistics(state);
        }

        #endregion // On PostBreeding
        #region Final

        /// <summary>
        /// Called immediately after the run has completed.  <i>result</i>
        /// is either <tt>state.R_FAILURE</tt>, indicating that an ideal individual
        /// was not found, or <tt>state.R_SUCCESS</tt>, indicating that an ideal
        /// individual <i>was</i> found. 
        /// </summary>
        public virtual void FinalStatistics(IEvolutionState state, int result)
        {
            foreach (var t in Children)
                t.FinalStatistics(state, result);
        }

        #endregion // Final
        #region SteadyState

        /// <summary>
        /// STEADY-STATE: called when we created an empty initial Population.
        /// </summary>
        public void EnteringInitialPopulationStatistics(SteadyStateEvolutionState state)
        {
            foreach (var t in Children)
                if (t is ISteadyStateStatistics)
                    ((ISteadyStateStatistics)t).EnteringInitialPopulationStatistics(state);
        }

        /// <summary>
        /// STEADY-STATE: called when a given Subpopulation is entering the Steady-State.
        /// </summary>
        public void EnteringSteadyStateStatistics(int subpop, SteadyStateEvolutionState state)
        {
            foreach (var t in Children)
                if (t is ISteadyStateStatistics)
                    ((ISteadyStateStatistics)t).EnteringSteadyStateStatistics(subpop, state);
        }

        /// <summary>
        /// STEADY-STATE: called each time new individuals are bred during the steady-state process. 
        /// </summary>
        public virtual void IndividualsBredStatistics(SteadyStateEvolutionState state, Individual[] newIndividuals)
        {
            foreach (var t in Children)
                if (t is ISteadyStateStatistics)
                    ((ISteadyStateStatistics)t).IndividualsBredStatistics(state, newIndividuals);
        }

        /// <summary>
        /// STEADY-STATE: called each time new individuals are evaluated during the steady-state
        /// process.  You can look up the individuals in state.newIndividuals[] 
        /// </summary>
        public virtual void IndividualsEvaluatedStatistics(SteadyStateEvolutionState state,
            Individual[] newIndividuals, Individual[] oldIndividuals, int[] subpops, int[] indicies)
        {
            foreach (var t in Children)
                if (t is ISteadyStateStatistics)
                    ((ISteadyStateStatistics)t).IndividualsEvaluatedStatistics(state, newIndividuals, oldIndividuals, subpops, indicies);
        }

        /// <summary>
        /// STEADY-STATE: called each time the generation count increments 
        /// </summary>
        public virtual void GenerationBoundaryStatistics(IEvolutionState state)
        {
            foreach (var t in Children)
                if (t is ISteadyStateStatistics)
                    ((ISteadyStateStatistics)t).GenerationBoundaryStatistics(state);
        }

        #endregion // SteadyState

        #endregion // Operations
    }
}