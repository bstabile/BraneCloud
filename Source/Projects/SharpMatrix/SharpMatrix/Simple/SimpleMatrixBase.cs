using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row;
using SharpMatrix.Ops;

namespace SharpMatrix.Simple
{
    public abstract class SimpleMatrixBase<TData, TMatrix, TSimple> : IEnumerable<TData>
        where TData : struct
        where TMatrix : class, Matrix, new()
        where TSimple : SimpleMatrixBase<TData, TMatrix, TSimple>, ISimpleMatrix<TData, TMatrix, TSimple>, new()
    {
        /**
         * Internal matrix which this is a wrapper around.
         */
        protected TMatrix mat;

        protected SimpleOperations<TData, TMatrix> ops;

        #region Protected Methods

        protected abstract void setMatrix(TMatrix mat);

        protected abstract TSimple wrapMatrix(TMatrix m);

        protected abstract TSimple createMatrix(int numRows, int numCols);

        #endregion // Protected Methods

        #region Public Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<TData> GetEnumerator();

        /// <summary>
        /// Returns the type of matrix is is wrapping.
        /// </summary>
        public MatrixType getType()
        {
            return mat.getType();
        }

        /// <summary>
        /// Size of internal array elements.  32 or 64 bits
        /// </summary>
        public int bits()
        {
            return mat.getType().getBits();
        }

        /// <summary>
        /// Returns true if this matrix is a vector.  
        /// A vector is defined as a matrix that has either one row or column.
        /// </summary>
        public bool isVector()
        {
            return mat.getNumRows() == 1 || mat.getNumCols() == 1;
        }

        /// <summary>
        /// Returns the number of rows in this matrix.
        /// </summary>
        public int numRows()
        {
            return mat.getNumRows();
        }

        /// <summary>
        /// Returns the number of columns in this matrix.
        /// </summary>
        public int numCols()
        {
            return mat.getNumCols();
        }

        /// <summary>
        /// Returns the number of elements in this matrix, which is 
        /// equal to the number of rows times the number of columns.
        /// </summary>
        public int getNumElements()
        {
            return mat.getNumRows() * mat.getNumCols();
        }

        /// <summary>
        /// Returns a reference to the matrix that it uses internally.  This is useful
        /// when an operation is needed that is not provided by this class.
        /// </summary>
        public TMatrix getMatrix()
        {
            return mat;
        }

        /// <summary>
        /// Returns the value of the specified matrix element.  
        /// Performs a bounds check to make sure the 
        /// requested element is part of the matrix.
        /// </summary>
        public abstract TData get(int row, int col);

        /// <summary>
        /// Returns the value of the matrix at the specified index of the 1D row major array.
        /// </summary>
        public abstract TData get(int index);

        /// <summary>
        /// Returns the index in the matrix's array.
        /// </summary>
        public int getIndex(int row, int col)
        {
            return row * mat.getNumCols() + col;
        }

        /// <summary>
        /// Sets all the elements in the matrix equal to zero.
        /// </summary>
        /// <see cref="CommonOps_DDRM.fill(DMatrixRMaj, double)"/>
        public abstract void zero();

        /// <summary>
        /// Sets all the elements in this matrix equal to the specified value.
        /// </summary>
        /// <see cref="CommonOps_DDRM.fill(DMatrixD1, double)"/>
        public abstract void set(TData val);

        /// <summary>
        /// Sets the elements in this matrix to be equal to the elements in the passed in matrix.
        /// Both matrix must have the same dimension.
        /// </summary>
        public void set(TSimple a)
        {
            mat.set(a.getMatrix());
        }

        /// <summary>
        /// Assigns the element in the Matrix to the specified value.  
        /// Performs a bounds check to make sure the requested element is part of the matrix.
        /// </summary>
        public abstract void set(int row, int col, TData value);

        /// <summary>
        /// Assigns an element a value based on its index in the internal array.
        /// </summary>
        public abstract void set(int index, TData value);

        /// <summary>
        /// Assigns consecutive elements inside a row to the provided array.
        /// </summary>
        public abstract void setRow(int row, int startColumn, TData[] values);

        /// <summary>
        /// Assigns consecutive elements inside a column to the provided array.
        /// A(offset:(offset + values.Length),column) = values
        /// </summary>
        public abstract void setColumn(int column, int startRow, TData[] values);

        /// <summary>
        /// Copy matrix B into this matrix at location (insertRow, insertCol).
        /// </summary>
        public abstract void insertIntoThis(int insertRow, int insertCol, TSimple B);

        /// <summary>
        /// Reshapes the matrix to the specified number of rows and columns.  
        /// If the total number of elements is &lt; number of elements it had 
        /// before the data is saved.  Otherwise a new internal array is
        /// declared and the old data lost.
        /// </summary>
        public void reshape(int numRows, int numCols)
        {
            if (mat.getType().isFixed())
                throw new ArgumentException("Can't rename a fixed sized matrix");

            ((ReshapeMatrix)mat).reshape(numRows, numCols);
        }

        /// <summary>
        /// Computes the dot product (a.k.a. inner product) between this vector and vector 'v'.
        /// </summary>
        /// <param name="v">The second vector in the dot product.  Not modified.</param>
        public abstract TData dot(TSimple v);

        /// <summary>
        /// Computes the Frobenius normal of the matrix:
        /// <code>normF = Math.Sqrt(A.Sum(v => Math.Abs(v * v))) /* fast version */</code>
        /// </summary>
        /// <see cref="NormOps_DDRM.normF(DMatrixD1)"/>
        public TData normF()
        {
            return ops.normF(mat);
        }

        /// <summary>
        /// The condition p = 2 number of a matrix is used to measure 
        /// the sensitivity of the linear system <b>Ax=b</b>.  
        /// A value near one indicates that it is a well conditioned matrix.
        /// </summary>
        /// <see cref="NormOps_DDRM.conditionP2(DMatrixRMaj)"/>
        public TData conditionP2()
        {
            return ops.conditionP2(mat);
        }

        /// <summary>
        /// Computes the determinant of the matrix.
        /// </summary>
        public TData determinant()
        {
            TData ret = ops.determinant(mat);
            // HACK!!!
            // Can't cast TData to double or float so...
            // Test for NaN and Infinity (uncountable) using ToString()
            var s = ret.ToString();
            if (s.Contains("NaN") || s.Contains("\u221E"))
                return default(TData); // zero for double or float
            return ret;
        }

        /// <summary>
        /// Computes the trace of the matrix.
        /// </summary>
        public TData trace()
        {
            return ops.trace(mat);
        }

        /// <summary>
        /// Returns true of the specified matrix element is valid element inside this matrix.
        /// </summary>
        public bool isInBounds(int row, int col)
        {
            return row >= 0 && col >= 0 && row < mat.getNumRows() && col < mat.getNumCols();
        }

        /// <summary>
        /// Checks to see if matrix 'a' is the same as this matrix within the specified tolerance.
        /// </summary>
        public abstract bool isIdentical(TSimple a, TData tol);

        /// <summary>
        /// Checks to see if any of the elements in this matrix are either NaN or infinite.
        /// </summary>
        /// <returns>True if an element is NaN or infinite.  False otherwise.</returns>
        public bool hasUncountable()
        {
            return ops.hasUncountable(mat);
        }

        /// <summary>
        /// Extracts the specified rows from the matrix.
        /// </summary>
        /// <param name="begin">First row.  Inclusive.</param>
        /// <param name="end">Last row + 1.</param>
        /// <returns></returns>
        public TSimple rows(int begin, int end)
        {
            return extractMatrix(begin, end, 0, SimpleMatrixD.END);
        }

        /// <summary>
        /// Extracts the specified rows from the matrix.
        /// </summary>
        /// <param name="begin">First row.  Inclusive.</param>
        /// <param name="end">Last row + 1.</param>
        /// <returns>Submatrix that contains the specified rows.</returns>
        public TSimple cols(int begin, int end)
        {
            return extractMatrix(0, SimpleMatrixD.END, begin, end);
        }

        /// <summary>
        /// Concatinates all the matrices together along their columns.  
        /// If the rows do not match the upper elements are set to zero.
        /// <code>A = [ m[0] , ... , m[n-1] ]</code>
        /// </summary>
        /// <param name="A">Set of matrices</param>
        /// <returns>Resulting matrix.</returns>
        public abstract TSimple concatColumns(params TSimple[] A);

        /// <summary>
        /// Concatinates all the matrices together along their columns.  
        /// If the rows do not match the upper elements are set to zero.
        /// <code>A = [ m[0] ; ... ; m[n-1] ]</code>
        /// </summary>
        /// <param name="A">A Set of matrices.</param>
        /// <returns>Resulting matrix.</returns>
        public abstract TSimple concatRows(params TSimple[] A);

        /// <summary>
        /// Returns the transpose of this matrix.
        /// </summary>
        public TSimple transpose()
        {
            TSimple ret = createMatrix(mat.getNumCols(), mat.getNumRows());
            ops.transpose(mat, mat);
            return ret;
        }

        /// <summary>
        /// Returns a matrix which is the result of matrix multiplication.
        /// <code>c = a * b</code>
        /// where c is the returned matrix, a is this matrix, and b is the passed in matrix.
        /// </summary>
        /// <see cref="CommonOps_DDRM.mult(DMatrix1Row, DMatrix1Row, DMatrix1Row)"/>
        /// <para name="b">A matrix that is n by bn. Not modified.</para>
        public TSimple mult(TSimple b)
        {
            TSimple ret = createMatrix(mat.getNumRows(), b.getMatrix().getNumCols());
            ops.mult(mat, b.mat, ret.mat);
            return ret;
        }

        /// <summary>
        /// Computes the Kronecker product between this matrix and the provided B matrix:
        /// <code>C = kron(A, B)</code>
        /// </summary>
        /// <param name="B">The right matrix in the operation. Not modified.</param>
        /// <returns>Kronecker product between this matrix and B.</returns>
        /// <see cref="CommonOps_DDRM.kron"/>
        public TSimple kron(TSimple B)
        {
            TSimple ret = createMatrix(mat.getNumRows() * B.numRows(), mat.getNumCols() * B.numCols());
            ops.kron(mat, B.mat, ret.mat);
            return ret;
        }

        /// <summary>
        /// Returns the result of matrix addition.
        /// </summary>
        /// <see cref="SimpleOperations{T}"/>
        public TSimple plus(TSimple b)
        {
            TSimple ret = createMatrix(mat.getNumRows(), mat.getNumCols());
            ops.plus(mat, b.mat, ret.mat);
            return ret;
        }

        /// <summary>
        /// Returns the result of matrix subtraction.
        /// <code>c = a - b</code>
        /// where c is the returned matrix, a is this matrix, and b is the passed in matrix.
        /// </summary>
        /// <see cref="SimpleOperations{T}"/>
        public TSimple minus(TSimple b)
        {
            TSimple ret = createMatrix(mat.getNumRows(), mat.getNumCols());
            ops.minus(mat, b.mat, ret.mat);
            return ret;
        }

        /// <summary>
        /// Returns the result of matrix-double subtraction:
        /// <code>c = a - b</code>
        /// where c is the returned matrix, a is this matrix, and b is the passed in double.
        /// </summary>
        /// <see cref="CommonOps_DDRM.subtract(DMatrixD1, double, DMatrixD1)"/>
        public abstract TSimple minus(TData b);

        /// <summary>
        /// Returns the result of scalar addition:
        /// <code>c = a + b</code>
        /// where c is the return matrix, a is this matrix, and b is the passed in double.
        /// </summary>
        /// <see cref="CommonOps_DDRM.add(DMatrixD1, double, DMatrixD1)"/>
        public abstract TSimple plus(TData b);

        /// <summary>
        /// Performs a matrix addition and scale operation.
        /// <code>c = a + beta*b</code>
        /// where c is the returned matrix, a is this matrix, and b is the passed in matrix.
        /// </summary>
        /// <see cref="CommonOps_DDRM.add(DMatrixD1, double, DMatrixD1, DMatrixD1)"/>
        public abstract TSimple plus(TData beta, TSimple b);

        /// <summary>
        ///  Returns the result of scaling each element by 'val':
        /// <code>b[i,j] = val*a[i,j]</code>
        /// </summary>
        /// <param name="val">The multiplication factor.</param>
        /// <see cref="CommonOps_DDRM.scale(double, DMatrixRMaj)"/>
        public abstract TSimple scale(TData val);

        /// <summary>
        /// Returns the inverse of this matrix.
        /// If the matrix could not be inverted then SingularMatrixException is thrown.  
        /// Even if no exception is thrown the matrix could still be singular or nearly singular.
        /// </summary>
        /// <exception cref="SingularMatrixException"/>
        /// <see cref="CommonOps_DDRM.invert(DMatrixRMaj, DMatrixRMaj)"/>
        public TSimple invert()
        {
            TSimple ret = createMatrix(mat.getNumRows(), mat.getNumCols());

            if (!ops.invert(mat, ret.mat))
                throw new SingularMatrixException();
            if (ops.hasUncountable(ret.mat))
                throw new SingularMatrixException("Solution contains uncountable numbers");

            return ret;
        }

        /// <summary>
        /// Computes the Moore-Penrose pseudo-inverse.
        /// </summary>
        public abstract TSimple pseudoInverse();

        /// <summary>
        /// Returns the result of dividing each element by 'val':
        /// <code>b[i,j] = a[i,j]/val</code>
        /// </summary>
        /// <param name="val">Divisor</param>
        /// <returns>Matrix with its elements divided by the specified value.</returns>
        /// <see cref="CommonOps_DDRM.divide(DMatrixD1, double)"/>
        public abstract TSimple divide(TData val);

        /// <summary>
        /// Solves for X in the following equation:
        /// <code>x = pow(a, -1)</code>
        /// where 'a' is this matrix and 'b' is an n by p matrix.
        /// If the system could not be solved then SingularMatrixException is thrown.  
        /// Even if no exception is thrown 'a' could still be singular or nearly singular.
        /// </summary>
        /// <see cref="CommonOps_DDRM.solve(DMatrixRMaj, DMatrixRMaj, DMatrixRMaj)"/>
        public TSimple solve(TSimple b)
        {
            TSimple x = createMatrix(mat.getNumCols(), b.getMatrix().getNumCols());

            if (!ops.solve(mat, x.mat, b.mat))
                throw new SingularMatrixException();
            if (ops.hasUncountable(x.mat))
                throw new SingularMatrixException("Solution contains uncountable numbers");

            return x;
        }

        /// <summary>
        /// Creates and returns a matrix which is idential to this one.
        /// </summary>
        public TSimple copy()
        {
            TSimple ret = createMatrix(mat.getNumRows(), mat.getNumCols());
            ret.getMatrix().set(getMatrix());
            return ret;
        }

        /// <summary>
        /// Creates a new SimpleMatrix which is a submatrix of this matrix.
        /// <code>s[i-y0, j-x0] = o[ij] // for all y0 &lt;= i &lt; y1 and x0 &lt;= j &lt; x1.</code>
        /// </summary>
        /// <param name="y0">Start row.</param>
        /// <param name="y1">Stop row + 1.</param>
        /// <param name="x0">Start column.</param>
        /// <param name="x1">Stop column + 1.</param>
        /// <returns>The submatrix.</returns>
        public TSimple extractMatrix(int y0, int y1, int x0, int x1)
        {
            if (y0 == SimpleMatrixD.END) y0 = mat.getNumRows();
            if (y1 == SimpleMatrixD.END) y1 = mat.getNumRows();
            if (x0 == SimpleMatrixD.END) x0 = mat.getNumCols();
            if (x1 == SimpleMatrixD.END) x1 = mat.getNumCols();

            TSimple ret = createMatrix(y1 - y0, x1 - x0);

            ops.extract(mat, y0, y1, x0, x1, mat, 0, 0);

            return ret;
        }

        /// <summary>
        /// Extracts a row or column from this matrix. 
        /// The returned vector will either be a row
        /// or column vector depending on the input type.
        /// </summary>
        /// <param name="extractRow">If true a row will be extracted.</param>
        /// <param name="element">The row or column the vector is contained in.</param>
        /// <returns>Extracted vector.</returns>
        public abstract TSimple extractVector(bool extractRow, int element);

        /// <summary>
        /// Returns the maximum absolute value of all the elements in this matrix.  
        /// This is equivalent the the infinite p-norm of the matrix.
        /// </summary>
        /// <returns>Largest absolute value of any element.</returns>
        public abstract TData elementMaxAbs();

        /// <summary>
        /// Computes the sum of all the elements in the matrix.
        /// </summary>
        public TData elementSum()
        {
            return ops.elementSum(mat);
        }

        /// <summary>
        /// Returns a matrix which is the result of an element by element multiplication of 'this' and 'b':
        /// <code>c[i,j] = a[i,j] * b[i,j]</code>
        /// </summary>
        /// <param name="b">A simple matrix.</param>
        /// <returns>The element by element multiplication of 'this' and 'b'.</returns>
        public TSimple elementMult(TSimple b)
        {
            TSimple c = createMatrix(mat.getNumRows(), mat.getNumCols());
            ops.elementMult(mat, b.mat, c.mat);
            return c;
        }

        /// <summary>
        /// Returns a matrix which is the result of an element by element division of 'this' and 'b':
        /// <code>c[i,j] = a[i,j] / b[i,j]</code>
        /// </summary>
        /// <param name="b">A simple matrix.</param>
        /// <returns>The element by element division of 'this' and 'b'.</returns>
        public TSimple elementDiv(TSimple b)
        {
            TSimple c = createMatrix(mat.getNumRows(), mat.getNumCols());
            ops.elementDiv(mat, b.mat, c.mat);
            return c;
        }

        /// <summary>
        /// Returns a matrix which is the result of an element by element power of 'this' and 'b':
        /// <code>c[i,j] = a[i,j] ^ b[i,j]</code>
        /// </summary>
        /// <param name="b">A simple matrix.</param>
        /// <returns>The element by element power of 'this' and 'b'.</returns>
        public TSimple elementPower(TSimple b)
        {
            TSimple c = createMatrix(mat.getNumRows(), mat.getNumCols());
            ops.elementPower(mat, b.mat, c.mat);
            return c;
        }

        /// <summary>
        /// Returns a matrix which is the result of an element by element power of 'this' and 'b':
        /// <code>c[i,j] = a[i,j] ^ b</code>
        /// </summary>
        /// <param name="b">Scalar.</param>
        /// <returns>The element by element power of 'this' and 'b'.</returns>
        public TSimple elementPower(TData b)
        {
            TSimple c = createMatrix(mat.getNumRows(), mat.getNumCols());
            ops.elementPower(mat, b, c.mat);
            return c;
        }

        /// <summary>
        /// Returns a matrix which is the result of an element by element exp of 'this':
        /// <code>c[i,j] = Math.Exp(a[i,j])</code>
        /// </summary>
        /// <returns>The element by element power of 'this' and 'b'.</returns>
        public TSimple elementExp()
        {
            TSimple c = createMatrix(mat.getNumRows(), mat.getNumCols());
            ops.elementExp(mat, c.mat);
            return c;
        }

        /// <summary>
        /// Returns a matrix which is the result of an element by element exp of 'this':
        /// <code>c[i,j] = Math.Log(a[i,j])</code>
        /// </summary>
        public TSimple elementLog()
        {
            TSimple c = createMatrix(mat.getNumRows(), mat.getNumCols());
            ops.elementLog(mat, c.mat);
            return c;
        }

        /// <summary>
        /// Computes a full Singular Value Decomposition (SVD) of this matrix 
        /// with the eigenvalues ordered from largest to smallest.
        /// </summary>
        /// <returns></returns>
        public SimpleSVD<TMatrix> svd()
        {
            return new SimpleSVD<TMatrix>(mat, false);
        }

        /// <summary>
        /// Computes the SVD in either  compact format or full format.
        /// </summary>
        public SimpleSVD<TMatrix> svd(bool compact)
        {
            return new SimpleSVD<TMatrix>(mat, compact);
        }

        /// <summary>
        /// Returns the Eigen Value Decomposition (EVD) of this matrix.
        /// </summary>
        public SimpleEVD<TMatrix> eig()
        {
            return new SimpleEVD<TMatrix>(mat);
        }

        /// <summary>
        /// <p>Allows you to perform an equation in-place on this matrix by specifying the right hand side.For information on how to define an equation
        /// see {@link org.ejml.equation.Equation}.  The variable sequence alternates between variable and it's label String.
        /// This matrix is by default labeled as 'A', but is a string is the first object in 'variables' then it will take
        /// on that value.The variable passed in can be any data type supported by Equation can be passed in.
        /// This includes matrices and scalars.</p>
        ///
        /// Examples:<br/>
        /// <pre>
        /// perform("A = A + B", matrix,"B");     // Matrix addition
        /// perform("A + B", matrix,"B");         // Matrix addition with implicit 'A = '
        /// perform("A(5,:) = B", matrix,"B");    // Insert a row defined by B into A
        /// perform("[A;A]");                    // stack A twice with implicit 'A = '
        /// perform("Q = B + 2","Q", matrix,"B"); // Specify the name of 'this' as Q
        /// </pre>
        /// </summary>
        /// <param name="equation">String representing the symbol equation.</param>
        /// <param name="variables">List of variable names and variables.</param>
        public void equation(string equation, params object[] variables)
        {
            if (variables.Length >= 25)
                throw new ArgumentException("Too many variables!  At most 25");

            Equation.Equation eq = new Equation.Equation();

            string nameThis = "A";
            int offset = 0;
            if (variables.Length > 0 && variables[0] is string)
            {
                nameThis = (string)variables[0];
                offset = 1;

                if (variables.Length % 2 != 1)
                    throw new ArgumentException("Expected and odd length for variables");
            }
            else
            {
                if (variables.Length % 2 != 0)
                    throw new ArgumentException("Expected and even length for variables");
            }

            eq.alias(mat, nameThis);

            for (int i = offset; i < variables.Length; i += 2)
            {
                if (!(variables[i + 1] is string))
                    throw new ArgumentException("String expected at variables index " + i);
                object o = variables[i];
                string name = (string)variables[i + 1];

                //if (typeof(SimpleBase).IsAssignableFrom(o.GetType()))
                if (o is SimpleBase<DMatrixRMaj>)
                {
                    // we already know that we've got a matrix type of DMatrixRMaj
                    // so operations involving the same type of matrix should be okay (right?).
                    eq.alias(((SimpleMatrixD)o).getMatrix(), name);
                }
                else if (o is TMatrix)
                {
                    eq.alias((TMatrix)o, name);
                }
                else if (o is TData)
                {
                    eq.alias((TData)o, name);
                }
                else if (o is int)
                {
                    eq.alias((int)o, name);
                }
                else
                {
                    //string type = o == null ? "null" : o.GetType().Name;
                    string type = o?.GetType().Name ?? "null";
                    throw new ArgumentException("Variable type not supported by Equation! " + type);
                }
            }

            // see if the assignment is implicit
            if (!equation.Contains("="))
            {
                equation = nameThis + " = " + equation;
            }

            eq.process(equation);
        }

        /// <summary>
        /// Saves this matrix to a file as a serialized binary object.
        /// </summary>
        /// <exception cref="IOException"/>
        /// <see cref="MatrixIO.saveBin(DMatrix, string)"/>
        public void saveToFileBinary(string fileName)
        {
            if (mat is DMatrix)
                MatrixIO.saveBin((DMatrix) mat, fileName);
            else if (mat is FMatrix)
                MatrixIO.saveBin((FMatrix)mat, fileName);
            else
                throw new InvalidOperationException("Unknown or unsupported matrix type.");
        }

        /// <summary>
        /// Saves this matrix to a file in a CSV format.  
        /// For the file format <see cref="MatrixIO"/>.
        /// </summary>
        /// <param name="fileName"></param>
        public void saveToFileCSV(string fileName)
        {
            if (mat is DMatrix)
                MatrixIO.saveCSV((DMatrix) mat, fileName);
            else if (mat is FMatrix)
                MatrixIO.saveCSV((FMatrix) mat, fileName);
            else
                throw new InvalidOperationException("Unknown or unsupported matrix type.");
        }

        /// <summary>
        /// Loads a new matrix from a CSV file.  
        /// For the file format <see cref="MatrixIO"/>.
        /// </summary>
        /// <exception cref="IOException"/>
        public TSimple loadCSV(string fileName)
        {
            TSimple ret = createMatrix(1, 1);
            var m = (TMatrix) MatrixIO.loadCSV(fileName);
            ret.setMatrix(m);
            return ret;
        }

        /// <summary>
        /// Prints the matrix to standard out.
        /// </summary>
        public void print()
        {
            ops.print(Console.OpenStandardOutput(), mat);
        }

        /// <summary>
        /// Prints the matrix to standard out with the specified precision.
        /// Example resulting string output: {0:F2,5} (numChar = 5, precision = 2).
        /// </summary>
        public void print(int numChar, int precision)
        {
            if (mat is DMatrix)
                MatrixIO.print(Console.OpenStandardOutput(), (DMatrix) mat, numChar, precision);
            else if (mat is FMatrix)
                MatrixIO.print(Console.OpenStandardOutput(), (FMatrix) mat, numChar, precision);
            else
                throw new InvalidOperationException("Unknown or unsupported matrix type.");
        }

        /// <summary>
        /// Prints the matrix to standard out given a floating point format.
        /// Example format: {0:F2,5} (numChar = 5, precision = 2).
        /// </summary>
        public void print(string format)
        {
            if (mat is DMatrix)
                MatrixIO.print(Console.OpenStandardOutput(), (DMatrix)mat, format);
            else if (mat is FMatrix)
                MatrixIO.print(Console.OpenStandardOutput(), (FMatrix)mat, format);
            else
                throw new InvalidOperationException("Unknown or unsupported matrix type.");
        }

        /// <summary>
        /// Prints the number of rows and column in this matrix.
        /// </summary>
        public void printDimensions()
        {
            Console.WriteLine("[rows = " + numRows() + " , cols = " + numCols() + " ]");
        }

        /// <summary>
        /// Converts the array into a string format for display purposes.
        /// </summary>
        /// <see cref="MatrixIO.print(Stream, DMatrix)"/>
        public string toString()
        {
            using (var stream = new MemoryStream())
            {
                if (mat is DMatrix)
                    MatrixIO.print(stream, (DMatrix) mat);
                if (mat is FMatrix)
                    MatrixIO.print(stream, (FMatrix) mat);
                else
                    throw new InvalidOperationException("Unknown or unsupported matrix type.");
                stream.Position = 0;
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }


        #endregion // Public Methods

    }
}
