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
    public interface CholeskyLDLDecomposition_F64<TMatrix>
        : CholeskyLDLDecomposition<TMatrix>
        where TMatrix : Matrix
    {

        /**
         * Returns the elements in the diagonal matrix
         * @return array with diagonal elements. Array might be larger than the number of elements.
         */
        double[] getDiagonal();
    }
}