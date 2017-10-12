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
    /// Setup classes are classes which get set up once from user-supplied parameters
    /// prior to being used.
    /// 
    /// Defines a single method, Setup(...), which is called at least once for the
    /// object, or for some object from which it was cloned.  This method
    /// allows the object to set itself up by reading from parameter lists and
    /// files on-disk.  You may assume that this method is called in a non-threaded
    /// environment, hence your thread number is 0 (so you can determine which
    /// random number generator to use).
    /// </summary>
    [ECConfiguration("ec.ISetup")]
    public interface ISetup
    {
        /// <summary>
        /// Sets up the object by reading it from the parameters stored
        /// in <i>state</i>, built off of the parameter base <i>base</i>.
        /// If an ancestor implements this method, be sure to call
        /// super.Setup(state,base);  before you do anything else. 
        /// </summary>
        void Setup(IEvolutionState state, IParameter paramBase);
    }
}