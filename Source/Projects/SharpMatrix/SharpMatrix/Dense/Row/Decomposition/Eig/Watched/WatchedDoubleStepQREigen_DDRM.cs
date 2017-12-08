using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row.Decomposition.QR;
using Randomization;

namespace SharpMatrix.Dense.Row.Decomposition.Eig.Watched
{
    //package org.ejml.dense.row.decomposition.eig.watched;

/**
 * <p>
 * The double step implicit Eigenvalue decomposition algorithm is fairly complicated and needs to be designed so that
 * it can handle several special cases.  To aid in development and debugging this class was created.  It allows
 * individual components to be tested and to print out their results.  This shows how each step is performed.
 * </p>
 *
 * <p>
 * Do not use this class to compute the eigenvalues since it is much slower than a non-debug implementation.
 * </p>
 *
 */
// TODO make rank1UpdateMultR efficient once again by setting 0 to x1 and creating a new one that updates all the rows
// TODO option of modifying original matrix
    public class WatchedDoubleStepQREigen_DDRM
    {

        private IMersenneTwister rand = new MersenneTwisterFast(0x2342);

        private int N;

        internal DMatrixRMaj A;
        private DMatrixRMaj u;
        private double gamma;

        private DMatrixRMaj _temp;

        // how many steps did it take to find the eigenvalue
        int[] numStepsFind;

        internal int steps;
        internal Complex_F64[] eigenvalues;
        internal int numEigen;

        // computes eigenvalues for 2 by 2 submatrices
        private EigenvalueSmall_F64 valueSmall = new EigenvalueSmall_F64();

        private double[] temp = new double[9];

        private bool printHumps = false;
        bool checkHessenberg = false;
        private bool checkOrthogonal = false;
        private bool checkUncountable = false;

        private bool useStandardEq = false;
        private bool useCareful2x2 = true;

        private bool normalize = true;

        internal int lastExceptional;
        int numExceptional;
        internal int exceptionalThreshold = 20;
        internal int maxIterations; // ctor => exceptionalThreshold*20;

        public bool createR = true;

        public DMatrixRMaj Q;

        public WatchedDoubleStepQREigen_DDRM()
        {
            maxIterations = exceptionalThreshold * 20;
        }

        public void incrementSteps()
        {
            steps++;
        }

        public void setQ(DMatrixRMaj Q)
        {
            this.Q = Q;
        }

        private void addEigenvalue(double v)
        {
            numStepsFind[numEigen] = steps;
            eigenvalues[numEigen].set(v, 0);
            numEigen++;
            steps = 0;
            lastExceptional = 0;
        }

        private void addEigenvalue(double v, double i)
        {
            numStepsFind[numEigen] = steps;
            eigenvalues[numEigen].set(v, i);
            numEigen++;
            steps = 0;
            lastExceptional = 0;
        }

        public void setChecks(bool hessenberg, bool orthogonal, bool uncountable)
        {
            this.checkHessenberg = hessenberg;
            this.checkOrthogonal = orthogonal;
            this.checkUncountable = uncountable;
        }


        public bool isZero(int x1, int x2)
        {
            // this provides a relative threshold for when dealing with very large/small numbers
            double target = Math.Abs(A.get(x1, x2));

            double above = Math.Abs(A.get(x1 - 1, x2));

            // according to Matrix Computations page 352 this is what is done in Eispack
            double right = Math.Abs(A.get(x1, x2 + 1));
            return target <= 0.5 * UtilEjml.EPS * (above + right);
        }

        public void setup(DMatrixRMaj A)
        {
            if (A.numRows != A.numCols)
                throw new InvalidOperationException("Must be square");

            if (N != A.numRows)
            {
                N = A.numRows;

                this.A = (DMatrixRMaj) A.copy();
                u = new DMatrixRMaj(A.numRows, 1);

                _temp = new DMatrixRMaj(A.numRows, 1);
                numStepsFind = new int[A.numRows];
            }
            else
            {
                this.A.set(A);
                UtilEjml.memset(numStepsFind, 0, numStepsFind.Length);
            }

            // zero all the off numbers that should be zero for a hessenberg matrix
            for (int i = 2; i < N; i++)
            {
                for (int j = 0; j < i - 1; j++)
                {
                    this.A.set(i, j, 0);
                }
            }

            eigenvalues = new Complex_F64[A.numRows];
            for (int i = 0; i < eigenvalues.Length; i++)
            {
                eigenvalues[i] = new Complex_F64();
            }

            numEigen = 0;
            lastExceptional = 0;
            numExceptional = 0;
            steps = 0;
        }

        /**
         * Perform a shift in a random direction that is of the same magnitude as the elements in the matrix.
         */
        public void exceptionalShift(int x1, int x2)
        {
            if (printHumps)
                Console.WriteLine("Performing exceptional implicit double step");

            // perform a random shift that is of the same magnitude as the matrix
            double val = Math.Abs(A.get(x2, x2));

            if (val == 0)
                val = 1;

            numExceptional++;
            // the closer the value is the better it handles identical eigenvalues cases
            double p = 1.0 - Math.Pow(0.1, numExceptional);
            val *= p + 2.0 * (1.0 - p) * (rand.NextDouble() - 0.5);

            if (rand.NextBoolean())
                val = -val;

            performImplicitSingleStep(x1, x2, val);

            lastExceptional = steps;
        }

        /**
         * Performs an implicit double step using the values contained in the lower right hand side
         * of the submatrix for the estimated eigenvector values.
         * @param x1
         * @param x2
         */
        public void implicitDoubleStep(int x1, int x2)
        {
            if (printHumps)
                Console.WriteLine("Performing implicit double step");

            // compute the wilkinson shift
            double z11 = A.get(x2 - 1, x2 - 1);
            double z12 = A.get(x2 - 1, x2);
            double z21 = A.get(x2, x2 - 1);
            double z22 = A.get(x2, x2);

            double a11 = A.get(x1, x1);
            double a21 = A.get(x1 + 1, x1);
            double a12 = A.get(x1, x1 + 1);
            double a22 = A.get(x1 + 1, x1 + 1);
            double a32 = A.get(x1 + 2, x1 + 1);

            if (normalize)
            {
                temp[0] = a11;
                temp[1] = a21;
                temp[2] = a12;
                temp[3] = a22;
                temp[4] = a32;
                temp[5] = z11;
                temp[6] = z22;
                temp[7] = z12;
                temp[8] = z21;

                double max = Math.Abs(temp[0]);
                for (int j = 1; j < temp.Length; j++)
                {
                    if (Math.Abs(temp[j]) > max)
                        max = Math.Abs(temp[j]);
                }
                a11 /= max;
                a21 /= max;
                a12 /= max;
                a22 /= max;
                a32 /= max;
                z11 /= max;
                z22 /= max;
                z12 /= max;
                z21 /= max;
            }

            // these equations are derived when the eigenvalues are extracted from the lower right
            // 2 by 2 matrix.  See page 388 of Fundamentals of Matrix Computations 2nd ed for details.
            double b11, b21, b31;
            if (useStandardEq)
            {
                b11 = ((a11 - z11) * (a11 - z22) - z21 * z12) / a21 + a12;
                b21 = a11 + a22 - z11 - z22;
                b31 = a32;
            }
            else
            {
                // this is different from the version in the book and seems in my testing to be more resilient to
                // over flow issues
                b11 = ((a11 - z11) * (a11 - z22) - z21 * z12) + a12 * a21;
                b21 = (a11 + a22 - z11 - z22) * a21;
                b31 = a32 * a21;
            }

            performImplicitDoubleStep(x1, x2, b11, b21, b31);
        }

        /**
         * Performs an implicit double step given the set of two imaginary eigenvalues provided.
         * Since one eigenvalue is the complex conjugate of the other only one set of real and imaginary
         * numbers is needed.
         *
         * @param x1 upper index of submatrix.
         * @param x2 lower index of submatrix.
         * @param real Real component of each of the eigenvalues.
         * @param img Imaginary component of one of the eigenvalues.
         */
        public void performImplicitDoubleStep(int x1, int x2, double real, double img)
        {
            double a11 = A.get(x1, x1);
            double a21 = A.get(x1 + 1, x1);
            double a12 = A.get(x1, x1 + 1);
            double a22 = A.get(x1 + 1, x1 + 1);
            double a32 = A.get(x1 + 2, x1 + 1);

            double p_plus_t = 2.0 * real;
            double p_times_t = real * real + img * img;

            double b11, b21, b31;
            if (useStandardEq)
            {
                b11 = (a11 * a11 - p_plus_t * a11 + p_times_t) / a21 + a12;
                b21 = a11 + a22 - p_plus_t;
                b31 = a32;
            }
            else
            {
                // this is different from the version in the book and seems in my testing to be more resilient to
                // over flow issues
                b11 = (a11 * a11 - p_plus_t * a11 + p_times_t) + a12 * a21;
                b21 = (a11 + a22 - p_plus_t) * a21;
                b31 = a32 * a21;
            }

            performImplicitDoubleStep(x1, x2, b11, b21, b31);
        }

        private void performImplicitDoubleStep(int x1, int x2,
            double b11, double b21, double b31)
        {
            if (!bulgeDoubleStepQn(x1, b11, b21, b31, 0, false))
                return;

            // get rid of the bump
            if (Q != null)
            {
                QrHelperFunctions_DDRM.rank1UpdateMultR(Q, u.data, gamma, 0, x1, x1 + 3, _temp.data);
                if (checkOrthogonal && !MatrixFeatures_DDRM.isOrthogonal(Q, UtilEjml.TEST_F64))
                {
                    u.print();

                    Q.print();
                    throw new InvalidOperationException("Bad");
                }
            }

            if (printHumps)
            {
                Console.WriteLine("Applied first Q matrix, it should be humped now. A = ");
                A.print("%12.3e");
                Console.WriteLine("Pushing the hump off the matrix.");
            }

            // perform double steps
            for (int i = x1; i < x2 - 2; i++)
            {
                if (bulgeDoubleStepQn(i) && Q != null)
                {
                    QrHelperFunctions_DDRM.rank1UpdateMultR(Q, u.data, gamma, 0, i + 1, i + 4, _temp.data);
                    if (checkOrthogonal && !MatrixFeatures_DDRM.isOrthogonal(Q, UtilEjml.TEST_F64))
                        throw new InvalidOperationException("Bad");
                }

                if (printHumps)
                {
                    Console.WriteLine("i = " + i + " A = ");
                    A.print("%12.3e");
                }
            }
            if (printHumps)
                Console.WriteLine("removing last bump");
            // the last one has to be a single step
            if (x2 - 2 >= 0 && bulgeSingleStepQn(x2 - 2) && Q != null)
            {
                QrHelperFunctions_DDRM.rank1UpdateMultR(Q, u.data, gamma, 0, x2 - 1, x2 + 1, _temp.data);
                if (checkOrthogonal && !MatrixFeatures_DDRM.isOrthogonal(Q, UtilEjml.TEST_F64))
                    throw new InvalidOperationException("Bad");

            }
            if (printHumps)
            {
                Console.WriteLine(" A = ");
                A.print("%12.3e");
            }
//        A.print("%12.3e");

            if (checkHessenberg && !MatrixFeatures_DDRM.isUpperTriangle(A, 1, UtilEjml.TEST_F64))
            {
                A.print("%12.3e");
                throw new InvalidOperationException("Bad matrix");
            }
        }


        public void performImplicitSingleStep(int x1, int x2, double eigenvalue)
        {
            if (!createBulgeSingleStep(x1, eigenvalue))
                return;

            // get rid of the bump
            if (Q != null)
            {
                QrHelperFunctions_DDRM.rank1UpdateMultR(Q, u.data, gamma, 0, x1, x1 + 2, _temp.data);
                if (checkOrthogonal && !MatrixFeatures_DDRM.isOrthogonal(Q, UtilEjml.TEST_F64))
                    throw new InvalidOperationException("Bad");
            }

            if (printHumps)
            {
                Console.WriteLine("Applied first Q matrix, it should be humped now. A = ");
                A.print("%12.3e");
                Console.WriteLine("Pushing the hump off the matrix.");
            }

            // perform simple steps
            for (int i = x1; i < x2 - 1; i++)
            {
                if (bulgeSingleStepQn(i) && Q != null)
                {
                    QrHelperFunctions_DDRM.rank1UpdateMultR(Q, u.data, gamma, 0, i + 1, i + 3, _temp.data);
                    if (checkOrthogonal && !MatrixFeatures_DDRM.isOrthogonal(Q, UtilEjml.TESTP_F64))
                        throw new InvalidOperationException("Bad");
                }

                if (printHumps)
                {
                    Console.WriteLine("i = " + i + " A = ");
                    A.print("%12.3e");
                }
            }

            if (checkHessenberg && !MatrixFeatures_DDRM.isUpperTriangle(A, 1, UtilEjml.TESTP_F64))
            {
                A.print("%12.3e");
                throw new InvalidOperationException("Bad matrix");
            }
        }

        public bool createBulgeSingleStep(int x1, double eigenvalue)
        {

            double b11 = A.get(x1, x1) - eigenvalue;
            double b21 = A.get(x1 + 1, x1);

            double threshold = Math.Abs(A.get(x1, x1)) * UtilEjml.EPS;

            return bulgeSingleStepQn(x1, b11, b21, threshold, false);
        }

        public bool bulgeDoubleStepQn(int i)
        {
            double a11 = A.get(i + 1, i);
            double a21 = A.get(i + 2, i);
            double a31 = A.get(i + 3, i);

            double threshold = Math.Abs(A.get(i, i)) * UtilEjml.EPS;

            return bulgeDoubleStepQn(i + 1, a11, a21, a31, threshold, true);
        }

        public bool bulgeDoubleStepQn(int i,
            double a11, double a21, double a31,
            double threshold, bool set)
        {
            double max;
            if (normalize)
            {
                double absA11 = Math.Abs(a11);
                double absA21 = Math.Abs(a21);
                double absA31 = Math.Abs(a31);

                max = absA11 > absA21 ? absA11 : absA21;
                if (absA31 > max) max = absA31;

//           if( max <= Math.Abs(A.get(i,i))*UtilEjml.EPS ) {
                if (max <= threshold)
                {
                    if (set)
                    {
                        A.set(i, i - 1, 0);
                        A.set(i + 1, i - 1, 0);
                        A.set(i + 2, i - 1, 0);
                    }
                    return false;
                }

                a11 /= max;
                a21 /= max;
                a31 /= max;
            }
            else
            {
                max = 1;
            }

            // compute the reflector using the b's above

            double tau = Math.Sqrt(a11 * a11 + a21 * a21 + a31 * a31);
            if (a11 < 0) tau = -tau;

            double div = a11 + tau;

            u.set(i, 0, 1);
            u.set(i + 1, 0, a21 / div);
            u.set(i + 2, 0, a31 / div);

            gamma = div / tau;

            // compute A_1 = Q_1^T * A * Q_1

            // apply Q*A  - just do the 3 rows
            QrHelperFunctions_DDRM.rank1UpdateMultR(A, u.data, gamma, 0, i, i + 3, _temp.data);

            if (set)
            {
                A.set(i, i - 1, -max * tau);
                A.set(i + 1, i - 1, 0);
                A.set(i + 2, i - 1, 0);
            }

            if (printHumps)
            {
                Console.WriteLine("  After Q.   A =");
                A.print();
            }

            // apply A*Q - just the three things
            QrHelperFunctions_DDRM.rank1UpdateMultL(A, u.data, gamma, 0, i, i + 3);

//        Console.WriteLine("  after Q*A*Q ");
//        A.print();

            if (checkUncountable && MatrixFeatures_DDRM.hasUncountable(A))
            {
                throw new InvalidOperationException("bad matrix");
            }

            return true;
        }

        public bool bulgeSingleStepQn(int i)
        {
            double a11 = A.get(i + 1, i);
            double a21 = A.get(i + 2, i);

            double threshold = Math.Abs(A.get(i, i)) * UtilEjml.EPS;

            return bulgeSingleStepQn(i + 1, a11, a21, threshold, true);
        }

        public bool bulgeSingleStepQn(int i,
            double a11, double a21,
            double threshold, bool set)
        {

            double max;
            if (normalize)
            {
                max = Math.Abs(a11);
                if (max < Math.Abs(a21)) max = Math.Abs(a21);

//            if( max <= Math.Abs(A.get(i,i))*UtilEjml.EPS ) {
                if (max <= threshold)
                {
//                Console.WriteLine("i = "+i);
//                A.print();
                    if (set)
                    {
                        A.set(i, i - 1, 0);
                        A.set(i + 1, i - 1, 0);
                    }
                    return false;
                }

                a11 /= max;
                a21 /= max;
            }
            else
            {
                max = 1;
            }

            // compute the reflector using the b's above

            double tau = Math.Sqrt(a11 * a11 + a21 * a21);
            if (a11 < 0) tau = -tau;

            double div = a11 + tau;

            u.set(i, 0, 1);
            u.set(i + 1, 0, a21 / div);

            gamma = div / tau;

            // compute A_1 = Q_1^T * A * Q_1

            // apply Q*A  - just do the 3 rows
            QrHelperFunctions_DDRM.rank1UpdateMultR(A, u.data, gamma, 0, i, i + 2, _temp.data);

            if (set)
            {
                A.set(i, i - 1, -max * tau);
                A.set(i + 1, i - 1, 0);
            }

            // apply A*Q - just the three things
            QrHelperFunctions_DDRM.rank1UpdateMultL(A, u.data, gamma, 0, i, i + 2);

            if (checkUncountable && MatrixFeatures_DDRM.hasUncountable(A))
            {
                throw new InvalidOperationException("bad matrix");
            }

            return true;
        }

        public void eigen2by2_scale(double a11, double a12, double a21, double a22)
        {
            double abs11 = Math.Abs(a11);
            double abs22 = Math.Abs(a22);
            double abs12 = Math.Abs(a12);
            double abs21 = Math.Abs(a21);

            double max = abs11 > abs22 ? abs11 : abs22;
            if (max < abs12) max = abs12;
            if (max < abs21) max = abs21;

            if (max == 0)
            {
                valueSmall.value0.real = 0;
                valueSmall.value0.imaginary = 0;
                valueSmall.value1.real = 0;
                valueSmall.value1.imaginary = 0;
            }
            else
            {
                a12 /= max;
                a21 /= max;
                a11 /= max;
                a22 /= max;

                if (useCareful2x2)
                {
                    valueSmall.value2x2(a11, a12, a21, a22);
                }
                else
                {
                    valueSmall.value2x2_fast(a11, a12, a21, a22);
                }
                valueSmall.value0.real *= max;
                valueSmall.value0.imaginary *= max;
                valueSmall.value1.real *= max;
                valueSmall.value1.imaginary *= max;

            }

//        System.out.printf("eigen (%6.3f , %6.3f) (%6.3f , %6.3f)\n",p0_real,p0_img,p1_real,p1_img);
        }

        public int getNumberOfEigenvalues()
        {
            return numEigen;
        }

        public Complex_F64[] getEigenvalues()
        {
            return eigenvalues;
        }

        public void addComputedEigen2x2(int x1, int x2)
        {
            eigen2by2_scale(A.get(x1, x1), A.get(x1, x2), A.get(x2, x1), A.get(x2, x2));

            if (checkUncountable &&
                (double.IsNaN(valueSmall.value0.real) || double.IsNaN(valueSmall.value1.real)))
            {
                throw new InvalidOperationException("Uncountable");
            }

            addEigenvalue(valueSmall.value0.real, valueSmall.value0.imaginary);
            addEigenvalue(valueSmall.value1.real, valueSmall.value1.imaginary);
        }

        public bool isReal2x2(int x1, int x2)
        {
            eigen2by2_scale(A.get(x1, x1), A.get(x1, x2), A.get(x2, x1), A.get(x2, x2));

            return valueSmall.value0.isReal();
        }

        public void addEigenAt(int x1)
        {
            addEigenvalue(A.get(x1, x1));
        }

        public void printSteps()
        {
            for (int i = 0; i < N; i++)
            {
                Console.WriteLine("Step[" + i + "] = " + numStepsFind[i]);
            }
        }
    }
}