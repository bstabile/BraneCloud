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
    /// Finisher is a singleton object which is responsible for cleaning up a
    /// population after a run has completed.  This is typically done after
    /// final statistics have been performed but before the exchanger's
    /// contacts have been closed.
    /// </summary>
    [ECConfiguration("ec.IFinisher")]
    public interface IFinisher : ISingleton
    {
        /// <summary>
        /// Cleans up the population after the run has completed. 
        /// result is either ec.EvolutionState.R_SUCCESS or ec.EvolutionState.R_FAILURE, 
        /// indicating whether or not an ideal individual was found. 
        /// </summary>
        void  FinishPopulation(IEvolutionState state, int result);
    }
}