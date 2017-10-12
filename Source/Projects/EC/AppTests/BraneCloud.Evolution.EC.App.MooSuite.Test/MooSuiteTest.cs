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

using System.Reflection;
using BraneCloud.Evolution.EC.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.MooSuite.Test
{
    /// <summary>
    /// Summary description for MooSuiteTest
    /// </summary>
    [TestClass]
    public class MooSuiteTest
    {
        #region Housekeeping

        public MooSuiteTest()
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
        [Description("MooSuite Basic Test.")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\ec.params", "App/MooSuite/Params")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\App\MooSuite\f2.params", "App/MooSuite/Params/App/MooSuite")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\App\MooSuite\fon.params", "App/MooSuite/Params/App/MooSuite")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\App\MooSuite\kur-nsga2.params", "App/MooSuite/Params/App/MooSuite")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\App\MooSuite\kur-spea2.params", "App/MooSuite/Params/App/MooSuite")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\App\MooSuite\moo_nsga2.params", "App/MooSuite/Params/App/MooSuite")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\App\MooSuite\moo_spea2.params", "App/MooSuite/Params/App/MooSuite")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\App\MooSuite\moosuite.params", "App/MooSuite/Params/App/MooSuite")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\App\MooSuite\pol.params", "App/MooSuite/Params/App/MooSuite")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\App\MooSuite\qv.params", "App/MooSuite/Params/App/MooSuite")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\App\MooSuite\sch.params", "App/MooSuite/Params/App/MooSuite")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\App\MooSuite\sphere.params", "App/MooSuite/Params/App/MooSuite")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\App\MooSuite\unconstrained-f3.params", "App/MooSuite/Params/App/MooSuite")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\App\MooSuite\zdt1.params", "App/MooSuite/Params/App/MooSuite")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\App\MooSuite\zdt2.params", "App/MooSuite/Params/App/MooSuite")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\App\MooSuite\zdt3.params", "App/MooSuite/Params/App/MooSuite")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\App\MooSuite\zdt4.params", "App/MooSuite/Params/App/MooSuite")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\App\MooSuite\zdt6.params", "App/MooSuite/Params/App/MooSuite")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\MultiObjective\multiobjective.params", "App/MooSuite/Params/MultiObjective")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\MultiObjective\Spea2\spea2.params", "App/MooSuite/Params/MultiObjective/Spea2")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\MultiObjective\Nsga2\nsga2.params", "App/MooSuite/Params/MultiObjective/Nsga2")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.MooSuite.Test\Params\Simple\simple.params", "App/MooSuite/Params/Simple")]
        public void MooSuiteBasic()
        {
            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
                                                {
                                                    Assembly.GetAssembly(typeof(IEvolutionState)), 
                                                    Assembly.GetAssembly(typeof(Evaluator)),
                                                    Assembly.GetAssembly(typeof(MooSuite))
                                                });

            // We are defaulting to problem type ZDT1. Change this to another specific type for alternate tests.
            // Choices include: [sphere, zdt1, zdt2, zdt3, zdt4, zdt6, fon, pol, kur-nsga2, kur-spea2, qv, sch, f2, f3(unconstrained-f2.params)]
            Evolve.Run(new[] { "-file", @"App/MooSuite/Params/App/MooSuite/zdt1.params" });
            context.WriteLine("\nDone!");
        }
    }
}