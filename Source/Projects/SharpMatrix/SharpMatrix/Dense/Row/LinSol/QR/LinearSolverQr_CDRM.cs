using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.LinSol.QR
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
 * Rx=Q^H b<br>
 * </p>
 *
 * @author Peter Abeles
 */
    public class LinearSolverQr_CDRM : LinearSolverAbstract_CDRM
    {

        private QRDecomposition<CMatrixRMaj> decomposer;

        protected int maxRows = -1;
        protected int maxCols = -1;

        protected CMatrixRMaj Q;
        protected CMatrixRMaj Qt;
        protected CMatrixRMaj R;

        private CMatrixRMaj Y, Z;

        /**
         * Creates a linear solver that uses QR decomposition.
         *
         */
        public LinearSolverQr_CDRM(QRDecomposition<CMatrixRMaj> decomposer)
        {
            this.decomposer = decomposer;
        }

        /**
         * Changes the size of the matrix it can solve for
         *
         * @param maxRows Maximum number of rows in the matrix it will decompose.
         * @param maxCols Maximum number of columns in the matrix it will decompose.
         */
        public void setMaxSize(int maxRows, int maxCols)
        {
            this.maxRows = maxRows;
            this.maxCols = maxCols;

            Q = new CMatrixRMaj(maxRows, maxRows);
            Qt = new CMatrixRMaj(maxRows, maxRows);
            R = new CMatrixRMaj(maxRows, maxCols);

            Y = new CMatrixRMaj(maxRows, 1);
            Z = new CMatrixRMaj(maxRows, 1);
        }

        /**
         * Performs QR decomposition on A
         *
         * @param A not modified.
         */
        //@Override
        public override bool setA(CMatrixRMaj A)
        {
            if (A.numRows > maxRows || A.numCols > maxCols)
            {
                setMaxSize(A.numRows, A.numCols);
            }

            _setA(A);
            if (!decomposer.decompose(A))
                return false;

            Q.reshape(numRows, numRows);
            R.reshape(numRows, numCols);
            decomposer.getQ(Q, false);
            decomposer.getR(R, false);
            CommonOps_CDRM.transposeConjugate(Q, Qt);

            return true;
        }

        //@Override
        public override /**/ double quality()
        {
            return SpecializedOps_CDRM.qualityTriangular(R);
        }

        /**
         * Solves for X using the QR decomposition.
         *
         * @param B A matrix that is n by m.  Not modified.
         * @param X An n by m matrix where the solution is written to.  Modified.
         */
        //@Override
        public override void solve(CMatrixRMaj B, CMatrixRMaj X)
        {
            if (X.numRows != numCols)
                throw new ArgumentException("Unexpected dimensions for X");
            else if (B.numRows != numRows || B.numCols != X.numCols)
                throw new ArgumentException("Unexpected dimensions for B");

            int BnumCols = B.numCols;

            Y.reshape(numRows, 1);
            Z.reshape(numRows, 1);

            // solve each column one by one
            for (int colB = 0; colB < BnumCols; colB++)
            {

                // make a copy of this column in the vector
                for (int i = 0; i < numRows; i++)
                {
                    int indexB = B.getIndex(i, colB);
                    Y.data[i * 2] = B.data[indexB];
                    Y.data[i * 2 + 1] = B.data[indexB + 1];
                }

                // Solve Qa=b
                // a = Q'b
                CommonOps_CDRM.mult(Qt, Y, Z);

                // solve for Rx = b using the standard upper triangular solver
                TriangularSolver_CDRM.solveU(R.data, Z.data, numCols);

                // save the results
                for (int i = 0; i < numCols; i++)
                {
                    X.set(i, colB, Z.data[i * 2], Z.data[i * 2 + 1]);
                }
            }
        }

        //@Override
        public override bool modifiesA()
        {
            return decomposer.inputModified();
        }

        //@Override
        public override bool modifiesB()
        {
            return false;
        }

        //@Override
        public override DecompositionInterface<CMatrixRMaj> getDecomposition()
        {
            return decomposer;
        }

        public QRDecomposition<CMatrixRMaj> getDecomposer()
        {
            return decomposer;
        }

        public CMatrixRMaj getQ()
        {
            return Q;
        }

        public CMatrixRMaj getR()
        {
            return R;
        }
    }
}