using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Block;
using SharpMatrix.Dense.Block.Decomposition.QR;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Row.Decomposition.QR
{
    //package org.ejml.dense.row.decomposition.qr;

/**
 * Wrapper that allows {@link QRDecomposition}(FMatrixRBlock) to be used
 * as a {@link QRDecomposition}(FMatrixRMaj).
 *
 * @author Peter Abeles
 */
    public class QRDecomposition_FDRB_to_FDRM
        : BaseDecomposition_FDRB_to_FDRM, QRDecomposition<FMatrixRMaj>
    {

        public QRDecomposition_FDRB_to_FDRM()
            : base(new QRDecompositionHouseholder_FDRB(), EjmlParameters.BLOCK_WIDTH)
        {
        }

        //@Override
        public FMatrixRMaj getQ(FMatrixRMaj Q, bool compact)
        {

            int minLength = Math.Min(Ablock.numRows, Ablock.numCols);
            if (Q == null)
            {
                if (compact)
                {
                    Q = new FMatrixRMaj(Ablock.numRows, minLength);
                    CommonOps_FDRM.setIdentity(Q);
                }
                else
                {
                    Q = new FMatrixRMaj(Ablock.numRows, Ablock.numRows);
                    CommonOps_FDRM.setIdentity(Q);
                }
            }

            FMatrixRBlock Qblock = new FMatrixRBlock();
            Qblock.numRows = Q.numRows;
            Qblock.numCols = Q.numCols;
            Qblock.blockLength = blockLength;
            Qblock.data = Q.data;

            ((QRDecompositionHouseholder_FDRB) alg).getQ(Qblock, compact);

            convertBlockToRow(Q.numRows, Q.numCols, Ablock.blockLength, Q.data);

            return Q;
        }

        //@Override
        public FMatrixRMaj getR(FMatrixRMaj R, bool compact)
        {
            FMatrixRBlock Rblock;

            Rblock = ((QRDecompositionHouseholder_FDRB) alg).getR(null, compact);

            if (R == null)
            {
                R = new FMatrixRMaj(Rblock.numRows, Rblock.numCols);
            }
            MatrixOps_FDRB.convert(Rblock, R);

            return R;
        }

    }
}