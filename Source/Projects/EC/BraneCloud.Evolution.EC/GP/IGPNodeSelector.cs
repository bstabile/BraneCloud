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

namespace BraneCloud.Evolution.EC.GP
{
    /// <summary> 
    /// GPNodeSelector is a IPrototype which describes algorithms which
    /// select random nodes out of trees, typically marking them for
    /// mutation, crossover, or whatnot.  GPNodeSelectors can cache information
    /// about a tree, as they may receive the pickNode(...) method more than
    /// once on a tree.  But this should really only be done if it can be
    /// done relatively efficiently; it's not all that common.  A GPNodeSelector
    /// will be called Reset() just before it is pressed into service in
    /// selecting nodes from a new tree, which gives it the chance to
    /// reset caches, etc.
    /// </summary>
    [ECConfiguration("ec.gp.IGPNodeSelector")]
    public interface IGPNodeSelector : IPrototype
    {
        /// <summary>
        /// Picks a node at random from tree and returns it.   
        /// The tree is located in ind, which is located in s.Population[subpop].
        /// This method will be preceded with a call to Reset();
        /// afterwards, pickNode(...) may be called several times for the same tree.
        /// </summary>	
        GPNode PickNode(IEvolutionState s, int subpop, int thread, GPIndividual ind, GPTree tree);
        
        /// <summary>
        /// Resets the Node Selector before a new series of pickNode() if need be. 
        /// </summary>
        void  Reset();
    }
}