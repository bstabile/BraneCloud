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

namespace BraneCloud.Evolution.EC.Problems.Spatial
{
    /// <summary>
    /// Several standard Evolutionary Computation functions are implemented: Rastrigin, De Jong's test suite
    /// F1-F4 problems (Sphere, Rosenbrock, Step, Noisy-Quartic), Booth (from [Schwefel, 1995]), and Griewangk.
    /// As the SimpleFitness is used for maximization problems, the mapping f(x) --> -f(x) is used to transform
    /// the problems into maximization ones.
    /// 
    /// <p/>Problems have been set up so that their traditional ranges are scaled so you can use a min-gene of -1.0
    /// and a max-gene of 1.0
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>type</tt><br/>
    /// <font size="-1">String, one of: rosenbrock rastrigin sphere step noisy-quartic kdj-f1 kdj-f2 kdj-f3 kdj-f4 booth median schwefel product [or] griewangk</font></td>
    /// <td valign="top">(The vector problem to test against.  Some of the types are synonyms: kdj-f1 = sphere, kdj-f2 = rosenbrock, kdj-f3 = step, kdj-f4 = noisy-quartic.  "kdj" stands for "Ken DeJong", and the numbers are the problems in his test suite)</td></tr>
    /// </table>
    /// </summary>
    [ECConfiguration("ec.problems.ecsuite.ECSuite")]
    public class ECSuiteSpacialProblem : Problem, ISimpleProblem
    {
        #region Constants

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

        // For the Rotation Facility
        /// <summary>
        /// Fixed rotation seed so all jobs use the same rotation space.
        /// </summary>
        public const long ROTATION_SEED = 9731297;

        #endregion // Constants
        #region Static

        public static readonly string[] ProblemName = new[]
                                                          {
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
                                                              V_ROTATED_SCHWEFEL
                                                          };

        public static readonly double[] MinRange = new[]
                                                       {
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

        public static readonly double[] MaxRange = new[]
                                                       {
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

        public static double[][][] RotationMatrix = new double[1][][];  // the actual matrix is stored in rotationMatrix[0] -- a hack

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
        public static double[ /* row */ ][ /* column */] BuildRotationMatrix(double rotationSeed, int N)
        {
            IMersenneTwister rand = new MersenneTwisterFast(ROTATION_SEED);
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

        bool _alreadyChecked = false;

        #endregion // Fields
        #region Properties

        /// <summary>
        /// Defaults on Rosenbrock
        /// </summary>
        public int ProblemType
        {
            get { return _problemType; }
            set { _problemType = value; }
        }
        private int _problemType = PROB_ROSENBROCK;

        #endregion // Properties
        #region Setup

        // nothing....
        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            var wp = state.Parameters.GetStringWithDefault(paramBase.Push(P_WHICH_PROBLEM), null, "");
            if (wp.CompareTo(V_ROSENBROCK) == 0 || wp.CompareTo(V_F2) == 0)
                _problemType = PROB_ROSENBROCK;
            else if (wp.CompareTo(V_RASTRIGIN) == 0)
                _problemType = PROB_RASTRIGIN;
            else if (wp.CompareTo(V_SPHERE) == 0 || wp.CompareTo(V_F1) == 0)
                _problemType = PROB_SPHERE;
            else if (wp.CompareTo(V_STEP) == 0 || wp.CompareTo(V_F3) == 0)
                _problemType = PROB_STEP;
            else if (wp.CompareTo(V_NOISY_QUARTIC) == 0 || wp.CompareTo(V_F4) == 0)
                _problemType = PROB_NOISY_QUARTIC;
            else if (wp.CompareTo(V_BOOTH) == 0)
                _problemType = PROB_BOOTH;
            else if (wp.CompareTo(V_GRIEWANK) == 0)
                _problemType = PROB_GRIEWANK;
            else if (wp.CompareTo(V_GRIEWANGK) == 0)
            {
                state.Output.Warning("Incorrect parameter name (\"griewangk\") used, should be \"griewank\"",
                                     paramBase.Push(P_WHICH_PROBLEM), null);
                _problemType = PROB_GRIEWANK;
            }
            else if (wp.CompareTo(V_MEDIAN) == 0)
                _problemType = PROB_MEDIAN;
            else if (wp.CompareTo(V_SUM) == 0)
                _problemType = PROB_SUM;
            else if (wp.CompareTo(V_PRODUCT) == 0)
                _problemType = PROB_PRODUCT;
            else if (wp.CompareTo(V_SCHWEFEL) == 0)
                _problemType = PROB_SCHWEFEL;
            else if (wp.CompareTo(V_MIN) == 0)
                _problemType = PROB_MIN;
            else if (wp.CompareTo(V_ROTATED_RASTRIGIN) == 0)
                _problemType = PROB_ROTATED_RASTRIGIN;
            else if (wp.CompareTo(V_ROTATED_SCHWEFEL) == 0)
                _problemType = PROB_ROTATED_SCHWEFEL;
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
                    "  " + V_ROTATED_SCHWEFEL + "\n",
                    paramBase.Push(P_WHICH_PROBLEM));
        }

        #endregion // Setup
        #region Operations

        public void CheckRange(IEvolutionState state, int problem)
        {
            if (_alreadyChecked || state.Generation > 0) return;
            _alreadyChecked = true;

            for (var i = 0; i < state.Population.Subpops.Length; i++)
            {
                if (!(state.Population.Subpops[i].Species is FloatVectorSpecies))
                {
                    state.Output.Fatal("ECSuite requires species " + i + " to be a FloatVectorSpecies, but it is a: " +
                                       state.Population.Subpops[i].Species);
                }
                var species = (FloatVectorSpecies)(state.Population.Subpops[i].Species);
                for (var k = 0; k < species.MinGenes.Length; k++)
                {
                    if (species.MinGenes[k] != MinRange[problem] ||
                        species.MaxGenes[k] != MaxRange[problem])
                    {
                        state.Output.Warning("Gene range is nonstandard for problem " + ProblemName[problem]
                                             + ".\nFirst occurrence: Subpopulation " + i + " Gene " + k
                                             + " range was [" + species.MinGenes[k] + ", " + species.MaxGenes[k]
                                             + "], expected [" + MinRange[problem] + ", " + MaxRange[problem] + "]");

                        return; // done here
                    }
                }
            }
        }

        public void Evaluate(IEvolutionState state,
            Individual ind,
            int subpop,
            int threadnum)
        {
            if (!(ind is DoubleVectorIndividual))
                state.Output.Fatal("The individuals for this problem should be DoubleVectorIndividuals.");

            var temp = (DoubleVectorIndividual)ind;
            var genome = temp.genome;
            var len = genome.Length;

            // this curious break-out makes it easy to use the isOptimal() and function() methods
            // for other purposes, such as coevolutionary versions of this class.

            // compute the fitness on a per-function basis
            var fit = (Function(state, _problemType, temp.genome, threadnum));

            // compute if we're optimal on a per-function basis
            var isOptimal = IsOptimal(_problemType, fit);

            // set the fitness appropriately
            if ((float)fit < (0.0f - float.MaxValue))  // uh oh -- can be caused by Product for example
            {
                ((SimpleFitness)(ind.Fitness)).SetFitness(state, 0.0f - float.MaxValue, isOptimal);
                state.Output.WarnOnce("'Product' type used: some fitnesses are negative infinity, setting to lowest legal negative number.");
            }
            else if ((float)fit > float.MaxValue)  // uh oh -- can be caused by Product for example
            {
                ((SimpleFitness)(ind.Fitness)).SetFitness(state, float.MaxValue, isOptimal);
                state.Output.WarnOnce("'Product' type used: some fitnesses are negative infinity, setting to lowest legal negative number.");
            }
            else
            {
                ((SimpleFitness)(ind.Fitness)).SetFitness(state, (float)fit, isOptimal);
            }
            ind.Evaluated = true;
        }

        public bool IsOptimal(int function, double fitness)
        {
            switch (_problemType)
            {
                case PROB_ROSENBROCK:
                case PROB_RASTRIGIN:
                case PROB_SPHERE:
                case PROB_STEP:
                    return fitness == 0.0f;
                case PROB_NOISY_QUARTIC:
                case PROB_BOOTH:
                case PROB_GRIEWANK:
                case PROB_MEDIAN:
                case PROB_SUM:
                case PROB_PRODUCT:
                case PROB_SCHWEFEL:
                case PROB_ROTATED_RASTRIGIN:	// not sure
                case PROB_ROTATED_SCHWEFEL:
                case PROB_MIN:
                default:
                    return false;
            }
        }

        public double Function(IEvolutionState state, int function, double[] genome, int threadnum)
        {
            CheckRange(state, function);

            double value = 0;
            double len = genome.Length;
            switch (function)
            {
                case PROB_ROSENBROCK:
                    for (var i = 1; i < len; i++)
                    {
                        var gj = genome[i - 1];
                        var gi = genome[i];
                        value += 100 * (gj * gj - gj) * (gj * gj - gj) + (1 - gj) * (1 - gj);
                    }
                    return -value;


                case PROB_RASTRIGIN:
                    var A = 10.0f;
                    value = len * A;
                    for (var i = 0; i < len; i++)
                    {
                        var gi = genome[i];
                        value += (gi * gi - A * Math.Cos(2 * Math.PI * gi));
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
                        value += (i + 1) * (gi * gi * gi * gi) + state.Random[threadnum].NextDouble();
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
                                RotationMatrix[0] = BuildRotationMatrix(ROTATION_SEED, (int)len);
                        }

                        // now we know the matrix exists rotate the matrix and return its value
                        var val = Mul(RotationMatrix[0], genome);
                        return Function(state, PROB_RASTRIGIN, val, threadnum);
                    }

                case PROB_ROTATED_SCHWEFEL:
                    {
                        lock (RotationMatrix.SyncRoot) // synchronizations are rare in ECJ.  :-(
                        {
                            if (RotationMatrix[0] == null)
                                RotationMatrix[0] = BuildRotationMatrix(ROTATION_SEED, (int)len);
                        }

                        // now we know the matrix exists rotate the matrix and return its value
                        var val = Mul(RotationMatrix[0], genome);
                        return Function(state, PROB_SCHWEFEL, val, threadnum);
                    }

                default:
                    state.Output.Fatal("ec.problems.ecsuite.ECSuite has an invalid problem -- how on earth did that happen?");
                    return 0; // never happens
            }
        }

        #endregion // Operations
    }
}