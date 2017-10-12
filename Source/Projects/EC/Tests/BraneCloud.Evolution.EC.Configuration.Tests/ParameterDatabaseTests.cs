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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BraneCloud.Evolution.EC.Configuration.Tests
{
    /// <summary>
    /// Summary description for ParameterDatabaseTests
    /// </summary>
    [TestClass]
    public class ParameterDatabaseTests
    {
        private const string RelativePath = @"..\..\..\..\..\Solutions\EC\ParamFiles\ec";
        private const string NameValueFormat = "{0} = {1}";
        private static FileTree Tree;

        #region Housekeeping

        public ParameterDatabaseTests()
        {
        }
        #region Test Context

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

        #endregion

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            Tree = new FileTree(Path.GetFullPath(RelativePath));
        }
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

        #region Ad Hoc parameters

        [TestMethod]
        [Description("Test that ad hoc parameters are properly parsed.")]
        public void OneLayerAdHocParametersTest()
        {
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            // Add some miscellaneous parameters, some valid, some not
            pd.SetParameter(new Parameter("Hi there"), "Whatever");
            pd.SetParameter(new Parameter(new[] { "1", "2", "3" }), "    Whatever "); // extra whitespace is trimmed
            pd.SetParameter(new Parameter(new[] { "a", "b", "c" }).Pop().Push("d"), "Whatever"); // This becomes a.b.d
            Assert.AreEqual(pd.Count, 3);

            foreach (var k in pd.Keys)
            {
                Assert.AreEqual(pd.GetProperty(k.ToString()), "Whatever");
                context.WriteLine("{0} = {1}", k, pd[k]);
            }

            context.WriteLine("");
            context.WriteLine("\n\n PRINTING ALL PARAMETERS \n\n");

            using (var memStream = new MemoryStream())
            {
                var temp_writer = new StreamWriter(memStream, Encoding.Default) { AutoFlush = true };
                // let the database write to our stream
                pd.List(temp_writer, false); // No shadowed parameters here
                memStream.Position = 0;
                // now write those out for diagnostics
                context.WriteLine(new StreamReader(memStream).ReadToEnd());
            }
        }

        [TestMethod]
        [Description("Test that ad hoc parameters are properly parsed.")]
        public void OneLayerAdHocStringGottenTest()
        {
            var key = "Hi there";
            var val = "Whatever";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            // Add some miscellaneous parameters, some valid, some not
            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            Assert.AreEqual(pd.Count, 1);
            // Neither Accessed nor Gotten
            Assert.IsFalse(pd.Accessed.ContainsKey(key));
            Assert.IsFalse(pd.Gotten.ContainsKey(key));
            var p2 = pd.GetString(p1, null);
            // Both Accessed and Gotten
            Assert.IsTrue(pd.Accessed.ContainsKey(key));
            Assert.IsTrue(pd.Gotten.ContainsKey(key));
        }

        #endregion // Ad Hoc parameters

        #region Accessed and Gotten Status

        [TestMethod]
        [Description("Calling the base class 'GetProperty' method does not affect a parameter's 'Accessed' or 'Gotten' status.")]
        public void GetPropertyCallDoesNotAffectAccessedOrGottenStatus()
        {
            var pd = new ParameterDatabase();
            // Nothing in the database yet
            Assert.AreEqual(pd.Count, 0);

            var key = "Hi there";
            var val = "Whatever";

            // Set single parameter
            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            Assert.AreEqual(pd.Count, 1);

            // Neither Accessed nor Gotten
            Assert.IsFalse(pd.Accessed.ContainsKey(key));
            Assert.IsFalse(pd.Gotten.ContainsKey(key));

            var propValue = pd.GetProperty(key);

            // Neither Accessed nor Gotten
            Assert.IsFalse(pd.Accessed.ContainsKey(key));
            Assert.IsFalse(pd.Gotten.ContainsKey(key));
        }

        //[Ignore]
        [TestMethod]
        [Description("Checking if a parameter 'Exists' does not affect its 'Gotten' status but rather only its 'Accessed' status.")]
        public void CheckingParameterExistsChangesAccessedStatusButNotGottenStatus()
        {
            var key = "Hi there";
            var val = "Whatever";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            // Add some miscellaneous parameters, some valid, some not
            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            Assert.AreEqual(pd.Count, 1);
            // Neither Accessed nor Gotten
            Assert.IsFalse(pd.Accessed.ContainsKey(key));
            Assert.IsFalse(pd.Gotten.ContainsKey(key));
            var p2 = pd.ParameterExists(p1, null);
            // Both Accessed and Gotten
            Assert.IsTrue(pd.Accessed.ContainsKey(key));
            Assert.IsFalse(pd.Gotten.ContainsKey(key));
        }

        [TestMethod]
        [Description("Getting a parameter affects BOTH its 'Gotten' status AND its 'Accessed' status.")]
        public void GettingAParameterChangesAccessedStatusAndGottenStatus()
        {
            var key = "Hi there";
            var val = "Whatever";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            // Add some miscellaneous parameters, some valid, some not
            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            Assert.AreEqual(pd.Count, 1);
            // Neither Accessed nor Gotten
            Assert.IsFalse(pd.Accessed.ContainsKey(key));
            Assert.IsFalse(pd.Gotten.ContainsKey(key));
            var p2 = pd.GetString(p1, null); // Here is where we are causing BOTH statuses to change
            // Both Accessed and Gotten
            Assert.IsTrue(pd.Accessed.ContainsKey(key));
            Assert.IsTrue(pd.Gotten.ContainsKey(key));
        }

        #endregion // Accessed and Gotten Status

        #region GetInt

        [TestMethod]
        [Description("Getting a parameter as an Integer works fine if the value can be parsed by Int32.")]
        public void GetInt()
        {
            var key = "Hi there";
            var val = "1";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var intVal = pd.GetInt(p1, null);
            Assert.AreEqual(intVal, 1);
        }

        [TestMethod]
        [Description("GetInt will parse simple real numbers (without explicit type code).")]
        public void GetIntWillParseRealNumberLiterals()
        {
            var key = "Hi there";
            var val = "1.0";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var intVal = pd.GetInt(p1, null);
            Assert.AreEqual(intVal, 1);
        }

        [TestMethod]
        [Description("GetInt will not parse a hex literal.")]
        [ExpectedException(typeof(FormatException))]
        public void GetIntWillNotParseHexLiteralValue()
        {
            var key = "Hi there";
            var val = "0x01";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var intVal = pd.GetInt(p1, null);
            Assert.AreEqual(intVal, 1);
        }

        [TestMethod]
        [Description("GetInt will parse exponential literals if they resolve to integrals.")]
        public void GetIntWillParseExponentialLiteralValue()
        {
            var key = "Hi there";
            var val = "1.09e2";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var intVal = pd.GetInt(p1, null);
            Assert.AreEqual(intVal, 109);
        }

        [TestMethod]
        [Description("GetInt will not parse exponential literals that are out of range.")]
        [ExpectedException(typeof(FormatException))]
        public void GetIntWillNotParseExponentialLiteralValueOutOfRange()
        {
            var key = "Hi there";
            var val = "1.097e2";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var intVal = pd.GetInt(p1, null);
            Assert.AreEqual(intVal, 109);
        }

        [TestMethod]
        [Description("GetInt will not parse a hex literal.")]
        [ExpectedException(typeof(FormatException))]
        public void GetIntWillNotParseNonIntegralLiteralValues()
        {
            var key = "Hi there";
            var val = "1.038";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var intVal = pd.GetInt(p1, null);
            Assert.AreEqual(intVal, 1);
        }

        [TestMethod]
        [Description("GetInt will parse exponential literals.")]
        [ExpectedException(typeof(FormatException))]
        public void GetIntWillNotParseExplicitFloatingPointLiteralCodes()
        {
            var key = "Hi there";
            var val = "1.09f";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var intVal = pd.GetInt(p1, null);
            Assert.AreEqual(intVal, 1);
        }

        [TestMethod]
        [Description("GetIntWithDefault checks primary parameter first, default paramemter second, and finally, the constant argument last.")]
        public void GetIntWithDefaultConstant()
        {
            var key = "myparam";
            var pd1 = new ParameterDatabase();
            var pd2 = new ParameterDatabase();

            var p1 = new Parameter(key);
            var p2 = new Parameter(key);

            pd1.AddParent(pd2);
            // Neither database has p1 or p2, thus the third constant argument (1) will be returned...
            var intVal = pd1.GetIntWithDefault(p1, p2, 1);
            Assert.AreEqual(intVal, 1);
        }

        [TestMethod]
        [Description("GetIntWithDefault checks primary parameter first, default paramemter second, and finally, the constant argument last.")]
        public void GetIntWithDefaultParameter()
        {
            var key = "myparam";
            var pd1 = new ParameterDatabase();
            var pd2 = new ParameterDatabase();

            var p1 = new Parameter(key);
            var p2 = new Parameter(key);
            pd2.SetParameter(p2, "9");
            pd1.AddParent(pd2);
            // Database pd1 has no parameters, pd2 has parameter p2, 
            // thus the default parameter argument (9), not the constant argument (1), will be returned...
            var intVal = pd1.GetIntWithDefault(p1, p2, 1);
            Assert.AreEqual(intVal, 9);
        }

        [TestMethod]
        [Description("GetIntWithMax returns -1 when Min is 0 and Max has been exceeded.")]
        public void GetIntWithMaxReturnsMinusOneWhenMinIsZero()
        {
            var key = "Hi there";
            var val = "11";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var intVal = pd.GetIntWithMax(p1, null, 0, 10); // Min is 0, max is 10, then the return value is -1
            Assert.AreEqual(intVal, -1);
        }

        [TestMethod]
        [Description("GetIntegerWithMax returns Min - 1 when Max has been exceeded.")]
        public void GetIntWithMaxThatExceedsMaxReturnsMinMinusOne()
        {
            var key = "Hi there";
            var val = "11";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var intVal = pd.GetIntWithMax(p1, null, 5, 10); // Min is 5, max is 10, then the return value is 4
            Assert.AreEqual(intVal, 4);
        }

        #endregion // GetInt

        #region GetLong

        [TestMethod]
        [Description("Getting a parameter as an Integer works fine if the value can be parsed by Int64.")]
        public void GetLong()
        {
            var key = "Hi there";
            var val = "1";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetLong(p1, null);
            Assert.AreEqual(v, 1);
        }

        [TestMethod]
        [Description("GetLong will parse simple real numbers (without explicit type code).")]
        public void GetLongWillParseRealNumberLiterals()
        {
            var key = "Hi there";
            var val = "1.0";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetLong(p1, null);
            Assert.AreEqual(v, 1);
        }

        [TestMethod]
        [Description("GetLong will not parse a hex literal.")]
        [ExpectedException(typeof(FormatException))]
        public void GetLongWillNotParseHexLiteralValue()
        {
            var key = "Hi there";
            var val = "0x01";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var intVal = pd.GetLong(p1, null);
            Assert.AreEqual(intVal, 1);
        }

        [TestMethod]
        [Description("GetLong will parse exponential literals if they resolve to integrals.")]
        public void GetLongWillParseExponentialLiteralValue()
        {
            var key = "Hi there";
            var val = "1.09e2";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetLong(p1, null);
            Assert.AreEqual(v, 109);
        }

        [TestMethod]
        [Description("GetLong will not parse exponential literals that are out of range.")]
        [ExpectedException(typeof(FormatException))]
        public void GetLongWillNotParseExponentialLiteralValueOutOfRange()
        {
            var key = "Hi there";
            var val = "1.097e2";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetLong(p1, null);
            Assert.AreEqual(v, 109);
        }

        [TestMethod]
        [Description("GetLong will not parse a hex literal.")]
        [ExpectedException(typeof(FormatException))]
        public void GetLongWillNotParseNonIntegralLiteralValues()
        {
            var key = "Hi there";
            var val = "1.038";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetLong(p1, null);
            Assert.AreEqual(v, 1);
        }

        [TestMethod]
        [Description("GetLong will parse exponential literals.")]
        [ExpectedException(typeof(FormatException))]
        public void GetLongWillNotParseExplicitFloatingPointLiteralCodes()
        {
            var key = "Hi there";
            var val = "1.09f";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetLong(p1, null);
            Assert.AreEqual(v, 1);
        }

        [TestMethod]
        [Description("GetLongWithDefault checks primary parameter first, default paramemter second, and finally, the constant argument last.")]
        public void GetLongWithDefaultConstant()
        {
            var key = "myparam";
            var pd1 = new ParameterDatabase();
            var pd2 = new ParameterDatabase();

            var p1 = new Parameter(key);
            var p2 = new Parameter(key);

            pd1.AddParent(pd2);
            // Neither database has p1 or p2, thus the third constant argument (1) will be returned...
            var v = pd1.GetLongWithDefault(p1, p2, 1);
            Assert.AreEqual(v, 1);
        }

        [TestMethod]
        [Description("GetLongWithDefault checks primary parameter first, default paramemter second, and finally, the constant argument last.")]
        public void GetLongWithDefaultParameter()
        {
            var key = "myparam";
            var pd1 = new ParameterDatabase();
            var pd2 = new ParameterDatabase();

            var p1 = new Parameter(key);
            var p2 = new Parameter(key);
            pd2.SetParameter(p2, "9");
            pd1.AddParent(pd2);
            // Database pd1 has no parameters, pd2 has parameter p2, 
            // thus the default parameter argument (9), not the constant argument (1), will be returned...
            var v = pd1.GetLongWithDefault(p1, p2, 1);
            Assert.AreEqual(v, 9);
        }

        //[Ignore]
        [TestMethod]
        [Description("GetLongWithMax returns -1 when Min is 0 and Max has been exceeded.")]
        public void GetLongWithMaxReturnsMinusOneWhenMinIsZero()
        {
            var key = "Hi there";
            var val = "11";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetLongWithMax(p1, null, 0, 10); // Min is 0, max is 10, then the return value is -1
            Assert.AreEqual(v, -1);
        }

        //[Ignore]
        [TestMethod]
        [Description("GetLongWithMax returns Min - 1 when Max has been exceeded.")]
        public void GetLongWithMaxThatExceedsMaxReturnsMinMinusOne()
        {
            var key = "Hi there";
            var val = "11";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetLongWithMax(p1, null, 5, 10); // Min is 5, max is 10, then the return value is 4
            Assert.AreEqual(v, 4);
        }

        #endregion // GetLong

        #region GetDouble

        [TestMethod]
        [Description("Getting a parameter as a double works fine if the value can be parsed by Double.")]
        public void GetDouble()
        {
            var key = "Hi there";
            var val = "1.0";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetDouble(p1, null);
            Assert.AreEqual(v, 1.0);
        }

        [TestMethod]
        [Description("Getting a parameter as a double works fine if the value can be parsed by Double.")]
        public void GetDoubleFromInt()
        {
            var key = "Hi there";
            var val = "1";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetDouble(p1, null);
            Assert.AreEqual(v, 1.0);
        }

        [TestMethod]
        [Description("GetDouble will not parse a hex literal.")]
        [ExpectedException(typeof(FormatException))]
        public void GetDoubleWillNotParseHexLiteralValue()
        {
            var key = "Hi there";
            var val = "0x01";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetDouble(p1, null);
            Assert.AreEqual(v, 1);
        }

        [TestMethod]
        [Description("GetDouble will parse exponential literals.")]
        public void GetDoubleWillParseExponentialLiteralValue()
        {
            var key = "Hi there";
            var val = "1.097e2";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetDouble(p1, null);
            Assert.AreEqual(v, 109.7);
        }

        [TestMethod]
        [Description("GetDouble will not parse real number literals with explicit type codes.")]
        [ExpectedException(typeof(FormatException))]
        public void GetDoubleWillNotParseExplicitFloatingPointLiteralCodes()
        {
            var key = "Hi there";
            var val = "1.09f";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetDouble(p1, null);
            Assert.AreEqual(v, 1.09);
        }

        [TestMethod]
        [Description("GetDoubleWithDefault checks primary parameter first, default paramemter second, and finally, the constant argument last.")]
        public void GetDoubleWithDefaultConstant()
        {
            var key = "myparam";
            var pd1 = new ParameterDatabase();
            var pd2 = new ParameterDatabase();

            var p1 = new Parameter(key);
            var p2 = new Parameter(key);

            pd1.AddParent(pd2);
            // Neither database has p1 or p2, thus the third constant argument (1) will be returned...
            var v = pd1.GetDoubleWithDefault(p1, p2, 1.09);
            Assert.AreEqual(v, 1.09);
        }

        [TestMethod]
        [Description("GetDoubleWithDefault checks primary parameter first, default paramemter second, and finally, the constant argument last.")]
        public void GetDoubleWithDefaultParameter()
        {
            var key = "myparam";
            var pd1 = new ParameterDatabase();
            var pd2 = new ParameterDatabase();

            var p1 = new Parameter(key);
            var p2 = new Parameter(key);
            pd2.SetParameter(p2, "9.01");
            pd1.AddParent(pd2);
            // Database pd1 has no parameters, pd2 has parameter p2, 
            // thus the default parameter argument (9.01), not the constant argument (1.09), will be returned...
            var v = pd1.GetDoubleWithDefault(p1, p2, 1.09);
            Assert.AreEqual(v, 9.01);
        }

        [TestMethod]
        [Description("GetDoubleWithMax returns -1.0 when Min is 0.0 and Max has been exceeded.")]
        public void GetDoubleWithMaxReturnsMinusOneWhenMinIsZero()
        {
            var key = "Hi there";
            var val = "11.29";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetDoubleWithMax(p1, null, 0.0, 10.09); // Min is 0.0, max is 10.09, then the return value is -1.0
            Assert.AreEqual(v, -1.0);
        }

        [TestMethod]
        [Description("GetDoubleWithMax returns Min - 1 when Max has been exceeded.")]
        public void GetDoubleWithMaxThatExceedsMaxReturnsMinMinusOne()
        {
            var key = "Hi there";
            var val = "11.09";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetDoubleWithMax(p1, null, 5.09, 10.09); // Min is 5, max is 10, then the return value is 4
            Assert.AreEqual(v, 4.09);
        }

        #endregion // GetDouble

        #region GetFloat

        [TestMethod]
        [Description("Getting a parameter as a double works fine if the value can be parsed by Float.")]
        public void GetFloat()
        {
            var key = "Hi there";
            var val = "1.0";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetFloat(p1, null);
            Assert.AreEqual(v, 1.0f);
        }

        [TestMethod]
        [Description("Getting a parameter as a double works fine if the value can be parsed by Float.")]
        public void GetFloatFromInt()
        {
            var key = "Hi there";
            var val = "1";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetFloat(p1, null);
            Assert.AreEqual(v, 1.0f);
        }

        [TestMethod]
        [Description("GetFloat will not parse a hex literal.")]
        [ExpectedException(typeof(FormatException))]
        public void GetFloatWillNotParseHexLiteralValue()
        {
            var key = "Hi there";
            var val = "0x01";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetFloat(p1, null);
            Assert.AreEqual(v, 1.0f);
        }

        [TestMethod]
        [Description("GetFloat will parse exponential literals.")]
        public void GetFloatWillParseExponentialLiteralValue()
        {
            var key = "Hi there";
            var val = "1.097e2";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetFloat(p1, null);
            Assert.AreEqual(v, 109.7f);
        }

        [TestMethod]
        [Description("GetFloat will not parse real number literals with explicit type codes.")]
        [ExpectedException(typeof(FormatException))]
        public void GetFloatWillNotParseExplicitFloatingPointLiteralCodes()
        {
            var key = "Hi there";
            var val = "1.09f";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetFloat(p1, null);
            Assert.AreEqual(v, 1.09f);
        }

        [TestMethod]
        [Description("GetFloatWithDefault checks primary parameter first, default paramemter second, and finally, the constant argument last.")]
        public void GetFloatWithDefaultConstant()
        {
            var key = "myparam";
            var pd1 = new ParameterDatabase();
            var pd2 = new ParameterDatabase();

            var p1 = new Parameter(key);
            var p2 = new Parameter(key);

            pd1.AddParent(pd2);
            // Neither database has p1 or p2, thus the third constant argument (1) will be returned...
            var v = pd1.GetFloatWithDefault(p1, p2, 1.09);
            Assert.AreEqual(v, 1.09f);
        }

        [TestMethod]
        [Description("GetFloatWithDefault checks primary parameter first, default paramemter second, and finally, the constant argument last.")]
        public void GetFloatWithDefaultParameter()
        {
            var key = "myparam";
            var pd1 = new ParameterDatabase();
            var pd2 = new ParameterDatabase();

            var p1 = new Parameter(key);
            var p2 = new Parameter(key);
            pd2.SetParameter(p2, "9.01");
            pd1.AddParent(pd2);
            // Database pd1 has no parameters, pd2 has parameter p2, 
            // thus the default parameter argument (9.01), not the constant argument (1.09), will be returned...
            var v = pd1.GetFloatWithDefault(p1, p2, 1.09);
            Assert.AreEqual(v, 9.01f);
        }

        [TestMethod]
        [Description("GetFloatWithMax returns -1.0 when Min is 0.0 and Max has been exceeded.")]
        public void GetFloatWithMaxReturnsMinusOneWhenMinIsZero()
        {
            var key = "Hi there";
            var val = "11.29";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetFloatWithMax(p1, null, 0.0, 10.09); // Min is 0.0, max is 10.09, then the return value is -1.0
            Assert.AreEqual(v, -1.0f);
        }

        [TestMethod]
        [Description("GetFloatWithMax returns Min - 1 when Max has been exceeded.")]
        public void GetFloatWithMaxThatExceedsMaxReturnsMinMinusOne()
        {
            var key = "Hi there";
            var val = "11.09";
            var pd = new ParameterDatabase();
            Assert.AreEqual(pd.Count, 0);

            var p1 = new Parameter(key);
            pd.SetParameter(p1, val);
            var v = pd.GetFloatWithMax(p1, null, 5.09, 10.09); // Min is 5, max is 10, then the return value is 4
            Assert.AreEqual(v, 4.09f);
        }

        #endregion // GetFloat

        #region File Parsing

        [TestMethod]
        [Description("Test that the root parameter file (ec.params) is properly parsed. (assumes 6 properties)")]
        public void SingleParameterFileTest()
        {
            var fileSpec = Path.Combine(RelativePath, "ec.params");
            Assert.IsTrue(System.IO.File.Exists(fileSpec));
            var pd = new ParameterDatabase(new FileInfo(fileSpec));
            Assert.AreEqual(pd.Count, 6);

            // Add some miscellaneous parameters, some valid, some not
            pd.SetParameter(new Parameter("Hi there"), "Whatever");
            pd.SetParameter(new Parameter(new[] { "1", "2", "3" }), "    Whatever "); // extra whitespace is trimmed
            pd.SetParameter(new Parameter(new[] { "a", "b", "c" }).Pop().Push("d"), "Whatever"); // This becomes a.b.d
            Assert.AreEqual(pd.Count, 9);

            foreach (var k in pd.Keys)
            {
                context.WriteLine("{0} = {1}", k, pd[k]);
            }

            context.WriteLine("");
            context.WriteLine("\n\n PRINTING ALL PARAMETERS (Including Shadowed) \n\n");

            var memStream = new MemoryStream();
            var temp_writer = new StreamWriter(memStream, Encoding.Default) { AutoFlush = true };
            pd.List(temp_writer, true);
            memStream.Position = 0;
            context.WriteLine(new StreamReader(memStream).ReadToEnd());
            memStream.Close();



            context.WriteLine("\n\n PRINTING SORTED PARAMETERS (Excluding Shadowed) \n\n");
            memStream = new MemoryStream();
            var temp_writer2 = new StreamWriter(memStream, Encoding.Default) { AutoFlush = true };
            pd.List(temp_writer2, false);
            memStream.Position = 0;
            context.WriteLine(new StreamReader(memStream).ReadToEnd());
            memStream.Close();
        }

        [TestMethod]
        [Description("Test that basic parent/Child hierarchy can be established.")]
        public void TwoLayerParameterDatabaseTest()
        {
            var fileSpec = Path.Combine(RelativePath, "ec.params");
            Assert.IsTrue(System.IO.File.Exists(fileSpec));
            var pd = new ParameterDatabase(new FileInfo(fileSpec));
            pd.SetParameter(new Parameter("Hi there"), "Whatever");
            pd.SetParameter(new Parameter(new[] { "1", "2", "3" }), " Whatever ");
            pd.SetParameter(new Parameter(new[] { "a", "b", "c" }).Pop().Push("d"), "Whatever"); // key becomes a.b.d

            var pd2 = new ParameterDatabase();
            pd2.SetParameter(new Parameter("Hi there"), "Hello!"); // This will override parent.0
            pd2.SetParameter(new Parameter(new[] { "1", "2", "3" }), "GoofBall"); // This will override parent.0
            pd2.SetParameter(new Parameter(new[] { "4", "5", "6" }), "GoofBall7"); // This is just added

            pd2.AddParent(pd);

            foreach (var k in pd2.Keys)
            {
                context.WriteLine("{0} = {1}", k, pd2[k]);
            }

            context.WriteLine("");
            context.WriteLine("\n\n PRINTING ALL PARAMETERS \n\n");

            var memStream = new MemoryStream();
            var temp_writer = new StreamWriter(memStream, Encoding.Default) { AutoFlush = true };
            pd2.List(temp_writer, true);
            memStream.Position = 0;
            context.WriteLine(new StreamReader(memStream).ReadToEnd());
            memStream.Close();

            context.WriteLine("\n\n PRINTING ONLY VALID PARAMETERS \n\n");
            memStream = new MemoryStream();
            var temp_writer2 = new StreamWriter(memStream, Encoding.Default) { AutoFlush = true };
            pd2.List(temp_writer2, false);
            memStream.Position = 0;
            context.WriteLine(new StreamReader(memStream).ReadToEnd());
            memStream.Close();
        }

        [TestMethod]
        [Description("NOT FINISHED: Loads database from 'de.params' which contains one parent, 'simple.params'. Checks to make sure inherited params can be accessed.")]
        public void AncestryFrom_EC_DE_de_params()
        {
            var subdir = "de";
            var fileName = "de.params";
            var parent0 = "simple.params";

            var fileSpec = Path.GetFullPath(Path.Combine(RelativePath, subdir, fileName));
            Assert.IsTrue(System.IO.File.Exists(fileSpec));

            // Set up the database
            var pd = new ParameterDatabase(new FileInfo(fileSpec));

            // Now look up the parent.0 parameter value and make sure it matches what we expect
            var parent = pd.GetFile(new Parameter("parent.0"), null);
            Assert.AreEqual(parent.Name, parent0);

            // The following seems very convoluted. But keep in mind 
            // we are trying to verify that one implementation is valid 
            // using a second implementation that is entirely different. (Indeed, that is the whole point!)

            // Use the ParameterFileTree to double check the details
            var subnode = Tree.Root[subdir]; // We can search down from the root given a relative subdirectory
            Assert.IsTrue(subnode.Name == fileName.Split(new[]{'.'})[0]); // Doublecheck that we've been given the correct subdirectory
            var paramFile = subnode.Files[fileName]; // Now grab the relevant source param file
            var parentRelPath = paramFile.Properties["parent.0"]; // This now gives the relative path to the parent param file
            var parentAbsPath = Path.GetFullPath(Path.Combine(subnode.FullName, parentRelPath)); // Now we should have the full path of the parent
            Assert.IsTrue(System.IO.File.Exists(parentAbsPath));
            //var fi = new FileInfo(parentAbsPath);
            // Now that we've verified the validity of the relative path specified in the source param file
            // we can get the copy from the Tree and use that to check all params 
            // in the database hierarchy (which is only two levels deep in this case) 
            // NOT FINISHED: This is currently comparing raw values from the Tree against over-shadowed values in the Database : NEED MORE WORK!
            //foreach (var p in Tree.FileCollection[Path.GetFullPath(fi.FullName)].Properties) // This if from the tree
            //{
            //    var v = pd.GetProperty(p.Key); // This is what we find in the database
            //    Assert.AreEqual(v, p.Value);
            //}

            var childFile = Tree.NodeRegistry[fileSpec];
            Assert.IsNotNull(childFile);
            var parentFile = Tree.NodeRegistry[parentAbsPath];
            Assert.IsNotNull(parentFile);
            foreach (DictionaryEntry kv in pd)
            {
                context.WriteLine("{0} : {1}", kv.Key, kv.Value);
            }
            foreach (var prop in parentFile.Properties)
            {
                if (childFile.Properties.ContainsKey(prop.Key))
                {
                    context.WriteLine("ShadowedProperty: {0}    Parent: {1}    Child: {2}", prop.Key,
                                      childFile.Properties[prop.Key], prop.Value);
                }
            }
        }

        #endregion // File Parsing
    }
}