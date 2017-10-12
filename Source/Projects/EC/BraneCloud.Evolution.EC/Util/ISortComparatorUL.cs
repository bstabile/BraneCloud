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
using System.Text;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Util
{
    /// <summary> 
    /// The interface for unsigned integral comparators passed to ec.util.QuickSort
    /// </summary>
    [ECConfiguration("ec.util.ISortComparatorUL")]
    public interface ISortComparatorUL
    {
        /// <summary>Returns true if a < b, else false </summary>
        bool lt(ulong a, ulong b);

        /// <summary>Returns true if a > b, else false </summary>
        bool gt(ulong a, ulong b);
    }
}