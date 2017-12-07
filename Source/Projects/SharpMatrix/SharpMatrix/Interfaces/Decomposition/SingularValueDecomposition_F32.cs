
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * <p>Implementation of {@link SingularValueDecomposition} for 64-bit floats.
 * </p>
 *
 * @author Peter Abeles
 */
    public interface SingularValueDecomposition_F32<T>
        : SingularValueDecomposition<T>
        where T : Matrix
    {

        /**
         * Returns the singular values.  This is the diagonal elements of the W matrix in the decomposition.
         * <b>Ordering of singular values is not guaranteed.</b>.
         * 
         * @return Singular values. Note this array can be longer than the number of singular values.
         * Extra elements have no meaning.
         */
        float[] getSingularValues();
    }
}