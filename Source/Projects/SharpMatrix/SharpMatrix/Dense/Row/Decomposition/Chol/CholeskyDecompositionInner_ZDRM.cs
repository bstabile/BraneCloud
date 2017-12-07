using System;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.Chol
{
    //package org.ejml.dense.row.decompose.chol;

/**
 * <p>
 * This implementation of a Cholesky decomposition using the inner-product form.
 * </p>
 *
 * @author Peter Abeles
 */
    public class CholeskyDecompositionInner_ZDRM : CholeskyDecompositionCommon_ZDRM
    {

        // tolerance for testing to see if diagonal elements are real
        double tolerance = UtilEjml.EPS;

        public CholeskyDecompositionInner_ZDRM()
            : base(true)
        {
        }

        public CholeskyDecompositionInner_ZDRM(bool lower)
            : base(lower)
        {
        }

        public void setTolerance(double tolerance)
        {
            this.tolerance = tolerance;
        }

        //@Override
        protected override bool decomposeLower()
        {
            if (n == 0)
                throw new ArgumentException("Cholesky is undefined for 0 by 0 matrix");

            double real_el_ii = 0;

            int stride = n * 2;
            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    double realSum = t[i * stride + j * 2];
                    double imagSum = t[i * stride + j * 2 + 1];

                    if (i == j)
                    {
                        // its easy to prove that for the cholesky decomposition to be valid the original
                        // diagonal elements must be real
                        if (Math.Abs(imagSum) > tolerance * Math.Abs(realSum))
                            return false;

                        // This takes advantage of the fact that when you multiply a complex number by
                        // its conjigate the result is a real number
                        int end = i * stride + i * 2;
                        for (int index = i * stride; index < end;)
                        {
                            double real = t[index++];
                            double imag = t[index++];

                            realSum -= real * real + imag * imag;
                        }

                        if (realSum <= 0)
                        {
                            return false;
                        }

                        real_el_ii = Math.Sqrt(realSum);
                        t[i * stride + i * 2] = real_el_ii;
                        t[i * stride + i * 2 + 1] = 0;
                    }
                    else
                    {
                        int iEl = i * stride; // row i is inside the lower triangle
                        int jEl = j * stride; // row j conjugate transposed upper triangle
                        int end = iEl + i * 2;
                        // k = 0:i-1
                        for (; iEl < end;)
                        {
//                    sum -= el[i*n+k]*el[j*n+k];
                            double realI = t[iEl++];
                            double imagI = t[iEl++];

                            double realJ = t[jEl++];
                            double imagJ = t[jEl++];

                            // multiply by the complex conjugate of I since the triangle being stored
                            // is the conjugate of L
                            realSum -= realI * realJ + imagI * imagJ;
                            imagSum -= realI * imagJ - realJ * imagI;
                        }

                        // divide the sum by the diagonal element, which is always real
                        // Note that it is storing the conjugate of L
                        t[j * stride + i * 2] = realSum / real_el_ii;
                        t[j * stride + i * 2 + 1] = imagSum / real_el_ii;
                    }
                }
            }
            // Make it L instead of the conjugate of L
            for (int i = 1; i < n; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    t[i * stride + j * 2 + 1] = -t[i * stride + j * 2 + 1];
                }
            }

            return true;
        }


        //@Override
        protected override bool decomposeUpper()
        {
            // See code comments in lower for more details on the algorithm

            if (n == 0)
                throw new ArgumentException("Cholesky is undefined for 0 by 0 matrix");

            double real_el_ii = 0;

            int stride = n * 2;

            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    double realSum = t[i * stride + j * 2];
                    double imagSum = t[i * stride + j * 2 + 1];

                    if (i == j)
                    {
                        if (Math.Abs(imagSum) > tolerance * Math.Abs(realSum))
                            return false;

                        for (int k = 0; k < i; k++)
                        {
                            double real = t[k * stride + i * 2];
                            double imag = t[k * stride + i * 2 + 1];

                            realSum -= real * real + imag * imag;
                        }

                        if (realSum <= 0)
                        {
                            return false;
                        }

                        real_el_ii = Math.Sqrt(realSum);
                        t[i * stride + i * 2] = real_el_ii;
                        t[i * stride + i * 2 + 1] = 0;
                    }
                    else
                    {
                        for (int k = 0; k < i; k++)
                        {
                            double realI = t[k * stride + i * 2];
                            double imagI = t[k * stride + i * 2 + 1];

                            double realJ = t[k * stride + j * 2];
                            double imagJ = t[k * stride + j * 2 + 1];

                            realSum -= realI * realJ + imagI * imagJ;
                            imagSum -= realI * imagJ - realJ * imagI;
                        }

                        t[i * stride + j * 2] = realSum / real_el_ii;
                        t[i * stride + j * 2 + 1] = imagSum / real_el_ii;
                    }
                }
            }

            return true;
        }
    }
}