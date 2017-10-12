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
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC
{
    /// <summary> 
    /// A Population is the repository for all the Individuals being bred or
    /// evaluated in the evolutionary run at a given time.
    /// A Population is basically an array of Subpops, each of which
    /// are arrays of Individuals coupled with a single Species per Subpoulation.
    /// 
    /// <p/>The first Population is created using the initializePopulation method
    /// of the Initializer object, which typically calls the Population's
    /// populate() method in turn.  On generational systems, subsequent populations
    /// are created on a generation-by-generation basis by the Breeder object,
    /// replacing the previous Population.
    /// 
    /// <p/>In a multithreaded area of a run, Pops should be considered
    /// immutable.  That is, once they are created, they should not be modified,
    /// nor anything they contain.  This protocol helps ensure read-safety under
    /// multithreading race conditions.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>subpops</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(the number of subpops)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>subpop</tt><i>.n</i><br/>
    /// <font size="-1">classname, inherits or = ec.Subpopulation</font></td>
    /// <td valign="top">(the class for subpop #<i>n</i>)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>default-subpop</tt><br/>
    /// <font size="-1">int &gt;= 0</font></td>
    /// <td valign="top"/>(the default subpopulation index.  
    /// The parameter base of this subpopulation will be used as the default base 
    /// for all subpopulations which do not define one themselves.</tr>
    /// </table>
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>subpop</tt><i>.n</i></td>
    /// <td>Subpopulation #<i>n</i>.</td></tr>
    /// </table>
    /// </summary>   
    [Serializable]
    [ECConfiguration("ec.Population")]
    public class Population : IGroup
    {
        #region Constants

        public const string P_SIZE = "subpops";
        public const string P_SUBPOP = "subpop";
        public const string P_DEFAULT_SUBPOP = "default-subpop";
        public const string P_FILE = "file";
        public const string NUM_SUBPOPS_PREAMBLE = "Number of Subpops: ";
        public const string SUBPOP_INDEX_PREAMBLE = "Subpop Number: ";

        #endregion // Constants
        #region Properties

        public Subpopulation[] Subpops { get; set; }

        /* A new population should be loaded from this resource name if it is non-null;
           otherwise they should be created at random.  */
        public bool LoadInds { get; set; }
        public IParameter FileParam { get; set; }

        #endregion // Properties
        #region Setup

        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            // how big should subpops be?  Don't have a default base

            // do we load from a file?
            FileParam = paramBase.Push(P_FILE);
            LoadInds = state.Parameters.ParameterExists(FileParam, null);

            // how many subpopulations do we have?

            var p = paramBase.Push(P_SIZE);
            var size = state.Parameters.GetInt(p, null, 1);
            if (size == 0)
                // uh oh
                state.Output.Fatal("Population size must be >0.\n", paramBase.Push(P_SIZE));
            Subpops = new Subpopulation[size];

            // Load the subpops
            for (var x = 0; x < size; x++)
            {
                p = paramBase.Push(P_SUBPOP).Push("" + x);
                if (!state.Parameters.ParameterExists(p, null))
                {
                    p = paramBase.Push(P_DEFAULT_SUBPOP);
                    var defaultSubpop = state.Parameters.GetInt(p, null, 0);
                    if (defaultSubpop >= 0)
                    {
                        state.Output.Warning("Using subpopulation " + defaultSubpop + " as the default for subpopulation " + x);
                        p = paramBase.Push(P_SUBPOP).Push("" + defaultSubpop);
                    }                    // else an error will occur on the next line anyway.
                }
                Subpops[x] = (Subpopulation)(state.Parameters.GetInstanceForParameterEq(p, null, typeof(Subpopulation))); // Subpopulation.class is fine
                Subpops[x].Setup(state, p);
            }
        }

        /* Sets all Individuals in the Population to null, preparing it to be reused. */
        public void Clear()
        {
            for (var i = 0; i < Subpops.Length; i++)
                Subpops[i].Clear();
        }

        #endregion // Setup
        #region Composition

        /// <summary>
        /// Populates the population with new random individuals. 
        /// </summary>
        public virtual void Populate(IEvolutionState state, int thread)
        {
            // should we load individuals from a file? -- duplicates are permitted
            if (LoadInds)
            {
                var stream = state.Parameters.GetResource(FileParam, null);
                if (stream == null)
                    state.Output.Fatal("Could not load population from file", FileParam);

                try { ReadPopulation(state, new StreamReader(stream, Encoding.Default)); }
                catch (IOException e)
                {
                    state.Output.Fatal("An IOException occurred when trying to read from the file " + state.Parameters.GetString(FileParam, null) + ".  The IOException was: \n" + e,
                        FileParam, null);
                }
            }
            else
            {
                // let's populate!
                for (var x = 0; x < Subpops.Length; x++)
                    Subpops[x].Populate(state, thread);
            }
        }

        #endregion // Composition
        #region Cloning

        /// <summary>
        /// Returns an instance of Population just like it had been before it was
        /// populated with individuals. You may need to override this if you override
        /// Population. <b>IMPORTANT NOTE</b>: if the size of the array in
        /// Population has been changed, then the clone will take on the new array
        /// size.  This helps some evolution strategies.
        /// </summary>
        /// <seealso cref="IGroup.EmptyClone()" />
        public virtual IGroup EmptyClone()
        {
            try
            {
                var p = (Population)Clone();
                p.Subpops = new Subpopulation[Subpops.Length];
                for (var x = 0; x < Subpops.Length; x++)
                    p.Subpops[x] = (Subpopulation)(Subpops[x].EmptyClone());
                return p;
            }
            catch (Exception)
            {
                throw new ApplicationException();
            } // never happens
        }

        virtual public object Clone()
        {
            // BRS : TODO : Figure out what this should do! Return MemberwiseClone()?
            return MemberwiseClone();
        }

        #endregion // Cloning
        #region IO

        /// <summary>
        /// Prints an entire population in a form readable by humans.
        /// </summary>
        public virtual void PrintPopulationForHumans(IEvolutionState state, int log)
        {
            state.Output.PrintLn(NUM_SUBPOPS_PREAMBLE + Subpops.Length, log);
            for (var i = 0; i < Subpops.Length; i++)
            {
                state.Output.PrintLn(SUBPOP_INDEX_PREAMBLE + i, log);
                Subpops[i].PrintSubpopulationForHumans(state, log);
            }
        }

        /// <summary>
        /// Prints an entire population in a form readable by humans but also parseable 
        /// by the computer using readPopulation(EvolutionState, LineNumberReader).
        /// </summary>
        public virtual void PrintPopulation(IEvolutionState state, int log)
        {
            state.Output.PrintLn(NUM_SUBPOPS_PREAMBLE + Code.Encode(Subpops.Length), log);
            for (var i = 0; i < Subpops.Length; i++)
            {
                state.Output.PrintLn(SUBPOP_INDEX_PREAMBLE + Code.Encode(i), log);
                Subpops[i].PrintSubpopulation(state, log);
            }
        }

        /// <summary>
        /// Prints an entire population in a form readable by humans but also parseable 
        /// by the computer using readPopulation(EvolutionState, LineNumberReader). 
        /// </summary>
        public virtual void PrintPopulation(IEvolutionState state, StreamWriter writer)
        {
            writer.WriteLine(NUM_SUBPOPS_PREAMBLE + Code.Encode(Subpops.Length));
            for (var i = 0; i < Subpops.Length; i++)
            {
                writer.WriteLine(SUBPOP_INDEX_PREAMBLE + Code.Encode(i));
                Subpops[i].PrintSubpopulation(state, writer);
            }
        }

        /// <summary>
        /// Reads a population from the format generated by printPopulation(....).  
        /// The number of subpops and the species information must be identical. 
        /// </summary>
        public virtual void ReadPopulation(IEvolutionState state, StreamReader reader)
        {
            // read the number of subpops and check to see if this appears to be a valid individual
            var numSubpops = Code.ReadIntWithPreamble(NUM_SUBPOPS_PREAMBLE, state, reader);

            // read in subpops
            if (numSubpops != Subpops.Length)
                // definitely wrong
                state.Output.Fatal("On reading population from text stream, the number of subpops was wrong.");

            for (var i = 0; i < Subpops.Length; i++)
            {
                var j = Code.ReadIntWithPreamble(SUBPOP_INDEX_PREAMBLE, state, reader);
                // sanity check
                if (j != i)
                    state.Output.WarnOnce("On reading population from text stream, some subpop indexes in the population did not match.");
                Subpops[i].ReadSubpopulation(state, reader);
            }
        }

        /// <summary>
        /// Writes a population in binary form, in a format readable by readPopulation(EvolutionState, DataInput). 
        /// </summary>
        public virtual void WritePopulation(IEvolutionState state, BinaryWriter dataOutput)
        {
            dataOutput.Write(Subpops.Length);
            foreach (var t in Subpops)
                t.WriteSubpopulation(state, dataOutput);
        }

        /// <summary>
        /// Reads a population in binary form, from the format generated by writePopulation(...). 
        /// The number of subpops and the species information must be identical. 
        /// </summary>
        public virtual void ReadPopulation(IEvolutionState state, BinaryReader dataInput)
        {
            var numSubpops = dataInput.ReadInt32();
            if (numSubpops != Subpops.Length)
                state.Output.Fatal("On reading subpop from binary stream, the number of subpops was wrong.");

            foreach (var t in Subpops)
                t.ReadSubpopulation(state, dataInput);
        }

        #endregion // IO
    }
}