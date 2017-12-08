using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row;

namespace SharpMatrix.Dense.Fixed
{
    //package org.ejml.dense.fixed;

/**
 * <p>Matrix features for fixed sized matrices which are 2 x 2 or 2 element vectors.</p>
 * <p>DO NOT MODIFY.  Automatically generated code created by GenerateFixedFeatures</p>
 *
 * @author Peter Abeles
 */
    public class MatrixFeatures_DDF2
    {
        public static bool isIdentical(DMatrix2x2 a, DMatrix2x2 b, double tol)
        {
            if (!MatrixFeatures_DDRM.isIdentical(a.a11, b.a11, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a12, b.a12, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a21, b.a21, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a22, b.a22, tol))
                return false;
            return true;
        }

        public static bool isIdentical(DMatrix2 a, DMatrix2 b, double tol)
        {
            if (!MatrixFeatures_DDRM.isIdentical(a.a1, b.a1, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a2, b.a2, tol))
                return false;
            return true;
        }

        public static bool hasUncountable(DMatrix2x2 a)
        {
            if (UtilEjml.isUncountable(a.a11 + a.a12))
                return true;
            if (UtilEjml.isUncountable(a.a21 + a.a22))
                return true;
            return false;
        }

        public static bool hasUncountable(DMatrix2 a)
        {
            if (UtilEjml.isUncountable(a.a1))
                return true;
            if (UtilEjml.isUncountable(a.a2))
                return true;
            return false;
        }

    }
}
