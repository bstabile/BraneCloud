using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.LinSol;
using BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.Decomposition.QR;
using BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.Misc;

namespace BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.LinSol.QR
{
    //package org.ejml.sparse.csc.linsol.qr;

/**
 * Sparse linear solver implemented using {@link QrLeftLookingDecomposition_DSCC}.
 *
 * @author Peter Abeles
 */
    public class LinearSolverQrLeftLooking_DSCC : LinearSolverSparse<DMatrixSparseCSC, DMatrixRMaj>
    {

        private QrLeftLookingDecomposition_DSCC qr;
        private int m, n;

        private DGrowArray gb = new DGrowArray();
        private DGrowArray gbp = new DGrowArray();
        private DGrowArray gx = new DGrowArray();

        public LinearSolverQrLeftLooking_DSCC(QrLeftLookingDecomposition_DSCC qr)
        {
            this.qr = qr;
        }

        //@Override
        public bool setA(DMatrixSparseCSC A)
        {
            if (A.numCols > A.numRows)
                throw new ArgumentException("Can't handle wide matrices");
            this.m = A.numRows;
            this.n = A.numCols;
            return qr.decompose(A) && !qr.isSingular();
        }

        //@Override
        public /**/ double quality()
        {
            return TriangularSolver_DSCC.qualityTriangular(qr.getR());
        }

        //@Override
        public void lockStructure()
        {
            qr.lockStructure();
        }

        //@Override
        public bool isStructureLocked()
        {
            return qr.isStructureLocked();
        }

        //@Override
        public void solve(DMatrixRMaj B, DMatrixRMaj X)
        {
            double[] b = TriangularSolver_DSCC.adjust(gb, B.numRows);
            double[] bp = TriangularSolver_DSCC.adjust(gbp, B.numRows);
            double[] x = TriangularSolver_DSCC.adjust(gx, X.numRows);

            int[] pinv = qr.getStructure().getPinv();

            // process each column in X and B individually
            for (int colX = 0; colX < X.numCols; colX++)
            {
                int index = colX;
                for (int i = 0; i < B.numRows; i++, index += X.numCols) b[i] = B.data[index];

                // apply row pivots
                CommonOps_DSCC.permuteInv(pinv, b, bp, m);

                // apply Householder reflectors
                for (int j = 0; j < n; j++)
                {
                    QrHelperFunctions_DSCC.applyHouseholder(qr.getV(), j, qr.getBeta(j), bp);
                }
                // Solve for R*x = b
                TriangularSolver_DSCC.solveU(qr.getR(), bp);

                // undo the permutation
                double[] output;
                if (qr.isFillPermutated())
                {
                    CommonOps_DSCC.permute(qr.getFillPermutation(), bp, x, X.numRows);
                    output = x;
                }
                else
                {
                    output = bp;
                }

                index = colX;
                for (int i = 0; i < X.numRows; i++, index += X.numCols) X.data[index] = output[i];
            }
        }

        //@Override
        public bool modifiesA()
        {
            return qr.inputModified();
        }

        //@Override
        public bool modifiesB()
        {
            return false;
        }

        //@Override
        public DecompositionInterface<DMatrixSparseCSC> getDecomposition()
        {
            return qr;
        }
    }
}