using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Core.Interfaces;
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.Runtime
{
    public class ECStateManager : IStateManager
    {
        #region Constants

        /// <summary>
        /// The argument indicating that we're starting up from a checkpoint file. 
        /// </summary>
        public const string A_CHECKPOINT = "-checkpoint";
        public const string P_CHECKPOINTDIRECTORY = "checkpoint-directory";
        public const string P_CHECKPOINTPREFIX = "checkpoint-prefix";
        public const string P_CHECKPOINTPREFIX_OLD = "prefix";
        public const string P_CHECKPOINTMODULO = "checkpoint-modulo";
        public const string P_CHECKPOINT = "checkpoint";

        #endregion // Constants
        #region Privae Fields

        #endregion // Pivate Fields
        
        #region Public Properties

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
        /// Should we Checkpoint at all? 
        /// </summary>
        public bool UseCheckpointing { get; set; }

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

        #endregion // Public Properties
        #region IStateManager

        public void SetupCheckpointing()
        {
            if (Parameters == null)
                throw new InvalidOperationException("Parameters property is null.");

            // we ignore the base, it's worthless anyway for EvolutionState

            IParameter p = new Parameter(P_CHECKPOINT);
            UseCheckpointing = Parameters.GetBoolean(p, null, false);

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
                if (CheckpointDirectory == null || !CheckpointDirectory.Exists)
                    Output.Fatal("The checkpoint directory name is invalid: " + CheckpointDirectory, p);
                else if (!Path.IsPathRooted(CheckpointDirectory.FullName))
                    Output.Fatal("The checkpoint directory location is not a directory: " + CheckpointDirectory, p);
            }
            else CheckpointDirectory = null;            
        }

        public IEvolutionState GetCheckpoint(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists)
            {
                Trace.WriteLine(String.Format("Checkpoint file could not be found: {0}", fileName), "Error");
            }
            Trace.WriteLine("Restoring from Checkpoint File: {0}" + fileName);
            try
            {
                return Checkpoint.RestoreFromCheckpoint(fileName);
            }
            catch (Exception e)
            {
                Output.InitialError("An exception was generated upon starting up from a checkpoint.\nHere it is:\n" + e, false);
                Environment.Exit(1); // This was originally part of the InitialError call in ECJ. But we make Evolve responsible.
            }
            return null;
        }

        public void SetCheckpoint(IEvolutionState state)
        {
            if (state == null)
                throw new ArgumentNullException("state");
            var fileName = state.CheckpointPrefix + "." + state.Generation + ".gz";
            if (state.CheckpointDirectory != null)
            {
                fileName = Path.Combine(state.CheckpointDirectory.FullName,
                    "" + state.CheckpointPrefix + "." + state.Generation + ".gz");
            } try
            {
                using (var w = new BinaryWriter(new GZipStream(new FileStream(fileName, FileMode.Create), CompressionMode.Compress)))
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(w.BaseStream, state);
                }
                state.Output.Message("Wrote out checkpoint file " + fileName);
            }
            catch (IOException e)
            {
                state.Output.Warning("Unable to create the checkpoint file " + fileName
                    + "because of an IOException:\n--EXCEPTION--\n" + e + "\n--EXCEPTION-END--\n");
            }
        }

        #endregion // IStateManager
        #region Static Methods

        public static IParameterDatabase LoadParametersFromFile(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException(String.Format("The specified file could not be found: {0}", fileInfo.FullName));
            }
            var parameterDatabase = new ParameterDatabase(fileInfo);
            return parameterDatabase;
        }

        #endregion // Static Methods
    }
}
