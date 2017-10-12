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
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.App.Regression.Test.Func
{
    [ECConfiguration("ec.app.regression.func.RegERC")]
    public class RegERC : ERC
    {
        public double value;

        // making sure that we don't have any children is already
        // done in ERC.checkConstraints(), so we don't need to implement that.

        // this will produce numbers from [-1.0, 1.0), which is probably
        // okay but you might want to modify it if you don't like seeing
        // -1.0's occasionally showing up very rarely.
        public override void ResetNode(IEvolutionState state, int thread)
        { value = state.Random[thread].NextDouble() * 2 - 1.0; }

        public override int NodeHashCode()
        {
            // a reasonable hash code
            return GetType().GetHashCode() + Convert.ToInt32((float)value);
        }

        public override bool NodeEquals(GPNode node)
        {
            // check first to see if we're the same kind of ERC -- 
            // won't work for subclasses; in that case you'll need
            // to change this to isAssignableTo(...)
            if (GetType() != node.GetType()) return false;
            // now check to see if the ERCs hold the same value
            return (((RegERC)node).value == value);
        }

        public override void ReadNode(IEvolutionState state, BinaryReader dataInput) // throws IOException
        {
            value = dataInput.ReadDouble();
        }

        public override void WriteNode(IEvolutionState state, BinaryWriter dataOutput) // throws IOException
        {
            dataOutput.Write(value);
        }

        public override string Encode()
        { return Code.Encode(value); }

        public override bool Decode(DecodeReturn dret)
        {
            // store the position and the string in case they
            // get modified by Code.java
            int pos = dret.Pos;
            String data = dret.Data;

            // decode
            Code.Decode(dret);

            if (dret.Type != DecodeReturn.T_DOUBLE) // uh oh!
            {
                // restore the position and the string; it was an error
                dret.Data = data;
                dret.Pos = pos;
                return false;
            }

            // store the data
            value = dret.D;
            return true;
        }

        public override string ToStringForHumans()
        { return "" + (float)value; }

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
            var rd = ((RegressionData)(input));
            rd.x = value;
        }
    }
}