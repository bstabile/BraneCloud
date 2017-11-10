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
    /// IClique is a class pattern marking classes which 
    /// create only a few instances, generally accessible through
    /// some global mechanism, and every single
    /// one of which gets its own distinct Setup(...) call.  ICliques should
    /// <b>not</b> be Cloneable, but they are Serializable.
    /// 
    /// <p/>All ICliques presently in ECJ rely on a central repository which
    /// stores members of that IClique for easy access by various objects.
    /// This repository typically includes a hashtable of the IClique members,
    /// plus perhaps one or more arrays of the members stored in different
    /// fashions.  Originally these central repositories were stored as static
    /// members of the IClique class; but as of ECJ 13 they have been moved
    /// to be instance variables of certain Initializers.  For example,
    /// GPInitializer holds the repositories for the GPFunctionSet, GPType,
    /// GPNodeConstraints, and GPTreeConstraints cliques.  Likewise,
    /// RuleInitializer holds the repository for the RuleConstraints clique.
    /// 
    /// <p/>This change was made to facilitate making ECJ modular; we had to remove
    /// all non-static members.  If you make your own IClique, its repository
    /// (if you have one) doesn't have to be in an Initializer, but it's a 
    /// convenient location.
    /// </summary>
    [ECConfiguration("ec.IClique")]
    public interface IClique : ISetup
    {
    }
}