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
using System.IO;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.GP.Koza;
using BraneCloud.Evolution.EC.Problems.Ant.Func;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Support;

namespace BraneCloud.Evolution.EC.Problems.Ant
{
    /// <summary>
    /// AntProblem implements the Artificial AntProblem problem.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>data</tt><br/>
    /// <font size="-1">classname, inherits or == ec.app.ant.AntData</font></td>
    /// <td valign="top">(the class for the prototypical GPData object for the Ant problem)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>file</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(filename of the .trl file for the Ant problem)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>turns</tt><br/>
    /// <font size="-1"/>int &gt;= 1</td>
    /// <td valign="top">(maximal number of moves the ant may make)</td></tr>
    /// </table>
    /// 
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>data</tt></td>
    /// <td>species (the GPData object)</td></tr>
    /// </table>
    /// </summary>
    [ECConfiguration("ec.problems.ant.Ant")]
    public class Ant : GPProblem, ISimpleProblem
    {
        #region Constants

        public const string P_FILE = "file";
        public const string P_MOVES = "moves";

        // map point descriptions
        public const int ERROR = 0;
        public const int FOOD = -1;
        public const int EMPTY = 1;
        public const int TRAIL = 2;
        public const int ATE = 3;

        // orientations
        public const int O_UP = 0;
        public const int O_LEFT = 1;
        public const int O_DOWN = 2;
        public const int O_RIGHT = 3;

        #endregion // Constants
        #region Properties

        /// <summary>
        /// Maximum number of moves
        /// </summary>
        public int MaxMoves { get; set; }

        /// <summary>
        /// how much food we have
        /// </summary>
        public int Food { get; set; }

        /// <summary>
        /// Our map.
        /// </summary>
        public int[][] Map { get; set; }

        // store the positions of food so we can reset our map
        // don't need to be deep-cloned, they're read-only
        public int[] FoodX { get; set; }
        public int[] FoodY { get; set; }

        // map[][]'s bounds
        public int MaxX { get; set; }
        public int MaxY { get; set; }

        // our position
        public int PosX { get; set; }
        public int PosY { get; set; }

        /// <summary>
        /// How many points we've gotten.
        /// </summary>
        public int Sum { get; set; }

        /// <summary>
        /// Our orientation.
        /// </summary>
        public int Orientation { get; set; }

        /// <summary>
        /// How many moves we've made.
        /// </summary>
        public int Moves { get; set; }

        /// <summary>
        /// Print modulo for doing the abcdefg.... thing at print-time
        /// </summary>
        public int PMod { get; set; }

        #endregion // Properties
        #region Cloning

        public override object Clone()
        {
            var myobj = (Ant)base.Clone();
            myobj.Input = (GPData)Input.Clone();
            myobj.Map = new int[Map.Length][];
            for (var x = 0; x < Map.Length; x++)
                myobj.Map[x] = (int[])(Map[x].Clone());
            return myobj;
        }

        #endregion // Cloning
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // very important, remember this
            base.Setup(state, paramBase);

            // No need to verify the GPData object

            // not using any default base -- it's not safe

            // how many maxMoves?
            MaxMoves = state.Parameters.GetInt(paramBase.Push(P_MOVES), null, 1);
            if (MaxMoves == 0)
                state.Output.Error("The number of moves an ant has to make must be >0");

            // load our file
            //var fileInfo = state.Parameters.GetFile(paramBase.Push(P_FILE), null);
            //if (fileInfo == null)
            //    state.Output.Fatal("AntProblem trail file name not provided.");
            var stream = state.Parameters.GetResource(paramBase.Push(P_FILE), null);
            if (stream == null)
                state.Output.Fatal("Error loading file or resource", paramBase.Push(P_FILE), null);

            Food = 0;
            try
            {
                //var lnr = new StreamReader(fileInfo.FullName);
                var lnr = new StreamReader(stream);

                var st = new Tokenizer(lnr.ReadLine()); // ugh
                MaxX = Int32.Parse(st.NextToken());
                MaxY = Int32.Parse(st.NextToken());
                Map = new int[MaxX][];
                for (var x = 0; x < MaxX; x++)
                {
                    Map[x] = new int[MaxY];
                }

                int y;
                for (y = 0; y < MaxY; y++)
                {
                    var s = lnr.ReadLine();
                    if (s == null)
                    {
                        state.Output.Warning("AntProblem trail file ended prematurely");
                        break;
                    }
                    int x;
                    for (x = 0; x < s.Length; x++)
                    {
                        switch (s[x])
                        {
                            case ' ':
                                Map[x][y] = EMPTY;
                                break;
                            case '#':
                                Map[x][y] = FOOD;
                                Food++;
                                break;
                            case '.':
                                Map[x][y] = TRAIL;
                                break;
                            default:
                                state.Output.Error("Bad character '" + s[x] + "' on line number " + y
                                                   /*lnr.GetLineNumber()*/+ " of the AntProblem trail file.");
                                break;
                        }
                    }
                    // fill out rest of X's
                    for (var z = x; z < MaxX; z++)
                        Map[z][y] = EMPTY;
                }
                // fill out rest of Y's
                for (var z = y; z < MaxY; z++)
                    for (var x = 0; x < MaxX; x++)
                        Map[x][z] = EMPTY;
            }
            catch (FormatException)
            {
                state.Output.Fatal("The AntProblem trail file does not begin with x and y integer values.");
            }
            catch (IOException e)
            {
                state.Output.Fatal("The AntProblem trail file could not be read due to an IOException:\n" + e);
            }
            state.Output.ExitIfErrors();

            // load foodx and foody reset arrays
            FoodX = new int[Food];
            FoodY = new int[Food];
            var tmpf = 0;
            for (var x = 0; x < Map.Length; x++)
                for (var y = 0; y < Map[0].Length; y++)
                    if (Map[x][y] == FOOD)
                    {
                        FoodX[tmpf] = x;
                        FoodY[tmpf] = y;
                        tmpf++;
                    }
        }

        #endregion // Setup
        #region Problem

        public void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            if (!ind.Evaluated)  // don't bother reevaluating
            {
                Sum = 0;
                PosX = 0;
                PosY = 0;
                Orientation = O_RIGHT;

                for (Moves = 0; Moves < MaxMoves && Sum < Food; )
                    ((GPIndividual)ind).Trees[0].Child.Eval(state, threadnum, Input, Stack, ((GPIndividual)ind), this);

                // the fitness better be KozaFitness!
                var f = ((KozaFitness)ind.Fitness);
                f.SetStandardizedFitness(state, (Food - Sum));
                f.Hits = Sum;
                ind.Evaluated = true;

                // clean up array
                for (var y = 0; y < Food; y++)
                    Map[FoodX[y]][FoodY[y]] = FOOD;
            }
        }

        public override void Describe(IEvolutionState state, Individual ind, int subpopulation, int threadnum, int log)
        {
            state.Output.PrintLn("\n\nBest Individual's Map\n=====================", log);

            Sum = 0;
            PMod = 97; /** ascii a */
            PosX = 0;
            PosY = 0;
            Orientation = O_RIGHT;

            var map2 = new int[Map.Length][];
            for (var x = 0; x < Map.Length; x++)
                map2[x] = (int[])(Map[x].Clone());

            map2[PosX][PosY] = PMod; PMod++;
            for (Moves = 0; Moves < MaxMoves && Sum < Food; )
                ((IEvalPrint)(((GPIndividual)ind).Trees[0].Child)).EvalPrint(state, threadnum, Input, Stack, ((GPIndividual)ind), this, map2);
            // print out the map
            for (var y = 0; y < map2.Length; y++)
            {
                foreach (var t in map2)
                {
                    switch (t[y])
                    {
                        case FOOD:
                            state.Output.Print("#", log);
                            break;
                        case EMPTY:
                            state.Output.Print(".", log);
                            break;
                        case TRAIL:
                            state.Output.Print("+", log);
                            break;
                        case ATE:
                            state.Output.Print("?", log);
                            break;
                        default:
                            state.Output.Print("" + ((char)t[y]), log);
                            break;
                    }
                }
                state.Output.PrintLn("", log);
            }
        }

        #endregion // Problem
    }
}