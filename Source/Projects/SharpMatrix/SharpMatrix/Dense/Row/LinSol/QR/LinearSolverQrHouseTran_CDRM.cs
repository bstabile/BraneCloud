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
 * Rx=Q^H b<br>
 * </p>
 *
 * <p>
 * A column major decomposition is used in this solver.
 * <p>
 *
 * @author Peter Abeles
 */
    public class LinearSolverQrHouseTran_CDRM : LinearSolverAbstract_CDRM
    {

        private QRDecompositionHouseholderTran_CDRM decomposer;

        private float[] a;

        protected int maxRows = -1;
        protected int maxCols = -1;

        private CMatrixRMaj QR; // a column major QR matrix
        private CMatrixRMaj U;

        /**
         * Creates a linear solver that uses QR decomposition.
         */
        public LinearSolverQrHouseTran_CDRM()
        {
            decomposer = new QRDecompositionHouseholderTran_CDRM();
        }

        public void setMaxSize(int maxRows, int maxCols)
        {
            this.maxRows = maxRows;
            this.maxCols = maxCols;

            a = new float[maxRows * 2];
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
            return SpecializedOps_CDRM.qualityTriangular(QR);
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
                    int indexB = (i * BnumCols + colB) * 2;
                    a[i * 2] = B.data[indexB];
                    a[i * 2 + 1] = B.data[indexB + 1];
                }

                // Solve Qa=b
                // a = Q'b
                // a = Q_{n-1}...Q_2*Q_1*b
                //
                // Q_n*b = (I-gamma*u*u^H)*b = b - u*(gamma*U^H*b)
                for (int n = 0; n < numCols; n++)
                {
                    int indexU = (n * numRows + n + 1) * 2;

                    float realUb = a[n * 2];
                    float imagUb = a[n * 2 + 1];

                    // U^H*b
                    for (int i = n + 1; i < numRows; i++)
                    {
                        float realU = dataQR[indexU++];
                        float imagU = -dataQR[indexU++];

                        float realB = a[i * 2];
                        float imagB = a[i * 2 + 1];

                        realUb += realU * realB - imagU * imagB;
                        imagUb += realU * imagB + imagU * realB;
                    }

                    // gamma*U^T*b
                    realUb *= gammas[n];
                    imagUb *= gammas[n];

                    a[n * 2] -= realUb;
                    a[n * 2 + 1] -= imagUb;

                    indexU = (n * numRows + n + 1) * 2;
                    for (int i = n + 1; i < numRows; i++)
                    {
                        float realU = dataQR[indexU++];
                        float imagU = dataQR[indexU++];

                        a[i * 2] -= realU * realUb - imagU * imagUb;
                        a[i * 2 + 1] -= realU * imagUb + imagU * realUb;
                    }
                }

                // solve for Rx = b using the standard upper triangular solver
                TriangularSolver_CDRM.solveU(U.data, a, numCols);

                // save the results

                for (int i = 0; i < numCols; i++)
                {
                    int indexX = (i * X.numCols + colB) * 2;

                    X.data[indexX] = a[i * 2];
                    X.data[indexX + 1] = a[i * 2 + 1];
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
    }
}