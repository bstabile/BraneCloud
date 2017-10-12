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
using BraneCloud.Evolution.EC.MultiObjective;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.App.MooSuite.Test
{
    /// <summary>
    /// Several standard Multi-objective benchmarks are implemented: 
    /// <ul>
    /// <li/>ZDT1: Zitzler, Deb & Thiele
    /// <li/>ZDT2: Zitzler, Deb & Thiele 
    /// <li/>ZDT3: Zitzler, Deb & Thiele 
    /// <li/>ZDT4: Zitzler, Deb & Thiele 
    /// <li/>ZDT6: Zitzler, Deb & Thiele 
    /// <li/>SPHERE: ftp.tik.ee.ethz.ch/pub/people/zitzler/ZLT2001a.pdf 
    /// <li/>SCH: (Schaffer), (a.k.a. F1 in Srinivas & Deb); requires exactly 1 decision variables (genes)
    /// <li/>F2: (Schaffer), (Srinivas & Deb),  (Coello Coello & Cortes); requires exactly 1 decision variables (genes)
    /// <li/>unconstrained F3: Schaffer, Srinivas & Deb  (Chankong & Haimes); requires exactly 2 decision variables (genes)
    /// <li/>QV: Quagliarella & Vicini
    /// <li/>FON: Fonseca & Fleming; requires exactly 3 decision variables (genes)
    /// <li/>POL: Poloni; requires exactly 2 decision variables (genes)
    /// <li/>KUR: Kursawe from the Errata of Zitzler's TIK-Report 103: "SPEA2: Improving the Strength Pareto Evolutionary Algorithm"
    /// (note that many different versions are described in the literature).
    /// </ul>   
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>type</tt><br/>
    /// <font size="-1">String, one of: zdt1, zdt2, zdt3, zdt4, zdt6, sphere, sch, fon, qv, pol, kur, f1, f2, unconstrained-f3</font></td>
    /// <td valign="top">The multi-objective optimization problem to test against. </td></tr>
    /// </table>
    /// </summary>
    [ECConfiguration("ec.app.moosuite.MooSuite")]
    public class MooSuite : Problem, ISimpleProblem
    {
        private const long serialVersionUID = 1L;
        public const string P_WHICH_PROBLEM = "type";
        public const string P_ZDT1 = "zdt1";
        public const string P_ZDT2 = "zdt2";
        public const string P_ZDT3 = "zdt3";
        public const string P_ZDT4 = "zdt4";
        public const string P_ZDT6 = "zdt6";
        public const string P_SPHERE = "sphere";
        public const string P_SCH = "sch";
        public const string P_FON = "fon";
        public const string P_QV = "qv";
        public const string P_POL = "pol";
        public const string P_KUR_NSGA2 = "kur-nsga2";
        public const string P_KUR_SPEA2 = "kur-spea2";
        public const string P_F1 = "f1";
        public const string P_F2 = "f2";
        public const string P_F3 = "unconstrained-f3";

        //Some of the following problems requires an exact number of decision variables (genes). This is mentioned in comment preceding the problem.

        public const int PROB_SPHERE = 0;
        public const int PROB_ZDT1 = 1;
        public const int PROB_ZDT2 = 2;
        public const int PROB_ZDT3 = 3;
        public const int PROB_ZDT4 = 4;
        public const int PROB_ZDT6 = 6;
        public const int PROB_FON = 7;
        public const int PROB_POL = 8;
        public const int PROB_KUR_NSGA2 = 9;
        public const int PROB_KUR_SPEA2 = 10;
        public const int PROB_QV = 11;
        public const int PROB_SCH = 12;
        public const int PROB_F2 = 13;
        public const int PROB_F3 = 14;

        public int problemType = PROB_ZDT1;  // defaults on zdt1

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            var wp = state.Parameters.GetStringWithDefault(paramBase.Push(P_WHICH_PROBLEM), null, "");
            if (wp.CompareTo(P_ZDT1) == 0 || wp.CompareTo("") == 0) // default
                problemType = PROB_ZDT1;
            else if (wp.CompareTo(P_ZDT2) == 0)
                problemType = PROB_ZDT2;
            else if (wp.CompareTo(P_ZDT3) == 0)
                problemType = PROB_ZDT3;
            else if (wp.CompareTo(P_ZDT4) == 0)
                problemType = PROB_ZDT4;
            else if (wp.CompareTo(P_ZDT6) == 0)
                problemType = PROB_ZDT6;
            else if (wp.CompareTo(P_FON) == 0)
                problemType = PROB_FON;
            else if (wp.CompareTo(P_POL) == 0)
                problemType = PROB_POL;
            else if (wp.CompareTo(P_QV) == 0)
                problemType = PROB_QV;
            else if (wp.CompareTo(P_KUR_NSGA2) == 0)
                problemType = PROB_KUR_NSGA2;
            else if (wp.CompareTo(P_KUR_SPEA2) == 0)
                problemType = PROB_KUR_SPEA2;
            else if (wp.CompareTo(P_SPHERE) == 0)
                problemType = PROB_SPHERE;
            else if (wp.CompareTo(P_F2) == 0)
                problemType = PROB_F2;
            else if (wp.CompareTo(P_F3) == 0)
                problemType = PROB_F3;
            else if (wp.CompareTo(P_SCH) == 0 || wp.CompareTo(P_F1) == 0)
                problemType = PROB_SCH;
            else state.Output.Fatal(
                "Invalid value for parameter, or parameter not found.\n" +
                "Acceptable values are:\n" +
                "  " + P_ZDT1 + "\n" +
                "  " + P_ZDT2 + "\n" +
                "  " + P_ZDT3 + "\n" +
                "  " + P_ZDT4 + "\n" +
                "  " + P_ZDT6 + "\n" +
                "  " + P_POL + "\n" +
                "  " + P_FON + "\n" +
                "  " + P_KUR_NSGA2 + "\n" +
                "  " + P_KUR_SPEA2 + "\n" +
                "  " + P_SPHERE + "\n" +
                "  " + P_SCH + "(or " + P_F1 + ")\n" +
                "  " + P_F2 + "\n",
                paramBase.Push(P_WHICH_PROBLEM));
        }
        private const double TWO_PI = Math.PI * 2;//QV uses it.
        private const double TEN_PI = Math.PI * 10;//ZDT3 uses it.
        private const double FOUR_PI = Math.PI * 4;//ZDT4 uses it.
        private const double SIX_PI = Math.PI * 6;//ZDT6 uses it.
        private static readonly double ONE_OVER_SQRT_3 = 1d / Math.Sqrt(3);//FON uses it.
        private static readonly double A1 = 0.5 * Math.Sin(1) - 2 * Math.Cos(1) + Math.Sin(2) - 1.5 * Math.Cos(2);//POL uses it
        private static readonly double A2 = 1.5 * Math.Sin(1) - Math.Cos(1) + 2 * Math.Sin(2) - 0.5 * Math.Cos(2);//POL uses it

        public void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            if (!(ind is DoubleVectorIndividual))
                state.Output.Fatal("The individuals for this problem should be DoubleVectorIndividuals.");

            var temp = (DoubleVectorIndividual)ind;
            var genome = temp.genome;
            var numDecisionVars = genome.Length;

            float[] objectives = ((MultiObjectiveFitness)ind.Fitness).GetObjectives();

            double f, g, h, sum;

            switch (problemType)
            {
                case PROB_ZDT1:
                    f = genome[0];
                    objectives[0] = (float)f;
                    sum = 0;
                    for (var i = 1; i < numDecisionVars; ++i)
                        sum += genome[i];
                    g = 1d + 9d * sum / (numDecisionVars - 1);
                    h = 1d - Math.Sqrt(f / g);
                    objectives[1] = (float)(g * h);
                    break;

                case PROB_ZDT2:
                    f = genome[0];
                    objectives[0] = (float)f;
                    sum = 0;
                    for (var i = 1; i < numDecisionVars; i++)
                        sum += genome[i];
                    g = 1.0 + 9.0 * sum / (numDecisionVars - 1);
                    h = 1.0 - (f / g) * (f / g);
                    objectives[1] = (float)(g * h);
                    break;

                case PROB_ZDT3:
                    f = genome[0];
                    objectives[0] = (float)f;
                    sum = 0;
                    for (var i = 1; i < numDecisionVars; i++)
                        sum += genome[i];
                    g = 1.0 + 9.0 * sum / (numDecisionVars - 1);
                    var foverg = f / g;
                    h = 1.0 - Math.Sqrt(foverg) - foverg * Math.Sin(TEN_PI * f);
                    objectives[1] = (float)(g * h);
                    break;
                case PROB_ZDT4:
                    f = genome[0];
                    objectives[0] = (float)f;
                    sum = 0;
                    for (var i = 1; i < numDecisionVars; ++i)
                        sum += genome[i] * genome[i] - 10 * Math.Cos(FOUR_PI * genome[i]);

                    g = 1 + 10 * (numDecisionVars - 1) + sum;
                    h = 1 - Math.Sqrt(f / g);
                    objectives[1] = (float)(g * h);
                    break;
                case PROB_ZDT6:
                    f = 1 - (Math.Exp(-4 * genome[0]) * Math.Pow(Math.Sin(SIX_PI * genome[0]), 6));
                    objectives[0] = (float)f;
                    sum = 0;
                    for (var i = 1; i < numDecisionVars; ++i)
                        sum += genome[i];
                    g = 1d + 9 * Math.Pow(sum / (numDecisionVars - 1), 0.25);
                    h = 1d - Math.Pow(f / g, 2);
                    objectives[1] = (float)(g * h);
                    break;
                case PROB_SPHERE:
                    var numObjectives = objectives.Length;
                    for (var j = 0; j < numObjectives; ++j)
                    {
                        sum = (genome[j] - 1) * (genome[j] - 1);
                        for (var i = 0; i < numDecisionVars; ++i)
                            if (i != j)
                                sum += genome[i] * genome[i];
                        objectives[j] = (float)sum;
                    }
                    break;
                case PROB_SCH:
                    if (numDecisionVars != 1) throw new ApplicationException("SCH needs exactly 1 decision variable (gene).");
                    var x = genome[0];
                    objectives[0] = (float)(x * x);
                    objectives[1] = (float)((x - 2) * (x - 2));
                    break;
                case PROB_F2:
                    if (numDecisionVars != 1) throw new ApplicationException("F2 needs exactly 1 decision variable (gene).");
                    x = genome[0];
                    objectives[0] = (float)(x <= 1 ? -x : (x <= 3 ? x - 2 : (x <= 4 ? 4 - x : x - 4)));
                    objectives[1] = (float)((x - 5) * (x - 5));
                    break;
                case PROB_F3:
                    if (numDecisionVars != 2) throw new ApplicationException("F3 needs exactly 2 decision variable (gene).");
                    var x1 = genome[0];
                    var x2 = genome[1];
                    objectives[0] = (float)((x1 - 2) * (x1 - 2) + (x2 - 1) * (x2 - 1) + 2);
                    objectives[1] = (float)(9 * x1 - (x2 - 1) * (x2 - 1));
                    break;
                case PROB_FON:
                    if (numDecisionVars != 3) throw new ApplicationException("FON needs exactly 3 decision variables (genes).");
                    double sum1 = 0, sum2 = 0;
                    for (var i = 0; i < numDecisionVars; i++)
                    {
                        var xi = genome[i];
                        var d = xi - ONE_OVER_SQRT_3;
                        var s = xi + ONE_OVER_SQRT_3;
                        sum1 += d * d;
                        sum2 += s * s;
                    }
                    objectives[0] = 1 - (float)Math.Exp(-sum1);
                    objectives[1] = 1 - (float)Math.Exp(-sum2);
                    break;
                case PROB_POL:
                    if (numDecisionVars != 2) throw new ApplicationException("POL needs exactly 2 decision variables (genes).");
                    x1 = genome[0];
                    x2 = genome[1];
                    var b1 = 0.5 * Math.Sin(x1) - 2 * Math.Cos(x1) + Math.Sin(x2) - 1.5 * Math.Cos(x2);
                    var b2 = 1.5 * Math.Sin(x1) - Math.Cos(x1) + 2 * Math.Sin(x2) - 0.5 * Math.Cos(x2);
                    objectives[0] = (float)(1 + (A1 - b1) * (A1 - b1) + (A2 - b2) * (A2 - b2));
                    objectives[1] = (float)((x1 + 3) * (x1 + 3) + (x2 + 1) * (x2 + 1));
                    break;
                case PROB_QV:
                    sum = 0;
                    for (var i = 0; i < numDecisionVars; i++)
                    {
                        var xi = genome[i];
                        sum += xi * xi - 10 * Math.Cos(TWO_PI * xi) + 10;
                    }
                    objectives[0] = (float)Math.Pow(sum / numDecisionVars, 0.25);
                    sum = 0;
                    for (var i = 0; i < numDecisionVars; i++)
                    {
                        var xi = genome[i] - 1.5;
                        sum += xi * xi - 10 * Math.Cos(TWO_PI * xi) + 10;
                    }
                    objectives[1] = (float)Math.Pow(sum / numDecisionVars, 0.25);
                    break;
                case PROB_KUR_NSGA2:
                    double thisSquared = genome[0] * genome[0];
                    sum = 0;
                    for (var i = 0; i < numDecisionVars - 1; ++i)
                    {
                        double nextSquared = genome[i + 1] * genome[i + 1];
                        //sum += 1d-Math.Exp(-0.2*Math.Sqrt(thisSquared + nextSquared));
                        sum += -10 - Math.Exp(-0.2 * Math.Sqrt(thisSquared + nextSquared));
                        thisSquared = nextSquared;
                    }
                    //objectives[1] = (float)sum;
                    objectives[0] = (float)sum;
                    sum = 0;
                    for (var i = 0; i < numDecisionVars; ++i)
                    {
                        //double sin_xi = Math.Sin(genome[i]);          
                        //double t1 = Math.Pow(Math.abs(genome[i]), 0.8);
                        //double t2 = 5 * sin_xi * sin_xi * sin_xi;
                        //sum +=t1+t2+ 3.5828;
                        var xi3 = Math.Pow(genome[i], 3);
                        var t1 = Math.Pow(Math.Abs(genome[i]), 0.8);
                        var t2 = 5 * Math.Sin(xi3);
                        sum += t1 + t2;
                    }
                    //objectives[0] = (float)sum;
                    objectives[1] = (float)sum;
                    break;

                default:
                    state.Output.Fatal("ec.app.ecsuite.ECSuite has an invalid problem -- how on earth did that happen?");
                    break;
            }

            ((MultiObjectiveFitness)ind.Fitness).SetObjectives(state, objectives);
            ind.Evaluated = true;
        }
    }
}