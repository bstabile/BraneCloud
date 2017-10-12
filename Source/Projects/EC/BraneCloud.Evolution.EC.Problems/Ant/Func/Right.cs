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
using BraneCloud.Evolution.EC.Problems.Ant;

namespace BraneCloud.Evolution.EC.Problems.AntApp
{
    [ECConfiguration("ec.problems.ant.func.Right")]
    public class Right : GPNode, IEvalPrint
    {
        public override string ToString() { return "right"; }

        //public void CheckConstraints(
        //    IEvolutionState state, 
        //    int tree, 
        //    GPIndividual typicalIndividual, 
        //    Parameter individualBase)
        //{
        //    base.CheckConstraints(state, tree, typicalIndividual, individualBase);
        //    if (Children.Length != 0)
        //        state.Output.Error("Incorrect number of children for node " +
        //            ToStringForError() + " at " +
        //            individualBase);
        //}

        public override int ExpectedChildren { get { return 0; } }

        public override void Eval(
            IEvolutionState state, 
            int thread, 
            GPData input, 
            ADFStack stack, 
            GPIndividual individual, 
            IProblem problem)
        {
            var p = (AntProblem)problem;
            switch (p.Orientation)
            {
                case AntProblem.O_UP:
                    p.Orientation = AntProblem.O_RIGHT;
                    break;
                case AntProblem.O_LEFT:
                    p.Orientation = AntProblem.O_UP;
                    break;
                case AntProblem.O_DOWN:
                    p.Orientation = AntProblem.O_LEFT;
                    break;
                case AntProblem.O_RIGHT:
                    p.Orientation = AntProblem.O_DOWN;
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