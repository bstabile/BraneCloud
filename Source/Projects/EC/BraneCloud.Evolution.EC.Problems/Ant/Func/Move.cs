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

namespace BraneCloud.Evolution.EC.Problems.Ant.Func
{
    [ECConfiguration("ec.problems.ant.func.Move")]
    public class Move : GPNode, IEvalPrint
    {
        public override string ToString() { return "move"; }

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
                    p.PosY--;
                    if (p.PosY < 0) p.PosY = p.MaxY - 1;
                    break;
                case Ant.O_LEFT:
                    p.PosX--;
                    if (p.PosX < 0) p.PosX = p.MaxX - 1;
                    break;
                case Ant.O_DOWN:
                    p.PosY++;
                    if (p.PosY >= p.MaxY) p.PosY = 0;
                    break;
                case Ant.O_RIGHT:
                    p.PosX++;
                    if (p.PosX >= p.MaxX) p.PosX = 0;
                    break;
                default:  // whoa!
                    state.Output.Fatal("Whoa, somehow I got a bad orientation! (" + p.Orientation + ")");
                    break;
            }

            p.Moves++;
            if (p.Map[p.PosX][p.PosY] == Ant.FOOD && p.Moves < p.MaxMoves)
            {
                p.Sum++;
                p.Map[p.PosX][p.PosY] = Ant.ATE;
            }
        }

        /// <summary>
        /// Just like eval, but it retraces the map and prints out info.
        /// </summary>
        public void EvalPrint(IEvolutionState state, int thread, GPData input, ADFStack stack, GPIndividual individual, IProblem problem, int[][] map2)
        {
            var p = (Ant)problem;
            switch (p.Orientation)
            {
                case Ant.O_UP:
                    p.PosY--;
                    if (p.PosY < 0) p.PosY = p.MaxY - 1;
                    break;
                case Ant.O_LEFT:
                    p.PosX--;
                    if (p.PosX < 0) p.PosX = p.MaxX - 1;
                    break;
                case Ant.O_DOWN:
                    p.PosY++;
                    if (p.PosY >= p.MaxY) p.PosY = 0;
                    break;
                case Ant.O_RIGHT:
                    p.PosX++;
                    if (p.PosX >= p.MaxX) p.PosX = 0;
                    break;
                default:  // whoa!
                    state.Output.Fatal("Whoa, somehow I got a bad orientation! (" + p.Orientation + ")");
                    break;
            }

            p.Moves++;
            if (p.Map[p.PosX][p.PosY] == Ant.FOOD && p.Moves < p.MaxMoves)
            {
                p.Sum++;
                p.Map[p.PosX][p.PosY] = Ant.ATE;
            }

            if (p.Moves < p.MaxMoves)
            {
                if (++p.PMod > 122 /* ascii z */) p.PMod = 97; /* ascii a */
                map2[p.PosX][p.PosY] = p.PMod;
            }
        }
    }
}