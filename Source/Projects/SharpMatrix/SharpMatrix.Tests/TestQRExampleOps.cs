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
    public class TestQRExampleOps
    {

        IMersenneTwister rand = new MersenneTwisterFast(23423);


        //@Test
        [TestMethod]
        public void basic()
        {
            checkMatrix(7, 5);
            checkMatrix(5, 5);
            checkMatrix(7, 7);
        }

        private void checkMatrix(int numRows, int numCols)
        {
            DMatrixRMaj A = RandomMatrices_DDRM.rectangle(numRows, numCols, -1, 1, rand);

            QRExampleOperations alg = new QRExampleOperations();

            alg.decompose(A);

            DMatrixRMaj Q = alg.getQ();
            DMatrixRMaj R = alg.getR();

            DMatrixRMaj A_found = new DMatrixRMaj(numRows, numCols);
            CommonOps_DDRM.mult(Q, R, A_found);

            Assert.IsTrue(MatrixFeatures_DDRM.isIdentical(A, A_found, UtilEjml.TEST_F64));
        }

    }
}