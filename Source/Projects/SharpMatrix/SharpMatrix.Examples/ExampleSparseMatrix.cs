using System;
using System.IO;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row;
using SharpMatrix.Ops;
using SharpMatrix.Sparse.Csc;
using Randomization;

namespace SharpMatrix.Examples
{
    //package org.ejml.example;

/**
 * Example showing how to construct and solve a linear system using sparse matrices
 *
 * @author Peter Abeles
 */
    public class ExampleSparseMatrix
    {


        public static int ROWS = 100000;
        public static int COLS = 1000;
        public static int XCOLS = 1;

        public static void main(String[] args)
        {
            IMersenneTwister rand = new MersenneTwisterFast(234);

            // easy to work with sparse format, but hard to do computations with
            DMatrixSparseTriplet work = new DMatrixSparseTriplet(5, 4, 5);
            work.addItem(0, 1, 1.2);
            work.addItem(3, 0, 3);
            work.addItem(1, 1, 22.21234);
            work.addItem(2, 3, 6);

            // convert into a format that's easier to perform math with
            DMatrixSparseCSC Z = ConvertDMatrixStruct.convert(work, (DMatrixSparseCSC) null);

            // print the matrix to standard out in two different formats
            Z.print();
            Console.WriteLine();
            Z.printNonZero();
            Console.WriteLine();

            // Create a large matrix that is 5% filled
            DMatrixSparseCSC A = RandomMatrices_DSCC.rectangle(ROWS, COLS, (int) (ROWS * COLS * 0.05), rand);
            //          large vector that is 70% filled
            DMatrixSparseCSC x = RandomMatrices_DSCC.rectangle(COLS, XCOLS, (int) (XCOLS * COLS * 0.7), rand);

            Console.WriteLine("Done generating random matrices");
            // storage for the initial solution
            DMatrixSparseCSC y = new DMatrixSparseCSC(ROWS, XCOLS, 0);
            DMatrixSparseCSC z = new DMatrixSparseCSC(ROWS, XCOLS, 0);

            // To demonstration how to perform sparse math let's multiply:
            //                  y=A*x
            // Optional storage is set to null so that it will declare it internally
            long before = DateTimeHelper.CurrentTimeMilliseconds;
            IGrowArray workA = new IGrowArray(A.numRows);
            DGrowArray workB = new DGrowArray(A.numRows);
            for (int i = 0; i < 100; i++)
            {
                CommonOps_DSCC.mult(A, x, y, workA, workB);
                CommonOps_DSCC.add(1.5, y, 0.75, y, z, workA, workB);
            }
            long after = DateTimeHelper.CurrentTimeMilliseconds;

            Console.WriteLine("norm = " + NormOps_DSCC.fastNormF(y) + "  sparse time = " + (after - before) + " ms");

            DMatrixRMaj Ad = ConvertDMatrixStruct.convert(A, (DMatrixRMaj) null);
            DMatrixRMaj xd = ConvertDMatrixStruct.convert(x, (DMatrixRMaj) null);
            DMatrixRMaj yd = new DMatrixRMaj(y.numRows, y.numCols);
            DMatrixRMaj zd = new DMatrixRMaj(y.numRows, y.numCols);

            before = DateTimeHelper.CurrentTimeMilliseconds;
            for (int i = 0; i < 100; i++)
            {
                CommonOps_DDRM.mult(Ad, xd, yd);
                CommonOps_DDRM.add(1.5, yd, 0.75, yd, zd);
            }
            after = DateTimeHelper.CurrentTimeMilliseconds;
            Console.WriteLine("norm = " + NormOps_DDRM.fastNormF(yd) + "  dense time  = " + (after - before) + " ms");

        }
    }
}