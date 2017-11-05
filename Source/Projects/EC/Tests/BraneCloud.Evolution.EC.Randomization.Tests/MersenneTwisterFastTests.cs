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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BraneCloud.Evolution.EC.Randomization.Tests
{
    [TestClass]
    public class MersenneTwisterFastTests
    {
        #region Housekeeping

        public MersenneTwisterFastTests()
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

        [Ignore]
        [TestMethod]
        [Description("This shows the MTF to be over 4 times slower than System.Random.")]
        public void SystemRandomComparison()
        {
            var sb = new StringBuilder();

            var r = new MersenneTwisterFast(new int[] { 0x123, 0x234, 0x345, 0x456 });

            // SPEED TEST

            sb.AppendLine("\nTime to test grabbing 100000000 ints");

            const long SEED = 4357;
            int j;

            var rr = new Random((Int32)SEED);
            var xx = 0;
            var ms = DateTimeHelper.CurrentTimeMilliseconds;
            for (j = 0; j < 100000000; j++)
            {
                xx += rr.Next();
            }
            sb.AppendLine("System.Random: " + (DateTimeHelper.CurrentTimeMilliseconds - ms) + "          Ignore this: " + xx);

            r = new MersenneTwisterFast(SEED);
            ms = DateTimeHelper.CurrentTimeMilliseconds;
            xx = 0;
            for (j = 0; j < 100000000; j++)
                xx += r.NextInt();
            sb.AppendLine("Mersenne Twister Fast: " + (DateTimeHelper.CurrentTimeMilliseconds - ms) + "          Ignore this: " + xx);

            context.WriteLine(sb.ToString());
        }

        [Ignore]
        [TestMethod]
        [Description("This tests shows the MTF to be about 2.5 times faster than another MT implementation (see MersenneTwisterPlaire).")]
        public void MersenneTwisterPlaireComparison()
        {
            var sb = new StringBuilder();

            var r = new MersenneTwisterFast(new int[] { 0x123, 0x234, 0x345, 0x456 });

            // SPEED TEST

            sb.AppendLine("\nTime to test grabbing 100000000 ints");

            const long SEED = 4357;
            int j;

            var rr = new MersenneTwisterPlaire((Int32)SEED);
            var xx = 0;
            var ms = DateTimeHelper.CurrentTimeMilliseconds;
            for (j = 0; j < 100000000; j++)
            {
                xx += rr.Next();
            }
            sb.AppendLine("MersenneTwisterPlaire: " + (DateTimeHelper.CurrentTimeMilliseconds - ms) + "          Ignore this: " + xx);

            r = new MersenneTwisterFast(SEED);
            ms = DateTimeHelper.CurrentTimeMilliseconds;
            xx = 0;
            for (j = 0; j < 100000000; j++)
                xx += r.NextInt();
            sb.AppendLine("MersenneTwisterFast: " + (DateTimeHelper.CurrentTimeMilliseconds - ms) + "          Ignore this: " + xx);

            context.WriteLine(sb.ToString());
        }

        [Ignore]
        [TestMethod]
        public void MersenneTwisterFastCorrectnessTest()
        {
            // CORRECTNESS TEST
            // COMPARE WITH http://www.math.keio.ac.jp/matumoto/CODES/MT2002/mt19937ar.out
            
            var sb = new StringBuilder();

            var r = new MersenneTwisterFast(new int[] { 0x123, 0x234, 0x345, 0x456 });

            int j;

            sb.AppendLine("Output of MersenneTwisterFast with new (2002/1/26) seeding mechanism");
            for (j = 0; j < 1000; j++)
            {
                // first, convert the int from signed to "unsigned"
                var l = (long)r.NextInt();
                if (l < 0)
                    l += 4294967296L; // max int value
                var s = Convert.ToString(l);
                while (s.Length < 10)
                    s = " " + s; // buffer
                sb.Append(s + " ");
                if (j % 5 == 4)
                    sb.AppendLine();
            }

            // SPEED TEST

            const long SEED = 4357;

            sb.AppendLine("\nTime to test grabbing 100000000 ints");

            var rr = new Random((Int32)SEED);
            var xx = 0;
            var ms = DateTimeHelper.CurrentTimeMilliseconds;
            for (j = 0; j < 100000000; j++)
            {
                xx += rr.Next();
            }
            sb.AppendLine("System.Random: " + (DateTimeHelper.CurrentTimeMilliseconds - ms) + "          Ignore this: " + xx);

            r = new MersenneTwisterFast(SEED);
            ms = DateTimeHelper.CurrentTimeMilliseconds;
            xx = 0;
            for (j = 0; j < 100000000; j++)
                xx += r.NextInt();
            sb.AppendLine("Mersenne Twister Fast: " + (DateTimeHelper.CurrentTimeMilliseconds - ms) + "          Ignore this: " + xx);

            // TEST TO COMPARE TYPE CONVERSION BETWEEN
            // MersenneTwisterFast.java AND MersenneTwister.java

            sb.AppendLine("\nGrab the first 1000 booleans");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                sb.Append(r.NextBoolean() + " ");
                if (j % 8 == 7)
                    sb.AppendLine();
            }
            if (j % 8 != 7)
                sb.AppendLine();

            sb.AppendLine("\nGrab 1000 booleans of increasing probability using nextBoolean(double)");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                sb.Append(r.NextBoolean(j / 999.0) + " ");
                if (j % 8 == 7)
                    sb.AppendLine();
            }
            if (j % 8 != 7)
                sb.AppendLine();

            sb.AppendLine("\nGrab 1000 booleans of increasing probability using nextBoolean(float)");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                sb.Append(r.NextBoolean(j / 999.0f) + " ");
                if (j % 8 == 7)
                    sb.AppendLine();
            }
            if (j % 8 != 7)
                sb.AppendLine();

            var bytes = new sbyte[1000];
            sb.AppendLine("\nGrab the first 1000 bytes using nextBytes");
            r = new MersenneTwisterFast(SEED);
            r.NextBytes(bytes);
            for (j = 0; j < 1000; j++)
            {
                sb.Append(bytes[j] + " ");
                if (j % 16 == 15)
                    sb.AppendLine();
            }
            if (j % 16 != 15)
                sb.AppendLine();

            sb.AppendLine("\nGrab the first 1000 bytes -- must be same as nextBytes");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                sbyte b;
                sb.Append((b = r.NextByte()) + " ");
                if (b != bytes[j])
                    sb.Append("BAD ");
                if (j % 16 == 15)
                    sb.AppendLine();
            }
            if (j % 16 != 15)
                sb.AppendLine();

            sb.AppendLine("\nGrab the first 1000 shorts");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                sb.Append(r.NextShort() + " ");
                if (j % 8 == 7)
                    sb.AppendLine();
            }
            if (j % 8 != 7)
                sb.AppendLine();

            sb.AppendLine("\nGrab the first 1000 ints");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                sb.Append(r.NextInt() + " ");
                if (j % 4 == 3)
                    sb.AppendLine();
            }
            if (j % 4 != 3)
                sb.AppendLine();

            sb.AppendLine("\nGrab the first 1000 ints of different sizes");
            r = new MersenneTwisterFast(SEED);
            var max = 1;
            for (j = 0; j < 1000; j++)
            {
                sb.Append(r.NextInt(max) + " ");
                max *= 2;
                if (max <= 0)
                    max = 1;
                if (j % 4 == 3)
                    sb.AppendLine();
            }
            if (j % 4 != 3)
                sb.AppendLine();

            sb.AppendLine("\nGrab the first 1000 longs");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                sb.Append(r.NextLong() + " ");
                if (j % 3 == 2)
                    sb.AppendLine();
            }
            if (j % 3 != 2)
                sb.AppendLine();

            sb.AppendLine("\nGrab the first 1000 longs of different sizes");
            r = new MersenneTwisterFast(SEED);
            long max2 = 1;
            for (j = 0; j < 1000; j++)
            {
                sb.Append(r.NextLong(max2) + " ");
                max2 *= 2;
                if (max2 <= 0)
                    max2 = 1;
                if (j % 4 == 3)
                    sb.AppendLine();
            }
            if (j % 4 != 3)
                sb.AppendLine();

            sb.AppendLine("\nGrab the first 1000 floats");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                sb.Append(r.NextFloat() + " ");
                if (j % 4 == 3)
                    sb.AppendLine();
            }
            if (j % 4 != 3)
                sb.AppendLine();

            sb.AppendLine("\nGrab the first 1000 doubles");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                sb.Append(r.NextDouble() + " ");
                if (j % 3 == 2)
                    sb.AppendLine();
            }
            if (j % 3 != 2)
                sb.AppendLine();

            sb.AppendLine("\nGrab the first 1000 gaussian doubles");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                sb.Append(r.NextGaussian() + " ");
                if (j % 3 == 2)
                    sb.AppendLine();
            }
            if (j % 3 != 2)
                sb.AppendLine();

            context.WriteLine(sb.ToString());
        }

    }
}