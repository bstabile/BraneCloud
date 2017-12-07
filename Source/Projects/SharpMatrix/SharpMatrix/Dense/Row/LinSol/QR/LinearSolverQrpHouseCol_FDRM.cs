using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.QR;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.LinSol.QR
{
    //package org.ejml.dense.row.linsol.qr;

/**
 * <p>
 * Performs a pseudo inverse solver using the {@link org.ejml.dense.row.decomposition.qr.QRColPivDecompositionHouseholderColumn_FDRM} decomposition
 * directly.  For details on how the pseudo inverse is computed see {@link BaseLinearSolverQrp_FDRM}.
 * </p>
 * 
 * @author Peter Abeles
 */
    public class LinearSolverQrpHouseCol_FDRM : BaseLinearSolverQrp_FDRM
    {

        // NOTE: This hides the protected field from the base class.
        //       I'm assuming this was intended.
        // Computes the QR decomposition
        private new QRColPivDecompositionHouseholderColumn_FDRM decomposition;

        // storage for basic solution
        private FMatrixRMaj x_basic = new FMatrixRMaj(1, 1);

        public LinearSolverQrpHouseCol_FDRM(QRColPivDecompositionHouseholderColumn_FDRM decomposition,
            bool norm2Solution)
            : base(decomposition, norm2Solution)
        {
            this.decomposition = decomposition;
        }

        public override void solve(FMatrixRMaj B, FMatrixRMaj X)
        {
            if (X.numRows != numCols)
                throw new ArgumentException("Unexpected dimensions for X");
            else if (B.numRows != numRows || B.numCols != X.numCols)
                throw new ArgumentException("Unexpected dimensions for B");

            int BnumCols = B.numCols;

            // get the pivots and transpose them
            int[] pivots = decomposition.getColPivots();

            float[][] qr = decomposition.getQR();
            float[] gammas = decomposition.getGammas();

            // solve each column one by one
            for (int colB = 0; colB < BnumCols; colB++)
            {
                x_basic.reshape(numRows, 1);
                Y.reshape(numRows, 1);

                // make a copy of this column in the vector
                for (int i = 0; i < numRows; i++)
                {
                    x_basic.data[i] = B.get(i, colB);
                }

                // Solve Q*x=b => x = Q'*b
                // Q_n*b = (I-gamma*u*u^T)*b = b - u*(gamma*U^T*b)
                for (int i = 0; i < rank; i++)
                {
                    float[] u = qr[i];

                    float vv = u[i];
                    u[i] = 1;
                    QrHelperFunctions_FDRM.rank1UpdateMultR(x_basic, u, gammas[i], 0, i, numRows, Y.data);
                    u[i] = vv;
                }

                // solve for Rx = b using the standard upper triangular solver
                TriangularSolver_FDRM.solveU(R11.data, x_basic.data, rank);

                // finish the basic solution by filling in zeros
                x_basic.reshape(numCols, 1, true);
                for (int i = rank; i < numCols; i++)
                    x_basic.data[i] = 0;

                if (norm2Solution && rank < numCols)
                    upgradeSolution(x_basic);

                // save the results
                for (int i = 0; i < numCols; i++)
                {
                    X.set(pivots[i], colB, x_basic.data[i]);
                }
            }
        }

        public override bool modifiesA()
        {
            return decomposition.inputModified();
        }

        public override bool modifiesB()
        {
            return false;
        }
    }
}