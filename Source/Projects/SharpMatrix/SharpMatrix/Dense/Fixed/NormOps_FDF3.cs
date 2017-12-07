using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Fixed
{
    //package org.ejml.dense.fixed;

/**
 * <p>Matrix norm related operations for fixed sized matrices of size 3.</p>
 * <p>DO NOT MODIFY.  Automatically generated code created by GenerateFixedNormOps</p>
 *
 * @author Peter Abeles
 */
    public class NormOps_FDF3
    {
        public static void normalizeF(FMatrix3x3 M)
        {
            float val = normF(M);
            CommonOps_FDF3.divide(M, val);
        }

        public static void normalizeF(FMatrix3 M)
        {
            float val = normF(M);
            CommonOps_FDF3.divide(M, val);
        }

        public static float fastNormF(FMatrix3x3 M)
        {
            float sum = 0;

            sum += M.a11 * M.a11 + M.a12 * M.a12 + M.a13 * M.a13;
            sum += M.a21 * M.a21 + M.a22 * M.a22 + M.a23 * M.a23;
            sum += M.a31 * M.a31 + M.a32 * M.a32 + M.a33 * M.a33;

            return (float) Math.Sqrt(sum);
        }

        public static float fastNormF(FMatrix3 M)
        {
            float sum = M.a1 * M.a1 + M.a2 * M.a2 + M.a3 * M.a3;
            return (float) Math.Sqrt(sum);
        }

        public static float normF(FMatrix3x3 M)
        {
            float scale = CommonOps_FDF3.elementMaxAbs(M);

            if (scale == 0.0f)
                return 0.0f;

            float a11 = M.a11 / scale, a12 = M.a12 / scale, a13 = M.a13 / scale;
            float a21 = M.a21 / scale, a22 = M.a22 / scale, a23 = M.a23 / scale;
            float a31 = M.a31 / scale, a32 = M.a32 / scale, a33 = M.a33 / scale;

            float sum = 0;
            sum += a11 * a11 + a12 * a12 + a13 * a13;
            sum += a21 * a21 + a22 * a22 + a23 * a23;
            sum += a31 * a31 + a32 * a32 + a33 * a33;

            return scale * (float) Math.Sqrt(sum);
        }

        public static float normF(FMatrix3 M)
        {
            float scale = CommonOps_FDF3.elementMaxAbs(M);

            if (scale == 0.0f)
                return 0.0f;

            float a1 = M.a1 / scale, a2 = M.a2 / scale, a3 = M.a3 / scale;
            float sum = a1 * a1 + a2 * a2 + a3 * a3;

            return scale * (float) Math.Sqrt(sum);
        }

    }

}