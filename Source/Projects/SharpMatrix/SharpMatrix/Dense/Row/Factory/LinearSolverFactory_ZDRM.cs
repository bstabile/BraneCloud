using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row.Decomposition.Chol;
using SharpMatrix.Dense.Row.Decomposition.LU;
using SharpMatrix.Dense.Row.LinSol.Chol;
using SharpMatrix.Dense.Row.LinSol.LU;
using SharpMatrix.Dense.Row.LinSol.QR;
using SharpMatrix.Interfaces.Decomposition;
using SharpMatrix.Interfaces.LinSol;

namespace SharpMatrix.Dense.Row.Factory
{
    //package org.ejml.dense.row.factory;

/**
 * Factory for creating linear solvers of complex matrices
 *
 * @author Peter Abeles
 */
    public class LinearSolverFactory_ZDRM
    {

        /**
         * Creates a linear solver which uses LU decomposition internally
         *
         * @param matrixSize Approximate of rows and columns
         * @return Linear solver
         */
        public static LinearSolverDense<ZMatrixRMaj> lu(int matrixSize)
        {
            return new LinearSolverLu_ZDRM(new LUDecompositionAlt_ZDRM());
        }

        /**
         * Creates a linear solver which uses Cholesky decomposition internally
         *
         * @param matrixSize Approximate of rows and columns
         * @return Linear solver
         */
        public static LinearSolverDense<ZMatrixRMaj> chol(int matrixSize)
        {
            return new LinearSolverChol_ZDRM(new CholeskyDecompositionInner_ZDRM());
        }

        /**
         * Creates a linear solver which uses QR decomposition internally
         *
         * @param numRows Approximate of rows
         * @param numCols Approximate of columns
         * @return Linear solver
         */
        public static LinearSolverDense<ZMatrixRMaj> qr(int numRows, int numCols)
        {
            return new LinearSolverQrHouseCol_ZDRM();
        }
    }
}