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

namespace BraneCloud.Evolution.EC.GP
{
    /// <summary> 
    /// A GPBreedingPipeline is a BreedingPipeline which produces only
    /// members of some subclass of GPSpecies.   This is just a convenience
    /// superclass for many of the breeding pipelines here; you don't have
    /// to be a GPBreedingPipeline in order to breed GPSpecies or anything. 
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.GPBreedingPipeline")]
    public abstract class GPBreedingPipeline : BreedingPipeline
    {
        #region Constants

        /// <summary>
        /// Standard parameter for node-selectors associated with a GPBreedingPipeline 
        /// </summary>
        public const string P_NODESELECTOR = "ns";
        
        /// <summary>
        /// Standard parameter for tree fixing 
        /// </summary>
        public const string P_TREE = "tree";
        
        /// <summary>
        /// Standard value for an unfixed tree 
        /// </summary>
        public const int TREE_UNFIXED = -1;

        #endregion // Constants
        #region Operations

        /// <summary>
        /// Returns true if <i>s</i> is a GPSpecies. 
        /// </summary>
        public override bool Produces(IEvolutionState state, Population newpop, int subpop, int thread)
        {
            if (!base.Produces(state, newpop, subpop, thread))
                return false;

            // we produce individuals which are owned by subclasses of GPSpecies
            return newpop.Subpops[subpop].Species is GPSpecies;
        }

        #endregion // Operations
    }
}