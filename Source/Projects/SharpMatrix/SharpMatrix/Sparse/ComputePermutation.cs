using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Sparse
{
    //package org.ejml.sparse;

/**
 * @author Peter Abeles
 */
    public abstract class ComputePermutation<T>
        where T : Matrix
    {

        protected IGrowArray prow;
        protected IGrowArray pcol;

        public ComputePermutation(bool hasRow, bool hasCol)
        {
            if (hasRow)
                prow = new IGrowArray();
            if (hasCol)
                pcol = new IGrowArray();
        }

        public abstract void process(T m);

        /**
         * Returns row permutation
         */
        public IGrowArray getRow()
        {
            return prow;
        }

        /**
         * Returns column permutation
         */
        public IGrowArray getColumn()
        {
            return pcol;
        }

        public bool hasRowPermutation()
        {
            return prow != null;
        }

        public bool hasColumnPermutation()
        {
            return pcol != null;
        }
    }
}