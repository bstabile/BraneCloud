using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * <p>
 * QR decompositions decompose a rectangular matrix 'A' such that 'A=QR'.  Where
 * A &isin; &real; <sup>n &times; m</sup> , n &ge; m, Q &isin; &real; <sup>n &times; n</sup> is an orthogonal matrix,
 * and R &isin; &real; <sup>n &times; m</sup> is an upper triangular matrix.  Some implementations
 * of QR decomposition require that A has full rank.
 * </p>
 *
 * Some features of QR decompositions:
 * <ul>
 * <li> Can decompose rectangular matrices. </li>
 * <li> Numerically stable solutions to least-squares problem, but not as stable as SVD </li>
 * <li> Can incrementally add and remove columns from the decomposed matrix.  See {@link org.ejml.dense.row.linsol.qr.AdjLinearSolverQr_DDRM} </li>
 * </ul>
 *
 * Orthogonal matrices have the following properties:
 * <ul>
 * <li>QQ<sup>T</sup>=I</li>
 * <li>Q<sup>T</sup>=Q<sup>-1</sup></li>
 * </ul>
 *
 * @author Peter Abeles
 */
    public interface QRDecomposition<T> : DecompositionInterface<T>
        where T : Matrix
    {
        /**
         * <p>
         * Returns the Q matrix from the decomposition.  Should only
         * be called after {@link #decompose(org.ejml.data.Matrix)} has
         * been called.
         * </p>
         *
         * <p>
         * If parameter Q is not null, then that matrix is used to store the Q matrix.  Otherwise
         * a new matrix is created.
         * </p>
         *
         * @param Q If not null then the Q matrix is written to it.  Modified.
         * @param compact If true an m by n matrix is created, otherwise n by n.
         * @return The Q matrix.
         */
        T getQ(T Q, bool compact);

        /**
         * <p>
         * Returns the R matrix from the decomposition.  Should only be
         * called after {@link #decompose(org.ejml.data.Matrix)} has been.
         * </p>
         * <p>
         * If setZeros is true then an n &times; m matrix is required and all the elements are set.
         * If setZeros is false then the matrix must be at least m &times; m and only the upper triangular
         * elements are set.
         * </p>
         *
         * <p>
         * If parameter R is not null, then that matrix is used to store the R matrix.  Otherwise
         * a new matrix is created.
         * </p>
         *
         * @param R If not null then the R matrix is written to it. Modified.
         * @param compact If true only the upper triangular elements are set
         * @return The R matrix.
         */
        T getR(T R, bool compact);
    }
}