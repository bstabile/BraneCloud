using System;
using SharpMatrix.Data;

namespace SharpMatrix.Dense.Row
{
    //package org.ejml.dense.row;

/**
 * @author Peter Abeles
 */
    public class NormOps_CDRM
    {
        /**
         * <p>
         * Computes the Frobenius matrix norm:<br>
         * <br>
         * normF = Sqrt{  &sum;<sub>i=1:m</sub> &sum;<sub>j=1:n</sub> { a<sub>ij</sub><sup>2</sup>}   }
         * </p>
         * <p>
         * This is equivalent to the element wise p=2 norm.
         * </p>
         *
         * @param a The matrix whose norm is computed.  Not modified.
         * @return The norm's value.
         */
        public static float normF(CMatrixRMaj a)
        {
            float total = 0;

            float scale = CommonOps_CDRM.elementMaxAbs(a);

            if (scale == 0.0f)
                return 0.0f;

            int size = a.getDataLength();

            for (int i = 0; i < size; i += 2)
            {
                float real = a.data[i] / scale;
                float imag = a.data[i + 1] / scale;

                total += real * real + imag * imag;
            }

            return scale * (float) Math.Sqrt(total);
        }
    }
}