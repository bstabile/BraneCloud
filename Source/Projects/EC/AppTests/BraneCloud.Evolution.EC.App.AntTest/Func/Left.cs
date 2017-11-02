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

namespace BraneCloud.Evolution.EC.App.AntTest.Func
{
    [ECConfiguration("ec.app.ant.func.Left")]
    public class Left : GPNode, IEvalPrint
    {
        public override string ToString() { return "left"; }

        /*        public override void CheckConstraints(
                    IEvolutionState state, 
                    int tree, 
                    GPIndividual typicalIndividual, 
                    IParameter individualBase)
                {
                    base.CheckConstraints(state, tree, typicalIndividual, individualBase);
                    if (Children.Length != 0)
                        state.Output.Error("Incorrect number of children for node " +
                            ToStringForError() + " at " +
                            individualBase);
                }*/

        public override int ExpectedChildren => 0;

        public override void Eval(
            IEvolutionState state, 
            int thread, 
            GPData input, 
            ADFStack stack, 
            GPIndividual individual, 
            IProblem problem)
        {
            var p = (Ant)problem;
            switch (p.Orientation)
            {
                case Ant.O_UP:
                    p.Orientation = Ant.O_LEFT;
                    break;
                case Ant.O_LEFT:
                    p.Orientation = Ant.O_DOWN;
                    break;
                case Ant.O_DOWN:
                    p.Orientation = Ant.O_RIGHT;
                    break;
                case Ant.O_RIGHT:
                    p.Orientation = Ant.O_UP;
                    break;
                default:  // whoa!
                    state.Output.Fatal("Whoa, somehow I got a bad orientation! (" + p.Orientation + ")");
                    break;
            }
            p.Moves++;
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
            Eval(state, thread, input, stack, individual, problem);
        }
    }
}