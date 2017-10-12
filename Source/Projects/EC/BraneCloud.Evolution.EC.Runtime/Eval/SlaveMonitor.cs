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
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using BraneCloud.Evolution.EC.Eval;
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.SteadyState;

namespace BraneCloud.Evolution.EC.Runtime.Eval
{	
    /// <summary> 
    /// SlaveMonitor
    /// 
    /// <p/>The SlaveMonitor manages slave connections to each remote slave, and provides synchronization facilities
    /// for the slave connections and for various other objects waiting to be notified when new slaves are
    /// available, space is available in a slave's job queue, an individual has been completed, etc.
    /// <p/>The monitor provides functions to create and delete slaves (registerSlave(), unregisterSlave()), 
    /// schedule a job for evaluation (scheduleJobForEvaluation(...)), block until all jobs have completed
    /// (waitForAllSlavesToFinishEvaluating(...)), test if any individual in a job has been finished
    /// (evaluatedIndividualAvailable()),  and block until an individual in a job is available and returned
    /// (waitForindividual()).
    /// 
    /// <p/>Generally speaking, the SlaveMonitor owns the SlaveConnections -- no one else
    /// should speak to them.  Also generally speaking, only MasterProblems create and speak to the SlaveMonitor.
    /// </summary>
    [ECConfiguration("ec.eval.SlaveMonitor")]
    public class SlaveMonitor : ISlaveMonitor
    {
        private class AnonymousClassRunnable : IThreadRunnable
        {
            public AnonymousClassRunnable(IEvolutionState state, ISlaveMonitor enclosingInstance, IMasterProblem problemPrototype)
            {
                InitBlock(state, enclosingInstance, problemPrototype);
            }
            private void  InitBlock(IEvolutionState state, ISlaveMonitor enclosingInstance, IMasterProblem problemPrototype)
            {
                _state = state;
                _enclosingInstance = enclosingInstance;
                _problemPrototype = problemPrototype;
            }
            private IEvolutionState _state;
            private ISlaveMonitor _enclosingInstance;
            private IMasterProblem _problemPrototype;

            private ISlaveMonitor Enclosing_Instance
            {
                get
                {
                    return _enclosingInstance;
                }
                
            }
            public virtual void  Run()
            {
                ThreadClass.Current().Name = "SlaveMonitor::    ";
                TcpClient slaveSock;
                
                while (!Enclosing_Instance.ShutdownInProgress)
                {
                    slaveSock = null;
                    while (slaveSock == null && !Enclosing_Instance.ShutdownInProgress)
                    {
                        try
                        {
                            slaveSock = Enclosing_Instance.ServSock.AcceptTcpClient();
                        }
                        catch (IOException)
                        {
                            slaveSock = null;
                        }
                    }
                    
                    Enclosing_Instance.Debug(ThreadClass.Current().Name + " Slave attempts to connect.");
                    
                    if (Enclosing_Instance.ShutdownInProgress)
                        break;
                    
                    try
                    {
                        Stream tmpIn = slaveSock.GetStream();
                        Stream tmpOut = slaveSock.GetStream();
                        if (Enclosing_Instance.UseCompression)
                        {
                            /*
                            state.Output.Fatal("JDK 1.5 has broken compression.  For now, you must set eval.compression=false");
                            tmpIn = new CompressingInputStream(tmpIn);
                            tmpOut = new CompressingOutputStream(tmpOut);
                            */
                            tmpIn = Output.MakeCompressingInputStream(tmpIn);
                            tmpOut = Output.MakeCompressingOutputStream(tmpOut);
                            if (tmpIn == null || tmpOut == null)
                            {
                                Output.InitialError(
                                    "You do not appear to have JZLib installed on your system, and so must set eval.compression=false. " +
                                    "To get JZLib, download from the ECJ website or from http://www.jcraft.com/jzlib/", false);
                                Environment.Exit(1); // This was originally part of the InitialError call in ECJ. But we make SlaveMonitor responsible.
                            }
                        }
                        
                        var dataIn = new BinaryReader(tmpIn);
                        var dataOut = new BinaryWriter(tmpOut);
                        var slaveName = dataIn.ReadString();
                        
                        dataOut.Write(Enclosing_Instance.RandomSeed);
                        Enclosing_Instance.RandomSeed += SEED_INCREMENT;
                        
                        // Write random state for eval thread to slave
                        dataOut.Flush();

                        // write out additional data as necessary
                        _problemPrototype.SendAdditionalData(_state, dataOut);
                        dataOut.Flush();

                        // write out additional data as necessary
                        _problemPrototype.SendAdditionalData(_state, dataOut);
                        dataOut.Flush();

                        Enclosing_Instance.RegisterSlave(_state, slaveName, slaveSock, dataOut, dataIn);
                        _state.Output.SystemMessage("Slave " + slaveName + " connected successfully.");
                    }
                    catch (IOException)
                    {
                    }
                }
                
                Enclosing_Instance.Debug(ThreadClass.Current().Name + " The monitor is shutting down.");
            }
        }

        #region Constants

        public const string P_EVALMASTERPORT = "eval.master.port";
        public const string P_EVALCOMPRESSION = "eval.compression";
        public const string P_MAXIMUMNUMBEROFCONCURRENTJOBSPERSLAVE = "eval.masterproblem.max-jobs-per-slave";

        /// <summary>
        /// A large value (prime for fun) bigger than expected number of threads per slave
        /// </summary>
        public const int SEED_INCREMENT = 7919;

        #endregion // Constants
        #region Fields

        /// <summary>
        /// The slaves (not really a queue).
        /// </summary>
        private readonly ArrayList _allSlaves = new ArrayList();

        /// <summary>
        /// The available slaves.
        /// </summary>
        private readonly ArrayList _availableSlaves = new ArrayList();

        internal object[] ShutdownInProgressLock = new Object[0]; // arrays are serializable

        #endregion // Fields
        #region Properties

        public IEvolutionState State { get; set; }

        public IMasterProblem ProblemPrototype { get; set; }

        /// <summary>  
        /// The socket where slaves connect.
        /// </summary>
        public TcpListener ServSock { get; set; }

        /// <summary> 
        /// Indicates whether compression is used over the socket IO streams.
        /// </summary>
        public bool UseCompression { get; set; }

        public int RandomSeed { get; set; }
        public ThreadClass Thread { get; set; }

        /// <summary>
        /// The maximum number of jobs per slave
        /// </summary>
        public int MaxJobsPerSlave { get; set; }

        /// <summary>
        /// Whether the system should display information that is useful for debugging 
        /// </summary>
        public bool ShowDebugInfo { get; set; }

        public List<QueueIndividual> EvaluatedIndividuals
        {
            get { return _evaluatedIndividuals; }
            set { _evaluatedIndividuals = value; }
        }
        private List<QueueIndividual> _evaluatedIndividuals = new List<QueueIndividual>();

        /// <summary>
        /// Returns the number of available slave (not busy) 
        /// </summary>
        public virtual int NumAvailableSlaves
        {
            get
            {
                var i = 0;
                lock (_availableSlaves.SyncRoot)
                {
                    i = _availableSlaves.Count;
                }
                return i;
            }
        }

        public virtual bool EvaluatedIndividualAvailable
        {
            get
            {
                lock (_evaluatedIndividuals)
                {
                    try
                    {
                        var generatedAux = _evaluatedIndividuals[0];
                        return true;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        return false;
                    }
                }
            }
        }

        public virtual bool ShutdownInProgress
        {
            get
            {
                lock (ShutdownInProgressLock)
                {
                    return _shutdownInProgress;
                }
            }

            set
            {
                lock (ShutdownInProgressLock)
                {
                    _shutdownInProgress = value;
                }
            }

        }
        private bool _shutdownInProgress;

        #endregion // Properties
        #region Setup

        /// <summary>
        /// Simple constructor that initializes the data structures for keeping track of the state of each slave.
        /// The constructor receives two parameters: a boolean flag indicating whether the system should display
        /// information that is useful for debugging, and the maximum load per slave (the maximum number of jobs
        /// that a slave can be entrusted with at each time).
        /// </summary>
        public SlaveMonitor(IEvolutionState state, bool showDebugInfo, IMasterProblem problemPrototype)
        {
            ShowDebugInfo = showDebugInfo;
            State = state;
            ProblemPrototype = problemPrototype;

            var port = state.Parameters.GetInt(new Parameter(P_EVALMASTERPORT), null);

            MaxJobsPerSlave = state.Parameters.GetInt(new Parameter(P_MAXIMUMNUMBEROFCONCURRENTJOBSPERSLAVE), null);

            UseCompression = state.Parameters.GetBoolean(new Parameter(P_EVALCOMPRESSION), null, false);

            try
            {
                var tempTcpListener = new TcpListener(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], port);
                tempTcpListener.Start();
                ServSock = tempTcpListener;
            }
            catch (IOException e)
            {
                state.Output.Fatal("Unable to bind to port " + port + ": " + e);
            }

            RandomSeed = (int)((DateTime.Now.Ticks - 621355968000000000) / 10000);

            // spawn the thread
            Thread = new ThreadClass(new ThreadStart(new AnonymousClassRunnable(state, this, problemPrototype).Run));
            Thread.Start();
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Shuts down the slave monitor (also shuts down all slaves).
        /// </summary>
        public virtual void Shutdown()
        {
            // kill the socket socket and bring down the thread
            ShutdownInProgress = true;
            try
            {
                ServSock.Stop();
            }
            catch (IOException)
            {
            }
            Thread.Interrupt();
            try
            {
                Thread.Join();
            }
            catch (ThreadInterruptedException)
            {
            }

            // gather all the slaves

            lock (_allSlaves.SyncRoot)
            {
                while (!(_allSlaves.Count == 0))
                {
                    var tempObject = _allSlaves[0];
                    _allSlaves.RemoveAt(0);
                    ((SlaveConnection)(tempObject)).Shutdown(State);
                }
                NotifyMonitor(_allSlaves);
            }
        }

        public void Debug(string s)
        {
            if (ShowDebugInfo)
            {
                Console.Error.WriteLine(ThreadClass.Current().Name + "->" + s);
            }
        }

        #region Registration

        /// <summary>
        /// Registers a new slave with the monitor.  Upon registration, a slave is marked as available for jobs.
        /// </summary>
        public virtual void RegisterSlave(IEvolutionState state, string name, TcpClient socket, BinaryWriter writer, BinaryReader reader)
        {
            var newSlave = new SlaveConnection(state, name, socket, writer, reader, this);

            lock (_availableSlaves.SyncRoot)
            {
                _availableSlaves.Insert(_availableSlaves.Count, newSlave);
                NotifyMonitor(_availableSlaves);
            }
            lock (_allSlaves.SyncRoot)
            {
                _allSlaves.Insert(_allSlaves.Count, newSlave);
                NotifyMonitor(_allSlaves);
            }
        }

        /// <summary>
        /// Unregisters a dead slave from the monitor.
        /// </summary>
        public virtual void UnregisterSlave(ISlaveConnection slave)
        {
            lock (_allSlaves.SyncRoot)
            {
                _allSlaves.Remove(slave);
                NotifyMonitor(_allSlaves);
            }
            lock (_availableSlaves.SyncRoot)
            {
                _availableSlaves.Remove(slave);
                NotifyMonitor(_availableSlaves);
            }
        }

        #endregion // Registration

        /// <summary>
        /// Schedules a job for execution on one of the available slaves.  
        /// The monitor waits until at least one slave is available to perform the job.
        /// </summary>
        public virtual void ScheduleJobForEvaluation(IEvolutionState state, IJob job)
        {
            if (ShutdownInProgress)
                return; // no more jobs allowed.  This line rejects requests from slaveConnections when THEY'RE shutting down.

            ISlaveConnection result;
            lock (_availableSlaves.SyncRoot)
            {
                while (true)
                {
                    if (!(_availableSlaves.Count == 0))
                    {
                        var tempObject = _availableSlaves[0];
                        _availableSlaves.RemoveAt(0);
                        result = (ISlaveConnection)(tempObject);
                        break;
                    }
                    Debug("Waiting for an available slave.");
                    WaitOnMonitor(_availableSlaves);
                }
                NotifyMonitor(_availableSlaves);
            }
            Debug("Got a slave available for work.");

            result.ScheduleJob(job);

            if (result.NumJobs < MaxJobsPerSlave)
            {
                lock (_availableSlaves.SyncRoot)
                {
                    if (!_availableSlaves.Contains(result))
                        _availableSlaves.Insert(_availableSlaves.Count, result); // so we're round-robin
                    NotifyMonitor(_availableSlaves);
                }
            }
        }

        /// <summary>
        /// This method returns only when all slaves have finished the jobs that they were assigned.  While this method waits,
        /// new jobs can be assigned to the slaves.  This method is usually invoked from MasterProblem.finishEvaluating.  You
        /// should not abuse using this method: if there are two evaluation threads, where one of them waits until all jobs are
        /// finished, while the second evaluation thread keeps posting jobs to the slaves, the first thread might have to wait
        /// until the second thread has had all its jobs finished.
        /// </summary>
        public virtual void WaitForAllSlavesToFinishEvaluating(IEvolutionState state)
        {
            lock (_allSlaves.SyncRoot)
            {
                var iter = _allSlaves.GetEnumerator();
                while (iter.MoveNext())
                {
                    var slaveConnection = (ISlaveConnection)(iter.Current);
                    try
                    {
                        slaveConnection.DataOut.Flush();
                    }
                    catch (IOException)
                    {
                    } // we'll catch this error later....
                }
                NotifyMonitor(_allSlaves);
            }

            var shouldCycle = true;
            lock (_allSlaves.SyncRoot)
            {
                while (shouldCycle)
                {
                    shouldCycle = false;
                    var iter = _allSlaves.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        var slaveConnection = (ISlaveConnection)(iter.Current);
                        var jobs = slaveConnection.NumJobs;
                        if (jobs != 0)
                        {
                            Debug("Slave " + slaveConnection + " has " + jobs + " more jobs to finish.");
                            shouldCycle = true;
                            break;
                        }
                    }
                    if (shouldCycle)
                    {
                        Debug("Waiting for slaves to finish their jobs.");
                        WaitOnMonitor(_allSlaves);
                        Debug("At least one job has been finished.");
                    }
                }
                NotifyMonitor(_allSlaves);
            }
            Debug("All slaves have finished their jobs.");
        }

        /// <summary>
        /// Notifies the monitor that the particular slave has finished performing a job, 
        /// and it (probably) is available for other jobs.
        /// </summary>
        public virtual void NotifySlaveAvailability(ISlaveConnection slave, IJob job, IEvolutionState state)
        {
            // first announce that a slave in allSlaves has finished, so people blocked on waitForAllSlavesToFinishEvaluating
            // can wake up and realize it.

            lock (_allSlaves.SyncRoot)
            {
                NotifyMonitor(_allSlaves);
            }

            // now announce that we've got a new available slave if someone wants it

            if (slave.NumJobs < MaxJobsPerSlave)
            {
                lock (_availableSlaves.SyncRoot)
                {
                    if (!_availableSlaves.Contains(slave))
                        _availableSlaves.Insert(_availableSlaves.Count, slave);
                    NotifyMonitor(_availableSlaves);
                }
            }

            Debug("Notify the monitor that the slave is available.");

            // now announce that we've got a new completed individual if someone is waiting for it

            if (state is SteadyStateEvolutionState)
            {
                // Perhaps we should the individuals by fitness first, so the fitter ones show up later
                // and don't get immediately wiped out by less fit ones.  Or should it be the other way
                // around?  We might revisit that in the future.

                // At any rate, add ALL the individuals that came back to the evaluatedIndividuals LinkedList
                lock (_evaluatedIndividuals)
                {
                    for (var x = 0; x < job.Inds.Length; x++)
                        _evaluatedIndividuals.Insert(_evaluatedIndividuals.Count, new QueueIndividual(job.Inds[x], job.Subpops[x]));
                    NotifyMonitor(_evaluatedIndividuals);
                }
            }
        }

        /// <summary>
        /// Blocks until an individual comes available 
        /// </summary>
        public virtual QueueIndividual WaitForIndividual()
        {
            while (true)
            {
                lock (_evaluatedIndividuals)
                {
                    if (EvaluatedIndividualAvailable)
                    {
                        var tempObject = _evaluatedIndividuals[0];
                        _evaluatedIndividuals.RemoveAt(0);
                        return tempObject;
                    }

                    Debug("Waiting for individual to be evaluated.");
                    WaitOnMonitor(_evaluatedIndividuals); // lets go of evaluatedIndividuals loc
                    Debug("At least one individual has been finished.");
                }
            }
        }

        public virtual bool WaitOnMonitor(object monitor)
        {
            try
            {
                Monitor.Wait(monitor);
            }
            catch (ThreadInterruptedException)
            {
                return false;
            }
            return true;
        }

        public virtual void NotifyMonitor(object monitor)
        {
            Monitor.PulseAll(monitor);
        }

        #endregion // Operations
        #region IO

        private void WriteObject(BinaryWriter writer)
        {
            State.Output.Fatal("Not implemented yet: SlaveMonitor.WriteObject");
        }

        private void ReadObject(BinaryReader reader)
        {
            State.Output.Fatal("Not implemented yet: SlaveMonitor.ReadObject");
        }

        #endregion // IO
    }
}