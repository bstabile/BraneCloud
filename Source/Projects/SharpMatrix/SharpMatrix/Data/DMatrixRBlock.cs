using System;
using BraneCloud.Evolution.EC.MatrixLib.Ops;

namespace BraneCloud.Evolution.EC.MatrixLib.Data
{
    /**
     * A row-major block matrix declared on to one continuous array.
     *
     * @author Peter Abeles
     */
    public class DMatrixRBlock : DMatrixD1
    {
        public int blockLength;

        public DMatrixRBlock(int numRows, int numCols, int blockLength)
        {
            this.data = new double[numRows * numCols];
            this.blockLength = blockLength;
            this.numRows = numRows;
            this.numCols = numCols;
        }

        public DMatrixRBlock(int numRows, int numCols)
            : this(numRows, numCols, EjmlParameters.BLOCK_WIDTH)
        {
        }

        public DMatrixRBlock()
        {
        }

        public void set(DMatrixRBlock A)
        {
            this.blockLength = A.blockLength;
            this.numRows = A.numRows;
            this.numCols = A.numCols;

            int N = numCols * numRows;

            if (data.Length < N)
                data = new double[N];

            Array.Copy(A.data, 0, data, 0, N);
        }

        public static DMatrixRBlock wrap(double[] data, int numRows, int numCols, int blockLength )
        {
            DMatrixRBlock ret = new DMatrixRBlock();
            ret.data = data;
            ret.numRows = numRows;
            ret.numCols = numCols;
            ret.blockLength = blockLength;

            return ret;
        }

        public override double[] getData()
        {
            return data;
        }

        public override void reshape(int numRows, int numCols, bool saveValues)
        {
            if (numRows * numCols <= data.Length)
            {
                this.numRows = numRows;
                this.numCols = numCols;
            }
            else
            {
                double[] data = new double[numRows * numCols];

                if (saveValues)
                {
                    Array.Copy(this.data, 0, data, 0, getNumElements());
                }

                this.numRows = numRows;
                this.numCols = numCols;
                this.data = data;
            }
        }

        public void reshape(int numRows, int numCols, int blockLength, bool saveValues)
        {
            this.blockLength = blockLength;
            this.reshape(numRows, numCols, saveValues);
        }

        public override int getIndex(int row, int col)
        {
            // find the block it is inside
            int blockRow = row / blockLength;
            int blockCol = col / blockLength;

            int localHeight = Math.Min(numRows - blockRow * blockLength, blockLength);

            int index = blockRow * blockLength * numCols + blockCol * localHeight * blockLength;

            int localLength = Math.Min(numCols - blockLength * blockCol, blockLength);

            row = row % blockLength;
            col = col % blockLength;

            return index + localLength * row + col;
        }

        public override double get(int row, int col)
        {
            return data[getIndex(row, col)];
        }

        public override double unsafe_get(int row, int col)
        {
            return data[getIndex(row, col)];
        }

        public override void set(int row, int col, double val)
        {
            data[getIndex(row, col)] = val;
        }

        public override void unsafe_set(int row, int col, double val)
        {
            data[getIndex(row, col)] = val;
        }

        public override int getNumRows()
        {
            return numRows;
        }

        public override int getNumCols()
        {
            return numCols;
        }

        public override Matrix createLike()
        {
            return new DMatrixRBlock(numRows, numCols);
        }

        public override void set(Matrix original)
        {
            if (original is DMatrixRBlock) {
                set((DMatrixRBlock) original);
            } else {
                DMatrix m = (DMatrix) original;

                for (int i = 0; i < numRows; i++)
                {
                    for (int j = 0; j < numCols; j++)
                    {
                        set(i, j, m.get(i, j));
                    }
                }
            }
        }

        public override int getNumElements()
        {
            return numRows * numCols;
        }

        public override void print()
        {
            MatrixIO.print(Console.OpenStandardOutput(), this);
        }

        public override Matrix copy()
        {
            DMatrixRBlock A = new DMatrixRBlock(numRows, numCols, blockLength);
            A.set(this);
            return A;
        }

        public override MatrixType getType()
        {
            return MatrixType.UNSPECIFIED;
        }

    }
}