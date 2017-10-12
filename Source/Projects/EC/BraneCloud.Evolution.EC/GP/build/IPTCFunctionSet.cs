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

namespace BraneCloud.Evolution.EC.GP.Build
{
    /// <summary> 
    /// IPTCFunctionSet defines the methods that the PTC1 and PTC2 tree-creation
    /// algorithms require of function sets.  Your GPFunctionSet must adhere to
    /// this form in order to be used by these algorithms; the PTCFunctionSet
    /// class is provided to simplify matters for you (it's a direct subclass of
    /// GPFunctionSet which adheres to this form).
    /// </summary>
    [ECConfiguration("ec.gp.build.IPTCFunctionSet")]
    public interface IPTCFunctionSet
    {
        /// <summary>
        /// Returns an organized distribution (see ec.util.RandomChoice) of likelihoods
        /// that various terminals in the function set will be chosen over other terminals
        /// with the same return type.  The ordering of the array is the same as
        /// the terminals[type][...] array in GPFunctionSet.  
        /// </summary>
        float[] TerminalProbabilities(int type);
        
        /// <summary>
        /// Returns an organized distribution (see ec.util.RandomChoice) of likelihoods
        /// that various nonterminals in the function set will be chosen over other nonterminals
        /// with the same return type. The ordering of the array is the same as
        /// the nonterminals[type][...] array in GPFunctionSet. 
        /// </summary>
        float[] NonterminalProbabilities(int type);
        
        /// <summary>
        /// Returns an array (by return type) of the probability that PTC1 must pick a
        /// nonterminal over a terminal in order to guarantee the expectedTreeSize.
        /// Only used by PTC1, not by PTC2. 
        /// </summary>
        float[] NonterminalSelectionProbabilities(int expectedTreeSize);
    }
}