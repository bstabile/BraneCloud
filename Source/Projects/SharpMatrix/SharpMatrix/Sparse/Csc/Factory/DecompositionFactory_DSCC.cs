using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;
using BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.Decomposition.Chol;
using BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.Decomposition.LU;
using BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.Decomposition.QR;

namespace BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.Factory
{
    //package org.ejml.sparse.csc.factory;

/**
 * Factory for sparse matrix decompositions
 *
 * @author Peter Abeles
 */
    public class DecompositionFactory_DSCC
    {
        public static CholeskySparseDecomposition_F64<DMatrixSparseCSC> cholesky()
        {
            return new CholeskyUpLooking_DSCC();
        }

        public static QRSparseDecomposition<DMatrixSparseCSC> qr(FillReducing permutation)
        {
            ComputePermutation<DMatrixSparseCSC> cp = FillReductionFactory_DSCC.create(permutation);
            return new QrLeftLookingDecomposition_DSCC(cp);
        }

        public static LUSparseDecomposition_F64<DMatrixSparseCSC> lu(FillReducing permutation)
        {
            ComputePermutation<DMatrixSparseCSC> cp = FillReductionFactory_DSCC.create(permutation);
            return new LuUpLooking_DSCC(cp);
        }
    }
}