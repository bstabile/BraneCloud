/*
 * BraneCloud
 * Copyright 2011 Bennett R. Stabile (BraneCloud.Evolution.net|com)
 * Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
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
    /// The purpose of DictionaryTree is a little hard to explain.
    /// Hopefully, for now, these initial units tests will help. 
    /// TODO: Explain! ("Miles to go before I sleep...")
    /// </summary>
    [TestClass]
    public class DictionaryTreeTests
    {
        #region Basic Dictionary Operations

        [TestMethod]
        [Description("The expected value is obtained by indexing with the key.")]
        public void SingleEntryAddAndGetByIndexer()
        {
            var pd = new DictionaryTree<string, string>();
            pd.Add("prop1", "value1");
            Assert.AreEqual(pd["prop1"], "value1");
        }

        [TestMethod]
        [Description("ContainsKey is true and the value is obtained by indexing with the key.")]
        public void ContainsKeyIsTrueAndCanIndexValueByKey()
        {
            var pd = new DictionaryTree<string, string>();
            pd.Add("prop1", "value1");
            Assert.IsTrue(pd.ContainsKey("prop1"));
            Assert.AreEqual(pd["prop1"], "value1");
        }

        [TestMethod]
        [Description("The Keys and Values collections contain the key and value of a new entry.")]
        public void KeysAndValuesCollectionsContainKeyAndValueOfNewEntry()
        {
            var pd = new DictionaryTree<string, string>();
            pd.Add("prop1", "value1");
            Assert.IsTrue(pd.ContainsKey("prop1"));
            Assert.IsTrue(pd.Keys.Contains("prop1"));
            Assert.IsTrue(pd.Values.Contains("value1"));
        }

        [TestMethod]
        [Description("TryGetValue returns true, and the out parameter receives the correct value.")]
        public void TryGetValueReturnsTrueAndOutParameterReceivesValue()
        {
            var pd = new DictionaryTree<string, string>();
            pd.Add("prop1", "value1");
            string v;
            Assert.IsTrue(pd.TryGetValue("prop1", out v));
            Assert.AreEqual(v, "value1");
        }

        #endregion // Basic Dictionary Operations
        #region Defaults Operations

        [TestMethod]
        [Description("The correct LocalDefaults value is returned when indexed by key.")]
        public void LocalDefaultsIndexerReturnsCorrectValue()
        {
            var pd = new DictionaryTree<string, string>();
            pd.LocalDefaults.Add("prop1", "defaultValue1");
            Assert.AreEqual(pd["prop1"], "defaultValue1");
        }

        [TestMethod]
        [Description("TryGetDefault returns true and the out parameter receives the correct value.")]
        public void TryGetDefaultReturnsTrueAndOutParameterRecievesValue()
        {
            var pd = new DictionaryTree<string, string>();
            pd.LocalDefaults.Add("prop1", "defaultValue1");
            string v;
            Assert.IsTrue(pd.TryGetDefault("prop1", out v));
            Assert.AreEqual(v, "defaultValue1");
        }

        [TestMethod]
        [Description("The default entry is overridden by a primary entry with the same key.")]
        public void DefaultIsShadowedByPrimaryEntry()
        {
            var pd = new DictionaryTree<string, string>();
            pd.LocalDefaults.Add("prop1", "defaultValue1");
            pd.Add("prop1", "value1");
            string v;
            Assert.IsTrue(pd.TryGetValue("prop1", out v));
            Assert.AreEqual(v, "value1");
        }

        [TestMethod]
        [Description("A default comes back into view when the primary entry for the same key is removed.")]
        public void DefaultShadowedThenExposedByRemoveKey()
        {
            var pd = new DictionaryTree<string, string>();
            pd.LocalDefaults.Add("prop1", "defaultValue1");
            pd.Add("prop1", "value1");
            Assert.AreEqual(pd["prop1"], "value1");
            pd.RemoveLocal("prop1");
            Assert.AreEqual(pd["prop1"], "defaultValue1");
        }

        #endregion // Defaults Operations
        #region Hierarchical Operations (Parent/Child)

        [TestMethod]
        [Description("A parent value is found with ContainsKey when the child does not contain the same entry locally.")]
        public void ParentValueResovedInChildWithContainsKey()
        {
            var parent = new DictionaryTree<string, string>();
            var child = new DictionaryTree<string, string>();

            parent.Add("prop1", "value1");
            child.Parents.Add(parent);
            Assert.IsTrue(child.ContainsKey("prop1"));
            Assert.AreEqual(child["prop1"], "value1");
        }

        [TestMethod]
        [Description("A parent entry becomes shadowed when the child has an entry with the same key.")]
        public void ParentOvershadowedByChild()
        {
            var parent = new DictionaryTree<string, string>();
            var child = new DictionaryTree<string, string>();

            parent.Add("prop1", "value1");
            child.Parents.Add(parent);
            child.Add("prop1", "value2");
            Assert.IsTrue(child.ContainsKey("prop1"));
            Assert.AreEqual(child["prop1"], "value2");
            Assert.AreEqual(parent["prop1"], "value1");
        }

        [TestMethod]
        [Description("A shadowed parent entry becomes visible when the child entry having the same key is removed.")]
        public void ParentEntryBecomesVisibleWhenChildEntryRemoved()
        {
            var parent = new DictionaryTree<string, string>();
            var child = new DictionaryTree<string, string>();

            parent.Add("prop1", "value1");
            child.Parents.Add(parent);
            child.Add("prop1", "value2");
            Assert.AreEqual(child["prop1"], "value2");
            child.RemoveLocal("prop1");
            Assert.AreEqual(child["prop1"], "value1");
        }

        [TestMethod]
        [Description("The Remove method affects both child and parent.")]
        public void RemoveAffectsBothChildAndParentPrimaryEntries()
        {
            var parent = new DictionaryTree<string, string>();
            var child = new DictionaryTree<string, string>();

            parent.Add("prop1", "value1");
            child.Parents.Add(parent);
            child.Add("prop1", "value2");
            child.Remove("prop1");
            Assert.IsFalse(child.ContainsKey("prop1"));
        }

        [TestMethod]
        [Description("The Remove method affects both primary entries and defaults, in both child and parent.")]
        public void RemoveAffectsBothPrimaryEntriesAndDefaultsInBothChildAndParent()
        {
            var parent = new DictionaryTree<string, string>();
            var child = new DictionaryTree<string, string>();
            child.LocalDefaults.Add("prop1", "defaultValue");
            parent.Add("prop1", "value1");
            child.Parents.Add(parent);
            child.Add("prop1", "value2");
            child.Remove("prop1");
            Assert.IsFalse(child.ContainsKey("prop1"));
        }

        [TestMethod]
        [Description("Demonstrates unshadowing behavior for flattening and hierarchical indexing over two generations of Primary values and then Defaults.")]
        public void UnshadowingPrimaryEntriesAndThenDefualtsOverTwoGenerations()
        {
            // Parent one
            var p1 = new DictionaryTree<string, string>();
            var p1g1 = new DictionaryTree<string, string>();
            var p1g2 = new DictionaryTree<string, string>();

            p1.Parents.Add(p1g1);   // Add grandparent one to parent one
            p1g1.Parents.Add(p1g2); // Add grandparent two to grandparent one

            p1.Add("prop1", "p1"); // identifying values
            p1g1.Add("prop1", "p1g1");
            p1g2.Add("prop1", "p1g2");

            p1.AddDefault("prop1", "def.p1"); // identifying values
            p1g1.AddDefault("prop1", "def.p1g1");
            p1g2.AddDefault("prop1", "def.p1g2");

            // Parent two
            var p2 = new DictionaryTree<string, string>();
            var p2g1 = new DictionaryTree<string, string>();
            var p2g2 = new DictionaryTree<string, string>();

            p2.Parents.Add(p2g1);   // Add grandparent one to parent two
            p2g1.Parents.Add(p2g2); // Add grandparent two to grandparent one

            p2.Add("prop1", "p2"); // identifying values
            p2g1.Add("prop1", "p2g1");
            p2g2.Add("prop1", "p2g2");

            p2.AddDefault("prop1", "def.p2"); // identifying values
            p2g1.AddDefault("prop1", "def.p2g1");
            p2g2.AddDefault("prop1", "def.p2g2");

            var child = new DictionaryTree<string, string>();
            child.Parents.Add(p1);
            child.Parents.Add(p2);

            child.Add("prop1", "child");
            child.AddDefault("prop1", "def child");

            Assert.AreEqual(child["prop1"], "child");
            Assert.AreEqual(child.Flatten()["prop1"], "child");
            child.RemoveLocal("prop1"); // now we expect p2 because it was the most recent parent added
            Assert.AreEqual(child["prop1"], "p2");
            Assert.AreEqual(child.Flatten()["prop1"], "p2");
            p2.RemoveLocal("prop1"); // now we expect p2g1 because p2 will search its own parents before we switch to p1
            Assert.AreEqual(child["prop1"], "p2g1");
            Assert.AreEqual(child.Flatten()["prop1"], "p2g1");
            p2g1.RemoveLocal("prop1"); // with p2g1 gone, we now look at it's parent: p2g2
            Assert.AreEqual(child["prop1"], "p2g2");
            Assert.AreEqual(child.Flatten()["prop1"], "p2g2");
            p2g2.RemoveLocal("prop1"); // with p2g2 gone, we now look back at the p1 lineage
            Assert.AreEqual(child["prop1"], "p1");
            Assert.AreEqual(child.Flatten()["prop1"], "p1");
            p1.RemoveLocal("prop1"); // with p2g2 gone, we now look down at the most recent parent for p1: p1g1
            Assert.AreEqual(child["prop1"], "p1g1");
            Assert.AreEqual(child.Flatten()["prop1"], "p1g1");
            p1g1.RemoveLocal("prop1"); // with p2g2 gone, we now look down at the second gen for p1: p1g2
            Assert.AreEqual(child["prop1"], "p1g2");
            Assert.AreEqual(child.Flatten()["prop1"], "p1g2");
            p1g2.RemoveLocal("prop1"); // Now all primary values should be gone.

            child.RemoveLocalDefault("prop1"); // now we expect p2 because it was the most recent parent added
            Assert.AreEqual(child["prop1"], "def.p2");
            Assert.AreEqual(child.AllDefaults["prop1"], "def.p2");
            Assert.AreEqual(child.Flatten()["prop1"], "def.p2");
            p2.RemoveLocalDefault("prop1"); // now we expect p2g1 because p2 will search its own parents before we switch to p1
            Assert.AreEqual(child["prop1"], "def.p2g1");
            Assert.AreEqual(child.AllDefaults["prop1"], "def.p2g1");
            Assert.AreEqual(child.Flatten()["prop1"], "def.p2g1");
            p2g1.RemoveLocalDefault("prop1"); // with p2g1 gone, we now look at it's parent: p2g2
            Assert.AreEqual(child["prop1"], "def.p2g2");
            Assert.AreEqual(child.AllDefaults["prop1"], "def.p2g2");
            Assert.AreEqual(child.Flatten()["prop1"], "def.p2g2");
            p2g2.RemoveLocalDefault("prop1"); // with p2g2 gone, we now look back at the p1 lineage
            Assert.AreEqual(child["prop1"], "def.p1");
            Assert.AreEqual(child.AllDefaults["prop1"], "def.p1");
            Assert.AreEqual(child.Flatten()["prop1"], "def.p1");
            p1.RemoveLocalDefault("prop1"); // with p2g2 gone, we now look down at the most recent parent for p1: p1g1
            Assert.AreEqual(child["prop1"], "def.p1g1");
            Assert.AreEqual(child.AllDefaults["prop1"], "def.p1g1");
            Assert.AreEqual(child.Flatten()["prop1"], "def.p1g1");
            p1g1.RemoveLocalDefault("prop1"); // with p2g2 gone, we now look down at the second gen for p1: p1g2
            Assert.AreEqual(child["prop1"], "def.p1g2");
            Assert.AreEqual(child.AllDefaults["prop1"], "def.p1g2");
            Assert.AreEqual(child.Flatten()["prop1"], "def.p1g2");
        }

        #endregion // Hierarchical Operations (Parent/Child)
        #region Flattening

        [TestMethod]
        [Description("When a child is flattened, its entries override those that were shadowed by parents.")]
        public void FlatteningFavorsChildOverParent()
        {
            var parent = new DictionaryTree<string, string>();
            var child = new DictionaryTree<string, string>();
            parent.Add("prop1", "value1");
            child.Parents.Add(parent);
            child.Add("prop1", "value2");
            Assert.IsTrue(child.Flatten().ContainsKey("prop1"));
            Assert.AreEqual(child.Flatten()["prop1"], "value2");
        }

        [TestMethod]
        [Description("Flattening favors a parent primary entry over a child default having the same key.")]
        public void FlatteningFavorsParentPrimaryEntryOverChildDefault()
        {
            var parent = new DictionaryTree<string, string>();
            var child = new DictionaryTree<string, string>();
            child.LocalDefaults.Add("prop1", "defaultValue");
            parent.Add("prop1", "value1");
            child.Parents.Add(parent);
            var flat = child.Flatten();
            Assert.AreEqual(child.Flatten()["prop1"], "value1");
        }

        [TestMethod]
        [Description("Flattening favors a child's primary entries over a parent's defaults.")]
        public void FlatteningFavorsChildPrimaryEntryOverParentDefault()
        {
            var parent = new DictionaryTree<string, string>();
            var child = new DictionaryTree<string, string>();
            parent.LocalDefaults.Add("prop1", "defaultValue");
            child.Add("prop1", "value2");
            child.Parents.Add(parent);
            Assert.AreEqual(child.Flatten()["prop1"], "value2");
        }

        [TestMethod]
        [Description("Flattening favors a child default over a parent's default with the same key.")]
        public void FlatteningFavorsChildDefaultOverParentDefault()
        {
            var parent = new DictionaryTree<string, string>();
            var child = new DictionaryTree<string, string>();
            parent.LocalDefaults.Add("prop1", "defaultValue1");
            child.LocalDefaults.Add("prop1", "defaultValue2");
            child.Parents.Add(parent);
            Assert.AreEqual(child.Flatten(null)["prop1"], "defaultValue2");
        }

        [TestMethod]
        [Description("Flattening Defaults reveals deeper shadowed values as overriding values are removed.")]
        public void FlattenedDefaultsOverTwoParents()
        {
            var p1 = new DictionaryTree<string, string>();
            var p2 = new DictionaryTree<string, string>();
            var child = new DictionaryTree<string, string>();
            p1.LocalDefaults.Add("prop1", "def1");
            p2.LocalDefaults.Add("prop1", "def2");
            child.LocalDefaults.Add("prop1", "defChild");
            child.Parents.Add(p1);
            child.Parents.Add(p2);
            var v = "";
            child.TryGetDefault("prop1", out v);
            Assert.AreEqual(v, "defChild");
            Assert.AreEqual(child.AllDefaults["prop1"], "defChild");
            Assert.AreEqual(child.Flatten()["prop1"], "defChild");
            child.RemoveLocalDefault("prop1");
            Assert.AreEqual(child.AllDefaults["prop1"], "def2");
            Assert.AreEqual(child.Flatten()["prop1"], "def2");
            p2.RemoveLocalDefault("prop1");
            Assert.AreEqual(child.AllDefaults["prop1"], "def1");
            Assert.AreEqual(child.Flatten()["prop1"], "def1");
            p1.RemoveLocalDefault("prop1");
            Assert.IsFalse(child.Flatten().ContainsKey("prop1"));
        }

        [TestMethod]
        [Description("Demonstrates flattening and hierarchical indexing over two generations of primary entries.")]
        public void FlattenedEntriesOverTwoGenerations()
        {
            // Parent one
            var p1 = new DictionaryTree<string, string>();
            var p1g1 = new DictionaryTree<string, string>();
            var p1g2 = new DictionaryTree<string, string>();

            p1.Parents.Add(p1g1);   // Add grandparent one to parent one
            p1g1.Parents.Add(p1g2); // Add grandparent two to grandparent one

            p1.Add("prop1", "p1"); // identifying values
            p1g1.Add("prop1", "p1g1");
            p1g2.Add("prop1", "p1g2");

            // Parent two
            var p2 = new DictionaryTree<string, string>();
            var p2g1 = new DictionaryTree<string, string>();
            var p2g2 = new DictionaryTree<string, string>();

            p2.Parents.Add(p2g1);   // Add grandparent one to parent two
            p2g1.Parents.Add(p2g2); // Add grandparent two to grandparent one

            p2.Add("prop1", "p2"); // identifying values
            p2g1.Add("prop1", "p2g1");
            p2g2.Add("prop1", "p2g2");


            var child = new DictionaryTree<string, string>();
            child.Parents.Add(p1);
            child.Parents.Add(p2);

            child.Add("prop1", "child");

            Assert.AreEqual(child["prop1"], "child");
            Assert.AreEqual(child.Flatten()["prop1"], "child");
            child.RemoveLocal("prop1"); // now we expect p2 because it was the most recent parent added
            Assert.AreEqual(child["prop1"], "p2");
            Assert.AreEqual(child.Flatten()["prop1"], "p2");
            p2.RemoveLocal("prop1"); // now we expect p2g1 because p2 will search its own parents before we switch to p1
            Assert.AreEqual(child["prop1"], "p2g1");
            Assert.AreEqual(child.Flatten()["prop1"], "p2g1");
            p2g1.RemoveLocal("prop1"); // with p2g1 gone, we now look at it's parent: p2g2
            Assert.AreEqual(child["prop1"], "p2g2");
            Assert.AreEqual(child.Flatten()["prop1"], "p2g2");
            p2g2.RemoveLocal("prop1"); // with p2g2 gone, we now look back at the p1 lineage
            Assert.AreEqual(child["prop1"], "p1");
            Assert.AreEqual(child.Flatten()["prop1"], "p1");
            p1.RemoveLocal("prop1"); // with p2g2 gone, we now look down at the most recent parent for p1: p1g1
            Assert.AreEqual(child["prop1"], "p1g1");
            Assert.AreEqual(child.Flatten()["prop1"], "p1g1");
            p1g1.RemoveLocal("prop1"); // with p2g2 gone, we now look down at the second gen for p1: p1g2
            Assert.AreEqual(child["prop1"], "p1g2");
            Assert.AreEqual(child.Flatten()["prop1"], "p1g2");
        }

        [TestMethod]
        [Description("Demonstrates flattening and hierarchical indexing over two generations of Defaults.")]
        public void FlattenedDefaultsOverTwoGenerations()
        {
            // Parent one
            var p1 = new DictionaryTree<string, string>();
            var p1g1 = new DictionaryTree<string, string>();
            var p1g2 = new DictionaryTree<string, string>();

            p1.Parents.Add(p1g1);   // Add grandparent one to parent one
            p1g1.Parents.Add(p1g2); // Add grandparent two to grandparent one

            p1.AddDefault("prop1", "p1"); // identifying values
            p1g1.AddDefault("prop1", "p1g1");
            p1g2.AddDefault("prop1", "p1g2");

            // Parent two
            var p2 = new DictionaryTree<string, string>();
            var p2g1 = new DictionaryTree<string, string>();
            var p2g2 = new DictionaryTree<string, string>();

            p2.Parents.Add(p2g1);   // Add grandparent one to parent two
            p2g1.Parents.Add(p2g2); // Add grandparent two to grandparent one

            p2.AddDefault("prop1", "p2"); // identifying values
            p2g1.AddDefault("prop1", "p2g1");
            p2g2.AddDefault("prop1", "p2g2");

            var child = new DictionaryTree<string, string>();
            child.Parents.Add(p1);
            child.Parents.Add(p2);

            child.AddDefault("prop1", "child");

            Assert.AreEqual(child.AllDefaults["prop1"], "child");
            Assert.AreEqual(child.Flatten()["prop1"], "child");
            child.RemoveLocalDefault("prop1"); // now we expect p2 because it was the most recent parent added
            Assert.AreEqual(child.AllDefaults["prop1"], "p2");
            Assert.AreEqual(child.Flatten()["prop1"], "p2");
            p2.RemoveLocalDefault("prop1"); // now we expect p2g1 because p2 will search its own parents before we switch to p1
            Assert.AreEqual(child.AllDefaults["prop1"], "p2g1");
            Assert.AreEqual(child.Flatten()["prop1"], "p2g1");
            p2g1.RemoveLocalDefault("prop1"); // with p2g1 gone, we now look at it's parent: p2g2
            Assert.AreEqual(child.AllDefaults["prop1"], "p2g2");
            Assert.AreEqual(child.Flatten()["prop1"], "p2g2");
            p2g2.RemoveLocalDefault("prop1"); // with p2g2 gone, we now look back at the p1 lineage
            Assert.AreEqual(child.AllDefaults["prop1"], "p1");
            Assert.AreEqual(child.Flatten()["prop1"], "p1");
            p1.RemoveLocalDefault("prop1"); // with p2g2 gone, we now look down at the most recent parent for p1: p1g1
            Assert.AreEqual(child.AllDefaults["prop1"], "p1g1");
            Assert.AreEqual(child.Flatten()["prop1"], "p1g1");
            p1g1.RemoveLocalDefault("prop1"); // with p2g2 gone, we now look down at the second gen for p1: p1g2
            Assert.AreEqual(child.AllDefaults["prop1"], "p1g2");
            Assert.AreEqual(child.Flatten()["prop1"], "p1g2");
        }

        #endregion // Flattening
        #region Tracking

        #region Enabling and Disabling Tracking

        [TestMethod]
        public void CheckThatTrackingIsDisabledByDefault()
        {
            var dt = new DictionaryTree<string, string>();
            dt.Add("prop1", "value1");
            Assert.IsFalse(dt.TrackAccessed);
            Assert.IsNull(dt.Accessed);
            Assert.IsFalse(dt.TrackGotten);
            Assert.IsNull(dt.Gotten);
            Assert.IsFalse(dt.TrackAll);
        }

        [TestMethod]
        public void TrackAllGivesEmptyResultsForAccessedAndGotten()
        {
            var dt = new DictionaryTree<string, string>();
            dt.TrackAll = true;
            Assert.IsTrue(dt.TrackAccessed);
            Assert.IsNotNull(dt.Accessed);
            Assert.IsTrue(dt.TrackGotten);
            Assert.IsNotNull(dt.Gotten);
            Assert.IsTrue(dt.TrackAll);
        }

        [TestMethod]
        public void CheckTrackingAccessedOnly()
        {
            var dt = new DictionaryTree<string, string>();
            dt.TrackAccessed = true;

            // Accessed tracking is now available
            Assert.IsTrue(dt.TrackAccessed);
            Assert.IsNotNull(dt.Accessed);

            // Gotten tracking is NOT available
            Assert.IsFalse(dt.TrackGotten);
            Assert.IsNull(dt.Gotten);

            // TrackAll is still false
            Assert.IsFalse(dt.TrackAll);
        }

        [TestMethod]
        public void CheckTrackingGottenOnly()
        {
            var dt = new DictionaryTree<string, string>();
            dt.TrackGotten = true;

            // Accessed tracking is NOT available
            Assert.IsFalse(dt.TrackAccessed);
            Assert.IsNull(dt.Accessed);

            // Gotten tracking is available
            Assert.IsTrue(dt.TrackGotten);
            Assert.IsNotNull(dt.Gotten);

            // TrackAll is still false
            Assert.IsFalse(dt.TrackAll);
        }

        [TestMethod]
        public void SetTrackingForAccessedAndGottenAlsoSetsTrackAll()
        {
            var dt = new DictionaryTree<string, string>();
            dt.TrackAccessed = true;
            dt.TrackGotten = true;
            
            // Accessed tracking is available
            Assert.IsTrue(dt.TrackAccessed);
            Assert.IsNotNull(dt.Accessed);

            // Gotten tracking is available
            Assert.IsTrue(dt.TrackGotten);
            Assert.IsNotNull(dt.Gotten);

            // TrackAll is also true
            Assert.IsTrue(dt.TrackAll);
        }

        #endregion // Enabling and Disabling Tracking
        #region Track Accessed

        [TestMethod]
        public void TrackAccessedGivesProperResultsLocallyWhenPrimaryIsAccessed()
        {
            var dt = new DictionaryTree<string, string>();
            dt.TrackAccessed = true;

            // Accessed tracking is available
            Assert.IsTrue(dt.TrackAccessed);
            Assert.IsNotNull(dt.Accessed);

            dt.Add("prop1", "value1"); // Adds an entry in Primary collection
            Assert.AreEqual(dt.Accessed.Count, 0); // Not yet accessed
            Assert.IsTrue(dt.ContainsKey("prop1"));
            Assert.AreEqual(dt.Accessed.Count, 1);
        }

        [TestMethod]
        public void TrackAccessedGivesProperResultsLocallyWhenDefaultIsAccessed()
        {
            var dt = new DictionaryTree<string, string>();
            dt.TrackAccessed = true;

            // Accessed tracking is available
            Assert.IsTrue(dt.TrackAccessed);
            Assert.IsNotNull(dt.Accessed);

            dt.Add("prop1", "value1"); // Adds an entry in Primary collection
            dt.AddDefault("prop1", "default1");

            Assert.AreEqual(dt.Accessed.Count, 0); // Not yet accessed
            Assert.IsTrue(dt.ContainsDefaultKey("prop1")); // key is accessed in default
            Assert.AreEqual(dt.Accessed.Count, 1);
        }

        [TestMethod]
        public void TrackAccessedGivesProperResultsLocallyWhenBothPrimaryAndDefaultsAreAccessed()
        {
            var dt = new DictionaryTree<string, string>();
            dt.TrackAccessed = true;

            // Accessed tracking is available
            Assert.IsTrue(dt.TrackAccessed);
            Assert.IsNotNull(dt.Accessed);

            dt.Add("prop1", "value1"); // Adds an entry in Primary collection
            dt.AddDefault("prop1", "default1");

            Assert.AreEqual(dt.Accessed.Count, 0); // Not yet accessed
            Assert.IsTrue(dt.ContainsKey("prop1"));
            Assert.AreEqual(dt.Accessed.Count, 1); // count of 1
            Assert.IsTrue(dt.ContainsDefaultKey("prop1")); // key is accessed in default
            Assert.AreEqual(dt.Accessed.Count, 1); // still only count of 1
        }

        #endregion // Track Accessed
        #region Track Gotten

        [TestMethod]
        public void TrackGottenGivesProperResultsLocallyWhenPrimaryIsGotten()
        {
            var dt = new DictionaryTree<string, string>();
            dt.TrackGotten = true;

            // Gotten tracking is available
            Assert.IsTrue(dt.TrackGotten);
            Assert.IsNotNull(dt.Gotten);

            dt.Add("prop1", "value1"); // Adds an entry in Primary collection
            Assert.AreEqual(dt.Gotten.Count, 0); // Not yet gotten
            Assert.AreEqual(dt["prop1"], "value1");
            Assert.AreEqual(dt.Gotten.Count, 1);
        }

        [TestMethod]
        public void TrackGottenGivesProperResultsLocallyWhenDefaultIsGotten()
        {
            var dt = new DictionaryTree<string, string>();
            dt.TrackGotten = true;

            // Gotten tracking is available
            Assert.IsTrue(dt.TrackGotten);
            Assert.IsNotNull(dt.Gotten);

            dt.Add("prop1", "value1"); // Adds an entry in Primary collection
            dt.AddDefault("prop1", "default1");

            Assert.AreEqual(dt.Gotten.Count, 0); // Not yet gotten
            Assert.AreEqual(dt.LocalDefaults["prop1"], "default1"); // key is gotten in defaults
            Assert.AreEqual(dt.Gotten.Count, 1);
        }

        [TestMethod]
        public void TrackGottenGivesProperResultsLocallyWhenBothPrimaryAndDefaultsAreGotten()
        {
            var dt = new DictionaryTree<string, string>();
            dt.TrackGotten = true;

            // Gotten tracking is available
            Assert.IsTrue(dt.TrackGotten);
            Assert.IsNotNull(dt.Gotten);

            dt.Add("prop1", "value1"); // Adds an entry in Primary collection
            dt.AddDefault("prop1", "default1");

            Assert.AreEqual(dt.Gotten.Count, 0); // Not yet gotten
            Assert.AreEqual(dt["prop1"], "value1");
            Assert.AreEqual(dt.Gotten.Count, 1); // count of 1
            Assert.AreEqual(dt.LocalDefaults["prop1"], "default1");
            Assert.AreEqual(dt.Gotten.Count, 1); // still only count of 1
        }

        #endregion // Track Gotten
        #region Track All

        [TestMethod]
        public void TrackAllGivesProperResultsLocallyWhenPrimaryIsGotten()
        {
            var dt = new DictionaryTree<string, string>();
            dt.TrackAll = true;

            // Accessed and Gotten tracking is available
            Assert.IsTrue(dt.TrackAll);
            Assert.IsTrue(dt.TrackAccessed);
            Assert.IsTrue(dt.TrackGotten);
            Assert.IsNotNull(dt.Accessed);
            Assert.IsNotNull(dt.Gotten);

            dt.Add("prop1", "value1"); // Adds an entry in Primary collection

            Assert.AreEqual(dt.Accessed.Count, 0);
            Assert.AreEqual(dt.Gotten.Count, 0); // Not yet gotten
            Assert.AreEqual(dt["prop1"], "value1");
            Assert.AreEqual(dt.Accessed.Count, 1);
            Assert.AreEqual(dt.Gotten.Count, 1);
        }

        [TestMethod]
        public void TrackAllGivesProperResultsLocallyWhenDefaultIsGotten()
        {
            var dt = new DictionaryTree<string, string>();
            dt.TrackAll = true;

            // Accessed and Gotten tracking is available
            Assert.IsTrue(dt.TrackAll);
            Assert.IsTrue(dt.TrackAccessed);
            Assert.IsTrue(dt.TrackGotten);
            Assert.IsNotNull(dt.Accessed);
            Assert.IsNotNull(dt.Gotten);

            dt.Add("prop1", "value1"); // Adds an entry in Primary collection
            dt.AddDefault("prop1", "default1");

            Assert.AreEqual(dt.Accessed.Count, 0);
            Assert.AreEqual(dt.Gotten.Count, 0); // Not yet gotten
            Assert.AreEqual(dt.LocalDefaults["prop1"], "default1");
            Assert.AreEqual(dt.Accessed.Count, 1);
            Assert.AreEqual(dt.Gotten.Count, 1);
        }

        [TestMethod]
        public void TrackAllGivesProperResultsLocallyWhenBothPrimaryAndDefaultsAreGotten()
        {
            var dt = new DictionaryTree<string, string>();
            dt.TrackAll = true;

            // Accessed and Gotten tracking is available
            Assert.IsTrue(dt.TrackAll);
            Assert.IsTrue(dt.TrackAccessed);
            Assert.IsTrue(dt.TrackGotten);
            Assert.IsNotNull(dt.Accessed);
            Assert.IsNotNull(dt.Gotten);

            dt.Add("prop1", "value1"); // Adds an entry in Primary collection
            dt.AddDefault("prop1", "default1");

            Assert.AreEqual(dt.Gotten.Count, 0); // Not yet accessed
            Assert.AreEqual(dt["prop1"], "value1");
            Assert.AreEqual(dt.Accessed.Count, 1); // count of 1
            Assert.AreEqual(dt.Gotten.Count, 1); // count of 1
            Assert.AreEqual(dt.LocalDefaults["prop1"], "default1");
            Assert.AreEqual(dt.Accessed.Count, 1); // still only count of 1
            Assert.AreEqual(dt.Gotten.Count, 1); // still only count of 1
        }

        #endregion // Track All
        #region Track Hierarchical

        [TestMethod]
        public void SetTrackAccessedSetsParentsAlso()
        {
            var parent1 = new DictionaryTree<string, string>();
            var parent2 = new DictionaryTree<string, string>();
            var child = new DictionaryTree<string, string>();
            child.Parents.Add(parent1);
            child.Parents.Add(parent2);

            // Check that Accessed tracking is disabled by default
            Assert.IsFalse(child.TrackAccessed);
            Assert.IsNull(child.Accessed);
            // Parent1
            Assert.IsFalse(child.Parents[0].TrackAccessed);
            Assert.IsNull(child.Parents[0].Accessed);
            // Parent2
            Assert.IsFalse(child.Parents[1].TrackAccessed);
            Assert.IsNull(child.Parents[1].Accessed);

            child.TrackAccessed = true;
            Assert.IsTrue(child.TrackAccessed);
            Assert.IsNotNull(child.Accessed);
            // Parent1
            Assert.IsTrue(child.Parents[0].TrackAccessed);
            Assert.IsNotNull(child.Parents[0].Accessed);
            // Parent2
            Assert.IsTrue(child.Parents[1].TrackAccessed);
            Assert.IsNotNull(child.Parents[1].Accessed);
        }

        [TestMethod]
        public void SetTrackGottenSetsParentsAlso()
        {
            var parent1 = new DictionaryTree<string, string>();
            var parent2 = new DictionaryTree<string, string>();
            var child = new DictionaryTree<string, string>();
            child.Parents.Add(parent1);
            child.Parents.Add(parent2);

            // Check that Accessed tracking is disabled by default
            Assert.IsFalse(child.TrackGotten);
            Assert.IsNull(child.Gotten);
            Assert.IsFalse(child.Parents[0].TrackGotten);
            Assert.IsNull(child.Parents[0].Gotten);

            child.TrackGotten = true;
            Assert.IsTrue(child.TrackGotten);
            Assert.IsNotNull(child.Gotten);
            // Parent1
            Assert.IsTrue(child.Parents[0].TrackGotten);
            Assert.IsNotNull(child.Parents[0].Gotten);
            // Parent2
            Assert.IsTrue(child.Parents[1].TrackGotten);
            Assert.IsNotNull(child.Parents[1].Gotten);

            // Check that Accessed Tracking is still disabled
            Assert.IsFalse(child.TrackAccessed);
            Assert.IsNull(child.Accessed);
            // Parent1
            Assert.IsFalse(child.Parents[0].TrackAccessed);
            Assert.IsNull(child.Parents[0].Accessed);
            // Parent2
            Assert.IsFalse(child.Parents[1].TrackAccessed);
            Assert.IsNull(child.Parents[1].Accessed);
        }

        [TestMethod]
        public void SetTrackAllSetsParentsAlso()
        {
            var parent1 = new DictionaryTree<string, string>();
            var parent2 = new DictionaryTree<string, string>();
            var child = new DictionaryTree<string, string>();
            child.Parents.Add(parent1);
            child.Parents.Add(parent2);

            // Check that Accessed and Gotten tracking is disabled by default
            Assert.IsFalse(child.TrackAccessed);
            Assert.IsFalse(child.TrackGotten);
            Assert.IsNull(child.Accessed);
            Assert.IsNull(child.Gotten);
            // Parent1
            Assert.IsFalse(child.Parents[0].TrackAccessed);
            Assert.IsFalse(child.Parents[0].TrackGotten);
            Assert.IsNull(child.Parents[0].Accessed);
            Assert.IsNull(child.Parents[0].Gotten);
            // Parent2
            Assert.IsFalse(child.Parents[1].TrackAccessed);
            Assert.IsFalse(child.Parents[1].TrackGotten);
            Assert.IsNull(child.Parents[1].Accessed);
            Assert.IsNull(child.Parents[1].Gotten);

            child.TrackAll = true;

            // Check that all tracking is now available
            Assert.IsTrue(child.TrackAll);
            Assert.IsTrue(child.TrackAccessed);
            Assert.IsTrue(child.TrackGotten);
            Assert.IsNotNull(child.Accessed);
            Assert.IsNotNull(child.Gotten);
            // Parent1
            Assert.IsTrue(child.Parents[0].TrackAll);
            Assert.IsTrue(child.Parents[0].TrackAccessed);
            Assert.IsTrue(child.Parents[0].TrackGotten);
            Assert.IsNotNull(child.Parents[0].Accessed);
            Assert.IsNotNull(child.Parents[0].Gotten);
            // Parent2
            Assert.IsTrue(child.Parents[1].TrackAll);
            Assert.IsTrue(child.Parents[1].TrackAccessed);
            Assert.IsTrue(child.Parents[1].TrackGotten);
            Assert.IsNotNull(child.Parents[1].Accessed);
            Assert.IsNotNull(child.Parents[1].Gotten);
        }

        [TestMethod]
        public void ParentAccessedAppearsInChild()
        {
            var parent1 = new DictionaryTree<string, string>();
            parent1.Add("prop1", "parent1");
            var parent2 = new DictionaryTree<string, string>();
            parent2.Add("prop1", "parent2");

            var child = new DictionaryTree<string, string>();
            child.Add("prop1", "child");
            child.Parents.Add(parent1);
            child.Parents.Add(parent2);

            // Check that Accessed tracking is disabled by default
            Assert.IsFalse(child.TrackAccessed);
            Assert.IsNull(child.Accessed);
            // Parent1
            Assert.IsFalse(child.Parents[0].TrackAccessed);
            Assert.IsNull(child.Parents[0].Accessed);
            // Parent2
            Assert.IsFalse(child.Parents[1].TrackAccessed);
            Assert.IsNull(child.Parents[1].Accessed);

            child.TrackAccessed = true;
            Assert.IsTrue(child.Parents[0].ContainsKey("prop1"));
            Assert.AreEqual(child.Accessed.Count, 1);
            Assert.AreEqual(child.Parents[0].Accessed.Count, 1);
            Assert.AreEqual(child.Parents[1].Accessed.Count, 0);
        }

        [TestMethod]
        public void ParentGottenAppearsInChild()
        {
            var parent1 = new DictionaryTree<string, string>();
            parent1.Add("prop1", "parent1");
            var parent2 = new DictionaryTree<string, string>();
            parent2.Add("prop1", "parent2");

            var child = new DictionaryTree<string, string>();
            child.Add("prop1", "child");
            child.Parents.Add(parent1);
            child.Parents.Add(parent2);

            // Check that Accessed tracking is disabled by default
            Assert.IsFalse(child.TrackAccessed);
            Assert.IsNull(child.Accessed);
            // Parent1
            Assert.IsFalse(child.Parents[0].TrackAccessed);
            Assert.IsNull(child.Parents[0].Accessed);
            // Parent2
            Assert.IsFalse(child.Parents[1].TrackAccessed);
            Assert.IsNull(child.Parents[1].Accessed);

            child.TrackGotten = true;
            Assert.AreEqual(child.Parents[0]["prop1"], "parent1"); // Gotten in parent
            Assert.AreEqual(child.Gotten.Count, 1); // Shows up in child
            Assert.AreEqual(child.Parents[0].Gotten.Count, 1); // Only in parent where gotten
            Assert.AreEqual(child.Parents[1].Gotten.Count, 0); // Doesn't show up here
        }

        [TestMethod]
        public void TrackAllParentGottenAppearsInChildAccessedAndGotten()
        {
            var parent1 = new DictionaryTree<string, string>();
            parent1.Add("prop1", "parent1");
            var parent2 = new DictionaryTree<string, string>();
            parent2.Add("prop1", "parent2");

            var child = new DictionaryTree<string, string>();
            child.Add("prop1", "child");
            child.Parents.Add(parent1);
            child.Parents.Add(parent2);

            // Check that Accessed tracking is disabled by default
            Assert.IsFalse(child.TrackAccessed);
            Assert.IsNull(child.Accessed);
            // Parent1
            Assert.IsFalse(child.Parents[0].TrackAccessed);
            Assert.IsNull(child.Parents[0].Accessed);
            // Parent2
            Assert.IsFalse(child.Parents[1].TrackAccessed);
            Assert.IsNull(child.Parents[1].Accessed);

            child.TrackAll = true;
            Assert.AreEqual(child.Parents[0]["prop1"], "parent1"); // Gotten in parent
            Assert.AreEqual(child.Accessed.Count, 1); // Shows up in child
            Assert.AreEqual(child.Gotten.Count, 1); // Shows up in child
            Assert.AreEqual(child.Parents[0].Accessed.Count, 1); // Only in parent where gotten
            Assert.AreEqual(child.Parents[0].Gotten.Count, 1); // Only in parent where gotten
            Assert.AreEqual(child.Parents[1].Accessed.Count, 0); // Doesn't show up here
            Assert.AreEqual(child.Parents[1].Gotten.Count, 0); // Doesn't show up here
        }

        #endregion // Track Hierarchical
        #region Clearing Tracked Keys

        [TestMethod]
        public void ClearAccessedKey()
        {
            var dt = new DictionaryTree<string, string>();
            dt.Add("prop1", "value1");
            dt.TrackAccessed = true;
            Assert.IsTrue(dt.ContainsKey("prop1")); // accessed
            Assert.AreEqual(dt.Accessed.Count, 1);
            dt.ClearAccessedKey("prop1");
            Assert.AreEqual(dt.Accessed.Count, 0);
        }

        [TestMethod]
        public void ClearGottenKey()
        {
            var dt = new DictionaryTree<string, string>();
            dt.Add("prop1", "value1");
            dt.TrackGotten = true;
            Assert.AreEqual(dt["prop1"], "value1"); // accessed
            Assert.AreEqual(dt.Gotten.Count, 1);
            dt.ClearGottenKey("prop1");
            Assert.AreEqual(dt.Gotten.Count, 0);
        }

        [TestMethod]
        public void ClearTrackedKey()
        {
            var dt = new DictionaryTree<string, string>();
            dt.Add("prop1", "value1");
            dt.TrackAll = true;
            Assert.AreEqual(dt["prop1"], "value1"); // gotten and therefore also accessed
            Assert.AreEqual(dt.Accessed.Count, 1);
            Assert.AreEqual(dt.Gotten.Count, 1);
            dt.ClearTrackedKey("prop1");
            Assert.AreEqual(dt.Accessed.Count, 0);
            Assert.AreEqual(dt.Gotten.Count, 0);
        }

        [TestMethod]
        public void ClearAccessedKeyExtendsToParents()
        {
            var child = new DictionaryTree<string, string>();
            child.Add("prop1", "child");
            var parent1 = new DictionaryTree<string, string>();
            parent1.Add("prop1", "parent1");
            var parent2 = new DictionaryTree<string, string>();
            parent2.Add("prop1", "parent2");

            child.Parents.Add(parent1);
            child.Parents.Add(parent2);

            child.TrackAccessed = true; // Sets all constituents to track accessed keys

            Assert.IsTrue(child.ContainsKey("prop1")); // accessed in child
            Assert.IsTrue(child.Parents[0].ContainsKey("prop1")); // accessed in parent1
            Assert.IsTrue(child.Parents[1].ContainsKey("prop1")); // accessed in parent2

            Assert.AreEqual(child.Accessed.Count, 1);
            Assert.AreEqual(child.Parents[0].Accessed.Count, 1);
            Assert.AreEqual(child.Parents[1].Accessed.Count, 1);

            child.ClearAccessedKey("prop1"); // clear accessed for the key

            Assert.AreEqual(child.Accessed.Count, 0); // no longer in child
            Assert.AreEqual(child.Parents[0].Accessed.Count, 0); // no longer in parent1
            Assert.AreEqual(child.Parents[1].Accessed.Count, 0); // no longer in parent2
        }

        [TestMethod]
        public void ClearGottenKeyExtendsToParents()
        {
            var child = new DictionaryTree<string, string>();
            child.Add("prop1", "child");
            var parent1 = new DictionaryTree<string, string>();
            parent1.Add("prop1", "parent1");
            var parent2 = new DictionaryTree<string, string>();
            parent2.Add("prop1", "parent2");

            child.Parents.Add(parent1);
            child.Parents.Add(parent2);

            child.TrackGotten = true; // Sets all constituents to track gotten values

            Assert.AreEqual(child["prop1"], "child"); // gotten in child
            Assert.AreEqual(child.Parents[0]["prop1"], "parent1"); // gotten in parent1
            Assert.AreEqual(child.Parents[1]["prop1"], "parent2"); // gotten in parent2

            Assert.AreEqual(child.Gotten.Count, 1);
            Assert.AreEqual(child.Parents[0].Gotten.Count, 1);
            Assert.AreEqual(child.Parents[1].Gotten.Count, 1);

            child.ClearGottenKey("prop1"); // clear gotten for the key

            Assert.AreEqual(child.Gotten.Count, 0); // no longer in child
            Assert.AreEqual(child.Parents[0].Gotten.Count, 0); // no longer in parent1
            Assert.AreEqual(child.Parents[1].Gotten.Count, 0); // no longer in parent2
        }

        [TestMethod]
        public void ClearTrackedKeyExtendsToParents()
        {
            var child = new DictionaryTree<string, string>();
            child.Add("prop1", "child");
            var parent1 = new DictionaryTree<string, string>();
            parent1.Add("prop1", "parent1");
            var parent2 = new DictionaryTree<string, string>();
            parent2.Add("prop1", "parent2");

            child.Parents.Add(parent1);
            child.Parents.Add(parent2);

            child.TrackAll = true; // Sets all constituents to track accessed and gotten

            Assert.AreEqual(child["prop1"], "child"); // gotten in child
            Assert.AreEqual(child.Parents[0]["prop1"], "parent1"); // gotten in parent1
            Assert.AreEqual(child.Parents[1]["prop1"], "parent2"); // gotten in parent2

            // Child
            Assert.AreEqual(child.Accessed.Count, 1); // shows up in child accessed
            Assert.AreEqual(child.Gotten.Count, 1); // show up in child gotten
            // Parent1
            Assert.AreEqual(child.Parents[0].Accessed.Count, 1); // shows up in parent1 accessed
            Assert.AreEqual(child.Parents[0].Gotten.Count, 1); // shows up in parent1 gotten
            // Parent2
            Assert.AreEqual(child.Parents[1].Accessed.Count, 1); // shows up in parent2 accessed
            Assert.AreEqual(child.Parents[1].Gotten.Count, 1); // shows up in parent2 gotten

            child.ClearTrackedKey("prop1"); // clear tracking for the key

            // Child
            Assert.AreEqual(child.Accessed.Count, 0); // no longer in child accessed
            Assert.AreEqual(child.Gotten.Count, 0); // no longer in child gotten
            // Parent1
            Assert.AreEqual(child.Parents[0].Accessed.Count, 0); // no longer in parent1 accessed
            Assert.AreEqual(child.Parents[0].Gotten.Count, 0); // no longer in parent1 gotten
            // Parent2
            Assert.AreEqual(child.Parents[1].Accessed.Count, 0); // no longer in parent2 accessed
            Assert.AreEqual(child.Parents[1].Gotten.Count, 0); // no longer in parent2 gotten
        }

        [TestMethod]
        public void ParentClearAccessedKey()
        {
            var child = new DictionaryTree<string, string>();
            child.Add("prop1", "child");
            var parent1 = new DictionaryTree<string, string>();
            parent1.Add("prop1", "parent1");
            var parent2 = new DictionaryTree<string, string>();
            parent2.Add("prop1", "parent2");

            child.Parents.Add(parent1);
            child.Parents.Add(parent2);

            child.TrackAccessed = true; // Sets all constituents to track accessed keys

            Assert.IsTrue(child.Parents[0].ContainsKey("prop1")); // accessed
            Assert.AreEqual(child.Parents[0].Accessed.Count, 1);
            Assert.AreEqual(child.Parents[1].Accessed.Count, 0);
            Assert.AreEqual(child.Accessed.Count, 1);
            child.Parents[0].ClearAccessedKey("prop1"); // clear accessed for the key
            Assert.AreEqual(child.Parents[0].Accessed.Count, 0);
            Assert.AreEqual(child.Accessed.Count, 0);
        }

        [TestMethod]
        public void ParentClearGottenKey()
        {
            var child = new DictionaryTree<string, string>();
            child.Add("prop1", "child");
            var parent1 = new DictionaryTree<string, string>();
            parent1.Add("prop1", "parent1");
            var parent2 = new DictionaryTree<string, string>();
            parent2.Add("prop1", "parent2");

            child.Parents.Add(parent1);
            child.Parents.Add(parent2);

            child.TrackGotten = true; // Sets all constituents to track gotten values

            Assert.AreEqual(child.Parents[0]["prop1"], "parent1"); // gotten
            Assert.AreEqual(child.Parents[0].Gotten.Count, 1);
            Assert.AreEqual(child.Parents[1].Gotten.Count, 0);
            Assert.AreEqual(child.Gotten.Count, 1);
            child.ClearGottenKey("prop1"); // clear gotten for the key
            Assert.AreEqual(child.Parents[0].Gotten.Count, 0);
            Assert.AreEqual(child.Gotten.Count, 0);
        }

        [TestMethod]
        public void ParentClearTrackedKey()
        {
            var child = new DictionaryTree<string, string>();
            child.Add("prop1", "child");
            var parent1 = new DictionaryTree<string, string>();
            parent1.Add("prop1", "parent1");
            var parent2 = new DictionaryTree<string, string>();
            parent2.Add("prop1", "parent2");

            child.Parents.Add(parent1);
            child.Parents.Add(parent2);

            child.TrackAll = true; // Sets all constituents to track gotten values

            Assert.AreEqual(child.Parents[0]["prop1"], "parent1"); // gotten
            // Parent1
            Assert.AreEqual(child.Parents[0].Accessed.Count, 1);
            Assert.AreEqual(child.Parents[0].Gotten.Count, 1);
            // Parent2
            Assert.AreEqual(child.Parents[1].Accessed.Count, 0);
            Assert.AreEqual(child.Parents[1].Gotten.Count, 0);
            // Child
            Assert.AreEqual(child.Accessed.Count, 1);
            Assert.AreEqual(child.Gotten.Count, 1);

            child.ClearTrackedKey("prop1"); // clear tracking for the key (both accessed and gotten)
            // Parent1
            Assert.AreEqual(child.Parents[0].Accessed.Count, 0);
            Assert.AreEqual(child.Parents[0].Gotten.Count, 0);
            // Child
            Assert.AreEqual(child.Accessed.Count, 0);
            Assert.AreEqual(child.Gotten.Count, 0);
        }

        [TestMethod]
        public void ClearAccessedKeyInOneParentOnly()
        {
            var parent1 = new DictionaryTree<string, string>();
            parent1.Add("prop1", "parent1");
            var parent2 = new DictionaryTree<string, string>();
            parent2.Add("prop1", "parent2");

            var child = new DictionaryTree<string, string>();
            child.Add("prop1", "child");
            child.Parents.Add(parent1);
            child.Parents.Add(parent2);

            child.TrackAccessed = true;
            Assert.IsTrue(child.Parents[0].ContainsKey("prop1")); // accessed in parent1
            Assert.IsTrue(child.Parents[1].ContainsKey("prop1")); // accessed in parent2

            Assert.AreEqual(child.Accessed.Count, 1); // Shows up in child accessed

            Assert.AreEqual(child.Parents[0].Accessed.Count, 1); // Shows up in parent1
            Assert.AreEqual(child.Parents[1].Accessed.Count, 1); // Shows up in parent2

            child.Parents[0].ClearAccessedKey("prop1"); // cleared from parent1
            Assert.AreEqual(child.Accessed.Count, 1); // child still shows accessed (because of parent2)
            Assert.AreEqual(child.Parents[0].Accessed.Count, 0); // no longer in parent1
            Assert.AreEqual(child.Parents[1].Accessed.Count, 1); // still shows up in parent2
        }

        [TestMethod]
        public void ClearGottenKeyInOneParentOnly()
        {
            var parent1 = new DictionaryTree<string, string>();
            parent1.Add("prop1", "parent1");
            var parent2 = new DictionaryTree<string, string>();
            parent2.Add("prop1", "parent2");

            var child = new DictionaryTree<string, string>();
            child.Add("prop1", "child");
            child.Parents.Add(parent1);
            child.Parents.Add(parent2);

            child.TrackGotten = true;
            Assert.AreEqual(child.Parents[0]["prop1"], "parent1"); // gotten in parent1
            Assert.AreEqual(child.Parents[1]["prop1"], "parent2"); // gotten in parent2

            Assert.AreEqual(child.Gotten.Count, 1); // Shows up in child
            Assert.AreEqual(child.Parents[0].Gotten.Count, 1); // Shows up in parent1
            Assert.AreEqual(child.Parents[1].Gotten.Count, 1); // Shows up in parent2

            child.Parents[0].ClearGottenKey("prop1"); // cleared from parent1
            Assert.AreEqual(child.Gotten.Count, 1); // child still shows gotten (because of parent2)
            Assert.AreEqual(child.Parents[0].Gotten.Count, 0); // no longer in parent1
            Assert.AreEqual(child.Parents[1].Gotten.Count, 1); // still shows up in parent2
        }

        [TestMethod]
        public void ClearAccessedKeyInBothParentsButNotChild()
        {
            var parent1 = new DictionaryTree<string, string>();
            parent1.Add("prop1", "parent1");
            var parent2 = new DictionaryTree<string, string>();
            parent2.Add("prop1", "parent2");

            var child = new DictionaryTree<string, string>();
            child.Add("prop1", "child");
            child.Parents.Add(parent1);
            child.Parents.Add(parent2);

            child.TrackAccessed = true;
            Assert.IsTrue(child.ContainsKey("prop1")); // accessed in child
            Assert.IsTrue(child.Parents[0].ContainsKey("prop1")); // accessed in parent1
            Assert.IsTrue(child.Parents[1].ContainsKey("prop1")); // accessed in parent2

            Assert.AreEqual(child.Accessed.Count, 1); // Shows up in child
            Assert.AreEqual(child.Parents[0].Accessed.Count, 1); // shows up in parent1
            Assert.AreEqual(child.Parents[1].Accessed.Count, 1); // shows up in parent2

            child.Parents[0].ClearAccessedKey("prop1");
            child.Parents[1].ClearAccessedKey("prop1");
            Assert.AreEqual(child.Accessed.Count, 1); // still shows up in child
            Assert.AreEqual(child.Parents[0].Accessed.Count, 0); // no longer in parent1
            Assert.AreEqual(child.Parents[1].Accessed.Count, 0); // no longer in parent2
        }

        [TestMethod]
        public void ClearGottenKeyInBothParentsButNotChild()
        {
            var parent1 = new DictionaryTree<string, string>();
            parent1.Add("prop1", "parent1");
            var parent2 = new DictionaryTree<string, string>();
            parent2.Add("prop1", "parent2");

            var child = new DictionaryTree<string, string>();
            child.Add("prop1", "child");
            child.Parents.Add(parent1);
            child.Parents.Add(parent2);

            child.TrackGotten = true;

            Assert.AreEqual(child["prop1"], "child"); // gotten in child
            Assert.AreEqual(child.Parents[0]["prop1"], "parent1"); // gotten in parent1
            Assert.AreEqual(child.Parents[1]["prop1"], "parent2"); // gotten in parent2

            Assert.AreEqual(child.Gotten.Count, 1); // shows up in child
            Assert.AreEqual(child.Parents[0].Gotten.Count, 1); // shows up in parent1
            Assert.AreEqual(child.Parents[1].Gotten.Count, 1); // shows up in parent2

            child.Parents[0].ClearGottenKey("prop1");
            child.Parents[1].ClearGottenKey("prop1");

            Assert.AreEqual(child.Gotten.Count, 1); // still shows up in child
            Assert.AreEqual(child.Parents[0].Gotten.Count, 0); // no longer in parent1
            Assert.AreEqual(child.Parents[1].Gotten.Count, 0); // no longer in parent2
        }

        [TestMethod]
        public void ClearTrackedKeyInBothParentsButNotChild()
        {
            var parent1 = new DictionaryTree<string, string>();
            parent1.Add("prop1", "parent1");
            var parent2 = new DictionaryTree<string, string>();
            parent2.Add("prop1", "parent2");

            var child = new DictionaryTree<string, string>();
            child.Add("prop1", "child");
            child.Parents.Add(parent1);
            child.Parents.Add(parent2);

            child.TrackAll = true;

            Assert.AreEqual(child["prop1"], "child"); // gotten in child
            Assert.AreEqual(child.Parents[0]["prop1"], "parent1"); // gotten in parent1
            Assert.AreEqual(child.Parents[1]["prop1"], "parent2"); // gotten in parent2

            // Child
            Assert.AreEqual(child.Accessed.Count, 1); // shows up in child accessed
            Assert.AreEqual(child.Gotten.Count, 1); // shows up in child gotten
            // Parent1
            Assert.AreEqual(child.Parents[0].Accessed.Count, 1); // shows up in parent1 accessed
            Assert.AreEqual(child.Parents[0].Gotten.Count, 1); // shows up in parent1 gotten
            // Parent2
            Assert.AreEqual(child.Parents[1].Accessed.Count, 1); // shows up in parent2 accessed
            Assert.AreEqual(child.Parents[1].Gotten.Count, 1); // shows up in parent2 gotten

            child.Parents[0].ClearTrackedKey("prop1");
            child.Parents[1].ClearTrackedKey("prop1");

            // Child
            Assert.AreEqual(child.Accessed.Count, 1); // still shows up in child accessed
            Assert.AreEqual(child.Gotten.Count, 1); // still shows up in child gotten
            // Parent1
            Assert.AreEqual(child.Parents[0].Accessed.Count, 0); // no longer in parent1 accessed
            Assert.AreEqual(child.Parents[0].Gotten.Count, 0); // no longer in parent1 gotten
            // Parent2
            Assert.AreEqual(child.Parents[1].Accessed.Count, 0); // no longer in parent2 accessed
            Assert.AreEqual(child.Parents[1].Gotten.Count, 0); // no longer in parent2 gotten
        }

        #endregion // Clearing Tracked Keys

        #endregion // Tracking
    }
}