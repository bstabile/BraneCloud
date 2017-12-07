using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Block.Decomposition.QR;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Block.Decomposition.Bidiagonal
{
    //package org.ejml.dense.block.decomposition.bidiagonal;

/**
 * @author Peter Abeles
 */
    public class BidiagonalHelper_FDRB
    {

        /**
         * Performs a standard bidiagonal decomposition just on the outer blocks of the provided matrix
         *
         * @param blockLength
         * @param A
         * @param gammasU
         */

        public static bool bidiagOuterBlocks(int blockLength,
            FSubmatrixD1 A,
            float[] gammasU,
            float[] gammasV)
        {
//        Console.WriteLine("---------- Orig");
//        A.original.print();

            int width = Math.Min(blockLength, A.col1 - A.col0);
            int height = Math.Min(blockLength, A.row1 - A.row0);

            int min = Math.Min(width, height);

            for (int i = 0; i < min; i++)
            {
                //--- Apply reflector to the column

                // compute the householder vector
                if (!BlockHouseHolder_FDRB.computeHouseHolderCol(blockLength, A, gammasU, i))
                    return false;

                // apply to rest of the columns in the column block
                BlockHouseHolder_FDRB.rank1UpdateMultR_Col(blockLength, A, i, gammasU[A.col0 + i]);

                // apply to the top row block
                BlockHouseHolder_FDRB.rank1UpdateMultR_TopRow(blockLength, A, i, gammasU[A.col0 + i]);

                Console.WriteLine("After column stuff");
                A.original.print();

                //-- Apply reflector to the row
                if (!BlockHouseHolder_FDRB.computeHouseHolderRow(blockLength, A, gammasV, i))
                    return false;

                // apply to rest of the rows in the row block
                BlockHouseHolder_FDRB.rank1UpdateMultL_Row(blockLength, A, i, i + 1, gammasV[A.row0 + i]);

                Console.WriteLine("After update row");
                A.original.print();

                // apply to the left column block
                // TODO THIS WON'T WORK!!!!!!!!!!!!!
                // Needs the whole matrix to have been updated by the left reflector to compute the correct solution
//            rank1UpdateMultL_LeftCol(blockLength,A,i,i+1,gammasV[A.row0+i]);

                Console.WriteLine("After row stuff");
                A.original.print();
            }

            return true;
        }
    }
}