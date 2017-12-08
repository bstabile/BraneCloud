using System;
using SharpMatrix.Data;

namespace SharpMatrix.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * @author Peter Abeles
 */
    public interface LUSparseDecomposition<T> : LUDecomposition<T>, DecompositionSparseInterface<T>
        where T : Matrix
    {
    }
}