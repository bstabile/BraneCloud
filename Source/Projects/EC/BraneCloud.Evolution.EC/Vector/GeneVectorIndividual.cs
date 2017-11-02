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
    /// GeneVectorIndividual is a VectorIndividual whose genome is an array of Genes.
    /// The default mutation method calls the mutate() method on each gene independently
    /// with <tt>species.MutationProbability</tt>.  Initialization calls Reset(), which
    /// should call Reset() on each gene.  Do not expect that the genes will actually
    /// exist during initialization -- see the default implementation of Reset() as an example
    /// for how to handle this.
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
    /// vector.gene-vect-ind
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.vector.GeneVectorIndividual")]
    public class GeneVectorIndividual : VectorIndividual
    {
        #region Constants

        public const string P_GENEVECTORINDIVIDUAL = "gene-vect-ind";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase => VectorDefaults.ParamBase.Push(P_GENEVECTORINDIVIDUAL);

        public override object Genome
        {
            get => genome;

            set => genome = (Gene[])value;
        }
        public override int GenomeLength
        {
            get => genome.Length;
            set
            {
                var s = (GeneVectorSpecies)Species;
                var newGenome = new Gene[value];
                Array.Copy(genome, 0, newGenome, 0, genome.Length < newGenome.Length ? genome.Length : newGenome.Length);
                for (var x = genome.Length; x < newGenome.Length; x++)
                    newGenome[x] = (Gene)s.GenePrototype.Clone();  // not reset
                genome = newGenome;
            }
        }
        public override long Length => genome.Length;

        public Gene[] genome { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase); // actually unnecessary (Individual.Setup() is empty)

            // since VectorSpecies set its constraint values BEFORE it called
            // super.Setup(...) [which in turn called our Setup(...)], we know that
            // stuff like GenomeSize has already been set...

            var def = DefaultBase;

            if (!(Species is GeneVectorSpecies))
                state.Output.Fatal("GeneVectorIndividual requires a GeneVectorSpecies", paramBase, def);
            var s = (GeneVectorSpecies)Species;

            // note that genome isn't initialized with any genes yet -- they're all null.
            // Reset() needs
            genome = new Gene[s.GenomeSize];
            Reset(state, 0);
        }

        #endregion // Setup
        #region Operations

        #region Genome

        /// <summary>
        /// Splits the genome into n pieces, according to points, which <i>must</i> be sorted. 
        /// pieces.length must be 1 + points.length 
        /// </summary>
        public override void Split(int[] points, object[] pieces)
        {
            var point0 = 0;
            var point1 = points[0];
            for (var x = 0; x < pieces.Length; x++)
            {
                pieces[x] = new Gene[point1 - point0];
                Array.Copy(genome, point0, (Array)pieces[x], 0, point1 - point0);
                point0 = point1;
                point1 = x >= pieces.Length - 2
                    ? genome.Length
                    : points[x + 1];
            }
        }

        /// <summary>
        /// Joins the n pieces and sets the genome to their concatenation.
        /// </summary>
        public override void Join(object[] pieces)
        {
            var sum = pieces.Sum(t => ((Gene[])t).Length);

            var runningsum = 0;
            var newgenome = new Gene[sum];
            foreach (var t in pieces)
            {
                Array.Copy((Array)t, 0, newgenome, runningsum, ((Gene[])t).Length);
                runningsum += ((Gene[])t).Length;
            }
            // set genome
            genome = newgenome;
        }

        /// <summary>
        /// Initializes the individual by calling Reset(...) on each gene. 
        /// </summary>
        public override void Reset(IEvolutionState state, int thread)
        {
            var s = (GeneVectorSpecies)Species;

            for (var x = 0; x < genome.Length; x++)
            {
                // first create the gene if it doesn't exist
                if (genome[x] == null)
                    genome[x] = (Gene)s.GenePrototype.Clone();
                // now reset it
                genome[x].Reset(state, thread);
            }
        }

        #endregion // Genome
        #region Breeding

        public override void DefaultCrossover(IEvolutionState state, int thread, VectorIndividual ind)
        {
            var s = (GeneVectorSpecies)Species;
            var i = (GeneVectorIndividual)ind;
            Gene tmp;
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
            }
        }

        /// <summary>
        /// Destructively mutates the individual in some default manner.  The default form
        /// simply randomizes genes to a uniform distribution from the min and max of the gene values. 
        /// </summary>
        public override void DefaultMutate(IEvolutionState state, int thread)
        {
            var s = (GeneVectorSpecies)Species;
            for (var x = 0; x < genome.Length; x++)
            {
                if (state.Random[thread].NextBoolean(s.MutationProbability[x]))
                {
                    if (s.GetDuplicateRetries(x) <= 0)  // a little optimization
                    {
                        genome[x].Mutate(state, thread);
                    }
                    else    // argh
                    {
                        Gene old = (Gene)genome[x].Clone();
                        for (int retries = 0; retries < s.GetDuplicateRetries(x) + 1; retries++)
                        {
                            genome[x].Mutate(state, thread);
                            if (!genome[x].Equals(old)) break;
                            else genome[x] = old;  // try again.  Note that we're copying back just in case.
                        }

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

            return genome.Aggregate(hash, (current, t) => (current << 1 | BitShifter.URShift(current, 31)) ^ t.GetHashCode());
        }

        public override bool Equals(object ind)
        {
            if (ind == null) return false;
            if (!GetType().Equals(ind.GetType()))
                return false;
            var i = (GeneVectorIndividual)ind;
            if (genome.Length != i.genome.Length)
                return false;
            return !genome.Where((t, j) => !t.Equals(i.genome[j])).Any();
        }

        #endregion // Comparison
        #region Cloning

        public override object Clone()
        {
            var myobj = (GeneVectorIndividual)base.Clone();

            // must clone the genome
            myobj.genome = (Gene[])genome.Clone();
            for (var x = 0; x < genome.Length; x++)
                myobj.genome[x] = (Gene)genome[x].Clone();

            return myobj;
        }

        /// <summary>
        /// Clone all the genes
        /// </summary>
        public override void CloneGenes(Object piece)
        {
            var genes = (Gene[])piece;
            for (var i = 0; i < genes.Length; i++)
            {
                if (genes[i] != null) genes[i] = (Gene)genes[i].Clone();
            }
        }

        #endregion // Cloning
        #region ToString

        public override string GenotypeToStringForHumans()
        {
            var s = new StringBuilder();
            for (var i = 0; i < genome.Length; i++)
            { if (i > 0) s.Append(" "); s.Append(genome[i].PrintGeneToStringForHumans()); }
            return s.ToString();
        }

        public override string GenotypeToString()
        {
            var s = new StringBuilder();
            foreach (var t in genome)
            {
                s.Append(" ");
                s.Append(t.PrintGeneToString());
            }
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
                state.Output.Fatal("Individual with genome:\n" + s + "\n... does not have an integer at the beginning indicating the genome count.");
            var lll = (int)d.L;

            genome = new Gene[lll];

            var species = (GeneVectorSpecies)Species;
            for (var i = 0; i < genome.Length; i++)
            {
                genome[i] = (Gene)species.GenePrototype.Clone();
                genome[i].ReadGene(state, reader);
            }
        }

        public override void WriteGenotype(IEvolutionState state, BinaryWriter writer)
        {
            writer.Write(genome.Length);
            foreach (var t in genome)
                t.WriteGene(state, writer);
        }

        public override void ReadGenotype(IEvolutionState state, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            if (genome == null || genome.Length != len)
                genome = new Gene[len];
            var species = (GeneVectorSpecies)Species;

            for (var x = 0; x < genome.Length; x++)
            {
                genome[x] = (Gene)species.GenePrototype.Clone();
                genome[x].ReadGene(state, reader);
            }
        }

        #endregion // IO
    }
}