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

namespace BraneCloud.Evolution.EC.Util
{
    interface INullOp<T>
    {
        bool HasValue(T value);
        bool AddIfNotNull(ref T accumulator, T value);
    }
    sealed class StructNullOp<T>
        : INullOp<T>, INullOp<T?>
        where T : struct
    {
        public bool HasValue(T value)
        {
            return true;
        }
        public bool AddIfNotNull(ref T accumulator, T value)
        {
            accumulator = Operator<T>.Add(accumulator, value);
            return true;
        }
        public bool HasValue(T? value)
        {
            return value.HasValue;
        }
        public bool AddIfNotNull(ref T? accumulator, T? value)
        {
            if (value.HasValue)
            {
                accumulator = accumulator.HasValue ?
                    Operator<T>.Add(
                        accumulator.GetValueOrDefault(),
                        value.GetValueOrDefault())
                    : value;
                return true;
            }
            return false;
        }
    }
    sealed class ClassNullOp<T>
        : INullOp<T>
        where T : class
    {
        public bool HasValue(T value)
        {
            return value != null;
        }
        public bool AddIfNotNull(ref T accumulator, T value)
        {
            if (value != null)
            {
                accumulator = accumulator == null ?
                    value : Operator<T>.Add(accumulator, value);
                return true;
            }
            return false;
        }
    }
}