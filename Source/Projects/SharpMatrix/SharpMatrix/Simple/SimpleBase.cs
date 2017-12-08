using System;
using System.CodeDom;
using System.IO;
using SharpMatrix.Data;
using SharpMatrix.Dense;
using SharpMatrix.Dense.Row;
using SharpMatrix.Ops;
using SharpMatrix.Simple.Ops;

namespace SharpMatrix.Simple
{
/**
 * Parent of {@link SimpleMatrix} implements all the standard matrix operations and uses
 * generics to allow the returned matrix type to be changed.  This class should be extended
 * instead of SimpleMatrix.
 *
 * @author Peter Abeles
 */
//@SuppressWarnings({"unchecked"})
    [Serializable]
    public abstract class SimpleBase<T> where T : class, Matrix
    {

        const long serialVersionUID = 2342556642L;

        /**
         * A simplified way to reference the last row or column in the matrix for some functions.
         */
        public const int END = int.MaxValue;

        /**
         * Internal matrix which this is a wrapper around.
         */
        protected T mat;

        protected SimpleOperations<T> ops;

        public SimpleBase(int numRows, int numCols)
        {
            if (typeof(T) == typeof(DMatrixRMaj))
                setMatrix(new DMatrixRMaj(numRows, numCols) as T);
            else if (typeof(T) == typeof(FMatrixRMaj))
                setMatrix(new FMatrixRMaj(numRows, numCols) as T);
            else if (typeof(T) == typeof(ZMatrixRMaj))
                setMatrix(new ZMatrixRMaj(numRows, numCols) as T);
            else if (typeof(T) == typeof(CMatrixRMaj))
                setMatrix(new CMatrixRMaj(numRows, numCols) as T);
            else if (typeof(T) == typeof(DMatrixSparseCSC))
                setMatrix(new DMatrixSparseCSC(numRows, numCols) as T);
            else if (typeof(T) == typeof(FMatrixSparseCSC))
                setMatrix(new FMatrixSparseCSC(numRows, numCols) as T);
            else
                throw new InvalidOperationException("Unknown matrix type!");
        }

        protected SimpleBase()
        {
        }

        /**
         * Used internally for creating new instances of SimpleMatrix.  If SimpleMatrix is extended
         * by another class this function should be overridden so that the returned matrices are
         * of the correct type.
         *
         * @param numRows number of rows in the new matrix.
         * @param numCols number of columns in the new matrix.
         * @param type Type of matrix it should create
         * @return A new matrix.
         */
        protected abstract SimpleBase<T> createMatrix(int numRows, int numCols, MatrixType type);

        /// <summary>
        /// We can't return SimpleMatrix here because we're implementing 
        /// an abstract method declared on the SimpleBase class.
        /// That allows other "wrappers" to adhere to the same API.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        protected virtual SimpleBase<T> wrapMatrix(T m)
        {
            return new SimpleMatrix<T>(m);
        }

        //protected abstract SimpleBase<T> wrapMatrix(T m);

        /**
         * <p>
         * Returns a reference to the matrix that it uses internally.  This is useful
         * when an operation is needed that is not provided by this class.
         * </p>
         *
         * @return Reference to the internal DMatrixRMaj.
         */
        public T getMatrix()
        {
            return mat;
        }

        #region Conversions return null if matrix type is wrong (kept for api conformity)

        public DMatrixRMaj getDDRM()
        {
            return mat as DMatrixRMaj;
        }

        public FMatrixRMaj getFDRM()
        {
            return mat as FMatrixRMaj;
        }

        public ZMatrixRMaj getZDRM()
        {
            return mat as ZMatrixRMaj;
        }

        public CMatrixRMaj getCDRM()
        {
            return mat as CMatrixRMaj;
        }

        public DMatrixSparseCSC getDSCC()
        {
            return mat as DMatrixSparseCSC;
        }

        public FMatrixSparseCSC getFSCC()
        {
            return mat as FMatrixSparseCSC;
        }

        #endregion // Conversions

        protected SimpleOperations<T> lookupOps(MatrixType type)
        {
            switch (type.CanonicalCode)
            {
                case (uint)MatrixType.Code.DDRM: return new SimpleOperations_DDRM() as SimpleOperations<T>;
                case (uint)MatrixType.Code.FDRM: return new SimpleOperations_FDRM() as SimpleOperations<T>;
                case (uint)MatrixType.Code.ZDRM: return new SimpleOperations_ZDRM() as SimpleOperations<T>;
                case (uint)MatrixType.Code.CDRM: return new SimpleOperations_CDRM() as SimpleOperations<T>;
                case (uint)MatrixType.Code.DSCC: return new SimpleOperations_SPARSE() as SimpleOperations<T>;
            }
            throw new InvalidOperationException("Unknown Matrix Type");
        }


        /**
         * <p>
         * Returns the transpose of this matrix.<br>
         * a<sup>T</sup>
         * </p>
         *
         * @see CommonOps_DDRM#transpose(DMatrixRMaj, DMatrixRMaj)
         *
         * @return A matrix that is n by m.
         */
        public SimpleBase<T> transpose()
        {
            SimpleBase<T> ret = createMatrix(mat.getNumCols(), mat.getNumRows(), mat.getType());

            ops.transpose((T) mat, (T) ret.mat);

            return ret;
        }

        /**
         * <p>
         * Returns a matrix which is the result of matrix multiplication:<br>
         * <br>
         * c = a * b <br>
         * <br>
         * where c is the returned matrix, a is this matrix, and b is the passed in matrix.
         * </p>
         *
         * @see CommonOps_DDRM#mult(DMatrix1Row, DMatrix1Row, DMatrix1Row)
         *
         * @param b A matrix that is n by bn. Not modified.
         *
         * @return The results of this operation.
         */
        public SimpleBase<T> mult(SimpleBase<T> b)
        {
            SimpleBase<T> ret = createMatrix(mat.getNumRows(), b.getMatrix().getNumCols(), mat.getType());

            ops.mult((T) mat, (T) b.mat, (T) ret.mat);

            return ret;
        }

        /**
         * <p>
         * Computes the Kronecker product between this matrix and the provided B matrix:<br>
         * <br>
         * C = kron(A,B)
         * </p>
    
         * @see CommonOps_DDRM#kron(DMatrixRMaj, DMatrixRMaj, DMatrixRMaj)
         *
         * @param B The right matrix in the operation. Not modified.
         * @return Kronecker product between this matrix and B.
         */
        public SimpleBase<T> kron(SimpleBase<T> B)
        {
            SimpleBase<T> ret = createMatrix(mat.getNumRows() * B.numRows(), mat.getNumCols() * B.numCols(), mat.getType());

            ops.kron((T) mat, (T) B.mat, (T) ret.mat);

            return ret;
        }

        /**
         * <p>
         * Returns the result of matrix addition:<br>
         * <br>
         * c = a + b <br>
         * <br>
         * where c is the returned matrix, a is this matrix, and b is the passed in matrix.
         * </p>
         *
         * @see CommonOps_DDRM#mult(DMatrix1Row, DMatrix1Row, DMatrix1Row)
         *
         * @param b m by n matrix. Not modified.
         *
         * @return The results of this operation.
         */
        public SimpleBase<T> plus(SimpleBase<T> b)
        {
            SimpleBase<T> ret = createMatrix(mat.getNumRows(), mat.getNumCols(), mat.getType());

            ops.plus((T) mat, (T) b.mat, (T) ret.mat);

            return ret;
        }

        /**
         * <p>
         * Returns the result of matrix subtraction:<br>
         * <br>
         * c = a - b <br>
         * <br>
         * where c is the returned matrix, a is this matrix, and b is the passed in matrix.
         * </p>
         *
         * @see CommonOps_DDRM#subtract(DMatrixD1, DMatrixD1, DMatrixD1)
         *
         * @param b m by n matrix. Not modified.
         *
         * @return The results of this operation.
         */
        public SimpleBase<T> minus(SimpleBase<T> b)
        {
            SimpleBase<T> ret = createMatrix(mat.getNumRows(), mat.getNumCols(), mat.getType());

            ops.minus((T) mat, (T) b.mat, (T) ret.mat);

            return ret;
        }

        /**
         * <p>
         * Returns the result of matrix-double subtraction:<br>
         * <br>
         * c = a - b <br>
         * <br>
         * where c is the returned matrix, a is this matrix, and b is the passed in double.
         * </p>
         *
         * @see CommonOps_DDRM#subtract(DMatrixD1, double , DMatrixD1)
         *
         * @param b Value subtracted from each element
         *
         * @return The results of this operation.
         */
        public SimpleBase<T> minus(double b)
        {
            SimpleBase<T> ret = copy();

            var m = getMatrix();
            var rm = ret.getMatrix();

            if (bits() == 64)
                CommonOps_DDRM.subtract(m as DMatrixRMaj, b, rm as DMatrixRMaj);
            else if (bits() == 32)
                CommonOps_FDRM.subtract(m as FMatrixRMaj, (float) b, rm as FMatrixRMaj);
            else
                throw new InvalidOperationException("Matrix type must be DMatrixRMaj or FMatrixRMaj.");

            return ret;
        }

        /**
         * <p>
         * Returns the result of scalar addition:<br>
         * <br>
         * c = a + b<br>
         * <br>
         * where c is the returned matrix, a is this matrix, and b is the passed in double.
         * </p>
         *
         * @see CommonOps_DDRM#add( DMatrixD1, double , DMatrixD1)
         *
         * @param b Value added to each element
         *
         * @return A matrix that contains the results.
         */
        public SimpleBase<T> plus(double b)
        {
            SimpleBase<T> ret = createMatrix(numRows(), numCols(), mat.getType());

            var m = getMatrix();
            var rm = ret.getMatrix();

            if (bits() == 64)
                CommonOps_DDRM.add(m as DMatrixRMaj, b, rm as DMatrixRMaj);
            else if (bits() == 32)
                CommonOps_FDRM.add(m as FMatrixRMaj, (float) b, rm as FMatrixRMaj);
            else
                throw new InvalidOperationException("Matrix type must be DMatrixRMaj or FMatrixRMaj.");

            return ret;
        }

        /**
         * <p>
         * Performs a matrix addition and scale operation.<br>
         * <br>
         * c = a + &beta;*b <br>
         * <br>
         * where c is the returned matrix, a is this matrix, and b is the passed in matrix.
         * </p>
         *
         * @see CommonOps_DDRM#add( DMatrixD1, double , DMatrixD1, DMatrixD1)
         *
         * @param b m by n matrix. Not modified.
         *
         * @return A matrix that contains the results.
         */
        public SimpleBase<T> plus(double beta, SimpleBase<T> b)
        {
            SimpleBase<T> ret = copy();

            var rm = ret.getMatrix();
            var bm = b.getMatrix();

            if (bits() == 64)
                CommonOps_DDRM.addEquals(rm as DMatrixRMaj, beta, bm as DMatrixRMaj);
            else if (bits() == 32)
                CommonOps_FDRM.addEquals(rm as FMatrixRMaj, (float) beta, bm as FMatrixRMaj);
            else
                throw new InvalidOperationException("Matrix type must be DMatrixRMaj or FMatrixRMaj.");

            return ret;
        }

        /**
         * Computes the dot product (a.k.a. inner product) between this vector and vector 'v'.
         *
         * @param v The second vector in the dot product.  Not modified.
         * @return dot product
         */
        public double dot(SimpleBase<T> v)
        {
            if (!isVector())
            {
                throw new ArgumentException("'this' matrix is not a vector.");
            }
            else if (!v.isVector())
            {
                throw new ArgumentException("'v' matrix is not a vector.");
            }

            var vm = v.getMatrix();

            if (bits() == 64)
                return VectorVectorMult_DDRM.innerProd(mat as DMatrixRMaj, vm as DMatrixRMaj);
            else if (bits() == 32)
                return VectorVectorMult_FDRM.innerProd(mat as FMatrixRMaj,  vm as FMatrixRMaj);
            else
                throw new InvalidOperationException("Matrix type must be DMatrixRMaj or FMatrixRMaj.");
        }

        /**
         * Returns true if this matrix is a vector.  A vector is defined as a matrix
         * that has either one row or column.
         *
         * @return Returns true for vectors and false otherwise.
         */
        public bool isVector()
        {
            return mat.getNumRows() == 1 || mat.getNumCols() == 1;
        }

        /**
         * <p>
         * Returns the result of scaling each element by 'val':<br>
         * b<sub>i,j</sub> = val*a<sub>i,j</sub>
         * </p>
         *
         * @see CommonOps_DDRM#scale(double, DMatrixD1)
         *
         * @param val The multiplication factor.
         * @return The scaled matrix.
         */
        public SimpleBase<T> scale(double val)
        {
            SimpleBase<T> ret = copy();

            var rm = ret.getMatrix();

            if (bits() == 64)
                CommonOps_DDRM.scale(val, rm as DMatrixRMaj);
            else if (bits() == 32)
                CommonOps_FDRM.scale((float) val, rm as FMatrixRMaj);
            else
                throw new InvalidOperationException("Matrix type must be DMatrixRMaj or FMatrixRMaj.");

            return ret;
        }

        /**
         * <p>
         * Returns the result of dividing each element by 'val':
         * b<sub>i,j</sub> = a<sub>i,j</sub>/val
         * </p>
         *
         * @see CommonOps_DDRM#divide(DMatrixD1,double)
         *
         * @param val Divisor.
         * @return Matrix with its elements divided by the specified value.
         */
        public SimpleBase<T> divide(double val)
        {
            SimpleBase<T> ret = copy();

            var rm = ret.getMatrix();

            if (bits() == 64)
                CommonOps_DDRM.divide(rm as DMatrixRMaj, val);
            else if (bits() == 32)
                CommonOps_FDRM.divide(rm as FMatrixRMaj, (float) val);
            else
                throw new InvalidOperationException("Matrix type must be DMatrixRMaj or FMatrixRMaj.");

            return ret;
        }

        /**
         * <p>
         * Returns the inverse of this matrix.<br>
         * <br>
         * b = a<sup>-1</sup><br>
         * </p>
         *
         * <p>
         * If the matrix could not be inverted then SingularMatrixException is thrown.  Even
         * if no exception is thrown the matrix could still be singular or nearly singular.
         * </p>
         *
         * @see CommonOps_DDRM#invert(DMatrixRMaj, DMatrixRMaj)
         *
         * @throws SingularMatrixException
         *
         * @return The inverse of this matrix.
         */
        public SimpleBase<T> invert()
        {
            SimpleBase<T> ret = createMatrix(mat.getNumRows(), mat.getNumCols(), mat.getType());

            if (!ops.invert((T) mat, (T) ret.mat))
                throw new SingularMatrixException();
            if (ops.hasUncountable((T) ret.mat))
                throw new SingularMatrixException("Solution contains uncountable numbers");

            return ret;
        }

        /**
         * <p>
         * Computes the Moore-Penrose pseudo-inverse
         * </p>
         *
         * @return inverse computed using the pseudo inverse.
         */
        public SimpleBase<T> pseudoInverse()
        {
            SimpleBase<T> ret = createMatrix(mat.getNumCols(), mat.getNumRows(), mat.getType());
            if (bits() == 64)
            {
                CommonOps_DDRM.pinv(mat as DMatrixRMaj, ret.getMatrix() as DMatrixRMaj);
            }
            else if(bits() == 32)
            {
                CommonOps_FDRM.pinv(mat as FMatrixRMaj, ret.getMatrix() as FMatrixRMaj);
            }
            else
                throw new InvalidOperationException("Matrix type must be DMatrixRMaj or FMatrixRMaj.");

            return ret;
        }

        /**
         * <p>
         * Solves for X in the following equation:<br>
         * <br>
         * x = a<sup>-1</sup>b<br>
         * <br>
         * where 'a' is this matrix and 'b' is an n by p matrix.
         * </p>
         *
         * <p>
         * If the system could not be solved then SingularMatrixException is thrown.  Even
         * if no exception is thrown 'a' could still be singular or nearly singular.
         * </p>
         *
         * @see CommonOps_DDRM#solve(DMatrixRMaj, DMatrixRMaj, DMatrixRMaj)
         *
         * @throws SingularMatrixException
         *
         * @param b n by p matrix. Not modified.
         * @return The solution for 'x' that is n by p.
         */
        public SimpleBase<T> solve(SimpleBase<T> b)
        {
            SimpleBase<T> x = createMatrix(mat.getNumCols(), b.getMatrix().getNumCols(), mat.getType());

            if (!ops.solve((T) mat, (T) x.mat, (T) b.mat))
                throw new SingularMatrixException();
            if (ops.hasUncountable((T) x.mat))
                throw new SingularMatrixException("Solution contains uncountable numbers");

            return x;
        }


        /**
         * Sets the elements in this matrix to be equal to the elements in the passed in matrix.
         * Both matrix must have the same dimension.
         *
         * @param a The matrix whose value this matrix is being set to.
         */
        public void set(SimpleBase<T> a)
        {
            mat.set(a.getMatrix());
        }


        /**
         * <p>
         * Sets all the elements in this matrix equal to the specified value.<br>
         * <br>
         * a<sub>ij</sub> = val<br>
         * </p>
         *
         * @see CommonOps_DDRM#fill(DMatrixD1, double)
         *
         * @param val The value each element is set to.
         */
        public void set(double val)
        {
            if (bits() == 64)
            {
                CommonOps_DDRM.fill(mat as DMatrixRMaj, val);
            }
            else
            {
                CommonOps_FDRM.fill(mat as FMatrixRMaj, (float) val);
            }
        }

        /**
         * Sets all the elements in the matrix equal to zero.
         *
         * @see CommonOps_DDRM#fill(DMatrixD1, double)
         */
        public void zero()
        {
            if (bits() == 64)
            {
                (mat as DMatrixRMaj).zero();
            }
            else
            {
                (mat as FMatrixRMaj).zero();
            }
        }

        /**
         * <p>
         * Computes the Frobenius normal of the matrix:<br>
         * <br>
         * normF = Sqrt{  &sum;<sub>i=1:m</sub> &sum;<sub>j=1:n</sub> { a<sub>ij</sub><sup>2</sup>}   }
         * </p>
         *
         * @see NormOps_DDRM#normF(DMatrixD1)
         *
         * @return The matrix's Frobenius normal.
         */
        public double normF()
        {
            return ops.normF(mat);
        }

        /**
         * <p>
         * The condition p = 2 number of a matrix is used to measure the sensitivity of the linear
         * system <b>Ax=b</b>.  A value near one indicates that it is a well conditioned matrix.
         * </p>
         *
         * @see NormOps_DDRM#conditionP2(DMatrixRMaj)
         *
         * @return The condition number.
         */
        public double conditionP2()
        {
            return ops.conditionP2(mat);
        }

        /**
         * Computes the determinant of the matrix.
         *
         * @see CommonOps_DDRM#det(DMatrixRMaj)
         *
         * @return The determinant.
         */
        public double determinant()
        {
            double ret = ops.determinant(mat);
            if (UtilEjml.isUncountable(ret))
                return 0;
            return ret;
        }

        /**
         * <p>
         * Computes the trace of the matrix.
         * </p>
         *
         * @see CommonOps_DDRM#trace(DMatrix1Row)
         *
         * @return The trace of the matrix.
         */
        public double trace()
        {
            return ops.trace(mat);
        }

        /**
         * <p>
         * Reshapes the matrix to the specified number of rows and columns.  If the total number of elements
         * is &le; number of elements it had before the data is saved.  Otherwise a new internal array is
         * declared and the old data lost.
         * </p>
         *
         * <p>
         * This is equivalent to calling A.getMatrix().reshape(numRows,numCols,false).
         * </p>
         *
         * @see DMatrixRMaj#reshape(int,int,bool)
         *
         * @param numRows The new number of rows in the matrix.
         * @param numCols The new number of columns in the matrix.
         */
        public void reshape(int numRows, int numCols)
        {
            if (mat.getType().isFixed())
            {
                throw new ArgumentException("Can't rename a fixed sized matrix");
            }
            else
            {
                ((ReshapeMatrix) mat).reshape(numRows, numCols);
            }
        }

        /**
         * Assigns the element in the Matrix to the specified value.  Performs a bounds check to make sure
         * the requested element is part of the matrix.
         *
         * @param row The row of the element.
         * @param col The column of the element.
         * @param value The element's new value.
         */
        public void set(int row, int col, double value)
        {
            if (bits() == 64)
            {
                (mat as DMatrixRMaj).set(row, col, value);
            }
            else
            {
                (mat as FMatrixRMaj).set(row, col, (float) value);
            }
        }

        /**
         * Assigns an element a value based on its index in the internal array..
         *
         * @param index The matrix element that is being assigned a value.
         * @param value The element's new value.
         */
        public void set(int index, double value)
        {
            if (bits() == 64)
            {
                (mat as DMatrixRMaj).set(index, value);
            }
            else
            {
                (mat as FMatrixRMaj).set(index, (float) value);
            }
        }

        /**
         * <p>
         * Assigns consecutive elements inside a row to the provided array.<br>
         * <br>
         * A(row,offset:(offset + values.Length)) = values
         * </p>
         *
         * @param row The row that the array is to be written to.
         * @param startColumn The initial column that the array is written to.
         * @param values Values which are to be written to the row in a matrix.
         */
        public void setRow(int row, int startColumn, double[] values)
        {
            ops.setRow(mat, row, startColumn, values);
        }

        /**
         * <p>
         * Assigns consecutive elements inside a column to the provided array.<br>
         * <br>
         * A(offset:(offset + values.Length),column) = values
         * </p>
         *
         * @param column The column that the array is to be written to.
         * @param startRow The initial column that the array is written to.
         * @param values Values which are to be written to the row in a matrix.
         */
        public void setColumn(int column, int startRow, double[] values)
        {
            ops.setColumn(mat, column, startRow, values);
        }

        /**
         * Returns the value of the specified matrix element.  Performs a bounds check to make sure
         * the requested element is part of the matrix.
         *
         * @param row The row of the element.
         * @param col The column of the element.
         * @return The value of the element.
         */
        public double get(int row, int col)
        {
            if (mat.getType().getBits() == 64)
            {
                return ((DMatrix) mat).get(row, col);
            }
            else
            {
                return ((FMatrix) mat).get(row, col);
            }
        }

        /**
         * Returns the value of the matrix at the specified index of the 1D row major array.
         *
         * @see DMatrixRMaj#get(int)
         *
         * @param index The element's index whose value is to be returned
         * @return The value of the specified element.
         */
        public double get(int index)
        {
            if (bits() == 64)
            {
                return (mat as DMatrixRMaj).data[index];
            }
            else
            {
                return (mat as FMatrixRMaj).data[index];
            }
        }

        /**
         * Returns the index in the matrix's array.
         *
         * @see DMatrixRMaj#getIndex(int, int)
         *
         * @param row The row number.
         * @param col The column number.
         * @return The index of the specified element.
         */
        public int getIndex(int row, int col)
        {
            return row * mat.getNumCols() + col;
        }

        /**
         * Creates a new iterator for traversing through a submatrix inside this matrix.  It can be traversed
         * by row or by column.  Range of elements is inclusive, e.g. minRow = 0 and maxRow = 1 will include rows
         * 0 and 1.  The iteration starts at (minRow,minCol) and ends at (maxRow,maxCol)
         *
         * @param rowMajor true means it will traverse through the submatrix by row first, false by columns.
         * @param minRow first row it will start at.
         * @param minCol first column it will start at.
         * @param maxRow last row it will stop at.
         * @param maxCol last column it will stop at.
         * @return A new MatrixIterator
         */
        public DMatrixIterator iterator(bool rowMajor, int minRow, int minCol, int maxRow, int maxCol)
        {
            return new DMatrixIterator(mat as DMatrixRMaj, rowMajor, minRow, minCol, maxRow, maxCol);
        }

        /**
         * Creates and returns a matrix which is idential to this one.
         *
         * @return A new identical matrix.
         */
        public SimpleBase<T> copy()
        {
            SimpleBase<T> ret = createMatrix(mat.getNumRows(), mat.getNumCols(), mat.getType());
            ret.getMatrix().set(getMatrix());
            return ret;
        }

        /**
         * Returns the number of rows in this matrix.
         *
         * @return number of rows.
         */
        public int numRows()
        {
            return mat.getNumRows();
        }

        /**
         * Returns the number of columns in this matrix.
         *
         * @return number of columns.
         */
        public int numCols()
        {
            return mat.getNumCols();
        }

        /**
         * Returns the number of elements in this matrix, which is equal to
         * the number of rows times the number of columns.
         *
         * @return The number of elements in the matrix.
         */
        public int getNumElements()
        {
            if (bits() == 64)
                return (mat as DMatrixRMaj).getNumElements();
            else
                return (mat as FMatrixRMaj).getNumElements();
        }


        /**
         * Prints the matrix to standard out.
         */
        public void print()
        {
            ops.print(Console.OpenStandardOutput(), mat);
        }

        /**
         * Prints the matrix to standard out with the specified precision.
         */
        public void print(int numChar, int precision)
        {
            if (bits() == 64)
            {
                MatrixIO.print(Console.OpenStandardOutput(), mat as DMatrixRMaj, numChar, precision);
            }
            else
            {
                MatrixIO.print(Console.OpenStandardOutput(), mat as FMatrixRMaj, numChar, precision);
            }
        }

        /**
         * <p>
         * Prints the matrix to standard out given a {@link java.io.PrintStream#printf} style floating point format,
         * e.g. print("%f").
         * </p>
         */
        public void print(string format)
        {
            if (bits() == 64)
            {
                MatrixIO.print(Console.OpenStandardOutput(), mat as DMatrixRMaj, format);
            }
            else
            {
                MatrixIO.print(Console.OpenStandardOutput(), mat as FMatrixRMaj, format);
            }
        }

        /**
         * <p>
         * Converts the array into a string format for display purposes.
         * The conversion is done using {@link MatrixIO#print(java.io.PrintStream, DMatrix)}.
         * </p>
         *
         * @return String representation of the matrix.
         */
        public string toString()
        {
            using (var stream = new MemoryStream())
            {
                if (bits() == 64)
                {
                    MatrixIO.print(stream, mat as DMatrixRMaj);
                }
                else
                {
                    MatrixIO.print(stream, mat as FMatrixRMaj);
                }
                stream.Position = 0;
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        /**
         * <p>
         * Creates a new SimpleMatrix which is a submatrix of this matrix.
         * </p>
         * <p>
         * s<sub>i-y0 , j-x0</sub> = o<sub>ij</sub> for all y0 &le; i &lt; y1 and x0 &le; j &lt; x1<br>
         * <br>
         * where 's<sub>ij</sub>' is an element in the submatrix and 'o<sub>ij</sub>' is an element in the
         * original matrix.
         * </p>
         *
         * <p>
         * If any of the inputs are set to SimpleMatrix.END then it will be set to the last row
         * or column in the matrix.
         * </p>
         *
         * @param y0 Start row.
         * @param y1 Stop row + 1.
         * @param x0 Start column.
         * @param x1 Stop column + 1.
         * @return The submatrix.
         */
        public SimpleBase<T> extractMatrix(int y0, int y1, int x0, int x1)
        {
            if (y0 == END) y0 = mat.getNumRows();
            if (y1 == END) y1 = mat.getNumRows();
            if (x0 == END) x0 = mat.getNumCols();
            if (x1 == END) x1 = mat.getNumCols();

            SimpleBase<T> ret = createMatrix(y1 - y0, x1 - x0, mat.getType());

            ops.extract(mat, y0, y1, x0, x1, ret.mat, 0, 0);

            return ret;
        }

        /**
         * <p>
         * Extracts a row or column from this matrix. The returned vector will either be a row
         * or column vector depending on the input type.
         * </p>
         *
         * @param extractRow If true a row will be extracted.
         * @param element The row or column the vector is contained in.
         * @return Extracted vector.
         */
        public SimpleBase<T> extractVector(bool extractRow, int element)
        {
            int length = extractRow ? mat.getNumCols() : mat.getNumRows();

            SimpleBase<T> ret = extractRow ? createMatrix(1, length, mat.getType()) : createMatrix(length, 1, mat.getType());

            if (bits() == 64)
            {
                var rm = ret.getMatrix() as DMatrixRMaj;
                if (extractRow)
                {
                    SpecializedOps_DDRM
                        .subvector(mat as DMatrixRMaj, element, 0, length, true, 0, rm);
                }
                else
                {
                    SpecializedOps_DDRM
                        .subvector(mat as DMatrixRMaj, 0, element, length, false, 0, rm);
                }
            }
            else
            {
                var rm = ret.getMatrix() as FMatrixRMaj;
                if (extractRow)
                {
                    SpecializedOps_FDRM
                        .subvector(mat as FMatrixRMaj, element, 0, length, true, 0, rm);
                }
                else
                {
                    SpecializedOps_FDRM
                        .subvector(mat as FMatrixRMaj, 0, element, length, false, 0, rm);
                }
            }

            return ret;
        }

        /**
         * <p>
         * If a vector then a square matrix is returned if a matrix then a vector of diagonal ements is returned
         * </p>
         *
         * @see CommonOps_DDRM#extractDiag(DMatrixRMaj, DMatrixRMaj)
         * @return Diagonal elements inside a vector or a square matrix with the same diagonal elements.
         */
        public SimpleBase<T> diag()
        {
            SimpleBase<T> diag;
            if (bits() == 64)
            {
                if (MatrixFeatures_DDRM.isVector(mat))
                {
                    int N = Math.Max(mat.getNumCols(), mat.getNumRows());
                    diag = createMatrix(N, N, mat.getType());
                    var dm = diag.getMatrix() as DMatrixRMaj;
                    CommonOps_DDRM.diag(dm, N, (mat as DMatrixRMaj).data);
                }
                else
                {
                    int N = Math.Min(mat.getNumCols(), mat.getNumRows());
                    diag = createMatrix(N, 1, mat.getType());
                    var dm = diag.getMatrix() as DMatrixRMaj;
                    CommonOps_DDRM.extractDiag(mat as DMatrixRMaj, dm);
                }
            }
            else
            {
                if (MatrixFeatures_FDRM.isVector(mat))
                {
                    int N = Math.Max(mat.getNumCols(), mat.getNumRows());
                    diag = createMatrix(N, N, mat.getType());
                    var dm = diag.getMatrix() as FMatrixRMaj;
                    CommonOps_FDRM.diag(dm, N, (mat as FMatrixRMaj).data);
                }
                else
                {
                    int N = Math.Min(mat.getNumCols(), mat.getNumRows());
                    diag = createMatrix(N, 1, mat.getType());
                    var dm = diag.getMatrix() as FMatrixRMaj;
                    CommonOps_FDRM.extractDiag(mat as FMatrixRMaj, dm);
                }
            }

            return diag;
        }

        /**
         * Checks to see if matrix 'a' is the same as this matrix within the specified
         * tolerance.
         *
         * @param a The matrix it is being compared against.
         * @param tol How similar they must be to be equals.
         * @return If they are equal within tolerance of each other.
         */
        public bool isIdentical(SimpleBase<T> a, double tol)
        {
            var am = a.getMatrix();
            if (bits() == 64)
            {
                return MatrixFeatures_DDRM.isIdentical(mat as DMatrixRMaj, am as DMatrixRMaj, tol);
            }
            else
            {
                return MatrixFeatures_FDRM.isIdentical(mat as FMatrixRMaj, am as FMatrixRMaj, (float) tol);
            }
        }

        /**
         * Checks to see if any of the elements in this matrix are either NaN or infinite.
         *
         * @return True of an element is NaN or infinite.  False otherwise.
         */
        public bool hasUncountable()
        {
            return ops.hasUncountable(mat);
        }

        /**
         * Computes a full Singular Value Decomposition (SVD) of this matrix with the
         * eigenvalues ordered from largest to smallest.
         *
         * @return SVD
         */
        public SimpleSVD<T> svd()
        {
            return new SimpleSVD<T>(mat, false);
        }

        /**
         * Computes the SVD in either  compact format or full format.
         *
         * @return SVD of this matrix.
         */
        public SimpleSVD<T> svd(bool compact)
        {
            return new SimpleSVD<T>(mat, compact);
        }

        /**
         * Returns the Eigen Value Decomposition (EVD) of this matrix.
         */
        public SimpleEVD<T> eig()
        {
            return new SimpleEVD<T>(mat);
        }

        /**
         * Copy matrix B into this matrix at location (insertRow, insertCol).
         *
         * @param insertRow First row the matrix is to be inserted into.
         * @param insertCol First column the matrix is to be inserted into.
         * @param B The matrix that is being inserted.
         */
        public void insertIntoThis(int insertRow, int insertCol, SimpleBase<T> B)
        {
            var bm = B.getMatrix();
            if (bits() == 64)
            {
                CommonOps_DDRM.insert(bm as DMatrixRMaj, mat as DMatrixRMaj, insertRow, insertCol);
            }
            else
            {
                CommonOps_FDRM.insert(bm as FMatrixRMaj, mat as FMatrixRMaj, insertRow, insertCol);
            }
        }

        /**
         * <p>
         * Creates a new matrix that is a combination of this matrix and matrix B.  B is
         * written into A at the specified location if needed the size of A is increased by
         * growing it.  A is grown by padding the new area with zeros.
         * </p>
         *
         * <p>
         * While useful when adding data to a matrix which will be solved for it is also much
         * less efficient than predeclaring a matrix and inserting data into it.
         * </p>
         *
         * <p>
         * If insertRow or insertCol is set to SimpleMatrix.END then it will be combined
         * at the last row or column respectively.
         * <p>
         *
         * @param insertRow Row where matrix B is written in to.
         * @param insertCol Column where matrix B is written in to.
         * @param B The matrix that is written into A.
         * @return A new combined matrix.
         */
        public SimpleBase<T> combine(int insertRow, int insertCol, SimpleBase<T> B)
        {

            if (insertRow == END)
            {
                insertRow = mat.getNumRows();
            }

            if (insertCol == END)
            {
                insertCol = mat.getNumCols();
            }

            int maxRow = insertRow + B.numRows();
            int maxCol = insertCol + B.numCols();

            SimpleBase<T> ret;

            if (maxRow > mat.getNumRows() || maxCol > mat.getNumCols())
            {
                int M = Math.Max(maxRow, mat.getNumRows());
                int N = Math.Max(maxCol, mat.getNumCols());

                ret = createMatrix(M, N, mat.getType());
                ret.insertIntoThis(0, 0, this);
            }
            else
            {
                ret = copy();
            }

            ret.insertIntoThis(insertRow, insertCol, B);

            return ret;
        }

        /**
         * Returns the maximum absolute value of all the elements in this matrix.  This is
         * equivalent the the infinite p-norm of the matrix.
         *
         * @return Largest absolute value of any element.
         */
        public double elementMaxAbs()
        {
            return ops.elementMaxAbs((T) mat);
        }

        /**
         * Computes the sum of all the elements in the matrix.
         *
         * @return Sum of all the elements.
         */
        public double elementSum()
        {
            return ops.elementSum((T) mat);
        }

        /**
         * <p>
         * Returns a matrix which is the result of an element by element multiplication of 'this' and 'b':
         * c<sub>i,j</sub> = a<sub>i,j</sub>*b<sub>i,j</sub>
         * </p>
         *
         * @param b A simple matrix.
         * @return The element by element multiplication of 'this' and 'b'.
         */
        public SimpleBase<T> elementMult(SimpleBase<T> b)
        {
            SimpleBase<T> c = createMatrix(mat.getNumRows(), mat.getNumCols(), mat.getType());

            ops.elementMult(mat, b.mat, c.mat);

            return c;
        }

        /**
         * <p>
         * Returns a matrix which is the result of an element by element division of 'this' and 'b':
         * c<sub>i,j</sub> = a<sub>i,j</sub>/b<sub>i,j</sub>
         * </p>
         *
         * @param b A simple matrix.
         * @return The element by element division of 'this' and 'b'.
         */
        public SimpleBase<T> elementDiv(SimpleBase<T> b)
        {
            SimpleBase<T> c = createMatrix(mat.getNumRows(), mat.getNumCols(), mat.getType());

            ops.elementDiv(mat, b.mat, c.mat);
            return c;
        }

        /**
         * <p>
         * Returns a matrix which is the result of an element by element power of 'this' and 'b':
         * c<sub>i,j</sub> = a<sub>i,j</sub> ^ b<sub>i,j</sub>
         * </p>
         *
         * @param b A simple matrix.
         * @return The element by element power of 'this' and 'b'.
         */
        public SimpleBase<T> elementPower(SimpleBase<T> b)
        {
            SimpleBase<T> c = createMatrix(mat.getNumRows(), mat.getNumCols(), mat.getType());

            ops.elementPower(mat, b.mat, c.mat);

            return c;
        }

        /**
         * <p>
         * Returns a matrix which is the result of an element by element power of 'this' and 'b':
         * c<sub>i,j</sub> = a<sub>i,j</sub> ^ b
         * </p>
         *
         * @param b Scalar
         * @return The element by element power of 'this' and 'b'.
         */
        public SimpleBase<T> elementPower(double b)
        {
            SimpleBase<T> c = createMatrix(mat.getNumRows(), mat.getNumCols(), mat.getType());

            ops.elementPower(mat, b, c.mat);
            return c;
        }

        /**
         * <p>
         * Returns a matrix which is the result of an element by element exp of 'this'
         * c<sub>i,j</sub> = Math.exp(a<sub>i,j</sub>)
         * </p>
         *
         * @return The element by element power of 'this' and 'b'.
         */
        public SimpleBase<T> elementExp()
        {
            SimpleBase<T> c = createMatrix(mat.getNumRows(), mat.getNumCols(), mat.getType());

            ops.elementExp(mat, c.mat);

            return c;
        }

        /**
         * <p>
         * Returns a matrix which is the result of an element by element exp of 'this'
         * c<sub>i,j</sub> = Math.log(a<sub>i,j</sub>)
         * </p>
         *
         * @return The element by element power of 'this' and 'b'.
         */
        public SimpleBase<T> elementLog()
        {
            SimpleBase<T> c = createMatrix(mat.getNumRows(), mat.getNumCols(), mat.getType());

            ops.elementLog(mat, c.mat);

            return c;
        }

        /**
         * <p>
         * Returns a new matrix whose elements are the negative of 'this' matrix's elements.<br>
         * <br>
         * b<sub>ij</sub> = -a<sub>ij</sub>
         * </p>
         *
         * @return A matrix that is the negative of the original.
         */
        public SimpleBase<T> negative()
        {
            SimpleBase<T> A = copy();
            ops.changeSign(A.mat);
            return A;
        }

        /**
         * <p>Allows you to perform an equation in-place on this matrix by specifying the right hand side.  For information on how to define an equation
         * see {@link org.ejml.equation.Equation}.  The variable sequence alternates between variable and it's label String.
         * This matrix is by default labeled as 'A', but is a string is the first object in 'variables' then it will take
         * on that value.  The variable passed in can be any data type supported by Equation can be passed in.
         * This includes matrices and scalars.</p>
         *
         * Examples:<br>
         * <pre>
         * perform("A = A + B",matrix,"B");     // Matrix addition
         * perform("A + B",matrix,"B");         // Matrix addition with implicit 'A = '
         * perform("A(5,:) = B",matrix,"B");    // Insert a row defined by B into A
         * perform("[A;A]");                    // stack A twice with implicit 'A = '
         * perform("Q = B + 2","Q",matrix,"B"); // Specify the name of 'this' as Q
         *
         * </pre>
         *
         * @param equation String representing the symbol equation
         * @param variables List of variable names and variables
         */
        public void equation(string equation, params object[] variables)
        {
            if (variables.Length >= 25)
                throw new ArgumentException("Too many variables!  At most 25");

            if (!(mat is DMatrixRMaj))
                return;

            Equation.Equation eq = new Equation.Equation();

            string nameThis = "A";
            int offset = 0;
            if (variables.Length > 0 && variables[0] is string)
            {
                nameThis = (string) variables[0];
                offset = 1;

                if (variables.Length % 2 != 1)
                    throw new ArgumentException("Expected and odd length for variables");
            }
            else
            {
                if (variables.Length % 2 != 0)
                    throw new ArgumentException("Expected and even length for variables");
            }


            eq.alias(mat as DMatrixRMaj, nameThis);

            for (int i = offset; i < variables.Length; i += 2)
            {
                if (!(variables[i + 1] is string))
                    throw new ArgumentException("String expected at variables index " + i);
                object o = variables[i];
                string name = (string) variables[i + 1];

                //if (typeof(SimpleBase).IsAssignableFrom(o.GetType()))
                if (o is SimpleBase<DMatrixRMaj>)
                {
                    // we already know that we've got a matrix type of DMatrixRMaj
                    // so operations involving the same type of matrix should be okay (right?).
                    eq.alias(((SimpleBase<DMatrixRMaj>) o).getDDRM(), name);
                }
                else if (o is DMatrixRMaj)
                {
                    eq.alias((DMatrixRMaj) o, name);
                }
                else if (o is double)
                {
                    eq.alias((double) o, name);
                }
                else if (o is int)
                {
                    eq.alias((int) o, name);
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

        /**
         * <p>
         * Saves this matrix to a file as a serialized binary object.
         * </p>
         *
         * @see MatrixIO#saveBin( DMatrix, String)
         *
         * @param fileName
         * @throws java.io.IOException
         */
        public void saveToFileBinary(string fileName)
        {
            MatrixIO.saveBin(mat as DMatrixRMaj, fileName);
        }

        /**
         * <p>
         * Loads a new matrix from a serialized binary file.
         * </p>
         *
         * @see MatrixIO#loadBin(String)
         *
         * @param fileName File which is to be loaded.
         * @return The matrix.
         * @throws IOException
         */
        public static SimpleMatrix<T> loadBinary(string fileName)
        {
            DMatrix mat = MatrixIO.loadBin<DMatrix>(fileName);

            // see if its a DMatrixRMaj
            if (mat is DMatrixRMaj)
            {
                return SimpleMatrix<T>.wrap(mat as T);
            }
            else
            {
                // if not convert it into one and wrap it
                return SimpleMatrix<T>.wrap(new DMatrixRMaj(mat) as T);
            }
        }

        /**
         * <p>
         * Saves this matrix to a file in a CSV format.  For the file format see {@link MatrixIO}.
         * </p>
         *
         * @see MatrixIO#saveBin( DMatrix, String)
         *
         * @param fileName
         * @throws java.io.IOException
         */
        public void saveToFileCSV(string fileName)
        {
            MatrixIO.saveCSV(mat as DMatrixRMaj, fileName);
        }

        /**
         * <p>
         * Loads a new matrix from a CSV file.  For the file format see {@link MatrixIO}.
         * </p>
         *
         * @see MatrixIO#loadCSV(String)
         *
         * @param fileName File which is to be loaded.
         * @return The matrix.
         * @throws IOException
         */
        public SimpleBase<T> loadCSV(string fileName)
        {
            DMatrix mat = MatrixIO.loadCSV(fileName);

            SimpleBase<T> ret = createMatrix(1, 1, mat.getType());

            ret.setMatrix((T) mat);

            return ret;
        }

        /**
         * Returns true of the specified matrix element is valid element inside this matrix.
         * 
         * @param row Row index.
         * @param col Column index.
         * @return true if it is a valid element in the matrix.
         */
        public bool isInBounds(int row, int col)
        {
            return row >= 0 && col >= 0 && row < mat.getNumRows() && col < mat.getNumCols();
        }

        /**
         * Prints the number of rows and column in this matrix.
         */
        public void printDimensions()
        {
            Console.WriteLine("[rows = " + numRows() + " , cols = " + numCols() + " ]");
        }

        /**
         * Size of internal array elements.  32 or 64 bits
         */
        public int bits()
        {
            return mat.getType().getBits();
        }

        /**
         * <p>Concatinates all the matrices together along their columns.  If the rows do not match the upper elements
         * are set to zero.</p>
         *
         * A = [ m[0] , ... , m[n-1] ]
         *
         * @param A Set of matrices
         * @return Resulting matrix
         */
        public SimpleBase<T> concatColumns(params SimpleBase<T>[] A)
        {
            T combined;
            if (mat.GetType() == typeof(DMatrixRMaj))
            {
                DMatrixRMaj[] m = new DMatrixRMaj[A.Length + 1];
                m[0] = mat as DMatrixRMaj;
                for (int i = 0; i < A.Length; i++)
                {
                    m[1] = A[i].getDDRM();
                }
                combined = CommonOps_DDRM.concatColumns(m) as T;
            }
            else if (mat.GetType() == typeof(FMatrixRMaj))
            {
                FMatrixRMaj[] m = new FMatrixRMaj[A.Length + 1];
                m[0] = mat as FMatrixRMaj;
                for (int i = 0; i < A.Length; i++)
                {
                    m[1] = A[i].getFDRM();
                }
                combined = CommonOps_FDRM.concatColumns(m) as T;
            }
            else
            {
                throw new InvalidOperationException("Unknown matrix type");
            }
            return wrapMatrix(combined);
        }

        /**
         * <p>Concatinates all the matrices together along their columns.  If the rows do not match the upper elements
         * are set to zero.</p>
         *
         * A = [ m[0] ; ... ; m[n-1] ]
         *
         * @param A Set of matrices
         * @return Resulting matrix
         */
        public SimpleBase<T> concatRows(params SimpleBase<T>[] A)
        {
            T combined;
            if (mat.GetType() == typeof(DMatrixRMaj))
            {
                DMatrixRMaj[] m = new DMatrixRMaj[A.Length + 1];
                m[0] = mat as DMatrixRMaj;
                for (int i = 0; i < A.Length; i++)
                {
                    m[1] = A[i].getDDRM();
                }
                combined = CommonOps_DDRM.concatRows(m) as T;
            }
            else if (mat.GetType() == typeof(FMatrixRMaj))
            {
                FMatrixRMaj[] m = new FMatrixRMaj[A.Length + 1];
                m[0] = mat as FMatrixRMaj;
                for (int i = 0; i < A.Length; i++)
                {
                    m[1] = A[i].getFDRM();
                }
                combined = CommonOps_FDRM.concatRows(m) as T;
            }
            else
            {
                throw new InvalidOperationException("Unknown matrix type");
            }
            return wrapMatrix(combined);
        }

        /**
         * Extracts the specified rows from the matrix.
         * @param begin First row.  Inclusive.
         * @param end Last row + 1.
         * @return Submatrix that contains the specified rows.
         */
        public SimpleBase<T> rows(int begin, int end)
        {
            return extractMatrix(begin, end, 0, END);
        }

        /**
         * Extracts the specified rows from the matrix.
         * @param begin First row.  Inclusive.
         * @param end Last row + 1.
         * @return Submatrix that contains the specified rows.
         */
        public SimpleBase<T> cols(int begin, int end)
        {
            return extractMatrix(0, END, begin, end);
        }

        /**
         * Returns the type of matrix is is wrapping.
         */
        public MatrixType getType()
        {
            return mat.getType();
        }

        protected void setMatrix(T mat)
        {
            this.mat = mat;
            this.ops = lookupOps(mat.getType());
        }
    }
}