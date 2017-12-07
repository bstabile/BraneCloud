using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.LU
{
    //package org.ejml.dense.row.decomposition.lu;

/**
 * <p>
 * An LU decomposition algorithm that originally came from Jama.  In general this is faster than
 * what is in NR since it creates a cache of a column, which makes a big difference in larger
 * matrices.
 * </p>
 *
 * @author Peter Abeles
 */
    public class LUDecompositionAlt_FDRM : LUDecompositionBase_FDRM
    {

        /**
         * This is a modified version of what was found in the JAMA package.  The order that it
         * performs its permutations in is the primary difference from NR
         *
         * @param a The matrix that is to be decomposed.  Not modified.
         * @return true If the matrix can be decomposed and false if it can not.
         */
        public override bool decompose(FMatrixRMaj a)
        {
            decomposeCommonInit(a);

            float[] LUcolj = vv;

            for (int j = 0; j < n; j++)
            {

                // make a copy of the column to avoid cache jumping issues
                for (int i = 0; i < m; i++)
                {
                    LUcolj[i] = dataLU[i * n + j];
                }

                // Apply previous transformations.
                for (int i = 0; i < m; i++)
                {
                    int rowIndex = i * n;

                    // Most of the time is spent in the following dot product.
                    int kmax = i < j ? i : j;
                    float s = 0.0f;
                    for (int k = 0; k < kmax; k++)
                    {
                        s += dataLU[rowIndex + k] * LUcolj[k];
                    }

                    dataLU[rowIndex + j] = LUcolj[i] -= s;
                }

                // Find pivot and exchange if necessary.
                int p = j;
                float max = Math.Abs(LUcolj[p]);
                for (int i = j + 1; i < m; i++)
                {
                    float v = Math.Abs(LUcolj[i]);
                    if (v > max)
                    {
                        p = i;
                        max = v;
                    }
                }

                if (p != j)
                {
                    // swap the rows
//                for (int k = 0; k < n; k++) {
//                    float t = dataLU[p*n + k];
//                    dataLU[p*n + k] = dataLU[j*n + k];
//                    dataLU[j*n + k] = t;
//                }
                    int rowP = p * n;
                    int rowJ = j * n;
                    int endP = rowP + n;
                    for (; rowP < endP; rowP++, rowJ++)
                    {
                        float t = dataLU[rowP];
                        dataLU[rowP] = dataLU[rowJ];
                        dataLU[rowJ] = t;
                    }
                    int k = pivot[p];
                    pivot[p] = pivot[j];
                    pivot[j] = k;
                    pivsign = -pivsign;
                }
                indx[j] = p;

                // Compute multipliers.
                if (j < m)
                {
                    float lujj = dataLU[j * n + j];
                    if (lujj != 0)
                    {
                        for (int i = j + 1; i < m; i++)
                        {
                            dataLU[i * n + j] /= lujj;
                        }
                    }
                }
            }

            return true;
        }
    }
}