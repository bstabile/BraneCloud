using System;
using SharpMatrix.Data;
using SharpMatrix.Interfaces.Decomposition;
using SharpMatrix.Sparse.Csc.Misc;

namespace SharpMatrix.Sparse.Csc.Decomposition.Chol
{
    //package org.ejml.sparse.csc.decomposition.chol;

/**
 * Performs a Cholesky decomposition using an up looking algorthm on a {@link DMatrixSparseCSC}.
 *
 * <p>See page 59 in "Direct Methods for Sparse Linear Systems" by Tomothy A. Davis</p>
 *
 * @author Peter Abeles
 */
    public class CholeskyUpLooking_DSCC : CholeskySparseDecomposition_F64<DMatrixSparseCSC>
    {
        private int N;

        // storage for decomposition
        DMatrixSparseCSC L = new DMatrixSparseCSC(1, 1, 0);

        // workspace storage
        IGrowArray gw = new IGrowArray(1);

        IGrowArray gs = new IGrowArray(1);
        DGrowArray gx = new DGrowArray(1);
        int[] parent = new int[1];
        int[] post = new int[1];
        int[] counts = new int[1];
        ColumnCounts_DSCC columnCounter = new ColumnCounts_DSCC(false);

        // true if it has successfully decomposed a matrix
        private bool decomposed = false;

        // if true then the structure is locked and won't be computed again
        private bool locked = false;

        //@Override
        public bool decompose(DMatrixSparseCSC orig)
        {
            if (orig.numCols != orig.numRows)
                throw new ArgumentException("Must be a square matrix");

            if (!locked || !decomposed)
                performSymbolic(orig);

            if (performDecomposition(orig))
            {
                decomposed = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void performSymbolic(DMatrixSparseCSC A)
        {
            init(A.numCols);

            TriangularSolver_DSCC.eliminationTree(A, false, parent, gw);
            TriangularSolver_DSCC.postorder(parent, N, post, gw);
            columnCounter.process(A, parent, post, counts);
            L.reshape(A.numRows, A.numCols, 0);
            L.colsum(counts);
        }

        private void init(int N)
        {
            this.N = N;
            if (parent.Length < N)
            {
                parent = new int[N];
                post = new int[N];
                counts = new int[N];
                gw.reshape(3 * N);
            }
        }

        private bool performDecomposition(DMatrixSparseCSC A)
        {
            int[] c = TriangularSolver_DSCC.adjust(gw, N);
            int[] s = TriangularSolver_DSCC.adjust(gs, N);
            double[] x = TriangularSolver_DSCC.adjust(gx, N);

            Array.Copy(L.col_idx, 0, c, 0, N);

            for (int k = 0; k < N; k++)
            {
                //----  Nonzero pattern of L(k,:)
                int top = TriangularSolver_DSCC.searchNzRowsElim(A, k, parent, s, c);

                // x(0:k) is now zero
                x[k] = 0;
                int idx0 = A.col_idx[k];
                int idx1 = A.col_idx[k + 1];

                // x = full(triu(C(:,k)))
                for (int q = idx0; q < idx1; q++)
                {
                    if (A.nz_rows[q] <= k)
                    {
                        x[A.nz_rows[q]] = A.nz_values[q];
                    }
                }
                double d = x[k]; // d = C(k,k)
                x[k] = 0; // clear x for k+1 iteration

                //---- Triangular Solve
                for (; top < N; top++)
                {
                    int i = s[top];
                    double lki = x[i] / L.nz_values[L.col_idx[i]]; // L(k,i) = x(i) / L(i,i)
                    x[i] = 0;
                    for (int r = L.col_idx[i] + 1; r < c[i]; r++)
                    {
                        x[L.nz_rows[r]] -= L.nz_values[r] * lki;
                    }
                    d -= lki * lki; // d = d - L(k,i)**L(k,i)
                    int t = c[i]++;
                    L.nz_rows[t] = k; // store L(k,i) in column i
                    L.nz_values[t] = lki;
                }

                //----- Compute L(k,k)
                if (d <= 0)
                {
                    // it's not positive definite
                    return false;
                }
                int p = c[k]++;
                L.nz_rows[p] = k;
                L.nz_values[p] = Math.Sqrt(d);
            }

            return true;
        }

        //@Override
        public bool inputModified()
        {
            return false;
        }

        //@Override
        public bool isLower()
        {
            return true;
        }

        //@Override
        public DMatrixSparseCSC getT(DMatrixSparseCSC T)
        {
            if (T == null)
            {
                T = new DMatrixSparseCSC(L.numRows, L.numCols, L.nz_length);
            }
            T.set(L);
            return T;
        }

        //@Override
        public Complex_F64 computeDeterminant()
        {
            double value = 1;
            for (int i = 0; i < N; i++)
            {
                value *= L.nz_values[L.col_idx[i]];
            }
            return new Complex_F64(value * value, 0);
        }

        public DGrowArray getGx()
        {
            return gx;
        }


        public DMatrixSparseCSC getL()
        {
            return L;
        }

        public IGrowArray getGw()
        {
            return gw;
        }

        //@Override
        public void lockStructure()
        {
            if (locked)
                return;
            this.locked = true;
        }

        //@Override
        public bool isStructureLocked()
        {
            return locked;
        }
    }
}