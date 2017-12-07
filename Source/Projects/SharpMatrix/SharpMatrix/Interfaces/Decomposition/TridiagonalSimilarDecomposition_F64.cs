using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * <p>Implementation of {@link TridiagonalSimilarDecomposition} for 64-bit floats</p>
 *
 * @author Peter Abeles
 */
    public interface TridiagonalSimilarDecomposition_F64<TMatrix> : TridiagonalSimilarDecomposition<TMatrix>
        where TMatrix : Matrix
    {

        /**
         * Extracts the diagonal and off diagonal elements of the decomposed tridiagonal matrix.
         * Since it is symmetric only one off diagonal array is returned.
         *
         * @param diag Diagonal elements. Modified.
         * @param off off diagonal elements. Modified.
         */
        void getDiagonal(double[]diag, double[]off);
    }
}
