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
using System.IO;
using BraneCloud.Evolution.EC.Eval;
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.CoEvolve;
using BraneCloud.Evolution.EC.SteadyState;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Runtime.Eval
{    
    /// <summary> 
    /// MasterProblem
    /// 
    /// <p/>The MasterProblem is a special ECJ problem that performs evaluations by sending them to
    /// a remote Slave process to be evaluated.  As it implements both the
    /// <i>SimpleProblemForm</i> and the <i>IGroupedProblem</i> interfaces, the MasterProblem
    /// can perform both traditional EC evaluations as well as coevolutionary evaluations.
    /// 
    /// <p/>When a MasterProblem is specified by the Evaluator, the Problem is set up as usual, but then
    /// the MasterProblem replaces it.  The Problem is not garbage collected -- instead, it's hung off the
    /// MasterProblem's <tt>problem</tt> variable.  In some sense the Problem is "pushed aside".
    /// 
    /// <p/>If the Evaluator begins by calling prepareToEvaluate(), and we're not doing coevolution, then
    /// the MasterProblem does not evaluate individuals immediately.  Instead, it waits for at most 
    /// <i>jobSize</i> individuals be submitted via evaluate(), and then sends them all off in a group,
    /// called a <i>job</i>, to the remote slave.  In other situations (coevolution, or no prepareToEvaluate())
    /// the MasterProblem sends off individuals immediately.
    /// 
    /// <p/>It may be the case that no Slave has space in its queue to accept a new job containing, among others,
    /// your new individual.  In this case, calling evaluate() will block until one comes available.  You can avoid
    /// this by testing for availability first by calling canEvaluate().  Note that canEvaluate() and evaluate()
    /// together are not atomic and so you should not rely on this facility if your system uses multiple threads.
    /// 
    /// <p/>When the individuals or their fitnesses return, they are immediately updated in place.  You have three
    /// options to wait for them:
    /// 
    /// <ul>
    /// <li/><p/>You can wait for all the individuals to finish evaluation by calling finishEvaluating().
    /// If you call this method before a job is entirely filled, it will be sent in truncated format (which
    /// generally is perfectly fine).  You then block until all the jobs have been completed and the individuals
    /// updated.
    /// 
    /// <li/><p/>You can block until at least one individual is available, by calling getNextEvaluatedIndividual(),
    /// which blocks and then returns the individual that was just completed.
    /// 
    /// <li/><p/>You can test in non-blocking fashion to see if an individual is available, by calling 
    /// evaluatedIndividualAvailable().  If this returns true, you may then call getNextEvaluatedIndividual()
    /// to get the individual.  Note that this isn't atomic, so don't use it if you have multiple threads.
    /// </ul>
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr/><td valign="top"><i>base.</i><tt>debug-info</tt><br/>
    /// <font size="-1">boolean</font></td>
    /// <td valign="top"/>(whether the system should display information useful for debugging purposes)<br/>
    /// <tr><td valign="top"><i>base.</i><tt>job-size</tt><br/>
    /// <font size="-1">integer &gt; 0 </font></td>
    /// <td valign="top">(how large should a job be at most?)<br/>
    /// </td></tr>
    /// <!-- technically these are handled by the SlaveMonitor -->
    /// <tr><td valign="top"><tt>eval.master.port</tt><br/>
    /// <font size="-1">int</font></td>
    /// <td valign="top">(the port where the slaves will connect)<br/>
    /// </td></tr>
    /// <tr><td valign="top"><tt>eval.compression</tt><br/>
    /// <font size="-1">boolean</font></td>
    /// <td valign="top">(whether the communication with the slaves should be compressed or not)<br/>
    /// </td></tr>
    /// <tr><td valign="top"><tt>eval.masterproblem.max-jobs-per-slave</tt><br/>
    /// <font size="-1">int</font></td>
    /// <td valign="top">(the maximum load (number of jobs) per slave at any point in time)<br/>
    /// </td></tr>
    /// </table>
    /// </summary>    
    [Serializable]
    [ECConfiguration("ec.eval.MasterProblem")]
    public class MasterProblem : Problem, IMasterProblem
    {
        #region Constants

        private const long SerialVersionUID = 1;

        public const string P_DEBUG_INFO = "debug-info";
        public const string P_JOB_SIZE = "job-size";

        #endregion // Constants
        #region Properties

        public int JobSize { get; set; }
        public bool ShowDebugInfo { get; set; }
        public IProblem Problem { get; set; }
        public bool BatchMode { get; set; }

        public ISlaveMonitor Monitor
        {
            get => _monitor;
            set => _monitor = value;
        }
        [NonSerialized]
        private ISlaveMonitor _monitor;

        public List<QueueIndividual> Queue { get; set; }

        public override bool CanEvaluate
        {
            get { return (Monitor.NumAvailableSlaves != 0); }
        }

        /// <summary>
        /// This will only return true if (1) the EvolutionState is a SteadyStateEvolutionState and
        /// (2) an individual has returned from the system.  If you're not doing steady state evolution,
        /// you should not call this method.  
        /// </summary>
        public virtual bool EvaluatedIndividualAvailable => Monitor.EvaluatedIndividualAvailable;

        #endregion // Properties
        #region Setup

        public MasterProblem()
        {
        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            ThreadClass.Current().Name = "MainThread: ";
            base.Setup(state, paramBase);
            ShowDebugInfo = state.Parameters.GetBoolean(paramBase.Push(P_DEBUG_INFO), null, false);

            JobSize = state.Parameters.GetIntWithDefault(paramBase.Push(P_JOB_SIZE), null, 1);
            if (JobSize <= 0)
                state.Output.Fatal("The job size must be an integer > 0.", paramBase.Push(P_JOB_SIZE));

            BatchMode = false;
        }

        #endregion // Setup
        #region Operations

        internal virtual void Flush(IEvolutionState state, int threadnum)
        {
            if (Queue != null && Queue.Count > 0)
            {
                var inds = new Individual[Queue.Count];
                var subpops = new int[Queue.Count];
                for (var i = 0; i < Queue.Count; i++)
                {
                    var qind = Queue[i];
                    inds[i] = qind.Ind;
                    subpops[i] = qind.Subpop;
                }
                Evaluate(state, inds, subpops, threadnum);
            }
            Queue = new List<QueueIndividual>();
        }

        public override void Describe(IEvolutionState state, Individual ind, int subpop, int threadnum, int log)
        {
            if (!(Problem is ISimpleProblem))
            {
                state.Output.Fatal("MasterProblem.Describe(...) invoked, but the underlying Problem is not of SimpleProblemForm");
            }

            ((ISimpleProblem)Problem).Describe(state, ind, subpop, threadnum, log);
        }

        public virtual void PreprocessPopulation(IEvolutionState state, Population pop, bool[] prepareForFitnessAssessment, bool countVictoriesOnly)
        {
            if (!(Problem is IGroupedProblem))
            {
                state.Output.Fatal("MasterProblem.PreprocessPopulation(...) invoked, but the underlying Problem is not of IGroupedProblem");
            }

            ((IGroupedProblem)Problem).PreprocessPopulation(state, pop, prepareForFitnessAssessment, countVictoriesOnly);
        }

        public virtual int PostprocessPopulation(IEvolutionState state, Population pop, bool[] assessFitness, bool countVictoriesOnly)
        {
            if (!(Problem is IGroupedProblem))
            {
                state.Output.Fatal("MasterProblem.PostprocessPopulation(...) invoked, but the underlying Problem is not of IGroupedProblem");
            }

            return ((IGroupedProblem)Problem).PostprocessPopulation(state, pop, assessFitness, countVictoriesOnly);
        }

        #region Evaluation

        /// <summary>
        /// This method blocks until an individual is available  from the slaves 
        /// (which will cause evaluatedIndividualAvailable() to return true), 
        /// at which time it returns the individual.  You should only call this method
        /// if you're doing steady state evolution -- otherwise, the method will block forever. 
        /// </summary>
        public virtual QueueIndividual NextEvaluatedIndividual
        {
            get
            {
                return Monitor.WaitForIndividual();
            }

        }

        /// <summary>
        /// Prepare for a batch of evaluations.
        /// </summary>
        public override void PrepareToEvaluate(IEvolutionState state, int threadnum)
        {
            if (JobSize > 1)
                Queue = new List<QueueIndividual>();
            BatchMode = true;
        }

        /// <summary>
        /// Wait until a batch of evaluations is finished.
        /// </summary>
        public override void FinishEvaluating(IEvolutionState state, int threadnum)
        {
            if (ShowDebugInfo)
                state.Output.Message(ThreadClass.Current().Name + "Waiting for all slaves to finish.");
            Flush(state, threadnum);
            Queue = null; // get rid of it just in case

            Monitor.WaitForAllSlavesToFinishEvaluating(state);
            BatchMode = false;
            if (ShowDebugInfo)
                state.Output.Message(ThreadClass.Current().Name + "All slaves have finished their jobs.");
        }

        /// <summary>
        /// Evaluate a regular individual.
        /// </summary>
        public virtual void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            if (JobSize > 1 && BatchMode)
            // chunked evaluation mechanism
            {
                Queue.Add(new QueueIndividual(ind, subpop));
                if (Queue.Count >= JobSize)
                    Flush(state, threadnum);
            }
            // ordinary evaluation mechanism  
            else
                Evaluate(state, new[] { ind }, new[] { subpop }, threadnum);
        }

        /// <summary>
        /// Send a group of individuals to one slave for evaluation.
        /// </summary>
        public virtual void Evaluate(IEvolutionState state, Individual[] inds, int[] subpops, int threadnum)
        {
            if (ShowDebugInfo)
                state.Output.Message(ThreadClass.Current().Name
                    + "Starting a " + (BatchMode ? "batched " : "") + "SimpleProblemForm evaluation.");

            // Acquire a slave socket
            var job = new Job
            {
                Type = SlaveEvaluationType.Simple,
                Inds = inds,
                Subpops = subpops,
                UpdateFitness = new bool[inds.Length]
            };
            for (var i = 0; i < inds.Length; i++)
                job.UpdateFitness[i] = true;
            Monitor.ScheduleJobForEvaluation(state, job);
            if (!BatchMode)
                Monitor.WaitForAllSlavesToFinishEvaluating(state);
            if (ShowDebugInfo)
                state.Output.Message(ThreadClass.Current().Name
                    + "Finished a " + (BatchMode ? "batched " : "") + "SimpleProblemForm evaluation.");
        }

        /// <summary>
        /// Regular coevolutionary evaluation.
        /// </summary>
        public virtual void Evaluate(IEvolutionState state, Individual[] inds, bool[] updateFitness, bool countVictoriesOnly, int[] subpops, int threadnum)
        {
            if (ShowDebugInfo)
                state.Output.Message("Starting a IGroupedProblem evaluation.");

            // Acquire a slave socket
            var job = new Job
            {
                Type = SlaveEvaluationType.Grouped,
                Subpops = subpops,
                CountVictoriesOnly = countVictoriesOnly,
                Inds = inds,
                UpdateFitness = updateFitness
            };
            Monitor.ScheduleJobForEvaluation(state, job);

            if (!BatchMode)
                Monitor.WaitForAllSlavesToFinishEvaluating(state);

            if (ShowDebugInfo)
                state.Output.Message("Finished the IGroupedProblem evaluation.");
        }

        /// <summary>
        /// This method blocks until an individual is available from the slaves 
        /// (which will cause evaluatedIndividualAvailable() to return true), 
        /// at which time it returns the individual.  You should only call this method
        /// if you're doing steady state evolution -- otherwise, the method will block forever.
        /// </summary>
        public QueueIndividual GetNextEvaluatedIndividual()
        {
            return Monitor.WaitForIndividual();
        }

        #endregion // Evaluation
        #region Additional Data

        /// <summary>
        /// This method is called from the SlaveMonitor's accept() thread to optionally send additional data to the
        /// Slave via the dataOut stream.  By default it does nothing.  If you override this you must also override (and use) 
        /// receiveAdditionalData() and transferAdditionalData().
        /// </summary>
        public void SendAdditionalData(IEvolutionState state, BinaryWriter dataOut)
        {
            // do nothing
        }

        /// <summary>
        /// This method is called on a MasterProblem by the Slave.  You should use this method to store away
        /// received data via the dataIn stream for later transferring to the current EvolutionState via the
        /// transferAdditionalData method.  You should NOT expect this MasterProblem to be used for by the Slave
        /// for evolution (though it might).  By default this method does nothing, which is the usual situation. 
        /// The EvolutionState is provided solely for you to be able to output warnings and errors: do not rely
        /// on it for any other purpose (including access of the random number generator or storing any data).
        /// </summary>
        public void ReceiveAdditionalData(IEvolutionState state, BinaryReader dataIn)
        {
            // do nothing
        }

        /// <summary>
        /// This method is called by a Slave to transfer data previously loaded via receiveAdditionalData() to
        /// a running EvolutionState at the beginning of evolution.  It may be called multiple times if multiple
        /// EvolutionStates are created. By default this method does nothing, which is the usual situation.
        /// </summary>
        public void TransferAdditionalData(IEvolutionState state)
        {
            // do nothing
        }

        #endregion // Additional Data
        #region Contacts

        /// <summary>
        /// Initialize contacts with the slaves. 
        /// </summary>
        public override void InitializeContacts(IEvolutionState state)
        {
            if (ShowDebugInfo)
                state.Output.Message(ThreadClass.Current().Name + "Spawning the server thread.");
            Monitor = new SlaveMonitor(state, ShowDebugInfo, this);
        }

        /// <summary>
        /// Reinitialize contacts with the slaves. 
        /// </summary>
        public override void ReinitializeContacts(IEvolutionState state)
        {
            InitializeContacts(state);
        }

        /// <summary>
        /// Gracefully close contacts with the slaves. 
        /// </summary>
        public override void CloseContacts(IEvolutionState state, int result)
        {
            Monitor.Shutdown();
        }

        #endregion // Contacts

        #endregion // Operations
        #region Cloning

        /// <summary>
        /// Except for the problem, everything else is shallow-cloned.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            var c = (IMasterProblem)(base.Clone());

            // shallow-cloned stuff
            c.Monitor = Monitor;
            c.BatchMode = BatchMode;
            c.JobSize = JobSize;
            c.ShowDebugInfo = ShowDebugInfo;

            // deep-cloned stuff
            c.Problem = (IProblem)(Problem.Clone());

            return c;
        }

        #endregion // Cloning

        //#region ISerializable

        ///// <summary>
        ///// Custom serialization. 
        ///// </summary>
        //public virtual void GetObjectData(SerializationInfo outputInfo, StreamingContext context)
        //{
        //    outputInfo.AddValue("ec.eval.MasterProblemData1", Problem);
        //}

        ///// <summary>
        ///// Custom serialization. 
        ///// </summary>
        //protected MasterProblem(SerializationInfo inputInfo, StreamingContext context)
        //{
        //    Problem = (IProblem)inputInfo.GetValue("ec.eval.MasterProblemData1", typeof(IProblem));
        //}

        //#endregion // ISerializable
    }
}