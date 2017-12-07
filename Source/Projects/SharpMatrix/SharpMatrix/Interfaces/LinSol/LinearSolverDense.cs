using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Interfaces.LinSol
{
    //package org.ejml.interfaces.linsol;

/**
 * <p>
 * An implementation of LinearSolverDense solves a linear system or inverts a matrix.  It masks more complex
 * implementation details, while giving the programmer control over memory management and performance.
 * To quickly detect nearly singular matrices without computing the SVD the {@link #quality()}
 * function is provided.
 * </p>
 *
 * <p>
 * A linear system is defined as:
 * A*X = B.<br>
 * where A &isin; &real; <sup>m &times; n</sup>, X &isin; &real; <sup>n &times; p</sup>,
 * B &isin; &real; <sup>m &times; p</sup>.  Different implementations can solve different
 * types and shapes in input matrices and have different memory and runtime performance.
 *</p>
 *
 * To solve a system:<br>
 * <ol>
 * <li> Call {@link #setA(org.ejml.data.Matrix)}
 * <li> Call {@link #solve(org.ejml.data.Matrix, org.ejml.data.Matrix)}.
 * </ol>
 *
 * <p>To invert a matrix:</p>
 * <ol>
 * <li> Call {@link #setA(org.ejml.data.Matrix)}
 * <li> Call {@link #invert(org.ejml.data.Matrix)}.
 * </ol>
 * <p>
 * A matrix can also be inverted by passing in an identity matrix to solve, but this will be
 * slower and more memory intensive than the specialized invert() function.
 * </p>
 *
 * <p>
 * <b>IMPORTANT:</b> Depending upon the implementation, input matrices might be overwritten by
 * the solver.  This
 * reduces memory and computational requirements and give more control to the programmer.  If
 * the input matrices need to be not modified then {@link LinearSolverSafe} can be used.  The
 * functions {@link #modifiesA()} and {@link #modifiesB()} specify which input matrices are being
 * modified.
 * </p>
 *
 * @author Peter Abeles
 */
    public interface LinearSolverDense<T> : LinearSolver<T, T>
        where T : Matrix
    {

        /**
         * Computes the inverse of of the 'A' matrix passed into {@link #setA(Matrix)}
         * and writes the results to the provided matrix.  If 'A_inv' needs to be different from 'A'
         * is implementation dependent.
         *
         * @param A_inv Where the inverted matrix saved. Modified.
         */
        void invert(T A_inv);

    }
}