using System;
using SharpMatrix.Data;
using SharpMatrix.Interfaces.LinSol;
using SharpMatrix.Sparse.Csc.Decomposition.Chol;
using SharpMatrix.Sparse.Csc.Decomposition.LU;
using SharpMatrix.Sparse.Csc.Decomposition.QR;
using SharpMatrix.Sparse.Csc.LinSol.Chol;
using SharpMatrix.Sparse.Csc.LinSol.LU;
using SharpMatrix.Sparse.Csc.LinSol.QR;

namespace SharpMatrix.Sparse.Csc.Factory
{
    //package org.ejml.sparse.csc.factory;

/**
 * Factory for sparse linear solvers
 *
 * @author Peter Abeles
 */
    public class LinearSolverFactory_DSCC
    {
        public static LinearSolverSparse<DMatrixSparseCSC, DMatrixRMaj> cholesky(FillReducing permutation)
        {
            ComputePermutation<DMatrixSparseCSC> cp = FillReductionFactory_DSCC.create(permutation);
            CholeskyUpLooking_DSCC chol = (CholeskyUpLooking_DSCC) DecompositionFactory_DSCC.cholesky();
            return new LinearSolverCholesky_DSCC(chol, cp);
        }

        public static LinearSolverSparse<DMatrixSparseCSC, DMatrixRMaj> qr(FillReducing permutation)
        {
            ComputePermutation<DMatrixSparseCSC> cp = FillReductionFactory_DSCC.create(permutation);
            QrLeftLookingDecomposition_DSCC qr = new QrLeftLookingDecomposition_DSCC(cp);
            return new LinearSolverQrLeftLooking_DSCC(qr);
        }

        public static LinearSolverSparse<DMatrixSparseCSC, DMatrixRMaj> lu(FillReducing permutation)
        {
            ComputePermutation<DMatrixSparseCSC> cp = FillReductionFactory_DSCC.create(permutation);
            LuUpLooking_DSCC lu = new LuUpLooking_DSCC(cp);
            return new LinearSolverLu_DSCC(lu);
        }
    }
}