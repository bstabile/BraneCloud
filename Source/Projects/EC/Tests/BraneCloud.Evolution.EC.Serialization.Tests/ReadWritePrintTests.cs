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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.GP.Koza;
using BraneCloud.Evolution.EC.MultiObjective;
using BraneCloud.Evolution.EC.Randomization;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.Serialization.Tests
{
    /// <summary>
    /// Read | Write | Print     Tests
    /// TODO: Needs more coverage.
    /// </summary>
    [TestClass]
    public class ReadWritePrintTests
    {
        #region Housekeeping

        public ReadWritePrintTests()
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

        #region Binary Write and Read

        // Individuals and Finesses have binary Write and Read methods that can be used
        // to transfer their "vitals" to and from a stream. This is primarily useful
        // for coevolutionary logic, and, as such, it is different from serialization.

        #region Fitness Types

        [TestMethod]
        [Description("SimpleFitness information is written to a binary stream and read back to update another instance.")]
        public void SimpleFitnessWriteAndRead()
        {
            var rand = new MersenneTwisterFast(0);
            var f = new SimpleFitness();
            f.SetFitness(null, float.MaxValue, true);

            // Set up some random Trials just to check that they get transmitted
            const int n = 10;
            f.Trials = new List<double>(n);
            for (var i = 0; i < n; i++)
            {
                f.Trials.Add(rand.NextDouble());
            }

            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                f.WriteFitness(null, writer); // Write
                ms.Position = 0;
                var reader = new BinaryReader(ms);
                var f2 = new SimpleFitness();
                f2.ReadFitness(null, reader); // Read

                // Compare
                Assert.AreEqual(f.Value, f2.Value); // Value is same
                Assert.IsTrue(f2.IsIdeal); // Fitness is ideal
                Assert.AreEqual(f2.Trials.Count, f.Trials.Count); // Number of trials is the same
                for (var i = 0; i < f.Trials.Count; i++)
                {
                    Assert.AreEqual((double)f.Trials[i], (double)f2.Trials[i]); // Trial values are all the same
                }
            }
        }

        [TestMethod]
        [Description("KozaFitness information is written to a binary stream and read back to update another instance.")]
        public void KozaFitnessWriteAndRead()
        {
            var rand = new MersenneTwisterFast(0);
            var f = new KozaFitness();
            // Standardized fitness ranges from 0.0f inclusive to PositiveInfinity exclusive where 0.0f is ideal
            // Adjusted fitness ranges from 0.0f inclusive to 1.0f exclusive, where higher is better
            f.SetStandardizedFitness(null, float.MaxValue);

            // Set up some random Trials just to check that they get transmitted
            const int n = 10;
            f.Trials = new List<double>(n);
            for (var i = 0; i < n; i++)
            {
                f.Trials.Add(rand.NextDouble() * double.MaxValue); // in the half-open interval [0.0, double.MaxValue)
            }

            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                f.WriteFitness(null, writer); // Write
                ms.Position = 0;
                var reader = new BinaryReader(ms);
                var f2 = new KozaFitness();
                f2.ReadFitness(null, reader); // Read

                // Compare
                Assert.AreEqual(f.Value, f2.Value); // Value is same
                // KozaFitness defines 'Value' as AdjustedFitness [0.0, 1.0), i.e. zero is valid but one is not
                Assert.IsTrue(f.Value < 1.0f && f2.Value < 1.0f);
                Assert.IsFalse(f2.IsIdeal); // Fitness is ideal
                Assert.AreEqual(f2.Trials.Count, f.Trials.Count); // Number of trials is the same
                for (var i = 0; i < f.Trials.Count; i++)
                {
                    Assert.AreEqual((double)f.Trials[i], (double)f2.Trials[i]); // Trial values are all the same
                }
            }
        }

        [TestMethod]
        [Description("MultiObjectiveFitness information is written to a binary stream and read back to update another instance.")]
        public void MultiObjectiveFitnessWriteAndRead()
        {
            var rand = new MersenneTwisterFast(0);
            var f = new MultiObjectiveFitness(2);
            var f2 = new MultiObjectiveFitness(2); // We'll use this when we read the other one back in
            f2.SetObjectives(null, new []{ 0.0, 0.0 }); // setting these to zero allows us to check if they change

            // Default is to Maximize!

            // These need to be set up manually here because we are not running 'Setup'.
            // Every instance would thus normally share the same min and max values.
            f.MaxObjective = new [] { 1.0, 1.0 };
            f.MinObjective = new [] { 0.0, 0.0 };
            f2.MaxObjective = new[] { 1.0, 1.0 };
            f2.MinObjective = new[] { 0.0, 0.0 };

            // Set two objective (fitness) values, worst and best respectively
            f.SetObjectives(null, new [] { 0.0, 1.0 });

            // Set up some random Trials just to check that they get transmitted
            const int n = 10;
            f.Trials = new List<double>(n);
            for (var i = 0; i < n; i++)
            {
                f.Trials.Add(rand.NextDouble() * double.MaxValue); // in the half-open interval [0.0, double.MaxValue)
            }

            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                f.WriteFitness(null, writer); // Write
                ms.Position = 0;
                var reader = new BinaryReader(ms);
                f2.ReadFitness(null, reader); // Read

                // Compare
                Assert.AreEqual(f.Value, f2.Value); // Value is same. This is the MAX objective value in the array

                // Just for kicks, lets make sure BOTH objectives are equal to the original values
                Assert.AreEqual(f.GetObjective(0), 0.0);
                Assert.AreEqual(f.GetObjective(0), f2.GetObjective(0));
                Assert.AreEqual(f.GetObjective(1), 1.0);
                Assert.AreEqual(f.GetObjective(1), f2.GetObjective(1));

                // And let's make sure our MAX and MIN are the same (these wouldn't normally change once established in 'Setup')
                Assert.AreEqual(f.MinObjective[0], f2.MinObjective[0]);
                Assert.AreEqual(f.MinObjective[1], f2.MinObjective[1]);
                Assert.AreEqual(f.MaxObjective[0], f2.MaxObjective[0]);
                Assert.AreEqual(f.MaxObjective[1], f2.MaxObjective[1]);

                Assert.IsTrue(f.Value <= 1.0f && f2.Value <= 1.0f); // This is the MAX objective value in the array
                Assert.IsFalse(f2.IsIdeal); // Fitness is not ideal by default (ideal must be defined in subclass)

                Assert.AreEqual(f2.Trials.Count, f.Trials.Count); // Number of trials is the same
                for (var i = 0; i < f.Trials.Count; i++)
                {
                    Assert.AreEqual((double)f.Trials[i], (double)f2.Trials[i]); // Trial values are all the same
                }
            }
        }

        #endregion // Fitness Types
        #region Individual Types

        #region GP (Complex. Writes and Reads a Graph of objects) 

        // The Individual Write/Read methods delegate some of the work to abstract methods Write/ReadGenotype.
        // In GPIndividual the genotype is an array of GPTrees. So each GPTree is asked to write itself.
        // In GPTree the WriteTree method first gets hold of the GPInitializer from State and then calls
        // the Child (root GPNode) WriteRootedTree method, passing in (along with the State and the BinaryWriter)
        // the GPType and the GPFunctionSet, associated with the initializer: 
        //      Constraints(initializer).TreeType
        //      Constraints(initializer).FunctionSet
        // The Constraints method simply returns a GPConstraints instance using ConstraintsCount as an index:
        //      return initializer.TreeConstraints[ConstraintsCount];
        // GPNode.WriteRootedTree then writes the following:
        //      Children.Length (the number of children)
        //      Node.Type (searches through the GPFunctionSet -- Terminals and Non-Terminals -- to find the preassigned type index)
        //      WriteNode (the default implementation does nothing)
        //      Calls WriteRootedTree (obviously recursive) on each of its children...

        // Thus working backwards, we can see that we need to create at least 
        // one GPType and one GPNode (for the root node) to define a minimal tree.

        [TestMethod]
        [Description("")]
        public void GPNodeWriteAndRead()
        {
            var state = new SimpleEvolutionState();
            var initializer = new GPInitializer();
            state.Initializer = initializer; // Ensure that our initializer is accessible
            
            var treeConstraints = new GPTreeConstraints();
            initializer.TreeConstraints = new List<GPTreeConstraints>();
            initializer.TreeConstraints.Add(treeConstraints); // Ensure that our constraints are accessible

            var nodeConstraints = new GPNodeConstraints();

            var rootType = new GPAtomicType("MyRootType");
            treeConstraints.TreeType = rootType;
            var root = new MyRootNode();
            var tree = new GPTree {Child = root};
            root.Parent = tree;
            root.ArgPosition = 0; // This is the root, so there is only one position (0)
        }

        [Serializable]
        private class MyTreeRootType : GPType
        {
            public override bool CompatibleWith(GPInitializer initializer, GPType t)
            {
                return t.Type == Type ? true : false;
            }
        }
        [Serializable]
        private class MyRootNode : GPNode
        {
            public MyRootNode()
            {
                Children = new GPNode[0]; // zero children! We are a terminal node.
            }
            public override void Eval(IEvolutionState state, int thread, GPData input, ADFStack stack, GPIndividual individual, IProblem problem)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// The ToString() method is called by default by the getter for the "Name" property in the base class.
            /// So to ensure that our derivation returns the proper name, we simply override this to deliver what is expected.
            /// Note that we could also override the Name getter and achieve the same result. 
            /// Then we could do something different with ToString(). But that might affect something else, so...
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return "MyRootNode";
            }
        }

        [Ignore]
        [TestMethod]
        [Description("GPIndividual's vital data is written to a binary stream and read back into another instance.")]
        public void GPIndividualWriteAndRead()
        {
            // First we'll set up a Fitness for the Individual
            var rand = new MersenneTwisterFast(0);

            // Initialize some required context
            var state = new SimpleEvolutionState();
            var initializer = new GPInitializer();
            state.Initializer = initializer; // Required for GPTree.WriteTree(...)
            var constraints = new GPTreeConstraints {Name = "TestTreeConstraints"};
            //initializer.TreeConstraints
            var tree = new GPTree();

            var f = new KozaFitness();
            f.SetStandardizedFitness(null, float.MaxValue);

            const int n = 10;
            f.Trials = new List<double>(n);
            for (var i = 0; i < n; i++)
            {
                f.Trials.Add(rand.NextDouble());
            }

            // Now we can create and initialize the Individual
            var ind = new GPIndividual();
            var ind2 = new GPIndividual(); // We'll read back into this instance

            ind.Trees = new GPTree[1]; // This is the set of genes

            ind.Trees[0] = new GPTree();

            ind.Fitness = f;
            ind.Evaluated = true;

            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                ind.WriteIndividual(null, writer);

                ms.Position = 0;
                var reader = new BinaryReader(ms);

                ind2.Fitness = new SimpleFitness();
                ind2.ReadIndividual(null, reader);

                Assert.IsTrue(ind.Fitness.EquivalentTo(ind2.Fitness));
                Assert.IsTrue(ind2.Fitness.IsIdeal);
                Assert.IsTrue(ind.Equals(ind2)); // Genetically equivalent

                Assert.AreEqual(ind.Trees.Length, ind2.Trees.Length);
            }
        }

        #endregion
        #region Vector

        [TestMethod]
        [Description("BitVectorIndividual's vital data is written to a binary stream and read back into another instance.")]
        public void BitVectorIndividualWriteAndRead()
        {
            // First we'll set up a Fitness for the Individual
            var rand = new MersenneTwisterFast(0);
            var f = new SimpleFitness();
            f.SetFitness(null, float.MaxValue, true);

            const int n = 10;
            f.Trials = new List<double>(n);
            for (var i = 0; i < n; i++)
            {
                f.Trials.Add(rand.NextDouble());
            }

            // Now we can create and initialize the Individual
            var ind = new BitVectorIndividual();
            var ind2 = new BitVectorIndividual(); // We'll read back into this instance

            ind.Genome = new bool[10]; // This is the set of genes
            for (var i = 0; i < 10; i++)
            {
                ind.genome[i] = rand.NextBoolean();
            }
            ind.Fitness = f;
            ind.Evaluated = true;

            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                ind.WriteIndividual(null, writer);

                ms.Position = 0;
                var reader = new BinaryReader(ms);

                ind2.Fitness = new SimpleFitness();
                ind2.ReadIndividual(null, reader);

                Assert.IsTrue(ind.Fitness.EquivalentTo(ind2.Fitness));
                Assert.IsTrue(ind2.Fitness.IsIdeal);
                Assert.IsTrue(ind.Equals(ind2)); // Genetically equivalent
                for (var i = 0; i < 10; i++)
                {
                    Assert.AreEqual(ind.genome[i], ind2.genome[i]); // check each gene
                }
            }
        }

        [TestMethod]
        [Description("ByteVectorIndividual's vital data is written to a binary stream and read back into another instance.")]
        public void ByteVectorIndividualWriteAndRead()
        {
            // First we'll set up a Fitness for the Individual
            var rand = new MersenneTwisterFast(0);
            var f = new SimpleFitness();
            f.SetFitness(null, float.MaxValue, true);

            const int n = 10;
            f.Trials = new List<double>(n);
            for (var i = 0; i < n; i++)
            {
                f.Trials.Add(rand.NextDouble());
            }

            // Now we can create and initialize the Individual
            var ind = new ByteVectorIndividual();
            var ind2 = new ByteVectorIndividual(); // We'll read back into this instance

            ind.Genome = new sbyte[10]; // This is the set of genes
            rand.NextBytes(ind.genome); // This will set the genes to random sbyte values

            ind.Fitness = f;
            ind.Evaluated = true;

            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                ind.WriteIndividual(null, writer);

                ms.Position = 0;
                var reader = new BinaryReader(ms);

                ind2.Fitness = new SimpleFitness();
                ind2.ReadIndividual(null, reader);

                Assert.IsTrue(ind.Fitness.EquivalentTo(ind2.Fitness));
                Assert.IsTrue(ind2.Fitness.IsIdeal);
                Assert.IsTrue(ind.Equals(ind2)); // Genetically equivalent
                for (var i = 0; i < 10; i++)
                {
                    Assert.AreEqual(ind.genome[i], ind2.genome[i]); // check each gene
                }
            }
        }

        [TestMethod]
        [Description("DoubleVectorIndividual's vital data is written to a binary stream and read back into another instance.")]
        public void DoubleVectorIndividualWriteAndRead()
        {
            // First we'll set up a Fitness for the Individual
            var rand = new MersenneTwisterFast(0);
            var f = new SimpleFitness();
            f.SetFitness(null, float.MaxValue, true);

            const int n = 10;
            f.Trials = new List<double>(n);
            for (var i = 0; i < n; i++)
            {
                f.Trials.Add(rand.NextDouble());
            }

            // Now we can create and initialize the Individual
            var ind = new DoubleVectorIndividual();
            var ind2 = new DoubleVectorIndividual(); // We'll read back into this instance

            ind.Genome = new double[10]; // This is the set of genes
            for (var i = 0; i < 10; i++)
            {
                ind.genome[i] = rand.NextDouble() * double.MaxValue; // some random genes
            }
            ind.Fitness = f;
            ind.Evaluated = true;

            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                ind.WriteIndividual(null, writer);

                ms.Position = 0;
                var reader = new BinaryReader(ms);

                ind2.Fitness = new SimpleFitness();
                ind2.ReadIndividual(null, reader);

                Assert.IsTrue(ind.Fitness.EquivalentTo(ind2.Fitness));
                Assert.IsTrue(ind2.Fitness.IsIdeal);
                Assert.IsTrue(ind.Equals(ind2)); // Genetically equivalent
                for (var i = 0; i < 10; i++)
                {
                    Assert.AreEqual(ind.genome[i], ind2.genome[i]); // check each gene
                }
            }
        }

        [TestMethod]
        [Description("FloatVectorIndividual's vital data is written to a binary stream and read back into another instance.")]
        public void FloatVectorIndividualWriteAndRead()
        {
            // First we'll set up a Fitness for the Individual
            var rand = new MersenneTwisterFast(0);
            var f = new SimpleFitness();
            f.SetFitness(null, float.MaxValue, true);

            const int n = 10;
            f.Trials = new List<double>(n);
            for (var i = 0; i < n; i++)
            {
                f.Trials.Add(rand.NextDouble());
            }

            // Now we can create and initialize the Individual
            var ind = new FloatVectorIndividual();
            var ind2 = new FloatVectorIndividual(); // We'll read back into this instance

            ind.Genome = new float[10]; // This is the set of genes
            for (var i = 0; i < 10; i++)
            {
                ind.genome[i] = ((float)rand.NextDouble()) * float.MaxValue; // some random genes
            }
            ind.Fitness = f;
            ind.Evaluated = true;

            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                ind.WriteIndividual(null, writer);

                ms.Position = 0;
                var reader = new BinaryReader(ms);

                ind2.Fitness = new SimpleFitness();
                ind2.ReadIndividual(null, reader);

                Assert.IsTrue(ind.Fitness.EquivalentTo(ind2.Fitness));
                Assert.IsTrue(ind2.Fitness.IsIdeal);
                Assert.IsTrue(ind.Equals(ind2)); // Genetically equivalent
                for (var i = 0; i < 10; i++)
                {
                    Assert.AreEqual(ind.genome[i], ind2.genome[i]); // check each gene
                }
            }
        }

        [TestMethod]
        [Description("IntegerVectorIndividual's vital data is written to a binary stream and read back into another instance.")]
        public void IntegerVectorIndividualWriteAndRead()
        {
            // First we'll set up a Fitness for the Individual
            var rand = new MersenneTwisterFast(0);
            var f = new SimpleFitness();
            f.SetFitness(null, float.MaxValue, true);

            const int n = 10;
            f.Trials = new List<double>(n);
            for (var i = 0; i < n; i++)
            {
                f.Trials.Add(rand.NextDouble());
            }

            // Now we can create and initialize the Individual
            var ind = new IntegerVectorIndividual();
            var ind2 = new IntegerVectorIndividual(); // We'll read back into this instance

            ind.Genome = new int[10]; // This is the set of genes
            for (var i = 0; i < 10; i++)
            {
                ind.genome[i] = rand.NextInt(); // Some random genes
            }
            ind.Fitness = f;
            ind.Evaluated = true;

            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                ind.WriteIndividual(null, writer);

                ms.Position = 0;
                var reader = new BinaryReader(ms);

                ind2.Fitness = new SimpleFitness();
                ind2.ReadIndividual(null, reader);

                Assert.IsTrue(ind.Fitness.EquivalentTo(ind2.Fitness));
                Assert.IsTrue(ind2.Fitness.IsIdeal);
                Assert.IsTrue(ind.Equals(ind2)); // Genetically equivalent
                for (var i = 0; i < 10; i++)
                {
                    Assert.AreEqual(ind.genome[i], ind2.genome[i]); // check each gene
                }
            }
        }

        [TestMethod]
        [Description("LongVectorIndividual's vital data is written to a binary stream and read back into another instance.")]
        public void LongVectorIndividualWriteAndRead()
        {
            // First we'll set up a Fitness for the Individual
            var rand = new MersenneTwisterFast(0);
            var f = new SimpleFitness();
            f.SetFitness(null, float.MaxValue, true);

            const int n = 10;
            f.Trials = new List<double>(n);
            for (var i = 0; i < n; i++)
            {
                f.Trials.Add(rand.NextDouble());
            }

            // Now we can create and initialize the Individual
            var ind = new LongVectorIndividual();
            var ind2 = new LongVectorIndividual(); // We'll read back into this instance

            ind.Genome = new long[10]; // This is the set of genes
            for (var i = 0; i < 10; i++)
            {
                ind.genome[i] = rand.NextLong(); // Some random genes
            }
            ind.Fitness = f;
            ind.Evaluated = true;

            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                ind.WriteIndividual(null, writer);

                ms.Position = 0;
                var reader = new BinaryReader(ms);

                ind2.Fitness = new SimpleFitness();
                ind2.ReadIndividual(null, reader);

                Assert.IsTrue(ind.Fitness.EquivalentTo(ind2.Fitness));
                Assert.IsTrue(ind2.Fitness.IsIdeal);
                Assert.IsTrue(ind.Equals(ind2)); // Genetically equivalent
                for (var i = 0; i < 10; i++)
                {
                    Assert.AreEqual(ind.genome[i], ind2.genome[i]); // check each gene
                }
            }
        }

        [TestMethod]
        [Description("ShortVectorIndividual's vital data is written to a binary stream and read back into another instance.")]
        public void ShortVectorIndividualWriteAndRead()
        {
            // First we'll set up a Fitness for the Individual
            var rand = new MersenneTwisterFast(0);
            var f = new SimpleFitness();
            f.SetFitness(null, float.MaxValue, true);

            const int n = 10;
            f.Trials = new List<double>(n);
            for (var i = 0; i < n; i++)
            {
                f.Trials.Add(rand.NextDouble());
            }

            // Now we can create and initialize the Individual
            var ind = new ShortVectorIndividual();
            var ind2 = new ShortVectorIndividual(); // We'll read back into this instance

            ind.Genome = new short[10]; // This is the set of genes
            for (var i = 0; i < 10; i++)
            {
                ind.genome[i] = (short)rand.NextInt(short.MaxValue); // Some random genes
            }
            ind.Fitness = f;
            ind.Evaluated = true;

            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                ind.WriteIndividual(null, writer);

                ms.Position = 0;
                var reader = new BinaryReader(ms);

                ind2.Fitness = new SimpleFitness();
                ind2.ReadIndividual(null, reader);

                Assert.IsTrue(ind.Fitness.EquivalentTo(ind2.Fitness));
                Assert.IsTrue(ind2.Fitness.IsIdeal);
                Assert.IsTrue(ind.Equals(ind2)); // Genetically equivalent
                for (var i = 0; i < 10; i++)
                {
                    Assert.AreEqual(ind.genome[i], ind2.genome[i]); // check each gene
                }
            }
        }

        #endregion // Vector

        #endregion // Individual Types

        #endregion // Write and Read
    }
}