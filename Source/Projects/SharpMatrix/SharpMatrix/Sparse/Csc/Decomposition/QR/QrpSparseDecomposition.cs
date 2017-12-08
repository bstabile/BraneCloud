using System;
using SharpMatrix.Data;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Sparse.Csc.Decomposition.QR
{
    //package org.ejml.sparse.csc.decomposition.qr;

/**
 * <p>
 * Similar to {@link QRDecomposition} but it can handle the rank deficient case by
 * performing column pivots during the decomposition. The decomposition has the
 * following structure:<br>
 * P_r*A*P_c=Q*R<br>
 * where A is the original matrix, P is a pivot matrix, Q is an orthogonal matrix, and R is
 * upper triangular.
 * </p>
 *
 * @author Peter Abeles
 */
// TODO make left looking just regular QR
    // TODO move row pivot matrix into generic header
    // TODO update unit tests
    public interface QrpSparseDecomposition<T> : QRDecomposition<T>
        where T : Matrix
    {
        /**
         * Returns the rank as determined by the algorithm.  This is dependent upon a fixed threshold
         * and might not be appropriate for some applications.
         *
         * @return Matrix's rank
         */
        int getRank();

        /**
         * Ordering of each column after pivoting.   The current column i was original at column pivot[i].
         *
         * @return Order of columns.
         */
        int[] getColPivots();

        /**
         * Creates the column pivot matrix.
         *
         * @param P Optional storage for pivot matrix.  If null a new matrix will be created.
         * @return The pivot matrix.
         */
        T getColPivotMatrix(T P);

        /**
         * Ordering of each row after pivoting.   The current row i was original at row pivot[i].
         *
         * @return Order of rows.
         */
        int[] getRowPivots();

        /**
         * Creates the row pivot matrix.
         *
         * @param P Optional storage for pivot matrix.  If null a new matrix will be created.
         * @return The pivot matrix.
         */
        T getRowPivotMatrix(T P);

        bool isColumnPivot();

        bool isRowPivot();
    }
}