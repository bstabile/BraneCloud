using System;
using SharpMatrix.Data;

namespace SharpMatrix.Dense.Row.Decomposition
{
    //package org.ejml.dense.row.decomposition;

/**
 * Helper functions for generic decompsotions.
 *
 * @author Peter Abeles
 */
    public class UtilDecompositons_FDRM
    {

        public static FMatrixRMaj checkIdentity(FMatrixRMaj A, int numRows, int numCols)
        {
            if (A == null)
            {
                return CommonOps_FDRM.identity(numRows, numCols);
            }
            else if (numRows != A.numRows || numCols != A.numCols)
                throw new ArgumentException("Input is not " + numRows + " x " + numCols + " matrix");
            else
                CommonOps_FDRM.setIdentity(A);
            return A;
        }

        public static FMatrixRMaj checkZeros(FMatrixRMaj A, int numRows, int numCols)
        {
            if (A == null)
            {
                return new FMatrixRMaj(numRows, numCols);
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
        public static FMatrixRMaj checkZerosLT(FMatrixRMaj A, int numRows, int numCols)
        {
            if (A == null)
            {
                return new FMatrixRMaj(numRows, numCols);
            }
            else if (numRows != A.numRows || numCols != A.numCols)
                throw new ArgumentException("Input is not " + numRows + " x " + numCols + " matrix");
            else
            {
                for (int i = 0; i < A.numRows; i++)
                {
                    int index = i * A.numCols;
                    int end = index + Math.Min(i, A.numCols);
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
        public static FMatrixRMaj checkZerosUT(FMatrixRMaj A, int numRows, int numCols)
        {
            if (A == null)
            {
                return new FMatrixRMaj(numRows, numCols);
            }
            else if (numRows != A.numRows || numCols != A.numCols)
                throw new ArgumentException("Input is not " + numRows + " x " + numCols + " matrix");
            else
            {
                int maxRows = Math.Min(A.numRows, A.numCols);
                for (int i = 0; i < maxRows; i++)
                {
                    int index = i * A.numCols + i + 1;
                    int end = i * A.numCols + A.numCols;
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