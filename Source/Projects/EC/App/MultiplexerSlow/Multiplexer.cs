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

namespace BraneCloud.Evolution.EC.App.MultiplexerSlow
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
    [ECConfiguration("ec.app.multiplexerslow.Multiplexer")]
    public class Multiplexer : GPProblem, ISimpleProblem
    {
        public const int NUMINPUTS = 20;
        public const string P_NUMBITS = "bits";

        public int bits;  // number of bits in the data
        public int amax; // maximum address value
        public int dmax; // maximum data value
        public int addressPart;  // the current address part
        public int dataPart;     // the current data part

        // we'll need to deep clone this one though.
        public MultiplexerData input;

        public override object Clone()
        {
            var myobj = (Multiplexer)(base.Clone());
            myobj.input = (MultiplexerData)(input.Clone());
            return myobj;
        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // very important, remember this
            base.Setup(state, paramBase);

            // not using any default base -- it's not safe

            // I figure 3 bits is plenty -- otherwise we'd be dealing with
            // REALLY big arrays!
            bits = state.Parameters.GetIntWithMax(paramBase.Push(P_NUMBITS), null, 1, 3);
            if (bits < 1)
                state.Output.Fatal("The number of bits for Multiplexer must be between 1 and 3 inclusive");

            amax = 1;
            for (var x = 0; x < bits; x++) amax *= 2;   // safer than Math.pow(...)

            dmax = 1;
            for (var x = 0; x < amax; x++) dmax *= 2;   // safer than Math.pow(...)

            // set up our input
            input = (MultiplexerData)state.Parameters.GetInstanceForParameterEq(
                paramBase.Push(P_DATA), null, typeof(MultiplexerData));
            input.Setup(state, paramBase.Push(P_DATA));
        }

        public void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            if (!ind.Evaluated)  // don't bother reevaluating
            {
                var sum = 0;

                for (addressPart = 0; addressPart < amax; addressPart++)
                    for (dataPart = 0; dataPart < dmax; dataPart++)
                    {
                        ((GPIndividual)ind).Trees[0].Child.Eval(
                            state, threadnum, input, Stack, ((GPIndividual)ind), this);
                        sum += 1 - (                  /* "Not" */
                            ((dataPart >> addressPart) & 1) /* extracts the address-th 
                                                            bit in data and moves 
                                                            it to position 0, 
                                                            clearing out all 
                                                            other bits */
                            ^                   /* "Is Different from" */
                            (input.x & 1));      /* A 1 if input.x is 
                                                non-zero, else 0. */
                    }

                // the fitness better be KozaFitness!
                var f = (KozaFitness)ind.Fitness;
                f.SetStandardizedFitness(state, (amax * dmax - sum));
                f.Hits = sum;
                ind.Evaluated = true;
            }
        }
    }
}