using System;
using SharpMatrix.Ops;

namespace SharpMatrix.Data
{
    //package org.ejml.data;

/**
 * Fixed sized 2 by FMatrix2x2 matrix.  The matrix is stored as class variables for very fast read/write.  aXY is the
 * value of row = X and column = Y.
 * <p>DO NOT MODIFY.  Automatically generated code created by GenerateMatrixFixedNxN</p>
 *
 * @author Peter Abeles
 */
    public class FMatrix2x2 : FMatrixFixed
    {

        public float a11, a12;
        public float a21, a22;

        public FMatrix2x2()
        {
        }

        public FMatrix2x2(float a11, float a12,
            float a21, float a22)
        {
            this.a11 = a11;
            this.a12 = a12;
            this.a21 = a21;
            this.a22 = a22;
        }

        public FMatrix2x2(FMatrix2x2 o)
        {
            this.a11 = o.a11;
            this.a12 = o.a12;
            this.a21 = o.a21;
            this.a22 = o.a22;
        }

        public virtual float get(int row, int col)
        {
            return unsafe_get(row, col);
        }

        public virtual float unsafe_get(int row, int col)
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
            throw new ArgumentException("Row and/or column out of range. " + row + " " + col);
        }

        public virtual void set(int row, int col, float val)
        {
            unsafe_set(row, col, val);
        }

        public virtual void unsafe_set(int row, int col, float val)
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
            throw new ArgumentException("Row and/or column out of range. " + row + " " + col);
        }

        public virtual void set(Matrix original)
        {
            if (original.getNumCols() != 2 || original.getNumRows() != 2)
                throw new ArgumentException("Rows and/or columns do not match");
            FMatrix m = (FMatrix) original;

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
            return new FMatrix2x2(this);
        }

        public virtual void print()
        {
            MatrixIO.print(Console.OpenStandardOutput(), this);
        }

        public virtual Matrix createLike()
        {
            return new FMatrix2x2();
        }

        public virtual MatrixType getType()
        {
            return MatrixType.UNSPECIFIED;
        }
    }
}
