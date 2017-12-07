using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using Randomization;

namespace BraneCloud.Evolution.EC.MatrixLib.Generic
{
    //package org.ejml.generic;

/**
 * @author Peter Abeles
 */
    public class GenericMatrixOps_F64
    {

//    public static DenseD2Matrix64F convertToD2( DMatrixRMaj orig ) {
//        DenseD2Matrix64F ret = new DenseD2Matrix64F(orig.getNumRows(),orig.getNumCols());
//
//        copy(orig,ret);
//
//        return ret;
//    }

        public static bool isEquivalent(DMatrix a, DMatrix b, double tol)
        {
            if (a.getNumRows() != b.getNumRows() || a.getNumCols() != b.getNumCols())
                return false;

            for (int i = 0; i < a.getNumRows(); i++)
            {
                for (int j = 0; j < a.getNumCols(); j++)
                {
                    double diff = Math.Abs(a.get(i, j) - b.get(i, j));

                    if (diff > tol)
                        return false;
                }
            }

            return true;
        }

        /**
         * Returns true if the provided matrix is has a value of 1 along the diagonal
         * elements and zero along all the other elements.
         *
         * @param a Matrix being inspected.
         * @param tol How close to zero or one each element needs to be.
         * @return If it is within tolerance to an identity matrix.
         */
        public static bool isIdentity(DMatrix a, double tol)
        {
            for (int i = 0; i < a.getNumRows(); i++)
            {
                for (int j = 0; j < a.getNumCols(); j++)
                {
                    if (i == j)
                    {
                        if (Math.Abs(a.get(i, j) - 1.0) > tol)
                            return false;
                    }
                    else
                    {
                        if (Math.Abs(a.get(i, j)) > tol)
                            return false;
                    }
                }
            }
            return true;
        }

        public static bool isEquivalentTriangle(bool upper, DMatrix a, DMatrix b, double tol)
        {
            if (a.getNumRows() != b.getNumRows() || a.getNumCols() != b.getNumCols())
                return false;

            if (upper)
            {
                for (int i = 0; i < a.getNumRows(); i++)
                {
                    for (int j = i; j < a.getNumCols(); j++)
                    {
                        double diff = Math.Abs(a.get(i, j) - b.get(i, j));

                        if (diff > tol)
                            return false;
                    }
                }
            }
            else
            {
                for (int j = 0; j < a.getNumCols(); j++)
                {
                    for (int i = j; i < a.getNumRows(); i++)
                    {
                        double diff = Math.Abs(a.get(i, j) - b.get(i, j));

                        if (diff > tol)
                            return false;
                    }
                }
            }

            return true;
        }

        public static void copy(DMatrix from, DMatrix to)
        {
            int numCols = from.getNumCols();
            int numRows = from.getNumRows();

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    to.set(i, j, from.get(i, j));
                }
            }
        }

        public static void setRandom(DMatrix a, double min, double max, IMersenneTwister rand)
        {
            for (int i = 0; i < a.getNumRows(); i++)
            {
                for (int j = 0; j < a.getNumCols(); j++)
                {
                    double val = rand.NextDouble() * (max - min) + min;
                    a.set(i, j, val);
                }
            }
        }
    }
}