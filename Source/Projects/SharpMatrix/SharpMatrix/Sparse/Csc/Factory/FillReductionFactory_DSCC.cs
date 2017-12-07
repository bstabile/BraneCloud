using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using Randomization;

namespace BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.Factory
{
    //package org.ejml.sparse.csc.factory;

/**
 * @author Peter Abeles
 */
    public class FillReductionFactory_DSCC
    {
        public static IMersenneTwister rand = new MersenneTwisterFast(234234);

        public static ComputePermutation<DMatrixSparseCSC> create(FillReducing type)
        {
            switch (type)
            {
                case FillReducing.NONE:
                    return null;

                case FillReducing.RANDOM:
                    // TODO : Implement!
                    throw new NotImplementedException();
                //return new ComputePermutation<DMatrixSparseCSC>(true, true) {
                //@Override
                //public void process(DMatrixSparseCSC m)
                //{
                //    prow.reshape(m.numRows);
                //    pcol.reshape(m.numCols);
                //    fillSequence(prow);
                //    fillSequence(pcol);
                //    Random _rand;
                //    synchronized(rand) {
                //        _rand = new Random(rand.nextInt());
                //    }
                //    UtilEjml.shuffle(prow.data, prow.Length, 0, prow.Length, _rand);
                //    UtilEjml.shuffle(pcol.data, pcol.Length, 0, pcol.Length, _rand);
                //}
                //};

                case FillReducing.IDENTITY:
                    // TODO : Implement!
                    throw new NotImplementedException();
                //return new ComputePermutation<DMatrixSparseCSC>(true,true) {
                //    //@Override
                //    public void process(DMatrixSparseCSC m) {
                //        prow.reshape(m.numRows);
                //        pcol.reshape(m.numCols);
                //        fillSequence(prow);
                //        fillSequence(pcol);
                //    }
                //};

                default:
                    throw new InvalidOperationException("Unknown " + type);
            }
        }

        private static void fillSequence(IGrowArray perm)
        {
            for (int i = 0; i < perm.Length; i++)
            {
                perm.data[i] = i;
            }
        }
    }
}