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

namespace BraneCloud.Evolution.EC.Problems.Parity
{
    /// <summary>
    /// Parity implements the family of <i>n</i>-[even|odd]-Parity problems up 
    /// to 32-parity.  Read the README file in this package for information on
    /// how to set up the parameter files to your liking -- it's a big family.
    /// 
    /// <p/>The Parity family evolves a boolean function on <i>n</i> sets of bits,
    /// which returns true if the number of 1's is even (for even-parity) or odd
    /// (for odd-parity), false otherwise. 
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>data</tt><br/>
    /// <font size="-1">classname, inherits or == ec.app.parity.ParityData</font></td>
    /// <td valign="top">(the class for the prototypical GPData object for the Parity problem)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>even</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> (default) or <tt>false</tt></font></td>
    /// <td valign="top">(is this even-parity (as opposed to odd-parity)?)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>bits</tt><br/>
    /// <font size="-1"> 2 &gt;= int &lt;= 31</font></td>
    /// <td valign="top">(The number of data bits)</td></tr>
    /// </table>
    /// 
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>data</tt></td>
    /// <td>species (the GPData object)</td></tr>
    /// </table>
    /// </summary>
    [ECConfiguration("ec.problems.parity.Parity")]
    public class ParityProblem : GPProblem, ISimpleProblem
    {
        public const string P_NUMBITS = "bits";
        public const string P_EVEN = "even";

        public bool doEven;
        public int numBits;
        public int totalSize;

        public int bits;  // data bits

        // we'll need to deep clone this one though.
        public ParityData input;

        public override object Clone()
        {
            var myobj = (ParityProblem)base.Clone();
            myobj.input = (ParityData)input.Clone();
            return myobj;
        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // very important, remember this
            base.Setup(state, paramBase);

            // not using a default base here

            // can't use all 32 bits -- Java is signed.  Must use 31 bits.

            numBits = state.Parameters.GetIntWithMax(paramBase.Push(P_NUMBITS), null, 2, 31);
            if (numBits < 2)
                state.Output.Fatal("The number of bits for Parity must be between 2 and 31 inclusive", paramBase.Push(P_NUMBITS));

            totalSize = 1;
            for (var x = 0; x < numBits; x++)
                totalSize *= 2;   // safer than Math.pow()

            doEven = state.Parameters.GetBoolean(paramBase.Push(P_EVEN), null, true);

            // set up our input
            input = (ParityData)state.Parameters.GetInstanceForParameterEq(paramBase.Push(P_DATA), null, typeof(ParityData));
            input.Setup(state, paramBase.Push(P_DATA));
        }

        public void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            if (!ind.Evaluated)  // don't bother reevaluating
            {
                var sum = 0;

                for (bits = 0; bits < totalSize; bits++)
                {
                    var tb = 0;
                    // first, is #bits even or odd?
                    for (var b = 0; b < numBits; b++)
                        tb += (bits >> b) & 1;
                    tb &= 1;  // now tb is 1 if we're odd, 0 if we're even

                    ((GPIndividual)ind).Trees[0].Child.Eval(state, threadnum, input, Stack, (GPIndividual)ind, this);

                    if ((doEven && ((input.x & 1) != tb)) ||
                        ((!doEven) && ((input.x & 1) == tb)))
                        sum++;
                }

                // the fitness better be KozaFitness!
                var f = ((KozaFitness)ind.Fitness);
                f.SetStandardizedFitness(state, totalSize - sum);
                f.Hits = sum;
                ind.Evaluated = true;
            }
        }
    }
}