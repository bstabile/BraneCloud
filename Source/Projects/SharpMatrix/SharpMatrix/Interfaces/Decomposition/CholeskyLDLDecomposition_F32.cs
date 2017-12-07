using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * <p>
 * Implementation of {@link CholeskyDecomposition} for 32-bit floats.
 * </p>
 *
 * @author Peter Abeles
 */
    public interface CholeskyLDLDecomposition_F32<T>
        : CholeskyLDLDecomposition<T>
        where T : Matrix
    {

        /**
         * Returns the elements in the diagonal matrix
         * @return array with diagonal elements. Array might be larger than the number of elements.
         */
        float[] getDiagonal();
    }
}