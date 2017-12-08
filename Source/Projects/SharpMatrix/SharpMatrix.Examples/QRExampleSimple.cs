using System;
using SharpMatrix.Data;
using SharpMatrix.Simple;

namespace SharpMatrix.Examples
{
    //package org.ejml.example;

/**
 * <p>
 * Example code for computing the QR decomposition of a matrix. It demonstrates how to
 * extract a submatrix and insert one matrix into another one using SimpleMatrix.
 * </p>
 *
 * Note: This code is horribly inefficient and is for demonstration purposes only.
 *
 * @author Peter Abeles
 */
    public class QRExampleSimple
    {

        // where the QR decomposition is stored
        private SimpleMatrix<DMatrixRMaj> QR;

        // used for computing Q
        private double[] gammas;

        /**
         * Computes the QR decomposition of the provided matrix.
         *
         * @param A Matrix which is to be decomposed.  Not modified.
         */
        public void decompose(SimpleMatrix<DMatrixRMaj> A)
        {

            this.QR = A.copy() as SimpleMatrix<DMatrixRMaj>;

            int N = Math.Min(A.numCols(), A.numRows());
            gammas = new double[A.numCols()];

            for (int i = 0; i < N; i++)
            {
                // use extract matrix to get the column that is to be zeroed
                SimpleMatrix<DMatrixRMaj> v = QR.extractMatrix(i, SimpleMatrix<DMatrixRMaj>.END, i, i + 1) as SimpleMatrix<DMatrixRMaj>;
                double max = v.elementMaxAbs();

                if (max > 0 && v.getNumElements() > 1)
                {
                    // normalize to reduce overflow issues
                    v = v.divide(max) as SimpleMatrix<DMatrixRMaj>;

                    // compute the magnitude of the vector
                    double tau = v.normF();

                    if (v.get(0) < 0)
                        tau *= -1.0;

                    double u_0 = v.get(0) + tau;
                    double gamma = u_0 / tau;

                    v = v.divide(u_0) as SimpleMatrix<DMatrixRMaj>;
                    v.set(0, 1.0);

                    // extract the submatrix of A which is being operated on
                    SimpleBase<DMatrixRMaj> A_small = QR.extractMatrix(i, SimpleMatrix<DMatrixRMaj>.END, i, SimpleMatrix<DMatrixRMaj>.END);

                    // A = (I - &gamma;*u*u<sup>T</sup>)A
                    A_small = A_small.plus(-gamma, v.mult(v.transpose()).mult(A_small));

                    // save the results
                    QR.insertIntoThis(i, i, A_small);
                    QR.insertIntoThis(i + 1, i, v.extractMatrix(1, SimpleMatrix<DMatrixRMaj>.END, 0, 1));

                    // Alternatively, the two lines above can be replaced with in-place equations
                    // READ THE JAVADOC TO UNDERSTAND HOW THIS WORKS!
//                QR.equation("QR(i:,i:) = A","QR",i,"i",A_small,"A");
//                QR.equation("QR((i+1):,i) = v(1:,0)","QR",i,"i",v,"v");

                    // save gamma for recomputing Q later on
                    gammas[i] = gamma;
                }
            }
        }

        /**
         * Returns the Q matrix.
         */
        public SimpleMatrix<DMatrixRMaj> getQ()
        {
            SimpleMatrix<DMatrixRMaj> Q = SimpleMatrix<DMatrixRMaj>.identity(QR.numRows());

            int N = Math.Min(QR.numCols(), QR.numRows());

            // compute Q by first extracting the householder vectors from the columns of QR and then applying it to Q
            for (int j = N - 1; j >= 0; j--)
            {
                SimpleMatrix<DMatrixRMaj> u = new SimpleMatrix<DMatrixRMaj>(QR.numRows(), 1);
                u.insertIntoThis(j, 0, QR.extractMatrix(j, SimpleMatrix<DMatrixRMaj>.END, j, j + 1));
                u.set(j, 1.0);

                // A = (I - &gamma;*u*u<sup>T</sup>)*A<br>
                Q = Q.plus(-gammas[j], u.mult(u.transpose()).mult(Q)) as SimpleMatrix<DMatrixRMaj>;
            }

            return Q;
        }

        /**
         * Returns the R matrix.
         */
        public SimpleMatrix<DMatrixRMaj> getR()
        {
            SimpleMatrix<DMatrixRMaj> R = new SimpleMatrix<DMatrixRMaj>(QR.numRows(), QR.numCols());

            int N = Math.Min(QR.numCols(), QR.numRows());

            for (int i = 0; i < N; i++)
            {
                for (int j = i; j < QR.numCols(); j++)
                {
                    R.set(i, j, QR.get(i, j));
                }
            }

            return R;
        }
    }
}