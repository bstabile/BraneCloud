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
using BraneCloud.Evolution.EC.Eval;

namespace BraneCloud.Evolution.EC
{
    /// <summary> 
    /// An Evaluator is a singleton object which is responsible for the
    /// evaluation process during the course of an evolutionary run.  Only
    /// one Evaluator is created in a run, and is stored in the EvolutionState
    /// object.
    /// 
    /// <p/>Evaluators typically do their work by applying an instance of some
    /// subclass of Problem to individuals in the population.  Evaluators come
    /// with a Problem prototype which they may clone as necessary to create
    /// new Problem spaces to evaluate individuals in (Problems may be reused
    /// to prevent constant cloning).
    /// 
    /// <p/>Evaluators may be multithreaded, with one Problem instance per thread
    /// usually.  The number of threads they may spawn (excepting a parent
    /// "gathering" thread) is governed by the EvolutionState's evalthreads value.
    /// 
    /// <p/>Be careful about spawning threads -- this system has no few synchronized 
    /// methods for efficiency's sake, so you must either divvy up evaluation in 
    /// a thread-safe fashion, or 
    /// otherwise you must obtain the appropriate locks on individuals in the population
    /// and other objects as necessary.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i><tt>.problem</tt><br/>
    /// <font size="-1">classname, inherits and != ec.Problem</font></td>
    /// <td valign="top">(the class for the Problem prototype p_problem)</td></tr>
    /// <tr><td valign="top"><i>base</i><tt>.masterproblem</tt><br/>
    /// <font size="-1">classname, inherits</font></td>
    /// <td valign="top">(the class for the MasterProblem prototype masterproblem)</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.Evaluator")]
    public abstract class Evaluator : IEvaluator
    {
        #region Constants

        public const string P_PROBLEM = "problem";

        public const string P_MASTERPROBLEM = "masterproblem";
        public const string P_IAMSLAVE = "i-am-slave";

        #endregion // Constants
        #region Properties

        public IProblem p_problem { get; set; }

        public IMasterProblem MasterProblem { get; set; }

        #endregion // Properties
        #region Setup

        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            // Load my problem
            p_problem = (IProblem)(state.Parameters.GetInstanceForParameter(paramBase.Push(P_PROBLEM), null, typeof(IProblem)));
            p_problem.Setup(state, paramBase.Push(P_PROBLEM));

            // Am I a master problem and NOT a slave.  Note that the "eval.i-am-slave" parameter
            // is not set by the user but rather programmatically by the Slave class
            if (state.Parameters.ParameterExists(paramBase.Push(P_MASTERPROBLEM), null))
            // there's a master problem to load
            {
                // load the masterproblem so it can be accessed by the Slave as well (even though it's not used in its official capacity)
                MasterProblem = (IMasterProblem)(state.Parameters.GetInstanceForParameter(
                    paramBase.Push(P_MASTERPROBLEM), null, typeof(IProblem)));
                MasterProblem.Setup(state, paramBase.Push(P_MASTERPROBLEM));

                if (!state.Parameters.GetBoolean(paramBase.Push(P_IAMSLAVE), null, false))
                // I am a master (or possibly a slave -- same params)
                {
                    //try
                    //{
                    //    var masterproblem = (IProblem)(state.Parameters.GetInstanceForParameter(
                    //        paramBase.Push(P_MASTERPROBLEM), null, typeof(IProblem)));
                    //    masterproblem.Setup(state, paramBase.Push(P_MASTERPROBLEM));

                        /*
                         * If a MasterProblem was specified, interpose it between the
                         * evaluator and the real problem.  This allows seamless use
                         * of the master problem.
                         */
                        MasterProblem.Problem = p_problem;
                        p_problem = MasterProblem;
                    //}
                    //catch (ParamClassLoadException)
                    //{
                    //    state.Output.Fatal("Parameter has an invalid value: " + paramBase.Push(P_MASTERPROBLEM));
                    //}
                }
            }
        }

        #endregion // Setup
        #region Operations

        #region Evaluation

        /// <summary>
        /// Evaluates the fitness of an entire population.  You will
        /// have to determine how to handle multiple threads on your own,
        /// as this is a very domain-specific thing. 
        /// </summary>
        public abstract void EvaluatePopulation(IEvolutionState state);

        /// <summary>
        /// Returns non-NULL if the Evaluator believes that the run is
        /// finished: perhaps an ideal individual has been found or some
        /// other run result has shortcircuited the run so that it should
        /// end prematurely right now.  Typically a message is stored in
        /// the String for the user to know why the system shut down.
        /// </summary>
        public abstract string RunComplete(IEvolutionState state);

        public string RunCompleted { get; protected set; }
        /** Requests that the Evaluator quit soon for a user-defined reason provided in the message. */
        public void SetRunCompleted(string message) { RunCompleted = message; }

        #endregion // Evaluation
        #region Contacts

        /// <summary>
        /// Called to set up remote evaluation network contacts when the run is started.  
        /// Mostly used for client/server evaluation (see MasterProblem).  
        /// By default calls p_problem.initializeContacts(state) 
        /// </summary>
        public virtual void InitializeContacts(IEvolutionState state)
        {
            p_problem.InitializeContacts(state);
        }

        /// <summary>
        /// Called to reinitialize remote evaluation network contacts when the run is restarted from checkpoint.  
        /// Mostly used for client/server evaluation (see MasterProblem).  
        /// By default calls p_problem.ReinitializeContacts(state) 
        /// </summary>
        public virtual void ReinitializeContacts(IEvolutionState state)
        {
            p_problem.ReinitializeContacts(state);
        }

        /// <summary>
        /// Called to shut down remote evaluation network contacts when the run is completed.  
        /// Mostly used for client/server evaluation (see MasterProblem).  
        /// By default calls p_problem.CloseContacts(state,result) 
        /// </summary>
        public virtual void CloseContacts(IEvolutionState state, int result)
        {
            p_problem.CloseContacts(state, result);
        }

        #endregion // Contacts

        #endregion // Operations
    }
}