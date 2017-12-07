
using System;

namespace BraneCloud.Evolution.EC.MatrixLib.Data
{
    /**
     * A generic abstract class for matrices whose data is stored in a single 1D array of floats.  The
     * format of the elements in this array is not specified.  For example row major, column major,
     * and block row major are all common formats.
     *
     * @author Peter Abeles
     */
     [Serializable]
    public abstract class FMatrixD1 : ReshapeMatrix, FMatrix
    {
        public abstract float get(int row, int col);
        public abstract float unsafe_get(int row, int col);
        public abstract void set(int row, int col, float val);
        public abstract void unsafe_set(int row, int col, float val);
        public abstract int getNumElements();
        public abstract Matrix copy();
        public abstract Matrix createLike();
        public abstract void set(Matrix original);
        public abstract void print();
        public abstract MatrixType getType();

        /**
         * Returns the internal array index for the specified row and column.
         *
         * @param row Row index.
         * @param col Column index.
         * @return Internal array index.
         */
        public abstract int getIndex(int row, int col);
        /**
         * <p>
         * Changes the number of rows and columns in the matrix, allowing its size to grow or shrink.
         * If the saveValues flag is set to true, then the previous values will be maintained, but
         * reassigned to new elements in a row-major ordering.  If saveValues is false values will only
         * be maintained when the requested size is less than or equal to the internal array size.
         * The primary use for this function is to encourage data reuse and avoid unnecessarily declaring
         * and initialization of new memory.
         * </p>
         *
         * <p>
         * Examples:<br>
         * [ 1 2 ; 3 4 ] &rarr; reshape( 2 , 3 , true ) = [ 1 2 3 ; 4 0 0 ]<br>
         * [ 1 2 ; 3 4 ] &rarr; reshape( 1 , 2 , true ) = [ 1 2 ]<br>
         * [ 1 2 ; 3 4 ] &rarr; reshape( 1 , 2 , false ) = [ 1 2 ]<br>
         * [ 1 2 ; 3 4 ] &rarr; reshape( 2 , 3 , false ) = [ 0 0 0 ; 0 0 0 ]
         * </p>
         *
         * @param numRows The new number of rows in the matrix.
         * @param numCols The new number of columns in the matrix.
         * @param saveValues If true then the value of each element will be save using a row-major reordering.  Typically this should be false.
         */
        public abstract void reshape(int numRows, int numCols, bool saveValues);



        /**
         * Where the raw data for the matrix is stored.  The format is type dependent.
         */
        public float[] data;

        /**
         * Number of rows in the matrix.
         */
        public int numRows;

        /**
         * Number of columns in the matrix.
         */
        public int numCols;

        /**
         * Used to get a reference to the internal data.
         *
         * @return Reference to the matrix's data.
         */
        public virtual float[] getData()
        {
            return data;
        }

        /**
         * Changes the internal array reference.
         */
        public void setData(float[] data)
        {
            this.data = data;
        }

        /**
         * Sets the value of this matrix to be the same as the value of the provided matrix.  Both
         * matrices must have the same shape:<br>
         * <br>
         * a<sub>ij</sub> = b<sub>ij</sub><br>
         * <br>
         *
         * @param b The matrix that this matrix is to be set equal to.
         */
        public void set(FMatrixD1 b)
        {
            this.reshape(b.numRows, b.numCols);

            int dataLength = b.getNumElements();

            Array.Copy(b.data, 0, this.data, 0, dataLength);
        }

        /**
         * Returns the value of the matrix at the specified internal array index. The element at which row and column
         * returned by this function depends upon the matrix's internal structure, e.g. row-major, column-major, or block.
         *
         * @param index Internal array index.
         * @return Value at the specified index.
         */
        public float get(int index)
        {
            return data[index];
        }

        /**
         * Sets the element's value at the specified index.  The element at which row and column
         * modified by this function depends upon the matrix's internal structure, e.g. row-major, column-major, or block.
         *
         * @param index Index of element that is to be set.
         * @param val The new value of the index.
         */
        public float set(int index, float val)
        {
            // See benchmarkFunctionReturn.  Pointless return does not degrade performance.  Tested on JDK 1.6f.0_21
            return data[index] = val;
        }

        /**
         * <p>
         * Adds the specified value to the internal data array at the specified index.<br>
         * <br>
         * Equivalent to: this.data[index] += val;
         * </p>
         *
         * <p>
         * Intended for use in highly optimized code.  The  row/column coordinate of the modified element is
         * dependent upon the matrix's internal structure.
         * </p>
         *
         * @param index The index which is being modified.
         * @param val The value that is being added.
         */
        public float plus(int index, float val)
        {
            // See benchmarkFunctionReturn.  Pointless return does not degrade performance.  Tested on JDK 1.6f.0_21
            return data[index] += val;
        }

        /**
         * <p>
         * Subtracts the specified value to the internal data array at the specified index.<br>
         * <br>
         * Equivalent to: this.data[index] -= val;
         * </p>
         *
         * <p>
         * Intended for use in highly optimized code.  The  row/column coordinate of the modified element is
         * dependent upon the matrix's internal structure.
         * </p>
         *
         * @param index The index which is being modified.
         * @param val The value that is being subtracted.
         */
        public float minus(int index, float val)
        {
            // See benchmarkFunctionReturn.  Pointless return does not degrade performance.  Tested on JDK 1.6f.0_21
            return data[index] -= val;
        }

        /**
         * <p>
         * Multiplies the specified value to the internal data array at the specified index.<br>
         * <br>
         * Equivalent to: this.data[index] *= val;
         * </p>
         *
         * <p>
         * Intended for use in highly optimized code.  The  row/column coordinate of the modified element is
         * dependent upon the matrix's internal structure.
         * </p>
         *
         * @param index The index which is being modified.
         * @param val The value that is being multiplied.
         */
        public float times(int index, float val)
        {
            // See benchmarkFunctionReturn.  Pointless return does not degrade performance.  Tested on JDK 1.6f.0_21
            return data[index] *= val;
        }

        /**
         * <p>
         * Divides the specified value to the internal data array at the specified index.<br>
         * <br>
         * Equivalent to: this.data[index] /= val;
         * </p>
         *
         * <p>
         * Intended for use in highly optimized code.  The  row/column coordinate of the modified element is
         * dependent upon the matrix's internal structure.
         * </p>
         *
         * @param index The index which is being modified.
         * @param val The value that is being divided.
         */
        public float div(int index, float val)
        {
            // See benchmarkFunctionReturn.  Pointless return does not degrade performance.  Tested on JDK 1.6f.0_21
            return data[index] /= val;
        }

        /**
         * Equivalent to invoking reshape(numRows,numCols,false);
         *
         * @param numRows The new number of rows in the matrix.
         * @param numCols The new number of columns in the matrix.
         */
        public virtual void reshape(int numRows, int numCols)
        {
            reshape(numRows, numCols, false);
        }

        /**
         * Creates a new iterator for traversing through a submatrix inside this matrix.  It can be traversed
         * by row or by column.  Range of elements is inclusive, e.g. minRow = 0 and maxRow = 1 will include rows
         * 0 and 1.  The iteration starts at (minRow,minCol) and ends at (maxRow,maxCol)
         *
         * @param rowMajor true means it will traverse through the submatrix by row first, false by columns.
         * @param minRow first row it will start at.
         * @param minCol first column it will start at.
         * @param maxRow last row it will stop at.
         * @param maxCol last column it will stop at.
         * @return A new MatrixIterator
         */
        public FMatrixIterator iterator(bool rowMajor, int minRow, int minCol, int maxRow, int maxCol)
        {
            return new FMatrixIterator(this, rowMajor, minRow, minCol, maxRow, maxCol);
        }

        /**
         * {@inheritDoc}
         */
        public virtual int getNumRows()
        {
            return numRows;
        }

        /**
         * {@inheritDoc}
         */
        public virtual int getNumCols()
        {
            return numCols;
        }

        /**
         * Sets the number of rows.
         *
         * @param numRows Number of rows
         */
        public void setNumRows(int numRows)
        {
            this.numRows = numRows;
        }

        /**
         * Sets the number of columns.
         *
         * @param numCols Number of columns
         */
        public void setNumCols(int numCols)
        {
            this.numCols = numCols;
        }

    }
}