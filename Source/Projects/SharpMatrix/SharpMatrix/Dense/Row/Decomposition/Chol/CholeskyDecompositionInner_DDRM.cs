using System;

namespace SharpMatrix.Dense.Row.Decomposition.Chol
{
    //package org.ejml.dense.row.decomposition.chol;

    /**
     * <p>
     * This implementation of a Cholesky decomposition using the inner-product form.
     * For large matrices a block implementation is better.  On larger matrices the lower triangular
     * decomposition is significantly faster.  This is faster on smaller matrices than {@link CholeskyDecompositionBlock_DDRM}
     * but much slower on larger matrices.
     * </p>
     *
     * @author Peter Abeles
     */
    public class CholeskyDecompositionInner_DDRM : CholeskyDecompositionCommon_DDRM
    {

        public CholeskyDecompositionInner_DDRM()
            : base(true)
        {
        }

        public CholeskyDecompositionInner_DDRM(bool lower)
            : base(lower)
        {
        }

        protected override bool decomposeLower()
        {
            double el_ii;
            double div_el_ii = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    double sum = t[i * n + j];

                    int iEl = i * n;
                    int jEl = j * n;
                    int end = iEl + i;
                    // k = 0:i-1
                    for (; iEl < end; iEl++, jEl++)
                    {
//                    sum -= el[i*n+k]*el[j*n+k];
                        sum -= t[iEl] * t[jEl];
                    }

                    if (i == j)
                    {
                        // is it positive-definite?
                        if (sum <= 0.0)
                            return false;

                        el_ii = Math.Sqrt(sum);
                        t[i * n + i] = el_ii;
                        div_el_ii = 1.0 / el_ii;
                    }
                    else
                    {
                        t[j * n + i] = sum * div_el_ii;
                    }
                }
            }

            // zero the top right corner.
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    t[i * n + j] = 0.0;
                }
            }

            return true;
        }


        protected override bool decomposeUpper()
        {
            double el_ii;
            double div_el_ii = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    double sum = t[i * n + j];

                    for (int k = 0; k < i; k++)
                    {
                        sum -= t[k * n + i] * t[k * n + j];
                    }

                    if (i == j)
                    {
                        // is it positive-definite?
                        if (sum <= 0.0)
                            return false;

                        // I suspect that the sqrt is slowing this down relative to MTJ
                        el_ii = Math.Sqrt(sum);
                        t[i * n + i] = el_ii;
                        div_el_ii = 1.0 / el_ii;
                    }
                    else
                    {
                        t[i * n + j] = sum * div_el_ii;
                    }
                }
            }
            // zero the lower left corner.
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    t[i * n + j] = 0.0;
                }
            }

            return true;
        }
    }
}