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
    /// Problem is a prototype which defines the problem against which we will
    /// evaluate individuals in a population. 
    /// 
    /// <p/>Since Problems are IPrototypes, you should expect a new Problem class to be
    /// cloned and used, on a per-thread basis, for the evolution of each
    /// chunk of individuals in a new population.  If you for some reason
    /// need global Problem information, you will have to provide it
    /// statically, or copy pointers over during the clone() process
    /// (there is likely only one Problem prototype, depending on the
    /// Evaluator class used).
    /// 
    /// <p/>Note that Problem does not implement a specific evaluation method.
    /// Your particular Problem subclass will need to implement a some kind of
    /// Problem Form (for example, ISimpleProblem) appropriate to the kind of
    /// evaluation being performed on the Problem.  These Problem Forms will provide
    /// the evaluation methods necessary.
    /// 
    /// <p/>Problem forms will define some kind of <i>evaluation</i> method.  This method
    /// may be called in one of two ways by the Evaluator.
    /// 
    /// <ul> 
    /// <li/> The evaluation is called for a series of individuals.  This is the old approach,
    /// and it means that each individual must be evaluated and modified as specified by the
    /// Problem Form during the evaluation call.
    /// <li/> prepareToEvaluate is called, then a series of individuals is evaluated, and then
    /// finishEvaluating is called.  This is the new approach, and in this case the Problem
    /// is free to delay evaluating and modifying the individuals until finishEvaluating has
    /// been called.  The Problem may perfectly well evaluate and modify the individuals during
    /// each evaluation call if it likes.  It's just given this additional option.
    /// </ul>
    /// 
    /// <p/>Problems should be prepared for both of the above situations.  The easiest way
    /// to handle it is to simply evaluate each individual as his evaluate(...) method is called,
    /// and do nothing during prepareToEvaluate or finishEvaluating.  That should be true for
    /// the vast majority of Problem types.
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.Problem")]
    public abstract class Problem : IProblem
    {
        #region Constants

        public const string P_PROBLEM = "problem";

        #endregion // Constants
        #region Properties

        /// <summary>
        /// Here's a nice default base for you -- you can change it if you like. 
        /// </summary>
        public virtual IParameter DefaultBase
        {
            get { return new Parameter(P_PROBLEM); }
        }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// Default form does nothing
        /// </summary>
        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
        }

        #endregion // Setup
        #region Operations

        #region Contacts

        /// <summary>
        /// Called to set up remote evaluation network contacts when the run is started.  By default does nothing. 
        /// </summary>
        public virtual void InitializeContacts(IEvolutionState state)
        {
        }

        /// <summary>
        /// Called to reinitialize remote evaluation network contacts when the run is restarted from checkpoint.  
        /// By default does nothing. 
        /// </summary>
        public virtual void ReinitializeContacts(IEvolutionState state)
        {
        }

        /// <summary>
        /// Called to shut down remote evaluation network contacts when the run is completed.  
        /// By default does nothing. 
        /// </summary>
        public virtual void CloseContacts(IEvolutionState state, int result)
        {
        }

        #endregion // Contacts
        #region Evaluation

        /// <summary>
        /// May be called by the Evaluator prior to a series of individuals to 
        /// evaluate, and then ended with a finishEvaluating(...).  If this is the
        /// case then the Problem is free to delay modifying the individuals or their
        /// fitnesses until at finishEvaluating(...).  If no prepareToEvaluate(...)
        /// is called prior to evaluation, the Problem must complete its modification
        /// of the individuals and their fitnesses as they are evaluated as stipulated
        /// in the relevant evaluate(...) documentation for ISimpleProblem 
        /// or IGroupedProblem.  The default method does nothing.  Note that
        /// prepareToEvaluate() can be called *multiple times* prior to finishEvaluating()
        /// being called -- in this case, the subsequent calls may be ignored. 
        /// </summary>
        public virtual void PrepareToEvaluate(IEvolutionState state, int threadNum)
        {
        }

        /// <summary>
        /// Will be called by the Evaluator after prepareToEvaluate(...) is called
        /// and then a series of individuals are evaluated.  However individuals may
        /// be evaluated without prepareToEvaluate or finishEvaluating being called
        /// at all.  See the documentation for prepareToEvaluate for more information. 
        /// The default method does nothing.
        /// </summary>
        public virtual void FinishEvaluating(IEvolutionState state, int threadNum)
        {
        }

        /// <summary>
        /// Asynchronous Steady-State EC only: Returns true if the problem is ready to evaluate.  
        /// In most cases, the default is true.  
        /// </summary>
        public virtual bool CanEvaluate
        {
            get { return true; }
        }

        #endregion // Evalutation

        #endregion // Operations
        #region Cloning

        public virtual object Clone()
        {
            try
            {
                return MemberwiseClone();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Cloning Error!", ex);
            } // never happens
        }

        #endregion // Cloning
        #region IO

        /// <summary>
        /// Part of ISimpleProblem.  Included here so you don't have to write the default version, which usually does nothing.
        /// </summary>
        public virtual void Describe(IEvolutionState state, Individual ind, int subpop, int threadnum, int log)
        {
            return;
        }

        #endregion IO
    }
}