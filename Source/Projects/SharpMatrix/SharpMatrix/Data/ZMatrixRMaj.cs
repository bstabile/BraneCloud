
using System;
using SharpMatrix.Ops;

namespace SharpMatrix.Data
{
    /**
     * Dense matrix for complex numbers.  Internally it stores its data in a single row-major array with the real
     * and imaginary components interlaces, in that order.  The total number of elements in the array will be
     * numRows*numColumns*2.
     *
     * @author Peter Abeles
     */
    public class ZMatrixRMaj : ZMatrixD1
    {

        /**
         * <p>
         * Creates a matrix with the values and shape defined by the 2D array 'data'.
         * It is assumed that 'data' has a row-major formatting:<br>
         *  <br>
         * data[ row ][ column ]
         * </p>
         * @param data 2D array representation of the matrix. Not modified.
         */
        public ZMatrixRMaj(double[][] data)
        {

            this.numRows = data.Length;
            this.numCols = data[0].Length / 2;

            this.data = new double[numRows * numCols * 2];

            for (int i = 0; i < numRows; i++)
            {
                double[] row = data[i];
                if (row.Length != numCols * 2)
                    throw new ArgumentException("Unexpected row size in input data at row " + i);

                Array.Copy(row, 0, this.data, i * numCols * 2, row.Length);
            }
        }

        public ZMatrixRMaj(int numRows, int numCols, bool rowMajor, params double[] data)
        {
            if (data.Length != numRows * numCols * 2)
                throw new InvalidOperationException("Unexpected length for data");

            this.data = new double[numRows * numCols * 2];

            this.numRows = numRows;
            this.numCols = numCols;

            set(numRows, numCols, rowMajor, data);
        }

        /**
         * Creates a new {@link ZMatrixRMaj} which is a copy of the passed in matrix.
         * @param original Matrix which is to be copied
         */
        public ZMatrixRMaj(ZMatrixRMaj original)
            : this(original.numRows, original.numCols)
        {
            set(original);
        }

        /**
         * Creates a new matrix with the specified number of rows and columns
         *
         * @param numRows number of rows
         * @param numCols number of columns
         */
        public ZMatrixRMaj(int numRows, int numCols)
        {
            this.numRows = numRows;
            this.numCols = numCols;
            this.data = new double[numRows * numCols * 2];
        }

        public override int getIndex(int row, int col)
        {
            return row * numCols * 2 + col * 2;
        }

        public override void reshape(int numRows, int numCols)
        {
            int newLength = numRows * numCols * 2;

            if (newLength > data.Length)
            {
                data = new double[newLength];
            }

            this.numRows = numRows;
            this.numCols = numCols;
        }

        public override void get(int row, int col, Complex_F64 output)
        {
            int index = row * numCols * 2 + col * 2;
            output.real = data[index];
            output.imaginary = data[index + 1];
        }

        public override void set(int row, int col, double real, double imaginary)
        {
            int index = row * numCols * 2 + col * 2;
            data[index] = real;
            data[index + 1] = imaginary;
        }

        public override double getReal(int row, int col)
        {
            return data[(row * numCols + col) * 2];
        }

        public override void setReal(int row, int col, double val)
        {
            data[(row * numCols + col) * 2] = val;
        }

        public override double getImag(int row, int col)
        {
            return data[(row * numCols + col) * 2 + 1];
        }

        public override void setImag(int row, int col, double val)
        {
            data[(row * numCols + col) * 2 + 1] = val;
        }

        public override int getDataLength()
        {
            return numRows * numCols * 2;
        }

        public void set(ZMatrixRMaj original)
        {
            reshape(original.numRows, original.numCols);
            int columnSize = numCols * 2;
            for (int y = 0; y < numRows; y++)
            {
                int index = y * numCols * 2;
                Array.Copy(original.data, index, data, index, columnSize);
            }
        }

        public override void set(Matrix original)
        {
            reshape(original.getNumRows(), original.getNumCols());

            ZMatrix n = (ZMatrix) original;

            Complex_F64 c = new Complex_F64();
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    n.get(i, j, c);
                    set(i, j, c.real, c.imaginary);
                }
            }
        }

        public override void print()
        {
            MatrixIO.print(Console.OpenStandardOutput(), this);
        }

        /**
         * Number of array elements in the matrix's row.
         */
        public int getRowStride()
        {
            return numCols * 2;
        }

        /**
         * Sets this matrix equal to the matrix encoded in the array.
         *
         * @param numRows The number of rows.
         * @param numCols The number of columns.
         * @param rowMajor If the array is encoded in a row-major or a column-major format.
         * @param data The formatted 1D array. Not modified.
         */
        public void set(int numrows, int numcols, bool rowMajor, params double[] arr)
        {
            reshape(numrows, numcols);
            int length = numrows * numcols * 2;

            if (length > arr.Length)
                throw new InvalidOperationException("Passed in array not long enough");

            if (rowMajor)
            {
                Array.Copy(arr, 0, this.data, 0, length);
            }
            else
            {
                int index = 0;
                int stride = numrows * 2;
                for (int i = 0; i < numrows; i++)
                {
                    for (int j = 0; j < numcols; j++)
                    {
                        data[index++] = arr[j * stride + i * 2];
                        data[index++] = arr[j * stride + i * 2 + 1];
                    }
                }
            }
        }

        /**
         * Sets all the elements in the matrix to zero
         */
        public void zero()
        {
            Array.Clear(data, 0, numCols * numRows * 2);
        }

        public override Matrix copy()
        {
            return new ZMatrixRMaj(this);
        }

        public override Matrix createLike()
        {
            return new ZMatrixRMaj(numRows, numCols);
        }

        public override MatrixType getType()
        {
            return MatrixType.ZDRM;
        }
    }
}