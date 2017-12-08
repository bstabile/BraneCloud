using System;
using SharpMatrix.Ops;

namespace SharpMatrix.Data
{
    //package org.ejml.data;

/**
 * Fixed sized 6 by FMatrix6x6 matrix.  The matrix is stored as class variables for very fast read/write.  aXY is the
 * value of row = X and column = Y.
 * <p>DO NOT MODIFY.  Automatically generated code created by GenerateMatrixFixedNxN</p>
 *
 * @author Peter Abeles
 */
    public class FMatrix6x6 : FMatrixFixed
    {

        public float a11, a12, a13, a14, a15, a16;
        public float a21, a22, a23, a24, a25, a26;
        public float a31, a32, a33, a34, a35, a36;
        public float a41, a42, a43, a44, a45, a46;
        public float a51, a52, a53, a54, a55, a56;
        public float a61, a62, a63, a64, a65, a66;

        public FMatrix6x6()
        {
        }

        public FMatrix6x6(float a11, float a12, float a13, float a14, float a15, float a16,
            float a21, float a22, float a23, float a24, float a25, float a26,
            float a31, float a32, float a33, float a34, float a35, float a36,
            float a41, float a42, float a43, float a44, float a45, float a46,
            float a51, float a52, float a53, float a54, float a55, float a56,
            float a61, float a62, float a63, float a64, float a65, float a66)
        {
            this.a11 = a11;
            this.a12 = a12;
            this.a13 = a13;
            this.a14 = a14;
            this.a15 = a15;
            this.a16 = a16;
            this.a21 = a21;
            this.a22 = a22;
            this.a23 = a23;
            this.a24 = a24;
            this.a25 = a25;
            this.a26 = a26;
            this.a31 = a31;
            this.a32 = a32;
            this.a33 = a33;
            this.a34 = a34;
            this.a35 = a35;
            this.a36 = a36;
            this.a41 = a41;
            this.a42 = a42;
            this.a43 = a43;
            this.a44 = a44;
            this.a45 = a45;
            this.a46 = a46;
            this.a51 = a51;
            this.a52 = a52;
            this.a53 = a53;
            this.a54 = a54;
            this.a55 = a55;
            this.a56 = a56;
            this.a61 = a61;
            this.a62 = a62;
            this.a63 = a63;
            this.a64 = a64;
            this.a65 = a65;
            this.a66 = a66;
        }

        public FMatrix6x6(FMatrix6x6 o)
        {
            this.a11 = o.a11;
            this.a12 = o.a12;
            this.a13 = o.a13;
            this.a14 = o.a14;
            this.a15 = o.a15;
            this.a16 = o.a16;
            this.a21 = o.a21;
            this.a22 = o.a22;
            this.a23 = o.a23;
            this.a24 = o.a24;
            this.a25 = o.a25;
            this.a26 = o.a26;
            this.a31 = o.a31;
            this.a32 = o.a32;
            this.a33 = o.a33;
            this.a34 = o.a34;
            this.a35 = o.a35;
            this.a36 = o.a36;
            this.a41 = o.a41;
            this.a42 = o.a42;
            this.a43 = o.a43;
            this.a44 = o.a44;
            this.a45 = o.a45;
            this.a46 = o.a46;
            this.a51 = o.a51;
            this.a52 = o.a52;
            this.a53 = o.a53;
            this.a54 = o.a54;
            this.a55 = o.a55;
            this.a56 = o.a56;
            this.a61 = o.a61;
            this.a62 = o.a62;
            this.a63 = o.a63;
            this.a64 = o.a64;
            this.a65 = o.a65;
            this.a66 = o.a66;
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
                else if (col == 3)
                {
                    return a14;
                }
                else if (col == 4)
                {
                    return a15;
                }
                else if (col == 5)
                {
                    return a16;
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
                else if (col == 3)
                {
                    return a24;
                }
                else if (col == 4)
                {
                    return a25;
                }
                else if (col == 5)
                {
                    return a26;
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
                else if (col == 3)
                {
                    return a34;
                }
                else if (col == 4)
                {
                    return a35;
                }
                else if (col == 5)
                {
                    return a36;
                }
            }
            else if (row == 3)
            {
                if (col == 0)
                {
                    return a41;
                }
                else if (col == 1)
                {
                    return a42;
                }
                else if (col == 2)
                {
                    return a43;
                }
                else if (col == 3)
                {
                    return a44;
                }
                else if (col == 4)
                {
                    return a45;
                }
                else if (col == 5)
                {
                    return a46;
                }
            }
            else if (row == 4)
            {
                if (col == 0)
                {
                    return a51;
                }
                else if (col == 1)
                {
                    return a52;
                }
                else if (col == 2)
                {
                    return a53;
                }
                else if (col == 3)
                {
                    return a54;
                }
                else if (col == 4)
                {
                    return a55;
                }
                else if (col == 5)
                {
                    return a56;
                }
            }
            else if (row == 5)
            {
                if (col == 0)
                {
                    return a61;
                }
                else if (col == 1)
                {
                    return a62;
                }
                else if (col == 2)
                {
                    return a63;
                }
                else if (col == 3)
                {
                    return a64;
                }
                else if (col == 4)
                {
                    return a65;
                }
                else if (col == 5)
                {
                    return a66;
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
                else if (col == 3)
                {
                    a14 = val;
                    return;
                }
                else if (col == 4)
                {
                    a15 = val;
                    return;
                }
                else if (col == 5)
                {
                    a16 = val;
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
                else if (col == 3)
                {
                    a24 = val;
                    return;
                }
                else if (col == 4)
                {
                    a25 = val;
                    return;
                }
                else if (col == 5)
                {
                    a26 = val;
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
                else if (col == 3)
                {
                    a34 = val;
                    return;
                }
                else if (col == 4)
                {
                    a35 = val;
                    return;
                }
                else if (col == 5)
                {
                    a36 = val;
                    return;
                }
            }
            else if (row == 3)
            {
                if (col == 0)
                {
                    a41 = val;
                    return;
                }
                else if (col == 1)
                {
                    a42 = val;
                    return;
                }
                else if (col == 2)
                {
                    a43 = val;
                    return;
                }
                else if (col == 3)
                {
                    a44 = val;
                    return;
                }
                else if (col == 4)
                {
                    a45 = val;
                    return;
                }
                else if (col == 5)
                {
                    a46 = val;
                    return;
                }
            }
            else if (row == 4)
            {
                if (col == 0)
                {
                    a51 = val;
                    return;
                }
                else if (col == 1)
                {
                    a52 = val;
                    return;
                }
                else if (col == 2)
                {
                    a53 = val;
                    return;
                }
                else if (col == 3)
                {
                    a54 = val;
                    return;
                }
                else if (col == 4)
                {
                    a55 = val;
                    return;
                }
                else if (col == 5)
                {
                    a56 = val;
                    return;
                }
            }
            else if (row == 5)
            {
                if (col == 0)
                {
                    a61 = val;
                    return;
                }
                else if (col == 1)
                {
                    a62 = val;
                    return;
                }
                else if (col == 2)
                {
                    a63 = val;
                    return;
                }
                else if (col == 3)
                {
                    a64 = val;
                    return;
                }
                else if (col == 4)
                {
                    a65 = val;
                    return;
                }
                else if (col == 5)
                {
                    a66 = val;
                    return;
                }
            }
            throw new ArgumentException("Row and/or column out of range. " + row + " " + col);
        }

        public virtual void set(Matrix original)
        {
            if (original.getNumCols() != 6 || original.getNumRows() != 6)
                throw new ArgumentException("Rows and/or columns do not match");
            FMatrix m = (FMatrix) original;

            a11 = m.get(0, 0);
            a12 = m.get(0, 1);
            a13 = m.get(0, 2);
            a14 = m.get(0, 3);
            a15 = m.get(0, 4);
            a16 = m.get(0, 5);
            a21 = m.get(1, 0);
            a22 = m.get(1, 1);
            a23 = m.get(1, 2);
            a24 = m.get(1, 3);
            a25 = m.get(1, 4);
            a26 = m.get(1, 5);
            a31 = m.get(2, 0);
            a32 = m.get(2, 1);
            a33 = m.get(2, 2);
            a34 = m.get(2, 3);
            a35 = m.get(2, 4);
            a36 = m.get(2, 5);
            a41 = m.get(3, 0);
            a42 = m.get(3, 1);
            a43 = m.get(3, 2);
            a44 = m.get(3, 3);
            a45 = m.get(3, 4);
            a46 = m.get(3, 5);
            a51 = m.get(4, 0);
            a52 = m.get(4, 1);
            a53 = m.get(4, 2);
            a54 = m.get(4, 3);
            a55 = m.get(4, 4);
            a56 = m.get(4, 5);
            a61 = m.get(5, 0);
            a62 = m.get(5, 1);
            a63 = m.get(5, 2);
            a64 = m.get(5, 3);
            a65 = m.get(5, 4);
            a66 = m.get(5, 5);
        }

        public virtual int getNumRows()
        {
            return 6;
        }

        public virtual int getNumCols()
        {
            return 6;
        }

        public virtual int getNumElements()
        {
            return 36;
        }

        public virtual Matrix copy()
        {
            return new FMatrix6x6(this);
        }

        public virtual void print()
        {
            MatrixIO.print(Console.OpenStandardOutput(), this);
        }

        public virtual Matrix createLike()
        {
            return new FMatrix6x6();
        }

        public virtual MatrixType getType()
        {
            return MatrixType.UNSPECIFIED;
        }
    }
}
