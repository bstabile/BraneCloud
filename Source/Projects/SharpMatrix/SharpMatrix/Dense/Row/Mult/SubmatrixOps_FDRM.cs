using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense
{
    //package org.ejml.dense.row.mult;

/**
 * Operations that are performed on a submatrix inside a larger matrix.
 *
 * @author Peter Abeles
 */
    public class SubmatrixOps_FDRM
    {

        public static void setSubMatrix(FMatrix1Row src, FMatrix1Row dst,
            int srcRow, int srcCol, int dstRow, int dstCol,
            int numSubRows, int numSubCols)
        {
            for (int i = 0; i < numSubRows; i++)
            {
                for (int j = 0; j < numSubCols; j++)
                {
                    float val = src.get(i + srcRow, j + srcCol);
                    dst.set(i + dstRow, j + dstCol, val);
                }
            }
        }
    }
}