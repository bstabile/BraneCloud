using System;
using SharpMatrix.Data;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Block.Decomposition.Chol
{
    //package org.ejml.dense.block.decomposition.chol;

/**
 * <p>
 * Block Cholesky using outer product form.  The original matrix is stored and modified.
 * </p>
 *
 * <p>
 * Based on the description provided in "Fundamentals of Matrix Computations" 2nd Ed. by David S. Watkins.
 * </p>
 *
 * @author Peter Abeles
 */
    public class CholeskyOuterForm_FDRB : CholeskyDecomposition_F32<FMatrixRBlock>
    {

        // if it should compute an upper or lower triangular matrix
        private bool lower = false;

        // The decomposed matrix.
        private FMatrixRBlock T;

        // predeclare local work space
        private FSubmatrixD1 subA = new FSubmatrixD1();

        private FSubmatrixD1 subB = new FSubmatrixD1();
        private FSubmatrixD1 subC = new FSubmatrixD1();

        // storage for the determinant
        private Complex_F32 det = new Complex_F32();

        /**
         * Creates a new BlockCholeskyOuterForm
         *
         * @param lower Should it decompose it into a lower triangular matrix or not.
         */
        public CholeskyOuterForm_FDRB(bool lower)
        {
            this.lower = lower;
        }

        /**
         * Decomposes the provided matrix and stores the result in the same matrix.
         *
         * @param A Matrix that is to be decomposed.  Modified.
         * @return If it succeeded or not.
         */
        public virtual bool decompose(FMatrixRBlock A)
        {
            if (A.numCols != A.numRows)
                throw new ArgumentException("A must be square");

            this.T = A;

            if (lower)
                return decomposeLower();
            else
                return decomposeUpper();
        }

        private bool decomposeLower()
        {
            int blockLength = T.blockLength;

            subA.set(T);
            subB.set(T);
            subC.set(T);

            for (int i = 0; i < T.numCols; i += blockLength)
            {
                int widthA = Math.Min(blockLength, T.numCols - i);

                subA.col0 = i;
                subA.col1 = i + widthA;
                subA.row0 = subA.col0;
                subA.row1 = subA.col1;

                subB.col0 = i;
                subB.col1 = i + widthA;
                subB.row0 = i + widthA;
                subB.row1 = T.numRows;

                subC.col0 = i + widthA;
                subC.col1 = T.numRows;
                subC.row0 = i + widthA;
                subC.row1 = T.numRows;

                // cholesky on inner block A
                if (!InnerCholesky_FDRB.lower(subA))
                    return false;

                // on the last block these operations are not needed.
                if (widthA == blockLength)
                {
                    // B = L^-1 B
                    TriangularSolver_FDRB.solveBlock(blockLength, false, subA, subB, false, true);

                    // C = C - B * B^T
                    InnerRankUpdate_FDRB.symmRankNMinus_L(blockLength, subC, subB);
                }
            }

            MatrixOps_FDRB.zeroTriangle(true, T);

            return true;
        }


        private bool decomposeUpper()
        {
            int blockLength = T.blockLength;

            subA.set(T);
            subB.set(T);
            subC.set(T);

            for (int i = 0; i < T.numCols; i += blockLength)
            {
                int widthA = Math.Min(blockLength, T.numCols - i);

                subA.col0 = i;
                subA.col1 = i + widthA;
                subA.row0 = subA.col0;
                subA.row1 = subA.col1;

                subB.col0 = i + widthA;
                subB.col1 = T.numCols;
                subB.row0 = i;
                subB.row1 = i + widthA;

                subC.col0 = i + widthA;
                subC.col1 = T.numCols;
                subC.row0 = i + widthA;
                subC.row1 = T.numCols;

                // cholesky on inner block A
                if (!InnerCholesky_FDRB.upper(subA))
                    return false;

                // on the last block these operations are not needed.
                if (widthA == blockLength)
                {
                    // B = U^-1 B
                    TriangularSolver_FDRB.solveBlock(blockLength, true, subA, subB, true, false);

                    // C = C - B^T * B
                    InnerRankUpdate_FDRB.symmRankNMinus_U(blockLength, subC, subB);
                }
            }

            MatrixOps_FDRB.zeroTriangle(false, T);

            return true;
        }

        public virtual bool isLower()
        {
            return lower;
        }

        public virtual FMatrixRBlock getT(FMatrixRBlock T)
        {
            if (T == null)
                return this.T;
            T.set(this.T);

            return T;
        }

        public virtual Complex_F32 computeDeterminant()
        {
            float prod = 1.0f;

            int blockLength = T.blockLength;
            for (int i = 0; i < T.numCols; i += blockLength)
            {
                // width of the submatrix
                int widthA = Math.Min(blockLength, T.numCols - i);

                // index of the first element in the block
                int indexT = i * T.numCols + i * widthA;

                // product along the diagonal
                for (int j = 0; j < widthA; j++)
                {
                    prod *= T.data[indexT];
                    indexT += widthA + 1;
                }
            }

            det.real = prod * prod;
            det.imaginary = 0;

            return det;
        }

        public virtual bool inputModified()
        {
            return true;
        }
    }
}