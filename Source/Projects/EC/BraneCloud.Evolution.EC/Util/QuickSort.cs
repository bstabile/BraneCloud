/*
 * BraneCloud.Evolution.EC (Evolutionary Computation)
 * Copyright 2011 Bennett R. Stabile (BraneCloud.Evolution.net|com)
 * Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
 *
 * This is an independent conversion from Java to .NET of ...
 *
 * Sean Luke's ECJ project at GMU 
 * (Academic Free License v3.0): 
 * http://www.cs.gmu.edu/~eclab/projects/ecj
 *
 * Radical alteration was required throughout (including structural).
 * The author of ECJ cannot and MUST not be expected to support this fork.
 *
 * If you wish to create yet another fork, please use a different root namespace.
 * BraneCloud is a registered domain that will be used for name/schema resolution.
 */

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Util
{
    /// <summary> 
    /// Implementations of various center-pivot QuickSort routines in Java,
    /// and (if you really want 'em) Insertion Sort routines as well.  This code
    /// is derived from the QuickSort example in the 
    /// <a href="ftp://ftp.prenhall.com/pub/esm/computer_science.s-041/shaffer/ds/code/JAVA/code/javacode.zip">
    /// source code </a> accompanying <i>A Practical Introduction to Data Structures 
    /// and Algorithm Analysis, Java Edition</i>, by Clifford Shaffer.
    /// Here's the original header:
    /// 
    /// <p/>
    /// Source code example for "A Practical Introduction
    /// to Data Structures and Algorithm Analysis"
    /// by Clifford A. Shaffer, Prentice Hall, 1998.
    /// Copyright 1998 by Clifford A. Shaffer
    /// 
    /// <p/>Sorting main function for testing correctness of sort algorithm.
    /// To use: <sortname> [+/-] <size_of_array> <threshold>
    /// + means increasing values, - means decreasing value and no
    /// parameter means random values;
    /// where <size_of_array> controls the number of objects to be sorted;
    /// and <threshold> controls the threshold parameter for certain sorts, e.g.,
    /// cutoff point for quicksort sublists.
    /// </summary>
    [ECConfiguration("ec.util.QuickSort")]
    public class QuickSort
    {
        #region Constants

        internal const int THRESHOLD = 0; // might want to tweak this
        internal const int MAXSTACKSIZE = 1000;

        #endregion // Constants

        #region Operations

        #region Char

        /// <summary>
        /// Non-Recursive QuickSort. 
        /// </summary>
        static public void QSort(char[] array)
        {
            QSort_h(array, 0, array.Length - 1);
        }

        /// <summary>
        /// Insertion Sort. 
        /// </summary>
        static public void InsSort(char[] array)
        {
            for (var i = 1; i < array.Length; i++)
                // Insert i'th record
                for (var j = i; (j > 0) && (array[j] < array[j - 1]); j--)
                //DSutil.swap(array, j, j-1);
                {
                    var tmp = array[j]; array[j] = array[j - 1]; array[j - 1] = tmp;
                }
        }

        static private void QSort_h(char[] array, int oi, int oj)
        {
            var stack = new int[MAXSTACKSIZE]; // Stack for array bounds

            var top = -1;

            stack[++top] = oi; // Initialize stack
            stack[++top] = oj;

            while (top > 0)
            // While there are unprocessed subarrays
            {
                // Pop stack
                var j = stack[top--];
                var i = stack[top--];

                // Findpivot
                var pivotindex = (i + j) / 2;
                var pivot = array[pivotindex];
                //DSutil.swap(array, pivotindex, j); // Stick pivot at end
                var tmp = array[pivotindex]; array[pivotindex] = array[j]; array[j] = tmp;
                // Partition
                var l = i - 1;
                var r = j;
                do
                {
                    while (array[++l] < pivot)
                        ;
                    while ((r != 0) && (array[--r] > pivot))
                        ;
                    //DSutil.swap(array, l, r);
                    tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                }
                while (l < r);
                //DSutil.swap(array, l, r);  // Undo final swap
                tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                //DSutil.swap(array, l, j);  // Put pivot value in place
                tmp = array[l]; array[l] = array[j]; array[j] = tmp;

                // Put new subarrays onto stack if they are small
                if ((l - i) > THRESHOLD)
                // Left partition
                {
                    stack[++top] = i;
                    stack[++top] = l - 1;
                }
                if ((j - l) > THRESHOLD)
                // Right partition 
                {
                    stack[++top] = l + 1;
                    stack[++top] = j;
                }
            }
            InsSort(array); // Final Insertion Sort
        }

        #endregion // Char
        #region Byte Arrays

        /// <summary>
        /// Non-Recursive QuickSort for byte arrays. 
        /// </summary>
        static public void QSort(byte[] array)
        {
            QSort_h(array, 0, array.Length - 1);
        }

        /// <summary>
        /// Insertion Sort. 
        /// </summary>
        static public void InsSort(byte[] array)
        {
            for (var i = 1; i < array.Length; i++)
                // Insert i'th record
                for (var j = i; (j > 0) && (array[j] < array[j - 1]); j--)
                //DSutil.swap(array, j, j-1);
                {
                    var tmp = array[j]; array[j] = array[j - 1]; array[j - 1] = tmp;
                }
        }

        static private void QSort_h(byte[] array, int oi, int oj)
        {
            var stack = new int[MAXSTACKSIZE]; // Stack for array bounds
            var top = -1;

            stack[++top] = oi; // Initialize stack
            stack[++top] = oj;

            while (top > 0)
            // While there are unprocessed subarrays
            {
                // Pop stack
                var j = stack[top--];
                var i = stack[top--];

                // Findpivot
                var pivotindex = (i + j) / 2;
                var pivot = array[pivotindex];
                //DSutil.swap(array, pivotindex, j); // Stick pivot at end
                var tmp = array[pivotindex]; array[pivotindex] = array[j]; array[j] = tmp;
                // Partition
                var l = i - 1;
                var r = j;
                do
                {
                    while (array[++l] < pivot)
                        ;
                    while ((r != 0) && (array[--r] > pivot))
                        ;
                    //DSutil.swap(array, l, r);
                    tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                }
                while (l < r);
                //DSutil.swap(array, l, r);  // Undo final swap
                tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                //DSutil.swap(array, l, j);  // Put pivot value in place
                tmp = array[l]; array[l] = array[j]; array[j] = tmp;

                // Put new subarrays onto stack if they are small
                if ((l - i) > THRESHOLD)
                // Left partition
                {
                    stack[++top] = i;
                    stack[++top] = l - 1;
                }
                if ((j - l) > THRESHOLD)
                // Right partition 
                {
                    stack[++top] = l + 1;
                    stack[++top] = j;
                }
            }
            InsSort(array); // Final Insertion Sort
        }

        #endregion // Byte Arrays
        #region SByte Arrays

        /// <summary>
        /// Non-Recursive QuickSort for sbyte arrays. 
        /// </summary>
        static public void QSort(sbyte[] array)
        {
            QSort_h(array, 0, array.Length - 1);
        }

        /// <summary>
        /// Insertion Sort. 
        /// </summary>
        static public void InsSort(sbyte[] array)
        {
            for (var i = 1; i < array.Length; i++)
                // Insert i'th record
                for (var j = i; (j > 0) && (array[j] < array[j - 1]); j--)
                //DSutil.swap(array, j, j-1);
                {
                    var tmp = array[j]; array[j] = array[j - 1]; array[j - 1] = tmp;
                }
        }

        static private void QSort_h(sbyte[] array, int oi, int oj)
        {
            var stack = new int[MAXSTACKSIZE]; // Stack for array bounds
            var top = -1;

            stack[++top] = oi; // Initialize stack
            stack[++top] = oj;

            while (top > 0)
            // While there are unprocessed subarrays
            {
                // Pop stack
                var j = stack[top--];
                var i = stack[top--];

                // Findpivot
                var pivotindex = (i + j) / 2;
                var pivot = array[pivotindex];
                //DSutil.swap(array, pivotindex, j); // Stick pivot at end
                var tmp = array[pivotindex]; array[pivotindex] = array[j]; array[j] = tmp;
                // Partition
                var l = i - 1;
                var r = j;
                do
                {
                    while (array[++l] < pivot)
                        ;
                    while ((r != 0) && (array[--r] > pivot))
                        ;
                    //DSutil.swap(array, l, r);
                    tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                }
                while (l < r);
                //DSutil.swap(array, l, r);  // Undo final swap
                tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                //DSutil.swap(array, l, j);  // Put pivot value in place
                tmp = array[l]; array[l] = array[j]; array[j] = tmp;

                // Put new subarrays onto stack if they are small
                if ((l - i) > THRESHOLD)
                // Left partition
                {
                    stack[++top] = i;
                    stack[++top] = l - 1;
                }
                if ((j - l) > THRESHOLD)
                // Right partition 
                {
                    stack[++top] = l + 1;
                    stack[++top] = j;
                }
            }
            InsSort(array); // Final Insertion Sort
        }

        #endregion // SByte Arrays
        #region UShort

        /// <summary>
        /// Non-Recursive QuickSort for ushort arrays. 
        /// </summary>
        static public void QSort(ushort[] array)
        {
            QSort_h(array, 0, array.Length - 1);
        }

        /// <summary>
        /// Insertion Sort for ushort arrays. 
        /// </summary>
        static public void InsSort(ushort[] array)
        {
            for (var i = 1; i < array.Length; i++)
                // Insert i'th record
                for (var j = i; (j > 0) && (array[j] < array[j - 1]); j--)
                //DSutil.swap(array, j, j-1);
                {
                    var tmp = array[j]; array[j] = array[j - 1]; array[j - 1] = tmp;
                }
        }

        static private void QSort_h(ushort[] array, int oi, int oj)
        {
            var stack = new int[MAXSTACKSIZE]; // Stack for array bounds
            var top = -1;

            stack[++top] = oi; // Initialize stack
            stack[++top] = oj;

            while (top > 0)
            // While there are unprocessed subarrays
            {
                // Pop stack
                var j = stack[top--];
                var i = stack[top--];

                // Findpivot
                var pivotindex = (i + j) / 2;
                var pivot = array[pivotindex];
                //DSutil.swap(array, pivotindex, j); // Stick pivot at end
                var tmp = array[pivotindex];
                array[pivotindex] = array[j]; array[j] = tmp;
                // Partition
                var l = i - 1;
                var r = j;
                do
                {
                    while (array[++l] < pivot)
                        ;
                    while ((r != 0) && (array[--r] > pivot))
                        ;
                    //DSutil.swap(array, l, r);
                    tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                }
                while (l < r);
                //DSutil.swap(array, l, r);  // Undo final swap
                tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                //DSutil.swap(array, l, j);  // Put pivot value in place
                tmp = array[l]; array[l] = array[j]; array[j] = tmp;

                // Put new subarrays onto stack if they are small
                if ((l - i) > THRESHOLD)
                // Left partition
                {
                    stack[++top] = i;
                    stack[++top] = l - 1;
                }
                if ((j - l) > THRESHOLD)
                // Right partition 
                {
                    stack[++top] = l + 1;
                    stack[++top] = j;
                }
            }
            InsSort(array); // Final Insertion Sort
        }

        #endregion // UShort
        #region Short

        /// <summary>
        /// Non-Recursive QuickSort for short arrays. 
        /// </summary>
        static public void QSort(short[] array)
        {
            QSort_h(array, 0, array.Length - 1);
        }

        /// <summary>
        /// Insertion Sort for short arrays. 
        /// </summary>
        static public void InsSort(short[] array)
        {
            for (var i = 1; i < array.Length; i++)
                // Insert i'th record
                for (var j = i; (j > 0) && (array[j] < array[j - 1]); j--)
                //DSutil.swap(array, j, j-1);
                {
                    var tmp = array[j]; array[j] = array[j - 1]; array[j - 1] = tmp;
                }
        }

        static private void QSort_h(short[] array, int oi, int oj)
        {
            var stack = new int[MAXSTACKSIZE]; // Stack for array bounds
            var top = -1;

            stack[++top] = oi; // Initialize stack
            stack[++top] = oj;

            while (top > 0)
            // While there are unprocessed subarrays
            {
                // Pop stack
                var j = stack[top--];
                var i = stack[top--];

                // Findpivot
                var pivotindex = (i + j) / 2;
                var pivot = array[pivotindex];
                //DSutil.swap(array, pivotindex, j); // Stick pivot at end
                var tmp = array[pivotindex];
                array[pivotindex] = array[j]; array[j] = tmp;
                // Partition
                var l = i - 1;
                var r = j;
                do
                {
                    while (array[++l] < pivot)
                        ;
                    while ((r != 0) && (array[--r] > pivot))
                        ;
                    //DSutil.swap(array, l, r);
                    tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                }
                while (l < r);
                //DSutil.swap(array, l, r);  // Undo final swap
                tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                //DSutil.swap(array, l, j);  // Put pivot value in place
                tmp = array[l]; array[l] = array[j]; array[j] = tmp;

                // Put new subarrays onto stack if they are small
                if ((l - i) > THRESHOLD)
                // Left partition
                {
                    stack[++top] = i;
                    stack[++top] = l - 1;
                }
                if ((j - l) > THRESHOLD)
                // Right partition 
                {
                    stack[++top] = l + 1;
                    stack[++top] = j;
                }
            }
            InsSort(array); // Final Insertion Sort
        }

        #endregion // Short
        #region UInt32

        /// <summary>
        /// Non-Recursive QuickSort. 
        /// </summary>
        static public void QSort(uint[] array)
        {
            QSort_h(array, 0, array.Length - 1);
        }

        /// <summary>
        /// Insertion Sort 
        /// </summary>
        static public void InsSort(uint[] array)
        {
            for (var i = 1; i < array.Length; i++)
                // Insert i'th record
                for (var j = i; (j > 0) && (array[j] < array[j - 1]); j--)
                //DSutil.swap(array, j, j-1);
                {
                    var tmp = array[j]; array[j] = array[j - 1]; array[j - 1] = tmp;
                }
        }

        static private void QSort_h(uint[] array, int oi, int oj)
        {
            var stack = new int[MAXSTACKSIZE]; // Stack for array bounds
            var top = -1;

            stack[++top] = oi; // Initialize stack
            stack[++top] = oj;

            while (top > 0)
            // While there are unprocessed subarrays
            {
                // Pop stack
                var j = stack[top--];
                var i = stack[top--];

                // Findpivot
                var pivotindex = (i + j) / 2;
                var pivot = array[pivotindex];
                //DSutil.swap(array, pivotindex, j); // Stick pivot at end
                var tmp = array[pivotindex]; array[pivotindex] = array[j]; array[j] = tmp;
                // Partition
                var l = i - 1;
                var r = j;
                do
                {
                    while (array[++l] < pivot)
                        ;
                    while ((r != 0) && (array[--r] > pivot))
                        ;
                    //DSutil.swap(array, l, r);
                    tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                }
                while (l < r);
                //DSutil.swap(array, l, r);  // Undo final swap
                tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                //DSutil.swap(array, l, j);  // Put pivot value in place
                tmp = array[l]; array[l] = array[j]; array[j] = tmp;

                // Put new subarrays onto stack if they are small
                if ((l - i) > THRESHOLD)
                // Left partition
                {
                    stack[++top] = i;
                    stack[++top] = l - 1;
                }
                if ((j - l) > THRESHOLD)
                // Right partition 
                {
                    stack[++top] = l + 1;
                    stack[++top] = j;
                }
            }
            InsSort(array); // Final Insertion Sort
        }

        /// <summary>
        /// Non-Recursive QuickSort. 
        /// </summary>
        static public void QSort(uint[] array, ISortComparatorUL comp)
        {
            QSort_h(array, 0, array.Length - 1, comp);
        }

        /// <summary>
        /// Insertion Sort. 
        /// </summary>
        static public void InsSort(uint[] array, ISortComparatorUL comp)
        {
            for (var i = 1; i < array.Length; i++)
                // Insert i'th record
                for (var j = i; (j > 0) && (comp.lt(array[j], array[j - 1])); j--)
                //DSutil.swap(array, j, j-1);
                {
                    var tmp = array[j]; array[j] = array[j - 1]; array[j - 1] = tmp;
                }
        }

        static private void QSort_h(uint[] array, int oi, int oj, ISortComparatorUL comp)
        {
            var stack = new int[MAXSTACKSIZE]; // Stack for array bounds
            var top = -1;

            stack[++top] = oi; // Initialize stack
            stack[++top] = oj;

            while (top > 0)
            // While there are unprocessed subarrays
            {
                // Pop stack
                var j = stack[top--];
                var i = stack[top--];

                // Findpivot
                var pivotindex = (i + j) / 2;
                var pivot = array[pivotindex];
                //DSutil.swap(array, pivotindex, j); // Stick pivot at end
                var tmp = array[pivotindex]; array[pivotindex] = array[j]; array[j] = tmp;
                // Partition
                var l = i - 1;
                var r = j;
                do
                {
                    while (comp.lt(array[++l], pivot))
                        ;
                    while ((r != 0) && (comp.gt(array[--r], pivot)))
                        ;
                    //DSutil.swap(array, l, r);
                    tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                }
                while (l < r);
                //DSutil.swap(array, l, r);  // Undo final swap
                tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                //DSutil.swap(array, l, j);  // Put pivot value in place
                tmp = array[l]; array[l] = array[j]; array[j] = tmp;

                // Put new subarrays onto stack if they are small
                if ((l - i) > THRESHOLD)
                // Left partition
                {
                    stack[++top] = i;
                    stack[++top] = l - 1;
                }
                if ((j - l) > THRESHOLD)
                // Right partition 
                {
                    stack[++top] = l + 1;
                    stack[++top] = j;
                }
            }
            InsSort(array, comp); // Final Insertion Sort
        }

        #endregion // UInt32
        #region Int32

        /// <summary>
        /// Non-Recursive QuickSort. 
        /// </summary>
        static public void QSort(int[] array)
        {
            QSort_h(array, 0, array.Length - 1);
        }

        /// <summary>
        /// Insertion Sort 
        /// </summary>
        static public void InsSort(int[] array)
        {
            for (var i = 1; i < array.Length; i++)
                // Insert i'th record
                for (var j = i; (j > 0) && (array[j] < array[j - 1]); j--)
                //DSutil.swap(array, j, j-1);
                {
                    var tmp = array[j]; array[j] = array[j - 1]; array[j - 1] = tmp;
                }
        }

        static private void QSort_h(int[] array, int oi, int oj)
        {
            var stack = new int[MAXSTACKSIZE]; // Stack for array bounds
            var top = -1;

            stack[++top] = oi; // Initialize stack
            stack[++top] = oj;

            while (top > 0)
            // While there are unprocessed subarrays
            {
                // Pop stack
                var j = stack[top--];
                var i = stack[top--];

                // Findpivot
                var pivotindex = (i + j) / 2;
                var pivot = array[pivotindex];
                //DSutil.swap(array, pivotindex, j); // Stick pivot at end
                var tmp = array[pivotindex]; array[pivotindex] = array[j]; array[j] = tmp;
                // Partition
                var l = i - 1;
                var r = j;
                do
                {
                    while (array[++l] < pivot)
                        ;
                    while ((r != 0) && (array[--r] > pivot))
                        ;
                    //DSutil.swap(array, l, r);
                    tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                }
                while (l < r);
                //DSutil.swap(array, l, r);  // Undo final swap
                tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                //DSutil.swap(array, l, j);  // Put pivot value in place
                tmp = array[l]; array[l] = array[j]; array[j] = tmp;

                // Put new subarrays onto stack if they are small
                if ((l - i) > THRESHOLD)
                // Left partition
                {
                    stack[++top] = i;
                    stack[++top] = l - 1;
                }
                if ((j - l) > THRESHOLD)
                // Right partition 
                {
                    stack[++top] = l + 1;
                    stack[++top] = j;
                }
            }
            InsSort(array); // Final Insertion Sort
        }

        /// <summary>
        /// Non-Recursive QuickSort. 
        /// </summary>
        static public void QSort(int[] array, ISortComparatorL comp)
        {
            QSort_h(array, 0, array.Length - 1, comp);
        }

        /// <summary>
        /// Insertion Sort. 
        /// </summary>
        static public void InsSort(int[] array, ISortComparatorL comp)
        {
            for (var i = 1; i < array.Length; i++)
                // Insert i'th record
                for (var j = i; (j > 0) && (comp.lt(array[j], array[j - 1])); j--)
                //DSutil.swap(array, j, j-1);
                {
                    var tmp = array[j]; array[j] = array[j - 1]; array[j - 1] = tmp;
                }
        }

        static private void QSort_h(int[] array, int oi, int oj, ISortComparatorL comp)
        {
            int[] stack = new int[MAXSTACKSIZE]; // Stack for array bounds
            int top = -1;

            stack[++top] = oi; // Initialize stack
            stack[++top] = oj;

            while (top > 0)
            // While there are unprocessed subarrays
            {
                // Pop stack
                var j = stack[top--];
                var i = stack[top--];

                // Findpivot
                var pivotindex = (i + j) / 2;
                var pivot = array[pivotindex];
                //DSutil.swap(array, pivotindex, j); // Stick pivot at end
                var tmp = array[pivotindex]; array[pivotindex] = array[j]; array[j] = tmp;
                // Partition
                var l = i - 1;
                var r = j;
                do
                {
                    while (comp.lt(array[++l], pivot))
                        ;
                    while ((r != 0) && (comp.gt(array[--r], pivot)))
                        ;
                    //DSutil.swap(array, l, r);
                    tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                }
                while (l < r);
                //DSutil.swap(array, l, r);  // Undo final swap
                tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                //DSutil.swap(array, l, j);  // Put pivot value in place
                tmp = array[l]; array[l] = array[j]; array[j] = tmp;

                // Put new subarrays onto stack if they are small
                if ((l - i) > THRESHOLD)
                // Left partition
                {
                    stack[++top] = i;
                    stack[++top] = l - 1;
                }
                if ((j - l) > THRESHOLD)
                // Right partition 
                {
                    stack[++top] = l + 1;
                    stack[++top] = j;
                }
            }
            InsSort(array, comp); // Final Insertion Sort
        }

        #endregion // Int32
        #region UInt64

        /// <summary>
        /// Non-Recursive QuickSort. 
        /// </summary>
        static public void QSort(ulong[] array)
        {
            QSort_h(array, 0, array.Length - 1);
        }

        /// <summary>
        /// Insertion Sort. 
        /// </summary>
        static public void InsSort(ulong[] array)
        {
            for (var i = 1; i < array.Length; i++)
                // Insert i'th record
                for (var j = i; (j > 0) && (array[j] < array[j - 1]); j--)
                //DSutil.swap(array, j, j-1);
                {
                    var tmp = array[j]; array[j] = array[j - 1]; array[j - 1] = tmp;
                }
        }

        static private void QSort_h(ulong[] array, int oi, int oj)
        {
            var stack = new int[MAXSTACKSIZE]; // Stack for array bounds
            var top = -1;

            stack[++top] = oi; // Initialize stack
            stack[++top] = oj;

            while (top > 0)
            // While there are unprocessed subarrays
            {
                // Pop stack
                var j = stack[top--];
                var i = stack[top--];

                // Findpivot
                var pivotindex = (i + j) / 2;
                var pivot = array[pivotindex];
                //DSutil.swap(array, pivotindex, j); // Stick pivot at end
                var tmp = array[pivotindex]; array[pivotindex] = array[j]; array[j] = tmp;
                // Partition
                var l = i - 1;
                var r = j;
                do
                {
                    while (array[++l] < pivot)
                        ;
                    while ((r != 0) && (array[--r] > pivot))
                        ;
                    //DSutil.swap(array, l, r);
                    tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                }
                while (l < r);
                //DSutil.swap(array, l, r);  // Undo final swap
                tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                //DSutil.swap(array, l, j);  // Put pivot value in place
                tmp = array[l]; array[l] = array[j]; array[j] = tmp;

                // Put new subarrays onto stack if they are small
                if ((l - i) > THRESHOLD)
                // Left partition
                {
                    stack[++top] = i;
                    stack[++top] = l - 1;
                }
                if ((j - l) > THRESHOLD)
                // Right partition 
                {
                    stack[++top] = l + 1;
                    stack[++top] = j;
                }
            }
            InsSort(array); // Final Insertion Sort
        }

        /// <summary>
        /// Non-Recursive QuickSort. 
        /// </summary>
        static public void QSort(ulong[] array, ISortComparatorUL comp)
        {
            QSort_h(array, 0, array.Length - 1, comp);
        }

        /// <summary>
        /// Insertion Sort. 
        /// </summary>
        static public void InsSort(ulong[] array, ISortComparatorUL comp)
        {
            for (var i = 1; i < array.Length; i++)
                // Insert i'th record
                for (var j = i; (j > 0) && (comp.lt(array[j], array[j - 1])); j--)
                //DSutil.swap(array, j, j-1);
                {
                    var tmp = array[j]; array[j] = array[j - 1]; array[j - 1] = tmp;
                }
        }

        static private void QSort_h(ulong[] array, int oi, int oj, ISortComparatorUL comp)
        {
            var stack = new int[MAXSTACKSIZE]; // Stack for array bounds
            var top = -1;

            stack[++top] = oi; // Initialize stack
            stack[++top] = oj;

            while (top > 0)
            // While there are unprocessed subarrays
            {
                // Pop stack
                var j = stack[top--];
                var i = stack[top--];

                // Findpivot
                var pivotindex = (i + j) / 2;
                var pivot = array[pivotindex];
                //DSutil.swap(array, pivotindex, j); // Stick pivot at end
                var tmp = array[pivotindex]; array[pivotindex] = array[j]; array[j] = tmp;
                // Partition
                var l = i - 1;
                var r = j;
                do
                {
                    while (comp.lt(array[++l], pivot))
                        ;
                    while ((r != 0) && (comp.gt(array[--r], pivot)))
                        ;
                    //DSutil.swap(array, l, r);
                    tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                }
                while (l < r);
                //DSutil.swap(array, l, r);  // Undo final swap
                tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                //DSutil.swap(array, l, j);  // Put pivot value in place
                tmp = array[l]; array[l] = array[j]; array[j] = tmp;

                // Put new subarrays onto stack if they are small
                if ((l - i) > THRESHOLD)
                // Left partition
                {
                    stack[++top] = i;
                    stack[++top] = l - 1;
                }
                if ((j - l) > THRESHOLD)
                // Right partition 
                {
                    stack[++top] = l + 1;
                    stack[++top] = j;
                }
            }
            InsSort(array, comp); // Final Insertion Sort
        }

        #endregion // UInt64
        #region Int64

        /// <summary>
        /// Non-Recursive QuickSort. 
        /// </summary>
        static public void QSort(long[] array)
        {
            QSort_h(array, 0, array.Length - 1);
        }

        /// <summary>
        /// Insertion Sort. 
        /// </summary>
        static public void InsSort(long[] array)
        {
            for (var i = 1; i < array.Length; i++)
                // Insert i'th record
                for (var j = i; (j > 0) && (array[j] < array[j - 1]); j--)
                //DSutil.swap(array, j, j-1);
                {
                    var tmp = array[j]; array[j] = array[j - 1]; array[j - 1] = tmp;
                }
        }

        static private void QSort_h(long[] array, int oi, int oj)
        {
            var stack = new int[MAXSTACKSIZE]; // Stack for array bounds
            var top = -1;

            stack[++top] = oi; // Initialize stack
            stack[++top] = oj;

            while (top > 0)
            // While there are unprocessed subarrays
            {
                // Pop stack
                var j = stack[top--];
                var i = stack[top--];

                // Findpivot
                var pivotindex = (i + j) / 2;
                var pivot = array[pivotindex];
                //DSutil.swap(array, pivotindex, j); // Stick pivot at end
                var tmp = array[pivotindex]; array[pivotindex] = array[j]; array[j] = tmp;
                // Partition
                var l = i - 1;
                var r = j;
                do
                {
                    while (array[++l] < pivot)
                        ;
                    while ((r != 0) && (array[--r] > pivot))
                        ;
                    //DSutil.swap(array, l, r);
                    tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                }
                while (l < r);
                //DSutil.swap(array, l, r);  // Undo final swap
                tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                //DSutil.swap(array, l, j);  // Put pivot value in place
                tmp = array[l]; array[l] = array[j]; array[j] = tmp;

                // Put new subarrays onto stack if they are small
                if ((l - i) > THRESHOLD)
                // Left partition
                {
                    stack[++top] = i;
                    stack[++top] = l - 1;
                }
                if ((j - l) > THRESHOLD)
                // Right partition 
                {
                    stack[++top] = l + 1;
                    stack[++top] = j;
                }
            }
            InsSort(array); // Final Insertion Sort
        }

        /// <summary>
        /// Non-Recursive QuickSort. 
        /// </summary>
        static public void QSort(long[] array, ISortComparatorL comp)
        {
            QSort_h(array, 0, array.Length - 1, comp);
        }

        /// <summary>
        /// Insertion Sort. 
        /// </summary>
        static public void InsSort(long[] array, ISortComparatorL comp)
        {
            for (var i = 1; i < array.Length; i++)
                // Insert i'th record
                for (var j = i; (j > 0) && (comp.lt(array[j], array[j - 1])); j--)
                //DSutil.swap(array, j, j-1);
                {
                    long tmp = array[j]; array[j] = array[j - 1]; array[j - 1] = tmp;
                }
        }

        static private void QSort_h(long[] array, int oi, int oj, ISortComparatorL comp)
        {
            int[] stack = new int[MAXSTACKSIZE]; // Stack for array bounds
            int top = -1;

            stack[++top] = oi; // Initialize stack
            stack[++top] = oj;

            while (top > 0)
            // While there are unprocessed subarrays
            {
                // Pop stack
                var j = stack[top--];
                var i = stack[top--];

                // Findpivot
                var pivotindex = (i + j) / 2;
                var pivot = array[pivotindex];
                //DSutil.swap(array, pivotindex, j); // Stick pivot at end
                var tmp = array[pivotindex]; array[pivotindex] = array[j]; array[j] = tmp;
                // Partition
                var l = i - 1;
                var r = j;
                do
                {
                    while (comp.lt(array[++l], pivot))
                        ;
                    while ((r != 0) && (comp.gt(array[--r], pivot)))
                        ;
                    //DSutil.swap(array, l, r);
                    tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                }
                while (l < r);
                //DSutil.swap(array, l, r);  // Undo final swap
                tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                //DSutil.swap(array, l, j);  // Put pivot value in place
                tmp = array[l]; array[l] = array[j]; array[j] = tmp;

                // Put new subarrays onto stack if they are small
                if ((l - i) > THRESHOLD)
                // Left partition
                {
                    stack[++top] = i;
                    stack[++top] = l - 1;
                }
                if ((j - l) > THRESHOLD)
                // Right partition 
                {
                    stack[++top] = l + 1;
                    stack[++top] = j;
                }
            }
            InsSort(array, comp); // Final Insertion Sort
        }

        #endregion // Int64
        #region Float

        /// <summary>
        /// Non-Recursive QuickSort. 
        /// </summary>
        static public void QSort(float[] array)
        {
            QSort_h(array, 0, array.Length - 1);
        }

        /// <summary>
        /// Insertion Sort. 
        /// </summary>
        static public void InsSort(float[] array)
        {
            for (var i = 1; i < array.Length; i++)
                // Insert i'th record
                for (var j = i; (j > 0) && (array[j] < array[j - 1]); j--)
                //DSutil.swap(array, j, j-1);
                {
                    var tmp = array[j];
                    array[j] = array[j - 1];
                    array[j - 1] = tmp;
                }
        }

        static private void QSort_h(float[] array, int oi, int oj)
        {
            var stack = new int[MAXSTACKSIZE]; // Stack for array bounds
            var top = -1;

            stack[++top] = oi; // Initialize stack
            stack[++top] = oj;

            while (top > 0)
            // While there are unprocessed subarrays
            {
                // Pop stack
                var j = stack[top--];
                var i = stack[top--];

                // Findpivot
                var pivotindex = (i + j) / 2;
                var pivot = array[pivotindex];
                //DSutil.swap(array, pivotindex, j); // Stick pivot at end
                var tmp = array[pivotindex]; array[pivotindex] = array[j]; array[j] = tmp;
                // Partition
                var l = i - 1;
                var r = j;
                do
                {
                    while (array[++l] < pivot)
                        ;
                    while ((r != 0) && (array[--r] > pivot))
                        ;
                    //DSutil.swap(array, l, r);
                    tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                }
                while (l < r);
                //DSutil.swap(array, l, r);  // Undo final swap
                tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                //DSutil.swap(array, l, j);  // Put pivot value in place
                tmp = array[l]; array[l] = array[j]; array[j] = tmp;

                // Put new subarrays onto stack if they are small
                if ((l - i) > THRESHOLD)
                // Left partition
                {
                    stack[++top] = i;
                    stack[++top] = l - 1;
                }
                if ((j - l) > THRESHOLD)
                // Right partition 
                {
                    stack[++top] = l + 1;
                    stack[++top] = j;
                }
            }
            InsSort(array); // Final Insertion Sort
        }

        #endregion // Float
        #region Double

        /// <summary>
        /// Non-Recursive QuickSort. 
        /// </summary>
        static public void QSort(double[] array)
        {
            QSort_h(array, 0, array.Length - 1);
        }

        /// <summary>
        /// Insertion Sort. 
        /// </summary>
        static public void InsSort(double[] array)
        {
            for (var i = 1; i < array.Length; i++)
                // Insert i'th record
                for (var j = i; (j > 0) && (array[j] < array[j - 1]); j--)
                //DSutil.swap(array, j, j-1);
                {
                    var tmp = array[j]; array[j] = array[j - 1]; array[j - 1] = tmp;
                }
        }

        static private void QSort_h(double[] array, int oi, int oj)
        {
            int[] stack = new int[MAXSTACKSIZE]; // Stack for array bounds
            int top = -1;

            stack[++top] = oi; // Initialize stack
            stack[++top] = oj;

            while (top > 0)
            // While there are unprocessed subarrays
            {
                // Pop stack
                var j = stack[top--];
                var i = stack[top--];

                // Findpivot
                var pivotindex = (i + j) / 2;
                var pivot = array[pivotindex];
                //DSutil.swap(array, pivotindex, j); // Stick pivot at end
                var tmp = array[pivotindex]; array[pivotindex] = array[j]; array[j] = tmp;
                // Partition
                var l = i - 1;
                var r = j;
                do
                {
                    while (array[++l] < pivot)
                        ;
                    while ((r != 0) && (array[--r] > pivot))
                        ;
                    //DSutil.swap(array, l, r);
                    tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                }
                while (l < r);
                //DSutil.swap(array, l, r);  // Undo final swap
                tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                //DSutil.swap(array, l, j);  // Put pivot value in place
                tmp = array[l]; array[l] = array[j]; array[j] = tmp;

                // Put new subarrays onto stack if they are small
                if ((l - i) > THRESHOLD)
                // Left partition
                {
                    stack[++top] = i;
                    stack[++top] = l - 1;
                }
                if ((j - l) > THRESHOLD)
                // Right partition 
                {
                    stack[++top] = l + 1;
                    stack[++top] = j;
                }
            }
            InsSort(array); // Final Insertion Sort
        }

        #endregion // Double
        #region Object
        /// <summary>
        /// Non-Recursive QuickSort. 
        /// </summary>
        static public void QSort(object[] array, ISortComparator comp)
        {
            QSort_h(array, 0, array.Length - 1, comp);
        }

        /// <summary>
        /// Insertion Sort. 
        /// </summary>
        static public void InsSort(object[] array, ISortComparator comp)
        {
            for (var i = 1; i < array.Length; i++)
                // Insert i'th record
                for (var j = i; (j > 0) && (comp.lt(array[j], array[j - 1])); j--)
                //DSutil.swap(array, j, j-1);
                {
                    object tmp = array[j]; array[j] = array[j - 1]; array[j - 1] = tmp;
                }
        }

        static private void QSort_h(object[] array, int oi, int oj, ISortComparator comp)
        {
            int[] stack = new int[MAXSTACKSIZE]; // Stack for array bounds
            int top = -1;

            stack[++top] = oi; // Initialize stack
            stack[++top] = oj;

            while (top > 0)
            // While there are unprocessed subarrays
            {
                // Pop stack
                var j = stack[top--];
                var i = stack[top--];

                // Findpivot
                var pivotindex = (i + j) / 2;
                var pivot = array[pivotindex];
                //DSutil.swap(array, pivotindex, j); // Stick pivot at end
                var tmp = array[pivotindex]; array[pivotindex] = array[j]; array[j] = tmp;
                // Partition
                var l = i - 1;
                var r = j;
                do
                {
                    while (comp.lt(array[++l], pivot))
                        ;
                    while ((r != 0) && (comp.gt(array[--r], pivot)))
                        ;
                    //DSutil.swap(array, l, r);
                    tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                }
                while (l < r);
                //DSutil.swap(array, l, r);  // Undo final swap
                tmp = array[l]; array[l] = array[r]; array[r] = tmp;
                //DSutil.swap(array, l, j);  // Put pivot value in place
                tmp = array[l]; array[l] = array[j]; array[j] = tmp;

                // Put new subarrays onto stack if they are small
                if ((l - i) > THRESHOLD)
                // Left partition
                {
                    stack[++top] = i;
                    stack[++top] = l - 1;
                }
                if ((j - l) > THRESHOLD)
                // Right partition 
                {
                    stack[++top] = l + 1;
                    stack[++top] = j;
                }
            }
            InsSort(array, comp); // Final Insertion Sort
        }

        #endregion // Object

        #endregion // Operations
    }

    /* The original code
    
    // Non-Recursive QuickSort
    static public void QSort(Elem[] array) 
    {
    qsort(array, 0, array.length-1);
    }
    
    static public int MAXSTACKSIZE = 1000;
    
    // Insertion Sort
    static void InsSort(Elem[] array) 
    {
    Elem tmp;
    
    for (var i=1; i<array.length; i++) // Insert i'th record
    for (var j=i; (j>0) && (array[j].key()<array[j-1].key()); j--)
    DSUtil.swap(array, j, j-1);
    }
    
    static private void QSort_h(Elem[] array, int oi, int oj) 
    {
    int[] stack = new int[MAXSTACKSIZE]; // Stack for array bounds
    int listsize = oj-oi+1;
    int top = -1;
    int pivot;
    int pivotindex, l, r;
    Elem tmp;
    
    stack[++top] = oi;  // Initialize stack
    stack[++top] = oj;
    
    while (top > 0)    // While there are unprocessed subarrays
    {
    // Pop stack
    int j = stack[top--];
    int i = stack[top--];
    
    // Findpivot
    pivotindex = (i+j)/2;
    pivot = array[pivotindex].key();
    DSUtil.swap(array, pivotindex, j); // Stick pivot at end
    // Partition
    l = i-1;
    r = j;
    do 
    {
    while (array[++l].key() < pivot);
    while ((r!=0) && (array[--r].key() > pivot));
    DSutil.swap(array, l, r);
    } while (l < r);
    DSUtil.swap(array, l, r);  // Undo final swap
    DSUtil.swap(array, l, j);  // Put pivot value in place
    
    // Put new subarrays onto stack if they are small
    if ((l-i) > THRESHOLD)   // Left partition
    { 
    stack[++top] = i;
    stack[++top] = l-1;
    }
    if ((j-l) > THRESHOLD) // Right partition 
    {   
    stack[++top] = l+1;
    stack[++top] = j;
    }
    }
    InsSort(array);             // Final Insertion Sort
    }*/
}