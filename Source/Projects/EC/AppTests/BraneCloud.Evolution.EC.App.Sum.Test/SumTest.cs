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
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using BraneCloud.Evolution.EC.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.Sum.Test
{
    /// <summary>
    /// Summary description for SumTest
    /// </summary>
    [TestClass]
    public class SumTest
    {
        #region Housekeeping

        public SumTest()
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
        [Description("Sum Basic Test.")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Sum.Test\Params\ec.params", "App/Sum/Params")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Sum.Test\Params\Simple\simple.params", "App/Sum/Params/Simple")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Sum.Test\Params\SteadyState\steadystate.params", "App/Sum/Params/SteadyState")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Sum.Test\Params\App\Sum\steadysum.params", "App/Sum/Params/App/Sum")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Sum.Test\Params\App\Sum\sum.params", "App/Sum/Params/App/Sum")]
        public void SumBasic()
        {
            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
                                                {
                                                    Assembly.GetAssembly(typeof(IEvolutionState)), 
                                                    Assembly.GetAssembly(typeof(Evaluator)),
                                                    Assembly.GetAssembly(typeof(Sum))
                                                });

            Evolve.Run(new[] { "-file", @"App/Sum/Params/App/Sum/sum.params" });
            context.WriteLine("\nDone!");
        }
    }
}