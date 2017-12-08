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
    public class ImplCommonOps_FDMA
    {
        public static void extract(FMatrix src,
            int srcY0, int srcX0,
            FMatrix dst,
            int dstY0, int dstX0,
            int numRows, int numCols)
        {
            for (int y = 0; y < numRows; y++)
            {
                for (int x = 0; x < numCols; x++)
                {
                    float v = src.get(y + srcY0, x + srcX0);
                    dst.set(dstY0 + y, dstX0 + x, v);
                }
            }
        }
    }
}