using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Block.Decomposition.Chol;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.LinSol;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Block.LinSol.Chol
{
    //package org.ejml.dense.block.linsol.chol;

/**
 * <p> Linear solver that uses a block cholesky decomposition. </p>
 *
 * <p>
 * Solver works by using the standard Cholesky solving strategy:<br>
 * A=L*L<sup>T</sup> <br>
 * A*x=b<br>
 * L*L<sup>T</sup>*x = b <br>
 * L*y = b<br>
 * L<sup>T</sup>*x = y<br>
 * x = L<sup>-T</sup>y
 * </p>
 *
 * <p>
 * It is also possible to use the upper triangular cholesky decomposition.
 * </p>
 *
 * @author Peter Abeles
 */
    public class CholeskyOuterSolver_DDRB : LinearSolverDense<DMatrixRBlock>
    {

        // cholesky decomposition
        private CholeskyOuterForm_DDRB decomposer = new CholeskyOuterForm_DDRB(true);

        // size of a block take from input matrix
        private int blockLength;

        // temporary data structure used in some calculation.
        private double[] temp;

        /**
         * Decomposes and overwrites the input matrix.
         *
         * @param A Semi-Positive Definite (SPD) system matrix. Modified. Reference saved.
         * @return If the matrix can be decomposed.  Will always return false of not SPD.
         */
        public virtual bool setA(DMatrixRBlock A)
        {
            // Extract a lower triangular solution
            if (!decomposer.decompose(A))
                return false;

            blockLength = A.blockLength;

            return true;
        }

        public virtual /**/ double quality()
        {
            return SpecializedOps_DDRM.qualityTriangular(decomposer.getT(null));
        }

        /**
         * If X == null then the solution is written into B.  Otherwise the solution is copied
         * from B into X.
         */
        public virtual void solve(DMatrixRBlock B, DMatrixRBlock X)
        {
            if (B.blockLength != blockLength)
                throw new ArgumentException("Unexpected blocklength in B.");

            DSubmatrixD1 L = new DSubmatrixD1(decomposer.getT(null));

            if (X != null)
            {
                if (X.blockLength != blockLength)
                    throw new ArgumentException("Unexpected blocklength in X.");
                if (X.numRows != L.col1) throw new ArgumentException("Not enough rows in X");
            }

            if (B.numRows != L.col1) throw new ArgumentException("Not enough rows in B");

            //  L * L^T*X = B

            // Solve for Y:  L*Y = B
            TriangularSolver_DDRB.solve(blockLength, false, L, new DSubmatrixD1(B), false);

            // L^T * X = Y
            TriangularSolver_DDRB.solve(blockLength, false, L, new DSubmatrixD1(B), true);

            if (X != null)
            {
                // copy the solution from B into X
                MatrixOps_DDRB.extractAligned(B, X);
            }

        }

        public virtual void invert(DMatrixRBlock A_inv)
        {
            DMatrixRBlock T = decomposer.getT(null);
            if (A_inv.numRows != T.numRows || A_inv.numCols != T.numCols)
                throw new ArgumentException("Unexpected number or rows and/or columns");


            if (temp == null || temp.Length < blockLength * blockLength)
                temp = new double[blockLength * blockLength];

            // zero the upper triangular portion of A_inv
            MatrixOps_DDRB.zeroTriangle(true, A_inv);

            DSubmatrixD1 L = new DSubmatrixD1(T);
            DSubmatrixD1 B = new DSubmatrixD1(A_inv);

            // invert L from cholesky decomposition and write the solution into the lower
            // triangular portion of A_inv
            // B = inv(L)
            TriangularSolver_DDRB.invert(blockLength, false, L, B, temp);

            // B = L^-T * B
            // todo could speed up by taking advantage of B being lower triangular
            // todo take advantage of symmetry
            TriangularSolver_DDRB.solveL(blockLength, L, B, true);
        }

        public virtual bool modifiesA()
        {
            return decomposer.inputModified();
        }

        public virtual bool modifiesB()
        {
            return true;
        }

        public virtual DecompositionInterface<DMatrixRBlock> getDecomposition()
        {
            return decomposer;
        }
    }
}