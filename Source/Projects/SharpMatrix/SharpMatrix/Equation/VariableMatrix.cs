using System;
using SharpMatrix.Data;

namespace SharpMatrix.Equation
{
    //package org.ejml.equation;

/**
 * Storage for {@link DMatrixRMaj matrix} type variables.
 *
 * @author Peter Abeles
 */
    public class VariableMatrix : Variable
    {
        public DMatrixRMaj matrix;

        /**
         * If true then the matrix is dynamically resized to match the output of a function
         */
        public bool temp;

        /**
         * Initializes the matrix variable.  If null then the variable will be a reference one.  If not null then
         * it will be assignment.
         * @param matrix Matrix.
         */
        public VariableMatrix(DMatrixRMaj matrix)
            : base(VariableType.MATRIX)
        {
            this.matrix = matrix;
        }

        public static VariableMatrix createTemp()
        {
            VariableMatrix ret = new VariableMatrix(new DMatrixRMaj(1, 1));
            ret.setTemp(true);
            return ret;
        }

        public bool isTemp()
        {
            return temp;
        }

        public void setTemp(bool temp)
        {
            this.temp = temp;
        }
    }
}