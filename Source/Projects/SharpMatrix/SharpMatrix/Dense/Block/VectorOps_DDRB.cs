using System;

namespace SharpMatrix.Dense.Block
{
    //package org.ejml.dense.block;

/**
 * <p>
 * Math operations for inner vectors (row and column) inside of block matrices:<br>
 * <br>
 * scale: b<sub>i</sub> = &alpha;*a<sub>i</sub><br>
 * div:  <sub>i</sub> = a<sub>i</sub>/&alpha;<br>
 * add: c<sub>i</sub> = &alpha;*a<sub>i</sub> + &beta;B<sub>i</sub><br>
 * dot: c = sum a<sub>i</sub>*b<sub>i</sub><br>
 * </p>
 *
 * <p>
 * All submatrices must be block aligned.  All offsets and end indexes are relative to the beginning of each
 * submatrix.
 * </p>
 *
 * @author Peter Abeles
 */
    public class VectorOps_DDRB
    {

        /**
         * <p>
         * Row vector scale:<br>
         * scale: b<sub>i</sub> = &alpha;*a<sub>i</sub><br>
         * where 'a' and 'b' are row vectors within the row block vector A and B.
         * </p>
         *
         * @param A submatrix. Not modified.
         * @param rowA which row in A the vector is contained in.
         * @param alpha scale factor.
         * @param B submatrix that the results are written to.  Modified.
         * @param offset Index at which the vectors start at.
         * @param end Index at which the vectors end at.
         */
        public static void scale_row(int blockLength,
            DSubmatrixD1 A, int rowA,
            double alpha, DSubmatrixD1 B, int rowB,
            int offset, int end)
        {
            double[] dataA = A.original.data;
            double[] dataB = B.original.data;

            // handle the case where offset is more than a block
            int startI = offset - offset % blockLength;
            offset = offset % blockLength;

            // handle rows in any block
            int rowBlockA = A.row0 + rowA - rowA % blockLength;
            rowA = rowA % blockLength;
            int rowBlockB = B.row0 + rowB - rowB % blockLength;
            rowB = rowB % blockLength;

            int heightA = Math.Min(blockLength, A.row1 - rowBlockA);
            int heightB = Math.Min(blockLength, B.row1 - rowBlockB);

            for (int i = startI; i < end; i += blockLength)
            {
                int segment = Math.Min(blockLength, end - i);

                int widthA = Math.Min(blockLength, A.col1 - A.col0 - i);
                int widthB = Math.Min(blockLength, B.col1 - B.col0 - i);

                int indexA = rowBlockA * A.original.numCols + (A.col0 + i) * heightA + rowA * widthA;
                int indexB = rowBlockB * B.original.numCols + (B.col0 + i) * heightB + rowB * widthB;

                if (i == startI)
                {
                    indexA += offset;
                    indexB += offset;

                    for (int j = offset; j < segment; j++)
                    {
                        dataB[indexB++] = alpha * dataA[indexA++];
                    }
                }
                else
                {
                    for (int j = 0; j < segment; j++)
                    {
                        dataB[indexB++] = alpha * dataA[indexA++];
                    }
                }
            }
        }

        /**
         * <p>
         * Row vector divide:<br>
         * div: b<sub>i</sub> = a<sub>i</sub>/&alpha;<br>
         * where 'a' and 'b' are row vectors within the row block vector A and B.
         * </p>
         *
         * @param A submatrix. Not modified.
         * @param rowA which row in A the vector is contained in.
         * @param alpha scale factor.
         * @param B submatrix that the results are written to.  Modified.
         * @param offset Index at which the vectors start at.
         * @param end Index at which the vectors end at.
         */
        public static void div_row(int blockLength,
            DSubmatrixD1 A, int rowA,
            double alpha, DSubmatrixD1 B, int rowB,
            int offset, int end)
        {
            double[] dataA = A.original.data;
            double[] dataB = B.original.data;

            // handle the case where offset is more than a block
            int startI = offset - offset % blockLength;
            offset = offset % blockLength;

            // handle rows in any block
            int rowBlockA = A.row0 + rowA - rowA % blockLength;
            rowA = rowA % blockLength;
            int rowBlockB = B.row0 + rowB - rowB % blockLength;
            rowB = rowB % blockLength;

            int heightA = Math.Min(blockLength, A.row1 - rowBlockA);
            int heightB = Math.Min(blockLength, B.row1 - rowBlockB);

            for (int i = startI; i < end; i += blockLength)
            {
                int segment = Math.Min(blockLength, end - i);

                int widthA = Math.Min(blockLength, A.col1 - A.col0 - i);
                int widthB = Math.Min(blockLength, B.col1 - B.col0 - i);

                int indexA = rowBlockA * A.original.numCols + (A.col0 + i) * heightA + rowA * widthA;
                int indexB = rowBlockB * B.original.numCols + (B.col0 + i) * heightB + rowB * widthB;

                if (i == startI)
                {
                    indexA += offset;
                    indexB += offset;

                    for (int j = offset; j < segment; j++)
                    {
                        dataB[indexB++] = dataA[indexA++] / alpha;
                    }
                }
                else
                {
                    for (int j = 0; j < segment; j++)
                    {
                        dataB[indexB++] = dataA[indexA++] / alpha;
                    }
                }
            }
        }

        /**
         * <p>
         * Row vector add:<br>
         * add: c<sub>i</sub> = &alpha;*a<sub>i</sub> + &beta;B<sub>i</sub><br>
         * where 'a', 'b', and 'c' are row vectors within the row block vectors of A, B, and C respectively.
         * </p>
         *
         * @param blockLength Length of each inner matrix block.
         * @param A submatrix. Not modified.
         * @param rowA which row in A the vector is contained in.
         * @param alpha scale factor of A
         * @param B submatrix. Not modified.
         * @param rowB which row in B the vector is contained in.
         * @param beta scale factor of B
         * @param C submatrix where the results are written to. Modified.
         * @param rowC which row in C is the vector contained.
         * @param offset Index at which the vectors start at.
         * @param end Index at which the vectors end at.
         */
        public static void add_row(int blockLength,
            DSubmatrixD1 A, int rowA, double alpha,
            DSubmatrixD1 B, int rowB, double beta,
            DSubmatrixD1 C, int rowC,
            int offset, int end)
        {
            int heightA = Math.Min(blockLength, A.row1 - A.row0);
            int heightB = Math.Min(blockLength, B.row1 - B.row0);
            int heightC = Math.Min(blockLength, C.row1 - C.row0);

            // handle the case where offset is more than a block
            int startI = offset - offset % blockLength;
            offset = offset % blockLength;

            double[] dataA = A.original.data;
            double[] dataB = B.original.data;
            double[] dataC = C.original.data;

            for (int i = startI; i < end; i += blockLength)
            {
                int segment = Math.Min(blockLength, end - i);

                int widthA = Math.Min(blockLength, A.col1 - A.col0 - i);
                int widthB = Math.Min(blockLength, B.col1 - B.col0 - i);
                int widthC = Math.Min(blockLength, C.col1 - C.col0 - i);

                int indexA = A.row0 * A.original.numCols + (A.col0 + i) * heightA + rowA * widthA;
                int indexB = B.row0 * B.original.numCols + (B.col0 + i) * heightB + rowB * widthB;
                int indexC = C.row0 * C.original.numCols + (C.col0 + i) * heightC + rowC * widthC;

                if (i == startI)
                {
                    indexA += offset;
                    indexB += offset;
                    indexC += offset;

                    for (int j = offset; j < segment; j++)
                    {
                        dataC[indexC++] = alpha * dataA[indexA++] + beta * dataB[indexB++];
                    }
                }
                else
                {
                    for (int j = 0; j < segment; j++)
                    {
                        dataC[indexC++] = alpha * dataA[indexA++] + beta * dataB[indexB++];
                    }
                }
            }
        }

        /**
         * <p>
         * Row vector dot/inner product:<br>
         * dot: c = sum a<sub>i</sub>*b<sub>i</sub><br>
         * where 'a' and 'b' are row vectors within the row block vector A and B, and 'c' is a scalar.
         * </p>
         *
         * @param A submatrix. Not modified.
         * @param rowA which row in A the vector is contained in.
         * @param B submatrix. Not modified.
         * @param rowB which row in B the vector is contained in.
         * @param offset Index at which the vectors start at.
         * @param end Index at which the vectors end at.
         * @return Results of the dot product.
         */
        public static double dot_row(int blockLength,
            DSubmatrixD1 A, int rowA,
            DSubmatrixD1 B, int rowB,
            int offset, int end)
        {


            // handle the case where offset is more than a block
            int startI = offset - offset % blockLength;
            offset = offset % blockLength;

            double[] dataA = A.original.data;
            double[] dataB = B.original.data;

            double total = 0;

            // handle rows in any block
            int rowBlockA = A.row0 + rowA - rowA % blockLength;
            rowA = rowA % blockLength;
            int rowBlockB = B.row0 + rowB - rowB % blockLength;
            rowB = rowB % blockLength;

            int heightA = Math.Min(blockLength, A.row1 - rowBlockA);
            int heightB = Math.Min(blockLength, B.row1 - rowBlockB);

            if (A.col1 - A.col0 != B.col1 - B.col0)
                throw new InvalidOperationException();

            for (int i = startI; i < end; i += blockLength)
            {
                int segment = Math.Min(blockLength, end - i);

                int widthA = Math.Min(blockLength, A.col1 - A.col0 - i);
                int widthB = Math.Min(blockLength, B.col1 - B.col0 - i);

                int indexA = rowBlockA * A.original.numCols + (A.col0 + i) * heightA + rowA * widthA;
                int indexB = rowBlockB * B.original.numCols + (B.col0 + i) * heightB + rowB * widthB;

                if (i == startI)
                {
                    indexA += offset;
                    indexB += offset;

                    for (int j = offset; j < segment; j++)
                    {
                        total += dataB[indexB++] * dataA[indexA++];
                    }
                }
                else
                {
                    for (int j = 0; j < segment; j++)
                    {
                        total += dataB[indexB++] * dataA[indexA++];
                    }
                }
            }

            return total;
        }

        /**
         * <p>
         * vector dot/inner product from one row vector and one column vector:<br>
         * dot: c = sum a<sub>i</sub>*b<sub>i</sub><br>
         * where 'a' is a row vector 'b' is a column vectors within the row block vector A and B, and 'c' is a scalar.
         * </p>
         *
         * @param A block row vector. Not modified.
         * @param rowA which row in A the vector is contained in.
         * @param B block column vector. Not modified.
         * @param colB which column in B is the vector contained in.
         * @param offset Index at which the vectors start at.
         * @param end Index at which the vectors end at.
         * @return Results of the dot product.
         */
        public static double dot_row_col(int blockLength,
            DSubmatrixD1 A, int rowA,
            DSubmatrixD1 B, int colB,
            int offset, int end)
        {


            // handle the case where offset is more than a block
            int startI = offset - offset % blockLength;
            offset = offset % blockLength;

            double[] dataA = A.original.data;
            double[] dataB = B.original.data;

            double total = 0;

            // handle rows in any block
            int rowBlockA = A.row0 + rowA - rowA % blockLength;
            rowA = rowA % blockLength;
            int colBlockB = B.col0 + colB - colB % blockLength;
            colB = colB % blockLength;

            int heightA = Math.Min(blockLength, A.row1 - rowBlockA);
            int widthB = Math.Min(blockLength, B.col1 - colBlockB);

            if (A.col1 - A.col0 != B.col1 - B.col0)
                throw new InvalidOperationException();

            for (int i = startI; i < end; i += blockLength)
            {
                int segment = Math.Min(blockLength, end - i);

                int widthA = Math.Min(blockLength, A.col1 - A.col0 - i);
                int heightB = Math.Min(blockLength, B.row1 - B.row0 - i);

                int indexA = rowBlockA * A.original.numCols + (A.col0 + i) * heightA + rowA * widthA;
                int indexB = (B.row0 + i) * B.original.numCols + colBlockB * heightB + colB;

                if (i == startI)
                {
                    indexA += offset;
                    indexB += offset * widthB;

                    for (int j = offset; j < segment; j++, indexB += widthB)
                    {
                        total += dataB[indexB] * dataA[indexA++];
                    }
                }
                else
                {
                    for (int j = 0; j < segment; j++, indexB += widthB)
                    {
                        total += dataB[indexB] * dataA[indexA++];
                    }
                }
            }

            return total;
        }
    }
}