using System.IO;
using SharpMatrix.Data;

namespace SharpMatrix.Simple
{
    /**
     * High level interface for operations inside of SimpleMatrix for one matrix type.
     *
     * @author Peter Abeles
     */
    public interface SimpleOperations<T>
        where T : Matrix
    {

        void transpose(T input, T output);

        void mult(T A, T B, T output);

        void kron(T A, T B, T output);

        void plus(T A, T B, T output);

        void minus(T A, T B, T output);

        void minus(T A, double b, T output);

        void plus(T A, double b, T output);

        void plus(T A, double beta, T b, T output);

        double dot(T A, T v);

        void scale(T A, double val, T output);

        void divide(T A, double val, T output);

        bool invert(T A, T output);

        void pseudoInverse(T A, T output);

        bool solve(T A, T X, T B);

        void set(T A, double val);

        void zero(T A);

        double normF(T A);

        double conditionP2(T A);

        double determinant(T A);

        double trace(T A);

        void setRow(T A, int row, int startColumn, double[] values);

        void setColumn(T A, int column, int startRow, double[] values);

        void extract(T src,
            int srcY0, int srcY1,
            int srcX0, int srcX1,
            T dst,
            int dstY0, int dstX0);

        bool hasUncountable(T M);

        void changeSign(T a);

        double elementMaxAbs(T A);

        double elementSum(T A);

        void elementMult(T A, T B, T output);

        void elementDiv(T A, T B, T output);

        void elementPower(T A, T B, T output);

        void elementPower(T A, double b, T output);

        void elementExp(T A, T output);

        void elementLog(T A, T output);

        void print(Stream output, Matrix mat);
    }
}