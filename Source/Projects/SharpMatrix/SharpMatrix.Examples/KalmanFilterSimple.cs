using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Simple;

namespace BraneCloud.Evolution.EC.MatrixLib.Examples
{
    //package org.ejml.example;

/**
 * A Kalman filter implemented using SimpleMatrix.  The code tends to be easier to
 * read and write, but the performance is degraded due to excessive creation/destruction of
 * memory and the use of more generic algorithms.  This also demonstrates how code can be
 * seamlessly implemented using both SimpleMatrix and DMatrixRMaj.  This allows code
 * to be quickly prototyped or to be written either by novices or experts.
 *
 * @author Peter Abeles
 */
    public class KalmanFilterSimple : KalmanFilter
    {

        // kinematics description
        private SimpleMatrix<DMatrixRMaj> F, Q, H;

        // sytem state estimate
        private SimpleMatrix<DMatrixRMaj> x, P;

        //@Override
        public void configure(DMatrixRMaj F, DMatrixRMaj Q, DMatrixRMaj H)
        {
            this.F = new SimpleMatrix<DMatrixRMaj>(F);
            this.Q = new SimpleMatrix<DMatrixRMaj>(Q);
            this.H = new SimpleMatrix<DMatrixRMaj>(H);
        }

        //@Override
        public void setState(DMatrixRMaj x, DMatrixRMaj P)
        {
            this.x = new SimpleMatrix<DMatrixRMaj>(x);
            this.P = new SimpleMatrix<DMatrixRMaj>(P);
        }

        //@Override
        public void predict()
        {
            // x = F x
            x = F.mult(x) as SimpleMatrix<DMatrixRMaj>;

            // P = F P F' + Q
            P = F.mult(P).mult(F.transpose()).plus(Q) as SimpleMatrix<DMatrixRMaj>;
        }

        //@Override
        public void update(DMatrixRMaj _z, DMatrixRMaj _R)
        {
            // a fast way to make the matrices usable by SimpleMatrix
            SimpleMatrix<DMatrixRMaj> z = SimpleMatrix<DMatrixRMaj>.wrap(_z);
            SimpleMatrix<DMatrixRMaj> R = SimpleMatrix<DMatrixRMaj>.wrap(_R);

            // y = z - H x
            SimpleMatrix<DMatrixRMaj> y = z.minus(H.mult(x)) as SimpleMatrix<DMatrixRMaj>;

            // S = H P H' + R
            SimpleMatrix<DMatrixRMaj> S = H.mult(P).mult(H.transpose()).plus(R) as SimpleMatrix<DMatrixRMaj>;

            // K = PH'S^(-1)
            SimpleMatrix<DMatrixRMaj> K = P.mult(H.transpose().mult(S.invert())) as SimpleMatrix<DMatrixRMaj>;

            // x = x + Ky
            x = x.plus(K.mult(y)) as SimpleMatrix<DMatrixRMaj>;

            // P = (I-kH)P = P - KHP
            P = P.minus(K.mult(H).mult(P)) as SimpleMatrix<DMatrixRMaj>;
        }

        //@Override
        public DMatrixRMaj getState()
        {
            return (DMatrixRMaj) x.getMatrix();
        }

        //@Override
        public DMatrixRMaj getCovariance()
        {
            return (DMatrixRMaj) P.getMatrix();
        }
    }
}