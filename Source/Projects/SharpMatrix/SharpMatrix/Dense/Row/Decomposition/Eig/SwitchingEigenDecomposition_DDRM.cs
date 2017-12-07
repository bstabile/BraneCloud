using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Factory;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.Eig
{
    //package org.ejml.dense.row.decomposition.eig;

/**
 * Checks to see what type of matrix is being decomposed and calls different eigenvalue decomposition
 * algorithms depending on the results.  This primarily checks to see if the matrix is symmetric or not.
 *
 *
 * @author Peter Abeles
 */
    public class SwitchingEigenDecomposition_DDRM
        : EigenDecomposition_F64<DMatrixRMaj>
    {
        // tolerance used in deciding if a matrix is symmetric or not
        private double tol;

        EigenDecomposition_F64<DMatrixRMaj> symmetricAlg;
        EigenDecomposition_F64<DMatrixRMaj> generalAlg;

        bool symmetric;

        // should it compute eigenvectors or just eigenvalues?
        bool computeVectors;

        DMatrixRMaj A = new DMatrixRMaj(1, 1);

        /**
         *
         * @param computeVectors
         * @param tol Tolerance for a matrix being symmetric
         */
        public SwitchingEigenDecomposition_DDRM(int matrixSize, bool computeVectors, double tol)
        {
            symmetricAlg = DecompositionFactory_DDRM.eig(matrixSize, computeVectors, true);
            generalAlg = DecompositionFactory_DDRM.eig(matrixSize, computeVectors, false);
            this.computeVectors = computeVectors;
            this.tol = tol;
        }

        public SwitchingEigenDecomposition_DDRM(int matrixSize)
            : this(matrixSize, true, UtilEjml.TEST_F64)
        {
        }

        public virtual int getNumberOfEigenvalues()
        {
            return symmetric ? symmetricAlg.getNumberOfEigenvalues() : generalAlg.getNumberOfEigenvalues();
        }

        public virtual Complex_F64 getEigenvalue(int index)
        {
            return symmetric ? symmetricAlg.getEigenvalue(index) : generalAlg.getEigenvalue(index);
        }

        public virtual DMatrixRMaj getEigenVector(int index)
        {
            if (!computeVectors)
                throw new ArgumentException("Configured to not compute eignevectors");

            return symmetric ? symmetricAlg.getEigenVector(index) : generalAlg.getEigenVector(index);
        }

        public virtual bool decompose(DMatrixRMaj orig)
        {
            A.set(orig);

            symmetric = MatrixFeatures_DDRM.isSymmetric(A, tol);

            return symmetric ? symmetricAlg.decompose(A) : generalAlg.decompose(A);

        }

        public virtual bool inputModified()
        {
            // since it doesn't know which algorithm will be used until a matrix is provided make a copy
            // of all inputs
            return false;
        }
    }
}