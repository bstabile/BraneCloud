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
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BraneCloud.Evolution.EC.Eval;
using BraneCloud.Evolution.EC.Evaluation;
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.CoEvolve;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.Runtime.Eval
{    
    /// <summary> 
    /// Slave
    /// 
    /// <p/>Slave is the main entry point for a slave evaluation process.  The slave works with a master process,
    /// receiving individuals from the master, evaluating them, and reporting the results back to the master, thus
    /// enabling distributed evolution.  
    /// 
    /// <p/>Slave replicates most of the functionality of
    /// the ec.Evolve class, for example in terms of parameters and checkpointing.  This is mostly because it needs
    /// to bootstrap and set up the EvolutionState in much the same way that ec.Evolve does.  Additionally, depending
    /// on settings below, the Slave may act like a mini-evolver on the individuals it receives from the master.
    /// 
    /// <p/>Like ec.Evolve, Slave is run with like this:
    /// 
    /// <p/><tt>java ec.eval.Slave -file </tt><i>parameter_file [</i><tt>-p </tt><i>parameter=value]*</i>
    /// 
    /// <p/>This starts a new slave, using the parameter file <i>parameter_file</i>.
    /// The user can provide optional overriding parameters on the command-line with the <tt>-p</tt> option.
    /// 
    /// <p/>Slaves need to know some things in order to run: the master's IP address and socket port number,
    /// whether to do compression, and whether or not to return individuals or just fitnesses.
    /// 
    /// <p/>Slaves presently always run in single-threaded mode and receive their random number generator seed
    /// from the master.  Thus they ignore any seed parameters given to them.
    /// 
    /// <p/>Slaves run in one of three modes:
    /// 
    /// <ul>
    /// <p/><li/>"Regular" mode, which does a loop where it receives N individuals, evaluates them, and
    /// returns either the individuals or their new fitnesses.
    /// <p/><li/>"Regular Coevolutionary" mode, which does a loop where it receives N individuals to assess together in
    /// a single coevolutionary evaluation, evaluates them, and returns either the individuals or their new fitnesses
    /// (or only some fitnesses if only some are requested).
    /// <p/><li/>"Evolve" mode, which does a loop where it receives
    /// N individuals, evaluates them, and if there's some more time left, does a little evolution on those individuals as
    /// if they were a population, then when the time is up, the current individuals in the population are returned in lieu
    /// of the original individuals.  In this second form, individuals MUST be returned, not fitnesses.  This mode is not
    /// available if you're doing coevolution.
    /// </ul>
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// 
    /// <tr><td valign="top"><tt>eval.slave-name</tt><br/>
    /// <font size="-1"> String </font></td>
    /// <td valign="top">(the slave's name, only for debugging purposes.  If not specified, the slave makes one up.)</td></tr>
    /// <tr><td valign="top"><tt>eval.master.host</tt><br/>
    /// <font size="-1"> String </font></td>
    /// <td valign="top">(the IP Address of the master.)</td></tr>
    /// <tr><td valign="top"><tt>eval.master.port</tt><br/>
    /// <font size="-1"> integer &gt;= 1024 </font></td>
    /// <td valign="top">(the socket port number of the master.)</td></tr>
    /// <tr><td valign="top"><tt>eval.compression</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> or <tt>false</tt> (default) </font></td>
    /// <td valign="top">(should we use compressed streams in communicating with the master?)</td></tr>
    /// <tr><td valign="top"><tt>eval.run-evolve</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> or <tt>false</tt> (default) </font></td>
    /// <td valign="top">(should we immediately evaluate the individuals and return them (or their fitnesses), or if we have extra time (defined by eval.runtime),
    /// should we do a little evolution on our individuals first?)</td></tr>
    /// <tr><td valign="top"><tt>eval.runtime</tt><br/>
    /// <font size="-1"> integer &gt; 0 </font></td>
    /// <td valign="top">(if eval.run-evolve is true, how long (in milliseconds wall-clock time) should we allow the individuals to evolve?)</td></tr>
    /// <tr><td valign="top"><tt>eval.return-inds</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> or <tt>false</tt> (default) </font></td>
    /// <td valign="top">(should we return whole individuals or (if false) just the fitnesses of the individuals?  This must be TRUE if eval.run-evolve is true.)</td></tr>
    /// <!-- 
    /// <tr><td valign="top"><tt>nostore</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">(should the ec.util.Output facility <i>not</i> store announcements in memory?)</td></tr>
    /// <tr><td valign="top"><tt>flush</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">(should I flush all output as soon as it's printed (useful for debugging when an exception occurs))</td></tr>
    /// -->
    /// <tr><td valign="top"><tt>state</tt><br/>
    /// <font size="-1">classname, inherits and != ec.EvolutionState</font></td>
    /// <td valign="top">(the EvolutionState object class)</td></tr>
    /// <tr><td valign="top"><tt>print-accessed-params</tt><br/>
    /// <font size="-1"/>bool = <tt>true</tt> or <tt>false</tt> (default)</td>
    /// <td valign="top">(at the end of a run, do we print out a list of all the parameters requested during the run?)</td></tr>
    /// <tr><td valign="top"><tt>print-used-params</tt><br/>
    /// <font size="-1"/>bool = <tt>true</tt> or <tt>false</tt> (default)</td>
    /// <td valign="top">(at the end of a run, do we print out a list of all the parameters actually <i>used</i> during the run?)</td></tr>
    /// <tr><td valign="top"><tt>print-unaccessed-params</tt><br/>
    /// <font size="-1"/>bool = <tt>true</tt> or <tt>false</tt> (default)</td>
    /// <td valign="top">(at the end of a run, do we print out a list of all the parameters NOT requested during the run?)</td></tr>
    /// <tr><td valign="top"><tt>print-unused-params</tt><br/>
    /// <font size="-1"/>bool = <tt>true</tt> or <tt>false</tt> (default)</td>
    /// <td valign="top">(at the end of a run, do we print out a list of all the parameters NOT actually used during the run?)</td></tr>
    /// <tr><td valign="top"><tt>print-all-params</tt><br/>
    /// <font size="-1"/>bool = <tt>true</tt> or <tt>false</tt> (default)</td>
    /// <td valign="top">(at the end of a run, do we print out a list of all the parameters stored in the parameter database?)</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.eval.Slave")]
    public class Slave
    {
        #region Constants

        public const string P_PRINTACCESSEDPARAMETERS = "print-accessed-params";
        public const string P_PRINTUSEDPARAMETERS = "print-used-params";
        public const string P_PRINTALLPARAMETERS = "print-all-params";
        public const string P_PRINTUNUSEDPARAMETERS = "print-unused-params";
        public const string P_PRINTUNACCESSEDPARAMETERS = "print-unaccessed-params";
        public const string P_EVALSLAVENAME = "eval.slave-name";
        public const string P_EVALMASTERHOST = "eval.master.host";
        public const string P_EVALMASTERPORT = "eval.master.port";
        public const string P_EVALCOMPRESSION = "eval.compression";
        public const string P_RETURNINDIVIDUALS = "eval.return-inds";

        public const string P_SUBPOP = "pop.subpop";

        /// <summary>
        /// The argument indicating that we're starting fresh from a new parameter file. 
        /// </summary>
        public const string A_FILE = "-file";

        /// <summary>
        /// State parameter. 
        /// </summary>
        public const string P_STATE = "state";

        /// <summary>
        /// Time to run evolution on the slaves in seconds. 
        /// </summary>
        public const string P_RUNTIME = "eval.runtime";

        /// <summary>
        /// Should slave run its own evolutionary process? 
        /// </summary>
        public const string P_RUNEVOLVE = "eval.run-evolve";

        /// <summary>
        /// How long we sleep in between attempts to connect to the master (in milliseconds). 
        /// </summary>
        public const int SLEEP_TIME = 100;

        #endregion // Constants
        #region Static

        public static int RunTime;

        public static bool RunEvolve;

        #endregion // Static
        #region Operations

        public static void EvaluateSimpleProblem(IEvolutionState state, bool returnIndividuals,
                                                     BinaryReader dataIn, BinaryWriter dataOut, string[] args)
        {
            ParameterDatabase parameters = null;

            // first load the individuals
            var numInds = 1;
            try
            {
                numInds = dataIn.ReadInt32();
            }
            catch (IOException e)
            {
                state.Output.Fatal("Unable to read the number of individuals from the master:\n" + e);
            }

            // load the subpops 
            var subpops = new int[numInds]; // subpops desired by each ind
            var indsPerSubpop = new int[state.Population.Subpops.Length]; // num inds for each subpop
            for (var i = 0; i < numInds; i++)
            {
                try
                {
                    subpops[i] = dataIn.ReadInt32();
                    if (subpops[i] < 0 || subpops[i] >= state.Population.Subpops.Length)
                        state.Output.Fatal("Bad subpop number for individual #" + i + ": " + subpops[i]);
                    indsPerSubpop[subpops[i]]++;
                }
                catch (IOException e)
                {
                    state.Output.Fatal("Unable to read the subpop number from the master:\n" + e);
                }
            }

            // Read the individual(s) from the stream and evaluate 

            var updateFitness = new bool[numInds];
            var inds = new Individual[numInds];

            // Either evaluate all the individuals once and return them immediately
            // (we'll do so in a steady-state-ish fashion, firing off threads as soon as we read in individuals,
            // and returning them as soon as they come in, albeit in the proper order)
            if (!RunEvolve)
            {
                var tasks = new Task<ECTaskArgs>[numInds];
                var problems = new ISimpleProblem[numInds];
                var indForThread = new int[numInds];

                try
                {
                    // start up all the threads
                    for (var i = 0; i < numInds; i++)
                    {
                        // load individual
                        inds[i] = state.Population.Subpops[subpops[i]].Species.NewIndividual(state, dataIn);
                        updateFitness[i] = dataIn.ReadBoolean();

                        problems[i] = ((ISimpleProblem) (state.Evaluator.p_problem.Clone()));
                        // TODO : Fix the Task implementation
                        //var task = new Task<ECTaskArgs>((o) =>
                        //                                     {
                        //                                         var a = (ECTaskArgs) o;
                        //                                         a.Problem.Evaluate(a.State, a.Individual, a.SubpopIndex,
                        //                                                            a.ThreadNum);
                        //                                         return a;
                        //                                     };
                        //                                );
                        //tasks[i] = task;
                    }
                    // gather everyone
                    for (var i = 0; i < numInds; i++)
                    {
                        ReturnIndividualsToMaster(state, inds, updateFitness, dataOut, returnIndividuals,
                                                      indForThread[i]); // return just that individual
                    }
                }
                catch (IOException e)
                {
                    state.Output.Fatal("Unable to read individual from master." + e);
                }
                try
                {
                    dataOut.Flush();
                }
                catch (IOException e)
                {
                    state.Output.Fatal("Caught fatal IOException\n" + e);
                }
            }

            // OR we will do some evolution.  Here we'll read in ALL the individuals, do some evolution, then
            // write them ALL out, very slightly less efficient
            else // (runEvolve) 
            {
                try // load up all the individuals
                {
                    for (var i = 0; i < numInds; i++)
                    {
                        inds[i] = state.Population.Subpops[subpops[i]].Species.NewIndividual(state, dataIn);
                        updateFitness[i] = dataIn.ReadBoolean();
                    }
                }
                catch (IOException e)
                {
                    state.Output.Fatal("Unable to read individual from master." + e);
                }

                var stopWatch = Stopwatch.StartNew();

                // Now we need to reset the subpopulations.  They were already set up with the right
                // classes, Species, etc. in state.setup(), so all we need to do is modify the number
                // of individuals in each subpopulation.

                for (var subpop = 0; subpop < state.Population.Subpops.Length; subpop++)
                {
                    if (state.Population.Subpops[subpop].Individuals.Length != indsPerSubpop[subpop])
                        state.Population.Subpops[subpop].Individuals = new Individual[indsPerSubpop[subpop]];
                }

                // Disperse into the population
                var counts = new int[state.Population.Subpops.Length];
                for (var i = 0; i < numInds; i++)
                    state.Population.Subpops[subpops[i]].Individuals[counts[subpops[i]]++] = inds[i];

                // Evaluate the population until time is up, or the evolution stops
                var result = EvolutionState.R_NOTDONE;
                while (result == EvolutionState.R_NOTDONE)
                {
                    result = state.Evolve();
                    if (stopWatch.ElapsedMilliseconds > RunTime)
                        break;
                }

                // re-gather from population in the same order
                counts = new int[state.Population.Subpops.Length];
                for (var i = 0; i < numInds; i++)
                    inds[i] = state.Population.Subpops[subpops[i]].Individuals[counts[subpops[i]]++];

                state.Finish(result);
                Evolve.Cleanup(state);

                // Return the evaluated individual to the master
                try
                {
                    ReturnIndividualsToMaster(state, inds, updateFitness, dataOut, returnIndividuals, -1);
                    // -1 == write all individuals
                    dataOut.Flush();
                }
                catch (IOException e)
                {
                    state.Output.Fatal("Caught fatal IOException\n" + e);
                }
            }
        }

        private static ECTaskArgs RunSimpleProblem(ECTaskArgs args)
        {
            args.Problem.Evaluate(args.State, args.Individual, args.SubpopIndex, args.ThreadNum);
            var fitness = args.Individual.Fitness;
            return args;
        }

        public static void EvaluateGroupedProblem(IEvolutionState state, bool returnIndividuals, BinaryReader dataIn, BinaryWriter dataOut)
        {
            var countVictoriesOnly = false;

            // first load the individuals
            var numInds = 1;
            try
            {
                countVictoriesOnly = dataIn.ReadBoolean();
                numInds = dataIn.ReadInt32();
            }
            catch (IOException e)
            {
                state.Output.Fatal("Unable to read the number of individuals from the master:\n" + e);
            }

            // load the subpops 
            var subpops = new int[numInds]; // subpops desired by each ind
            var indsPerSubpop = new int[state.Population.Subpops.Length]; // num inds for each subpop
            for (var i = 0; i < numInds; i++)
            {
                try
                {
                    subpops[i] = dataIn.ReadInt32();
                    if (subpops[i] < 0 || subpops[i] >= state.Population.Subpops.Length)
                        state.Output.Fatal("Bad subpop number for individual #" + i + ": " + subpops[i]);
                    indsPerSubpop[subpops[i]]++;
                }
                catch (IOException e)
                {
                    state.Output.Fatal("Unable to read the subpop number from the master:\n" + e);
                }
            }

            // Read the individuals from the stream
            var inds = new Individual[numInds];
            var updateFitness = new bool[numInds];
            try
            {
                for (var i = 0; i < inds.Length; ++i)
                {
                    inds[i] = state.Population.Subpops[subpops[i]].Species.NewIndividual(state, dataIn);
                    updateFitness[i] = dataIn.ReadBoolean();
                }
            }
            catch (IOException e)
            {
                state.Output.Fatal("Unable to read individual from master.");
            }

            // Evaluate the individuals together
            ((IGroupedProblem) (state.Evaluator.p_problem)).Evaluate(state, inds, updateFitness, countVictoriesOnly, subpops, 0);

            try
            {
                ReturnIndividualsToMaster(state, inds, updateFitness, dataOut, returnIndividuals, -1);
                    // -1 == write all individuals
                dataOut.Flush();
            }
            catch (IOException e)
            {
                state.Output.Fatal("Caught fatal IOException\n" + e);
            }
        }

        private static void ReturnIndividualsToMaster(
            IEvolutionState state, IList<Individual> inds,
            IList<bool> updateFitness, BinaryWriter dataOut,
            bool returnIndividuals, int individualInQuestion)
        {
            // Return the evaluated individual to the master
            // just write evaluated and fitness
            var startInd = (individualInQuestion == -1 ? 0 : individualInQuestion);
            var endInd = (individualInQuestion == -1 ? inds.Count : individualInQuestion + 1);
            for (var i = startInd; i < endInd; i++)
            {
                dataOut.Write((byte) (returnIndividuals
                                          ? SlaveReturnType.Individual
                                          : (updateFitness[i]
                                                 ? SlaveReturnType.Fitness
                                                 : SlaveReturnType.Nothing)));

                if (returnIndividuals)
                {
                    inds[i].WriteIndividual(state, dataOut);
                }
                else if (updateFitness[i])
                {
                    dataOut.Write(inds[i].Evaluated);
                    inds[i].Fitness.WriteFitness(state, dataOut);
                }
            }
        }

        #endregion // Operations
        #region Main

        [STAThread]
        public static void Main(string[] args)
        {
            IEvolutionState state = null;
            IParameterDatabase parameters = null;
            Output output = null;

            //bool store;
            int x;

            // 0. find the parameter database
            for (x = 0; x < args.Length - 1; x++)
                if (args[x].Equals(A_FILE))
                {
                    try
                    {
                        parameters = new ParameterDatabase(new FileInfo(new FileInfo(args[x + 1]).FullName), args);

                        // add the fact that I am a slave:      eval.i-am-slave = true
                        // this is used only by the Evaluator to determine whether to use the MasterProblem
                        parameters.SetParameter(new Parameter(EvolutionState.P_EVALUATOR).Push(Evaluator.P_IAMSLAVE),
                                                "true");
                        break;
                    }
                    catch (FileNotFoundException e)
                    {
                        Output.InitialError(
                            "A File Not Found Exception was generated upon" + " reading the parameter file \""
                            + args[x + 1] + "\".\nHere it is:\n" + e, false);
                        Environment.Exit(1);
                            // This was originally part of the InitialError call in ECJ. But we make Slave responsible.
                    }
                    catch (IOException e)
                    {
                        Output.InitialError("An IO Exception was generated upon reading the" + " parameter file \""
                                            + args[x + 1] + "\".\nHere it is:\n" + e, false);
                        Environment.Exit(1);
                            // This was originally part of the InitialError call in ECJ. But we make Slave responsible.
                    }
                }
            if (parameters == null)
            {
                Output.InitialError("No parameter file was specified.", false);
                Environment.Exit(1);
                    // This was originally part of the InitialError call in ECJ. But we make Slave responsible.
            }

            // 5. Determine whether or not to return entire Individuals or just Fitnesses
            //    (plus whether or not the Individual has been evaluated).

            var returnIndividuals = parameters.GetBoolean(new Parameter(P_RETURNINDIVIDUALS), null, false);


            // 6. Open a server socket and listen for requests
            var slaveName = parameters.GetString(new Parameter(P_EVALSLAVENAME), null);

            var masterHost = parameters.GetString(new Parameter(P_EVALMASTERHOST), null);
            var masterPort = parameters.GetInt(new Parameter(P_EVALMASTERPORT), null);
            var useCompression = parameters.GetBoolean(new Parameter(P_EVALCOMPRESSION), null, false);

            RunTime = parameters.GetInt(new Parameter(P_RUNTIME), null, 0);

            RunEvolve = parameters.GetBoolean(new Parameter(P_RUNEVOLVE), null, false);

            if (RunEvolve && !returnIndividuals)
            {
                Output.InitialError(
                    "You have the slave running in 'evolve' mode, but it's only returning fitnesses to the master, not whole individuals.  This is almost certainly wrong.",
                    new Parameter(P_RUNEVOLVE), new Parameter(P_RETURNINDIVIDUALS));
                Environment.Exit(1);
                    // This was originally part of the InitialError call in ECJ. But we make Slave responsible.
            }

            Output.InitialMessage("ECCS Slave");
            if (RunEvolve)
                Output.InitialMessage("Running in Evolve mode, evolve time is " + RunTime + " milliseconds");
            if (returnIndividuals)
                Output.InitialMessage("Whole individuals will be returned");
            else
                Output.InitialMessage("Only fitnesses will be returned");


            // Continue to serve new masters until killed.
            while (true)
            {
                try
                {
                    TcpClient socket;
                    long connectAttemptCount = 0;
                    Output.InitialMessage("Connecting to master at " + masterHost + ":" + masterPort);
                    while (true)
                    {
                        try
                        {
                            socket = new TcpClient(masterHost, masterPort);
                            break;
                        }
                        catch (Exception)
                            // it's not up yet...
                        {
                            connectAttemptCount++;
                            try
                            {
                                Thread.Sleep(new TimeSpan((Int64) 10000*SLEEP_TIME));
                            }
                            catch (ThreadInterruptedException)
                            {
                            }
                        }
                    }
                    Output.InitialMessage("Connected to master after " + (connectAttemptCount*SLEEP_TIME) + " ms");

                    BinaryReader dataIn = null;
                    BinaryWriter dataOut = null;

                    try
                    {
                        Stream tmpIn = socket.GetStream();
                        Stream tmpOut = socket.GetStream();
                        if (useCompression)
                        {
                            //Output.InitialError("JDK 1.5 has broken compression.  For now, you must set eval.compression=false");
                            //Environment.Exit(1); // This was originally part of the InitialError call in ECJ. But we make Slave responsible.
                            /*
                            tmpIn = new CompressingInputStream(tmpIn);
                            tmpOut = new CompressingOutputStream(tmpOut);
                            */
                            tmpIn = Output.MakeCompressingInputStream(tmpIn);
                            tmpOut = Output.MakeCompressingOutputStream(tmpOut);
                            if (tmpIn == null || tmpOut == null)
                            {
                                Output.InitialError(
                                    "You do not appear to have JZLib installed on your system, and so must set eval.compression=false.  "
                                    +
                                    "To get JZLib, download from the ECJ website or from http://www.jcraft.com/jzlib/",
                                    false);
                                Environment.Exit(1);
                                    // This was originally part of the InitialError call in ECJ. But we make Slave responsible.
                            }
                        }

                        dataIn = new BinaryReader(tmpIn);
                        dataOut = new BinaryWriter(tmpOut);
                    }
                    catch (IOException e)
                    {
                        Output.InitialError("Unable to open input stream from socket:\n" + e, false);
                        Environment.Exit(1);
                            // This was originally part of the InitialError call in ECJ. But we make Slave responsible.
                    }

                    // specify the slaveName
                    if (slaveName == null)
                    {
                        // BRS : TODO : Check equivalence of the address returned from .NET socket.Client.LocalEndPoint
                        slaveName = socket.Client.LocalEndPoint + "/" + (DateTime.Now.Ticks - 621355968000000000)/10000;
                        Output.InitialMessage("No slave name specified.  Using: " + slaveName);
                    }

                    dataOut.Write(slaveName); // Default encoding of BinaryWriter is UTF-8
                    dataOut.Flush();

                    // 1. create the output
                    // store = parameters.GetBoolean(new Parameter(P_STORE), null, false);

                    if (output != null)
                        output.Close();
                    output = new Output(true);

                    //output.setFlush(
                    //    parameters.GetBoolean(new Parameter(P_FLUSH),null,false));

                    // stdout is always log #0. stderr is always log #1.
                    // stderr accepts announcements, and both are fully verbose
                    // by default.
                    output.AddLog(Log.D_STDOUT, false);
                    output.AddLog(Log.D_STDERR, true);

                    output.SystemMessage(ECVersion.Message());


                    // 2. set up thread values

                    /*
                    int breedthreads = parameters.GetInt(
                    new Parameter(Evolve.P_BREEDTHREADS),null,1);
                    
                    if (breedthreads < 1)
                    output.Fatal("Number of breeding threads should be an integer >0.",
                    new Parameter(Evolve.P_BREEDTHREADS),null);
                    
                    
                    int evalthreads = parameters.GetInt(
                    new Parameter(Evolve.P_EVALTHREADS),null,1);
                    
                    if (evalthreads < 1)
                    output.Fatal("Number of eval threads should be an integer >0.",
                    new Parameter(Evolve.P_EVALTHREADS),null);*/

                    var breedthreads = Evolve.DetermineThreads(output, parameters, new Parameter(Evolve.P_BREEDTHREADS));
                    var evalthreads = Evolve.DetermineThreads(output, parameters, new Parameter(Evolve.P_EVALTHREADS));

                    // Note that either breedthreads or evalthreads (or both) may be 'auto'.  We don't warn about this because
                    // the user isn't providing the thread seeds.


                    // 3. create the Mersenne Twister random number generators,
                    // one per thread

                    IMersenneTwister[] random =
                        new MersenneTwisterFast[breedthreads > evalthreads ? breedthreads : evalthreads];

                    var seed = dataIn.ReadInt32();
                    for (var i = 0; i < random.Length; i++)
                        random[i] = Evolve.PrimeGenerator(new MersenneTwisterFast(seed++));
                            // we prime the generator to be more sure of randomness.

                    // 4. Set up the evolution state

                    // what evolution state to use?
                    state =
                        (IEvolutionState)
                        parameters.GetInstanceForParameter(new Parameter(P_STATE), null, typeof (IEvolutionState));
                    state.Parameters = new ParameterDatabase();
                    state.Parameters.AddParent(parameters);
                    state.Random = random;
                    state.Output = output;
                    state.EvalThreads = evalthreads;
                    state.BreedThreads = breedthreads;

                    state.Setup(state, null);
                    state.Population = state.Initializer.SetupPopulation(state, 0);


                    // 5. Optionally do further loading
                    var storage = state.Evaluator.MasterProblem;
                    storage.ReceiveAdditionalData(state, dataIn);
                    storage.TransferAdditionalData(state);

                    try
                    {
                        while (true)
                        {
                            var newState = state;

                            if (RunEvolve)
                            {
                                // Construct and use a new EvolutionState.  This will be inefficient the first time around
                                // as we've set up TWO EvolutionStates in a row with no good reason.
                                IParameterDatabase coverDatabase = new ParameterDatabase();
                                    // protect the underlying one
                                coverDatabase.AddParent(state.Parameters);
                                newState = Evolve.Initialize(coverDatabase, 0);
                                newState.StartFresh();
                                newState.Output.Message("Replacing random number generators, ignore above seed message");
                                newState.Random = state.Random; // continue with RNG
                                storage.TransferAdditionalData(newState); // load the arbitrary data again
                            }

                            // 0 means to shut down
                            Console.Error.WriteLine("reading next problem");
                            int problemType = dataIn.ReadByte();
                            Console.Error.WriteLine("Read problem: " + problemType);
                            switch (problemType)
                            {

                                case (int) SlaveEvaluationType.Shutdown:
                                    socket.Close();
                                    return; // we're outa here

                                case (int) SlaveEvaluationType.Simple:
                                    EvaluateSimpleProblem(newState, returnIndividuals, dataIn, dataOut, args);
                                    break;

                                case (int) SlaveEvaluationType.Grouped:
                                    EvaluateGroupedProblem(newState, returnIndividuals, dataIn, dataOut);
                                    break;

                                default:
                                    state.Output.Fatal("Unknown problem form specified: " + problemType);
                                    break;

                            }
                            //System.err.PrintLn("Done Evaluating Individual");
                        }
                    }
                    catch (IOException e)
                    {
                        // Since an IOException can happen here if the peer closes the socket
                        // on it's end, we don't necessarily have to exit.  Maybe we don't
                        // even need to print a warning, but we'll do so just to indicate
                        // something happened.
                        state.Output.Warning(
                            "Unable to read type of evaluation from master.  Maybe the master closed its socket and exited?:\n" +
                            e);
                    }
                }
                catch (Exception e)
                {
                    state.Output.Fatal(e.Message);
                }
            }
        }

        #endregion // Main
    }

}