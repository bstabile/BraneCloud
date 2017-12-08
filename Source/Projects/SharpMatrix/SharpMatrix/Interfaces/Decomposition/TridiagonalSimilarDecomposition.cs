using SharpMatrix.Data;

namespace SharpMatrix.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * <p>
 * Finds the decomposition of a matrix in the form of:<br>
 * <br>
 * A = O*T*O<sup>T</sup><br>
 * <br>
 * where A is a symmetric m by m matrix, O is an orthogonal matrix, and T is a tridiagonal matrix.
 * </p>
 *
 * @author Peter Abeles
 */
    public interface TridiagonalSimilarDecomposition<TMatrix> : DecompositionInterface<TMatrix>
        where TMatrix : Matrix
    {

        /**
         * Extracts the tridiagonal matrix found in the decomposition.
         *
         * @param T If not null then the results will be stored here.  Otherwise a new matrix will be created.
         * @return The extracted T matrix.
         */
        TMatrix getT(TMatrix T);

        /**
         * An orthogonal matrix that has the following property: T = Q<sup>H</sup>AQ
         *
         * @param Q If not null then the results will be stored here.  Otherwise a new matrix will be created.
         * @param transposed If true then the transpose (real) or conjugate transpose (complex) of Q is returned.
         * @return The extracted Q matrix.
         */
        TMatrix getQ(TMatrix Q, bool transposed);
    }
}