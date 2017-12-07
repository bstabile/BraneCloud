using System;
using System.Collections.Generic;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Factory;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;

namespace BraneCloud.Evolution.EC.MatrixLib.Simple
{
    //package org.ejml.simple;

    /**
     * Wrapper around EigenDecomposition for SimpleMatrix
     *
     * @author Peter Abeles
     */
    //@SuppressWarnings({"unchecked"})
    public class SimpleEVD<T> where T : class, Matrix
    {
        private EigenDecomposition<T> eig;

        T mat;
        bool is64;

        public SimpleEVD(T mat)
        {
            this.mat = mat;
            this.is64 = mat is DMatrixRMaj;

            if (is64)
            {
                eig = (EigenDecomposition<T>) DecompositionFactory_DDRM.eig(mat.getNumCols(), true);
            }
            else
            {
                eig = (EigenDecomposition<T>) DecompositionFactory_FDRM.eig(mat.getNumCols(), true);

            }
            if (!eig.decompose((T) mat))
                throw new InvalidOperationException("Eigenvalue Decomposition failed");
        }

        /**
         * Returns a list of all the eigenvalues
         */
        public List<Complex_F64> getEigenvalues()
        {
            List<Complex_F64> ret = new List<Complex_F64>();

            if (is64)
            {
                var d = (EigenDecomposition_F64<DMatrixRMaj>) eig;
                for (int i = 0; i < eig.getNumberOfEigenvalues(); i++)
                {
                    ret.Add(d.getEigenvalue(i));
                }
            }
            else
            {
                var d = (EigenDecomposition_F32<FMatrixRMaj>) eig;
                for (int i = 0; i < eig.getNumberOfEigenvalues(); i++)
                {
                    Complex_F32 c = d.getEigenvalue(i);
                    ret.Add(new Complex_F64(c.real, c.imaginary));
                }
            }

            return ret;
        }

        /**
         * Returns the number of eigenvalues/eigenvectors.  This is the matrix's dimension.
         *
         * @return number of eigenvalues/eigenvectors.
         */
        public int getNumberOfEigenvalues()
        {
            return eig.getNumberOfEigenvalues();
        }

        /**
         * <p>
         * Returns an eigenvalue as a complex number.  For symmetric matrices the returned eigenvalue will always be a real
         * number, which means the imaginary component will be equal to zero.
         * </p>
         *
         * <p>
         * NOTE: The order of the eigenvalues is dependent upon the decomposition algorithm used.  This means that they may
         * or may not be ordered by magnitude.  For example the QR algorithm will returns results that are partially
         * ordered by magnitude, but this behavior should not be relied upon.
         * </p>
         *
         * @param index Index of the eigenvalue eigenvector pair.
         * @return An eigenvalue.
         */
        public Complex_F64 getEigenvalue(int index)
        {
            if (is64)
                return ((EigenDecomposition_F64<DMatrixRMaj>)eig).getEigenvalue(index);
            else
            {
                Complex_F64 c = ((EigenDecomposition_F64<FMatrixRMaj>)eig).getEigenvalue(index);
                return new Complex_F64(c.real, c.imaginary);
            }
        }

        /**
         * <p>
         * Used to retrieve real valued eigenvectors.  If an eigenvector is associated with a complex eigenvalue
         * then null is returned instead.
         * </p>
         *
         * @param index Index of the eigenvalue eigenvector pair.
         * @return If the associated eigenvalue is real then an eigenvector is returned, null otherwise.
         */
        public SimpleBase<T> getEigenVector(int index)
        {
            T v = eig.getEigenVector(index);
            if (v == null)
                return null;
            return SimpleMatrix<T>.wrap(v);
        }

        /**
         * <p>
         * Computes the quality of the computed decomposition.  A value close to or less than 1e-15
         * is considered to be within machine precision.
         * </p>
         *
         * <p>
         * This function must be called before the original matrix has been modified or else it will
         * produce meaningless results.
         * </p>
         *
         * @return Quality of the decomposition.
         */
        public /**/ double quality()
        {
            if (is64)
            {
                return DecompositionFactory_DDRM.quality(mat as DMatrixRMaj, (EigenDecomposition_F64<DMatrixRMaj>)eig);
            }
            else
            {
                return DecompositionFactory_FDRM.quality(mat as FMatrixRMaj, (EigenDecomposition_F32<FMatrixRMaj>)eig);
            }
        }

        /**
         * Returns the underlying decomposition that this is a wrapper around.
         *
         * @return EigenDecomposition
         */
        public EigenDecomposition<T> getEVD()
        {
            return eig;
        }

        /**
         * Returns the index of the eigenvalue which has the largest magnitude.
         *
         * @return index of the largest magnitude eigen value.
         */
        public int getIndexMax()
        {
            int indexMax = 0;
            double max = getEigenvalue(0).getMagnitude2();

            int N = getNumberOfEigenvalues();
            for (int i = 1; i < N; i++)
            {
                double m = getEigenvalue(i).getMagnitude2();
                if (m > max)
                {
                    max = m;
                    indexMax = i;
                }
            }

            return indexMax;
        }

        /**
         * Returns the index of the eigenvalue which has the smallest magnitude.
         *
         * @return index of the smallest magnitude eigen value.
         */
        public int getIndexMin()
        {
            int indexMin = 0;
            double min = getEigenvalue(0).getMagnitude2();

            int N = getNumberOfEigenvalues();
            for (int i = 1; i < N; i++)
            {
                double m = getEigenvalue(i).getMagnitude2();
                if (m < min)
                {
                    min = m;
                    indexMin = i;
                }
            }

            return indexMin;
        }
    }
}