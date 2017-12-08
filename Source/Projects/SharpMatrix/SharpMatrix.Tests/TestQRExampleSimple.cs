using SharpMatrix.Data;
using SharpMatrix.Simple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Randomization;

namespace SharpMatrix.Examples
{
    //package org.ejml.example;

/**
 * @author Peter Abeles
 */
    public class TestQRExampleSimple
    {

        IMersenneTwister rand = new MersenneTwisterFast(23423);


        //@Test
        public void basic()
        {
            checkMatrix(7, 5);
            checkMatrix(5, 5);
            checkMatrix(7, 7);
        }

        private void checkMatrix(int numRows, int numCols)
        {
            SimpleMatrix<DMatrixRMaj> A = SimpleMatrix<DMatrixRMaj>.random64(numRows, numCols, -1, 1, rand);

            QRExampleSimple alg = new QRExampleSimple();

            alg.decompose(A);

            SimpleMatrix<DMatrixRMaj> Q = alg.getQ();
            SimpleMatrix<DMatrixRMaj> R = alg.getR();

            SimpleMatrix<DMatrixRMaj> A_found = Q.mult(R) as SimpleMatrix<DMatrixRMaj>;

            Assert.IsTrue(A.isIdentical(A_found, UtilEjml.TEST_F64));
        }

    }
}
