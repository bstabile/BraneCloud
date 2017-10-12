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
    /// An ADM is an ADF which doesn't evaluate its arguments beforehand, but
    /// instead only evaluates them (and possibly repeatedly) when necessary
    /// at runtime.
    /// <seealso cref="BraneCloud.Evolution.EC.GP.ADF" />
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.ADM")]
    public class ADM : ADF
    {
        #region Operations

        public override void Eval(IEvolutionState state, int thread, GPData input, ADFStack stack, GPIndividual individual, IProblem problem)
        {
            // prepare a context
            var c = stack.Push(stack.Take());
            c.PrepareADM(this);
            
            // evaluate the top of the associatedTree
            individual.Trees[AssociatedTree].Child.Eval(state, thread, input, stack, individual, problem);
            
            // pop the context off, and we're done!
            if (stack.Pop(1) != 1)
                state.Output.Fatal("Stack prematurely empty for " + ToStringForError());
        }

        #endregion // Operations
    }
}