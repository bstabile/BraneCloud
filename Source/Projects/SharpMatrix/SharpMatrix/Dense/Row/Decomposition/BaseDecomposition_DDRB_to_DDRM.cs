using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Block;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Row.Decomposition
{
    //package org.ejml.dense.row.decomposition;

    /**
     * Generic interface for wrapping a {@link DMatrixRBlock} decomposition for
     * processing of {@link DMatrixRMaj}.
     *
     * @author Peter Abeles
     */
    public class BaseDecomposition_DDRB_to_DDRM : DecompositionInterface<DMatrixRMaj>
    {

        protected DecompositionInterface<DMatrixRBlock> alg;

        protected double[] tmp;
        protected DMatrixRBlock Ablock = new DMatrixRBlock();
        protected int blockLength;

        public BaseDecomposition_DDRB_to_DDRM(DecompositionInterface<DMatrixRBlock> alg,
            int blockLength)
        {
            this.alg = alg;
            this.blockLength = blockLength;
        }

        public virtual bool decompose(DMatrixRMaj A)
        {
            Ablock.numRows = A.numRows;
            Ablock.numCols = A.numCols;
            Ablock.blockLength = blockLength;
            Ablock.data = A.data;

            int tmpLength = Math.Min(Ablock.blockLength, A.numRows) * A.numCols;

            if (tmp == null || tmp.Length < tmpLength)
                tmp = new double[tmpLength];

            // doing an in-place convert is much more memory efficient at the cost of a little
            // but of CPU
            MatrixOps_DDRB.convertRowToBlock(A.numRows, A.numCols, Ablock.blockLength, A.data, tmp);

            bool ret = alg.decompose(Ablock);

            // convert it back to the normal format if it wouldn't have been modified
            if (!alg.inputModified())
            {
                MatrixOps_DDRB.convertBlockToRow(A.numRows, A.numCols, Ablock.blockLength, A.data, tmp);
            }

            return ret;
        }

        public virtual void convertBlockToRow(int numRows, int numCols, int blockLength,
            double[] data)
        {
            int tmpLength = Math.Min(blockLength, numRows) * numCols;

            if (tmp == null || tmp.Length < tmpLength)
                tmp = new double[tmpLength];

            MatrixOps_DDRB.convertBlockToRow(numRows, numCols, Ablock.blockLength, data, tmp);
        }

        public virtual bool inputModified()
        {
            return alg.inputModified();
        }
    }
}