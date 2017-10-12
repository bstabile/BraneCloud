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

namespace BraneCloud.Evolution.EC.App.Multiplexer.Test
{
    [ECConfiguration("ec.app.multiplexer.Fast")]
    public class Fast
    {
        /* 3-Multiplexer has 3 boolean variables (A0, D0, D1) */
        public const int M_3_OUTPUT = 3;
        /* 3-Multiplexer has 8 permutations of its 3 boolean variables */
        public const int M_3_SIZE = 8;
        /* 3-Multiplexer bitfield values for the 3 boolean variables and 1 output variable, stored as bytes (8 bits used) */
        public static readonly byte[/*4*/] M_3 = new byte[] { 85, 51, 15, 39 };
        /* 3-Multiplexer names for the 3 boolean variables and 1 output variable */
        public static readonly string[/*4*/] M_3_NAMES = { "A0", "D0", "D1", "Output" };

        /* 6-Multiplexer has 6 boolean variables (A0, A1, D0, D1, D2, D3) */
        public const int M_6_OUTPUT = 6;
        /* 6-Multiplexer has 64 permutations of its 6 boolean variables */
        public const int M_6_SIZE = 64;
        /* 6-Multiplexer bitfield values for the 6 boolean variables and 1 output variable, stored as longs (64 bits used) */
        public static readonly long[/*7*/] M_6 = { 6148914691236517205L, 3689348814741910323L, 1085102592571150095L, 71777214294589695L, 281470681808895L, 4294967295L, 597899502893742975L };
        /* 6-Multiplexer names for the 6 boolean variables and 1 output variable */
        public static readonly string[/*7*/] M_6_NAMES = { "A0", "A1", "D0", "D1", "D2", "D3", "Output" };

        /* 11-Multiplexer has 11 boolean variables (A0, A1, A2, D0, D1, D2, D3, D4, D5, D6, D7) */
        public const int M_11_OUTPUT = 11;
        /* 11-Multiplexer has 2048 permutations of its 11 boolean variables */
        public const int M_11_SIZE = 2048;
        /* 11-Multiplexer bitfield values for the 11 boolean variables and 1 output variable, where each of the 12 variable slots is an array of 32 longs (32 x 64 bits = 2048) which together comprise the big long 2048-bit permutation vector for that variable. */
        public static readonly long[/*12*/][/*32*/] M_11 = new long[12][]
        {
            new long[] { 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L, 6148914691236517205L },        
            new long[] { 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L, 3689348814741910323L },    
            new long[] { 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L, 1085102592571150095L },        
            new long[] { 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L, 71777214294589695L }, 
            new long[] { 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L, 281470681808895L },
            new long[] { 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L, 4294967295L },
            new long[] { 0L, -1L, 0L, -1L, 0L, -1L, 0L, -1L, 0L, -1L, 0L, -1L, 0L, -1L, 0L, -1L, 0L, -1L, 0L, -1L, 0L, -1L, 0L, -1L, 0L, -1L, 0L, -1L, 0L, -1L, 0L, -1L },
            new long[] { 0L, 0L, -1L, -1L, 0L, 0L, -1L, -1L, 0L, 0L, -1L, -1L, 0L, 0L, -1L, -1L, 0L, 0L, -1L, -1L, 0L, 0L, -1L, -1L, 0L, 0L, -1L, -1L, 0L, 0L, -1L, -1L },    
            new long[] { 0L, 0L, 0L, 0L, -1L, -1L, -1L, -1L, 0L, 0L, 0L, 0L, -1L, -1L, -1L, -1L, 0L, 0L, 0L, 0L, -1L, -1L, -1L, -1L, 0L, 0L, 0L, 0L, -1L, -1L, -1L, -1L },       
            new long[] { 0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L, -1L, -1L, -1L, -1L, -1L, -1L, -1L, -1L, 0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L, -1L, -1L, -1L, -1L, -1L, -1L, -1L, -1L },
            new long[] { 0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L, -1L, -1L, -1L, -1L, -1L, -1L, -1L, -1L, -1L, -1L, -1L, -1L, -1L, -1L, -1L, -1L },
            new long[] { 36099990944243936L, 1193542756353470704L, 614821373648857320L, 1772264139058084088L, 325460682296550628L, 1482903447705777396L, 904182065001164012L, 2061624830410390780L, 180780336620397282L, 1338223102029624050L, 759501719325010666L, 1916944484734237434L, 470141027972703974L, 1627583793381930742L, 1048862410677317358L, 2206305176086544126L, 108440163782320609L, 1265882929191547377L, 687161546486933993L, 1844604311896160761L, 397800855134627301L, 1555243620543854069L, 976522237839240685L, 2133965003248467453L, 253120509458473955L, 1410563274867700723L, 831841892163087339L, 1989284657572314107L, 542481200810780647L, 1699923966220007415L, 1121202583515394031L, 2278645348924620799L }
        };

        /* 11-Multiplexer names for the 11 boolean variables and 1 output variable */
        public static readonly string[/*12*/] M_11_NAMES = { "A0", "A1", "A2", "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "Output" };
    }
}