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

namespace BraneCloud.Evolution.EC
{
    
    /// <summary> 
    /// Groups are used for populations and subpops.  They are slightly
    /// different from IPrototypes in a few important ways.
    /// 
    /// A Group instance typically is set up with Setup(...) and then <i>used</i>
    /// (unlike in a IPrototype, where the prototype instance is never used, 
    /// but only makes clones which are used).  
    /// When a new Group instance is needed, it is created by
    /// calling emptyClone() on a previous Group instance, which returns a
    /// new instance set up exactly like the first Group instance had been set up
    /// when Setup(...) was called on it.
    /// 
    /// Groups are Serializable and Cloneable, but you should not clone
    /// them -- use emptyClone instead.
    /// </summary>
    [ECConfiguration("ec.IGroup")]
    public interface IGroup : ISetup, ICloneable
    {
        /// <summary>
        /// Returns a copy of the object just as it had been 
        /// immediately after Setup was called on it (or on
        /// an ancestor object).  You can obtain a fresh instance
        /// using clone(), and then modify that.
        /// </summary>
        /// <remarks> 
        /// This could be replaced by the ICloneable semantics if clients are changed appropriately.
        /// </remarks>
        IGroup EmptyClone();
    }
}