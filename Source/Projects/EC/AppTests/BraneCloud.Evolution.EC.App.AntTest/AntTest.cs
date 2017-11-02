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

namespace BraneCloud.Evolution.EC.App.AntTest
{
    /// <summary>
    /// Summary description for AntTest
    /// </summary>
    [TestClass]
    public class AntTest
    {
        #region Housekeeping

        public AntTest()
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
        [Description("Ant problem, Santa Fe trail.")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\App\Ant\ant_santafe.params", "App/Ant/Params/App/Ant")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\ec.params", "App/Ant/Params")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\GP\Koza\koza.params", "App/Ant/Params/GP/Koza")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\Simple\simple.params", "App/Ant/Params/Simple")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Trails\santafe.trl", "App/Ant/Trails")]
        public void AntSantaFe()
        {
            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
                                                {
                                                    Assembly.GetAssembly(typeof(IEvolutionState)), 
                                                    Assembly.GetAssembly(typeof(Evaluator)),
                                                    Assembly.GetAssembly(typeof(Ant))
                                                });

            // Here we are starting up with ant_santefe.params
            // But this can also be started ant_santefe_ge.params (which presumably does generational evolution; need to check)
            Evolve.Run(new[] { "-file", @"App/Ant/Params/App/Ant/ant_santafe.params" });
            context.WriteLine("\nDone!");
        }

        [TestMethod]
        [Description("Ant problem, Los Altos trail.")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\App\Ant\ant_losaltos.params", "App/Ant/Params/App/Ant")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\ec.params", "App/Ant/Params")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\GP\Koza\koza.params", "App/Ant/Params/GP/Koza")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\Simple\simple.params", "App/Ant/Params/Simple")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Trails\losaltos.trl", "App/Ant/Trails")]
        public void AntLosAltos()
        {
            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
                                                {
                                                    Assembly.GetAssembly(typeof(IEvolutionState)), 
                                                    Assembly.GetAssembly(typeof(Evaluator)),
                                                    Assembly.GetAssembly(typeof(Ant))
                                                });

            // Here we are starting up with ant_losaltos.params
            // But this can also be started ant_losaltos_ge.params (which presumably does generational evolution; need to check)
            Evolve.Run(new[] { "-file", @"App/Ant/Params/App/Ant/ant_losaltos.params" });
            context.WriteLine("\nDone!");
        }

        [TestMethod]
        [Description("Ant problem, Santa Fe trail, Grammatical Evolution.")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\App\Ant\ant_santafe_ge.params", "App/Ant/Params/App/Ant")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\App\Ant\ant_santafe.params", "App/Ant/Params/App/Ant")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\ec.params", "App/Ant/Params")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\GP\GE\ge.params", "App/Ant/Params/GP/GE")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\GP\Koza\koza.params", "App/Ant/Params/GP/Koza")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\Simple\simple.params", "App/Ant/Params/Simple")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Grammar\ant.grammar", "App/Ant/Grammar")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Trails\santafe.trl", "App/Ant/Trails")]
        public void AntSantaFe_GE()
        {
            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
                                                {
                                                    Assembly.GetAssembly(typeof(IEvolutionState)),
                                                    Assembly.GetAssembly(typeof(Evaluator)),
                                                    Assembly.GetAssembly(typeof(Ant))
                                                });

            // Here we are starting up with ant_santefe.params
            // But this can also be started ant_santefe_ge.params (which presumably does generational evolution; need to check)
            Evolve.Run(new[] { "-file", @"App/Ant/Params/App/Ant/ant_santafe_ge.params" });
            context.WriteLine("\nDone!");
        }

        [TestMethod]
        [Description("Ant problem, Los Altos trail, Grammatical Evolution.")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\App\Ant\ant_losaltos_ge.params", "App/Ant/Params/App/Ant")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\App\Ant\ant_losaltos.params", "App/Ant/Params/App/Ant")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\ec.params", "App/Ant/Params")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\GP\GE\ge.params", "App/Ant/Params/GP/GE")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\GP\Koza\koza.params", "App/Ant/Params/GP/Koza")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\Simple\simple.params", "App/Ant/Params/Simple")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Grammar\ant.grammar", "App/Ant/Grammar")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Trails\losaltos.trl", "App/Ant/Trails")]
        public void AntLosAltos_GE()
        {
            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
                                                {
                                                    Assembly.GetAssembly(typeof(IEvolutionState)), 
                                                    Assembly.GetAssembly(typeof(Evaluator)),
                                                    Assembly.GetAssembly(typeof(Ant))
                                                });

            // Here we are starting up with ant_losaltos.params
            // But this can also be started ant_losaltos_ge.params (which presumably does generational evolution; need to check)
            Evolve.Run(new[] { "-file", @"App/Ant/Params/App/Ant/ant_losaltos_ge.params" });
            context.WriteLine("\nDone!");
        }

        [TestMethod]
        [Description("Ant problem, Santa Fe trail, with Progn4.")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\App\Ant\progn4_santafe.params", "App/Ant/Params/App/Ant")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\App\Ant\ant_santafe_progn4.params", "App/Ant/Params/App/Ant")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\ec.params", "App/Ant/Params")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\GP\Koza\koza.params", "App/Ant/Params/GP/Koza")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\Simple\simple.params", "App/Ant/Params/Simple")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Grammar\ant_progn4.grammar", "App/Ant/Grammar")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Trails\santafe.trl", "App/Ant/Trails")]
        public void AntSantaFe_Progn4()
        {
            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
                                                {
                                                    Assembly.GetAssembly(typeof(IEvolutionState)), 
                                                    Assembly.GetAssembly(typeof(Evaluator)),
                                                    Assembly.GetAssembly(typeof(Ant))
                                                });

            // Here we are starting up with ant_santefe.params
            // But this can also be started ant_santefe_ge.params (which presumably does generational evolution; need to check)
            Evolve.Run(new[] { "-file", @"App/Ant/Params/App/Ant/progn4_santafe.params" });
            context.WriteLine("\nDone!");
        }

        [TestMethod]
        [Description("Ant problem, Los Altos trail, with Progn4.")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\App\Ant\progn4_losaltos.params", "App/Ant/Params/App/Ant")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\App\Ant\ant_losaltos_progn4.params", "App/Ant/Params/App/Ant")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\ec.params", "App/Ant/Params")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\GP\Koza\koza.params", "App/Ant/Params/GP/Koza")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Params\Simple\simple.params", "App/Ant/Params/Simple")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Grammar\ant_progn4.grammar", "App/Ant/Grammar")]
        [DeploymentItem(@"..\..\Projects\EC\Tests\App\BraneCloud.Evolution.EC.App.AntTest\Trails\losaltos.trl", "App/Ant/Trails")]
        public void AntLosAltos_Progn4()
        {
            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
                                                {
                                                    Assembly.GetAssembly(typeof(IEvolutionState)), 
                                                    Assembly.GetAssembly(typeof(Evaluator)),
                                                    Assembly.GetAssembly(typeof(Ant))
                                                });

            // Here we are starting up with ant_losaltos.params
            // But this can also be started ant_losaltos_ge.params (which presumably does generational evolution; need to check)
            Evolve.Run(new[] { "-file", @"App/Ant/Params/App/Ant/progn4_losaltos.params" });
            context.WriteLine("\nDone!");
        }
    }
}