using System;
using SharpMatrix.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpMatrix.Examples
{
    //package org.ejml.example;

/**
 * @author Peter Abeles
 */
    [TestClass]
    public class TestPolynomialRootFinder
    {

        //@Test
        [TestMethod]
        public void findRoots()
        {
            Complex_F64[] roots = PolynomialRootFinder.findRoots(4, 3, 2, 1);

            int numReal = 0;
            foreach (Complex_F64 c in roots)
            {
                if (c.isReal())
                {
                    checkRoot(c.real, 4, 3, 2, 1);
                    numReal++;
                }
            }

            Assert.IsTrue(numReal > 0);
        }

        private void checkRoot(double root, params double[] coefs)
        {
            double total = 0;

            double a = 1;
            foreach (double c in coefs)
            {
                total += a * c;
                a *= root;
            }

            Assert.IsTrue(Math.Abs(0 - total) < UtilEjml.TEST_F64);
        }
    }
}