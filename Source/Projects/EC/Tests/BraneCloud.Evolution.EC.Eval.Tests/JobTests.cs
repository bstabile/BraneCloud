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

using System.Collections;
using System.Collections.Generic;
using BraneCloud.Evolution.EC.Runtime.Eval;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.Eval.Tests
{
    /// <summary>
    /// Job Tests
    /// </summary>
    [TestClass]
    public class JobTests
    {
        #region Housekeeping

        public JobTests()
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

        //[Ignore]
        [TestMethod]
        [Description(
            "This ensures that individuals copied forward to a slave for evalutation" 
            + " are NOT the same ones that are copied back (i.e. their references are different)."
            + " This is accomplished by serializing and deserializing the individuals using BinaryFormatter."
            + " The reason we avoid using 'Clone' is that there could be special deep/shallow semantics that"
            + " are not relevant to the simple mechanism needed in a master/slave situation where individuals"
            + " are fully expected to be manipulated in various ways remotely and then returned as first-order"
            + " citizens to the same spot that was previously held by an entirely different 'instance'.")]
        public void CopyIndividualsForwardAndBack()
        {
            var state = new SimpleEvolutionState();
            var job = new Job();
            var ind = new FloatVectorIndividual {Fitness = new SimpleFitness()};
            var genome = new float[1] {1.1f};
            ind.Genome = genome;

            job.Inds = new Individual[]{ ind };

            job.CopyIndividualsForward();
            var newFit = new SimpleFitness {Trials = new List<double>(10)};
            for (var i = 0; i < 10; i++)
            {
                newFit.Trials.Add(i*100.0);
            }
            newFit.SetFitness(state, float.MaxValue, true);
            job.NewInds[0].Fitness = newFit;
            job.CopyIndividualsBack(state);
            Assert.IsTrue(job.Inds[0].Fitness.Value == newFit.Value); // Fitness has been updated
            for (var i = 0; i < job.Inds[0].Fitness.Trials.Count; i++)
            {
                Assert.AreEqual(job.Inds[0].Fitness.Trials[i], i*100.0); // Trials have been merged into original instance
            }
        }
    }
}