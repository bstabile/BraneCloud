using SharpMatrix.Data;
using SharpMatrix.Dense.Block;
using SharpMatrix.Dense.Block.LinSol.Chol;

namespace SharpMatrix.Dense.Row.LinSol.Chol
{
    //package org.ejml.dense.row.linsol.chol;

/**
 * A wrapper around {@link CholeskyDecomposition_F64}(DMatrixRBlock) that allows
 * it to be easily used with {@link DMatrixRMaj}.
 *
 * @author Peter Abeles
 */
    public class LinearSolverChol_DDRB : LinearSolver_DDRB_to_DDRM
    {

        public LinearSolverChol_DDRB()
            : base(new CholeskyOuterSolver_DDRB())
        {
        }

        /**
         * Only converts the B matrix and passes that onto solve.  Te result is then copied into
         * the input 'X' matrix.
         * 
         * @param B A matrix &real; <sup>m &times; p</sup>.  Not modified.
         * @param X A matrix &real; <sup>n &times; p</sup>, where the solution is written to.  Modified.
         */
        public override void solve(DMatrixRMaj B, DMatrixRMaj X)
        {
            blockB.reshape(B.numRows, B.numCols, false);
            MatrixOps_DDRB.convert(B, blockB);

            // since overwrite B is true X does not need to be passed in
            alg.solve(blockB, null);

            MatrixOps_DDRB.convert(blockB, X);
        }

    }
}