using System;
using SharpMatrix.Data;
using Randomization;

namespace SharpMatrix.Sparse.Triplet
{
    //package org.ejml.sparse.triplet;

/**
 * @author Peter Abeles
 */
    public class RandomMatrices_DSTL
    {
        /**
         * Randomly generates matrix with the specified number of matrix elements filled with values from min to max.
         *
         * @param numRows Number of rows
         * @param numCols Number of columns
         * @param nz_total Total number of non-zero elements in the matrix
         * @param min Minimum value
         * @param max maximum value
         * @param rand Random number generated
         * @return Randomly generated matrix
         */
        public static DMatrixSparseTriplet uniform(int numRows, int numCols, int nz_total,
            double min, double max, IMersenneTwister rand)
        {
            // Create a list of all the possible element values
            int N = numCols * numRows;
            if (N < 0)
                throw new ArgumentException("matrix size is too large");
            nz_total = Math.Min(N, nz_total);

            int[] selected = new int[N];
            for (int i = 0; i < N; i++)
            {
                selected[i] = i;
            }

            for (int i = 0; i < nz_total; i++)
            {
                int s = rand.NextInt(N);
                int tmp = selected[s];
                selected[s] = selected[i];
                selected[i] = tmp;
            }

            // Create a sparse matrix
            DMatrixSparseTriplet ret = new DMatrixSparseTriplet(numRows, numCols, nz_total);

            for (int i = 0; i < nz_total; i++)
            {
                int row = selected[i] / numCols;
                int col = selected[i] % numCols;

                double value = rand.NextDouble() * (max - min) + min;

                ret.addItem(row, col, value);
            }

            return ret;
        }
    }
}