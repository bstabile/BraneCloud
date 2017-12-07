using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Block.Decomposition.QR;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Block.Decomposition.Hessenberg
{

    //package org.ejml.dense.block.decomposition.hessenberg;

/**
 * <p>
 * Tridiagonal similar decomposition for block matrices.  Orthogonal matrices are computed using
 * householder vectors.
 * </p>
 *
 * <p>
 * Based off algorithm in section 2 of J. J. Dongarra, D. C. Sorensen, S. J. Hammarling,
 * "Block Reduction of Matrices to Condensed Forms for Eigenvalue Computations" Journal of
 * Computations and Applied Mathematics 27 (1989) 215-227<br>
 * <br>
 * Computations of Householder reflectors has been modified from what is presented in that paper to how 
 * it is performed in "Fundamentals of Matrix Computations" 2nd ed. by David S. Watkins.
 * </p>
 *
 * @author Peter Abeles
 */
    public class TridiagonalDecompositionHouseholder_DDRB
        : TridiagonalSimilarDecomposition_F64<DMatrixRBlock>
    {

        // matrix which is being decomposed
        // householder vectors are stored along the upper triangle rows
        protected DMatrixRBlock A;

        // temporary storage for block computations
        protected DMatrixRBlock V = new DMatrixRBlock(1, 1);

        // stores intermediate results in matrix multiplication
        protected DMatrixRBlock tmp = new DMatrixRBlock(1, 1);

        protected double[] gammas = new double[1];

        // temporary storage for zeros and ones in U
        protected DMatrixRMaj zerosM = new DMatrixRMaj(1, 1);

        public virtual DMatrixRBlock getT(DMatrixRBlock T)
        {
            if (T == null)
            {
                T = new DMatrixRBlock(A.numRows, A.numCols, A.blockLength);
            }
            else
            {
                if (T.numRows != A.numRows || T.numCols != A.numCols)
                    throw new ArgumentException("T must have the same dimensions as the input matrix");

                CommonOps_DDRM.fill(T, 0);
            }

            T.set(0, 0, A.data[0]);
            for (int i = 1; i < A.numRows; i++)
            {
                double d = A.get(i - 1, i);
                T.set(i, i, A.get(i, i));
                T.set(i - 1, i, d);
                T.set(i, i - 1, d);
            }

            return T;
        }

        public virtual DMatrixRBlock getQ(DMatrixRBlock Q, bool transposed)
        {
            Q = QRDecompositionHouseholder_DDRB.initializeQ(Q, A.numRows, A.numCols, A.blockLength, false);

            int height = Math.Min(A.blockLength, A.numRows);
            V.reshape(height, A.numCols, false);
            this.tmp.reshape(height, A.numCols, false);

            DSubmatrixD1 subQ = new DSubmatrixD1(Q);
            DSubmatrixD1 subU = new DSubmatrixD1(A);
            DSubmatrixD1 subW = new DSubmatrixD1(V);
            DSubmatrixD1 temp = new DSubmatrixD1(this.tmp);


            int N = A.numRows;

            int start = N - N % A.blockLength;
            if (start == N)
                start -= A.blockLength;
            if (start < 0)
                start = 0;

            // (Q1^T * (Q2^T * (Q3^t * A)))
            for (int i = start; i >= 0; i -= A.blockLength)
            {
                int blockSize = Math.Min(A.blockLength, N - i);

                subW.col0 = i;
                subW.row1 = blockSize;
                subW.original.reshape(subW.row1, subW.col1, false);

                if (transposed)
                {
                    temp.row0 = i;
                    temp.row1 = A.numCols;
                    temp.col0 = 0;
                    temp.col1 = blockSize;
                }
                else
                {
                    temp.col0 = i;
                    temp.row1 = blockSize;
                }
                temp.original.reshape(temp.row1, temp.col1, false);

                subU.col0 = i;
                subU.row0 = i;
                subU.row1 = subU.row0 + blockSize;

                // zeros and ones are saved and overwritten in U so that standard matrix multiplication can be used
                copyZeros(subU);

                // compute W for Q(i) = ( I + W*Y^T)
                TridiagonalHelper_DDRB.computeW_row(A.blockLength, subU, subW, gammas, i);

                subQ.col0 = i;
                subQ.row0 = i;

                // Apply the Qi to Q
                // Qi = I + W*U^T

                // Note that U and V are really row vectors.  but standard notation assumed they are column vectors.
                // which is why the functions called don't match the math above

                // (I + W*U^T)*Q
                // F=U^T*Q(i)
                if (transposed)
                    MatrixMult_DDRB.multTransB(A.blockLength, subQ, subU, temp);
                else
                    MatrixMult_DDRB.mult(A.blockLength, subU, subQ, temp);
                // Q(i+1) = Q(i) + W*F
                if (transposed)
                    MatrixMult_DDRB.multPlus(A.blockLength, temp, subW, subQ);
                else
                    MatrixMult_DDRB.multPlusTransA(A.blockLength, subW, temp, subQ);

                replaceZeros(subU);
            }

            return Q;
        }

        private void copyZeros(DSubmatrixD1 subU)
        {
            int N = Math.Min(A.blockLength, subU.col1 - subU.col0);
            for (int i = 0; i < N; i++)
            {
                // save the zeros
                for (int j = 0; j <= i; j++)
                {
                    zerosM.unsafe_set(i, j, subU.get(i, j));
                    subU.set(i, j, 0);
                }
                // save the one
                if (subU.col0 + i + 1 < subU.original.numCols)
                {
                    zerosM.unsafe_set(i, i + 1, subU.get(i, i + 1));
                    subU.set(i, i + 1, 1);
                }
            }
        }

        private void replaceZeros(DSubmatrixD1 subU)
        {
            int N = Math.Min(A.blockLength, subU.col1 - subU.col0);
            for (int i = 0; i < N; i++)
            {
                // save the zeros
                for (int j = 0; j <= i; j++)
                {
                    subU.set(i, j, zerosM.get(i, j));
                }
                // save the one
                if (subU.col0 + i + 1 < subU.original.numCols)
                {
                    subU.set(i, i + 1, zerosM.get(i, i + 1));
                }
            }
        }

        public virtual void getDiagonal(double[] diag, double[] off)
        {
            diag[0] = A.data[0];
            for (int i = 1; i < A.numRows; i++)
            {
                diag[i] = A.get(i, i);
                off[i - 1] = A.get(i - 1, i);
            }
        }

        public virtual bool decompose(DMatrixRBlock orig)
        {
            if (orig.numCols != orig.numRows)
                throw new ArgumentException("Input matrix must be square.");

            init(orig);

            DSubmatrixD1 subA = new DSubmatrixD1(A);
            DSubmatrixD1 subV = new DSubmatrixD1(V);
            DSubmatrixD1 subU = new DSubmatrixD1(A);

            int N = orig.numCols;

            for (int i = 0; i < N; i += A.blockLength)
            {
//            Console.WriteLine("-------- triag i "+i);
                int height = Math.Min(A.blockLength, A.numRows - i);

                subA.col0 = subU.col0 = i;
                subA.row0 = subU.row0 = i;

                subU.row1 = subU.row0 + height;

                subV.col0 = i;
                subV.row1 = height;
                subV.original.reshape(subV.row1, subV.col1, false);

                // bidiagonalize the top row
                TridiagonalHelper_DDRB.tridiagUpperRow(A.blockLength, subA, gammas, subV);

                // apply Householder reflectors to the lower portion using block multiplication

                if (subU.row1 < orig.numCols)
                {
                    // take in account the 1 in the last row.  The others are skipped over.
                    double before = subU.get(A.blockLength - 1, A.blockLength);
                    subU.set(A.blockLength - 1, A.blockLength, 1);

                    // A = A + U*V^T + V*U^T
                    multPlusTransA(A.blockLength, subU, subV, subA);
                    multPlusTransA(A.blockLength, subV, subU, subA);

                    subU.set(A.blockLength - 1, A.blockLength, before);
                }
            }

            return true;
        }

        /**
         * C = C + A^T*B
         *
         * @param blockLength
         * @param A row block vector
         * @param B row block vector
         * @param C
         */
        public static void multPlusTransA(int blockLength,
            DSubmatrixD1 A, DSubmatrixD1 B,
            DSubmatrixD1 C)
        {
            int heightA = Math.Min(blockLength, A.row1 - A.row0);

            for (int i = C.row0 + blockLength; i < C.row1; i += blockLength)
            {
                int heightC = Math.Min(blockLength, C.row1 - i);

                int indexA = A.row0 * A.original.numCols + (i - C.row0 + A.col0) * heightA;

                for (int j = i; j < C.col1; j += blockLength)
                {
                    int widthC = Math.Min(blockLength, C.col1 - j);

                    int indexC = i * C.original.numCols + j * heightC;
                    int indexB = B.row0 * B.original.numCols + (j - C.col0 + B.col0) * heightA;

                    InnerMultiplication_DDRB.blockMultPlusTransA(A.original.data, B.original.data, C.original.data,
                        indexA, indexB, indexC, heightA, heightC, widthC);
                }
            }
        }

        private void init(DMatrixRBlock orig)
        {
            this.A = orig;

            int height = Math.Min(A.blockLength, A.numRows);
            V.reshape(height, A.numCols, A.blockLength, false);
            tmp.reshape(height, A.numCols, A.blockLength, false);

            if (gammas.Length < A.numCols)
                gammas = new double[A.numCols];

            zerosM.reshape(A.blockLength, A.blockLength + 1, false);
        }

        public virtual bool inputModified()
        {
            return true;
        }
    }
}
