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
    /// PropertiesTreeTests
    /// </summary>
    [TestClass]
    public class FileDictionaryTreeTests
    {
        #region Housekeeping

        public FileDictionaryTreeTests()
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
        public void DefaultConstructionAndSimplePropertyInheritance()
        {
            var child = new FileDictionaryTree();
            var parent = new FileDictionaryTree();
            var grandparent = new FileDictionaryTree();

            parent.Parents.Add(grandparent);
            child.Parents.Add(parent);

            grandparent.Add("prop1", "grandparent");
            Assert.AreEqual(child["prop1"], "grandparent"); // inherits prop1 from grandparent

            parent.Add("prop1", "parent");
            Assert.AreEqual(child["prop1"], "parent"); // inherits prop1 from parent

            child.Add("prop1", "child"); // overriding prop1 locally
            Assert.AreEqual(child["prop1"], "child");

            var lines = child.ToString("").Split('\n');
            foreach (var line in lines)
            {
                context.WriteLine(line);
            }
            var xml = child.ToXml();
            context.WriteLine(child.ToXml().ToString());
        }

        [TestMethod]
        public void SimpleDefaultsInheritance()
        {
            var child = new FileDictionaryTree("child");
            var parent = new FileDictionaryTree("parent");
            var grandparent = new FileDictionaryTree("grandparent");

            parent.Parents.Add(grandparent);
            child.Parents.Add(parent);

            grandparent.AddDefault("prop1", "grandparent");
            Assert.AreEqual(child["prop1"], "grandparent"); // inherits default prop1 from grandparent

            parent.AddDefault("prop1", "parent");
            Assert.AreEqual(child["prop1", true], "parent"); // inherits default prop1 from parent

            child.AddDefault("prop1", "child"); // overriding default prop1 locally
            Assert.AreEqual(child["prop1", true], "child");

            var lines = child.ToString("").Split('\n');
            foreach (var line in lines)
            {
                context.WriteLine(line);
            }
            var xml = child.ToXml();
            context.WriteLine(child.ToXml().ToString());
        }
    }
}