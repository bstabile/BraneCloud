using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row.Decomposition;
using SharpMatrix.Dense.Row.Decomposition.QR;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Row.LinSol.QR
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
 * Rx=Q^H b<br>
 * </p>
 *
 * <p>
 * A column major decomposition is used in this solver.
 * <p>
 *
 * @author Peter Abeles
 */
    public class LinearSolverQrHouseCol_CDRM : LinearSolverAbstract_CDRM
    {

        private QRDecompositionHouseholderColumn_CDRM decomposer;

        private CMatrixRMaj a = new CMatrixRMaj(1, 1);
        private CMatrixRMaj temp = new CMatrixRMaj(1, 1);

        protected int maxRows = -1;
        protected int maxCols = -1;

        private float[][] QR; // a column major QR matrix
        private CMatrixRMaj R = new CMatrixRMaj(1, 1);
        private float[] gammas;

        /**
         * Creates a linear solver that uses QR decomposition.
         */
        public LinearSolverQrHouseCol_CDRM()
        {
            decomposer = new QRDecompositionHouseholderColumn_CDRM();
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
        //@Override
        public override bool setA(CMatrixRMaj A)
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
                    int indexB = (i * BnumCols + colB) * 2;
                    a.data[i * 2] = B.data[indexB];
                    a.data[i * 2 + 1] = B.data[indexB + 1];
                }

                // Solve Qa=b
                // a = Q'b
                // a = Q_{n-1}...Q_2*Q_1*b
                //
                // Q_n*b = (I-gamma*u*u^T)*b = b - u*(gamma*U^T*b)
                for (int n = 0; n < numCols; n++)
                {
                    float[] u = QR[n];

                    float realVV = u[n * 2];
                    float imagVV = u[n * 2 + 1];

                    u[n * 2] = 1;
                    u[n * 2 + 1] = 0;

                    QrHelperFunctions_CDRM.rank1UpdateMultR(a, u, 0, gammas[n], 0, n, numRows, temp.data);

                    u[n * 2] = realVV;
                    u[n * 2 + 1] = imagVV;
                }

                // solve for Rx = b using the standard upper triangular solver
                TriangularSolver_CDRM.solveU(R.data, a.data, numCols);

                // save the results
                for (int i = 0; i < numCols; i++)
                {
                    int indexB = (i * BnumCols + colB) * 2;
                    X.data[indexB] = a.data[i * 2];
                    X.data[indexB + 1] = a.data[i * 2 + 1];
                }
            }
        }

        //@Override
        public override bool modifiesA()
        {
            return false;
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
    }
}