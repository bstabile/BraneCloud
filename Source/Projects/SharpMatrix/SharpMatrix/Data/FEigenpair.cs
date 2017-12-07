using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Block
{
    //package org.ejml.data;


/**
 * An eigenpair is a set composed of an eigenvalue and an eigenvector.  In this library since only real
 * matrices are supported, all eigenpairs are real valued.
 *
 * @author Peter Abeles
 */
    public class FEigenpair
    {
        public float value;
        public FMatrixRMaj vector;

        public FEigenpair(float value, FMatrixRMaj vector)
        {
            this.value = value;
            this.vector = vector;
        }
    }
}