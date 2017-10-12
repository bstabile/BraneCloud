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
using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.App.Parity.Test
{
    [ECConfiguration("ec.app.parity.ParityData")]
    public class ParityData : GPData
    {
        // return value -- should ALWAYS be either 1 or 0
        public int x;

        public override void CopyTo(GPData gpd)
        { ((ParityData)gpd).x = x; }
    }
}