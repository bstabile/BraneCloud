using System;

namespace SharpMatrix.Equation
{
    //package org.ejml.equation;

/**
 * Variable which stores/describes a sequence of integers
 *
 * @author Peter Abeles
 */
    public class VariableIntegerSequence : Variable
    {
        public IntegerSequence sequence;

        public VariableIntegerSequence(IntegerSequence sequence)
            : base(VariableType.INTEGER_SEQUENCE)
        {
            ;
            this.sequence = sequence;
        }
    }
}
