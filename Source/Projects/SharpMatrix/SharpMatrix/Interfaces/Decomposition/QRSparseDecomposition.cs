using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition
{
    //package org.ejml.interfaces.decomposition;

/**
 * <p>Sparse {@link QRDecomposition}</p>
 *
 * @author Peter Abeles
 */
    public interface QRSparseDecomposition<T> : QRDecomposition<T>, DecompositionSparseInterface<T>
        where T : Matrix
    {
    }
}
