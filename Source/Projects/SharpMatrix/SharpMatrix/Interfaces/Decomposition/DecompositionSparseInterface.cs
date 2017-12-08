using System;
using SharpMatrix.Data;

namespace SharpMatrix.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * Decomposition for sparse matrices. For direct solvers the structure often needs to be determined first. This
 * interface provides the capability to lock that structure in place and speed up future calculations.
 *
 * @param <T>
 */
    public interface DecompositionSparseInterface<T> : DecompositionInterface<T>
        where T : Matrix
    {
        /**
         * <p>Save results from structural analysis step. This can reduce computations if a matrix with the exactly same
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
