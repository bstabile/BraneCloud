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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using BraneCloud.Evolution.EC.SteadyState;
using BraneCloud.Evolution.EC.Support;

namespace BraneCloud.Evolution.EC.Eval
{
    public interface ISlaveMonitor
    {
        #region Properties

        IEvolutionState State { get; set; }

        /// <summary>  
        /// The socket where slaves connect.
        /// </summary>
        TcpListener ServSock { get; set; }

        /// <summary> 
        /// Indicates whether compression is used over the socket IO streams.
        /// </summary>
        bool UseCompression { get; set; }

        int RandomSeed { get; set; }
        ThreadClass Thread { get; set; }

        /// <summary>
        /// The maximum number of jobs per slave
        /// </summary>
        int MaxJobsPerSlave { get; set; }

        /// <summary>
        /// Whether the system should display information that is useful for debugging 
        /// </summary>
        bool ShowDebugInfo { get; set; }

        List<QueueIndividual> EvaluatedIndividuals { get; set; }

        /// <summary>
        /// Returns the number of available slave (not busy) 
        /// </summary>
        int NumAvailableSlaves { get; }

        bool EvaluatedIndividualAvailable { get; }

        bool ShutdownInProgress { get; set; }

        #endregion // Properties
        #region Operations

        /// <summary>
        /// Shuts down the slave monitor (also shuts down all slaves).
        /// </summary>
        void Shutdown();

        void Debug(string s);

        #region Registration

        /// <summary>
        /// Registers a new slave with the monitor.  Upon registration, a slave is marked as available for jobs.
        /// </summary>
        void RegisterSlave(IEvolutionState state, string name, TcpClient socket, BinaryWriter writer,
                           BinaryReader reader);

        /// <summary>
        /// Unregisters a dead slave from the monitor.
        /// </summary>
        void UnregisterSlave(ISlaveConnection slave);

        #endregion // Registration

        /// <summary>
        /// Schedules a job for execution on one of the available slaves.  
        /// The monitor waits until at least one slave is available to perform the job.
        /// </summary>
        void ScheduleJobForEvaluation(IEvolutionState state, IJob job);

        /// <summary>
        /// This method returns only when all slaves have finished the jobs that they were assigned.  While this method waits,
        /// new jobs can be assigned to the slaves.  This method is usually invoked from MasterProblem.finishEvaluating.  You
        /// should not abuse using this method: if there are two evaluation threads, where one of them waits until all jobs are
        /// finished, while the second evaluation thread keeps posting jobs to the slaves, the first thread might have to wait
        /// until the second thread has had all its jobs finished.
        /// </summary>
        void WaitForAllSlavesToFinishEvaluating(IEvolutionState state);

        /// <summary>
        /// Notifies the monitor that the particular slave has finished performing a job, 
        /// and it (probably) is available for other jobs.
        /// </summary>
        void NotifySlaveAvailability(ISlaveConnection slave, IJob job, IEvolutionState state);

        /// <summary>
        /// Blocks until an individual comes available 
        /// </summary>
        QueueIndividual WaitForIndividual();

        bool WaitOnMonitor(object monitor);

        void NotifyMonitor(object monitor);

        #endregion // Operations
    }
}