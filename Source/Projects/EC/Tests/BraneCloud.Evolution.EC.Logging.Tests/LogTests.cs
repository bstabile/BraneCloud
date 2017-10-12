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
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BraneCloud.Evolution.EC.Logging.Tests
{
    /// <summary>
    /// Log Tests
    /// 
    /// Just the basics for now.
    /// </summary>
    [TestClass]
    public class LogTests
    {
        #region Housekeeping

        public LogTests()
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

        #region Construction

        [TestMethod]
        public void ConstructWithStringFilename()
        {
            var fileName = MethodInfo.GetCurrentMethod().Name + ".log";
            var log = new Log(fileName, true, true);
            Assert.IsNotNull(log);
            Assert.IsTrue(File.Exists(fileName));
        }

        [TestMethod]
        public void ConstructWithFileInfo()
        {
            var fileName = MethodInfo.GetCurrentMethod().Name + ".log";
            var fileInfo = new FileInfo(fileName);
            var log = new Log(fileInfo, true, true);
            Assert.IsNotNull(log);
            Assert.IsTrue(File.Exists(fileName));

        }

        [TestMethod]
        [Description("Shows that AppendOnRestart and GZip compression are NOT compatible.")]
        [ExpectedException(typeof(IOException))]
        public void ConstructWithAppendOnRestartAndGZipCompressionThrows()
        {
            var fileName = MethodInfo.GetCurrentMethod().Name + ".log";
            var log = new Log(new FileInfo(fileName), true, true, true);
            Assert.IsNotNull(log);
            Assert.IsTrue(File.Exists(fileName));
        }

        [TestMethod]
        [Description("Shows that GZip compression is okay if AppendOnRestart is false.")]
        public void ConstructWithGZipCompression()
        {
            var fileName = MethodInfo.GetCurrentMethod().Name + ".log";
            var log = new Log(new FileInfo(fileName), true, false, true);
            Assert.IsNotNull(log);
            Assert.IsTrue(File.Exists(fileName));
        }

        #endregion // Construction
    }
}