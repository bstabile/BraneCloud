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

namespace BraneCloud.Evolution.EC.App.Lawnmower.Func
{
    [ECConfiguration("ec.app.lawnmower.func.Mow")]
    public class Mow : GPNode
    {
        public override string ToString() { return "mow"; }

        //public override void CheckConstraints(IEvolutionState state,
        //    int tree,
        //    GPIndividual typicalIndividual,
        //    IParameter individualBase)
        //{
        //    base.CheckConstraints(state, tree, typicalIndividual, individualBase);
        //    if (Children.Length != 0)
        //        state.Output.Error("Incorrect number of children for node " +
        //            ToStringForError() + " at " +
        //            individualBase);
        //}

        public override int ExpectedChildren => 0;

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
            var p = (Lawnmower)problem;
            var d = (LawnmowerData)input;

            switch (p.Orientation)
            {
                case Lawnmower.O_UP:
                    p.PosY--;
                    if (p.PosY < 0) p.PosY = p.MaxY - 1;
                    break;
                case Lawnmower.O_LEFT:
                    p.PosX--;
                    if (p.PosX < 0) p.PosX = p.MaxX - 1;
                    break;
                case Lawnmower.O_DOWN:
                    p.PosY++;
                    if (p.PosY >= p.MaxY) p.PosY = 0;
                    break;
                case Lawnmower.O_RIGHT:
                    p.PosX++;
                    if (p.PosX >= p.MaxX) p.PosX = 0;
                    break;
                default:  // whoa!
                    state.Output.Fatal("Whoa, somehow I got a bad orientation! (" + p.Orientation + ")");
                    break;
            }

            p.Moves++;
            if (p.Map[p.PosX][p.PosY] == Lawnmower.UNMOWED)
            {
                p.Sum++;
                p.Map[p.PosX][p.PosY] = p.Moves;
            }

            // return [0,0]
            d.x = 0;
            d.y = 0;
        }
    }
}