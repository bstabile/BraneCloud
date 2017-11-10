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
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Vector
{
    /// <summary> 
    /// VectorIndividual is the abstract class of simple individual representations
    /// which consist of vectors of values (booleans, integers, floating-point, etc.)
    /// 
    /// <p/>This class contains two methods, DefaultCrossover and DefaultMutate, which can
    /// be overridden if all you need is a simple crossover and a simple mutate mechanism.
    /// the VectorCrossoverPipeline and VectorMutationPipeline classes use these methods to do their
    /// handiwork.  For more sophisticated crossover and mutation, you'll need to write
    /// a custom breeding pipeline.
    /// 
    /// <p/>The <i>kind</i> of default crossover and mutation, and associated information,
    /// is stored in the VectorIndividual's VectorSpecies object, which is obtained through
    /// the <tt>Species</tt> variable.  For example, 
    /// VectorIndividual assumes three common types of crossover as defined in VectorSpecies
    /// which you should implement in your DefaultCrossover method: one-point, 
    /// two-point, and any-point (otherwise known as "uniform") crossover.
    /// 
    /// <p/>VectorIndividual is typically used for fixed-length vector representations;
    /// however, it can also be used with variable-length representations.  Two methods have
    /// been provided in all subclasses of VectorIndividual to help you there: split and
    /// join, which you can use to break up and reconnect VectorIndividuals in a variety
    /// of ways.  Note that you may want to override the Reset() method to create individuals
    /// with different initial lengths.
    /// 
    /// <p/>VectorIndividuals must belong to the species VectorSpecies (or some subclass of it).
    /// 
    /// <p/><b>From ec.Individual:</b>
    /// 
    /// <p/>In addition to serialization for checkpointing, Individuals may read and write themselves to streams in three ways.
    /// 
    /// <ul>
    /// <li/><b>WriteIndividual(...,BinaryWriter) / ReadIndividual(...,BinaryReader)</b>&nbsp;&nbsp;&nbsp;
    /// This method transmits or receives an individual in binary.  It is the most efficient approach to sending
    /// individuals over networks, etc.  These methods write the evaluated flag and the fitness, then
    /// call <b>ReadGenotype/WriteGenotype</b>, which you must implement to write those parts of your 
    /// Individual special to your functions-- the default versions of ReadGenotype/WriteGenotype throw errors.
    /// You don't need to implement them if you don't plan on using Read/WriteIndividual.
    /// 
    /// <li/><b>PrintIndividual(...,StreamWriter) / ReadIndividual(...,StreamWriter)</b>&nbsp;&nbsp;&nbsp;
    /// This approach transmits or receives an indivdual in text encoded such that the individual is largely readable
    /// by humans but can be read back in 100% by ECJ as well.  To do this, these methods will encode numbers
    /// using the <tt>ec.util.Code</tt> class.  These methods are mostly used to write out populations to
    /// files for inspection, slight modification, then reading back in later on.  <b>ReadIndividual</b> reads
    /// in the fitness and the evaluation flag, then calls <b>ParseGenotype</b> to read in the remaining individual.
    /// You are responsible for implementing ParseGenotype: the Code class is there to help you.
    /// <b>PrintIndividual</b> writes out the fitness and evaluation flag, then calls <b>GenotypeToString</b> 
    /// and PrintLns the resultant string. You are responsible for implementing the GenotypeToString method in such
    /// a way that ParseGenotype can read back in the individual PrintLn'd with GenotypeToString.  The default form
    /// of GenotypeToString simply calls <b>ToString</b>, which you may override instead if you like.  The default
    /// form of <b>ParseGenotype</b> throws an error.  You are not required to implement these methods, but without
    /// them you will not be able to write individuals to files in a simultaneously computer- and human-readable fashion.
    /// 
    /// <li/><b>PrintIndividualForHumans(...,StreamWriter)</b>&nbsp;&nbsp;&nbsp;
    /// This approach prints an individual in a fashion intended for human consumption only.
    /// <b>PrintIndividualForHumans</b> writes out the fitness and evaluation flag, then calls <b>GenotypeToStringForHumans</b> 
    /// and PrintLns the resultant string. You are responsible for implementing the GenotypeToStringForHumans method.
    /// The default form of GenotypeToStringForHumans simply calls <b>ToString</b>, which you may override instead if you like
    /// (though note that GenotypeToString's default also calls ToString).  You should handle one of these methods properly
    /// to ensure individuals can be printed by ECJ.
    /// </ul>
    /// <p/>In general, the various readers and writers do three things: they tell the Fitness to read/write itself,
    /// they read/write the evaluated flag, and they read/write the gene array.  If you add instance variables to
    /// a VectorIndividual or subclass, you'll need to read/write those variables as well.
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.vector.VectorIndividual")]
    public abstract class VectorIndividual : Individual
    {
        #region Properties

        /// <summary>
        /// The gene array.  If you know the type of the array, you can cast it and work on
        /// it directly.  Otherwise, you can still manipulate it in general, because arrays (like
        /// all objects) respond to Clone() and can be manipulated with <b>Array.Copy(...)</b> without bothering
        /// with their type.  This might be useful in creating special generalized crossover operators
        /// -- we apologize in advance for the fact that Java doesn't have a template system.  :-( 
        /// The default version returns null. 
        /// The default setter does nothing.
        /// </summary>
        public virtual object Genome
        {
            get { return null; }
            set { }
        }

        /// <summary>
        /// Sets the genome length.  If the length is longer, then it is filled with a default value (likely 0 or false).
        /// This may or may not be a valid value -- you will need to set appropriate values here. 
        /// The default implementation does nothing; but all subclasses in ECJ implement a subset of this. 
        /// </summary>
        public virtual int GenomeLength { get; set; }

        /// <summary>
        /// Returns the length of the gene array.  By default, this method returns 0. 
        /// </summary>
        public virtual long Length => 0;
        

        public override long Size => Length;

        #endregion // Properties
        #region Operations

        #region Genome

        /// <summary>
        /// Splits the genome into n pieces, according to points, which <i>must</i> be sorted. 
        /// pieces.Length must be 1 + points.Length.  The default form does nothing -- be careful
        /// not to use this method if it's not implemented!  It should be trivial to implement it
        /// for your genome -- just like at the other implementations.  
        /// </summary>
        public virtual void Split(int[] points, object[] pieces)
        {
        }

        /// <summary>
        /// Joins the n pieces and sets the genome to their concatenation.  The default form does nothing. 
        /// It should be trivial to implement it
        /// for your genome -- just like at the other implementations.  
        /// </summary>
        public virtual void Join(object[] pieces)
        {
        }

        /// <summary>
        /// Initializes the individual. 
        /// </summary>
        public abstract void Reset(IEvolutionState state, int thread);

        /// <summary>
        /// Initializes the individual to a new size.  Only use this if you need to initialize variable-length individuals. 
        /// </summary>
        public virtual void Reset(IEvolutionState state, int thread, int newSize)
        {
            GenomeLength = newSize;
            Reset(state, thread);
        }

        #endregion // Genome
        #region Breeding

        /// <summary>
        /// Destructively crosses over the individual with another in some default manner.  In most
        /// implementations provided in ECJ, one-, two-, and any-point crossover is done with a 
        /// for loop, rather than a possibly more efficient approach like arrayCopy().  The disadvantage
        /// is that Array.Copy(...) takes advantage of a CPU's bulk copying.  The advantage is that Array.Copy(...)
        /// would require a scratch array, so you'd be allocing and GCing an array for every crossover.
        /// Dunno which is more efficient.  
        /// </summary>
        public virtual void DefaultCrossover(IEvolutionState state, int thread, VectorIndividual ind)
        {
        }
        
        /// <summary>
        /// Destructively mutates the individual in some default manner.  The default version calls Reset().
        /// </summary>
        public virtual void DefaultMutate(IEvolutionState state, int thread)
        {
            Reset(state, thread);
        }

        #endregion // Breeding

        #endregion // Operations
        #region Cloning

        /// <summary>
        /// Clones the genes in pieces, and replaces the genes with their copies.  
        /// Does NOT copy the array, but modifies it in place.
        /// If the VectorIndividual holds numbers or booleans etc. 
        /// instead of genes, nothing is cloned (why bother?).
        /// </summary>
        /// <remarks>The default here does nothing.</remarks>
        public virtual void CloneGenes(Object piece) { }

        #endregion // Cloning
    }
}