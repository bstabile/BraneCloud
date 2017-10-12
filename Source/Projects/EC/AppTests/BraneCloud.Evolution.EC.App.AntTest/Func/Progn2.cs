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
using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.App.AntTest
{
    [ECConfiguration("ec.app.ant.func.Progn2")]
    public class Progn2 : GPNode, IEvalPrint
    {
        public override string ToString() { return "progn2"; }

        //public void CheckConstraints(IEvolutionState state, int tree, GPIndividual typicalIndividual, Parameter individualBase)
        //{
        //    base.CheckConstraints(state, tree, typicalIndividual, individualBase);
        //    if (Children.Length != 2)
        //        state.Output.Error("Incorrect number of children for node " +
        //            ToStringForError() + " at " +
        //            individualBase);
        //}

        public override int ExpectedChildren { get { return 2; } }

        public override void Eval(
            IEvolutionState state, 
            int thread, 
            GPData input, 
            ADFStack stack, 
            GPIndividual individual, 
            IProblem problem)
        {
            // Evaluate both children.  Easy as cake.
            Children[0].Eval(state, thread, input, stack, individual, problem);
            Children[1].Eval(state, thread, input, stack, individual, problem);
        }

        public void EvalPrint(
            IEvolutionState state, 
            int thread, 
            GPData input, 
            ADFStack stack, 
            GPIndividual individual, 
            IProblem problem, 
            int[][] map2)
        {
            // Evaluate both children.  Easy as cake.
            ((IEvalPrint)Children[0]).EvalPrint(state, thread, input, stack, individual, problem, map2);
            ((IEvalPrint)Children[1]).EvalPrint(state, thread, input, stack, individual, problem, map2);
        }
    }
}