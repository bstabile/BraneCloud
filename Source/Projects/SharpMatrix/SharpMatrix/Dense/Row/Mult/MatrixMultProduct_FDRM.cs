using System;
using SharpMatrix.Data;

namespace SharpMatrix.Dense
{
    //package org.ejml.dense.row.mult;

/**
 * <p>
 * Specialized operations for performing inner and outer products for matrices.
 * </p>
 * 
 * <p>
 * inner product: B=A<sup>T</sup>*A<br>
 * outer product: B=A*A<sup>T</sup>
 * </p>
 * 
 * @author Peter Abeles
 */
    public class MatrixMultProduct_FDRM
    {

        public static void outer(FMatrix1Row a, FMatrix1Row c)
        {
            for (int i = 0; i < a.numRows; i++)
            {
                int indexC1 = i * c.numCols + i;
                int indexC2 = indexC1;
                for (int j = i; j < a.numRows; j++, indexC2 += c.numCols)
                {
                    int indexA = i * a.numCols;
                    int indexB = j * a.numCols;
                    float sum = 0;
                    int end = indexA + a.numCols;
                    for (; indexA < end; indexA++, indexB++)
                    {
                        sum += a.data[indexA] * a.data[indexB];
                    }
                    c.data[indexC2] = c.data[indexC1++] = sum;
                }
            }
//        for( int i = 0; i < a.numRows; i++ ) {
//            for( int j = 0; j < a.numRows; j++ ) {
//                float sum = 0;
//                for( int k = 0; k < a.numCols; k++ ) {
//                    sum += a.get(i,k)*a.get(j,k);
//                }
//                c.set(i,j,sum);
//            }
//        }
        }

        public static void inner_small(FMatrix1Row a, FMatrix1Row c)
        {

            for (int i = 0; i < a.numCols; i++)
            {
                for (int j = i; j < a.numCols; j++)
                {
                    int indexC1 = i * c.numCols + j;
                    int indexC2 = j * c.numCols + i;
                    int indexA = i;
                    int indexB = j;
                    float sum = 0;
                    int end = indexA + a.numRows * a.numCols;
                    for (; indexA < end; indexA += a.numCols, indexB += a.numCols)
                    {
                        sum += a.data[indexA] * a.data[indexB];
                    }
                    c.data[indexC1] = c.data[indexC2] = sum;
                }
            }
//        for( int i = 0; i < a.numCols; i++ ) {
//            for( int j = i; j < a.numCols; j++ ) {
//                float sum = 0;
//                for( int k = 0; k < a.numRows; k++ ) {
//                    sum += a.get(k,i)*a.get(k,j);
//                }
//                c.set(i,j,sum);
//                c.set(j,i,sum);
//            }
//        }
        }

        public static void inner_reorder(FMatrix1Row a, FMatrix1Row c)
        {

            for (int i = 0; i < a.numCols; i++)
            {
                int indexC = i * c.numCols + i;
                float valAi = a.data[i];
                for (int j = i; j < a.numCols; j++)
                {
                    c.data[indexC++] = valAi * a.data[j];
                }

                for (int k = 1; k < a.numRows; k++)
                {
                    indexC = i * c.numCols + i;
                    int indexB = k * a.numCols + i;
                    valAi = a.data[indexB];
                    for (int j = i; j < a.numCols; j++)
                    {
                        c.data[indexC++] += valAi * a.data[indexB++];
                    }
                }

                indexC = i * c.numCols + i;
                int indexC2 = indexC;
                for (int j = i; j < a.numCols; j++, indexC2 += c.numCols)
                {
                    c.data[indexC2] = c.data[indexC++];
                }
            }

//        for( int i = 0; i < a.numCols; i++ ) {
//            for( int j = i; j < a.numCols; j++ ) {
//                c.set(i,j,a.get(0,i)*a.get(0,j));
//            }
//
//            for( int k = 1; k < a.numRows; k++ ) {
//                for( int j = i; j < a.numCols; j++ ) {
//                    c.set(i,j, c.get(i,j)+ a.get(k,i)*a.get(k,j));
//                }
//            }
//            for( int j = i; j < a.numCols; j++ ) {
//                c.set(j,i,c.get(i,j));
//            }
//        }
        }

        public static void inner_reorder_upper(FMatrix1Row a, FMatrix1Row c)
        {
            for (int i = 0; i < a.numCols; i++)
            {
                int indexC = i * c.numCols + i;
                float valAi = a.data[i];
                for (int j = i; j < a.numCols; j++)
                {
                    c.data[indexC++] = valAi * a.data[j];
                }

                for (int k = 1; k < a.numRows; k++)
                {
                    indexC = i * c.numCols + i;
                    int indexB = k * a.numCols + i;
                    valAi = a.data[indexB];
                    for (int j = i; j < a.numCols; j++)
                    {
                        c.data[indexC++] += valAi * a.data[indexB++];
                    }
                }
            }
        }
    }
}