using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.QR
{
    //package org.ejml.dense.row.decomposition.qr;

/**
 * <p>
 * This variation of QR decomposition uses reflections to compute the Q matrix.
 * Each reflection uses a householder operations, hence its name.  To provide a meaningful solution
 * the original matrix must have full rank.  This is intended for processing of small to medium matrices.
 * </p>
 * <p>
 * Both Q and R are stored in the same m by n matrix.  Q is not stored directly, instead the u from
 * Q<sub>k</sub>=(I-&gamma;*u*u<sup>T</sup>) is stored.  Decomposition requires about 2n*m<sup>2</sup>-2m<sup>2</sup>/3 flops.
 * </p>
 *
 * <p>
 * See the QR reflections algorithm described in:<br>
 * David S. Watkins, "Fundamentals of Matrix Computations" 2nd Edition, 2002
 * </p>
 *
 * <p>
 * For the most part this is a straight forward implementation.  To improve performance on large matrices a column is writen to an array and the order
 * of some of the loops has been changed.  This will degrade performance noticeably on small matrices.  Since
 * it is unlikely that the QR decomposition would be a bottle neck when small matrices are involved only
 * one implementation is provided.
 * </p>
 *
 * @author Peter Abeles
 */
    public class QRDecompositionHouseholder_FDRM : QRDecomposition<FMatrixRMaj>
    {

        /**
         * Where the Q and R matrices are stored.  R is stored in the
         * upper triangular portion and Q on the lower bit.  Lower columns
         * are where u is stored.  Q_k = (I - gamma_k*u_k*u_k^T).
         */
        protected FMatrixRMaj QR;

        // used internally to store temporary data
        protected float[] u, v;

        // dimension of the decomposed matrices
        protected int numCols; // this is 'n'

        protected int numRows; // this is 'm'
        protected int minLength;

        protected float[] dataQR;

        // the computed gamma for Q_k matrix
        protected float[] gammas;

        // local variables
        protected float gamma;

        protected float tau;

        // did it encounter an error?
        protected bool error;

        public void setExpectedMaxSize(int numRows, int numCols)
        {
            error = false;

            this.numCols = numCols;
            this.numRows = numRows;
            minLength = Math.Min(numRows, numCols);
            int maxLength = Math.Max(numRows, numCols);

            if (QR == null)
            {
                QR = new FMatrixRMaj(numRows, numCols);
                u = new float[maxLength];
                v = new float[maxLength];
                gammas = new float[minLength];
            }
            else
            {
                QR.reshape(numRows, numCols, false);
            }

            dataQR = QR.data;

            if (u.Length < maxLength)
            {
                u = new float[maxLength];
                v = new float[maxLength];
            }

            if (gammas.Length < minLength)
            {
                gammas = new float[minLength];
            }
        }

        /**
         * Returns a single matrix which contains the combined values of Q and R.  This
         * is possible since Q is symmetric and R is upper triangular.
         *
         * @return The combined Q R matrix.
         */
        public FMatrixRMaj getQR()
        {
            return QR;
        }

        /**
         * Computes the Q matrix from the imformation stored in the QR matrix.  This
         * operation requires about 4(m<sup>2</sup>n-mn<sup>2</sup>+n<sup>3</sup>/3) flops.
         *
         * @param Q The orthogonal Q matrix.
         */
        //@Override
        public FMatrixRMaj getQ(FMatrixRMaj Q, bool compact)
        {
            if (compact)
            {
                if (Q == null)
                {
                    Q = CommonOps_FDRM.identity(numRows, minLength);
                }
                else
                {
                    if (Q.numRows != numRows || Q.numCols != minLength)
                    {
                        throw new ArgumentException("Unexpected matrix dimension.");
                    }
                    else
                    {
                        CommonOps_FDRM.setIdentity(Q);
                    }
                }
            }
            else
            {
                if (Q == null)
                {
                    Q = CommonOps_FDRM.identity(numRows);
                }
                else
                {
                    if (Q.numRows != numRows || Q.numCols != numRows)
                    {
                        throw new ArgumentException("Unexpected matrix dimension.");
                    }
                    else
                    {
                        CommonOps_FDRM.setIdentity(Q);
                    }
                }
            }

            for (int j = minLength - 1; j >= 0; j--)
            {
                u[j] = 1;
                for (int i = j + 1; i < numRows; i++)
                {
                    u[i] = QR.get(i, j);
                }
                QrHelperFunctions_FDRM.rank1UpdateMultR(Q, u, gammas[j], j, j, numRows, v);
            }

            return Q;
        }

        /**
         * Returns an upper triangular matrix which is the R in the QR decomposition.
         *
         * @param R An upper triangular matrix.
         * @param compact
         */
        //@Override
        public FMatrixRMaj getR(FMatrixRMaj R, bool compact)
        {
            if (R == null)
            {
                if (compact)
                {
                    R = new FMatrixRMaj(minLength, numCols);
                }
                else
                    R = new FMatrixRMaj(numRows, numCols);
            }
            else
            {
                if (compact)
                {
                    if (R.numCols != numCols || R.numRows != minLength)
                        throw new ArgumentException("Unexpected dimensions");
                }
                else
                {
                    if (R.numCols != numCols || R.numRows != numRows)
                        throw new ArgumentException("Unexpected dimensions");
                }

                for (int i = 0; i < R.numRows; i++)
                {
                    int min = Math.Min(i, R.numCols);
                    for (int j = 0; j < min; j++)
                    {
                        R.set(i, j, 0);
                    }
                }
            }

            for (int i = 0; i < minLength; i++)
            {
                for (int j = i; j < numCols; j++)
                {
                    float val = QR.get(i, j);
                    R.set(i, j, val);
                }
            }

            return R;
        }

        /**
         * <p>
         * In order to decompose the matrix 'A' it must have full rank.  'A' is a 'm' by 'n' matrix.
         * It requires about 2n*m<sup>2</sup>-2m<sup>2</sup>/3 flops.
         * </p>
         *
         * <p>
         * The matrix provided here can be of different
         * dimension than the one specified in the constructor.  It just has to be smaller than or equal
         * to it.
         * </p>
         */
        //@Override
        public bool decompose(FMatrixRMaj A)
        {
            commonSetup(A);

            for (int j = 0; j < minLength; j++)
            {
                householder(j);
                updateA(j);
            }

            return !error;
        }

        //@Override
        public bool inputModified()
        {
            return false;
        }

        /**
         * <p>
         * Computes the householder vector "u" for the first column of submatrix j.  Note this is
         * a specialized householder for this problem.  There is some protection against
         * overflow and underflow.
         * </p>
         * <p>
         * Q = I - &gamma;uu<sup>T</sup>
         * </p>
         * <p>
         * This function finds the values of 'u' and '&gamma;'.
         * </p>
         *
         * @param j Which submatrix to work off of.
         */
        protected void householder(int j)
        {
            // find the element with the largest absolute value in the column and make a copy
            int index = j + j * numCols;
            float max = 0;
            for (int i = j; i < numRows; i++)
            {

                float d = u[i] = dataQR[index];

                // absolute value of d
                if (d < 0) d = -d;
                if (max < d)
                {
                    max = d;
                }
                index += numCols;
            }

            if (max == 0.0f)
            {
                gamma = 0;
                error = true;
            }
            else
            {
                // compute the norm2 of the matrix, with each element
                // normalized by the max value to avoid overflow problems
                tau = 0;
                for (int i = j; i < numRows; i++)
                {
                    u[i] /= max;
                    float d = u[i];
                    tau += d * d;
                }
                tau = (float) Math.Sqrt(tau);

                if (u[j] < 0)
                    tau = -tau;

                float u_0 = u[j] + tau;
                gamma = u_0 / tau;
                for (int i = j + 1; i < numRows; i++)
                {
                    u[i] /= u_0;
                }
                u[j] = 1;
                tau *= max;
            }

            gammas[j] = gamma;
        }

        /**
         * <p>
         * Takes the results from the householder computation and updates the 'A' matrix.<br>
         * <br>
         * A = (I - &gamma;*u*u<sup>T</sup>)A
         * </p>
         *
         * @param w The submatrix.
         */
        protected void updateA(int w)
        {
            // much of the code below is equivalent to the rank1Update function
            // however, since &tau; has already been computed there is no need to
            // recompute it, saving a few multiplication operations
//        for( int i = w+1; i < numCols; i++ ) {
//            float val = 0;
//
//            for( int k = w; k < numRows; k++ ) {
//                val += u[k]*dataQR[k*numCols +i];
//            }
//            v[i] = gamma*val;
//        }

            // This is functionally the same as the above code but the order has been changed
            // to avoid jumping the cpu cache
            for (int i = w + 1; i < numCols; i++)
            {
                v[i] = u[w] * dataQR[w * numCols + i];
            }

            for (int k = w + 1; k < numRows; k++)
            {
                int indexQR = k * numCols + w + 1;
                for (int i = w + 1; i < numCols; i++)
                {
//                v[i] += u[k]*dataQR[k*numCols +i];
                    v[i] += u[k] * dataQR[indexQR++];
                }
            }

            for (int i = w + 1; i < numCols; i++)
            {
                v[i] *= gamma;
            }

            // end of reordered code

            for (int i = w; i < numRows; i++)
            {
                float valU = u[i];

                int indexQR = i * numCols + w + 1;
                for (int j = w + 1; j < numCols; j++)
                {
//                dataQR[i*numCols+j] -= valU*v[j];
                    dataQR[indexQR++] -= valU * v[j];
                }
            }

            if (w < numCols)
            {
                dataQR[w + w * numCols] = -tau;
            }

            // save the Q matrix in the lower portion of QR
            for (int i = w + 1; i < numRows; i++)
            {
                dataQR[w + i * numCols] = u[i];
            }
        }

        /**
         * This function performs sanity check on the input for decompose and sets up the QR matrix.
         *
         * @param A
         */
        protected void commonSetup(FMatrixRMaj A)
        {
            setExpectedMaxSize(A.numRows, A.numCols);

            QR.set(A);
        }

        public float[] getGammas()
        {
            return gammas;
        }

    }
}