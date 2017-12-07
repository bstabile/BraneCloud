using System;
using System.Collections.Generic;
using System.IO;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Ops
{
    /**
     * Reads in a matrix that is in a column-space-value (CSV) format.
     *
     * @author Peter Abeles
     */
    public class ReadMatrixCsv : ReadCsv
    {

        /**
         * Specifies where input comes from.
         *
         * @param in Where the input comes from.
         */
        public ReadMatrixCsv(Stream input)
            : base(input)
        {
        }

        /**
         * Reads in a DMatrixRMaj from the IO stream.
         * @return DMatrixRMaj
         * @throws IOException If anything goes wrong.
         */
        public Matrix read()
        {
            List<string> words = extractWords();
            if (words.Count != 3)
                throw new IOException("Unexpected number of words on first line.");

            int numRows = int.Parse(words[0]);
            int numCols = int.Parse(words[1]);
            bool real = words[2].Equals("real", StringComparison.InvariantCultureIgnoreCase);

            if (numRows < 0 || numCols < 0)
                throw new IOException("Invalid number of rows and/or columns: " + numRows + " " + numCols);

            if (real)
                return readReal(numRows, numCols);
            else
                return readComplex(numRows, numCols);
        }

        /**
         * Reads in a DMatrixRMaj from the IO stream where the user specifies the matrix dimensions.
         *
         * @param numRows Number of rows in the matrix
         * @param numCols Number of columns in the matrix
         * @return DMatrixRMaj
         * @throws IOException
         */
        public DMatrixRMaj readReal(int numRows, int numCols)
        {

            DMatrixRMaj A = new DMatrixRMaj(numRows, numCols);

            for (int i = 0; i < numRows; i++)
            {
                List<string> words = extractWords();
                if (words == null)
                    throw new IOException("Too few rows found. expected " + numRows + " actual " + i);

                if (words.Count != numCols)
                    throw new IOException("Unexpected number of words in column. Found " + words.Count + " expected " +
                                          numCols);
                for (int j = 0; j < numCols; j++)
                {
                    A.set(i, j, double.Parse(words[j]));
                }
            }

            return A;
        }

        /**
         * Reads in a ZMatrixRMaj from the IO stream where the user specifies the matrix dimensions.
         *
         * @param numRows Number of rows in the matrix
         * @param numCols Number of columns in the matrix
         * @return DMatrixRMaj
         * @throws IOException
         */
        public ZMatrixRMaj readComplex(int numRows, int numCols)
        {

            ZMatrixRMaj A = new ZMatrixRMaj(numRows, numCols);

            int wordsCol = numCols * 2;

            for (int i = 0; i < numRows; i++)
            {
                List<string> words = extractWords();
                if (words == null)
                    throw new IOException("Too few rows found. expected " + numRows + " actual " + i);

                if (words.Count != wordsCol)
                    throw new IOException("Unexpected number of words in column. Found " + words.Count + " expected " +
                                          wordsCol);
                for (int j = 0; j < wordsCol; j += 2)
                {

                    double real = double.Parse(words[j]);
                    double imaginary = double.Parse(words[j + 1]);

                    A.set(i, j, real, imaginary);
                }
            }

            return A;
        }
    }
}