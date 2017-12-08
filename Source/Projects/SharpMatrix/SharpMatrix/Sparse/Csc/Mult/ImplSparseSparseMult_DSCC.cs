using System;
using SharpMatrix.Data;

namespace SharpMatrix.Sparse.Csc.Misc
{
    //package org.ejml.sparse.csc.mult;

/**
 * @author Peter Abeles
 */
    public class ImplSparseSparseMult_DSCC
    {

        /**
         * Performs matrix multiplication.  C = A*B
         *
         * @param A Matrix
         * @param B Matrix
         * @param C Storage for results.  Data length is increased if increased if insufficient.
         * @param gw (Optional) Storage for internal workspace.  Can be null.
         * @param gx (Optional) Storage for internal workspace.  Can be null.
         */
        public static void mult(DMatrixSparseCSC A, DMatrixSparseCSC B, DMatrixSparseCSC C,
            IGrowArray gw, DGrowArray gx)
        {
            double[] x = TriangularSolver_DSCC.adjust(gx, A.numRows);
            int[] w = TriangularSolver_DSCC.adjust(gw, A.numRows, A.numRows);

            C.growMaxLength(A.nz_length + B.nz_length, false);
            C.indicesSorted = false;
            C.nz_length = 0;

            // C(i,j) = sum_k A(i,k) * B(k,j)
            int idx0 = B.col_idx[0];
            for (int bj = 1; bj <= B.numCols; bj++)
            {
                int colB = bj - 1;
                int idx1 = B.col_idx[bj];
                C.col_idx[bj] = C.nz_length;

                if (idx0 == idx1)
                {
                    continue;
                }

                // C(:,j) = sum_k A(:,k)*B(k,j)
                for (int bi = idx0; bi < idx1; bi++)
                {
                    int rowB = B.nz_rows[bi];
                    double valB = B.nz_values[bi]; // B(k,j)  k=rowB j=colB

                    multAddColA(A, rowB, valB, C, colB + 1, x, w);
                }

                // take the values in the dense vector 'x' and put them into 'C'
                int idxC0 = C.col_idx[colB];
                int idxC1 = C.col_idx[colB + 1];

                for (int i = idxC0; i < idxC1; i++)
                {
                    C.nz_values[i] = x[C.nz_rows[i]];
                }

                idx0 = idx1;
            }

        }

        /**
         * Performs matrix multiplication.  C = A<sup>T</sup></sup>*B
         *
         * @param A Matrix
         * @param B Matrix
         * @param C Storage for results.  Data length is increased if increased if insufficient.
         * @param gw (Optional) Storage for internal workspace.  Can be null.
         * @param gx (Optional) Storage for internal workspace.  Can be null.
         */
        public static void multTransA(DMatrixSparseCSC A, DMatrixSparseCSC B, DMatrixSparseCSC C,
            IGrowArray gw, DGrowArray gx)
        {
            double[] x = TriangularSolver_DSCC.adjust(gx, A.numRows);
            int[] w = TriangularSolver_DSCC.adjust(gw, A.numRows, A.numRows);

            C.growMaxLength(A.nz_length + B.nz_length, false);
            C.indicesSorted = true;
            C.nz_length = 0;
            C.col_idx[0] = 0;

            int idxB0 = B.col_idx[0];
            for (int bj = 1; bj <= B.numCols; bj++)
            {
                int idxB1 = B.col_idx[bj];
                C.col_idx[bj] = C.nz_length;

                if (idxB0 == idxB1)
                {
                    continue;
                }

                // convert the column of B into a dense format and mark which rows are used
                for (int bi = idxB0; bi < idxB1; bi++)
                {
                    int rowB = B.nz_rows[bi];
                    x[rowB] = B.nz_values[bi];
                    w[rowB] = bj;
                }

                // C(colA,colB) = A(:,colA)*B(:,colB)
                for (int colA = 0; colA < A.numCols; colA++)
                {
                    int idxA0 = A.col_idx[colA];
                    int idxA1 = A.col_idx[colA + 1];

                    double sum = 0;
                    for (int ai = idxA0; ai < idxA1; ai++)
                    {
                        int rowA = A.nz_rows[ai];
                        if (w[rowA] == bj)
                        {
                            sum += x[rowA] * A.nz_values[ai];
                        }
                    }

                    if (sum != 0)
                    {
                        if (C.nz_length == C.nz_values.Length)
                        {
                            C.growMaxLength(C.nz_length * 2 + 1, true);
                        }
                        C.nz_values[C.nz_length] = sum;
                        C.nz_rows[C.nz_length++] = colA;
                    }
                }
                C.col_idx[bj] = C.nz_length;
                idxB0 = idxB1;
            }
        }

        /**
         * Performs matrix multiplication.  C = A*B<sup>T</sup></sup>
         *
         * @param A Matrix
         * @param B Matrix
         * @param C Storage for results.  Data length is increased if increased if insufficient.
         * @param gw (Optional) Storage for internal workspace.  Can be null.
         * @param gx (Optional) Storage for internal workspace.  Can be null.
         */
        public static void multTransB(DMatrixSparseCSC A, DMatrixSparseCSC B, DMatrixSparseCSC C,
            IGrowArray gw, DGrowArray gx)
        {
            if (!B.isIndicesSorted())
                throw new ArgumentException("B must have its indices sorted.");
            else if (!CommonOps_DSCC.checkIndicesSorted(B))
            {
                throw new ArgumentException("Crap. Not really sorted");
            }

            double[] x = TriangularSolver_DSCC.adjust(gx, A.numRows);
            int[] w = TriangularSolver_DSCC.adjust(gw, A.numRows + B.numCols, A.numRows);

            C.growMaxLength(A.nz_length + B.nz_length, false);
            C.indicesSorted = false;
            C.nz_length = 0;
            C.col_idx[0] = 0;

            // initialize w is the first index in each column of B
            int locationB = A.numRows;
            Array.Copy(B.col_idx, 0, w, locationB, B.numCols);

            for (int colC = 0; colC < B.numRows; colC++)
            {
                C.col_idx[colC + 1] = C.nz_length; // needs a value of B has nothing in the row

                // find the column in the transposed B
                int mark = colC + 1;
                for (int colB = 0; colB < B.numCols; colB++)
                {
                    int bi = w[locationB + colB];
                    if (bi < B.col_idx[colB + 1])
                    {
                        int row = B.nz_rows[bi];
                        if (row == colC)
                        {
                            multAddColA(A, colB, B.nz_values[bi], C, mark, x, w);
                            w[locationB + colB]++;
                        }
                    }
                }

                // take the values in the dense vector 'x' and put them into 'C'
                int idxC0 = C.col_idx[colC];
                int idxC1 = C.col_idx[colC + 1];

                for (int i = idxC0; i < idxC1; i++)
                {
                    C.nz_values[i] = x[C.nz_rows[i]];
                }
            }
        }

        /**
         * Performs the performing operation x = x + A(:,i)*alpha
         *
         * <p>NOTE: This is the same as cs_scatter() in csparse.</p>
         */
        public static void multAddColA(DMatrixSparseCSC A, int colA,
            double alpha,
            DMatrixSparseCSC C, int mark,
            double[] x, int[] w)
        {
            int idxA0 = A.col_idx[colA];
            int idxA1 = A.col_idx[colA + 1];

            for (int j = idxA0; j < idxA1; j++)
            {
                int row = A.nz_rows[j];

                if (w[row] < mark)
                {
                    if (C.nz_length >= C.nz_rows.Length)
                    {
                        C.growMaxLength(C.nz_length * 2 + 1, true);
                    }

                    w[row] = mark;
                    C.nz_rows[C.nz_length] = row;
                    C.col_idx[mark] = ++C.nz_length;
                    x[row] = A.nz_values[j] * alpha;
                }
                else
                {
                    x[row] += A.nz_values[j] * alpha;
                }
            }
        }

        /**
         * Adds rows to C[*,colC] that are in A[*,colA] as long as they are marked in w. This is used to grow C
         * and colC must be the last filled in column in C.
         *
         * <p>NOTE: This is the same as cs_scatter if x is null.</p>
         * @param A Matrix
         * @param colA The column in A that is being examined
         * @param C Matrix
         * @param colC Column in C that rows in A are being added to.
         * @param w An array used to indicate if a row in A should be added to C. if w[i] < colC AND i is a row
         *          in A[*,colA] then it will be added.
         */
        public static void addRowsInAInToC(DMatrixSparseCSC A, int colA,
            DMatrixSparseCSC C, int colC,
            int[] w)
        {
            int idxA0 = A.col_idx[colA];
            int idxA1 = A.col_idx[colA + 1];

            for (int j = idxA0; j < idxA1; j++)
            {
                int row = A.nz_rows[j];

                if (w[row] < colC)
                {
                    if (C.nz_length >= C.nz_rows.Length)
                    {
                        C.growMaxLength(C.nz_length * 2 + 1, true);
                    }

                    w[row] = colC;
                    C.nz_rows[C.nz_length++] = row;
                }
            }
            C.col_idx[colC + 1] = C.nz_length;
        }

        public static void mult(DMatrixSparseCSC A, DMatrixRMaj B, DMatrixRMaj C)
        {

            C.zero();

            // C(i,j) = sum_k A(i,k) * B(k,j)
            for (int k = 0; k < A.numCols; k++)
            {
                int idx0 = A.col_idx[k];
                int idx1 = A.col_idx[k + 1];

                for (int indexA = idx0; indexA < idx1; indexA++)
                {
                    int i = A.nz_rows[indexA];
                    double valueA = A.nz_values[indexA];

                    int indexB = k * B.numCols;
                    int indexC = i * C.numCols;
                    int end = indexB + B.numCols;

//                for (int j = 0; j < B.numCols; j++) {
                    while (indexB < end)
                    {
                        C.data[indexC++] += valueA * B.data[indexB++];
                    }
                }
            }
        }

        /**
         * Computes the inner product of two column vectors taken from the input matrices.
         *
         * <p>dot = A(:,colA)'*B(:,colB)</p>
         *
         * @param A Matrix
         * @param colA Column in A
         * @param B Matrix
         * @param colB Column in B
         * @return Dot product
         */
        public static double dotInnerColumns(DMatrixSparseCSC A, int colA, DMatrixSparseCSC B, int colB,
            IGrowArray gw, DGrowArray gx)
        {
            if (A.numRows != B.numRows)
                throw new ArgumentException("Number of rows must match.");

            int[] w = TriangularSolver_DSCC.adjust(gw, A.numRows);
            //Arrays.fill(w,0,A.numRows,-1);
            for (var i = 0; i < A.numRows; i++)
                w[i] = -1;
            double[] x = TriangularSolver_DSCC.adjust(gx, A.numRows);

            int length = 0;

            int idx0 = A.col_idx[colA];
            int idx1 = A.col_idx[colA + 1];
            for (int i = idx0; i < idx1; i++)
            {
                int row = A.nz_rows[i];
                x[length] = A.nz_values[i];
                w[row] = length++;
            }

            double dot = 0;

            idx0 = B.col_idx[colB];
            idx1 = B.col_idx[colB + 1];
            for (int i = idx0; i < idx1; i++)
            {
                int row = B.nz_rows[i];
                if (w[row] != -1)
                {
                    dot += x[w[row]] * B.nz_values[i];
                }
            }

            return dot;
        }
    }
}