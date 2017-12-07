using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Block;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Block.LinSol.Chol;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.LinSol.Chol
{
    //package org.ejml.dense.row.linsol.chol;

/**
 * A wrapper around {@link CholeskyDecomposition_F32}(FMatrixRBlock) that allows
 * it to be easily used with {@link FMatrixRMaj}.
 *
 * @author Peter Abeles
 */
    public class LinearSolverChol_FDRB : LinearSolver_FDRB_to_FDRM
    {

        public LinearSolverChol_FDRB()
            : base(new CholeskyOuterSolver_FDRB())
        {
        }

        /**
         * Only converts the B matrix and passes that onto solve.  Te result is then copied into
         * the input 'X' matrix.
         * 
         * @param B A matrix &real; <sup>m &times; p</sup>.  Not modified.
         * @param X A matrix &real; <sup>n &times; p</sup>, where the solution is written to.  Modified.
         */
        public override void solve(FMatrixRMaj B, FMatrixRMaj X)
        {
            blockB.reshape(B.numRows, B.numCols, false);
            MatrixOps_FDRB.convert(B, blockB);

            // since overwrite B is true X does not need to be passed in
            alg.solve(blockB, null);

            MatrixOps_FDRB.convert(blockB, X);
        }

    }
}