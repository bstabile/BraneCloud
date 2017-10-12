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

namespace BraneCloud.Evolution.EC.GP
{
    /// <summary> 
    /// A GPSetType is a GPType which contains GPAtomicTypes in a set, and is used
    /// as a generic GP type.  For more information, see GPType
    /// 
    /// GPSetTypes implement their set using both a hash table and an array.
    /// if the size of the set is "significantly big", then the hash table
    /// is used to look up membership in the set (O(1), but with a big constant).
    /// If the size is small, then the array is used (O(n)).  The dividing line
    /// is determined by the constant START_USING_HASH_BEYOND, which you might
    /// play with to optimize for your system.
    /// <seealso cref="BraneCloud.Evolution.EC.GP.GPType"/>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.GPSetType")]
    public sealed class GPSetType : GPType
    {
        #region Constants

        public const string P_MEMBER = "member";
        public const string P_SIZE = "size";

        #endregion // Constants
        #region Properties

        /// <summary>
        /// A packed, sorted array of atomic types in the set 
        /// </summary>
        public int[] TypesPacked { get; set; }

        /// <summary>
        /// A sparse array of atomic types in the set 
        /// </summary>
        public bool[] TypesSparse { get; set; }

        /// <summary>
        /// The hashtable of types in the set 
        /// </summary>
        public Hashtable Types_h { get; set; }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// You should not construct new types. 
        /// </summary>
        public GPSetType()
        {
        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            // Make my Hashtable
            Types_h = Hashtable.Synchronized(new Hashtable());

            // How many atomic types do I have?
            var len = state.Parameters.GetInt(paramBase.Push(P_SIZE), null, 1);
            if (len <= 0)
                state.Output.Fatal("The number of atomic types in the GPSetType " + Name + " must be >= 1.", paramBase.Push(P_SIZE));

            // Load the GPAtomicTypes
            for (var x = 0; x < len; x++)
            {
                var s = state.Parameters.GetString(paramBase.Push(P_MEMBER).Push("" + x), null);
                if (s == null)
                    state.Output.Fatal("Atomic type member #" + x + " is not defined for the GPSetType "
                                        + Name + ".", paramBase.Push(P_MEMBER).Push("" + x));

                var t = TypeFor(s, state);
                if (!(t is GPAtomicType))
                    // uh oh
                    state.Output.Fatal("Atomic type member #" + x + " of GPSetType "
                                        + Name + " is not a GPAtomicType.", paramBase.Push(P_MEMBER).Push("" + x));

                if (Types_h[t] != null)
                    state.Output.Warning("Atomic type member #" + x + " is included more than once in GPSetType "
                                        + Name + ".", paramBase.Push(P_MEMBER).Push("" + x));
                Types_h[t] = t;
            }
        }

        /// <summary>
        /// Sets up the packed and sparse arrays based on the hashtable 
        /// </summary>
        public void PostProcessSetType(int totalAtomicTypes)
        {
            // load the hashtable into the arrays
            var x = 0;
            TypesPacked = new int[Types_h.Count];
            TypesSparse = new bool[totalAtomicTypes];

            var e = Types_h.Values.GetEnumerator();

            while (e.MoveNext())
            {
                var t = (GPAtomicType)(e.Current);
                TypesPacked[x++] = t.Type;
                TypesSparse[t.Type] = true;
            }

            // Sort the packed array
            Array.Sort(TypesPacked);
        }

        #endregion // Setup
        #region Comparison

        public override bool CompatibleWith(GPInitializer initializer, GPType t)
        {
            // if the type is me, then I'm compatible with it.
            if (t.Type == Type)
                return true;

            // if the type is an atomic type, then I'm compatible with it if I contain it.
            // Use the sparse array.
            if (t.Type < initializer.NumAtomicTypes)
                // atomic type, faster than doing instanceof
                return TypesSparse[t.Type];

            // else the type is a set type.  I'm compatible with it if we contain
            // an atomic type in common.   Use the sorted packed array.

            var s = (GPSetType)t;
            var x = 0;
            var y = 0;
            for (; x < TypesPacked.Length && y < s.TypesPacked.Length; )
            {
                if (TypesPacked[x] == s.TypesPacked[y])
                    return true;
                if (TypesPacked[x] < s.TypesPacked[y])
                    x++;
                else
                    y++;
            }
            return false;
        }

        #endregion // Comparison
    }
}