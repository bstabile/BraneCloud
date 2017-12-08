using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row;

namespace SharpMatrix.Simple
{
    public abstract class SimpleMatrixBase<TData, TMatrix, TSimple> : IEnumerable<TData>
        where TData : struct
        where TMatrix : class, Matrix, new()
        where TSimple : ISimpleMatrix<TData, TMatrix, TSimple>, new()
    {
        /**
         * Internal matrix which this is a wrapper around.
         */
        protected TMatrix mat;

        protected SimpleOperations<TMatrix> ops;

        #region Protected Methods

        protected abstract void setMatrix(TMatrix mat);

        protected abstract TSimple wrapMatrix(TMatrix m);

        protected abstract TSimple createMatrix(int numRows, int numCols);

        #endregion // Protected Methods

        #region Public Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<TData> GetEnumerator();

        /// <summary>
        /// Returns the type of matrix is is wrapping.
        /// </summary>
        public MatrixType getType()
        {
            return mat.getType();
        }

        /// <summary>
        /// Size of internal array elements.  32 or 64 bits
        /// </summary>
        public int bits()
        {
            return mat.getType().getBits();
        }

        /// <summary>
        /// Returns true if this matrix is a vector.  
        /// A vector is defined as a matrix that has either one row or column.
        /// </summary>
        public bool isVector()
        {
            return mat.getNumRows() == 1 || mat.getNumCols() == 1;
        }

        /// <summary>
        /// Returns the number of rows in this matrix.
        /// </summary>
        public int numRows()
        {
            return mat.getNumRows();
        }

        /// <summary>
        /// Returns the number of columns in this matrix.
        /// </summary>
        public int numCols()
        {
            return mat.getNumCols();
        }

        /// <summary>
        /// Returns the number of elements in this matrix, which is 
        /// equal to the number of rows times the number of columns.
        /// </summary>
        public int getNumElements()
        {
            return mat.getNumRows() * mat.getNumCols();
        }

        /// <summary>
        /// Returns a reference to the matrix that it uses internally.  This is useful
        /// when an operation is needed that is not provided by this class.
        /// </summary>
        public TMatrix getMatrix()
        {
            return mat;
        }

        /// <summary>
        /// Returns the value of the specified matrix element.  
        /// Performs a bounds check to make sure the 
        /// requested element is part of the matrix.
        /// </summary>
        public abstract TData get(int row, int col);

        /// <summary>
        /// Returns the value of the matrix at the specified index of the 1D row major array.
        /// </summary>
        public abstract TData get(int index);

        /// <summary>
        /// Returns the index in the matrix's array.
        /// </summary>
        public int getIndex(int row, int col)
        {
            return row * mat.getNumCols() + col;
        }

        /// <summary>
        /// Sets all the elements in the matrix equal to zero.
        /// </summary>
        /// <see cref="CommonOps_DDRM.fill(DMatrixRMaj, double)"/>
        public abstract void zero();

        /// <summary>
        /// Sets all the elements in this matrix equal to the specified value.
        /// </summary>
        /// <see cref="CommonOps_DDRM.fill(DMatrixD1, double)"/>
        public abstract void set(TData val);

        /// <summary>
        /// Sets the elements in this matrix to be equal to the elements in the passed in matrix.
        /// Both matrix must have the same dimension.
        /// </summary>
        public void set(TSimple a)
        {
            mat.set(a.getMatrix());
        }

        /// <summary>
        /// Assigns the element in the Matrix to the specified value.  
        /// Performs a bounds check to make sure the requested element is part of the matrix.
        /// </summary>
        public abstract void set(int row, int col, TData value);

        /// <summary>
        /// Assigns an element a value based on its index in the internal array.
        /// </summary>
        public abstract void set(int index, TData value);

        /// <summary>
        /// Assigns consecutive elements inside a row to the provided array.
        /// </summary>
        public abstract void setRow(int row, int startColumn, TData[] values);

        #endregion // Public Methods

    }
}
