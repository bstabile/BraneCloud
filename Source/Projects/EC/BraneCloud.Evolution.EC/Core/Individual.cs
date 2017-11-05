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
using System.Diagnostics;
using System.IO;
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC
{
    /// <summary> 
    /// An Individual is an item in the EC population stew which is evaluated 
    /// and assigned a fitness which determines its likelihood of selection.
    /// Individuals are created most commonly by the NewIndividual(...) method
    /// of the ec.Species class.
    /// <p />In general Individuals are immutable.  That is, once they are created
    /// their genetic material should not be modified.  This protocol helps insure that they are
    /// safe to read under multithreaded conditions.  You can violate this protocol,
    /// but try to do so when you know you have only have a single thread.
    /// <p />In addition to serialization for checkpointing, Individuals may read and write themselves to streams in three ways.
    /// <ul>
    /// 
    /// <li /><b>WriteIndividual(...,BinaryWriter) / ReadIndividual(...,BinaryReader)</b>&nbsp;&nbsp;&nbsp;
    /// This method transmits or receives an individual in binary.  It is the most efficient approach to sending
    /// individuals over networks, etc.  These methods write the evaluated flag and the fitness, then
    /// call <b>ReadGenotype/WriteGenotype</b>, which you must implement to write those parts of your 
    /// Individual special to your functions-- the default versions of ReadGenotype/WriteGenotype throw errors.
    /// You don't need to implement them if you don't plan on using Read/WriteIndividual.
    /// 
    /// <li /><b>PrintIndividual(...,StreamWriter)/ReadIndividual(...,StreamReader)</b>&nbsp;&nbsp;&nbsp;
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
    /// <li /><b>PrintIndividualForHumans(...,PrintWriter)</b>&#160;&#160;&#160; 
    /// This approach prints an individual in a fashion intended for human consumption only.
    /// <b>PrintIndividualForHumans</b> writes out the fitness and evaluation flag, then calls <b>GenotypeToStringForHumans</b> 
    /// and <i>PrintLn</i>s the resultant string. You are responsible for implementing the GenotypeToStringForHumans method.
    /// The default form of GenotypeToStringForHumans simply calls <b>ToString</b>, which you may override instead if you like
    /// (though note that <b>GenotypeToString</b>'s default also calls <b>ToString</b>).  You should handle one of these methods properly
    /// to ensure individuals can be printed by ECJ.
    /// </ul>
    /// 
    /// <p />Since individuals should be largely immutable, why is there a <b>ReadIndividual</b> method?
    /// after all this method doesn't create a <i>new</i> individual -- it just erases the existing one.  This is
    /// largely historical; but the method is used underneath by the various <b>NewIndividual</b> methods in Species,
    /// which <i>do</i> create new individuals read from files.  If you're trying to create a brand new individual
    /// read from a file, look in Species.
    /// </summary>    
    [Serializable]
    [ECConfiguration("ec.Individual")]
    public abstract class Individual : IPrototype, IComparable
    {
        #region Constants

        /// <summary>
        /// A reasonable parameter base element for individuals.
        /// </summary>
        public const string P_INDIVIDUAL = "individual";
        
        /// <summary>
        /// A string appropriate to put in front of whether or not the individual has been printed. 
        /// </summary>
        public const string EVALUATED_PREAMBLE = "Evaluated: ";

        #endregion // Constants
        #region Properties

        public abstract IParameter DefaultBase { get; }

        /// <summary>
        /// The fitness of the Individual. 
        /// </summary>
        public IFitness Fitness { get; set; }
        
        /// <summary>
        /// The species of the Individual.
        /// </summary>
        public Species Species { get; set; }

        /// <summary>
        /// Has the individual been evaluated and its fitness determined yet? 
        /// </summary>
        public bool Evaluated { get; set; }

        /// <summary>
        /// Returns the "size" of the individual.  This is used for things like
        /// parsimony pressure.  The default form of this method returns 0 --
        /// if you care about parsimony pressure, you'll need to override the
        /// default to provide a more descriptive measure of size. 
        /// </summary>
        public virtual long Size { get; protected set; }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// This should be used to set up only those things which you share in common
        /// with all other individuals in your species; individual-specific items
        /// which make you <i>you</i> should be filled in by Species.NewIndividual(...),
        /// and modified by breeders. 
        /// </summary>
        /// <seealso cref="IPrototype.Setup(IEvolutionState, IParameter)"> </seealso>
        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            // does nothing by default.
            // So where is the species set?  The Species does so after it
            // loads me but before it calls Setup on me.
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Replaces myself with the other Individual, while merging our evaluation results together.  May destroy
        /// the other Individual in the process.  By default this procedure calls Fitness.Merge(...) to merge the old
        /// fitness (backwards) into the new fitness, then entirely overwrites myself with the other Individual
        /// (including the merged fitness).
        ///         
        /// <p/>What is the purpose of this method?   When coevolution is done in combination with distributed evaluation,
        /// an Individual may be sent to multiple remote sites to be tested in different trials prior to having a completed
        /// fitness assessed.  As those trials complete, we need a way to merge them together.  By default this method
        /// simply merges the trial arrays (using fitness.Merge(...)), and determines the "best" context,
        /// then copies the other Individual to me.  But if you
        /// store additional trial results outside fitness---for example, if you keep around the best collaborators from
        /// coevolution, say---you may need a way to guarantee that this Individual reflects the most up to date information
        /// about recent trials arriving via the other Individual.  In this case, override the method and perform merging 
        /// by hand.
        /// </summary>
        public void Merge(IEvolutionState state, Individual other)
        {
            // merge the fitnesses backwards:  merge the fitness INTO the other fitness
            other.Fitness.Merge(state, Fitness);

            // now push the Individual back to us, including the merged fitness
            try
            {
                using (var ms = new MemoryStream())
                {
                    other.WriteIndividual(state, new BinaryWriter(ms));
                    ms.Position = 0;
                    ReadIndividual(state, new BinaryReader(ms));
                }
            }
            catch (IOException e)
            {
                Trace.WriteLine(e.StackTrace);
                state.Output.Fatal("Caught impossible IOException in Individual.Merge(...).");
                throw; // Don't let this get away if Output is changed to no longer kill the process!
            }
        }

        #endregion // Operations
        #region Comparison

        /// <summary>
        /// Returns a hashcode for the individual, such that individuals which
        /// are Equals(...) each other always return the same hash code. 
        /// </summary>
        public abstract override int GetHashCode();

        /// <summary>
        /// Returns true if I am genetically "equal" to ind.  This should
        /// mostly be interpreted as saying that we are of the same class
        /// and that we hold the same data. It should NOT be a pointer comparison. 
        /// </summary>
        public abstract override bool Equals(object other);

        /// <summary>
        /// Returns the metric distance to another individual, if such a thing can be measured.
        /// Subclassess of Individual should implement this if it exists for their representation.
        /// The default implementation here, which isn't very helpful, returns 0 if the individuals are equal
        /// and infinity if they are not.
        /// </summary>
        public virtual double DistanceTo(Individual otherInd)
        {
            return (Equals(otherInd) ? 0 : Double.PositiveInfinity);
        }

        /// <summary>
        /// Returns -1 if I am BETTER in some way than the other Individual, 1 if the other Individual is BETTER than me,
        /// and 0 if we are equivalent.  The default implementation assumes BETTER means FITTER, by simply calling
        /// CompareTo on the fitnesses themselves
        /// </summary>
        public int CompareTo(object o)
        {
            var other = (Individual)o;
            return Fitness.CompareTo(other.Fitness);
        }

        #endregion // Comparison
        #region Cloning

        /// <inheritdoc />
        /// <summary>
        /// This is a memberwise clone implementation.
        /// </summary>
        public virtual object Clone()
        {
            try
            {
                var myobj = (Individual) MemberwiseClone();
                if (myobj.Fitness != null)
                    myobj.Fitness = (IFitness) Fitness.Clone();
                return myobj;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Clone not supported", ex);
            } // never happens
        }

        #endregion // Cloning
        #region ToString

        /// <summary>
        /// Overridden here because GetHashCode() is not expected to return the pointer
        /// to the object.  ToString() normally uses GetHashCode() to print a unique identifier,
        /// and that's no longer the case.   You're welcome to override this anyway you 
        /// like to make the individual print out in a more lucid fashion. 
        /// </summary>
        public override string ToString()
        {
            return "" + GetType().FullName + "@" + GetHashCode() + "{" + GetHashCode() + "}";
        }
        
        /// <summary>
        /// Print to a string the genotype of the Individual in a fashion readable by humans, and not intended
        /// to be parsed in again.  The fitness and evaluated flag should not be included.  The default form
        /// simply calls ToString(), but you'll probably want to override this to something else. 
        /// </summary>
        public virtual string GenotypeToStringForHumans()
        {
            return ToString();
        }
        
        /// <summary>
        /// Print to a string the genotype of the Individual in a fashion intended
        /// to be parsed in again via ParseGenotype(...).
        /// The fitness and evaluated flag should not be included.  
        /// The default form simply calls ToString(), which is almost certainly wrong, 
        /// and you'll probably want to override this to something else. 
        /// </summary>
        public virtual string GenotypeToString()
        {
            return ToString();
        }

        #endregion // ToString
        #region IO

        /// <summary>
        /// Should print the individual out in a pleasing way for humans.
        /// </summary>
        public virtual void PrintIndividualForHumans(IEvolutionState state, int log)
        {
            state.Output.PrintLn(EVALUATED_PREAMBLE + Code.Encode(Evaluated), log);
            Fitness.PrintFitnessForHumans(state, log);
            state.Output.PrintLn(GenotypeToStringForHumans(), log);
        }

        /// <summary>
        /// Should print the individual in a way that can be read by computer.
        /// </summary>
        public virtual void PrintIndividual(IEvolutionState state, int log)
        {
            state.Output.PrintLn(EVALUATED_PREAMBLE + Code.Encode(Evaluated), log);
            Fitness.PrintFitness(state, log);
            state.Output.PrintLn(GenotypeToString(), log);
        }

        /// <summary>
        /// Should print the individual in a way that can be read by computer,
        /// including its fitness.  You can get fitness to print itself at the
        /// appropriate time by calling fitness.PrintFitness(state,log,writer); 
        /// Usually you should try to use PrintIndividual(state,log)
        /// instead -- use this method only if you can't print through the 
        /// Output facility for some reason.
        /// <p/>The default form of this method simply prints out whether or not the
        /// individual has been evaluated, its fitness, and then calls Individual.GenotypeToString().
        /// Feel free to override this to produce more sophisticated behavior, 
        /// though it is rare to need to -- instead you could just override GenotypeToString().
        /// </summary>        
        public virtual void PrintIndividual(IEvolutionState state, StreamWriter writer)
        {
            writer.WriteLine(EVALUATED_PREAMBLE + Code.Encode(Evaluated));
            Fitness.PrintFitness(state, writer);
            writer.WriteLine(GenotypeToString());
        }
        
        /// <summary>
        /// Reads in the individual from a form printed by PrintIndividual(), erasing the previous
        /// information stored in this Individual.  If you are trying to <i>create</i> an Individual
        /// from information read in from a stream or BinaryReader,
        /// see the various NewIndividual() methods in Species. The default form of this method
        /// simply reads in evaluation information, then fitness information, and then 
        /// calls ParseGenotype() (which you should implement).  The Species is not changed or
        /// attached, so you may need to do that elsewhere.  Feel free to override 
        /// this method to produce more sophisticated behavior, 
        /// though it is rare to need to -- instead you could just override ParseGenotype(). 
        /// </summary>
        public virtual void ReadIndividual(IEvolutionState state, StreamReader reader)
        {
            Evaluated = Code.ReadBooleanWithPreamble(EVALUATED_PREAMBLE, state, reader);
            
            // Next, what's my fitness?
            Fitness.ReadFitness(state, reader);
            
            // next, read me in
            ParseGenotype(state, reader);
        }

        /// <summary>
        /// This method is used only by the default version of ReadIndividual(state,reader),
        /// and it is intended to be overridden to parse in that part of the individual that
        /// was outputted in the GenotypeToString() method.  The default version of this method
        /// exits the program with an "unimplemented" error.  You'll want to override this method,
        /// or to override ReadIndividual(...) to not use this method. 
        /// </summary>
        public virtual void ParseGenotype(IEvolutionState state, StreamReader reader)
        {
            state.Output.Fatal("ParseGenotype(EvolutionState, LineNumberReader) not implemented in " + this.GetType());
        }

        /// <summary>
        /// Writes the binary form of an individual out to a BinaryWriter.  This is not for serialization:
        /// the object should only write out the data relevant to the object sufficient to rebuild it from a BinaryReader.
        /// The Species will be reattached later, and you should not write it.   The default version of this
        /// method writes the evaluated and fitness information, then calls WriteGenotype() to write the genotype
        /// information.  Feel free to override this method to produce more sophisticated behavior, 
        /// though it is rare to need to -- instead you could just override WriteGenotype(). 
        /// </summary>
        public virtual void WriteIndividual(IEvolutionState state, BinaryWriter writer)
        {
            writer.Write(Evaluated);
            Fitness.WriteFitness(state, writer);
            WriteGenotype(state, writer);
        }

        /// <summary>
        /// Reads the binary form of an individual from a BinaryReader, erasing the previous
        /// information stored in this Individual.  This is not for serialization:
        /// the object should only read in the data written out via PrintIndividual(state,writer).  
        /// If you are trying to <i>create</i> an Individual
        /// from information read in from a stream or BinaryReader,
        /// see the various NewIndividual() methods in Species. The default form of this method
        /// simply reads in evaluation information, then fitness information, and then 
        /// calls ReadGenotype() (which you will need to override -- its default form simply throws an error).
        /// The Species is not changed or attached, so you may need to do that elsewhere.  Feel free to override 
        /// this method to produce more sophisticated behavior, though it is rare to need to -- instead you could
        /// just override ReadGenotype().
        /// </summary>
        public virtual void ReadIndividual(IEvolutionState state, BinaryReader reader)
        {
            Evaluated = reader.ReadBoolean();
            Fitness.ReadFitness(state, reader);
            ReadGenotype(state, reader);
        }

        /// <summary>
        /// Writes the genotypic information to a BinaryWriter.  Largely called by WriteIndividual(), and
        /// nothing else.  The default simply throws an error.  Various subclasses of Individual override this as
        /// appropriate. For example, if your custom individual's genotype consists of an array of 
        /// integers, you might do this:
        /// <code>
        ///     writer.WriteInt(integers.length);
        ///     for(var x = 0; x &lt; integers.length; x++)
        ///     writer.writeInt(integers[x]);
        /// </code>
        /// </summary>
        public virtual void WriteGenotype(IEvolutionState state, BinaryWriter writer)
        {
            state.Output.Fatal("writeGenotype(EvolutionState, DataOutput) not implemented in " + this.GetType());
        }

        /// <summary>
        /// Reads in the genotypic information from a BinaryReader, erasing the previous genotype
        /// of this Individual.  Largely called by ReadIndividual(), and nothing else.  
        /// If you are trying to <i>create</i> an Individual
        /// from information read in from a stream or BinaryReader,
        /// see the various NewIndividual() methods in Species.
        /// The default simply throws an error.  Various subclasses of Individual override this as
        /// appropriate.  For example, if your custom individual's genotype consists of an array of 
        /// integers, you might do this:
        /// 
        /// <code>
        ///     integers = new int[reader.ReadInt()];
        ///     for(int x = 0;x &lt; integers.length; x++)
        ///     integers[x] = reader.ReadInt();
        /// </code>
        /// </summary>
        public virtual void ReadGenotype(IEvolutionState state, BinaryReader dataInput)
        {
            state.Output.Fatal("ReadGenotype(EvolutionState, BinaryReader) not implemented in " + GetType());
        }

        #endregion // Print, Read, Parse Textual
    }
}