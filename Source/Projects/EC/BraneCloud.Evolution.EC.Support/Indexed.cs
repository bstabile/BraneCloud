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

namespace BraneCloud.Evolution.EC.Support
{
    /// <summary>
    /// A simple interface (simpler than List) for accessing random-access objects without changing their size.  
    /// Adhered to by Bag, IntBag, and DoubleBag
    /// </summary>       
    public interface Indexed
    {
        /** Should return the base component type for this Indexed object, or
            null if the component type should be queried via getValue(index).getClass.getComponentType() */
        Type ComponentType { get; }

        int Size { get; }

        /** Throws an IndexOutOfBoundsException if index is inappropriate, and ArgumentException
            if the value is inappropriate.  Not called set() in order to be consistent with getValue(...)*/
        object SetValue(int index, object value);

        /** Throws an IndexOutOfBoundsException if index is inappropriate.  Not called get() because
            this would conflict with get() methods in IntBag etc. which don't return objects. */
        object GetValue(int index);
    }
}