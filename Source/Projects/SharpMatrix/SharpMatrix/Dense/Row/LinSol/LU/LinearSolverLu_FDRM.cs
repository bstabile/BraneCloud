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
    public class LinearSolverLu_FDRM : LinearSolverLuBase_FDRM
    {

        bool doImprove = false;

        public LinearSolverLu_FDRM(LUDecompositionBase_FDRM decomp)
            : base(decomp)
        {
        }

        public LinearSolverLu_FDRM(LUDecompositionBase_FDRM decomp, bool doImprove)
            : base(decomp)
        {
            this.doImprove = doImprove;
        }


        public override void solve(FMatrixRMaj b, FMatrixRMaj x)
        {
            if (b.numCols != x.numCols || b.numRows != numRows || x.numRows != this.numCols)
            {
                throw new ArgumentException("Unexpected matrix size");
            }

            int numCols = b.numCols;

            float[] dataB = b.data;
            float[] dataX = x.data;

            float[] vv = decomp._getVV();

//        for( int j = 0; j < numCols; j++ ) {
//            for( int i = 0; i < this.numCols; i++ ) vv[i] = dataB[i*numCols+j];
//            decomp._solveVectorInternal(vv);
//            for( int i = 0; i < this.numCols; i++ ) dataX[i*numCols+j] = vv[i];
//        }
            for (int j = 0; j < numCols; j++)
            {
                int index = j;
                for (int i = 0; i < this.numCols; i++, index += numCols) vv[i] = dataB[index];
                decomp._solveVectorInternal(vv);
                index = j;
                for (int i = 0; i < this.numCols; i++, index += numCols) dataX[index] = vv[i];
            }

            if (doImprove)
            {
                improveSol(b, x);
            }
        }
    }
}