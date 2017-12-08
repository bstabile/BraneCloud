using System;
using SharpMatrix.Data;

namespace SharpMatrix.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * <p>
 * Computes a matrix decomposition such that:<br>
 * <br>
 * A = U*B*V<sup>T</sup><br>
 * <br>
 * where A is m by n, U is orthogonal and m by m, B is an m by n bidiagonal matrix, V is orthogonal and n by n.
 * This is used as a first step in computing the SVD of a matrix for the QR algorithm approach.
 * </p>
 * <p>
 * A bidiagonal matrix has zeros in every element except for the two diagonals.<br>
 * <br>
 * b_ij = 0    if i &gt; j or i &lt; j-1<br>
 * </p>
 *
 *
 * @author Peter Abeles
 */
    public interface BidiagonalDecomposition<T> : DecompositionInterface<T>
        where T : Matrix
    {
        /**
         * Returns the bidiagonal matrix.
         *
         * @param B If not null the results are stored here, if null a new matrix is created.
         * @return The bidiagonal matrix.
         */
        T getB(T B, bool compact);

        /**
         * Returns the orthogonal U matrix.
         *
         * @param U If not null then the results will be stored here.  Otherwise a new matrix will be created.
         * @return The extracted Q matrix.
         */
        T getU(T U, bool transpose, bool compact);

        /**
         * Returns the orthogonal V matrix.
         *
         * @param V If not null then the results will be stored here.  Otherwise a new matrix will be created.
         * @return The extracted Q matrix.
         */
        T getV(T V, bool transpose, bool compact);
    }
}