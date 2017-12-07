using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Block;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Block.Decomposition.Chol;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.Chol
{
    //package org.ejml.dense.row.decomposition.chol;

    /**
     * Wrapper around {@link org.ejml.dense.block.decomposition.chol.CholeskyOuterForm_DDRB} that allows
     * it to process DMatrixRMaj.
     *
     * @author Peter Abeles
     */
    public class CholeskyDecomposition_DDRB_to_DDRM
        : BaseDecomposition_DDRB_to_DDRM, CholeskyDecomposition_F64<DMatrixRMaj>
    {

        public CholeskyDecomposition_DDRB_to_DDRM(bool lower)
            : base(new CholeskyOuterForm_DDRB(lower), EjmlParameters.BLOCK_WIDTH)
        {
        }

        public virtual bool isLower()
        {
            return ((CholeskyOuterForm_DDRB) alg).isLower();
        }

        public MatrixType getT(MatrixType T)
        {
            throw new NotImplementedException();
        }

        public virtual DMatrixRMaj getT(DMatrixRMaj T)
        {
            DMatrixRBlock T_block = ((CholeskyOuterForm_DDRB)alg).getT(null);

            if (T == null)
            {
                T = new DMatrixRMaj(T_block.numRows, T_block.numCols);
            }

            MatrixOps_DDRB.convert(T_block, T);
            // todo set zeros
            return T;
        }

        public virtual Complex_F64 computeDeterminant()
        {
            return ((CholeskyOuterForm_DDRB) alg).computeDeterminant();
        }
    }
}