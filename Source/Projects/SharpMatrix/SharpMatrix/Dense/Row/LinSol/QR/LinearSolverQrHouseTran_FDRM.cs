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
    public class LinearSolverQrHouseTran_FDRM : LinearSolverAbstract_FDRM
    {

        private QRDecompositionHouseholderTran_FDRM decomposer;

        private float[] a;

        protected int maxRows = -1;
        protected int maxCols = -1;

        private FMatrixRMaj QR; // a column major QR matrix
        private FMatrixRMaj U;

        /**
         * Creates a linear solver that uses QR decomposition.
         */
        public LinearSolverQrHouseTran_FDRM()
        {
            decomposer = new QRDecompositionHouseholderTran_FDRM();
        }

        public void setMaxSize(int maxRows, int maxCols)
        {
            this.maxRows = maxRows;
            this.maxCols = maxCols;

            a = new float[maxRows];
        }

        /**
         * Performs QR decomposition on A
         *
         * @param A not modified.
         */
        //@Override
        public override bool setA(FMatrixRMaj A)
        {
            if (A.numRows > maxRows || A.numCols > maxCols)
                setMaxSize(A.numRows, A.numCols);

            _setA(A);
            if (!decomposer.decompose(A))
                return false;

            QR = decomposer.getQR();
            return true;
        }

        //@Override
        public override /**/ double quality()
        {
            // even those it is transposed the diagonal elements are at the same
            // elements
            return SpecializedOps_FDRM.qualityTriangular(QR);
        }

        /**
         * Solves for X using the QR decomposition.
         *
         * @param B A matrix that is n by m.  Not modified.
         * @param X An n by m matrix where the solution is written to.  Modified.
         */
        //@Override
        public override void solve(FMatrixRMaj B, FMatrixRMaj X)
        {
            if (X.numRows != numCols)
                throw new ArgumentException("Unexpected dimensions for X: X rows = " + X.numRows + " expected = " +
                                            numCols);
            else if (B.numRows != numRows || B.numCols != X.numCols)
                throw new ArgumentException("Unexpected dimensions for B");

            U = decomposer.getR(U, true);
            float[] gammas = decomposer.getGammas();
            float[] dataQR = QR.data;

            int BnumCols = B.numCols;

            // solve each column one by one
            for (int colB = 0; colB < BnumCols; colB++)
            {

                // make a copy of this column in the vector
                for (int i = 0; i < numRows; i++)
                {
                    a[i] = B.data[i * BnumCols + colB];
                }

                // Solve Qa=b
                // a = Q'b
                // a = Q_{n-1}...Q_2*Q_1*b
                //
                // Q_n*b = (I-gamma*u*u^T)*b = b - u*(gamma*U^T*b)
                for (int n = 0; n < numCols; n++)
                {
                    int indexU = n * numRows + n + 1;

                    float ub = a[n];
                    // U^T*b
                    for (int i = n + 1; i < numRows; i++, indexU++)
                    {
                        ub += dataQR[indexU] * a[i];
                    }

                    // gamma*U^T*b
                    ub *= gammas[n];

                    a[n] -= ub;
                    indexU = n * numRows + n + 1;
                    for (int i = n + 1; i < numRows; i++, indexU++)
                    {
                        a[i] -= dataQR[indexU] * ub;
                    }
                }

                // solve for Rx = b using the standard upper triangular solver
                TriangularSolver_FDRM.solveU(U.data, a, numCols);

                // save the results
                for (int i = 0; i < numCols; i++)
                {
                    X.data[i * X.numCols + colB] = a[i];
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
        public override DecompositionInterface<FMatrixRMaj> getDecomposition()
        {
            return decomposer;
        }
    }
}