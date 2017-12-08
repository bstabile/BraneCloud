using System;
using SharpMatrix.Data;

namespace SharpMatrix.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * <p>
 * Implementation of {@link CholeskyDecomposition} for 32-bit floats.
 * </p>
 *
 * @author Peter Abeles
 */
    public interface CholeskyDecomposition_F32<T>
        : CholeskyDecomposition<T>
        where T : Matrix
    {

        /**
         * Computes the matrix's determinant using the decomposition.
         *
         * @return The determinant.
         */
        Complex_F32 computeDeterminant();

    }
}