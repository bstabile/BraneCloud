using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;


/**
 * <p>
 * Cholesky decomposition.  It decomposes positive-definite symmetric matrices (real)
 * or hermitian-positive definite (complex) into either upper or lower triangles:<br>
 * <br>
 * L*L<sup>H</sup>=A<br>
 * R<sup>H</sup>*R=A<br>
 * <br>
 * where L is a lower triangular matrix and R is an upper triangular matrix.  This is typically 
 * used to invert matrices, such as a covariance matrix.<br>
 * </p>
 *
 * @author Peter Abeles
 */
    public interface CholeskyDecomposition<TMatrix> : DecompositionInterface<TMatrix>
        where TMatrix : Matrix
    {

        /**
         * If true the decomposition was for a lower triangular matrix.
         * If false it was for an upper triangular matrix.
         *
         * @return True if lower, false if upper.
         */
        bool isLower();

        /**
         * <p>
         * Returns the triangular matrix from the decomposition.
         * </p>
         *
         * <p>
         * If an input is provided that matrix is used to write the results to.
         * Otherwise a new matrix is created and the results written to it.
         * </p>
         *
         * @param T If not null then the decomposed matrix is written here.
         * @return A lower or upper triangular matrix.
         */
        TMatrix getT(TMatrix T);
    }
}