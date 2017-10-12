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

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Vector
{
    /// <summary> 
    /// VectorGene is an abstract superclass of objects which may be used in the genome array of GeneVectorIndividuals.
    /// 
    /// <p/>In addition to serialization for checkpointing, VectorGenes may read and write themselves to streams in three ways.
    /// 
    /// <ul>
    /// <li/><b>WriteGene(...,BinaryWriter) / ReadGene(...,BinaryReader)</b>&nbsp;&nbsp;&nbsp;
    /// This method transmits or receives a VectorGene in binary.  It is the most efficient approach to sending
    /// VectorGenes over networks, etc.  The default versions of WriteGene/ReadGene throw errors.
    /// You don't need to implement them if you don't plan on using Read/WriteGene.
    /// 
    /// <li/><b>PrintGene(...,StreamWriter) / ReadGene(...,StreamReader)</b>&nbsp;&nbsp;&nbsp;
    /// This approach transmits or receives a VectorGene in text encoded such that the VectorGene is largely readable
    /// by humans but can be read back in 100% by ECJ as well.  To do this, these methods will typically encode numbers
    /// using the <tt>ec.util.Code</tt> class.  These methods are mostly used to write out populations to
    /// files for inspection, slight modification, then reading back in later on.  <b>ReadGene</b>
    /// reads in a line, then calls <b>ReadGeneFromString</b> on that line.
    /// You are responsible for implementing ReadGeneFromString: the Code class is there to help you.
    /// The default version throws an error if called.
    /// <b>PrintGene</b> calls <b>PrintGeneToString</b>
    /// and PrintLns the resultant string. You are responsible for implementing the PrintGeneToString method in such
    /// a way that ReadGeneFromString can read back in the VectorGene PrintLn'd with PrintGeneToString.  The default form
    /// of PrintGeneToString() simply calls <b>ToString()</b> by default.  
    /// You might override <b>PrintGeneToString()</b> to provide better information.   
    /// You are not required to implement these methods, but without them you will not be able 
    /// to write VectorGenes to files in a simultaneously computer- and human-readable fashion.
    /// 
    /// <li/><b>PrintGeneForHumans(...,StreamWriter)</b>&nbsp;&nbsp;&nbsp;
    /// This approach prints a VectorGene in a fashion intended for human consumption only.
    /// <b>PrintGeneForHumans</b> calls <b>PrintGeneToStringForHumans()</b> 
    /// and PrintLns the resultant string.  The default form of this method just returns the value of
    /// <b>ToString()</b>. You may wish to override this to provide more information instead. 
    /// You should handle one of these methods properly to ensure VectorGenes can be printed by ECJ.
    /// </ul>
    /// <p/><b>Default Base</b><br/>
    /// vector.vect-gene
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.vector.VectorGene")]
    public abstract class VectorGene : IPrototype
    {
        #region Constants

        public const string P_VECTORGENE = "vect-gene";

        #endregion // Constants
        #region Properties

        public virtual IParameter DefaultBase
        {
            get { return VectorDefaults.ParamBase.Push(P_VECTORGENE); }
        }

        #endregion // Properties
        #region Setup

        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            // nothing by default
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// The reset method randomly reinitializes the gene.
        /// </summary>
        public abstract void Reset(IEvolutionState state, int thread);

        /// <summary>
        /// Mutate the gene.  The default form just resets the gene.
        /// </summary>
        public virtual void Mutate(IEvolutionState state, int thread)
        {
            Reset(state, thread);
        }

        #endregion // Operations
        #region Comparison

        /// <summary>
        /// Generates a hash code for this gene -- the rule for this is that the hash code
        /// must be the same for two genes that are equal to each other genetically. 
        /// </summary>
        abstract public override int GetHashCode();

        /// <summary>
        /// Unlike the standard form for Java, this function should return true if this
        /// gene is "genetically identical" to the other gene. 
        /// </summary>
        abstract public override bool Equals(object other);

        #endregion // Comparison
        #region Cloning

        public virtual object Clone()
        {
            try
            {
                return MemberwiseClone();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Cloning Error!", ex);
            } // never happens
        }

        #endregion // Cloning
        #region ToString

        /// <summary>
        /// Prints the gene to a string in a human-readable fashion.  The default simply calls ToString(). 
        /// </summary>
        public virtual string PrintGeneToStringForHumans()
        {
            return ToString();
        }

        /// <summary>
        /// Prints the gene to a string in a fashion readable by ReadGeneFromString 
        /// and parseable by ReadGene(state, reader).
        /// Override this.  The default form returns ToString(). 
        /// </summary>
        public virtual string PrintGeneToString()
        {
            return ToString();
        }

        #endregion // ToString
        #region IO

        /// <summary>
        /// Nice printing.  The default form simply calls PrintGeneToStringForHumans 
        /// and prints the result, but you might want to override this.
        /// </summary>
        public virtual void PrintGeneForHumans(IEvolutionState state, int log)
        {
            state.Output.PrintLn(PrintGeneToStringForHumans(), log);
        }

        /// <summary>
        /// Reads a gene from a string, which may contain a final '\n'.
        /// Override this method.  The default form generates an error.
        /// </summary>
        public virtual void ReadGeneFromString(string geneText, IEvolutionState state)
        {
            state.Output.Error("readGeneFromString(string,state) unimplemented in " + GetType());
        }

        /// <summary>
        /// Prints the gene in a way that can be read by ReadGene().  The default form simply
        /// calls PrintGeneToString(state).   Override this gene to do custom writing to the log,
        /// or just override PrintGeneToString(...), which is probably easier to do.
        /// </summary>
        public virtual void PrintGene(IEvolutionState state, int log)
        {
            state.Output.PrintLn(PrintGeneToString(), log);
        }

        /// <summary>
        /// Reads a gene printed by PrintGene(...).  The default form simply reads a line into
        /// a string, and then calls ReadGeneFromString() on that line.  Override this gene to do
        /// custom reading, or just override ReadGeneFromString(...), which is probably easier to do.
        /// </summary>
        public virtual void ReadGene(IEvolutionState state, StreamReader reader)
        {
            ReadGeneFromString(reader.ReadLine(), state);
        }

        /// <summary>
        /// Override this if you need to write rules out to a binary stream. 
        /// </summary>
        public virtual void WriteGene(IEvolutionState state, BinaryWriter writer)
        {
            state.Output.Fatal("writeGene(EvolutionState, DataOutput) not implemented in " + GetType());
        }

        /// <summary>
        /// Override this if you need to read rules in from a binary stream. 
        /// </summary>
        public virtual void ReadGene(IEvolutionState state, BinaryReader reader)
        {
            state.Output.Fatal("readGene(EvolutionState, DataInput) not implemented in " + GetType());
        }

        #endregion // IO
    }
}