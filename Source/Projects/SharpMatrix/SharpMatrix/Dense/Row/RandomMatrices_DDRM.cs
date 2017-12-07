using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using Randomization;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row
{
    //package org.ejml.dense.row;

/**
 * Contains a list of functions for creating random row real matrices and vectors with different structures.
 *
 * @author Peter Abeles
 */
    public class RandomMatrices_DDRM
    {

        /**
         * <p>
         * Creates a randomly generated set of orthonormal vectors.  At most it can generate the same
         * number of vectors as the dimension of the vectors.
         * </p>
         *
         * <p>
         * This is done by creating random vectors then ensuring that they are orthogonal
         * to all the ones previously created with reflectors.
         * </p>
         *
         * <p>
         * NOTE: This employs a brute force O(N<sup>3</sup>) algorithm.
         * </p>
         *
         * @param dimen dimension of the space which the vectors will span.
         * @param numVectors How many vectors it should generate.
         * @param rand Used to create random vectors.
         * @return Array of N random orthogonal vectors of unit length.
         */
        // is there a faster algorithm out there? This one is a bit sluggish
        public static DMatrixRMaj[] span(int dimen, int numVectors, IMersenneTwister rand)
        {
            if (dimen < numVectors)
                throw new ArgumentException("The number of vectors must be less than or equal to the dimension");

            DMatrixRMaj[] u = new DMatrixRMaj[numVectors];

            u[0] = RandomMatrices_DDRM.rectangle(dimen, 1, -1, 1, rand);
            NormOps_DDRM.normalizeF(u[0]);

            for (int i = 1; i < numVectors; i++)
            {
//            Console.WriteLine(" i = "+i);
                DMatrixRMaj a = new DMatrixRMaj(dimen, 1);
                DMatrixRMaj r = null;

                for (int j = 0; j < i; j++)
                {
//                Console.WriteLine("j = "+j);
                    if (j == 0)
                        r = RandomMatrices_DDRM.rectangle(dimen, 1, -1, 1, rand);

                    // find a vector that is normal to vector j
                    // u[i] = (1/2)*(r + Q[j]*r)
                    a.set(r);
                    VectorVectorMult_DDRM.householder(-2.0, u[j], r, a);
                    CommonOps_DDRM.add(r, a, a);
                    CommonOps_DDRM.scale(0.5, a);

//                UtilEjml.print(a);

                    DMatrixRMaj t = a;
                    a = r;
                    r = t;

                    // normalize it so it doesn't get too small
                    double val = NormOps_DDRM.normF(r);
                    if (val == 0 || double.IsNaN(val) || double.IsInfinity(val))
                        throw new InvalidOperationException("Failed sanity check");
                    CommonOps_DDRM.divide(r, val);
                }

                u[i] = r;
            }

            return u;
        }

        /**
         * Creates a random vector that is inside the specified span.
         *
         * @param span The span the random vector belongs in.
         * @param rand RNG
         * @return A random vector within the specified span.
         */
        public static DMatrixRMaj insideSpan(DMatrixRMaj[] span, double min, double max, IMersenneTwister rand)
        {
            DMatrixRMaj A = new DMatrixRMaj(span.Length, 1);

            DMatrixRMaj B = new DMatrixRMaj(span[0].getNumElements(), 1);

            for (int i = 0; i < span.Length; i++)
            {
                B.set(span[i]);
                double val = rand.NextDouble() * (max - min) + min;
                CommonOps_DDRM.scale(val, B);

                CommonOps_DDRM.add(A, B, A);

            }

            return A;
        }

        /**
         * <p>
         * Creates a random orthogonal or isometric matrix, depending on the number of rows and columns.
         * The number of rows must be more than or equal to the number of columns.
         * </p>
         *
         * @param numRows Number of rows in the generated matrix.
         * @param numCols Number of columns in the generated matrix.
         * @param rand Random number generator used to create matrices.
         * @return A new isometric matrix.
         */
        public static DMatrixRMaj orthogonal(int numRows, int numCols, IMersenneTwister rand)
        {
            if (numRows < numCols)
            {
                throw new ArgumentException("The number of rows must be more than or equal to the number of columns");
            }

            DMatrixRMaj[] u = span(numRows, numCols, rand);

            DMatrixRMaj ret = new DMatrixRMaj(numRows, numCols);
            for (int i = 0; i < numCols; i++)
            {
                SubmatrixOps_DDRM.setSubMatrix(u[i], ret, 0, 0, 0, i, numRows, 1);
            }

            return ret;
        }

        /**
         * Creates a random diagonal matrix where the diagonal elements are selected from a uniform
         * distribution that goes from min to max.
         *
         * @param N Dimension of the matrix.
         * @param min Minimum value of a diagonal element.
         * @param max Maximum value of a diagonal element.
         * @param rand Random number generator.
         * @return A random diagonal matrix.
         */
        public static DMatrixRMaj diagonal(int N, double min, double max, IMersenneTwister rand)
        {
            return diagonal(N, N, min, max, rand);
        }

        /**
         * Creates a random matrix where all elements are zero but diagonal elements.  Diagonal elements
         * randomly drawn from a uniform distribution from min to max, inclusive.
         *
         * @param numRows Number of rows in the returned matrix..
         * @param numCols Number of columns in the returned matrix.
         * @param min Minimum value of a diagonal element.
         * @param max Maximum value of a diagonal element.
         * @param rand Random number generator.
         * @return A random diagonal matrix.
         */
        public static DMatrixRMaj diagonal(int numRows, int numCols, double min, double max, IMersenneTwister rand)
        {
            if (max < min)
                throw new ArgumentException("The max must be >= the min");

            DMatrixRMaj ret = new DMatrixRMaj(numRows, numCols);

            int N = Math.Min(numRows, numCols);

            double r = max - min;

            for (int i = 0; i < N; i++)
            {
                ret.set(i, i, rand.NextDouble() * r + min);
            }

            return ret;
        }

        /**
         * <p>
         * Creates a random matrix which will have the provided singular values.  The length of sv
         * is assumed to be the rank of the matrix.  This can be useful for testing purposes when one
         * needs to ensure that a matrix is not singular but randomly generated.
         * </p>
         * 
         * @param numRows Number of rows in generated matrix.
         * @param numCols NUmber of columns in generated matrix.
         * @param rand Random number generator.
         * @param sv Singular values of the matrix.
         * @return A new matrix with the specified singular values.
         */
        public static DMatrixRMaj singleValues(int numRows, int numCols,
            IMersenneTwister rand, double[] sv)
        {
            DMatrixRMaj U = RandomMatrices_DDRM.orthogonal(numRows, numRows, rand);
            DMatrixRMaj V = RandomMatrices_DDRM.orthogonal(numCols, numCols, rand);

            DMatrixRMaj S = new DMatrixRMaj(numRows, numCols);

            int min = Math.Min(numRows, numCols);
            min = Math.Min(min, sv.Length);

            for (int i = 0; i < min; i++)
            {
                S.set(i, i, sv[i]);
            }

            DMatrixRMaj tmp = new DMatrixRMaj(numRows, numCols);
            CommonOps_DDRM.mult(U, S, tmp);
            CommonOps_DDRM.multTransB(tmp, V, S);

            return S;
        }

        /**
         * Creates a new random symmetric matrix that will have the specified real eigenvalues.
         *
         * @param num Dimension of the resulting matrix.
         * @param rand Random number generator.
         * @param eigenvalues Set of real eigenvalues that the matrix will have.
         * @return A random matrix with the specified eigenvalues.
         */
        public static DMatrixRMaj symmetricWithEigenvalues(int num, IMersenneTwister rand, double[] eigenvalues)
        {
            DMatrixRMaj V = RandomMatrices_DDRM.orthogonal(num, num, rand);
            DMatrixRMaj D = CommonOps_DDRM.diag(eigenvalues);

            DMatrixRMaj temp = new DMatrixRMaj(num, num);

            CommonOps_DDRM.mult(V, D, temp);
            CommonOps_DDRM.multTransB(temp, V, D);

            return D;
        }

        /**
         * Returns a matrix where all the elements are selected independently from
         * a uniform distribution between 0 and 1 inclusive.
         *
         * @param numRow Number of rows in the new matrix.
         * @param numCol Number of columns in the new matrix.
         * @param rand Random number generator used to fill the matrix.
         * @return The randomly generated matrix.
         */
        public static DMatrixRMaj rectangle(int numRow, int numCol, IMersenneTwister rand)
        {
            DMatrixRMaj mat = new DMatrixRMaj(numRow, numCol);

            fillUniform(mat, 0, 1, rand);

            return mat;
        }

        /**
         * Returns new bool matrix with true or false values selected with equal probability.
         *
         * @param numRow Number of rows in the new matrix.
         * @param numCol Number of columns in the new matrix.
         * @param rand Random number generator used to fill the matrix.
         * @return The randomly generated matrix.
         */
        public static BMatrixRMaj randomBinary(int numRow, int numCol, IMersenneTwister rand)
        {
            BMatrixRMaj mat = new BMatrixRMaj(numRow, numCol);

            setRandomB(mat, rand);

            return mat;
        }

        /**
         * <p>
         * Adds random values to each element in the matrix from an uniform distribution.<br>
         * <br>
         * a<sub>ij</sub> = a<sub>ij</sub> + U(min,max)<br>
         * </p>
         *
         * @param A The matrix who is to be randomized. Modified
         * @param min The minimum value each element can be.
         * @param max The maximum value each element can be..
         * @param rand Random number generator used to fill the matrix.
         */
        public static void addUniform(DMatrixRMaj A, double min, double max, IMersenneTwister rand)
        {
            double[] d = A.getData();
            int size = A.getNumElements();

            double r = max - min;

            for (int i = 0; i < size; i++)
            {
                d[i] += r * rand.NextDouble() + min;
            }
        }

        /**
         * <p>
         * Returns a matrix where all the elements are selected independently from
         * a uniform distribution between 'min' and 'max' inclusive.
         * </p>
         *
         * @param numRow Number of rows in the new matrix.
         * @param numCol Number of columns in the new matrix.
         * @param min The minimum value each element can be.
         * @param max The maximum value each element can be.
         * @param rand Random number generator used to fill the matrix.
         * @return The randomly generated matrix.
         */
        public static DMatrixRMaj rectangle(int numRow, int numCol, double min, double max, IMersenneTwister rand)
        {
            DMatrixRMaj mat = new DMatrixRMaj(numRow, numCol);

            fillUniform(mat, min, max, rand);

            return mat;
        }

        /**
         * <p>
         * Sets each element in the matrix to a value drawn from an uniform distribution from 0 to 1 inclusive.
         * </p>
         *
         * @param mat The matrix who is to be randomized. Modified.
         * @param rand Random number generator used to fill the matrix.
         */
        public static void fillUniform(DMatrixRMaj mat, IMersenneTwister rand)
        {
            fillUniform(mat, 0, 1, rand);
        }

        /**
         * <p>
         * Sets each element in the matrix to a value drawn from an uniform distribution from 'min' to 'max' inclusive.
         * </p>
         *
         * @param min The minimum value each element can be.
         * @param max The maximum value each element can be.
         * @param mat The matrix who is to be randomized. Modified.
         * @param rand Random number generator used to fill the matrix.
         */
        public static void fillUniform(DMatrixD1 mat, double min, double max, IMersenneTwister rand)
        {
            double[] d = mat.getData();
            int size = mat.getNumElements();

            double r = max - min;

            for (int i = 0; i < size; i++)
            {
                d[i] = r * rand.NextDouble() + min;
            }
        }

        /**
         * <p>
         * Sets each element in the bool matrix to true or false with equal probability
         * </p>
         *
         * @param mat The matrix who is to be randomized. Modified.
         * @param rand Random number generator used to fill the matrix.
         */
        public static void setRandomB(BMatrixRMaj mat, IMersenneTwister rand)
        {
            bool[] d = mat.data;
            int size = mat.getNumElements();


            for (int i = 0; i < size; i++)
            {
                d[i] = rand.NextBoolean();
            }
        }


        /**
         * <p>
         * Sets each element in the matrix to a value drawn from an Gaussian distribution with the specified mean and
         * standard deviation
         * </p>
         *
         *
         * @param numRow Number of rows in the new matrix.
         * @param numCol Number of columns in the new matrix.
         * @param mean Mean value in the distribution
         * @param stdev Standard deviation in the distribution
         * @param rand Random number generator used to fill the matrix.
         */
        public static DMatrixRMaj rectangleGaussian(int numRow, int numCol, double mean, double stdev, IMersenneTwister rand)
        {
            DMatrixRMaj m = new DMatrixRMaj(numRow, numCol);
            fillGaussian(m, mean, stdev, rand);
            return m;
        }

        /**
         * <p>
         * Sets each element in the matrix to a value drawn from an Gaussian distribution with the specified mean and
         * standard deviation
         * </p>
         *
         * @param mat The matrix who is to be randomized. Modified.
         * @param mean Mean value in the distribution
         * @param stdev Standard deviation in the distribution
         * @param rand Random number generator used to fill the matrix.
         */
        public static void fillGaussian(DMatrixD1 mat, double mean, double stdev, IMersenneTwister rand)
        {
            double[] d = mat.getData();
            int size = mat.getNumElements();

            for (int i = 0; i < size; i++)
            {
                d[i] = mean + stdev * (double) rand.NextGaussian();
            }
        }

        /**
         * Creates a random symmetric positive definite matrix.
         *
         * @param width The width of the square matrix it returns.
         * @param rand Random number generator used to make the matrix.
         * @return The random symmetric  positive definite matrix.
         */
        public static DMatrixRMaj symmetricPosDef(int width, IMersenneTwister rand)
        {
            // This is not formally proven to work.  It just seems to work.
            DMatrixRMaj a = new DMatrixRMaj(width, 1);
            DMatrixRMaj b = new DMatrixRMaj(width, width);

            for (int i = 0; i < width; i++)
            {
                a.set(i, 0, rand.NextDouble());
            }

            CommonOps_DDRM.multTransB(a, a, b);

            for (int i = 0; i < width; i++)
            {
                b.add(i, i, 1);
            }

            return b;
        }

        /**
         * Creates a random symmetric matrix whose values are selected from an uniform distribution
         * from min to max, inclusive.
         *
         * @param length Width and height of the matrix.
         * @param min Minimum value an element can have.
         * @param max Maximum value an element can have.
         * @param rand Random number generator.
         * @return A symmetric matrix.
         */
        public static DMatrixRMaj symmetric(int length, double min, double max, IMersenneTwister rand)
        {
            DMatrixRMaj A = new DMatrixRMaj(length, length);

            symmetric(A, min, max, rand);

            return A;
        }

        /**
         * Sets the provided square matrix to be a random symmetric matrix whose values are selected from an uniform distribution
         * from min to max, inclusive.
         *
         * @param A The matrix that is to be modified.  Must be square.  Modified.
         * @param min Minimum value an element can have.
         * @param max Maximum value an element can have.
         * @param rand Random number generator.
         */
        public static void symmetric(DMatrixRMaj A, double min, double max, IMersenneTwister rand)
        {
            if (A.numRows != A.numCols)
                throw new ArgumentException("A must be a square matrix");

            double range = max - min;

            int length = A.numRows;

            for (int i = 0; i < length; i++)
            {
                for (int j = i; j < length; j++)
                {
                    double val = rand.NextDouble() * range + min;
                    A.set(i, j, val);
                    A.set(j, i, val);
                }
            }
        }

        /**
         * Creates an upper triangular matrix whose values are selected from a uniform distribution.  If hessenberg
         * is greater than zero then a hessenberg matrix of the specified degree is created instead.
         *
         * @param dimen Number of rows and columns in the matrix..
         * @param hessenberg 0 for triangular matrix and &gt; 0 for hessenberg matrix.
         * @param min minimum value an element can be.
         * @param max maximum value an element can be.
         * @param rand random number generator used.
         * @return The randomly generated matrix.
         */
        public static DMatrixRMaj triangularUpper(int dimen, int hessenberg, double min, double max, IMersenneTwister rand)
        {
            if (hessenberg < 0)
                throw new InvalidOperationException("hessenberg must be more than or equal to 0");

            double range = max - min;

            DMatrixRMaj A = new DMatrixRMaj(dimen, dimen);

            for (int i = 0; i < dimen; i++)
            {
                int start = i <= hessenberg ? 0 : i - hessenberg;

                for (int j = start; j < dimen; j++)
                {
                    A.set(i, j, rand.NextDouble() * range + min);
                }

            }

            return A;
        }
    }
}