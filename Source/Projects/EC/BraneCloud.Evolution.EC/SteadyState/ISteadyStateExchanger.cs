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

namespace BraneCloud.Evolution.EC.SteadyState
{
    /// <summary> 
    /// The ISteadyStateExchanger is a badge which Exchanger subclasses
    /// may wear if they work properly with the SteadyStateEvolutionState
    /// mechanism.  The basic thing such classes must remember to do is:
    /// Remember to call state.Breeder.IndividualsReplaced(...) if
    /// you modify or replace any individuals in a subpop.  Also,
    /// realize that any individuals you exchange in will not be checked
    /// to see if they're the ideal individual
    /// </summary>
    [ECConfiguration("ec.steadystate.ISteadyStateExchanger")]
    public interface ISteadyStateExchanger
    {
    }
}