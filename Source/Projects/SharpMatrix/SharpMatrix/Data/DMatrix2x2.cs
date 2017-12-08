using System;
using SharpMatrix.Ops;

namespace SharpMatrix.Data
{
    /**
     * Fixed sized 2 by DMatrix2x2 matrix.  The matrix is stored as class variables for very fast read/write.  aXY is the
     * value of row = X and column = Y.
     * <p>DO NOT MODIFY.  Automatically generated code created by GenerateMatrixFixedNxN</p>
     *
     * @author Peter Abeles
     */
    public class DMatrix2x2 : DMatrixFixed
    {

        public double a11, a12;
        public double a21, a22;

        public DMatrix2x2()
        {
        }

        public DMatrix2x2(double a11, double a12,
            double a21, double a22)
        {
            this.a11 = a11;
            this.a12 = a12;
            this.a21 = a21;
            this.a22 = a22;
        }

        public DMatrix2x2(DMatrix2x2 o)
        {
            this.a11 = o.a11;
            this.a12 = o.a12;
            this.a21 = o.a21;
            this.a22 = o.a22;
        }

        public virtual double get(int row, int col)
        {
            return unsafe_get(row, col);
        }

        public virtual double unsafe_get(int row, int col)
        {
            if (row == 0)
            {
                if (col == 0)
                {
                    return a11;
                }
                else if (col == 1)
                {
                    return a12;
                }
            }
            else if (row == 1)
            {
                if (col == 0)
                {
                    return a21;
                }
                else if (col == 1)
                {
                    return a22;
                }
            }
            throw new ArgumentException("Row and/or column output of range. " + row + " " + col);
        }

        public virtual void set(int row, int col, double val)
        {
            unsafe_set(row, col, val);
        }

        public virtual void unsafe_set(int row, int col, double val)
        {
            if (row == 0)
            {
                if (col == 0)
                {
                    a11 = val;
                    return;
                }
                else if (col == 1)
                {
                    a12 = val;
                    return;
                }
            }
            else if (row == 1)
            {
                if (col == 0)
                {
                    a21 = val;
                    return;
                }
                else if (col == 1)
                {
                    a22 = val;
                    return;
                }
            }
            throw new ArgumentException("Row and/or column output of range. " + row + " " + col);
        }

        public virtual void set(Matrix original)
        {
            if (original.getNumCols() != 2 || original.getNumRows() != 2)
                throw new ArgumentException("Rows and/or columns do not match");
            DMatrix m = (DMatrix) original;

            a11 = m.get(0, 0);
            a12 = m.get(0, 1);
            a21 = m.get(1, 0);
            a22 = m.get(1, 1);
        }


        public virtual int getNumRows()
        {
            return 2;
        }


        public virtual int getNumCols()
        {
            return 2;
        }

        public virtual int getNumElements()
        {
            return 4;
        }

        public virtual Matrix copy()
        {
            return new DMatrix2x2(this);
        }


        public virtual void print()
        {
            MatrixIO.print(Console.OpenStandardOutput(), this);
        }

        public virtual Matrix createLike()
        {
            return new DMatrix2x2();
        }


        public virtual MatrixType getType()
        {
            return MatrixType.UNSPECIFIED;
        }
    }

}