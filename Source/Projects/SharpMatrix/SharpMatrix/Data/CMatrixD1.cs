using System;

namespace BraneCloud.Evolution.EC.MatrixLib.Data
{
    //package org.ejml.data;

    /**
     * A generic abstract class for matrices whose data is stored in a single 1D array of floats.  The
     * format of the elements in this array is not specified.  For example row major, column major,
     * and block row major are all common formats.
     *
     * @author Peter Abeles
     */
    public abstract class CMatrixD1 : CMatrix, ReshapeMatrix
    {
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
        public float[] getData()
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
         * Returns the internal array index for the specified row and column.
         *
         * @param row Row index.
         * @param col Column index.
         * @return Internal array index.
         */
        public abstract int getIndex(int row, int col);

        /**
         * Sets the value of this matrix to be the same as the value of the provided matrix.  Both
         * matrices must have the same shape:<br>
         * <br>
         * a<sub>ij</sub> = b<sub>ij</sub><br>
         * <br>
         *
         * @param b The matrix that this matrix is to be set equal to.
         */
        public void set(CMatrixD1 b)
        {
            if (numRows != b.numRows || numCols != b.numCols)
            {
                throw new MatrixDimensionException("The two matrices do not have compatible shapes.");
            }

            int dataLength = b.getDataLength();

            Array.Copy(b.data, 0, this.data, 0, dataLength);
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

        public int getNumElements()
        {
            return numRows * numCols;
        }

        public abstract Matrix copy();
        public abstract Matrix createLike();
        public abstract void set(Matrix original);
        public abstract void print();
        public abstract MatrixType getType();
        public abstract void reshape(int numRows, int numCols);
        public abstract void get(int row, int col, Complex_F32 output);
        public abstract void set(int row, int col, float real, float imaginary);
        public abstract float getReal(int row, int col);
        public abstract void setReal(int row, int col, float val);
        public abstract float getImag(int row, int col);
        public abstract void setImag(int row, int col, float val);
        public abstract int getDataLength();

    }
}