using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row;

namespace SharpMatrix.Examples
{
    //package org.ejml.example;

/**
 * <p>
 * Example code for computing the QR decomposition of a matrix. It demonstrates how to
 * extract a submatrix and insert one matrix into another one using the operator interface.
 * </p>
 *
 * Note: This code is horribly inefficient and is for demonstration purposes only.
 *
 * @author Peter Abeles
 */
    public class QRExampleOperations
    {

        // where the QR decomposition is stored
        private DMatrixRMaj QR;

        // used for computing Q
        private double[] gammas;

        /**
         * Computes the QR decomposition of the provided matrix.
         *
         * @param A Matrix which is to be decomposed.  Not modified.
         */
        public void decompose(DMatrixRMaj A)
        {

            this.QR = (DMatrixRMaj) A.copy();

            int N = Math.Min(A.numCols, A.numRows);

            gammas = new double[A.numCols];

            DMatrixRMaj A_small = new DMatrixRMaj(A.numRows, A.numCols);
            DMatrixRMaj A_mod = new DMatrixRMaj(A.numRows, A.numCols);
            DMatrixRMaj v = new DMatrixRMaj(A.numRows, 1);
            DMatrixRMaj Q_k = new DMatrixRMaj(A.numRows, A.numRows);

            for (int i = 0; i < N; i++)
            {
                // reshape temporary variables
                A_small.reshape(QR.numRows - i, QR.numCols - i, false);
                A_mod.reshape(A_small.numRows, A_small.numCols, false);
                v.reshape(A_small.numRows, 1, false);
                Q_k.reshape(v.getNumElements(), v.getNumElements(), false);

                // use extract matrix to get the column that is to be zeroed
                CommonOps_DDRM.extract(QR, i, QR.numRows, i, i + 1, v, 0, 0);

                double max = CommonOps_DDRM.elementMaxAbs(v);

                if (max > 0 && v.getNumElements() > 1)
                {
                    // normalize to reduce overflow issues
                    CommonOps_DDRM.divide(v, max);

                    // compute the magnitude of the vector
                    double tau = NormOps_DDRM.normF(v);

                    if (v.get(0) < 0)
                        tau *= -1.0;

                    double u_0 = v.get(0) + tau;
                    double gamma = u_0 / tau;

                    CommonOps_DDRM.divide(v, u_0);
                    v.set(0, 1.0);

                    // extract the submatrix of A which is being operated on
                    CommonOps_DDRM.extract(QR, i, QR.numRows, i, QR.numCols, A_small, 0, 0);

                    // A = (I - &gamma;*u*u<sup>T</sup>)A
                    CommonOps_DDRM.setIdentity(Q_k);
                    CommonOps_DDRM.multAddTransB(-gamma, v, v, Q_k);
                    CommonOps_DDRM.mult(Q_k, A_small, A_mod);

                    // save the results
                    CommonOps_DDRM.insert(A_mod, QR, i, i);
                    CommonOps_DDRM.insert(v, QR, i, i);
                    QR.unsafe_set(i, i, -tau * max);

                    // save gamma for recomputing Q later on
                    gammas[i] = gamma;
                }
            }
        }

        /**
         * Returns the Q matrix.
         */
        public DMatrixRMaj getQ()
        {
            DMatrixRMaj Q = CommonOps_DDRM.identity(QR.numRows);
            DMatrixRMaj Q_k = new DMatrixRMaj(QR.numRows, QR.numRows);
            DMatrixRMaj u = new DMatrixRMaj(QR.numRows, 1);

            DMatrixRMaj temp = new DMatrixRMaj(QR.numRows, QR.numRows);

            int N = Math.Min(QR.numCols, QR.numRows);

            // compute Q by first extracting the householder vectors from the columns of QR and then applying it to Q
            for (int j = N - 1; j >= 0; j--)
            {
                CommonOps_DDRM.extract(QR, j, QR.numRows, j, j + 1, u, j, 0);
                u.set(j, 1.0);

                // A = (I - &gamma;*u*u<sup>T</sup>)*A<br>
                CommonOps_DDRM.setIdentity(Q_k);
                CommonOps_DDRM.multAddTransB(-gammas[j], u, u, Q_k);
                CommonOps_DDRM.mult(Q_k, Q, temp);
                Q.set(temp);
            }

            return Q;
        }

        /**
         * Returns the R matrix.
         */
        public DMatrixRMaj getR()
        {
            DMatrixRMaj R = new DMatrixRMaj(QR.numRows, QR.numCols);

            int N = Math.Min(QR.numCols, QR.numRows);

            for (int i = 0; i < N; i++)
            {
                for (int j = i; j < QR.numCols; j++)
                {
                    R.unsafe_set(i, j, QR.unsafe_get(i, j));
                }
            }

            return R;
        }
    }
}