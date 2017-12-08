using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row.Decomposition.LU;

namespace SharpMatrix.Dense.Row.LinSol.LU
{
    //package org.ejml.dense.row.linsol.lu;

/**
 * For each column in the B matrix it makes a copy, which is then solved for and
 * writen into X.  By making a copy of the column cpu cache issues are reduced.
 *
 * @author Peter Abeles
 */
    public class LinearSolverLu_CDRM : LinearSolverLuBase_CDRM
    {

        public LinearSolverLu_CDRM(LUDecompositionBase_CDRM decomp)
            : base(decomp)
        {
        }



        //@Override
        public override void solve(CMatrixRMaj b, CMatrixRMaj x)
        {
            if (b.numCols != x.numCols || b.numRows != numRows || x.numRows != numCols)
            {
                throw new ArgumentException("Unexpected matrix size");
            }

            int bnumCols = b.numCols;
            int bstride = b.getRowStride();

            float[] dataB = b.data;
            float[] dataX = x.data;

            float[] vv = decomp._getVV();

//        for( int j = 0; j < numCols; j++ ) {
//            for( int i = 0; i < this.numCols; i++ ) vv[i] = dataB[i*numCols+j];
//            decomp._solveVectorInternal(vv);
//            for( int i = 0; i < this.numCols; i++ ) dataX[i*numCols+j] = vv[i];
//        }
            for (int j = 0; j < bnumCols; j++)
            {
                int index = j * 2;
                for (int i = 0; i < numRows; i++, index += bstride)
                {
                    vv[i * 2] = dataB[index];
                    vv[i * 2 + 1] = dataB[index + 1];
                }
                decomp._solveVectorInternal(vv);
                index = j * 2;
                for (int i = 0; i < numRows; i++, index += bstride)
                {
                    dataX[index] = vv[i * 2];
                    dataX[index + 1] = vv[i * 2 + 1];
                }
            }

        }
    }
}