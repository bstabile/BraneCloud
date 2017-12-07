using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;


/**
 * <p>
 * An interface for performing matrix decompositions.
 * </p>
 *
 * <p>
 * A matrix decomposition is an algorithm which decomposes the input matrix into a set of equivalent
 * matrices that store the same information as the original.  Decompositions are useful
 * in that they allow specialized efficient algorithms to be run on generic input
 * matrices.
 * </p>
 *
 * <p>
 * By default most decompositions will modify the input matrix.  This is done to save
 * memory and simplify code by reducing the number of cases which need to be tested.
 * </p>
 *
 * @author Peter Abeles
 */
    public interface DecompositionInterface<T>
        where T : Matrix
    {

        /**
         * Computes the decomposition of the input matrix.  Depending on the implementation
         * the input matrix might be stored internally or modified.  If it is modified then
         * the function {@link #inputModified()} will return true and the matrix should not be
         * modified until the decomposition is no longer needed.
         *
         * @param orig The matrix which is being decomposed.  Modification is implementation dependent.
         * @return Returns if it was able to decompose the matrix.
         */
        bool decompose(T orig);

        /**
         * Is the input matrix to {@link #decompose(org.ejml.data.Matrix)} is modified during
         * the decomposition process.
         *
         * @return true if the input matrix to decompose() is modified.
         */
        bool inputModified();
    }
}