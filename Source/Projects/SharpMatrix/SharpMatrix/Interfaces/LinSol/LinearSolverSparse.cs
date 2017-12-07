using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Interfaces.LinSol
{
    //package org.ejml.interfaces.linsol;

/**

 * @author Peter Abeles
 */
// Ideas. Base linear solver will have two inputs
//    Matrix type specific solvers, e.g. LinearSolver_DDRM implements LinearSolverDense<DMatrixRMaj,DMatrixRMaj>
// have sparse solver extend LinearSolverDense but add functions to it
    public interface LinearSolverSparse<S, D> : LinearSolver<S, D>
        where S : Matrix
        where D : Matrix
    {

        /**
         * <p>Save results from structural analysis step. This can reduce computations of a matrix with the exactly same
         * non-zero pattern is decomposed in the future.  If a matrix has yet to be processed then the structure of
         * the next matrix is saved. If a matrix has already been processed then the structure of the most recently
         * processed matrix will be saved.</p>
         */
        void lockStructure();

        /**
         * Checks to see if the structure is locked.
         * @return true if locked or false if not locked.
         */
        bool isStructureLocked();

    }
}