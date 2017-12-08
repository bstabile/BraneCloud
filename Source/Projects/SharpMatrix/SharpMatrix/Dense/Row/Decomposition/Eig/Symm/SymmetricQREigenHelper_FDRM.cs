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
    public class SymmetricQREigenHelper_FDRM
    {

        // used in exceptional shifts
        protected IMersenneTwister rand = new MersenneTwisterFast(0x34671e);

        // how many steps has it taken
        internal int steps;

        // how many exception shifts has it performed
        protected int numExceptional;

        // the step number of the last exception shift
        internal int lastExceptional;

        // used to compute eigenvalues directly
        protected EigenvalueSmall_F32 eigenSmall = new EigenvalueSmall_F32();

        // orthogonal matrix used in similar transform.  optional
        protected FMatrixRMaj Q;

        // size of the matrix being processed
        internal int N;

        // diagonal elements in the matrix
        internal float[] diag;

        // the off diagonal elements
        protected float[] off;

        // which submatrix is being processed
        internal int x1;

        internal int x2;

        // where splits are performed
        internal int[] splits;

        internal int numSplits;

        // current value of the bulge
        private float bulge;

        // local helper functions
        private float c, s, c2, s2, cs;

        public SymmetricQREigenHelper_FDRM()
        {
            splits = new int[1];
        }

        public void printMatrix()
        {
            Console.Write("Off Diag[ ");
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

        public void setQ(FMatrixRMaj q)
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
        public void init(float[] diag,
            float[] off,
            int numCols)
        {
            reset(numCols);

            this.diag = diag;
            this.off = off;
        }

        /**
         * Exchanges the internal array of the diagonal elements for the provided one.
         */
        public float[] swapDiag(float[] diag)
        {
            float[] ret = this.diag;
            this.diag = diag;

            return ret;
        }

        /**
         * Exchanges the internal array of the off diagonal elements for the provided one.
         */
        public float[] swapOff(float[] off)
        {
            float[] ret = this.off;
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

        public float[] copyDiag(float[] ret)
        {
            if (ret == null || ret.Length < N)
            {
                ret = new float[N];
            }

            Array.Copy(diag, 0, ret, 0, N);

            return ret;
        }

        public float[] copyOff(float[] ret)
        {
            if (ret == null || ret.Length < N - 1)
            {
                ret = new float[N - 1];
            }

            Array.Copy(off, 0, ret, 0, N - 1);

            return ret;
        }

        public float[] copyEigenvalues(float[] ret)
        {
            if (ret == null || ret.Length < N)
            {
                ret = new float[N];
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
            float bottom = Math.Abs(diag[index]) + Math.Abs(diag[index + 1]);

            return(Math.Abs(off[index]) <= bottom * UtilEjml.F_EPS);
        }

        internal void performImplicitSingleStep(float lambda, bool byAngle)
        {
            if (x2 - x1 == 1)
            {
                createBulge2by2(x1, lambda, byAngle);
            }
            else
            {
                createBulge(x1, lambda, byAngle);

                for (int i = x1; i < x2 - 2 && bulge != 0.0f; i++)
                {
                    removeBulge(i);

                }
                if (bulge != 0.0f)
                    removeBulgeEnd(x2 - 2);
            }
        }

        protected void updateQ(int m, int n, float c, float s)
        {
            int rowA = m * N;
            int rowB = n * N;

//        for( int i = 0; i < N; i++ ) {
//            float a = Q.data[rowA+i];
//            float b = Q.data[rowB+i];
//            Q.data[rowA+i] = c*a + s*b;
//            Q.data[rowB+i] = -s*a + c*b;
//        }
            int endA = rowA + N;
            while (rowA < endA)
            {
                float a = Q.data[rowA];
                float b = Q.data[rowB];
                Q.data[rowA++] = c * a + s * b;
                Q.data[rowB++] = -s * a + c * b;
            }
        }

        /**
         * Performs a similar transform on A-pI
         */
        protected void createBulge(int x1, float p, bool byAngle)
        {
            float a11 = diag[x1];
            float a22 = diag[x1 + 1];
            float a12 = off[x1];
            float a23 = off[x1 + 1];

            if (byAngle)
            {
                c = (float) Math.Cos(p);
                s = (float) Math.Sin(p);

                c2 = c * c;
                s2 = s * s;
                cs = c * s;
            }
            else
            {
                computeRotation(a11 - p, a12);
            }

            // multiply the rotator on the top left.
            diag[x1] = c2 * a11 + 2.0f * cs * a12 + s2 * a22;
            diag[x1 + 1] = c2 * a22 - 2.0f * cs * a12 + s2 * a11;
            off[x1] = a12 * (c2 - s2) + cs * (a22 - a11);
            off[x1 + 1] = c * a23;
            bulge = s * a23;

            if (Q != null)
                updateQ(x1, x1 + 1, c, s);
        }

        protected void createBulge2by2(int x1, float p, bool byAngle)
        {
            float a11 = diag[x1];
            float a22 = diag[x1 + 1];
            float a12 = off[x1];

            if (byAngle)
            {
                c = (float) Math.Cos(p);
                s = (float) Math.Sin(p);

                c2 = c * c;
                s2 = s * s;
                cs = c * s;
            }
            else
            {
                computeRotation(a11 - p, a12);
            }

            // multiply the rotator on the top left.
            diag[x1] = c2 * a11 + 2.0f * cs * a12 + s2 * a22;
            diag[x1 + 1] = c2 * a22 - 2.0f * cs * a12 + s2 * a11;
            off[x1] = a12 * (c2 - s2) + cs * (a22 - a11);

            if (Q != null)
                updateQ(x1, x1 + 1, c, s);
        }

        /**
         * Computes the rotation and stores it in (c,s)
         */
        private void computeRotation(float run, float rise)
        {
//        float alpha = (float)Math.Sqrt(run*run + rise*rise);
//        c = run/alpha;
//        s = rise/alpha;

            if (Math.Abs(rise) > Math.Abs(run))
            {
                float k = run / rise;

                float bottom = 1.0f + k * k;
                float bottom_sq = (float) Math.Sqrt(bottom);

                s2 = 1.0f / bottom;
                c2 = k * k / bottom;
                cs = k / bottom;
                s = 1.0f / bottom_sq;
                c = k / bottom_sq;
            }
            else
            {
                float t = rise / run;

                float bottom = 1.0f + t * t;
                float bottom_sq = (float) Math.Sqrt(bottom);

                c2 = 1.0f / bottom;
                s2 = t * t / bottom;
                cs = t / bottom;
                c = 1.0f / bottom_sq;
                s = t / bottom_sq;
            }
        }

        protected void removeBulge(int x1)
        {
            float a22 = diag[x1 + 1];
            float a33 = diag[x1 + 2];
            float a12 = off[x1];
            float a23 = off[x1 + 1];
            float a34 = off[x1 + 2];

            computeRotation(a12, bulge);

            // multiply the rotator on the top left.
            diag[x1 + 1] = c2 * a22 + 2.0f * cs * a23 + s2 * a33;
            diag[x1 + 2] = c2 * a33 - 2.0f * cs * a23 + s2 * a22;
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
            float a22 = diag[x1 + 1];
            float a12 = off[x1];
            float a23 = off[x1 + 1];
            float a33 = diag[x1 + 2];

            computeRotation(a12, bulge);

            // multiply the rotator on the top left.
            diag[x1 + 1] = c2 * a22 + 2.0f * cs * a23 + s2 * a33;
            diag[x1 + 2] = c2 * a33 - 2.0f * cs * a23 + s2 * a22;
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
            float a = diag[x1];
            float b = off[x1];
            float c = diag[x1 + 1];

            // normalize to reduce overflow
            float absA = Math.Abs(a);
            float absB = Math.Abs(b);
            float absC = Math.Abs(c);

            float scale = absA > absB ? absA : absB;
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
            float mag = 0.05f * numExceptional;
            if (mag > 1.0f) mag = 1.0f;

            float theta = 2.0f * (rand.NextFloat() - 0.5f) * mag;
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

        public float computeShift()
        {
            if (x2 - x1 >= 1)
                return computeWilkinsonShift();
            else
                return diag[x2];
        }

        public float computeWilkinsonShift()
        {
            float a = diag[x2 - 1];
            float b = off[x2 - 1];
            float c = diag[x2];

            // normalize to reduce overflow
            float absA = Math.Abs(a);
            float absB = Math.Abs(b);
            float absC = Math.Abs(c);

            float scale = absA > absB ? absA : absB;
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
            float diff0 = Math.Abs(eigenSmall.value0.real - c);
            float diff1 = Math.Abs(eigenSmall.value1.real - c);

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