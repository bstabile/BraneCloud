using System;

namespace BraneCloud.Evolution.EC.MatrixLib.Data
{
    //package org.ejml.data;

/**
 * Scalar value. Useful when a function needs more than one output.
 *
 * @author Peter Abeles
 */
    public class IScalar
    {
        public int value;

        public int getValue()
        {
            return value;
        }

        public void setValue(int value)
        {
            this.value = value;
        }
    }
}