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
using BraneCloud.Evolution.EC.Randomization;
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Vector
{
    /// <summary> 
    /// FloatVectorIndividual is a VectorIndividual whose genome is an array of
    /// floats. Gene values may range from species.GetMinGene(x) to species.GetMaxGene(x),
    /// inclusive. The default mutation method randomizes genes to new values in this
    /// range, with <tt>species.MutationProbability</tt>. It can also add gaussian noise 
    /// to the genes, if so directed in the FloatVectorSpecies. If the gaussian noise 
    /// pushes the gene out of range, a new noise value is generated.
    /// 
    /// <p/>
    /// <b>From ec.Individual:</b>  
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
    /// <b>Default Base</b><br/>
    /// vector.float-vect-ind
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.vector.FloatVectorIndividual")]
    public class FloatVectorIndividual : VectorIndividual
    {
        #region Constants

        public const string P_FloatVectorIndividual = "float-vect-ind";

        public const double MAXIMUM_SHORT_IN_FLOAT = 1.6777216E7f;

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase => VectorDefaults.ParamBase.Push(P_FloatVectorIndividual);

        public override object Genome
        {
            get => genome;
            set => genome = (float[])value;
        }
        public float[] genome { get; set; }

        public override int GenomeLength
        {
            get => genome.Length;
            set
            {
                var newGenome = new float[value];
                Array.Copy(genome, 0, newGenome, 0, genome.Length < newGenome.Length ? genome.Length : newGenome.Length);
                genome = newGenome;
            }

        }

        /// <summary>
        /// Returns true if each gene value is within is specified [min,max] range.
        /// NaN is presently considered in range but the behavior of this method
        /// should be assumed to be unspecified on encountering NaN. 
        /// </summary>
        public virtual bool IsInRange
        {
            get
            {
                var species = (FloatVectorSpecies)Species;
                for (var i = 0; i < Length; i++)
                    if (genome[i] < species.GetMinGene(i) || genome[i] > species.GetMaxGene(i))
                        return false;
                return true;
            }
        }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase); // actually unnecessary (Individual.Setup() is empty)

            // since VectorSpecies set its constraint values BEFORE it called
            // super.Setup(...) [which in turn called our Setup(...)], we know that
            // stuff like GenomeSize has already been set...

            var def = DefaultBase;

            if (!(Species is FloatVectorSpecies))
                state.Output.Fatal("FloatVectorIndividual requires a FloatVectorSpecies", paramBase, def);
            var s = (FloatVectorSpecies)Species;

            genome = new float[s.GenomeSize];
        }
       
        #endregion // Setup
        #region Operations

        #region Genome

        /// <summary> 
        /// Splits the genome into n pieces, according to points, which *must* be
        /// sorted. pieces.length must be 1 + points.length
        /// </summary>
        public override void Split(int[] points, object[] pieces)
        {
            var point0 = 0;
            var point1 = points[0];
            for (var x = 0; x < pieces.Length; x++)
            {
                pieces[x] = new float[point1 - point0];
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
            var sum = pieces.Sum(t => ((float[]) t).Length);

            var runningsum = 0;
            var newgenome = new float[sum];
            foreach (var t in pieces)
            {
                Array.Copy((Array)t, 0, newgenome, runningsum, ((float[]) t).Length);
                runningsum += ((float[]) t).Length;
            }
            // set genome
            genome = newgenome;
        }

        /// <summary>
        /// Clips each gene value to be within its specified [min,max] range.  
        /// NaN is presently considered in range but the behavior of this method
        /// should be assumed to be unspecified on encountering NaN. 
        /// </summary>
        public virtual void Clamp()
        {
            var species = (FloatVectorSpecies)Species;
            for (var i = 0; i < Length; i++)
            {
                var minGene = (float)species.GetMinGene(i);
                if (genome[i] < minGene)
                    genome[i] = minGene;
                else
                {
                    var maxGene = (float)species.GetMaxGene(i);
                    if (genome[i] > maxGene)
                        genome[i] = maxGene;
                }
            }
        }

        /// <summary> 
        /// Initializes the individual by randomly choosing floats uniformly from MinGene to MaxGene.
        /// </summary>
        public override void Reset(IEvolutionState state, int thread)
        {
            var s = (FloatVectorSpecies)Species;
            IMersenneTwister random = state.Random[thread];
            for (int x = 0; x < genome.Length; x++)
            {
                int type = s.GetMutationType(x);
                if (type == FloatVectorSpecies.C_INTEGER_RESET_MUTATION ||
                    type == FloatVectorSpecies.C_INTEGER_RANDOM_WALK_MUTATION)  // integer type
                {
                    genome[x] = (float)(s.GetMinGene(x) + random.NextDouble(true, true) * (s.GetMaxGene(x) - s.GetMinGene(x)));
                }
                else
                {
                    int minGene = (int)Math.Floor(s.GetMinGene(x));
                    int maxGene = (int)Math.Floor(s.GetMaxGene(x));
                    genome[x] = RandomValueFromClosedInterval(minGene, maxGene, random); //minGene + random.nextInt(maxGene - minGene + 1);
                }
            }
        }

        #endregion // Genome
        #region Breeding

        public override void DefaultCrossover(IEvolutionState state, int thread, VectorIndividual ind)
        {
            var s = (FloatVectorSpecies) Species;
            var i = (FloatVectorIndividual) ind;
            float tmp;
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
                        var alpha = state.Random[thread].NextFloat(true, true) * (1 + 2 * s.LineDistance) - s.LineDistance;
                        var beta = state.Random[thread].NextFloat(true, true) * (1 + 2 * s.LineDistance) - s.LineDistance;
                        for (var x = 0; x < genome.Length; x++)
                        {
                            var min = s.GetMinGene(x);
                            var max = s.GetMaxGene(x);
                            var t = alpha * genome[x] + (1 - alpha) * i.genome[x];
                            var u = beta * i.genome[x] + (1 - beta) * genome[x];
                            if (!(t < min || t > max || u < min || u > max))
                            {
                                genome[x] = (float)t;
                                i.genome[x] = (float)u;
                            }
                        }
                    }
                    break;
                case VectorSpecies.C_INTERMED_RECOMB:
                    {
                        for (var x = 0; x < genome.Length; x++)
                        {
                            double t;
                            double u;
                            double min;
                            double max;
                            do
                            {
                                var alpha = state.Random[thread].NextFloat(true, true) * (1 + 2 * s.LineDistance) - s.LineDistance;
                                var beta = state.Random[thread].NextFloat(true, true) * (1 + 2 * s.LineDistance) - s.LineDistance;
                                min = s.GetMinGene(x);
                                max = s.GetMaxGene(x);
                                t = alpha * genome[x] + (1 - alpha) * i.genome[x];
                                u = beta * i.genome[x] + (1 - beta) * genome[x];
                            } while (t < min || t > max || u < min || u > max);
                            genome[x] = (float)t;
                            i.genome[x] = (float)u;
                        }
                    }
                    break;
                case VectorSpecies.C_SIMULATED_BINARY:
                    {
                        SimulatedBinaryCrossover(state.Random[thread], i, s.CrossoverDistributionIndex);
                    }
                    break;
            }
        }

        public void SimulatedBinaryCrossover(IMersenneTwister random, FloatVectorIndividual other, double eta_c)
        {
            const double eps = FloatVectorSpecies.SIMULATED_BINARY_CROSSOVER_EPS;
            var s = (FloatVectorSpecies) Species;
            float[] parent1 = genome;
            float[] parent2 = other.genome;
            //var minRealvar = s.MinGenes;
            //var maxRealvar = s.MaxGenes;

            for (var i = 0; i < parent1.Length; i++)
            {
                if (random.NextBoolean()) // 0.5f
                {
                    if (Math.Abs(parent1[i] - parent2[i]) > eps)
                    {
                        double y1;
                        double y2;
                        if (parent1[i] < parent2[i])
                        {
                            y1 = parent1[i];
                            y2 = parent2[i];
                        }
                        else
                        {
                            y1 = parent2[i];
                            y2 = parent1[i];
                        }
                        var yl = s.GetMinGene(i); //min_realvar[i];
                        var yu = s.GetMaxGene(i); //max_realvar[i];    
                        var rand = random.NextFloat();
                        var beta = 1.0 + (2.0*(y1 - yl)/(y2 - y1));
                        var alpha = 2.0 - Math.Pow(beta, -(eta_c + 1.0));
                        double betaq;
                        if (rand <= (1.0/alpha))
                        {
                            betaq = Math.Pow((rand*alpha), (1.0/(eta_c + 1.0)));
                        }
                        else
                        {
                            betaq = Math.Pow((1.0/(2.0 - rand*alpha)), (1.0/(eta_c + 1.0)));
                        }
                        var c1 = 0.5*((y1 + y2) - betaq*(y2 - y1));
                        beta = 1.0 + (2.0*(yu - y2)/(y2 - y1));
                        alpha = 2.0 - Math.Pow(beta, -(eta_c + 1.0));
                        if (rand <= (1.0/alpha))
                        {
                            betaq = Math.Pow((rand*alpha), (1.0/(eta_c + 1.0)));
                        }
                        else
                        {
                            betaq = Math.Pow((1.0/(2.0 - rand*alpha)), (1.0/(eta_c + 1.0)));
                        }
                        var c2 = 0.5*((y1 + y2) + betaq*(y2 - y1));
                        if (c1 < yl)
                            c1 = yl;
                        if (c2 < yl)
                            c2 = yl;
                        if (c1 > yu)
                            c1 = yu;
                        if (c2 > yu)
                            c2 = yu;
                        if (random.NextBoolean())
                        {
                            parent1[i] = (float) c2;
                            parent2[i] = (float) c1;
                        }
                        else
                        {
                            parent1[i] = (float) c1;
                            parent2[i] = (float) c2;
                        }
                    }
                    else
                    {
                        // do nothing
                    }
                }
                else
                {
                    // do nothing
                }
            }
        }

        // for ints
        int RandomValueFromClosedInterval(int min, int max, IMersenneTwister random)
        {
            if (max - min < 0) // we had an overflow
            {
                int l = 0;
                do l = random.NextInt();
                while (l < min || l > max);
                return l;
            }
            return min + random.NextInt(max - min + 1);
        }

        /// <summary> 
        /// Destructively mutates the individual in some default manner. The default
        /// form simply randomizes genes to a uniform distribution from the min and
        /// max of the gene values. It can also add gaussian noise to the genes, 
        /// if so directed in the FloatVectorSpecies. If the gaussian noise
        /// pushes the gene out of range, a new noise value is generated.
        /// </summary>
        public override void  DefaultMutate(IEvolutionState state, int thread)
        {
            var s = (FloatVectorSpecies)Species;

            var rng = state.Random[thread];

            for (int x = 0; x < genome.Length; x++)
                if (rng.NextBoolean(s.GetMutationProbability(x)))
                {
                    float old = genome[x];
                    for (int retries = 0; retries < s.GetDuplicateRetries(x) + 1; retries++)
                    {
                        switch (s.GetMutationType(x))
                        {
                            case FloatVectorSpecies.C_GAUSS_MUTATION:
                                GaussianMutation(state, rng, s, x);
                                break;
                            case FloatVectorSpecies.C_POLYNOMIAL_MUTATION:
                                PolynomialMutation(state, rng, s, x);
                                break;
                            case FloatVectorSpecies.C_RESET_MUTATION:
                                FloatResetMutation(rng, s, x);
                                break;
                            case FloatVectorSpecies.C_INTEGER_RESET_MUTATION:
                                IntegerResetMutation(rng, s, x);
                                break;
                            case FloatVectorSpecies.C_INTEGER_RANDOM_WALK_MUTATION:
                                IntegerRandomWalkMutation(rng, s, x);
                                break;
                        }
                        if (genome[x] != old) break;
                        // else genome[x] = old;  // try again
                    }
                }
        }

        void IntegerRandomWalkMutation(IMersenneTwister random, FloatVectorSpecies species, int index)
        {
            double min = species.GetMinGene(index);
            double max = species.GetMaxGene(index);
            if (!species.GetMutationIsBounded(index))
            {
                // okay, technically these are still bounds, but we can't go beyond this without weird things happening
                max = MAXIMUM_SHORT_IN_FLOAT;
                min = -(max);
            }
            do
            {
                int n = (int)(random.NextBoolean() ? 1 : -1);
                float g = (float)Math.Floor(genome[index]);
                if ((n == 1 && g < max) ||
                    (n == -1 && g > min))
                    genome[index] = g + n;
                else if ((n == -1 && g < max) ||
                    (n == 1 && g > min))
                    genome[index] = g - n;
            }
            while (random.NextBoolean(species.GetRandomWalkProbability(index)));
        }

        void IntegerResetMutation(IMersenneTwister random, FloatVectorSpecies species, int index)
        {
            int minGene = (int)Math.Floor(species.GetMinGene(index));
            int maxGene = (int)Math.Floor(species.GetMaxGene(index));
            genome[index] = RandomValueFromClosedInterval(minGene, maxGene, random);  // minGene + random.nextLong(maxGene - minGene + 1);
        }

        void FloatResetMutation(IMersenneTwister random, FloatVectorSpecies species, int index)
        {
            double minGene = species.GetMinGene(index);
            double maxGene = species.GetMaxGene(index);
            genome[index] = (float)(minGene + random.NextFloat(true, true) * (maxGene - minGene));
        }

        void GaussianMutation(IEvolutionState state, IMersenneTwister random, FloatVectorSpecies species, int index)
        {
            double val;
            double min = species.GetMinGene(index);
            double max = species.GetMaxGene(index);
            double stdev = species.GetGaussMutationStdev(index);
            int outOfBoundsLeftOverTries = species.OutOfBoundsRetries;
            bool givingUpAllowed = species.OutOfBoundsRetries != 0;
            do
            {
                val = random.NextGaussian() * stdev + genome[index];
                outOfBoundsLeftOverTries--;
                if (species.GetMutationIsBounded(index) && (val > max || val < min))
                {
                    if (givingUpAllowed && (outOfBoundsLeftOverTries == 0))
                    {
                        val = min + random.NextFloat() * (max - min);
                        species.OutOfRangeRetryLimitReached(state);// it better get inlined
                        break;
                    }
                }
                else break;
            }
            while (true);
            genome[index] = (float)val;
        }

        void PolynomialMutation(IEvolutionState state, IMersenneTwister random, FloatVectorSpecies species, int index)
        {
            double eta_m = species.GetMutationDistributionIndex(index);
            bool alternativePolynomialVersion = species.GetPolynomialIsAlternative(index);

            double rnd, delta1, delta2, mut_pow, deltaq;
            double y, yl, yu, val, xy;
            double y1;

            y1 = y = genome[index];  // ind[index];
            yl = species.GetMinGene(index); // min_realvar[index];
            yu = species.GetMaxGene(index); // max_realvar[index];
            delta1 = (y - yl) / (yu - yl);
            delta2 = (yu - y) / (yu - yl);

            int totalTries = species.OutOfBoundsRetries;
            int tries = 0;
            for (tries = 0; tries < totalTries || totalTries == 0; tries++)  // keep trying until totalTries is reached if it's not zero.  If it's zero, go on forever.
            {
                rnd = random.NextFloat();
                mut_pow = 1.0 / (eta_m + 1.0);
                if (rnd <= 0.5)
                {
                    xy = 1.0 - delta1;
                    val = 2.0 * rnd + (alternativePolynomialVersion ? (1.0 - 2.0 * rnd) * (Math.Pow(xy, (eta_m + 1.0))) : 0.0);
                    deltaq = Math.Pow(val, mut_pow) - 1.0;
                }
                else
                {
                    xy = 1.0 - delta2;
                    val = 2.0 * (1.0 - rnd) + (alternativePolynomialVersion ? 2.0 * (rnd - 0.5) * (Math.Pow(xy, (eta_m + 1.0))) : 0.0);
                    deltaq = 1.0 - (Math.Pow(val, mut_pow));
                }
                y1 = y + deltaq * (yu - yl);
                if (!species.GetMutationIsBounded(index) || (y1 >= yl && y1 <= yu)) break;  // yay, found one
            }

            // at this point, if tries is totalTries, we failed
            if (totalTries != 0 && tries == totalTries)
            {
                // just randomize
                y1 = (float)(species.GetMinGene(index) + random.NextFloat(true, true) * (species.GetMaxGene(index) - species.GetMinGene(index)));  //(float)(min_realvar[index] + random.NextFloat() * (max_realvar[index] - min_realvar[index]));
                species.OutOfRangeRetryLimitReached(state);// it better get inlined
            }
            genome[index] = (float)y1; // ind[index] = y1;
        }

        /// <summary>
        /// This function is broken out to keep it identical to NSGA-II's mutation.c code.
        /// eta_m is the distribution index.
        /// </summary>
        public void PolynomialMutate(IEvolutionState state, IMersenneTwister random, FloatVectorIndividual individual,
            double eta_m, bool alternativePolynomialVersion, bool mutationIsBounded)
        {
            var s = (FloatVectorSpecies) individual.Species;
            float[] ind = genome;
            //var minRealvar = s.MinGenes;
            //var maxRealvar = s.MaxGenes;

            for (var j = 0; j < ind.Length; j++)
            {
                if (random.NextBoolean(s.MutationProbability[j]))
                {
                    double y;
                    var y1 = y = ind[j];
                    var yl = s.GetMinGene(j); //min_realvar[j];
                    var yu = s.GetMaxGene(j); //max_realvar[j];
                    var delta1 = (y - yl)/(yu - yl);
                    var delta2 = (yu - y)/(yu - yl);

                    var totalTries = s.OutOfBoundsRetries;
                    var tries = 0;
                    for (tries = 0; tries < totalTries || totalTries == 0; tries++)
                        // keep trying until totalTries is reached if it's not zero.  If it's zero, go on forever.
                    {
                        var rnd = random.NextFloat();
                        var mutPow = 1.0 / (eta_m + 1.0);
                        double deltaq;
                        double val;
                        double xy;
                        if (rnd <= 0.5)
                        {
                            xy = 1.0 - delta1;
                            val = 2.0 * rnd +
                                  (alternativePolynomialVersion ? (1.0 - 2.0*rnd) * (Math.Pow(xy, (eta_m + 1.0))) : 0.0);
                            deltaq = Math.Pow(val, mutPow) - 1.0;
                        }
                        else
                        {
                            xy = 1.0 - delta2;
                            val = 2.0 * (1.0 - rnd) +
                                  (alternativePolynomialVersion ? 2.0 * (rnd - 0.5) * (Math.Pow(xy, (eta_m + 1.0))) : 0.0);
                            deltaq = 1.0 - (Math.Pow(val, mutPow));
                        }
                        y1 = y + deltaq * (yu - yl);
                        if (!mutationIsBounded || (y1 >= yl && y1 <= yu)) break; // yay, found one
                    }

                    // at this point, if tries is totalTries, we failed
                    if (totalTries != 0 && tries == totalTries)
                    {
                        // just randomize
                        //y1 = (float) (minRealvar[j] + random.NextFloat() * (maxRealvar[j] - minRealvar[j]));
                        y1 = (float)(s.GetMinGene(j) + random.NextFloat(true, true) * (s.GetMaxGene(j) - s.GetMinGene(j)));
                        s.OutOfRangeRetryLimitReached(state);// it better get inlined
                    }
                    ind[j] = (float) y1;
                }
            }
        }

        #endregion // Breeding

        #endregion // Operations
        #region Comparison

        public override int GetHashCode()
        {
            // stolen from GPIndividual. It's a decent algorithm.
            var hash = GetType().GetHashCode();
            
            hash = (hash << 1 | BitShifter.URShift(hash, 31));
            for (var x = 0; x < genome.Length; x++)
            {
                hash = (hash << 1 | BitShifter.URShift(hash, 31)) ^ ((int)BitConverter.DoubleToInt64Bits(genome[x]));
            }
            
            return hash;
        }

        public override bool Equals(object ind)
        {
            if (ind == null) return false;
            if (!(GetType().Equals(ind.GetType())))
                return false; // SimpleRuleIndividuals are special.
            var i = (FloatVectorIndividual)ind;
            if (genome.Length != i.genome.Length)
                return false;
            for (var j = 0; j < genome.Length; j++)
                if (genome[j] != i.genome[j])
                    return false;
            return true;
        }

        #endregion // Comparison
        #region Cloning

        public override object Clone()
        {
            var myobj = (FloatVectorIndividual)(base.Clone());

            // must clone the genome
            myobj.Genome = genome.Clone();

            return myobj;
        }

        #endregion // Cloning
        #region ToString

        public override string GenotypeToStringForHumans()
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < genome.Length; i++)
            { if (i > 0) s.Append(" "); s.Append(genome[i]); }
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
            // read in the next line. The first item is the number of genes
            var s = reader.ReadLine();
            var d = new DecodeReturn(s);
            Code.Decode(d);
            if (d.Type != DecodeReturn.T_INTEGER)  // uh oh
                state.Output.Fatal("Individual with genome:\n" + s + "\n... does not have an integer at the beginning indicating the genome count.");
            var lll = (int)(d.L);

            genome = new float[lll];

            // read in the genes
            for (var i = 0; i < genome.Length; i++)
            {
                Code.Decode(d);
                genome[i] = (float)(d.D);
            }
        }

        public override void WriteGenotype(IEvolutionState state, BinaryWriter writer)
        {
            writer.Write(genome.Length);
            foreach (var t in genome)
                writer.Write(t);
        }

        public override void ReadGenotype(IEvolutionState state, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            if (genome == null || genome.Length != len)
                genome = new float[len];
            for (var x = 0; x < genome.Length; x++)
                genome[x] = reader.ReadSingle();
        }

        #endregion // IO
    }
}