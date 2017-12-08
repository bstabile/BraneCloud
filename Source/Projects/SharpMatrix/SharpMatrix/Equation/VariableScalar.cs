using System;

namespace SharpMatrix.Equation
{
    //package org.ejml.equation;

/**
 * Variable for storing primitive scalar data types, e.g. int and double.
 *
 * @author Peter Abeles
 */
    public abstract class VariableScalar : Variable
    {

        public new ScalarType type;

        public VariableScalar(ScalarType type)
            : base(VariableType.SCALAR)
        {
            this.type = type;
        }

        public abstract double getDouble();

        //@Override
        public override string toString()
        {
            switch (type)
            {
                case ScalarType.INTEGER:
                    return "ScalarI";
                case ScalarType.DOUBLE:
                    return "ScalarD";
                case ScalarType.COMPLEX:
                    return "ScalarC";
                default:
                    return "ScalarUnknown";
            }
        }

        public ScalarType getScalarType()
        {
            return type;
        }

        public enum ScalarType
        {
            INTEGER,
            DOUBLE,
            COMPLEX,
            UNKOWN,
        }
    }
}