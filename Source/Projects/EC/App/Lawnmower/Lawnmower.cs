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
using BraneCloud.Evolution.EC.GP.Koza;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.App.Lawnmower
{
    /// <summary>
    /// Lawnmower implements the Koza-II Lawnmower problem.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>data</tt><br/>
    /// <font size="-1">classname, inherits or == ec.app.lawnmower.LawnmowerData</font></td>
    /// <td valign="top">(the class for the prototypical GPData object for the Lawnmower problem)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>file</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(filename of the .trl file for the Lawnmower problem)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>turns</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(maximal number of moves the lawnmower may make)</td></tr>
    /// </table>
    /// 
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>data</tt></td>
    /// <td>species (the GPData object)</td></tr>
    /// </table>
    /// </summary>
    [ECConfiguration("ec.app.lawnmower.Lawnmower")]
    public class Lawnmower : GPProblem, ISimpleProblem
    {
        public const string P_X = "x";
        public const string P_Y = "y";

        /// <summary>
        /// Map point descriptions 
        /// </summary>
        public const int UNMOWED = 0;

        // orientations
        public const int O_UP = 0;
        public const int O_LEFT = 1;
        public const int O_DOWN = 2;
        public const int O_RIGHT = 3;

        /// <summary>
        /// We'll deep clone this
        /// </summary>
        public LawnmowerData Input;

        /// <summary>
        /// Our map
        /// </summary>
        public int[][] Map;

        // map[][]'s bounds
        public int MaxX;
        public int MaxY;

        // our current position
        public int PosX;
        public int PosY;

        /// <summary>
        /// How many points we've gotten
        /// </summary>
        public int Sum;

        /// <summary>
        /// Our orientation
        /// </summary>
        public int Orientation;

        /// <summary>
        /// How many moves we've made
        /// </summary>
        public int Moves;

        /// <summary>
        /// print modulo for doing the abcdefg.... thing at print-time
        /// </summary>
        public int Pmod;

        public override object Clone()
        {
            var myobj = (Lawnmower)(base.Clone());
            myobj.Input = (LawnmowerData)(Input.Clone());
            myobj.Map = new int[Map.Length][];
            for (var x = 0; x < Map.Length; x++)
                myobj.Map[x] = (int[])(Map[x].Clone());
            return myobj;
        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // very important, remember this
            base.Setup(state, paramBase);

            // I'm not using the default base for any of this stuff;
            // it's not safe I think.

            // set up our input
            Input = (LawnmowerData)state.Parameters.GetInstanceForParameterEq(
                paramBase.Push(P_DATA), null, typeof(LawnmowerData));
            Input.Setup(state, paramBase.Push(P_DATA));

            // load our map coordinates
            MaxX = state.Parameters.GetInt(paramBase.Push(P_X), null, 1);
            if (MaxX == 0)
                state.Output.Error("The width (x dimension) of the lawn must be >0",
                    paramBase.Push(P_X));
            MaxY = state.Parameters.GetInt(paramBase.Push(P_Y), null, 1);
            if (MaxY == 0)
                state.Output.Error("The length (y dimension) of the lawn must be >0",
                    paramBase.Push(P_Y));
            state.Output.ExitIfErrors();

            // set up the map

            Map = TensorFactory.Create<int>(MaxX, MaxY); // new int[maxx][maxy];
            for (var x = 0; x < MaxX; x++)
                for (var y = 0; y < MaxY; y++)
                    Map[x][y] = UNMOWED;
        }

        public void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            if (!ind.Evaluated)  // don't bother reevaluating
            {
                Sum = 0;
                Moves = 0;
                PosX = MaxX / 2 + 1;
                PosY = MaxY / 2 + 1;
                Orientation = O_UP;

                // evaluate the individual
                ((GPIndividual)ind).Trees[0].Child.Eval(
                    state, threadnum, Input, Stack, ((GPIndividual)ind), this);

                // clean up the map
                for (var x = 0; x < MaxX; x++)
                    for (var y = 0; y < MaxY; y++)
                        Map[x][y] = UNMOWED;

                // the fitness better be KozaFitness!
                var f = ((KozaFitness)ind.Fitness);
                f.SetStandardizedFitness(state, (float)(MaxX * MaxY - Sum));
                f.Hits = Sum;
                ind.Evaluated = true;
            }
        }

        public override void Describe(IEvolutionState state, Individual ind, int subpopulation, int threadnum, int log)
        {
            state.Output.PrintLn("\n\nBest Individual's Map\n=====================", log);

            Sum = 0;
            Moves = 0;
            PosX = MaxX / 2 + 1;
            PosY = MaxY / 2 + 1;
            Orientation = O_UP;

            // evaluate the individual
            ((GPIndividual)ind).Trees[0].Child.Eval(
                state, threadnum, Input, Stack, ((GPIndividual)ind), this);

            // print out the map
            state.Output.PrintLn(" Y ->", log);
            for (var x = 0; x < Map.Length; x++)
            {
                if (x == 1) state.Output.Print("v", log);
                else if (x == 0) state.Output.Print("X", log);
                else state.Output.Print(" ", log);
                state.Output.Print("+", log);
                for (var y = 0; y < Map[x].Length; y++)
                    state.Output.Print("----+", log);
                state.Output.PrintLn("", log);
                if (x == 0) state.Output.Print("|", log);
                else state.Output.Print(" ", log);
                state.Output.Print("|", log);

                for (var y = 0; y < Map[x].Length; y++)
                {
                    if (Map[x][y] == UNMOWED)
                        state.Output.Print("    ", log);
                    else
                    {
                        var s = "" + (Map[x][y]);
                        while (s.Length < 4) s = " " + s;
                        state.Output.Print(s + "|", log);
                    }
                }
                state.Output.PrintLn("", log);
            }
            if (Map.Length == 1) state.Output.Print("v", log);
            else state.Output.Print(" ", log);
            state.Output.Print("+", log);
            for (var y = 0; y < Map[Map.Length - 1].Length; y++)
                state.Output.Print("----+", log);
            state.Output.PrintLn("", log);


            // clean up the map
            for (var x = 0; x < MaxX; x++)
                for (var y = 0; y < MaxY; y++)
                    Map[x][y] = UNMOWED;
        }
    }
}