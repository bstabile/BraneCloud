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

namespace BraneCloud.Evolution.EC
{
    /// <summary> 
    /// IDefaults is the interface which describes how Defaults objects
    /// should work.  In general there is one Defaults class for each
    /// package (there doesn't have to be, but it would be nice).  This
    /// class should be relatively uniquely named (the defaults class in
    /// the GP package is called GPDefaults for example).
    /// IDefaults objects should implement a single static method:
    /// 
    /// <p/><tt>public Parameter base();</tt>
    /// 
    /// <p/>...which returns the default parameter base for the package.  This
    /// method cannot be declared in this interface, however, because it is
    /// static.  :-)  So this interface isn't much use, except to describe how
    /// defaults objects should generally work.
    /// 
    /// <p/> A parameter base is a secondary "default" place for the parameters database 
    /// to look for a parameter value if the primary value was not defined.
    /// </summary>
    [ECConfiguration("ec.IDefaults")]
    public interface IDefaults
    {
    }
}