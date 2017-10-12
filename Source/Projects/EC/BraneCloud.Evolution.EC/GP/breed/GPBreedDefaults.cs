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

namespace BraneCloud.Evolution.EC.GP.Breed
{
    /// <summary> 
    /// A static class that returns the base for "default values" which various
    /// GP breeding
    /// operators use, rather than making the user specify them all on a per-
    /// species basis.
    /// </summary>
    [ECConfiguration("ec.gp.breed.GPBreedDefaults")]
    public sealed class GPBreedDefaults : IDefaults
    {
        public const string P_BREED = "breed";

        /// <summary>
        /// Returns the default base, which is built off of the GPDefaults base. 
        /// </summary>
        public static IParameter ParamBase
        {
            get { return GPDefaults.ParamBase.Push(P_BREED); }
        }
    }
}