using System;
using SharpMatrix.Data;
using Randomization;

namespace SharpMatrix.Dense.Row.Decomposition.Eig.Symm
{
    //package org.ejml.dense.row.decomposition.eig.symm;

/**
 * A helper class for the symmetric matrix implicit QR algorithm for eigenvalue decomposition.
 * Performs most of the basic operations needed to extract eigenvalues and eigenvectors.
 *
 * @author Peter Abeles
 */
    public class SymmetricQREigenHelper_DDRM
    {

        // used in exceptional shifts
        protected IMersenneTwister rand = new MersenneTwisterFast(0x34671e);

        // how many steps has it taken
        internal int steps;

        // how many exception shifts has it performed
        internal int numExceptional;

        // the step number of the last exception shift
        internal int lastExceptional;

        // used to compute eigenvalues directly
        internal EigenvalueSmall_F64 eigenSmall = new EigenvalueSmall_F64();

        // orthogonal matrix used in similar transform.  optional
        internal DMatrixRMaj Q;

        // size of the matrix being processed
        internal int N;

        // diagonal elements in the matrix
        internal double[] diag;

        // the off diagonal elements
        internal double[] off;

        // which submatrix is being processed
        internal int x1;

        internal int x2;

        // where splits are performed
        internal int[] splits;

        internal int numSplits;

        // current value of the bulge
        private double bulge;

        // local helper functions
        private double c, s, c2, s2, cs;

        public SymmetricQREigenHelper_DDRM()
        {
            splits = new int[1];
        }

        public void printMatrix()
        {
            Console.WriteLine("Off Diag[ ");
            for (int j = 0; j < N - 1; j++)
            {
                Console.Write("{0:F2,5} ", off[j]);
            }
            Console.WriteLine();
            Console.Write("    Diag[ ");
            for (int j = 0; j < N; j++)
            {
                Console.Write("{0:F2,5} ", diag[j]);
            }
            Console.WriteLine();
        }

        public void setQ(DMatrixRMaj q)
        {
            Q = q;
        }

        public void incrementSteps()
        {
            steps++;
        }

        /**
         * Sets up and declares internal data structures.
         *
         * @param diag Diagonal elements from tridiagonal matrix. Modified.
         * @param off Off diagonal elements from tridiagonal matrix. Modified.
         * @param numCols number of columns (and rows) in the matrix.
         */
        public void init(double[] diag,
            double[] off,
            int numCols)
        {
            reset(numCols);

            this.diag = diag;
            this.off = off;
        }

        /**
         * Exchanges the internal array of the diagonal elements for the provided one.
         */
        public double[] swapDiag(double[] diag)
        {
            double[] ret = this.diag;
            this.diag = diag;

            return ret;
        }

        /**
         * Exchanges the internal array of the off diagonal elements for the provided one.
         */
        public double[] swapOff(double[] off)
        {
            double[] ret = this.off;
            this.off = off;

            return ret;
        }

        /**
         * Sets the size of the matrix being decomposed, declares new memory if needed,
         * and sets all helper functions to their initial value.
         */
        public void reset(int N)
        {
            this.N = N;

            this.diag = null;
            this.off = null;

            if (splits.Length < N)
            {
                splits = new int[N];
            }

            numSplits = 0;

            x1 = 0;
            x2 = N - 1;

            steps = numExceptional = lastExceptional = 0;

            this.Q = null;
        }

        public double[] copyDiag(double[]ret)
        {
            if (ret == null || ret.Length < N)
            {
                ret = new double[N];
            }

            Array.Copy(diag, 0, ret, 0, N);

            return ret;
        }

        public double[] copyOff(double[]ret)
        {
            if (ret == null || ret.Length < N - 1)
            {
                ret = new double[N - 1];
            }

            Array.Copy(off, 0, ret, 0, N - 1);

            return ret;
        }

        public double[] copyEigenvalues(double[]ret)
        {
            if (ret == null || ret.Length < N)
            {
                ret = new double[N];
            }

            Array.Copy(diag, 0, ret, 0, N);

            return ret;
        }

        /**
         * Sets which submatrix is being processed.
         * @param x1 Lower bound, inclusive.
         * @param x2 Upper bound, inclusive.
         */
        public void setSubmatrix(int x1, int x2)
        {
            this.x1 = x1;
            this.x2 = x2;
        }

        /**
         * Checks to see if the specified off diagonal element is zero using a relative metric.
         */
        internal bool isZero(int index)
        {
            double bottom = Math.Abs(diag[index]) + Math.Abs(diag[index + 1]);

            return(Math.Abs(off[index]) <= bottom * UtilEjml.EPS);
        }

        internal void performImplicitSingleStep(double lambda, bool byAngle)
        {
            if (x2 - x1 == 1)
            {
                createBulge2by2(x1, lambda, byAngle);
            }
            else
            {
                createBulge(x1, lambda, byAngle);

                for (int i = x1; i < x2 - 2 && bulge != 0.0; i++)
                {
                    removeBulge(i);

                }
                if (bulge != 0.0)
                    removeBulgeEnd(x2 - 2);
            }
        }

        protected void updateQ(int m, int n, double c, double s)
        {
            int rowA = m * N;
            int rowB = n * N;

//        for( int i = 0; i < N; i++ ) {
//            double a = Q.data[rowA+i];
//            double b = Q.data[rowB+i];
//            Q.data[rowA+i] = c*a + s*b;
//            Q.data[rowB+i] = -s*a + c*b;
//        }
            int endA = rowA + N;
            while (rowA < endA)
            {
                double a = Q.data[rowA];
                double b = Q.data[rowB];
                Q.data[rowA++] = c * a + s * b;
                Q.data[rowB++] = -s * a + c * b;
            }
        }

        /**
         * Performs a similar transform on A-pI
         */
        protected void createBulge(int x1, double p, bool byAngle)
        {
            double a11 = diag[x1];
            double a22 = diag[x1 + 1];
            double a12 = off[x1];
            double a23 = off[x1 + 1];

            if (byAngle)
            {
                c = Math.Cos(p);
                s = Math.Sin(p);

                c2 = c * c;
                s2 = s * s;
                cs = c * s;
            }
            else
            {
                computeRotation(a11 - p, a12);
            }

            // multiply the rotator on the top left.
            diag[x1] = c2 * a11 + 2.0 * cs * a12 + s2 * a22;
            diag[x1 + 1] = c2 * a22 - 2.0 * cs * a12 + s2 * a11;
            off[x1] = a12 * (c2 - s2) + cs * (a22 - a11);
            off[x1 + 1] = c * a23;
            bulge = s * a23;

            if (Q != null)
                updateQ(x1, x1 + 1, c, s);
        }

        protected void createBulge2by2(int x1, double p, bool byAngle)
        {
            double a11 = diag[x1];
            double a22 = diag[x1 + 1];
            double a12 = off[x1];

            if (byAngle)
            {
                c = Math.Cos(p);
                s = Math.Sin(p);

                c2 = c * c;
                s2 = s * s;
                cs = c * s;
            }
            else
            {
                computeRotation(a11 - p, a12);
            }

            // multiply the rotator on the top left.
            diag[x1] = c2 * a11 + 2.0 * cs * a12 + s2 * a22;
            diag[x1 + 1] = c2 * a22 - 2.0 * cs * a12 + s2 * a11;
            off[x1] = a12 * (c2 - s2) + cs * (a22 - a11);

            if (Q != null)
                updateQ(x1, x1 + 1, c, s);
        }

        /**
         * Computes the rotation and stores it in (c,s)
         */
        private void computeRotation(double run, double rise)
        {
//        double alpha = Math.Sqrt(run*run + rise*rise);
//        c = run/alpha;
//        s = rise/alpha;

            if (Math.Abs(rise) > Math.Abs(run))
            {
                double k = run / rise;

                double bottom = 1.0 + k * k;
                double bottom_sq = Math.Sqrt(bottom);

                s2 = 1.0 / bottom;
                c2 = k * k / bottom;
                cs = k / bottom;
                s = 1.0 / bottom_sq;
                c = k / bottom_sq;
            }
            else
            {
                double t = rise / run;

                double bottom = 1.0 + t * t;
                double bottom_sq = Math.Sqrt(bottom);

                c2 = 1.0 / bottom;
                s2 = t * t / bottom;
                cs = t / bottom;
                c = 1.0 / bottom_sq;
                s = t / bottom_sq;
            }
        }

        protected void removeBulge(int x1)
        {
            double a22 = diag[x1 + 1];
            double a33 = diag[x1 + 2];
            double a12 = off[x1];
            double a23 = off[x1 + 1];
            double a34 = off[x1 + 2];

            computeRotation(a12, bulge);

            // multiply the rotator on the top left.
            diag[x1 + 1] = c2 * a22 + 2.0 * cs * a23 + s2 * a33;
            diag[x1 + 2] = c2 * a33 - 2.0 * cs * a23 + s2 * a22;
            off[x1] = c * a12 + s * bulge;
            off[x1 + 1] = a23 * (c2 - s2) + cs * (a33 - a22);
            off[x1 + 2] = c * a34;
            bulge = s * a34;

            if (Q != null)
                updateQ(x1 + 1, x1 + 2, c, s);
        }

        /**
         * Rotator to remove the bulge
         */
        protected void removeBulgeEnd(int x1)
        {
            double a22 = diag[x1 + 1];
            double a12 = off[x1];
            double a23 = off[x1 + 1];
            double a33 = diag[x1 + 2];

            computeRotation(a12, bulge);

            // multiply the rotator on the top left.
            diag[x1 + 1] = c2 * a22 + 2.0 * cs * a23 + s2 * a33;
            diag[x1 + 2] = c2 * a33 - 2.0 * cs * a23 + s2 * a22;
            off[x1] = c * a12 + s * bulge;
            off[x1 + 1] = a23 * (c2 - s2) + cs * (a33 - a22);

            if (Q != null)
                updateQ(x1 + 1, x1 + 2, c, s);
        }

        /**
         * Computes the eigenvalue of the 2 by 2 matrix.
         */
        internal void eigenvalue2by2(int x1)
        {
            double a = diag[x1];
            double b = off[x1];
            double c = diag[x1 + 1];

            // normalize to reduce overflow
            double absA = Math.Abs(a);
            double absB = Math.Abs(b);
            double absC = Math.Abs(c);

            double scale = absA > absB ? absA : absB;
            if (absC > scale) scale = absC;

            // see if it is a pathological case.  the diagonal must already be zero
            // and the eigenvalues are all zero.  so just return
            if (scale == 0)
            {
                off[x1] = 0;
                diag[x1] = 0;
                diag[x1 + 1] = 0;
                return;
            }

            a /= scale;
            b /= scale;
            c /= scale;

            eigenSmall.symm2x2_fast(a, b, c);

            off[x1] = 0;
            diag[x1] = scale * eigenSmall.value0.real;
            diag[x1 + 1] = scale * eigenSmall.value1.real;
        }

        /**
         * Perform a shift in a random direction that is of the same magnitude as the elements in the matrix.
         */
        public void exceptionalShift()
        {
            // rotating by a random angle handles at least one case using a random lambda
            // does not handle well:
            // - two identical eigenvalues are next to each other and a very small diagonal element
            numExceptional++;
            double mag = 0.05 * numExceptional;
            if (mag > 1.0) mag = 1.0;

            double theta = 2.0 * (rand.NextDouble() - 0.5) * mag;
            performImplicitSingleStep(theta, true);

            lastExceptional = steps;
        }

        /**
         * Tells it to process the submatrix at the next split.  Should be called after the
         * current submatrix has been processed.
         */
        public bool nextSplit()
        {
            if (numSplits == 0)
                return false;
            x2 = splits[--numSplits];
            if (numSplits > 0)
                x1 = splits[numSplits - 1] + 1;
            else
                x1 = 0;

            return true;
        }

        public double computeShift()
        {
            if (x2 - x1 >= 1)
                return computeWilkinsonShift();
            else
                return diag[x2];
        }

        public double computeWilkinsonShift()
        {
            double a = diag[x2 - 1];
            double b = off[x2 - 1];
            double c = diag[x2];

            // normalize to reduce overflow
            double absA = Math.Abs(a);
            double absB = Math.Abs(b);
            double absC = Math.Abs(c);

            double scale = absA > absB ? absA : absB;
            if (absC > scale) scale = absC;

            if (scale == 0)
            {
                throw new InvalidOperationException("this should never happen");
            }

            a /= scale;
            b /= scale;
            c /= scale;

            // TODO see 385

            eigenSmall.symm2x2_fast(a, b, c);

            // return the eigenvalue closest to c
            double diff0 = Math.Abs(eigenSmall.value0.real - c);
            double diff1 = Math.Abs(eigenSmall.value1.real - c);

            if (diff0 < diff1)
                return scale * eigenSmall.value0.real;
            else
                return scale * eigenSmall.value1.real;
        }

        public int getMatrixSize()
        {
            return N;
        }

        public void resetSteps()
        {
            steps = 0;
            lastExceptional = 0;
        }
    }
}