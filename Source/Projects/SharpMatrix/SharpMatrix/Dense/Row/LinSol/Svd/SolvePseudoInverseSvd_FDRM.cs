using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row.Factory;
using SharpMatrix.Interfaces.Decomposition;
using SharpMatrix.Interfaces.LinSol;

namespace SharpMatrix.Dense.Row.LinSol.SVD
{
    //package org.ejml.dense.row.linsol.svd;

/**
 * <p>
 * The pseudo-inverse is typically used to solve over determined system for which there is no unique solution.<br>
 * x=inv(A<sup>T</sup>A)A<sup>T</sup>b<br>
 * where A &isin; &real; <sup>m &times; n</sup> and m &ge; n.
 * </p>
 *
 * <p>
 * This class implements the Moore-Penrose pseudo-inverse using SVD and should never fail.  Alternative implementations
 * can use Cholesky decomposition, but those will fail if the A<sup>T</sup>A matrix is singular.
 * However the Cholesky implementation is much faster.
 * </p>
 *
 * @author Peter Abeles
 */
    public class SolvePseudoInverseSvd_FDRM : LinearSolverDense<FMatrixRMaj>
    {

        // Used to compute pseudo inverse
        private SingularValueDecomposition_F32<FMatrixRMaj> svd;

        // the results of the pseudo-inverse
        private FMatrixRMaj pinv = new FMatrixRMaj(1, 1);

        // relative threshold used to select singular values
        private float threshold = UtilEjml.F_EPS;

        // Internal workspace
        private FMatrixRMaj U_t = new FMatrixRMaj(1, 1), V = new FMatrixRMaj(1, 1);

        /**
         * Creates a new solver targeted at the specified matrix size.
         *
         * @param maxRows The expected largest matrix it might have to process.  Can be larger.
         * @param maxCols The expected largest matrix it might have to process.  Can be larger.
         */
        public SolvePseudoInverseSvd_FDRM(int maxRows, int maxCols)
        {

            svd = DecompositionFactory_FDRM.svd(maxRows, maxCols, true, true, true);
        }

        /**
         * Creates a solver targeted at matrices around 100x100
         */
        public SolvePseudoInverseSvd_FDRM()
            : this(100, 100)
        {
        }

        public virtual bool setA(FMatrixRMaj A)
        {
            pinv.reshape(A.numCols, A.numRows, false);

            if (!svd.decompose(A))
                return false;

            svd.getU(U_t, true);
            svd.getV(V, false);
            float[] S = svd.getSingularValues();
            int N = Math.Min(A.numRows, A.numCols);

            // compute the threshold for singular values which are to be zeroed
            float maxSingular = 0;
            for (int i = 0; i < N; i++)
            {
                if (S[i] > maxSingular)
                    maxSingular = S[i];
            }

            float tau = threshold * Math.Max(A.numCols, A.numRows) * maxSingular;

            // computer the pseudo inverse of A
            if (maxSingular != 0.0f)
            {
                for (int i = 0; i < N; i++)
                {
                    float s = S[i];
                    if (s < tau)
                        S[i] = 0;
                    else
                        S[i] = 1.0f / S[i];
                }
            }

            // V*W
            for (int i = 0; i < V.numRows; i++)
            {
                int index = i * V.numCols;
                for (int j = 0; j < V.numCols; j++)
                {
                    V.data[index++] *= S[j];
                }
            }

            // V*W*U^T
            CommonOps_FDRM.mult(V, U_t, pinv);

            return true;
        }

        public virtual /**/ double quality()
        {
            throw new ArgumentException("Not supported by this solver.");
        }

        public virtual void solve(FMatrixRMaj b, FMatrixRMaj x)
        {
            CommonOps_FDRM.mult(pinv, b, x);
        }

        public virtual void invert(FMatrixRMaj A_inv)
        {
            A_inv.set(pinv);
        }

        public virtual bool modifiesA()
        {
            return svd.inputModified();
        }

        public virtual bool modifiesB()
        {
            return false;
        }

        public virtual DecompositionInterface<FMatrixRMaj> getDecomposition()
        {
            return svd;
        }

        /**
         * Specify the relative threshold used to select singular values.  By default it's UtilEjml.F_EPS.
         * @param threshold The singular value threshold
         */
        public void setThreshold(float threshold)
        {
            this.threshold = threshold;
        }

        public SingularValueDecomposition<FMatrixRMaj> getDecomposer()
        {
            return svd;
        }
    }
}