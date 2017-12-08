using System;
using SharpMatrix.Data;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Row.Decomposition.Chol
{
    //package org.ejml.dense.row.decompose.chol;


/**
 *
 * <p>
 * This is an abstract class for a Cholesky decomposition.  It provides the solvers, but the actual
 * decomposition is provided in other classes.
 * </p>
 *
 * @see CholeskyDecomposition_F64
 * @author Peter Abeles
 */
    public abstract class CholeskyDecompositionCommon_ZDRM : CholeskyDecomposition_F64<ZMatrixRMaj>
    {

        // width and height of the matrix
        protected int n;

        // the decomposed matrix
        protected ZMatrixRMaj T;

        protected double[] t;


        // is it a lower triangular matrix or an upper triangular matrix
        protected bool lower;

        // storage for the determinant
        protected Complex_F64 det = new Complex_F64();

        /**
         * Specifies if a lower or upper variant should be constructed.
         *
         * @param lower should a lower or upper triangular matrix be used.
         */
        public CholeskyDecompositionCommon_ZDRM(bool lower)
        {
            this.lower = lower;
        }


        /**
         * {@inheritDoc}
         */
        //@Override
        public bool isLower()
        {
            return lower;
        }

        /**
         * {@inheritDoc}
         */
        //@Override
        public bool decompose(ZMatrixRMaj mat)
        {
            if (mat.numRows != mat.numCols)
            {
                throw new ArgumentException("Must be a square matrix.");
            }

            n = mat.numRows;

            T = mat;
            t = T.data;

            if (lower)
            {
                return decomposeLower();
            }
            else
            {
                return decomposeUpper();
            }
        }

        //@Override
        public bool inputModified()
        {
            return true;
        }

        /**
         * Performs an lower triangular decomposition.
         *
         * @return true if the matrix was decomposed.
         */
        protected abstract bool decomposeLower();

        /**
         * Performs an upper triangular decomposition.
         *
         * @return true if the matrix was decomposed.
         */
        protected abstract bool decomposeUpper();

        //@Override
        public ZMatrixRMaj getT(ZMatrixRMaj T)
        {
            // write the values to T
            if (lower)
            {
                T = UtilDecompositons_ZDRM.checkZerosUT(T, n, n);
                for (int i = 0; i < n; i++)
                {
                    int index = i * n * 2;
                    for (int j = 0; j <= i; j++)
                    {
                        T.data[index] = this.T.data[index];
                        index++;
                        T.data[index] = this.T.data[index];
                        index++;
                    }
                }
            }
            else
            {
                T = UtilDecompositons_ZDRM.checkZerosLT(T, n, n);
                for (int i = 0; i < n; i++)
                {
                    int index = (i * n + i) * 2;
                    for (int j = i; j < n; j++)
                    {
                        T.data[index] = this.T.data[index];
                        index++;
                        T.data[index] = this.T.data[index];
                        index++;
                    }
                }
            }

            return T;
        }

        /**
         * Returns the raw decomposed matrix.
         *
         * @return A lower or upper triangular matrix.
         */
        public ZMatrixRMaj _getT()
        {
            return T;
        }

        //@Override
        public Complex_F64 computeDeterminant()
        {
            double prod = 1;

            // take advantage of the diagonal elements all being real
            int total = n * n * 2;
            for (int i = 0; i < total; i += 2 * (n + 1))
            {
                prod *= t[i];
            }

            det.real = prod * prod;
            det.imaginary = 0;

            return det;
        }
    }
}