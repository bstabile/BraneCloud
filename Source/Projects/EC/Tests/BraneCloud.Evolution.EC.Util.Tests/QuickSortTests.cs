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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.Util.Tests
{
    /// <summary>
    /// Summary description for QuickSortTests
    /// </summary>
    [TestClass]
    public class QuickSortTests
    {
        #region Housekeeping

        public QuickSortTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private TestContext context { get { return testContextInstance; } }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #endregion // Housekeeping

        #region Char

        [TestMethod]
        [Description("Ensures that QuickSort results for a Char array are equal to Array.Sort() results. (Performance is not compared.)")]
        public void CharArray()
        {
            var n = 1000;
            var rand = new MersenneTwisterFast(0L);
            var r = new char[n];
            for (var i = 0; i < n; i++)
            {
                r[i] = rand.NextChar();
            }
                
            var a = (char[])r.Clone();

            Array.Sort(r); // Reference
            QuickSort.QSort(a); // Test

            // First we just ensure that values are sorted.
            var last = r[0];
            for (var i = 1; i < r.Length; i++)
            {
                Assert.IsTrue(r[i] >= last);
            }
            // Now we make sure both arrays are in the same order.
            for (var i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(r[i], a[i]);
            }
        }

        #endregion // Char
        #region Byte

        [TestMethod]
        [Description("Ensures that QuickSort results for an Byte array are equal to Array.Sort() results. (Performance is not compared.)")]
        public void ByteArray()
        {
            var n = 1000;
            var rand = new MersenneTwisterFast(0L);
            var r = new byte[n];
            rand.NextBytes(r);
            var a = (byte[])r.Clone();

            Array.Sort(r); // Reference
            QuickSort.QSort(a); // Test

            // First we just ensure that values are sorted.
            var last = r[0];
            for (var i = 1; i < r.Length; i++)
            {
                Assert.IsTrue(r[i] >= last);
            }
            // Now we make sure both arrays are in the same order.
            for (var i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(r[i], a[i]);
            }
        }

        #endregion // Byte
        #region SByte

        [TestMethod]
        [Description("Ensures that QuickSort results for an SByte array are equal to Array.Sort() results. (Performance is not compared.)")]
        public void SByteArray()
        {
            var n = 1000;
            var rand = new MersenneTwisterFast(0L);
            var r = new sbyte[n];
            rand.NextBytes(r);
            var a = (sbyte[])r.Clone();

            Array.Sort(r); // Reference
            QuickSort.QSort(a); // Test

            // First we just ensure that values are sorted.
            var last = r[0];
            for (var i = 1; i < r.Length; i++)
            {
                Assert.IsTrue(r[i] >= last);
            }
            // Now we make sure both arrays are in the same order.
            for (var i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(r[i], a[i]);
            }
        }

        #endregion // SByte
        #region UInt16

        [TestMethod]
        [Description("Ensures that QuickSort results for an UInt16 array are equal to Array.Sort() results. (Performance is not compared.)")]
        public void UInt16Array()
        {
            var n = 1000;
            var rand = new MersenneTwisterFast(0L);
            var r = new ushort[n];
            for (var i = 0; i < n; i++)
            {
                r[i] = (ushort)rand.NextShort();
            }
            var a = (ushort[])r.Clone();

            Array.Sort(r); // Reference
            QuickSort.QSort(a); // Test

            // First we just ensure that values are sorted.
            var last = r[0];
            for (var i = 1; i < r.Length; i++)
            {
                Assert.IsTrue(r[i] >= last);
            }
            // Now we make sure both arrays are in the same order.
            for (var i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(r[i], a[i]);
            }
        }

        #endregion // UInt16
        #region Int16

        [TestMethod]
        [Description("Ensures that QuickSort results for an Int16 array are equal to Array.Sort() results. (Performance is not compared.)")]
        public void Int16Array()
        {
            var n = 1000;
            var rand = new MersenneTwisterFast(0L);
            var r = new short[n];
            for (var i = 0; i < n; i++)
            {
                r[i] = rand.NextShort();
            }
            var a = (short[])r.Clone();

            Array.Sort(r); // Reference
            QuickSort.QSort(a); // Test

            // First we just ensure that values are sorted.
            var last = r[0];
            for (var i = 1; i < r.Length; i++)
            {
                Assert.IsTrue(r[i] >= last);
            }
            // Now we make sure both arrays are in the same order.
            for (var i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(r[i], a[i]);
            }
        }

        #endregion // UInt16
        #region UInt32

        [TestMethod]
        [Description("Ensures that QuickSort results for an UInt32 array are equal to Array.Sort() results. (Performance is not compared.)")]
        public void UInt32Array()
        {
            var n = 1000;
            var rand = new MersenneTwisterFast(0L);
            var r = new uint[n];
            for (var i = 0; i < n; i++)
            {
                r[i] = (uint)rand.NextInt();
            }               
            var a = (uint[])r.Clone();

            Array.Sort(r); // Reference
            QuickSort.QSort(a); // Test

            // First we just ensure that values are sorted.
            var last = r[0];
            for (var i = 1; i < r.Length; i++)
            {
                Assert.IsTrue(r[i] >= last);
            }
            // Now we make sure both arrays are in the same order.
            for (var i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(r[i], a[i]);
            }
        }

        #endregion // UInt32
        #region Int32

        [TestMethod]
        [Description("Ensures that QuickSort results for an Int32 array are equal to Array.Sort() results. (Performance is not compared.)")]
        public void Int32Array()
        {
            var n = 1000;
            var rand = new MersenneTwisterFast(0L);
            var r = new int[n];
            for (var i = 0; i < n; i++)
            {
                r[i] = rand.NextInt();
            }
            var a = (int[])r.Clone();

            Array.Sort(r); // Reference
            QuickSort.QSort(a); // Test

            // First we just ensure that values are sorted.
            var last = r[0];
            for (var i = 1; i < r.Length; i++)
            {
                Assert.IsTrue(r[i] >= last);
            }
            // Now we make sure both arrays are in the same order.
            for (var i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(r[i], a[i]);
            }
        }

        #endregion // UInt32
        #region UInt64

        [TestMethod]
        [Description("Ensures that QuickSort results for an UInt64 array are equal to Array.Sort() results. (Performance is not compared.)")]
        public void UInt64Array()
        {
            var n = 1000;
            var rand = new MersenneTwisterFast(0L);
            var r = new ulong[n];
            for (var i = 0; i < n; i++)
            {
                r[i] = (ulong)rand.NextLong();
            }
            var a = (ulong[])r.Clone();

            Array.Sort(r); // Reference
            QuickSort.QSort(a); // Test

            // First we just ensure that values are sorted.
            var last = r[0];
            for (var i = 1; i < r.Length; i++)
            {
                Assert.IsTrue(r[i] >= last);
            }
            // Now we make sure both arrays are in the same order.
            for (var i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(r[i], a[i]);
            }
        }

        #endregion // UInt32
        #region Int64

        [TestMethod]
        [Description("Ensures that QuickSort results for an Int64 array are equal to Array.Sort() results. (Performance is not compared.)")]
        public void Int64Array()
        {
            var n = 1000;
            var rand = new MersenneTwisterFast(0L);
            var r = new long[n];
            for (var i = 0; i < n; i++)
            {
                r[i] = rand.NextLong();
            }
            var a = (long[])r.Clone();

            Array.Sort(r); // Reference
            QuickSort.QSort(a); // Test

            // First we just ensure that values are sorted.
            var last = r[0];
            for (var i = 1; i < r.Length; i++)
            {
                Assert.IsTrue(r[i] >= last);
            }
            // Now we make sure both arrays are in the same order.
            for (var i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(r[i], a[i]);
            }
        }

        #endregion // UInt32
        #region Float

        [TestMethod]
        [Description("Ensures that QuickSort results for an Float array are equal to Array.Sort() results. (Performance is not compared.)")]
        public void FloatArray()
        {
            var n = 1000;
            var rand = new MersenneTwisterFast(0L);
            var r = new float[n];
            for (var i = 0; i < n; i++)
            {
                r[i] = rand.NextFloat();
            }
            var a = (float[])r.Clone();

            Array.Sort(r); // Reference
            QuickSort.QSort(a); // Test

            // First we just ensure that values are sorted.
            var last = r[0];
            for (var i = 1; i < r.Length; i++)
            {
                Assert.IsTrue(r[i] >= last);
            }
            // Now we make sure both arrays are in the same order.
            for (var i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(r[i], a[i]);
            }
        }

        #endregion // Float
        #region Double

        [TestMethod]
        [Description("Ensures that QuickSort results for an Float array are equal to Array.Sort() results. (Performance is not compared.)")]
        public void DoubleArray()
        {
            var n = 1000;
            var rand = new MersenneTwisterFast(0L);
            var r = new double[n];
            for (var i = 0; i < n; i++)
            {
                r[i] = rand.NextDouble();
            }
            var a = (double[])r.Clone();

            Array.Sort(r); // Reference
            QuickSort.QSort(a); // Test

            // First we just ensure that values are sorted.
            var last = r[0];
            for (var i = 1; i < r.Length; i++)
            {
                Assert.IsTrue(r[i] >= last);
            }
            // Now we make sure both arrays are in the same order.
            for (var i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(r[i], a[i]);
            }
        }

        #endregion // Double
        #region Object Array With Custom SortComparator

        [TestMethod]
        [Description("Ensures that QuickSort results for an Object array with a custom SortComparator" 
            + " are equal to {Array.Sort();Array.Reverse()} results. (Performance is not compared.)")]
        public void ObjectArray()
        {
            var n = 1000;
            var rand = new MersenneTwisterFast(0L);
            var r = new object[n];
            for (var i = 0; i < n; i++)
            {
                r[i] = rand.NextChar();
            }
            var a = (object[])r.Clone();

            Array.Sort(r); // Reference
            Array.Reverse(r); // Reverse the sorted array so we can check results against those produced by the custom comparator.
            QuickSort.QSort(a, new CharReverseSortComparator()); // Test

            // First we just ensure that values are sorted.
            var last = r[0];
            for (var i = 1; i < r.Length; i++)
            {
                Assert.IsTrue(((char)r[i]) <= ((char)last));
            }
            // Now we make sure both arrays are in the same order.
            for (var i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(((char)r[i]), ((char)a[i]));
            }
        }

        /// <summary>
        /// This custom ISortComparator is used in the ObjectArray test. It simply sorts characters in reverse order.
        /// </summary>
        private class CharReverseSortComparator : ISortComparator
        {
            public bool lt(object o1, object o2)
            {
                return ((char) o1) > ((char) o2) ? true : false;
            }
            public bool gt(object o1, object o2)
            {
                return ((char)o1) < ((char)o2) ? true : false;
            }
        }

        #endregion // Object Array With Custom SortComparator
    }
}