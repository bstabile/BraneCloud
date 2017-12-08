using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Block;
using SharpMatrix.Dense.Block.Decomposition.Chol;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Row.Decomposition.Chol
{
    //package org.ejml.dense.row.decomposition.chol;

/**
 * Wrapper around {@link org.ejml.dense.block.decomposition.chol.CholeskyOuterForm_FDRB} that allows
 * it to process FMatrixRMaj.
 *
 * @author Peter Abeles
 */
    public class CholeskyDecomposition_FDRB_to_FDRM
        : BaseDecomposition_FDRB_to_FDRM, CholeskyDecomposition_F32<FMatrixRMaj>
    {

        public CholeskyDecomposition_FDRB_to_FDRM(bool lower)
            : base(new CholeskyOuterForm_FDRB(lower), EjmlParameters.BLOCK_WIDTH)
        {
        }

        public virtual bool isLower()
        {
            return ((CholeskyOuterForm_FDRB) alg).isLower();
        }

        public virtual FMatrixRMaj getT(FMatrixRMaj T)
        {
            FMatrixRBlock T_block = ((CholeskyOuterForm_FDRB) alg).getT(null);

            if (T == null)
            {
                T = new FMatrixRMaj(T_block.numRows, T_block.numCols);
            }

            MatrixOps_FDRB.convert(T_block, T);
            // todo set zeros
            return T;
        }

        public virtual Complex_F32 computeDeterminant()
        {
            return ((CholeskyOuterForm_FDRB) alg).computeDeterminant();
        }
    }
}