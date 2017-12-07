using System;

namespace BraneCloud.Evolution.EC.MatrixLib.Equation
{
    //package org.ejml.equation;

/**
 * Variable which stores an instance of double.
 *
 * @author Peter Abeles
 */
    public class VariableDouble : VariableScalar
    {
        public double value;

        public VariableDouble(double value)
            : base(ScalarType.DOUBLE)
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