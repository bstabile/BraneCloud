using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SharpMatrix.Data;
using SharpMatrix.Dense;
using SharpMatrix.Dense.Row;
using SharpMatrix.Ops;
using SharpMatrix.Simple.Ops;
using Randomization;

namespace SharpMatrix.Simple
{
    //package org.ejml.simple;

/**
 * <p>
 * {@link SimpleMatrix} is a wrapper around {@link DMatrixRMaj} that provides an
 * easy to use object oriented interface for performing matrix operations.  It is designed to be
 * more accessible to novice programmers and provide a way to rapidly code up solutions by simplifying
 * memory management and providing easy to use functions.
 * </p>
 *
 * <p>
 * Most functions in SimpleMatrix do not modify the original matrix.  Instead they
 * create a new SimpleMatrix instance which is modified and returned.  This greatly simplifies memory
 * management and writing of code in general. It also allows operations to be chained, as is shown
 * below:<br>
 * <br>
 * SimpleMatrix K = P.mult(H.transpose().mult(S.invert()));
 * </p>
 *
 * <p>
 * Working with both {@link DMatrixRMaj} and SimpleMatrix in the same code base is easy.
 * To access the internal DMatrixRMaj in a SimpleMatrix simply call {@link SimpleMatrix#getMatrix()}.
 * To turn a DMatrixRMaj into a SimpleMatrix use {@link SimpleMatrix#wrap(org.ejml.data.Matrix)}.  Not
 * all operations in EJML are provided for SimpleMatrix, but can be accessed by extracting the internal
 * DMatrixRMaj.
 * </p>
 *
 * <p>
 * EXTENDING: SimpleMatrix contains a list of narrowly focused functions for linear algebra.  To harness
 * the functionality for another application and to the number of functions it supports it is recommended
 * that one extends {@link SimpleBase} instead.  This way the returned matrix type's of SimpleMatrix functions
 * will be of the appropriate types.  See StatisticsMatrix inside of the examples directory.
 * </p>
 *
 * <p>
 * PERFORMANCE: The disadvantage of using this class is that it is more resource intensive, since
 * it creates a new matrix each time an operation is performed.  This makes the JavaVM work harder and
 * Java automatically initializes the matrix to be all zeros.  Typically operations on small matrices
 * or operations that have a runtime linear with the number of elements are the most affected.  More
 * computationally intensive operations have only a slight unnoticeable performance loss.  MOST PEOPLE
 * SHOULD NOT WORRY ABOUT THE SLIGHT LOSS IN PERFORMANCE.
 * </p>
 *
 * <p>
 * It is hard to judge how significant the performance hit will be in general.  Often the performance
 * hit is insignificant since other parts of the application are more processor intensive or the bottle
 * neck is a more computationally complex operation.  The best approach is benchmark and then optimize the code.
 * </p>
 *
 * <p>
 * If SimpleMatrix is extended then the protected function {link #createMatrix} should be extended and return
 * the child class.  The results of SimpleMatrix operations will then be of the correct matrix type. 
 * </p>
 *
 * <p>
 * The object oriented approach used in SimpleMatrix was originally inspired by Jama.
 * http://math.nist.gov/javanumerics/jama/
 * </p>
 *
 * @author Peter Abeles
 */

    public class SimpleMatrixD : SimpleMatrixBase<double, DMatrixRMaj, SimpleMatrixD>,
        ISimpleMatrix<double, DMatrixRMaj, SimpleMatrixD>
    {
        const long serialVersionUID = 2342556642L;
        /**
         * A simplified way to reference the last row or column in the matrix for some functions.
         */
        public const int END = int.MaxValue;

        #region Construction

        /**
         * Constructor for internal library use only.  Nothing is configured and is intended for serialization.
         */
        public SimpleMatrixD()
        {
        }

        /**
         * Creates a new matrix that is initially set to zero with the specified dimensions.
         */
        public SimpleMatrixD(int numRows, int numCols)
        {
            setMatrix(new DMatrixRMaj(numRows, numCols));
        }

        /**
         * <p>
         * Creates a new matrix which has the same value as the matrix encoded in the
         * provided array.  The input matrix's format can either be row-major or
         * column-major.
         * </p>
         *
         * <p>
         * Note that 'data' is a variable argument type, so either 1D arrays or a set of numbers can be
         * passed in:<br>
         * SimpleMatrix a = new SimpleMatrix(2,2,true,new double[]{1,2,3,4});<br>
         * SimpleMatrix b = new SimpleMatrix(2,2,true,1,2,3,4);<br>
         * <br>
         * Both are equivalent.
         * </p>
         *
         * @see DMatrixRMaj#DMatrixRMaj(int, int, bool, double...)
         *
         * @param numRows The number of rows.
         * @param numCols The number of columns.
         * @param rowMajor If the array is encoded in a row-major or a column-major format.
         * @param data The formatted 1D array. Not modified.
         */
        public SimpleMatrixD(int numRows, int numCols, bool rowMajor, double[] data)
        {
            setMatrix(new DMatrixRMaj(numRows, numCols, rowMajor, data));
        }

        /**
         * <p>
         * Creates a matrix with the values and shape defined by the 2D array 'data'.
         * It is assumed that 'data' has a row-major formatting:<br>
         * <br>
         * data[ row ][ column ]
         * </p>
         *
         * @see DMatrixRMaj#DMatrixRMaj(double[][])
         *`
         * @param data 2D array representation of the matrix. Not modified.
         */
        public SimpleMatrixD(double[][] data)
        {
            setMatrix(new DMatrixRMaj(data));
        }

        /**
         * Creats a new SimpleMatrix which is identical to the original.
         *
         * @param orig The matrix which is to be copied. Not modified.
         */
        public SimpleMatrixD(SimpleMatrixD orig)
        {
            setMatrix(orig.mat.copy() as DMatrixRMaj);
        }

        /**
         * Creates a new SimpleMatrix which is a copy of the Matrix.
         *
         * @param orig The original matrix whose value is copied.  Not modified.
         */
        public SimpleMatrixD(DMatrixRBlock orig)
        {
            if (orig == null)
                throw new ArgumentNullException();

            DMatrixRMaj a = new DMatrixRMaj(orig.getNumRows(), orig.getNumCols());
            ConvertDMatrixStruct.convert((DMatrixRBlock) orig, a);
            setMatrix(a);
        }

        /**
         * Creates a new SimpleMatrixD that uses the supplied DMatrixRMaj internally (will modify).
         */
        public SimpleMatrixD(DMatrixRMaj matrix)
        {
            setMatrix(matrix);
        }

        #endregion // Construction


        #region Static Methods

        /**
         * Creates a new identity matrix with the specified size.
         *
         * @see CommonOps_DDRM#identity(int)
         *
         * @param width The width and height of the matrix.
         * @return An identity matrix.
         */
        public static SimpleMatrixD identity(int width)
        {
            SimpleMatrixD ret = new SimpleMatrixD(width, width);
            CommonOps_DDRM.setIdentity(ret.mat);
            return ret;
        }

        /**
         * <p>
         * Creates a matrix where all but the diagonal elements are zero.  The values
         * of the diagonal elements are specified by the parameter 'vals'.
         * </p>
         *
         * <p>
         * To extract the diagonal elements from a matrix see {@link #diag()}.
         * </p>
         *
         * @see CommonOps_DDRM#diag(double...)
         *
         * @param vals The values of the diagonal elements.
         * @return A diagonal matrix.
         */
        public static SimpleMatrixD diag(double[] vals)
        {
            DMatrixRMaj m = CommonOps_DDRM.diag(vals);
            SimpleMatrixD ret = wrap(m);
            return ret;
        }

        /**
         * <p>
         * Creates a new SimpleMatrix with random elements drawn from a uniform distribution from minValue to maxValue.
         * </p>
         *
         * @see RandomMatrices_DDRM#fillUniform(DMatrixRMaj,java.util.Random)
         *
         * @param numRows The number of rows in the new matrix
         * @param numCols The number of columns in the new matrix
         * @param minValue Lower bound
         * @param maxValue Upper bound
         * @param rand The random number generator that's used to fill the matrix.  @return The new random matrix.
         */
        public static SimpleMatrixD random64(int numRows, int numCols, double minValue, double maxValue, IMersenneTwister rand)
        {
            SimpleMatrixD ret = new SimpleMatrixD(numRows, numCols);
            RandomMatrices_DDRM.fillUniform(ret.mat as DMatrixRMaj, minValue, maxValue, rand);
            return ret;
        }

        /**
         * <p>
         * Creates a new vector which is drawn from a multivariate normal distribution with zero mean
         * and the provided covariance.
         * </p>
         *
         * @see CovarianceRandomDraw_DDRM
         *
         * @param covariance Covariance of the multivariate normal distribution
         * @return Vector randomly drawn from the distribution
         */
        public static SimpleMatrixD randomNormal(SimpleMatrixD covariance, IMersenneTwister random)
        {
            SimpleMatrixD found = new SimpleMatrixD(covariance.numRows(), 1);

            var draw = new CovarianceRandomDraw_DDRM(random, covariance.getMatrix());
            draw.next(found.getMatrix());

            return found;
        }

        /// <summary>
        /// Creates a new SimpleMatrix with the specified matrix used as its internal matrix.
        /// This means that the reference is saved and calls made to the returned SimpleMatrix 
        /// will modify the passed in matrix.
        ///
        /// </summary>
        /// <param name="internalMat">The internal matrix of the returned SimpleMatrix (will be modified).</param>
        public static SimpleMatrixD wrap(DMatrixRMaj internalMat)
        {
            SimpleMatrixD ret = new SimpleMatrixD();
            ret.setMatrix(internalMat);
            return ret;
        }

        /// <summary>
        /// Loads a new matrix from a serialized binary file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <see cref="MatrixIO.loadBin{DMatrix}(string)"/>
        public static SimpleMatrixD loadBinary(string fileName)
        {
            DMatrix mat = MatrixIO.loadBin<DMatrix>(fileName);

            // see if its a DMatrixRMaj
            if (mat is DMatrixRMaj)
            {
                return wrap(mat as DMatrixRMaj);
            }
            else
            {
                // if not convert it into one and wrap it
                return wrap(new DMatrixRMaj(mat));
            }
        }

        #endregion // Static Methods

        #region Protected Methods

        protected override void setMatrix(DMatrixRMaj mat)
        {
            this.mat = mat;
            this.ops = new SimpleOperations_DDRM();
        }

        /// <summary>
        /// We can't return SimpleMatrix here because we're implementing 
        /// an abstract method declared on the SimpleBase class.
        /// That allows other "wrappers" to adhere to the same API.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        protected override SimpleMatrixD wrapMatrix(DMatrixRMaj m)
        {
            return new SimpleMatrixD(m);
        }

        /// <summary>
        /// This is dangerous (and pointless) because it really only creates 
        /// a matrix of THIS type, so passing in a MatrixType enum value is problematic.
        /// TODO : Check that this is only used in a safe way, or better yet,
        /// change it so only types matching the current generic type can be created.
        /// </summary>
        protected override SimpleMatrixD createMatrix(int numRows, int numCols)
        {
            return new SimpleMatrixD(numRows, numCols);
        }

        #endregion // Protected Methods

        #region Public Methods

        public override IEnumerator<double> GetEnumerator()
        {
            return new DMatrixIterator(this.mat, true, 0, 0, mat.numRows, mat.numCols);
        }


        /// <summary>
        /// Returns the value of the specified matrix element.  
        /// Performs a bounds check to make sure the 
        /// requested element is part of the matrix.
        /// </summary>
        public override double get(int row, int col)
        {
            return mat.get(row, col);
        }

        /// <summary>
        /// Returns the value of the matrix at the specified index of the 1D row major array.
        /// </summary>
        public override double get(int index)
        {
            return mat.data[index];
        }

        ///// <summary>
        ///// Creates a new iterator for traversing through a submatrix 
        ///// inside this matrix. It can be traversed by row or by column.
        ///// Range of elements is inclusive, e.g. minRow = 0 and maxRow = 1 will include rows
        ///// 0 and 1.  The iteration starts at (minRow, minCol) and ends at (maxRow, maxCol).
        ///// </summary>
        ///// <param name="rowMajor">true means it will traverse through the submatrix by row first, false by columns.</param>
        ///// <param name="minRow"> first row it will start at.</param>
        ///// <param name="minCol">first column it will start at.</param>
        ///// <param name="maxRow">last row it will stop at.</param>
        ///// <param name="maxCol">last column it will stop at.</param>
        ///// <returns>A new MatrixIterator</returns>
        //public DMatrixIterator iterator(bool rowMajor, int minRow, int minCol, int maxRow, int maxCol)
        //{
        //    return new DMatrixIterator(mat, rowMajor, minRow, minCol, maxRow, maxCol);
        //}

        /// <summary>
        /// Sets all the elements in the matrix equal to zero.
        /// </summary>
        /// <see cref="CommonOps_DDRM.fill(DMatrixD1, double)"/>
        public override void zero()
        {
            mat.zero();
        }

        /// <summary>
        /// Sets all the elements in this matrix equal to the specified value.
        /// </summary>
        /// <see cref="CommonOps_DDRM.fill(DMatrixD1, double)"/>
        public override void set(double val)
        {
            CommonOps_DDRM.fill(mat, val);
        }


        /// <summary>
        /// Assigns the element in the Matrix to the specified value.  
        /// Performs a bounds check to make sure the requested element is part of the matrix.
        /// </summary>
        public override void set(int row, int col, double value)
        {
            mat.set(row, col, value);
        }

        /// <summary>
        /// Assigns an element a value based on its index in the internal array.
        /// </summary>
        public override void set(int index, double value)
        {
            mat.set(index, value);
        }

        /// <summary>
        /// Assigns consecutive elements inside a row to the provided array.
        /// </summary>
        public override void setRow(int row, int startColumn, double[] values)
        {
            ops.setRow(mat, row, startColumn, values);
        }

        /// <summary>
        /// Assigns consecutive elements inside a column to the provided array.
        /// A(offset:(offset + values.Length),column) = values
        /// </summary>
        public void setColumn(int column, int startRow, double[] values)
        {
            ops.setColumn(mat, column, startRow, values);
        }

        /// <summary>
        /// Copy matrix B into this matrix at location (insertRow, insertCol).
        /// </summary>
        public void insertIntoThis(int insertRow, int insertCol, SimpleMatrixD B)
        {
            var bm = B.getMatrix();
            CommonOps_DDRM.insert(bm, mat, insertRow, insertCol);
        }

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
        public double dot(SimpleMatrixD v)
        {
            if (!isVector())
                throw new ArgumentException("'this' matrix is not a vector.");
            else if (!v.isVector())
                throw new ArgumentException("'v' matrix is not a vector.");

            var vm = v.getMatrix();

            return VectorVectorMult_DDRM.innerProd(mat, vm);
        }

        /// <summary>
        /// Computes the Frobenius normal of the matrix:
        /// <code>normF = Math.Sqrt(A.Sum(v => Math.Abs(v * v))) /* fast version */</code>
        /// </summary>
        /// <see cref="NormOps_DDRM.normF(DMatrixD1)"/>
        public double normF()
        {
            return ops.normF(mat);
        }

        /// <summary>
        /// The condition p = 2 number of a matrix is used to measure 
        /// the sensitivity of the linear system <b>Ax=b</b>.  
        /// A value near one indicates that it is a well conditioned matrix.
        /// </summary>
        /// <see cref="NormOps_DDRM.conditionP2(DMatrixRMaj)"/>
        public double conditionP2()
        {
            return ops.conditionP2(mat);
        }

        /// <summary>
        /// Computes the determinant of the matrix.
        /// </summary>
        public double determinant()
        {
            double ret = ops.determinant(mat);
            if (UtilEjml.isUncountable(ret))
                return 0;
            return ret;
        }

        /// <summary>
        /// Computes the trace of the matrix.
        /// </summary>
        public double trace()
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
        public bool isIdentical(SimpleMatrixD a, double tol)
        {
            var am = a.getMatrix();
            return MatrixFeatures_DDRM.isIdentical(mat, am, tol);
        }

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
        public SimpleMatrixD rows(int begin, int end)
        {
            return extractMatrix(begin, end, 0, END);
        }

        /// <summary>
        /// Extracts the specified rows from the matrix.
        /// </summary>
        /// <param name="begin">First row.  Inclusive.</param>
        /// <param name="end">Last row + 1.</param>
        /// <returns>Submatrix that contains the specified rows.</returns>
        public SimpleMatrixD cols(int begin, int end)
        {
            return extractMatrix(0, END, begin, end);
        }

        /// <summary>
        /// Concatinates all the matrices together along their columns.  
        /// If the rows do not match the upper elements are set to zero.
        /// <code>A = [ m[0] , ... , m[n-1] ]</code>
        /// </summary>
        /// <param name="A">Set of matrices</param>
        /// <returns>Resulting matrix.</returns>
        public SimpleMatrixD concatColumns(params SimpleMatrixD[] A)
        {
            DMatrixRMaj[] m = new DMatrixRMaj[A.Length + 1];
            m[0] = mat;
            for (int i = 0; i < A.Length; i++)
            {
                m[1] = A[i].getMatrix();
            }
            var combined = CommonOps_DDRM.concatColumns(m);
            return wrapMatrix(combined);
        }

        /// <summary>
        /// Concatinates all the matrices together along their columns.  
        /// If the rows do not match the upper elements are set to zero.
        /// <code>A = [ m[0] ; ... ; m[n-1] ]</code>
        /// </summary>
        /// <param name="A">A Set of matrices.</param>
        /// <returns>Resulting matrix.</returns>
        public SimpleMatrixD concatRows(params SimpleMatrixD[] A)
        {
            DMatrixRMaj[] m = new DMatrixRMaj[A.Length + 1];
            m[0] = mat;
            for (int i = 0; i < A.Length; i++)
            {
                m[1] = A[i].getMatrix();
            }
            var combined = CommonOps_DDRM.concatRows(m);
            return wrapMatrix(combined);
        }

        /// <summary>
        /// Returns the transpose of this matrix.
        /// </summary>
        public SimpleMatrixD transpose()
        {
            SimpleMatrixD ret = createMatrix(mat.getNumCols(), mat.getNumRows());
            ops.transpose(mat, ret.mat);
            return ret;
        }

        /// <summary>
        /// Returns a matrix which is the result of matrix multiplication.
        /// <code>c = a * b</code>
        /// where c is the returned matrix, a is this matrix, and b is the passed in matrix.
        /// </summary>
        /// <see cref="CommonOps_DDRM.mult(DMatrix1Row, DMatrix1Row, DMatrix1Row)"/>
        /// <para name="b">A matrix that is n by bn. Not modified.</para>
        public SimpleMatrixD mult(SimpleMatrixD b)
        {
            SimpleMatrixD ret = createMatrix(mat.getNumRows(), b.getMatrix().getNumCols());
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
        public SimpleMatrixD kron(SimpleMatrixD B)
        {
            SimpleMatrixD ret = createMatrix(mat.getNumRows() * B.numRows(), mat.getNumCols() * B.numCols());
            ops.kron(mat, B.mat, ret.mat);
            return ret;
        }

        /// <summary>
        /// Returns the result of matrix addition.
        /// </summary>
        /// <see cref="SimpleOperations{T}"/>
        public SimpleMatrixD plus(SimpleMatrixD b)
        {
            SimpleMatrixD ret = createMatrix(mat.getNumRows(), mat.getNumCols());
            ops.plus(mat, b.mat, ret.mat);
            return ret;
        }

        /// <summary>
        /// Returns the result of matrix subtraction.
        /// <code>c = a - b</code>
        /// where c is the returned matrix, a is this matrix, and b is the passed in matrix.
        /// </summary>
        /// <see cref="SimpleOperations{T}"/>
        public SimpleMatrixD minus(SimpleMatrixD b)
        {
            SimpleMatrixD ret = createMatrix(mat.getNumRows(), mat.getNumCols());
            ops.minus(mat, b.mat, ret.mat);
            return ret;
        }

        /// <summary>
        /// Returns the result of matrix-double subtraction:
        /// <code>c = a - b</code>
        /// where c is the returned matrix, a is this matrix, and b is the passed in double.
        /// </summary>
        /// <see cref="CommonOps_DDRM.subtract(DMatrixD1, double, DMatrixD1)"/>
        public SimpleMatrixD minus(double b)
        {
            SimpleMatrixD ret = copy();

            var m = getMatrix();
            var rm = ret.getMatrix();

            CommonOps_DDRM.subtract(m, b, rm);

            return ret;
        }

        /// <summary>
        /// Returns the result of scalar addition:
        /// <code>c = a + b</code>
        /// where c is the return matrix, a is this matrix, and b is the passed in double.
        /// </summary>
        /// <see cref="CommonOps_DDRM.add(DMatrixD1, double, DMatrixD1)"/>
        public SimpleMatrixD plus(double b)
        {
            SimpleMatrixD ret = createMatrix(numRows(), numCols());

            var m = getMatrix();
            var rm = ret.getMatrix();

            CommonOps_DDRM.add(m, b, rm);

            return ret;
        }

        /// <summary>
        /// Performs a matrix addition and scale operation.
        /// <code>c = a + beta*b</code>
        /// where c is the returned matrix, a is this matrix, and b is the passed in matrix.
        /// </summary>
        /// <see cref="CommonOps_DDRM.add(DMatrixD1, double, DMatrixD1, DMatrixD1)"/>
        public SimpleMatrixD plus(double beta, SimpleMatrixD b)
        {
            SimpleMatrixD ret = copy();

            var rm = ret.getMatrix();
            var bm = b.getMatrix();

            CommonOps_DDRM.addEquals(rm as DMatrixRMaj, beta, bm as DMatrixRMaj);

            return ret;
        }

        /// <summary>
        ///  Returns the result of scaling each element by 'val':
        /// <code>b[i,j] = val*a[i,j]</code>
        /// </summary>
        /// <param name="val">The multiplication factor.</param>
        /// <see cref="CommonOps_DDRM.scale(double, DMatrixRMaj)"/>
        public SimpleMatrixD scale(double val)
        {
            SimpleMatrixD ret = copy();

            var rm = ret.getMatrix();

            CommonOps_DDRM.scale(val, rm);

            return ret;
        }

        /// <summary>
        /// Returns the result of dividing each element by 'val':
        /// <code>b[i,j] = a[i,j]/val</code>
        /// </summary>
        /// <param name="val">Divisor</param>
        /// <returns>Matrix with its elements divided by the specified value.</returns>
        /// <see cref="CommonOps_DDRM.divide(DMatrixD1, double)"/>
        public SimpleMatrixD divide(double val)
        {
            SimpleMatrixD ret = copy();
            var rm = ret.getMatrix();
            CommonOps_DDRM.divide(rm, val);
            return ret;
        }

        /// <summary>
        /// Returns the inverse of this matrix.
        /// If the matrix could not be inverted then SingularMatrixException is thrown.  
        /// Even if no exception is thrown the matrix could still be singular or nearly singular.
        /// </summary>
        /// <exception cref="SingularMatrixException"/>
        /// <see cref="CommonOps_DDRM.invert(DMatrixRMaj, DMatrixRMaj)"/>
        public SimpleMatrixD invert()
        {
            SimpleMatrixD ret = createMatrix(mat.getNumRows(), mat.getNumCols());

            if (!ops.invert(mat, ret.mat))
                throw new SingularMatrixException();
            if (ops.hasUncountable(ret.mat))
                throw new SingularMatrixException("Solution contains uncountable numbers");

            return ret;
        }

        /// <summary>
        /// Computes the Moore-Penrose pseudo-inverse.
        /// </summary>
        public SimpleMatrixD pseudoInverse()
        {
            SimpleMatrixD ret = createMatrix(mat.getNumCols(), mat.getNumRows());
            CommonOps_DDRM.pinv(mat, ret.getMatrix());

            return ret;
        }

        /*
         * <p>
         * <br>
         * <br>
         * x = a<sup>-1</sup>b<br>
         * <br>
         * where 'a' is this matrix and 'b' is an n by p matrix.
         * </p>
         *
         * <p>
         * 
         *
         * </p>
         *
         * @see CommonOps_DDRM#solve(DMatrixRMaj, DMatrixRMaj, DMatrixRMaj)
         *
         * @throws SingularMatrixException
         *
         * @param b n by p matrix. Not modified.
         * @return The solution for 'x' that is n by p.
         */
        /// <summary>
        /// Solves for X in the following equation:
        /// <code>x = pow(a, -1)</code>
        /// where 'a' is this matrix and 'b' is an n by p matrix.
        /// If the system could not be solved then SingularMatrixException is thrown.  
        /// Even if no exception is thrown 'a' could still be singular or nearly singular.
        /// </summary>
        /// <see cref="CommonOps_DDRM.solve(DMatrixRMaj, DMatrixRMaj, DMatrixRMaj)"/>
        public SimpleMatrixD solve(SimpleMatrixD b)
        {
            SimpleMatrixD x = createMatrix(mat.getNumCols(), b.getMatrix().getNumCols());

            if (!ops.solve(mat, x.mat, b.mat))
                throw new SingularMatrixException();
            if (ops.hasUncountable(x.mat))
                throw new SingularMatrixException("Solution contains uncountable numbers");

            return x;
        }


        /// <summary>
        /// Creates and returns a matrix which is idential to this one.
        /// </summary>
        public SimpleMatrixD copy()
        {
            SimpleMatrixD ret = createMatrix(mat.getNumRows(), mat.getNumCols());
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
        public SimpleMatrixD extractMatrix(int y0, int y1, int x0, int x1)
        {
            if (y0 == END) y0 = mat.getNumRows();
            if (y1 == END) y1 = mat.getNumRows();
            if (x0 == END) x0 = mat.getNumCols();
            if (x1 == END) x1 = mat.getNumCols();

            SimpleMatrixD ret = createMatrix(y1 - y0, x1 - x0);

            ops.extract(mat, y0, y1, x0, x1, ret.mat, 0, 0);

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
        public SimpleMatrixD extractVector(bool extractRow, int element)
        {
            int length = extractRow ? mat.getNumCols() : mat.getNumRows();

            SimpleMatrixD ret = extractRow ? createMatrix(1, length) : createMatrix(length, 1);

            var rm = ret.getMatrix();

            if (extractRow)
                SpecializedOps_DDRM.subvector(mat, element, 0, length, true, 0, rm);
            else
                SpecializedOps_DDRM.subvector(mat, 0, element, length, false, 0, rm);

            return ret;
        }

        /// <summary>
        /// If a vector then a square matrix is returned if a matrix then a vector of diagonal ements is returned.
        /// </summary>
        /// <returns>Diagonal elements inside a vector or a square matrix with the same diagonal elements.</returns>
        /// <see cref="CommonOps_DDRM.extractDiag(DMatrixRMaj, DMatrixRMaj)"/>
        public SimpleMatrixD diag()
        {
            SimpleMatrixD diag;

            if (MatrixFeatures_DDRM.isVector(mat))
            {
                int N = Math.Max(mat.getNumCols(), mat.getNumRows());
                diag = createMatrix(N, N);
                var dm = diag.getMatrix();
                CommonOps_DDRM.diag(dm, N, mat.data);
            }
            else
            {
                int N = Math.Min(mat.getNumCols(), mat.getNumRows());
                diag = createMatrix(N, 1);
                var dm = diag.getMatrix();
                CommonOps_DDRM.extractDiag(mat, dm);
            }

            return diag;
        }

        /// <summary>
        /// Creates a new matrix that is a combination of this matrix and matrix B.  B is
        /// written into A at the specified location if needed the size of A is increased by
        /// growing it.  A is grown by padding the new area with zeros.
        /// 
        /// While useful when adding data to a matrix which will be solved for it is also much
        /// less efficient than predeclaring a matrix and inserting data into it.
        /// 
        /// If insertRow or insertCol is set to SimpleMatrix.END then it will be combined
        /// at the last row or column respectively.
        /// </summary>
        /// <param name="insertRow">Row where matrix B is written in to.</param>
        /// <param name="insertCol">Column where matrix B is written in to.</param>
        /// <param name="B">The matrix that is written into A.</param>
        /// <returns>A new combined matrix.</returns>
        public SimpleMatrixD combine(int insertRow, int insertCol, SimpleMatrixD B)
        {
            if (insertRow == END)
                insertRow = mat.getNumRows();
            if (insertCol == END)
                insertCol = mat.getNumCols();

            int maxRow = insertRow + B.numRows();
            int maxCol = insertCol + B.numCols();

            SimpleMatrixD ret;

            if (maxRow > mat.getNumRows() || maxCol > mat.getNumCols())
            {
                int M = Math.Max(maxRow, mat.getNumRows());
                int N = Math.Max(maxCol, mat.getNumCols());

                ret = createMatrix(M, N);
                ret.insertIntoThis(0, 0, this);
            }
            else
            {
                ret = copy();
            }

            ret.insertIntoThis(insertRow, insertCol, B);

            return ret;
        }

        /// <summary>
        /// Returns a new matrix whose elements are the negative of 'this' matrix's elements.
        /// <code>b[ij] = -a[ij]</code>
        /// </summary>
        /// <returns>A matrix that is the negative of the original.</returns>
        public SimpleMatrixD negative()
        {
            SimpleMatrixD A = copy();
            ops.changeSign(A.mat);
            return A;
        }



        /// <summary>
        /// Returns the maximum absolute value of all the elements in this matrix.  
        /// This is equivalent the the infinite p-norm of the matrix.
        /// </summary>
        /// <returns>Largest absolute value of any element.</returns>
        public double elementMaxAbs()
        {
            return ops.elementMaxAbs(mat);
        }

        /// <summary>
        /// Computes the sum of all the elements in the matrix.
        /// </summary>
        public double elementSum()
        {
            return ops.elementSum(mat);
        }

        /// <summary>
        /// Returns a matrix which is the result of an element by element multiplication of 'this' and 'b':
        /// <code>c[i,j] = a[i,j] * b[i,j]</code>
        /// </summary>
        /// <param name="b">A simple matrix.</param>
        /// <returns>The element by element multiplication of 'this' and 'b'.</returns>
        public SimpleMatrixD elementMult(SimpleMatrixD b)
        {
            SimpleMatrixD c = createMatrix(mat.getNumRows(), mat.getNumCols());
            ops.elementMult(mat, b.mat, c.mat);
            return c;
        }

        /// <summary>
        /// Returns a matrix which is the result of an element by element division of 'this' and 'b':
        /// <code>c[i,j] = a[i,j] / b[i,j]</code>
        /// </summary>
        /// <param name="b">A simple matrix.</param>
        /// <returns>The element by element division of 'this' and 'b'.</returns>
        public SimpleMatrixD elementDiv(SimpleMatrixD b)
        {
            SimpleMatrixD c = createMatrix(mat.getNumRows(), mat.getNumCols());
            ops.elementDiv(mat, b.mat, c.mat);
            return c;
        }

        /// <summary>
        /// Returns a matrix which is the result of an element by element power of 'this' and 'b':
        /// <code>c[i,j] = a[i,j] ^ b[i,j]</code>
        /// </summary>
        /// <param name="b">A simple matrix.</param>
        /// <returns>The element by element power of 'this' and 'b'.</returns>
        public SimpleMatrixD elementPower(SimpleMatrixD b)
        {
            SimpleMatrixD c = createMatrix(mat.getNumRows(), mat.getNumCols());
            ops.elementPower(mat, b.mat, c.mat);
            return c;
        }

        /// <summary>
        /// Returns a matrix which is the result of an element by element power of 'this' and 'b':
        /// <code>c[i,j] = a[i,j] ^ b</code>
        /// </summary>
        /// <param name="b">Scalar.</param>
        /// <returns>The element by element power of 'this' and 'b'.</returns>
        public SimpleMatrixD elementPower(double b)
        {
            SimpleMatrixD c = createMatrix(mat.getNumRows(), mat.getNumCols());
            ops.elementPower(mat, b, c.mat);
            return c;
        }

        /// <summary>
        /// Returns a matrix which is the result of an element by element exp of 'this':
        /// <code>c[i,j] = Math.Exp(a[i,j])</code>
        /// </summary>
        /// <returns>The element by element power of 'this' and 'b'.</returns>
        public SimpleMatrixD elementExp()
        {
            SimpleMatrixD c = createMatrix(mat.getNumRows(), mat.getNumCols());
            ops.elementExp(mat, c.mat);
            return c;
        }

        /// <summary>
        /// Returns a matrix which is the result of an element by element exp of 'this':
        /// <code>c[i,j] = Math.Log(a[i,j])</code>
        /// </summary>
        public SimpleMatrixD elementLog()
        {
            SimpleMatrixD c = createMatrix(mat.getNumRows(), mat.getNumCols());
            ops.elementLog(mat, c.mat);
            return c;
        }


        /// <summary>
        /// Computes a full Singular Value Decomposition (SVD) of this matrix 
        /// with the eigenvalues ordered from largest to smallest.
        /// </summary>
        /// <returns></returns>
        public SimpleSVD<DMatrixRMaj> svd()
        {
            return new SimpleSVD<DMatrixRMaj>(mat, false);
        }

        /// <summary>
        /// Computes the SVD in either  compact format or full format.
        /// </summary>
        public SimpleSVD<DMatrixRMaj> svd(bool compact)
        {
            return new SimpleSVD<DMatrixRMaj>(mat, compact);
        }

        /// <summary>
        /// Returns the Eigen Value Decomposition (EVD) of this matrix.
        /// </summary>
        public SimpleEVD<DMatrixRMaj> eig()
        {
            return new SimpleEVD<DMatrixRMaj>(mat);
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
            MatrixIO.print(Console.OpenStandardOutput(), mat, numChar, precision);
        }

        /// <summary>
        /// Prints the matrix to standard out given a floating point format.
        /// Example format: {0:F2,5} (numChar = 5, precision = 2).
        /// </summary>
        public void print(string format)
        {
            MatrixIO.print(Console.OpenStandardOutput(), mat, format);
        }

        /// <summary>
        /// Converts the array into a string format for display purposes.
        /// </summary>
        /// <see cref="MatrixIO.print(Stream, DMatrix)"/>
        public string toString()
        {
            using (var stream = new MemoryStream())
            {
                MatrixIO.print(stream, mat);
                stream.Position = 0;
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
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
                else if (o is DMatrixRMaj)
                {
                    eq.alias((DMatrixRMaj)o, name);
                }
                else if (o is double)
                {
                    eq.alias((double)o, name);
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
            MatrixIO.saveBin(mat, fileName);
        }

        /// <summary>
        /// Saves this matrix to a file in a CSV format.  
        /// For the file format <see cref="MatrixIO"/>.
        /// </summary>
        /// <param name="fileName"></param>
        public void saveToFileCSV(string fileName)
        {
            MatrixIO.saveCSV(mat, fileName);
        }

        /// <summary>
        /// Loads a new matrix from a CSV file.  
        /// For the file format <see cref="MatrixIO"/>.
        /// </summary>
        /// <exception cref="IOException"/>
        public SimpleMatrixD loadCSV(string fileName)
        {
            DMatrix mat = MatrixIO.loadCSV(fileName);

            SimpleMatrixD ret = createMatrix(1, 1);

            ret.setMatrix(mat as DMatrixRMaj);

            return ret;
        }

        /// <summary>
        /// Prints the number of rows and column in this matrix.
        /// </summary>
        public void printDimensions()
        {
            Console.WriteLine("[rows = " + numRows() + " , cols = " + numCols() + " ]");
        }

        #endregion // Public Methods
    }

}