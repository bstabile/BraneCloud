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

namespace BraneCloud.Evolution.EC.Configuration.Tests
{
    /// <summary>
    /// Summary description for ParameterTests
    /// </summary>
    [TestClass]
    public class ParameterTests
    {
        #region Housekeeping

        public ParameterTests()
        {
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

        [TestMethod]
        public void PushCreatesNewTargetParameterWithoutChangingSource()
        {
            var p1 = new Parameter("param1");
            p1.Push("secondLevel");
            Assert.AreEqual(p1.Param, "param1");
            var p2 = p1.Push("secondLevel");
            Assert.AreEqual(p2.Param, "param1.secondLevel");
        }

        [TestMethod]
        public void AppendedPathArgumentsSeparatedByDot()
        {
            var p1 = new Parameter("param1", new[]{"level2", "level3"});
            Assert.AreEqual(p1.Param, "param1.level2.level3");
        }

        [TestMethod]
        public void PushCreatesNewParamWithAppendedPathSegment()
        {
            var p1 = new Parameter("param1", new[] { "level2", "level3" });
            var p2 = p1.Push("level4");
            Assert.AreEqual(p2.Param, "param1.level2.level3.level4");
        }

        [TestMethod]
        public void PopCreatesNewParamTruncatedPath()
        {
            var p1 = new Parameter("param1", new[] { "level2", "level3" });
            var p2 = p1.Pop();
            Assert.AreEqual(p2.Param, "param1.level2");
        }

        [TestMethod]
        public void PopPushReplacesTailPathSegment()
        {
            var p1 = new Parameter("param1", new[] { "level2", "level3" });
            var p2 = p1.Pop().Push("level5");
            Assert.AreEqual(p2.Param, "param1.level2.level5");
        }

        [TestMethod]
        public void PopPopPushReplacesTwoTrailingSegmentsWithNewOne()
        {
            var p1 = new Parameter("param1", new[] { "level2", "level3" });
            var p2 = p1.Pop().Pop().Push("level5");
            Assert.AreEqual(p2.Param, "param1.level5");
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void PopAllSegmentsSetsParamPropertyToNull()
        {
            var p1 = new Parameter("param1", new[] {"level2", "level3"});
            var p2 = p1.Pop().Pop().Pop();
            Assert.AreEqual(p2.Param, ""); // p2.Param property is null at this point!
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void PushWhenParamPropertyIsNullThrows()
        {
            var p1 = new Parameter("param1", new[] { "level2", "level3" });
            var p2 = p1.Pop().Pop().Pop().Push("param2");
            Assert.AreEqual(p2.Param, "param2"); // p2.Param property is null at this point!
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void AttemptToResetNullParamPropertyDoesNotWork()
        {
            var p1 = new Parameter("param1", new[] { "level2", "level3" });
            var p2 = p1.Pop().Pop().Pop();
            p2.Param = ""; // Setting p2.Param property after it has been nulled is not allowed!
        }

        [TestMethod]
        public void AttemptToResetCurrentValidParamPropertyIsOkay()
        {
            var p1 = new Parameter("param1", new[] { "level2", "level3" });
            p1.Param = "Hello World!";
            Assert.AreEqual(p1.Param, "Hello World!");
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void AttemptToPopNonExistentSegmentThrows()
        {
            var p1 = new Parameter("param1", new[] { "level2", "level3" });
            var p2 = p1.Pop().Pop().Pop().Pop();
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void TestingForNullParamValueThrows()
        {
            var p1 = new Parameter("param1", new[] { "level2", "level3" });
            var p2 = p1.Pop().Pop().Pop();
            Assert.IsNull(p2.Param); // Not too sure this is a good behavior!
        }

        [TestMethod]
        public void TestingForNullParamValueAfterSettingItToNullIsOkay()
        {
            var p1 = new Parameter("param1", new[] { "level2", "level3" });
            p1.Param = null;
            Assert.IsNull(p1.Param); // Not too sure this is a good behavior!
            // You cannot pop all path items and then perform this test for null.
        }

        [TestMethod]
        public void SpeedThingsUpWithPopN()
        {
            var p1 = new Parameter("param1", new[] { "level2", "level3", "level4", "level5" });
            var p2 = p1.PopN(4);
            Assert.AreEqual(p2.Param, "param1"); // Not too sure this is a good behavior!
        }


        [TestMethod]
        public void TopGivesTheLastPathItem()
        {
            var p1 = new Parameter("param1", new[] { "level2", "level3", "level4", "level5" });
            var top = p1.Top();
            Assert.AreEqual(top, "level5");
            top = p1.Pop().Top();
            Assert.AreEqual(top, "level4");
            top = p1.Pop().Pop().Top();
            Assert.AreEqual(top, "level3");
            top = p1.Pop().Pop().Pop().Top();
            Assert.AreEqual(top, "level2");
            top = p1.Pop().Pop().Pop().Pop().Top();
            Assert.AreEqual(top, "param1");
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void TopWhenLastPathItemHasBeenPoppedThrows()
        {
            var p1 = new Parameter("param1", new[] { "level2" });
            context.WriteLine(p1.ToString());
            var top = p1.Top();
            Assert.AreEqual(top, "level2");
            top = p1.Pop().Top();
            Assert.AreEqual(top, "param1");
            top = p1.Pop().Pop().Top(); // Instance returned by final pop is null!
        }

        [TestMethod]
        public void ToStringYieldsParamProperty()
        {
            var p1 = new Parameter("param1", new[] { "level2" });
            Assert.AreEqual(p1.ToString(), "param1.level2");
        }
    }
}