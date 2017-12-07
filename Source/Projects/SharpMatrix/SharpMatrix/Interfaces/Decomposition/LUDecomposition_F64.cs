using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * Implementation of {@link LUDecomposition} for 64-bit numbers
 *
 * @author Peter Abeles
 */
    public interface LUDecomposition_F64<T> : LUDecomposition<T>
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