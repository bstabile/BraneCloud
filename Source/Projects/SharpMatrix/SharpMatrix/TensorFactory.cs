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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace SharpMatrix
{
    /// <summary>
    /// This is to overcome .NET's lack of Java's jagged array initialization.
    /// It should be possible to convert the associated code to use "[,,,]" style.
    /// But that will require checking how the tensors are used on an individual basis.
    /// </summary>
    public static class TensorFactory
    {
        public static T[][] Create<T>(long dim1, long dim2)
        {
            var arr = new T[dim1][];
            for (var x = 0; x < dim1; x++)
            {
                arr[x] = new T[dim2];
            }
            return arr;
        }

        public static T[][][] Create<T>(long dim1, long dim2, long dim3)
        {
            var arr = new T[dim1][][];
            for (var x = 0; x < dim1; x++)
            {
                arr[x] = new T[dim2][];
                for (var y = 0; y < dim2; y++)
                    arr[x][y] = new T[dim3];
            }
            return arr;
        }

        public static T[][][][] CreateOpenEnded<T>(long dim1, long dim2, long dim3)
        {
            var arr = new T[dim1][][][];
            for (var w = 0; w < dim1; w++)
            {
                arr[w] = new T[dim2][][];
                for (var x = 0; x < dim2; x++)
                    arr[w][x] = new T[dim3][];
            }
            return arr;
        }

        public static T[][][][][] Create<T>(long dim1, long dim2, long dim3, long dim4, long dim5)
        {
            var arr = new T[dim1][][][][];
            for (var w = 0; w < dim1; w++)
            {
                arr[w] = new T[dim2][][][];
                for (var x = 0; x < dim2; x++)
                {
                    arr[w][x] = new T[dim3][][];
                    for (var y = 0; y < dim3; y++)
                    {
                        arr[w][x][y] = new T[dim4][];
                        for (var z = 0; z < dim4; z++)
                        {
                            arr[w][x][y][z] = new T[dim5];
                        }
                    }
                }
            }
            return arr;
        }

        public static T[][][][][] CreateOpenEnded<T>(long dim1, long dim2, long dim3, long dim4)
        {
            var arr = new T[dim1][][][][];
            for (var w = 0; w < dim1; w++)
            {
                arr[w] = new T[dim2][][][];
                for (var x = 0; x < dim2; x++)
                {
                    arr[w][x] = new T[dim3][][];
                    for (var y = 0; y < dim3; y++)
                    {
                        arr[w][x][y] = new T[dim4][];
                    }
                }
            }
            return arr;
        }
    }
}