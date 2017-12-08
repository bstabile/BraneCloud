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
 * Rx=Q^T b<br>
 * </p>
 *
 * @author Peter Abeles
 */
    public class LinearSolverQrHouse_DDRM : LinearSolverAbstract_DDRM
    {

        private QRDecompositionHouseholder_DDRM decomposer;

        private double[] a, u;

        private int maxRows = -1;

        private DMatrixRMaj QR;
        private double[] gammas;

        /**
         * Creates a linear solver that uses QR decomposition.
         */
        public LinearSolverQrHouse_DDRM()
        {
            decomposer = new QRDecompositionHouseholder_DDRM();


        }

        public void setMaxSize(int maxRows)
        {
            this.maxRows = maxRows;

            a = new double[maxRows];
            u = new double[maxRows];
        }

        /**
         * Performs QR decomposition on A
         *
         * @param A not modified.
         */
        public override bool setA(DMatrixRMaj A)
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

        public override /**/ double quality()
        {
            return SpecializedOps_DDRM.qualityTriangular(QR);
        }

        /**
         * Solves for X using the QR decomposition.
         *
         * @param B A matrix that is n by m.  Not modified.
         * @param X An n by m matrix where the solution is writen to.  Modified.
         */
        public override void solve(DMatrixRMaj B, DMatrixRMaj X)
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
                    a[i] = B.data[i * BnumCols + colB];
                }

                // Solve Qa=b
                // a = Q'b
                // a = Q_{n-1}...Q_2*Q_1*b
                //
                // Q_n*b = (I-gamma*u*u^T)*b = b - u*(gamma*U^T*b)
                for (int n = 0; n < numCols; n++)
                {
                    u[n] = 1;
                    double ub = a[n];
                    // U^T*b
                    for (int i = n + 1; i < numRows; i++)
                    {
                        ub += (u[i] = QR.unsafe_get(i, n)) * a[i];
                    }

                    // gamma*U^T*b
                    ub *= gammas[n];

                    for (int i = n; i < numRows; i++)
                    {
                        a[i] -= u[i] * ub;
                    }
                }

                // solve for Rx = b using the standard upper triangular solver
                TriangularSolver_DDRM.solveU(QR.data, a, numCols);

                // save the results
                for (int i = 0; i < numCols; i++)
                {
                    X.data[i * X.numCols + colB] = a[i];
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

        public override DecompositionInterface<DMatrixRMaj> getDecomposition()
        {
            return decomposer;
        }
    }
}