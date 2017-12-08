using System;
using SharpMatrix.Ops;

namespace SharpMatrix.Data
{
    //package org.ejml.data;

/**
 * Dense matrix for complex numbers.  Internally it stores its data in a single row-major array with the real
 * and imaginary components interlaces, in that order.  The total number of elements in the array will be
 * numRows*numColumns*2.
 *
 * @author Peter Abeles
 */
    public class CMatrixRMaj : CMatrixD1
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
        public CMatrixRMaj(float[][] data)
        {

            this.numRows = data.Length;
            this.numCols = data[0].Length / 2;

            this.data = new float[numRows * numCols * 2];

            for (int i = 0; i < numRows; i++)
            {
                float[] row = data[i];
                if (row.Length != numCols * 2)
                    throw new ArgumentException("Unexpected row size in input data at row " + i);

                Array.Copy(row, 0, this.data, i * numCols * 2, row.Length);
            }
        }

        public CMatrixRMaj(int numRows, int numCols, bool rowMajor, float[] data)
        {
            if (data.Length != numRows * numCols * 2)
                throw new InvalidOperationException("Unexpected length for data");

            this.data = new float[numRows * numCols * 2];

            this.numRows = numRows;
            this.numCols = numCols;

            set(numRows, numCols, rowMajor, data);
        }

        /**
         * Creates a new {@link CMatrixRMaj} which is a copy of the passed in matrix.
         * @param original Matrix which is to be copied
         */
        public CMatrixRMaj(CMatrixRMaj original)
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
        public CMatrixRMaj(int numRows, int numCols)
        {
            this.numRows = numRows;
            this.numCols = numCols;
            this.data = new float[numRows * numCols * 2];
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
                data = new float[newLength];
            }

            this.numRows = numRows;
            this.numCols = numCols;
        }

        public override void get(int row, int col, Complex_F32 output)
        {
            int index = row * numCols * 2 + col * 2;
            output.real = data[index];
            output.imaginary = data[index + 1];
        }

        public override void set(int row, int col, float real, float imaginary)
        {
            int index = row * numCols * 2 + col * 2;
            data[index] = real;
            data[index + 1] = imaginary;
        }

        public override float getReal(int row, int col)
        {
            return data[(row * numCols + col) * 2];
        }

        public override void setReal(int row, int col, float val)
        {
            data[(row * numCols + col) * 2] = val;
        }

        public override float getImag(int row, int col)
        {
            return data[(row * numCols + col) * 2 + 1];
        }

        public override void setImag(int row, int col, float val)
        {
            data[(row * numCols + col) * 2 + 1] = val;
        }

        public override int getDataLength()
        {
            return numRows * numCols * 2;
        }

        public void set(CMatrixRMaj original)
        {
            reshape(original.numRows, original.numCols);
            int columnSize = numCols * 2;
            for (int y = 0; y < numRows; y++)
            {
                int index = y * numCols * 2;
                Array.Copy(original.data, index, data, index, columnSize);
            }
        }

        public override Matrix copy()
        {
            return new CMatrixRMaj(this);
        }

        public override void set(Matrix original)
        {
            reshape(original.getNumRows(), original.getNumCols());

            CMatrix n = (CMatrix) original;

            Complex_F32 c = new Complex_F32();
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
        public void set(int numRows, int numCols, bool rowMajor, float[] data)
        {
            reshape(numRows, numCols);
            int length = numRows * numCols * 2;

            if (length > data.Length)
                throw new InvalidOperationException("Passed in array not long enough");

            if (rowMajor)
            {
                Array.Copy(data, 0, this.data, 0, length);
            }
            else
            {
                int index = 0;
                int stride = numRows * 2;
                for (int i = 0; i < numRows; i++)
                {
                    for (int j = 0; j < numCols; j++)
                    {
                        this.data[index++] = data[j * stride + i * 2];
                        this.data[index++] = data[j * stride + i * 2 + 1];
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

        public override Matrix createLike()
        {
            return new CMatrixRMaj(numRows, numCols);
        }

        public override MatrixType getType()
        {
            return MatrixType.CDRM;
        }
    }
}