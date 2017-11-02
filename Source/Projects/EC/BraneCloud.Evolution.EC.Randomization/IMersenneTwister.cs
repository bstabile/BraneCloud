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
using System.Runtime.Serialization;

namespace BraneCloud.Evolution.EC.Randomization
{
    public interface IMersenneTwister : ICloneable, ISerializable
    {
        bool StateEquals(IMersenneTwister o);
        void ReadState(BinaryReader reader);
        void WriteState(BinaryWriter writer);
        void SetSeed(long seed);
        void SetSeed(int[] array);

        bool NextBoolean();
        bool NextBoolean(float probability);
        bool NextBoolean(double probability);

        int Next(int n);
        int NextInt();
        int NextInt(int n);
        long NextLong();
        long NextLong(long n);

        T NextIntegral<T>() where T : struct, IConvertible, IComparable;

        double NextDouble();
        double NextDouble(bool includeZero, bool includeOne);
        float NextFloat();
        double NextFloat(bool includeZero, bool includeOne);
        void NextBytes(byte[] bytes);
        char NextChar();
        short NextShort();
        sbyte NextByte();
        double NextGaussian();

        void ClearGaussian();
    }
}