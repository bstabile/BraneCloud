using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Block.Decomposition.QR;
using SharpMatrix.Dense.Row;
using SharpMatrix.Interfaces.Decomposition;
using SharpMatrix.Interfaces.LinSol;

namespace SharpMatrix.Dense.Block.LinSol.QR
{
    //package org.ejml.dense.block.linsol.qr;

/**
 * <p>
 * A solver for {@link org.ejml.dense.block.decomposition.qr.QRDecompositionHouseholder_FDRB}.  Systems are solved for using the standard
 * QR decomposition method, sketched below.
 * </p>
 *
 * <p>
 * A = Q*R<br>
 * A*x = b<br>
 * Q*R*x = b <br>
 * R*x = y = Q<sup>T</sup>b<br>
 * x = R<sup>-1</sup>y<br>
 * <br>
 * Where A is the m by n matrix being decomposed. Q is an orthogonal matrix. R is upper triangular matrix.
 * </p>
 *
 * @author Peter Abeles
 */
    public class QrHouseHolderSolver_FDRB : LinearSolverDense<FMatrixRBlock>
    {

        // QR decomposition algorithm
        protected QRDecompositionHouseholder_FDRB decomposer = new QRDecompositionHouseholder_FDRB();

        // the input matrix which has been decomposed
        protected FMatrixRBlock QR;


        public QrHouseHolderSolver_FDRB()
        {
            decomposer.setSaveW(false);
        }

        /**
         * Computes the QR decomposition of A and store the results in A.
         *
         * @param A The A matrix in the linear equation. Modified. Reference saved.
         * @return true if the decomposition was successful.
         */
        public virtual bool setA(FMatrixRBlock A)
        {
            if (A.numRows < A.numCols)
                throw new ArgumentException("Number of rows must be more than or equal to the number of columns.  " +
                                            "Can't solve an underdetermined system.");

            if (!decomposer.decompose(A))
                return false;

            this.QR = decomposer.getQR();

            return true;
        }

        /**
         * Computes the quality using diagonal elements the triangular R matrix in the QR decomposition.
         *
         * @return Solutions quality.
         */
        public virtual /**/ double quality()
        {
            return SpecializedOps_FDRM.qualityTriangular(decomposer.getQR());
        }

        public virtual void solve(FMatrixRBlock B, FMatrixRBlock X)
        {

            if (B.numCols != X.numCols)
                throw new ArgumentException("Columns of B and X do not match");
            if (QR.numCols != X.numRows)
                throw new ArgumentException("Rows in X do not match the columns in A");
            if (QR.numRows != B.numRows)
                throw new ArgumentException("Rows in B do not match the rows in A.");
            if (B.blockLength != QR.blockLength || X.blockLength != QR.blockLength)
                throw new ArgumentException("All matrices must have the same block length.");

            // The system being solved for can be described as:
            // Q*R*X = B

            // First apply householder reflectors to B
            // Y = Q^T*B
            decomposer.applyQTran(B);

            // Second solve for Y using the upper triangle matrix R and the just computed Y
            // X = R^-1 * Y
            MatrixOps_FDRB.extractAligned(B, X);

            // extract a block aligned matrix
            int M = Math.Min(QR.numRows, QR.numCols);

            TriangularSolver_FDRB.solve(QR.blockLength, true,
                new FSubmatrixD1(QR, 0, M, 0, M), new FSubmatrixD1(X), false);

        }

        /**
         * Invert by solving for against an identity matrix.
         *
         * @param A_inv Where the inverted matrix saved. Modified.
         */
        public virtual void invert(FMatrixRBlock A_inv)
        {
            int M = Math.Min(QR.numRows, QR.numCols);
            if (A_inv.numRows != M || A_inv.numCols != M)
                throw new ArgumentException("A_inv must be square an have dimension " + M);


            // Solve for A^-1
            // Q*R*A^-1 = I

            // Apply householder reflectors to the identity matrix
            // y = Q^T*I = Q^T
            MatrixOps_FDRB.setIdentity(A_inv);
            decomposer.applyQTran(A_inv);

            // Solve using upper triangular R matrix
            // R*A^-1 = y
            // A^-1 = R^-1*y
            TriangularSolver_FDRB.solve(QR.blockLength, true,
                new FSubmatrixD1(QR, 0, M, 0, M), new FSubmatrixD1(A_inv), false);
        }

        public virtual bool modifiesA()
        {
            return decomposer.inputModified();
        }

        public virtual bool modifiesB()
        {
            return true;
        }

        public virtual DecompositionInterface<FMatrixRBlock> getDecomposition()
        {
            return decomposer;
        }
    }
}