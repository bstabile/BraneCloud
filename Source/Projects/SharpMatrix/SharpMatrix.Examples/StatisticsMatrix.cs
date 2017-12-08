using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row;
using SharpMatrix.Simple;
using Randomization;

namespace SharpMatrix.Examples
{
    //package org.ejml.example;

/**
 * Example of how to extend "SimpleMatrix" and add your own functionality.  In this case
 * two basic statistic operations are added.  Since SimpleBase is extended and StatisticsMatrix
 * is specified as the generics type, all "SimpleMatrix" operations return a matrix of
 * type StatisticsMatrix, ensuring strong typing.
 *
 * @author Peter Abeles
 */
    public class StatisticsMatrix : SimpleBase<DMatrixRMaj>
    {

        public StatisticsMatrix(int numRows, int numCols)
            : base(numRows, numCols)
        {
        }

        protected StatisticsMatrix()
        {
        }

        /**
         * Wraps a StatisticsMatrix around 'm'.  Does NOT create a copy of 'm' but saves a reference
         * to it.
         */
        public static StatisticsMatrix wrap(DMatrixRMaj m)
        {
            StatisticsMatrix ret = new StatisticsMatrix();
            ret.setMatrix(m);

            return ret;
        }

        /**
         * Computes the mean or average of all the elements.
         *
         * @return mean
         */
        public double mean()
        {
            double total = 0;

            int N = getNumElements();
            for (int i = 0; i < N; i++)
            {
                total += get(i);
            }

            return total / N;
        }

        /**
         * Computes the unbiased standard deviation of all the elements.
         *
         * @return standard deviation
         */
        public double stdev()
        {
            double m = mean();

            double total = 0;

            int N = getNumElements();
            if (N <= 1)
                throw new ArgumentException("There must be more than one element to compute stdev");


            for (int i = 0; i < N; i++)
            {
                double x = get(i);

                total += (x - m) * (x - m);
            }

            total /= (N - 1);

            return Math.Sqrt(total);
        }

        /**
         * Returns a matrix of StatisticsMatrix type so that SimpleMatrix functions create matrices
         * of the correct type.
         */
        //@Override
        protected override SimpleBase<DMatrixRMaj> createMatrix(int numRows, int numCols, MatrixType type)
        {
            return new StatisticsMatrix(numRows, numCols);
        }

        //@Override
        protected override SimpleBase<DMatrixRMaj> wrapMatrix(DMatrixRMaj m)
        {
            StatisticsMatrix r = new StatisticsMatrix();
            r.setMatrix(m);
            return r;
        }

        public static void main(string[] args)
        {
            IMersenneTwister rand = new MersenneTwisterFast(24234);

            int N = 500;

            // create two vectors whose elements are drawn from uniform distributions
            StatisticsMatrix A = StatisticsMatrix.wrap(RandomMatrices_DDRM.rectangle(N, 1, 0, 1, rand));
            StatisticsMatrix B = StatisticsMatrix.wrap(RandomMatrices_DDRM.rectangle(N, 1, 1, 2, rand));

            // the mean should be about 0.5
            Console.WriteLine("Mean of A is               " + A.mean());
            // the mean should be about 1.5
            Console.WriteLine("Mean of B is               " + B.mean());

            StatisticsMatrix C = (StatisticsMatrix) A.plus(B);

            // the mean should be about 2.0
            Console.WriteLine("Mean of C = A + B is       " + C.mean());

            Console.WriteLine("Standard deviation of A is " + A.stdev());
            Console.WriteLine("Standard deviation of B is " + B.stdev());
            Console.WriteLine("Standard deviation of C is " + C.stdev());
        }
    }
}