using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.Eig.Symm
{
    //package org.ejml.dense.row.decomposition.eig.symm;

/**
 * <p>
 * Computes the eigenvalues and eigenvectors of a symmetric tridiagonal matrix using the symmetric QR algorithm.
 * </p>
 * <p>
 * This implementation is based on the algorithm is sketched out in:<br>
 * David S. Watkins, "Fundamentals of Matrix Computations," Second Edition. page 377-385
 * </p>
 * @author Peter Abeles
 */
    public class SymmetricQrAlgorithm_FDRM
    {

        // performs many of the low level calculations
        private SymmetricQREigenHelper_FDRM helper;

        // transpose of the orthogonal matrix
        private FMatrixRMaj Q;

        // the eigenvalues previously computed
        private float[] eigenvalues;

        private int exceptionalThresh = 15;
        private int maxIterations; // ctor => exceptionalThresh * 15;

        // should it ever analytically compute eigenvalues
        // if this is true then it can't compute eigenvalues at the same time
        private bool fastEigenvalues;

        // is it following a script or not
        private bool followingScript;

        public SymmetricQrAlgorithm_FDRM(SymmetricQREigenHelper_FDRM helper)
        {
            maxIterations = exceptionalThresh * 15;
            this.helper = helper;
        }

        /**
         * Creates a new SymmetricQREigenvalue class that declares its own SymmetricQREigenHelper.
         */
        public SymmetricQrAlgorithm_FDRM()
            : this(new SymmetricQREigenHelper_FDRM())
        {
        }

        public void setMaxIterations(int maxIterations)
        {
            this.maxIterations = maxIterations;
        }

        public FMatrixRMaj getQ()
        {
            return Q;
        }

        public void setQ(FMatrixRMaj q)
        {
            Q = q;
        }

        public void setFastEigenvalues(bool fastEigenvalues)
        {
            this.fastEigenvalues = fastEigenvalues;
        }

        /**
         * Returns the eigenvalue at the specified index.
         *
         * @param index Which eigenvalue.
         * @return The eigenvalue.
         */
        public float getEigenvalue(int index)
        {
            return helper.diag[index];
        }

        /**
         * Returns the number of eigenvalues available.
         *
         * @return How many eigenvalues there are.
         */
        public int getNumberOfEigenvalues()
        {
            return helper.N;
        }

        /**
         * Computes the eigenvalue of the provided tridiagonal matrix.  Note that only the upper portion
         * needs to be tridiagonal.  The bottom diagonal is assumed to be the same as the top.
         *
         * @param sideLength Number of rows and columns in the input matrix.
         * @param diag Diagonal elements from tridiagonal matrix. Modified.
         * @param off Off diagonal elements from tridiagonal matrix. Modified.
         * @return true if it succeeds and false if it fails.
         */
        public bool process(int sideLength,
            float[] diag,
            float[] off,
            float[] eigenvalues)
        {
            if (diag != null)
                helper.init(diag, off, sideLength);
            if (Q == null)
                Q = CommonOps_FDRM.identity(helper.N);
            helper.setQ(Q);

            this.followingScript = true;
            this.eigenvalues = eigenvalues;
            this.fastEigenvalues = false;

            return _process();
        }

        public bool process(int sideLength,
            float[] diag,
            float[] off)
        {
            if (diag != null)
                helper.init(diag, off, sideLength);

            this.followingScript = false;
            this.eigenvalues = null;

            return _process();
        }


        private bool _process()
        {
            while (helper.x2 >= 0)
            {
                // if it has cycled too many times give up
                if (helper.steps > maxIterations)
                {
                    return false;
                }

                if (helper.x1 == helper.x2)
                {
//                Console.WriteLine("Steps = "+helper.steps);
                    // see if it is done processing this submatrix
                    helper.resetSteps();
                    if (!helper.nextSplit())
                        break;
                }
                else if (fastEigenvalues && helper.x2 - helper.x1 == 1)
                {
                    // There are analytical solutions to this case. Just compute them directly.
                    // TODO might be able to speed this up by doing the 3 by 3 case also
                    helper.resetSteps();
                    helper.eigenvalue2by2(helper.x1);
                    helper.setSubmatrix(helper.x2, helper.x2);
                }
                else if (helper.steps - helper.lastExceptional > exceptionalThresh)
                {
                    // it isn't a good sign if exceptional shifts are being done here
                    helper.exceptionalShift();
                }
                else
                {
                    performStep();
                }
                helper.incrementSteps();
//            helper.printMatrix();
            }

//        helper.printMatrix();
            return true;
        }

        /**
         * First looks for zeros and then performs the implicit single step in the QR Algorithm.
         */
        public void performStep()
        {
            // check for zeros
            for (int i = helper.x2 - 1; i >= helper.x1; i--)
            {
                if (helper.isZero(i))
                {
                    helper.splits[helper.numSplits++] = i;
                    helper.x1 = i + 1;
                    return;
                }
            }

            float lambda;

            if (followingScript)
            {
                if (helper.steps > 10)
                {
                    followingScript = false;
                    return;
                }
                else
                {
                    // Using the true eigenvalues will in general lead to the fastest convergence
                    // typically takes 1 or 2 steps
                    lambda = eigenvalues[helper.x2];
                }
            }
            else
            {
                // the current eigenvalue isn't working so try something else
                lambda = helper.computeShift();
            }

            // similar transforms
            helper.performImplicitSingleStep(lambda, false);
        }
    }
}