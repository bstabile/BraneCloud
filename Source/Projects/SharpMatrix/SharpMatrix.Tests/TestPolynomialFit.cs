using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BraneCloud.Evolution.EC.MatrixLib.Examples
{
    //package org.ejml.example;

/**
 * @author Peter Abeles
 */
    public class TestPolynomialFit
    {

        /**
         * Test with perfect data
         */
        //@Test
        public void testPerfect()
        {
            double[] coef = new double[] {1, -2, 3};

            double[] x = new double[] {-2, 1, 0.5, 2, 3, 4, 5, 7, 8, 9.2, 10.2, 4.3, 6.7};
            double[] y = new double[x.Length];

            for (int i = 0; i < y.Length; i++)
            {
                double v = 0;
                double xx = 1;
                foreach (double c in coef)
                {
                    v += c * xx;
                    xx *= x[i];
                }

                y[i] = v;
            }

            PolynomialFit alg = new PolynomialFit(2);

            alg.fit(x, y);

            double[] found = alg.getCoef();

            for (int i = 0; i < coef.Length; i++)
            {
                Assert.IsTrue(Math.Abs(coef[i] - found[i]) < UtilEjml.TEST_F64);
            }
        }

        /**
         * Make one of the observations way off and see if it is removed
         */
        //@Test
        public void testNoise()
        {
            double[] coef = new double[] {1, -2, 3};

            double[] x = new double[] {-2, 1, 0.5, 2, 3, 4, 5, 7, 8, 9.2, 10.2, 4.3, 6.7};
            double[] y = new double[x.Length];

            for (int i = 0; i < y.Length; i++)
            {
                double v = 0;
                double xx = 1;
                foreach (double c in coef)
                {
                    v += c * xx;
                    xx *= x[i];
                }

                y[i] = v;
            }

            y[4] += 3.5;

            PolynomialFit alg = new PolynomialFit(2);

            alg.fit(x, y);

            double[] found = alg.getCoef();

            // the coefficients that it initialy computes should be incorrect

            for (int i = 0; i < coef.Length; i++)
            {
                Assert.IsTrue(Math.Abs(coef[i] - found[i]) > UtilEjml.TEST_F64);
            }

            //remove the outlier
            alg.removeWorstFit();

            // now see if the solution is perfect
            found = alg.getCoef();

            for (int i = 0; i < coef.Length; i++)
            {
                Assert.IsTrue(Math.Abs(coef[i] - found[i]) < UtilEjml.TEST_F64);
            }
        }
    }
}