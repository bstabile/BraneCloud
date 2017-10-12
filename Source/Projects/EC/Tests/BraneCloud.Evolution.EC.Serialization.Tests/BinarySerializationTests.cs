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
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using BraneCloud.Evolution.EC.Evaluation;
using BraneCloud.Evolution.EC.Runtime;
using BraneCloud.Evolution.EC.Runtime.Eval;
using BraneCloud.Evolution.EC.Runtime.Exchange;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BraneCloud.Evolution.EC.Breed;
using BraneCloud.Evolution.EC.CoEvolve;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.DE;
using BraneCloud.Evolution.EC.ES;
using BraneCloud.Evolution.EC.Eval;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.GP.Breed;
using BraneCloud.Evolution.EC.GP.Build;
using BraneCloud.Evolution.EC.GP.GE;
using BraneCloud.Evolution.EC.GP.Koza;
using BraneCloud.Evolution.EC.MultiObjective;
using BraneCloud.Evolution.EC.MultiObjective.NSGA2;
using BraneCloud.Evolution.EC.MultiObjective.SPEA2;
using BraneCloud.Evolution.EC.Parsimony;
using BraneCloud.Evolution.EC.PSO;
using BraneCloud.Evolution.EC.Rule;
using BraneCloud.Evolution.EC.Rule.Breed;
using BraneCloud.Evolution.EC.Select;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Spatial;
using BraneCloud.Evolution.EC.SteadyState;
using BraneCloud.Evolution.EC.Vector;
using BraneCloud.Evolution.EC.Vector.Breed;

namespace BraneCloud.Evolution.EC.Serialization.Tests
{
    /// <summary>
    /// Binary Serialization Tests for core library types.
    /// I think most of the important types have some coverage here.
    /// </summary>
    [TestClass]
    public class BinarySerializationTests
    {
        #region Housekeeping

        public BinarySerializationTests()
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

        #region Binary Serialization

        #region Breed

        [TestMethod]
        [Description("BufferedBreedingPipeline binary serialization.")]
        public void BufferedBreedingPipeline()
        {
            var o = new BufferedBreedingPipeline();

            BufferedBreedingPipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (BufferedBreedingPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("ForceBreedingPipeline binary serialization.")]
        public void ForceBreedingPipeline()
        {
            var o = new ForceBreedingPipeline();

            ForceBreedingPipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (ForceBreedingPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GenerationSwitchPipeline binary serialization.")]
        public void GenerationSwitchPipeline()
        {
            var o = new GenerationSwitchPipeline();

            GenerationSwitchPipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GenerationSwitchPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("MultiBreedingPipeline binary serialization.")]
        public void MultiBreedingPipeline()
        {
            var o = new MultiBreedingPipeline();

            MultiBreedingPipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MultiBreedingPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("ReproductionPipeline binary serialization.")]
        public void ReproductionPipeline()
        {
            var o = new ReproductionPipeline();

            ReproductionPipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (ReproductionPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // Breed
        #region CoEvolve

        [TestMethod]
        [Description("CompetitiveEvaluator binary serialization.")]
        public void CompetitiveEvaluator()
        {
            var o = new CompetitiveEvaluator();

            CompetitiveEvaluator o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (CompetitiveEvaluator)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("MultiPopCoevolutionaryEvaluator binary serialization.")]
        public void MultiPopCoevolutionaryEvaluator()
        {
            var o = new MultiPopCoevolutionaryEvaluator();

            MultiPopCoevolutionaryEvaluator o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MultiPopCoevolutionaryEvaluator)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // CoEvolve
        #region DE

        [TestMethod]
        [Description("Best1BinDEBreeder binary serialization.")]
        public void Best1BinDEBreeder()
        {
            var o = new Best1BinDEBreeder();

            Best1BinDEBreeder o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (Best1BinDEBreeder)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("DEBreeder binary serialization.")]
        public void DEBreeder()
        {
            var o = new DEBreeder();

            DEBreeder o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (DEBreeder)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("DEEvaluator binary serialization.")]
        public void DEEvaluator()
        {
            var o = new DEEvaluator();

            DEEvaluator o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (DEEvaluator)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("Rand1EitherOrDEBreeder binary serialization.")]
        public void Rand1EitherOrDEBreeder()
        {
            var o = new Rand1EitherOrDEBreeder();

            Rand1EitherOrDEBreeder o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (Rand1EitherOrDEBreeder)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // DE
        #region ES

        [TestMethod]
        [Description("ESSelection binary serialization.")]
        public void ESSelection()
        {
            var o = new ESSelection();

            ESSelection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (ESSelection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("MuCommaLambdaBreeder binary serialization.")]
        public void MuCommaLambdaBreeder()
        {
            var o = new MuCommaLambdaBreeder();

            MuCommaLambdaBreeder o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MuCommaLambdaBreeder)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("MuPlusLambdaBreeder binary serialization.")]
        public void MuPlusLambdaBreeder()
        {
            var o = new MuPlusLambdaBreeder();

            MuPlusLambdaBreeder o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MuPlusLambdaBreeder)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // ES
        #region Eval

        [TestMethod]
        [Description("Job binary serialization.")]
        public void Job()
        {
            var o = new Job();

            Job o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (Job)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("MasterProblem binary serialization.")]
        public void MasterProblem()
        {
            var o = new MasterProblem();

            MasterProblem o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MasterProblem)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("Slave binary serialization. Because of the way they are implemented, "
            + "SlaveConnection and SlaveMonitor cannot (and should not) be serialized. "
            + "But since Slave itself stores some useful state, serializing this might come in handy.")]
        public void Slave()
        {
            var o = new Slave();

            Slave o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (Slave)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // Eval
        #region Evolve

        [TestMethod]
        [Description("RandomRestarts binary serialization.")]
        public void RandomRestarts()
        {
            var o = new RandomRestarts();

            RandomRestarts o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (RandomRestarts)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // Evolve
        #region Exchange

        [TestMethod]
        [Description("InterPopulationExchange binary serialization.")]
        public void InterPopulationExchange()
        {
            var o = new InterPopulationExchange();

            InterPopulationExchange o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (InterPopulationExchange)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("IslandExchangeIslandInfo binary serialization.")]
        public void IslandExchangeIslandInfo()
        {
            var o = new IslandExchangeIslandInfo();

            IslandExchangeIslandInfo o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (IslandExchangeIslandInfo)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        // The types IslandExchange and IslandExchangeMailbox do not lend themselves to serialization.
        // TODO: Check that all of the info related to these can be transmitted, for debugging at least.

        #endregion // Exchange
        #region GP

        #region GPBreed

        [TestMethod]
        [Description("InternalCrossoverPipeline binary serialization.")]
        public void InternalCrossoverPipeline()
        {
            var o = new InternalCrossoverPipeline();

            InternalCrossoverPipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (InternalCrossoverPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("MutateAllNodesPipeline binary serialization.")]
        public void MutateAllNodesPipeline()
        {
            var o = new MutateAllNodesPipeline();

            MutateAllNodesPipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MutateAllNodesPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("MutateDemotePipeline binary serialization.")]
        public void MutateDemotePipeline()
        {
            var o = new MutateDemotePipeline();

            MutateDemotePipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MutateDemotePipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("MutateERCPipeline binary serialization.")]
        public void MutateERCPipeline()
        {
            var o = new MutateERCPipeline();

            MutateERCPipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MutateERCPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("MutateOneNodePipeline binary serialization.")]
        public void MutateOneNodePipeline()
        {
            var o = new MutateOneNodePipeline();

            MutateOneNodePipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MutateOneNodePipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("MutatePromotePipeline binary serialization.")]
        public void MutatePromotePipeline()
        {
            var o = new MutatePromotePipeline();

            MutatePromotePipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MutatePromotePipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("MutateSwapPipeline binary serialization.")]
        public void MutateSwapPipeline()
        {
            var o = new MutateSwapPipeline();

            MutateSwapPipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MutateSwapPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("RehangPipeline binary serialization.")]
        public void RehangPipeline()
        {
            var o = new RehangPipeline();

            RehangPipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (RehangPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // GPBreed
        #region GPBuild

        [TestMethod]
        [Description("PTC1 binary serialization.")]
        public void PTC1()
        {
            var o = new PTC1();

            PTC1 o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (PTC1)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("PTC2 binary serialization.")]
        public void PTC2()
        {
            var o = new PTC2();

            PTC2 o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (PTC2)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("PTCFunctionSet binary serialization.")]
        public void PTCFunctionSet()
        {
            var o = new PTCFunctionSet();

            PTCFunctionSet o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (PTCFunctionSet)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("RandomBranch binary serialization.")]
        public void RandomBranch()
        {
            var o = new RandomBranch();

            RandomBranch o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (RandomBranch)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("RandTree binary serialization.")]
        public void RandTree()
        {
            var o = new RandTree();

            RandTree o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (RandTree)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("Uniform binary serialization.")]
        public void Uniform()
        {
            var o = new Uniform();

            Uniform o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (Uniform)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("UniformGPNodeStorage binary serialization.")]
        public void UniformGPNodeStorage()
        {
            var o = new UniformGPNodeStorage();

            UniformGPNodeStorage o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (UniformGPNodeStorage)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // GPBuild
        #region GE

        [TestMethod]
        [Description("GEIndividual binary serialization.")]
        public void GEIndividual()
        {
            var o = new GEIndividual();

            GEIndividual o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GEIndividual)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GEProblem binary serialization.")]
        public void GEProblem()
        {
            var o = new GEProblem();

            GEProblem o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GEProblem)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GESpecies binary serialization.")]
        public void GESpecies()
        {
            var o = new GESpecies();

            GESpecies o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GESpecies)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GrammarFunctionNode binary serialization.")]
        public void GrammarFunctionNode()
        {
            var fs = new GPFunctionSet();
            fs.NodesByName = new Hashtable();
            fs.NodesByName.Add("someName", new GPNode[] { new ADF() });
            var o = new GrammarFunctionNode(fs, "someName");

            GrammarFunctionNode o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GrammarFunctionNode)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GrammarNode binary serialization.")]
        public void GrammarNode()
        {
            var o = new MyGrammarNode();

            GrammarNode o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GrammarNode)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }
        [Serializable]
        private class MyGrammarNode : GrammarNode
        {
            public MyGrammarNode() : base("head"){}
        }

        [TestMethod]
        [Description("GrammarParser binary serialization.")]
        public void GrammarParser()
        {
            var o = new GrammarParser();

            GrammarParser o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GrammarParser)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GrammarRuleNode binary serialization.")]
        public void GrammarRuleNode()
        {
            var o = new GrammarRuleNode("head");

            GrammarRuleNode o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GrammarRuleNode)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // GE
        #region Koza

        [TestMethod]
        [Description("CrossoverPipeline binary serialization.")]
        public void CrossoverPipeline()
        {
            var o = new CrossoverPipeline();

            CrossoverPipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (CrossoverPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("FullBuilder binary serialization.")]
        public void FullBuilder()
        {
            var o = new FullBuilder();

            FullBuilder o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (FullBuilder)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GrowBuilder binary serialization.")]
        public void GrowBuilder()
        {
            var o = new GrowBuilder();

            GrowBuilder o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GrowBuilder)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("HalfBuilder binary serialization.")]
        public void HalfBuilder()
        {
            var o = new HalfBuilder();

            HalfBuilder o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (HalfBuilder)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("KozaBuilder subclass binary serialization.")]
        public void KozaBuilder()
        {
            var o = new MyKozaBuilder();

            MyKozaBuilder o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MyKozaBuilder)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }
        [Serializable]
        private class MyKozaBuilder : KozaBuilder
        {
            public override IParameter DefaultBase
            {
                get { throw new NotImplementedException(); }
            }
            public override GPNode NewRootedTree(IEvolutionState state, GPType type, int thread, IGPNodeParent parent, GPFunctionSet funcs, int argPosition, int requestedSize)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        [Description("KozaFitness binary serialization.")]
        public void KozaFitness()
        {
            var o = new KozaFitness();

            KozaFitness o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (KozaFitness)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("KozaNodeSelector binary serialization.")]
        public void KozaNodeSelector()
        {
            var o = new KozaNodeSelector();

            KozaNodeSelector o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (KozaNodeSelector)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("KozaShortStatistics binary serialization.")]
        public void KozaShortStatistics()
        {
            var o = new KozaShortStatistics();

            KozaShortStatistics o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (KozaShortStatistics)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("MutationPipeline binary serialization.")]
        public void MutationPipeline()
        {
            var o = new MutationPipeline();

            MutationPipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MutationPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // Koza

        [TestMethod]
        [Description("ADF binary serialization.")]
        public void ADF()
        {
            var o = new ADF();

            ADF o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (ADF)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("ADFArgument binary serialization.")]
        public void ADFArgument()
        {
            var o = new ADFArgument();

            ADFArgument o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (ADFArgument)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("ADFContext binary serialization.")]
        public void ADFContext()
        {
            var o = new ADFContext();

            ADFContext o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (ADFContext)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("ADFStack binary serialization.")]
        public void ADFStack()
        {
            var o = new ADFStack();

            ADFStack o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (ADFStack)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("ADM binary serialization.")]
        public void ADM()
        {
            var o = new ADM();

            ADM o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (ADM)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("ERC subclass binary serialization.")]
        public void ERC()
        {
            var o = new MyERC();

            MyERC o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MyERC)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [Serializable]
        private class MyERC : ERC // Simply to test if ERC serializes
        {
            public override void ResetNode(IEvolutionState state, int nothing){}
            public override bool NodeEquals(GPNode node){return false;}
            public override string Encode(){return null;}
            public override void Eval(IEvolutionState state, int nothing, GPData data, ADFStack stack, GPIndividual ind, IProblem problem){}
        }

        [TestMethod]
        [Description("GPAtomicType binary serialization.")]
        public void GPAtomicType()
        {
            var o = new GPAtomicType();

            GPAtomicType o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GPAtomicType)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GPBreedingPipeline subclass binary serialization.")]
        public void GPBreedingPipeline()
        {
            var o = new MyGPBreedingPipeline();

            MyGPBreedingPipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MyGPBreedingPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [Serializable]
        private class MyGPBreedingPipeline : GPBreedingPipeline
        {
            public override int NumSources { get { return 0; } }
            public override IParameter DefaultBase { get { return null; } }
            public override int Produce(int i1, int i2, int i3, int i4, Individual[] inds, IEvolutionState state, int i5){return 0;}
        }

        [TestMethod]
        [Description("GPData subclass binary serialization.")]
        public void GPData()
        {
            var o = new MyGPData();

            MyGPData o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MyGPData)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [Serializable]
        private class MyGPData : GPData
        {
            public override void CopyTo(GPData other) { }
        }

        [TestMethod]
        [Description("GPFunctionSet binary serialization.")]
        public void GPFunctionSet()
        {
            var o = new GPFunctionSet();

            GPFunctionSet o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GPFunctionSet)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GPIndividual binary serialization.")]
        public void GPIndividual()
        {
            var o = new GPIndividual();

            GPIndividual o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GPIndividual)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GPInitializer binary serialization.")]
        public void GPInitializer()
        {
            var o = new GPInitializer();

            GPInitializer o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GPInitializer)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GPNode subclass binary serialization.")]
        public void GPNode()
        {
            var o = new MyGPNode();

            MyGPNode o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MyGPNode)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }
        [Serializable]
        private class MyGPNode : GPNode
        {
            public override string ToString() {return null;}
            public override void Eval(IEvolutionState state, int i1, GPData data, ADFStack stack, GPIndividual ind, IProblem problem){}
        }

        [TestMethod]
        [Description("GPNodeBuilder subclass binary serialization.")]
        public void GPNodeBuilder()
        {
            var o = new MyGPNodeBuilder();

            MyGPNodeBuilder o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MyGPNodeBuilder)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }
        [Serializable]
        private class MyGPNodeBuilder : GPNodeBuilder
        {
            public override IParameter DefaultBase
            {
                get { { throw new NotImplementedException();} }
            }
            public override GPNode NewRootedTree(IEvolutionState state, GPType type, int thread, IGPNodeParent parent, GPFunctionSet funcs, int argPosition, int requestedSize)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        [Description("GPNodeConstraints binary serialization.")]
        public void GPNodeConstraints()
        {
            var o = new GPNodeConstraints();

            GPNodeConstraints o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GPNodeConstraints)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GPNodeGatherer binary serialization.")]
        public void GPNodeGatherer()
        {
            var o = new GPNodeGatherer();

            GPNodeGatherer o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GPNodeGatherer)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GPProblem subclass binary serialization.")]
        public void GPProblem()
        {
            var o = new MyGPProblem();

            MyGPProblem o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MyGPProblem)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }
        [Serializable]
        private class MyGPProblem : GPProblem {}

        [TestMethod]
        [Description("GPSetType binary serialization.")]
        public void GPSetType()
        {
            var o = new GPSetType();

            GPSetType o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GPSetType)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GPSpecies binary serialization.")]
        public void GPSpecies()
        {
            var o = new GPSpecies();

            GPSpecies o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GPSpecies)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GPSpecies binary serialization.")]
        public void GPTree()
        {
            var o = new GPTree();

            GPTree o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GPTree)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GPTreeConstraints binary serialization.")]
        public void GPTreeConstraints()
        {
            var o = new GPTreeConstraints();

            GPTreeConstraints o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GPTreeConstraints)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GPType subclass binary serialization.")]
        public void GPType()
        {
            var o = new MyGPType();

            MyGPType o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MyGPType)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }
        [Serializable]
        private class MyGPType : GPType
        {
            public override bool CompatibleWith(GPInitializer initializer, GPType t)
            {
                throw new NotImplementedException();
            }
        }

        #endregion // GP
        #region MultiObjective

        [TestMethod]
        [Description("MultiObjectiveFitness binary serialization.")]
        public void MultiObjectiveFitness()
        {
            var o = new MultiObjectiveFitness();

            MultiObjectiveFitness o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MultiObjectiveFitness)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("MultiObjectiveFitnessComparator binary serialization. "
            + "Serializing this is pointless, but just in case an instance of it gets saved somewhere....")]
        public void MultiObjectiveFitnessComparator()
        {
            var o = new MultiObjectiveFitnessComparator();

            MultiObjectiveFitnessComparator o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MultiObjectiveFitnessComparator)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("MultiObjectiveStatistics binary serialization.")]
        public void MultiObjectiveStatistics()
        {
            var o = new MultiObjectiveStatistics();

            MultiObjectiveStatistics o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MultiObjectiveStatistics)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #region NSGA2

        [TestMethod]
        [Description("NSGA2Breeder binary serialization.")]
        public void NSGA2Breeder()
        {
            var o = new NSGA2Breeder();

            NSGA2Breeder o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (NSGA2Breeder)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("NSGA2Evaluator binary serialization.")]
        public void NSGA2Evaluator()
        {
            var o = new NSGA2Evaluator();

            NSGA2Evaluator o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (NSGA2Evaluator)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("NSGA2MultiObjectiveFitness binary serialization.")]
        public void NSGA2MultiObjectiveFitness()
        {
            var o = new NSGA2MultiObjectiveFitness();

            NSGA2MultiObjectiveFitness o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (NSGA2MultiObjectiveFitness)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("NSGA2MultiObjectiveFitnessComparator binary serialization.")]
        public void NSGA2MultiObjectiveFitnessComparator()
        {
            var o = new NSGA2MultiObjectiveFitnessComparator();

            NSGA2MultiObjectiveFitnessComparator o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (NSGA2MultiObjectiveFitnessComparator)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // NSGA2
        #region SPEA2

        [TestMethod]
        [Description("SPEA2Breeder binary serialization.")]
        public void SPEA2Breeder()
        {
            var o = new SPEA2Breeder();

            SPEA2Breeder o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SPEA2Breeder)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SPEA2Evaluator binary serialization.")]
        public void SPEA2Evaluator()
        {
            var o = new SPEA2Evaluator();

            SPEA2Evaluator o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SPEA2Evaluator)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SPEA2MultiObjectiveFitness binary serialization.")]
        public void SPEA2MultiObjectiveFitness()
        {
            var o = new SPEA2MultiObjectiveFitness();

            SPEA2MultiObjectiveFitness o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SPEA2MultiObjectiveFitness)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SPEA2TournamentSelection binary serialization.")]
        public void SPEA2TournamentSelection()
        {
            var o = new SPEA2TournamentSelection();

            SPEA2TournamentSelection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SPEA2TournamentSelection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // SPEA2

        #endregion // MultiObjective
        #region Parsimony

        [TestMethod]
        [Description("BucketTournamentSelection binary serialization.")]
        public void BucketTournamentSelection()
        {
            var o = new BucketTournamentSelection();

            BucketTournamentSelection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (BucketTournamentSelection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("DoubleTournamentSelection binary serialization.")]
        public void DoubleTournamentSelection()
        {
            var o = new DoubleTournamentSelection();

            DoubleTournamentSelection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (DoubleTournamentSelection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("LexicographicTournamentSelection binary serialization.")]
        public void LexicographicTournamentSelection()
        {
            var o = new LexicographicTournamentSelection();

            LexicographicTournamentSelection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (LexicographicTournamentSelection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("ProportionalTournamentSelection binary serialization.")]
        public void ProportionalTournamentSelection()
        {
            var o = new ProportionalTournamentSelection();

            ProportionalTournamentSelection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (ProportionalTournamentSelection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("RatioBucketTournamentSelection binary serialization.")]
        public void RatioBucketTournamentSelection()
        {
            var o = new RatioBucketTournamentSelection();

            RatioBucketTournamentSelection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (RatioBucketTournamentSelection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("TarpeianStatistics binary serialization.")]
        public void TarpeianStatistics()
        {
            var o = new TarpeianStatistics();

            TarpeianStatistics o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (TarpeianStatistics)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // Parsimony
        #region PSO

        [TestMethod]
        [Description("PSOBreeder binary serialization.")]
        public void PSOBreeder()
        {
            var o = new PSOBreeder();

            PSOBreeder o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (PSOBreeder)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("PSOSubpopulation binary serialization.")]
        public void PSOSubpopulation()
        {
            var o = new PSOSubpopulation();

            PSOSubpopulation o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (PSOSubpopulation)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // PSO
        #region Rule

        [TestMethod]
        [Description("Rule subclass binary serialization.")]
        public void Rule()
        {
            var o = new MyRule();

            MyRule o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MyRule)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }
        [Serializable]
        private class MyRule : Rule.Rule
        {
            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
            public override void Reset(IEvolutionState state, int thread)
            {
                throw new NotImplementedException();
            }
            public override int CompareTo(object o)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        [Description("RuleConstraints binary serialization.")]
        public void RuleConstraints()
        {
            var o = new RuleConstraints();

            RuleConstraints o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (RuleConstraints)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("RuleIndividual binary serialization.")]
        public void RuleIndividual()
        {
            var o = new RuleIndividual();

            RuleIndividual o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (RuleIndividual)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("RuleInitializer binary serialization.")]
        public void RuleInitializer()
        {
            var o = new RuleInitializer();

            RuleInitializer o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (RuleInitializer)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("RuleSet binary serialization.")]
        public void RuleSet()
        {
            var o = new RuleSet();

            RuleSet o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (RuleSet)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("RuleSetConstraints binary serialization.")]
        public void RuleSetConstraints()
        {
            var o = new RuleSetConstraints();

            RuleSetConstraints o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (RuleSetConstraints)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("RuleSpecies binary serialization.")]
        public void RuleSpecies()
        {
            var o = new RuleSpecies();

            RuleSpecies o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (RuleSpecies)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #region Breed

        [TestMethod]
        [Description("RuleCrossoverPipeline binary serialization.")]
        public void RuleCrossoverPipeline()
        {
            var o = new RuleCrossoverPipeline();

            RuleCrossoverPipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (RuleCrossoverPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("RuleMutationPipeline binary serialization.")]
        public void RuleMutationPipeline()
        {
            var o = new RuleMutationPipeline();

            RuleMutationPipeline o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (RuleMutationPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // Breed

        #endregion // Rule
        #region Select

        [TestMethod]
        [Description("BestSelection binary serialization.")]
        public void BestSelection()
        {
            var o = new BestSelection();

            BestSelection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (BestSelection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("BoltzmannSelection binary serialization.")]
        public void BoltzmannSelection()
        {
            var o = new BoltzmannSelection();

            BoltzmannSelection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (BoltzmannSelection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("FirstSelection binary serialization.")]
        public void FirstSelection()
        {
            var o = new FirstSelection();

            FirstSelection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (FirstSelection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("FitProportionateSelection binary serialization.")]
        public void FitProportionateSelection()
        {
            var o = new FitProportionateSelection();

            FitProportionateSelection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (FitProportionateSelection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("GreedyOverselection binary serialization.")]
        public void GreedyOverselection()
        {
            var o = new GreedyOverselection();

            GreedyOverselection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (GreedyOverselection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("MultiSelection binary serialization.")]
        public void MultiSelection()
        {
            var o = new MultiSelection();

            MultiSelection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (MultiSelection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("RandomSelection binary serialization.")]
        public void RandomSelection()
        {
            var o = new RandomSelection();

            RandomSelection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (RandomSelection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SigmaScalingSelection binary serialization.")]
        public void SigmaScalingSelection()
        {
            var o = new SigmaScalingSelection();

            SigmaScalingSelection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SigmaScalingSelection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SUSSelection binary serialization.")]
        public void SUSSelection()
        {
            var o = new SUSSelection();

            SUSSelection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SUSSelection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("TournamentSelection binary serialization.")]
        public void TournamentSelection()
        {
            var o = new TournamentSelection();

            TournamentSelection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (TournamentSelection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // Select
        #region Simple

        [TestMethod]
        [Description("SimpleBreeder binary serialization.")]
        public void SimpleBreeder()
        {
            var o = new SimpleBreeder();

            SimpleBreeder o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SimpleBreeder)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SimpleEvaluator binary serialization.")]
        public void SimpleEvaluator()
        {
            var o = new SimpleEvaluator();

            SimpleEvaluator o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SimpleEvaluator)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SimpleEvolutionState binary serialization.")]
        public void SimpleEvolutionState()
        {
            var o = new SimpleEvolutionState();

            SimpleEvolutionState o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SimpleEvolutionState)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SimpleExchanger binary serialization.")]
        public void SimpleExchanger()
        {
            var o = new SimpleExchanger();

            SimpleExchanger o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SimpleExchanger)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SimpleFinisher binary serialization.")]
        public void SimpleFinisher()
        {
            var o = new SimpleFinisher();

            SimpleFinisher o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SimpleFinisher)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SimpleFitness binary serialization.")]
        public void SimpleFitness()
        {
            var o = new SimpleFitness();

            SimpleFitness o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SimpleFitness)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SimpleInitializer binary serialization.")]
        public void SimpleInitializer()
        {
            var o = new SimpleInitializer();

            SimpleInitializer o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SimpleInitializer)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SimpleShortStatistics binary serialization.")]
        public void SimpleShortStatistics()
        {
            var o = new SimpleShortStatistics();

            SimpleShortStatistics o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SimpleShortStatistics)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SimpleStatistics binary serialization.")]
        public void SimpleStatistics()
        {
            var o = new SimpleStatistics();

            SimpleStatistics o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SimpleStatistics)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // Simple
        #region Spatial

        [TestMethod]
        [Description("Spatial1DSubpopulation binary serialization.")]
        public void Spatial1DSubpopulation()
        {
            var o = new Spatial1DSubpopulation();

            Spatial1DSubpopulation o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (Spatial1DSubpopulation)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SpatialBreeder binary serialization.")]
        public void SpatialBreeder()
        {
            var o = new SpatialBreeder();

            SpatialBreeder o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SpatialBreeder)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SpatialMultiPopCoevolutionaryEvaluator binary serialization.")]
        public void SpatialMultiPopCoevolutionaryEvaluator()
        {
            var o = new SpatialMultiPopCoevolutionaryEvaluator();

            SpatialMultiPopCoevolutionaryEvaluator o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SpatialMultiPopCoevolutionaryEvaluator)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SpatialTournamentSelection binary serialization.")]
        public void SpatialTournamentSelection()
        {
            var o = new SpatialTournamentSelection();

            SpatialTournamentSelection o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SpatialTournamentSelection)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // Spatial
        #region SteadyState

        [TestMethod]
        [Description("QueueIndividual binary serialization.")]
        public void QueueIndividual()
        {
            var o = new QueueIndividual(new BitVectorIndividual(), 2);

            QueueIndividual o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (QueueIndividual)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SteadyStateBreeder binary serialization.")]
        public void SteadyStateBreeder()
        {
            var o = new SteadyStateBreeder();

            SteadyStateBreeder o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SteadyStateBreeder)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SteadyStateEvaluator binary serialization.")]
        public void SteadyStateEvaluator()
        {
            var o = new SteadyStateEvaluator();

            SteadyStateEvaluator o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SteadyStateEvaluator)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        [TestMethod]
        [Description("SteadyStateEvolutionState binary serialization.")]
        public void SteadyStateEvolutionState()
        {
            var o = new SteadyStateEvolutionState();

            SteadyStateEvolutionState o2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Position = 0;
                o2 = (SteadyStateEvolutionState)bf.Deserialize(ms);
            }
            Assert.IsNotNull(o2);
        }

        #endregion // SteadyState
        #region Vector

        #region Vector Species Family

        [TestMethod]
        [Description("VectorSpecies binary serialization. This is the base for other members of VectorSpecies family.")]
        public void VectorSpecies()
        {
            var spec = new VectorSpecies();

            VectorSpecies spec2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, spec);
                ms.Position = 0;
                spec2 = (VectorSpecies)bf.Deserialize(ms);
            }
            Assert.IsNotNull(spec2);
        }

        [TestMethod]
        [Description("FloatVectorSpecies binary serialization.")]
        public void FloatVectorSpecies()
        {
            var spec = new FloatVectorSpecies();

            FloatVectorSpecies spec2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, spec);
                ms.Position = 0;
                spec2 = (FloatVectorSpecies)bf.Deserialize(ms);
            }
            Assert.IsNotNull(spec2);
        }

        [TestMethod]
        [Description("GeneVectorSpecies binary serialization.")]
        public void GeneVectorSpecies()
        {
            var spec = new GeneVectorSpecies();

            GeneVectorSpecies spec2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, spec);
                ms.Position = 0;
                spec2 = (GeneVectorSpecies)bf.Deserialize(ms);
            }
            Assert.IsNotNull(spec2);
        }

        [TestMethod]
        [Description("IntegerVectorSpecies binary serialization.")]
        public void IntegerVectorSpecies()
        {
            var spec = new IntegerVectorSpecies();

            IntegerVectorSpecies spec2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, spec);
                ms.Position = 0;
                spec2 = (IntegerVectorSpecies)bf.Deserialize(ms);
            }
            Assert.IsNotNull(spec2);
        }

        #endregion // Vector Species Family
        #region Vector Individuals Family

        [TestMethod]
        [Description("BitVectorIndividual binary serialization with only a Fitness initialized (Allows us to use CompareTo).")]
        public void BitVectorIndivdidualWithSimpleFitness()
        {
            var ind = new BitVectorIndividual();
            var fit = new SimpleFitness();
            ind.Fitness = fit;

            BitVectorIndividual ind2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, ind);
                ms.Position = 0;
                ind2 = (BitVectorIndividual) bf.Deserialize(ms);
            }
            Assert.AreEqual(ind.CompareTo(ind2), 0);
        }

        [TestMethod]
        [Description("ByteVectorIndividual binary serialization with only a Fitness initialized (Allows us to use CompareTo).")]
        public void ByteVectorIndivdidualWithSimpleFitness()
        {
            var ind = new ByteVectorIndividual();
            var fit = new SimpleFitness();
            ind.Fitness = fit;

            ByteVectorIndividual ind2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, ind);
                ms.Position = 0;
                ind2 = (ByteVectorIndividual)bf.Deserialize(ms);
            }
            Assert.AreEqual(ind.CompareTo(ind2), 0);
        }

        [TestMethod]
        [Description("DoubleVectorIndividual binary serialization with only a Fitness initialized (Allows us to use CompareTo).")]
        public void DoubleVectorIndivdidualWithSimpleFitness()
        {
            var ind = new DoubleVectorIndividual();
            var fit = new SimpleFitness();
            ind.Fitness = fit;

            DoubleVectorIndividual ind2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, ind);
                ms.Position = 0;
                ind2 = (DoubleVectorIndividual)bf.Deserialize(ms);
            }
            Assert.AreEqual(ind.CompareTo(ind2), 0);
        }

        [TestMethod]
        [Description("FloatVectorIndividual binary serialization with only a Fitness initialized (Allows us to use CompareTo).")]
        public void FloatVectorIndivdidualWithSimpleFitness()
        {
            var ind = new FloatVectorIndividual();
            var fit = new SimpleFitness();
            ind.Fitness = fit;

            FloatVectorIndividual ind2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, ind);
                ms.Position = 0;
                ind2 = (FloatVectorIndividual)bf.Deserialize(ms);
            }
            Assert.AreEqual(ind.CompareTo(ind2), 0);
        }

        [TestMethod]
        [Description("GeneVectorIndividual binary serialization with only a Fitness initialized (Allows us to use CompareTo).")]
        public void GeneVectorIndivdidualWithSimpleFitness()
        {
            var ind = new GeneVectorIndividual();
            var fit = new SimpleFitness();
            ind.Fitness = fit;

            GeneVectorIndividual ind2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, ind);
                ms.Position = 0;
                ind2 = (GeneVectorIndividual)bf.Deserialize(ms);
            }
            Assert.AreEqual(ind.CompareTo(ind2), 0);
        }

        [TestMethod]
        [Description("IntegerVectorIndividual binary serialization with only a Fitness initialized (Allows us to use CompareTo).")]
        public void IntegerVectorIndivdidualWithSimpleFitness()
        {
            var ind = new IntegerVectorIndividual();
            var fit = new SimpleFitness();
            ind.Fitness = fit;

            IntegerVectorIndividual ind2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, ind);
                ms.Position = 0;
                ind2 = (IntegerVectorIndividual)bf.Deserialize(ms);
            }
            Assert.AreEqual(ind.CompareTo(ind2), 0);
        }

        [TestMethod]
        [Description("LongVectorIndividual binary serialization with only a Fitness initialized (Allows us to use CompareTo).")]
        public void LongVectorIndivdidualWithSimpleFitness()
        {
            var ind = new LongVectorIndividual();
            var fit = new SimpleFitness();
            ind.Fitness = fit;

            LongVectorIndividual ind2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, ind);
                ms.Position = 0;
                ind2 = (LongVectorIndividual)bf.Deserialize(ms);
            }
            Assert.AreEqual(ind.CompareTo(ind2), 0);
        }

        [TestMethod]
        [Description("ShortVectorIndividual binary serialization with only a Fitness initialized (Allows us to use CompareTo).")]
        public void ShortVectorIndivdidualWithSimpleFitness()
        {
            var ind = new ShortVectorIndividual();
            var fit = new SimpleFitness();
            ind.Fitness = fit;

            ShortVectorIndividual ind2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, ind);
                ms.Position = 0;
                ind2 = (ShortVectorIndividual)bf.Deserialize(ms);
            }
            Assert.AreEqual(ind.CompareTo(ind2), 0);
        }

        #endregion // Vector Individuals Family
        #region Vector Breeding Pipelines

        [TestMethod]
        [Description("GeneDuplicationPipeline binary serialization.")]
        public void GeneDuplicationPipeline()
        {
            var p = new GeneDuplicationPipeline();

            GeneDuplicationPipeline p2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, p);
                ms.Position = 0;
                p2 = (GeneDuplicationPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(p2);
        }

        [TestMethod]
        [Description("ListCrossoverPipeline binary serialization.")]
        public void ListCrossoverPipeline()
        {
            var p = new ListCrossoverPipeline();

            ListCrossoverPipeline p2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, p);
                ms.Position = 0;
                p2 = (ListCrossoverPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(p2);
        }

        [TestMethod]
        [Description("MultipleVectorCrossoverPipeline binary serialization.")]
        public void MultipleVectorCrossoverPipeline()
        {
            var p = new MultipleVectorCrossoverPipeline();

            MultipleVectorCrossoverPipeline p2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, p);
                ms.Position = 0;
                p2 = (MultipleVectorCrossoverPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(p2);
        }

        [TestMethod]
        [Description("VectorCrossoverPipeline binary serialization.")]
        public void VectorCrossoverPipeline()
        {
            var p = new VectorCrossoverPipeline();

            VectorCrossoverPipeline p2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, p);
                ms.Position = 0;
                p2 = (VectorCrossoverPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(p2);
        }

        [TestMethod]
        [Description("VectorMutationPipeline binary serialization.")]
        public void VectorMutationPipeline()
        {
            var p = new VectorMutationPipeline();

            VectorMutationPipeline p2;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, p);
                ms.Position = 0;
                p2 = (VectorMutationPipeline)bf.Deserialize(ms);
            }
            Assert.IsNotNull(p2);
        }

        #endregion // Vector Breeding Pipelines

        #endregion // Vector

        #endregion // Binary Serialization
    }
}