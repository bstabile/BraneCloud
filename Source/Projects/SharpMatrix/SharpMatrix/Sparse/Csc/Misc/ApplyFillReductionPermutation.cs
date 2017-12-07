using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.Misc
{
    //package org.ejml.sparse.csc.misc;

/**
 * Applies the fill reduction row pivots to the input matrix to reduce fill in during decomposition/solve.
 *
 * P*A*Q where P are row pivots and Q are column pivots.
 *
 * @author Peter Abeles
 */
    public class ApplyFillReductionPermutation
    {
        // fill reduction permutation
        private ComputePermutation<DMatrixSparseCSC> fillReduce;

        // storage for permuted A matrix
        DMatrixSparseCSC Aperm = new DMatrixSparseCSC(1, 1, 0);

        int[] pinv = new int[1]; // inverse row pivots

        IGrowArray gw = new IGrowArray();

        bool symmetric;

        public ApplyFillReductionPermutation(ComputePermutation<DMatrixSparseCSC> fillReduce,
            bool symmetric)
        {
            this.fillReduce = fillReduce;
            this.symmetric = symmetric;
        }

        /**
         * Computes and applies the fill reduction permutation. Either A is returned (unmodified) or the permutated
         * version of A.
         * @param A Input matrix. unmodified.
         * @return A permuted matrix. Might be A or a different matrix.
         */
        public DMatrixSparseCSC apply(DMatrixSparseCSC A)
        {
            if (fillReduce == null)
                return A;
            fillReduce.process(A);

            IGrowArray gp = fillReduce.getRow();

            if (pinv.Length < gp.Length)
                pinv = new int[gp.Length];
            CommonOps_DSCC.permutationInverse(gp.data, pinv, gp.Length);
            if (symmetric)
                CommonOps_DSCC.permuteSymmetric(A, pinv, Aperm, gw);
            else
                CommonOps_DSCC.permuteRowInv(pinv, A, Aperm);
            return Aperm;
        }

        public int[] getArrayPinv()
        {
            return fillReduce == null ? null : pinv;
        }

        public int[] getArrayP()
        {
            return fillReduce == null ? null : fillReduce.getRow().data;
        }

        public int[] getArrayQ()
        {
            return fillReduce == null ? null : fillReduce.getColumn().data;
        }

        public IGrowArray getGw()
        {
            return gw;
        }

        public void setGw(IGrowArray gw)
        {
            this.gw = gw;
        }

        public ComputePermutation<DMatrixSparseCSC> getFillReduce()
        {
            return fillReduce;
        }

        public bool isApplied()
        {
            return fillReduce != null;
        }
    }
}