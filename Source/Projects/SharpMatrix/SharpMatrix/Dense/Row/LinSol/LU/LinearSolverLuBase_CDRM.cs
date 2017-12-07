using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.LU;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.LinSol.LU
{
    //package org.ejml.dense.row.linsol.lu;

/**
 * @author Peter Abeles
 */
    public abstract class LinearSolverLuBase_CDRM : LinearSolverAbstract_CDRM
    {

        protected LUDecompositionBase_CDRM decomp;

        public LinearSolverLuBase_CDRM(LUDecompositionBase_CDRM decomp)
        {
            this.decomp = decomp;

        }

        //@Override
        public override bool setA(CMatrixRMaj A)
        {
            _setA(A);

            return decomp.decompose(A);
        }

        //@Override
        public override /**/ double quality()
        {
            return decomp.quality();
        }

        //@Override
        public override void invert(CMatrixRMaj A_inv)
        {
            float[] vv = decomp._getVV();
            CMatrixRMaj LU = decomp.getLU();

            if (A_inv.numCols != LU.numCols || A_inv.numRows != LU.numRows)
                throw new ArgumentException("Unexpected matrix dimension");

            int n = A.numCols;

            float[] dataInv = A_inv.data;
            int strideAinv = A_inv.getRowStride();

            for (int j = 0; j < n; j++)
            {
                // don't need to change inv into an identity matrix before hand
                Array.Clear(vv, 0, n * 2);
                vv[j * 2] = 1;
                vv[j * 2 + 1] = 0;

                decomp._solveVectorInternal(vv);
//            for( int i = 0; i < n; i++ ) dataInv[i* n +j] = vv[i];
                int index = j * 2;
                for (int i = 0; i < n; i++, index += strideAinv)
                {
                    dataInv[index] = vv[i * 2];
                    dataInv[index + 1] = vv[i * 2 + 1];
                }
            }
        }

        //@Override
        public override bool modifiesA()
        {
            return false;
        }

        //@Override
        public override bool modifiesB()
        {
            return false;
        }

        //@Override
        public override DecompositionInterface<CMatrixRMaj> getDecomposition()
        {
            return decomp;
        }
    }
}