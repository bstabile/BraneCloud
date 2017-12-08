using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row.Decomposition.Chol;
using SharpMatrix.Dense.Row.Decomposition.LU;
using SharpMatrix.Dense.Row.Decomposition.QR;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Row.Factory
{
    //package org.ejml.dense.row.factory;

/**
 * <p>
 * Contains operations related to creating and evaluating the quality of common matrix decompositions.  Except
 * in specialized situations, matrix decompositions should be instantiated from this factory instead of being
 * directly constructed.  Low level implementations are more prone to changes and new algorithms will be
 * automatically placed here.
 * </p>
 *
 * @author Peter Abeles
 */
    public class DecompositionFactory_ZDRM
    {
        /**
         * <p>
         * Returns a {@link org.ejml.interfaces.decomposition.LUDecomposition} that has been optimized for the specified matrix size.
         * </p>
         *
         * @param numRows Number of rows the returned decomposition is optimized for.
         * @param numCols Number of columns that the returned decomposition is optimized for.
         * @return LUDecomposition
         */
        public static LUDecomposition_F64<ZMatrixRMaj> lu(int numRows, int numCols)
        {
            return new LUDecompositionAlt_ZDRM();
        }

        /**
         * <p>
         * Returns a {@link org.ejml.interfaces.decomposition.QRDecomposition} that has been optimized for the specified matrix size.
         * </p>
         *
         * @param numRows Number of rows the returned decomposition is optimized for.
         * @param numCols Number of columns that the returned decomposition is optimized for.
         * @return QRDecomposition
         */
        public static QRDecomposition<ZMatrixRMaj> qr(int numRows, int numCols)
        {
            return new QRDecompositionHouseholderColumn_ZDRM();
        }

        /**
         * <p>
         * Returns a {@link CholeskyDecomposition_F64} that has been optimized for the specified matrix size.
         * </p>
         *
         * @param size Number of rows and columns it should be optimized for
         * @param lower if true then it will be a lower cholesky.  false for upper.  Try lower.
         * @return QRDecomposition
         */
        public static CholeskyDecomposition_F64<ZMatrixRMaj> chol(int size, bool lower)
        {
            return new CholeskyDecompositionInner_ZDRM(lower);
        }

        /**
         * Decomposes the input matrix 'a' and makes sure it isn't modified.
         */
        public static bool decomposeSafe(DecompositionInterface<ZMatrixRMaj> decomposition, ZMatrixRMaj a)
        {

            if (decomposition.inputModified())
            {
                a = (ZMatrixRMaj) a.copy();
            }
            return decomposition.decompose(a);
        }
    }
}