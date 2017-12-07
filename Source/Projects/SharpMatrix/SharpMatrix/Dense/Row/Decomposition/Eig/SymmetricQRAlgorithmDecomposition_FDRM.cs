using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.Eig.Symm;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Factory;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.Eig
{
    //package org.ejml.dense.row.decomposition.eig;

/**
 * <p>
 * Computes the eigenvalues and eigenvectors of a real symmetric matrix using the symmetric implicit QR algorithm.
 * Inside each iteration a QR decomposition of A<sub>i</sub>-p<sub>i</sub>I is implicitly computed.
 * </p>
 * <p>
 * This implementation is based on the algorithm is sketched out in:<br>
 * David S. Watkins, "Fundamentals of Matrix Computations," Second Edition. page 377-385
 * </p>
 *
 * @see SymmetricQrAlgorithm_FDRM
 * @see org.ejml.dense.row.decomposition.hessenberg.TridiagonalDecompositionHouseholder_FDRM
 *
 * @author Peter Abeles
 */
    public class SymmetricQRAlgorithmDecomposition_FDRM : EigenDecomposition_F32<FMatrixRMaj>
    {

        // computes a tridiagonal matrix whose eigenvalues are the same as the original
        // matrix and can be easily computed.
        private TridiagonalSimilarDecomposition_F32<FMatrixRMaj> decomp;

        // helper class for eigenvalue and eigenvector algorithms
        private SymmetricQREigenHelper_FDRM helper;

        // computes the eigenvectors
        private SymmetricQrAlgorithm_FDRM vector;

        // should it compute eigenvectors at the same time as the eigenvalues?
        private bool computeVectorsWithValues = false;

        // where the found eigenvalues are stored
        private float[] values;

        // where the tridiagonal matrix is stored
        private float[] diag;

        private float[] off;

        private float[] diagSaved;
        private float[] offSaved;

        // temporary variable used to store/compute eigenvectors
        private FMatrixRMaj V;

        // the extracted eigenvectors
        private FMatrixRMaj[] eigenvectors;

        // should it compute eigenvectors or just eigenvalues
        bool computeVectors;

        public SymmetricQRAlgorithmDecomposition_FDRM(TridiagonalSimilarDecomposition_F32<FMatrixRMaj> decomp,
            bool computeVectors)
        {

            this.decomp = decomp;
            this.computeVectors = computeVectors;

            helper = new SymmetricQREigenHelper_FDRM();

            vector = new SymmetricQrAlgorithm_FDRM(helper);
        }

        public SymmetricQRAlgorithmDecomposition_FDRM(bool computeVectors)
            : this(DecompositionFactory_FDRM.tridiagonal(0), computeVectors)
        {

        }

        public void setComputeVectorsWithValues(bool computeVectorsWithValues)
        {
            if (!computeVectors)
                throw new ArgumentException("Compute eigenvalues has been set to false");

            this.computeVectorsWithValues = computeVectorsWithValues;
        }

        /**
         * Used to limit the number of internal QR iterations that the QR algorithm performs.  20
         * should be enough for most applications.
         *
         * @param max The maximum number of QR iterations it will perform.
         */
        public void setMaxIterations(int max)
        {
            vector.setMaxIterations(max);
        }

        public virtual int getNumberOfEigenvalues()
        {
            return helper.getMatrixSize();
        }

        public virtual Complex_F32 getEigenvalue(int index)
        {
            return new Complex_F32(values[index], 0);
        }

        public virtual FMatrixRMaj getEigenVector(int index)
        {
            return eigenvectors[index];
        }

        /**
         * Decomposes the matrix using the QR algorithm.  Care was taken to minimize unnecessary memory copying
         * and cache skipping.
         *
         * @param orig The matrix which is being decomposed.  Not modified.
         * @return true if it decomposed the matrix or false if an error was detected.  This will not catch all errors.
         */
        public virtual bool decompose(FMatrixRMaj orig)
        {
            if (orig.numCols != orig.numRows)
                throw new ArgumentException("Matrix must be square.");
            if (orig.numCols <= 0)
                return false;

            int N = orig.numRows;

            // compute a similar tridiagonal matrix
            if (!decomp.decompose(orig))
                return false;

            if (diag == null || diag.Length < N)
            {
                diag = new float[N];
                off = new float[N - 1];
            }
            decomp.getDiagonal(diag, off);

            // Tell the helper to work with this matrix
            helper.init(diag, off, N);

            if (computeVectors)
            {
                if (computeVectorsWithValues)
                {
                    return extractTogether();
                }
                else
                {
                    return extractSeparate(N);
                }
            }
            else
            {
                return computeEigenValues();
            }
        }

        public virtual bool inputModified()
        {
            return decomp.inputModified();
        }

        private bool extractTogether()
        {
            // extract the orthogonal from the similar transform
            V = decomp.getQ(V, true);

            // tell eigenvector algorithm to update this matrix as it computes the rotators
            helper.setQ(V);

            vector.setFastEigenvalues(false);

            // extract the eigenvalues
            if (!vector.process(-1, null, null))
                return false;

            // the V matrix contains the eigenvectors.  Convert those into column vectors
            eigenvectors = CommonOps_FDRM.rowsToVector(V, eigenvectors);

            // save a copy of them since this data structure will be recycled next
            values = helper.copyEigenvalues(values);

            return true;
        }

        private bool extractSeparate(int numCols)
        {
            if (!computeEigenValues())
                return false;

            // ---- set up the helper to decompose the same tridiagonal matrix
            // swap arrays instead of copying them to make it slightly faster
            helper.reset(numCols);
            diagSaved = helper.swapDiag(diagSaved);
            offSaved = helper.swapOff(offSaved);

            // extract the orthogonal from the similar transform
            V = decomp.getQ(V, true);

            // tell eigenvector algorithm to update this matrix as it computes the rotators
            vector.setQ(V);

            // extract eigenvectors
            if (!vector.process(-1, null, null, values))
                return false;

            // the ordering of the eigenvalues might have changed
            values = helper.copyEigenvalues(values);
            // the V matrix contains the eigenvectors.  Convert those into column vectors
            eigenvectors = CommonOps_FDRM.rowsToVector(V, eigenvectors);

            return true;
        }

        /**
          * Computes eigenvalues only
         *
          * @return
          */
        private bool computeEigenValues()
        {
            // make a copy of the internal tridiagonal matrix data for later use
            diagSaved = helper.copyDiag(diagSaved);
            offSaved = helper.copyOff(offSaved);

            vector.setQ(null);
            vector.setFastEigenvalues(true);

            // extract the eigenvalues
            if (!vector.process(-1, null, null))
                return false;

            // save a copy of them since this data structure will be recycled next
            values = helper.copyEigenvalues(values);
            return true;
        }
    }
}