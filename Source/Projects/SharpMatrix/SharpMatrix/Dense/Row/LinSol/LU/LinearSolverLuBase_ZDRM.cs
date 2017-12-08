using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row.Decomposition.LU;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Row.LinSol.LU
{
    //package org.ejml.dense.row.linsol.lu;

/**
 * @author Peter Abeles
 */
    public abstract class LinearSolverLuBase_ZDRM : LinearSolverAbstract_ZDRM
    {

        protected LUDecompositionBase_ZDRM decomp;

        public LinearSolverLuBase_ZDRM(LUDecompositionBase_ZDRM decomp)
        {
            this.decomp = decomp;

        }

        //@Override
        public override bool setA(ZMatrixRMaj A)
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
        public override void invert(ZMatrixRMaj A_inv)
        {
            double[] vv = decomp._getVV();
            ZMatrixRMaj LU = decomp.getLU();

            if (A_inv.numCols != LU.numCols || A_inv.numRows != LU.numRows)
                throw new ArgumentException("Unexpected matrix dimension");

            int n = A.numCols;

            double[] dataInv = A_inv.data;
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
        public override DecompositionInterface<ZMatrixRMaj> getDecomposition()
        {
            return decomp;
        }
    }
}