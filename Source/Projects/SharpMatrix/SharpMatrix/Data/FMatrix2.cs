using System;
using SharpMatrix.Ops;

namespace SharpMatrix.Data
{
    //package org.ejml.data;

/**
 * Fixed sized vector with 2 elements.  Can represent a 2 x 1 or 1 x 2 matrix, context dependent.
 * <p>DO NOT MODIFY.  Automatically generated code created by GenerateMatrixFixedN</p>
 *
 * @author Peter Abeles
 */
    public class FMatrix2 : FMatrixFixed
    {
        public float a1, a2;

        public FMatrix2()
        {
        }

        public FMatrix2(float a1, float a2)
        {
            this.a1 = a1;
            this.a2 = a2;
        }

        public FMatrix2(FMatrix2 o)
        {
            this.a1 = o.a1;
            this.a2 = o.a2;
        }

        public virtual float get(int row, int col)
        {
            return unsafe_get(row, col);
        }

        public virtual float unsafe_get(int row, int col)
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

        public virtual void set(int row, int col, float val)
        {
            unsafe_set(row, col, val);
        }

        public virtual void unsafe_set(int row, int col, float val)
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
            FMatrix m = (FMatrix) original;

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

        public virtual Matrix copy()
        {
            return new FMatrix2(this);
        }

        public virtual void print()
        {
            MatrixIO.print(Console.OpenStandardOutput(), this);
        }

        public virtual Matrix createLike()
        {
            return new FMatrix2();
        }

        public virtual MatrixType getType()
        {
            return MatrixType.UNSPECIFIED;
        }
    }
}
