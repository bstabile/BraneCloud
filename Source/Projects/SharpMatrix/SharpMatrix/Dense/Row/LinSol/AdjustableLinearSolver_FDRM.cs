using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.LinSol;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.LinSol
{
    //package org.ejml.dense.row.linsol;

/**
 * In many situations solutions to linear systems that share many of the same data points are needed.
 * This can happen when solving using the most recent data or when rejecting outliers.  In these situations
 * it is possible to solve these related systems much faster than solving the entire data set again.
 *
 * @see LinearSolverDense
 *
 * @author Peter Abeles
 */
    public interface AdjustableLinearSolver_FDRM : LinearSolverDense<FMatrixRMaj>
    {


        /**
         * Adds a row to A.  This has the same effect as creating a new A and calling {@link #setA}.
         *
         * @param A_row The row in A.
         * @param rowIndex Where the row appears in A.
         * @return if it succeeded or not.
         */
        bool addRowToA(float[]A_row, int rowIndex);

        /**
         * Removes a row from A.  This has the same effect as creating a new A and calling {@link #setA}.
         *
         * @param index which row is removed from A.
         * @return If it succeeded or not.
         */
        bool removeRowFromA(int index);
    }
}