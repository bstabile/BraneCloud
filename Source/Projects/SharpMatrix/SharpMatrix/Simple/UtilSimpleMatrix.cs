using SharpMatrix.Data;
using SharpMatrix.Ops;

namespace SharpMatrix.Simple
{
    //package org.ejml.simple;

    /**
     * @author Peter Abeles
     */
    public class UtilSimpleMatrix
    {
        /**
         * <p>Converts the block matrix into a SimpleMatrix.</p>
         *
         * @param A Block matrix that is being converted.  Not modified.
         * @return Equivalent SimpleMatrix.
         */
        public static SimpleMatrix<T> convertSimple<T>(DMatrixRBlock A) where T : class, Matrix
        {
            var B = ConvertDMatrixStruct.convert(A, null) as T;

            return SimpleMatrix<T>.wrap(B);
        }
    }
}