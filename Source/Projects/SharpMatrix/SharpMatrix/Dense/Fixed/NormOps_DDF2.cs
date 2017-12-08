using System;
using SharpMatrix.Data;

namespace SharpMatrix.Dense.Fixed
{
    //package org.ejml.dense.fixed;

/**
 * <p>Matrix norm related operations for fixed sized matrices of size 2.</p>
 * <p>DO NOT MODIFY.  Automatically generated code created by GenerateFixedNormOps</p>
 *
 * @author Peter Abeles
 */
    public class NormOps_DDF2
    {
        public static void normalizeF(DMatrix2x2 M)
        {
            double val = normF(M);
            CommonOps_DDF2.divide(M, val);
        }

        public static void normalizeF(DMatrix2 M)
        {
            double val = normF(M);
            CommonOps_DDF2.divide(M, val);
        }

        public static double fastNormF(DMatrix2x2 M)
        {
            double sum = 0;

            sum += M.a11 * M.a11 + M.a12 * M.a12;
            sum += M.a21 * M.a21 + M.a22 * M.a22;

            return Math.Sqrt(sum);
        }

        public static double fastNormF(DMatrix2 M)
        {
            double sum = M.a1 * M.a1 + M.a2 * M.a2;
            return Math.Sqrt(sum);
        }

        public static double normF(DMatrix2x2 M)
        {
            double scale = CommonOps_DDF2.elementMaxAbs(M);

            if (scale == 0.0)
                return 0.0;

            double a11 = M.a11 / scale, a12 = M.a12 / scale;
            double a21 = M.a21 / scale, a22 = M.a22 / scale;

            double sum = 0;
            sum += a11 * a11 + a12 * a12;
            sum += a21 * a21 + a22 * a22;

            return scale * Math.Sqrt(sum);
        }

        public static double normF(DMatrix2 M)
        {
            double scale = CommonOps_DDF2.elementMaxAbs(M);

            if (scale == 0.0)
                return 0.0;

            double a1 = M.a1 / scale, a2 = M.a2 / scale;
            double sum = a1 * a1 + a2 * a2;

            return scale * Math.Sqrt(sum);
        }

    }
}
