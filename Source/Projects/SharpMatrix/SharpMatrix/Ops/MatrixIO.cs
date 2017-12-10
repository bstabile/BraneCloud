using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using SharpMatrix.Data;

namespace SharpMatrix.Ops
{
    /**
     * Provides simple to use routines for reading and writing matrices to and from files.
     *
     * @author Peter Abeles
     */
    public class MatrixIO
    {

        /**
         * Saves a matrix to disk using Java binary serialization.
         *
         * @param A The matrix being saved.
         * @param fileName Name of the file its being saved at.
         * @throws java.io.IOException
         */
        public static void saveBin(FMatrix A, string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, A);
                stream.Flush();
            }
        }

        /**
         * Saves a matrix to disk using Java binary serialization.
         *
         * @param A The matrix being saved.
         * @param fileName Name of the file its being saved at.
         * @throws java.io.IOException
         */
        public static void saveBin(DMatrix A, string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, A);
                stream.Flush();
            }
        }

        /**
         * Loads a DeneMatrix64F which has been saved to file using Java binary
         * serialization.
         *
         * @param fileName The file being loaded.
         * @return  DMatrixRMaj
         * @throws IOException
         */
        public static T loadBin<T>(string fileName) where T : Matrix
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var formatter = new BinaryFormatter();

                T ret = (T) formatter.Deserialize(stream);

                stream.Close();
                return ret;
            }
        }

        /**
         * Saves a matrix to disk using in a Column Space Value (CSV) format. For a 
         * description of the format see {@link MatrixIO#loadCSV(String)}.
         *
         * @param A The matrix being saved.
         * @param fileName Name of the file its being saved at.
         * @throws java.io.IOException
         */
        public static void saveCSV(FMatrix A, string fileName)
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine(A.getNumRows() + " " + A.getNumCols() + " real");
            for (int i = 0; i < A.getNumRows(); i++)
            {
                for (int j = 0; j < A.getNumCols(); j++)
                {
                    sb = sb.Append(A.get(i, j) + " ");
                }
                sb = sb.AppendLine();
            }
            using (var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(sb.ToString());
            }
        }

        /**
         * Saves a matrix to disk using in a Column Space Value (CSV) format. For a 
         * description of the format see {@link MatrixIO#loadCSV(String)}.
         *
         * @param A The matrix being saved.
         * @param fileName Name of the file its being saved at.
         * @throws java.io.IOException
         */
        public static void saveCSV(DMatrix A, string fileName)
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine(A.getNumRows() + " " + A.getNumCols() + " real");
            for (int i = 0; i < A.getNumRows(); i++)
            {
                for (int j = 0; j < A.getNumCols(); j++)
                {
                    sb = sb.Append(A.get(i, j) + " ");
                }
                sb = sb.AppendLine();
            }
            using (var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(sb.ToString());
            }
        }

        /**
         * Reads a matrix in which has been encoded using a Column Space Value (CSV)
         * file format. The number of rows and columns are read in on the first line. Then
         * each row is read in the subsequent lines.
         *
         * @param fileName The file being loaded.
         * @return DMatrixRMaj
         * @throws IOException
         * @throws InvalidCastException (if type is ZMatrixRMaj instead of DMatrixRMaj)
         */
        public static Matrix loadCSV(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                ReadMatrixCsv csv = new ReadMatrixCsv(stream);
                Matrix ret = csv.read();
                return ret;
            }
        }

        /**
         * Reads a matrix in which has been encoded using a Column Space Value (CSV)
         * file format.  For a description of the format see {@link MatrixIO#loadCSV(String)}.
         *
         * @param fileName The file being loaded.
         * @param numRows number of rows in the matrix.
         * @param numCols number of columns in the matrix.
         * @return DMatrixRMaj
         * @throws IOException
         */
        public static DMatrixRMaj loadCSV(string fileName, int numRows, int numCols)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                ReadMatrixCsv csv = new ReadMatrixCsv(stream);
                DMatrixRMaj ret = csv.readReal(numRows, numCols);
                stream.Close();
                return ret;
            }
        }

        public static void print(Stream output, DMatrix mat)
        {
            print(output, mat, 6, 3);
        }

        public static void print(Stream output, DMatrix mat, int numChar, int precision)
        {
            string format = "%" + numChar + "." + precision + "f";

            print(output, mat, format);
        }

        public static void print(Stream output, DMatrix mat, string format)
        {

            string type = mat is ReshapeMatrix ? "dense64" : "dense64 fixed";

            Console.WriteLine("Type = " + type + " real , numRows = " + mat.getNumRows() + " , numCols = " +
                              mat.getNumCols());

            format += " ";

            for (int y = 0; y < mat.getNumRows(); y++)
            {
                for (int x = 0; x < mat.getNumCols(); x++)
                {
                    Console.Write(format, mat.get(y, x));
                }
                Console.WriteLine();
            }
        }

        public static void print(Stream output, FMatrix mat)
        {
            print(output, mat, 6, 3);
        }

        public static void print(Stream output, FMatrix mat, int numChar, int precision)
        {
            string format = "%" + numChar + "." + precision + "f ";

            print(output, mat, format);
        }

        public static void print(Stream output, FMatrix mat, string format)
        {

            string type = mat.GetType().Name;

            Console.WriteLine(
                "Type = " + type + " , numRows = " + mat.getNumRows() + " , numCols = " + mat.getNumCols());

            format += " ";

            for (int y = 0; y < mat.getNumRows(); y++)
            {
                for (int x = 0; x < mat.getNumCols(); x++)
                {
                    Console.Write(format, mat.get(y, x));
                }
                Console.WriteLine();
            }
        }

        public static void print(Stream output, DMatrix mat, string format,
            int row0, int row1, int col0, int col1)
        {
            Console.WriteLine("Type = submatrix , rows " + row0 + " to " + row1 + "  columns " + col0 + " to " + col1);

            format += " ";

            for (int y = row0; y < row1; y++)
            {
                for (int x = col0; x < col1; x++)
                {
                    Console.Write(format, mat.get(y, x));
                }
                Console.WriteLine();
            }
        }

        public static void print(Stream output, FMatrix mat, string format,
            int row0, int row1, int col0, int col1)
        {
            Console.WriteLine("Type = submatrix , rows " + row0 + " to " + row1 + "  columns " + col0 + " to " + col1);

            format += " ";

            for (int y = row0; y < row1; y++)
            {
                for (int x = col0; x < col1; x++)
                {
                    Console.Write(format, mat.get(y, x));
                }
                Console.WriteLine();
            }
        }

        public static void print(Stream output, ZMatrix mat)
        {
            print(output, mat, 6, 3);
        }

        public static void print(Stream output, CMatrix mat)
        {
            print(output, mat, 6, 3);
        }

        public static void print(Stream output, ZMatrix mat, int numChar, int precision)
        {
            string format = "%" + numChar + "." + precision + "f + %" + numChar + "." + precision + "fi";

            print(output, mat, format);
        }

        public static void print(Stream output, CMatrix mat, int numChar, int precision)
        {
            string format = "%" + numChar + "." + precision + "f + %" + numChar + "." + precision + "fi";

            print(output, mat, format);
        }

        public static void print(Stream output, ZMatrix mat, string format)
        {

            string type = "dense64";

            Console.WriteLine("Type = " + type + " complex , numRows = " + mat.getNumRows() + " , numCols = " +
                              mat.getNumCols());

            format += " ";

            Complex_F64 c = new Complex_F64();
            for (int y = 0; y < mat.getNumRows(); y++)
            {
                for (int x = 0; x < mat.getNumCols(); x++)
                {
                    mat.get(y, x, c);
                    Console.Write(format, c.real, c.imaginary);
                    if (x < mat.getNumCols() - 1)
                    {
                        Console.Write(" , ");
                    }
                }
                Console.WriteLine();
            }
        }

        public static void print(Stream output, CMatrix mat, string format)
        {

            string type = "dense32";

            Console.WriteLine("Type = " + type + " complex , numRows = " + mat.getNumRows() + " , numCols = " +
                              mat.getNumCols());

            format += " ";

            Complex_F32 c = new Complex_F32();
            for (int y = 0; y < mat.getNumRows(); y++)
            {
                for (int x = 0; x < mat.getNumCols(); x++)
                {
                    mat.get(y, x, c);
                    Console.Write(format, c.real, c.imaginary);
                    if (x < mat.getNumCols() - 1)
                    {
                        Console.Write(" , ");
                    }
                }
                Console.WriteLine();
            }
        }

        //    public static void main( String []args ) {
        //        IMersenneTwister rand = new Random(234234);
        //        DMatrixRMaj A = RandomMatrices.createRandom(50,70,rand);
        //
        //        SingularValueDecomposition decomp = DecompositionFactory.svd();
        //
        //        decomp.decompose(A);
        //
        //        displayMatrix(A,"Original");
        //        displayMatrix(decomp.getU(false),"U");
        //        displayMatrix(decomp.getV(false),"V");
        //        displayMatrix(decomp.getW(null),"W");
        //    }
    }
}