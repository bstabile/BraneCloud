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

namespace BraneCloud.Evolution.EC.Simple
{
    /// <summary> 
    /// SimpleProblemForm is an interface which defines methods
    /// for Problems to implement simple, single-individual (non-coevolutionary)
    /// evaluation.
    /// </summary>

    [ECConfiguration("ec.simple.ISimpleProblem")]
    public interface ISimpleProblem
    {
        /// <summary>
        /// Evaluates the individual in ind, if necessary (perhaps
        /// not evaluating them if their evaluated flags are true),
        /// and sets their fitness appropriately. 
        /// </summary>		
        void  Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum);
        
        /// <summary>
        /// "Reevaluates" an individual,
        /// for the purpose of printing out
        /// interesting facts about the individual in the context of the
        /// Problem, and logs the results.  This might be called to print out 
        /// facts about the best individual in the population, for example. 
        /// </summary>
        void Describe(IEvolutionState state, Individual ind, int subpop, int threadnum, int log);
    }
}