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
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Vector
{
    /// <summary> 
    /// BitVectorIndividual is a VectorIndividual whose genome is an array of booleans.
    /// The default mutation method simply flips bits with <tt>mutationProbability</tt>.
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
    /// <li/><b>PrintIndividual(...,StreamWriter) / ReadIndividual(...,StreamReader)</b>&nbsp;&nbsp;&nbsp;
    /// This approach transmits or receives an indivdual in text encoded such that the individual is largely readable
    /// by humans but can be read back in 100% by ECJ as well.  To do this, these methods will encode numbers
    /// using the <tt>ec.util.Code</tt> class.  These methods are mostly used to write out populations to
    /// files for inspection, slight modification, then reading back in later on.  <b>ReadIndividual</b> reads
    /// in the fitness and the evaluation flag, then calls <b>ParseGenotype</b> to read in the remaining individual.
    /// You are responsible for implementing ParseGenotype: the Code class is there to help you.
    /// <b>printIndividual</b> writes out the fitness and evaluation flag, then calls <b>GenotypeToString</b> 
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
    /// <p/><b>Default Base</b><br/>
    /// vector.bit-vect-ind
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.vector.BitVectorIndividual")]
    public class BitVectorIndividual : VectorIndividual
    {
        #region Constants

        public const string P_BITVECTORINDIVIDUAL = "bit-vect-ind";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase => VectorDefaults.ParamBase.Push(P_BITVECTORINDIVIDUAL);

        public override object Genome
        {
            get => genome;
            set => genome = (bool[])value; // Possible InvalidCastException
        }

        public bool[] genome { get; set; }

        public override int GenomeLength
        {
            get => genome.Length;
            set
            {
                var newGenome = new bool[value];
                Array.Copy(genome, 0, newGenome, 0, genome.Length < newGenome.Length ? genome.Length : newGenome.Length);
                genome = newGenome;
            }
        }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase); // actually unnecessary (Individual.Setup() is empty)

            var s = (BitVectorSpecies)Species; // where my default info is stored
            genome = new bool[s.GenomeSize];
        }

        #endregion // Setup
        #region Operations

        #region Genome

        /// <summary>
        /// Splits the genome into n pieces, according to points, which *must* be sorted. 
        /// pieces.Length must be 1 + points.Length 
        /// </summary>
        public override void Split(int[] points, object[] pieces)
        {
            var point0 = 0;
            var point1 = points[0];

            for (var x = 0; x < pieces.Length; x++)
            {
                pieces[x] = new bool[point1 - point0];
                Array.Copy(genome, point0, (Array)pieces[x], 0, point1 - point0);
                point0 = point1;
                if (x >= pieces.Length - 2)
                    point1 = genome.Length;
                else
                    point1 = points[x + 1];
            }
        }

        /// <summary>
        /// Joins the n pieces and sets the genome to their concatenation.
        /// </summary>
        public override void Join(object[] pieces)
        {
            var sum = 0;
            for (var x = 0; x < pieces.Length; x++)
                sum += ((bool[])pieces[x]).Length;

            var runningsum = 0;
            var newgenome = new bool[sum];
            for (var x = 0; x < pieces.Length; x++)
            {
                Array.Copy((Array)pieces[x], 0, newgenome, runningsum, ((bool[])pieces[x]).Length);
                runningsum += ((bool[])pieces[x]).Length;
            }
            // set genome
            genome = newgenome;
        }

        /// <summary>
        /// Initializes the individual by randomly flipping the bits. 
        /// </summary>
        public override void Reset(IEvolutionState state, int thread)
        {
            for (var x = 0; x < genome.Length; x++)
                genome[x] = state.Random[thread].NextBoolean();
        }

        #endregion // Genome
        #region Breeding

        public override void DefaultCrossover(IEvolutionState state, int thread, VectorIndividual ind)
        {
            var s = (BitVectorSpecies)Species; // where my default info is stored
            var i = (BitVectorIndividual)ind;
            bool tmp;
            int point;

            if (genome.Length != i.genome.Length)
                state.Output.Fatal("Genome lengths are not the same for fixed-length vector crossover");
            switch (s.CrossoverType)
            {

                case BitVectorSpecies.C_ONE_POINT:
                    point = state.Random[thread].NextInt((genome.Length / s.ChunkSize) + 1);
                    for (var x = 0; x < point * s.ChunkSize; x++)
                    {
                        tmp = i.genome[x];
                        i.genome[x] = genome[x];
                        genome[x] = tmp;
                    }
                    break;

                case BitVectorSpecies.C_TWO_POINT:
                    var point0 = state.Random[thread].NextInt((genome.Length / s.ChunkSize) + 1);
                    point = state.Random[thread].NextInt((genome.Length / s.ChunkSize) + 1);
                    if (point0 > point)
                    {
                        var p = point0; point0 = point; point = p;
                    }
                    for (var x = point0 * s.ChunkSize; x < point * s.ChunkSize; x++)
                    {
                        tmp = i.genome[x];
                        i.genome[x] = genome[x];
                        genome[x] = tmp;
                    }
                    break;

                case BitVectorSpecies.C_ANY_POINT:
                    for (var x = 0; x < genome.Length / s.ChunkSize; x++)
                        if (state.Random[thread].NextBoolean(s.CrossoverProbability))
                            for (var y = x * s.ChunkSize; y < (x + 1) * s.ChunkSize; y++)
                            {
                                tmp = i.genome[y];
                                i.genome[y] = genome[y];
                                genome[y] = tmp;
                            }
                    break;
            }
        }

        /// <summary>
        /// Destructively mutates the individual in some default manner.  
        /// The default form does a bit-flip with a probability depending on parameters. 
        /// </summary>
        public override void DefaultMutate(IEvolutionState state, int thread)
        {
            var s = (BitVectorSpecies)Species; // where my default info is stored
            for (int x = 0; x < genome.Length; x++)
            {
                if (state.Random[thread].NextBoolean(s.MutationProbability[x]))
                {
                    bool old = genome[x];
                    for (int retries = 0; retries < s.GetDuplicateRetries(x) + 1; retries++)
                    {
                        switch (s.MutationType(x))
                        {
                            case BitVectorSpecies.C_FLIP_MUTATION:
                                genome[x] = !genome[x];
                                break;
                            case BitVectorSpecies.C_RESET_MUTATION:
                                genome[x] = state.Random[thread].NextBoolean();
                                break;
                        }
                        if (genome[x] != old) break;
                        // else genome[x] = old;  // try again
                    }
                }
            }
        }

        #endregion // Breeding

        #endregion // Operations
        #region Comparison

        public override int GetHashCode()
        {
            // stolen from GPIndividual.  It's a decent algorithm.
            var hash = GetType().GetHashCode();

            hash = (hash << 1 | BitShifter.URShift(hash, 31)) ^ genome.GetHashCode();

            return hash;
        }

        public override bool Equals(object ind)
        {
            if (!(GetType().Equals(ind.GetType())))
                return false; // SimpleRuleIndividuals are special.
            var i = (BitVectorIndividual)ind;
            if (genome.Length != i.genome.Length)
                return false;
            for (var j = 0; j < genome.Length; j++)
                if (genome[j] != i.genome[j])
                    return false;
            return true;
        }

        /// <summary>
        /// Implements distance as hamming distance.
        /// </summary>
        public override double DistanceTo(Individual otherInd)
        {
            if (!(otherInd is BitVectorIndividual))
                return base.DistanceTo(otherInd);  // will return infinity!

            var other = (BitVectorIndividual)otherInd;
            var otherGenome = other.genome;
            var hammingDistance = 0.0;
            for (var i = 0; i < other.GenomeLength; i++)
            {
                if (genome[i] ^ otherGenome[i])  //^ is xor
                    hammingDistance++;
            }

            return hammingDistance;
        }

        #endregion // Comparison
        #region Cloning

        public override object Clone()
        {
            var myobj = (BitVectorIndividual)base.Clone();

            // must clone the genome
            myobj.Genome = genome.Clone();

            return myobj;
        }

        #endregion // Cloning
        #region ToString

        public override string GenotypeToStringForHumans()
        {
            StringBuilder s = new StringBuilder();
            foreach (bool t in genome)
            {
                s.Append(t ? "1" : "0");
            }
            return s.ToString();
        }

        public override string GenotypeToString()
        {
            var s = new StringBuilder();
            s.Append(Code.Encode(genome.Length));
            foreach (var t in genome)
                s.Append(Code.Encode(t));
            return s.ToString();
        }

        #endregion // ToString
        #region IO

        public override void ParseGenotype(IEvolutionState state, StreamReader reader)
        {
            // read in the next line.  The first item is the number of genes
            var s = reader.ReadLine();
            var d = new DecodeReturn(s);
            Code.Decode(d);

            if (d.Type != DecodeReturn.T_INTEGER)  // uh oh
                state.Output.Fatal("Individual with genome:\n" + s 
                    + "\n... does not have an integer at the beginning indicating the genome count.");

            var lll = (int)(d.L);

            genome = new bool[lll];

            // read in the genes
            for (var i = 0; i < genome.Length; i++)
            {
                Code.Decode(d);
                genome[i] = (d.L != 0);
            }
        }

        public override void WriteGenotype(IEvolutionState state, BinaryWriter writer)
        {
            writer.Write(genome.Length);
            foreach (var t in genome)
                writer.Write(t); // inefficient: booleans are written out as bytes
        }

        public override void ReadGenotype(IEvolutionState state, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            if (genome == null || genome.Length != len)
                genome = new bool[len];
            for (var x = 0; x < genome.Length; x++)
                genome[x] = reader.ReadBoolean();
        }

        #endregion // IO
    }
}