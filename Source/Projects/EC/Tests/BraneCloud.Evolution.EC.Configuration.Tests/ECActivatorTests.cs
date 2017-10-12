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
    /// Summary description for ECActivatorTests
    /// </summary>
    [TestClass]
    public class ECActivatorTests
    {
        #region Housekeeping
        public ECActivatorTests()
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
        public void ActivateAllCanonicalTypesTest()
        {
            var failed = false;
            var failedList = new List<string>();
            foreach (var canonName in ECTypeNameMap.Keys)
            {
                string typeName;
                if (!ECTypeNameMap.TryGetTypeName(canonName, out typeName) || String.IsNullOrEmpty(typeName))
                {
                    context.WriteLine("Could not get the type name for '{0}'", canonName);
                    failed = true;
                    failedList.Add(canonName);
                    continue;
                }
                try
                {
                    object o;
                    Type t = null;
                    if (!ECTypeMap.TryGetType(canonName, out t))
                    {
                        context.WriteLine("Could not get the type for '{0}' from ECTypeMap.TryGetType(<canonName>)", canonName);
                        failed = true;
                        failedList.Add(canonName);
                        continue;
                    }
                    if (!t.IsAbstract && !t.IsInterface)
                    {
                        var ctor = t.GetConstructor(new Type[] {});
                        if (ctor == null)
                        {
                            context.WriteLine("No default constructor for : {0}", t.Name);
                            continue;
                        }
                        o = Activator.CreateInstance(t);

                        context.WriteLine("Success for type '{0}'", o.GetType().FullName);
                    }
                    else
                    {
                        context.WriteLine("Type is not createable (Abstract or Interfact): {0}", t.Name);
                    }
                }
                catch (Exception)
                {
                    failed = true;
                    failedList.Add(canonName);
                }
            }
            if (failedList.Count > 0)
            {
                context.WriteLine("CreateInstance failed for the following canonical names:");
            }
            foreach (var name in failedList)
            {
                context.WriteLine(name);
            }
            Assert.IsFalse(failed, "At least one type could not be created.");
        }
    }
}