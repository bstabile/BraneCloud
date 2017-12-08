using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Block.Decomposition.Hessenberg;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Row.Decomposition.Hessenberg
{
    //package org.ejml.dense.row.decomposition.hessenberg;

/**
 * Wrapper around a block implementation of TridiagonalSimilarDecomposition_F32
 *
 * @author Peter Abeles
 */
    public class TridiagonalDecomposition_FDRB_to_FDRM
        : BaseDecomposition_FDRB_to_FDRM, TridiagonalSimilarDecomposition_F32<FMatrixRMaj>
    {


        public TridiagonalDecomposition_FDRB_to_FDRM()
            : this(EjmlParameters.BLOCK_WIDTH)
        {
        }

        public TridiagonalDecomposition_FDRB_to_FDRM(int blockSize)
            : base(new TridiagonalDecompositionHouseholder_FDRB(), blockSize)
        {
        }

        public virtual FMatrixRMaj getT(FMatrixRMaj T)
        {
            int N = Ablock.numRows;

            if (T == null)
            {
                T = new FMatrixRMaj(N, N);
            }
            else
            {
                CommonOps_FDRM.fill(T, 0);
            }

            float[] diag = new float[N];
            float[] off = new float[N];

            ((TridiagonalDecompositionHouseholder_FDRB) alg).getDiagonal(diag, off);

            T.unsafe_set(0, 0, diag[0]);
            for (int i = 1; i < N; i++)
            {
                T.unsafe_set(i, i, diag[i]);
                T.unsafe_set(i, i - 1, off[i - 1]);
                T.unsafe_set(i - 1, i, off[i - 1]);
            }

            return T;
        }

        public virtual FMatrixRMaj getQ(FMatrixRMaj Q, bool transposed)
        {
            if (Q == null)
            {
                Q = new FMatrixRMaj(Ablock.numRows, Ablock.numCols);
            }

            FMatrixRBlock Qblock = new FMatrixRBlock();
            Qblock.numRows = Q.numRows;
            Qblock.numCols = Q.numCols;
            Qblock.blockLength = blockLength;
            Qblock.data = Q.data;

            ((TridiagonalDecompositionHouseholder_FDRB) alg).getQ(Qblock, transposed);

            convertBlockToRow(Q.numRows, Q.numCols, Ablock.blockLength, Q.data);

            return Q;
        }

        public virtual void getDiagonal(float[] diag, float[] off)
        {
            ((TridiagonalDecompositionHouseholder_FDRB) alg).getDiagonal(diag, off);
        }
    }
}