using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row.Decomposition;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Row.LinSol.QR
{
    //package org.ejml.dense.row.linsol.qr;

/**
 * <p>
 * A pseudo inverse solver for a generic QR column pivot decomposition algorithm.  See
 * {@link BaseLinearSolverQrp_DDRM} for technical details on the algorithm.
 * </p>
 *
 * @author Peter Abeles
 */
    public class SolvePseudoInverseQrp_DDRM : BaseLinearSolverQrp_DDRM
    {

        // stores the orthogonal Q matrix from QR decomposition
        private DMatrixRMaj Q = new DMatrixRMaj(1, 1);

        // storage for basic solution
        private DMatrixRMaj x_basic = new DMatrixRMaj(1, 1);

        /**
         * Configure and provide decomposition
         *
         * @param decomposition Decomposition used.
         * @param norm2Solution If true the basic solution will be returned, false the minimal 2-norm solution.
         */
        public SolvePseudoInverseQrp_DDRM(QRPDecomposition_F64<DMatrixRMaj> decomposition,
            bool norm2Solution)
            : base(decomposition, norm2Solution)
        {
        }

        public override bool setA(DMatrixRMaj A)
        {
            if (!base.setA(A))
                return false;

            Q.reshape(A.numRows, A.numRows);

            decomposition.getQ(Q, false);

            return true;
        }

        public override void solve(DMatrixRMaj B, DMatrixRMaj X)
        {
            if (X.numRows != numCols)
                throw new ArgumentException("Unexpected dimensions for X");
            else if (B.numRows != numRows || B.numCols != X.numCols)
                throw new ArgumentException("Unexpected dimensions for B");

            int BnumCols = B.numCols;

            // get the pivots and transpose them
            int[] pivots = decomposition.getColPivots();

            // solve each column one by one
            for (int colB = 0; colB < BnumCols; colB++)
            {
                x_basic.reshape(numRows, 1);
                Y.reshape(numRows, 1);

                // make a copy of this column in the vector
                for (int i = 0; i < numRows; i++)
                {
                    Y.data[i] = B.get(i, colB);
                }

                // Solve Q*a=b => a = Q'*b
                CommonOps_DDRM.multTransA(Q, Y, x_basic);

                // solve for Rx = b using the standard upper triangular solver
                TriangularSolver_DDRM.solveU(R11.data, x_basic.data, rank);

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