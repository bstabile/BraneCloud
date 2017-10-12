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
using System.Linq;
using System.Text;

using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Vector
{
    /// <summary> 
    /// ByteVectorIndividual is a VectorIndividual whose genome is an array of bytes.
    /// Gene values may range from species.MinGene(x) to species.MaxGene(x), inclusive.
    /// The default mutation method randomizes genes to new values in this range,
    /// with <tt>species.MutationProbability</tt>.
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
    /// <p/><b>Default Base</b><br/>
    /// vector.byte-vect-ind
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.vector.ByteVectorIndividual")]
    public class ByteVectorIndividual : VectorIndividual
    {
        #region Constants

        public const string P_BYTEVECTORINDIVIDUAL = "byte-vect-ind";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return VectorDefaults.ParamBase.Push(P_BYTEVECTORINDIVIDUAL); }
        }

        public override object Genome
        {
            get
            {
                return genome;
            }

            set
            {
                genome = (sbyte[])value;
            }

        }
        public sbyte[] genome { get; set; }
        public override int GenomeLength
        {
            get { return genome.Length; }
            set
            {
                var newGenome = new sbyte[value];
                Array.Copy(genome, 0, newGenome, 0, genome.Length < newGenome.Length ? genome.Length : newGenome.Length);
                genome = newGenome;
            }
        }
        public override long Length
        {
            get { return genome.Length; }
        }

        /// <summary>
        /// Returns true if each gene value is within is specified [min,max] range.
        /// </summary>
        public virtual bool IsInRange
        {
            get
            {
                var species = (IntegerVectorSpecies)Species;
                for (var i = 0; i < GenomeLength; i++)
                    if (genome[i] < species.MinGene(i) ||
                        genome[i] > species.MaxGene(i)) return false;
                return true;
            }
        }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase); // actually unnecessary (Individual.Setup() is empty)

            // since IntegerVectorSpecies set its constraint values BEFORE it called
            // super.Setup(...) [which in turn called our Setup(...)], we know that
            // stuff like GenomeSize has already been set...

            var def = DefaultBase;

            if (!(Species is IntegerVectorSpecies))
                state.Output.Fatal("ByteVectorIndividual requires an IntegerVectorSpecies", paramBase, def);
            var s = (IntegerVectorSpecies)Species;

            genome = new sbyte[s.GenomeSize];
        }

        #endregion // Setup
        #region Operations

        #region Genome

        public void SetGenomeLength(int len)
        {
            var newGenome = new sbyte[len];
            Array.Copy(genome, 0, newGenome, 0, genome.Length < newGenome.Length ? genome.Length : newGenome.Length);
            genome = newGenome;
        }

        /// <summary>
        /// Splits the genome into n pieces, according to points, which *must* be sorted. 
        /// pieces.length must be 1 + points.length 
        /// </summary>
        public override void Split(int[] points, object[] pieces)
        {
            var point0 = 0;
            var point1 = points[0];
            for (var x = 0; x < pieces.Length; x++)
            {
                pieces[x] = new sbyte[point1 - point0];
                Array.Copy(genome, point0, (Array)pieces[x], 0, point1 - point0);
                point0 = point1;
                point1 = x >= pieces.Length - 2 ? genome.Length : points[x + 1];
            }
        }

        /// <summary>
        /// Joins the n pieces and sets the genome to their concatenation.
        /// </summary>
        public override void Join(object[] pieces)
        {
            var sum = pieces.Sum(t => ((sbyte[])t).Length);

            var runningsum = 0;
            var newgenome = new sbyte[sum];
            foreach (var t in pieces)
            {
                Array.Copy((Array)t, 0, newgenome, runningsum, ((sbyte[])t).Length);
                runningsum += ((sbyte[])t).Length;
            }
            // set genome
            genome = newgenome;
        }

        /// <summary>
        /// Initializes the individual by randomly choosing Bytes uniformly from MinGene to MaxGene. 
        /// </summary>
        public override void Reset(IEvolutionState state, int thread)
        {
            var s = (IntegerVectorSpecies)Species;
            for (var x = 0; x < genome.Length; x++)
                genome[x] = (sbyte)((int)s.MinGene(x) + state.Random[thread].NextInt((int)s.MaxGene(x) - (int)s.MinGene(x) + 1));
        }

        /// <summary>
        /// Clips each gene value to be within its specified [min,max] range. 
        /// </summary>
        public virtual void Clamp()
        {
            var species = (IntegerVectorSpecies)Species;
            for (var i = 0; i < Length; i++)
            {
                var minGene = (sbyte)species.MinGene(i);
                if (genome[i] < minGene)
                    genome[i] = minGene;
                else
                {
                    var maxGene = (sbyte)species.MaxGene(i);
                    if (genome[i] > maxGene)
                        genome[i] = maxGene;
                }
            }
        }

        #endregion // Genome
        #region Breeding

        public override void DefaultCrossover(IEvolutionState state, int thread, VectorIndividual ind)
        {
            var s = (IntegerVectorSpecies)Species;
            var i = (ByteVectorIndividual)ind;
            sbyte tmp;
            int point;

            if (genome.Length != i.genome.Length)
                state.Output.Fatal("Genome lengths are not the same for fixed-length vector crossover");
            switch (s.CrossoverType)
            {

                case VectorSpecies.C_ONE_POINT:
                    point = state.Random[thread].NextInt((genome.Length / s.ChunkSize) + 1);
                    for (var x = 0; x < point * s.ChunkSize; x++)
                    {
                        tmp = i.genome[x];
                        i.genome[x] = genome[x];
                        genome[x] = tmp;
                    }
                    break;

                case VectorSpecies.C_TWO_POINT:
                    var point0 = state.Random[thread].NextInt((genome.Length / s.ChunkSize) + 1);
                    point = state.Random[thread].NextInt((genome.Length / s.ChunkSize) + 1);
                    if (point0 > point)
                    {
                        var p = point0;
                        point0 = point;
                        point = p;
                    }
                    for (var x = point0 * s.ChunkSize; x < point * s.ChunkSize; x++)
                    {
                        tmp = i.genome[x];
                        i.genome[x] = genome[x];
                        genome[x] = tmp;
                    }
                    break;

                case VectorSpecies.C_ANY_POINT:
                    for (var x = 0; x < genome.Length / s.ChunkSize; x++)
                        if (state.Random[thread].NextBoolean(s.CrossoverProbability))
                            for (var y = x * s.ChunkSize; y < (x + 1) * s.ChunkSize; y++)
                            {
                                tmp = i.genome[y];
                                i.genome[y] = genome[y];
                                genome[y] = tmp;
                            }
                    break;
                case VectorSpecies.C_LINE_RECOMB:
                    {
                        var alpha = state.Random[thread].NextDouble() * (1 + 2 * s.LineDistance) - s.LineDistance;
                        var beta = state.Random[thread].NextDouble() * (1 + 2 * s.LineDistance) - s.LineDistance;
                        for (var x = 0; x < genome.Length; x++)
                        {
                            var min = s.MinGene(x);
                            var max = s.MaxGene(x);
                            var t = (long)Math.Floor(alpha * genome[x] + (1 - alpha) * i.genome[x] + 0.5);
                            var u = (long)Math.Floor(beta * i.genome[x] + (1 - beta) * genome[x] + 0.5);
                            if (!(t < min || t > max || u < min || u > max))
                            {
                                genome[x] = (sbyte)t;
                                i.genome[x] = (sbyte)u;
                            }
                        }
                    }
                    break;
                case VectorSpecies.C_INTERMED_RECOMB:
                    {
                        for (var x = 0; x < genome.Length; x++)
                        {
                            long t;
                            long u;
                            long min;
                            long max;
                            do
                            {
                                var alpha = state.Random[thread].NextDouble() * (1 + 2 * s.LineDistance) - s.LineDistance;
                                var beta = state.Random[thread].NextDouble() * (1 + 2 * s.LineDistance) - s.LineDistance;
                                min = s.MinGene(x);
                                max = s.MaxGene(x);
                                t = (long)Math.Floor(alpha * genome[x] + (1 - alpha) * i.genome[x] + 0.5);
                                u = (long)Math.Floor(beta * i.genome[x] + (1 - beta) * genome[x] + 0.5);
                            } while (t < min || t > max || u < min || u > max);
                            genome[x] = (sbyte)t;
                            i.genome[x] = (sbyte)u;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Destructively mutates the individual in some default manner.  The default form
        /// simply randomizes genes to a uniform distribution from the min and max of the gene values. 
        /// </summary>
        public override void DefaultMutate(IEvolutionState state, int thread)
        {
            var s = (IntegerVectorSpecies)Species;
            if (s.MutationProbability > 0.0)
                for (var x = 0; x < genome.Length; x++)
                    if (state.Random[thread].NextBoolean(s.MutationProbability))
                        genome[x] = (sbyte)((int)s.MinGene(x) + state.Random[thread].NextInt((int)s.MaxGene(x) - (int)s.MinGene(x) + 1));
        }

        #endregion // Breeding

        #endregion // Operations
        #region Comparison

        public override int GetHashCode()
        {
            // stolen from GPIndividual.  It's a decent algorithm.
            var hash = GetType().GetHashCode();

            hash = (hash << 1 | BitShifter.URShift(hash, 31));

            return genome.Aggregate(hash, (current, t) => (current << 1 | BitShifter.URShift(current, 31)) ^ t);
        }

        public override bool Equals(object ind)
        {
            if (!(GetType().Equals(ind.GetType())))
                return false; // SimpleRuleIndividuals are special.
            var i = (ByteVectorIndividual)ind;
            if (genome.Length != i.genome.Length)
                return false;
            return !genome.Where((t, j) => t != i.genome[j]).Any();
        }

        public override double DistanceTo(Individual otherInd)
        {
            if (!(otherInd is ByteVectorIndividual))
                return base.DistanceTo(otherInd);  // will return infinity!

            var other = (ByteVectorIndividual)otherInd;
            var otherGenome = other.genome;
            var sumSquaredDistance = 0.0;
            for (var i = 0; i < other.GenomeLength; i++)
            {
                var dist = genome[i] - (long)otherGenome[i];
                sumSquaredDistance += dist * dist;
            }
            //return StrictMath.Sqrt(sumSquaredDistance); ????
            return Math.Sqrt(sumSquaredDistance);
        }

        #endregion // Comparison
        #region Cloning

        public override object Clone()
        {
            var myobj = (ByteVectorIndividual)base.Clone();

            // must clone the genome, property setter performs cast
            myobj.Genome = genome.Clone();

            return myobj;
        }

        #endregion // Cloning
        #region ToString

        public override string GenotypeToStringForHumans()
        {
            return genome.Aggregate("", (current, t) => current + " " + t);
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
            var lll = (int)(d.L);

            genome = new sbyte[lll];

            // read in the genes
            for (var i = 0; i < genome.Length; i++)
            {
                Code.Decode(d);
                genome[i] = (sbyte)(d.L);
            }
        }

        public override void WriteGenotype(IEvolutionState state, BinaryWriter writer)
        {
            writer.Write(genome.Length);
            foreach (var t in genome)
                writer.Write((byte)t);
        }

        public override void ReadGenotype(IEvolutionState state, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            if (genome == null || genome.Length != len)
                genome = new sbyte[len];
            for (var x = 0; x < genome.Length; x++)
                genome[x] = (sbyte)reader.ReadByte();
        }

        #endregion // IO
    }
}