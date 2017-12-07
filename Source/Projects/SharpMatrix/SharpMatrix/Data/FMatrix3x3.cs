using System;
using BraneCloud.Evolution.EC.MatrixLib.Ops;

namespace BraneCloud.Evolution.EC.MatrixLib.Data
{
    //package org.ejml.data;

/**
 * Fixed sized 3 by FMatrix3x3 matrix.  The matrix is stored as class variables for very fast read/write.  aXY is the
 * value of row = X and column = Y.
 * <p>DO NOT MODIFY.  Automatically generated code created by GenerateMatrixFixedNxN</p>
 *
 * @author Peter Abeles
 */
    public class FMatrix3x3 : FMatrixFixed
    {

        public float a11, a12, a13;
        public float a21, a22, a23;
        public float a31, a32, a33;

        public FMatrix3x3()
        {
        }

        public FMatrix3x3(float a11, float a12, float a13,
            float a21, float a22, float a23,
            float a31, float a32, float a33)
        {
            this.a11 = a11;
            this.a12 = a12;
            this.a13 = a13;
            this.a21 = a21;
            this.a22 = a22;
            this.a23 = a23;
            this.a31 = a31;
            this.a32 = a32;
            this.a33 = a33;
        }

        public FMatrix3x3(FMatrix3x3 o)
        {
            this.a11 = o.a11;
            this.a12 = o.a12;
            this.a13 = o.a13;
            this.a21 = o.a21;
            this.a22 = o.a22;
            this.a23 = o.a23;
            this.a31 = o.a31;
            this.a32 = o.a32;
            this.a33 = o.a33;
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
                else if (col == 2)
                {
                    return a13;
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
                else if (col == 2)
                {
                    return a23;
                }
            }
            else if (row == 2)
            {
                if (col == 0)
                {
                    return a31;
                }
                else if (col == 1)
                {
                    return a32;
                }
                else if (col == 2)
                {
                    return a33;
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
                else if (col == 2)
                {
                    a13 = val;
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
                else if (col == 2)
                {
                    a23 = val;
                    return;
                }
            }
            else if (row == 2)
            {
                if (col == 0)
                {
                    a31 = val;
                    return;
                }
                else if (col == 1)
                {
                    a32 = val;
                    return;
                }
                else if (col == 2)
                {
                    a33 = val;
                    return;
                }
            }
            throw new ArgumentException("Row and/or column out of range. " + row + " " + col);
        }

        public virtual void set(Matrix original)
        {
            if (original.getNumCols() != 3 || original.getNumRows() != 3)
                throw new ArgumentException("Rows and/or columns do not match");
            FMatrix m = (FMatrix) original;

            a11 = m.get(0, 0);
            a12 = m.get(0, 1);
            a13 = m.get(0, 2);
            a21 = m.get(1, 0);
            a22 = m.get(1, 1);
            a23 = m.get(1, 2);
            a31 = m.get(2, 0);
            a32 = m.get(2, 1);
            a33 = m.get(2, 2);
        }

        public virtual int getNumRows()
        {
            return 3;
        }

        public virtual int getNumCols()
        {
            return 3;
        }

        public virtual int getNumElements()
        {
            return 9;
        }

        public virtual Matrix copy()
        {
            return new FMatrix3x3(this);
        }

        public virtual void print()
        {
            MatrixIO.print(Console.OpenStandardOutput(), this);
        }

        public virtual Matrix createLike()
        {
            return new FMatrix3x3();
        }

        public virtual MatrixType getType()
        {
            return MatrixType.UNSPECIFIED;
        }
    }
}
