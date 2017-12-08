using System;
using SharpMatrix.Data;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Row.Decomposition.Chol
{
    //package org.ejml.dense.row.decomposition.chol;

/**
 * <p>
 * This variant on the Cholesky decomposition avoid the need to take the square root
 * by performing the following decomposition:<br>
 * <br>
 * L*D*L<sup>T</sup>=A<br>
 * <br>
 * where L is a lower triangular matrix with zeros on the diagonal. D is a diagonal matrix.
 * The diagonal elements of L are equal to one.
 * </p>
 * <p>
 * Unfortunately the speed advantage of not computing the square root is washed out by the
 * increased number of array accesses.  There only appears to be a slight speed boost for
 * very small matrices.
 * </p>
 *
 * @author Peter Abeles
 */
    public class CholeskyDecompositionLDL_DDRM
        : CholeskyLDLDecomposition_F64<DMatrixRMaj>
    {

        // it can decompose a matrix up to this width
        private int maxWidth;

        // width and height of the matrix
        private int n;

        // the decomposed matrix
        private DMatrixRMaj L;

        // the D vector
        private double[] d;

        // tempoary variable used by various functions
        double[] vv;

        public void setExpectedMaxSize(int numRows, int numCols)
        {
            if (numRows != numCols)
            {
                throw new ArgumentException("Can only decompose square matrices");
            }

            this.maxWidth = numRows;

            this.L = new DMatrixRMaj(maxWidth, maxWidth);

            this.vv = new double[maxWidth];
            this.d = new double[maxWidth];
        }

        /**
         * <p>
         * Performs Choleksy decomposition on the provided matrix.
         * </p>
         *
         * <p>
         * If the matrix is not positive definite then this function will return
         * false since it can't complete its computations.  Not all errors will be
         * found.
         * </p>
         * @param mat A symetric n by n positive definite matrix.
         * @return True if it was able to finish the decomposition.
         */
        public bool decompose(DMatrixRMaj mat)
        {
            if (mat.numRows > maxWidth)
            {
                setExpectedMaxSize(mat.numRows, mat.numCols);
            }
            else if (mat.numRows != mat.numCols)
            {
                throw new InvalidOperationException("Can only decompose square matrices");
            }
            n = mat.numRows;

            L.set(mat);
            double[] el = L.data;

            double d_inv = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    double sum = el[i * n + j];

                    for (int k = 0; k < i; k++)
                    {
                        sum -= el[i * n + k] * el[j * n + k] * d[k];
                    }

                    if (i == j)
                    {
                        // is it positive-definite?
                        if (sum <= 0.0)
                            return false;

                        d[i] = sum;
                        d_inv = 1.0 / sum;
                        el[i * n + i] = 1;
                    }
                    else
                    {
                        el[j * n + i] = sum * d_inv;
                    }
                }
            }
            // zero the top right corner.
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    el[i * n + j] = 0.0;
                }
            }

            return true;
        }

        public virtual bool inputModified()
        {
            return false;
        }

        /**
         * Diagonal elements of the diagonal D matrix.
         *
         * @return diagonal elements of D
         */
        public virtual double[] getDiagonal()
        {
            return d;
        }

        /**
         * Returns L matrix from the decomposition.<br>
         * L*D*L<sup>T</sup>=A
         *
         * @return A lower triangular matrix.
         */
        public DMatrixRMaj getL()
        {
            return L;
        }

        public double[] _getVV()
        {
            return vv;
        }

        public virtual DMatrixRMaj getL(DMatrixRMaj L)
        {
            if (L == null)
            {
                L = (DMatrixRMaj) this.L.copy();
            }
            else
            {
                L.set(this.L);
            }

            return L;
        }

        public virtual DMatrixRMaj getD(DMatrixRMaj D)
        {
            return CommonOps_DDRM.diag(D, L.numCols, d);
        }
    }
}