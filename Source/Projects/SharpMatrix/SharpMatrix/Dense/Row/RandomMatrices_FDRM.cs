using System;
using SharpMatrix.Data;
using Randomization;

namespace SharpMatrix.Dense.Row
{
    //package org.ejml.dense.row;

/**
 * Contains a list of functions for creating random row real matrices and vectors with different structures.
 *
 * @author Peter Abeles
 */
    public class RandomMatrices_FDRM
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
        public static FMatrixRMaj[] span(int dimen, int numVectors, IMersenneTwister rand)
        {
            if (dimen < numVectors)
                throw new ArgumentException("The number of vectors must be less than or equal to the dimension");

            FMatrixRMaj[] u = new FMatrixRMaj[numVectors];

            u[0] = RandomMatrices_FDRM.rectangle(dimen, 1, -1, 1, rand);
            NormOps_FDRM.normalizeF(u[0]);

            for (int i = 1; i < numVectors; i++)
            {
//            Console.WriteLine(" i = "+i);
                FMatrixRMaj a = new FMatrixRMaj(dimen, 1);
                FMatrixRMaj r = null;

                for (int j = 0; j < i; j++)
                {
//                Console.WriteLine("j = "+j);
                    if (j == 0)
                        r = RandomMatrices_FDRM.rectangle(dimen, 1, -1, 1, rand);

                    // find a vector that is normal to vector j
                    // u[i] = (1/2)*(r + Q[j]*r)
                    a.set(r);
                    VectorVectorMult_FDRM.householder(-2.0f, u[j], r, a);
                    CommonOps_FDRM.add(r, a, a);
                    CommonOps_FDRM.scale(0.5f, a);

//                UtilEjml.print(a);

                    FMatrixRMaj t = a;
                    a = r;
                    r = t;

                    // normalize it so it doesn't get too small
                    float val = NormOps_FDRM.normF(r);
                    if (val == 0 || float.IsNaN(val) || float.IsInfinity(val))
                        throw new InvalidOperationException("Failed sanity check");
                    CommonOps_FDRM.divide(r, val);
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
        public static FMatrixRMaj insideSpan(FMatrixRMaj[] span, float min, float max, IMersenneTwister rand)
        {
            FMatrixRMaj A = new FMatrixRMaj(span.Length, 1);

            FMatrixRMaj B = new FMatrixRMaj(span[0].getNumElements(), 1);

            for (int i = 0; i < span.Length; i++)
            {
                B.set(span[i]);
                float val = rand.NextFloat() * (max - min) + min;
                CommonOps_FDRM.scale(val, B);

                CommonOps_FDRM.add(A, B, A);

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
        public static FMatrixRMaj orthogonal(int numRows, int numCols, IMersenneTwister rand)
        {
            if (numRows < numCols)
            {
                throw new ArgumentException("The number of rows must be more than or equal to the number of columns");
            }

            FMatrixRMaj[] u = span(numRows, numCols, rand);

            FMatrixRMaj ret = new FMatrixRMaj(numRows, numCols);
            for (int i = 0; i < numCols; i++)
            {
                SubmatrixOps_FDRM.setSubMatrix(u[i], ret, 0, 0, 0, i, numRows, 1);
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
        public static FMatrixRMaj diagonal(int N, float min, float max, IMersenneTwister rand)
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
        public static FMatrixRMaj diagonal(int numRows, int numCols, float min, float max, IMersenneTwister rand)
        {
            if (max < min)
                throw new ArgumentException("The max must be >= the min");

            FMatrixRMaj ret = new FMatrixRMaj(numRows, numCols);

            int N = Math.Min(numRows, numCols);

            float r = max - min;

            for (int i = 0; i < N; i++)
            {
                ret.set(i, i, rand.NextFloat() * r + min);
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
        public static FMatrixRMaj singleValues(int numRows, int numCols,
            IMersenneTwister rand, float[] sv)
        {
            FMatrixRMaj U = RandomMatrices_FDRM.orthogonal(numRows, numRows, rand);
            FMatrixRMaj V = RandomMatrices_FDRM.orthogonal(numCols, numCols, rand);

            FMatrixRMaj S = new FMatrixRMaj(numRows, numCols);

            int min = Math.Min(numRows, numCols);
            min = Math.Min(min, sv.Length);

            for (int i = 0; i < min; i++)
            {
                S.set(i, i, sv[i]);
            }

            FMatrixRMaj tmp = new FMatrixRMaj(numRows, numCols);
            CommonOps_FDRM.mult(U, S, tmp);
            CommonOps_FDRM.multTransB(tmp, V, S);

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
        public static FMatrixRMaj symmetricWithEigenvalues(int num, IMersenneTwister rand, float[] eigenvalues)
        {
            FMatrixRMaj V = RandomMatrices_FDRM.orthogonal(num, num, rand);
            FMatrixRMaj D = CommonOps_FDRM.diag(eigenvalues);

            FMatrixRMaj temp = new FMatrixRMaj(num, num);

            CommonOps_FDRM.mult(V, D, temp);
            CommonOps_FDRM.multTransB(temp, V, D);

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
        public static FMatrixRMaj rectangle(int numRow, int numCol, IMersenneTwister rand)
        {
            FMatrixRMaj mat = new FMatrixRMaj(numRow, numCol);

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
        public static void addUniform(FMatrixRMaj A, float min, float max, IMersenneTwister rand)
        {
            float[] d = A.getData();
            int size = A.getNumElements();

            float r = max - min;

            for (int i = 0; i < size; i++)
            {
                d[i] += r * rand.NextFloat() + min;
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
        public static FMatrixRMaj rectangle(int numRow, int numCol, float min, float max, IMersenneTwister rand)
        {
            FMatrixRMaj mat = new FMatrixRMaj(numRow, numCol);

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
        public static void fillUniform(FMatrixRMaj mat, IMersenneTwister rand)
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
        public static void fillUniform(FMatrixD1 mat, float min, float max, IMersenneTwister rand)
        {
            float[] d = mat.getData();
            int size = mat.getNumElements();

            float r = max - min;

            for (int i = 0; i < size; i++)
            {
                d[i] = r * rand.NextFloat() + min;
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
        public static FMatrixRMaj rectangleGaussian(int numRow, int numCol, float mean, float stdev, IMersenneTwister rand)
        {
            FMatrixRMaj m = new FMatrixRMaj(numRow, numCol);
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
        public static void fillGaussian(FMatrixD1 mat, float mean, float stdev, IMersenneTwister rand)
        {
            float[] d = mat.getData();
            int size = mat.getNumElements();

            for (int i = 0; i < size; i++)
            {
                d[i] = mean + stdev * (float) rand.NextGaussian();
            }
        }

        /**
         * Creates a random symmetric positive definite matrix.
         *
         * @param width The width of the square matrix it returns.
         * @param rand Random number generator used to make the matrix.
         * @return The random symmetric  positive definite matrix.
         */
        public static FMatrixRMaj symmetricPosDef(int width, IMersenneTwister rand)
        {
            // This is not formally proven to work.  It just seems to work.
            FMatrixRMaj a = new FMatrixRMaj(width, 1);
            FMatrixRMaj b = new FMatrixRMaj(width, width);

            for (int i = 0; i < width; i++)
            {
                a.set(i, 0, rand.NextFloat());
            }

            CommonOps_FDRM.multTransB(a, a, b);

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
        public static FMatrixRMaj symmetric(int length, float min, float max, IMersenneTwister rand)
        {
            FMatrixRMaj A = new FMatrixRMaj(length, length);

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
        public static void symmetric(FMatrixRMaj A, float min, float max, IMersenneTwister rand)
        {
            if (A.numRows != A.numCols)
                throw new ArgumentException("A must be a square matrix");

            float range = max - min;

            int length = A.numRows;

            for (int i = 0; i < length; i++)
            {
                for (int j = i; j < length; j++)
                {
                    float val = rand.NextFloat() * range + min;
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
        public static FMatrixRMaj triangularUpper(int dimen, int hessenberg, float min, float max,
            IMersenneTwister rand)
        {
            if (hessenberg < 0)
                throw new InvalidOperationException("hessenberg must be more than or equal to 0");

            float range = max - min;

            FMatrixRMaj A = new FMatrixRMaj(dimen, dimen);

            for (int i = 0; i < dimen; i++)
            {
                int start = i <= hessenberg ? 0 : i - hessenberg;

                for (int j = start; j < dimen; j++)
                {
                    A.set(i, j, rand.NextFloat() * range + min);
                }

            }

            return A;
        }
    }
}