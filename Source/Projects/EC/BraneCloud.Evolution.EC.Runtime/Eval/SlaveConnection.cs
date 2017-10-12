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
using System.Net.Sockets;
using System.Collections;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Eval;
using BraneCloud.Evolution.EC.Support;

namespace BraneCloud.Evolution.EC.Runtime.Eval
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
    [ECConfiguration("ec.eval.SlaveConnection")]
    public class SlaveConnection : ISlaveConnection
    {
        private class AnonymousClassThread : ThreadClass
        {
            public AnonymousClassThread(SlaveConnection enclosingInstance)
            {
                InitBlock(enclosingInstance);
            }
            private void  InitBlock(SlaveConnection enclosingInstance)
            {
                Enclosing_Instance = enclosingInstance;
            }

            private SlaveConnection Enclosing_Instance { get; set; }

            override public void Run()
            {
                while (Enclosing_Instance.ReadLoop()) {;}
            }
        }
        private class AnonymousClassThread1 : ThreadClass
        {
            public AnonymousClassThread1(SlaveConnection enclosingInstance)
            {
                InitBlock(enclosingInstance);
            }
            private void InitBlock(SlaveConnection enclosingInstance)
            {
                Enclosing_Instance = enclosingInstance;
            }

            private SlaveConnection Enclosing_Instance { get; set; }

            override public void  Run()
            {
                while (Enclosing_Instance.WriteLoop()) {;}
            }
        }

        #region Fields

        /// <summary>
        /// Given that we expect the slave to return the evaluated individuals in the exact same order,
        /// the jobs need to be represented as a queue.
        /// </summary>
        private readonly List<IJob> _jobs = new List<IJob>();

        #endregion // Fields
        #region Properties

        /// <summary>
        /// Name of the slave process. 
        /// </summary>
        public string SlaveName { get; set; }

        /// <summary>
        /// Socket for communication with the slave process. 
        /// </summary>
        public TcpClient EvalSocket { get; set; }

        /// <summary>
        /// Used to transmit data to the slave. 
        /// </summary>
        public BinaryWriter DataOut { get; set; }

        /// <summary>
        /// Used to read results and randoms state from slave. 
        /// </summary>
        public BinaryReader DataIn { get; set; }

        /// <summary>
        /// A pointer to the evolution state.
        /// </summary>
        public IEvolutionState State { get; set; }

        /// <summary>
        /// A pointer to the monitor.
        /// </summary>
        public ISlaveMonitor SlaveMonitor { get; set; }

        /// <summary>
        /// A pointer to the worker thread that is working for this slave.
        /// </summary>
        public ThreadClass Reader { get; set; }

        public ThreadClass Writer { get; set; }

        public bool ShowDebugInfo { get; set; }

        /// <summary>
        /// This method is called whenever there are any communication problems with the slave
        /// (indicating possibly that the slave might have crashed).  In this case, the jobs will
        /// be rescheduled for evaluation on other slaves.
        /// </summary>
        public bool ShuttingDown { get; set; }

        public object ShutDownLock
        {
            get { return _shutDownLock; }
            set { _shutDownLock = value; }
        }
        private object _shutDownLock = new int[0]; // serializable and lockable

        /// <summary>
        /// Returns the number of jobs that a slave is in charge of.
        /// </summary>
        public virtual int NumJobs
        {
            get
            {
                lock (_jobs)
                {
                    return _jobs.Count;
                }
            }
        }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// The constructor also creates the queue storing the jobs that the slave
        /// has been asked to evaluate.  It also creates and launches the worker
        /// thread that is communicating with the remote slave to read back the results
        /// of the evaluations.
        /// </summary>
        public SlaveConnection(IEvolutionState state, string slaveName, TcpClient evalSocket,
                                BinaryWriter dataOut, BinaryReader dataIn, ISlaveMonitor slaveMonitor)
        {
            SlaveName = slaveName;
            EvalSocket = evalSocket;
            DataOut = dataOut;
            DataIn = dataIn;
            State = state;
            SlaveMonitor = slaveMonitor;
            BuildThreads();
            ShowDebugInfo = slaveMonitor.ShowDebugInfo;
        }

        #endregion // Setup
        #region Operations

        public virtual void Shutdown(IEvolutionState state)
        {
            // prevent me from hitting this multiple times
            lock (_shutDownLock)
            {
                if (ShuttingDown)
                    return;

                ShuttingDown = true;
            }

            // don't want to miss any of these so we'll wrap them individually
            try
            {
                // BRS: TODO : Not sure if shutdown status was intended here?
                DataOut.Write((byte)SlaveEvaluationType.Shutdown); 
            }
            catch (Exception) { } // exception, not IOException, because JZLib throws some array exceptions
            try { DataOut.Flush(); }
            catch (Exception) { }
            try { DataOut.Close(); }
            catch (Exception) { }
            try { DataIn.Close(); }
            catch (Exception) { }
            try { EvalSocket.Close(); }
            catch (IOException) { }

            State.Output.SystemMessage(ToString() + " Slave is shutting down....");
            SlaveMonitor.UnregisterSlave(this); // unregister me BEFORE I reschedule my jobs
            RescheduleJobs(State);
            lock (_jobs)
            {
                // notify my threads now that I've closed stuff in case they're still waiting
                SlaveMonitor.NotifyMonitor(_jobs);
                Reader.Interrupt(); // not important right now but...
                Writer.Interrupt(); // very important that we be INSIDE the jobs synchronization here so the writer doesn't try to wait on the monitor again.
            }
            State.Output.SystemMessage(ToString() + " Slave exits....");
        }

        public void Debug(string s)
        {
            if (ShowDebugInfo)
            {
                Console.Error.WriteLine(ThreadClass.Current().Name + "->" + s);
            }
        }

        /// <summary>
        /// Constructs the worker thread for the slave and starts it
        /// </summary>
        public virtual void BuildThreads()
        {
            Reader = new AnonymousClassThread(this);
            Writer = new AnonymousClassThread1(this);
            Writer.Start();
            Reader.Start();
        }

        /// <summary>
        /// Returns the oldest unsent job, or null if there is no unsent job.
        /// Marks the job as sent so we don't try to grab it next time.
        /// NOT SYNCHRONIZED -- YOU MUST SYNCHRONIZE ON jobs!
        /// </summary>
        public virtual IJob OldestUnsentJob()
        {
            // jobs are loaded into the queue from the back and go to the front.
            // so the oldest jobs are in the front and we should search starting
            // at the front.  List iterators go from front to back, so we can iterate
            // starting with the oldest.

            // This all could have been O(1) if we had used two queues, but we're being
            // intentionally lazy to keep this from getting to complex.
            var i = _jobs.GetEnumerator();
            while (i.MoveNext())
            {
                var job = (IJob)(i.Current);
                if (!job.Sent)
                {
                    job.Sent = true;
                    return job;
                }
            }
            return null;
        }

        public virtual bool WriteLoop()
        {
            IJob job;

            try
            {
                lock (_jobs)
                {
                    // check for an unsent job
                    if ((job = OldestUnsentJob()) == null)
                    // automatically marks as sent
                    {
                        // failed -- wait and drop out of the loop and come in again
                        Debug("" + ThreadClass.Current().Name + "Waiting for a job to send");
                        SlaveMonitor.WaitOnMonitor(_jobs);
                    }
                }
                if (job != null)
                // we got a job inside our synchronized wait
                {
                    // send the job
                    Debug("" + ThreadClass.Current().Name + "Sending Job");
                    if (job.Type == SlaveEvaluationType.Simple)
                    {
                        // Tell the server we're evaluating a SimpleProblemForm
                        DataOut.Write((byte)SlaveEvaluationType.Simple);
                    }
                    else
                    {
                        // Tell the server we're evaluating a IGroupedProblem
                        DataOut.Write((byte)SlaveEvaluationType.Grouped);

                        // Tell the server whether to count victories only or not.
                        DataOut.Write(job.CountVictoriesOnly);
                    }

                    // transmit number of individuals 
                    DataOut.Write(job.Inds.Length);

                    // Transmit the subpops to the slave 
                    foreach (var t in job.Subpops)
                        DataOut.Write(t);

                    Debug("Starting to transmit individuals");

                    // Transmit the individuals to the server for evaluation...
                    for (var i = 0; i < job.Inds.Length; i++)
                    {
                        job.Inds[i].WriteIndividual(State, DataOut);
                        DataOut.Write(job.UpdateFitness[i]);
                    }
                    DataOut.Flush();
                }
            }
            catch (Exception)
            {
                Shutdown(State); return false;
            }
            return true;
        }

        public virtual bool ReadLoop()
        {
            IJob job;

            try
            {
                // block on an incoming job
                var val = (sbyte)DataIn.ReadByte();
                Debug(ToString() + " Incoming Job");

                // okay, we've got a job.  Grab the earliest job, that's what's coming in

                lock (_jobs)
                {
                    job = _jobs[0]; // NO SUCH ELEMENT EXCEPTION
                }
                Debug("Got job: " + job);


                ///// NEXT STEP: COPY THE INDIVIDUALS FORWARD INTO NEWINDS.
                ///// WE DO THIS SO WE CAN LOAD THE INDIVIDUALS BACK INTO NEWINDS
                ///// AND THEN COPY THEM BACK INTO THE ORIGINAL INDS, BECAUSE ECJ
                ///// DOESN'T HAVE A COPY(INDIVIDUAL,INTO_INDIVIDUAL) FUNCTION

                job.CopyIndividualsForward();

                // now start reading.  Remember that we've already got a byte.

                for (var i = 0; i < job.NewInds.Length; i++)
                {
                    Debug(ToString() + " Individual# " + i);
                    Debug(ToString() + " Reading Byte");
                    if (i > 0)
                        val = (sbyte)DataIn.ReadByte(); // otherwise we've got it already
                    Debug(ToString() + " Reading Individual");
                    if (val == (sbyte)SlaveReturnType.Individual)
                    {
                        job.NewInds[i].ReadIndividual(State, DataIn);
                    }
                    else if (val == (sbyte)SlaveReturnType.Fitness)
                    {
                        job.NewInds[i].Evaluated = DataIn.ReadBoolean();
                        job.NewInds[i].Fitness.ReadFitness(State, DataIn);
                    }
                    else if (val == (sbyte)SlaveReturnType.Nothing)
                    {
                        // do nothing
                    }
                    Debug(ToString() + " Read Individual");
                }


                ///// NEXT STEP: COPY THE NEWLY-READ INDIVIDUALS BACK INTO THE ORIGINAL
                ///// INDIVIDUALS.  THIS IS QUITE A HACK, IF YOU READ JOB.JAVA

                // Now we have all the individuals in so we're good.  Copy them back into the original individuals
                job.CopyIndividualsBack(State);


                ///// LAST STEP: LET OTHERS KNOW WE'RE DONE AND AVAILABLE FOR ANOTHER JOB

                // we're all done!  Yank the job from the queue so others think we're available
                lock (_jobs)
                {
                    _jobs.RemoveAt(0);
                }

                // And let the slave monitor we just finished a job
                SlaveMonitor.NotifySlaveAvailability(this, job, State);
            }
            catch (IOException)
            {
                Shutdown(State); // will redistribute jobs
                return false;
            }

            return true;
        }

        /// <summary>
        /// Adds a new jobs to the queue.  This implies that the slave 
        /// will be in charge of executing this particular job.
        /// </summary>
        public virtual void ScheduleJob(IJob job)
        {
            lock (_jobs)
            {
                if (job.Sent)
                    // just in case
                    State.Output.Fatal("Tried to reschedule an existing job");
                _jobs.Insert(_jobs.Count, job);
                SlaveMonitor.NotifyMonitor(_jobs);
            }
        }

        /// <summary>
        /// Reschedules the jobs in this job queue to other slaves in the system.  It assumes that the slave associated
        /// with this queue has already been removed from the available slaves, such that it is not assigned its own jobs.
        /// </summary>
        /// <remarks>Only called when we're shutting down, so we're not waiting for any notification.</remarks>
        public virtual void RescheduleJobs(IEvolutionState state)
        {
            while (true)
            {
                IJob job;
                lock (_jobs)
                {
                    if ((_jobs.Count == 0))
                    {
                        return;
                    }
                    var tempObject = _jobs[0];
                    _jobs.RemoveAt(0);
                    job = tempObject;
                }
                Debug(ThreadClass.Current().Name + " Waiting for a slave to reschedule the evaluation.");
                job.Sent = false; // reuse
                SlaveMonitor.ScheduleJobForEvaluation(State, job);
                Debug(ThreadClass.Current().Name + " Got a slave to reschedule the evaluation.");
            }
        }

        #endregion // Operations
        #region ToString

        public override string ToString()
        {
            return "Slave(" + SlaveName + ")";
        }

        #endregion // ToString
    }
}