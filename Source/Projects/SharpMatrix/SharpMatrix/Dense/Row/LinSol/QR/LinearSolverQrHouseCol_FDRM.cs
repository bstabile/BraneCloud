using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.QR;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.LinSol.QR
{
    //package org.ejml.dense.row.linsol.qr;

/**
 * <p>
 * QR decomposition can be used to solve for systems.  However, this is not as computationally efficient
 * as LU decomposition and costs about 3n<sup>2</sup> flops.
 * </p>
 * <p>
 * It solve for x by first multiplying b by the transpose of Q then solving for the result.
 * <br>
 * QRx=b<br>
 * Rx=Q^T b<br>
 * </p>
 *
 * <p>
 * A column major decomposition is used in this solver.
 * <p>
 *
 * @author Peter Abeles
 */
    public class LinearSolverQrHouseCol_FDRM : LinearSolverAbstract_FDRM
    {

        private QRDecompositionHouseholderColumn_FDRM decomposer;

        private FMatrixRMaj a = new FMatrixRMaj(1, 1);
        private FMatrixRMaj temp = new FMatrixRMaj(1, 1);

        protected int maxRows = -1;
        protected int maxCols = -1;

        private float[][] QR; // a column major QR matrix
        private FMatrixRMaj R = new FMatrixRMaj(1, 1);
        private float[] gammas;

        /**
         * Creates a linear solver that uses QR decomposition.
         */
        public LinearSolverQrHouseCol_FDRM()
        {
            decomposer = new QRDecompositionHouseholderColumn_FDRM();
        }

        public void setMaxSize(int maxRows, int maxCols)
        {
            this.maxRows = maxRows;
            this.maxCols = maxCols;
        }

        /**
         * Performs QR decomposition on A
         *
         * @param A not modified.
         */
        public override bool setA(FMatrixRMaj A)
        {
            if (A.numRows < A.numCols)
                throw new ArgumentException("Can't solve for wide systems.  More variables than equations.");
            if (A.numRows > maxRows || A.numCols > maxCols)
                setMaxSize(A.numRows, A.numCols);

            R.reshape(A.numCols, A.numCols);
            a.reshape(A.numRows, 1);
            temp.reshape(A.numRows, 1);

            _setA(A);
            if (!decomposer.decompose(A))
                return false;

            gammas = decomposer.getGammas();
            QR = decomposer.getQR();
            decomposer.getR(R, true);
            return true;
        }

        public override /**/ double quality()
        {
            return SpecializedOps_FDRM.qualityTriangular(R);
        }

        /**
         * Solves for X using the QR decomposition.
         *
         * @param B A matrix that is n by m.  Not modified.
         * @param X An n by m matrix where the solution is written to.  Modified.
         */
        public override void solve(FMatrixRMaj B, FMatrixRMaj X)
        {
            if (X.numRows != numCols)
                throw new ArgumentException("Unexpected dimensions for X: X rows = " + X.numRows + " expected = " +
                                            numCols);
            else if (B.numRows != numRows || B.numCols != X.numCols)
                throw new ArgumentException("Unexpected dimensions for B");

            int BnumCols = B.numCols;

            // solve each column one by one
            for (int colB = 0; colB < BnumCols; colB++)
            {

                // make a copy of this column in the vector
                for (int i = 0; i < numRows; i++)
                {
                    a.data[i] = B.data[i * BnumCols + colB];
                }

                // Solve Qa=b
                // a = Q'b
                // a = Q_{n-1}...Q_2*Q_1*b
                //
                // Q_n*b = (I-gamma*u*u^T)*b = b - u*(gamma*U^T*b)
                for (int n = 0; n < numCols; n++)
                {
                    float[] u = QR[n];

                    float vv = u[n];
                    u[n] = 1;
                    QrHelperFunctions_FDRM.rank1UpdateMultR(a, u, gammas[n], 0, n, numRows, temp.data);
                    u[n] = vv;
                }

                // solve for Rx = b using the standard upper triangular solver
                TriangularSolver_FDRM.solveU(R.data, a.data, numCols);

                // save the results
                for (int i = 0; i < numCols; i++)
                {
                    X.data[i * X.numCols + colB] = a.data[i];
                }
            }
        }

        public override bool modifiesA()
        {
            return false;
        }

        public override bool modifiesB()
        {
            return false;
        }

        public override DecompositionInterface<FMatrixRMaj> getDecomposition()
        {
            return decomposer;
        }
    }
}