using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Ops;

namespace BraneCloud.Evolution.EC.MatrixLib.Data
{
    //package org.ejml.data;

/**
 * Dense matrix composed of bool values
 *
 * @author Peter Abeles
 */
    public class BMatrixRMaj : ReshapeMatrix
    {
        /**
         * 1D row-major array for storing theboolean matrix
         */
        public bool[] data;

        /**
         * Number of rows in the matrix.
         */
        public int numRows;

        /**
         * Number of columns in the matrix.
         */
        public int numCols;

        public BMatrixRMaj(int numRows, int numCols)
        {
            data = new bool[numRows * numCols];
            this.numRows = numRows;
            this.numCols = numCols;
        }

        public int getNumElements()
        {
            return numRows * numCols;
        }

        public int getIndex(int row, int col)
        {
            return row * numCols + col;
        }

        public bool get(int index)
        {
            return data[index];
        }

        public bool get(int row, int col)
        {
            if (!isInBounds(row, col))
                throw new ArgumentException("Out of matrix bounds. " + row + " " + col);
            return data[row * numCols + col];
        }

        public void set(int row, int col, bool value)
        {
            if (!isInBounds(row, col))
                throw new ArgumentException("Out of matrix bounds. " + row + " " + col);
            data[row * numCols + col] = value;
        }

        public bool unsafe_get(int row, int col)
        {
            return data[row * numCols + col];
        }

        public void unsafe_set(int row, int col, bool value)
        {
            data[row * numCols + col] = value;
        }

        /**
         * Determines if the specified element is inside the bounds of the Matrix.
         *
         * @param row The element's row.
         * @param col The element's column.
         * @return True if it is inside the matrices bound, false otherwise.
         */
        public bool isInBounds(int row, int col)
        {
            return(col >= 0 && col < numCols && row >= 0 && row < numRows);
        }

        public virtual void reshape(int numRows, int numCols)
        {
            int N = numRows * numCols;
            if (data.Length < N)
            {
                data = new bool[N];
            }
            this.numRows = numRows;
            this.numCols = numCols;
        }

        public virtual int getNumRows()
        {
            return numRows;
        }

        public virtual int getNumCols()
        {
            return numCols;
        }

        public virtual Matrix copy()
        {
            BMatrixRMaj ret = new BMatrixRMaj(numRows, numCols);
            ret.set(this);
            return ret;
        }

        public virtual void set(Matrix original)
        {
            BMatrixRMaj orig = (BMatrixRMaj) original;

            reshape(original.getNumRows(), original.getNumCols());
            Array.Copy(orig.data, 0, data, 0, orig.getNumElements());
        }

        public virtual void print()
        {
            Console.WriteLine("Implement this");
        }

        public virtual Matrix createLike()
        {
            return new BMatrixRMaj(numRows, numCols);
        }

        public virtual MatrixType getType()
        {
            return MatrixType.UNSPECIFIED;
        }
    }
}