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
    /// <inheritdoc />
    /// <summary> 
    /// ISimpleProblem is an interface which defines methods
    /// for Problems to implement simple, single-individual (non-coevolutionary)
    /// evaluation.
    /// </summary>

    [ECConfiguration("ec.simple.ISimpleProblem")]
    public interface ISimpleProblem : IProblem
    {
        /// <summary>
        /// Evaluates the individual in ind, if necessary (perhaps
        /// not evaluating them if their evaluated flags are true),
        /// and sets their fitness appropriately. 
        /// </summary>		
        void  Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum);
        
    }
}