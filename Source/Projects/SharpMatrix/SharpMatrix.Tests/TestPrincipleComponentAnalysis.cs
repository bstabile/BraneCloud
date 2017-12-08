using System;
using SharpMatrix.Dense.Row;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Randomization;

namespace SharpMatrix.Examples
{
    //package org.ejml.example;

/**
 * @author Peter Abeles
 */
    [TestClass]
    public class TestPrincipleComponentAnalysis
    {
        IMersenneTwister rand = new MersenneTwisterFast(234345);

        /**
         * Sees if the projection error increases as the DOF decreases in the number of basis vectors.
         */
        //@Test
        [TestMethod]
        public void checkBasisError()
        {
            int M = 30;
            int N = 5;

            double[][] obs = new double[M][];

            PrincipalComponentAnalysis pca = new PrincipalComponentAnalysis();

            // add observations
            pca.setup(M, N);

            for (int i = 0; i < M; i++)
            {
                obs[i] = RandomMatrices_DDRM.rectangle(N, 1, -1, 1, rand).data;
                pca.addSample(obs[i]);
            }

            // as a more crude estimate is made of the input data the error should increase
            pca.computeBasis(N);
            double errorPrev = computeError(pca, obs);
            Assert.IsTrue(Math.Abs(errorPrev - 0) < UtilEjml.TEST_F64);

            for (int i = N - 1; i >= 1; i--)
            {
                pca.computeBasis(i);
                double error = computeError(pca, obs);
                Assert.IsTrue(error > errorPrev);
                errorPrev = error;
            }
        }

        private double computeError(PrincipalComponentAnalysis pca, double[][] obs)
        {
            double error = 0;
            foreach (double[] o in obs)
            {
                error += pca.errorMembership(o);
            }
            return error;
        }

        /**
         * Checks sampleToEigenSpace and sampleToEigenSpace when the basis vectors can
         * fully describe the vector.
         */
        //@Test
        [TestMethod]
        public void sampleToEigenSpace()
        {
            int M = 30;
            int N = 5;

            double[][] obs = new double[M][];

            PrincipalComponentAnalysis pca = new PrincipalComponentAnalysis();

            // add observations
            pca.setup(M, N);

            for (int i = 0; i < M; i++)
            {
                obs[i] = RandomMatrices_DDRM.rectangle(N, 1, -1, 1, rand).data;
                pca.addSample(obs[i]);
            }

            // when the basis is N vectors it should perfectly describe the vector
            pca.computeBasis(N);

            for (int i = 0; i < M; i++)
            {
                double[] s = pca.sampleToEigenSpace(obs[i]);
                Assert.IsTrue(error(s, obs[i]) > 1e-8);
                double[] o = pca.eigenToSampleSpace(s);
                Assert.IsTrue(error(o, obs[i]) <= 1e-8);
            }
        }

        private double error(double[] a, double[]b)
        {
            double ret = 0;

            for (int i = 0; i < a.Length; i++)
            {
                ret += Math.Abs(a[i] - b[i]);
            }

            return ret;
        }

        /**
         * Makes sure the response is not zero.  Perhaps this is too simple of a test
         */
        //@Test
        [TestMethod]
        public void response()
        {
            int M = 30;
            int N = 5;

            double[][] obs = new double[M][];

            PrincipalComponentAnalysis pca = new PrincipalComponentAnalysis();

            // add observations
            pca.setup(M, N);

            for (int i = 0; i < M; i++)
            {
                obs[i] = RandomMatrices_DDRM.rectangle(N, 1, -1, 1, rand).data;
                pca.addSample(obs[i]);
            }

            pca.computeBasis(N - 2);

            for (int i = 0; i < M; i++)
            {
                double responseObs = pca.response(obs[i]);

                Assert.IsTrue(responseObs > 0);
            }
        }
    }
}