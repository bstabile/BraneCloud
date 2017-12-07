using System;
using BraneCloud.Evolution.EC.MatrixLib.Ops;

namespace BraneCloud.Evolution.EC.MatrixLib.Data
{
    /**
     * <p>Compressed Column (CC) sparse matrix format.   Only non-zero elements are stored.</p>
     * <p>
     * Format:<br>
     * Row indexes for column j are stored in rol_idx[col_idx[j]] to rol_idx[col_idx[j+1]-1].  The values
     * for the corresponding elements are stored at data[col_idx[j]] to data[col_idx[j+1]-1].<br>
     * <br>
     * Row indexes must be specified in chronological order.
     * </p>
     *
     *
     * TODO fully describe
     *
     * @author Peter Abeles
     */
    public class DMatrixSparseCSC : DMatrixSparse
    {
        /**
         * Storage for non-zero values.  Only valid up to length-1.
         */
        public double[] nz_values;

        /**
         * Length of data. Number of non-zero values in the matrix
         */
        public int nz_length;

        /**
         * Specifies which row a specific non-zero value corresponds to.  If they are sorted or not with in each column
         * is specified by the {@link #indicesSorted} flag.
         */
        public int[] nz_rows;

        /**
         * Stores the range of indexes in the non-zero lists that belong to each column.  Column 'i' corresponds to
         * indexes col_idx[i] to col_idx[i+1]-1, inclusive.
         */
        public int[] col_idx;

        /**
         * Number of rows in the matrix
         */
        public int numRows;

        /**
         * Number of columns in the matrix
         */
        public int numCols;

        /**
         * Flag that's used to indicate of the row indices are sorted or not.
         */
        public bool indicesSorted = false;

        /**
         * Constructor with a default arrayLength of zero.
         *
         * @param numRows Number of rows
         * @param numCols Number of columns
         */
        public DMatrixSparseCSC(int numRows, int numCols)
            : this(numRows, numCols, 0)
        {
        }

        /**
         * Specifies shape and number of non-zero elements that can be stored.
         *
         * @param numRows Number of rows
         * @param numCols Number of columns
         * @param arrayLength Initial maximum number of non-zero elements that can be in the matrix
         */
        public DMatrixSparseCSC(int numRows, int numCols, int arrayLength)
        {
            arrayLength = Math.Min(numCols * numRows, arrayLength);

            this.numRows = numRows;
            this.numCols = numCols;
            this.nz_length = 0;

            nz_values = new double[arrayLength];
            col_idx = new int[numCols + 1];
            nz_rows = new int[arrayLength];
        }

        public DMatrixSparseCSC(DMatrixSparseCSC original)
            : this(original.numRows, original.numCols, original.nz_length)
        {

            set(original);
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
            return new DMatrixSparseCSC(this);
        }

        public virtual Matrix createLike()
        {
            return new DMatrixSparseCSC(numRows, numCols);
        }

        public virtual void set(Matrix original)
        {
            DMatrixSparseCSC o = (DMatrixSparseCSC) original;
            reshape(o.numRows, o.numCols, o.nz_length);
            this.nz_length = o.nz_length;

            Array.Copy(o.nz_values, 0, nz_values, 0, nz_length);
            Array.Copy(o.nz_rows, 0, nz_rows, 0, nz_length);
            Array.Copy(o.col_idx, 0, col_idx, 0, numCols + 1);
            this.indicesSorted = o.indicesSorted;
        }

        public virtual void print()
        {
            Console.WriteLine(GetType().Name + "\nnumRows = " + numRows + " , numCols = " + numCols
                              + " , nz_length = " + nz_length);
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    int index = nz_index(row, col);
                    if (index >= 0)
                        Console.Write("{0:F3, 6}", get(row, col));
                    else
                    Console.Write("   *  ");
                    if (col != numCols - 1)
                        Console.Write(" ");
                }
                Console.WriteLine();
            }
        }

        public virtual void printNonZero()
        {
            Console.WriteLine(GetType().Name + "\nnumRows = " + numRows + " , numCols = " + numCols
                               + " , nz_length = " + nz_length);

            for (int col = 0; col < numCols; col++)
            {
                int idx0 = col_idx[col];
                int idx1 = col_idx[col + 1];

                for (int i = idx0; i < idx1; i++)
                {
                    int row = nz_rows[i];
                    double value = nz_values[i];

                    Console.WriteLine("{0} {1} {2:F}\n", row, col, value);
                }
            }
        }

        public virtual bool isAssigned(int row, int col)
        {
            return nz_index(row, col) >= 0;
        }

        public virtual double get(int row, int col)
        {
            if (row < 0 || row >= numRows || col < 0 || col >= numCols)
                throw new ArgumentException("Outside of matrix bounds");

            return unsafe_get(row, col);
        }

        public virtual double unsafe_get(int row, int col)
        {
            int index = nz_index(row, col);
            if (index >= 0)
                return nz_values[index];
            return 0;
        }

        /**
         * Returns the index in nz_rows for the element at (row,col) if it already exists in the matrix. If not then -1
         * is returned.
         * @param row row coordinate
         * @param col column coordinate
         * @return nz_row index or -1 if the element does not exist
         */
        public int nz_index(int row, int col)
        {
            int col0 = col_idx[col];
            int col1 = col_idx[col + 1];

            for (int i = col0; i < col1; i++)
            {
                if (nz_rows[i] == row)
                {
                    return i;
                }
            }
            return -1;
        }

        public virtual void set(int row, int col, double val)
        {
            if (row < 0 || row >= numRows || col < 0 || col >= numCols)
                throw new ArgumentException("Outside of matrix bounds");

            unsafe_set(row, col, val);
        }

        public virtual void unsafe_set(int row, int col, double val)
        {
            int index = nz_index(row, col);
            if (index >= 0)
            {
                nz_values[index] = val;
            }
            else
            {

                int idx0 = col_idx[col];
                int idx1 = col_idx[col + 1];

                // determine the index the new element should be inserted at. This is done to keep it sorted if
                // it was already sorted
                for (index = idx0; index < idx1; index++)
                {
                    if (row < nz_rows[index])
                    {
                        break;
                    }
                }

                // shift all the col_idx after this point by 1
                for (int i = col + 1; i <= numCols; i++)
                {
                    col_idx[i]++;
                }

                // if it's already at the maximum array length grow the arrays
                if (nz_length >= nz_values.Length)
                    growMaxLength(nz_length * 2 + 1, true);

                // shift everything by one
                for (int i = nz_length; i > index; i--)
                {
                    nz_rows[i] = nz_rows[i - 1];
                    nz_values[i] = nz_values[i - 1];
                }
                nz_rows[index] = row;
                nz_values[index] = val;
                nz_length++;
            }
        }

        public virtual void remove(int row, int col)
        {
            int index = nz_index(row, col);

            if (index < 0) // it's not in the nz structure
                return;

            // shift all the col_idx after this point by -1
            for (int i = col + 1; i <= numCols; i++)
            {
                col_idx[i]--;
            }

            nz_length--;
            for (int i = index; i < nz_length; i++)
            {
                nz_rows[i] = nz_rows[i + 1];
                nz_values[i] = nz_values[i + 1];
            }
        }

        public virtual void zero()
        {
            Array.Clear(col_idx, 0, numCols + 1);
            nz_length = 0;
            indicesSorted = false; // see justification in reshape
        }

        public virtual int getNumElements()
        {
            return nz_length;
        }

        public virtual void reshape(int numRows, int numCols, int arrayLength)
        {
            // OK so technically it is sorted, but forgetting to correctly set this flag is a common mistake so
            // decided to be conservative and mark it as unsorted so that stuff doesn't blow up
            this.indicesSorted = false;
            this.numRows = numRows;
            this.numCols = numCols;
            growMaxLength(arrayLength, false);
            this.nz_length = 0;

            if (numCols + 1 > col_idx.Length)
            {
                col_idx = new int[numCols + 1];
            }
            else
            {
                Array.Clear(col_idx, 0, numCols + 1);
            }
        }

        public virtual void shrinkArrays()
        {
            if (nz_length < nz_values.Length)
            {
                double[] tmp_values = new double[nz_length];
                int[] tmp_rows = new int[nz_length];

                Array.Copy(this.nz_values, 0, tmp_values, 0, nz_length);
                Array.Copy(this.nz_rows, 0, tmp_rows, 0, nz_length);

                this.nz_values = tmp_values;
                this.nz_rows = tmp_rows;
            }
        }

        /**
         * Increases the maximum size of the data array so that it can store sparse data up to 'length'.  The class
         * parameter nz_length is not modified by this function call.
         *
         * @param arrayLength Desired maximum length of sparse data
         * @param preserveValue If true the old values will be copied into the new arrays.  If false that step will be skipped.
         */
        public void growMaxLength(int arrayLength, bool preserveValue)
        {
            // don't increase the size beyound the max possible matrix size
            arrayLength = Math.Min(numRows * numCols, arrayLength);
            if (arrayLength > this.nz_values.Length)
            {
                double[] data = new double[arrayLength];
                int[] row_idx = new int[arrayLength];

                if (preserveValue)
                {
                    Array.Copy(this.nz_values, 0, data, 0, this.nz_length);
                    Array.Copy(this.nz_rows, 0, row_idx, 0, this.nz_length);
                }

                this.nz_values = data;
                this.nz_rows = row_idx;
            }
        }

        /**
         * Increases the maximum number of columns in the matrix.
         * @param desiredColumns Desired number of columns.
         * @param preserveValue If the array needs to be expanded should it copy the previous values?
         */
        public void growMaxColumns(int desiredColumns, bool preserveValue)
        {
            if (col_idx.Length < desiredColumns + 1)
            {
                int[] c = new int[desiredColumns + 1];
                if (preserveValue)
                    Array.Copy(col_idx, 0, c, 0, col_idx.Length);
                col_idx = c;
            }
        }

        /**
         * Given the histogram of columns compute the col_idx for the matrix.  Then overwrite histogram with
         * those values.  nz_length is automatically set and nz_values will grow if needed.
         * @param histogram histogram of column values in the sparse matrix. modified, see above.
         */
        public void colsum(int[] histogram)
        {
            col_idx[0] = 0;
            int index = 0;
            for (int i = 1; i <= numCols; i++)
            {
                col_idx[i] = index += histogram[i - 1];
            }
            Array.Copy(col_idx, 0, histogram, 0, numCols); // TODO move this outside?
            nz_length = index;
            growMaxLength(nz_length, false);
            if (col_idx[numCols] != nz_length)
                throw new InvalidOperationException("Egads");
        }

        /**
         * Sorts the row indices in ascending order.
         * @param sorter (Optional) Used to sort rows.  If null a new instance will be declared internally.
         */
        public void sortIndices(SortCoupledArray_F64 sorter)
        {
            if (sorter == null)
                sorter = new SortCoupledArray_F64();

            sorter.quick(col_idx, numCols + 1, nz_rows, nz_values);
            indicesSorted = true;
        }

        /**
         * Copies the non-zero structure of orig into "this"
         * @param orig Matrix who's structure is to be copied
         */
        public void copyStructure(DMatrixSparseCSC orig)
        {
            reshape(orig.numRows, orig.numCols, orig.nz_length);
            this.nz_length = orig.nz_length;
            Array.Copy(orig.col_idx, 0, col_idx, 0, orig.numCols + 1);
            Array.Copy(orig.nz_rows, 0, nz_rows, 0, orig.nz_length);
        }

        /**
         * If the indices has been sorted or not
         * @return true if sorted or false if not sorted
         */
        public bool isIndicesSorted()
        {
            return indicesSorted;
        }

        /**
         * Returns true if number of non-zero elements is the maximum size
         * @return true if no more non-zero elements can be added
         */
        public bool isFull()
        {
            return nz_length == numRows * numCols;
        }

        public virtual MatrixType getType()
        {
            return MatrixType.DSCC;
        }
    }
}