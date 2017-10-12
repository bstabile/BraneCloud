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
using System.Collections;

using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC
{
    /// <summary> 
    /// Subpopulation is a group which is basically an array of Individuals.
    /// There is always one or more Subpopulations in the Population.  Each
    /// Subpopulation has a Species, which governs the formation of the Individuals
    /// in that Subpopulation.  Subpopulations also contain a Fitness prototype
    /// which is cloned to form Fitness objects for individuals in the subpop.
    /// 
    /// <p/>An initial subpop is populated with new random individuals 
    /// using the populate(...) method.  This method typically populates
    /// by filling the array with individuals created using the Subpopulations' 
    /// species' emptyClone() method, though you might override this to create
    /// them with other means, by loading from text files for example.
    /// 
    /// <p/>In a multithreaded area of a run, Subpopulations should be considered
    /// immutable.  That is, once they are created, they should not be modified,
    /// nor anything they contain.  This protocol helps ensure read-safety under
    /// multithreading race conditions.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>size</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(total number of individuals in the subpop)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>species</tt><br/>
    /// <font size="-1">classname, inherits and != ec.Species</font></td>
    /// <td valign="top">(the class of the subpops' Species)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>fitness</tt><br/>
    /// <font size="-1">classname, inherits and != ec.Fitness</font></td>
    /// <td valign="top">(the class for the prototypical Fitness for individuals in this subpop)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>file</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(pathname of file from which the population is to be loaded.  If not defined, or empty, then the population will be initialized at random in the standard manner)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>duplicate-retries</tt><br/>
    /// <font size="-1">int &gt;= 0</font></td>
    /// <td valign="top">(during initialization, when we produce an individual which already exists in the subpop, the number of times we try to replace it with something unique.  Ignored if we're loading from a file.)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// ec.subpop
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>species</tt></td>
    /// <td>species (the subpops' species)</td></tr>
    /// </table>
    /// </summary>	    
    [Serializable]
    [ECConfiguration("ec.Subpopulation")]
    public class Subpopulation : IGroup
    {
        #region Constants

        public const string P_SUBPOPULATION = "subpop";
        public const string P_FILE = "file";
        public const string P_SUBPOPSIZE = "size"; // parameter for number of subpops or pops
        public const string P_SPECIES = "species";
        public const string P_RETRIES = "duplicate-retries";

        public const string NUM_INDIVIDUALS_PREAMBLE = "Number of Individuals: ";
        public const string INDIVIDUAL_INDEX_PREAMBLE = "Individual Number: ";

        #endregion // Constants
        #region Properties

        /// <summary>
        /// A new subpopulation should be loaded from this resource name if it is non-null;
        /// otherwise they should be created at random.
        /// </summary>
        public IParameter FileParam { get; set; }
        public bool LoadInds { get; set; }

        /// <summary>
        /// The species for individuals in this subpop. 
        /// </summary>
        public Species Species { get; set; }

        /// <summary>
        /// The subpop's individuals. 
        /// </summary>
        public Individual[] Individuals { get; set; }

        /// <summary>
        /// Do we allow duplicates? 
        /// </summary>
        public int NumDuplicateRetries { get; set; }

        #endregion // Properties
        #region Setup

        public virtual IParameter DefaultBase
        {
            get { return ECDefaults.ParamBase.Push(P_SUBPOPULATION); }
        }

        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            var def = DefaultBase;

            // do we load from a file?
            FileParam = paramBase.Push(P_FILE);
            LoadInds = state.Parameters.ParameterExists(FileParam, null);
            // what species do we use?

            Species = (Species)state.Parameters.GetInstanceForParameter(paramBase.Push(P_SPECIES), def.Push(P_SPECIES), typeof(Species));
            Species.Setup(state, paramBase.Push(P_SPECIES));

            // how big should our subpop be?

            var size = state.Parameters.GetInt(paramBase.Push(P_SUBPOPSIZE), def.Push(P_SUBPOPSIZE), 1);
            if (size <= 0)
                state.Output.Fatal("Subpopulation size must be an integer >= 1.\n", paramBase.Push(P_SUBPOPSIZE), def.Push(P_SUBPOPSIZE));

            // How often do we retry if we find a duplicate?
            NumDuplicateRetries = state.Parameters.GetInt(paramBase.Push(P_RETRIES), def.Push(P_RETRIES), 0);
            if (NumDuplicateRetries < 0)
                state.Output.Fatal("The number of retries for duplicates must be an integer >= 0.\n", paramBase.Push(P_RETRIES), def.Push(P_RETRIES));

            Individuals = new Individual[size];
        }

        /// <summary>
        /// Sets all Individuals in the Subpopulation to null, preparing it to be reused.
        /// </summary>
        public void Clear()
        {
            for (var i = 0; i < Individuals.Length; i++)
                Individuals[i] = null;
        }

        #endregion // Setup
        #region Operations

        public virtual void Populate(IEvolutionState state, int thread)
        {
            // should we load individuals from a file? -- duplicates are permitted
            if (LoadInds)
            {
                var stream = state.Parameters.GetResource(FileParam, null);
                if (stream == null)
                {
                    state.Output.Fatal("Could not load subpopulation from file", FileParam);
                }

                try
                {
                    ReadSubpopulation(state, new StreamReader(stream, Encoding.Default));
                }
                catch (IOException e)
                {
                    state.Output.Fatal(
                        "An IOException occurred when trying to read from the file " +
                        state.Parameters.GetString(FileParam, null) + ".  The IOException was: \n" + e,
                        FileParam, null);
                }
                //try { readSubpopulation(state, new LineNumberReader(new FileReader(loadInds))); }
                //catch (IOException e) { state.output.fatal("An IOException occurred when trying to read from the file " + loadInds + ".  The IOException was: \n" + e); }
            }
            else
            {
                Hashtable h = null;
                if (NumDuplicateRetries >= 1)
                    h = Hashtable.Synchronized(new Hashtable(Individuals.Length / 2)); // seems reasonable

                for (var x = 0; x < Individuals.Length; x++)
                {
                    for (var tries = 0; tries <= NumDuplicateRetries; tries++)
                    {
                        Individuals[x] = Species.NewIndividual(state, thread);

                        if (NumDuplicateRetries >= 1)
                        {
                            // check for duplicates
                            var o = h[Individuals[x]];
                            if (o == null)
                            // found nothing, we're safe
                            // hash it and go
                            {
                                h[Individuals[x]] = Individuals[x];
                                break;
                            }
                        }
                    } // oh well, we tried to cut down the duplicates
                }
            }
        }

        #endregion // Operations
        #region Cloning

        /// <summary>
        /// Returns an instance of Subpopulation just like it had been before it was
        /// populated with individuals. You may need to override this if you override
        /// Subpopulation.   <b>IMPORTANT NOTE</b>: if the size of the array in
        /// Subpopulation has been changed, then the clone will take on the new array
        /// size.  This helps some evolution strategies.
        /// </summary>
        /// <seealso cref="IGroup.EmptyClone()" />        
        public virtual IGroup EmptyClone()
        {
            try
            {
                var p = (Subpopulation) Clone();
                p.Species = Species; // don't throw it away...maybe this is a bad idea...
                p.Individuals = new Individual[Individuals.Length]; // empty
                return p;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Cloning Error!", ex);
            } // never happens
        }

        virtual public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion // Cloning
        #region IO

        /// <summary>
        /// Prints an entire subpop in a form readable by humans. 
        /// </summary>
        public virtual void PrintSubpopulationForHumans(IEvolutionState state, int log)
        {
            state.Output.PrintLn(NUM_INDIVIDUALS_PREAMBLE + Individuals.Length, log);
            for (var i = 0; i < Individuals.Length; i++)
            {
                state.Output.PrintLn(INDIVIDUAL_INDEX_PREAMBLE + Code.Encode(i), log);
                Individuals[i].PrintIndividualForHumans(state, log);
            }
        }

        /// <summary>
        /// Prints an entire subpopulation in a form readable by humans
        /// but also parseable by the computer using readSubpopulation(EvolutionState, LineNumberReader).
        /// </summary>
        public virtual void PrintSubpopulation(IEvolutionState state, int log)
        {
            state.Output.PrintLn(NUM_INDIVIDUALS_PREAMBLE + Code.Encode(Individuals.Length), log);
            for (var i = 0; i < Individuals.Length; i++)
            {
                state.Output.PrintLn(INDIVIDUAL_INDEX_PREAMBLE + Code.Encode(i), log);
                Individuals[i].PrintIndividual(state, log);
            }
        }

        /// <summary>
        /// Prints an entire subpop in a form readable by humans but also parseable 
        /// by the computer using readSubpopulation(EvolutionState, LineNumberReader). 
        /// </summary>
        public virtual void PrintSubpopulation(IEvolutionState state, StreamWriter writer)
        {
            writer.WriteLine(NUM_INDIVIDUALS_PREAMBLE + Code.Encode(Individuals.Length));
            for (var i = 0; i < Individuals.Length; i++)
            {
                writer.WriteLine(INDIVIDUAL_INDEX_PREAMBLE + Code.Encode(i));
                Individuals[i].PrintIndividual(state, writer);
            }
        }

        /// <summary>
        /// Reads a subpop from the format generated by printSubpopulation(....).  
        /// If the number of individuals is not identical, the individuals array will
        /// be deleted and replaced with a new array, and a warning will be generated 
        /// as individuals will have to be created using newIndividual(...) rather
        /// than ReadIndividual(...). 
        /// </summary>
        public virtual void ReadSubpopulation(IEvolutionState state, StreamReader reader)
        {
            // read in number of individuals and check to see if this appears to be a valid subpop
            var numInds = Code.ReadIntWithPreamble(NUM_INDIVIDUALS_PREAMBLE, state, reader);

            // read in individuals
            if (numInds != Individuals.Length)
            {
                state.Output.WarnOnce("On reading subpop from text stream, the subpop size didn't match.\n"
                    + "Had to resize and use newIndividual() instead of ReadIndividual().");

                Individuals = new Individual[numInds];
                for (var i = 0; i < Individuals.Length; i++)
                {
                    var j = Code.ReadIntWithPreamble(INDIVIDUAL_INDEX_PREAMBLE, state, reader);
                    // sanity check
                    if (j != i)
                        state.Output.WarnOnce("On reading subpop from text stream, some individual indexes in the subpop did not match.");
                    Individuals[i] = Species.NewIndividual(state, reader);
                }
            }
            else
                for (var i = 0; i < Individuals.Length; i++)
                {
                    var j = Code.ReadIntWithPreamble(INDIVIDUAL_INDEX_PREAMBLE, state, reader);
                    // sanity check
                    if (j != i)
                        state.Output.WarnOnce("On reading subpop from text stream, some individual indexes in the subpop did not match.");
                    if (Individuals[i] != null)
                        Individuals[i].ReadIndividual(state, reader);
                    else
                    {
                        state.Output.WarnOnce("On reading subpop from text stream, some of the preexisting subpop's slots were null.\n"
                            + "Had to use newIndividual() instead of ReadIndividual().");
                        Individuals[i] = Species.NewIndividual(state, reader);
                    }
                }
        }

        /// <summary>
        /// Writes a subpop in binary form, in a format readable by ReadSubpopulation(IEvolutionState, BinaryWriter). 
        /// </summary>
        public virtual void WriteSubpopulation(IEvolutionState state, BinaryWriter writer)
        {
            writer.Write(Individuals.Length);
            foreach (var t in Individuals)
                t.WriteIndividual(state, writer);
        }

        /// <summary>
        /// Reads a subpop in binary form, from the format generated by WriteSubpopulation(...).  
        /// If the number of individuals is not identical, the individuals array will
        /// be deleted and replaced with a new array, and a warning will be generated as individuals 
        /// will have to be created using NewIndividual(...) rather
        /// than ReadIndividual(...) 
        /// </summary>
        public virtual void ReadSubpopulation(IEvolutionState state, BinaryReader reader)
        {
            var numInds = reader.ReadInt32();
            if (numInds != Individuals.Length)
            {
                state.Output.WarnOnce("On reading subpop from binary stream, the subpop size was incorrect.\n"
                    + "Had to resize and use newIndividual() instead of ReadIndividual().");

                Individuals = new Individual[numInds];
                for (var i = 0; i < Individuals.Length; i++)
                    Individuals[i] = Species.NewIndividual(state, reader);
            }
            else
                foreach (var t in Individuals)
                    t.ReadIndividual(state, reader);
        }

        #endregion // IO
    }
}