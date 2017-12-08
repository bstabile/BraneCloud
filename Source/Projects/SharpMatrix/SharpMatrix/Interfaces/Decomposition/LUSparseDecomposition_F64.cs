using System;
using SharpMatrix.Data;

namespace SharpMatrix.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * Implementation of {@link LUSparseDecomposition} for 64-bit numbers
 *
 * @author Peter Abeles
 */
    public interface LUSparseDecomposition_F64<T> : LUSparseDecomposition<T>
        where T : Matrix
    {
        /**
         * Computes the matrix's determinant using the LU decomposition.
         *
         * @return The determinant.
         */
        Complex_F64 computeDeterminant();
    }
}