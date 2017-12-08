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
    public class LinearSolverLu_DDRM : LinearSolverLuBase_DDRM
    {

        bool doImprove = false;

        public LinearSolverLu_DDRM(LUDecompositionBase_DDRM decomp)
            : base(decomp)
        {
        }

        public LinearSolverLu_DDRM(LUDecompositionBase_DDRM decomp, bool doImprove)
            : base(decomp)
        {
            this.doImprove = doImprove;
        }


        public override void solve(DMatrixRMaj b, DMatrixRMaj x)
        {
            if (b.numCols != x.numCols || b.numRows != numRows || x.numRows != this.numCols)
            {
                throw new ArgumentException("Unexpected matrix size");
            }

            int nCols = b.numCols;

            double[] dataB = b.data;
            double[] dataX = x.data;

            double[] vv = decomp._getVV();

//        for( int j = 0; j < numCols; j++ ) {
//            for( int i = 0; i < this.numCols; i++ ) vv[i] = dataB[i*numCols+j];
//            decomp._solveVectorInternal(vv);
//            for( int i = 0; i < this.numCols; i++ ) dataX[i*numCols+j] = vv[i];
//        }
            for (int j = 0; j < nCols; j++)
            {
                int index = j;
                for (int i = 0; i < this.numCols; i++, index += nCols) vv[i] = dataB[index];
                decomp._solveVectorInternal(vv);
                index = j;
                for (int i = 0; i < this.numCols; i++, index += nCols) dataX[index] = vv[i];
            }

            if (doImprove)
            {
                improveSol(b, x);
            }
        }
    }

}