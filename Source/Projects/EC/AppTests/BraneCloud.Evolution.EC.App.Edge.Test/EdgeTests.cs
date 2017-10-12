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

namespace BraneCloud.Evolution.EC.App.Edge.Test
{
    /// <summary>
    /// Summary description for EdgeTests
    /// </summary>
    [TestClass]
    public class EdgeTests
    {
        #region Housekeeping

        public EdgeTests()
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
        [Description("Edge Test Basic.")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Params\App\Edge\1.params", "App/Edge/Params/App/Edge")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Params\App\Edge\2.params", "App/Edge/Params/App/Edge")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Params\App\Edge\3.params", "App/Edge/Params/App/Edge")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Params\App\Edge\4.params", "App/Edge/Params/App/Edge")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Params\App\Edge\5.params", "App/Edge/Params/App/Edge")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Params\App\Edge\6.params", "App/Edge/Params/App/Edge")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Params\App\Edge\7.params", "App/Edge/Params/App/Edge")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Params\ec.params", "App/Edge/Params")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Params\GP\Koza\koza.params", "App/Edge/Params/GP/Koza")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Params\Simple\simple.params", "App/Edge/Params/Simple")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\1.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\1a.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\1s.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\1sa.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\2.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\2a.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\2s.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\2sa.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\3.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\3a.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\3s.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\3sa.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\4.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\4a.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\4s.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\4sa.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\5.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\5a.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\5s.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\5sa.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\6.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\6a.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\6s.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\6sa.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\7.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\7a.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\7s.out.gz", "App/Edge/Input")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.Edge.Test\Input\7sa.out.gz", "App/Edge/Input")]
        public void EdgeTestBasic()
        {
            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
                                                {
                                                    Assembly.GetAssembly(typeof(IEvolutionState)), 
                                                    Assembly.GetAssembly(typeof(Evaluator)),
                                                    Assembly.GetAssembly(typeof(Edge))
                                                });

            // We can run in a loop using each numbered param file. But here we are just running one individually to examine the output.
            Evolve.Run(new[] { "-file", @"App/Edge/Params/App/Edge/1.params" });
            context.WriteLine("\nDone!");
        }
    }
}