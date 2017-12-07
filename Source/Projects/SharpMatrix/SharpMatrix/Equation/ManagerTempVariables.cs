using System;

namespace BraneCloud.Evolution.EC.MatrixLib.Equation
{
    //package org.ejml.equation;

/**
 * Manages the creation and recycling of temporary variables used to store intermediate results.  The user
 * cannot directly access these variables
 *
 * @author Peter Abeles
 */
// TODO add function to purge temporary variables.  basicaly resize and redeclare their array to size 1
    public class ManagerTempVariables
    {

        public VariableMatrix createMatrix()
        {
            return VariableMatrix.createTemp();
        }

        public VariableDouble createDouble()
        {
            return new VariableDouble(0);
        }

        public VariableDouble createDouble(double value)
        {
            return new VariableDouble(value);
        }

        public VariableInteger createInteger()
        {
            return createInteger(0);
        }

        public VariableInteger createInteger(int value)
        {
            return new VariableInteger(value);
        }

        public VariableIntegerSequence createIntegerSequence(IntegerSequence sequence)
        {
            return new VariableIntegerSequence(sequence);
        }
    }
}