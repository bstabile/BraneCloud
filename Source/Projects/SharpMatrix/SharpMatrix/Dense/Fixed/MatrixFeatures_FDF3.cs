using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row;

namespace SharpMatrix.Dense.Fixed
{
    //package org.ejml.dense.fixed;

/**
 * <p>Matrix features for fixed sized matrices which are 3 x 3 or 3 element vectors.</p>
 * <p>DO NOT MODIFY.  Automatically generated code created by GenerateFixedFeatures</p>
 *
 * @author Peter Abeles
 */
    public class MatrixFeatures_FDF3
    {
        public static bool isIdentical(FMatrix3x3 a, FMatrix3x3 b, float tol)
        {
            if (!MatrixFeatures_FDRM.isIdentical(a.a11, b.a11, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a12, b.a12, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a13, b.a13, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a21, b.a21, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a22, b.a22, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a23, b.a23, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a31, b.a31, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a32, b.a32, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a33, b.a33, tol))
                return false;
            return true;
        }

        public static bool isIdentical(FMatrix3 a, FMatrix3 b, float tol)
        {
            if (!MatrixFeatures_FDRM.isIdentical(a.a1, b.a1, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a2, b.a2, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a3, b.a3, tol))
                return false;
            return true;
        }

        public static bool hasUncountable(FMatrix3x3 a)
        {
            if (UtilEjml.isUncountable(a.a11 + a.a12 + a.a13))
                return true;
            if (UtilEjml.isUncountable(a.a21 + a.a22 + a.a23))
                return true;
            if (UtilEjml.isUncountable(a.a31 + a.a32 + a.a33))
                return true;
            return false;
        }

        public static bool hasUncountable(FMatrix3 a)
        {
            if (UtilEjml.isUncountable(a.a1))
                return true;
            if (UtilEjml.isUncountable(a.a2))
                return true;
            if (UtilEjml.isUncountable(a.a3))
                return true;
            return false;
        }

    }

}