using System;
using SharpMatrix.Data;
using SharpMatrix.Interfaces.Decomposition;
using SharpMatrix.Sparse.Csc.Misc;

namespace SharpMatrix.Sparse.Csc.Decomposition.LU
{
    //package org.ejml.sparse.csc.decomposition.lu;

/**
 * LU Decomposition using a left looking algorithm for {@link DMatrixSparseCSC}.
 *
 * <p>NOTE: Based mostly on the algorithm described on page 86 in csparse. cs_lu</p>
 * <p>NOTE: See in code comment for a modification from csparse.</p>
 * @author Peter Abeles
 */
    public class LuUpLooking_DSCC : LUSparseDecomposition_F64<DMatrixSparseCSC>
    {
        private ApplyFillReductionPermutation applyReduce;

        // storage for LU decomposition
        private DMatrixSparseCSC L = new DMatrixSparseCSC(0, 0, 0);

        private DMatrixSparseCSC U = new DMatrixSparseCSC(0, 0, 0);

        // row pivot matrix, for numerical stability
        private int[] pinv = new int[0];

        // work space variables
        private double[] x = new double[0];

        private IGrowArray gxi = new IGrowArray(); // storage for non-zero pattern
        private IGrowArray gw = new IGrowArray();

        // true if a singular matrix is detected
        private bool singular;

        public LuUpLooking_DSCC(ComputePermutation<DMatrixSparseCSC> reduceFill)
        {
            this.applyReduce = new ApplyFillReductionPermutation(reduceFill, false);
        }

        //@Override
        public bool decompose(DMatrixSparseCSC A)
        {
            initialize(A);
            return performLU(applyReduce.apply(A));
        }

        private void initialize(DMatrixSparseCSC A)
        {
            int m = A.numRows;
            int n = A.numCols;
            int o = Math.Min(m, n);
            // number of non-zero elements can only be easily estimated because of pivots
            L.reshape(m, m, 4 * A.nz_length + o);
            L.nz_length = 0;
            U.reshape(m, n, 4 * A.nz_length + o);
            U.nz_length = 0;

            singular = false;
            if (pinv.Length != m)
            {
                pinv = new int[m];
                x = new double[m];
            }

            for (int i = 0; i < m; i++)
            {
                pinv[i] = -1;
                L.col_idx[i] = 0;
            }
        }

        private bool performLU(DMatrixSparseCSC A)
        {
            int m = A.numRows;
            int n = A.numCols;
            int[] q = applyReduce.getArrayP();

            // main loop for computing L and U
            for (int k = 0; k < n; k++)
            {
                //--------- Triangular Solve
                L.col_idx[k] = L.nz_length; // start of column k
                U.col_idx[k] = U.nz_length;

                // grow storage in L and U if needed
                if (L.nz_length + n > L.nz_values.Length)
                    L.growMaxLength(2 * L.nz_values.Length + n, true);
                if (U.nz_length + n > U.nz_values.Length)
                    U.growMaxLength(2 * U.nz_values.Length + n, true);

                int col = q != null ? q[k] : k;
                int top = TriangularSolver_DSCC.solve(L, true, A, col, x, pinv, gxi, gw);
                int[] xi = gxi.data;

                //--------- Find the Next Pivot. That will be the row with the largest value
                //
                int ipiv = -1;
                double a = -double.MaxValue;
                for (int p = top; p < n; p++)
                {
                    int i = xi[p]; // x(i) is nonzero
                    if (pinv[i] < 0)
                    {
                        double t;
                        if ((t = Math.Abs(x[i])) > a)
                        {
                            a = t;
                            ipiv = i;
                        }
                    }
                    else
                    {
                        U.nz_rows[U.nz_length] = pinv[i];
                        U.nz_values[U.nz_length++] = x[i];
                    }
                }
                if (ipiv == -1 || a <= 0)
                {
                    singular = true;
                    return false;
                }

                // NOTE: The line is commented out below. It can cause a poor pivot to be selected. Instead of the largest
                //       row it will pick whatever is in this column. it does try to make sure it's not zero, but I'm not
                //       sure what it's purpose is.
//            if( pinv[col] < 0 && Math.Abs(x[col]) >= a*tol ) {
//                ipiv = col;
//            }

                //---------- Divide by the pivot
                double pivot = x[ipiv];
                U.nz_rows[U.nz_length] = k;
                U.nz_values[U.nz_length++] = pivot; // last entry in U(:k) us U(k,k)
                pinv[ipiv] = k;
                L.nz_rows[L.nz_length] = ipiv; // First entry L(:,k) is L(k,k) = 1
                L.nz_values[L.nz_length++] = 1;

                for (int p = top; p < n; p++)
                {
                    int i = xi[p];
                    if (pinv[i] < 0)
                    {
                        // x(i) is entry in L(:,k)
                        L.nz_rows[L.nz_length] = i;
                        L.nz_values[L.nz_length++] = x[i] / pivot;
                    }
                    x[i] = 0;
                }
            }
            //----------- Finalize L and U
            L.col_idx[n] = L.nz_length;
            U.col_idx[n] = U.nz_length;
            for (int p = 0; p < L.nz_length; p++)
            {
                L.nz_rows[p] = pinv[L.nz_rows[p]];
            }

//        Console.WriteLine("  reduce "+(reduceFill!=null));
//        System.out.print("  pinv[ ");
//        for (int i = 0; i < A.numCols; i++) {
//            System.out.printf("%2d ",pinv[i]);
//        }
//        Console.WriteLine(" ]");

            return true;
        }

        //@Override
        public Complex_F64 computeDeterminant()
        {
            // see dense algorithm. There is probably a faster way to compute the sign while decomposing
            // the matrix.
            double value = UtilEjml.permutationSign(pinv, U.numCols, gw.data);
            for (int i = 0; i < U.numCols; i++)
            {
                value *= U.nz_values[U.col_idx[i + 1] - 1];
            }
            return new Complex_F64(value, 0);
        }

        //@Override
        public DMatrixSparseCSC getLower(DMatrixSparseCSC lower)
        {
            if (lower == null)
                lower = new DMatrixSparseCSC(1, 1, 0);
            lower.set(L);
            return lower;
        }

        //@Override
        public DMatrixSparseCSC getUpper(DMatrixSparseCSC upper)
        {
            if (upper == null)
                upper = new DMatrixSparseCSC(1, 1, 0);
            upper.set(U);
            return upper;
        }

        //@Override
        public DMatrixSparseCSC getRowPivot(DMatrixSparseCSC pivot)
        {
            if (pivot == null)
                pivot = new DMatrixSparseCSC(L.numRows, L.numRows, 0);
            pivot.reshape(L.numRows, L.numRows, L.numRows);
            CommonOps_DSCC.permutationMatrix(pinv, true, L.numRows, pivot);
            return pivot;
        }

        //@Override
        public int[] getRowPivotV(IGrowArray pivot)
        {
            return UtilEjml.pivotVector(pinv, L.numRows, pivot);
        }

        //@Override
        public bool isSingular()
        {
            return singular;
        }

        //@Override
        public bool inputModified()
        {
            return false;
        }

        public int[] getPinv()
        {
            return pinv;
        }

        public DMatrixSparseCSC getL()
        {
            return L;
        }

        public DMatrixSparseCSC getU()
        {
            return U;
        }

        public ComputePermutation<DMatrixSparseCSC> getReduceFill()
        {
            return applyReduce.getFillReduce();
        }

        public int[] getReducePermutation()
        {
            return applyReduce.getArrayP();
        }

        //@Override
        public void lockStructure()
        {
            throw new InvalidOperationException(
                "Can't lock a LU decomposition. Pivots change depending on numerical values and not just" +
                "the matrix's structure");
        }

        //@Override
        public bool isStructureLocked()
        {
            return false;
        }
    }
}