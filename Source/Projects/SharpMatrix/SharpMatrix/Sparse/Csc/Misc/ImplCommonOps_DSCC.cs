using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Sparse.Csc.Misc
{
    //package org.ejml.sparse.csc.misc;

/**
 * Implementation class.  Not recommended for direct use.  Instead use {@link CommonOps_DSCC}
 * instead.
 *
 * @author Peter Abeles
 */
    public class ImplCommonOps_DSCC
    {

        /**
         * Performs a matrix transpose.
         *
         * @param A Original matrix.  Not modified.
         * @param C Storage for transposed 'a'.  Reshaped.
         * @param gw (Optional) Storage for internal workspace.  Can be null.
         */
        public static void transpose(DMatrixSparseCSC A, DMatrixSparseCSC C, IGrowArray gw)
        {
            int[] work = TriangularSolver_DSCC.adjust(gw, A.numRows, A.numRows);
            C.reshape(A.numCols, A.numRows, A.nz_length);

            // compute the histogram for each row in 'a'
            int idx0 = A.col_idx[0];
            for (int j = 1; j <= A.numCols; j++)
            {
                int idx1 = A.col_idx[j];
                for (int i = idx0; i < idx1; i++)
                {
                    if (A.nz_rows.Length <= i)
                        throw new InvalidOperationException("Egads");
                    work[A.nz_rows[i]]++;
                }
                idx0 = idx1;
            }

            // construct col_idx in the transposed matrix
            C.colsum(work);

            // fill in the row indexes
            idx0 = A.col_idx[0];
            for (int j = 1; j <= A.numCols; j++)
            {
                int col = j - 1;
                int idx1 = A.col_idx[j];
                for (int i = idx0; i < idx1; i++)
                {
                    int row = A.nz_rows[i];
                    int index = work[row]++;
                    C.nz_rows[index] = col;
                    C.nz_values[index] = A.nz_values[i];
                }
                idx0 = idx1;
            }
        }

        /**
         * Performs matrix addition:<br>
         * C = &alpha;A + &beta;B
         *
         * @param alpha scalar value multiplied against A
         * @param A Matrix
         * @param beta scalar value multiplied against B
         * @param B Matrix
         * @param C Output matrix.
         * @param gw (Optional) Storage for internal workspace.  Can be null.
         * @param gx (Optional) Storage for internal workspace.  Can be null.
         */
        public static void add(double alpha, DMatrixSparseCSC A, double beta, DMatrixSparseCSC B, DMatrixSparseCSC C,
            IGrowArray gw, DGrowArray gx)
        {
            double[] x = TriangularSolver_DSCC.adjust(gx, A.numRows);
            int[] w = TriangularSolver_DSCC.adjust(gw, A.numRows, A.numRows);

            C.indicesSorted = false;
            C.nz_length = 0;

            for (int col = 0; col < A.numCols; col++)
            {
                C.col_idx[col] = C.nz_length;

                ImplSparseSparseMult_DSCC.multAddColA(A, col, alpha, C, col + 1, x, w);
                ImplSparseSparseMult_DSCC.multAddColA(B, col, beta, C, col + 1, x, w);

                // take the values in the dense vector 'x' and put them into 'C'
                int idxC0 = C.col_idx[col];
                int idxC1 = C.col_idx[col + 1];

                for (int i = idxC0; i < idxC1; i++)
                {
                    C.nz_values[i] = x[C.nz_rows[i]];
                }
            }
        }

        /**
         * Adds the results of adding a column in A and B as a new column in C.<br>
         * C(:,end+1) = &alpha;*A(:,colA) + &beta;*B(:,colB)
         *
         * @param alpha scalar
         * @param A matrix
         * @param colA column in A
         * @param beta scalar
         * @param B matrix
         * @param colB column in B
         * @param C Column in C
         * @param gw workspace
         */
        public static void addColAppend(double alpha, DMatrixSparseCSC A, int colA, double beta, DMatrixSparseCSC B,
            int colB,
            DMatrixSparseCSC C, IGrowArray gw)
        {
            if (A.numRows != B.numRows || A.numRows != C.numRows)
                throw new ArgumentException("Number of rows in A, B, and C do not match");

            int idxA0 = A.col_idx[colA];
            int idxA1 = A.col_idx[colA + 1];
            int idxB0 = B.col_idx[colB];
            int idxB1 = B.col_idx[colB + 1];

            C.growMaxColumns(++C.numCols, true);
            C.growMaxLength(C.nz_length + idxA1 - idxA0 + idxB1 - idxB0, true);

            int[] w = TriangularSolver_DSCC.adjust(gw, A.numRows);
            //Arrays.fill(w, 0, A.numRows, -1);
            for (var i = 0; i < A.numRows; i++)
                w[i] = -1;

            for (int i = idxA0; i < idxA1; i++)
            {
                int row = A.nz_rows[i];
                C.nz_rows[C.nz_length] = row;
                C.nz_values[C.nz_length] = alpha * A.nz_values[i];
                w[row] = C.nz_length++;
            }

            for (int i = idxB0; i < idxB1; i++)
            {
                int row = B.nz_rows[i];
                if (w[row] != -1)
                {
                    C.nz_values[w[row]] += beta * B.nz_values[i];
                }
                else
                {
                    C.nz_values[C.nz_length] = beta * B.nz_values[i];
                    C.nz_rows[C.nz_length++] = row;
                }
            }
            C.col_idx[C.numCols] = C.nz_length;
        }

        /**
         * Performs element-wise multiplication:<br>
         * C_ij = A_ij * B_ij
         *
         * @param A (Input) Matrix
         * @param B (Input) Matrix
         * @param C (Output) matrix.
         * @param gw (Optional) Storage for internal workspace.  Can be null.
         * @param gx (Optional) Storage for internal workspace.  Can be null.
         */
        public static void elementMult(DMatrixSparseCSC A, DMatrixSparseCSC B, DMatrixSparseCSC C,
            IGrowArray gw, DGrowArray gx)
        {
            double[] x = TriangularSolver_DSCC.adjust(gx, A.numRows);
            int[] w = TriangularSolver_DSCC.adjust(gw, A.numRows);

            //Arrays.fill(w, 0, A.numRows, -1); // fill with -1. This will be a value less than column
            for (var i = 0; i < A.numRows; i++)
                w[i] = -1;

            C.indicesSorted = false; // Hmm I think if B is storted then C will be sorted...
            C.nz_length = 0;

            for (int col = 0; col < A.numCols; col++)
            {
                int idxA0 = A.col_idx[col];
                int idxA1 = A.col_idx[col + 1];
                int idxB0 = B.col_idx[col];
                int idxB1 = B.col_idx[col + 1];

                // compute the maximum number of elements that there can be in this row
                int maxInRow = Math.Min(idxA1 - idxA0, idxB1 - idxB0);

                // make sure there are enough non-zero elements in C
                if (C.nz_length + maxInRow > C.nz_values.Length)
                    C.growMaxLength(C.nz_values.Length + maxInRow, true);

                // update the structure of C
                C.col_idx[col] = C.nz_length;

                // mark the rows that appear in A and save their value
                for (int i = idxA0; i < idxA1; i++)
                {
                    int row = A.nz_rows[i];
                    w[row] = col;
                    x[row] = A.nz_values[i];
                }

                // If a row appears in A and B, multiply and set as an element in C
                for (int i = idxB0; i < idxB1; i++)
                {
                    int row = B.nz_rows[i];
                    if (w[row] == col)
                    {
                        C.nz_values[C.nz_length] = x[row] * B.nz_values[i];
                        C.nz_rows[C.nz_length++] = row;
                    }
                }
            }
            C.col_idx[C.numCols] = C.nz_length;
        }

        public static void removeZeros(DMatrixSparseCSC input, DMatrixSparseCSC output, double tol)
        {
            output.reshape(input.numRows, input.numCols, input.nz_length);
            output.nz_length = 0;

            for (int i = 0; i < input.numCols; i++)
            {
                output.col_idx[i] = output.nz_length;

                int idx0 = input.col_idx[i];
                int idx1 = input.col_idx[i + 1];

                for (int j = idx0; j < idx1; j++)
                {
                    double val = input.nz_values[j];
                    if (Math.Abs(val) > tol)
                    {
                        output.nz_rows[output.nz_length] = input.nz_rows[j];
                        output.nz_values[output.nz_length++] = val;
                    }
                }
            }
            output.col_idx[output.numCols] = output.nz_length;
        }

        public static void removeZeros(DMatrixSparseCSC A, double tol)
        {

            int offset = 0;
            for (int i = 0; i < A.numCols; i++)
            {
                int idx0 = A.col_idx[i] + offset;
                int idx1 = A.col_idx[i + 1];

                for (int j = idx0; j < idx1; j++)
                {
                    double val = A.nz_values[j];
                    if (Math.Abs(val) > tol)
                    {
                        A.nz_rows[j - offset] = A.nz_rows[j];
                        A.nz_values[j - offset] = val;
                    }
                    else
                    {
                        offset++;
                    }
                }
                A.col_idx[i + 1] -= offset;
            }
            A.nz_length -= offset;
        }
    }
}