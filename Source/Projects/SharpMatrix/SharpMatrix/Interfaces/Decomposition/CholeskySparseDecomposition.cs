using System;
using SharpMatrix.Data;

namespace SharpMatrix.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * @author Peter Abeles
 */
    public interface CholeskySparseDecomposition<T>
        : CholeskyDecomposition<T>, DecompositionSparseInterface<T>
        where T : Matrix
    {
    }
}