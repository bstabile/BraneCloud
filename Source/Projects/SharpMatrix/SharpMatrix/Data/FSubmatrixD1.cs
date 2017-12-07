using System;
using BraneCloud.Evolution.EC.MatrixLib.Ops;

namespace BraneCloud.Evolution.EC.MatrixLib.Data
{
    //package org.ejml.data;

/**
 * <p>
 * Describes a rectangular submatrix inside of a {@link FMatrixD1}.
 * </p>
 *
 * <p>
 * Rows are row0 &le; i &lt; row1 and Columns are col0 &le; j &lt; col1
 * </p>
 * 
 * @author Peter Abeles
 */
    public class FSubmatrixD1
    {
        public FMatrixD1 original;

        // bounding rows and columns
        public int row0, col0;

        public int row1, col1;

        public FSubmatrixD1()
        {
        }

        public FSubmatrixD1(FMatrixD1 original)
        {
            set(original);
        }

        public FSubmatrixD1(FMatrixD1 original,
            int row0, int row1, int col0, int col1)
        {
            set(original, row0, row1, col0, col1);
        }

        public void set(FMatrixD1 original,
            int row0, int row1, int col0, int col1)
        {
            this.original = original;
            this.row0 = row0;
            this.col0 = col0;
            this.row1 = row1;
            this.col1 = col1;
        }

        public void set(FMatrixD1 original)
        {
            this.original = original;
            row1 = original.numRows;
            col1 = original.numCols;
        }

        public int getRows()
        {
            return row1 - row0;
        }

        public int getCols()
        {
            return col1 - col0;
        }

        public float get(int row, int col)
        {
            return original.get(row + row0, col + col0);
        }

        public void set(int row, int col, float value)
        {
            original.set(row + row0, col + col0, value);
        }

        public FMatrixRMaj extract()
        {
            FMatrixRMaj ret = new FMatrixRMaj(row1 - row0, col1 - col0);

            for (int i = 0; i < ret.numRows; i++)
            {
                for (int j = 0; j < ret.numCols; j++)
                {
                    ret.set(i, j, get(i, j));
                }
            }

            return ret;
        }

        public void print()
        {
            MatrixIO.print(Console.OpenStandardOutput(), original, "%6.3ff", row0, row1, col0, col1);
        }
    }
}