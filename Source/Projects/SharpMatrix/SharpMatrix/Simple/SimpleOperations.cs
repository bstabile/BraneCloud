using System.IO;
using SharpMatrix.Data;

namespace SharpMatrix.Simple
{
    /**
     * High level interface for operations inside of SimpleMatrix for one matrix type.
     *
     * @author Peter Abeles
     */
    public interface SimpleOperations<TData, TMatrix>
        where TData : struct
        where TMatrix : Matrix
    {

        void transpose(TMatrix input, TMatrix output);

        void mult(TMatrix A, TMatrix B, TMatrix output);

        void kron(TMatrix A, TMatrix B, TMatrix output);

        void plus(TMatrix A, TMatrix B, TMatrix output);

        void minus(TMatrix A, TMatrix B, TMatrix output);

        void minus(TMatrix A, TData b, TMatrix output);

        void plus(TMatrix A, TData b, TMatrix output);

        void plus(TMatrix A, TData beta, TMatrix b, TMatrix output);

        TData dot(TMatrix A, TMatrix v);

        void scale(TMatrix A, TData val, TMatrix output);

        void divide(TMatrix A, TData val, TMatrix output);

        bool invert(TMatrix A, TMatrix output);

        void pseudoInverse(TMatrix A, TMatrix output);

        bool solve(TMatrix A, TMatrix X, TMatrix B);

        void set(TMatrix A, TData val);

        void zero(TMatrix A);

        TData normF(TMatrix A);

        TData conditionP2(TMatrix A);

        TData determinant(TMatrix A);

        TData trace(TMatrix A);

        void setRow(TMatrix A, int row, int startColumn, TData[] values);

        void setColumn(TMatrix A, int column, int startRow, TData[] values);

        void extract(TMatrix src,
            int srcY0, int srcY1,
            int srcX0, int srcX1,
            TMatrix dst,
            int dstY0, int dstX0);

        bool hasUncountable(TMatrix M);

        void changeSign(TMatrix a);

        TData elementMaxAbs(TMatrix A);

        TData elementSum(TMatrix A);

        void elementMult(TMatrix A, TMatrix B, TMatrix output);

        void elementDiv(TMatrix A, TMatrix B, TMatrix output);

        void elementPower(TMatrix A, TMatrix B, TMatrix output);

        void elementPower(TMatrix A, TData b, TMatrix output);

        void elementExp(TMatrix A, TMatrix output);

        void elementLog(TMatrix A, TMatrix output);

        void print(Stream output, Matrix mat);

    }
}