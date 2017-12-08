using System;

namespace SharpMatrix.Data
{
    /**
     * TODO describe
     *
     * @author Peter Abeles
     */
    public class DMatrixSparseTriplet : DMatrixSparse
    {
        public Element[] nz_data = new Element[0];
        public int nz_length;
        public int numRows;
        public int numCols;

        public DMatrixSparseTriplet()
        {
        }

        /**
         *
         * @param numRows Number of rows in the matrix
         * @param numCols Number of columns in the matrix
         * @param initLength Initial maximum length of data array.
         */
        public DMatrixSparseTriplet(int numRows, int numCols, int initLength)
        {
            growData(initLength);
            this.numRows = numRows;
            this.numCols = numCols;
        }

        public DMatrixSparseTriplet(DMatrixSparseTriplet orig)
        {
            set(orig);
        }

        public void reset()
        {
            nz_length = 0;
            numRows = 0;
            numCols = 0;
        }

        public void reshape(int numRows, int numCols)
        {
            this.numRows = numRows;
            this.numCols = numCols;
            this.nz_length = 0;
        }

        public virtual void reshape(int numRows, int numCols, int arrayLength)
        {
            reshape(numRows, numCols);
            growData(arrayLength);
        }

        public void addItem(int row, int col, double value)
        {
            if (nz_length == nz_data.Length)
            {
                growData((nz_length * 2 + 10));
            }
            nz_data[nz_length++].set(row, col, value);
        }

        public virtual void set(int row, int col, double value)
        {
            if (row < 0 || row >= numRows || col < 0 || col >= numCols)
                throw new ArgumentException("Outside of matrix bounds");

            unsafe_set(row, col, value);
        }

        public virtual void unsafe_set(int row, int col, double value)
        {
            int index = nz_index(row, col);
            if (index < 0)
                addItem(row, col, value);
            else
                nz_data[index].value = value;
        }

        public virtual int getNumElements()
        {
            return nz_length;
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
            if (index < 0)
                return 0;
            else
                return nz_data[index].value;
        }

        public int nz_index(int row, int col)
        {
            for (int i = 0; i < nz_length; i++)
            {
                Element e = nz_data[i];
                if (e.row == row && e.col == col)
                    return i;
            }
            return -1;
        }

        /**
         * Will resize the array and keep all the old data
         * @param max_nz_length New maximum length of data
         */
        public void growData(int max_nz_length)
        {
            if (nz_data.Length < max_nz_length)
            {
                Element[] tmp = new Element[max_nz_length];
                Array.Copy(nz_data, 0, tmp, 0, nz_data.Length);
                for (int i = nz_data.Length; i < max_nz_length; i++)
                {
                    tmp[i] = new Element();
                }
                nz_data = tmp;
            }
        }

        public int getLength()
        {
            return nz_length;
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
            return new DMatrixSparseTriplet(this);
        }

        public virtual Matrix createLike()
        {
            return new DMatrixSparseTriplet(numRows, numCols, nz_length);
        }

        public virtual void set(Matrix original)
        {
            DMatrixSparseTriplet orig = (DMatrixSparseTriplet) original;
            reshape(orig.numRows, orig.numCols);
            growData(orig.nz_length);

            this.nz_length = orig.nz_length;
            for (int i = 0; i < nz_length; i++)
            {
                nz_data[i].set(orig.nz_data[i]);
            }
        }

        public virtual void shrinkArrays()
        {
            if (nz_length < nz_data.Length)
            {
                Element[] tmp_data = new Element[nz_length];

                Array.Copy(this.nz_data, 0, tmp_data, 0, nz_length);

                this.nz_data = tmp_data;
            }
        }

        public virtual void remove(int row, int col)
        {
            int where = nz_index(row, col);
            if (where >= 0)
            {
                Element e = nz_data[where];
                nz_length -= 1;
                for (int i = where; i < nz_length; i++)
                {
                    nz_data[i] = nz_data[i + 1];
                }
                nz_data[nz_length] = e;
            }
        }

        public virtual bool isAssigned(int row, int col)
        {
            return nz_index(row, col) >= 0;
        }

        public virtual void zero()
        {
            nz_length = 0;
        }

        public virtual void print()
        {
            Console.WriteLine(GetType().Name + "\n , numRows = " + numRows + " , numCols = " + numCols
                              + " , nz_length = " + nz_length);
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    int index = nz_index(row, col);
                    if (index >= 0)
                        Console.Write("{0:F3, 6}", nz_data[index].value);
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
            Console.WriteLine(GetType().Name + "\n , numRows = " + numRows + " , numCols = " + numCols
                              + " , nz_length = " + nz_length);

            for (int i = 0; i < nz_length; i++)
            {
                Element e = nz_data[i];
                Console.Write("{0} {1} {2:F3, 6}\n", e.row, e.col, e.value);
            }
        }

        public class Element
        {
            public int row, col;
            public double value;

            public void set(int row, int col, double value)
            {
                this.row = row;
                this.col = col;
                this.value = value;
            }

            public void set(Element e)
            {
                row = e.row;
                col = e.col;
                value = e.value;
            }
        }

        public virtual MatrixType getType()
        {
            return MatrixType.UNSPECIFIED;
        }
    }
}