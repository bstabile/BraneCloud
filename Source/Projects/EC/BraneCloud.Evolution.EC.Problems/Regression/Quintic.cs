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

namespace BraneCloud.Evolution.EC.Problems.Regression
{
    /// <summary>
    /// Quintic implements a Symbolic Regression problem.
    /// 
    /// <p/>The equation to be regressed is y = x^5 - 2x^3 + x, {x in [-1,1]}
    /// <p/>This equation was introduced in J. R. Koza, GP II, 1994.
    /// </summary>
    [ECConfiguration("ec.problems.regression.Quintic")]
    public class Quintic : Regression
    {
        public override double Func(double x)
        { return x * x * x * x * x - 2.0 * x * x * x + x; }
    }
}