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
 * @author Peter Abeles
 */
    public class LinearSolverQrHouse_ZDRM : LinearSolverAbstract_ZDRM
    {

        private QRDecompositionHouseholder_ZDRM decomposer;

        private double[] a, u;

        private int maxRows = -1;

        private ZMatrixRMaj QR;
        private double[] gammas;

        /**
         * Creates a linear solver that uses QR decomposition.
         */
        public LinearSolverQrHouse_ZDRM()
        {
            decomposer = new QRDecompositionHouseholder_ZDRM();


        }

        public void setMaxSize(int maxRows)
        {
            this.maxRows = maxRows;

            a = new double[maxRows * 2];
            u = new double[maxRows * 2];
        }

        /**
         * Performs QR decomposition on A
         *
         * @param A not modified.
         */
        //@Override
        public override bool setA(ZMatrixRMaj A)
        {
            if (A.numRows > maxRows)
            {
                setMaxSize(A.numRows);
            }

            _setA(A);
            if (!decomposer.decompose(A))
                return false;

            gammas = decomposer.getGammas();
            QR = decomposer.getQR();

            return true;
        }

        //@Override
        public override /**/ double quality()
        {
            return SpecializedOps_ZDRM.qualityTriangular(QR);
        }

        /**
         * Solves for X using the QR decomposition.
         *
         * @param B A matrix that is n by m.  Not modified.
         * @param X An n by m matrix where the solution is writen to.  Modified.
         */
        //@Override
        public override void solve(ZMatrixRMaj B, ZMatrixRMaj X)
        {
            if (X.numRows != numCols)
                throw new ArgumentException("Unexpected dimensions for X");
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
                    u[n * 2] = 1;
                    u[n * 2 + 1] = 0;

                    double realUb = a[2 * n];
                    double imagUb = a[2 * n + 1];
                    // U^H*b
                    for (int i = n + 1; i < numRows; i++)
                    {
                        int indexQR = (i * QR.numCols + n) * 2;
                        double realU = u[i * 2] = QR.data[indexQR];
                        double imagU = u[i * 2 + 1] = QR.data[indexQR + 1];

                        double realB = a[i * 2];
                        double imagB = a[i * 2 + 1];

                        realUb += realU * realB + imagU * imagB;
                        imagUb += realU * imagB - imagU * realB;
                    }

                    // gamma*U^H*b
                    realUb *= gammas[n];
                    imagUb *= gammas[n];

                    for (int i = n; i < numRows; i++)
                    {
                        double realU = u[i * 2];
                        double imagU = u[i * 2 + 1];

                        a[i * 2] -= realU * realUb - imagU * imagUb;
                        a[i * 2 + 1] -= realU * imagUb + imagU * realUb;
                    }
                }

                // solve for Rx = b using the standard upper triangular solver
                TriangularSolver_ZDRM.solveU(QR.data, a, numCols);

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
            return false;
        }

        //@Override
        public override bool modifiesB()
        {
            return false;
        }

        //@Override
        public override DecompositionInterface<ZMatrixRMaj> getDecomposition()
        {
            return decomposer;
        }
    }
}