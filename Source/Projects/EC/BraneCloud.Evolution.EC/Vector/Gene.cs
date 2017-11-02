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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Vector
{
    /**
     * Gene is an abstract superclass of objects which may be used in
     * the genome array of GeneVectorIndividuals.
     *

     * <p>In addition to serialization for checkpointing, Genes may read and write themselves to streams in three ways.
     *
     * <ul>
     * <li><b>writeGene(...,DataOutput)/readGene(...,DataInput)</b>&nbsp;&nbsp;&nbsp;This method
     * transmits or receives a Gene in binary.  It is the most efficient approach to sending
     * Genes over networks, etc.  The default versions of writeGene/readGene throw errors.
     * You don't need to implement them if you don't plan on using read/writeGene.
     *
     * <li><b>printGene(...,PrintWriter)/readGene(...,LineNumberReader)</b>&nbsp;&nbsp;&nbsp;This
     * approach transmits or receives a Gene in text encoded such that the Gene is largely readable
     * by humans but can be read back in 100% by ECJ as well.  To do this, these methods will typically encode numbers
     * using the <tt>ec.util.Code</tt> class.  These methods are mostly used to write out populations to
     * files for inspection, slight modification, then reading back in later on.  <b>readGene</b>
     * reads in a line, then calls <b>readGeneFromString</b> on that line.
     * You are responsible for implementing readGeneFromString: the Code class is there to help you.
     * The default version throws an error if called.
     * <b>printGene</b> calls <b>printGeneToString<b>
     * and printlns the resultant string. You are responsible for implementing the printGeneToString method in such
     * a way that readGeneFromString can read back in the Gene println'd with printGeneToString.  The default form
     * of printGeneToString() simply calls <b>toString()</b> 
     * by default.  You might override <b>printGeneToString()</b> to provide better information.   You are not required to implement these methods, but without
     * them you will not be able to write Genes to files in a simultaneously computer- and human-readable fashion.
     *
     * <li><b>printGeneForHumans(...,PrintWriter)</b>&nbsp;&nbsp;&nbsp;This
     * approach prints a Gene in a fashion intended for human consumption only.
     * <b>printGeneForHumans</b> calls <b>printGeneToStringForHumans()<b> 
     * and printlns the resultant string.  The default form of this method just returns the value of
     * <b>toString()</b>. You may wish to override this to provide more information instead. 
     * You should handle one of these methods properly
     * to ensure Genes can be printed by ECJ.
     * </ul>

     <p><b>Default Base</b><br>
     vector.gene

     * @author Sean Luke
     * @version 2.0
     */

    [ECConfiguration("ec.vector.Gene")]
    public abstract class Gene : IPrototype
    {
        public const string P_GENE = "gene";

        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            // nothing by default
        }

        public IParameter DefaultBase => VectorDefaults.ParamBase.Push(P_GENE);

        public virtual object Clone()
        {
            try { return base.MemberwiseClone(); }
            catch (CloneNotSupportedException e)
            { throw new InvalidOperationException(); } // never happens
        }


        /// <summary>
        /// Generates a hash code for this gene -- the rule for this is that the hash code
        /// must be the same for two genes that are equal to each other genetically.
        /// </summary>
         public abstract override int GetHashCode();

        /// <summary>
        /// Unlike the standard form for Java, this function should return true if this
        /// gene is "genetically identical" to the other gene.
        /// </summary>
        public abstract override bool Equals(Object other );

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

        /// <summary>
        /// Nice printing.  The default form simply calls printGeneToStringForHumans and prints the result, 
        /// but you might want to override this.
        /// </summary>
        public virtual void PrintGeneForHumans(IEvolutionState state, int verbosity, int log)
        {
            state.Output.PrintLn(PrintGeneToStringForHumans(), log);
        }

        /// <summary>
        /// Prints the gene to a string in a human-readable fashion.  The default simply calls toString().
        /// </summary>
        public virtual string PrintGeneToStringForHumans()
        {
            return ToString();
        }

        /// <summary>
        /// Prints the gene to a string in a fashion readable by readGeneFromString and parseable by readGene(state, reader).
        /// Override this.  The default form returns toString().
        /// </summary>
        public virtual string PrintGeneToString()
        {
            return ToString();
        }

        /// <summary>
        /// Reads a gene from a string, which may contain a '\n'.
        /// Override this method.The default form generates an error.
        /// </summary>
        public virtual void ReadGeneFromString(string s, IEvolutionState state)
        {
            state.Output.Error("readGeneFromString(string,state) unimplemented in " + GetType().Name);
        }

        /// <summary>
        /// Prints the gene in a way that can be read by readGene().  The default form simply
        /// calls printGeneToString().   Override this gene to do custom writing to the log,
        /// or just override printGeneToString(...), which is probably easier to do.
        /// </summary>
        public virtual void PrintGene(IEvolutionState state, int verbosity, int log)
        {
            state.Output.PrintLn(PrintGeneToString(), log);
        }

        /// <summary>
        /// Prints the gene in a way that can be read by readGene().  The default form simply
        /// calls printGeneToString(state).   Override this gene to do custom writing,
        /// or just override printGeneToString(...), which is probably easier to do.
        /// </summary>
        public virtual void PrintGene(IEvolutionState state, StreamWriter writer)
        {
            writer.Write(PrintGeneToString());
        }

        /// <summary>
        /// Reads a gene printed by printGene(...).  The default form simply reads a line into
        /// a string, and then calls readGeneFromString() on that line.Override this gene to do
        /// custom reading, or just override readGeneFromString(...), which is probably easier to do.
        /// </summary>
        public virtual void ReadGene(IEvolutionState state, StreamReader reader)
        {
            ReadGeneFromString(reader.ReadLine(),state);
        }

        /// <summary>
        /// Override this if you need to write rules out to a binary stream.
        /// </summary>
        public virtual void WriteGene(IEvolutionState state, BinaryWriter dataOutput)
        {
            state.Output.Fatal("WriteGene(EvolutionState, DataOutput) not implemented in " + GetType().Name);
        }

        /// <summary>
        /// Override this if you need to read rules in from a binary stream.
        /// </summary>
        public virtual void ReadGene(IEvolutionState state, BinaryReader dataInput)
    {
        state.Output.Fatal("ReadGene(EvolutionState, DataInput) not implemented in " + GetType().Name);
    }
}
}
