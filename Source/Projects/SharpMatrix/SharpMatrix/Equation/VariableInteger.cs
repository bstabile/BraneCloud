using System;

namespace BraneCloud.Evolution.EC.MatrixLib.Equation
{
    //package org.ejml.equation;

/**
 * Variable which stores an instance of int.
 *
 * @author Peter Abeles
 */
    public class VariableInteger : VariableScalar
    {
        public int value;

        public VariableInteger(int value)
            : base(ScalarType.INTEGER)
        {
            this.value = value;
        }

        //@Override
        public override double getDouble()
        {
            return value;
        }
    }
}