using System;
using System.Collections.Generic;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row;
using Randomization;

namespace SharpMatrix.Examples
{
    //package org.ejml.example;

/**
 * Compares how fast the filters all run relative to each other.
 *
 * @author Peter Abeles
 */
    public class BenchmarkKalmanPerformance
    {

        private static int NUM_TRIALS = 200;
        private static int MAX_STEPS = 1000;
        private static double T = 1.0;

        private static int measDOF = 8;

        List<KalmanFilter> filters = new List<KalmanFilter>();

        public void run()
        {
            DMatrixRMaj priorX = new DMatrixRMaj(9, 1, true, 0.5, -0.2, 0, 0, 0.2, -0.9, 0, 0.2, -0.5);
            DMatrixRMaj priorP = CommonOps_DDRM.identity(9);

            DMatrixRMaj trueX = new DMatrixRMaj(9, 1, true, 0, 0, 0, 0.2, 0.2, 0.2, 0.5, 0.1, 0.6);

            List<DMatrixRMaj> meas = createSimulatedMeas(trueX);

            DMatrixRMaj F = createF(T);
            DMatrixRMaj Q = createQ(T, 0.1);
            DMatrixRMaj H = createH();

            foreach (KalmanFilter f in filters ) {

                long timeBefore = DateTimeHelper.CurrentTimeMilliseconds;

                f.configure(F, Q, H);

                for (int trial = 0; trial < NUM_TRIALS; trial++)
                {
                    f.setState(priorX, priorP);
                    processMeas(f, meas);
                }

                long timeAfter = DateTimeHelper.CurrentTimeMilliseconds;

                Console.WriteLine("Filter = " + f.GetType().Name);
                Console.WriteLine("Elapsed time: " + (timeAfter - timeBefore));

                //System.gc();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private List<DMatrixRMaj> createSimulatedMeas(DMatrixRMaj x)
        {

            List<DMatrixRMaj> ret = new List<DMatrixRMaj>();

            DMatrixRMaj F = createF(T);
            DMatrixRMaj H = createH();

//        UtilEjml.print(F);
//        UtilEjml.print(H);

            DMatrixRMaj x_next = new DMatrixRMaj(x);
            DMatrixRMaj z = new DMatrixRMaj(H.numRows, 1);

            for (int i = 0; i < MAX_STEPS; i++)
            {
                CommonOps_DDRM.mult(F, x, x_next);
                CommonOps_DDRM.mult(H, x_next, z);
                ret.Add((DMatrixRMaj) z.copy());
                x.set(x_next);
            }

            return ret;
        }

        private void processMeas(KalmanFilter f,
            List<DMatrixRMaj> meas)
        {
            DMatrixRMaj R = CommonOps_DDRM.identity(measDOF);

            foreach (DMatrixRMaj z in meas)
            {
                f.predict();
                f.update(z, R);
            }
        }


        public static DMatrixRMaj createF(double T)
        {
            double[] a = new double[]
            {
                1, 0, 0, T, 0, 0, 0.5 * T * T, 0, 0,
                0, 1, 0, 0, T, 0, 0, 0.5 * T * T, 0,
                0, 0, 1, 0, 0, T, 0, 0, 0.5 * T * T,
                0, 0, 0, 1, 0, 0, T, 0, 0,
                0, 0, 0, 0, 1, 0, 0, T, 0,
                0, 0, 0, 0, 0, 1, 0, 0, T,
                0, 0, 0, 0, 0, 0, 1, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 1, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 1
            };

            return new DMatrixRMaj(9, 9, true, a);
        }

        public static DMatrixRMaj createQ(double T, double var)
        {
            DMatrixRMaj Q = new DMatrixRMaj(9, 9);

            double a00 = (1.0 / 4.0) * T * T * T * T * var;
            double a01 = (1.0 / 2.0) * T * T * T * var;
            double a02 = (1.0 / 2.0) * T * T * var;
            double a11 = T * T * var;
            double a12 = T * var;
            double a22 = var;

            for (int i = 0; i < 3; i++)
            {
                Q.set(i, i, a00);
                Q.set(i, 3 + i, a01);
                Q.set(i, 6 + i, a02);
                Q.set(3 + i, 3 + i, a11);
                Q.set(3 + i, 6 + i, a12);
                Q.set(6 + i, 6 + i, a22);
            }

            for (int y = 1; y < 9; y++)
            {
                for (int x = 0; x < y; x++)
                {
                    Q.set(y, x, Q.get(x, y));
                }
            }

            return Q;
        }

        public static DMatrixRMaj createH()
        {
            DMatrixRMaj H = new DMatrixRMaj(measDOF, 9);
            for (int i = 0; i < measDOF; i++)
            {
                H.set(i, i, 1.0);
            }

            return H;
        }

        public static void main(string[] args)
        {
            BenchmarkKalmanPerformance benchmark = new BenchmarkKalmanPerformance();

            benchmark.filters.Add(new KalmanFilterOperations());
            benchmark.filters.Add(new KalmanFilterSimple());
            benchmark.filters.Add(new KalmanFilterEquation());


            benchmark.run();
        }
    }
}