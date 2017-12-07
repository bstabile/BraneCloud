using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * <p>
 * Similar to {@link QRDecomposition} but it can handle the rank deficient case by
 * performing column pivots during the decomposition. The decomposition has the
 * following structure:<br>
 * A*P=Q*R<br>
 * where A is the original matrix, P is a pivot matrix, Q is an orthogonal matrix, and R is
 * upper triangular.
 * </p>
 *
 * @author Peter Abeles
 */
    public interface QRPDecomposition<T> : QRDecomposition<T> where T : Matrix
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
    }
}