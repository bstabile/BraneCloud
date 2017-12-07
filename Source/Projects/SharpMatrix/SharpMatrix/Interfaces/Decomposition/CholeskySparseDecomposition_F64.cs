using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * <p>
 * Implementation of {@link CholeskySparseDecomposition} for 64-bit floats.
 * </p>
 *
 * @author Peter Abeles
 */
    public interface CholeskySparseDecomposition_F64<T> : CholeskySparseDecomposition<T>
        where T : Matrix
    {
        /**
         * Computes the matrix's determinant using the decomposition.
         *
         * @return The determinant.
         */
        Complex_F64 computeDeterminant();
    }
}