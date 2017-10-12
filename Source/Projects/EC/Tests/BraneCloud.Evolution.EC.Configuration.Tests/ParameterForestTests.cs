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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BraneCloud.Evolution.EC.Configuration.Tests
{
    /// <summary>
    /// Summary description for ParameterFileForestTests
    /// </summary>
    [TestClass]
    public class ParameterForestTests
    {
        private const string RelativePath = @"..\..\..\..\..\Solutions\EC\ParamFiles\ec";
        private const string NameValueFormat = "{0} = {1}";
        private const string Divider =
            "**************************************************************************************";

        #region Housekeeping

        public ParameterForestTests()
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

        [TestMethod]
        public void TwoRootForest()
        {
            var path1 = Path.GetFullPath(Path.Combine(RelativePath, "gp"));
            var path2 = Path.GetFullPath(Path.Combine(RelativePath, "simple"));
            var builder = new ParameterForestBuilder();
            var loc1 = new ParameterSourceLocator(path1);
            var loc2 = new ParameterSourceLocator(path2);
            var forest = builder.Build(new[] {loc1, loc2});
            Assert.AreEqual(forest.Sources.Count, 2);
            context.WriteLine(Divider);
            context.WriteLine("Source #1");
            context.WriteLine(Divider);
            context.WriteLine(forest.Sources[loc1.Path].ToXml().ToString());
            context.WriteLine(Divider);
            context.WriteLine("Source #2");
            context.WriteLine(Divider);
            context.WriteLine(forest.Sources[loc2.Path].ToXml().ToString());
            context.WriteLine(Divider);
            if (forest.Nodes.Count > 0)
                context.WriteLine("Nodes:");
            context.WriteLine(Divider);
            foreach (var node in forest.Nodes)
            {
                context.WriteLine("{0}", node.Key);
            }
        }

        [TestMethod]
        public void AppRootForest()
        {
            var path = Path.GetFullPath(Path.Combine(RelativePath, "app"));
            var builder = new ParameterForestBuilder();
            var loc = new ParameterSourceLocator(path);
            var forest = builder.Build(new[] { loc });
            Assert.AreEqual(forest.Sources.Count, 1);
            context.WriteLine(Divider);
            context.WriteLine("Source #1");
            context.WriteLine(Divider);
            context.WriteLine(forest.Sources[loc.Path].ToXml().ToString());
            context.WriteLine(Divider);
            if (forest.Nodes.Count > 0)
                context.WriteLine("Nodes:");
            context.WriteLine(Divider);
            foreach (var node in forest.Nodes)
            {
                context.WriteLine("{0}", node.Key);
            }
        }

        [TestMethod]
        public void ECRootForestPropertyFileHierarchy()
        {
            var path = Path.GetFullPath(RelativePath);
            var builder = new ParameterForestBuilder();
            var loc = new ParameterSourceLocator(path);
            var forest = builder.Build(new[] { loc });
            Assert.AreEqual(forest.Sources.Count, 1);

            //context.WriteLine("File Ineritance:");
            //context.WriteLine(Divider);
            //context.WriteLine("");
            //foreach (var entry in forest.Nodes)
            //{
            //    WriteNodeAndParentNames(entry.Value, forest.Nodes, "");
            //}

            context.WriteLine(Divider);
            context.WriteLine("FileDictionaryTrees:");
            context.WriteLine(Divider);
            foreach (var entry in forest.Trees)
            {
                WriteTree(entry.Value, "");
            }
            context.WriteLine(Divider);
        }

        public void WriteNodeAndParentNames(File currNode, IDictionary<string, File> nodes, string indent)
        {
            var fullName = currNode.FullName;
            var currDir = Path.GetDirectoryName(fullName);

            context.WriteLine(String.Format("{0}{1}", indent, fullName));

            var x = 0;
            
            while (currNode.Properties.ContainsKey("parent." + x))
            {
                var parentRelPath = currNode.Properties["parent." + x];
                if (!String.IsNullOrEmpty(parentRelPath) && !String.IsNullOrEmpty(currDir))
                {
                    var parentAbsPath = Path.IsPathRooted(parentRelPath) ? parentRelPath : Path.Combine(currDir, parentRelPath);

                    if (!String.IsNullOrEmpty(parentAbsPath) && nodes.ContainsKey(parentAbsPath))
                    {
                        var parent = nodes[parentAbsPath];
                        WriteNodeAndParentNames(parent, nodes, indent + "\t"); // increase the indent for the recursive call
                    }
                }
                x++;
            }
        }

        public void WriteTree(FileDictionaryTree tree, string indent)
        {
            var shortName = tree.FullName.Substring(RelativePath.Length + 2);
            context.WriteLine(indent + "{0}", shortName);
            foreach (FileDictionaryTree parent in tree.Parents)
            {
                WriteTree(parent, indent + "\t");
            }
        }
    }
}