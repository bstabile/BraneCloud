using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.LinSol;
using BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.Decomposition.Chol;
using BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.Decomposition.LU;
using BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.Decomposition.QR;
using BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.LinSol.Chol;
using BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.LinSol.LU;
using BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.LinSol.QR;

namespace BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.Factory
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