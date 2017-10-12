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

namespace BraneCloud.Evolution.EC.GP
{
    /// <summary> 
    /// A GPAtomicType is a simple, atomic GPType.  For more information, see GPType.
    /// <seealso cref="BraneCloud.Evolution.EC.GP.GPType" />
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.GPAtomicType")]
    public sealed class GPAtomicType : GPType
    {
        #region Setup

        /// <summary>
        /// Use this constructor for GPAtomic Type unless you know what you're doing 
        /// </summary>
        public GPAtomicType(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Don't use this constructor unless you call Setup(...) immediately after it. 
        /// </summary>
        public GPAtomicType()
        {
        }

        #endregion // Setup
        #region Comparison

        public override bool CompatibleWith(GPInitializer initializer, GPType t)
        {
            // if the type is me, then I'm compatible with it
            if (t.Type == Type)
                return true;

            // if the type an atomic type, then return false
            if (t.Type < initializer.NumAtomicTypes)
                return false;

            // if the type is < 0 (it's a set type), then I'm compatible
            // if I'm contained in it.  Use its sparse array.
            return ((GPSetType)t).TypesSparse[Type];
        }
        
        #endregion // Comparison
    }
}