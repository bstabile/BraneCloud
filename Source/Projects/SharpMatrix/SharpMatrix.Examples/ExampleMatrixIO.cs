using System;
using System.IO;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Ops;
using BraneCloud.Evolution.EC.MatrixLib.Simple;

namespace BraneCloud.Evolution.EC.MatrixLib.Examples
{
    //package org.ejml.example;

/**
 * Examples for reading and writing matrices to files in different formats
 *
 * @author Peter Abeles
 */
    public class ExampleMatrixIO
    {

        public static void csv()
        {
            DMatrixRMaj A = new DMatrixRMaj(2, 3, true, new double[] {1, 2, 3, 4, 5, 6});

            try
            {
                MatrixIO.saveCSV(A, "matrix_file.csv");
                DMatrixRMaj B = MatrixIO.loadCSV("matrix_file.csv");
                B.print();
            }
            catch (IOException e)
            {
                throw new InvalidOperationException(e.Message, e);
            }
        }

        public static void csv_simple()
        {
            SimpleMatrix<DMatrixRMaj> A = new SimpleMatrix<DMatrixRMaj>(2, 3, true, new double[] {1, 2, 3, 4, 5, 6});

            try
            {
                A.saveToFileCSV("matrix_file.csv");
                SimpleMatrix<DMatrixRMaj> B = new SimpleMatrix<DMatrixRMaj>().loadCSV("matrix_file.csv") as SimpleMatrix<DMatrixRMaj>;
                B.print();
            }
            catch (IOException e)
            {
                throw new InvalidOperationException(e.Message, e);
            }
        }

        public static void serializedBinary()
        {
            DMatrixRMaj A = new DMatrixRMaj(2, 3, true, new double[] {1, 2, 3, 4, 5, 6});

            try
            {
                MatrixIO.saveBin(A, "matrix_file.data");
                DMatrixRMaj B = MatrixIO.loadBin<DMatrixRMaj>("matrix_file.data");
                B.print();
            }
            catch (IOException e)
            {
                throw new InvalidOperationException(e.Message, e);
            }
        }

        public static void csv_serializedBinary()
        {
            SimpleMatrix<DMatrixRMaj> A = new SimpleMatrix<DMatrixRMaj>(2, 3, true, new double[] {1, 2, 3, 4, 5, 6});

            try
            {
                A.saveToFileBinary("matrix_file.data");
                SimpleMatrix<DMatrixRMaj> B = SimpleMatrix<DMatrixRMaj>.loadBinary("matrix_file.data");
                B.print();
            }
            catch (IOException e)
            {
                throw new InvalidOperationException(e.Message, e);
            }
        }

        public static void main(string[] args)
        {
            csv();
            serializedBinary();
        }
    }
}