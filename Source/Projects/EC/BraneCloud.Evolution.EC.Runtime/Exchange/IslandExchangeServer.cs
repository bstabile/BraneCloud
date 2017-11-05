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
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Runtime.Exchange
{
    /// <summary>
    /// The IslandExchangeServer is the class that manages the main server that coordinates all the islands. 
    /// The class implements Runnable (for Running on a different thread).
    /// </summary>
    [ECConfiguration("ec.exchange.IslandExchangeServer")]
    public class IslandExchangeServer : IThreadRunnable
    {
        #region Constants

        /*		
        The server-specific parameters look roughly like this:
        
        exch.server-port = 8021
        exch.num-islands = 3
        exch.island.0.id = SurvivorIsland
        exch.island.0.num-mig = 1
        exch.island.0.mig.0 = GilligansIsland
        exch.island.0.size = 5
        exch.island.0.mod = 2
        exch.island.0.start = 4
        exch.island.1.id = GilligansIsland
        exch.island.1.mod = 1
        exch.island.1.start = 2
        exch.island.1.size = 10
        exch.island.1.num-mig = 2
        exch.island.1.mig.0 = SurvivorIsland
        exch.island.1.mig.1 = GilligansIsland
        exch.island.2.id = BermudaIsland
        exch.island.2.mod = 2
        ...
        */

        //// Server information

        /// <summary>
        /// The server port. 
        /// </summary>
        public const string P_SERVER_PORT = "server-port";

        /// <summary>
        /// The number of islands. 
        /// </summary>
        public const string P_NUM_ISLANDS = "num-islands";

        /// <summary>
        /// The parameter for the island's information. 
        /// </summary>
        public const string P_ISLAND = "island";

        /// <summary>
        /// The id. 
        /// </summary>
        public const string P_ID = "id";

        /// <summary>
        /// The number of islands that will send immigrants to the current island.
        /// </summary>
        public const string P_NUM_INCOMING_MIGRATING_COUNTRIES = "num-incoming-mig";

        /// <summary>
        /// The number of islands where immigrants will be sent. 
        /// </summary>
        public const string P_NUM_MIGRATING_COUNTRIES = "num-mig";

        /// <summary>
        /// The parameter for migrating islands' ids. 
        /// </summary>
        public const string P_MIGRATING_ISLAND = "mig";

        /// <summary>
        /// The size of the Mailbox (for each of the subpops). 
        /// </summary>
        public const string P_MAILBOX_CAPACITY = "mailbox-capacity";

        /// <summary>
        /// The parameter for the Modulo (how many generations should pass between consecutive sendings of individuals. 
        /// </summary>
        public const string P_MODULO = "mod";

        /// <summary>
        /// The number of immigrants to be sent. 
        /// </summary>
        public const string P_SIZE = "size";

        /// <summary>
        /// How many generations to pass at the beginning of the evolution before the first emigration from the current island. 
        /// </summary>
        public const string P_OFFSET = "start";

        /// <summary>
        /// Whether the execution should be Synchronous or aSynchronous. 
        /// </summary>
        public const string P_SYNCHRONOUS = "sync";

        /// <summary>
        /// The run message to be sent to the clients. 
        /// </summary>
        public const string RUN = "run";

        /// <summary>
        /// How much to wait for the found message (on a non-blocking socket). 
        /// </summary>
        public const int FOUND_TIMEOUT = 100;

        /// <summary>
        /// How much to sleep between checking for a FOUND message. 
        /// </summary>
        public const int SLEEP_TIME = 100;

        /// <summary>
        /// The final message to be sent to all islands when an individual has been found. 
        /// </summary>
        public const string GOODBYE = "bye-bye";

        #endregion // Constants
        #region Properties

        /// <summary>
        /// The number of islands in the topology.
        /// </summary>
        public int NumIslands { get; set; }

        /// <summary>
        /// The port of the server.
        /// </summary>
        public int ServerPort { get; set; }

        /// <summary>
        /// The server's socket.
        /// </summary>
        public TcpListener ServerSocket { get; set; }

        /// <summary>
        /// Hashtable for faster lookup of information regarding islands.
        /// </summary>
        public Hashtable Info { get; set; }

        /// <summary>
        ///  Hashtable to count how many islands send individuals to each of the islands.
        /// </summary>
        public Hashtable InfoImmigrants { get; set; }

        public IEvolutionState State { get; set; }

        /// <summary>
        /// Index of island ids sorted by parameter file.
        /// </summary>
        public string[] IslandIds { get; set; }

        /// <summary>
        /// Index of island ids sorted by order of connection.
        /// </summary>
        public string[] ConnectedIslandIds { get; set; }


        // variables used if the execution is Synchronous
        public int GlobalModulo { get; set; }
        public int GlobalOffset { get; set; }
        public bool Synchronous { get; set; }

        /// <summary>
        /// How many individuals asked to be synchronized (when it reaches the total number of
        /// Running clients, the server resets this variable and allows everybody to continue Running).
        /// </summary>
        public bool[] WhoIsSynchronized { get; set; }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// This Setup should get called from the IslandExchange Setup method. 
        /// </summary>
        public virtual void SetupServerFromDatabase(IEvolutionState state, IParameter paramBase)
        {

            // Store the evolution state for further use in other functions ( ie. run )
            State = state;

            // Don't bother with getting the default base -- we're a singleton!

            // get the number of islands
            var p = paramBase.Push(P_NUM_ISLANDS);
            NumIslands = State.Parameters.GetInt(p, null, 1);
            if (NumIslands == 0)
                State.Output.Fatal("The number of islands must be >0.", p);

            // get the port of the server
            p = paramBase.Push(P_SERVER_PORT);
            ServerPort = State.Parameters.GetInt(p, null, 1);
            if (ServerPort == 0)
                State.Output.Fatal("The server port must be >0.", p);

            // information on the islands = hashtable of ID and socket information
            Info = Hashtable.Synchronized(new Hashtable(NumIslands));

            // initialize the hash table to count how many islands send individuals
            // to each of the islands
            InfoImmigrants = Hashtable.Synchronized(new Hashtable(NumIslands));

            // allocate the ids sorted by parameter file
            IslandIds = new String[NumIslands];

            // allocate the ids sorted by connection
            ConnectedIslandIds = new String[NumIslands];

            // check whether the execution is Synchronous or aSynchronous
            // if it is Synchronous, there should be two parameters in the parameters file:
            // the global Modulo and Offset (such that the islands coordinate smoothly)
            p = paramBase.Push(P_SYNCHRONOUS);

            // get the value of the Synchronous parameter (default is false)
            Synchronous = State.Parameters.GetBoolean(p, null, false);

            // if Synchronous, read the other two global parameters
            if (Synchronous)
            {
                State.Output.Message("The communication will be Synchronous.");

                //// get the global Modulo
                //p = paramBase.Push(P_MODULO);
                //global_Modulo = state.Parameters.GetInt(p, null, 1);
                //if (global_Modulo == 0)
                //    state.Output.Fatal("Parameter not found, or it has an incorrect value.", p);

                //// get the global Offset
                //p = paramBase.Push(P_OFFSET);
                //global_Offset = state.Parameters.GetInt(p, null, 0);
                //if (global_Offset == -1)
                //    state.Output.Fatal("Parameter not found, or it has an incorrect value.", p);
            }
            else
            {

                State.Output.Message("The communication will be aSynchronous.");
            }

            // get a new local base
            var islandBase = paramBase.Push(P_ISLAND);

            // load the island topology
            for (var x = 0; x < NumIslands; x++)
            {

                var ieii = new IslandExchangeIslandInfo();

                var localBase = islandBase.Push("" + x);

                // get the id of the current island
                p = localBase.Push(P_ID);
                IslandIds[x] = State.Parameters.GetStringWithDefault(p, null, "");
                if (IslandIds[x].Equals(""))
                    State.Output.Fatal("Parameter not found.", p);

                // get the Mailbox capacity of the imigration from the current island
                p = localBase.Push(P_MAILBOX_CAPACITY);
                ieii.MailboxCapacity = State.Parameters.GetInt(p, paramBase.Push(P_MAILBOX_CAPACITY), 0);
                if (ieii.MailboxCapacity == -1)
                    State.Output.Fatal("Parameter not found, or it has an incorrect value.", p, paramBase.Push(P_MAILBOX_CAPACITY));

                // get the size of the imigration from the current island
                p = localBase.Push(P_SIZE);
                ieii.Size = State.Parameters.GetInt(p, paramBase.Push(P_SIZE), 0);
                if (ieii.Size == -1)
                    State.Output.Fatal("Parameter not found, or it has an incorrect value.", p, paramBase.Push(P_SIZE));

                //// if Synchronous execution, use the global Modulo and Offset
                //if (Synchronous)
                //{
                //    ieii.Modulo = global_Modulo;
                //    ieii.Offset = global_Offset;
                //}
                //else
                //{
                // get the Modulo of the imigration from the current island
                p = localBase.Push(P_MODULO);
                ieii.Modulo = State.Parameters.GetInt(p, paramBase.Push(P_MODULO), 1);
                if (ieii.Modulo == 0)
                    State.Output.Fatal("Parameter not found, or it has an incorrect value.", p, paramBase.Push(P_MODULO));

                // get the Offset of the imigration from the current island
                p = localBase.Push(P_OFFSET);
                ieii.Offset = State.Parameters.GetInt(p, paramBase.Push(P_OFFSET), 0);
                if (ieii.Offset == -1)
                    State.Output.Fatal("Parameter not found, or it has an incorrect value.", p, paramBase.Push(P_OFFSET));
                //}

                // mark as uninitialized
                ieii.Port = -1;

                // insert the id in the hashset with the ids of the islands
                Info[IslandIds[x]] = ieii;
            }

            // get the information on destination islands (with checking for consistency)
            for (var x = 0; x < NumIslands; x++)
            {

                var ieii = (IslandExchangeIslandInfo)Info[IslandIds[x]];

                if (ieii == null)
                {
                    State.Output.Error("No information for island " + IslandIds[x] + " is stored in the server's information database.");
                    continue;
                }

                var localBase = islandBase.Push("" + x);

                // get the number of islands where individuals should be sent
                p = localBase.Push(P_NUM_MIGRATING_COUNTRIES);
                ieii.NumMig = State.Parameters.GetInt(p, null, 0);
                if (ieii.NumMig == -1)
                    State.Output.Fatal("Parameter not found, or it has an incorrect value.", p);

                // if there is at least 1 destination islands
                if (ieii.NumMig > 0)
                {

                    // allocate the storage for ids
                    ieii.MigratingIslandIds = new String[ieii.NumMig];

                    // store a new base parameter
                    var ll = localBase.Push(P_MIGRATING_ISLAND);

                    // for each of the islands
                    for (var y = 0; y < ieii.NumMig; y++)
                    {

                        // read the id & check for errors
                        ieii.MigratingIslandIds[y] = State.Parameters.GetStringWithDefault(ll.Push("" + y), null, null);
                        if (ieii.MigratingIslandIds[y] == null)
                            State.Output.Fatal("Parameter not found.", ll.Push("" + y));
                        else if (!Info.ContainsKey(ieii.MigratingIslandIds[y]))
                            State.Output.Fatal("Unknown island.", ll.Push("" + y));
                        else
                        {
                            // insert this knowledge into the hashtable for counting how many islands
                            // send individuals to each island
                            var integer = InfoImmigrants[ieii.MigratingIslandIds[y]];
                            if (integer == null)
                                InfoImmigrants[ieii.MigratingIslandIds[y]] = 1;
                            else
                                InfoImmigrants[ieii.MigratingIslandIds[y]] = (int)integer + 1;
                        }
                    }
                }

                // save the information back in the hash table
                // info.put( island_ids[x], ieii );                         // unneccessary -- Sean
            }

            for (var x = 0; x < NumIslands; x++)
            {

                var ieii = (IslandExchangeIslandInfo)Info[IslandIds[x]];

                if (ieii == null)
                {
                    State.Output.Fatal("No information for island " + IslandIds[x] + " is stored in the server's information database.");
                }

                var integer = InfoImmigrants[IslandIds[x]];

                // if the information does not exist in the hasthable,
                // it means no islands send individuals there
                if (integer == null)
                    ieii.NumIncoming = 0;
                else
                    ieii.NumIncoming = (int)integer;

                // save the information back in the hash table
                // info.put( island_ids[x], ieii );                 // unneccessary -- Sean
            }

            // allocate and reset this variable to false
            WhoIsSynchronized = new bool[NumIslands];

            for (var x = 0; x < NumIslands; x++)
                WhoIsSynchronized[x] = false;
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// The main function Running in the thread. 
        /// </summary>
        public virtual void Run()
        {

            // sockets to communicate to each of the islands
            var con = new TcpClient[NumIslands];

            // readers and writters for communication with each island
            var dataIn = new BinaryReader[NumIslands];
            var dataOut = new BinaryWriter[NumIslands];



            // whether each client is working (and communicating with the server) or not
            var clientRunning = new bool[NumIslands];

            // initialize the Running status of all clients
            for (var x = 0; x < NumIslands; x++)
                clientRunning[x] = true;

            try
            {
                // create a server
                var tempTcpListener = new TcpListener(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], ServerPort);
                tempTcpListener.Start();
                ServerSocket = tempTcpListener;
            }
            catch (IOException)
            {
                State.Output.Fatal("Error creating a socket on port " + ServerPort);
            }

            // for each of the islands
            for (var x = 0; x < NumIslands; x++)
            {
                try
                {
                    // set up connection with the island
                    con[x] = ServerSocket.AcceptTcpClient();

                    // initialize the reader and the writer
                    dataIn[x] = new BinaryReader(con[x].GetStream());
                    dataOut[x] = new BinaryWriter(con[x].GetStream());

                    // read the id
                    ConnectedIslandIds[x] = dataIn[x].ReadString().Trim();

                    State.Output.Message("Island " + ConnectedIslandIds[x] + " logged in");

                    // check whether the id appears in the information at the server
                    if (!Info.ContainsKey(ConnectedIslandIds[x]))
                    {
                        State.Output.Error("Incorrect ID (" + ConnectedIslandIds[x] + ")");
                        clientRunning[x] = false;
                        continue;
                    }

                    var ieii = (IslandExchangeIslandInfo)Info[ConnectedIslandIds[x]];

                    // redundant check, i know....
                    if (ieii == null)
                    {
                        State.Output.Error("Can't get IslandExchangeInfo for " + ConnectedIslandIds[x]);
                        clientRunning[x] = false;
                        continue;
                    }

                    // check whether an island with this id already registered with the server
                    if (ieii.Port >= 0)
                    {
                        State.Output.Error("Multiple islands are claiming the same ID (" + ConnectedIslandIds[x] + ")");
                        clientRunning[x] = false;
                        continue;
                    }

                    // send the number of ids that will be send through the communication link
                    dataOut[x].Write(ieii.NumIncoming);

                    // send the capacity of the Mailbox
                    dataOut[x].Write(ieii.MailboxCapacity);

                    dataOut[x].Flush();

                    // read the address and port of the island
                    ieii.Address = dataIn[x].ReadString().Trim();
                    ieii.Port = dataIn[x].ReadInt32();

                    State.Output.Message("" + x + ": Island " + ConnectedIslandIds[x]
                        + " has address " + ieii.Address + " : " + ieii.Port);

                    // re-insert the information in the hash table
                    // info.put( id, ieii );                                // unnecessary -- Sean
                }
                catch (IOException)
                {
                    State.Output.Error("Could not open connection #" + x);
                    clientRunning[x] = false;
                }
            }

            State.Output.ExitIfErrors();

            // By this time, all Mailboxes have been started and
            // they should be waiting for incoming messages. this is because
            // in order to send the server the information about the address and port
            // of the Mailbox, they have to start them first. This is the reason
            // that makes us be able to start connecting without other synchronization
            // stuff right at this point.

            // Now, I think, we've got a 1:1 mapping of keys to items in the info hashtable
            // So we tell everyone who they will communicate to

            for (var x = 0; x < NumIslands; x++)
            {
                if (clientRunning[x])
                {
                    var ieii = (IslandExchangeIslandInfo)Info[ConnectedIslandIds[x]];

                    if (ieii == null)
                    {
                        State.Output.Warning("There is no information about island " + ConnectedIslandIds[x]);
                        clientRunning[x] = false;
                        continue;
                    }

                    try
                    {
                        // send the Synchronous, Modulo, Offset and size information to the current islands
                        if (Synchronous)
                            dataOut[x].Write(1);
                        else
                            dataOut[x].Write(0);
                        dataOut[x].Write(ieii.Modulo);
                        dataOut[x].Write(ieii.Offset);
                        dataOut[x].Write(ieii.Size);

                        // send the number of address-port pairs that will be sent
                        dataOut[x].Write(ieii.NumMig);

                        for (var y = 0; y < ieii.NumMig; y++)
                        {
                            var temp = (IslandExchangeIslandInfo)Info[ieii.MigratingIslandIds[y]];

                            if (temp == null)
                            {
                                State.Output.Warning("There is incorrect information on the island " + ConnectedIslandIds[x]);
                                dataOut[x].Write(" ");
                                dataOut[x].Write(-1);
                            }
                            else
                            {
                                State.Output.Message("Island " + ConnectedIslandIds[x] + " should connect to island "
                                    + ieii.MigratingIslandIds[y] + " at " + temp.Address + " : " + temp.Port);

                                dataOut[x].Write(temp.Address);
                                dataOut[x].Write(temp.Port);
                            }
                        }
                        dataOut[x].Flush();
                    }
                    catch (IOException)
                    {
                        // other errors while reading
                        State.Output.Message("Server: Island " + IslandIds[x] + " dropped connection");
                        clientRunning[x] = false;
                        continue;
                    }
                    catch (NullReferenceException)
                    {
                        // other errors while reading
                        State.Output.Message("Server: Island " + IslandIds[x] + " dropped connection");
                        clientRunning[x] = false;
                        try
                        {
                            dataIn[x].Close();
                            dataOut[x].Close();
                            con[x].Close();
                        }
                        catch (IOException)
                        {
                        }
                        continue;
                    }
                }
            }

            try
            {
                // Next we wait until everyone acknowledges this
                foreach (BinaryReader t in dataIn)
                {
                    t.ReadString();
                }

                // Now we tell everyone to start Running
                foreach (BinaryWriter t in dataOut)
                {
                    t.Write(RUN);
                    t.Flush();
                }
            }
            catch (IOException)
            {
            }

            // Okay we've sent off our information.  Now we wait until a client
            // tells us that he's found the solution, or until all the clients
            // have broken connections 

            for (var x = 0; x < con.Length; x++)
            {
                try
                {
                    con[x].ReceiveTimeout = FOUND_TIMEOUT;
                }
                catch (SocketException)
                {
                    State.Output.Error("Could not set the connect with island " + x + " to non-blocking.");
                }
            }

            var shouldExit = false;

            while (!shouldExit)
            {
                // check whether there is at least one client Running
                // otherwise the server might continue functioning just because the last client crashed or finished connection
                shouldExit = true;
                for (var x = 0; x < dataOut.Length; x++)
                    if (clientRunning[x])
                    {
                        shouldExit = false;
                        break;
                    }
                if (shouldExit)
                    break;

                // sleep a while
                try
                {
                    Thread.Sleep(new TimeSpan((Int64)10000 * SLEEP_TIME));
                }
                catch (ThreadInterruptedException)
                {
                }

                for (var x = 0; x < dataOut.Length; x++)
                {
                    if (clientRunning[x])
                    {

                        // initialize ww
                        string ww;
                        
                        // check to see if he's still up, and if he's
                        // sent us a "I found it" signal
                        try
                        {
                            ww = dataIn[x].ReadString().Trim();
                        }
                        //catch (System.IO.IOException e)
                        //{
                        //    // means that it run out of time and got no message,
                        //    // so it should just continue with the other sockets
                        //    continue;
                        //}
                        catch (IOException)
                        {
                            // other errors while reading
                            State.Output.Message("Server: Island " + IslandIds[x] + " dropped connection");
                            clientRunning[x] = false;
                            continue;
                        }
                        catch (NullReferenceException)
                        {
                            // other errors while reading
                            State.Output.Message("Server: Island " + IslandIds[x] + " dropped connection");
                            clientRunning[x] = false;
                            try
                            {
                                dataIn[x].Close();
                                dataOut[x].Close();
                                con[x].Close();
                            }
                            catch (IOException)
                            {
                            }
                            catch (Exception)
                            {
                            }
                            continue;
                        }

                        if (ww == null) // the connection has been broken   
                        {
                            State.Output.Message("Server: Island " + IslandIds[x] + " dropped connection");
                            clientRunning[x] = false;
                            try
                            {
                                dataIn[x].Close();
                                dataOut[x].Close();
                                con[x].Close();
                            }
                            catch (IOException)
                            {
                            }
                        }
                        else if (ww.Equals(IslandExchange.FOUND))
                        // he found it!
                        {
                            // inform everyone that they need to shut down --
                            // we do not need to wrap
                            // our PrintLn statements in anything, they just
                            // return even if the client has broken the connection
                            for (var y = 0; y < dataOut.Length; y++)
                            {
                                if (clientRunning[y])
                                {
                                    try
                                    {
                                        dataOut[y].Write(GOODBYE);
                                        dataOut[y].Close();
                                        dataIn[y].Close();
                                        con[y].Close();
                                    }
                                    catch (IOException)
                                    {
                                    }
                                }
                            }
                            // now we can just get out of all this and
                            // quit the thread 
                            shouldExit = true;
                            break;
                        }
                        else if (ww.Equals(IslandExchange.SYNC))
                        {
                            WhoIsSynchronized[x] = true;

                            var complete_synchronization = true;

                            for (var y = 0; y < NumIslands; y++)
                                complete_synchronization = complete_synchronization && (!clientRunning[y] || WhoIsSynchronized[y]);

                            // if the number of total Running islands is smaller than the
                            // number of islands that ask for synchronization, let them continue
                            // Running
                            if (complete_synchronization)
                            {

                                for (var y = 0; y < NumIslands; y++)
                                {
                                    // send the okay message (the client can continue executing)
                                    if (clientRunning[y])
                                        try
                                        {
                                            dataOut[y].Write(IslandExchange.OKAY);
                                            dataOut[y].Flush();
                                        }
                                        catch (IOException)
                                        {
                                        }
                                    // reset the who_is_synchronized variable
                                    WhoIsSynchronized[y] = false;
                                }
                            }
                        }
                    }
                }
            }
            State.Output.Message("Server Exiting");
        }

        /// <summary>
        /// Here we spawn off the thread on ourselves. 
        /// </summary>
        public virtual ThreadClass SpawnThread()
        {
            var thread = new ThreadClass(this.Run);
            thread.Start();
            return thread;
        }

        #endregion // Operations
    }
}