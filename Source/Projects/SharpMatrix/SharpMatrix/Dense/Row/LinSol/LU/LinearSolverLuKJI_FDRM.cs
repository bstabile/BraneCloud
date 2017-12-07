using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.LU;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.LinSol.LU
{
    //package org.ejml.dense.row.linsol.lu;

/**
 * To avoid cpu cache issues the order in which the arrays are traversed have been changed.
 * There seems to be no performance benit relative to {@link LinearSolverLu_FDRM} in this approach
 * and b and x can't be the same instance, which means it has slightly less functionality.
 *
 * @author Peter Abeles
 */
    public class LinearSolverLuKJI_FDRM : LinearSolverLuBase_FDRM
    {

        private float[] dataLU;
        private int[] pivot;

        public LinearSolverLuKJI_FDRM(LUDecompositionBase_FDRM decomp)
            : base(decomp)
        {

        }

        //@Override
        public override bool setA(FMatrixRMaj A)
        {
            bool ret = base.setA(A);

            pivot = decomp.getPivot();
            dataLU = decomp.getLU().data;

            return ret;
        }

        /**
         * An other implementation of solve() that processes the matrices in a different order.
         * It seems to have the same runtime performance as {@link #solve} and is more complicated.
         * It is being kept around to avoid future replication of work.
         *
         * @param b A matrix that is n by m.  Not modified.
         * @param x An n by m matrix where the solution is writen to.  Modified.
         */
        //@Override
        public override void solve(FMatrixRMaj b, FMatrixRMaj x)
        {
            if (b.numCols != x.numCols || b.numRows != numRows || x.numRows != numCols)
            {
                throw new ArgumentException("Unexpected matrix size");
            }

            if (b != x)
            {
                SpecializedOps_FDRM.copyChangeRow(pivot, b, x);
            }
            else
            {
                throw new ArgumentException("Current doesn't support using the same matrix instance");
            }

            // Copy right hand side with pivoting
            int nx = b.numCols;
            float[] dataX = x.data;

            // Solve L*Y = B(piv,:)
            for (int k = 0; k < numCols; k++)
            {
                for (int i = k + 1; i < numCols; i++)
                {
                    for (int j = 0; j < nx; j++)
                    {
                        dataX[i * nx + j] -= dataX[k * nx + j] * dataLU[i * numCols + k];
                    }
                }
            }
            // Solve U*X = Y;
            for (int k = numCols - 1; k >= 0; k--)
            {
                for (int j = 0; j < nx; j++)
                {
                    dataX[k * nx + j] /= dataLU[k * numCols + k];
                }
                for (int i = 0; i < k; i++)
                {
                    for (int j = 0; j < nx; j++)
                    {
                        dataX[i * nx + j] -= dataX[k * nx + j] * dataLU[i * numCols + k];
                    }
                }
            }
        }
    }
}