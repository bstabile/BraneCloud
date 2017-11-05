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
using System.Text;
using System.Diagnostics;

using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Randomization;
using File = System.IO.File;

namespace BraneCloud.Evolution.EC
{
    /// <summary> 
    /// Evolve is the main entry class for an evolutionary computation run.
    /// 
    /// <p/> An EC run is done with one of two argument formats:
    /// 
    /// <p/><tt>java ec.Evolve -file </tt><i>parameter_file [</i><tt>-p </tt><i>parameter=value]*</i>
    /// 
    /// <p/>This starts a new evolutionary run, using the parameter file <i>parameter_file</i>.
    /// The user can provide optional overriding parameters on the command-line with the <tt>-p</tt> option.
    /// 
    /// <p/><tt>java ec.Evolve -checkpoint </tt><i>checkpoint_file</i>
    /// 
    /// <p/>This starts up an evolutionary run from a previous checkpoint file.
    /// 
    /// <p/>The basic Evolve class has a main() loop with a simple job iteration facility.
    /// If you'd like to run the evolutionary system four times, each with a different random
    /// seed, you might do:
    /// 
    /// <p/><tt>java ec.Evolve -file </tt><i>parameter_file</i> <tt>-p jobs=4</tt>
    /// 
    /// <p/>Here, Evolve will run the first time with the random seed equal to whatever's specified
    /// in your file, then job#2 will be run with the seed + 1, job#3 with the seed + 2, 
    /// and job#4 with the seed + 3.  If you have multiple seeds, ECJ will try to make sure they're
    /// all different even across jobs by adding the job number * numberOfSeeds to each of them.
    /// This means that if you're doing multiple jobs with multiple seeds, you should probably set
    /// seed.0 to x, seed.1 to x+1, seed.2 to x+2, etc. for best results.  It also works if seed.0
    /// is x, seed.1 is y (a number much bigger than x), seed.2 is z (a number much bigger than y) etc.
    /// 
    /// If you set seed.0=time etc. for multiple jobs, the values of each seed will be set to the
    /// current time that the job starts plus the job number * numberOfSeeds.  As current time always
    /// goes up, this shouldn't be an issue.  However it's theoretically possible that if you checkpoint and restart
    /// on another system with a clock set back in time, you could get the same seed in a later job.
    /// 
    /// <p/><b>Main() has been designed to be modified.</b>  The comments for the Evolve.java file contain
    /// a lot discussion of how ECJ's Main() bootstraps the EvolutionState object and runs it, plus a much
    /// simpler example of Main() and explanations for how Main() works.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><tt>jobs</tt><br/>
    /// <font size="-1"> int >= 1 (default)</font></td>
    /// <td valign="top">(The number of jobs to iterate.  
    /// The current job number (0...jobs-1) will be added to each seed UNLESS the seed is loaded from the system time.  
    /// The job number also gets added as a prefix (if the number of jobs is more than 1)).</td></tr>
    /// <tr><td valign="top"><tt>nostore</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">(should the ec.util.Output facility <i>not</i> store announcements in memory?)</td></tr>
    /// <tr><td valign="top"><tt>flush</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">(should I flush all output as soon as it's printed (useful for debugging when an exception occurs))</td></tr>
    /// <tr><td valign="top"><tt>evalthreads</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(the number of threads to spawn for evaluation)</td></tr>
    /// <tr><td valign="top"><tt>breedthreads</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(the number of threads to spawn for breeding)</td></tr>
    /// <tr><td valign="top"><tt>seed.</tt><i>n</i><br/>
    /// <font size="-1">int != 0, or string  = <tt>time</tt></font></td>
    /// <td valign="top">(the seed for random number generator #<i>n</i>.  <i>n</i> should range from 0 to Max(evalthreads,breedthreads)-1.  
    /// If value is <tt>time</tt>, then the seed is based on the system clock plus <i>n</i>.)</td></tr>
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
    [ECConfiguration("ec.problems.tutorial1.MyEvolve")]
    public class MyEvolve
    {
        public const string P_PRINTACCESSEDPARAMETERS = "print-accessed-params";
        public const string P_PRINTUSEDPARAMETERS = "print-used-params";
        public const string P_PRINTALLPARAMETERS = "print-all-params";
        public const string P_PRINTUNUSEDPARAMETERS = "print-unused-params";
        public const string P_PRINTUNACCESSEDPARAMETERS = "print-unaccessed-params";

        /// <summary>The argument indicating that we're starting up from a checkpoint file. </summary>
        public const string A_CHECKPOINT = "-checkpoint";

        /// <summary>The argument indicating that we're starting fresh from a new parameter file. </summary>
        public const string A_FILE = "-file";

        /// <summary>evalthreads parameter </summary>
        public const string P_EVALTHREADS = "evalthreads";

        /// <summary>breedthreads parameter </summary>
        public const string P_BREEDTHREADS = "breedthreads";

        /// <summary>seed parameter </summary>
        public const string P_SEED = "seed";

        /// <summary>'time' seed parameter value </summary>
        public const string V_SEED_TIME = "time";

        /// <summary>state parameter </summary>
        public const string P_STATE = "state";

        /// <summary>'auto' thread parameter value </summary>
        public const string V_THREADS_AUTO = "auto";


        /// <summary>
        /// Begins a fresh evolutionary run with a given state.  The state should have been
        /// provided by Initialize(...).  The jobPrefix is added to the front of output and
        /// checkpoint filenames.  If it's null, nothing is added to the front.  
        /// </summary>

        /*		
        * MAIN
        * 
        * Evolve has... evolved from previous Evolves.  The goal behind these changes is:
        *       1. To provide a simple jobs facility
        *       2. To make it easy for you to make your own main(), including more
        *          sophisticated jobs facilities.
        * 
        * Before we get into the specifics of this file, let's first look at the main
        * evolution loop in EvolutionState.  The general code is:
        *       1.  If I was loaded from a checkpoint, call the hook StartFromCheckpoint()
        *       2.  If I'm instead starting from scratch, call the hook StartFresh() 
        *       3.  Loop:
        *               4. result = Evolve() 
        *               5. If result != EvolutionState.R_NOTDONE, break from loop
        *       6.      Call the hook Finish(result)
        * 
        * That's all there's to it.  Various EvolutionState classes need to implement
        * the StartFromCheckpoint, StartFresh, Evolve, and Finish methods.  This basic
        * evolution loop is encapsulated in a convenience method called EvolutionState.Run(...).
        * 
        * Evolve is little more than code to fire up the right EvolutionState class,
        * call Run(...), and then shut down.  The complexity mostly comes from bringing
        * up the class (loading it from checkpoint or from scratch) and in shutting down.
        * Here's the general mechanism:
        * 
        * - To load from checkpoint, we must find the checkpoint filename and call
        *       Checkpoint.RestoreFromCheckpoint(filename) to generate the EvolutionState
        *       instance.  Evolve provides a convenience function for this called
        *       PossiblyRestoreFromCheckpoint(...), which returns null if there <i>isn't</i>
        *       a checkpoint file to load from.  Else it returns the unfrozen EvolutionState.
        *       
        * - To instead set up from scratch, you have to do a bunch of stuff to set up the state.
        *       First, you need to load a parameter database.  Evolve has a convenience function
        *       for that called LoadParameterDatabase(...).  Second, you must do a series
        *       of items: (1) generate an Output object (2) identify the number of threads
        *       (3) create the IMersenneTwister random number generators (4) instantiate
        *       the EvolutionState subclass instance (5) plug these items, plus the random 
        *       seed offset and the parameter database, into the instance.  These five
        *       steps are done for you in a convenience function called Initialize(...).
        * 
        * -     Now the state is ready to go. Call Run(...) on your EvolutionState
        *       (or do the evolution loop described above manually if you wish)
        * 
        * - Finally, to shut down, you need to (1) flush the Output (2) print out
        *       the used, accessed, unused, unaccessed, and all parameters if the user
        *       requested a printout at the end [rarely] (3) flush err and out
        *       for good measure, and (4) close Output -- which closes its streams except
        *       for Err and Out.  There is a convenience function for this as
        *       well.  It's called Cleanup(...).
        *       
        * - Last, you shut down with Exit(0) -- very important because it quits
        *       any remaining threads the user might have had running and forgot about.
        *       
        * So there you have it.  Several convenience functions in Evolve...
        *       Evolve.PossiblyRestoreFromCheckpoint
        *       Evolve.LoadParameterDatabase
        *       Evolve.Initialize
        *       EvolutionState.Run
        *       Evolve.Cleanup
        * ... result in a very simple basic Main() function:
        *       
        *
        *               public static void Main(String[] args)
        *                       {
        *                       IEvolutionState state = PossiblyRestoreFromCheckpoint(args);
        *                       if (state != null)  // loaded from checkpoint
        *                               state.Run(EvolutionState.C_STARTED_FROM_CHECKPOINT);
        *                       else
        *                               {
        *                               state = Initialize(LoadParameterDatabase(args), 0);
        *                               state.Run(EvolutionState.C_STARTED_FRESH);
        *                               }
        *                       Cleanup(state);
        *                       Exit(0);
        *                       }
        *
        *
        * Piece of cake!
        * 
        * The more extravagant Main(...) you see below just has a few extra gizmos for
        * doing basic job iteration.  EvolutionState has two convenience slots for
        * doing job iteration:
        *
        *       Job                     (an Object[]    use this as you like)
        *       RuntimeArguments        (a  String[]    put args in here)
        *
        * The reason these are slots in EvolutionState is so you can store this information
        * across checkpoints and continue where you had started job-number-wise when the
        * user starts up from a checkpoint again.
        * 
        * You'll probably want the EvolutionState to output its stat files etc. using unique
        * prefixes to differentiate between jobs (0.stat, 1.stat, or whatever you like -- it
        * doesn't have to be numbers), and you'll also probably want checkpoint files to be
        * similarly prefixed.  So you'll probably want to do:
        *
        *       state.Output.SetFilePrefix(jobPrefix);
        *       state.CheckpointPrefix = jobPrefix + state.CheckpointPrefix;
        *
        * The extravagant main below is basically doing this.  We're using state.Job to stash
        * away a single iterated job number, stored as an Integer in state.Job[0], and then
        * iterating that way, making sure we stash the job number and runtime arguments each time 
        * so we can recover them when loading from checkpoint.  We use the "jobs" parameter 
        * to determine how many jobs to run.  If this number is 1, we don't even bother to set
        * the file prefixes, so ECJ generates files just like it used to.
        *
        * It's important to note that this main was created with the assumption that you might
        * modify it for your own purposes.  Do you want a nested loop, perhaps to do all combinations
        * of two parameters or something?  Rewrite it to use two array slots in the job array.
        * Want to store more information on a per-job basis?  Feel free to use the job array any
        * way you like -- it's ONLY used by this Main() loop.
        */
        /// <summary>
        /// Top-level evolutionary loop.  
        /// </summary>
        [STAThread]
        public static void Run(string paramsFileName)
        {
            EvolutionState state;

            var currentJob = 0; // the next job number (0 by default)

            // Now we're going to load the parameter database to see if there are any more jobs.
            // We could have done this using the previous parameter database, but it's no big deal.
            var parameters = LoadParameterDatabase(paramsFileName);

            if (currentJob == 0)  // no current job number yet
                currentJob = parameters.GetIntWithDefault(new Parameter("current-job"), null, 0);
            if (currentJob < 0)
            {
                Output.InitialError("The 'current-job' parameter must be >= 0 (or not exist, which defaults to 0)", false);
                Environment.Exit(1); // This was originally part of the InitialError call in ECJ. But we make Evolve responsible.
            }

            var numJobs = parameters.GetIntWithDefault(new Parameter("jobs"), null, 1);
            if (numJobs < 1)
            {
                Output.InitialError("The 'jobs' parameter must be >= 1 (or not exist, which defaults to 1)", false);
                Environment.Exit(1); // This was originally part of the InitialError call in ECJ. But we make Evolve responsible.
            }

            const int job = 0;
            try
            {
                // Initialize the EvolutionState, then set its job variables
                state = Initialize(parameters, job); // pass in job# as the seed increment
                state.Output.SystemMessage("Job: " + job);
                state.Job = new Object[1]; // make the job argument storage
                state.Job[0] = job; // stick the current job in our job storage
                state.RuntimeArguments = new string[0]; // stick the runtime arguments in our storage

                // Here you can set up the EvolutionState's parameters further before it's Setup(...).
                // This includes replacing the random number generators, changing values in state.Parameters,
                // changing instance variables (except for job and runtimeArguments, please), etc.


                // now we let it go
                state.Run(EvolutionState.C_STARTED_FRESH);
                Cleanup(state); // flush and close various streams, print out parameters if necessary
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                GC.Collect(); // take a shot!
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        /// <summary>
        /// Initializes an evolutionary run given the parameters and a random seed adjustment (added to each random seed).
        /// The adjustment offers a convenient way to change the seeds of the random number generators each time you
        /// do a new evolutionary run.  You are of course welcome to replace the random number generators after initialize(...)
        /// but before startFresh(...) 
        /// </summary>
        public static EvolutionState Initialize(IParameterDatabase parameters, int randomSeedOffset)
        {
            var output = new Output(true);

            // stdout is always log #0.  stderr is always log #1.
            // stderr accepts announcements, and both are fully verbose 
            // by default.
            output.AddLog(Log.D_STDOUT, false);
            output.AddLog(Log.D_STDERR, true);

            // now continue intialization
            return Initialize(parameters, randomSeedOffset, output);
        }

        /// <summary>
        /// Initializes an evolutionary run given the parameters and a random seed adjustment (added to each random seed),
        /// with the Output pre-constructed.
        /// The adjustment offers a convenient way to change the seeds of the random number generators each time you
        /// do a new evolutionary run.  You are of course welcome to replace the random number generators after initialize(...)
        /// but before startFresh(...).
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="randomSeedOffset"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static EvolutionState Initialize(IParameterDatabase parameters, int randomSeedOffset, Output output)
        {
            var breedthreads = 1;
            var evalthreads = 1;
            //bool store;
            //int x;

            // output was already created for us.  
            output.SystemMessage(ECVersion.Message());

            // 2. set up thread values

            breedthreads = DetermineThreads(output, parameters, new Parameter(P_BREEDTHREADS));
            evalthreads = DetermineThreads(output, parameters, new Parameter(P_EVALTHREADS));

            var auto = (V_THREADS_AUTO.ToUpper().Equals(parameters.GetString(new Parameter(P_BREEDTHREADS), null).ToUpper())
                         ||
                         V_THREADS_AUTO.ToUpper().Equals(
                             parameters.GetString(new Parameter(P_EVALTHREADS), null).ToUpper()));
            // at least one thread is automatic.  Seeds may need to be dynamic.

            // 3. create the Mersenne Twister random number generators,
            // one per thread

            var random = new MersenneTwisterFast[1];

            var seedMessage = "Seed: ";

            // Get time in milliseconds
            var time = (int)DateTimeHelper.CurrentTimeMilliseconds;

            var seed = 0;
            seed = DetermineSeed(output, parameters, new Parameter(P_SEED).Push("" + 0),
                                        time + 0, random.Length * randomSeedOffset, auto);

            random[0] = PrimeGenerator(new MersenneTwisterFast(seed));
            // we prime the generator to be more sure of randomness.
            seedMessage = seedMessage + seed + " ";

            // 4.  Start up the evolution

            // what evolution state to use?
            var state = (EvolutionState)parameters.GetInstanceForParameter(new Parameter(P_STATE), null, typeof(IEvolutionState));
            state.Parameters = parameters;
            state.Random = random;
            state.Output = output;
            state.EvalThreads = evalthreads;
            state.BreedThreads = breedthreads;
            state.RandomSeedOffset = randomSeedOffset;

            output.SystemMessage($"Threads:  breed/{breedthreads} eval/{evalthreads}");
            output.SystemMessage(seedMessage);

            return state;
        }

        /// <summary>Loads a ParameterDatabase from checkpoint if "-params" is in the command-line arguments. </summary>
        public static IParameterDatabase LoadParameterDatabase(string paramsFileName)
        {
            if (string.IsNullOrEmpty(paramsFileName))
            {
                Output.InitialError("The parameter file name is null or empty.", false);
                Environment.Exit(1); // This was originally part of the InitialError call in ECJ. But we make Evolve responsible.
            }

            if (!File.Exists(paramsFileName))
            {
                Output.InitialError($"The specified parameter file does not exist: {paramsFileName}", false);
                Environment.Exit(1); // This was originally part of the InitialError call in ECJ. But we make Evolve responsible.
            }

            IParameterDatabase parameters = null;
            try
            {
                if (!string.IsNullOrEmpty(paramsFileName))
                    parameters = new ParameterDatabase(new FileInfo(paramsFileName));
            }
            catch (Exception e)
            {
                Output.InitialError($"Error reading the parameter file \"{paramsFileName}\": {e.Message}", false);
                Environment.Exit(1); // This was originally part of the InitialError call in ECJ. But we make Evolve responsible.
            }
            if (parameters == null)
            {
                Output.InitialError("No parameter file was specified.", false);
                Environment.Exit(1); // This was originally part of the InitialError call in ECJ. But we make Evolve responsible.
            }
            return parameters;
        }

        /// <summary>Loads the number of threads. </summary>
        public static int DetermineThreads(Output output, IParameterDatabase parameters, IParameter threadParameter)
        {
            var thread = 1;
            var tmp_s = parameters.GetString(threadParameter, null);
            if (tmp_s == null)
            // uh oh
            {
                output.Fatal("Threads number must exist.", threadParameter, null);
            }
            else if (V_THREADS_AUTO.ToUpper().Equals(tmp_s.ToUpper()))
            {
            }
            else
            {
                try
                {
                    thread = parameters.GetInt(threadParameter, null);
                }
                catch (FormatException)
                {
                    output.Fatal("Invalid, non-integer threads value (" + thread + ")", threadParameter, null);
                }
            }
            return thread;
        }

        /// <summary>
        /// Primes the generator.  Mersenne Twister seeds its first 624 numbers using a basic
        /// linear congruential generator; thereafter it uses the MersenneTwister algorithm to
        /// build new seeds.  Those first 624 numbers are generally just fine, but to be extra
        /// safe, you can prime the generator by calling nextInt() on it some (N>1) * 624 times.
        /// This method does exactly that, presently with N=2. 
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        public static MersenneTwisterFast PrimeGenerator(MersenneTwisterFast generator)
        {
            for (var i = 0; i < 624 * 2; i++)
                generator.NextInt();
            return generator;
        }

        /// <summary>
        /// Loads a random generator seed.  First, the seed is loaded from the seedParameter.  If the parameter
        /// is V_SEED_TIME, the seed is set to the currentTime value.  Then the seed is incremented by the offset. 
        /// This method is broken out of initialize(...) primarily to share code with ec.eval.MasterProblem.
        /// </summary>
        public static int DetermineSeed(Output output, IParameterDatabase parameters, IParameter seedParameter, long currentTime, int offset, bool auto)
        {
            var seed = 1; // have to initialize to make the compiler happy
            try
            {
                seed = parameters.GetInt(seedParameter, null);
            }
            catch (FormatException)
            {
                output.Fatal("Invalid, non-integer seed value (" + seed + ")", seedParameter, null);
            }
            return seed + offset;
        }

        public static void Cleanup(IEvolutionState state)
        {
            // flush the output
            state.Output.Flush();

            // Possibly print out the run parameters
            var pw = new StreamWriter(Console.OpenStandardError(), Encoding.Default);

            // before we print out access information, we need to still "get" these
            // parameters, so that they show up as accessed and gotten.
            state.Parameters.GetBoolean(new Parameter(P_PRINTUSEDPARAMETERS), null, false);
            state.Parameters.GetBoolean(new Parameter(P_PRINTACCESSEDPARAMETERS), null, false);
            state.Parameters.GetBoolean(new Parameter(P_PRINTUNUSEDPARAMETERS), null, false);
            state.Parameters.GetBoolean(new Parameter(P_PRINTUNACCESSEDPARAMETERS), null, false);
            state.Parameters.GetBoolean(new Parameter(P_PRINTALLPARAMETERS), null, false);

            //...okay, here we go...

            if (state.Parameters.GetBoolean(new Parameter(P_PRINTUSEDPARAMETERS), null, false))
            {
                pw.WriteLine("\n\nUsed Parameters\n===============\n");
                state.Parameters.ListGotten(pw);
            }

            if (state.Parameters.GetBoolean(new Parameter(P_PRINTACCESSEDPARAMETERS), null, false))
            {
                pw.WriteLine("\n\nAccessed Parameters\n===================\n");
                state.Parameters.ListAccessed(pw);
            }

            if (state.Parameters.GetBoolean(new Parameter(P_PRINTUNUSEDPARAMETERS), null, false))
            {
                pw.WriteLine("\n\nUnused Parameters\n" + "================= (Ignore parent.x references) \n");
                state.Parameters.ListNotGotten(pw);
            }

            if (state.Parameters.GetBoolean(new Parameter(P_PRINTUNACCESSEDPARAMETERS), null, false))
            {
                pw.WriteLine("\n\nUnaccessed Parameters\n" + "===================== (Ignore parent.x references) \n");
                state.Parameters.ListNotAccessed(pw);
            }

            if (state.Parameters.GetBoolean(new Parameter(P_PRINTALLPARAMETERS), null, false))
            {
                pw.WriteLine("\n\nAll Parameters\n==============\n");
                // list only the parameters visible.  Shadowed parameters not shown
                state.Parameters.List(pw, false);
            }

            pw.Flush();
            Console.Error.Flush();
            Console.Out.Flush();

            // finish by closing down Output.  This is because gzipped and other buffered
            // streams just don't shut write themselves out, and finalize isn't called
            // on them because Java's being obnoxious.  Pretty stupid.
            state.Output.Close();
        }
    }
}