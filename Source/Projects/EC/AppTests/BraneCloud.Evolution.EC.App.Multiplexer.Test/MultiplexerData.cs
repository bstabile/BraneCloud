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
using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.App.Multiplexer.Test
{
    /// <summary>
    /// This is ugly and complicated because it needs to hold a variety
    /// of different-length bitstrings, including temporary ones held
    /// while computing subtrees.
    /// </summary>
    [ECConfiguration("ec.app.multiplexer.MultiplexerData")]
    public class MultiplexerData : GPData
    {
        /// <summary>
        /// A stack of available long arrays for popDat11/pushDat11
        /// </summary>
        public Stack Tmp;

        /// <summary>
        /// The number of Dn in Multiplexer-3
        /// </summary>
        public const byte STATUS_3 = 1;

        /// <summary>
        /// The number of Dn in Multiplexer-6
        /// </summary>
        public const byte STATUS_6 = 2;

        /// <summary>
        /// The number of Dn in Multiplexer-11
        /// </summary>
        public const byte STATUS_11 = 3;

        /// <summary>
        /// The length of an atomic data element in Multiplexer-3 (a byte)
        /// </summary>
        public const int MULTI_3_BITLENGTH = 8;

        /// <summary>
        /// The length of an atomic data element in Multiplexer-6 (a long)
        /// </summary>
        public const int MULTI_6_BITLENGTH = 64;

        /// <summary>
        /// The length of an atomic data element in Multiplexer-11 (a long)
        /// </summary>
        public const int MULTI_11_BITLENGTH = 64;

        /// <summary>
        /// The number of atomic elements in Multiplexer-11 comprising one string (32)
        /// </summary>
        public const int MULTI_11_NUM_BITSTRINGS = 32;

        /// <summary>
        /// An array of 32 longs for Multiplexer-11 data
        /// </summary>
        public long[] Dat11;

        /// <summary>
        /// A long for Multiplexer-6 data
        /// </summary>
        public long Dat6;

        /// <summary>
        /// A byte for Multiplexer-3 data
        /// </summary>
        public byte Dat3;

        /// <summary>
        /// A byte indicating the number of Dn in this problem
        /// </summary>
        public byte Status;

        /// <summary>
        /// Pops a dat_11 off of the stack; if the stack is empty, creates a new dat_11 and returns that.
        /// </summary>
        /// <returns></returns>
        public long[] PopDat11()
        {
            if (Tmp.Count == 0)
                return new long[MULTI_11_NUM_BITSTRINGS];
            else return (long[])(Tmp.Pop());
        }

        /// <summary>
        /// Pushes a dat_11 onto the stack
        /// </summary>
        /// <param name="l"></param>
        public void PushDat11(long[] l)
        {
            Tmp.Push(l);
        }

        public MultiplexerData()
        {
            Dat11 = new long[MULTI_11_NUM_BITSTRINGS];
            Tmp = new Stack();
        }

        public override object Clone()
        {
            var dat = (MultiplexerData)(base.Clone());
            dat.Dat11 = new long[MULTI_11_NUM_BITSTRINGS];
            Array.Copy(Dat11, 0, dat.Dat11, 0, MULTI_11_NUM_BITSTRINGS);
            dat.Tmp = new Stack();
            return dat;
        }

        public override void CopyTo(GPData gpd)
        {
            var md = ((MultiplexerData)gpd);
            for (var x = 0; x < MULTI_11_NUM_BITSTRINGS; x++)
                md.Dat11[x] = Dat11[x];
            md.Dat6 = Dat6;
            md.Status = Status;
        }
    }
}