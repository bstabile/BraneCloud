using System;
using SharpMatrix.Data;
using SharpMatrix.Interfaces.Decomposition;
using SharpMatrix.Interfaces.LinSol;
using SharpMatrix.Sparse.Csc.Decomposition.LU;
using SharpMatrix.Sparse.Csc.Misc;

namespace SharpMatrix.Sparse.Csc.LinSol.LU
{
    //package org.ejml.sparse.csc.linsol.lu;

/**
 * LU Decomposition based solver for square matrices. Uses {@link LuUpLooking_DSCC} internally.
 *
 * @author Peter Abeles
 */
    public class LinearSolverLu_DSCC : LinearSolverSparse<DMatrixSparseCSC, DMatrixRMaj>
    {

        LuUpLooking_DSCC decomposition;

        private DGrowArray gx = new DGrowArray();
        private DGrowArray gb = new DGrowArray();

        public LinearSolverLu_DSCC(LuUpLooking_DSCC decomposition)
        {
            this.decomposition = decomposition;
        }

        //@Override
        public bool setA(DMatrixSparseCSC A)
        {
            return decomposition.decompose(A);
        }

        //@Override
        public double quality()
        {
            return TriangularSolver_DSCC.qualityTriangular(decomposition.getU());
        }

        //@Override
        public void lockStructure()
        {
            decomposition.lockStructure();
        }

        //@Override
        public bool isStructureLocked()
        {
            return decomposition.isStructureLocked();
        }

        //@Override
        public void solve(DMatrixRMaj B, DMatrixRMaj X)
        {
//        if( B.numCols != X.numCols || B.numRows != numRows || X.numRows != numCols) {
//            throw new ArgumentException("Unexpected matrix size");
//        }

            int[] pinv = decomposition.getPinv();
            int[] q = decomposition.getReducePermutation();
            double[] x = TriangularSolver_DSCC.adjust(gx, X.numRows);
            double[] b = TriangularSolver_DSCC.adjust(gb, B.numRows);

            DMatrixSparseCSC L = decomposition.getL();
            DMatrixSparseCSC U = decomposition.getU();

            bool reduceFill = decomposition.getReduceFill() != null;

            // process each column in X and B individually
            for (int colX = 0; colX < X.numCols; colX++)
            {
                int index = colX;
                for (int i = 0; i < B.numRows; i++, index += X.numCols) b[i] = B.data[index];

                CommonOps_DSCC.permuteInv(pinv, b, x, X.numRows);
                TriangularSolver_DSCC.solveL(L, x);
                TriangularSolver_DSCC.solveU(U, x);
                double[] d;
                if (reduceFill)
                {
                    CommonOps_DSCC.permute(q, x, b, X.numRows);
                    d = b;
                }
                else
                {
                    d = x;
                }
                index = colX;
                for (int i = 0; i < X.numRows; i++, index += X.numCols) X.data[index] = d[i];
            }
        }

        //@Override
        public bool modifiesA()
        {
            return decomposition.inputModified();
        }

        //@Override
        public bool modifiesB()
        {
            return false;
        }

        //@Override
        public DecompositionInterface<DMatrixSparseCSC> getDecomposition()
        {
            return decomposition;
        }
    }
}