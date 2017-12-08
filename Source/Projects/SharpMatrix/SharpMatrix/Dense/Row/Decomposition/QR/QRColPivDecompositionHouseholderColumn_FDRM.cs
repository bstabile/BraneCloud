using System;
using SharpMatrix.Data;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Row.Decomposition.QR
{
    //package org.ejml.dense.row.decomposition.qr;

/**
 * <p>
 * Performs QR decomposition with column pivoting.  To prevent overflow/underflow the whole matrix
 * is normalized by the max value, but columns are not normalized individually any more. To enable
 * code reuse it extends {@link QRDecompositionHouseholderColumn_FDRM} and functions from that class
 * are used whenever possible.  Columns are transposed into single arrays, which allow for
 * fast pivots.
 * </p>
 *
 * <p>
 * Decomposition: A*P = Q*R
 * </p>
 *
 * <p>
 * Based off the description in "Fundamentals of Matrix Computations", 2nd by David S. Watkins.
 * </p>
 *
 * @author Peter Abeles
 */
    public class QRColPivDecompositionHouseholderColumn_FDRM : QRDecompositionHouseholderColumn_FDRM
        , QRPDecomposition_F32<FMatrixRMaj>
    {
        // the ordering of each column, the current column i is the original column pivots[i]
        protected int[] pivots;

        // F-norm  squared for each column
        protected float[] normsCol;

        // value of the maximum abs element
        protected float maxAbs;

        // threshold used to determine when a column is considered to be singular
        // Threshold is relative to the maxAbs
        protected float singularThreshold = UtilEjml.F_EPS;

        // the matrix's rank
        protected int rank;

        /**
         * Configure parameters.
         *
         * @param singularThreshold The singular threshold.
         */
        public QRColPivDecompositionHouseholderColumn_FDRM(float singularThreshold)
        {
            this.singularThreshold = singularThreshold;
        }

        public QRColPivDecompositionHouseholderColumn_FDRM()
        {
        }

        public virtual void setSingularThreshold(float threshold)
        {
            this.singularThreshold = threshold;
        }

        public override void setExpectedMaxSize(int numRows, int numCols)
        {
            base.setExpectedMaxSize(numRows, numCols);

            if (pivots == null || pivots.Length < numCols)
            {
                pivots = new int[numCols];
                normsCol = new float[numCols];
            }
        }

        /**
         * Computes the Q matrix from the information stored in the QR matrix.  This
         * operation requires about 4(m<sup>2</sup>n-mn<sup>2</sup>+n<sup>3</sup>/3) flops.
         *
         * @param Q The orthogonal Q matrix.
         */
        public override FMatrixRMaj getQ(FMatrixRMaj Q, bool compact)
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

            for (int j = rank - 1; j >= 0; j--)
            {
                float[] u = dataQR[j];

                float vv = u[j];
                u[j] = 1;
                QrHelperFunctions_FDRM.rank1UpdateMultR(Q, u, gammas[j], j, j, numRows, v);
                u[j] = vv;
            }

            return Q;
        }

        /**
         * <p>
         * To decompose the matrix 'A' it must have full rank.  'A' is a 'm' by 'n' matrix.
         * It requires about 2n*m<sup>2</sup>-2m<sup>2</sup>/3 flops.
         * </p>
         *
         * <p>
         * The matrix provided here can be of different
         * dimension than the one specified in the constructor.  It just has to be smaller than or equal
         * to it.
         * </p>
         */
        public override bool decompose(FMatrixRMaj A)
        {
            setExpectedMaxSize(A.numRows, A.numCols);

            convertToColumnMajor(A);

            maxAbs = CommonOps_FDRM.elementMaxAbs(A);
            // initialize pivot variables
            setupPivotInfo();

            // go through each column and perform the decomposition
            for (int j = 0; j < minLength; j++)
            {
                if (j > 0)
                    updateNorms(j);
                swapColumns(j);
                // if its degenerate stop processing
                if (!householderPivot(j))
                    break;
                updateA(j);
                rank = j + 1;
            }

            return true;
        }

        /**
         * Sets the initial pivot ordering and compute the F-norm squared for each column
         */
        private void setupPivotInfo()
        {
            for (int col = 0; col < numCols; col++)
            {
                pivots[col] = col;
                float[] c = dataQR[col];
                float norm = 0;
                for (int row = 0; row < numRows; row++)
                {
                    float element = c[row];
                    norm += element * element;
                }
                normsCol[col] = norm;
            }
        }


        /**
         * Performs an efficient update of each columns' norm
         */
        private void updateNorms(int j)
        {
            bool foundNegative = false;
            for (int col = j; col < numCols; col++)
            {
                float e = dataQR[col][j - 1];
                normsCol[col] -= e * e;

                if (normsCol[col] < 0)
                {
                    foundNegative = true;
                    break;
                }
            }

            // if a negative sum has been found then clearly too much precision has been last
            // and it should recompute the column norms from scratch
            if (foundNegative)
            {
                for (int col = j; col < numCols; col++)
                {
                    float[] u = dataQR[col];
                    float actual = 0;
                    for (int i = j; i < numRows; i++)
                    {
                        float v = u[i];
                        actual += v * v;
                    }
                    normsCol[col] = actual;
                }
            }
        }

        /**
         * Finds the column with the largest normal and makes that the first column
         *
         * @param j Current column being inspected
         */
        private void swapColumns(int j)
        {

            // find the column with the largest norm
            int largestIndex = j;
            float largestNorm = normsCol[j];
            for (int col = j + 1; col < numCols; col++)
            {
                float n = normsCol[col];
                if (n > largestNorm)
                {
                    largestNorm = n;
                    largestIndex = col;
                }
            }
            // swap the columns
            float[] tempC = dataQR[j];
            dataQR[j] = dataQR[largestIndex];
            dataQR[largestIndex] = tempC;
            float tempN = normsCol[j];
            normsCol[j] = normsCol[largestIndex];
            normsCol[largestIndex] = tempN;
            int tempP = pivots[j];
            pivots[j] = pivots[largestIndex];
            pivots[largestIndex] = tempP;
        }

        /**
         * <p>
         * Computes the householder vector "u" for the first column of submatrix j. The already computed
         * norm is used and checks to see if the matrix is singular at this point.
         * </p>
         * <p>
         * Q = I - &gamma;uu<sup>T</sup>
         * </p>
         * <p>
         * This function finds the values of 'u' and '&gamma;'.
         * </p>
         *
         * @param j Which submatrix to work off of.
         * @return false if it is degenerate
         */
        protected bool householderPivot(int j)
        {
            float[] u = dataQR[j];

            // find the largest value in this column
            // this is used to normalize the column and mitigate overflow/underflow
            float max = QrHelperFunctions_FDRM.findMax(u, j, numRows - j);

            if (max <= 0)
            {
                return false;
            }
            else
            {
                // computes tau and normalizes u by max
                tau = QrHelperFunctions_FDRM.computeTauAndDivide(j, numRows, u, max);

                // divide u by u_0
                float u_0 = u[j] + tau;
                QrHelperFunctions_FDRM.divideElements(j + 1, numRows, u, u_0);

                gamma = u_0 / tau;
                tau *= max;

                u[j] = -tau;

                if (Math.Abs(tau) <= singularThreshold)
                {
                    return false;
                }
            }

            gammas[j] = gamma;

            return true;
        }

        public virtual int getRank()
        {
            return rank;
        }

        public virtual int[] getColPivots()
        {
            return pivots;
        }

        public virtual FMatrixRMaj getColPivotMatrix(FMatrixRMaj P)
        {
            if (P == null)
                P = new FMatrixRMaj(numCols, numCols);
            else if (P.numRows != numCols)
                throw new ArgumentException("Number of rows must be " + numCols);
            else if (P.numCols != numCols)
                throw new ArgumentException("Number of columns must be " + numCols);
            else
            {
                P.zero();
            }

            for (int i = 0; i < numCols; i++)
            {
                P.set(pivots[i], i, 1);
            }

            return P;
        }
    }
}