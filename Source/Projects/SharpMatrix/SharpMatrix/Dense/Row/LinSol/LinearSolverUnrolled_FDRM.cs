using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Misc;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.LinSol;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.LinSol
{
    //package org.ejml.dense.row.linsol;

/**
 * Solver which uses an unrolled inverse to compute the inverse.  This can only invert matrices and not solve.
 * This is faster than LU inverse but only supports small matrices..
 *
 * @author Peter Abeles
 */
    public class LinearSolverUnrolled_FDRM : LinearSolverDense<FMatrixRMaj>
    {
        FMatrixRMaj A;

        //@Override
        public bool setA(FMatrixRMaj A)
        {
            if (A.numRows != A.numCols)
                return false;

            this.A = A;
            return A.numRows <= UnrolledInverseFromMinor_FDRM.MAX;
        }

        //@Override
        public /**/ double quality()
        {
            throw new ArgumentException("Not supported by this solver.");
        }

        //@Override
        public void solve(FMatrixRMaj B, FMatrixRMaj X)
        {
            throw new InvalidOperationException("Not supported");
        }

        //@Override
        public void invert(FMatrixRMaj A_inv)
        {
            if (A.numRows == 1)
                A_inv.set(0, 1.0f / A.get(0));
            UnrolledInverseFromMinor_FDRM.inv(A, A_inv);
        }

        //@Override
        public bool modifiesA()
        {
            return false;
        }

        //@Override
        public bool modifiesB()
        {
            return false;
        }

        //@Override
        public DecompositionInterface<FMatrixRMaj> getDecomposition()
        {
            return null;
        }
    }
}
