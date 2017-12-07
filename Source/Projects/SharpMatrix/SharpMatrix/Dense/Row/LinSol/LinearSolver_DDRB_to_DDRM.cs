using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Block;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Block.LinSol.Chol;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.LinSol;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.LinSol
{
    //package org.ejml.dense.row.linsol;

/**
 * Wrapper that allows {@link DMatrixRBlock} to : {@link LinearSolverDense}.  It works
 * by converting {@link DMatrixRMaj} into {@link DMatrixRBlock} and calling the equivalent
 * functions.  Since a local copy is made all input matrices are never modified.
 *
 * @author Peter Abeles
 */
    public class LinearSolver_DDRB_to_DDRM : LinearSolverDense<DMatrixRMaj>
    {
        protected LinearSolverDense<DMatrixRBlock> alg = new CholeskyOuterSolver_DDRB();

        // block matrix copy of the system A matrix.
        protected DMatrixRBlock blockA = new DMatrixRBlock(1, 1);

        // block matrix copy of B matrix passed into solve
        protected DMatrixRBlock blockB = new DMatrixRBlock(1, 1);

        // block matrix copy of X matrix passed into solve
        protected DMatrixRBlock blockX = new DMatrixRBlock(1, 1);

        public LinearSolver_DDRB_to_DDRM(LinearSolverDense<DMatrixRBlock> alg)
        {
            this.alg = alg;
        }

        /**
         * Converts 'A' into a block matrix and call setA() on the block matrix solver.
         *
         * @param A The A matrix in the linear equation. Not modified. Reference saved.
         * @return true if it can solve the system.
         */
        public virtual bool setA(DMatrixRMaj A)
        {
            blockA.reshape(A.numRows, A.numCols, false);
            MatrixOps_DDRB.convert(A, blockA);

            return alg.setA(blockA);
        }

        public virtual /**/ double quality()
        {
            return alg.quality();
        }

        /**
         * Converts B and X into block matrices and calls the block matrix solve routine.
         *
         * @param B A matrix &real; <sup>m &times; p</sup>.  Not modified.
         * @param X A matrix &real; <sup>n &times; p</sup>, where the solution is written to.  Modified.
         */
        public virtual void solve(DMatrixRMaj B, DMatrixRMaj X)
        {
            blockB.reshape(B.numRows, B.numCols, false);
            blockX.reshape(X.numRows, X.numCols, false);
            MatrixOps_DDRB.convert(B, blockB);

            alg.solve(blockB, blockX);

            MatrixOps_DDRB.convert(blockX, X);
        }

        /**
         * Creates a block matrix the same size as A_inv, inverts the matrix and copies the results back
         * onto A_inv.
         * 
         * @param A_inv Where the inverted matrix saved. Modified.
         */
        public virtual void invert(DMatrixRMaj A_inv)
        {
            blockB.reshape(A_inv.numRows, A_inv.numCols, false);

            alg.invert(blockB);

            MatrixOps_DDRB.convert(blockB, A_inv);
        }

        public virtual bool modifiesA()
        {
            return false;
        }

        public virtual bool modifiesB()
        {
            return false;
        }

        public virtual DecompositionInterface<DMatrixRMaj> getDecomposition()
        {
            return (DecompositionInterface<DMatrixRMaj>)alg.getDecomposition();
        }
    }
}
