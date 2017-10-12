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

namespace BraneCloud.Evolution.EC.App.Lawnmower.Test
{
    /// <summary>
    /// Summary description for LawnmowerTest
    /// </summary>
    [TestClass]
    public class LawnmowerTest
    {
        #region Housekeeping

        public LawnmowerTest()
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
        [Description("Lawnmower Basic Test.")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Lawnmower.Test\Params\App\Lawnmower\adf.params", "App/Lawnmower/Params/App/Lawnmower")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Lawnmower.Test\Params\ec.params", "App/Lawnmower/Params")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Lawnmower.Test\Params\GP\GE\ge.params", "App/Lawnmower/Params/GP/GE")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Lawnmower.Test\Params\App\Lawnmower\ge2.params", "App/Lawnmower/Params/App/Lawnmower")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Lawnmower.Test\Params\GP\Koza\koza.params", "App/Lawnmower/Params/GP/Koza")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Lawnmower.Test\Params\App\Lawnmower\noadf.params", "App/Lawnmower/Params/App/Lawnmower")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Lawnmower.Test\Params\Simple\simple.params", "App/Lawnmower/Params/Simple")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Lawnmower.Test\Grammar\adf0.grammar", "App/Lawnmower/Grammar")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Lawnmower.Test\Grammar\adf1.grammar", "App/Lawnmower/Grammar")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Lawnmower.Test\Grammar\lawnmower.grammar", "App/Lawnmower/Grammar")]
        public void LawnmowerBasic()
        {
            // With the default parameters, this finds an ideal solution in generation 14.

            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
                                                {
                                                    Assembly.GetAssembly(typeof(IEvolutionState)), 
                                                    Assembly.GetAssembly(typeof(Evaluator)),
                                                    Assembly.GetAssembly(typeof(Lawnmower))
                                                });

            Evolve.Run(new[] { "-file", @"App/Lawnmower/Params/App/Lawnmower/ge2.params" });
            context.WriteLine("\nDone!");
        }
    }
}