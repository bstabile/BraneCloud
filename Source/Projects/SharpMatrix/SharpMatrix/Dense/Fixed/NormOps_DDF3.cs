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
    public class NormOps_DDF3
    {
        public static void normalizeF(DMatrix3x3 M)
        {
            double val = normF(M);
            CommonOps_DDF3.divide(M, val);
        }

        public static void normalizeF(DMatrix3 M)
        {
            double val = normF(M);
            CommonOps_DDF3.divide(M, val);
        }

        public static double fastNormF(DMatrix3x3 M)
        {
            double sum = 0;

            sum += M.a11 * M.a11 + M.a12 * M.a12 + M.a13 * M.a13;
            sum += M.a21 * M.a21 + M.a22 * M.a22 + M.a23 * M.a23;
            sum += M.a31 * M.a31 + M.a32 * M.a32 + M.a33 * M.a33;

            return Math.Sqrt(sum);
        }

        public static double fastNormF(DMatrix3 M)
        {
            double sum = M.a1 * M.a1 + M.a2 * M.a2 + M.a3 * M.a3;
            return Math.Sqrt(sum);
        }

        public static double normF(DMatrix3x3 M)
        {
            double scale = CommonOps_DDF3.elementMaxAbs(M);

            if (scale == 0.0)
                return 0.0;

            double a11 = M.a11 / scale, a12 = M.a12 / scale, a13 = M.a13 / scale;
            double a21 = M.a21 / scale, a22 = M.a22 / scale, a23 = M.a23 / scale;
            double a31 = M.a31 / scale, a32 = M.a32 / scale, a33 = M.a33 / scale;

            double sum = 0;
            sum += a11 * a11 + a12 * a12 + a13 * a13;
            sum += a21 * a21 + a22 * a22 + a23 * a23;
            sum += a31 * a31 + a32 * a32 + a33 * a33;

            return scale * Math.Sqrt(sum);
        }

        public static double normF(DMatrix3 M)
        {
            double scale = CommonOps_DDF3.elementMaxAbs(M);

            if (scale == 0.0)
                return 0.0;

            double a1 = M.a1 / scale, a2 = M.a2 / scale, a3 = M.a3 / scale;
            double sum = a1 * a1 + a2 * a2 + a3 * a3;

            return scale * Math.Sqrt(sum);
        }

    }
}
