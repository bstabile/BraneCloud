using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Block;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Row.Decomposition
{
    //package org.ejml.dense.row.decomposition;

/**
 * Generic interface for wrapping a {@link FMatrixRBlock} decomposition for
 * processing of {@link FMatrixRMaj}.
 *
 * @author Peter Abeles
 */
    public class BaseDecomposition_FDRB_to_FDRM : DecompositionInterface<FMatrixRMaj>
    {

        protected DecompositionInterface<FMatrixRBlock> alg;

        protected float[] tmp;
        protected FMatrixRBlock Ablock = new FMatrixRBlock();
        protected int blockLength;

        public BaseDecomposition_FDRB_to_FDRM(DecompositionInterface<FMatrixRBlock> alg,
            int blockLength)
        {
            this.alg = alg;
            this.blockLength = blockLength;
        }

        public virtual bool decompose(FMatrixRMaj A)
        {
            Ablock.numRows = A.numRows;
            Ablock.numCols = A.numCols;
            Ablock.blockLength = blockLength;
            Ablock.data = A.data;

            int tmpLength = Math.Min(Ablock.blockLength, A.numRows) * A.numCols;

            if (tmp == null || tmp.Length < tmpLength)
                tmp = new float[tmpLength];

            // doing an in-place convert is much more memory efficient at the cost of a little
            // but of CPU
            MatrixOps_FDRB.convertRowToBlock(A.numRows, A.numCols, Ablock.blockLength, A.data, tmp);

            bool ret = alg.decompose(Ablock);

            // convert it back to the normal format if it wouldn't have been modified
            if (!alg.inputModified())
            {
                MatrixOps_FDRB.convertBlockToRow(A.numRows, A.numCols, Ablock.blockLength, A.data, tmp);
            }

            return ret;
        }

        public void convertBlockToRow(int numRows, int numCols, int blockLength,
            float[] data)
        {
            int tmpLength = Math.Min(blockLength, numRows) * numCols;

            if (tmp == null || tmp.Length < tmpLength)
                tmp = new float[tmpLength];

            MatrixOps_FDRB.convertBlockToRow(numRows, numCols, Ablock.blockLength, data, tmp);
        }

        public virtual bool inputModified()
        {
            return alg.inputModified();
        }
    }
}