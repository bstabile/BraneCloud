using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row;

namespace BraneCloud.Evolution.EC.MatrixLib.Examples
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
    public class QRExampleEquation
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

            Equation.Equation eq = new Equation.Equation();

            this.QR = (DMatrixRMaj) A.copy();

            int N = Math.Min(A.numCols, A.numRows);

            gammas = new double[A.numCols];

            for (int i = 0; i < N; i++)
            {
                // update temporary variables
                eq.alias(QR.numRows - i, "Ni", QR, "QR", i, "i");

                // Place the column that should be zeroed into v
                eq.process("v=QR(i:,i)");
                eq.process("maxV=max(abs(v))");

                // Note that v is lazily created above.  Need direct access to it, which is done below.
                DMatrixRMaj v = eq.lookupMatrix("v");

                double maxV = eq.lookupDouble("maxV");
                if (maxV > 0 && v.getNumElements() > 1)
                {
                    // normalize to reduce overflow issues
                    eq.process("v=v/maxV");

                    // compute the magnitude of the vector
                    double tau = NormOps_DDRM.normF(v);

                    if (v.get(0) < 0)
                        tau *= -1.0;

                    eq.alias(tau, "tau");
                    eq.process("u_0 = v(0,0)+tau");
                    eq.process("gamma = u_0/tau");
                    eq.process("v=v/u_0");
                    eq.process("v(0,0)=1");
                    eq.process("QR(i:,i:) = (eye(Ni) - gamma*v*v')*QR(i:,i:)");
                    eq.process("QR(i:,i) = v");
                    eq.process("QR(i,i) = -1*tau*maxV");

                    // save gamma for recomputing Q later on
                    gammas[i] = eq.lookupDouble("gamma");
                }
            }
        }

        /**
         * Returns the Q matrix.
         */
        public DMatrixRMaj getQ()
        {
            Equation.Equation eq = new Equation.Equation();

            DMatrixRMaj Q = CommonOps_DDRM.identity(QR.numRows);
            DMatrixRMaj u = new DMatrixRMaj(QR.numRows, 1);

            int N = Math.Min(QR.numCols, QR.numRows);

            eq.alias(u, "u", Q, "Q", QR, "QR", QR.numRows, "r");

            // compute Q by first extracting the householder vectors from the columns of QR and then applying it to Q
            for (int j = N - 1; j >= 0; j--)
            {
                eq.alias(j, "j", gammas[j], "gamma");

                eq.process("u(j:,0) = [1 ; QR((j+1):,j)]");
                eq.process("Q=(eye(r)-gamma*u*u')*Q");
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