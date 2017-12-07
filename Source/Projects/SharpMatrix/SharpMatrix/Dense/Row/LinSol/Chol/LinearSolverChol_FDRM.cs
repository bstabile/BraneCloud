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
    public class LinearSolverChol_FDRM : LinearSolverAbstract_FDRM
    {

        CholeskyDecompositionCommon_FDRM decomposer;
        int n;
        float[] vv;
        float[] t;

        public LinearSolverChol_FDRM(CholeskyDecompositionCommon_FDRM decomposer)
        {
            this.decomposer = decomposer;
        }

        public override bool setA(FMatrixRMaj A)
        {
            if (A.numRows != A.numCols)
                throw new ArgumentException("Matrix must be square");

            _setA(A);

            if (decomposer.decompose(A))
            {
                n = A.numCols;
                vv = decomposer._getVV();
                t = decomposer.getT().data;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override /**/ double quality()
        {
            return SpecializedOps_FDRM.qualityTriangular(decomposer.getT());
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
            if (B.numCols != X.numCols || B.numRows != n || X.numRows != n)
            {
                throw new ArgumentException("Unexpected matrix size");
            }

            int numCols = B.numCols;

            float[] dataB = B.data;
            float[] dataX = X.data;

            if (decomposer.isLower())
            {
                for (int j = 0; j < numCols; j++)
                {
                    for (int i = 0; i < n; i++) vv[i] = dataB[i * numCols + j];
                    solveInternalL();
                    for (int i = 0; i < n; i++) dataX[i * numCols + j] = vv[i];
                }
            }
            else
            {
                throw new InvalidOperationException("Implement");
            }
        }

        /**
         * Used internally to find the solution to a single column vector.
         */
        private void solveInternalL()
        {
            // solve L*y=b storing y in x
            TriangularSolver_FDRM.solveL(t, vv, n);

            // solve L^T*x=y
            TriangularSolver_FDRM.solveTranL(t, vv, n);
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
            if (inv.data == t)
            {
                throw new ArgumentException("Passing in the same matrix that was decomposed.");
            }

            float[] a = inv.data;

            if (decomposer.isLower())
            {
                setToInverseL(a);
            }
            else
            {
                throw new InvalidOperationException("Implement");
            }
        }

        /**
         * Sets the matrix to the inverse using a lower triangular matrix.
         */
        public void setToInverseL(float[] a)
        {
            // TODO reorder these operations to avoid cache misses

            // inverts the lower triangular system and saves the result
            // in the upper triangle to minimize cache misses
            for (int i = 0; i < n; i++)
            {
                float el_ii = t[i * n + i];
                for (int j = 0; j <= i; j++)
                {
                    float sum = (i == j) ? 1.0f : 0;
                    for (int k = i - 1; k >= j; k--)
                    {
                        sum -= t[i * n + k] * a[j * n + k];
                    }
                    a[j * n + i] = sum / el_ii;
                }
            }
            // solve the system and handle the previous solution being in the upper triangle
            // takes advantage of symmetry
            for (int i = n - 1; i >= 0; i--)
            {
                float el_ii = t[i * n + i];

                for (int j = 0; j <= i; j++)
                {
                    float sum = (i < j) ? 0 : a[j * n + i];
                    for (int k = i + 1; k < n; k++)
                    {
                        sum -= t[k * n + i] * a[j * n + k];
                    }
                    a[i * n + j] = a[j * n + i] = sum / el_ii;
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