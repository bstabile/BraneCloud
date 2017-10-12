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

namespace BraneCloud.Evolution.EC.MultiObjective
{
    [ECConfiguration("ec.multiobjective.MultiObjectiveDefaults")]
    public sealed class MultiObjectiveDefaults : IDefaults
    {
        public const string P_MULTI = "multi";

        /// <summary>
        /// Returns the default base. 
        /// </summary>
        public static IParameter ParamBase
        {
            get { return new Parameter(P_MULTI); }
        }
    }
}