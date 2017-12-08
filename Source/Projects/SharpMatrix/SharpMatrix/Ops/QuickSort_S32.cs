using System;

namespace SharpMatrix.Ops
{
    /**
     * An implementation of the quick sort algorithm from Numerical Recipes Third Edition
     * that is specified for arrays of doubles.
     * <p>
     * A small amount of memory is declared for this sorting algorithm.
     * <p>
     * This implementation seems to often perform slower than Shell sort.  A comment in Numerical
     * recipes about unnecessary array checks makes me think this is slow because java always
     * does a bounds check on arrays.
     * <p>
     * This has slightly better performance than Arrays.sort(double[]).  Not noticeable in most applications.
     */
    public class QuickSort_S32
    {
        // an architecture dependent tuning parameter
        private int M = 7;

        private int NSTACK;

        private int[] istack;

        public QuickSort_S32()
        {
            NSTACK = 65;
            istack = new int[NSTACK];
        }

        public QuickSort_S32(int NSTACK, int M)
        {
            this.M = M;
            this.NSTACK = NSTACK;

            istack = new int[NSTACK];
        }

        public void sort(int[] arr, int length, int[] indexes)
        {
            for (int ix = 0; ix < length; ix++)
            {
                indexes[ix] = ix;
            }

            int i, ir, j, k;
            int jstack = -1;
            int l = 0;
            // if I ever publish a book I will never use variable l in an algorithm with lots of 1

            int a;

            ir = length - 1;

            int temp;

            for (;;)
            {
                if (ir - l < M)
                {
                    for (j = l + 1; j <= ir; j++)
                    {
                        a = arr[indexes[j]];
                        temp = indexes[j];
                        for (i = j - 1; i >= l; i--)
                        {
                            if (arr[indexes[i]] <= a) break;
                            indexes[i + 1] = indexes[i];
                        }
                        indexes[i + 1] = temp;
                    }
                    if (jstack < 0) break;

                    ir = istack[jstack--];
                    l = istack[jstack--];

                }
                else
                {
                    k = (int) ((uint) (l + ir)) >> 1;
                    temp = indexes[k];
                    indexes[k] = indexes[l + 1];
                    indexes[l + 1] = temp;

                    if (arr[indexes[l]] > arr[indexes[ir]])
                    {
                        temp = indexes[l];
                        indexes[l] = indexes[ir];
                        indexes[ir] = temp;
                    }
                    if (arr[indexes[l + 1]] > arr[indexes[ir]])
                    {
                        temp = indexes[l + 1];
                        indexes[l + 1] = indexes[ir];
                        indexes[ir] = temp;
                    }
                    if (arr[indexes[l]] > arr[indexes[l + 1]])
                    {
                        temp = indexes[l];
                        indexes[l] = indexes[l + 1];
                        indexes[l + 1] = temp;
                    }
                    i = l + 1;
                    j = ir;
                    a = arr[indexes[l + 1]];
                    for (;;)
                    {
                        do
                        {
                            i++;
                        } while (arr[indexes[i]] < a);
                        do
                        {
                            j--;
                        } while (arr[indexes[j]] > a);
                        if (j < i) break;
                        temp = indexes[i];
                        indexes[i] = indexes[j];
                        indexes[j] = temp;
                    }
                    temp = indexes[l + 1];
                    indexes[l + 1] = indexes[j];
                    indexes[j] = temp;
                    jstack += 2;

                    if (jstack >= NSTACK)
                        throw new InvalidOperationException("NSTACK too small");
                    if (ir - i + 1 >= j - l)
                    {
                        istack[jstack] = ir;
                        istack[jstack - 1] = i;
                        ir = j - 1;
                    }
                    else
                    {
                        istack[jstack] = j - 1;
                        istack[jstack - 1] = l;
                        l = i;
                    }
                }
            }
        }
    }
}