using System;
using SharpMatrix.Data;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Row.Decomposition.Chol
{
    //package org.ejml.dense.row.decomposition.chol;

/**
 *
 * <p>
 * This is an abstract class for a Cholesky decomposition.  It provides the solvers, but the actual
 * decomposition is provided in other classes.
 * </p>
 *
 * @see CholeskyDecomposition_F32
 * @author Peter Abeles
 */
    public abstract class CholeskyDecompositionCommon_FDRM
        : CholeskyDecomposition_F32<FMatrixRMaj>
    {

        // it can decompose a matrix up to this width
        protected int maxWidth = -1;

        // width and height of the matrix
        protected int n;

        // the decomposed matrix
        protected FMatrixRMaj T;

        protected float[] t;

        // tempoary variable used by various functions
        protected float[] vv;

        // is it a lower triangular matrix or an upper triangular matrix
        protected bool lower;

        // storage for computed determinant
        protected Complex_F32 det = new Complex_F32();

        /**
         * Specifies if a lower or upper variant should be constructed.
         *
         * @param lower should a lower or upper triangular matrix be used.
         */
        public CholeskyDecompositionCommon_FDRM(bool lower)
        {
            this.lower = lower;
        }

        public virtual void setExpectedMaxSize(int numRows, int numCols)
        {
            if (numRows != numCols)
            {
                throw new ArgumentException("Can only decompose square matrices");
            }

            this.maxWidth = numCols;

            this.vv = new float[maxWidth];
        }

        /**
         * If true the decomposition was for a lower triangular matrix.
         * If false it was for an upper triangular matrix.
         *
         * @return True if lower, false if upper.
         */
        public virtual bool isLower()
        {
            return lower;
        }

        /**
         * <p>
         * Performs Choleksy decomposition on the provided matrix.
         * </p>
         *
         * <p>
         * If the matrix is not positive definite then this function will return
         * false since it can't complete its computations.  Not all errors will be
         * found.  This is an efficient way to check for positive definiteness.
         * </p>
         * @param mat A symmetric positive definite matrix with n &le; widthMax.
         * @return True if it was able to finish the decomposition.
         */
        public virtual bool decompose(FMatrixRMaj mat)
        {
            if (mat.numRows > maxWidth)
            {
                setExpectedMaxSize(mat.numRows, mat.numCols);
            }
            else if (mat.numRows != mat.numCols)
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

        public virtual bool inputModified()
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

        public virtual FMatrixRMaj getT(FMatrixRMaj T)
        {

            // write the values to T
            if (lower)
            {
                T = UtilDecompositons_FDRM.checkZerosUT(T, n, n);
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        T.unsafe_set(i, j, this.T.unsafe_get(i, j));
                    }
                }
            }
            else
            {
                T = UtilDecompositons_FDRM.checkZerosLT(T, n, n);
                for (int i = 0; i < n; i++)
                {
                    for (int j = i; j < n; j++)
                    {
                        T.unsafe_set(i, j, this.T.unsafe_get(i, j));
                    }
                }
            }

            return T;
        }

        /**
         * Returns the triangular matrix from the decomposition.
         *
         * @return A lower or upper triangular matrix.
         */
        public FMatrixRMaj getT()
        {
            return T;
        }

        public float[] _getVV()
        {
            return vv;
        }

        public virtual Complex_F32 computeDeterminant()
        {
            float prod = 1;

            int total = n * n;
            for (int i = 0; i < total; i += n + 1)
            {
                prod *= t[i];
            }

            det.real = prod * prod;
            det.imaginary = 0;

            return det;
        }
    }
}