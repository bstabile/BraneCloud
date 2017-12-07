using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * <p>
 * This is a generic interface for computing the eigenvalues and eigenvectors of a matrix.
 * Eigenvalues and eigenvectors have the following property:<br>
 * <br>
 * A*v=&lambda;*v<br>
 * <br>
 * where A is a square matrix and v is an eigenvector associated with the eigenvalue &lambda;.
 * </p>
 *
 * <p>
 * In general, both eigenvalues and eigenvectors can be complex numbers.  For symmetric matrices the
 * eigenvalues and eigenvectors are always real numbers.  EJML does not support complex matrices but
 * it does have minimal support for complex numbers.  As a result complex eigenvalues are found, but only
 * the real eigenvectors are computed.
 * </p>
 *
 * <p>
 * To create a new instance of {@link EigenDecomposition} use DecompositionFactory_XXXX. If the matrix
 * is known to be symmetric be sure to use the symmetric decomposition, which is much faster and more accurate
 * than the general purpose one.
 * </p>
 * @author Peter Abeles
 */
    public interface EigenDecomposition<T> : DecompositionInterface<T>
        where T : Matrix
    {

        /**
         * Returns the number of eigenvalues/eigenvectors.  This is the matrix's dimension.
         *
         * @return number of eigenvalues/eigenvectors.
         */
        int getNumberOfEigenvalues();


        /**
         * <p>
         * Used to retrieve real valued eigenvectors.  If an eigenvector is associated with a complex eigenvalue
         * then null is returned instead.
         * </p>
         *
         * @param index Index of the eigenvalue eigenvector pair.
         * @return If the associated eigenvalue is real then an eigenvector is returned, null otherwise.
         */
        T getEigenVector(int index);
    }
}