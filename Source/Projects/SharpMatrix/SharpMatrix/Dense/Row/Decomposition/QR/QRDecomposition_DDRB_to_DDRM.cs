using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Block;
using SharpMatrix.Dense.Block.Decomposition.QR;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Row.Decomposition.QR
{
    //package org.ejml.dense.row.decomposition.qr;

/**
 * Wrapper that allows {@link QRDecomposition}(DMatrixRBlock) to be used
 * as a {@link QRDecomposition}(DMatrixRMaj).
 *
 * @author Peter Abeles
 */
    public class QRDecomposition_DDRB_to_DDRM : BaseDecomposition_DDRB_to_DDRM, QRDecomposition<DMatrixRMaj>
    {

        public QRDecomposition_DDRB_to_DDRM()
            : base(new QRDecompositionHouseholder_DDRB(), EjmlParameters.BLOCK_WIDTH)
        {
        }

        //@Override
        public DMatrixRMaj getQ(DMatrixRMaj Q, bool compact)
        {

            int minLength = Math.Min(Ablock.numRows, Ablock.numCols);
            if (Q == null)
            {
                if (compact)
                {
                    Q = new DMatrixRMaj(Ablock.numRows, minLength);
                    CommonOps_DDRM.setIdentity(Q);
                }
                else
                {
                    Q = new DMatrixRMaj(Ablock.numRows, Ablock.numRows);
                    CommonOps_DDRM.setIdentity(Q);
                }
            }

            DMatrixRBlock Qblock = new DMatrixRBlock();
            Qblock.numRows = Q.numRows;
            Qblock.numCols = Q.numCols;
            Qblock.blockLength = blockLength;
            Qblock.data = Q.data;

            ((QRDecompositionHouseholder_DDRB) alg).getQ(Qblock, compact);

            convertBlockToRow(Q.numRows, Q.numCols, Ablock.blockLength, Q.data);

            return Q;
        }

        //@Override
        public DMatrixRMaj getR(DMatrixRMaj R, bool compact)
        {
            DMatrixRBlock Rblock;

            Rblock = ((QRDecompositionHouseholder_DDRB) alg).getR(null, compact);

            if (R == null)
            {
                R = new DMatrixRMaj(Rblock.numRows, Rblock.numCols);
            }
            MatrixOps_DDRB.convert(Rblock, R);

            return R;
        }

    }
}