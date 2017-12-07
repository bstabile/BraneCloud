using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * <p>Implementation of {@link BidiagonalDecomposition} for 32-bit floats</p>
 *
 *
 * @author Peter Abeles
 */
    public interface BidiagonalDecomposition_F32<T> : BidiagonalDecomposition<T>
        where T : Matrix
    {
        /**
         * Extracts the diagonal and off diagonal elements from the decomposition.
         *
         * @param diag diagonal elements from B.
         * @param off off diagonal elements form B.
         */
        void getDiagonal(float[] diag, float[] off);
    }
}