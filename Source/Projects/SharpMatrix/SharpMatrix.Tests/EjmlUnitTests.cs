using System;
using SharpMatrix.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpMatrix
{
    //package org.ejml;

/**
 * Contains various functions related to unit testing matrix operations.
 *
 * @author Peter Abeles
 */
    public class EjmlUnitTests
    {

        /**
         * Checks to see if every element in A is countable.  A doesn't have any element with
         * a value of NaN or infinite.
         *
         * @param A Matrix
         */
        public static void assertCountable(DMatrix A)
        {
            for (int i = 0; i < A.getNumRows(); i++)
            {
                for (int j = 0; j < A.getNumCols(); j++)
                {
                    assertTrue(!double.IsNaN(A.get(i, j)), "NaN found at " + i + " " + j);
                    assertTrue(!double.IsInfinity(A.get(i, j)), "Infinite found at " + i + " " + j);
                }
            }
        }

        /**
         * <p>
         * Checks to see if A and B have the same shape.
         * </p>
         *
         * @param A Matrix
         * @param B Matrix
         */
        public static void assertShape(Matrix A, Matrix B)
        {
            assertTrue(A.getNumRows() == B.getNumRows(), "Number of rows do not match");
            assertTrue(A.getNumCols() == B.getNumCols(), "Number of columns do not match");
        }

        /**
         * <p>
         * Checks to see if the matrix has the specified number of rows and columns.
         * </p>
         *
         * @param A Matrix
         * @param numRows expected number of rows in the matrix
         * @param numCols expected number of columns in the matrix
         */
        public static void assertShape(Matrix A, int numRows, int numCols)
        {
            assertTrue(A.getNumRows() == numRows, "Unexpected number of rows.");
            assertTrue(A.getNumCols() == numCols, "Unexpected number of columns.");
        }

        /**
         * <p>
         * Checks to see if each element in the matrix is within tolerance of each other:
         * </p>
         *
         * <p>
         * The two matrices are identical with in tolerance if:<br>
         * |a<sub>ij</sub> - b<sub>ij</sub>| &le; tol
         * </p>
         *
         * <p>
         * In addition if an element is NaN or infinite in one matrix it must be the same in the other.
         * </p>
         *
         * @param A Matrix A
         * @param B Matrix B
         * @param tol Tolerance
         */
        public static void assertEqualsUncountable(DMatrix A, DMatrix B, double tol)
        {
            assertShape(A, B);

            for (int i = 0; i < A.getNumRows(); i++)
            {
                for (int j = 0; j < A.getNumCols(); j++)
                {
                    double valA = A.get(i, j);
                    double valB = B.get(i, j);

                    if (double.IsNaN(valA))
                    {
                        assertTrue(double.IsNaN(valB), "At (" + i + "," + j + ") A = " + valA + " B = " + valB);
                    }
                    else if (double.IsInfinity(valA))
                    {
                        assertTrue(double.IsInfinity(valB), "At (" + i + "," + j + ") A = " + valA + " B = " + valB);
                    }
                    else
                    {
                        double diff = Math.Abs(valA - valB);
                        assertTrue(diff <= tol, "At (" + i + "," + j + ") A = " + valA + " B = " + valB);
                    }
                }
            }
        }

        public static void assertEquals(Matrix A, Matrix B)
        {
            if (A is DMatrix)
            {
                assertEquals((DMatrix) A, (DMatrix) B, UtilEjml.TEST_F64);
            }
            else
            {
                assertEquals((FMatrix) A, (FMatrix) B, UtilEjml.TEST_F32);
            }
        }

        /**
         * <p>
         * Checks to see if each element in the matrices are within tolerance of each other and countable:
         * </p>
         *
         * <p>
         * The two matrices are identical with in tolerance if:<br>
         * |a<sub>ij</sub> - b<sub>ij</sub>| &le; tol
         * </p>
         *
         * <p>
         * The test will fail if any element in either matrix is NaN or infinite.
         * </p>
         *
         * @param A Matrix A
         * @param B Matrix B
         * @param tol Tolerance
         */
        public static void assertEquals(DMatrix A, DMatrix B, double tol)
        {
            assertShape(A, B);

            for (int i = 0; i < A.getNumRows(); i++)
            {
                for (int j = 0; j < A.getNumCols(); j++)
                {
                    double valA = A.get(i, j);
                    double valB = B.get(i, j);

                    assertTrue(!double.IsNaN(valA) && !double.IsNaN(valB),
                        "At (" + i + "," + j + ") A = " + valA + " B = " + valB);
                    assertTrue(!double.IsInfinity(valA) && !double.IsInfinity(valB),
                        "At (" + i + "," + j + ") A = " + valA + " B = " + valB);
                    assertTrue(Math.Abs(valA - valB) <= tol, "At (" + i + "," + j + ") A = " + valA + " B = " + valB);
                }
            }
        }

        /**
         * Assert equals with a relative error
         */
        public static void assertRelativeEquals(DMatrix A, DMatrix B, double tol)
        {
            assertShape(A, B);

            for (int i = 0; i < A.getNumRows(); i++)
            {
                for (int j = 0; j < A.getNumCols(); j++)
                {
                    double valA = A.get(i, j);
                    double valB = B.get(i, j);

                    if ((double.IsNaN(valA) != double.IsNaN(valB)) ||
                        (double.IsInfinity(valA) != double.IsInfinity(valB)))
                    {
                        throw new AssertFailedException("At (" + i + "," + j + ") A = " + valA + " B = " + valB);
                    }
                    double max = Math.Max(Math.Abs(valA), Math.Abs(valB));
                    double error = Math.Abs(valA - valB) / max;
                    if (error > tol)
                    {
                        Console.WriteLine("------------  A  -----------");
                        A.print();
                        Console.WriteLine("\n------------  B  -----------");
                        B.print();
                        throw new AssertFailedException("At (" + i + "," + j + ") A = " + valA + " B = " + valB +
                                                 "   error = " + error);
                    }
                }
            }
        }

        public static void assertEquals(FMatrix A, FMatrix B, float tol)
        {
            assertShape(A, B);

            for (int i = 0; i < A.getNumRows(); i++)
            {
                for (int j = 0; j < A.getNumCols(); j++)
                {
                    float valA = A.get(i, j);
                    float valB = B.get(i, j);

                    assertTrue(!float.IsNaN(valA) && !float.IsNaN(valB),
                        "At (" + i + "," + j + ") A = " + valA + " B = " + valB);
                    assertTrue(!float.IsInfinity(valA) && !float.IsInfinity(valB),
                        "At (" + i + "," + j + ") A = " + valA + " B = " + valB);
                    assertTrue(Math.Abs(valA - valB) <= tol, "At (" + i + "," + j + ") A = " + valA + " B = " + valB);
                }
            }
        }

        public static void assertEquals(Complex_F64 a, Complex_F64 b, double tol)
        {
            assertTrue(!double.IsNaN(a.real) && !double.IsNaN(b.real), "real a = " + a.real + " b = " + b.real);
            assertTrue(!double.IsInfinity(a.real) && !double.IsInfinity(b.real),
                "real a = " + a.real + " b = " + b.real);
            assertTrue(Math.Abs(a.real - b.real) <= tol, "real a = " + a.real + " b = " + b.real);

            assertTrue(!double.IsNaN(a.imaginary) && !double.IsNaN(b.imaginary),
                "imaginary a = " + a.imaginary + " b = " + b.imaginary);
            assertTrue(!double.IsInfinity(a.imaginary) && !double.IsInfinity(b.imaginary),
                "imaginary a = " + a.imaginary + " b = " + b.imaginary);
            assertTrue(Math.Abs(a.imaginary - b.imaginary) <= tol,
                "imaginary a = " + a.imaginary + " b = " + b.imaginary);
        }

        public static void assertEquals(Complex_F32 a, Complex_F32 b, float tol)
        {
            assertTrue(!float.IsNaN(a.real) && !float.IsNaN(b.real), "real a = " + a.real + " b = " + b.real);
            assertTrue(!float.IsInfinity(a.real) && !float.IsInfinity(b.real), "real a = " + a.real + " b = " + b.real);
            assertTrue(Math.Abs(a.real - b.real) <= tol, "real a = " + a.real + " b = " + b.real);

            assertTrue(!float.IsNaN(a.imaginary) && !float.IsNaN(b.imaginary),
                "imaginary a = " + a.imaginary + " b = " + b.imaginary);
            assertTrue(!float.IsInfinity(a.imaginary) && !float.IsInfinity(b.imaginary),
                "imaginary a = " + a.imaginary + " b = " + b.imaginary);
            assertTrue(Math.Abs(a.imaginary - b.imaginary) <= tol,
                "imaginary a = " + a.imaginary + " b = " + b.imaginary);
        }

        public static void assertEquals(ZMatrix A, ZMatrix B, double tol)
        {
            assertShape(A, B);

            Complex_F64 a = new Complex_F64();
            Complex_F64 b = new Complex_F64();

            for (int i = 0; i < A.getNumRows(); i++)
            {
                for (int j = 0; j < A.getNumCols(); j++)
                {
                    A.get(i, j, a);
                    B.get(i, j, b);

                    assertTrue(!double.IsNaN(a.real) && !double.IsNaN(b.real),
                        "Real At (" + i + "," + j + ") A = " + a.real + " B = " + b.real);
                    assertTrue(!double.IsInfinity(a.real) && !double.IsInfinity(b.real),
                        "Real At (" + i + "," + j + ") A = " + a.real + " B = " + b.real);
                    assertTrue(Math.Abs(a.real - b.real) <= tol,
                        "Real At (" + i + "," + j + ") A = " + a.real + " B = " + b.real);

                    assertTrue(!double.IsNaN(a.imaginary) && !double.IsNaN(b.imaginary),
                        "Img At (" + i + "," + j + ") A = " + a.imaginary + " B = " + b.imaginary);
                    assertTrue(!double.IsInfinity(a.imaginary) && !double.IsInfinity(b.imaginary),
                        "Img At (" + i + "," + j + ") A = " + a.imaginary + " B = " + b.imaginary);
                    assertTrue(Math.Abs(a.imaginary - b.imaginary) <= tol,
                        "Img At (" + i + "," + j + ") A = " + a.imaginary + " B = " + b.imaginary);

                }
            }
        }

        public static void assertEquals(CMatrix A, CMatrix B, float tol)
        {
            assertShape(A, B);

            Complex_F32 a = new Complex_F32();
            Complex_F32 b = new Complex_F32();

            for (int i = 0; i < A.getNumRows(); i++)
            {
                for (int j = 0; j < A.getNumCols(); j++)
                {
                    A.get(i, j, a);
                    B.get(i, j, b);

                    assertTrue(!float.IsNaN(a.real) && !float.IsNaN(b.real),
                        "Real At (" + i + "," + j + ") A = " + a.real + " B = " + b.real);
                    assertTrue(!float.IsInfinity(a.real) && !float.IsInfinity(b.real),
                        "Real At (" + i + "," + j + ") A = " + a.real + " B = " + b.real);
                    assertTrue(Math.Abs(a.real - b.real) <= tol,
                        "Real At (" + i + "," + j + ") A = " + a.real + " B = " + b.real);

                    assertTrue(!float.IsNaN(a.imaginary) && !float.IsNaN(b.imaginary),
                        "Img At (" + i + "," + j + ") A = " + a.imaginary + " B = " + b.imaginary);
                    assertTrue(!float.IsInfinity(a.imaginary) && !float.IsInfinity(b.imaginary),
                        "Img At (" + i + "," + j + ") A = " + a.imaginary + " B = " + b.imaginary);
                    assertTrue(Math.Abs(a.imaginary - b.imaginary) <= tol,
                        "Img At (" + i + "," + j + ") A = " + a.imaginary + " B = " + b.imaginary);
                }

            }
        }

        /**
         * <p>
         * Checks to see if the transpose of B is equal to A and countable:
         * </p>
         *
         * <p>
         * |a<sub>ij</sub> - b<sub>ji</sub>| &le; tol
         * </p>
         *
         * <p>
         * The test will fail if any element in either matrix is NaN or infinite.
         * </p>
         *
         * @param A Matrix A
         * @param B Matrix B
         * @param tol Tolerance
         */
        public static void assertEqualsTrans(DMatrix A, DMatrix B, double tol)
        {
            assertShape(A, B.getNumCols(), B.getNumRows());

            for (int i = 0; i < A.getNumRows(); i++)
            {
                for (int j = 0; j < A.getNumCols(); j++)
                {
                    double valA = A.get(i, j);
                    double valB = B.get(j, i);

                    assertTrue(!double.IsNaN(valA) && !double.IsNaN(valB),
                        "A(" + i + "," + j + ") = " + valA + ") B(" + j + "," + i + ") = " + valB);
                    assertTrue(!double.IsInfinity(valA) && !double.IsInfinity(valB),
                        "A(" + i + "," + j + ") = " + valA + ") B(" + j + "," + i + ") = " + valB);
                    assertTrue(Math.Abs(valA - valB) <= tol,
                        "A(" + i + "," + j + ") = " + valA + ") B(" + j + "," + i + ") = " + valB);
                }
            }
        }

        public static void assertEqualsTrans(FMatrix A, FMatrix B, double tol)
        {
            assertShape(A, B.getNumCols(), B.getNumRows());

            for (int i = 0; i < A.getNumRows(); i++)
            {
                for (int j = 0; j < A.getNumCols(); j++)
                {
                    float valA = A.get(i, j);
                    float valB = B.get(j, i);

                    assertTrue(!float.IsNaN(valA) && !float.IsNaN(valB),
                        "A(" + i + "," + j + ") = " + valA + ") B(" + j + "," + i + ") = " + valB);
                    assertTrue(!float.IsInfinity(valA) && !float.IsInfinity(valB),
                        "A(" + i + "," + j + ") = " + valA + ") B(" + j + "," + i + ") = " + valB);
                    assertTrue(Math.Abs(valA - valB) <= tol,
                        "A(" + i + "," + j + ") = " + valA + ") B(" + j + "," + i + ") = " + valB);
                }
            }
        }

        //@SuppressWarnings({"ConstantConditions"})
        private static void assertTrue(bool result, string message)
        {
            // if turned on use asserts
            //assert result : message;

            // otherwise throw an exception
            if (!result) throw new AssertFailedException(message);
        }
    }
}