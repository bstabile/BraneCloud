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
    /// These tests are based on a set of parameter files that may change over time.
    /// To synchronize the tests with the current versions, it may be necessary to
    /// regenerate (or manually edit) the ParameterFileTree.xml file that is located
    /// in the "ParamFiles" subdirectory. One way to do this is to grab the XML text
    /// that is output from the ParameterFileTree.ToXml() method. Of course, that
    /// would invalidate the tests that are using the file to check that the code
    /// hasn't been broken. If we compare the file contents against the code that
    /// created it, we're not learning much. Therefore, it might be a good idea to
    /// maintain more than one means of parsing the parameter files. ;-)
    /// </summary>
    [TestClass]
    public class ParameterFileTreeTests
    {
        private const string RelativePath = @"..\..\..\..\..\Solutions\EC\ParamFiles\ec";
        private const string NameValueFormat = "{0} = {1}";
        private static FileTree Tree;

        #region Housekeeping

        public ParameterFileTreeTests()
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
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            Tree = new FileTree(Path.GetFullPath(RelativePath));
        }
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
        public void RunThisToViewTreeOutput()
        {
            PrintTreeForDiagnostics();
        }

        //*********************************************************************************

        [TestMethod]
        public void ec_params()
        {
            var fi = Tree.Root.Files["ec.params"];
            Assert.IsNotNull(fi);
            Assert.IsTrue(fi.Info.Name == "ec.params");

        }

        [TestMethod]
        [Description("Verifies that the tree can be traversed by an indexer that uses relative subdirectory to deliver embedded ParameterDirectoryNodes.")]
        public void TraverseTreeUsingSubdirectoryIndexingTwoLevels()
        {
            var subNode = Tree.Root[@"gp\ge"];
            Assert.AreEqual(subNode.Name, "ge");
        }

        [TestMethod]
        [Description("Verifies that the tree can be traversed by an indexer that uses relative subdirectory to deliver embedded ParameterDirectoryNodes.")]
        public void TraverseTreeUsingSubdirectoryIndexingThreeLevels()
        {
            var subNode = Tree.Root[@"app\star\old"];
            Assert.AreEqual(subNode.Name, "old");
        }

        #region Helpers

        private static void PrintTreeForDiagnostics()
        {
            var rootDir = Path.GetFullPath(RelativePath);
            Console.WriteLine(NameValueFormat, "RelativePath", RelativePath);
            Console.WriteLine(NameValueFormat, "RootDir", rootDir);

            if (!Directory.Exists(rootDir))
            {
                Console.WriteLine("Invalid path specified: {0}", rootDir);
                Console.WriteLine("Press <enter> to exit...");
                Console.ReadLine();
                return;
            }
            var tree = new FileTree(rootDir);
            Console.WriteLine();
            var writer = new StreamWriter("tree.txt", false);
            writer.Write(tree.ToString());
            writer.Write(Environment.NewLine);
            writer.Write(tree.ToXml());
            writer.Flush();
            writer.Close();
            Process.Start("tree.txt");
            Console.WriteLine();

            Console.WriteLine("Press <enter> to exit...");
            Console.ReadLine();

        }

        #endregion // Helpers
    }
}