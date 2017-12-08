using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row.Decomposition;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Row.LinSol.QR
{
    //package org.ejml.dense.row.linsol.qr;

/**
 * <p>
 * A solver for a generic QR decomposition algorithm.  This will in general be a bit slower than the
 * specialized once since the full Q and R matrices need to be extracted.
 * </p>
 * <p>
 * It solve for x by first multiplying b by the transpose of Q then solving for the result.
 * <br>
 * QRx=b<br>
 * Rx=Q^T b<br>
 * </p>
 *
 * @author Peter Abeles
 */
    public class LinearSolverQr_DDRM : LinearSolverAbstract_DDRM
    {

        private QRDecomposition<DMatrixRMaj> decomposer;

        protected int maxRows = -1;
        protected int maxCols = -1;

        protected DMatrixRMaj Q;
        protected DMatrixRMaj R;

        private DMatrixRMaj Y, Z;

        /**
         * Creates a linear solver that uses QR decomposition.
         *
         */
        public LinearSolverQr_DDRM(QRDecomposition<DMatrixRMaj> decomposer)
        {
            this.decomposer = decomposer;
        }

        /**
         * Changes the size of the matrix it can solve for
         *
         * @param maxRows Maximum number of rows in the matrix it will decompose.
         * @param maxCols Maximum number of columns in the matrix it will decompose.
         */
        public virtual void setMaxSize(int maxRows, int maxCols)
        {
            this.maxRows = maxRows;
            this.maxCols = maxCols;

            Q = new DMatrixRMaj(maxRows, maxRows);
            R = new DMatrixRMaj(maxRows, maxCols);

            Y = new DMatrixRMaj(maxRows, 1);
            Z = new DMatrixRMaj(maxRows, 1);
        }

        /**
         * Performs QR decomposition on A
         *
         * @param A not modified.
         */
        public override bool setA(DMatrixRMaj A)
        {
            if (A.numRows > maxRows || A.numCols > maxCols)
            {
                setMaxSize(A.numRows, A.numCols);
            }

            _setA(A);
            if (!decomposer.decompose(A))
                return false;

            Q.reshape(numRows, numRows, false);
            R.reshape(numRows, numCols, false);
            decomposer.getQ(Q, false);
            decomposer.getR(R, false);

            return true;
        }

        public override /**/ double quality()
        {
            return SpecializedOps_DDRM.qualityTriangular(R);
        }

        /**
         * Solves for X using the QR decomposition.
         *
         * @param B A matrix that is n by m.  Not modified.
         * @param X An n by m matrix where the solution is written to.  Modified.
         */
        public override void solve(DMatrixRMaj B, DMatrixRMaj X)
        {
            if (X.numRows != numCols)
                throw new ArgumentException("Unexpected dimensions for X");
            else if (B.numRows != numRows || B.numCols != X.numCols)
                throw new ArgumentException("Unexpected dimensions for B");

            int BnumCols = B.numCols;

            Y.reshape(numRows, 1, false);
            Z.reshape(numRows, 1, false);

            // solve each column one by one
            for (int colB = 0; colB < BnumCols; colB++)
            {

                // make a copy of this column in the vector
                for (int i = 0; i < numRows; i++)
                {
                    Y.data[i] = B.get(i, colB);
                }

                // Solve Qa=b
                // a = Q'b
                CommonOps_DDRM.multTransA(Q, Y, Z);

                // solve for Rx = b using the standard upper triangular solver
                TriangularSolver_DDRM.solveU(R.data, Z.data, numCols);

                // save the results
                for (int i = 0; i < numCols; i++)
                {
                    X.set(i, colB, Z.data[i]);
                }
            }
        }

        public override bool modifiesA()
        {
            return decomposer.inputModified();
        }

        public override bool modifiesB()
        {
            return false;
        }

        public override DecompositionInterface<DMatrixRMaj> getDecomposition()
        {
            return decomposer;
        }

        public QRDecomposition<DMatrixRMaj> getDecomposer()
        {
            return decomposer;
        }

        public DMatrixRMaj getQ()
        {
            return Q;
        }

        public DMatrixRMaj getR()
        {
            return R;
        }
    }
}