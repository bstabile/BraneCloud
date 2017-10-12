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

namespace BraneCloud.Evolution.EC.Spatial
{
    /// <summary>      
    /// In a spatially-embedded EA, the subpops of individuals are assumed to be
    /// spatially distributed in some sort of space, be it one-dimmensional, two-
    /// dimmensional, or whatever else.  The space may or may not be toroidal (although
    /// it usually is).  Each location in the space has a set of neighboring locations.
    /// Thus, each individual has an index in the subpop, and also a location in
    /// the space.
    /// 
    /// This public interface provides a method to obtain the indexes of the neighbors
    /// of a location.
    /// </summary>
    [ECConfiguration("ec.spatial.ISpace")]
    public interface ISpace
    {
        /// <summary>
        /// Input: a threadnumber (either for evaluation or for breeding), and an index in a subpop
        /// (the index in the subpop is, of course, associated with a location in the space)
        /// Functionality: stores the index and the threadnumber for further accesses to the getIndexRandomNeighbor
        /// method.  All such accesses from the specific thread will use the exact same index, until
        /// this function is called again to change the index.
        /// </summary>
        void  SetIndex(int threadnum, int index);
        
        /// <summary>
        /// Functionality: retrieve the index for a specific threanum.
        /// Returns -1 if any error is encountered.
        /// </summary>
        int GetIndex(int threadnum);
        
        /// <summary>
        /// Input: the maximum distance for neighbors.
        /// Functionality: computes the location in space associated with the index, then
        /// computes the neighbors of that location that are within the specified distance.
        /// Output: returns one random neighbor within that distance (possibly including self)
        /// </summary>
        int GetIndexRandomNeighbor(IEvolutionState state, int threadnum, int distance);
    }
}