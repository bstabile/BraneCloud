using System.Collections.Generic;
using SharpMatrix.Data;

namespace SharpMatrix.Simple
{
    public interface ISimpleMatrix<TData, TMatrix, TSimple> : IEnumerable<TData>
        where TData : struct
        where TMatrix : class, Matrix
    {
        MatrixType getType();

        int bits();
        bool isVector();
        int numRows();
        int numCols();
        int getNumElements();

        TMatrix getMatrix();
        TData get(int row, int col);
        TData get(int index);
        int getIndex(int row, int col);
        //DMatrixIterator iterator(bool rowMajor, int minRow, int minCol, int maxRow, int maxCol);

        void zero();
        void set(TData val);
        void set(TSimple a);
        void set(int row, int col, TData value);
        void set(int index, TData value);
        void setRow(int row, int startColumn, TData[] values);
        void setColumn(int column, int startRow, TData[] values);
        void insertIntoThis(int insertRow, int insertCol, TSimple B);
        void reshape(int numRows, int numCols);

        TData dot(TSimple v);
        TData normF();
        TData conditionP2();
        TData determinant();
        TData trace();

        bool isInBounds(int row, int col);
        bool isIdentical(TSimple a, TData tol);
        bool hasUncountable();

        TSimple rows(int begin, int end);
        TSimple cols(int begin, int end);
        TSimple concatColumns(params TSimple[] A);
        TSimple concatRows(params TSimple[] A);

        TSimple transpose();
        TSimple mult(TSimple b);
        TSimple kron(TSimple B);
        TSimple plus(TSimple b);
        TSimple minus(TSimple b);
        TSimple minus(TData b);
        TSimple plus(TData b);
        TSimple plus(TData beta, TSimple b);
        TSimple scale(TData val);
        TSimple divide(TData val);
        TSimple invert();
        TSimple pseudoInverse();
        TSimple solve(TSimple b);

        TSimple copy();
        TSimple extractMatrix(int y0, int y1, int x0, int x1);
        TSimple extractVector(bool extractRow, int element);
        TSimple diag();
        TSimple combine(int insertRow, int insertCol, TSimple B);
        TSimple negative();


        TData elementMaxAbs();
        TData elementSum();
        TSimple elementMult(TSimple b);
        TSimple elementDiv(TSimple b);
        TSimple elementPower(TSimple b);
        TSimple elementPower(TData b);
        TSimple elementExp();
        TSimple elementLog();


        SimpleSVD<TMatrix> svd();
        SimpleSVD<TMatrix> svd(bool compact);
        SimpleEVD<TMatrix> eig();


        void equation(string equation, params object[] variables);


        void saveToFileBinary(string fileName);
        void saveToFileCSV(string fileName);
        TSimple loadCSV(string fileName);


        void print();
        void print(int numChar, int precision);
        void print(string format);
        void printDimensions();

        string toString();

    }
}