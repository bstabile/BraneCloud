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
using System.Diagnostics;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BraneCloud.Evolution.EC.Configuration.Tests
{
    /// <summary>
    /// This fixture attempts to locate and parse all known parameter files.
    /// The files are located relative to the deployment directory,
    /// so it is important to make whatever adjustments are necessary if these
    /// are to be run on a remote test server or CI environment.
    /// NOTE: Although this is named "Ordered..", these <i>should</i> run fine in any order.
    /// </summary>
    [TestClass]
    public class OrderedFileParsingTests
    {
        #region Private Fields

        /// <summary>
        /// This gives us the relative path to the parameter files root.
        /// This assumes that the "ParamFiles" solution directory lives
        /// in the same location as the solution file itself.
        /// NOTE: It is important to adjust this for formal test environments.
        /// </summary>
        private const string Root = @"..\..\..\ParamFiles\ec";

        private const string DivLine = "*******************************************************";

        #endregion // Private Fields

        public OrderedFileParsingTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return context;
            }
            set
            {
                context = value;
            }
        }

        private TestContext context { get; set; }

        #endregion

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

        [Ignore]
        [TestMethod]
        [Description("This is just to print out context details for debugging.")]
        public void PrintContextDetails()
        {
            var fmt = "{0} = {1}";
            WriteDivider();
            context.WriteLine(fmt, "AssemblyCodeBase", Assembly.GetExecutingAssembly().CodeBase);
            context.WriteLine(fmt, "FullyQualifiedTestClassName", context.FullyQualifiedTestClassName);
            context.WriteLine(fmt, "TestName", context.TestName);
            context.WriteLine(fmt, "DeploymentDirectory", context.DeploymentDirectory);
            context.WriteLine(fmt, "ResultsDirectory", context.ResultsDirectory);
            context.WriteLine(fmt, "TestDeploymentDir", context.TestDeploymentDir);
            context.WriteLine(fmt, "TestDir", context.TestDir);
            context.WriteLine(fmt, "TestLogsDir", context.TestLogsDir);
            context.WriteLine(fmt, "TestResultsDirectory", context.TestResultsDirectory);
            context.WriteLine(fmt, "TestRunDirectory", context.TestRunDirectory);
            context.WriteLine(fmt, "TestRunResultsDirectory", context.TestRunResultsDirectory);
            WriteDivider();
        }

        [TestMethod]
        [Description("This just tests to see if the other tests will be able to locate the various parameter files.")]
        public void CheckFilesFound()
        {
            var fileSpec = Path.Combine(Root, "ec.params");           
            context.WriteLine(fileSpec);
            context.WriteLine(Path.GetFullPath(fileSpec));
            Assert.IsTrue(System.IO.File.Exists(fileSpec));
        }

        [TestMethod]
        [Description("Test that the root parameter file (ec.params) is properly parsed. (assumes 6 properties)")]
        public void ParseFile_ec_params()
        {
            var fileSpec = Path.Combine(Root, "ec.params");
            var fileExists = System.IO.File.Exists(fileSpec);
            Assert.IsTrue(fileExists);
            var props = new PropertiesClass();
            var fstream = new FileStream(fileSpec, FileMode.Open, FileAccess.Read, FileShare.Read);
            props.Load(fstream);
            fstream.Close();
            Assert.AreEqual(props.Count, 6);
            foreach (var k in props.Keys)
            {
                var v = props[k];
                context.WriteLine("{0} = {1}", k, v);
            }
            WriteFileToContext(fileSpec);
        }

        #region Helpers

        public void WriteFileToContext(string fileSpec)
        {
            WriteDivider();
            if (!System.IO.File.Exists(fileSpec))
            {
                context.WriteLine("File not found!");
            }
            else
            {
                writeFile(fileSpec);
            }
            WriteDivider();
        }
        private void writeFile(string fileSpec)
        {
            context.WriteLine(new StreamReader(fileSpec).ReadToEnd());            
        }
        private void WriteDivider()
        {
            context.WriteLine("");
            context.WriteLine(DivLine);
            context.WriteLine("");
        }

        #endregion
    }
}