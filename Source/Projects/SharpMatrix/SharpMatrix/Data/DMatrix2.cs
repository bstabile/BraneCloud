using System;
using System.Runtime.Serialization;
using SharpMatrix.Ops;

namespace SharpMatrix.Data
{
    /**
     * Fixed sized vector with 2 elements.  Can represent a 2 x 1 or 1 x 2 matrix, context dependent.
     * <p>DO NOT MODIFY.  Automatically generated code created by GenerateMatrixFixedN</p>
     *
     * @author Peter Abeles
     */
     [Serializable]
    public class DMatrix2 : DMatrixFixed
    {
        public double a1, a2;

        public DMatrix2()
        {
        }

        public DMatrix2(double a1, double a2)
        {
            this.a1 = a1;
            this.a2 = a2;
        }

        public DMatrix2(DMatrix2 o)
        {
            this.a1 = o.a1;
            this.a2 = o.a2;
        }

        public virtual double get(int row, int col)
        {
            return unsafe_get(row, col);
        }


        public virtual double unsafe_get(int row, int col)
        {
            if (row != 0 && col != 0)
                throw new ArgumentException("Row or column must be zero since this is a vector");

            int w = Math.Max(row, col);

            if (w == 0)
            {
                return a1;
            }
            else if (w == 1)
            {
                return a2;
            }
            else
            {
                throw new ArgumentException("Out of range.  " + w);
            }
        }


        public virtual void set(int row, int col, double val)
        {
            unsafe_set(row, col, val);
        }


        public virtual void unsafe_set(int row, int col, double val)
        {
            if (row != 0 && col != 0)
                throw new ArgumentException("Row or column must be zero since this is a vector");

            int w = Math.Max(row, col);

            if (w == 0)
            {
                a1 = val;
            }
            else if (w == 1)
            {
                a2 = val;
            }
            else
            {
                throw new ArgumentException("Out of range.  " + w);
            }
        }


        public virtual void set(Matrix original)
        {
            DMatrix m = (DMatrix) original;

            if (m.getNumCols() == 1 && m.getNumRows() == 2)
            {
                a1 = m.get(0, 0);
                a2 = m.get(1, 0);
            }
            else if (m.getNumRows() == 1 && m.getNumCols() == 2)
            {
                a1 = m.get(0, 0);
                a2 = m.get(0, 1);
            }
            else
            {
                throw new ArgumentException("Incompatible shape");
            }
        }


        public virtual int getNumRows()
        {
            return 2;
        }


        public virtual int getNumCols()
        {
            return 1;
        }


        public virtual int getNumElements()
        {
            return 2;
        }

        public virtual void print()
        {
            MatrixIO.print(Console.OpenStandardOutput(), this);
        }

        public virtual Matrix copy()
        {
            return new DMatrix2(this);
        }

        public virtual Matrix createLike()
        {
            return new DMatrix2();
        }


        public virtual MatrixType getType()
        {
            return MatrixType.UNSPECIFIED;
        }

    }
}
