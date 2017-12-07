using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Block.Decomposition.Hessenberg;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.Hessenberg
{
    //package org.ejml.dense.row.decomposition.hessenberg;

/**
 * Wrapper around a block implementation of TridiagonalSimilarDecomposition_F64
 *
 * @author Peter Abeles
 */
    public class TridiagonalDecomposition_DDRB_to_DDRM : BaseDecomposition_DDRB_to_DDRM,
        TridiagonalSimilarDecomposition_F64<DMatrixRMaj>
    {


        public TridiagonalDecomposition_DDRB_to_DDRM()
            : this(EjmlParameters.BLOCK_WIDTH)
        {
        }

        public TridiagonalDecomposition_DDRB_to_DDRM(int blockSize)
            : base(new TridiagonalDecompositionHouseholder_DDRB(), blockSize)
        {
        }

        public virtual DMatrixRMaj getT(DMatrixRMaj T)
        {
            int N = Ablock.numRows;

            if (T == null)
            {
                T = new DMatrixRMaj(N, N);
            }
            else
            {
                CommonOps_DDRM.fill(T, 0);
            }

            double[] diag = new double[N];
            double[] off = new double[N];

            ((TridiagonalDecompositionHouseholder_DDRB) alg).getDiagonal(diag, off);

            T.unsafe_set(0, 0, diag[0]);
            for (int i = 1; i < N; i++)
            {
                T.unsafe_set(i, i, diag[i]);
                T.unsafe_set(i, i - 1, off[i - 1]);
                T.unsafe_set(i - 1, i, off[i - 1]);
            }

            return T;
        }

        public virtual DMatrixRMaj getQ(DMatrixRMaj Q, bool transposed)
        {
            if (Q == null)
            {
                Q = new DMatrixRMaj(Ablock.numRows, Ablock.numCols);
            }

            DMatrixRBlock Qblock = new DMatrixRBlock();
            Qblock.numRows = Q.numRows;
            Qblock.numCols = Q.numCols;
            Qblock.blockLength = blockLength;
            Qblock.data = Q.data;

            ((TridiagonalDecompositionHouseholder_DDRB) alg).getQ(Qblock, transposed);

            convertBlockToRow(Q.numRows, Q.numCols, Ablock.blockLength, Q.data);

            return Q;
        }

        public virtual void getDiagonal(double[] diag, double[] off)
        {
            ((TridiagonalDecompositionHouseholder_DDRB) alg).getDiagonal(diag, off);
        }
    }
}