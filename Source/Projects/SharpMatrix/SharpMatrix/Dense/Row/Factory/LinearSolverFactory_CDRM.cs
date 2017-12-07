using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.Chol;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.LU;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.LinSol.Chol;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.LinSol.LU;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.LinSol.QR;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.LinSol;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Factory
{
    //package org.ejml.dense.row.factory;

/**
 * Factory for creating linear solvers of complex matrices
 *
 * @author Peter Abeles
 */
    public class LinearSolverFactory_CDRM
    {

        /**
         * Creates a linear solver which uses LU decomposition internally
         *
         * @param matrixSize Approximate of rows and columns
         * @return Linear solver
         */
        public static LinearSolverDense<CMatrixRMaj> lu(int matrixSize)
        {
            return new LinearSolverLu_CDRM(new LUDecompositionAlt_CDRM());
        }

        /**
         * Creates a linear solver which uses Cholesky decomposition internally
         *
         * @param matrixSize Approximate of rows and columns
         * @return Linear solver
         */
        public static LinearSolverDense<CMatrixRMaj> chol(int matrixSize)
        {
            return new LinearSolverChol_CDRM(new CholeskyDecompositionInner_CDRM());
        }

        /**
         * Creates a linear solver which uses QR decomposition internally
         *
         * @param numRows Approximate of rows
         * @param numCols Approximate of columns
         * @return Linear solver
         */
        public static LinearSolverDense<CMatrixRMaj> qr(int numRows, int numCols)
        {
            return new LinearSolverQrHouseCol_CDRM();
        }
    }
}