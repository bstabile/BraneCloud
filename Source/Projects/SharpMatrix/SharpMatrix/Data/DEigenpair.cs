using System;
using SharpMatrix.Data;

namespace SharpMatrix.Dense.Block
{
    //package org.ejml.data;


/**
 * An eigenpair is a set composed of an eigenvalue and an eigenvector.  In this library since only real
 * matrices are supported, all eigenpairs are real valued.
 *
 * @author Peter Abeles
 */
    public class DEigenpair
    {
        public double value;
        public DMatrixRMaj vector;

        public DEigenpair(double value, DMatrixRMaj vector)
        {
            this.value = value;
            this.vector = vector;
        }
    }
}