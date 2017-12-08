using System;
using SharpMatrix.Data;

namespace SharpMatrix.Sparse.Triplet
{
    //package org.ejml.sparse.triplet;

/**
 * @author Peter Abeles
 */
    public class MatrixFeatures_DSTL
    {

        public static bool isEquals(DMatrixSparseTriplet a, DMatrixSparseTriplet b)
        {
            if (!isSameShape(a, b))
                return false;

            for (int i = 0; i < a.nz_length; i++)
            {
                DMatrixSparseTriplet.Element ea = a.nz_data[i];
                DMatrixSparseTriplet.Element eb = b.nz_data[b.nz_index(ea.row, ea.col)];

                if (eb == null || ea.value != eb.value)
                    return false;
            }
            return true;
        }

        public static bool isEquals(DMatrixSparseTriplet a, DMatrixSparseTriplet b, double tol)
        {
            if (!isSameShape(a, b))
                return false;

            for (int i = 0; i < a.nz_length; i++)
            {
                DMatrixSparseTriplet.Element ea = a.nz_data[i];
                DMatrixSparseTriplet.Element eb = b.nz_data[b.nz_index(ea.row, ea.col)];

                if (eb == null || Math.Abs(ea.value - eb.value) > tol)
                    return false;
            }
            return true;
        }

        public static bool isSameShape(DMatrixSparseTriplet a, DMatrixSparseTriplet b)
        {
            return a.numRows == b.numRows && a.numCols == b.numCols && a.nz_length == b.nz_length;
        }
    }
}