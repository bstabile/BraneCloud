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

namespace BraneCloud.Evolution.EC.GP.Tests
{
    /// <summary>
    /// Summary description for GPTreeTests
    /// </summary>
    [TestClass]
    public class GPTreeTests
    {
        #region Housekeeping

        public GPTreeTests()
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

        /// <summary>
        /// UNDER CONSTRUCTION!
        /// </summary>
        [TestMethod]
        public void TestMethod1()
        {
            var tree = new GPTree();

            var root = new ADF { FunctionName = "ADF0", Children = new GPNode[2]};
            tree.Child = root;

            var c1 = new ADM { FunctionName = "ADM1", Parent = root, Children = new GPNode[2] };
            var c11 = new ADF {FunctionName = "ADF11", Parent = c1 };
            var c12 = new DummyERC {FunctionName = "ERC12", Parent = c1 };
            c1.Children[0] = c11;
            c1.Children[1] = c12;

            var c2 = new ADM { FunctionName = "ADM2", Parent = root, Children = new GPNode[1] };
            var c21 = new ADF {FunctionName = "ADF21", Parent = c2};
            c2.Children[0] = c21;

            root.Children = new GPNode[3];
            root.Children[0] = c1;
            root.Children[1] = c2;
            root.Children[2] = new DummyERC { Parent = root };

            // nodesearch (0=All, 1=Terminals, 2=Nonterminals)
            foreach (GPNode node in tree.Descendants(nodesearch: GPNode.NODESEARCH_ALL))
            {
                var nodeName = node.Name;
                var t1 = node.Parent != null;
                var t2 = ((GPNode) node.Parent)?.Parent != null;
                var tabs = t2 ? 2 : t1 ? 1 : 0;
                var s = (tabs == 2 ? "\t\t" : tabs == 1 ? "\t" : "") + $"{nodeName}";
                Console.WriteLine(s);
            }
        }
    }

    public class DummyERC : ERC
    {
        public string FunctionName { get; set; }

        public override void ResetNode(IEvolutionState state, int thread)
        {
            throw new NotImplementedException();
        }

        public override bool NodeEquals(GPNode node)
        {
            throw new NotImplementedException();
        }

        public override string Encode()
        {
            throw new NotImplementedException();
        }

        public override void Eval(IEvolutionState state, int thread, GPData input, ADFStack stack, GPIndividual individual, IProblem problem)
        {
            throw new NotImplementedException();
        }
    }
}