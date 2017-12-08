using System;
using SharpMatrix.Data;
using SharpMatrix.Dense;
using SharpMatrix.Dense.Block;
using SharpMatrix.Dense.Row;
using SharpMatrix.Dense.Row.Factory;
using SharpMatrix.Interfaces.LinSol;

namespace SharpMatrix.Examples
{
    //package org.ejml.example;

/**
 * A Kalman filter that is implemented using the operations API, which is procedural.  Much of the excessive
 * memory creation/destruction has been reduced from the KalmanFilterSimple. A specialized solver is
 * under to invert the SPD matrix.
 *
 * @author Peter Abeles
 */
    public class KalmanFilterOperations : KalmanFilter
    {

        // kinematics description
        private DMatrixRMaj F, Q, H;

        // system state estimate
        private DMatrixRMaj x, P;

        // these are predeclared for efficiency reasons
        private DMatrixRMaj a, b;

        private DMatrixRMaj y, S, S_inv, c, d;
        private DMatrixRMaj K;

        private LinearSolverDense<DMatrixRMaj> solver;

        //@Override
        public void configure(DMatrixRMaj F, DMatrixRMaj Q, DMatrixRMaj H)
        {
            this.F = F;
            this.Q = Q;
            this.H = H;

            int dimenX = F.numCols;
            int dimenZ = H.numRows;

            a = new DMatrixRMaj(dimenX, 1);
            b = new DMatrixRMaj(dimenX, dimenX);
            y = new DMatrixRMaj(dimenZ, 1);
            S = new DMatrixRMaj(dimenZ, dimenZ);
            S_inv = new DMatrixRMaj(dimenZ, dimenZ);
            c = new DMatrixRMaj(dimenZ, dimenX);
            d = new DMatrixRMaj(dimenX, dimenZ);
            K = new DMatrixRMaj(dimenX, dimenZ);

            x = new DMatrixRMaj(dimenX, 1);
            P = new DMatrixRMaj(dimenX, dimenX);

            // covariance matrices are symmetric positive semi-definite
            solver = LinearSolverFactory_DDRM.symmPosDef(dimenX);
        }

        //@Override
        public void setState(DMatrixRMaj x, DMatrixRMaj P)
        {
            this.x.set(x);
            this.P.set(P);
        }

        //@Override
        public void predict()
        {

            // x = F x
            CommonOps_DDRM.mult(F, x, a);
            x.set(a);

            // P = F P F' + Q
            CommonOps_DDRM.mult(F, P, b);
            CommonOps_DDRM.multTransB(b, F, P);
            CommonOps_DDRM.addEquals(P, Q);
        }

        //@Override
        public void update(DMatrixRMaj z, DMatrixRMaj R)
        {
            // y = z - H x
            CommonOps_DDRM.mult(H, x, y);
            CommonOps_DDRM.subtract(z, y, y);

            // S = H P H' + R
            CommonOps_DDRM.mult(H, P, c);
            CommonOps_DDRM.multTransB(c, H, S);
            CommonOps_DDRM.addEquals(S, R);

            // K = PH'S^(-1)
            if (!solver.setA(S)) throw new InvalidOperationException("Invert failed");
            solver.invert(S_inv);
            CommonOps_DDRM.multTransA(H, S_inv, d);
            CommonOps_DDRM.mult(P, d, K);

            // x = x + Ky
            CommonOps_DDRM.mult(K, y, a);
            CommonOps_DDRM.addEquals(x, a);

            // P = (I-kH)P = P - (KH)P = P-K(HP)
            CommonOps_DDRM.mult(H, P, c);
            CommonOps_DDRM.mult(K, c, b);
            CommonOps_DDRM.subtractEquals(P, b);
        }

        //@Override
        public DMatrixRMaj getState()
        {
            return x;
        }

        //@Override
        public DMatrixRMaj getCovariance()
        {
            return P;
        }
    }
}