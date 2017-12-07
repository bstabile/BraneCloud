using System;
using BraneCloud.Evolution.EC.MatrixLib.Ops;

namespace BraneCloud.Evolution.EC.MatrixLib.Data
{
    //package org.ejml.data;

/**
 * Fixed sized vector with 4 elements.  Can represent a 4 x 1 or 1 x 4 matrix, context dependent.
 * <p>DO NOT MODIFY.  Automatically generated code created by GenerateMatrixFixedN</p>
 *
 * @author Peter Abeles
 */
    public class FMatrix4 : FMatrixFixed
    {
        public float a1, a2, a3, a4;

        public FMatrix4()
        {
        }

        public FMatrix4(float a1, float a2, float a3, float a4)
        {
            this.a1 = a1;
            this.a2 = a2;
            this.a3 = a3;
            this.a4 = a4;
        }

        public FMatrix4(FMatrix4 o)
        {
            this.a1 = o.a1;
            this.a2 = o.a2;
            this.a3 = o.a3;
            this.a4 = o.a4;
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
            else if (w == 2)
            {
                return a3;
            }
            else if (w == 3)
            {
                return a4;
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
            else if (w == 2)
            {
                a3 = val;
            }
            else if (w == 3)
            {
                a4 = val;
            }
            else
            {
                throw new ArgumentException("Out of range.  " + w);
            }
        }

        public virtual void set(Matrix original)
        {
            FMatrix m = (FMatrix) original;

            if (m.getNumCols() == 1 && m.getNumRows() == 4)
            {
                a1 = m.get(0, 0);
                a2 = m.get(1, 0);
                a3 = m.get(2, 0);
                a4 = m.get(3, 0);
            }
            else if (m.getNumRows() == 1 && m.getNumCols() == 4)
            {
                a1 = m.get(0, 0);
                a2 = m.get(0, 1);
                a3 = m.get(0, 2);
                a4 = m.get(0, 3);
            }
            else
            {
                throw new ArgumentException("Incompatible shape");
            }
        }

        public virtual int getNumRows()
        {
            return 4;
        }

        public virtual int getNumCols()
        {
            return 1;
        }

        public virtual int getNumElements()
        {
            return 4;
        }

        public virtual Matrix copy()
        {
            return new FMatrix4(this);
        }

        public virtual void print()
        {
            MatrixIO.print(Console.OpenStandardOutput(), this);
        }

        public virtual Matrix createLike()
        {
            return new FMatrix4();
        }

        public virtual MatrixType getType()
        {
            return MatrixType.UNSPECIFIED;
        }
    }
}
