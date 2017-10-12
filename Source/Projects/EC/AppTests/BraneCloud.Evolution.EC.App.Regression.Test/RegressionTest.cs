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

namespace BraneCloud.Evolution.EC.App.Regression.Test
{
    /// <summary>
    /// Summary description for RegressionTest
    /// </summary>
    [TestClass]
    public class RegressionTest
    {
        #region Housekeeping

        public RegressionTest()
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
        [Description("Regression Basic Test.")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Regression.Test\Params\App\Regression\erc.params", "App/Regression/Params/App/Regression")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Regression.Test\Params\App\Regression\ge2.params", "App/Regression/Params/App/Regression")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Regression.Test\Params\App\Regression\noerc.params", "App/Regression/Params/App/Regression")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Regression.Test\Params\App\Regression\noerc2.params", "App/Regression/Params/App/Regression")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Regression.Test\Params\App\Regression\quinticerc.params", "App/Regression/Params/App/Regression")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Regression.Test\Params\App\Regression\quinticnoerc.params", "App/Regression/Params/App/Regression")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Regression.Test\Params\App\Regression\sexticerc.params", "App/Regression/Params/App/Regression")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Regression.Test\Params\App\Regression\sexticnoerc.params", "App/Regression/Params/App/Regression")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Regression.Test\Params\App\Regression\steadynoerc.params", "App/Regression/Params/App/Regression")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Regression.Test\Params\ec.params", "App/Regression/Params")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Regression.Test\Params\GP\GE\ge.params", "App/Regression/Params/GP/GE")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Regression.Test\Params\GP\Koza\koza.params", "App/Regression/Params/GP/Koza")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Regression.Test\Params\Simple\simple.params", "App/Regression/Params/Simple")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Regression.Test\Params\SteadyState\steadystate.params", "App/Regression/Params/SteadyState")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Regression.Test\Grammar\regression.grammar", "App/Regression/Grammar")]
        public void RegressionBasic()
        {
            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
                                                {
                                                    Assembly.GetAssembly(typeof(IEvolutionState)), 
                                                    Assembly.GetAssembly(typeof(Evaluator)),
                                                    Assembly.GetAssembly(typeof(Regression))
                                                });

            Evolve.Run(new[] { "-file", @"App/Regression/Params/App/Regression/erc.params" });
            context.WriteLine("\nDone!");
        }
    }
}