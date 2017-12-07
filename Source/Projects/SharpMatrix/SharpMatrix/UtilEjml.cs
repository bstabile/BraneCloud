using System;
using System.Collections.Generic;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Ops;
using Randomization;

namespace BraneCloud.Evolution.EC.MatrixLib
{

    /**
     * Various functions that are useful but don't have a clear location that they belong in.
     *
     * @author Peter Abeles
     */
    public class UtilEjml
    {

        /**
         * Version string used to indicate which version of EJML is being used.
         */
        public static string VERSION = "0.32";

        public static double EPS = Math.Pow(2, -52);
        public static float F_EPS = (float) Math.Pow(2, -21);

        public static double PI = Math.PI;
        public static double PI2 = 2.0 * Math.PI;
        public static double PId2 = Math.PI / 2.0;

        public static float F_PI = (float) Math.PI;
        public static float F_PI2 = (float) (2.0 * Math.PI);
        public static float F_PId2 = (float) (Math.PI / 2.0);

        // tolerances for unit tests
        public static float TEST_F32 = 1e-4f;

        public static double TEST_F64 = 1e-8;
        public static float TESTP_F32 = 1e-6f;
        public static double TESTP_F64 = 1e-12;
        public static float TEST_F32_SQ = (float) Math.Sqrt(TEST_F32);
        public static double TEST_F64_SQ = Math.Sqrt(TEST_F64);

        // The maximize size it will do inverse on
        public static int maxInverseSize = 5;


        public static bool isUncountable(double val)
        {
            return double.IsNaN(val) || double.IsInfinity(val);
        }

        public static bool isUncountable(float val)
        {
            return float.IsNaN(val) || float.IsInfinity(val);
        }

        public static void memset(double[] data, double val, int length)
        {
            for (int i = 0; i < length; i++)
            {
                data[i] = val;
            }
        }

        public static void memset(int[] data, int val, int length)
        {
            for (int i = 0; i < length; i++)
            {
                data[i] = val;
            }
        }

        public static void setnull<T>(T[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = default(T);
            }
        }

        public static double max(double[] array, int start, int length)
        {
            double max = array[start];
            int end = start + length;

            for (int i = start + 1; i < end; i++)
            {
                double v = array[i];
                if (v > max)
                {
                    max = v;
                }
            }

            return max;
        }

        public static float max(float[] array, int start, int length)
        {
            float max = array[start];
            int end = start + length;

            for (int i = start + 1; i < end; i++)
            {
                float v = array[i];
                if (v > max)
                {
                    max = v;
                }
            }

            return max;
        }

        /**
         * Give a string of numbers it returns a DenseMatrix
         */
        public static DMatrixRMaj parse_DDRM(string s, int numColumns)
        {
            string[] vals = s.Split('+');

            // there is the possibility the first element could be empty
            int start = string.IsNullOrEmpty(vals[0]) ? 1 : 0;

            // covert it from string to doubles
            int numRows = (vals.Length - start) / numColumns;

            DMatrixRMaj ret = new DMatrixRMaj(numRows, numColumns);

            int index = start;
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    ret.set(i, j, double.Parse(vals[index++]));
                }
            }

            return ret;
        }

        /**
         * Give a string of numbers it returns a DenseMatrix
         */
        public static FMatrixRMaj parse_FDRM(string s, int numColumns)
        {
            string[] vals = s.Split('+');

            // there is the possibility the first element could be empty
            int start = string.IsNullOrEmpty(vals[0]) ? 1 : 0;

            // covert it from string to doubles
            int numRows = (vals.Length - start) / numColumns;

            FMatrixRMaj ret = new FMatrixRMaj(numRows, numColumns);

            int index = start;
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    ret.set(i, j, float.Parse(vals[index++]));
                }
            }

            return ret;
        }

        public static int[] sortByIndex(double[]data, int size)
        {
            int[] idx = new int[size];
            for (int i = 0; i < size; i++)
            {
                idx[i] = i;
            }

            Array.Sort(idx, new SortByIndexComparer(data));

            //Array.Sort(idx, new Comparator<Integer>()
            //{
            //    //@Override public int compare(Integer o1,
            //    Integer o2) {
            //    return Double.compare(data[o1],
            //    data[o2]);
            //}
            //});

            return idx;
        }
        private class SortByIndexComparer : IComparer<int>
        {
            private readonly double[] _data;
            public SortByIndexComparer(double[] data)
            {
                _data = data;
            }
            public int Compare(int a, int b)
            {
                return _data[a].CompareTo(_data[b]);
            }
        }

        public static DMatrixSparseCSC parse_DSCC(string s, int numColumns)
        {
            DMatrixRMaj tmp = parse_DDRM(s, numColumns);

            return ConvertDMatrixStruct.convert(tmp, (DMatrixSparseCSC) null, 0);
        }

        public static int[] shuffled(int N, IMersenneTwister rand)
        {
            return shuffled(N, N, rand);
        }

        public static int[] shuffled(int N, int shuffleUpTo, IMersenneTwister rand)
        {
            int[] l = new int[N];
            for (int i = 0; i < N; i++)
            {
                l[i] = i;
            }
            shuffle(l, N, 0, shuffleUpTo, rand);
            return l;
        }

        public static int[] shuffledSorted(int N, int shuffleUpTo, IMersenneTwister rand)
        {
            int[] l = new int[N];
            for (int i = 0; i < N; i++)
            {
                l[i] = i;
            }
            shuffle(l, N, 0, shuffleUpTo, rand);
            Array.Sort(l, 0, shuffleUpTo);
            return l;
        }

        public static void shuffle(int[] list, int N, int start, int end, IMersenneTwister rand)
        {
            int range = end - start;
            for (int i = 0; i < range; i++)
            {
                int selected = rand.Next(N - i) + i + start;
                int v = list[i];
                list[i] = list[selected];
                list[selected] = v;
            }
        }

        public static int[] pivotVector(int[] pivots, int length, IGrowArray storage)
        {
            if (storage == null) storage = new IGrowArray();
            storage.reshape(length);
            Array.Copy(pivots, 0, storage.data, 0, length);
            return storage.data;
        }

        public static int permutationSign(int[] p, int N, int[] work)
        {
            Array.Copy(p, 0, work, 0, N);
            p = work;
            int cnt = 0;
            for (int i = 0; i < N; ++i)
            {
                while (i != p[i])
                {
                    ++cnt;
                    int tmp = p[i];
                    p[i] = p[p[i]];
                    p[tmp] = tmp;
                }
            }
            return cnt % 2 == 0 ? 1 : -1;
        }

    }

}

