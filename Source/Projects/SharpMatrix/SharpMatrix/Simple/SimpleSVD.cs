using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Factory;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;

namespace BraneCloud.Evolution.EC.MatrixLib.Simple
{
    //package org.ejml.simple;

    /**
     * <p>
     * Wrapper around SVD for simple matrix.  See {@link SingularValueDecomposition} for more details.
     * </p>
     * SVD is defined as the following decomposition:<br>
     * <center> A = U * W * V <sup>T</sup> </center>
     * where A is m by n, and U and V are orthogonal matrices, and  W is a diagonal matrix
     *
     * <p>
     * Tolerance for singular values is
     * Math.Max(mat.numRows,mat.numCols) * W.get(0,0) * UtilEjml.EPS;
     * where W.get(0,0) is the largest singular value.
     * </p>
     *
     * @author Peter Abeles
     */
    //@SuppressWarnings({"unchecked"})
    public class SimpleSVD<T> 
        where T : class, Matrix 
    {

        private SingularValueDecomposition<T> svd;
        private SimpleBase<T> U;
        private SimpleBase<T> W;
        private SimpleBase<T> V;

        private Matrix mat;
        bool is64;

        // tolerance for singular values
        double tol;

        public SimpleSVD(Matrix mat, bool compact)
        {
            this.mat = mat;
            this.is64 = mat is DMatrixRMaj;
            if (is64)
            {
                DMatrixRMaj m = (DMatrixRMaj) mat;
                svd = (SingularValueDecomposition<T>) DecompositionFactory_DDRM.svd(m.numRows, m.numCols, true, true, compact);
            }
            else
            {
                FMatrixRMaj m = (FMatrixRMaj) mat;
                svd = (SingularValueDecomposition<T>) DecompositionFactory_FDRM.svd(m.numRows, m.numCols, true, true, compact);
            }

            if (!svd.decompose((T) mat))
                throw new InvalidOperationException("Decomposition failed");
            U = SimpleMatrix<T>.wrap(svd.getU(null, false));
            W = SimpleMatrix<T>.wrap(svd.getW(null));
            V = SimpleMatrix<T>.wrap(svd.getV(null, false));

            // order singular values from largest to smallest
            if (is64)
            {
                var um = U.getMatrix() as DMatrixRMaj;
                var wm = W.getMatrix() as DMatrixRMaj;
                var vm = V.getMatrix() as DMatrixRMaj;
                SingularOps_DDRM.descendingOrder(um, false, wm, vm, false);
                tol = SingularOps_DDRM.singularThreshold((SingularValueDecomposition_F64<DMatrixRMaj>) svd);
            }
            else
            {
                var um = U.getMatrix() as FMatrixRMaj;
                var wm = W.getMatrix() as FMatrixRMaj;
                var vm = V.getMatrix() as FMatrixRMaj;
                SingularOps_FDRM.descendingOrder(um, false, wm, vm, false);
                tol = SingularOps_FDRM.singularThreshold((SingularValueDecomposition_F32<FMatrixRMaj>) svd);
            }

        }

        /**
         * <p>
         * Returns the orthogonal 'U' matrix.
         * </p>
         *
         * @return An orthogonal m by m matrix.
         */
        public SimpleBase<T> getU()
        {
            return U;
        }

        /**
         * Returns a diagonal matrix with the singular values.  The singular values are ordered
         * from largest to smallest.
         *
         * @return Diagonal matrix with singular values along the diagonal.
         */
        public SimpleBase<T> getW()
        {
            return W;
        }

        /**
         * <p>
         * Returns the orthogonal 'V' matrix.
         * </p>
         *
         * @return An orthogonal n by n matrix.
         */
        public SimpleBase<T> getV()
        {
            return V;
        }

        /**
         * <p>
         * Computes the quality of the computed decomposition.  A value close to or less than 1e-15
         * is considered to be within machine precision.
         * </p>
         *
         * <p>
         * This function must be called before the original matrix has been modified or else it will
         * produce meaningless results.
         * </p>
         *
         * @return Quality of the decomposition.
         */
        public /**/ double quality()
        {
            if (is64)
            {
                var m = mat as DMatrixRMaj;
                var um = U.getMatrix() as DMatrixRMaj;
                var wm = W.getMatrix() as DMatrixRMaj;
                var vmt = V.transpose().getMatrix() as DMatrixRMaj;
                return DecompositionFactory_DDRM.quality(m, um, wm, vmt);
            }
            else
            {
                var m = mat as FMatrixRMaj;
                var um = U.getMatrix() as FMatrixRMaj;
                var wm = W.getMatrix() as FMatrixRMaj;
                var vmt = V.transpose().getMatrix() as FMatrixRMaj;
                return DecompositionFactory_FDRM.quality(m, um, wm, vmt);
            }
        }

        /**
         * Computes the null space from an SVD.  For more information see {@link SingularOps_DDRM#nullSpace}.
         * @return Null space vector.
         */
        public SimpleMatrix<T> nullSpace()
        {
            // TODO take advantage of the singular values being ordered already
            if (is64)
            {
                var ns = SingularOps_DDRM.nullSpace((SingularValueDecomposition_F64<DMatrixRMaj>) svd, 
                    null, tol);
                return SimpleMatrix<T>.wrap(ns as T);
            }
            else
            {
                var ns = SingularOps_FDRM.nullSpace((SingularValueDecomposition_F32<FMatrixRMaj>) svd, 
                    null, (float) tol);
                return SimpleMatrix<T>.wrap(ns as T);
            }
        }

        /**
         * Returns the specified singular value.
         *
         * @param index Which singular value is to be returned.
         * @return A singular value.
         */
        public double getSingleValue(int index)
        {
            return W.get(index, index);
        }

        /**
         * Returns an array of all the singular values
         */
        public double[] getSingularValues()
        {
            double[] ret = new double[W.numCols()];

            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = getSingleValue(i);
            }
            return ret;
        }

        /**
         * Returns the rank of the decomposed matrix.
         *
         * @see SingularOps_DDRM#rank(SingularValueDecomposition_F64, double)
         *
         * @return The matrix's rank
         */
        public int rank()
        {
            if (is64)
            {
                return SingularOps_DDRM.rank((SingularValueDecomposition_F64<DMatrixRMaj>) svd, tol);
            }
            else
            {
                return SingularOps_FDRM.rank((SingularValueDecomposition_F32<FMatrixRMaj>) svd, (float) tol);
            }
        }

        /**
         * The nullity of the decomposed matrix.
         *
         * @see SingularOps_DDRM#nullity(SingularValueDecomposition_F64, double)
         *
         * @return The matrix's nullity
         */
        public int nullity()
        {
            if (is64)
            {
                return SingularOps_DDRM.nullity((SingularValueDecomposition_F64<DMatrixRMaj>) svd, 10.0 * UtilEjml.EPS);
            }
            else
            {
                return SingularOps_FDRM.nullity((SingularValueDecomposition_F32<FMatrixRMaj>) svd, 5.0f * UtilEjml.F_EPS);
            }
        }

        /**
         * Returns the underlying decomposition that this is a wrapper around.
         *
         * @return SingularValueDecomposition
         */
        public SingularValueDecomposition<T> getSVD()
        {
            return svd;
        }
    }
}