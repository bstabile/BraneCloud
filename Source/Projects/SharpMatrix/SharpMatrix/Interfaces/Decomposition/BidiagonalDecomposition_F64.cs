using System;
using SharpMatrix.Data;

namespace SharpMatrix.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * <p>Implementation of {@link BidiagonalDecomposition} for 64-bit floats</p>
 *
 *
 * @author Peter Abeles
 */
    public interface BidiagonalDecomposition_F64<T> : BidiagonalDecomposition<T>
        where T : Matrix
    {
        /**
         * Extracts the diagonal and off diagonal elements from the decomposition.
         *
         * @param diag diagonal elements from B.
         * @param off off diagonal elements form B.
         */
        void getDiagonal(double[] diag, double[] off);
    }
}