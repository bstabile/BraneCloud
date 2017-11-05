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

namespace BraneCloud.Evolution.EC.App.BBob
{
    /// <summary>
    /// The Black Box Optimization workshop (BBOB) has an annual competition for doing real-valued parameter optimization.
    /// The examples shown here are more or less faithful reproductions of the BBOB 2010 C code, only using Mersenne Twister
    /// instead of BBOB's random number generator.  Unfortunately, the original BBOB code has various magic numbers, unexplained
    /// variables, and unfortunate algorithmic decisions.  We've reproduced them exactly rather than attempt to convert to a 
    /// standard ECJ template, and simply apologize beforehand.
    /// 
    /// <p/>
    /// <b>Parameters</b><br/>
    /// <table>
    /// <tr>
    /// <td valign="top"><i>base</i>.<tt>type</tt><br/>
    /// <font size="-1"> String = <tt>none </tt>(default)
    /// <tt>, sphere, ellipsoidal, rastrigin, buch-rastrigin, linear-slope, attractive-sector, step-elipsoidal, 
    /// rosenbrock, rosenbrock-rotated, ellipsoidal-2, discus, bent-cigar, sharp-ridge, different-powers, rastrigin-2,
    /// weierstrass, schaffers-f7, schaffers-f7-2, griewak-rosenbrock, schwefel, gallagher-gaussian-101me, gallagher-gaussian-21hi, katsuura, lunacek</tt>
    /// </font></td>
    /// <td valign="top">(The particular function)</td></tr>
    /// <tr>
    /// <td valign="top"><i>base</i>.<tt>noise</tt><br/>
    /// <font size="-1"> String = <tt>none </tt>(default)
    /// <tt>, gauss, uniform, cauchy, gauss-moderate, uniform-moderate, cauchy-moderate</tt>
    /// </font></td>
    /// <td valign="top">(what type of noise (if any) to add to the function value)</td></tr>
    /// <tr>
    /// <td valign="top"><i>base</i>.<tt>reevaluate-noisy-problems</tt><br/>
    /// <font size="-1"> boolean = <tt>true</tt>(default)
    /// </font></td>
    /// <td valign="top">(whether to reevaluate noisy problems)</td></tr>    
    /// </table>
    /// </summary>
    [ECConfiguration("ec.app.bbob.BBOBenchmarks")]
    public class BBOBenchmarks : Problem, ISimpleProblem
    {
        public const string P_GENOME_SIZE = "genome-size";
        public const string P_WHICH_PROBLEM = "type";
        public const string P_NOISE = "noise";
        public const string P_REEVALUATE_NOISY_PROBLEMS = "reevaluate-noisy-problems";

        public string[] problemTypes =
        { "sphere", "ellipsoidal", "rastrigin", "buche-rastrigin", "linear-slope", "attractive-sector", "step-ellipsoidal", "rosenbrock", "rosenbrock-rotated", "ellipsoidal-2", "discus", "bent-cigar", "sharp-ridge", "different-powers", "rastrigin-2",
          "weierstrass", "schaffers-f7", "schaffers-f7-2", "griewank-rosenbrock", "schwefel", "gallagher-gaussian-101me", "gallagher-gaussian-21hi", "katsuura", "lunacek" };

        public const int SPHERE = 0;
        public const int ELLIPSOIDAL = 1;
        public const int RASTRIGIN = 2;
        public const int BUCHE_RASTRIGIN = 3;
        public const int LINEAR_SLOPE = 4;
        public const int ATTRACTIVE_SECTOR = 5;
        public const int STEP_ELLIPSOIDAL = 6;
        public const int ROSENBROCK = 7;
        public const int ROSENBROCK_ROTATED = 8;
        public const int ELLIPSOIDAL_2 = 9;
        public const int DISCUS = 10;
        public const int BENT_CIGAR = 11;
        public const int SHARP_RIDGE = 12;
        public const int DIFFERENT_POWERS = 13;
        public const int RASTRIGIN_2 = 14;
        public const int WEIERSTRASS = 15;
        public const int SCHAFFERS_F7 = 16;
        public const int SCHAFFERS_F7_2 = 17;
        public const int GRIEWANK_ROSENBROCK = 18;
        public const int SCHWEFEL = 19;
        public const int GALLAGHER_GAUSSIAN_101ME = 20;
        public const int GALLAGHER_GAUSSIAN_21HI = 21;
        public const int KATSUURA = 22;
        public const int LUNACEK = 23;

        // Noise types
        public String[] noiseTypes = { "none", "gauss", "uniform", "cauchy", "gauss-moderate", "uniform-moderate", "cauchy-moderate" };

        public const int NONE = 0;
        public const int GAUSSIAN = 1;
        public const int UNIFORM = 2;
        public const int CAUCHY = 3;
        public const int GAUSSIAN_MODERATE = 4;
        public const int UNIFORM_MODERATE = 5;
        public const int CAUCHY_MODERATE = 6;

        public int problemType = 0; // defaults on SPHERE

        public int noise = NONE; // defaults to NONE

        public bool reevaluateNoisyProblems;

        public int NHIGHPEAKS21 = 101;
        public int NHIGHPEAKS22 = 21;

        // DO NOT MODIFY THESE VARIABLES except in the Setup method: global
        // variables are not threadsafe.
        double fOpt;
        double[] xOpt;
        double fAdd_Init;

        double f0;
        double[][] rotation;
        double[][] rot2;
        double[][] linearTF;
        double[] peaks21;
        double[] peaks22;
        int[] rperm;
        int[] rperm21;
        int[] rperm22;
        double[][] xLocal;
        double[][] xLocal21;
        double[][] xLocal22;
        double[][] arrScales;
        double[][] arrScales21;
        double[][] arrScales22;
        double[] aK;
        double[] bK;
        double[] peakvalues;
        double scales;


        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            var wp = state.Parameters.GetStringWithDefault(paramBase.Push(P_WHICH_PROBLEM), null, "");
            int i, j, k;
            IParameter p = new Parameter(Initializer.P_POP);
            var genomeSize = state.Parameters.GetInt(p.Push(Population.P_SUBPOP).Push("0").Push(Subpopulation.P_SPECIES).Push(P_GENOME_SIZE), null, 1);
            var noiseStr = state.Parameters.GetString(paramBase.Push(P_NOISE), null);

            for (i = 0; i < noiseTypes.Length; i++)
                if (noiseStr == noiseTypes[i])
                    noise = i;

            reevaluateNoisyProblems = state.Parameters.GetBoolean(paramBase.Push(P_REEVALUATE_NOISY_PROBLEMS), null, true);

            var condition = 10.0;
            var alpha = 100.0;
            double tmp, maxCondition;
            double[] fitValues = { 1.1, 9.1 };

            double[] arrCondition, peaks, tmpvect;

            for (i = 0; i < problemTypes.Length; i++)
                if (wp.Equals(problemTypes[i]))
                    problemType = i;

            // common Initialization
            fOpt = computeFopt(state.Random[0]);
            xOpt = new double[genomeSize];

            switch (problemType)
            {
                case SPHERE:
                    /* INITIALIZATION */
                    computeXopt(xOpt, state.Random[0]);
                    break;

                case ELLIPSOIDAL: // f2
                    computeXopt(xOpt, state.Random[0]);
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    computeRotation(rotation, state.Random[0], genomeSize);
                    if (noise != NONE)
                    {
                        rot2 = TensorFactory.Create<double>(genomeSize, genomeSize);
                        computeRotation(rot2, state.Random[0], genomeSize);
                    }
                    break;

                case RASTRIGIN:
                    computeXopt(xOpt, state.Random[0]);
                    break;

                case BUCHE_RASTRIGIN:
                    computeXopt(xOpt, state.Random[0]);
                    for (i = 0; i < genomeSize; i += 2)
                        xOpt[i] = Math.Abs(xOpt[i]); /* Skew */
                    break;

                case LINEAR_SLOPE:
                    computeXopt(xOpt, state.Random[0]);
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp = Math.Pow(Math.Sqrt(alpha), ((double)i) / ((double)(genomeSize - 1)));
                        if (xOpt[i] > 0)
                        {
                            xOpt[i] = 5.0;
                        }
                        else if (xOpt[i] < 0)
                        {
                            xOpt[i] = -5.0;
                        }
                        fAdd_Init += 5.0 * tmp;
                    }
                    break;

                case ATTRACTIVE_SECTOR:
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    rot2 = TensorFactory.Create<double>(genomeSize, genomeSize);
                    linearTF = TensorFactory.Create<double>(genomeSize, genomeSize);
                    computeXopt(xOpt, state.Random[0]);
                    computeRotation(rotation, state.Random[0], genomeSize);
                    computeRotation(rot2, state.Random[0], genomeSize);
                    /* decouple scaling from function definition */
                    for (i = 0; i < genomeSize; i++)
                    {
                        for (j = 0; j < genomeSize; j++)
                        {
                            linearTF[i][j] = 0.0;
                            for (k = 0; k < genomeSize; k++)
                            {
                                linearTF[i][j] += rotation[i][k] * Math.Pow(Math.Sqrt(condition), ((double)k) / ((double)(genomeSize - 1))) * rot2[k][j];
                            }
                        }
                    }
                    break;

                case STEP_ELLIPSOIDAL:
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    rot2 = TensorFactory.Create<double>(genomeSize, genomeSize);
                    computeXopt(xOpt, state.Random[0]);
                    computeRotation(rotation, state.Random[0], genomeSize);
                    computeRotation(rot2, state.Random[0], genomeSize);
                    break;

                case ROSENBROCK:
                    computeXopt(xOpt, state.Random[0]);
                    scales = Math.Max(1.0, Math.Sqrt(genomeSize) / 8.0);
                    if (noise == NONE)
                        for (i = 0; i < genomeSize; i++)
                            xOpt[i] *= 0.75;
                    break;

                case ROSENBROCK_ROTATED:
                    /* INITIALIZATION */
                    linearTF = TensorFactory.Create<double>(genomeSize, genomeSize);
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    /* computeXopt(state.random[0], genomeSize); */
                    computeRotation(rotation, state.Random[0], genomeSize);
                    scales = Math.Max(1.0, Math.Sqrt(genomeSize) / 8.0);
                    for (i = 0; i < genomeSize; i++)
                    {
                        for (j = 0; j < genomeSize; j++)
                            linearTF[i][j] = scales * rotation[i][j];
                    }
                    break;

                case ELLIPSOIDAL_2:
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    computeXopt(xOpt, state.Random[0]);
                    computeRotation(rotation, state.Random[0], genomeSize);
                    break;

                case DISCUS:
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    computeXopt(xOpt, state.Random[0]);
                    computeRotation(rotation, state.Random[0], genomeSize);
                    break;

                case BENT_CIGAR:
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    computeXopt(xOpt, state.Random[0]);
                    computeRotation(rotation, state.Random[0], genomeSize);
                    break;

                case SHARP_RIDGE:
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    rot2 = TensorFactory.Create<double>(genomeSize, genomeSize);
                    linearTF = TensorFactory.Create<double>(genomeSize, genomeSize);
                    computeXopt(xOpt, state.Random[0]);
                    computeRotation(rotation, state.Random[0], genomeSize);
                    computeRotation(rot2, state.Random[0], genomeSize);
                    for (i = 0; i < genomeSize; i++)
                    {
                        for (j = 0; j < genomeSize; j++)
                        {
                            linearTF[i][j] = 0.0;
                            for (k = 0; k < genomeSize; k++)
                            {
                                linearTF[i][j] += rotation[i][k] * Math.Pow(Math.Sqrt(condition), ((double)k) / ((double)(genomeSize - 1))) * rot2[k][j];
                            }
                        }
                    }
                    break;

                case DIFFERENT_POWERS:
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    computeXopt(xOpt, state.Random[0]);
                    computeRotation(rotation, state.Random[0], genomeSize);
                    break;

                case RASTRIGIN_2:
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    rot2 = TensorFactory.Create<double>(genomeSize, genomeSize);
                    linearTF = TensorFactory.Create<double>(genomeSize, genomeSize);
                    computeXopt(xOpt, state.Random[0]);
                    computeRotation(rotation, state.Random[0], genomeSize);
                    computeRotation(rot2, state.Random[0], genomeSize);
                    for (i = 0; i < genomeSize; i++)
                    {
                        for (j = 0; j < genomeSize; j++)
                        {
                            linearTF[i][j] = 0.0;
                            for (k = 0; k < genomeSize; k++)
                            {
                                linearTF[i][j] += rotation[i][k] * Math.Pow(Math.Sqrt(condition), ((double)k) / ((double)(genomeSize - 1))) * rot2[k][j];
                            }
                        }
                    }
                    break;

                case WEIERSTRASS:
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    rot2 = TensorFactory.Create<double>(genomeSize, genomeSize);
                    linearTF = TensorFactory.Create<double>(genomeSize, genomeSize);
                    aK = new double[12];
                    bK = new double[12];
                    computeXopt(xOpt, state.Random[0]);
                    computeRotation(rotation, state.Random[0], genomeSize);
                    computeRotation(rot2, state.Random[0], genomeSize);

                    for (i = 0; i < genomeSize; i++)
                    {
                        for (j = 0; j < genomeSize; j++)
                        {
                            linearTF[i][j] = 0.0;
                            for (k = 0; k < genomeSize; k++)
                            {
                                linearTF[i][j] += rotation[i][k] * Math.Pow(1.0 / Math.Sqrt(condition), ((double)k) / ((double)(genomeSize - 1))) * rot2[k][j];
                            }
                        }
                    }

                    f0 = 0.0;
                    for (i = 0; i < 12; i++) /*
                                          * number of summands, 20 in CEC2005, 10/12
                                          * saves 30% of time
                                          */
                    {
                        aK[i] = Math.Pow(0.5, (double)i);
                        bK[i] = Math.Pow(3.0, (double)i);
                        f0 += aK[i] * Math.Cos(2 * Math.PI * bK[i] * 0.5);
                    }
                    break;

                case SCHAFFERS_F7:
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    rot2 = TensorFactory.Create<double>(genomeSize, genomeSize);
                    computeXopt(xOpt, state.Random[0]);
                    computeRotation(rotation, state.Random[0], genomeSize);
                    computeRotation(rot2, state.Random[0], genomeSize);
                    break;

                case SCHAFFERS_F7_2:
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    rot2 = TensorFactory.Create<double>(genomeSize, genomeSize);
                    linearTF = TensorFactory.Create<double>(genomeSize, genomeSize);
                    computeXopt(xOpt, state.Random[0]);
                    computeRotation(rotation, state.Random[0], genomeSize);
                    computeRotation(rot2, state.Random[0], genomeSize);
                    break;

                case GRIEWANK_ROSENBROCK:
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    scales = Math.Max(1.0, Math.Sqrt(genomeSize) / 8.0);
                    computeRotation(rotation, state.Random[0], genomeSize);
                    if (noise == NONE)
                    {
                        rot2 = TensorFactory.Create<double>(genomeSize, genomeSize);
                        linearTF = TensorFactory.Create<double>(genomeSize, genomeSize);
                        for (i = 0; i < genomeSize; i++)
                        {
                            for (j = 0; j < genomeSize; j++)
                            {
                                linearTF[i][j] = scales * rotation[i][j];
                            }
                        }
                        for (i = 0; i < genomeSize; i++)
                        {
                            xOpt[i] = 0.0;
                            for (j = 0; j < genomeSize; j++)
                            {
                                xOpt[i] += linearTF[j][i] * 0.5 / scales / scales;
                            }
                        }
                    }
                    else
                    {
                        // TODO
                    }
                    break;

                case SCHWEFEL:
                    /* INITIALIZATION */
                    tmpvect = new double[genomeSize];

                    for (i = 0; i < genomeSize; i++)
                        tmpvect[i] = nextDoubleClosedInterval(state.Random[0]);
                    for (i = 0; i < genomeSize; i++)
                    {
                        xOpt[i] = 0.5 * 4.2096874633;
                        if (tmpvect[i] - 0.5 < 0)
                            xOpt[i] *= -1.0;
                    }
                    break;

                case GALLAGHER_GAUSSIAN_101ME:
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    maxCondition = 1000.0;
                    arrCondition = new double[NHIGHPEAKS21];
                    peaks21 = new double[genomeSize * NHIGHPEAKS21];
                    rperm21 = new int[Math.Max(genomeSize, NHIGHPEAKS21)];
                    peaks = peaks21;
                    peakvalues = new double[NHIGHPEAKS21];
                    arrScales21 = TensorFactory.Create<double>(NHIGHPEAKS21, genomeSize);
                    xLocal21 = TensorFactory.Create<double>(genomeSize, NHIGHPEAKS21);
                    computeRotation(rotation, state.Random[0], genomeSize);

                    for (i = 0; i < NHIGHPEAKS21 - 1; i++)
                        peaks[i] = nextDoubleClosedInterval(state.Random[0]);
                    rperm = rperm21;
                    for (i = 0; i < NHIGHPEAKS21 - 1; i++)
                        rperm[i] = i;
                    QuickSort.QSort(rperm);

                    /* Random permutation */

                    arrCondition[0] = Math.Sqrt(maxCondition);
                    peakvalues[0] = 10;
                    for (i = 1; i < NHIGHPEAKS21; i++)
                    {
                        arrCondition[i] = Math.Pow(maxCondition, (double)(rperm[i - 1]) / ((double)(NHIGHPEAKS21 - 2)));
                        peakvalues[i] = (double)(i - 1) / (double)(NHIGHPEAKS21 - 2) * (fitValues[1] - fitValues[0]) + fitValues[0];
                    }
                    arrScales = arrScales21;
                    for (i = 0; i < NHIGHPEAKS21; i++)
                    {
                        for (j = 0; j < genomeSize; j++)
                            peaks[j] = nextDoubleClosedInterval(state.Random[0]);
                        for (j = 0; j < genomeSize; j++)
                            rperm[j] = j;
                        // qsort(rperm, genomeSize, sizeof(int), compare_doubles);
                        QuickSort.QSort(rperm);
                        for (j = 0; j < genomeSize; j++)
                        {
                            arrScales[i][j] = Math.Pow(arrCondition[i], ((double)rperm[j]) / ((double)(genomeSize - 1)) - 0.5);
                        }
                    }

                    for (i = 0; i < genomeSize * NHIGHPEAKS21; i++)
                        peaks[i] = nextDoubleClosedInterval(state.Random[0]);
                    xLocal = xLocal21;
                    for (i = 0; i < genomeSize; i++)
                    {
                        xOpt[i] = 0.8 * (10.0 * peaks[i] - 5.0);
                        for (j = 0; j < NHIGHPEAKS21; j++)
                        {
                            xLocal[i][j] = 0.0;
                            for (k = 0; k < genomeSize; k++)
                            {
                                xLocal[i][j] += rotation[i][k] * (10.0 * peaks[j * genomeSize + k] - 5.0);
                            }
                            if (j == 0)
                                xLocal[i][j] *= 0.8;
                        }
                    }
                    break;

                case GALLAGHER_GAUSSIAN_21HI:
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    maxCondition = 1000.0;
                    arrCondition = new double[NHIGHPEAKS22];
                    peaks22 = new double[genomeSize * NHIGHPEAKS22];
                    rperm22 = new int[Math.Max(genomeSize, NHIGHPEAKS22)];
                    arrScales22 = TensorFactory.Create<double>(NHIGHPEAKS22, genomeSize);
                    xLocal22 = TensorFactory.Create<double>(genomeSize, NHIGHPEAKS22);
                    peaks = peaks22;
                    peakvalues = new double[NHIGHPEAKS22];
                    computeRotation(rotation, state.Random[0], genomeSize);
                    peaks = peaks22;
                    for (i = 0; i < NHIGHPEAKS22 - 1; i++)
                        peaks[i] = nextDoubleClosedInterval(state.Random[0]);
                    rperm = rperm22;
                    for (i = 0; i < NHIGHPEAKS22 - 1; i++)
                        rperm[i] = i;
                    // NOTE: confirm if this is a valid java conversion.
                    QuickSort.QSort(rperm);
                    /* Random permutation */
                    arrCondition[0] = maxCondition;
                    peakvalues[0] = 10;
                    for (i = 1; i < NHIGHPEAKS22; i++)
                    {
                        arrCondition[i] = Math.Pow(maxCondition, (double)(rperm[i - 1]) / ((double)(NHIGHPEAKS22 - 2)));
                        peakvalues[i] = (double)(i - 1) / (double)(NHIGHPEAKS22 - 2) * (fitValues[1] - fitValues[0]) + fitValues[0];
                    }
                    arrScales = arrScales22;
                    for (i = 0; i < NHIGHPEAKS22; i++)
                    {
                        for (j = 0; j < genomeSize; j++)
                            peaks[j] = nextDoubleClosedInterval(state.Random[0]);
                        for (j = 0; j < genomeSize; j++)
                            rperm[j] = j;
                        // qsort(rperm, genomeSize, sizeof(int), compare_doubles);
                        // NOTE: confirm if converted correctly
                        QuickSort.QSort(rperm);
                        for (j = 0; j < genomeSize; j++)
                        {
                            arrScales[i][j] = Math.Pow(arrCondition[i], ((double)rperm[j]) / ((double)(genomeSize - 1)) - 0.5);
                        }
                    }

                    for (i = 0; i < genomeSize * NHIGHPEAKS22; i++)
                        peaks[i] = nextDoubleClosedInterval(state.Random[0]);
                    xLocal = xLocal22;
                    for (i = 0; i < genomeSize; i++)
                    {
                        xOpt[i] = 0.8 * (9.8 * peaks[i] - 4.9);
                        for (j = 0; j < NHIGHPEAKS22; j++)
                        {
                            xLocal[i][j] = 0.0;
                            for (k = 0; k < genomeSize; k++)
                            {
                                xLocal[i][j] += rotation[i][k] * (9.8 * peaks[j * genomeSize + k] - 4.9);
                            }
                            if (j == 0)
                                xLocal[i][j] *= 0.8;
                        }
                    }
                    break;

                case KATSUURA:
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    rot2 = TensorFactory.Create<double>(genomeSize, genomeSize);
                    linearTF = TensorFactory.Create<double>(genomeSize, genomeSize);
                    computeXopt(xOpt, state.Random[0]);
                    computeRotation(rotation, state.Random[0], genomeSize);
                    computeRotation(rot2, state.Random[0], genomeSize);
                    for (i = 0; i < genomeSize; i++)
                    {
                        for (j = 0; j < genomeSize; j++)
                        {
                            linearTF[i][j] = 0.0;
                            for (k = 0; k < genomeSize; k++)
                            {
                                linearTF[i][j] += rotation[i][k] * Math.Pow(Math.Sqrt(condition), ((double)k) / (double)(genomeSize - 1)) * rot2[k][j];
                            }
                        }
                    }
                    break;

                case LUNACEK:
                    rotation = TensorFactory.Create<double>(genomeSize, genomeSize);
                    rot2 = TensorFactory.Create<double>(genomeSize, genomeSize);
                    tmpvect = new double[genomeSize];
                    linearTF = TensorFactory.Create<double>(genomeSize, genomeSize);
                    var mu1 = 2.5;
                    computeXopt(xOpt, state.Random[0]);
                    computeRotation(rotation, state.Random[0], genomeSize);
                    computeRotation(rot2, state.Random[0], genomeSize);
                    gauss(tmpvect, state.Random[0]);
                    for (i = 0; i < genomeSize; i++)
                    {
                        xOpt[i] = 0.5 * mu1;
                        if (tmpvect[i] < 0.0)
                            xOpt[i] *= -1.0;
                    }

                    for (i = 0; i < genomeSize; i++)
                    {
                        for (j = 0; j < genomeSize; j++)
                        {
                            linearTF[i][j] = 0.0;
                            for (k = 0; k < genomeSize; k++)
                            {
                                linearTF[i][j] += rotation[i][k] * Math.Pow(Math.Sqrt(condition), ((double)k) / ((double)(genomeSize - 1))) * rot2[k][j];
                            }
                        }
                    }
                    break;

                default:
                    var outputStr = "Invalid value for parameter, or parameter not found.\n" + "Acceptable values are:\n";
                    for (i = 0; i < problemTypes.Length; i++)
                        outputStr += problemTypes[i] + "\n";
                    state.Output.Fatal(outputStr, paramBase.Push(P_WHICH_PROBLEM));
                    break;
            }

        }

        public void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            if (noise != NONE)
            {
                if (!reevaluateNoisyProblems && ind.Evaluated) // don't bother reevaluating
                    return;
            }
            else if (ind.Evaluated)  // don't bother reevaluating
                return;
            
            if (!(ind is DoubleVectorIndividual))
                state.Output.Fatal("The individuals for this problem should be DoubleVectorIndividuals.");
            var temp = (DoubleVectorIndividual)ind;
            var genome = temp.genome;
            var genomeSize = genome.Length;
            double value = 0;
            double fit;
            int i, j;
            double condition, alpha, beta, tmp = 0.0, tmp2, fAdd, fPen = 0.0, x1, fac, a, f = 0.0, f2;
            var tmx = new double[genomeSize];
            var tmpvect = new double[genomeSize];
            switch (problemType)
            {
                case SPHERE:// f1
                    /* Sphere function */
                    fAdd = fOpt;
                    if (noise != NONE)
                    {
                        for (i = 0; i < genomeSize; i++)
                        {
                            tmp = Math.Abs(genome[i]) - 5.0;
                            if (tmp > 0.0)
                            {
                                fPen += tmp * tmp;
                            }
                        }
                        fAdd += 100.0 * fPen;
                    }
                    /* COMPUTATION core */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp = genome[i] - xOpt[i];
                        value += tmp * tmp;
                    }
                    switch (noise)
                    {
                        case NONE:
                            break;
                        case GAUSSIAN:
                            value = fGauss(value, 1.0, state.Random[threadnum]);
                            break;
                        case UNIFORM:
                            value = fUniform(value, 0.49 + 1.0 / genomeSize, 1.0, state.Random[threadnum]);
                            break;
                        case CAUCHY:
                            value = fCauchy(value, 1.0, 0.2, state.Random[threadnum]);
                            break;
                        case GAUSSIAN_MODERATE:
                            value = fGauss(value, 0.01, state.Random[threadnum]);
                            break;
                        case UNIFORM_MODERATE:
                            value = fUniform(value, 0.01 * (0.49 + 1.0 / genomeSize), 0.01, state.Random[threadnum]);
                            break;
                        case CAUCHY_MODERATE:
                            value = fCauchy(value, 0.01, 0.05, state.Random[threadnum]);
                            break;
                        default:
                            String outputStr = "Invalid value for parameter, or parameter not found.\n" + "Acceptable values are:\n";
                            for (i = 0; i < noiseTypes.Length; i++)
                                outputStr += noiseTypes[i] + "\n";
                            state.Output.Fatal(outputStr, new Parameter(P_NOISE));
                            break;
                    }
                    value += fAdd;
                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit == 0.0);
                    break;



                case ELLIPSOIDAL:// f2
                    /*
                     * separable ellipsoid with monotone transformation with noiseless
                     * condition 1e6 and noisy condition 1e4
                     */
                    fAdd = fOpt;
                    if (noise == NONE)
                    {
                        condition = 1e6;
                        for (i = 0; i < genomeSize; i++)
                        {
                            tmx[i] = genome[i] - xOpt[i];
                        }
                    }
                    else
                    {
                        condition = 1e4;
                        fAdd = fOpt;

                        /* BOUNDARY HANDLING */
                        for (i = 0; i < genomeSize; i++)
                        {
                            tmp = Math.Abs(genome[i]) - 5.0;
                            if (tmp > 0.0)
                            {
                                fPen += tmp * tmp;
                            }
                        }
                        fAdd += 100.0 * fPen;

                        /* TRANSFORMATION IN SEARCH SPACE */
                        for (i = 0; i < genomeSize; i++)
                        {
                            tmx[i] = 0.0;
                            for (j = 0; j < genomeSize; j++)
                            {
                                tmx[i] += rotation[i][j] * (genome[j] - xOpt[j]);
                            }
                        }
                    }

                    monotoneTFosc(tmx);
                    /* COMPUTATION core */
                    for (i = 0; i < genomeSize; i++)
                    {
                        value += Math.Pow(condition, ((double)i) / ((double)(genomeSize - 1))) * tmx[i] * tmx[i];
                    }

                    switch (noise)
                    {
                        case NONE:
                            break;
                        case GAUSSIAN:
                            value = fGauss(value, 1.0, state.Random[threadnum]);
                            break;
                        case UNIFORM:
                            value = fUniform(value, 0.49 + 1.0 / genomeSize, 1.0, state.Random[threadnum]);
                            break;
                        case CAUCHY:
                            value = fCauchy(value, 1.0, 0.2, state.Random[threadnum]);
                            break;
                        default:
                            String outputStr = "Invalid value for parameter, or parameter not found.\n" + "Acceptable values are:\n";
                            for (i = 0; i < 4; i++)
                                outputStr += noiseTypes[i] + "\n";
                            state.Output.Fatal(outputStr, new Parameter(P_NOISE));
                            break;
                    }
                    value += fAdd;
                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case RASTRIGIN:// f3
                    /* Rastrigin with monotone transformation separable "condition" 10 */
                    condition = 10;
                    beta = 0.2;
                    fAdd = fOpt;
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] = genome[i] - xOpt[i];
                    }
                    monotoneTFosc(tmx);
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp = ((double)i) / ((double)(genomeSize - 1));
                        if (tmx[i] > 0)
                            tmx[i] = Math.Pow(tmx[i], 1 + beta * tmp * Math.Sqrt(tmx[i]));
                        tmx[i] = Math.Pow(Math.Sqrt(condition), tmp) * tmx[i];
                    }
                    /* COMPUTATION core */
                    tmp = 0;
                    tmp2 = 0;
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp += Math.Cos(2 * Math.PI * tmx[i]);
                        tmp2 += tmx[i] * tmx[i];
                    }
                    value = 10 * (genomeSize - tmp) + tmp2;
                    value += fAdd;

                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case BUCHE_RASTRIGIN:// f4
                    /* skew Rastrigin-Bueche, condition 10, skew-"condition" 100 */
                    condition = 10.0;
                    alpha = 100;
                    fAdd = fOpt;
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp = Math.Abs(genome[i]) - 5.0;
                        if (tmp > 0.0)
                            fPen += tmp * tmp;
                    }
                    fPen *= 1e2;
                    fAdd += fPen;

                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] = genome[i] - xOpt[i];
                    }

                    monotoneTFosc(tmx);
                    for (i = 0; i < genomeSize; i++)
                    {
                        if (i % 2 == 0 && tmx[i] > 0)
                            tmx[i] = Math.Sqrt(alpha) * tmx[i];
                        tmx[i] = Math.Pow(Math.Sqrt(condition), ((double)i) / ((double)(genomeSize - 1))) * tmx[i];
                    }
                    /* COMPUTATION core */
                    tmp = 0.0;
                    tmp2 = 0.0;
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp += Math.Cos(2 * Math.PI * tmx[i]);
                        tmp2 += tmx[i] * tmx[i];
                    }
                    value = 10 * (genomeSize - tmp) + tmp2;
                    value += fAdd;

                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case LINEAR_SLOPE:// f5
                    /* linear slope */
                    alpha = 100;
                    fAdd = fOpt;
                    /* BOUNDARY HANDLING */
                    /* move "too" good coordinates back into domain */
                    for (i = 0; i < genomeSize; i++)
                    {
                        if (xOpt[i].Equals(5.0) && genome[i] > 5)
                            tmx[i] = 5.0;
                        else if (xOpt[i].Equals(-5.0) && genome[i] < -5)
                            tmx[i] = -5.0;
                        else
                            tmx[i] = genome[i];
                    }

                    /* COMPUTATION core */
                    for (i = 0; i < genomeSize; i++)
                    {
                        if (xOpt[i] > 0)
                        {
                            value -= Math.Pow(Math.Sqrt(alpha), ((double)i) / ((double)(genomeSize - 1))) * tmx[i];
                        }
                        else
                        {
                            value += Math.Pow(Math.Sqrt(alpha), ((double)i) / ((double)(genomeSize - 1))) * tmx[i];
                        }
                    }
                    value += fAdd;

                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case ATTRACTIVE_SECTOR:// f6
                    /* attractive sector function */
                    alpha = 100.0;
                    fAdd = fOpt;

                    /* BOUNDARY HANDLING */
                    /* TRANSFORMATION IN SEARCH SPACE */
                    for (i = 0; i < genomeSize; i++)
                    {

                        tmx[i] = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmx[i] += linearTF[i][j] * (genome[j] - xOpt[j]);
                        }
                    }

                    /* COMPUTATION core */
                    for (i = 0; i < genomeSize; i++)
                    {
                        if (tmx[i] * xOpt[i] > 0)
                            tmx[i] *= alpha;
                        value += tmx[i] * tmx[i];
                    }

                    /* monotoneTFosc... */
                    if (value > 0)
                    {
                        value = Math.Pow(Math.Exp(Math.Log(value) / 0.1 + 0.49 * (Math.Sin(Math.Log(value) / 0.1) + Math.Sin(0.79 * Math.Log(value) / 0.1))), 0.1);
                    }
                    else if (value < 0)
                    {
                        value = -Math.Pow(Math.Exp(Math.Log(-value) / 0.1 + 0.49 * (Math.Sin(0.55 * Math.Log(-value) / 0.1) + Math.Sin(0.31 * Math.Log(-value) / 0.1))), 0.1);
                    }
                    value = Math.Pow(value, 0.9);
                    value += fAdd;
                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case STEP_ELLIPSOIDAL:// f7
                    /* step-ellipsoid, condition 100 */
                    condition = 100.0;
                    alpha = 10.0;
                    fAdd = fOpt;
                    /* BOUNDARY HANDLING */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp = Math.Abs(genome[i]) - 5.0;
                        if (tmp > 0.0)
                        {
                            fPen += tmp * tmp;
                        }
                    }
                    if (noise == NONE)
                        fAdd += fPen;
                    else
                        fAdd += 100.0 * fPen;

                    /* TRANSFORMATION IN SEARCH SPACE */
                    for (i = 0; i < genomeSize; i++)
                    {

                        tmpvect[i] = 0.0;
                        tmp = Math.Sqrt(Math.Pow(condition / 10.0, ((double)i) / ((double)(genomeSize - 1))));
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmpvect[i] += tmp * rot2[i][j] * (genome[j] - xOpt[j]);
                        }

                    }
                    x1 = tmpvect[0];

                    for (i = 0; i < genomeSize; i++)
                    {
                        if (Math.Abs(tmpvect[i]) > 0.5)
                            tmpvect[i] = Math.Round(tmpvect[i]);
                        else
                            tmpvect[i] = Math.Round(alpha * tmpvect[i]) / alpha;
                    }

                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmx[i] += rotation[i][j] * tmpvect[j];
                        }
                    }

                    /* COMPUTATION core */
                    for (i = 0; i < genomeSize; i++)
                    {
                        value += Math.Pow(condition, ((double)i) / ((double)(genomeSize - 1))) * tmx[i] * tmx[i];
                    }
                    value = 0.1 * Math.Max(1e-4 * Math.Abs(x1), value);
                    switch (noise)
                    {
                        case NONE:
                            break;
                        case GAUSSIAN:
                            value = fGauss(value, 1.0, state.Random[threadnum]);
                            break;
                        case UNIFORM:
                            value = fUniform(value, 0.49 + 1.0 / genomeSize, 1.0, state.Random[threadnum]);
                            break;
                        case CAUCHY:
                            value = fCauchy(value, 1.0, 0.2, state.Random[threadnum]);
                            break;
                        default:
                            String outputStr = "Invalid value for parameter, or parameter not found.\n" + "Acceptable values are:\n";
                            for (i = 0; i < 4; i++)
                                outputStr += noiseTypes[i] + "\n";
                            state.Output.Fatal(outputStr, new Parameter(P_NOISE));
                            break;
                    }
                    value += fAdd;
                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case ROSENBROCK:// f8
                    /* Rosenbrock, non-rotated */
                    fAdd = fOpt;
                    if (noise == NONE)
                    {
                        /* TRANSFORMATION IN SEARCH SPACE */
                        for (i = 0; i < genomeSize; i++)
                        {
                            tmx[i] = scales * (genome[i] - xOpt[i]) + 1;
                        }
                    }
                    else
                    {
                        /* BOUNDARY HANDLING */
                        for (i = 0; i < genomeSize; i++)
                        {
                            tmp = Math.Abs(genome[i]) - 5.0;
                            if (tmp > 0.0)
                            {
                                fPen += tmp * tmp;
                            }
                        }
                        fAdd += 100.0 * fPen;
                        /* TRANSFORMATION IN SEARCH SPACE */
                        for (i = 0; i < genomeSize; i++)
                        {
                            tmx[i] = scales * (genome[i] - 0.75 * xOpt[i]) + 1;
                        }
                    }

                    /* COMPUTATION core */
                    for (i = 0; i < genomeSize - 1; i++)
                    {
                        tmp = (tmx[i] * tmx[i] - tmx[i + 1]);
                        value += tmp * tmp;
                    }
                    value *= 1e2;
                    for (i = 0; i < genomeSize - 1; i++)
                    {
                        tmp = (tmx[i] - 1.0);
                        value += tmp * tmp;
                    }

                    switch (noise)
                    {
                        case NONE:
                            break;
                        case GAUSSIAN:
                            value = fGauss(value, 1.0, state.Random[threadnum]);
                            break;
                        case UNIFORM:
                            value = fUniform(value, 0.49 + 1.0 / genomeSize, 1.0, state.Random[threadnum]);
                            break;
                        case CAUCHY:
                            value = fCauchy(value, 1.0, 0.2, state.Random[threadnum]);
                            break;
                        case GAUSSIAN_MODERATE:
                            value = fGauss(value, 0.01, state.Random[threadnum]);
                            break;
                        case UNIFORM_MODERATE:
                            value = fUniform(value, 0.01 * (0.49 + 1.0 / genomeSize), 0.01, state.Random[threadnum]);
                            break;
                        case CAUCHY_MODERATE:
                            value = fCauchy(value, 0.01, 0.05, state.Random[threadnum]);
                            break;
                        default:
                            String outputStr = "Invalid value for parameter, or parameter not found.\n" + "Acceptable values are:\n";
                            for (i = 0; i < noiseTypes.Length; i++)
                                outputStr += noiseTypes[i] + "\n";
                            state.Output.Fatal(outputStr, new Parameter(P_NOISE));
                            break;
                    }

                    value += fAdd;

                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;




                case ROSENBROCK_ROTATED:// f9
                    /* Rosenbrock, rotated */
                    fAdd = fOpt;

                    /* BOUNDARY HANDLING */

                    /* TRANSFORMATION IN SEARCH SPACE */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] = 0.5;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmx[i] += linearTF[i][j] * genome[j];
                        }
                    }

                    /* COMPUTATION core */
                    for (i = 0; i < genomeSize - 1; i++)
                    {
                        tmp = (tmx[i] * tmx[i] - tmx[i + 1]);
                        value += tmp * tmp;
                    }
                    value *= 1e2;
                    for (i = 0; i < genomeSize - 1; i++)
                    {
                        tmp = (tmx[i] - 1.0);
                        value += tmp * tmp;
                    }

                    value += fAdd;
                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case ELLIPSOIDAL_2:// f10
                    /* ellipsoid with monotone transformation, condition 1e6 */
                    condition = 1e6;

                    fAdd = fOpt;
                    /* BOUNDARY HANDLING */

                    /* TRANSFORMATION IN SEARCH SPACE */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmx[i] += rotation[i][j] * (genome[j] - xOpt[j]);
                        }
                    }

                    monotoneTFosc(tmx);
                    /* COMPUTATION core */
                    for (i = 0; i < genomeSize; i++)
                    {
                        fAdd += Math.Pow(condition, ((double)i) / ((double)(genomeSize - 1))) * tmx[i] * tmx[i];
                    }
                    value = fAdd;
                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case DISCUS:// f11
                    /* DISCUS (tablet) with monotone transformation, condition 1e6 */
                    condition = 1e6;
                    fAdd = fOpt;
                    /* BOUNDARY HANDLING */

                    /* TRANSFORMATION IN SEARCH SPACE */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmx[i] += rotation[i][j] * (genome[j] - xOpt[j]);
                        }
                    }

                    monotoneTFosc(tmx);

                    /* COMPUTATION core */
                    value = condition * tmx[0] * tmx[0];
                    for (i = 1; i < genomeSize; i++)
                    {
                        value += tmx[i] * tmx[i];
                    }
                    value += fAdd; /* without noise */
                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case BENT_CIGAR:// f12
                    /* bent cigar with asymmetric space distortion, condition 1e6 */
                    condition = 1e6;
                    beta = 0.5;
                    fAdd = fOpt;
                    /* BOUNDARY HANDLING */

                    /* TRANSFORMATION IN SEARCH SPACE */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmpvect[i] = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmpvect[i] += rotation[i][j] * (genome[j] - xOpt[j]);
                        }
                        if (tmpvect[i] > 0)
                        {
                            tmpvect[i] = Math.Pow(tmpvect[i], 1 + beta * ((double)i) / ((double)(genomeSize - 1)) * Math.Sqrt(tmpvect[i]));
                        }
                    }

                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmx[i] += rotation[i][j] * tmpvect[j];
                        }
                    }

                    /* COMPUTATION core */
                    value = tmx[0] * tmx[0];
                    for (i = 1; i < genomeSize; i++)
                    {
                        value += condition * tmx[i] * tmx[i];
                    }
                    value += fAdd;
                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case SHARP_RIDGE:// f13
                    /* sharp ridge */
                    condition = 10.0;
                    alpha = 100.0;

                    fAdd = fOpt;
                    /* BOUNDARY HANDLING */

                    /* TRANSFORMATION IN SEARCH SPACE */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmx[i] += linearTF[i][j] * (genome[j] - xOpt[j]);
                        }
                    }

                    /* COMPUTATION core */
                    for (i = 1; i < genomeSize; i++)
                    {
                        value += tmx[i] * tmx[i];
                    }
                    value = alpha * Math.Sqrt(value);
                    value += tmx[0] * tmx[0];
                    value += fAdd;
                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case DIFFERENT_POWERS:// f14
                    /* sum of different powers, between x^2 and x^6 */
                    alpha = 4.0;
                    fAdd = fOpt;
                    if (noise != NONE)
                    {
                        /* BOUNDARY HANDLING */
                        for (i = 0; i < genomeSize; i++)
                        {
                            tmp = Math.Abs(genome[i]) - 5.0;
                            if (tmp > 0.0)
                            {
                                fPen += tmp * tmp;
                            }
                        }
                        fAdd += 100.0 * fPen;
                    }

                    /* TRANSFORMATION IN SEARCH SPACE */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmx[i] += rotation[i][j] * (genome[j] - xOpt[j]);
                        }
                    }

                    /* COMPUTATION core */
                    for (i = 0; i < genomeSize; i++)
                    {
                        value += Math.Pow(Math.Abs(tmx[i]), 2.0 + alpha * ((double)i) / ((double)(genomeSize - 1)));
                    }
                    value = Math.Sqrt(value);
                    switch (noise)
                    {
                        case NONE:
                            break;
                        case GAUSSIAN:
                            value = fGauss(value, 1.0, state.Random[threadnum]);
                            break;
                        case UNIFORM:
                            value = fUniform(value, 0.49 + 1.0 / genomeSize, 1.0, state.Random[threadnum]);
                            break;
                        case CAUCHY:
                            value = fCauchy(value, 1.0, 0.2, state.Random[threadnum]);
                            break;
                        default:
                            String outputStr = "Invalid value for parameter, or parameter not found.\n" + "Acceptable values are:\n";
                            for (i = 0; i < 4; i++)
                                outputStr += noiseTypes[i] + "\n";
                            state.Output.Fatal(outputStr, new Parameter(P_NOISE));
                            break;
                    }
                    value += fAdd;
                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case RASTRIGIN_2:// f15
                    /* Rastrigin with asymmetric non-linear distortion, "condition" 10 */
                    condition = 10.0;
                    beta = 0.2;
                    tmp = tmp2 = 0;

                    fAdd = fOpt;
                    /* BOUNDARY HANDLING */

                    /* TRANSFORMATION IN SEARCH SPACE */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmpvect[i] = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmpvect[i] += rotation[i][j] * (genome[j] - xOpt[j]);
                        }
                    }

                    monotoneTFosc(tmpvect);
                    for (i = 0; i < genomeSize; i++)
                    {
                        if (tmpvect[i] > 0)
                            tmpvect[i] = Math.Pow(tmpvect[i], 1 + beta * ((double)i) / ((double)(genomeSize - 1)) * Math.Sqrt(tmpvect[i]));
                    }
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmx[i] += linearTF[i][j] * tmpvect[j];
                        }
                    }
                    /* COMPUTATION core */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp += Math.Cos(2.0 * Math.PI * tmx[i]);
                        tmp2 += tmx[i] * tmx[i];
                    }
                    value = 10.0 * ((double)genomeSize - tmp) + tmp2;
                    value += fAdd;
                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case WEIERSTRASS:// f16
                    /* Weierstrass, condition 100 */
                    condition = 100.0;
                    fPen = 0;

                    fAdd = fOpt;

                    /* BOUNDARY HANDLING */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp = Math.Abs(genome[i]) - 5.0;
                        if (tmp > 0.0)
                        {
                            fPen += tmp * tmp;
                        }
                    }
                    fAdd += 10.0 / (double)genomeSize * fPen;

                    /* TRANSFORMATION IN SEARCH SPACE */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmpvect[i] = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmpvect[i] += rotation[i][j] * (genome[j] - xOpt[j]);
                        }
                    }

                    monotoneTFosc(tmpvect);
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmx[i] += linearTF[i][j] * tmpvect[j];
                        }
                    }
                    /* COMPUTATION core */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp = 0.0;
                        for (j = 0; j < 12; j++)
                        {
                            tmp += Math.Cos(2 * Math.PI * (tmx[i] + 0.5) * bK[j]) * aK[j];
                        }
                        value += tmp;
                    }
                    value = 10.0 * Math.Pow(value / (double)genomeSize - f0, 3.0);
                    value += fAdd;
                    ;

                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case SCHAFFERS_F7:// f17
                    /*
                     * Schaffers F7 with asymmetric non-linear transformation, condition
                     * 10
                     */
                    condition = 10.0;
                    beta = 0.5;
                    fAdd = fOpt;

                    /* BOUNDARY HANDLING */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp = Math.Abs(genome[i]) - 5.0;
                        if (tmp > 0.0)
                        {
                            fPen += tmp * tmp;
                        }
                    }
                    fAdd += 10.0 * fPen;

                    /* TRANSFORMATION IN SEARCH SPACE */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmpvect[i] = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmpvect[i] += rotation[i][j] * (genome[j] - xOpt[j]);
                        }
                        if (tmpvect[i] > 0)
                            tmpvect[i] = Math.Pow(tmpvect[i], 1 + beta * ((double)i) / ((double)(genomeSize - 1)) * Math.Sqrt(tmpvect[i]));
                    }

                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] = 0.0;
                        tmp = Math.Pow(Math.Sqrt(condition), ((double)i) / ((double)(genomeSize - 1)));
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmx[i] += tmp * rot2[i][j] * tmpvect[j];
                        }
                    }

                    /* COMPUTATION core */
                    for (i = 0; i < genomeSize - 1; i++)
                    {
                        tmp = tmx[i] * tmx[i] + tmx[i + 1] * tmx[i + 1];
                        value += Math.Pow(tmp, 0.25) * (Math.Pow(Math.Sin(50 * Math.Pow(tmp, 0.1)), 2.0) + 1.0);
                    }
                    value = Math.Pow(value / (double)(genomeSize - 1), 2.0);
                    switch (noise)
                    {
                        case NONE:
                            break;
                        case GAUSSIAN:
                            value = fGauss(value, 1.0, state.Random[threadnum]);
                            break;
                        case UNIFORM:
                            value = fUniform(value, 0.49 + 1.0 / genomeSize, 1.0, state.Random[threadnum]);
                            break;
                        case CAUCHY:
                            value = fCauchy(value, 1.0, 0.2, state.Random[threadnum]);
                            break;
                        default:
                            String outputStr = "Invalid value for parameter, or parameter not found.\n" + "Acceptable values are:\n";
                            for (i = 0; i < 4; i++)
                                outputStr += noiseTypes[i] + "\n";
                            state.Output.Fatal(outputStr, new Parameter(P_NOISE));
                            break;
                    }
                    value += fAdd;
                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case SCHAFFERS_F7_2:// f18
                    /*
                     * Schaffers F7 with asymmetric non-linear transformation, condition
                     * 1000
                     */
                    condition = 1e3;
                    beta = 0.5;
                    fPen = 0.0;
                    fAdd = fOpt;
                    /* BOUNDARY HANDLING */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp = Math.Abs(genome[i]) - 5.0;
                        if (tmp > 0.0)
                        {
                            fPen += tmp * tmp;
                        }
                    }
                    fAdd += 10.0 * fPen;

                    /* TRANSFORMATION IN SEARCH SPACE */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmpvect[i] = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmpvect[i] += rotation[i][j] * (genome[j] - xOpt[j]);
                        }
                        if (tmpvect[i] > 0)
                            tmpvect[i] = Math.Pow(tmpvect[i], 1.0 + beta * ((double)i) / ((double)(genomeSize - 1)) * Math.Sqrt(tmpvect[i]));
                    }

                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] = 0.0;
                        tmp = Math.Pow(Math.Sqrt(condition), ((double)i) / ((double)(genomeSize - 1)));
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmx[i] += tmp * rot2[i][j] * tmpvect[j];
                        }
                    }

                    /* COMPUTATION core */
                    for (i = 0; i < genomeSize - 1; i++)
                    {
                        tmp = tmx[i] * tmx[i] + tmx[i + 1] * tmx[i + 1];
                        value += Math.Pow(tmp, 0.25) * (Math.Pow(Math.Sin(50.0 * Math.Pow(tmp, 0.1)), 2.0) + 1.0);
                    }
                    value = Math.Pow(value / (double)(genomeSize - 1), 2.0);
                    value += fAdd;
                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case GRIEWANK_ROSENBROCK:// f19
                    /* F8f2 sum of Griewank-Rosenbrock 2-D blocks */
                    fAdd = fOpt;
                    if (noise == NONE)
                    {
                        /* TRANSFORMATION IN SEARCH SPACE */
                        for (i = 0; i < genomeSize; i++)
                        {
                            tmx[i] = 0.5;
                            for (j = 0; j < genomeSize; j++)
                            {
                                tmx[i] += linearTF[i][j] * genome[j];
                            }
                        }
                        /* COMPUTATION core */
                        for (i = 0; i < genomeSize - 1; i++)
                        {
                            tmp2 = tmx[i] * tmx[i] - tmx[i + 1];
                            f2 = 100.0 * tmp2 * tmp2;
                            tmp2 = 1 - tmx[i];
                            f2 += tmp2 * tmp2;
                            tmp += f2 / 4000.0 - Math.Cos(f2);
                        }
                        value = 10.0 + 10.0 * tmp / (double)(genomeSize - 1);
                    }
                    else
                    {
                        /* BOUNDARY HANDLING */
                        for (i = 0; i < genomeSize; i++)
                        {
                            tmp = Math.Abs(genome[i]) - 5.0;
                            if (tmp > 0.0)
                            {
                                fPen += tmp * tmp;
                            }
                        }
                        fAdd += 100.0 * fPen;

                        /* TRANSFORMATION IN SEARCH SPACE */
                        for (i = 0; i < genomeSize; i++)
                        {
                            tmx[i] = 0.5;
                            for (j = 0; j < genomeSize; j++)
                            {
                                tmx[i] += scales * rotation[i][j] * genome[j];
                            }
                        }
                        /* COMPUTATION core */
                        tmp = 0.0;
                        for (i = 0; i < genomeSize - 1; i++)
                        {
                            f2 = 100.0 * (tmx[i] * tmx[i] - tmx[i + 1]) * (tmx[i] * tmx[i] - tmx[i + 1]) + (1 - tmx[i]) * (1 - tmx[i]);
                            tmp += f2 / 4000.0 - Math.Cos(f2);
                        }
                        value = 1.0 + 1.0 * tmp / (double)(genomeSize - 1);
                    }
                    switch (noise)
                    {
                        case NONE:
                            break;
                        case GAUSSIAN:
                            value = fGauss(value, 1.0, state.Random[threadnum]);
                            break;
                        case UNIFORM:
                            value = fUniform(value, 0.49 + 1.0 / genomeSize, 1.0, state.Random[threadnum]);
                            break;
                        case CAUCHY:
                            value = fCauchy(value, 1.0, 0.2, state.Random[threadnum]);
                            break;
                        default:
                            String outputStr = "Invalid value for parameter, or parameter not found.\n" + "Acceptable values are:\n";
                            for (i = 0; i < 4; i++)
                                outputStr += noiseTypes[i] + "\n";
                            state.Output.Fatal(outputStr, new Parameter(P_NOISE));
                            break;
                    }
                    value += fAdd;
                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case SCHWEFEL:// f20
                    /* Schwefel with tridiagonal variable transformation */
                    condition = 10.0;
                    fPen = 0.0;
                    fAdd = fOpt;

                    /* TRANSFORMATION IN SEARCH SPACE */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmpvect[i] = 2.0 * genome[i];
                        if (xOpt[i] < 0.0)
                            tmpvect[i] *= -1.0;
                    }

                    tmx[0] = tmpvect[0];
                    for (i = 1; i < genomeSize; i++)
                    {
                        tmx[i] = tmpvect[i] + 0.25 * (tmpvect[i - 1] - 2.0 * Math.Abs(xOpt[i - 1]));
                    }

                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] -= 2 * Math.Abs(xOpt[i]);
                        tmx[i] *= Math.Pow(Math.Sqrt(condition), ((double)i) / ((double)(genomeSize - 1)));
                        tmx[i] = 100.0 * (tmx[i] + 2 * Math.Abs(xOpt[i]));
                    }

                    /* BOUNDARY HANDLING */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp = Math.Abs(tmx[i]) - 500.0;
                        if (tmp > 0.0)
                        {
                            fPen += tmp * tmp;
                        }
                    }
                    fAdd += 0.01 * fPen;

                    /* COMPUTATION core */
                    for (i = 0; i < genomeSize; i++)
                    {
                        value += tmx[i] * Math.Sin(Math.Sqrt(Math.Abs(tmx[i])));
                    }
                    value = 0.01 * ((418.9828872724339) - value / (double)genomeSize);
                    value += fAdd;/* without noise */
                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case GALLAGHER_GAUSSIAN_101ME:// f21
                    /*
                     * Gallagher with 101 Gaussian peaks, condition up to 1000, one
                     * global rotation
                     */
                    a = 0.1;
                    fac = -0.5 / (double)genomeSize;
                    fAdd = fOpt;

                    /* BOUNDARY HANDLING */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp = Math.Abs(genome[i]) - 5.0;
                        if (tmp > 0.0)
                        {
                            fPen += tmp * tmp;
                        }
                    }
                    if (noise == NONE)
                        fAdd += fPen;
                    else
                        fAdd += 100.0 * fPen;

                    /* TRANSFORMATION IN SEARCH SPACE */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmx[i] += rotation[i][j] * genome[j];
                        }
                    }

                    /* COMPUTATION core */
                    if (noise == NONE)
                        for (i = 0; i < NHIGHPEAKS21; i++)
                        {
                            tmp2 = 0.0;
                            for (j = 0; j < genomeSize; j++)
                            {
                                tmp = (tmx[j] - xLocal[j][i]);
                                tmp2 += arrScales[i][j] * tmp * tmp;
                            }
                            tmp2 = peakvalues[i] * Math.Exp(fac * tmp2);
                            f = Math.Max(f, tmp2);
                        }
                    else
                        /* COMPUTATION core */
                        for (i = 0; i < NHIGHPEAKS21; i++)
                        {
                            tmp2 = 0.0;
                            for (j = 0; j < genomeSize; j++)
                            {
                                tmp2 += arrScales[i][j] * (tmx[j] - xLocal[j][i]) * (tmx[j] - xLocal[j][i]);
                            }
                            tmp2 = peakvalues[i] * Math.Exp(fac * tmp2);
                            f = Math.Max(f, tmp2);
                        }

                    f = 10.0 - f;
                    /* monotoneTFosc */
                    if (f > 0)
                    {
                        value = Math.Log(f) / a;
                        value = Math.Pow(Math.Exp(value + 0.49 * (Math.Sin(value) + Math.Sin(0.79 * value))), a);
                    }
                    else if (f < 0)
                    {
                        value = Math.Log(-f) / a;
                        value = -Math.Pow(Math.Exp(value + 0.49 * (Math.Sin(0.55 * value) + Math.Sin(0.31 * value))), a);
                    }
                    else
                        value = f;

                    value *= value;
                    switch (noise)
                    {
                        case NONE:
                            break;
                        case GAUSSIAN:
                            value = fGauss(value, 1.0, state.Random[threadnum]);
                            break;
                        case UNIFORM:
                            value = fUniform(value, 0.49 + 1.0 / genomeSize, 1.0, state.Random[threadnum]);
                            break;
                        case CAUCHY:
                            value = fCauchy(value, 1.0, 0.2, state.Random[threadnum]);
                            break;
                        default:
                            var outputStr = "Invalid value for parameter, or parameter not found.\n" + "Acceptable values are:\n";
                            for (i = 0; i < 4; i++)
                                outputStr += noiseTypes[i] + "\n";
                            state.Output.Fatal(outputStr, new Parameter(P_NOISE));
                            break;
                    }
                    value += fAdd;
                    ; /* without noise */

                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case GALLAGHER_GAUSSIAN_21HI:// f22
                    /*
                     * Gallagher with 21 Gaussian peaks, condition up to 1000, one
                     * global rotation
                     */
                    a = 0.1;
                    f = 0;
                    fac = -0.5 / (double)genomeSize;
                    fPen = 0.0;

                    fAdd = fOpt;

                    /* BOUNDARY HANDLING */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp = Math.Abs(genome[i]) - 5.0;
                        if (tmp > 0.0)
                        {
                            fPen += tmp * tmp;
                        }
                    }
                    fAdd += fPen;

                    /* TRANSFORMATION IN SEARCH SPACE */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmx[i] += rotation[i][j] * genome[j];
                        }
                    }

                    /* COMPUTATION core */
                    for (i = 0; i < NHIGHPEAKS22; i++)
                    {
                        tmp2 = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmp = (tmx[j] - xLocal[j][i]);
                            tmp2 += arrScales[i][j] * tmp * tmp;
                        }
                        tmp2 = peakvalues[i] * Math.Exp(fac * tmp2);
                        f = Math.Max(f, tmp2);
                    }

                    f = 10.0 - f;
                    if (f > 0)
                    {
                        value = Math.Log(f) / a;
                        value = Math.Pow(Math.Exp(value + 0.49 * (Math.Sin(value) + Math.Sin(0.79 * value))), a);
                    }
                    else if (f < 0)
                    {
                        value = Math.Log(-f) / a;
                        value = -Math.Pow(Math.Exp(value + 0.49 * (Math.Sin(0.55 * value) + Math.Sin(0.31 * value))), a);
                    }
                    else
                        value = f;

                    value *= value;
                    value += fAdd;
                    ; /* without noise */

                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case KATSUURA:// f23
                    /* Katsuura function */
                    condition = 100.0;
                    fAdd = 0;
                    fPen = 0;
                    double arr;
                    double prod = 1.0;
                    double[] ptmx,
                        plinTF,
                        ptmp;

                    fAdd = fOpt;

                    /* BOUNDARY HANDLING */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp = Math.Abs(genome[i]) - 5.0;
                        if (tmp > 0.0)
                        {
                            fPen += tmp * tmp;
                        }
                    }
                    fAdd += fPen;

                    /* TRANSFORMATION IN SEARCH SPACE */
                    /* write rotated difference vector into tmx */
                    for (j = 0; j < genomeSize; j++)
                        /* store difference vector */
                        tmpvect[j] = genome[j] - xOpt[j];
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] = 0.0;
                        ptmx = tmx;
                        plinTF = linearTF[i];
                        ptmp = tmpvect;
                        for (j = 0; j < genomeSize; j++)
                        {
                            // *ptmx += *plinTF++ * *ptmp++;
                            ptmx[j] += plinTF[j] * ptmp[j];
                        }
                    }

                    /*
                     * for (i = 0; i < genomeSize; i++) { tmx[i] = 0.0; for (j = 0; j <
                     * genomeSize; j++) { tmx[i] += linearTF[i][j] * (genome[j] -
                     * xOpt[j]); } }
                     */

                    /* COMPUTATION core */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp = 0.0;
                        for (j = 1; j < 33; j++)
                        {
                            tmp2 = Math.Pow(2.0, (double)j);
                            arr = tmx[i] * tmp2;
                            tmp += Math.Abs(arr - Math.Round(arr)) / tmp2;
                        }
                        tmp = 1.0 + tmp * (double)(i + 1);
                        prod *= tmp;
                    }
                    value = 10.0 / (double)genomeSize / (double)genomeSize * (-1.0 + Math.Pow(prod, 10.0 / Math.Pow((double)genomeSize, 1.2)));
                    value += fAdd;
                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;



                case LUNACEK:// f24
                    /* Lunacek bi-Rastrigin, condition 100 */
                    /* in PPSN 2008, Rastrigin part rotated and scaled */
                    condition = 100.0;
                    double mu1 = 2.5;
                    double tmp3,
                        tmp4;
                    fPen = tmp2 = tmp3 = tmp4 = 0.0;
                    double s = 1.0 - 0.5 / (Math.Sqrt((double)(genomeSize + 20)) - 4.1);
                    double d = 1.0;
                    double mu2 = -Math.Sqrt((mu1 * mu1 - d) / s);

                    fAdd = fOpt;

                    /* BOUNDARY HANDLING */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp = Math.Abs(genome[i]) - 5.0;
                        if (tmp > 0.0)
                        {
                            fPen += tmp * tmp;
                        }
                    }
                    fAdd += 1e4 * fPen;

                    /* TRANSFORMATION IN SEARCH SPACE */
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmx[i] = 2.0 * genome[i];
                        if (xOpt[i] < 0.0)
                            tmx[i] *= -1.0;
                    }

                    /* COMPUTATION core */
                    tmp = 0.0;
                    for (i = 0; i < genomeSize; i++)
                    {
                        tmp2 += (tmx[i] - mu1) * (tmx[i] - mu1);
                        tmp3 += (tmx[i] - mu2) * (tmx[i] - mu2);
                        tmp4 = 0.0;
                        for (j = 0; j < genomeSize; j++)
                        {
                            tmp4 += linearTF[i][j] * (tmx[j] - mu1);
                        }
                        tmp += Math.Cos(2 * Math.PI * tmp4);
                    }
                    value = Math.Min(tmp2, d * (double)genomeSize + s * tmp3) + 10.0 * ((double)genomeSize - tmp);
                    value += fAdd;
                    fit = -value;
                    ((SimpleFitness)ind.Fitness).SetFitness(state, fit, fit.Equals(0.0));
                    break;
                default:
                    break;
            }
            ind.Evaluated = true;
        }

        public const double TOL = 1e-8;

        void gauss(double[] g, IMersenneTwister random)
        {
            /*
             * samples N standard normally distributed numbers being the same for a
             * given seed.
             */
            var uniftmp = new double[2 * g.Length];
            int i;
            for (i = 0; i < uniftmp.Length; i++)
                uniftmp[i] = nextDoubleClosedInterval(random);
            for (i = 0; i < g.Length; i++)
            {

                g[i] = Math.Sqrt(-2 * Math.Log(uniftmp[i])) * Math.Cos(2 * Math.PI * uniftmp[g.Length + i]);
                if (g[i] == 0.0)
                    g[i] = 1e-99;
            }
            return;
        }

        void gauss(double[] g, IMersenneTwister random, int n)
        {
            /*
             * samples N standard normally distributed numbers being the same for a
             * given seed.
             */
            var uniftmp = new double[2 * g.Length];
            int i;
            for (i = 0; i < uniftmp.Length; i++)
                uniftmp[i] = nextDoubleClosedInterval(random);
            for (i = 0; i < n; i++)
            {
                g[i] = Math.Sqrt(-2 * Math.Log(uniftmp[i])) * Math.Cos(2 * Math.PI * uniftmp[n + i]);
                if (g[i].Equals(0.0))
                    g[i] = 1e-99;
            }
            return;
        }

        void computeXopt(double[] xOpt, IMersenneTwister random)
        {
            int i;
            int n = xOpt.Length;
            for (i = 0; i < n; i++)
            {
                xOpt[i] = 8 * (int)Math.Floor(1e4 * nextDoubleClosedInterval(random)) / 1e4 - 4;
                if (xOpt[i].Equals(0.0))
                    xOpt[i] = -1e-5;
            }
        }

        void monotoneTFosc(double[] f)
        {
            var a = 0.1;
            int i;
            var n = f.Length;
            for (i = 0; i < n; i++)
            {
                if (f[i] > 0)
                {
                    f[i] = Math.Log(f[i]) / a;
                    f[i] = Math.Pow(Math.Exp(f[i] + 0.49 * (Math.Sin(f[i]) + Math.Sin(0.79 * f[i]))), a);
                }
                else if (f[i] < 0)
                {
                    f[i] = Math.Log(-f[i]) / a;
                    f[i] = -Math.Pow(Math.Exp(f[i] + 0.49 * (Math.Sin(0.55 * f[i]) + Math.Sin(0.31 * f[i]))), a);
                }
            }
        }

        double[][] reshape(double[][] b, double[] vector, int m, int n)
        {
            int i, j;
            for (i = 0; i < m; i++)
            {
                for (j = 0; j < n; j++)
                {
                    b[i][j] = vector[j * m + i];
                }
            }
            return b;
        }

        void computeRotation(double[][] b, IMersenneTwister random, int genomeSize)
        {
           var gvect = new double[genomeSize * genomeSize];
            double prod;
            int i, j, k; /* Loop over pairs of column vectors */

            gauss(gvect, random);
            reshape(b, gvect, genomeSize, genomeSize);
            /* 1st coordinate is row, 2nd is column. */

            for (i = 0; i < genomeSize; i++)
            {
                for (j = 0; j < i; j++)
                {
                    prod = 0;
                    for (k = 0; k < genomeSize; k++)
                    {
                        prod += b[k][i] * b[k][j];
                    }
                    for (k = 0; k < genomeSize; k++)
                    {
                        b[k][i] -= prod * b[k][j];
                    }
                }
                prod = 0;
                for (k = 0; k < genomeSize; k++)
                {
                    prod += b[k][i] * b[k][i];
                }
                for (k = 0; k < genomeSize; k++)
                {
                    b[k][i] /= Math.Sqrt(prod);
                }
            }
        }

        double fGauss(double fTrue, double beta, IMersenneTwister random)
        {
            var fVal = fTrue * Math.Exp(beta * nextDoubleClosedInterval(random));
            fVal += 1.01 * TOL;
            if (fTrue < TOL)
            {
                fVal = fTrue;
            }
            return fVal;
        }

        double fUniform(double fTrue, double alpha, double beta, IMersenneTwister random)
        {
            var fVal = Math.Pow(nextDoubleClosedInterval(random), beta) * fTrue 
                * Math.Max(1.0, Math.Pow(1e9 / (fTrue + 1e-99), alpha * nextDoubleClosedInterval(random)));
            fVal += 1.01 * TOL;
            if (fTrue < TOL)
            {
                fVal = fTrue;
            }
            return fVal;
        }

        double fCauchy(double fTrue, double alpha, double p, IMersenneTwister random)
        {
            double fVal;
            var tmp = nextDoubleClosedInterval(random) / Math.Abs(nextDoubleClosedInterval(random) + 1e-199);
            /*
             * tmp is so as to actually do the calls to randn in order for the
             * number of calls to be the same as in the Matlab code.
             */
            if (nextDoubleClosedInterval(random) < p)
                fVal = fTrue + alpha * Math.Max(0.0, 1e3 + tmp);
            else
                fVal = fTrue + alpha * 1e3;

            fVal += 1.01 * TOL;
            if (fTrue < TOL)
            {
                fVal = fTrue;
            }
            return fVal;
        }

        double computeFopt(IMersenneTwister random)
        {
            var gval = new double[1];
            var gval2 = new double[1];
            gauss(gval, random, 1);
            gauss(gval2, random, 1);
            return Math.Min(1000.0, Math.Max(-1000.0, Math.Round(100.0 * 100.0 * gval[0] / gval2[0]) / 100.0));
        }

        double nextDoubleClosedInterval(IMersenneTwister random)
        {
            var tmp = random.NextDouble() * 2.0;
            while (tmp > 1.0)
                tmp = random.NextDouble() * 2.0;
            return tmp;
        }
    }
}