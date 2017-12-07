using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc
{
    //package org.ejml.sparse.csc;

/**
 * @author Peter Abeles
 */
    public class NormOps_DSCC
    {

        public static double fastNormF(DMatrixSparseCSC A)
        {
            double total = 0;

            for (int i = 0; i < A.nz_length; i++)
            {
                double x = A.nz_values[i];
                total += x * x;
            }

            return Math.Sqrt(total);
        }

        public static double normF(DMatrixSparseCSC A)
        {
            double total = 0;
            double max = CommonOps_DSCC.elementMaxAbs(A);

            for (int i = 0; i < A.nz_length; i++)
            {
                double x = A.nz_values[i] / max;
                total += x * x;
            }

            return max * Math.Sqrt(total);
        }
    }
}