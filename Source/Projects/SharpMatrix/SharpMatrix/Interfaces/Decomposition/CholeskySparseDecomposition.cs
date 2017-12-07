using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition
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