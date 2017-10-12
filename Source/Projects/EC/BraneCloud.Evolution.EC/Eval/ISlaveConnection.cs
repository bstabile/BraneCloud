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

using System.IO;
using System.Net.Sockets;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Support;

namespace BraneCloud.Evolution.EC.Eval
{
    /// <summary> 
    /// SlaveConnection
    /// 
    /// This class contains certain information associated with a slave: its name, connection socket,
    /// input and output streams, and the job queue.  Additionally, the class sets up an auxillary thread
    /// which reads and writes to the streams to talk to the slave in the background.  This thread uses
    /// the SlaveMonitor as its synchronization point (it sleeps with wait() and wakes up when notified()
    /// to do some work).
    /// 
    /// <p/>Generally SlaveConnection is only seen by communicates only with SlaveMonitor.
    /// </summary>
    [ECConfiguration("ec.eval.ISlaveConnection")]
    public interface ISlaveConnection
    {
        #region Properties

        /// <summary>
        /// Name of the slave process. 
        /// </summary>
        string SlaveName { get; set; }

        /// <summary>
        /// Socket for communication with the slave process. 
        /// </summary>
        TcpClient EvalSocket { get; set; }

        /// <summary>
        /// Used to transmit data to the slave. 
        /// </summary>
        BinaryWriter DataOut { get; set; }

        /// <summary>
        /// Used to read results and randoms state from slave. 
        /// </summary>
        BinaryReader DataIn { get; set; }

        /// <summary>
        /// A pointer to the evolution state.
        /// </summary>
        IEvolutionState State { get; set; }

        /// <summary>
        /// A pointer to the monitor.
        /// </summary>
        ISlaveMonitor SlaveMonitor { get; set; }

        /// <summary>
        /// A pointer to the worker thread that is working for this slave.
        /// </summary>
        ThreadClass Reader { get; set; }

        ThreadClass Writer { get; set; }

        bool ShowDebugInfo { get; set; }

        /// <summary>
        /// This method is called whenever there are any communication problems with the slave
        /// (indicating possibly that the slave might have crashed).  In this case, the jobs will
        /// be rescheduled for evaluation on other slaves.
        /// </summary>
        bool ShuttingDown { get; set; }

        object ShutDownLock { get; set; }

        /// <summary>
        /// Returns the number of jobs that a slave is in charge of.
        /// </summary>
        int NumJobs { get; }

        #endregion // Properties
        #region Operations

        void Shutdown(IEvolutionState state);

        void Debug(string s);

        /// <summary>
        /// Constructs the worker thread for the slave and starts it
        /// </summary>
        void BuildThreads();

        /// <summary>
        /// Returns the oldest unsent job, or null if there is no unsent job.
        /// Marks the job as sent so we don't try to grab it next time.
        /// NOT SYNCHRONIZED -- YOU MUST SYNCHRONIZE ON jobs!
        /// </summary>
        IJob OldestUnsentJob();

        bool WriteLoop();

        bool ReadLoop();

        /// <summary>
        /// Adds a new jobs to the queue.  This implies that the slave 
        /// will be in charge of executing this particular job.
        /// </summary>
        void ScheduleJob(IJob job);

        /// <summary>
        /// Reschedules the jobs in this job queue to other slaves in the system.  It assumes that the slave associated
        /// with this queue has already been removed from the available slaves, such that it is not assigned its own jobs.
        /// </summary>
        /// <remarks>Only called when we're shutting down, so we're not waiting for any notification.</remarks>
        void RescheduleJobs(IEvolutionState state);

        #endregion // Operations
    }
}