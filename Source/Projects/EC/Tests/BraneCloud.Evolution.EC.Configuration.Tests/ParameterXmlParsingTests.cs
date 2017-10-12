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
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BraneCloud.Evolution.EC.Configuration.Tests
{
    /// <summary>
    /// Summary description for ParameterXmlParsingTests
    /// </summary>
    [TestClass]
    public class ParameterXmlParsingTests
    {
        #region Housekeeping

        public ParameterXmlParsingTests()
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
        [DeploymentItem(@"..\..\Projects\EC\Tests\BraneCloud.Evolution.EC.Configuration.Tests\ParamFiles\ParameterFileTreeDocument.xml")]
        public void ParseXmlFile()
        {
            //var xml = XElement.Load("ParameterFileTreeDocument.xml");
            //var nt = new NameTable();
            //nt.Add("file");
            //var nav = xml.CreateNavigator(nt);
            //var count = 0;
            //var depth = 0;

            //XNode node = xml.FirstNode;
            //node.

        }

        private void VisitDirectory(XPathNavigator nav, int currentDepth)
        {
            if (!nav.MoveToFirstChild()) return;
            do
            {
                if (nav.LocalName == "files")
                {
                    VisitFiles(nav, currentDepth);
                    nav.MoveToParent(); // this gets us back to currentDepth.
                }
                else if (nav.LocalName == "directory")
                {
                    currentDepth++;
                    do
                    {
                        VisitDirectory(nav, currentDepth);
                    } while (nav.MoveToNext());
                }
            } while (nav.MoveToNext());
            nav.MoveToParent();
        }
       
        private void VisitFiles(XPathNavigator nav, int currentDepth)
        {
            if (!nav.MoveToFirstChild())
            {
                return;
            }
            do
            {
                context.WriteLine("{0}{1}", GetTabs(currentDepth), nav.GetAttribute("name", ""));
            } while (nav.MoveToNext());
        }

        private static string GetTabs(int count)
        {
            var tabs = "";
            for (var i = 0; i < count; i++)
                tabs += "\t";
            return tabs;
        }
    }
}