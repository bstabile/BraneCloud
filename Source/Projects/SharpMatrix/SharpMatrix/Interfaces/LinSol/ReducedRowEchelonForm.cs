using SharpMatrix.Data;

namespace SharpMatrix.Interfaces.LinSol
{
    //package org.ejml.interfaces.linsol;

/**
 * <p>
 * An augmented system matrix is said to be in reduced row echelon form (RREF) if the following are true:
 * </p>
 *
 * <ol>
 *     <li>If a row has non-zero entries, then the first non-zero entry is 1.  This is known as the leading one.</li>
 *     <li>If a column contains a leading one then all other entries in that column are zero.</li>
 *     <li>If a row contains a leading 1, then each row above contains a leading 1 further to the left.</li>
 * </ol>
 *
 * <p>
 * [1] Page 19 in, Otter Bretscherm "Linear Algebra with Applications" Prentice-Hall Inc, 1997
 * </p>
 *
 * @author Peter Abeles
 */
    public interface ReducedRowEchelonForm<T> where T : Matrix
    {

        /**
         * Puts the augmented matrix into RREF.  The coefficient matrix is stored in
         * columns less than coefficientColumns.
         *
         *
         * @param A Input: Augmented matrix.  Output: RREF.  Modified.
         * @param coefficientColumns Number of coefficients in the system matrix.
         */
        void reduce(T A, int coefficientColumns);
    }
}