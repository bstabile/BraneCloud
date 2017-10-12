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
using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.App.Lawnmower.Test.Func
{
    [ECConfiguration("ec.app.lawnmower.func.LawnERC")]
    public class LawnERC : ERC
    {
        public int maxx;
        public int maxy;

        public int x;
        public int y;

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            // figure the coordinate base -- this will break if the underlying
            // base changes, oops
            var newbase = new Parameter(EvolutionState.P_EVALUATOR).Push(Evaluator.P_PROBLEM);

            // obviously not using the default base for any of this stuff

            // load our map coordinates
            maxx = state.Parameters.GetInt(newbase.Push(Lawnmower.P_X), null, 1);
            if (maxx == 0)
                state.Output.Error("The width (x dimension) of the lawn must be >0",
                    newbase.Push(Lawnmower.P_X));
            maxy = state.Parameters.GetInt(newbase.Push(Lawnmower.P_Y), null, 1);
            if (maxy == 0)
                state.Output.Error("The length (y dimension) of the lawn must be >0",
                    newbase.Push(Lawnmower.P_X));
            state.Output.ExitIfErrors();
        }

        public override void ResetNode(IEvolutionState state, int thread)
        {
            x = state.Random[thread].NextInt(maxx);
            y = state.Random[thread].NextInt(maxy);
        }

        public override int NodeHashCode()
        {
            // a reasonable hash code
            return GetType().GetHashCode() + x * maxy + y;
        }

        public override bool NodeEquals(GPNode node)
        {
            // check first to see if we're the same kind of ERC -- 
            // won't work for subclasses; in that case you'll need
            // to change this to isAssignableTo(...)
            if (GetType() != node.GetType()) return false;
            // now check to see if the ERCs hold the same value
            var n = (LawnERC)node;
            return (n.x == x && n.y == y);
        }

        public override void ReadNode(IEvolutionState state, BinaryReader dataInput) // throws IOException
        {
            x = dataInput.ReadInt32();
            y = dataInput.ReadInt32();
        }

        public override void WriteNode(IEvolutionState state, BinaryWriter dataOutput) // throws IOException
        {
            dataOutput.Write(x);
            dataOutput.Write(y);
        }

        public override string Encode()
        { return Code.Encode(x) + Code.Encode(y); }

        public override bool Decode(DecodeReturn dret)
        {
            // store the position and the string in case they
            // get modified by Code.java
            int pos = dret.Pos;
            String data = dret.Data;

            // decode
            Code.Decode(dret);

            if (dret.Type != DecodeReturn.T_INT) // uh oh!
            {
                // restore the position and the string; it was an error
                dret.Data = data;
                dret.Pos = pos;
                return false;
            }

            // store the data
            x = (int)(dret.L);

            // decode
            Code.Decode(dret);

            if (dret.Type != DecodeReturn.T_INT) // uh oh!
            {
                // restore the position and the string; it was an error
                dret.Data = data;
                dret.Pos = pos;
                return false;
            }

            // store the data
            y = (int)(dret.L);

            return true;
        }

        public override string ToStringForHumans()
        { return "[" + x + "," + y + "]"; }

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
            var rd = ((LawnmowerData)(input));
            rd.x = x;
            rd.y = y;
        }
    }
}