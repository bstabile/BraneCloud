using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Factory;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.Bidiagonal
{
    //package org.ejml.dense.row.decomposition.bidiagonal;

/**
 * <p>
 * {@link BidiagonalDecomposition_F64} specifically designed for tall matrices.
 * First step is to perform QR decomposition on the input matrix.  Then R is decomposed using
 * a bidiagonal decomposition.  By performing the bidiagonal decomposition on the smaller matrix
 * computations can be saved if m/n &gt; 5/3 and if U is NOT needed.
 * </p>
 *
 * <p>
 * A = [Q<sub>1</sub> Q<sub>2</sub>][U1 0; 0 I] [B1;0] V<sup>T</sup><br>
 * U=[Q<sub>1</sub>*U1 Q<sub>2</sub>]<br>
 * B=[B1;0]<br>
 * A = U*B*V<sup>T</sup>
 * </p>
 *
 * <p>
 * A QRP decomposition is used internally.  That decomposition relies an a fixed threshold for selecting singular
 * values and is known to be less stable than SVD.  There is the potential for a degregation of stability
 * by using BidiagonalDecompositionTall instead of BidiagonalDecomposition_F64. A few simple tests have shown
 * that loss in stability to be insignificant.
 * </p>
 *
 * <p>
 * See page 404 in "Fundamentals of Matrix Computations", 2nd by David S. Watkins.
 * </p>
 *
 *
 * @author Peter Abeles
 */
// TODO optimize this code
    public class BidiagonalDecompositionTall_DDRM : BidiagonalDecomposition_F64<DMatrixRMaj>
    {
        QRPDecomposition_F64<DMatrixRMaj> decompQRP = DecompositionFactory_DDRM.qrp(500, 100)
            ; // todo this should be passed in

        BidiagonalDecomposition_F64<DMatrixRMaj> decompBi = new BidiagonalDecompositionRow_DDRM();

        DMatrixRMaj B = new DMatrixRMaj(1, 1);

        // number of rows
        int m;

        // number of column
        int n;

        // min(m,n)
        int min;

        public virtual void getDiagonal(double[] diag, double[] off)
        {
            diag[0] = B.get(0);
            for (int i = 1; i < n; i++)
            {
                diag[i] = B.unsafe_get(i, i);
                off[i - 1] = B.unsafe_get(i - 1, i);
            }
        }

        public virtual DMatrixRMaj getB(DMatrixRMaj B, bool compact)
        {
            B = BidiagonalDecompositionRow_DDRM.handleB(B, compact, m, n, min);

            B.set(0, 0, this.B.get(0, 0));
            for (int i = 1; i < min; i++)
            {
                B.set(i, i, this.B.get(i, i));
                B.set(i - 1, i, this.B.get(i - 1, i));
            }
            if (n > m)
                B.set(min - 1, min, this.B.get(min - 1, min));

            return B;
        }

        public virtual DMatrixRMaj getU(DMatrixRMaj U, bool transpose, bool compact)
        {
            U = BidiagonalDecompositionRow_DDRM.handleU(U, false, compact, m, n, min);

            if (compact)
            {
                // U = Q*U1
                DMatrixRMaj Q1 = decompQRP.getQ(null, true);
                DMatrixRMaj U1 = decompBi.getU(null, false, true);
                CommonOps_DDRM.mult(Q1, U1, U);
            }
            else
            {
                // U = [Q1*U1 Q2]
                DMatrixRMaj Q = decompQRP.getQ(U, false);
                DMatrixRMaj U1 = decompBi.getU(null, false, true);
                DMatrixRMaj Q1 = CommonOps_DDRM.extract(Q, 0, Q.numRows, 0, min);
                DMatrixRMaj tmp = new DMatrixRMaj(Q1.numRows, U1.numCols);
                CommonOps_DDRM.mult(Q1, U1, tmp);
                CommonOps_DDRM.insert(tmp, Q, 0, 0);
            }

            if (transpose)
                CommonOps_DDRM.transpose(U);

            return U;
        }

        public virtual DMatrixRMaj getV(DMatrixRMaj V, bool transpose, bool compact)
        {
            return decompBi.getV(V, transpose, compact);
        }

        public virtual bool decompose(DMatrixRMaj orig)
        {

            if (!decompQRP.decompose(orig))
            {
                return false;
            }

            m = orig.numRows;
            n = orig.numCols;
            min = Math.Min(m, n);
            B.reshape(min, n, false);

            decompQRP.getR(B, true);

            // apply the column pivots.
            // TODO this is horribly inefficient
            DMatrixRMaj result = new DMatrixRMaj(min, n);
            DMatrixRMaj P = decompQRP.getColPivotMatrix(null);
            CommonOps_DDRM.multTransB(B, P, result);
            B.set(result);

            return decompBi.decompose(B);
        }

        public virtual bool inputModified()
        {
            return decompQRP.inputModified();
        }
    }
}