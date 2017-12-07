using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.Chol;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.LinSol.Chol
{
    //package org.ejml.dense.row.linsol.chol;

    /**
     * @author Peter Abeles
     */
    public class LinearSolverCholLDL_FDRM : LinearSolverAbstract_FDRM
    {

        private CholeskyDecompositionLDL_FDRM decomposer;
        private int n;
        private float[] vv;
        private float[] el;
        private float[] d;

        public LinearSolverCholLDL_FDRM(CholeskyDecompositionLDL_FDRM decomposer)
        {
            this.decomposer = decomposer;
        }

        public LinearSolverCholLDL_FDRM()
        {
            this.decomposer = new CholeskyDecompositionLDL_FDRM();
        }

        public override bool setA(FMatrixRMaj A)
        {
            _setA(A);

            if (decomposer.decompose(A))
            {
                n = A.numCols;
                vv = decomposer._getVV();
                el = decomposer.getL().data;
                d = decomposer.getDiagonal();
                return true;
            }
            else
            {
                return false;
            }
        }

        public override /**/ double quality()
        {
            return Math.Abs(SpecializedOps_FDRM.diagProd(decomposer.getL()));
        }

        /**
         * <p>
         * Using the decomposition, finds the value of 'X' in the linear equation below:<br>
         *
         * A*x = b<br>
         *
         * where A has dimension of n by n, x and b are n by m dimension.
         * </p>
         * <p>
         * *Note* that 'b' and 'x' can be the same matrix instance.
         * </p>
         *
         * @param B A matrix that is n by m.  Not modified.
         * @param X An n by m matrix where the solution is writen to.  Modified.
         */
        public override void solve(FMatrixRMaj B, FMatrixRMaj X)
        {
            if (B.numCols != X.numCols && B.numRows != n && X.numRows != n)
            {
                throw new ArgumentException("Unexpected matrix size");
            }

            int numCols = B.numCols;

            float[] dataB = B.data;
            float[] dataX = X.data;

            for (int j = 0; j < numCols; j++)
            {
                for (int i = 0; i < n; i++) vv[i] = dataB[i * numCols + j];
                solveInternal();
                for (int i = 0; i < n; i++) dataX[i * numCols + j] = vv[i];
            }
        }

        /**
         * Used internally to find the solution to a single column vector.
         */
        private void solveInternal()
        {
            // solve L*s=b storing y in x
            TriangularSolver_FDRM.solveL(el, vv, n);

            // solve D*y=s
            for (int i = 0; i < n; i++)
            {
                vv[i] /= d[i];
            }

            // solve L^T*x=y
            TriangularSolver_FDRM.solveTranL(el, vv, n);
        }

        /**
         * Sets the matrix 'inv' equal to the inverse of the matrix that was decomposed.
         *
         * @param inv Where the value of the inverse will be stored.  Modified.
         */
        public override void invert(FMatrixRMaj inv)
        {
            if (inv.numRows != n || inv.numCols != n)
            {
                throw new InvalidOperationException("Unexpected matrix dimension");
            }

            float[] a = inv.data;

            // solve L*z = b
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    float sum = (i == j) ? 1.0f : 0.0f;
                    for (int k = i - 1; k >= j; k--)
                    {
                        sum -= el[i * n + k] * a[j * n + k];
                    }
                    a[j * n + i] = sum;
                }
            }

            // solve D*y=z
            for (int i = 0; i < n; i++)
            {
                float inv_d = 1.0f / d[i];
                for (int j = 0; j <= i; j++)
                {
                    a[j * n + i] *= inv_d;
                }
            }

            // solve L^T*x = y
            for (int i = n - 1; i >= 0; i--)
            {
                for (int j = 0; j <= i; j++)
                {
                    float sum = (i < j) ? 0 : a[j * n + i];
                    for (int k = i + 1; k < n; k++)
                    {
                        sum -= el[k * n + i] * a[j * n + k];
                    }
                    a[i * n + j] = a[j * n + i] = sum;
                }
            }
        }

        public override bool modifiesA()
        {
            return decomposer.inputModified();
        }

        public override bool modifiesB()
        {
            return false;
        }

        public override DecompositionInterface<FMatrixRMaj> getDecomposition()
        {
            return decomposer;
        }
    }
}