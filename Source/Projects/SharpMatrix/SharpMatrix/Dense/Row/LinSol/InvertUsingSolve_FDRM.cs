using System;
using SharpMatrix.Data;
using SharpMatrix.Interfaces.LinSol;

namespace SharpMatrix.Dense.Row.LinSol
{
    //package org.ejml.dense.row.linsol;

/**
 * A matrix can be easily inverted by solving a system with an identify matrix.  The only
 * disadvantage of this approach is that additional computations are required compared to
 * a specialized solution.
 *
 * @author Peter Abeles
 */
    public class InvertUsingSolve_FDRM
    {

        public static void invert(LinearSolverDense<FMatrixRMaj> solver, FMatrix1Row A, FMatrixRMaj A_inv,
            FMatrixRMaj storage)
        {

            if (A.numRows != A_inv.numRows || A.numCols != A_inv.numCols)
            {
                throw new ArgumentException("A and A_inv must have the same dimensions");
            }

            CommonOps_FDRM.setIdentity(storage);

            solver.solve(storage, A_inv);
        }

        public static void invert(LinearSolverDense<FMatrixRMaj> solver, FMatrix1Row A, FMatrixRMaj A_inv)
        {

            if (A.numRows != A_inv.numRows || A.numCols != A_inv.numCols)
            {
                throw new ArgumentException("A and A_inv must have the same dimensions");
            }

            CommonOps_FDRM.setIdentity(A_inv);

            solver.solve(A_inv, A_inv);
        }
    }
}