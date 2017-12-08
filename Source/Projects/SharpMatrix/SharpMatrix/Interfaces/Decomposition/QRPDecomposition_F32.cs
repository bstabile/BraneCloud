using System;
using SharpMatrix.Data;

namespace SharpMatrix.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * <p>
 * Implementation of {@link QRPDecomposition} for 3-bit floats
 * </p>
 *
 * @author Peter Abeles
 */
    public interface QRPDecomposition_F32<T> : QRPDecomposition<T>
        where T : Matrix
    {
        /**
         * <p>
         * Specifies the threshold used to flag a column as being singular.  The specified threshold is relative
         * and will very depending on the system.  The default value is UtilEJML.F_EPS.
         * </p>
         *
         * @param threshold Singular threshold.
         */
        void setSingularThreshold(float threshold);
    }
}