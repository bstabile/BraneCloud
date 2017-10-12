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
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Util
{	
    /// <summary> 
    /// Checkpoints ec.EvolutionState objects out to checkpoint files, or 
    /// restores the same from checkpoint files.  Checkpoint take the following
    /// form:
    /// 
    /// <p/><i>CheckpointPrefix</i><tt>.</tt><i>generation</i><tt>.gz</tt>
    /// 
    /// <p/>...where <i>CheckpointPrefix</i> is the checkpoing prefix given
    /// in ec.EvolutionState, and <i>generation</i> is the current generation number
    /// also given in ec.EvolutionState.
    /// The ".gz" is added because the file is GZIPped to save space.
    ///
    /// <p/>When writing a checkpoint file, if you have specified a checkpoint directory
    /// in ec.EvolutionState.checkpointDirectory, then this directory will be used to
    /// write the checkpoint files.  Otherwise they will be written in your working
    /// directory (where you ran the Java process).
    ///    
    /// </summary>
    [ECConfiguration("ec.util.Checkpoint")]
    public class Checkpoint
    {
        #region Operations

        /// <summary>
        /// Writes the evolution state out to a file. 
        /// </summary>		
        public static void SetCheckpoint(IEvolutionState state)
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
                
        /// <summary>
        /// Returns an EvolutionState object read from a checkpoint file
        /// whose filename is <i>checkpoint</i>. 
        /// </summary>
        /// <exception>
        /// Thrown when the checkpoint file contains a class reference which doesn't exist in your class hierarchy.
        /// </exception>
        public static IEvolutionState RestoreFromCheckpoint(string fileName)
        {
            IEvolutionState state;
            // load from the file
            using (var r = new BinaryReader(new GZipStream(new FileStream(fileName, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)))
            {
                var bf = new BinaryFormatter();
                state = (IEvolutionState)bf.Deserialize(r.BaseStream);
                // restart from the checkpoint
            }
            if (state != null)
                state.ResetFromCheckpoint();
            return state;
        }

        #endregion // Operations
    }
}