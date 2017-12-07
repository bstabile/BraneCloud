using System;

namespace BraneCloud.Evolution.EC.MatrixLib.Data
{
    //package org.ejml.data;

/**
 * Scalar value. Useful when a function needs more than one output.
 *
 * @author Peter Abeles
 */
    public class DScalar
    {
        public double value;

        public double getValue()
        {
            return value;
        }

        public void setValue(double value)
        {
            this.value = value;
        }
    }
}