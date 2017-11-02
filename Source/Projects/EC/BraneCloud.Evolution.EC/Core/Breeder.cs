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

namespace BraneCloud.Evolution.EC
{
    /// <summary> 
    /// A Breeder is a singleton object which is responsible for the breeding
    /// process during the course of an evolutionary run.  Only one Breeder
    /// is created in a run, and is stored in the EvolutionState object.
    /// 
    /// <p/>Breeders typically do their work by applying a Species' BreedingPipelines
    /// on subpops of that species to produce new individuals for those
    /// subpops.
    /// 
    /// <p/>Breeders may be multithreaded.  The number of threads they may spawn
    /// (excepting a parent "gathering" thread) is governed by the EvolutionState's
    /// breedthreads value.
    /// 
    /// <p/>Be careful about spawning threads -- this system has no few synchronized 
    /// methods for efficiency's sake, so you must either divvy up breeding in a
    /// thread-safe fashion and assume that all individuals
    /// in the current population are read-only (which you may assume for a generational
    /// breeder which needs to return a whole new population each generation), or
    /// otherwise you must obtain the appropriate locks on individuals in the population
    /// and other objects as necessary.
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.Breeder")]
    public abstract class Breeder : IBreeder, ISingleton
    {
        public abstract void Setup(IEvolutionState param1, IParameter param2);

        /// <summary>
        /// Breeds state.Population, returning a new population.  
        /// In general, state.Population should not be modified. 
        /// </summary>		
        public abstract Population BreedPopulation(IEvolutionState state);

        public virtual void BreedPopChunk(
            Population newpop, 
            IEvolutionState state, 
            int[] numinds, 
            int[] from,
            int threadnum)
        { /* Does nothing by default */ }
    }
}