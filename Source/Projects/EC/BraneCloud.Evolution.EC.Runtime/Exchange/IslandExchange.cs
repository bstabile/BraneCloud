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
using System.Runtime.Serialization;

using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Runtime;
using BraneCloud.Evolution.EC.Select;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Runtime.Exchange
{
    /// <summary> 
    /// IslandExchange is an Exchanger which 
    /// implements a simple but quite functional aSynchronous
    /// island model for doing massive parallel distribution of evolution across
    /// beowulf clusters.  One of its really nice features is that because everything
    /// is in Java, your cluster can have mixed platforms in it (MacOS, Unix, 
    /// Windoze, whatever you like).  You can also have multiple processes Running
    /// on the same machine, as long as they're given different client ports.
    /// IslandExchange operates over TCP/IP with Java sockets, and is compatible 
    /// with checkpointing.
    /// 
    /// <p/>IslandExchange uses an arbitrary graph topology for migrating individuals
    /// from island (EC process) to island over the network.  There are a few
    /// restrictions for simplicity, however:
    /// <ul>
    /// <li/> Every island must have the same kind of subpops and species.
    /// <li/> Every subpop will send the same number of migrants as any 
    /// other subpop.
    /// <li/> Migrants from a subpop will go to the same subpop.
    /// </ul>
    /// <p/>Every island is a <i>client</i>.  Additionally one island is designated
    /// a <i>server</i>.  Note that, just like in the Hair Club for Men, the server
    /// is also a client.  The purpose of the server is to synchronize the clients
    /// so that they all get set up properly and hook up to each other, then to
    /// send them small signal messages (like informing them that another client has
    /// discovered the ideal individual), and help them gracefully shut down.  Other
    /// than these few signals which are routed through the server to the clients,
    /// all other information -- namely the migrants themselves -- are sent directly
    /// from client to client in a peer-to-peer fashion.
    /// 
    /// <p/>The topology of the network is stored solely in the server's parameter
    /// database.  When the clients fire up, they first set up "Mailboxes" (where
    /// immigrants from other clients will appear), then they go to the server 
    /// and ask it who they should connect to to send migrants.  The server tells
    /// them, and then they then hook up.  When a client has finished hooking up, it
    /// reports this to the server.  After everyone has hooked up, the server tells
    /// the clients to begin evolution, and they're off and Running.
    /// 
    /// <p/>Islands send emigrants to other islands by copying good individuals
    /// (selected with a SelectionMethod) and sending the good individuals to
    /// the Mailboxes of receiving clients.  Once an individual has been received,
    /// it is considered to be unevaluated by the receiving island, even though 
    /// it had been previously evaluated by the sending island.
    /// 
    /// <p/>The IslandExchange model is typically <i>aSynchronous</i> because migrants may
    /// appear in your Mailbox at any time; islands do not wait for each other
    /// to complete the next generation.  This is a more efficient usage of network
    /// bandwidth.  When an island completes its breeding, it looks inside its
    /// Mailbox for new migrants.  It then replaces some of its newly-bred
    /// individuals (chosen entirely at random)
    /// with the migrants (we could have increased the population size so we didn't
    /// waste that breeding time, but we were lazy).  It then flushes the Mailbox,
    /// which patiently sits waiting for more individuals.
    /// 
    /// <p/>Clients may also be given different start times and Modulos for 
    /// migrating.  For example, client A might be told that he begins sending emigrants
    /// only after generation 6, and then sends emigrants on every 4 generations beyond
    /// that.  The purpose for the start times and Modulos is so that not every client
    /// sends emigrants at the same time; this also makes better use of network bandwidth.
    /// 
    /// <p/>When a client goes down, the other clients deal with it gracefully; they
    /// simply stop trying to send to it.  But if the server goes down, the clients
    /// do not continue operation; they will shut themselves down.  This means that in
    /// general you can shut down an entire island model network just by killing the
    /// server process.  However, if the server quits because it runs out of generations,
    /// it will wait for the clients to all quit before it finally stops.
    /// 
    /// <p/>IslandExchange works correctly with checkpointing.  If you restart from
    /// a checkpoint, the IslandExchange will start up the clients and servers again
    /// and reconnect.  Processes can start from different checkpoints, of course.
    /// However, realize that if you restart from a checkpoint, some migrants
    /// may have been lost in transit from island to island.  That's the nature of
    /// networking without heavy-duty transaction management! This means that we
    /// cannot guarantee that restarting from checkpoint will yield the same results
    /// as the first run yielded.
    /// 
    /// <p/>Islands are not described in the topology parameters by their
    /// IP addresses; instead, they are described by "ids", strings which uniquely 
    /// identify each island.  For example, "gilligans-island" might be an id.  :-)
    /// This allows you to move your topology to different IP addresses without having
    /// to change all your parameter files!  You can even move your topology to totally
    /// different machines, and restart from previous checkpoints, and everything
    /// should still work correctly.
    /// 
    /// <p/>There are times, especially to experiment with dynamics, that you need
    /// a <i>Synchronous</i> island model.  If you specify synchronicity, the server's
    /// stated Modulo and Offset override any modulii or Offsets specified by clients.
    /// Everyone will use the server's Modulo and Offset.  This means that everyone
    /// will trade individuals on the same generation.  Additionally, clients will wait
    /// until everyone else has traded, before they are permitted to continue evolving.
    /// This has the effect of locking all the clients together generation-wise; no
    /// clients can run faster than any other clients.
    /// 
    /// <p/>One last item: normally in this model, the server is also a client.  But 
    /// if for some reason you need the server to be a process all by itself, without
    /// creating a client as well, you can do that.  You spawn such a server differently
    /// than the main execution of ECJ.  To spawn a server on a given server params file
    /// (let's say it's server.params) but NOT spawn a client, you do:
    /// <p/><pre>
    /// java ec.exchange.IslandExchange -file server.params
    /// </pre>
    /// <p/> ...this sets up a special process which just spawns a server, and doesn't do
    /// all the Setup of an evolutionary run.  Of course as usual, for each of the 
    /// clients, you'll run <tt>java ec.Evolve ...</tt> instead.
    /// <p/><b>Parameters</b><br/>
    /// <p/><i>Note:</i> some of these parameters are only necessary for creating
    /// <b>clients</b>.  Others are necessary for creating the <b>server</b>.
    /// <table>
    /// <tr><td valign="top"><tt><i>base</i>.Chatty</tt><br/>
    /// <font size="-1">boolean, default = true</font></td>
    /// <td valign="top"> Should we be verbose or silent about our exchanges?
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.select</tt><br/>
    /// <font size="-1">classname, inherits and != ec.SelectionMethod</font></td>
    /// <td valign="top">
    /// <i>client</i>: The selection method used for picking migrants to emigrate to other islands
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.select-to-die</tt><br/>
    /// <font size="-1">classname, inherits and != ec.SelectionMethod, default is ec.select.RandomSelection</font></td>
    /// <td valign="top">
    /// <i>client</i>: The selection method used for picking individuals to be replaced by incoming migrants.
    /// <b>IMPORTANT Note</b>.  This selection method must <i>not</i> pick an individual based on fitness.
    /// The selection method will be called just after breeding but <i>before</i> evaluation; many individuals
    /// will not have had a fitness assigned at that point.  You might want to design a SelectionMethod
    /// other than RandomSelection, however, to do things like not picking elites to die.
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.server-addr</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">
    /// <i>client</i>: The IP address of the server
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.server-port</tt><br/>
    /// <font size="-1">int >= 1</font></td>
    /// <td valign="top">
    /// <i>client</i>: The port number of the server
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.client-port</tt><br/>
    /// <font size="-1">int >= 1</font></td>
    /// <td valign="top">
    /// <i>client</i>: The port number of the client (where it will receive migrants) -- this should be different from the server port.
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.id</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">
    /// <i>client</i>: The "name" the client is giving itself.  Each client should have a unique name.  For example, "gilligans-island".
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.compressed</tt><br/>
    /// <font size="-1">bool = <tt>true</tt> (default) or <tt>false</tt></font></td>
    /// <td valign="top">
    /// <i>client</i>: Whether the communication with other islands should be compressed or not.  Compressing uses more CPU, but it may also significantly reduce communication.
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.i-am-server</tt><br/>
    /// <font size="-1">bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">
    /// <i>client</i>: Is this client also the server?  If so, it'll read the server parameters and set up a server as well.
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.sync</tt><br/>
    /// <font size="-1">bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">
    /// <i>server</i>: Are we doing a Synchronous island model?  If so, the server's Modulo and Offset override any client's stated Modulo and Offset.
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.start</tt><br/>
    /// <font size="-1">bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">
    /// <i>server</i>: (Only if island model is Synchronous) The generation when islands begin sending emigrants.
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.mod</tt><br/>
    /// <font size="-1">bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">
    /// <i>server</i>: (Only if island model is Synchronous) The number of generations islands wait between sending emigrants.
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.num-islands</tt><br/>
    /// <font size="-1">int >= 1</font></td>
    /// <td valign="top">
    /// <i>server</i>: The number of islands in the topology.
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.island.<i>n</i>.id</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">
    /// <i>server</i>: The ID of island #n in the topology.
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.island.<i>n</i>.num-mig</tt><br/>
    /// <font size="-1">int >= 1</font></td>
    /// <td valign="top">
    /// <i>server</i>: The number of islands that island #n sends emigrants to.
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.island.<i>n</i>.mig.</tt><i>m</i><br/>
    /// <font size="-1">int >= 1</font></td>
    /// <td valign="top">
    /// <i>server</i>: The ID of island #m that island #n sends emigrants to.
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.island.<i>n</i>.size</tt><br/>
    /// <font size="-1">int >= 1</font></td>
    /// <td valign="top">
    /// <i>server</i>: The number of emigrants (per subpop) that island #n sends to other islands.
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.island.<i>n</i>.start</tt><br/>
    /// <font size="-1">int >= 0</font></td>
    /// <td valign="top">
    /// <i>server</i>: The generation when island #n begins sending emigrants.
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.island.<i>n</i>.mod</tt><br/>
    /// <font size="-1">int >= 1</font></td>
    /// <td valign="top">
    /// <i>server</i>: The number of generations that island #n waits between sending emigrants.
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.island.<i>n</i>.Mailbox-capacity</tt><br/>
    /// <font size="-1">int >= 1</font></td>
    /// <td valign="top">
    /// <i>server</i>: The maximum size (per subpop) of the Mailbox for island #n.
    /// </td></tr>
    /// </table>
    /// 
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><tt><i>base</i>.select</tt></td>
    /// <td>selection method for the client's migrants</td></tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.exchange.IslandExchange")]
    public class IslandExchange : Exchanger, ISerializable
    {
        #region Constants

        private const long SerialVersionUID = 1;

        //// Client information

        /// <summary>
        /// The server address. 
        /// </summary>
        public const string P_SERVER_ADDRESS = "server-addr";

        /// <summary>
        /// The server port. 
        /// </summary>
        public const string P_SERVER_PORT = IslandExchangeServer.P_SERVER_PORT;

        /// <summary>
        /// The client port. 
        /// </summary>
        public const string P_CLIENT_PORT = "client-port";

        /// <summary>
        /// Whether the server is also on this island. 
        /// </summary>
        public const string P_IS_SERVER = "i-am-server";

        /// <summary>
        /// The id of the island. 
        /// </summary>
        public const string P_OWN_ID = "id";

        /// <summary>
        /// Whether the communication is compressed or not. 
        /// </summary>
        public const string P_COMPRESSED_COMMUNICATION = "compressed";

        /// <summary>
        /// The selection method for sending individuals to other islands. 
        /// </summary>
        public const string P_SELECT_METHOD = "select";

        /// <summary>
        /// The selection method for deciding individuals to be replaced by immigrants. 
        /// </summary>
        public const string P_SELECT_TO_DIE_METHOD = "select-to-die";

        /// <summary>
        /// How long we sleep in between attempts to connect or look for signals. 
        /// </summary>
        public const int SLEEP_TIME = 100;

        /// <summary>
        /// How long we sleep between checking for FOUND messages. 
        /// </summary>
        public const int FOUND_TIMEOUT = 100;

        /// <summary>
        /// Whether or not we're Chatty. 
        /// </summary>
        public const string P_CHATTY = "Chatty";

        /// <summary>
        /// Okay signal. 
        /// </summary>
        public const string OKAY = "okay";

        /// <summary>
        /// Synchronize signal 
        /// </summary>
        public const string SYNC = "sync";

        /// <summary>
        /// Found signal. 
        /// </summary>
        public const string FOUND = "found";

        #endregion // Constants
        #region Static

        /// <summary>
        /// Am I ONLY a server?
        /// </summary>
        internal static bool JustServer;

        #endregion // Static
        #region Properties

        /// <summary>
        /// Our chattiness. 
        /// </summary>
        public bool Chatty { get; set; }

        /// <summary>
        /// The thread of the server (is different than null only for the island with the server). 
        /// </summary>
        public ThreadClass ServerThread { get; set; }

        /// <summary>
        /// My parameter base -- I need to keep this in order to help the server
        /// reinitialize contacts. 
        /// </summary>
        /// <remarks>This needs to be serialized.</remarks>
        public IParameter ParamBase { get; set; }

        /// <summary>
        /// The address of the server. 
        /// </summary>
        /// <remarks>This needs to be serialized.</remarks>
        public string ServerAddress { get; set; }

        /// <summary>
        /// The port of the server. 
        /// </summary>
        /// <remarks>This needs to be serialized.</remarks>
        public int ServerPort { get; set; }

        /// <summary>
        /// The port of the client Mailbox. 
        /// </summary>
        /// <remarks>This needs to be serialized.</remarks>
        public int ClientPort { get; set; }

        /// <summary>
        /// Whether the server should be Running on the current island or not.
        /// </summary>
        /// <remarks>This needs to be serialized.</remarks>
        public bool AmServer { get; set; }

        /// <summary>
        /// The id of the current island. 
        /// </summary>
        /// <remarks>This needs to be serialized.</remarks>
        public string OwnId { get; set; }

        /// <summary>
        /// Whether the communication is compressed or not 
        /// </summary>
        /// <remarks>This needs to be serialized.</remarks>
        public bool CompressedCommunication { get; set; }

        /// <summary>
        /// The selection method for emigrants 
        /// </summary>
        /// <remarks>This needs to be serialized.</remarks>
        public SelectionMethod ImmigrantsSelectionMethod { get; set; }

        /// <summary>
        /// The selection method for individuals to be replaced by immigrants. 
        /// </summary>
        /// <remarks>This needs to be serialized.</remarks>
        public SelectionMethod IndsToDieSelectionMethod { get; set; }

        /// <summary>
        /// The Mailbox of the current client (exchanger).
        /// </summary>
        public IslandExchangeMailbox Mailbox { get; set; }

        /// <summary>
        /// The thread of the Mailbox.
        /// </summary>
        public ThreadClass MailboxThread { get; set; }

        // Communication with the islands where individuals have to be sent.

        /// <summary>
        /// Number of islands to send individuals to.
        /// </summary>
        public int NumberOfDestinationIslands { get; set; }

        /// <summary>
        /// Synchronous or aSynchronous communication. 
        /// </summary>
        public bool Synchronous { get; set; }

        /// <summary>
        /// How often to send individuals. 
        /// </summary>
        public int Modulo { get; set; }

        /// <summary>
        /// After how many generations to start sending individuals. 
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// How many individuals to send each time. 
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Sockets to the destination islands.
        /// </summary>
        public TcpClient[] OutSockets { get; set; }

        /// <summary>
        /// DataOutputStream to the destination islands.
        /// </summary>
        public BinaryWriter[] OutWriters { get; set; }

        /// <summary>
        /// So we can print out nice names for our outgoing connections.
        /// </summary>
        public string[] OutgoingIds { get; set; }

        /// <summary>
        /// Information on the availability of the different islands.
        /// </summary>
        public bool[] Running { get; set; }

        /// <summary>
        /// The socket to the server.
        /// </summary>
        public TcpClient ServerSocket { get; set; }

        /// <summary>
        /// Reader and writer to the ServerSocket.
        /// </summary>
        public BinaryWriter ToServer { get; set; }

        public BinaryReader FromServer { get; set; }

        /// <summary>
        /// If the GOODBYE message sent by the server gets read in the wrong place, this variable is set to true.
        /// </summary>
        internal bool AlreadyReadGoodBye { get; set; }

        /// <summary>
        /// Keeps the message to be returned next time on runComplete.
        /// </summary>
        internal string Message { get; set; }

        #endregion 
        #region Setup

        /// <summary>
        /// Sets up the Island Exchanger
        /// </summary>
        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            ParamBase = paramBase;

            // get the port of the server
            var p = paramBase.Push(P_SERVER_PORT);
            ServerPort = state.Parameters.GetInt(p, null, 1);
            if (ServerPort == 0)
                state.Output.Fatal("Could not get the port of the server, or it is invalid.", p);

            Chatty = state.Parameters.GetBoolean(paramBase.Push(P_CHATTY), null, true);

            // by default, communication is not compressed
            CompressedCommunication = state.Parameters.GetBoolean(paramBase.Push(P_COMPRESSED_COMMUNICATION), null, false);
            if (CompressedCommunication)
            {
                //            state.Output.Fatal("JDK 1.5 has broken compression.  For now, you must set " + base.Push(P_COMPRESSED_COMMUNICATION) + "=false");
                state.Output.Message("Communication will be compressed");
            }

            // check whether it has to launch the main server for coordination
            p = paramBase.Push(P_IS_SERVER);
            AmServer = state.Parameters.GetBoolean(p, null, false);

            // Am I ONLY the server or not?
            if (JustServer)
            {
                // print out my IP address
                try
                {
                    state.Output.Message("IP ADDRESS: " + Dns.GetHostEntry(Dns.GetHostName()).AddressList[0]);
                }
                catch (Exception)
                {
                }
            }
            else
            {
                // Setup the selection method
                p = paramBase.Push(P_SELECT_METHOD);
                ImmigrantsSelectionMethod = (SelectionMethod)state.Parameters.GetInstanceForParameter(p, null, typeof(SelectionMethod));
                ImmigrantsSelectionMethod.Setup(state, paramBase);

                // Setup the selection method
                p = paramBase.Push(P_SELECT_TO_DIE_METHOD);
                if (state.Parameters.ParameterExists(p, null))
                    IndsToDieSelectionMethod = (SelectionMethod)state.Parameters.GetInstanceForParameter(p, null, typeof(SelectionMethod));
                // use RandomSelection
                else
                    IndsToDieSelectionMethod = new RandomSelection();
                IndsToDieSelectionMethod.Setup(state, paramBase);

                // get the address of the server
                p = paramBase.Push(P_SERVER_ADDRESS);
                ServerAddress = state.Parameters.GetStringWithDefault(p, null, "");
                if (String.IsNullOrEmpty(ServerAddress))
                    state.Output.Fatal("Could not get the address of the server.", p);

                // get the port of the client Mailbox
                p = paramBase.Push(P_CLIENT_PORT);
                ClientPort = state.Parameters.GetInt(p, null, 1);
                if (ClientPort == 0)
                    state.Output.Fatal("Could not get the port of the client, or it is invalid.", p);

                // get the id of the island
                p = paramBase.Push(P_OWN_ID);
                OwnId = state.Parameters.GetStringWithDefault(p, null, "");
                if (String.IsNullOrEmpty(OwnId))
                    state.Output.Fatal("Could not get the Id of the island.", p);
            }
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Fires up the server. 
        /// </summary>        
        public virtual void FireUpServer(IEvolutionState state, IParameter serverBase)
        {
            var serv = new IslandExchangeServer();
            serv.SetupServerFromDatabase(state, serverBase);
            ServerThread = serv.SpawnThread();
        }

        /// <summary>
        /// Initializes contacts with other processes, if that's what you're doing. 
        /// Called at the beginning of an evolutionary run, before a population is set up. 
        /// </summary>
        public override void InitializeContacts(IEvolutionState state)
        {

            // launch the server
            if (AmServer)
            {
                FireUpServer(state, ParamBase);
                state.Output.Message("Server Launched.");
            }
            else
            {
                state.Output.Message("I'm just a client.");
            }



            // In this thread, *I* am the client.  I connect to the server
            // and get the information from the server, then I connect
            // to the clients and go through the synchronization process
            // with the server.  Spawn the Mailbox. When the server says "go", I'm done with
            // this function.


            /** Make our connections and hook up */
            long l = 0;
            try
            {
                // spin until we get a connection
                state.Output.Message("Connecting to Server " + ServerAddress + ", port " + ServerPort);
                while (true)
                {
                    try
                    {
                        ServerSocket = new TcpClient(ServerAddress, ServerPort);
                        break;
                    }
                    catch (IOException)
                    // it's not up yet...
                    {
                        l++;
                        try
                        {
                            Thread.Sleep(new TimeSpan((Int64)10000 * 5000));
                        }
                        catch (ThreadInterruptedException f)
                        {
                            state.Output.Message("" + f);
                        }
                        state.Output.Message("Retrying");
                    }
                }

                // okay, we're connected.  Send our info.
                state.Output.Message("Connected to Server after " + (l * SLEEP_TIME) + " ms");
                FromServer = new BinaryReader(ServerSocket.GetStream());
                ToServer = new BinaryWriter(ServerSocket.GetStream());

                // sending the server own contact information
                ToServer.Write(OwnId); // Default encoding of BinaryWriter is UTF-8
                ToServer.Flush();

                // Launch the Mailbox thread (read from the server how many sockets to allocate
                // on the Mailbox. Obtain the port and address of the Mailbox.
                Mailbox = new IslandExchangeMailbox(state, ClientPort, FromServer.ReadInt32(),
                                    FromServer.ReadInt32(), OwnId, Chatty, CompressedCommunication);

                MailboxThread = new ThreadClass(new ThreadStart(Mailbox.Run));
                MailboxThread.Start();

                // record that the Mailbox has been created
                state.Output.Message("IslandExchangeMailbox created.");

                // tell the server the address and port of the Mailbox
                try
                {
                    ToServer.Write(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString());
                    ToServer.Flush();
                    state.Output.Message("My address is: " + Dns.GetHostEntry(Dns.GetHostName()).AddressList[0]);
                }
                catch (Exception)
                {
                    state.Output.Fatal("Could not get the address of the local computer.");
                }
                ToServer.Write(Mailbox.Port);
                ToServer.Flush();

                // read from the server the Modulo, Offset and size it has to use.
                // this parameters allow an extendable/modifiable version where different
                // islands send different number of individuals (based on the size of their populations)
                Synchronous = (FromServer.ReadInt32() == 1);
                if (Synchronous)
                {
                    state.Output.Message("The communication will be Synchronous.");
                }
                else
                {
                    state.Output.Message("The communication will be aSynchronous.");
                }
                Modulo = FromServer.ReadInt32();
                Offset = FromServer.ReadInt32();
                Size = FromServer.ReadInt32();

                // read the number of islands it has to send messages to
                NumberOfDestinationIslands = FromServer.ReadInt32();

                // allocate the arrays
                OutSockets = new TcpClient[NumberOfDestinationIslands];
                OutWriters = new BinaryWriter[NumberOfDestinationIslands];
                Running = new bool[NumberOfDestinationIslands];
                OutgoingIds = new String[NumberOfDestinationIslands];

                // open connections to each of the destination islands
                for (var y = 0; y < NumberOfDestinationIslands; y++)
                {
                    // get the address and the port
                    var address = FromServer.ReadString().Trim();
                    var port = FromServer.ReadInt32();
                    try
                    {
                        try
                        {
                            state.Output.Message("Trying to connect to " + address + " : " + port);
                            // try opening a connection
                            OutSockets[y] = new TcpClient(address, port);
                        }
                        catch (Exception)
                        {
                            // gracefully handle communication errors
                            state.Output.Warning("Unknown host exception while the client was opening a socket to " + address + " : " + port);
                            Running[y] = false;
                            continue;
                        }

                        if (CompressedCommunication)
                        {
                            /*                        
                            OutWriters[y] = new DataOutputStream(new CompressingOutputStream(OutSockets[y].getOutputStream()));
                            // read the Mailbox's id, then write my own id
                            OutgoingIds[y] = new DataInputStream(new CompressingInputStream(OutSockets[y].getInputStream())).ReadString().trim();
                            */

                            var compressedo = Output.MakeCompressingOutputStream(OutSockets[y].GetStream());
                            var compressedi = Output.MakeCompressingInputStream(OutSockets[y].GetStream());
                            if (compressedi == null || compressedo == null)
                                state.Output.Fatal("You do not appear to have JZLib installed on your system, and so may must have compression turned off for IslandExchange.  " + "To get JZLib, download from the ECJ website or from http://www.jcraft.com/jzlib/");
                            OutWriters[y] = new BinaryWriter(compressedo);
                            OutgoingIds[y] = new BinaryReader(compressedi).ReadString().Trim();
                        }
                        else
                        {
                            OutWriters[y] = new BinaryWriter(OutSockets[y].GetStream());

                            // read the Mailbox's id, then write my own id
                            OutgoingIds[y] = new BinaryReader(OutSockets[y].GetStream()).ReadString().Trim();
                        }

                        OutWriters[y].Write(OwnId);
                        OutWriters[y].Flush();

                        Running[y] = true;
                    }
                    catch (IOException e)
                    {
                        // this is caused if the server had problems locating information
                        // on the Mailbox of the other island, therefore remember the
                        // communication with this island is not Setup properly
                        state.Output.Warning("IO exception while the client was opening sockets to other islands' Mailboxes :" + e);
                        Running[y] = false;
                    }
                }

                // synchronization stuff: tells the server it finished connecting to other Mailboxes
                ToServer.Write(OKAY);
                ToServer.Flush();

                // wait for the run signal
                FromServer.ReadString();
            }
            catch (IOException)
            {
                state.Output.Fatal("Error communicating to the server.");
            }

            // at this point, the Mailbox is looking for incoming messages
            // form other islands. we have to exit the function. there is
            // one more thing to be done: to check for the server sending a
            // FOUND signal. In order to do this, we set the socket to the
            // server as non-blocking, and verify that for messages from the
            // server in the runComplete function
            try
            {
                ServerSocket.ReceiveTimeout = FOUND_TIMEOUT;
            }
            catch (SocketException)
            {
                state.Output.Fatal("Could not set the connection to the server to non-blocking.");
            }
        }

        /// <summary>
        /// Initializes contacts with other processes, if that's what you're doing.  
        /// Called after restarting from a checkpoint. 
        /// </summary>
        public override void ReinitializeContacts(IEvolutionState state)
        {
            // This function is almost the same as initializeContacts.
            // The only main difference is that when reinitializeContacts
            // is called, it's called because I started up from a checkpoint file.
            // This means that I'm in the middle of evolution, so the Modulo
            // and start might cause me to update more recently than if I had
            // started fresh.  But maybe it won't make a difference in this method
            // if the way I determine when I'm firing off migrants is on a
            // generation-by-generation basis.

            InitializeContacts(state);
        }

        public override Population PreBreedingExchangePopulation(IEvolutionState state)
        {
            // sending individuals to other islands
            // BUT ONLY if my Modulo and Offset are appropriate for this
            // generation (state.Generation)
            // I am responsible for returning a population.  This could
            // be a new population that I created fresh, or I could modify
            // the existing population and return that.

            // else, check whether the emigrants need to be sent
            if ((state.Generation >= Offset) && ((Modulo == 0) || (((state.Generation - Offset) % Modulo) == 0)))
            {

                // send the individuals!!!!

                // for each of the islands where we have to send individuals
                for (var x = 0; x < NumberOfDestinationIslands; x++)
                    try
                    {

                        // check whether the communication is ok with the current island
                        if (Running[x])
                        {

                            if (Chatty)
                                state.Output.Message("Sending " + Size + " emigrants to island " + OutgoingIds[x]);

                            // for each of the subpops
                            for (var subpop = 0; subpop < state.Population.Subpops.Length; subpop++)
                            {
                                // send the subpop
                                OutWriters[x].Write(subpop);

                                // send the number of individuals to be sent
                                // it's better to send this information too, such that islands can (potentially)
                                // send different numbers of individuals
                                OutWriters[x].Write(Size);

                                // select "Size" individuals and send then to the destination as emigrants
                                ImmigrantsSelectionMethod.PrepareToProduce(state, subpop, 0);
                                for (var y = 0; y < Size; y++)
                                // send all necesary individuals
                                {
                                    var index = ImmigrantsSelectionMethod.Produce(subpop, state, 0);
                                    state.Population.Subpops[subpop].Individuals[index].WriteIndividual(state, OutWriters[x]);
                                    OutWriters[x].Flush(); // just in case the individuals didn't do a PrintLn
                                }
                                ImmigrantsSelectionMethod.FinishProducing(state, subpop, 0); // end the selection step
                            }
                        }
                    }
                    catch (IOException)
                    {
                        Running[x] = false;
                    }
            }
            return state.Population;
        }

        public override Population PostBreedingExchangePopulation(IEvolutionState state)
        {
            // receiving individuals from other islands
            // same situation here of course.

            // if Synchronous communication, synchronize with the Mailbox
            //if ((state.Generation >= Offset) && Synchronous && ((Modulo == 0) || (((state.Generation - Offset) % Modulo) == 0)))
            if (Synchronous)
            {

                state.Output.Message("Waiting for synchronization....");

                // set the socket to the server to blocking
                try
                {
                    ServerSocket.ReceiveTimeout = 0;
                }
                catch (SocketException)
                {
                    state.Output.Fatal("Could not set the connection to the server to blocking.");
                }

                try
                {
                    // send the sync message
                    ToServer.Write(SYNC);
                    ToServer.Flush();
                    // wait for the okay message
                    var temp = FromServer.ReadString();
                    if (temp.Equals(IslandExchangeServer.GOODBYE))
                    {
                        AlreadyReadGoodBye = true;
                    }
                }
                catch (IOException)
                {
                    state.Output.Fatal("Could not communicate to the server. Exiting....");
                }

                // set the socket to the server to non-blocking
                try
                {
                    ServerSocket.ReceiveTimeout = FOUND_TIMEOUT;
                }
                catch (SocketException)
                {
                    state.Output.Fatal("Could not set the connection to the server to non-blocking.");
                }

                //state.Output.Message("Synchronized. Reading individuals....");
            }

            // synchronize, because immigrants is also accessed by the Mailbox thread
            lock (Mailbox.Immigrants)
            {
                for (var x = 0; x < Mailbox.Immigrants.Length; x++)
                {
                    if (Mailbox.NumImmigrants[x] > 0)
                    {
                        if (Chatty)
                            state.Output.Message("Immigrating " + Mailbox.NumImmigrants[x] + " individuals from Mailbox for subpop " + x);

                        var selected = new bool[state.Population.Subpops[x].Individuals.Length];
                        var indices = new int[Mailbox.NumImmigrants[x]];
                        for (var i = 0; i < selected.Length; i++)
                            selected[i] = false;
                        IndsToDieSelectionMethod.PrepareToProduce(state, x, 0);
                        for (var i = 0; i < Mailbox.NumImmigrants[x]; i++)
                        {
                            do
                            {
                                indices[i] = IndsToDieSelectionMethod.Produce(x, state, 0);
                            }
                            while (selected[indices[i]]);
                            selected[indices[i]] = true;
                        }
                        IndsToDieSelectionMethod.FinishProducing(state, x, 0);

                        // there is no need to check for the differences in size: the Mailbox.immigrants,
                        // state.Population.Subpops and the Mailbox.person2die should have the same size
                        for (var y = 0; y < Mailbox.NumImmigrants[x]; y++)
                        {

                            // read the individual
                            state.Population.Subpops[x].Individuals[indices[y]] = Mailbox.Immigrants[x][y];

                            // reset the evaluated flag (the individuals are not evaluated in the current island */
                            state.Population.Subpops[x].Individuals[indices[y]].Evaluated = false;
                        }

                        // reset the number of immigrants in the Mailbox for the current subpop
                        // this doesn't need another synchronization, because the thread is already synchronized
                        Mailbox.NumImmigrants[x] = 0;
                    }
                }
            }
            return state.Population;
        }

        /// <summary>
        /// Called after PreBreedingExchangePopulation(...) to evaluate whether or not
        /// the exchanger wishes the run to shut down (with ec.EvolutionState.R_FAILURE).
        /// This would happen for two reasons.  First, another process might have found
        /// an ideal individual and the global run is now over.  Second, some network
        /// or operating system error may have occurred and the system needs to be shut
        /// down gracefully.
        /// This function does not return a String as soon as it wants to exit (another island found
        /// the perfect individual, or couldn't connect to the server). Instead, it sets a flag, called
        /// message, to remember next time to exit. This is due to a need for a graceful
        /// shutdown, where checkpoints are working properly and save all needed information. 
        /// </summary>
        public override string RunComplete(IEvolutionState state)
        {

            // first test the flag, and exit if it was previously set
            if (Message != null)
            // if an error occured earlier
            {
                return Message;
            }

            // check whether the server sent a FOUND message.
            // if it did, check whether it should exit or not
            try
            {
                // read a line. if it is successful, it means that the server sent a FOUND message
                // (this is the only message the server sends right now), and it should set the flag
                // for exiting next time when in this procedure
                var ww = FromServer.ReadString();
                if (ww != null || AlreadyReadGoodBye)
                // FOUND message sent from the server
                {
                    // we should exit because some other individual has
                    // found the perfect fit individual
                    if (state.QuitOnRunComplete)
                    {
                        Message = "Exit: Another island found the perfect individual.";
                        state.Output.Message("Another island found the perfect individual. Exiting....");
                        ToServer.Write(OKAY);
                        ToServer.Flush();
                    }
                    else
                    {
                        state.Output.Message("Another island found the perfect individual.");
                    }
                }
                // ( ww == null ) // the connection with the server was closed
                else
                {
                    // we should exit, because we cannot communicate with the
                    // server anyway
                    Message = "Exit: Could not communicate with the server.";
                    state.Output.Warning("Could not communicate with the server. Exiting....");
                }
            }
            catch (IOException)
            {
                // here don't do anything: it reaches this point when the server is on, but nobody found
                // the perfect individual. in this case, it should just return null, so that the
                // execution continues
            }
            catch (Exception)
            {
                // some weird error
                // report it in a warning
                state.Output.Warning("Some weird IO exception reported by the system in the IslandExchange::runComplete function.  Is it possible that the server has crashed?");
            }
            return null;
        }

        /// <summary>
        /// Closes contacts with other processes, if that's what you're doing.  
        /// Called at the end of an evolutionary run. result is either 
        /// ec.EvolutionState.R_SUCCESS or ec.EvolutionState.R_FAILURE, 
        /// indicating whether or not an ideal individual was found. 
        /// </summary>
        public override void CloseContacts(IEvolutionState state, int result)
        {
            // if the run was successful (perfect individual was found)
            // then send a message to the server that it was found
            if (result == EvolutionState.R_SUCCESS)
            {
                try
                {
                    ToServer.Write(FOUND);
                    ToServer.Flush();
                }
                catch (IOException)
                {
                }
            }

            // close socket to server
            try
            {
                ServerSocket.Close();
            }
            catch (IOException)
            {
            }

            state.Output.Message("Shutting down the Mailbox");
            // close the Mailbox and wait for the thread to terminate
            Mailbox.ShutDown();
            MailboxThread.Interrupt();
            try
            {
                MailboxThread.Join();
            }
            catch (ThreadInterruptedException)
            {
            }
            state.Output.Message("Mailbox shut down");

            // close out-going sockets
            for (var x = 0; x < NumberOfDestinationIslands; x++)
            {
                // catch each exception apart (don't take into consideration the Running variables)
                try
                {
                    if (Running[x])
                        OutSockets[x].Close();
                }
                catch (IOException)
                {
                }
            }

            // if the island also hosts the server, wait till it terminates
            if (AmServer)
            {
                state.Output.Message("Shutting down the server");
                try
                {
                    ServerThread.Join();
                }
                catch (ThreadInterruptedException)
                {
                }
                state.Output.Message("Server shut down");
            }
        }

        public virtual void Finish(int result)
        {
        }
        public virtual void StartFromCheckpoint()
        {
        }
        public virtual void StartFresh()
        {
        }
        public virtual int Evolve()
        {
            return 0;
        }

        #endregion // Operations
        #region Main

        [STAThread]
        public static void Main(string[] args)
        {
            JustServer = true;
            int x;
            ParameterDatabase parameters = null;
            //bool store;

            // The following is a little chunk of the ec.Evolve code sufficient
            // to get IslandExchange up and Running all by itself.

            Console.Error.WriteLine("Island Exchange Server\n" + "Used in ECCS\n");


            // 0. find the parameter database
            for (x = 0; x < args.Length - 1; x++)
                if (args[x].Equals(Runtime.Evolve.A_FILE))
                {
                    try
                    {
                        parameters = new ParameterDatabase(new FileInfo(new FileInfo(args[x + 1]).FullName), args);
                        break;
                    }
                    catch (FileNotFoundException e)
                    {
                        Output.InitialError("A File Not Found Exception was generated upon"
                            + "reading the parameter file \"" + args[x + 1] + "\".\nHere it is:\n" + e,
                            false);
                        Environment.Exit(1); // This was originally part of the InitialError call in ECJ. But we make IslandExchange responsible.
                    }
                    catch (IOException e)
                    {
                        Output.InitialError("An IO Exception was generated upon reading the"
                            + "parameter file \"" + args[x + 1] + "\".\nHere it is:\n" + e,
                            false);
                        Environment.Exit(1); // This was originally part of the InitialError call in ECJ. But we make IslandExchange responsible.
                    }
                }
            if (parameters == null)
            {
                Output.InitialError("No parameter file was specified.", false);
                Environment.Exit(1); // This was originally part of the InitialError call in ECJ. But we make IslandExchange responsible.
            }

            // 1. create the output
            //store = (parameters.GetBoolean(new Parameter(Evolve.P_STORE),null,false));

            var output = new Output(true);

            // output.setFlush(
            //    parameters.GetBoolean(new Parameter(Evolve.P_FLUSH),null,false));


            // stdout is always log #0.  stderr is always log #1.
            // stderr accepts announcements, and both are fully verbose 
            // by default.

            output.AddLog(Log.D_STDOUT, false);
            output.AddLog(Log.D_STDERR, true);


            // this is an ugly, ugly, ugly, UGLY HACK
            // it will only work if we don't ask interesting things
            // of our "EvolutionState"  :-)  you know, things like
            // random number generators or generation numbers!

            var myEvolutionState = new EvolutionState { Parameters = parameters, Output = output };

            // set me up
            IParameter myBase = new Parameter(EvolutionState.P_EXCHANGER);

            var ie = (IslandExchange)parameters.GetInstanceForParameterEq(myBase, null, typeof(IslandExchange));

            ie.Setup(myEvolutionState, myBase);
            ie.FireUpServer(myEvolutionState, myBase);
            ie.ServerThread.Join();

            // flush the output
            output.Flush();
            Console.Error.Flush();
            Console.Out.Flush();
            Environment.Exit(0);
        }

        #endregion // Main
        #region ISerializable

        /// <summary>
        /// Custom serialization. 
        /// </summary>
        public virtual void GetObjectData(SerializationInfo outputInfo, StreamingContext context)
        {
            // this is all we need to write out -- everything else
            // gets recreated when we call ReinitializeContacts(...) again...

            outputInfo.AddValue("ec.exchange.IslandExchangedata1", ParamBase);
            outputInfo.AddValue("ec.exchange.IslandExchangedata2", ServerAddress);
            outputInfo.AddValue("ec.exchange.IslandExchangedata3", OwnId);
            outputInfo.AddValue("ec.exchange.IslandExchangedata4", CompressedCommunication);
            outputInfo.AddValue("ec.exchange.IslandExchangedata5", ImmigrantsSelectionMethod);
            outputInfo.AddValue("ec.exchange.IslandExchangedata6", IndsToDieSelectionMethod);
            outputInfo.AddValue("ec.exchange.IslandExchangedata7", ServerPort);
            outputInfo.AddValue("ec.exchange.IslandExchangedata8", ClientPort);
            outputInfo.AddValue("ec.exchange.IslandExchangedata9", AmServer);
        }

        /// <summary>
        /// Custom serialization. 
        /// </summary>
        protected IslandExchange(SerializationInfo inputInfo, StreamingContext context)
        {
            // this is all we need to read in -- everything else
            // gets recreated when we call ReinitializeContacts(...) again...

            ParamBase = (IParameter)(inputInfo.GetValue("ec.exchange.IslandExchangedata1", typeof(Object)));
            ServerAddress = ((String)(inputInfo.GetValue("ec.exchange.IslandExchangedata2", typeof(Object))));
            OwnId = ((String)(inputInfo.GetValue("ec.exchange.IslandExchangedata3", typeof(Object))));
            CompressedCommunication = inputInfo.GetBoolean("ec.exchange.IslandExchangedata4");
            ImmigrantsSelectionMethod = (SelectionMethod)(inputInfo.GetValue("ec.exchange.IslandExchangedata5", typeof(Object)));
            IndsToDieSelectionMethod = (SelectionMethod)(inputInfo.GetValue("ec.exchange.IslandExchangedata6", typeof(Object)));
            ServerPort = inputInfo.GetInt32("ec.exchange.IslandExchangedata7");
            ClientPort = inputInfo.GetInt32("ec.exchange.IslandExchangedata8");
            AmServer = inputInfo.GetBoolean("ec.exchange.IslandExchangedata9");
        }

        #endregion // ISerializable
    }
}