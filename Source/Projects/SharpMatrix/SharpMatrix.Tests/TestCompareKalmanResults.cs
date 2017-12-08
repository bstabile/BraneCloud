using System.Collections.Generic;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpMatrix.Examples
{
    //package org.ejml.example;

/**
 * Make sure all the filters produce exactly the same results.
 *
 * @author Peter Abeles
 */
    [TestClass]
    public class TestCompareKalmanResults
    {

        private static double T = 0.5;

        /**
         * See if all the filters produce the same reslts.
         */
        //@Test
        [TestMethod]
        public void checkIdentical()
        {
            KalmanFilterSimple simple = new KalmanFilterSimple();

            List<KalmanFilter> all = new List<KalmanFilter>();
            all.Add(new KalmanFilterOperations());
            all.Add(new KalmanFilterEquation());
            all.Add(simple);

            DMatrixRMaj priorX = new DMatrixRMaj(9, 1, true, 0.5, -0.2, 0, 0, 0.2, -0.9, 0, 0.2, -0.5);
            DMatrixRMaj priorP = CommonOps_DDRM.identity(9);

            DMatrixRMaj F = BenchmarkKalmanPerformance.createF(T);
            DMatrixRMaj Q = BenchmarkKalmanPerformance.createQ(T, 0.1);
            DMatrixRMaj H = BenchmarkKalmanPerformance.createH();


            foreach (KalmanFilter f in all)
            {
                f.configure(F, Q, H);
                f.setState(priorX, priorP);
                f.predict();
            }

            foreach (KalmanFilter f in all)
            {
                compareFilters(simple, f);
            }

            DMatrixRMaj z = new DMatrixRMaj(H.numRows, 1);
            DMatrixRMaj R = CommonOps_DDRM.identity(H.numRows);

            foreach (KalmanFilter f in all)
            {
                f.update(z, R);
            }

            foreach (KalmanFilter f in all)
            {
                compareFilters(simple, f);
            }
        }

        private void compareFilters(KalmanFilter a, KalmanFilter b)
        {
            DMatrixRMaj testX = b.getState();
            DMatrixRMaj testP = b.getCovariance();

            DMatrixRMaj X = a.getState();
            DMatrixRMaj P = a.getCovariance();

            EjmlUnitTests.assertEquals(testX, X, UtilEjml.TEST_F64);
            EjmlUnitTests.assertEquals(testP, P, UtilEjml.TEST_F64);
        }
    }
}