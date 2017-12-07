using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.Bidiagonal;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.Svd.ImplicitQR;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.Svd
{
    //package org.ejml.dense.row.decomposition.svd;

/**
 * <p>
 * Computes the Singular value decomposition of a matrix using the implicit QR algorithm
 * for singular value decomposition.  It works by first by transforming the matrix
 * to a bidiagonal A=U*B*V<sup>T</sup> form, then it implicitly computing the eigenvalues of the B<sup>T</sup>B matrix,
 * which are the same as the singular values in the original A matrix.
 * </p>
 *
 * <p>
 * Based off of the description provided in:<br>
 * <br>
 * David S. Watkins, "Fundamentals of Matrix Computations," Second Edition. Page 404-411
 * </p>
 *
 * @author Peter Abeles
 */
    public class SvdImplicitQrDecompose_FDRM : SingularValueDecomposition_F32<FMatrixRMaj>
    {

        private int _numRows;
        private int _numCols;

        // dimensions of transposed matrix
        private int _numRowsT;

        private int _numColsT;

        // if true then it can use the special Bidiagonal decomposition
        private bool canUseTallBidiagonal;

        // If U is not being computed and the input matrix is 'tall' then a special bidiagonal decomposition
        // can be used which is faster.
        private BidiagonalDecomposition_F32<FMatrixRMaj> bidiag;

        private SvdImplicitQrAlgorithm_FDRM qralg = new SvdImplicitQrAlgorithm_FDRM();

        float[] diag;
        float[] off;

        private FMatrixRMaj Ut;
        private FMatrixRMaj Vt;

        private float[] singularValues;
        private int numSingular;

        // compute a compact SVD
        private bool compact;

        // What is actually computed
        private bool computeU;

        private bool computeV;

        // What the user requested to be computed
        // If the transpose is computed instead then what is actually computed is swapped
        private bool prefComputeU;

        private bool prefComputeV;

        // Should it compute the transpose instead
        private bool transposed;

        // Either a copy of the input matrix or a copy of it transposed
        private FMatrixRMaj A_mod = new FMatrixRMaj(1, 1);

        /**
         * Configures the class
         *
         * @param compact Compute a compact SVD
         * @param computeU If true it will compute the U matrix
         * @param computeV If true it will compute the V matrix
         * @param canUseTallBidiagonal If true then it can choose to use a tall Bidiagonal decomposition to improve runtime performance.
         */
        public SvdImplicitQrDecompose_FDRM(bool compact, bool computeU, bool computeV,
            bool canUseTallBidiagonal)
        {
            this.compact = compact;
            this.prefComputeU = computeU;
            this.prefComputeV = computeV;
            this.canUseTallBidiagonal = canUseTallBidiagonal;
        }

        //@Override
        public float[] getSingularValues()
        {
            return singularValues;
        }

        //@Override
        public int numberOfSingularValues()
        {
            return numSingular;
        }

        //@Override
        public bool isCompact()
        {
            return compact;
        }

        //@Override
        public FMatrixRMaj getU(FMatrixRMaj U, bool transpose)
        {
            if (!prefComputeU)
                throw new ArgumentException("As requested U was not computed.");
            if (transpose)
            {
                if (U == null)
                    return Ut;
                U.set(Ut);
            }
            else
            {
                if (U == null)
                    U = new FMatrixRMaj(Ut.numCols, Ut.numRows);
                else
                    U.reshape(Ut.numCols, Ut.numRows);

                CommonOps_FDRM.transpose(Ut, U);
            }

            return U;
        }

        //@Override
        public FMatrixRMaj getV(FMatrixRMaj V, bool transpose)
        {
            if (!prefComputeV)
                throw new ArgumentException("As requested V was not computed.");
            if (transpose)
            {
                if (V == null)
                    return Vt;

                V.set(Vt);
            }
            else
            {
                if (V == null)
                    V = new FMatrixRMaj(Vt.numCols, Vt.numRows);
                else
                    V.reshape(Vt.numCols, Vt.numRows);

                CommonOps_FDRM.transpose(Vt, V);
            }

            return V;
        }

        //@Override
        public FMatrixRMaj getW(FMatrixRMaj W)
        {
            int m = compact ? numSingular : _numRows;
            int n = compact ? numSingular : _numCols;

            if (W == null)
                W = new FMatrixRMaj(m, n);
            else
            {
                W.reshape(m, n, false);
                W.zero();
            }

            for (int i = 0; i < numSingular; i++)
            {
                W.unsafe_set(i, i, singularValues[i]);
            }

            return W;
        }

        //@Override
        public bool decompose(FMatrixRMaj orig)
        {
            if (!setup(orig))
                return false;

            if (bidiagonalization(orig))
                return false;

            if (computeUWV())
                return false;

            // make sure all the singular values or positive
            makeSingularPositive();

            // if transposed undo the transposition
            undoTranspose();

            return true;
        }

        //@Override
        public bool inputModified()
        {
            return false;
        }

        private bool bidiagonalization(FMatrixRMaj orig)
        {
            // change the matrix to bidiagonal form
            if (transposed)
            {
                A_mod.reshape(orig.numCols, orig.numRows, false);
                CommonOps_FDRM.transpose(orig, A_mod);
            }
            else
            {
                A_mod.reshape(orig.numRows, orig.numCols, false);
                A_mod.set(orig);
            }
            return !bidiag.decompose(A_mod);
        }

        /**
         * If the transpose was computed instead do some additional computations
         */
        private void undoTranspose()
        {
            if (transposed)
            {
                FMatrixRMaj temp = Vt;
                Vt = Ut;
                Ut = temp;
            }
        }

        /**
         * Compute singular values and U and V at the same time
         */
        private bool computeUWV()
        {
            bidiag.getDiagonal(diag, off);
            qralg.setMatrix(_numRowsT, _numColsT, diag, off);

//        long pointA = System.currentTimeMillis();
            // compute U and V matrices
            if (computeU)
                Ut = bidiag.getU(Ut, true, compact);
            if (computeV)
                Vt = bidiag.getV(Vt, true, compact);

            qralg.setFastValues(false);
            if (computeU)
                qralg.setUt(Ut);
            else
                qralg.setUt(null);
            if (computeV)
                qralg.setVt(Vt);
            else
                qralg.setVt(null);

//        long pointB = System.currentTimeMillis();

            bool ret = !qralg.process();

//        long pointC = System.currentTimeMillis();
//        Console.WriteLine("  compute UV "+(pointB-pointA)+"  QR = "+(pointC-pointB));

            return ret;
        }

        private bool setup(FMatrixRMaj orig)
        {
            transposed = orig.numCols > orig.numRows;

            // flag what should be computed and what should not be computed
            if (transposed)
            {
                computeU = prefComputeV;
                computeV = prefComputeU;
                _numRowsT = orig.numCols;
                _numColsT = orig.numRows;
            }
            else
            {
                computeU = prefComputeU;
                computeV = prefComputeV;
                _numRowsT = orig.numRows;
                _numColsT = orig.numCols;
            }

            _numRows = orig.numRows;
            _numCols = orig.numCols;

            if (_numRows == 0 || _numCols == 0)
                return false;

            if (diag == null || diag.Length < _numColsT)
            {
                diag = new float[_numColsT];
                off = new float[_numColsT - 1];
            }

            // if it is a tall matrix and U is not needed then there is faster decomposition algorithm
            if (canUseTallBidiagonal && _numRows > _numCols * 2 && !computeU)
            {
                if (bidiag == null || !(bidiag is BidiagonalDecompositionTall_FDRM))
                {
                    bidiag = new BidiagonalDecompositionTall_FDRM();
                }
            }
            else if (bidiag == null || !(bidiag is BidiagonalDecompositionRow_FDRM))
            {
                bidiag = new BidiagonalDecompositionRow_FDRM();
            }

            return true;
        }

        /**
         * With the QR algorithm it is possible for the found singular values to be negative.  This
         * makes them all positive by multiplying it by a diagonal matrix that has
         */
        private void makeSingularPositive()
        {
            numSingular = qralg.getNumberOfSingularValues();
            singularValues = qralg.getSingularValues();

            for (int i = 0; i < numSingular; i++)
            {
                float val = qralg.getSingularValue(i);

                if (val < 0)
                {
                    singularValues[i] = 0.0f - val;

                    if (computeU)
                    {
                        // compute the results of multiplying it by an element of -1 at this location in
                        // a diagonal matrix.
                        int start = i * Ut.numCols;
                        int stop = start + Ut.numCols;

                        for (int j = start; j < stop; j++)
                        {
                            Ut.set(j, 0.0f - Ut.get(j));
                        }
                    }
                }
                else
                {
                    singularValues[i] = val;
                }
            }
        }

        //@Override
        public int numRows()
        {
            return _numRows;
        }

        //@Override
        public int numCols()
        {
            return _numCols;
        }
    }
}