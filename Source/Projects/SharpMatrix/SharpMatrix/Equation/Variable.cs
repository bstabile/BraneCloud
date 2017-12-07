using System;

namespace BraneCloud.Evolution.EC.MatrixLib.Equation
{
    //package org.ejml.equation;

/**
 * Instance of a variable created at compile time.  This base class only specifies the type of variable which it is.
 *
 * @author Peter Abeles
 */
    public class Variable
    {
        public VariableType type;

        protected Variable(VariableType type)
        {
            this.type = type;
        }

        public VariableType getType()
        {
            return type;
        }

        public virtual string toString()
        {
            return "VAR_" + type;
        }
    }
}