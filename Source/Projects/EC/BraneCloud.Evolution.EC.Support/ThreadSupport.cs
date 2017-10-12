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
using System.Threading;

namespace BraneCloud.Evolution.EC.Support
{
    /// <summary>
    /// This interface should be implemented by any class whose instances are intended 
    /// to be executed by a thread.
    /// </summary>
    public interface IThreadRunnable
    {
        /// <summary>
        /// This method has to be implemented in order that starting of the thread causes the object's 
        /// run method to be called in that separately executing thread.
        /// </summary>
        void Run();
    }

    /*******************************/
    /// <summary>
    /// Support class used to handle threads with semantics close to the original Java version.
    /// Eventually this should go away.
    /// </summary>
    public class ThreadClass : IThreadRunnable
    {
        /// <summary>
        /// The instance of System.Threading.Thread
        /// </summary>
        private Thread threadField;

        /// <summary>
        /// Initializes a new instance of the ThreadClass class
        /// </summary>
        public ThreadClass()
        {
            threadField = new Thread(Run);
        }

        /// <summary>
        /// Initializes a new instance of the Thread class.
        /// </summary>
        /// <param name="Name">The name of the thread</param>
        public ThreadClass(string Name)
        {
            threadField = new Thread(Run);
            this.Name = Name;
        }

        /// <summary>
        /// Initializes a new instance of the Thread class.
        /// </summary>
        /// <param name="start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing</param>
        public ThreadClass(ThreadStart start)
        {
            threadField = new Thread(start);
        }

        /// <summary>
        /// Initializes a new instance of the Thread class.
        /// </summary>
        /// <param name="start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing</param>
        /// <param name="name">The name of the thread</param>
        public ThreadClass(ThreadStart start, string name)
        {
            threadField = new Thread(start);
            Name = name;
        }

        /// <summary>
        /// This method has no functionality unless the method is overridden
        /// </summary>
        public virtual void Run()
        {
        }

        /// <summary>
        /// Causes the operating system to change the state of the current thread instance to ThreadState.Running
        /// </summary>
        public virtual void Start()
        {
            threadField.Start();
        }

        /// <summary>
        /// Interrupts a thread that is in the WaitSleepJoin thread state
        /// </summary>
        public virtual void Interrupt()
        {
            threadField.Interrupt();
        }

        /// <summary>
        /// Gets the current thread instance
        /// </summary>
        public Thread Instance
        {
            get
            {
                return threadField;
            }
            set
            {
                threadField = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the thread
        /// </summary>
        public string Name
        {
            get
            {
                return threadField.Name;
            }
            set
            {
                if (threadField.Name == null)
                    threadField.Name = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the scheduling priority of a thread
        /// </summary>
        public ThreadPriority Priority
        {
            get
            {
                return threadField.Priority;
            }
            set
            {
                threadField.Priority = value;
            }
        }

        /// <summary>
        /// Gets a value indicating the execution status of the current thread
        /// </summary>
        public bool IsAlive
        {
            get
            {
                return threadField.IsAlive;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not a thread is a background thread.
        /// </summary>
        public bool IsBackground
        {
            get
            {
                return threadField.IsBackground;
            }
            set
            {
                threadField.IsBackground = value;
            }
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates
        /// </summary>
        public void Join()
        {
            threadField.Join();
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates or the specified time elapses
        /// </summary>
        /// <param name="miliSeconds">Time of wait in milliseconds</param>
        public void Join(long miliSeconds)
        {
            lock (this)
            {
                threadField.Join(new TimeSpan(miliSeconds * 10000));
            }
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates or the specified time elapses
        /// </summary>
        /// <param name="miliSeconds">Time of wait in milliseconds</param>
        /// <param name="nanoSeconds">Time of wait in nanoseconds</param>
        public void Join(long miliSeconds, int nanoSeconds)
        {
            lock (this)
            {
                threadField.Join(new TimeSpan(miliSeconds * 10000 + nanoSeconds * 100));
            }
        }

        /// <summary>
        /// Resumes a thread that has been suspended
        /// </summary>
        public void Resume()
        {
#pragma warning disable 618
            // BRS: Don't worry about this obsolete call. This whole class will soon be going away.
            threadField.Resume();
#pragma warning restore 618
        }

        /// <summary>
        /// Raises a ThreadAbortException in the thread on which it is invoked, 
        /// to begin the process of terminating the thread. Calling this method 
        /// usually terminates the thread
        /// </summary>
        public void Abort()
        {
            threadField.Abort();
        }

        /// <summary>
        /// Raises a ThreadAbortException in the thread on which it is invoked, 
        /// to begin the process of terminating the thread while also providing
        /// exception information about the thread termination. 
        /// Calling this method usually terminates the thread.
        /// </summary>
        /// <param name="stateInfo">An object that contains application-specific information, such as state, which can be used by the thread being aborted</param>
        public void Abort(object stateInfo)
        {
            lock (this)
            {
                threadField.Abort(stateInfo);
            }
        }

        /// <summary>
        /// Suspends the thread, if the thread is already suspended it has no effect
        /// </summary>
        public void Suspend()
        {
#pragma warning disable 618
            // BRS: Don't worry about this obsolete call. This whole class will soon be going away.
            threadField.Suspend();
#pragma warning restore 618
        }

        /// <summary>
        /// Obtain a String that represents the current Object
        /// </summary>
        /// <returns>A String that represents the current Object</returns>
        public override string ToString()
        {
            return "Thread[" + Name + "," + Priority + "," + "" + "]";
        }

        /// <summary>
        /// Gets the currently running thread
        /// </summary>
        /// <returns>The currently running thread</returns>
        public static ThreadClass Current()
        {
            var currentThread = new ThreadClass {Instance = Thread.CurrentThread};
            return currentThread;
        }
    }
}