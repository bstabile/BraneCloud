using System;

namespace BraneCloud.Evolution.EC.MatrixLib.Ops
{
    //package org.ejml.ops;

/**
 * @author Peter Abeles
 */
    public class SortCoupledArray_F32
    {

        int[] tmp = new int[0];

        int[] copyA = new int[0];
        float[] copyB = new float[0];

        QuickSort_S32 quicksort = new QuickSort_S32();

        public void quick(int[] segments, int length, int[] valuesA, float[] valuesB)
        {
            for (int i = 1; i < length; i++)
            {
                int x0 = segments[i - 1];
                int x1 = segments[i];

                quick(x0, x1 - x0, valuesA, valuesB);
            }
        }

        private void quick(int offset, int length, int[] valuesA, float[] valuesB)
        {

            if (length <= 1)
                return;

            if (tmp.Length < length)
            {
                int l = length * 2 + 1;
                tmp = new int[l];
                copyA = new int[l];
                copyB = new float[l];
            }

            Array.Copy(valuesA, offset, copyA, 0, length);
            Array.Copy(valuesB, offset, copyB, 0, length);

            if (length > 50)
                quicksort.sort(copyA, length, tmp);
            else
                shellSort(copyA, 0, length, tmp);

            for (int i = 0; i < length; i++)
            {
                valuesA[offset + i] = copyA[tmp[i]];
                valuesB[offset + i] = copyB[tmp[i]];
            }
        }

        public static void shellSort(int[] data, int offset, int length, int[] indexes)
        {
            for (int k = 0; k < length; k++)
            {
                indexes[k] = offset + k;
            }

            int i, j;
            int inc = 1;
            int v;

            do
            {
                inc *= 3;
                inc++;
            } while (inc <= length);

            do
            {
                inc /= 3;

                for (i = inc; i < length; i++)
                {
                    v = data[indexes[i]];
                    int idx_i = indexes[i];
                    j = i;
                    while (data[indexes[j - inc]] > v)
                    {
                        indexes[j] = indexes[j - inc];
                        j -= inc;
                        if (j < inc) break;
                    }
                    indexes[j] = idx_i;
                }
            } while (inc > 1);
        }
    }
}