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
using System.Collections;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.App.NK
{
    /// <summary>
    /// NK implmements the NK-landscape developed by Stuart Kauffman (in the book <i>The Origins of
    /// Order: Self-Organization and Selection in Evolution</i>).  In the NK model, the fitness 
    /// contribution of each allele depends on how that allele interacts with K other alleles.  Based on 
    /// this interaction, each gene contributes a random number between 0 and 1.  The individual's 
    /// fitness is the average of these N random numbers.  
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>k</tt><br/>
    /// <font size="-1"/>int >= 0 && &lt; 31</td>
    /// <td valign="top">(number of interacting alleles)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>adjacent</tt><br/> 
    /// <font size="-1">boolean</font></td>
    /// <td valign="top">(should interacting alleles be adjacent to the given allele)</td></tr> 
    /// </table>
    /// </summary>
    [ECConfiguration("ec.app.nk.NK")]
    public class NK : Problem, ISimpleProblem
    {
        public const string P_N = "n";
        public const string P_K = "k";
        public const string P_ADJACENT = "adjacent";

        int _k;
        bool _adjacentNeighborhoods;
        Hashtable _oldValues;

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            _k = state.Parameters.GetInt(paramBase.Push(P_K), null, 1);
            if ((_k < 0) || (_k > 31))
                state.Output.Fatal("Value of k must be between 0 and 31", paramBase.Push(P_K));

            _adjacentNeighborhoods = state.Parameters.GetBoolean(paramBase.Push(P_ADJACENT), null, true);
            _oldValues = new Hashtable();
        }

        public void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            var ind2 = (BitVectorIndividual)ind;
            double fitness = 0;
            var n = ind2.genome.Length;

            for (var i = 0; i < n; i++)
            {
                var tmpInd = new bool[_k + 1];
                tmpInd[0] = ind2.genome[i];

                double val;
                if (_adjacentNeighborhoods)
                {
                    var offset = n - _k / 2;
                    for (int j = 0; j < _k; j++)
                    {
                        tmpInd[j + 1] = ind2.genome[(j + i + offset) % n];
                    }
                }
                else
                {
                    int j;
                    for (var l = 0; l < _k; l++)
                    {
                        while ((j = state.Random[0].NextInt(_k)) == i) ;
                        tmpInd[l + 1] = ind2.genome[j];
                    }
                }

                if (_oldValues.ContainsKey(tmpInd))
                    val = (Double)_oldValues[tmpInd];
                else
                {
                    double tmp = 0;
                    for (var j = 0; j < tmpInd.Length; j++)
                        if (tmpInd[j]) tmp += 1 << j;
                    val = tmp / Int32.MaxValue;

                    _oldValues[tmpInd] = val;
                }

                fitness += val;
            }

            fitness /= n;
            ((SimpleFitness)ind2.Fitness).SetFitness(state, fitness, false);
            ind2.Evaluated = true;
        }
    }
}