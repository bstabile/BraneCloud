using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row;

namespace SharpMatrix.Dense.Fixed
{
    //package org.ejml.dense.fixed;

/**
 * <p>Matrix features for fixed sized matrices which are 6 x 6 or 6 element vectors.</p>
 * <p>DO NOT MODIFY.  Automatically generated code created by GenerateFixedFeatures</p>
 *
 * @author Peter Abeles
 */
    public class MatrixFeatures_DDF6
    {
        public static bool isIdentical(DMatrix6x6 a, DMatrix6x6 b, double tol)
        {
            if (!MatrixFeatures_DDRM.isIdentical(a.a11, b.a11, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a12, b.a12, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a13, b.a13, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a14, b.a14, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a15, b.a15, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a16, b.a16, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a21, b.a21, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a22, b.a22, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a23, b.a23, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a24, b.a24, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a25, b.a25, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a26, b.a26, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a31, b.a31, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a32, b.a32, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a33, b.a33, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a34, b.a34, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a35, b.a35, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a36, b.a36, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a41, b.a41, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a42, b.a42, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a43, b.a43, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a44, b.a44, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a45, b.a45, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a46, b.a46, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a51, b.a51, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a52, b.a52, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a53, b.a53, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a54, b.a54, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a55, b.a55, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a56, b.a56, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a61, b.a61, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a62, b.a62, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a63, b.a63, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a64, b.a64, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a65, b.a65, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a66, b.a66, tol))
                return false;
            return true;
        }

        public static bool isIdentical(DMatrix6 a, DMatrix6 b, double tol)
        {
            if (!MatrixFeatures_DDRM.isIdentical(a.a1, b.a1, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a2, b.a2, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a3, b.a3, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a4, b.a4, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a5, b.a5, tol))
                return false;
            if (!MatrixFeatures_DDRM.isIdentical(a.a6, b.a6, tol))
                return false;
            return true;
        }

        public static bool hasUncountable(DMatrix6x6 a)
        {
            if (UtilEjml.isUncountable(a.a11 + a.a12 + a.a13 + a.a14 + a.a15 + a.a16))
                return true;
            if (UtilEjml.isUncountable(a.a21 + a.a22 + a.a23 + a.a24 + a.a25 + a.a26))
                return true;
            if (UtilEjml.isUncountable(a.a31 + a.a32 + a.a33 + a.a34 + a.a35 + a.a36))
                return true;
            if (UtilEjml.isUncountable(a.a41 + a.a42 + a.a43 + a.a44 + a.a45 + a.a46))
                return true;
            if (UtilEjml.isUncountable(a.a51 + a.a52 + a.a53 + a.a54 + a.a55 + a.a56))
                return true;
            if (UtilEjml.isUncountable(a.a61 + a.a62 + a.a63 + a.a64 + a.a65 + a.a66))
                return true;
            return false;
        }

        public static bool hasUncountable(DMatrix6 a)
        {
            if (UtilEjml.isUncountable(a.a1))
                return true;
            if (UtilEjml.isUncountable(a.a2))
                return true;
            if (UtilEjml.isUncountable(a.a3))
                return true;
            if (UtilEjml.isUncountable(a.a4))
                return true;
            if (UtilEjml.isUncountable(a.a5))
                return true;
            if (UtilEjml.isUncountable(a.a6))
                return true;
            return false;
        }

    }

}