using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.LinSol;
using BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.Decomposition.Chol;
using BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.Misc;

namespace BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.LinSol.Chol
{
    //package org.ejml.sparse.csc.linsol.chol;

/**
 * Linear solver using a sparse Cholesky decomposition.
 *
 * @author Peter Abeles
 */
    public class LinearSolverCholesky_DSCC : LinearSolverSparse<DMatrixSparseCSC, DMatrixRMaj>
    {

        CholeskyUpLooking_DSCC cholesky;

        ApplyFillReductionPermutation reduce;

        DGrowArray gb = new DGrowArray();
        DGrowArray gx = new DGrowArray();

        public LinearSolverCholesky_DSCC(CholeskyUpLooking_DSCC cholesky,
            ComputePermutation<DMatrixSparseCSC> fillReduce)
        {
            this.cholesky = cholesky;
            this.reduce = new ApplyFillReductionPermutation(fillReduce, true);
        }

        //@Override
        public bool setA(DMatrixSparseCSC A)
        {
            DMatrixSparseCSC C = reduce.apply(A);
            return cholesky.decompose(C);
        }

        //@Override
        public double quality()
        {
            return TriangularSolver_DSCC.qualityTriangular(cholesky.getL());
        }

        //@Override
        public void lockStructure()
        {
            cholesky.lockStructure();
        }

        //@Override
        public bool isStructureLocked()
        {
            return cholesky.isStructureLocked();
        }

        //@Override
        public void solve(DMatrixRMaj B, DMatrixRMaj X)
        {

            DMatrixSparseCSC L = cholesky.getL();

            int N = L.numRows;

            double[] b = TriangularSolver_DSCC.adjust(gb, N);
            double[] x = TriangularSolver_DSCC.adjust(gx, N);

            int[] Pinv = reduce.getArrayPinv();

            for (int col = 0; col < B.numCols; col++)
            {
                int index = col;
                for (int i = 0; i < N; i++, index += B.numCols) b[i] = B.data[index];

                if (Pinv != null)
                {
                    CommonOps_DSCC.permuteInv(Pinv, b, x, N);
                    TriangularSolver_DSCC.solveL(L, x);
                    TriangularSolver_DSCC.solveTranL(L, x);
                    CommonOps_DSCC.permute(Pinv, x, b, N);
                }
                else
                {
                    TriangularSolver_DSCC.solveL(L, b);
                    TriangularSolver_DSCC.solveTranL(L, b);
                }

                index = col;
                for (int i = 0; i < N; i++, index += X.numCols) X.data[index] = b[i];
            }
        }

        //@Override
        public bool modifiesA()
        {
            return cholesky.inputModified();
        }

        //@Override
        public bool modifiesB()
        {
            return false;
        }

        //@Override
        public DecompositionInterface<DMatrixSparseCSC> getDecomposition()
        {
            return cholesky;
        }
    }
}