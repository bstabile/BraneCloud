using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row.Decomposition.Chol;
using Randomization;

namespace SharpMatrix.Dense.Row
{
    //package org.ejml.dense.row;

/**
 * Generates random vectors based on a zero mean multivariate Gaussian distribution.  The covariance
 * matrix is provided in the constructor.
 */
    public class CovarianceRandomDraw_FDRM
    {
        private FMatrixRMaj A;
        private IMersenneTwister rand;
        private FMatrixRMaj r;

        /**
         * Creates a random distribution with the specified mean and covariance.  The references
         * to the variables are not saved, their value are copied.
         *
         * @param rand Used to create the random numbers for the draw. Reference is saved.
         * @param cov The covariance of the distribution.  Not modified.
         */
        public CovarianceRandomDraw_FDRM(IMersenneTwister rand, FMatrixRMaj cov)
        {
            r = new FMatrixRMaj(cov.numRows, 1);
            CholeskyDecompositionInner_FDRM cholesky = new CholeskyDecompositionInner_FDRM(true);

            if (cholesky.inputModified())
                cov = (FMatrixRMaj) cov.copy();
            if (!cholesky.decompose(cov))
                throw new InvalidOperationException("Decomposition failed!");

            A = cholesky.getT();
            this.rand = rand;
        }

        /**
         * Makes a draw on the distribution.  The results are added to parameter 'x'
         */
        public void next(FMatrixRMaj x)
        {
            for (int i = 0; i < r.numRows; i++)
            {
                r.set(i, 0, (float) rand.NextGaussian());
            }

            CommonOps_FDRM.multAdd(A, r, x);
        }

        /**
         * Computes the likelihood of the random draw
         *
         * @return The likelihood.
         */
        public float computeLikelihoodP()
        {
            float ret = 1.0f;

            for (int i = 0; i < r.numRows; i++)
            {
                float a = r.get(i, 0);

                ret *= (float) Math.Exp(-a * a / 2.0f);
            }

            return ret;
        }
    }
}