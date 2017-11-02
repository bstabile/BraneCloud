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
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.GP.Koza;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.Problems.Multiplexer
{
    /// <summary>
    /// Multiplexer implements the family of <i>n</i>-Multiplexer problems.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>data</tt><br/>
    /// <font size="-1">classname, inherits or == ec.app.multiplexer.MultiplexerData</font></td>
    /// <td valign="top">(the class for the prototypical GPData object for the Multiplexer problem)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>bits</tt><br/>
    /// <font size="-1">1, 2, or 3</font></td>
    /// <td valign="top">(The number of address bits (1 == 3-multiplexer, 2 == 6-multiplexer, 3==11-multiplexer)</td></tr>
    /// </table>
    /// 
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>data</tt></td>
    /// <td>species (the GPData object)</td></tr>
    /// </table>
    /// </summary>
    [ECConfiguration("ec.problems.multiplexer.Multiplexer")]
    public class Multiplexer : GPProblem, ISimpleProblem
    {
        private const long SerialVersionUID = 1;

        public const int NUMINPUTS = 20;
        public const string P_NUMBITS = "bits";

        public int bits;  // number of bits in the data

        public override object Clone()
        {
            var myobj = (Multiplexer)base.Clone();
            myobj.Input = (MultiplexerData)Input.Clone();
            return myobj;
        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // very important, remember this
            base.Setup(state, paramBase);

            // not using any default base -- it's not safe

            // verify our input is the right class (or subclasses from it)
            if (!(Input is MultiplexerData))
            state.Output.Fatal("GPData class must subclass from " + typeof(MultiplexerData).Name,
            paramBase.Push(P_DATA), null);

            // I figure 3 bits is plenty -- otherwise we'd be dealing with
            // REALLY big arrays!
            bits = state.Parameters.GetIntWithMax(paramBase.Push(P_NUMBITS), null, 1, 3);
            if (bits < 1)
                state.Output.Fatal("The number of bits for Multiplexer must be between 1 and 3 inclusive");
        }


        public void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            ((MultiplexerData)Input).Status = (byte)bits;

            if (!ind.Evaluated)  // don't bother reevaluating
            {
                int sum = 0;

                ((GPIndividual)ind).Trees[0].Child.Eval(state, threadnum, Input, Stack, (GPIndividual)ind, this);

                if (bits == 1)
                {
                    byte item1 = ((MultiplexerData) Input).Dat3;
                    byte item2 = Fast.M_3[Fast.M_3_OUTPUT];
                    for (var y = 0; y < MultiplexerData.MULTI_3_BITLENGTH; y++)
                    {
                        // if the first bit matches, grab it as:
                        // sum += 1 and not(item1 xor item2)
                        // that is, if item1 and item2 are the SAME at bit 1
                        // then we increase
                        sum += (1 & ((item1 ^ item2) ^ (-1)));
                        // shift to the next bit
                        item1 = (byte)(item1 >> 1);
                        item2 = (byte)(item2 >> 1);
                    }
                }
                else if (bits == 2)
                {
                    long item1 = ((MultiplexerData)Input).Dat6;
                    long item2 = Fast.M_6[Fast.M_6_OUTPUT];
                    for (int y = 0; y < MultiplexerData.MULTI_6_BITLENGTH; y++)
                    {
                        // if the first bit matches, grab it
                        sum += (int)(1L & ((item1 ^ item2) ^ (-1L)));
                        // shift to the next bit
                        item1 = item1 >> 1;
                        item2 = item2 >> 1;
                    }
                }
                else // bits==3
                {
                    long item1, item2;
                    for (var y = 0; y < MultiplexerData.MULTI_11_NUM_BITSTRINGS; y++)
                    {
                        item1 = ((MultiplexerData)Input).Dat11[y];
                        item2 = Fast.M_11[Fast.M_11_OUTPUT][y];
                        //System.out.PrintLn("" + y + " ### " + item1 + " " + item2);
                        for (var z = 0; z < MultiplexerData.MULTI_11_BITLENGTH; z++)
                        {
                            // if the first bit matches, grab it
                            sum += (int)(1L & ((item1 ^ item2) ^ (-1L)));
                            // shift to the next bit
                            item1 = item1 >> 1;
                            item2 = item2 >> 1;
                        }
                    }
                }

                // the fitness better be KozaFitness!
                var f = ((KozaFitness)ind.Fitness);
                if (bits == 1)
                    f.SetStandardizedFitness(state, (Fast.M_3_SIZE - sum));
                else if (bits == 2)
                    f.SetStandardizedFitness(state, (Fast.M_6_SIZE - sum));
                else // (bits == 3)
                    f.SetStandardizedFitness(state, (Fast.M_11_SIZE - sum));
                f.Hits = sum;
                ind.Evaluated = true;
            }
        }
    }
}