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

namespace BraneCloud.Evolution.EC.SteadyState
{
    /// <summary>
    /// A simple class which contains both an Individual and the Queue it's located in.
    /// Used by SteadyState and by various assistant functions in the distributed evaluator
    /// to provide individuals to SteadyState
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.steadystate.QueueIndividual")]
    public class QueueIndividual
    {
        #region Properties

        public Individual Ind { get; set; }
        public int Subpop { get; set; }

        #endregion // Properties
        #region Setup

        public QueueIndividual(Individual i, int s)
        {
            Ind = i;
            Subpop = s;
        }

        #endregion // Setup
    }
    
}