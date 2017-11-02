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
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC
{
    /// <summary> 
    /// An EvolutionState object is a singleton object which holds the entire
    /// state of an evolutionary run.  By serializing EvolutionState, the entire
    /// run can be Checkpointed out to a file.
    /// 
    /// <p/>The EvolutionState instance is passed around in a <i>lot</i> of methods,
    /// so objects can read from the parameter database, pick random numbers,
    /// and write to the output facility.
    /// 
    /// <p/>EvolutionState is a unique object in that it calls its own Setup(...)
    /// method, from run(...).
    /// 
    /// <p/>An EvolutionState object contains quite a few objects, including:
    /// <ul>
    /// <li/><i>Objects you may safely manipulate during the multithreaded sections of a run:</i>
    /// <ul>
    /// <li/> MersenneTwisterFast random number generators (one for each evaluation or breeding thread -- use the thread number you were provided to determine which random number generator to use)
    /// <li/> The ParameterDatabase
    /// <li/> The Output facility for writing messages and logging
    /// </ul>
    /// <li/><i>ISingleton objects:</i>
    /// <ul>
    /// <li/> The Initializer.
    /// <li/> The Finisher.
    /// <li/> The Breeder.
    /// <li/> The Evaluator.
    /// <li/> The Statistics facility.
    /// <li/> The Exchanger.
    /// </ul>
    /// <li/><i>The current evolution state:</i>
    /// <ul>
    /// <li/> The generation number.
    /// <li/> The population.
    /// <li/> The maximal number of generations.
    /// </ul>
    /// <li/><i>Auxillary read-only information:</i>
    /// <ul>
    /// <li/> The prefix to begin Checkpoint file names with.
    /// <li/> Whether to quit upon finding a perfect individual.
    /// <li/> The number of breeding threads to spawn.
    /// <li/> The number of evaluation threads to spawn.
    /// </ul>
    /// 
    /// <li/><i>A place to stash pointers to static variables so they'll get serialized: </i>
    /// <ul>
    /// <li/> Statics
    /// </ul>
    /// </ul>
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><tt>generations</tt><br/>
    /// <font size="-1">int &gt;= 1</font> or undefined</td>
    /// <td valign="top">(maximal number of generations to run. Either this or evaluations must be set, but not both.)</td></tr>
    ///
    /// <tr><td valign="top"><tt> evaluations </tt ><br/>
    /// < font size="-1">int &gt;= 1</font> or undefined</td>
    /// <td valign="top">(maximal number of evaluations to run (in subpopulation 0).    Either this or generations must be set, but not both.)</td></tr>

    /// <tr><td valign="top"><tt>Checkpoint-modulo</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(how many generations should pass before we do a Checkpoint?  
    /// The definition of "generations" depends on the particular EvolutionState 
    /// implementation you're using)</td></tr>
    /// 
    /// <tr><td valign="top"><tt>Checkpoint</tt><br/>
    /// <font size="-1"/>bool = <tt>true</tt> or <tt>false</tt> (default)</td>
    /// <td valign="top">(should we Checkpoint?)</td></tr>
    /// 
    /// <tr><td valign="top"><tt>prefix</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(the prefix to prepend to Checkpoint files -- see ec.util.Checkpoint)</td></tr>
    ///
    /// <tr><td valign="top"><tt>checkpoint-directory</tt><br/>
    /// <font size="-1"/>File (default is empty)</td>
    /// <td valign="top">(directory where the checkpoint files should be located)</td></tr> 
    /// 
    /// <tr><td valign="top"><tt>quit-on-run-complete</tt><br/>
    /// <font size="-1"/>bool = <tt>true</tt> or <tt>false</tt> (default)</td>
    /// <td valign="top">(do we prematurely quit the run when we find a perfect individual?)</td></tr>
    /// <tr><td valign="top"><tt>init</tt><br/>
    /// <font size="-1">classname, inherits and != ec.Initializer</font></td>
    /// <td valign="top">(the class for initializer)</td></tr>
    /// <tr><td valign="top"><tt>finish</tt><br/>
    /// <font size="-1">classname, inherits and != ec.Finisher</font></td>
    /// <td valign="top">(the class for finisher)</td></tr>
    /// <tr><td valign="top"><tt>breed</tt><br/>
    /// <font size="-1">classname, inherits and != ec.Breeder</font></td>
    /// <td valign="top">(the class for breeder)</td></tr>
    /// <tr><td valign="top"><tt>eval</tt><br/>
    /// <font size="-1">classname, inherits and != ec.Evaluator</font></td>
    /// <td valign="top">(the class for evaluator)</td></tr>
    /// <tr><td valign="top"><tt>stat</tt><br/>
    /// <font size="-1">classname, inherits or = ec.Statistics</font></td>
    /// <td valign="top">(the class for statistics)</td></tr>
    /// <tr><td valign="top"><tt>exch</tt><br/>
    /// <font size="-1">classname, inherits and != ec.Exchanger</font></td>
    /// <td valign="top">(the class for exchanger)</td></tr>
    /// </table>
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><tt>init</tt></td>
    /// <td>initializer</td></tr>
    /// <tr><td valign="top"><tt>finish</tt></td>
    /// <td>finisher</td></tr>
    /// <tr><td valign="top"><tt>breed</tt></td>
    /// <td>breeder</td></tr>
    /// <tr><td valign="top"><tt>eval</tt></td>
    /// <td>evaluator</td></tr>
    /// <tr><td valign="top"><tt>stat</tt></td>
    /// <td>statistics</td></tr>
    /// <tr><td valign="top"><tt>exch</tt></td>
    /// <td>exchanger</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.EvolutionState")]
    public class EvolutionState : IEvolutionState
    {
        #region Constants

        private const long SerialVersionUID = 1;

        public const int UNDEFINED = 0;

        /// <summary>
        /// The population has started fresh (not from a Checkpoint). 
        /// </summary>
        public const int C_STARTED_FRESH = 0;

        /// <summary>
        /// The population started from a Checkpoint. 
        /// </summary>
        public const int C_STARTED_FROM_CHECKPOINT = 1;

        /// <summary>
        /// The evolution run has quit, finding a perfect individual. 
        /// </summary>
        public const int R_SUCCESS = 0;

        /// <summary>
        /// The evolution run has quit, failing to find a perfect individual. 
        /// </summary>
        public const int R_FAILURE = 1;

        /// <summary>
        /// The evolution run has not quit. 
        /// </summary>
        public const int R_NOTDONE = 2;

        public const string P_INITIALIZER = "init";
        public const string P_FINISHER = "finish";
        public const string P_BREEDER = "breed";
        public const string P_EVALUATOR = "eval";
        public const string P_STATISTICS = "stat";
        public const string P_EXCHANGER = "exch";
        public const string P_GENERATIONS = "generations";
        public const string P_EVALUATIONS = "evaluations";
        public const string P_QUITONRUNCOMPLETE = "quit-on-run-complete";
        public const string P_CHECKPOINTDIRECTORY = "checkpoint-directory";
        public const string P_CHECKPOINTPREFIX = "checkpoint-prefix";
        public const string P_CHECKPOINTPREFIX_OLD = "prefix";
        public const string P_CHECKPOINTMODULO = "checkpoint-modulo";
        public const string P_CHECKPOINT = "checkpoint";

        #endregion // Constants
        #region Properties

        /// <summary>
        /// The parameter database (threadsafe).  Parameter objects are also threadsafe.
        /// Nonetheless, you should generally try to treat this database as read-only. 
        /// </summary>
        public IParameterDatabase Parameters { get; set; }

        /// <summary>
        /// The original runtime arguments passed to the Java process. 
        /// You probably should not modify this inside an evolutionary run.  
        /// </summary>
        public string[] RuntimeArguments { get; set; }

        /// <summary>
        /// The output and logging facility (threadsafe).  Keep in mind that output is expensive. 
        /// </summary>
        public Output Output { get; set; }

        /// <summary>
        /// Current job iteration variables, set by Evolve.  The default version simply sets this to a single Object[1] containing
        /// the current job iteration number as an Integer (for a single job, it's 0).  You probably should not modify this inside
        /// an evolutionary run.  
        /// </summary>
        public object[] Job { get; set; }

        /// <summary>
        /// Whether or not the system should prematurely quit when Evaluator returns true for RunComplete(...) 
        /// (that is, when the system found an ideal individual. 
        /// </summary>
        public bool QuitOnRunComplete { get; set; }

        #region Threads

        /// <summary>
        /// The requested number of threads to be used in breeding, excepting perhaps a "parent" thread 
        /// which gathers the other threads.  If breedthreads = 1, then the system should not be multithreaded 
        /// during breeding.  Don't modify this during a run. 
        /// </summary>
        public int BreedThreads { get; set; }

        /// <summary>
        /// The requested number of threads to be used in evaluation, excepting perhaps a "parent" thread 
        /// which gathers the other threads.  If evalthreads = 1, then the system should not be multithreaded during 
        /// evaluation. Don't modify this during a run.
        /// </summary>
        public int EvalThreads { get; set; }

        #endregion // Threads
        #region Randomization

        /// <summary>
        /// An amount to add to each random number generator seed to "offset" it -- often this is simply the job number.  
        /// If you are using more random number generators
        /// internally than the ones initially created for you in the EvolutionState, you might want to create them with the seed
        /// value of <tt>seedParameter+randomSeedOffset</tt>.  At present the only such class creating additional generators
        /// is ec.eval.MasterProblem. 
        /// </summary>
        public int RandomSeedOffset { get; set; }

        /// <summary>
        /// An array of random number generators, indexed by the thread number you were given 
        /// (or, if you're not in a multithreaded area, use 0).  These generators are not threadsafe 
        /// in and of themselves, but if you only use the random number generator assigned to your thread, 
        /// as was intended, then you get random numbers in a threadsafe way.  These generators must each 
        /// have a different seed, of course.v</summary>
        public IMersenneTwister[] Random { get; set; }

        #endregion // Randomization
        #region Singletons

        /// <summary>
        /// The population initializer, a singleton object.  You should only access this in a read-only fashion. 
        /// </summary>
        public Initializer Initializer { get; set; }

        /// <summary>
        /// The population finisher, a singleton object.  You should only access this in a read-only fashion. 
        /// </summary>
        public IFinisher Finisher { get; set; }

        /// <summary>
        /// The population breeder, a singleton object.  You should only access this in a read-only fashion. 
        /// </summary>
        public Breeder Breeder { get; set; }

        /// <summary>
        /// The population evaluator, a singleton object.  You should only access this in a read-only fashion. 
        /// </summary>
        public IEvaluator Evaluator { get; set; }

        /// <summary>
        /// The population statistics, a singleton object.  You should generally only access this in a read-only fashion. 
        /// </summary>
        public Statistics Statistics { get; set; }

        /// <summary>
        /// The population exchanger, a singleton object.  You should only access this in a read-only fashion. 
        /// </summary>
        public Exchanger Exchanger { get; set; }

        #endregion // Singletons
        #region Population

        // set during running

        /// <summary>
        /// The current generation of the population in the run.  For non-generational approaches, 
        /// this probably should represent some kind of incrementing value, perhaps the number of individuals evaluated so far.  
        /// You probably shouldn't modify this. 
        /// </summary>
        public int Generation { get; set; }

        /// <summary>
        /// The number of generations the evolutionary computation system will run until it ends.  
        /// If the user has specified a desired number of evaluations instead of generations, then
        /// this value will not be valid until after the first generation has been created(but before
        /// it has bene evaluated).
        /// If after the population has been evaluated the Evaluator returns true for RunComplete(...), 
        /// and quitOnRunComplete is true, then the system will quit.  You probably shouldn't modify this.  
        /// </summary>
        public int NumGenerations { get; set; }

        /// How many evaluations should we run for?  If set to UNDEFINED (0), we run for the number of generations instead.
        public long NumEvaluations { get; set; } = UNDEFINED;

        /// <summary>
        /// The current population.  This is <i>not</i> a singleton object, and may be replaced after every 
        /// generation in a generational approach. You should only access this in a read-only fashion.  
        /// </summary>
        public Population Population { get; set; }

        #endregion // Population
        #region Checkpoint

        /// <summary>
        /// Should we Checkpoint at all? 
        /// </summary>
        public bool Checkpoint { get; set; }

        /// <summary>
        /// The requested directory where checkpoints should be located.  
        /// This must be a directory, not a file.  
        /// You probably shouldn't modify this during a run.
        /// </summary>
        public DirectoryInfo CheckpointDirectory { get; set; }

        /// <summary>
        /// The requested prefix start filenames, not including a following period.  
        /// You probably shouldn't modify this during a run.
        /// </summary>
        public string CheckpointPrefix { get; set; }

        /// <summary>
        /// The requested number of generations that should pass before we write out a Checkpoint file. 
        /// </summary>
        public int CheckpointModulo { get; set; }

        #endregion // Checkpoint

        #endregion // Properties
        #region Setup

        /// <summary>
        /// This will be called to create your evolution state; immediately
        /// after the constructor is called,
        /// the Parameters, random, and output fields will be set
        /// for you.  The constructor probably won't be called ever if
        /// restoring (deserializing) from a Checkpoint.
        /// </summary>
        public EvolutionState()
        {
        }

        /// <summary>
        /// Unlike for other Setup() methods, ignore the base; it will always be null. 
        /// </summary>
        /// <seealso cref="IPrototype.Setup(IEvolutionState,IParameter)"/>
        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            // we ignore the base, it's worthless anyway for EvolutionState

            IParameter p = new Parameter(P_CHECKPOINT);
            Checkpoint = Parameters.GetBoolean(p, null, false);

            p = new Parameter(P_CHECKPOINTPREFIX);
            CheckpointPrefix = Parameters.GetString(p, null);
            if (CheckpointPrefix == null)
            {
                // check for the old-style checkpoint prefix parameter
                var p2 = new Parameter(P_CHECKPOINTPREFIX_OLD);
                CheckpointPrefix = Parameters.GetString(p2, null);
                if (CheckpointPrefix == null)
                {
                    Output.Fatal("No checkpoint prefix specified.", p);  // indicate the new style, not old parameter
                }
                else
                {
                    Output.Warning("The parameter \"prefix\" is deprecated.  Please use \"checkpoint-prefix\".", p2);
                }
            }
            else
            {
                // check for the old-style checkpoint prefix parameter as an acciental duplicate
                var p2 = new Parameter(P_CHECKPOINTPREFIX_OLD);
                if (Parameters.GetString(p2, null) != null)
                {
                    Output.Warning("You have BOTH the deprecated parameter \"prefix\" and its replacement \"checkpoint-prefix\" defined.  The replacement will be used,  Please remove the \"prefix\" parameter.", p2);
                }

            }

            p = new Parameter(P_CHECKPOINTMODULO);
            CheckpointModulo = Parameters.GetInt(p, null, 1);
            if (CheckpointModulo == 0)
                Output.Fatal("The Checkpoint modulo must be an integer >0.", p);

            p = new Parameter(P_CHECKPOINTDIRECTORY);
            if (Parameters.ParameterExists(p, null))
            {
                CheckpointDirectory = Parameters.GetDirectory(p);
                if (CheckpointDirectory == null)
                    Output.Fatal("The checkpoint directory name is invalid: " + CheckpointDirectory, p);
                else if (!Path.IsPathRooted(CheckpointDirectory.FullName))
                    Output.Fatal("The checkpoint directory location is not a directory: " + CheckpointDirectory, p);
            }
            else CheckpointDirectory = null;

            // load evaluations, or generations, or both
            p = new Parameter(P_EVALUATIONS);
            if (Parameters.ParameterExists(p, null))
            {
                NumEvaluations = Parameters.GetInt(p, null, 1);  // 0 would be UNDEFINED
                if (NumEvaluations <= 0)
                    Output.Fatal("If defined, the number of evaluations must be an integer >= 1", p, null);
            }

            p = new Parameter(P_GENERATIONS);
            if (Parameters.ParameterExists(p, null))
            {
                NumGenerations = Parameters.GetInt(p, null, 1);  // 0 would be UDEFINED                 

                if (NumGenerations <= 0)
                    Output.Fatal("If defined, the number of generations must be an integer >= 1.", p, null);

                if (NumEvaluations != UNDEFINED)  // both defined
                {
                    state.Output.Warning("Both generations and evaluations defined: generations will be ignored and computed from the evaluations.");
                    NumGenerations = UNDEFINED;
                }
            }
            else if (NumEvaluations == UNDEFINED)  // uh oh, something must be defined
                Output.Fatal("Either evaluations or generations must be defined.", new Parameter(P_GENERATIONS), new Parameter(P_EVALUATIONS));

            p = new Parameter(P_QUITONRUNCOMPLETE);
            QuitOnRunComplete = Parameters.GetBoolean(p, null, false);


            /* Set up the singletons */
            p = new Parameter(P_INITIALIZER);
            Initializer = (Initializer)(Parameters.GetInstanceForParameter(p, null, typeof(Initializer)));
            Initializer.Setup(this, p);

            p = new Parameter(P_FINISHER);
            Finisher = (IFinisher)(Parameters.GetInstanceForParameter(p, null, typeof(IFinisher)));
            Finisher.Setup(this, p);

            p = new Parameter(P_BREEDER);
            Breeder = (Breeder)(Parameters.GetInstanceForParameter(p, null, typeof(Breeder)));
            Breeder.Setup(this, p);

            p = new Parameter(P_EVALUATOR);
            Evaluator = (IEvaluator)(Parameters.GetInstanceForParameter(p, null, typeof(IEvaluator)));
            Evaluator.Setup(this, p);

            p = new Parameter(P_STATISTICS);
            Statistics = (Statistics)(Parameters.GetInstanceForParameterEq(p, null, typeof(Statistics)));
            Statistics.Setup(this, p);

            p = new Parameter(P_EXCHANGER);
            Exchanger = (Exchanger)(Parameters.GetInstanceForParameter(p, null, typeof(Exchanger)));
            Exchanger.Setup(this, p);

            Generation = 0;
        }

        #endregion // Setup
        #region Operations

        public virtual void StartFresh()
        {
        }

        public virtual int Evolve()
        {
            return R_NOTDONE;
        }

        /// <summary>
        /// Starts the run. <i>condition</i> indicates whether or not the
        /// run was restarted from a Checkpoint (C_STARTED_FRESH vs
        /// C_STARTED_FROM_CHECKPOINT).  At the point that run(...) has been
        /// called, the parameter database has already been set up, as have
        /// the random number generators, the number of threads, and the
        /// Output facility.  This method should call this.Setup(...) to
        /// set up the EvolutionState object if condition equals C_STARTED_FRESH. 
        /// </summary>
        public virtual void Run(int condition)
        {
            if (condition == C_STARTED_FRESH)
            {
                StartFresh();
            }
            // condition == C_STARTED_FROM_CHECKPOINT
            else
            {
                StartFromCheckpoint();
            }

            /* the big loop */
            var result = R_NOTDONE;
            while (result == R_NOTDONE)
            {
                result = Evolve();
            }

            Finish(result);
        }

        public virtual void Finish(int result)
        {
        }

        /// <summary>
        /// This method is called after a Checkpoint
        /// is restored from but before the run starts up again.  
        /// You might use this to set up file pointers that were lost, etc. 
        /// </summary>        
        public virtual void ResetFromCheckpoint()
        {
            Output.Restart(); // may throw an exception if there's a bad file
            Exchanger.ReinitializeContacts(this);
            Evaluator.ReinitializeContacts(this);
        }

        public virtual void StartFromCheckpoint()
        {
        }

        #endregion // Operations
    }
}