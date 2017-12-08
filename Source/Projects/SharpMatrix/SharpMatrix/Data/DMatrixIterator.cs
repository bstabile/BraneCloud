using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpMatrix.Data
{

    /**
     * This is a matrix iterator for traversing through a submatrix.  For speed it is recommended
     * that you directly access the elements in the matrix, but there are some situations where this
     * can be a better design.
     *
     * @author Peter Abeles
     */
    public class DMatrixIterator : IEnumerator<double>
    {
        // the matrix which is being iterated through
        private DMatrixD1 a;

        // should it iterate through by row or by column
        private bool rowMajor;

        // the first row and column it returns
        private int minCol;

        private int minRow;

        // where in the iteration it is
        private int index = 0;

        // how many elements inside will it return
        private int size;

        // how wide the submatrix is
        private int submatrixStride;

        // the current element
        int subRow, subCol;

        /**
         * Creates a new iterator for traversing through a submatrix inside this matrix.  It can be traversed
         * by row or by column.  Range of elements is inclusive, e.g. minRow = 0 and maxRow = 1 will include rows
         * 0 and 1.  The iteration starts at (minRow,minCol) and ends at (maxRow,maxCol)
         *
         * @param a the matrix it is iterating through
         * @param rowMajor true means it will traverse through the submatrix by row first, false by columns.
         * @param minRow first row it will start at.
         * @param minCol first column it will start at.
         * @param maxRow last row it will stop at.
         * @param maxCol last column it will stop at.
         */
        public DMatrixIterator(DMatrixD1 a, bool rowMajor,
            int minRow, int minCol, int maxRow, int maxCol
        )
        {
            if (maxCol < minCol)
                throw new ArgumentException("maxCol has to be more than or equal to minCol");
            if (maxRow < minRow)
                throw new ArgumentException("maxRow has to be more than or equal to minCol");
            if (maxCol >= a.numCols)
                throw new ArgumentException("maxCol must be < numCols");
            if (maxRow >= a.numRows)
                throw new ArgumentException("maxRow must be < numCRows");

            this.a = a;
            this.rowMajor = rowMajor;
            this.minCol = minCol;
            this.minRow = minRow;

            size = (maxCol - minCol + 1) * (maxRow - minRow + 1);

            if (rowMajor)
                submatrixStride = maxCol - minCol + 1;
            else
                submatrixStride = maxRow - minRow + 1;
        }

        #region .NET Implementation of IEnumerator

        private int _currIndex = -1;

        public double Current { get; private set; }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            var hasNext = ++_currIndex < size;
            if (hasNext)
            {
                if (rowMajor)
                {
                    subRow = _currIndex / submatrixStride;
                    subCol = _currIndex % submatrixStride;
                }
                else
                {
                    subRow = _currIndex % submatrixStride;
                    subCol = _currIndex / submatrixStride;
                }
                Current = a.get(subRow + minRow, subCol + minCol);
                return true;
            }
            Current = double.NaN;
            return false;
        }

        public void Reset()
        {
            _currIndex = -1;
        }

        void IDisposable.Dispose() { }

        #endregion // .NET Implementation of IEnumerator

        public virtual bool hasNext()
        {
            return index < size;
        }

        public virtual double next()
        {
            if (rowMajor)
            {
                subRow = index / submatrixStride;
                subCol = index % submatrixStride;
            }
            else
            {
                subRow = index % submatrixStride;
                subCol = index / submatrixStride;
            }
            index++;
            return a.get(subRow + minRow, subCol + minCol);
        }

        public virtual void remove()
        {
            throw new InvalidOperationException("Operation not supported");
        }

        /**
         * Which element in the submatrix was returned by next()
         *
         * @return Submatrix element's index.
         */
        public int getIndex()
        {
            return index - 1;
        }

        /**
         * True if it is iterating through the matrix by rows and false if by columns.
         * @return row major or column major
         */
        public bool isRowMajor()
        {
            return rowMajor;
        }

        /**
         * Sets the value of the current element.
         *
         * @param value The element's new value.
         */
        public void set(double value)
        {
            a.set(subRow + minRow, subCol + minCol, value);
        }
    }
}