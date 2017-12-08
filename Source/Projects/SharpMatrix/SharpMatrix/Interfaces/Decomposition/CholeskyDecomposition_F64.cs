using System;
using SharpMatrix.Data;

namespace SharpMatrix.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;


    /**
     * <p>
     * Implementation of {@link CholeskyDecomposition} for 64-bit floats.
     * </p>
     *
     * @author Peter Abeles
     */
    public interface CholeskyDecomposition_F64<TMatrix> : CholeskyDecomposition<TMatrix>
        where TMatrix : Matrix
    {

        /**
         * Computes the matrix's determinant using the decomposition.
         *
         * @return The determinant.
         */
        Complex_F64 computeDeterminant();

    }
}