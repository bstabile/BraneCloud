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
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.MultiObjective.NSGA2
{
    [Serializable]
    [ECConfiguration("ec.multiobjective.nsga2.NSGA2MultiObjectiveFitnessComparator")]
    public class NSGA2MultiObjectiveFitnessComparator : ISortComparator
    {
        public bool lt(Object a, Object b)
        {
            var i1 = (Individual)a;
            var i2 = (Individual)b;
            return (((NSGA2MultiObjectiveFitness)i1.Fitness).Sparsity > ((NSGA2MultiObjectiveFitness)i2.Fitness).Sparsity);
        }

        public bool gt(Object a, Object b)
        {
            var i1 = (Individual)a;
            var i2 = (Individual)b;
            return (((NSGA2MultiObjectiveFitness)i1.Fitness).Sparsity < ((NSGA2MultiObjectiveFitness)i2.Fitness).Sparsity);
        }
    }
}