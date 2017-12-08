using System;
using SharpMatrix.Data;

namespace SharpMatrix.Dense.Row.Misc
{
    //package org.ejml.dense.row.misc;

/**
 * Algorithms for transposing row complex matrices
 *
 * @author Peter Abeles
 */
    public class TransposeAlgs_ZDRM
    {
        /**
         * In-place transpose for a square matrix.  On most architectures it is faster than the standard transpose
         * algorithm, but on most modern computers it's slower than block transpose.
         *
         * @param mat The matrix that is transposed in-place.  Modified.
         */
        public static void square(ZMatrixRMaj mat)
        {
            int index = 2;
            int rowStride = mat.getRowStride();
            int indexEnd = rowStride;
            for (int i = 0;
                i < mat.numRows;
                i++, index += (i + 1) * 2, indexEnd += rowStride)
            {

                int indexOther = (i + 1) * rowStride + i * 2;
                for (; index < indexEnd; index += 2, indexOther += rowStride)
                {
                    double real = mat.data[index];
                    double img = mat.data[index + 1];

                    mat.data[index] = mat.data[indexOther];
                    mat.data[index + 1] = mat.data[indexOther + 1];
                    mat.data[indexOther] = real;
                    mat.data[indexOther + 1] = img;
                }
            }
        }

        public static void squareConjugate(ZMatrixRMaj mat)
        {
            int index = 2;
            int rowStride = mat.getRowStride();
            int indexEnd = rowStride;
            for (int i = 0;
                i < mat.numRows;
                i++, index += (i + 1) * 2, indexEnd += rowStride)
            {

                mat.data[index - 1] = -mat.data[index - 1];

                int indexOther = (i + 1) * rowStride + i * 2;
                for (; index < indexEnd; index += 2, indexOther += rowStride)
                {
                    double real = mat.data[index];
                    double img = mat.data[index + 1];

                    mat.data[index] = mat.data[indexOther];
                    mat.data[index + 1] = -mat.data[indexOther + 1];
                    mat.data[indexOther] = real;
                    mat.data[indexOther + 1] = -img;
                }
            }
        }

        /**
         * A straight forward transpose.  Good for small non-square matrices.
         *
         * @param A Original matrix.  Not modified.
         * @param A_tran Transposed matrix.  Modified.
         */
        public static void standard(ZMatrixRMaj A, ZMatrixRMaj A_tran)
        {
            int index = 0;
            int rowStrideTran = A_tran.getRowStride();
            int rowStride = A.getRowStride();
            for (int i = 0; i < A_tran.numRows; i++)
            {
                int index2 = i * 2;

                int end = index + rowStrideTran;
                while (index < end)
                {
                    A_tran.data[index++] = A.data[index2];
                    A_tran.data[index++] = A.data[index2 + 1];
                    index2 += rowStride;
                }
            }
        }

        /**
         * A straight forward conjugate transpose.  Good for small non-square matrices.
         *
         * @param A Original matrix.  Not modified.
         * @param A_tran Transposed matrix.  Modified.
         */
        public static void standardConjugate(ZMatrixRMaj A, ZMatrixRMaj A_tran)
        {
            int index = 0;
            int rowStrideTran = A_tran.getRowStride();
            int rowStride = A.getRowStride();
            for (int i = 0; i < A_tran.numRows; i++)
            {
                int index2 = i * 2;

                int end = index + rowStrideTran;
                while (index < end)
                {
                    A_tran.data[index++] = A.data[index2];
                    A_tran.data[index++] = -A.data[index2 + 1];
                    index2 += rowStride;
                }
            }
        }
    }
}