using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition
{
    //package org.ejml.dense.row.decompose;

/**
 * Helper functions for generic decompsotions.
 *
 * @author Peter Abeles
 */
    public class UtilDecompositons_ZDRM
    {

        public static ZMatrixRMaj checkIdentity(ZMatrixRMaj A, int numRows, int numCols)
        {
            if (A == null)
            {
                return CommonOps_ZDRM.identity(numRows, numCols);
            }
            else if (numRows != A.numRows || numCols != A.numCols)
                throw new ArgumentException("Input is not " + numRows + " x " + numCols + " matrix");
            else
                CommonOps_ZDRM.setIdentity(A);
            return A;
        }

        public static ZMatrixRMaj checkZeros(ZMatrixRMaj A, int numRows, int numCols)
        {
            if (A == null)
            {
                return new ZMatrixRMaj(numRows, numCols);
            }
            else if (numRows != A.numRows || numCols != A.numCols)
                throw new ArgumentException("Input is not " + numRows + " x " + numCols + " matrix");
            else
                A.zero();
            return A;
        }

        /**
         * Creates a zeros matrix only if A does not already exist.  If it does exist it will fill
         * the lower triangular portion with zeros.
         */
        public static ZMatrixRMaj checkZerosLT(ZMatrixRMaj A, int numRows, int numCols)
        {
            if (A == null)
            {
                return new ZMatrixRMaj(numRows, numCols);
            }
            else if (numRows != A.numRows || numCols != A.numCols)
                throw new ArgumentException("Input is not " + numRows + " x " + numCols + " matrix");
            else
            {
                for (int i = 0; i < A.numRows; i++)
                {
                    int index = i * A.numCols * 2;
                    int end = index + Math.Min(i, A.numCols) * 2;
                    ;
                    while (index < end)
                    {
                        A.data[index++] = 0;
                    }
                }
            }
            return A;
        }

        /**
         * Creates a zeros matrix only if A does not already exist.  If it does exist it will fill
         * the upper triangular portion with zeros.
         */
        public static ZMatrixRMaj checkZerosUT(ZMatrixRMaj A, int numRows, int numCols)
        {
            if (A == null)
            {
                return new ZMatrixRMaj(numRows, numCols);
            }
            else if (numRows != A.numRows || numCols != A.numCols)
                throw new ArgumentException("Input is not " + numRows + " x " + numCols + " matrix");
            else
            {
                int maxRows = Math.Min(A.numRows, A.numCols);
                for (int i = 0; i < maxRows; i++)
                {
                    int index = (i * A.numCols + i + 1) * 2;
                    int end = (i * A.numCols + A.numCols) * 2;
                    while (index < end)
                    {
                        A.data[index++] = 0;
                    }
                }
            }
            return A;
        }

    }
}