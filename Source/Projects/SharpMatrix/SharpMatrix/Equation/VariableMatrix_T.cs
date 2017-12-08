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
    public class VariableMatrix<T> : Variable 
        where T : class, Matrix
    {
        public T matrix;

        /**
         * If true then the matrix is dynamically resized to match the output of a function
         */
        public bool temp;

        /**
         * Initializes the matrix variable.  If null then the variable will be a reference one.  If not null then
         * it will be assignment.
         * @param matrix Matrix.
         */
        public VariableMatrix(T matrix)
            : base(VariableType.MATRIX)
        {
            this.matrix = matrix;
        }

        public static VariableMatrix<T> createTemp()
        {
            VariableMatrix<T> ret;
            if (typeof(DMatrixRMaj).IsAssignableFrom(typeof(T)))
            {
                ret = new VariableMatrix<T>(new DMatrixRMaj(1, 1) as T);
            }
            else if (typeof(FMatrixRMaj).IsAssignableFrom(typeof(T)))
            {
                ret = new VariableMatrix<T>(new FMatrixRMaj(1, 1) as T);
            }
            else
                throw new InvalidOperationException("Unknown matrix type!");

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