using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row;

namespace SharpMatrix.Dense.Fixed
{
    //package org.ejml.dense.fixed;

/**
 * <p>Matrix features for fixed sized matrices which are 4 x 4 or 4 element vectors.</p>
 * <p>DO NOT MODIFY.  Automatically generated code created by GenerateFixedFeatures</p>
 *
 * @author Peter Abeles
 */
    public class MatrixFeatures_FDF4
    {
        public static bool isIdentical(FMatrix4x4 a, FMatrix4x4 b, float tol)
        {
            if (!MatrixFeatures_FDRM.isIdentical(a.a11, b.a11, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a12, b.a12, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a13, b.a13, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a14, b.a14, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a21, b.a21, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a22, b.a22, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a23, b.a23, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a24, b.a24, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a31, b.a31, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a32, b.a32, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a33, b.a33, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a34, b.a34, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a41, b.a41, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a42, b.a42, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a43, b.a43, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a44, b.a44, tol))
                return false;
            return true;
        }

        public static bool isIdentical(FMatrix4 a, FMatrix4 b, float tol)
        {
            if (!MatrixFeatures_FDRM.isIdentical(a.a1, b.a1, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a2, b.a2, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a3, b.a3, tol))
                return false;
            if (!MatrixFeatures_FDRM.isIdentical(a.a4, b.a4, tol))
                return false;
            return true;
        }

        public static bool hasUncountable(FMatrix4x4 a)
        {
            if (UtilEjml.isUncountable(a.a11 + a.a12 + a.a13 + a.a14))
                return true;
            if (UtilEjml.isUncountable(a.a21 + a.a22 + a.a23 + a.a24))
                return true;
            if (UtilEjml.isUncountable(a.a31 + a.a32 + a.a33 + a.a34))
                return true;
            if (UtilEjml.isUncountable(a.a41 + a.a42 + a.a43 + a.a44))
                return true;
            return false;
        }

        public static bool hasUncountable(FMatrix4 a)
        {
            if (UtilEjml.isUncountable(a.a1))
                return true;
            if (UtilEjml.isUncountable(a.a2))
                return true;
            if (UtilEjml.isUncountable(a.a3))
                return true;
            if (UtilEjml.isUncountable(a.a4))
                return true;
            return false;
        }

    }

}