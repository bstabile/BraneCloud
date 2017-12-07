using System.Runtime.Serialization;

namespace BraneCloud.Evolution.EC.MatrixLib.Data
{
    /**
     * Base interface for all rectangular matrices
     *
     * @author Peter Abeles
     */
    public interface Matrix
    {
        /**
         * Returns the number of rows in this matrix.
         *
         * @return Number of rows.
         */
        int getNumRows();

        /**
         * Returns the number of columns in this matrix.
         *
         * @return Number of columns.
         */
        int getNumCols();

        /**
         * Creates an exact copy of the matrix
         */
        Matrix copy();

        /**
         * Creates a new matrix with the same shape as this matrix
         */
        Matrix createLike();

        /**
         * Sets this matrix to be identical to the 'original' matrix passed in.
         */
        void set(Matrix original);

        /**
         * Prints the matrix to standard out.
         */
        void print();

        /**
         * Returns the type of matrix
         */
        MatrixType getType();
    }
}