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

namespace BraneCloud.Evolution.EC.Problems.Regression.Func
{
    [ECConfiguration("ec.problems.regression.func.RegERC")]
    public class RegERC : ERC
    {
        public double value;

        // Koza claimed to be generating from [-1.0, 1.0] but he wasn't,
        // given the published simple-lisp code.  It was [-1.0, 1.0).  This is
        // pretty minor, but we're going to go with the code rather than the
        // published specs in the books.  If you want to go with [-1.0, 1.0],
        // just change nextDouble() to nextDouble(true, true)

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
        { return "" + (double)value; }

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