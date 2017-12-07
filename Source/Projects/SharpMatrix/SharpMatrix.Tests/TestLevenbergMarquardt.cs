using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Randomization;

namespace BraneCloud.Evolution.EC.MatrixLib.Examples
{
    //package org.ejml.example;

/**
 * @author Peter Abeles
 */
    [TestClass]
    public class TestLevenbergMarquardt
    {
        int NUM_PTS = 50;

        IMersenneTwister rand = new MersenneTwisterFast(7264);

        /**
         * Give it a simple function and see if it computes something close to it for its results.
         */
        //@Test
        [TestMethod]
        public void testNumericalJacobian()
        {
            JacobianTestFunction func = new JacobianTestFunction();

            DMatrixRMaj param = new DMatrixRMaj(3, 1, true, 2, -1, 4);

            LevenbergMarquardt alg = new LevenbergMarquardt(func);

            DMatrixRMaj X = RandomMatrices_DDRM.rectangle(NUM_PTS, 1, rand);

            DMatrixRMaj numJacobian = new DMatrixRMaj(3, NUM_PTS);
            DMatrixRMaj analyticalJacobian = new DMatrixRMaj(3, NUM_PTS);

            alg.configure(param, X, new DMatrixRMaj(NUM_PTS, 1));
            alg.computeNumericalJacobian(param, X, numJacobian);
            func.deriv(X, analyticalJacobian);

            EjmlUnitTests.assertEquals(analyticalJacobian, numJacobian, 1e-6);
        }

        /**
         * See if it can solve an easy optimization problem.
         */
        //@Test
        public void testTrivial()
        {
            // the number of sample points is equal to the max allowed points
            runTrivial(NUM_PTS);
            // do the same thing but with a different number of poitns from the max allowed
            runTrivial(20);
        }

        /**
         * Runs the simple optimization problem with a set of randomly generated inputs.
         *
         * @param numPoints How many sample points there are.
         */
        public void runTrivial(int numPoints)
        {
            JacobianTestFunction func = new JacobianTestFunction();

            DMatrixRMaj paramInit = new DMatrixRMaj(3, 1);
            DMatrixRMaj param = new DMatrixRMaj(3, 1, true, 2, -1, 4);

            LevenbergMarquardt alg = new LevenbergMarquardt(func);

            DMatrixRMaj X = RandomMatrices_DDRM.rectangle(numPoints, 1, rand);
            DMatrixRMaj Y = new DMatrixRMaj(numPoints, 1);
            func.compute(param, X, Y);

            alg.optimize(paramInit, X, Y);

            DMatrixRMaj foundParam = alg.getParameters();

            Assert.IsTrue(Math.Abs(0 - alg.getFinalCost()) < UtilEjml.TEST_F64);
            EjmlUnitTests.assertEquals(param, foundParam, 1e-6);
        }

        /**
         * A very simple function to test how well the numerical jacobian is computed.
         */
        private class JacobianTestFunction : LevenbergMarquardt.Function
        {

            public void deriv(DMatrixRMaj x, DMatrixRMaj deriv)
            {
                double[] dataX = x.data;

                int length = x.numRows;

                for (int j = 0; j < length; j++)
                {
                    double v = dataX[j];

                    double dA = 1;
                    double dB = v;
                    double dC = v * v;

                    deriv.set(0, j, dA);
                    deriv.set(1, j, dB);
                    deriv.set(2, j, dC);
                }

            }

            //@Override
            public void compute(DMatrixRMaj param, DMatrixRMaj x, DMatrixRMaj y)
            {
                double a = param.data[0];
                double b = param.data[1];
                double c = param.data[2];

                double[] dataX = x.data;
                double[] dataY = y.data;

                int length = x.numRows;

                for (int i = 0; i < length; i++)
                {
                    double v = dataX[i];

                    dataY[i] = a + b * v + c * v * v;
                }
            }
        }
    }
}