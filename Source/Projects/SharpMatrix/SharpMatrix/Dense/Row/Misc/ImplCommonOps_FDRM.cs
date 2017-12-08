using System;
using SharpMatrix.Data;

namespace SharpMatrix.Dense.Row.Misc
{
    //package org.ejml.dense.row.misc;

/**
 * Implementations of common ops routines for {@link FMatrixRMaj}.  In general
 * there is no need to directly invoke these functions.
 *
 * @author Peter Abeles
 */
    public class ImplCommonOps_FDRM
    {
        public static void extract(FMatrixRMaj src,
            int srcY0, int srcX0,
            FMatrixRMaj dst,
            int dstY0, int dstX0,
            int numRows, int numCols)
        {
            for (int y = 0; y < numRows; y++)
            {
                int indexSrc = src.getIndex(y + srcY0, srcX0);
                int indexDst = dst.getIndex(y + dstY0, dstX0);
                Array.Copy(src.data, indexSrc, dst.data, indexDst, numCols);
            }
        }
    }
}