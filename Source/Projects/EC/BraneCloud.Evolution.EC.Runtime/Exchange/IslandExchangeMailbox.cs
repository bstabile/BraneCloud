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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Runtime.Exchange
{
    /// <summary>
    /// Class that contains all the Mailbox functionality. It is supposed to wait on a new thread for incoming
    /// immigrants from other islands (it will receive in the constructor the number of islands that will send
    /// messages to the current island). Waiting on sockets is non-blocking, such that the order in which the
    /// islands send messages is unimportant. When immigrants are received, they are stored in a special buffer
    /// called immigrants. The storage is managed in a queue-like fashion, such that when the storage is full,
    /// the first immigrants that came are erased (hopefully the storage will be emptied fast enough such that
    /// this case doesn't appear too often).
    /// All accesses to the "immigrants" variable (also applies to NumImmigrants) should be done only in the presence
    /// of synchronization, because there might be other threads using them too. The number of immigrants for each
    /// of the subpops (NumImmigrants[x]) is between 0 and the size of the queue structure (received as a
    /// parameter in the constructor). 
    /// </summary>
    [ECConfiguration("ec.exchange.IslandExchangeMailbox")]
    public class IslandExchangeMailbox : IThreadRunnable
    {
        #region Constants

        /// <summary>
        /// How much to wait before starting checking for immigrants. 
        /// </summary>
        public const int SLEEP_BETWEEN_CHECKING_FOR_IMMIGRANTS = 1000;

        /// <summary>
        /// How much to wait on a socket for a message, before starting to wait on another socket. 
        /// </summary>
        public const int CHECK_TIMEOUT = 1000;

        /// <summary>
        /// How much to wait while synchronizing. 
        /// </summary>
        public const int SYNCHRONIZATION_SLEEP = 100;

        #endregion // Constants
        #region Fields

        // synchronization variables
        internal bool syncVar;
        internal object syncRoot = new object();

        #endregion // Fields
        #region Properties

        /// <summary>
        /// Return the port of the ServerSocket (where the islands where the other islands should
        /// connect in order to send their emigrants).
        /// </summary>
        public virtual int Port
        {
            get
            {
                // return the port of the ServerSocket
                return ((IPEndPoint)ServerSocket.LocalEndpoint).Port;
            }
        }

        /// <summary>
        /// Storage for the incoming immigrants: 2 sizes: the subpop and the index of the emigrant. 
        /// </summary>
        public Individual[][] Immigrants { get; set; }

        /// <summary>
        /// The number of immigrants in the storage for each of the subpops. 
        /// </summary>
        public int[] NumImmigrants { get; set; }

        /// <summary>
        /// Auxiliary variables to manage the queue storages.
        /// </summary>
        public int[] Person2Die { get; set; }

        /// <summary>
        /// The socket where it listens for incomming messages.
        /// </summary>
        public TcpListener ServerSocket { get; set; }

        /// <summary>
        /// The number of islands that send messages to the current Mailbox.
        /// </summary>
        public int NumIncoming { get; set; }

        /// <summary>
        /// Whether the information on sockets is compressed or not (receives this information in the constructor).
        /// </summary>
        public bool CompressedCommunication { get; set; }

        /// <summary>
        /// The sockets and readers for receiving incoming messages.
        /// </summary>
        public TcpClient[] InSockets { get; set; }

        public BinaryReader[] DataInput { get; set; }

        /// <summary>
        /// So we can print out nice names for our incoming connections.
        /// </summary>
        public string[] IncomingIds { get; set; }

        /// <summary>
        /// The state of the islands it is communicating to.
        /// </summary>
        public bool[] Running { get; set; }

        /// <summary>
        /// The state (to display messages mainly).
        /// </summary>
        public IEvolutionState State { get; set; }

        /// <summary>
        /// My ID.
        /// </summary>
        public string MyId { get; set; }

        public bool Chatty { get; set; }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// Public constructor used to initialize most of the parameters of the Mailbox:
        /// state_p : the EvolutionState, used mainly for displaying messages
        /// port : the port used to listen for incoming messages
        /// n_incoming_p : the number of islands that will send messages to the current island
        /// how_many : how many immigrants to manage in the queue-like storage for each of the subpops
        /// </summary>
        public IslandExchangeMailbox(IEvolutionState state_p, int port, int n_incoming_p, int how_many, string myId, bool chatty, bool compressedCommunication)
        {
            MyId = myId;
            CompressedCommunication = compressedCommunication;

            Chatty = chatty;

            // initialize public variables from the parameters of the constructor
            State = state_p;
            NumIncoming = n_incoming_p;

            var p_numsubpops = new Parameter(Initializer.P_POP).Push(Population.P_SIZE);
            var numsubpops = State.Parameters.GetInt(p_numsubpops, null, 1);
            if (numsubpops == 0)
            {
                // later on, Population will complain with this fatally, so don't
                // exit here, just deal with it and assume that you'll soon be shut
                // down
            }

            // allocate the storages:
            // - immigrants = storage for the immigrants that will come to the current island
            //   - first dimension: the number of subpops
            //   - second dimension: how many immigrants to store for each of the subpops.
            // - person2die = where to insert next in the queue structure "immigrants"
            // - NumImmigrants = how many immigrants there are in the storage "immigrants" for each of the subpops
            Immigrants = new Individual[numsubpops][];
            for (var i = 0; i < numsubpops; i++)
            {
                Immigrants[i] = new Individual[how_many];
            }
            Person2Die = new int[numsubpops];
            NumImmigrants = new int[numsubpops];

            // set the synchronization variable to false (it will be set to true to signal exiting the waiting loop)
            syncVar = false;

            // create the ServerSocket to listen to incoming messages
            try
            {
                var tempTcpListener = new TcpListener(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], port);
                tempTcpListener.Start();
                ServerSocket = tempTcpListener;
            }
            catch (IOException)
            {
                State.Output.Fatal("Could not start Mailbox for incoming messages.  Perhaps the port ("
                                    + port + ") is bad?\n...or someone else already has it?");
            }

            // allocate the sockets and the readers (will be used in the near future)
            InSockets = new TcpClient[NumIncoming];
            DataInput = new BinaryReader[NumIncoming];
            IncomingIds = new String[NumIncoming];

            // allocate the status of the different readers
            Running = new bool[NumIncoming];
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// The main functionality of the Mailbox: 
        /// waiting for incoming messages and dealing with the incoming immigrants 
        /// </summary>
        public virtual void Run()
        {

            // wait for the "NumIncoming" incoming connections from different islands, and initialize
            // the sockets and the readers to communicate with (receive messages from) them. All the
            // sockets are set to be non-blocking, such that they can be checked alternatively without
            // waiting for messages on a particular one.
            for (var x = 0; x < NumIncoming; x++)
            {
                try
                {
                    InSockets[x] = ServerSocket.AcceptTcpClient();

                    BinaryWriter dataOutput;

                    if (CompressedCommunication)
                    {
                        /*
                        dataInput[x] = new DataInputStream(new CompressingInputStream(inSockets[x].getInputStream()));
                        dataOutput = new DataOutputStream(new CompressingOutputStream(inSockets[x].getOutputStream()));
                        */
                        var compressedo = Output.MakeCompressingOutputStream(InSockets[x].GetStream());
                        var compressedi = Output.MakeCompressingInputStream(InSockets[x].GetStream());
                        if (compressedi == null || compressedo == null)
                            State.Output.Fatal("You do not appear to have JZLib installed on your system, and so may must have compression turned off for IslandExchange.  " + "To get JZLib, download from the ECJ website or from http://www.jcraft.com/jzlib/");

                        DataInput[x] = new BinaryReader(compressedi);
                        dataOutput = new BinaryWriter(compressedo);
                    }
                    else
                    {
                        DataInput[x] = new BinaryReader(InSockets[x].GetStream());
                        dataOutput = new BinaryWriter(InSockets[x].GetStream());
                    }

                    // send my id, then read an id
                    dataOutput.Write(MyId);
                    dataOutput.Flush();
                    IncomingIds[x] = DataInput[x].ReadString().Trim();

                    State.Output.Message("Island " + IncomingIds[x] + " connected to my Mailbox");

                    // set the socket to non-blocking
                    InSockets[x].ReceiveTimeout = CHECK_TIMEOUT;
                    Running[x] = true;
                }
                catch (IOException e)
                {
                    Running[x] = false;
                    State.Output.Fatal("An exception was generated while creating communication structures for island " + x + ".  Here it is: " + e);
                }
            }

            State.Output.Message("All islands have connected to my client.");

            // variable used for deciding (based on the synchronized variable "syncVar") when to exit
            bool shouldExit;

            // enter the main loop
            do
            {

                // wait some (do not check all the time, cause it would be a waste of time and computational resources)
                try
                {
                    Thread.Sleep(new TimeSpan((Int64)10000 * SLEEP_BETWEEN_CHECKING_FOR_IMMIGRANTS));
                }
                catch (ThreadInterruptedException)
                {
                }

                // for each of the connections established with the islands
                for (var x = 0; x < NumIncoming; x++)
                {
                    if (Running[x])
                    {
                        try
                        {
                            // enter an infinite loop to receive all the messages form the "x"s island
                            // it will exit the loop as soon as there are no more messages coming from
                            // the "x"s island (non-blocking socket)
                            while (true)
                            {
                                // read the subpop where the immigrants need to be inserted. In case there
                                // is no incoming message, an exception will be generated and the infinite loop
                                // will be exited (the Mailbox will search the next socket (communication link)
                                // for incoming messages
                                var subpop = DataInput[x].ReadInt32();

                                // if it gets to this point, it means that a number of individuals will be sent
                                // it is the time to set up the receiving storages

                                // set the socket to blocking for reading the individuals
                                try
                                {
                                    InSockets[x].ReceiveTimeout = 0;
                                }
                                catch (SocketException)
                                {
                                    State.Output.Warning("Could not set the socket to blocking while receiving individuals in the Mailbox.");
                                }

                                // how many individuals will be received in the current dialogue?
                                var how_many_to_come = DataInput[x].ReadInt32();

                                if (Chatty)
                                    State.Output.Message("Receiving " + how_many_to_come
                                        + " immigrants for subpop " + subpop + " from island " + IncomingIds[x]);

                                // synchronize on the immigrants (such that other threads cannot access it during its
                                // being modified)
                                lock (Immigrants)
                                {

                                    // in case the immigrants buffer was emptied, the person2die is not reset (it is not public)
                                    // so we have to reset it now
                                    if (NumImmigrants[subpop] == 0)
                                        // if it was reset
                                        Person2Die[subpop] = 0; // reset the person2die[x]

                                    // loop in order to receive all the incoming individuals in the current dialogue
                                    for (var ind = 0; ind < how_many_to_come; ind++)
                                    {
                                        // read the individual
                                        try
                                        {
                                            // read the emigrant in the storage
                                            Immigrants[subpop][Person2Die[subpop]] =
                                                State.Population.Subpops[subpop].Species.NewIndividual(State, DataInput[x]);

                                            //state.Output.Message( "Individual received." );

                                            // increase the queue index
                                            if (Person2Die[subpop] == Immigrants[subpop].Length - 1)
                                                Person2Die[subpop] = 0;
                                            else
                                                Person2Die[subpop]++;

                                            // can increment it without synchronization, as we do synchronization on the immigrants
                                            if (NumImmigrants[subpop] < Immigrants[subpop].Length)
                                                NumImmigrants[subpop]++;
                                        }
                                        catch (IOException)
                                        {
                                            // i hope it will also never happen :)
                                            State.Output.Message("IO exception while communicating with an island");
                                            Running[x] = false;
                                            continue;
                                        }
                                        catch (FormatException)
                                        {
                                            // it happens when the socket is closed and cannot be doing any reading
                                            State.Output.Message("IO exception while communicating with an island");
                                            Running[x] = false;
                                            continue;
                                        }
                                    }
                                } // end synchronized block on "immigrants"

                                // set the socket to non-blocking (after current set of immigrants is over)
                                try
                                {
                                    InSockets[x].ReceiveTimeout = CHECK_TIMEOUT;
                                }
                                catch (SocketException)
                                {
                                    State.Output.Warning("Could not set the socket to non-blocking while receiving individuals in the Mailbox.");
                                }
                            }
                        }
                        //catch (InterruptedIOException e) // BRS : TODO : Find the .NET exception that we want to catch here?
                        //{
                        //    // here everything is ok
                        //    // just that there were no messages
                        //}
                        catch (IOException)
                        {
                            // now this is not nice
                            // report the error so that the programmer can fix it (hopefully)
                            State.Output.Message("IO exception while communicating with an island");
                            Running[x] = false;
                        }
                        catch (FormatException) // BRS : TODO : Verify that this is the .NET exception to expect?
                        {
                            // error received when some sockets break
                            State.Output.Message("Socket closed");
                            Running[x] = false;
                        }
                    }
                }

                // again with synchronization, try to access the syncVar to check whether the Mailbox needs to finish
                // Running (maybe some other island already found the perfect individual, or the resources of the current
                // run have been wasted)
                lock (syncRoot)
                {
                    // get the value of the syncVar. If it is true, we should exit.
                    shouldExit = syncVar;
                }
            }
            while (!shouldExit);

            // close the sockets (don't care about the Running, but deal with exceptions)
            try
            {
                // close the ServerSocket
                ServerSocket.Stop();
            }
            catch (IOException)
            {
            }
            for (var x = 0; x < NumIncoming; x++)
            {
                try
                {
                    // close the sockets to communicate (receive messages) with the other islands
                    InSockets[x].Close();
                }
                catch (IOException)
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// Method used to shutdown the Mailbox. What it does is that it closes all communication links (sockets)
        /// and sets the syncVar to true (such that if the run() method is run on another thread, it will exit the
        /// loop and terminate.
        /// </summary>
        public virtual void ShutDown()
        {
            // BRS : TODO : The synchronization code is not implemented properly. Fix it!
            // set the syncVar to true (such that if another thread executes this.Run(), it will exit the main loop
            // (hopefully, the information from the server was correct
            lock (syncRoot)
            {
                syncVar = true;
            }
        }

        /// <summary>
        /// Return the port of the ServerSocket (where the islands where the other islands should
        /// connect in order to send their emigrants).
        /// </summary>
        public int GetPort()
        {
            // BRS : TODO : Make sure this is equivalent to the ECJ implementation
            // return the port of the ServerSocket
            var ep = ServerSocket.LocalEndpoint as IPEndPoint;
            if (ep == null) throw new ApplicationException("Invalid Endpoint! Expected typeof(IPEndpoint).");
            return ep.Port;
        }

        #endregion // Operations
    }
}