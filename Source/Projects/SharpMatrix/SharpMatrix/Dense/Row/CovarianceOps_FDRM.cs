using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Factory;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Misc;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.LinSol;
using Randomization;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row
{
    //package org.ejml.dense.row;

/**
 * Contains operations specific to covariance matrices.
 *
 * @author Peter Abeles
 */
    public class CovarianceOps_FDRM
    {

        public static float TOL = UtilEjml.TESTP_F32;

        /**
         * This is a fairly light weight check to see of a covariance matrix is valid.
         * It checks to see if the diagonal elements are all positive, which they should be
         * if it is valid.  Not all invalid covariance matrices will be caught by this method.
         *
         * @return true if valid and false if invalid
         */
        public static bool isValidFast(FMatrixRMaj cov)
        {
            return MatrixFeatures_FDRM.isDiagonalPositive(cov);
        }

        /**
         * Performs a variety of tests to see if the provided matrix is a valid
         * covariance matrix.
         *
         * @return  0 = is valid 1 = failed positive diagonal, 2 = failed on symmetry, 2 = failed on positive definite
         */
        public static int isValid(FMatrixRMaj cov)
        {
            if (!MatrixFeatures_FDRM.isDiagonalPositive(cov))
                return 1;

            if (!MatrixFeatures_FDRM.isSymmetric(cov, TOL))
                return 2;

            if (!MatrixFeatures_FDRM.isPositiveSemidefinite(cov))
                return 3;

            return 0;
        }

        /**
         * Performs a matrix inversion operations that takes advantage of the special
         * properties of a covariance matrix.
         *
         * @param cov On input it is a covariance matrix, on output it is the inverse.  Modified.
         * @return true if it could invert the matrix false if it could not.
         */
        public static bool invert(FMatrixRMaj cov)
        {
            return invert(cov, cov);
        }

        /**
         * Performs a matrix inversion operations that takes advantage of the special
         * properties of a covariance matrix.
         *
         * @param cov A covariance matrix. Not modified.
         * @param cov_inv The inverse of cov.  Modified.
         * @return true if it could invert the matrix false if it could not.
         */
        public static bool invert(FMatrixRMaj cov, FMatrixRMaj cov_inv)
        {
            if (cov.numCols <= 4)
            {
                if (cov.numCols != cov.numRows)
                {
                    throw new ArgumentException("Must be a square matrix.");
                }

                if (cov.numCols >= 2)
                    UnrolledInverseFromMinor_FDRM.inv(cov, cov_inv);
                else
                    cov_inv.data[0] = 1.0f / cov_inv.data[0];

            }
            else
            {
                LinearSolverDense<FMatrixRMaj> solver = LinearSolverFactory_FDRM.symmPosDef(cov.numRows);
                // wrap it to make sure the covariance is not modified.
                solver = new LinearSolverSafe<FMatrixRMaj>(solver);
                if (!solver.setA(cov))
                    return false;
                solver.invert(cov_inv);
            }
            return true;
        }

        /**
         * Sets vector to a random value based upon a zero-mean multivariate Gaussian distribution with
         * covariance 'cov'.  If repeat calls are made to this class, consider using {@link CovarianceRandomDraw_FDRM} instead.
         *
         * @param cov The distirbutions covariance.  Not modified.
         * @param vector The random vector. Modified.
         * @param rand Random number generator.
         */
        public static void randomVector(FMatrixRMaj cov,
            FMatrixRMaj vector,
            IMersenneTwister rand)
        {
            CovarianceRandomDraw_FDRM rng = new CovarianceRandomDraw_FDRM(rand, cov);
            rng.next(vector);
        }
    }
}