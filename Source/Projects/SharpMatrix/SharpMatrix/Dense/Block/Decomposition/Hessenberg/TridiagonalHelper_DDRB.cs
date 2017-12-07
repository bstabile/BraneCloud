using System;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Block.Decomposition.QR;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Block.Decomposition.Hessenberg
{

    //package org.ejml.dense.block.decomposition.hessenberg;

/**
 * @author Peter Abeles
 */
    public class TridiagonalHelper_DDRB
    {

        /**
         * <p>
         * Performs a tridiagonal decomposition on the upper row only.
         * </p>
         *
         * <p>
         * For each row 'a' in 'A':
         * Compute 'u' the householder reflector.
         * y(:) = A*u
         * v(i) = y - (1/2)*(y^T*u)*u
         * a(i+1) = a(i) - u*&gamma;*v^T - v*u^t
         * </p>
         *
         * @param blockLength Size of a block
         * @param A is the row block being decomposed.  Modified.
         * @param gammas Householder gammas.
         * @param V Where computed 'v' are stored in a row block.  Modified.
         */
        public static void tridiagUpperRow(int blockLength,
            DSubmatrixD1 A,
            double[] gammas,
            DSubmatrixD1 V)
        {
            int blockHeight = Math.Min(blockLength, A.row1 - A.row0);
            if (blockHeight <= 1)
                return;
            int width = A.col1 - A.col0;
            int num = Math.Min(width - 1, blockHeight);
            int applyIndex = Math.Min(width, blockHeight);

            // step through rows in the block
            for (int i = 0; i < num; i++)
            {
                // compute the new reflector and save it in a row in 'A'
                BlockHouseHolder_DDRB.computeHouseHolderRow(blockLength, A, gammas, i);
                double gamma = gammas[A.row0 + i];

                // compute y
                computeY(blockLength, A, V, i, gamma);

                // compute v from y
                computeRowOfV(blockLength, A, V, i, gamma);

                // Apply the reflectors to the next row in 'A' only
                if (i + 1 < applyIndex)
                {
                    applyReflectorsToRow(blockLength, A, V, i + 1);
                }
            }
        }

        /**
         * <p>
         * Computes W from the householder reflectors stored in the columns of the row block
         * submatrix Y.
         * </p>
         *
         * <p>
         * Y = v<sup>(1)</sup><br>
         * W = -&beta;<sub>1</sub>v<sup>(1)</sup><br>
         * for j=2:r<br>
         * &nbsp;&nbsp;z = -&beta;(I +WY<sup>T</sup>)v<sup>(j)</sup> <br>
         * &nbsp;&nbsp;W = [W z]<br>
         * &nbsp;&nbsp;Y = [Y v<sup>(j)</sup>]<br>
         * end<br>
         * <br>
         * where v<sup>(.)</sup> are the house holder vectors, and r is the block length.  Note that
         * Y already contains the householder vectors so it does not need to be modified.
         * </p>
         *
         * <p>
         * Y and W are assumed to have the same number of rows and columns.
         * </p>
         */
        public static void computeW_row(int blockLength,
            DSubmatrixD1 Y, DSubmatrixD1 W,
            double[] beta, int betaIndex)
        {

            int heightY = Y.row1 - Y.row0;
            CommonOps_DDRM.fill(W.original, 0);

            // W = -beta*v(1)
            BlockHouseHolder_DDRB.scale_row(blockLength, Y, W, 0, 1, -beta[betaIndex++]);

            int min = Math.Min(heightY, W.col1 - W.col0);

            // set up rest of the rows
            for (int i = 1; i < min; i++)
            {
                // w=-beta*(I + W*Y^T)*u
                double b = -beta[betaIndex++];

                // w = w -beta*W*(Y^T*u)
                for (int j = 0; j < i; j++)
                {
                    double yv = BlockHouseHolder_DDRB.innerProdRow(blockLength, Y, i, Y, j, 1);
                    VectorOps_DDRB.add_row(blockLength, W, i, 1, W, j, b * yv, W, i, 1, Y.col1 - Y.col0);
                }

                //w=w -beta*u + stuff above
                BlockHouseHolder_DDRB.add_row(blockLength, Y, i, b, W, i, 1, W, i, 1, Y.col1 - Y.col0);
            }
        }


        /**
         * <p>
         * Given an already computed tridiagonal decomposition, compute the V row block vector.<br>
         * <br>
         * y(:) = A*u<br>
         * v(i) = y - (1/2)*&gamma;*(y^T*u)*u
         * </p>
         */
        public static void computeV_blockVector(int blockLength,
            DSubmatrixD1 A,
            double[] gammas,
            DSubmatrixD1 V)
        {
            int blockHeight = Math.Min(blockLength, A.row1 - A.row0);
            if (blockHeight <= 1)
                return;
            int width = A.col1 - A.col0;
            int num = Math.Min(width - 1, blockHeight);

            for (int i = 0; i < num; i++)
            {
                double gamma = gammas[A.row0 + i];

                // compute y
                computeY(blockLength, A, V, i, gamma);

                // compute v from y
                computeRowOfV(blockLength, A, V, i, gamma);
            }
        }

        /**
         * <p>
         * Applies the reflectors that have been computed previously to the specified row.
         * <br>
         * A = A + u*v^T + v*u^T only along the specified row in A.
         * </p>
         *
         * @param blockLength
         * @param A Contains the reflectors and the row being updated.
         * @param V Contains previously computed 'v' vectors.
         * @param row The row of 'A' that is to be updated.
         */
        public static void applyReflectorsToRow(int blockLength,
            DSubmatrixD1 A,
            DSubmatrixD1 V,
            int row)
        {
            int height = Math.Min(blockLength, A.row1 - A.row0);

            double[] dataA = A.original.data;
            double[] dataV = V.original.data;

            int indexU, indexV;

            // for each previously computed reflector
            for (int i = 0; i < row; i++)
            {
                int width = Math.Min(blockLength, A.col1 - A.col0);

                indexU = A.original.numCols * A.row0 + height * A.col0 + i * width + row;
                indexV = V.original.numCols * V.row0 + height * V.col0 + i * width + row;

                double u_row = (i + 1 == row) ? 1.0 : dataA[indexU];
                double v_row = dataV[indexV];

                // take in account the leading one
                double before = A.get(i, i + 1);
                A.set(i, i + 1, 1);

                // grab only the relevant row from A = A + u*v^T + v*u^T
                VectorOps_DDRB.add_row(blockLength, A, row, 1, V, i, u_row, A, row, row, A.col1 - A.col0);
                VectorOps_DDRB.add_row(blockLength, A, row, 1, A, i, v_row, A, row, row, A.col1 - A.col0);

                A.set(i, i + 1, before);
            }
        }

        /**
         * <p>
         * Computes the 'y' vector and stores the result in 'v'<br>
         * <br>
         * y = -&gamma;(A + U*V^T + V*U^T)u
         * </p>
         *
         * @param blockLength
         * @param A Contains the reflectors and the row being updated.
         * @param V Contains previously computed 'v' vectors.
         * @param row The row of 'A' that is to be updated.
         */
        public static void computeY(int blockLength,
            DSubmatrixD1 A,
            DSubmatrixD1 V,
            int row,
            double gamma)
        {
            // Elements in 'y' before 'row' are known to be zero and the element at 'row'
            // is not used. Thus only elements after row and after are computed.
            // y = A*u
            multA_u(blockLength, A, V, row);

            for (int i = 0; i < row; i++)
            {
                // y = y + u_i*v_i^t*u + v_i*u_i^t*u

                // v_i^t*u
                double dot_v_u = BlockHouseHolder_DDRB.innerProdRow(blockLength, A, row, V, i, 1);

                // u_i^t*u
                double dot_u_u = BlockHouseHolder_DDRB.innerProdRow(blockLength, A, row, A, i, 1);

                // y = y + u_i*(v_i^t*u)
                // the ones in these 'u' are skipped over since the next submatrix of A
                // is only updated
                VectorOps_DDRB.add_row(blockLength, V, row, 1, A, i, dot_v_u, V, row, row + 1, A.col1 - A.col0);

                // y = y + v_i*(u_i^t*u)
                // the 1 in U is taken account above
                VectorOps_DDRB.add_row(blockLength, V, row, 1, V, i, dot_u_u, V, row, row + 1, A.col1 - A.col0);
            }

            // y = -gamma*y
            VectorOps_DDRB.scale_row(blockLength, V, row, -gamma, V, row, row + 1, V.col1 - V.col0);
        }

        /**
         * <p>
         * Multiples the appropriate submatrix of A by the specified reflector and stores
         * the result ('y') in V.<br>
         * <br>
         * y = A*u<br>
         * </p>
         *
         * @param blockLength
         * @param A Contains the 'A' matrix and 'u' vector.
         * @param V Where resulting 'y' row vectors are stored.
         * @param row row in matrix 'A' that 'u' vector and the row in 'V' that 'y' is stored in.
         */
        public static void multA_u(int blockLength,
            DSubmatrixD1 A,
            DSubmatrixD1 V,
            int row)
        {
            int heightMatA = A.row1 - A.row0;

            for (int i = row + 1; i < heightMatA; i++)
            {

                double val = innerProdRowSymm(blockLength, A, row, A, i, 1);

                V.set(row, i, val);
            }
        }

        public static double innerProdRowSymm(int blockLength,
            DSubmatrixD1 A,
            int rowA,
            DSubmatrixD1 B,
            int rowB, int zeroOffset)
        {
            int offset = rowA + zeroOffset;
            if (offset + B.col0 >= B.col1)
                return 0;

            if (offset < rowB)
            {
                // take in account the one in 'A'
                double total = B.get(offset, rowB);

                total += VectorOps_DDRB.dot_row_col(blockLength, A, rowA, B, rowB, offset + 1, rowB);
                total += VectorOps_DDRB.dot_row(blockLength, A, rowA, B, rowB, rowB, A.col1 - A.col0);

                return total;
            }
            else
            {
                // take in account the one in 'A'
                double total = B.get(rowB, offset);

                total += VectorOps_DDRB.dot_row(blockLength, A, rowA, B, rowB, offset + 1, A.col1 - A.col0);

                return total;
            }
        }

        /**
         * <p>
         * Final computation for a single row of 'v':<br>
         * <br>
         * v = y -(1/2)&gamma;(y^T*u)*u
         * </p>
         *
         * @param blockLength
         * @param A
         * @param V
         * @param row
         * @param gamma
         */
        public static void computeRowOfV(int blockLength,
            DSubmatrixD1 A,
            DSubmatrixD1 V,
            int row,
            double gamma)
        {
            // val=(y^T*u)
            double val = BlockHouseHolder_DDRB.innerProdRow(blockLength, A, row, V, row, 1);

            // take in account the one
            double before = A.get(row, row + 1);
            A.set(row, row + 1, 1);

            // v = y - (1/2)gamma*val * u
            VectorOps_DDRB.add_row(blockLength, V, row, 1, A, row, -0.5 * gamma * val, V, row, row + 1,
                A.col1 - A.col0);

            A.set(row, row + 1, before);
        }
    }
}