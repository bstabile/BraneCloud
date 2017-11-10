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
using BraneCloud.Evolution.EC.Randomization;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.App.ECSuite
{
    /**
       Several standard Evolutionary Computation functions are implemented.
       As the SimpleFitness is used for maximization problems, the mapping f(x) --> -f(x) is used to transform
       the problems into maximization ones.

       <p><b>Parameters</b><br>
       <table>
       <tr><td valign=top><i>base</i>.<tt>type</tt><br>
       <font size=-1>String, one of: rosenbrock rastrigin sphere step noisy-quartic kdj-f1 kdj-f2 kdj-f3 kdj-f4 booth griewank median sum product schwefel min rotated-rastrigin rotated-schwefel rotated-griewank langerman lennard-jones lunacek</font></td>
       <td valign=top>(The vector problem to test against.  Some of the types are synonyms: kdj-f1 = sphere, kdj-f2 = rosenbrock, kdj-f3 = step, kdj-f4 = noisy-quartic.  "kdj" stands for "Ken DeJong", and the numbers are the problems in his test suite)</td></tr>
       <tr><td valign=top><i>base</i>.<tt>seed</tt><br>
       <font size=-1>int > 0</font></td>
       <td valign=top>(Random number seed for rotated problems)</td></tr>
       </table>
*/

    [ECConfiguration("ec.app.ecsuite.ECSuite")]
    public class ECSuite : Problem, ISimpleProblem
    {
        #region Constants

        public const string P_SEED = "seed";
        public const string P_WHICH_PROBLEM = "type";

        public const string V_ROSENBROCK = "rosenbrock";
        public const string V_RASTRIGIN = "rastrigin";
        public const string V_SPHERE = "sphere";
        public const string V_STEP = "step";
        public const string V_NOISY_QUARTIC = "noisy-quartic";
        public const string V_F1 = "kdj-f1";
        public const string V_F2 = "kdj-f2";
        public const string V_F3 = "kdj-f3";
        public const string V_F4 = "kdj-f4";
        public const string V_BOOTH = "booth";
        public const string V_GRIEWANGK = "griewangk";
        public const string V_GRIEWANK = "griewank";
        public const string V_MEDIAN = "median";
        public const string V_SUM = "sum";
        public const string V_PRODUCT = "product";
        public const string V_SCHWEFEL = "schwefel";
        public const string V_MIN = "min";
        public const string V_ROTATED_RASTRIGIN = "rotated-rastrigin";
        public const string V_ROTATED_SCHWEFEL = "rotated-schwefel";
        public const string V_ROTATED_GRIEWANK = "rotated-griewank";
        public const string V_LANGERMAN = "langerman" ;
        public const string V_LENNARDJONES = "lennard-jones" ;
        public const string V_LUNACEK = "lunacek" ;

        public const int PROB_ROSENBROCK = 0;
        public const int PROB_RASTRIGIN = 1;
        public const int PROB_SPHERE = 2;
        public const int PROB_STEP = 3;
        public const int PROB_NOISY_QUARTIC = 4;
        public const int PROB_BOOTH = 5;
        public const int PROB_GRIEWANK = 6;
        public const int PROB_MEDIAN = 7;
        public const int PROB_SUM = 8;
        public const int PROB_PRODUCT = 9;
        public const int PROB_SCHWEFEL = 10;
        public const int PROB_MIN = 11;
        public const int PROB_ROTATED_RASTRIGIN = 12;
        public const int PROB_ROTATED_SCHWEFEL = 13;
        public const int PROB_ROTATED_GRIEWANK = 14;
        public const int PROB_LANGERMAN = 15;
        public const int PROB_LENNARDJONES = 16;
        public const int PROB_LUNACEK = 17;

        // For the Rotation Facility
        /// <summary>
        /// Fixed rotation seed so all jobs use the same rotation space.
        /// </summary>
        public const long ROTATION_SEED = 9731297;

        #endregion // Constants
        #region Static

        public static readonly string[] ProblemName = {
                                                            V_ROSENBROCK,
                                                            V_RASTRIGIN,
                                                            V_SPHERE,
                                                            V_STEP,
                                                            V_NOISY_QUARTIC,
                                                            V_BOOTH,
                                                            V_GRIEWANK,
                                                            V_MEDIAN,
                                                            V_SUM,
                                                            V_PRODUCT,
                                                            V_SCHWEFEL,
                                                            V_MIN,
                                                            V_ROTATED_RASTRIGIN,
                                                            V_ROTATED_SCHWEFEL,
                                                            V_ROTATED_GRIEWANK,
                                                            V_LANGERMAN,
                                                            V_LENNARDJONES,
                                                            V_LUNACEK,
                                                        };

        public static readonly double[] MinRange = {
                                                           -2.048, // rosenbrock
                                                           -5.12, // rastrigin
                                                           -5.12, // sphere
                                                           -5.12, // step
                                                           -1.28, // noisy quartic
                                                           -5.12, // booth
                                                           -600.0, // griewank
                                                           0.0, // median
                                                           0.0, // sum
                                                           0.0, // product
                                                           -512.03, // schwefel
                                                           0.0, // min
                                                           -5.12, // rotated-rastrigin
                                                           -512.03 // rotated-schwefel
                                                    };

        public static readonly double[] MaxRange = {
                                                           2.048, // rosenbrock
                                                           5.12, // rastrigin
                                                           5.12, // sphere
                                                           5.12, // step
                                                           1.28, // noisy quartic
                                                           5.12, // booth
                                                           600.0, // griewank
                                                           1.0, // median
                                                           1.0, // sum
                                                           2.0, // product
                                                           511.97, // schwefel
                                                           1.0, // min
                                                           5.12, // rotated-rastrigin
                                                           511.97 // rotated-schwefel
                                                    };


        #region Rotation Facility

        /*	
       -----------------
       Rotation facility
       -----------------	
       This code is just used by the Rotated Schwefel and Rotated Rastrigin functions to rotate their
       functions by a certain amount.  The code is largely based on the rotation scheme described in
       "Completely Derandomized Self-Adaptation in Evolutionary Strategies", 
       Nikolaus Hansen and Andreas Ostermeier, Evolutionary Computation 9(2): 159--195.
    
       We fix a hard-coded rotation matrix which is the same for all problems, in order to guarantee
       correctness in gathering results over multiple jobs.  But you can change that easily if you like.	
       */

        public static double[][][] RotationMatrix = new double[1][][];  // the actual matrix is stored in RotationMatrix[0] -- a hack

        /// <summary>
        /// Dot product between two column vectors.  Does not modify the original vectors.
        /// </summary>
        public static double Dot(double[] x, double[] y)
        {
            double val = 0;
            for (var i = 0; i < x.Length; i++)
                val += x[i] * y[i];
            return val;
        }

        /// <summary>
        /// Multiply a column vector against a matrix[row][column].  Does not modify the original vector or matrix.
        /// </summary>
        public static double[] Mul(double[/* row */ ][ /* column */] matrix, double[] x)
        {
            var val = new double[matrix.Length];
            for (var i = 0; i < matrix.Length; i++)
            {
                var sum = 0.0;
                var m = matrix[i];
                for (var j = 0; j < m.Length; j++)
                    sum += m[j] * x[j];
                val[i] = sum;
            }
            return val;
        }

        /// <summary>
        /// Scalar multiply against a column vector. Does not modify the original vector.
        /// </summary>
        public static double[] ScalarMul(double scalar, double[] x)
        {
            var val = new double[x.Length];
            for (var i = 0; i < x.Length; i++)
                val[i] = x[i] * scalar;
            return val;
        }

        /// <summary>
        /// Subtract two column vectors.  Does not modify the original vectors.
        /// </summary>
        public static double[] Sub(double[] x, double[] y)
        {
            var val = new double[x.Length];
            for (var i = 0; i < x.Length; i++)
                val[i] = x[i] - y[i];
            return val;
        }

        /// <summary>
        /// Normalize a column vector.  Does not modify the original vector.
        /// </summary>
        public static double[] Normalize(double[] x)
        {
            var val = new double[x.Length];
            var sumsq = 0.0;
            for (var i = 0; i < x.Length; i++)
                sumsq += x[i] * x[i];
            sumsq = Math.Sqrt(sumsq);
            for (var i = 0; i < x.Length; i++)
                val[i] = x[i] / sumsq;
            return val;
        }

        /// <summary>
        /// Build an NxN rotation matrix[row][column] with a given seed.
        /// </summary>
        public static double[ /* row */ ][ /* column */] BuildRotationMatrix(IEvolutionState state, long rotationSeed, int N)
        {
            if (rotationSeed == ROTATION_SEED)
                state.Output.WarnOnce("Default rotation seed being used (" + rotationSeed + ")");

            IMersenneTwister rand = new MersenneTwisterFast(ROTATION_SEED);
            for (int i = 0; i < 624 * 4; i++) // prime the MT for 4 full sample iterations to get it warmed up
                rand.NextInt();

            double[/* row */][/* column */] o = TensorFactory.Create<double>(N, N);

            // make random values
            for (var i = 0; i < N; i++)
                for (var k = 0; k < N; k++)
                    o[i][k] = rand.NextGaussian();

            // build random values
            for (var i = 0; i < N; i++)
            {
                // extract o[i] -> no
                var no = new double[N];
                for (var k = 0; k < N; k++)
                    no[k] = o[i][k];

                // go through o[i] and o[j], modifying no 
                for (var j = 0; j < i; j++)
                {
                    var d = Dot(o[i], o[j]);
                    var val = ScalarMul(d, o[j]);
                    no = Sub(no, val);
                }
                o[i] = Normalize(no);
            }

            return o;
        }

        #endregion // Rotation Facility

        #endregion // Static
        #region Fields

        bool _alreadyChecked;

        /// <summary>
        /// Rotation seed for rotation problems
        /// </summary>
        private long seed;

        #region Magic Arrays for the Langerman Problem

        private readonly double[][] _afox10 =
            {
                new [] {9.681, 0.667, 4.783, 9.095, 3.517, 9.325, 6.544, 0.211, 5.122, 2.020},
                new [] {9.400, 2.041, 3.788, 7.931, 2.882, 2.672, 3.568, 1.284, 7.033, 7.374},
                new [] {8.025, 9.152, 5.114, 7.621, 4.564, 4.711, 2.996, 6.126, 0.734, 4.982},
                new [] {2.196, 0.415, 5.649, 6.979, 9.510, 9.166, 6.304, 6.054, 9.377, 1.426},
                new [] {8.074, 8.777, 3.467, 1.863, 6.708, 6.349, 4.534, 0.276, 7.633, 1.567},
                new [] {7.650, 5.658, 0.720, 2.764, 3.278, 5.283, 7.474, 6.274, 1.409, 8.208},
                new [] {1.256, 3.605, 8.623, 6.905, 0.584, 8.133, 6.071, 6.888, 4.187, 5.448},
                new [] {8.314, 2.261, 4.224, 1.781, 4.124, 0.932, 8.129, 8.658, 1.208, 5.762},
                new [] {0.226, 8.858, 1.420, 0.945, 1.622, 4.698, 6.228, 9.096, 0.972, 7.637},
                new [] {7.305, 2.228, 1.242, 5.928, 9.133, 1.826, 4.060, 5.204, 8.713, 8.247},
                new [] {0.652, 7.027, 0.508, 4.876, 8.807, 4.632, 5.808, 6.937, 3.291, 7.016},
                new [] {2.699, 3.516, 5.874, 4.119, 4.461, 7.496, 8.817, 0.690, 6.593, 9.789},
                new [] {8.327, 3.897, 2.017, 9.570, 9.825, 1.150, 1.395, 3.885, 6.354, 0.109},
                new [] {2.132, 7.006, 7.136, 2.641, 1.882, 5.943, 7.273, 7.691, 2.880, 0.564},
                new [] {4.707, 5.579, 4.080, 0.581, 9.698, 8.542, 8.077, 8.515, 9.231, 4.670},
                new [] {8.304, 7.559, 8.567, 0.322, 7.128, 8.392, 1.472, 8.524, 2.277, 7.826},
                new [] {8.632, 4.409, 4.832, 5.768, 7.050, 6.715, 1.711, 4.323, 4.405, 4.591},
                new [] {4.887, 9.112, 0.170, 8.967, 9.693, 9.867, 7.508, 7.770, 8.382, 6.740},
                new [] {2.440, 6.686, 4.299, 1.007, 7.008, 1.427, 9.398, 8.480, 9.950, 1.675},
                new [] {6.306, 8.583, 6.084, 1.138, 4.350, 3.134, 7.853, 6.061, 7.457, 2.258},
                new [] {0.652, 0.343, 1.370, 0.821, 1.310, 1.063, 0.689, 8.819, 8.833, 9.070},
                new [] {5.558, 1.272, 5.756, 9.857, 2.279, 2.764, 1.284, 1.677, 1.244, 1.234},
                new [] {3.352, 7.549, 9.817, 9.437, 8.687, 4.167, 2.570, 6.540, 0.228, 0.027},
                new [] {8.798, 0.880, 2.370, 0.168, 1.701, 3.680, 1.231, 2.390, 2.499, 0.064},
                new [] {1.460, 8.057, 1.336, 7.217, 7.914, 3.615, 9.981, 9.198, 5.292, 1.224},
                new [] {0.432, 8.645, 8.774, 0.249, 8.081, 7.461, 4.416, 0.652, 4.002, 4.644},
                new [] {0.679, 2.800, 5.523, 3.049, 2.968, 7.225, 6.730, 4.199, 9.614, 9.229},
                new [] {4.263, 1.074, 7.286, 5.599, 8.291, 5.200, 9.214, 8.272, 4.398, 4.506},
                new [] {9.496, 4.830, 3.150, 8.270, 5.079, 1.231, 5.731, 9.494, 1.883, 9.732},
                new [] {4.138, 2.562, 2.532, 9.661, 5.611, 5.500, 6.886, 2.341, 9.699, 6.500}
        };

        private readonly double[] _cfox10 =
            {
                0.806,  0.517,  1.5,    0.908,  0.965,
                0.669,  0.524,  0.902,  0.531,  0.876,
                0.462,  0.491,  0.463,  0.714,  0.352,
                0.869,  0.813,  0.811,  0.828,  0.964,
                0.789,  0.360,  0.369,  0.992,  0.332,
                0.817,  0.632,  0.883,  0.608,  0.326
            };

        private double langerman(double[] genome)
        {

            double sum = 0;

            for (int i = 0; i < 30; i++)
            {
                // compute squared distance
                double distsq = 0.0;
                double t;
                double[] afox10i = _afox10[i];
                for (int j = 0; j < genome.Length; j++)
                {
                    t = genome[j] - afox10i[j];
                    distsq += t * t;
                }

                sum += _cfox10[i] * Math.Exp(-distsq / Math.PI) * Math.Cos(distsq * Math.PI);
            }
            return 0 - sum;
        }

        #endregion

        #endregion // Fields
        #region Properties

        /// <summary>
        /// Defaults on Rosenbrock
        /// </summary>
        public int ProblemType { get; set; } = PROB_ROSENBROCK;

        #endregion // Properties
        #region Setup

        // nothing....
        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            var wp = state.Parameters.GetStringWithDefault(paramBase.Push(P_WHICH_PROBLEM), null, "");
            if (wp.CompareTo(V_ROSENBROCK) == 0 || wp.CompareTo(V_F2) == 0)
                ProblemType = PROB_ROSENBROCK;
            else if (wp.CompareTo(V_RASTRIGIN) == 0)
                ProblemType = PROB_RASTRIGIN;
            else if (wp.CompareTo(V_SPHERE) == 0 || wp.CompareTo(V_F1) == 0)
                ProblemType = PROB_SPHERE;
            else if (wp.CompareTo(V_STEP) == 0 || wp.CompareTo(V_F3) == 0)
                ProblemType = PROB_STEP;
            else if (wp.CompareTo(V_NOISY_QUARTIC) == 0 || wp.CompareTo(V_F4) == 0)
                ProblemType = PROB_NOISY_QUARTIC;
            else if (wp.CompareTo(V_BOOTH) == 0)
                ProblemType = PROB_BOOTH;
            else if (wp.CompareTo(V_GRIEWANK) == 0)
                ProblemType = PROB_GRIEWANK;
            else if (wp.CompareTo(V_GRIEWANGK) == 0)
            {
                state.Output.Warning("Incorrect parameter name (\"griewangk\") used, should be \"griewank\"",
                                     paramBase.Push(P_WHICH_PROBLEM), null);
                ProblemType = PROB_GRIEWANK;
            }
            else if (wp.CompareTo(V_MEDIAN) == 0)
                ProblemType = PROB_MEDIAN;
            else if (wp.CompareTo(V_SUM) == 0)
                ProblemType = PROB_SUM;
            else if (wp.CompareTo(V_PRODUCT) == 0)
                ProblemType = PROB_PRODUCT;
            else if (wp.CompareTo(V_SCHWEFEL) == 0)
                ProblemType = PROB_SCHWEFEL;
            else if (wp.CompareTo(V_MIN) == 0)
                ProblemType = PROB_MIN;
            else if (wp.CompareTo(V_ROTATED_RASTRIGIN) == 0)
                ProblemType = PROB_ROTATED_RASTRIGIN;
            else if (wp.CompareTo(V_ROTATED_SCHWEFEL) == 0)
                ProblemType = PROB_ROTATED_SCHWEFEL;
            else if (wp.CompareTo(V_ROTATED_GRIEWANK) == 0)
                ProblemType = PROB_ROTATED_GRIEWANK;
            else if (wp.CompareTo(V_LANGERMAN) == 0)
                ProblemType = PROB_LANGERMAN;
            else if (wp.CompareTo(V_LENNARDJONES) == 0)
                ProblemType = PROB_LENNARDJONES;
            else if (wp.CompareTo(V_LUNACEK) == 0)
                ProblemType = PROB_LUNACEK;
            else
                state.Output.Fatal(
                    "Invalid value for parameter, or parameter not found.\n" +
                    "Acceptable values are:\n" +
                    "  " + V_ROSENBROCK + " (or " + V_F2 + ")\n" +
                    "  " + V_RASTRIGIN + "\n" +
                    "  " + V_SPHERE + " (or " + V_F1 + ")\n" +
                    "  " + V_STEP + " (or " + V_F3 + ")\n" +
                    "  " + V_NOISY_QUARTIC + " (or " + V_F4 + ")\n" +
                    "  " + V_BOOTH + "\n" +
                    "  " + V_GRIEWANK + "\n" +
                    "  " + V_MEDIAN + "\n" +
                    "  " + V_SUM + "\n" +
                    "  " + V_PRODUCT + "\n" +
                    "  " + V_SCHWEFEL + "\n" +
                    "  " + V_MIN + "\n" +
                    "  " + V_ROTATED_RASTRIGIN + "\n" +
                    "  " + V_ROTATED_SCHWEFEL + "\n" +
                    "  " + V_ROTATED_GRIEWANK + "\n" +
                    "  " + V_LANGERMAN + "\n" +
                    "  " + V_LENNARDJONES + "\n" +
                    "  " + V_LUNACEK + "\n",
                    paramBase.Push(P_WHICH_PROBLEM));

            seed = state.Parameters.GetLongWithDefault(paramBase.Push(P_SEED), null, ROTATION_SEED);
            if (seed <= 0)
                state.Output.Fatal("If a rotation seed is provided, it must be > 0", paramBase.Push(P_SEED), null);
        }

        #endregion // Setup
        #region Operations

        public void CheckRange(IEvolutionState state, int problem, double[] genome)
        {
            if (_alreadyChecked || state.Generation > 0) return;
            _alreadyChecked = true;

            for (var i = 0; i < state.Population.Subpops.Count; i++)
            {
                if (!(state.Population.Subpops[i].Species is FloatVectorSpecies))
                {
                    state.Output.Fatal("ECSuite requires species " + i + " to be a FloatVectorSpecies, but it is a: " +
                                       state.Population.Subpops[i].Species);
                }
                var species = (FloatVectorSpecies)state.Population.Subpops[i].Species;
                for (var k = 0; k < species.MinGenes.Length; k++)
                {
                    if (!species.MinGenes[k].Equals(MinRange[problem]) ||
                        !species.MaxGenes[k].Equals(MaxRange[problem]))
                    {
                        state.Output.Warning("Gene range is nonstandard for problem " + ProblemName[problem]
                                             + ".\nFirst occurrence: Subpopulation " + i + " Gene " + k
                                             + " range was [" + species.MinGenes[k] + ", " + species.MaxGenes[k]
                                             + "], expected [" + MinRange[problem] + ", " + MaxRange[problem] + "]");

                        return; // done here
                    }
                }
            }
            if (ProblemType == PROB_LANGERMAN)
            {
                // Langerman has a maximum genome size of 10
                if (genome.Length > 10)
                    state.Output.Fatal("The Langerman function requires that the genome size be a value from 1 to 10 inclusive.  It is presently " + genome.Length);
            }

            else if (ProblemType == PROB_LENNARDJONES)
            {
                // Lennard-Jones requires that its genomes be multiples of 3
                if (genome.Length % 3 != 0)
                    state.Output.Fatal("The Lennard-Jones function requires that the genome size be a multiple of 3.  It is presently " + genome.Length);
            }
        }

        public void Evaluate(IEvolutionState state,
            Individual ind,
            int subpop,
            int threadnum)
        {
            if (ind.Evaluated)  // don't bother reevaluating
                return;

            if (!(ind is DoubleVectorIndividual))
                state.Output.Fatal("The individuals for this problem should be DoubleVectorIndividuals.");

            var temp = (DoubleVectorIndividual)ind;
            double[] genome = temp.genome;
            //var len = genome.Length;

            // this curious break-out makes it easy to use the isOptimal() and function() methods
            // for other purposes, such as coevolutionary versions of this class.

            // compute the fitness on a per-function basis
            double fit = Function(state, ProblemType, temp.genome, threadnum);

            // compute if we're optimal on a per-function basis
            bool isOptimal = IsOptimal(ProblemType, fit);

            // TODO: BRS: I don't think this works the same way in .NET as in Java! ;-)
            // set the fitness appropriately
            //if (fit < (0.0 - double.MaxValue))  // uh oh -- can be caused by Product for example
            if (double.IsNegativeInfinity(fit))  // uh oh -- can be caused by Product for example
            {
                ((SimpleFitness)ind.Fitness).SetFitness(state, double.MinValue, isOptimal);
                state.Output.WarnOnce("'Product' type used: some fitnesses are negative infinity, setting to lowest legal negative number.");
            }
            //else if (fit > double.MaxValue)  // uh oh -- can be caused by Product for example
            else if (double.IsPositiveInfinity(fit))  // uh oh -- can be caused by Product for example
            {
                ((SimpleFitness)ind.Fitness).SetFitness(state, double.MaxValue, isOptimal);
                state.Output.WarnOnce("'Product' type used: some fitnesses are positive infinity, setting to highest legal number.");
            }
            else
            {
                ((SimpleFitness)ind.Fitness).SetFitness(state, fit, isOptimal);
            }
            ind.Evaluated = true;
        }

        public bool IsOptimal(int function, double fitness)
        {
            switch (ProblemType)
            {
                case PROB_ROSENBROCK:
                case PROB_RASTRIGIN:
                case PROB_SPHERE:
                case PROB_STEP:
                    return fitness.Equals(0.0);
                case PROB_NOISY_QUARTIC:
                case PROB_BOOTH:
                case PROB_GRIEWANK:
                case PROB_MEDIAN:
                case PROB_SUM:
                case PROB_PRODUCT:
                case PROB_SCHWEFEL:
                case PROB_ROTATED_RASTRIGIN:	// not sure
                case PROB_ROTATED_SCHWEFEL:
                case PROB_ROTATED_GRIEWANK:
                case PROB_MIN:
                case PROB_LANGERMAN:        // may be around -1.4
                case PROB_LENNARDJONES:
                case PROB_LUNACEK:
                default:
                    return false;
            }
        }

        public double Function(IEvolutionState state, int function, double[] genome, int threadnum)
        {
            CheckRange(state, function, genome);

            double value = 0;
            double len = genome.Length;
            switch (function)
            {
                case PROB_ROSENBROCK:
                    for (var i = 1; i < len; i++)
                    {
                        var gj = genome[i - 1];
                        var gi = genome[i];
                        value += (1 - gj) * (1 - gj) + 100 * (gi - gj * gj) * (gi - gj * gj);
                    }
                    return -value;


                case PROB_RASTRIGIN:
                    var A = 10.0;
                    value = len * A;
                    for (var i = 0; i < len; i++)
                    {
                        var gi = genome[i];
                        value += gi * gi - A * Math.Cos(2 * Math.PI * gi);
                    }
                    return -value;


                case PROB_SPHERE:
                    for (var i = 0; i < len; i++)
                    {
                        var gi = genome[i];
                        value += gi * gi;
                    }
                    return -value;


                case PROB_STEP:
                    for (var i = 0; i < len; i++)
                    {
                        var gi = genome[i];
                        value += 6 + Math.Floor(gi);
                    }
                    return -value;


                case PROB_NOISY_QUARTIC:
                    for (var i = 0; i < len; i++)
                    {
                        var gi = genome[i];
                        value += (i + 1) * (gi * gi * gi * gi) + state.Random[threadnum].NextGaussian(); // gauss(0,1)
                    }
                    return -value;


                case PROB_BOOTH:
                    if (len != 2)
                        state.Output.Fatal(
                            "The Booth problem is defined for only two terms, and as a consequence the genome of the DoubleVectorIndividual should have size 2.");
                    var g0 = genome[0];
                    var g1 = genome[1];
                    value = (g0 + 2 * g1 - 7) * (g0 + 2 * g1 - 7) +
                            (2 * g0 + g1 - 5) * (2 * g0 + g1 - 5);
                    return -value;


                case PROB_GRIEWANK:
                    value = 1;
                    double prod = 1;
                    for (var i = 0; i < len; i++)
                    {
                        var gi = genome[i];
                        value += (gi * gi) / 4000.0;
                        prod *= Math.Cos(gi / Math.Sqrt(i + 1));
                    }
                    value -= prod;
                    return -value;


                case PROB_SCHWEFEL:
                    for (var i = 0; i < len; i++)
                    {
                        var gi = genome[i];
                        value += -gi * Math.Sin(Math.Sqrt(Math.Abs(gi)));
                    }
                    return -value;


                case PROB_MEDIAN:
                    // FIXME, need to do a better median-finding algorithm, such as http://www.ics.uci.edu/~eppstein/161/960130.html
                    var sorted = new double[(int)len];
                    Array.Copy(genome, 0, sorted, 0, sorted.Length);
                    QuickSort.QSort(sorted);
                    return sorted[sorted.Length / 2]; // note positive

                case PROB_SUM:
                    value = 0.0;
                    for (var i = 0; i < len; i++)
                    {
                        var gi = genome[i];
                        value += gi;
                    }
                    return value; // note positive

                case PROB_MIN:
                    value = genome[0];
                    for (var i = 1; i < len; i++)
                    {
                        var gi = genome[i];
                        if (value > gi) value = gi;
                    }
                    return value; // note positive

                case PROB_PRODUCT:
                    value = 1.0;
                    for (var i = 0; i < len; i++)
                    {
                        var gi = genome[i];
                        value *= gi;
                    }
                    return value; // note positive

                case PROB_ROTATED_RASTRIGIN:
                {
                    lock (RotationMatrix.SyncRoot) // synchronizations are rare in ECJ.  :-(
                    {
                        if (RotationMatrix[0] == null)
                            RotationMatrix[0] = BuildRotationMatrix(state, seed, (int) len);
                    } // BRS: Why are we unlocking here?

                    // now we know the matrix exists rotate the matrix and return its value
                    double[] val = Mul(RotationMatrix[0], genome);
                    return Function(state, PROB_RASTRIGIN, val, threadnum);
                }

                case PROB_ROTATED_SCHWEFEL:
                {
                    lock (RotationMatrix.SyncRoot) // synchronizations are rare in ECJ.  :-(
                    {
                        if (RotationMatrix[0] == null)
                            RotationMatrix[0] = BuildRotationMatrix(state, seed, (int) len);
                    } // BRS: Why are we unlocking here?

                    // now we know the matrix exists rotate the matrix and return its value
                    double[] val = Mul(RotationMatrix[0], genome);
                    return Function(state, PROB_SCHWEFEL, val, threadnum);
                }

                case PROB_ROTATED_GRIEWANK:
                {
                    lock (RotationMatrix.SyncRoot) // synchronizations are rare in ECJ.  :-(
                    {
                        if (RotationMatrix[0] == null)
                            RotationMatrix[0] = BuildRotationMatrix(state, seed, (int) len);
                    } // BRS: Why are we unlocking here?

                    // now we know the matrix exists rotate the matrix and return its value
                    double[] val = Mul(RotationMatrix[0], genome);
                    return Function(state, PROB_GRIEWANK, val, threadnum);
                }

                case PROB_LANGERMAN:
                    {
                        return 0.0 - langerman(genome);
                    }

                case PROB_LENNARDJONES:
                    {
                        int numAtoms = genome.Length / 3;
                        double v = 0.0;

                        for (int i = 0; i < numAtoms - 1; i++)
                        {
                            for (int j = i + 1; j < numAtoms; j++)
                            {
                                // double d = dist(genome, i, j);
                                double a = genome[i * 3] - genome[j * 3];
                                double b = genome[i * 3 + 1] - genome[j * 3 + 1];
                                double c = genome[i * 3 + 2] - genome[j * 3 + 2];

                                double d = Math.Sqrt(a * a + b * b + c * c);

                                double r12 = Math.Pow(d, -12.0);
                                double r6 = Math.Pow(d, -6.0);
                                double e = r12 - r6;
                                v += e;
                            }
                        }
                        v *= -4.0;
                        return v;
                    }

                case PROB_LUNACEK:
                    {
                        // Lunacek function: for more information, please see --
                        // http://arxiv.org/pdf/1207.4318.pdf
                        // http://citeseerx.ist.psu.edu/viewdoc/summary?doi=10.1.1.154.1657
                        // // // //

                        double s = 1.0 - 1.0 / (2.0 * Math.Sqrt(genome.Length + 20.0) - 8.2);

                        // depth of the sphere, could be 1, 2, 3, or 4. 1 is deeper than 4
                        // this could be also be a fraction I guess.
                        double d = 1.0; // depth of the sphere, could be 1, 2, 3, or 4. 1 is deeper than 4
                                        // this could be also be a fraction I guess.
                        double mu1 = 2.5;
                        double mu2 = -1.0 * Math.Sqrt(Math.Abs((mu1 * mu1 - d) / s)); // probably don't need the abs
                        double sum1 = 0.0;
                        double sum2 = 0.0;
                        double sum3 = 0.0;
                        foreach (double genomei in genome)
                        {
                            sum1 = (genomei - mu1) * (genomei - mu1);
                            sum2 = (genomei - mu2) * (genomei - mu2);
                            sum3 += 1.0 - Math.Cos(2.0 * Math.PI * (genomei - mu1));
                        }
                        return Math.Min(sum1, d * genome.Length + s * sum2) + 10.0 * sum3;
                    }

                default:
                    state.Output.Fatal("ec.app.ecsuite.ECSuite has an invalid problem -- how on earth did that happen?");
                    return 0; // never happens
            }
        }

        #endregion // Operations
    }
}