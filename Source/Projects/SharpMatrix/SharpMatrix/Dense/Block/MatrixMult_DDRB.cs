using System;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Block
{
    //package org.ejml.dense.block;

/**
 * <p>
 * Matrix multiplication for {@link DMatrixRBlock}.  All sub-matrices must be
 * block aligned.
 * </p>
 * 
 * @author Peter Abeles
 */
    public class MatrixMult_DDRB
    {

        /**
         * <p>
         * Performs a matrix multiplication on {@link DMatrixRBlock} submatrices.<br>
         * <br>
         * c = a * b <br>
         * <br>
         * </p>
         *
         * <p>
         * It is assumed that all submatrices start at the beginning of a block and end at the end of a block.
         * </p>
         *
         * @param blockLength Size of the blocks in the submatrix.
         * @param A A submatrix.  Not modified.
         * @param B A submatrix.  Not modified.
         * @param C Result of the operation.  Modified,
         */
        public static void mult(int blockLength, DSubmatrixD1 A, DSubmatrixD1 B, DSubmatrixD1 C)
        {
            for (int i = A.row0; i < A.row1; i += blockLength)
            {
                int heightA = Math.Min(blockLength, A.row1 - i);

                for (int j = B.col0; j < B.col1; j += blockLength)
                {
                    int widthB = Math.Min(blockLength, B.col1 - j);

                    int indexC = (i - A.row0 + C.row0) * C.original.numCols + (j - B.col0 + C.col0) * heightA;

                    for (int k = A.col0; k < A.col1; k += blockLength)
                    {
                        int widthA = Math.Min(blockLength, A.col1 - k);

                        int indexA = i * A.original.numCols + k * heightA;
                        int indexB = (k - A.col0 + B.row0) * B.original.numCols + j * widthA;

                        if (k == A.col0)
                            InnerMultiplication_DDRB.blockMultSet(A.original.data, B.original.data, C.original.data,
                                indexA, indexB, indexC, heightA, widthA, widthB);
                        else
                            InnerMultiplication_DDRB.blockMultPlus(A.original.data, B.original.data, C.original.data,
                                indexA, indexB, indexC, heightA, widthA, widthB);
                    }
                }
            }
        }

        /**
         * <p>
         * Performs a matrix multiplication on {@link DMatrixRBlock} submatrices.<br>
         * <br>
         * c = c + a * b <br>
         * <br>
         * </p>
         *
         * <p>
         * It is assumed that all submatrices start at the beginning of a block and end at the end of a block.
         * </p>
         *
         * @param blockLength Size of the blocks in the submatrix.
         * @param A A submatrix.  Not modified.
         * @param B A submatrix.  Not modified.
         * @param C Result of the operation.  Modified,
         */
        public static void multPlus(int blockLength, DSubmatrixD1 A, DSubmatrixD1 B, DSubmatrixD1 C)
        {
//        checkInput( blockLength,A,B,C);

            for (int i = A.row0; i < A.row1; i += blockLength)
            {
                int heightA = Math.Min(blockLength, A.row1 - i);

                for (int j = B.col0; j < B.col1; j += blockLength)
                {
                    int widthB = Math.Min(blockLength, B.col1 - j);

                    int indexC = (i - A.row0 + C.row0) * C.original.numCols + (j - B.col0 + C.col0) * heightA;

                    for (int k = A.col0; k < A.col1; k += blockLength)
                    {
                        int widthA = Math.Min(blockLength, A.col1 - k);

                        int indexA = i * A.original.numCols + k * heightA;
                        int indexB = (k - A.col0 + B.row0) * B.original.numCols + j * widthA;

                        InnerMultiplication_DDRB.blockMultPlus(A.original.data, B.original.data, C.original.data,
                            indexA, indexB, indexC, heightA, widthA, widthB);
                    }
                }
            }
        }

        /**
         * <p>
         * Performs a matrix multiplication on {@link DMatrixRBlock} submatrices.<br>
         * <br>
         * c = c - a * b <br>
         * <br>
         * </p>
         *
         * <p>
         * It is assumed that all submatrices start at the beginning of a block and end at the end of a block.
         * </p>
         *
         * @param blockLength Size of the blocks in the submatrix.
         * @param A A submatrix.  Not modified.
         * @param B A submatrix.  Not modified.
         * @param C Result of the operation.  Modified,
         */
        public static void multMinus(int blockLength, DSubmatrixD1 A, DSubmatrixD1 B, DSubmatrixD1 C)
        {
            checkInput(blockLength, A, B, C);

            for (int i = A.row0; i < A.row1; i += blockLength)
            {
                int heightA = Math.Min(blockLength, A.row1 - i);

                for (int j = B.col0; j < B.col1; j += blockLength)
                {
                    int widthB = Math.Min(blockLength, B.col1 - j);

                    int indexC = (i - A.row0 + C.row0) * C.original.numCols + (j - B.col0 + C.col0) * heightA;

                    for (int k = A.col0; k < A.col1; k += blockLength)
                    {
                        int widthA = Math.Min(blockLength, A.col1 - k);

                        int indexA = i * A.original.numCols + k * heightA;
                        int indexB = (k - A.col0 + B.row0) * B.original.numCols + j * widthA;

                        InnerMultiplication_DDRB.blockMultMinus(A.original.data, B.original.data, C.original.data,
                            indexA, indexB, indexC, heightA, widthA, widthB);
                    }
                }
            }
        }

        private static void checkInput(int blockLength, DSubmatrixD1 A, DSubmatrixD1 B, DSubmatrixD1 C)
        {
            int Arow = A.getRows();
            int Acol = A.getCols();
            int Brow = B.getRows();
            int Bcol = B.getCols();
            int Crow = C.getRows();
            int Ccol = C.getCols();

            if (Arow != Crow)
                throw new InvalidOperationException("Mismatch A and C rows");
            if (Bcol != Ccol)
                throw new InvalidOperationException("Mismatch B and C columns");
            if (Acol != Brow)
                throw new InvalidOperationException("Mismatch A columns and B rows");

            if (!MatrixOps_DDRB.blockAligned(blockLength, A))
                throw new InvalidOperationException("Sub-Matrix A is not block aligned");

            if (!MatrixOps_DDRB.blockAligned(blockLength, B))
                throw new InvalidOperationException("Sub-Matrix B is not block aligned");

            if (!MatrixOps_DDRB.blockAligned(blockLength, C))
                throw new InvalidOperationException("Sub-Matrix C is not block aligned");
        }

        /**
         * <p>
         * Performs a matrix multiplication with a transpose on {@link DMatrixRBlock} submatrices.<br>
         * <br>
         * c = a<sup>T</sup> * b <br>
         * <br>
         * </p>
         *
         * <p>
         * It is assumed that all submatrices start at the beginning of a block and end at the end of a block.
         * </p>
         *
         * @param blockLength Size of the blocks in the submatrix.
         * @param A A submatrix.  Not modified.
         * @param B A submatrix.  Not modified.
         * @param C Result of the operation.  Modified,
         */
        public static void multTransA(int blockLength, DSubmatrixD1 A, DSubmatrixD1 B, DSubmatrixD1 C)
        {
            for (int i = A.col0; i < A.col1; i += blockLength)
            {
                int widthA = Math.Min(blockLength, A.col1 - i);

                for (int j = B.col0; j < B.col1; j += blockLength)
                {
                    int widthB = Math.Min(blockLength, B.col1 - j);

                    int indexC = (i - A.col0 + C.row0) * C.original.numCols + (j - B.col0 + C.col0) * widthA;

                    for (int k = A.row0; k < A.row1; k += blockLength)
                    {
                        int heightA = Math.Min(blockLength, A.row1 - k);

                        int indexA = k * A.original.numCols + i * heightA;
                        int indexB = (k - A.row0 + B.row0) * B.original.numCols + j * heightA;

                        if (k == A.row0)
                            InnerMultiplication_DDRB.blockMultSetTransA(A.original.data, B.original.data, C.original.data,
                                indexA, indexB, indexC, heightA, widthA, widthB);
                        else
                            InnerMultiplication_DDRB.blockMultPlusTransA(A.original.data, B.original.data, C.original.data,
                                indexA, indexB, indexC, heightA, widthA, widthB);
                    }
                }
            }
        }

        public static void multPlusTransA(int blockLength, DSubmatrixD1 A, DSubmatrixD1 B, DSubmatrixD1 C)
        {
            for (int i = A.col0; i < A.col1; i += blockLength)
            {
                int widthA = Math.Min(blockLength, A.col1 - i);

                for (int j = B.col0; j < B.col1; j += blockLength)
                {
                    int widthB = Math.Min(blockLength, B.col1 - j);

                    int indexC = (i - A.col0 + C.row0) * C.original.numCols + (j - B.col0 + C.col0) * widthA;

                    for (int k = A.row0; k < A.row1; k += blockLength)
                    {
                        int heightA = Math.Min(blockLength, A.row1 - k);

                        int indexA = k * A.original.numCols + i * heightA;
                        int indexB = (k - A.row0 + B.row0) * B.original.numCols + j * heightA;

                        InnerMultiplication_DDRB.blockMultPlusTransA(A.original.data, B.original.data, C.original.data,
                            indexA, indexB, indexC, heightA, widthA, widthB);
                    }
                }
            }
        }

        public static void multMinusTransA(int blockLength, DSubmatrixD1 A, DSubmatrixD1 B, DSubmatrixD1 C)
        {
            for (int i = A.col0; i < A.col1; i += blockLength)
            {
                int widthA = Math.Min(blockLength, A.col1 - i);

                for (int j = B.col0; j < B.col1; j += blockLength)
                {
                    int widthB = Math.Min(blockLength, B.col1 - j);

                    int indexC = (i - A.col0 + C.row0) * C.original.numCols + (j - B.col0 + C.col0) * widthA;

                    for (int k = A.row0; k < A.row1; k += blockLength)
                    {
                        int heightA = Math.Min(blockLength, A.row1 - k);

                        int indexA = k * A.original.numCols + i * heightA;
                        int indexB = (k - A.row0 + B.row0) * B.original.numCols + j * heightA;

                        InnerMultiplication_DDRB.blockMultMinusTransA(A.original.data, B.original.data, C.original.data,
                            indexA, indexB, indexC, heightA, widthA, widthB);

                    }
                }
            }
        }

        /**
         * <p>
         * Performs a matrix multiplication with a transpose on {@link DMatrixRBlock} submatrices.<br>
         * <br>
         * c = a * b <sup>T</sup> <br>
         * <br>
         * </p>
         *
         * <p>
         * It is assumed that all submatrices start at the beginning of a block and end at the end of a block.
         * </p>
         *
         * @param blockLength Length of the blocks in the submatrix.
         * @param A A submatrix.  Not modified.
         * @param B A submatrix.  Not modified.
         * @param C Result of the operation.  Modified,
         */
        public static void multTransB(int blockLength, DSubmatrixD1 A, DSubmatrixD1 B, DSubmatrixD1 C)
        {
            for (int i = A.row0; i < A.row1; i += blockLength)
            {
                int heightA = Math.Min(blockLength, A.row1 - i);

                for (int j = B.row0; j < B.row1; j += blockLength)
                {
                    int widthC = Math.Min(blockLength, B.row1 - j);

                    int indexC = (i - A.row0 + C.row0) * C.original.numCols + (j - B.row0 + C.col0) * heightA;

                    for (int k = A.col0; k < A.col1; k += blockLength)
                    {
                        int widthA = Math.Min(blockLength, A.col1 - k);

                        int indexA = i * A.original.numCols + k * heightA;
                        int indexB = j * B.original.numCols + (k - A.col0 + B.col0) * widthC;

                        if (k == A.col0)
                            InnerMultiplication_DDRB.blockMultSetTransB(A.original.data, B.original.data, C.original.data,
                                indexA, indexB, indexC, heightA, widthA, widthC);
                        else
                            InnerMultiplication_DDRB.blockMultPlusTransB(A.original.data, B.original.data, C.original.data,
                                indexA, indexB, indexC, heightA, widthA, widthC);
                    }
                }
            }
        }

    }
}