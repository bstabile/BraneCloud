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

namespace BraneCloud.Evolution.Collections.Generic.Tests
{
    /// <summary>
    /// Summary description for SynchronizedDictionaryTests
    /// </summary>
    [TestClass]
    public class SynchronizedDictionaryTests
    {
        #region Housekeeping

        public SynchronizedDictionaryTests()
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

        #region Tracking Tests

        [TestMethod]
        public void TrackAccessedTrueEnablesAccessedCollection()
        {
            var sd = new SynchronizedDictionary<string, string>();

            // here the tracking collections are null
            Assert.IsNull(sd.Accessed);
            Assert.IsNull(sd.Gotten);

            sd.TrackAccessed = true;
            // now the Accessed collection will be available
            Assert.IsNotNull(sd.Accessed);
            // but the Gotten collection is still null
            Assert.IsNull(sd.Gotten);
        }

        [TestMethod]
        public void TrackGottenTrueEnablesGottenCollection()
        {
            var sd = new SynchronizedDictionary<string, string>();

            // here the tracking collections are null
            Assert.IsNull(sd.Accessed);
            Assert.IsNull(sd.Gotten);

            sd.TrackGotten = true;
            // now the Gotten collection will be available
            Assert.IsNotNull(sd.Gotten);
            // but the Accessed collection is still null
            Assert.IsNull(sd.Accessed);
        }

        [TestMethod]
        public void TrackAllTrueEnablesAccessedAndGottenCollections()
        {
            var sd = new SynchronizedDictionary<string, string>();

            // No tracking available yet
            Assert.IsNull(sd.Accessed);
            Assert.IsNull(sd.Gotten);

            Assert.IsFalse(sd.TrackAccessed);
            Assert.IsFalse(sd.TrackGotten);

            sd.TrackAll = true;

            // Now tracking is enabled and collections have been created
            Assert.IsTrue(sd.TrackAccessed && sd.TrackGotten);
            Assert.IsNotNull(sd.Accessed);
            Assert.IsNotNull(sd.Gotten);
        }

        [TestMethod]
        public void TrackAllFalseDisablesAccessedAndGottenCollections()
        {
            var sd = new SynchronizedDictionary<string, string>();

            Assert.IsNull(sd.Accessed);
            Assert.IsNull(sd.Gotten);

            // Default is tracking disabled
            Assert.IsFalse(sd.TrackAccessed);
            Assert.IsFalse(sd.TrackGotten);

            sd.TrackAll = true;

            // First ensure that tracking is enabled
            Assert.IsTrue(sd.TrackAccessed && sd.TrackGotten);
            Assert.IsNotNull(sd.Accessed);
            Assert.IsNotNull(sd.Gotten);

            sd.TrackAll = false;

            // Now ensure that tracking has been disabled
            Assert.IsFalse(sd.TrackAccessed );
            Assert.IsFalse(sd.TrackGotten);
            Assert.IsNull(sd.Accessed);
            Assert.IsNull(sd.Gotten);
        }

        [TestMethod]
        public void AddingKeysDoesNotAffectTrackingStatusOrCollectionsWhenInitiallyDisabled()
        {
            var sd = new SynchronizedDictionary<string, string>();

            sd.Add("key1", "value1");
            sd.Add("key2", "value2");

            // First ensure that items are available
            Assert.IsTrue(sd.ContainsKey("key1"));
            Assert.IsTrue(sd.ContainsKey("key2"));

            // Now ensure that tracking is disabled by default
            Assert.IsFalse(sd.TrackAccessed);
            Assert.IsFalse(sd.TrackGotten);

            // Now ensure that tracking collections are null
            Assert.IsNull(sd.Accessed);
            Assert.IsNull(sd.Gotten);
        }

        [TestMethod]
        public void AddingKeysDoesNotAffectTrackingStatusOrCollectionsWhenInitiallyEnabled()
        {
            var sd = new SynchronizedDictionary<string, string>();
            sd.TrackAccessed = true;
            sd.TrackGotten = true;

            sd.Add("key1", "value1");
            sd.Add("key2", "value2");

            // First ensure that items are available
            Assert.IsTrue(sd.ContainsKey("key1"));
            Assert.IsTrue(sd.ContainsKey("key2"));

            // Now ensure that tracking is enabled
            Assert.IsTrue(sd.TrackAccessed);
            Assert.IsTrue(sd.TrackGotten);

            // Now ensure that tracking collections are NOT null
            Assert.IsNotNull(sd.Accessed);
            Assert.IsNotNull(sd.Gotten);
        }

        [TestMethod]
        public void AccessedTrackingBeginsWhenItIsEnabled()
        {
            var sd = new SynchronizedDictionary<string, string>();

            sd.Add("key1", "value1");
            sd.Add("key2", "value2");

            // Turn on access tracking
            sd.TrackAccessed = true;
            Assert.IsTrue(sd.TrackAccessed);
            Assert.IsNotNull(sd.Accessed);

            sd.ContainsKey("key1");
            Assert.AreEqual(sd.Accessed.Count, 1);
            sd.ContainsKey("key2");
            Assert.AreEqual(sd.Accessed.Count, 2);

            // A second 'access' does not affect count
            sd.ContainsKey("key1");
            Assert.AreEqual(sd.Accessed.Count, 2);
        }

        [TestMethod]
        public void GottenTrackingBeginsWhenItIsEnabled()
        {
            var sd = new SynchronizedDictionary<string, string>();

            sd.Add("key1", "value1");
            sd.Add("key2", "value2");

            // Turn on gotten tracking
            sd.TrackGotten = true;
            Assert.IsTrue(sd.TrackGotten);
            Assert.IsNotNull(sd.Gotten);

            var v1 = sd["key1"];
            Assert.AreEqual(v1, "value1");
            Assert.AreEqual(sd.Gotten.Count, 1);

            var v2 = sd["key2"];
            Assert.AreEqual(v2, "value2");
            Assert.AreEqual(sd.Gotten.Count, 2);

            // A second 'get' does not affect count
            v1 = sd["key1"];
            Assert.AreEqual(sd.Gotten.Count, 2);
        }

        [TestMethod]
        public void TrackingBeginsWhenTrackAllConstructorIsCalled()
        {
            // Construct with trackAll enabled
            var sd = new SynchronizedDictionary<string, string>(true);

            sd.Add("key1", "value1");
            sd.Add("key2", "value2");

            // Check that Access Tracking is enabled
            Assert.IsTrue(sd.TrackAccessed);
            Assert.IsNotNull(sd.Accessed);

            // Check that Gotten Tracking is enabled
            Assert.IsTrue(sd.TrackGotten);
            Assert.IsNotNull(sd.Gotten);
        }

        [TestMethod]
        public void OnlyAccessedTrackingBeginsWhenTrackAccessedConstructorIsCalled()
        {
            // Construct with trackAll enabled
            var sd = new SynchronizedDictionary<string, string>(true, false);

            sd.Add("key1", "value1");
            sd.Add("key2", "value2");

            // Check that Access Tracking is enabled
            Assert.IsTrue(sd.TrackAccessed);
            Assert.IsNotNull(sd.Accessed);

            // Check that Gotten Tracking is disabled
            Assert.IsFalse(sd.TrackGotten);
            Assert.IsNull(sd.Gotten);
        }

        [TestMethod]
        public void OnlyGottenTrackingBeginsWhenTrackGottenConstructorIsCalled()
        {
            // Construct with trackAll enabled
            var sd = new SynchronizedDictionary<string, string>(false, true);

            sd.Add("key1", "value1");
            sd.Add("key2", "value2");

            // Check that Access Tracking is disabled
            Assert.IsFalse(sd.TrackAccessed);
            Assert.IsNull(sd.Accessed);

            // Check that Gotten Tracking is enabled
            Assert.IsTrue(sd.TrackGotten);
            Assert.IsNotNull(sd.Gotten);
        }

        [TestMethod]
        public void CopyConstructorProperlyCopiesInstanceIncludingTracking()
        {
            var other = new SynchronizedDictionary<string, string>(true, true);
            other.Add("key1", "otherValue1");
            other.Add("key2", "otherValue2");

            other.ContainsKey("key1");        // Accessed
            var otherValue1 = other["key1"];  // Gotten
            other.ContainsKey("key2");        // Accessed
            var otherValue2 = other["key2"];  // Gotten

            var sd = new SynchronizedDictionary<string, string>(other);

            Assert.IsTrue(sd.TrackAccessed);
            Assert.IsTrue(sd.TrackGotten);

            // As of now we have not accessed or gotten anything from our new clone
            Assert.AreEqual(sd.Accessed.Count, 2); // retains Accessed count from original
            Assert.AreEqual(sd.Gotten.Count, 2);   // retains Gotten count from original
        }

        [TestMethod]
        public void DictionaryCopyConstructorProperlyCopiesEntries()
        {
            var other = new Dictionary<string, string>();
            other.Add("key1", "otherValue1");
            other.Add("key2", "otherValue2");

            var sd = new SynchronizedDictionary<string, string>(other);

            Assert.IsTrue(sd.ContainsKey("key1"));
            Assert.IsTrue(sd.ContainsKey("key2"));

            Assert.AreEqual(sd["key1"], "otherValue1");
            Assert.AreEqual(sd["key2"], "otherValue2");
            Assert.IsFalse(sd.TrackAccessed);
            Assert.IsNull(sd.Accessed);
            Assert.IsFalse(sd.TrackGotten);
            Assert.IsNull(sd.Gotten);
        }

        [TestMethod]
        public void DictionaryCopyConstructorProperlyCopiesEntriesWithTrackingExplicitlyDisabled()
        {
            var other = new Dictionary<string, string>();
            other.Add("key1", "otherValue1");
            other.Add("key2", "otherValue2");

            var sd = new SynchronizedDictionary<string, string>(other, false);

            Assert.IsTrue(sd.ContainsKey("key1"));
            Assert.IsTrue(sd.ContainsKey("key2"));

            Assert.AreEqual(sd["key1"], "otherValue1");
            Assert.AreEqual(sd["key2"], "otherValue2");
            Assert.IsFalse(sd.TrackAccessed);
            Assert.IsNull(sd.Accessed);
            Assert.IsFalse(sd.TrackGotten);
            Assert.IsNull(sd.Gotten);
        }

        [TestMethod]
        public void DictionaryCopyConstructorProperlyCopiesEntriesAndSetsTracking()
        {
            var other = new Dictionary<string, string>();
            other.Add("key1", "otherValue1");
            other.Add("key2", "otherValue2");

            // TrackAll = true
            var sd = new SynchronizedDictionary<string, string>(other, true);

            Assert.IsTrue(sd.ContainsKey("key1"));
            Assert.IsTrue(sd.ContainsKey("key2"));

            Assert.AreEqual(sd["key1"], "otherValue1");
            Assert.AreEqual(sd["key2"], "otherValue2");

            // Accessed Tracking is enabled
            Assert.IsTrue(sd.TrackAccessed);
            Assert.IsNotNull(sd.Accessed);
            Assert.AreEqual(sd.Accessed.Count, 2);

            // Gotten Tracking is enabled
            Assert.IsTrue(sd.TrackGotten);
            Assert.IsNotNull(sd.Gotten);
            Assert.AreEqual(sd.Gotten.Count, 2);
        }

        [TestMethod]
        public void DictionaryCopyConstructorProperlyCopiesEntriesAndSetsAccessedTracking()
        {
            var other = new Dictionary<string, string>();
            other.Add("key1", "otherValue1");
            other.Add("key2", "otherValue2");

            // trackAccessed = true, trackGotten = false
            var sd = new SynchronizedDictionary<string, string>(other, true, false);

            Assert.IsTrue(sd.ContainsKey("key1"));
            Assert.IsTrue(sd.ContainsKey("key2"));

            Assert.AreEqual(sd["key1"], "otherValue1");
            Assert.AreEqual(sd["key2"], "otherValue2");

            // Accessed Tracking is enabled
            Assert.IsTrue(sd.TrackAccessed);
            Assert.IsNotNull(sd.Accessed);
            Assert.AreEqual(sd.Accessed.Count, 2);

            // Gotten Tracking is disabled
            Assert.IsFalse(sd.TrackGotten);
            Assert.IsNull(sd.Gotten);
        }

        [TestMethod]
        public void DictionaryCopyConstructorProperlyCopiesEntriesAndSetsGottenTracking()
        {
            var other = new Dictionary<string, string>();
            other.Add("key1", "otherValue1");
            other.Add("key2", "otherValue2");

            // trackAccessed = false, trackGotten = true
            var sd = new SynchronizedDictionary<string, string>(other, false, true); 

            Assert.IsTrue(sd.ContainsKey("key1"));
            Assert.IsTrue(sd.ContainsKey("key2"));

            Assert.AreEqual(sd["key1"], "otherValue1");
            Assert.AreEqual(sd["key2"], "otherValue2");

            // Accessed Tracking is disabled
            Assert.IsFalse(sd.TrackAccessed);
            Assert.IsNull(sd.Accessed);

            // Accessed Tracking is enabled
            Assert.IsTrue(sd.TrackGotten);
            Assert.IsNotNull(sd.Gotten);
            Assert.AreEqual(sd.Gotten.Count, 2);
        }

        #endregion // Tracking

    }
}