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
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.App.Sat.Test
{
    /// <summary>
    /// SAT implements the boolean satisfiability problem. 
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>sat-filename</tt><br/>
    /// <font size="-1"/>String</td>
    /// <td valign="top">(Filename containing boolean satisfiability formula in Dimacs CNF format)</td></tr>
    /// </table>
    /// </summary>
    [ECConfiguration("ec.app.sat.SAT")]
    public class SAT : Problem, ISimpleProblem
    {

        public const string P_FILENAME = "sat-filename";

        Clause[] _formula;

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            var fileName = state.Parameters.GetString(paramBase.Push(P_FILENAME), null);

            try
            {
                var inFile = new StreamReader(fileName);
                var line = "";
                var cnt = 0;
                var start = false;
                while ((line = inFile.ReadLine()) != null)
                {
                    if (start)
                    {
                        _formula[cnt++] = new Clause(line);
                        continue;
                    }

                    if (line.StartsWith("p"))
                    {
                        start = true;
                        line = line.Trim();
                        var index = line.LastIndexOf(" ");
                        _formula = new Clause[Int32.Parse(line.Substring(index + 1))];
                    }
                }
                inFile.Close();
            }
            catch (IOException e)
            {
                state.Output.Fatal("Error in SAT Setup, while loading from file " + fileName +
                    "\nFrom parameter " + paramBase.Push(P_FILENAME) + "\nError:\n" + e);
            }
        }

        /// <summary>
        /// Evalutes the individual using the MAXSAT fitness function.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="ind"></param>
        /// <param name="subpop"></param>
        /// <param name="threadnum"></param>
        public void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            var ind2 = (BitVectorIndividual)ind;
            var fitness = 0.0;

            for (var i = 0; i < _formula.Length; i++)
                fitness += _formula[i].Eval(ind2);

            ((SimpleFitness)(ind2.Fitness)).SetFitness(state, (float)fitness, false);
            ind2.Evaluated = true;
        }


        /// <summary>
        /// Private helper class holding a single clause in the boolean formula. Each clause 
        /// is a disjunction of boolean variables (or their negation).
        /// </summary>
        public class Clause
        {
            readonly int[] _variables;
            public Clause(String c)
            {
                var st = new Tokenizer(c);
                _variables = new int[st.Count - 1];
                for (var i = 0; i < _variables.Length; i++)
                {
                    _variables[i] = Int32.Parse(st.NextToken());
                }
            }

            /// <summary>
            /// Evaluates the individual with the clause.  Returns 1 is clase is satisfiabile, 0 otherwise.
            /// </summary>
            /// <param name="ind"></param>
            /// <returns></returns>
            public int Eval(BitVectorIndividual ind)
            {
                for (var i = 0; i < _variables.Length; i++)
                {
                    var x = _variables[i];
                    bool tmp;
                    if (x < 0)
                        tmp = !ind.genome[-x - 1];
                    else
                        tmp = ind.genome[x - 1];

                    if (tmp) return 1;
                }
                return 0;
            }
        }
    }
}