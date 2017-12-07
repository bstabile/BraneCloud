using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.Chol
{
    //package org.ejml.dense.row.decomposition.chol;

/**
 * A specialized Cholesky decomposition algorithm that is designed to help out
 * {@link CholeskyDecompositionBlock_DDRM} perform its calculations.  While decomposing
 * the matrix it will modify its internal lower triangular matrix and the original
 * that is being modified.
 *
 * @author Peter Abeles
 */
    public class CholeskyBlockHelper_DDRM
    {

        // the decomposed matrix
        private DMatrixRMaj L;

        private double[] el;

        /**
         * Creates a CholeksyDecomposition capable of decomposing a matrix that is
         * n by n, where n is the width.
         *
         * @param widthMax The maximum width of a matrix that can be processed.
         */
        public CholeskyBlockHelper_DDRM(int widthMax)
        {

            this.L = new DMatrixRMaj(widthMax, widthMax);
            this.el = L.data;
        }

        /**
         * Decomposes a submatrix.  The results are written to the submatrix
         * and to its internal matrix L.
         *
         * @param mat A matrix which has a submatrix that needs to be inverted
         * @param indexStart the first index of the submatrix
         * @param n The width of the submatrix that is to be inverted.
         * @return True if it was able to finish the decomposition.
         */
        public bool decompose(DMatrixRMaj mat, int indexStart, int n)
        {
            double[] m = mat.data;

            double el_ii;
            double div_el_ii = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    double sum = m[indexStart + i * mat.numCols + j];

                    int iEl = i * n;
                    int jEl = j * n;
                    int end = iEl + i;
                    // k = 0:i-1
                    for (; iEl < end; iEl++, jEl++)
                    {
//                    sum -= el[i*n+k]*el[j*n+k];
                        sum -= el[iEl] * el[jEl];
                    }

                    if (i == j)
                    {
                        // is it positive-definate?
                        if (sum <= 0.0)
                            return false;

                        el_ii = Math.Sqrt(sum);
                        el[i * n + i] = el_ii;
                        m[indexStart + i * mat.numCols + i] = el_ii;
                        div_el_ii = 1.0 / el_ii;
                    }
                    else
                    {
                        double v = sum * div_el_ii;
                        el[j * n + i] = v;
                        m[indexStart + j * mat.numCols + i] = v;
                    }
                }
            }

            return true;
        }

        /**
         * Returns L matrix from the decomposition.<br>
         * L*L<sup>T</sup>=A
         *
         * @return A lower triangular matrix.
         */
        public DMatrixRMaj getL()
        {
            return L;
        }
    }
}