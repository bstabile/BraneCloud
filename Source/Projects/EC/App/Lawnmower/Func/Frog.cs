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
    [ECConfiguration("ec.app.lawnmower.func.Frog")]
    public class Frog : GPNode
    {
        public override string ToString() { return "frog"; }

        //public override void CheckConstraints(IEvolutionState state,
        //    int tree,
        //    GPIndividual typicalIndividual,
        //    IParameter individualBase)
        //{
        //    base.CheckConstraints(state, tree, typicalIndividual, individualBase);
        //    if (Children.Length != 1)
        //        state.Output.Error("Incorrect number of children for node " +
        //            ToStringForError() + " at " +
        //            individualBase);
        //}

        public override int ExpectedChildren { get { return 1; } }

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
            var p = (Lawnmower)problem;
            var d = (LawnmowerData)input;

            Children[0].Eval(state, thread, input, stack, individual, problem);

            // we follow the Koza-II example, not the lil-gp example.
            // that is, we "assume" that in our orientation the X axis
            // is moving out away from us, and the Y axis is moving
            // out to the left.  In lil-gp, the assumption is that the Y axis
            // axis is moving out away from us, and the X axis is moving out
            // to the right.

            switch (p.Orientation)
            {
                case Lawnmower.O_UP:
                    // counter-clockwise rotation
                    p.PosX -= d.y;
                    p.PosY += d.x;
                    break;
                case Lawnmower.O_LEFT:
                    // flipped orientation
                    p.PosX -= d.x;
                    p.PosY -= d.y;
                    break;
                case Lawnmower.O_DOWN:
                    // clockwise rotation
                    p.PosX += d.y;
                    p.PosY -= d.x;
                    break;
                case Lawnmower.O_RIGHT:
                    // proper orientation
                    p.PosX += d.x;
                    p.PosY += d.y;
                    break;
                default:  // whoa!
                    state.Output.Fatal("Whoa, somehow I got a bad orientation! (" + p.Orientation + ")");
                    break;
            }

            // shift back into the lawn frame.
            // because Java's % on negative numbers preserves the
            // minus sign, we have to mod twice with an addition.
            // C has to do this too.
            p.PosX = ((p.PosX % p.MaxX) + p.MaxX) % p.MaxX;
            p.PosY = ((p.PosY % p.MaxY) + p.MaxY) % p.MaxY;

            p.Moves++;
            if (p.Map[p.PosX][p.PosY] == Lawnmower.UNMOWED)
            {
                p.Sum++;
                p.Map[p.PosX][p.PosY] = p.Moves;
            }

            // return [x,y] -- to do this, simply don't modify input
        }
    }
}