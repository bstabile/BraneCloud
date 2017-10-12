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

using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.Problems.Hiff
{
    /// <summary>
    /// HIFF implements the Hierarchical If-And-Only-If problem developed by Watson, Hornby and Pollack.
    /// See <a href="http://www.cs.brandeis.edu/~richardw/hiff.html">The HIFF Generator</a> for more information
    /// and papers.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>p</tt><br/>
    /// <font size="-1"/>int >= 0 </td>
    /// <td valign="top">(number of blocks at each level)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>k</tt><br/>
    /// <font size="-1"/>int >= 0 </td>
    /// <td valign="top">(number of hierarchical levels)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>rc</tt><br/>
    /// <font size="-1"/>double </td>
    /// <td valign="top">(ratio of block contributions)</td></tr>
    /// </table>
    /// </summary>
    [ECConfiguration("ec.problems.hiff.HIFF")]
    public class HIFFProblem : Problem, ISimpleProblem
    {

        public const string P_K = "k";
        public const string P_P = "p";
        public const string P_RC = "rc";

        int K, P, Rc;

        public void Setup(EvolutionState state, Parameter paramBase)
        {
            base.Setup(state, paramBase);

            K = state.Parameters.GetInt(paramBase.Push(P_K), null, 0);
            if (K < 0)
                state.Output.Fatal("k must be > 0", paramBase.Push(P_K));

            P = state.Parameters.GetInt(paramBase.Push(P_P), null, 0);
            if (P < 0)
                state.Output.Fatal("p must be > 0", paramBase.Push(P_P));

            Rc = state.Parameters.GetInt(paramBase.Push(P_RC), null, 0);
            if (Rc < 0)
                state.Output.Fatal("rc must be > 0", paramBase.Push(P_RC));
        }

        public void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            var ind2 = (BitVectorIndividual)ind;

            var genes = new double[ind2.genome.Length];
            for (var i = 0; i < genes.Length; i++)
                genes[i] = (ind2.genome[i] ? 1 : 0);
            var fitness = H(genes);

            ((SimpleFitness) ind2.Fitness).SetFitness(state, (float)fitness, false);
            ind2.Evaluated = true;
        }

        double H(double[] genes)
        {
            var bonus = 1.0;
            var F = 0.0;
            var last = genes.Length;

            for (var i = 0; i < last; i++)
                F += f(genes[i]);

            for (var i = 1; i <= P; i++)
            {
                last /= K;
                bonus *= Rc;
                for (var j = 0; j < last; j++)
                {
                    genes[j] = t(genes, j * K);
                    F += f(genes[j]) * bonus;
                }
            }

            return F;
        }

        double t(double[] transform, int first)
        {
            var s = 0;
            for (var i = first + 1; i < first + K; i++)
            {
                if (transform[first] == transform[i])
                    s++;
            }
            if (s == (K - 1)) return transform[first];

            return -1;
        }

        double f(double b)
        {
            return b != -1 ? 1 : 0;
        }
    }
}