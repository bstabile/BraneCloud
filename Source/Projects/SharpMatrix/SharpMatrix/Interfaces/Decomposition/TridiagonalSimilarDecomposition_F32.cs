using SharpMatrix.Data;

namespace SharpMatrix.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * <p>Implementation of {@link TridiagonalSimilarDecomposition} for 32-bit floats</p>
 *
 * @author Peter Abeles
 */
    public interface TridiagonalSimilarDecomposition_F32<T>
        : TridiagonalSimilarDecomposition<T>
        where T : Matrix
    {

        /**
         * Extracts the diagonal and off diagonal elements of the decomposed tridiagonal matrix.
         * Since it is symmetric only one off diagonal array is returned.
         *
         * @param diag Diagonal elements. Modified.
         * @param off off diagonal elements. Modified.
         */
        void getDiagonal(float[] diag, float[] off);
    }
}