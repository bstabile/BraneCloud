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

namespace BraneCloud.Evolution.EC.App.AntApp
{
    [ECConfiguration("ec.app.ant.func.IfFoodAhead")]
    public class IfFoodAhead : GPNode, IEvalPrint
    {
        public override string ToString() { return "if-food-ahead"; }

        //public void CheckConstraints(IEvolutionState state, int tree, GPIndividual typicalIndividual, Parameter individualBase)
        //{
        //    base.CheckConstraints(state, tree, typicalIndividual, individualBase);
        //    if (Children.Length != 2)
        //        state.Output.Error("Incorrect number of children for node " +
        //                           ToStringForError() + " at " +
        //                           individualBase);
        //}

        public override int ExpectedChildren => 2;

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
                    if (p.Map[p.PosX][(p.PosY - 1 + p.MaxY) % p.MaxY] == Ant.FOOD)
                        Children[0].Eval(state, thread, input, stack, individual, problem);
                    else Children[1].Eval(state, thread, input, stack, individual, problem);
                    break;
                case Ant.O_LEFT:
                    if (p.Map[(p.PosX - 1 + p.MaxX) % p.MaxX][p.PosY] == Ant.FOOD)
                        Children[0].Eval(state, thread, input, stack, individual, problem);
                    else Children[1].Eval(state, thread, input, stack, individual, problem);
                    break;
                case Ant.O_DOWN:
                    if (p.Map[p.PosX][(p.PosY + 1) % p.MaxY] == Ant.FOOD)
                        Children[0].Eval(state, thread, input, stack, individual, problem);
                    else Children[1].Eval(state, thread, input, stack, individual, problem);
                    break;
                case Ant.O_RIGHT:
                    if (p.Map[(p.PosX + 1) % p.MaxX][p.PosY] == Ant.FOOD)
                        Children[0].Eval(state, thread, input, stack, individual, problem);
                    else Children[1].Eval(state, thread, input, stack, individual, problem);
                    break;
                default:  // whoa!
                    state.Output.Fatal("Whoa, somehow I got a bad orientation! (" + p.Orientation + ")");
                    break;
            }
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
            var p = (Ant)problem;
            switch (p.Orientation)
            {
                case Ant.O_UP:
                    if (p.Map[p.PosX][(p.PosY - 1 + p.MaxY) % p.MaxY] == Ant.FOOD)
                        ((IEvalPrint)Children[0]).EvalPrint(state, thread, input, stack, individual, problem, map2);
                    else ((IEvalPrint)Children[1]).EvalPrint(state, thread, input, stack, individual, problem, map2);
                    break;
                case Ant.O_LEFT:
                    if (p.Map[(p.PosX - 1 + p.MaxX) % p.MaxX][p.PosY] == Ant.FOOD)
                        ((IEvalPrint)Children[0]).EvalPrint(state, thread, input, stack, individual, problem, map2);
                    else ((IEvalPrint)Children[1]).EvalPrint(state, thread, input, stack, individual, problem, map2);
                    break;
                case Ant.O_DOWN:
                    if (p.Map[p.PosX][(p.PosY + 1) % p.MaxY] == Ant.FOOD)
                        ((IEvalPrint)Children[0]).EvalPrint(state, thread, input, stack, individual, problem, map2);
                    else ((IEvalPrint)Children[1]).EvalPrint(state, thread, input, stack, individual, problem, map2);
                    break;
                case Ant.O_RIGHT:
                    if (p.Map[(p.PosX + 1) % p.MaxX][p.PosY] == Ant.FOOD)
                        ((IEvalPrint)Children[0]).EvalPrint(state, thread, input, stack, individual, problem, map2);
                    else ((IEvalPrint)Children[1]).EvalPrint(state, thread, input, stack, individual, problem, map2);
                    break;
                default:  // whoa!
                    state.Output.Fatal("Whoa, somehow I got a bad orientation! (" + p.Orientation + ")");
                    break;
            }
        }
    }
}