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
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BraneCloud.Evolution.EC.Configuration.Tests
{
    /// <summary>
    /// Summary description for PropertyClassTests
    /// </summary>
    [TestClass]
    public class PropertyClassTests
    {
        private const string divider = "***********************************************************************************************************";
        private const string RelativePath = @"..\..\..\..\..\Solutions\EC\ParamFiles\ec";

        #region Housekeeping   ***************************************************************

        public PropertyClassTests()
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
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
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
        [Description("All parameter files are read by ParameterFileTree and properties are compared with what is parsed by PropertiesClass")]
        public void PropertiesClassGivesSameResultsAsParameterFileTree()
        {
            var rootDir = Path.GetFullPath(RelativePath);
            var tree = new FileTree(rootDir);
            var filesRead = 0;
            var propsRead = 0;
            foreach (var file in tree.NodeRegistry)
            {
                var pc = new PropertiesClass();
                Assert.IsTrue(System.IO.File.Exists(file.Key));
                using (var stream = new FileStream(file.Key, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    pc.Load(stream);
                    foreach (var prop in file.Value.Properties)
                    {
                        Assert.AreEqual(prop.Value, pc.GetProperty(prop.Key));
                        propsRead++;
                    }
                }
                filesRead++;
            }
            context.WriteLine("Total Files Parsed: {0}", filesRead);
            context.WriteLine("Total Properties Compared: {0}", propsRead);
        }

        #region Additional tests for diagnostic purposes

        [Ignore]
        [TestMethod]
        [Description("This will write all parameter files and properties to a text file which is then displayed in a spawned process. Use only for diagnostics!")]
        public void ViewParameterFileTreeOutputForDiagnostics()
        {
            var rootDir = Path.GetFullPath(RelativePath);
            if (!Directory.Exists(rootDir))
            {
                context.WriteLine("Invalid path specified: {0}", rootDir);
                context.WriteLine("Press <enter> to exit...");
                return;
            }
            context.WriteLine("{0} = {1}", "RelativePath", RelativePath);
            context.WriteLine("{0} = {1}", "AbsolutePath", rootDir);

            var tree = new FileTree(rootDir);
            context.WriteLine("Total number of files in the tree: {0}", tree.NodeRegistry.Count);
            context.WriteLine("");
            context.WriteLine("Filename (property count)");
            context.WriteLine("");
            foreach (var f in tree.NodeRegistry)
            {
                context.WriteLine("{0} ({1})", f.Key, f.Value.Properties.Count);
            }
            PrintTreeForDiagnostics(tree);
        }

        [Ignore]
        [TestMethod]
        [Description("The following tests are just a few examples that show how to test individual files. (Adjust the 'subdir' and 'fileName' properties as needed.)")]
        public void ParseSingleParameterFileForDiagnostics()
        {
            var rootDir = Path.GetFullPath(RelativePath);
            var subDir = @"gp\koza";
            var fileName = "koza.params";
            var filePath = Path.Combine(rootDir, subDir, fileName);

            if (!Directory.Exists(rootDir))
            {
                context.WriteLine("Invalid path specified: {0}", rootDir);
                return;
            }
            if (!System.IO.File.Exists(filePath))
            {
                context.WriteLine("Invalid file specified {0}", fileName);
                context.WriteLine("FullName: {0}", filePath);
                return;
            }
            context.WriteLine("{0} = {1}", "RelativePath", RelativePath);
            context.WriteLine("{0} = {1}", "AbsolutePath", rootDir);

            var tree = new FileTree(rootDir);
            var pf = tree.NodeRegistry[Path.Combine(rootDir, filePath)];
            Assert.IsNotNull(pf);
            context.WriteLine("Current File: {0}", pf.FullName);
            PrintParameterFile(pf);

            var pc = new PropertiesClass();
            using (var fs = new FileStream(pf.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                pc.Load(fs);
                Assert.AreEqual(pc.Keys.Count, pf.Properties.Count);
                PrintPropertiesClass(pc);
            }
        }

        #endregion

        #region Helpers

        private void PrintTreeForDiagnostics(FileTree tree)
        {
            using (var writer = new StreamWriter("tree.txt", false))
            {
                writer.WriteLine("Total number of files in the tree: {0}", tree.NodeRegistry.Count);
                writer.Write(tree.ToString());
                writer.Write(Environment.NewLine);
                writer.Write(tree.ToXml());
            }
            Process.Start("tree.txt");
            context.WriteLine("");
        }

        private void PrintParameterFile(File pfile)
        {
            context.WriteLine(divider);
            context.WriteLine("ParameterFile: {0}", pfile.FullName);
            context.WriteLine("Name: {0}", pfile.Name);
            context.WriteLine("Properties: {0}", pfile.Properties.Count);
            var fmt = "{0}{1}{2}{3}";
            foreach (var p in pfile.Properties)
            {
                context.WriteLine(fmt, "\t", p.Key, " = ", p.Value);
            }
            context.WriteLine(divider);
        }

        private void PrintPropertiesClass(PropertiesClass pclass)
        {
            context.WriteLine(divider);
            context.WriteLine("PropertiesClass Properties: {0}", pclass.Keys.Count);
            var fmt = "{0}{1}{2}{3}";
            foreach (var key in pclass.Keys)
            {
                context.WriteLine(fmt, "\t", key, " = ", pclass.GetProperty(key.ToString()));
            }
            context.WriteLine(divider);
        }

        #endregion // Helpers
    }
}