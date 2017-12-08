using SharpMatrix.Data;

namespace SharpMatrix.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * <p>
 * Cholesky LDL<sup>T</sup> decomposition.
 * </p>
 * <p>
 * A Cholesky LDL decomposition decomposes positive-definite symmetric matrices into:<br>
 * <br>
 * L*D*L<sup>T</sup>=A<br>
 * <br>
 * where L is a lower triangular matrix and D is a diagonal matrix.  The main advantage of LDL versus LL or RR Cholesky is that
 * it avoid a square root operation.
 * </p>
 *
 * @author Peter Abeles
 */
    public interface CholeskyLDLDecomposition<TMatrix>
        : DecompositionInterface<TMatrix>
        where TMatrix : Matrix
    {


        /**
         * <p>
         * Returns the lower triangular matrix from the decomposition.
         * </p>
         *
         * <p>
         * If an input is provided that matrix is used to write the results to.
         * Otherwise a new matrix is created and the results written to it.
         * </p>
         *
         * @param L If not null then the decomposed matrix is written here.
         * @return A lower triangular matrix.
         */
        TMatrix getL(TMatrix L);

        /**
         * <p>
         * Returns the diagonal matrixfrom the decomposition.
         * </p>
         *
         * <p>
         * If an input is provided that matrix is used to write the results to.
         * Otherwise a new matrix is created and the results written to it.
         * </p>
         *
         * @param D If not null it will be used to store the diagonal matrix
         * @return D Square diagonal matrix
         */
        TMatrix getD(TMatrix D);
    }
}