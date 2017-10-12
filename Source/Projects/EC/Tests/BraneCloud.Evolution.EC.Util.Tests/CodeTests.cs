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
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BraneCloud.Evolution.EC.Util.Tests
{
    /// <summary>
    /// The <see cref="Code"/> class defines various methods for handling Encoding and Decoding
    /// of standard types. This basically amounts to human-readable serialization without loss
    /// of precision (especially for floats and doubles). These tests merely verify that what
    /// goes out can come back in unchanged.
    /// </summary>
    /// <remarks>
    /// When encoding the human-readable version of a float or double, 
    /// <see cref="Code"/> uses the 'roundtrip' formatting style (i.e. ToString("R")). 
    /// This ensures that output (in human-readable form) can be parsed back to the nearest possible value. 
    /// But, of course, it does NOT guarantee that will be the same value as before encoding.
    /// Compare that to the Int64 encoding (which IS exact), and there could very well be a slight difference.
    /// </remarks>
    [TestClass]
    public class CodeTests
    {
        #region Housekeeping

        public CodeTests()
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

        #region Decoding

        [TestMethod]
        public void DecodeEmptyStringIsError()
        {
            var dr = new DecodeReturn("");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_ERROR);
        }

        #region Boolean

        #region TRUE

        [TestMethod]
        public void DecodeBooleanUppercaseT()
        {
            var dr = new DecodeReturn("T");
            Code.Decode(dr);
            Assert.IsTrue(dr.B.HasValue);
            Assert.IsTrue(dr.B.Value);
            Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
        }

        [TestMethod]
        public void DecodeBooleanLowercaseTrue()
        {
            var dr = new DecodeReturn("true");
            Code.Decode(dr);
            Assert.IsTrue(dr.B.HasValue);
            Assert.IsTrue(dr.B.Value);
            Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
        }

        [TestMethod]
        public void DecodeBooleanPropercaseTrue()
        {
            var dr = new DecodeReturn("True");
            Code.Decode(dr);
            Assert.IsTrue(dr.B.HasValue);
            Assert.IsTrue(dr.B.Value);
            Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
        }

        [TestMethod]
        public void DecodeBooleanUppercaseTrue()
        {
            var dr = new DecodeReturn("TRUE");
            Code.Decode(dr);
            Assert.IsTrue(dr.B.HasValue);
            Assert.IsTrue(dr.B.Value);
            Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
        }

        #endregion // TRUE
        #region FALSE

        [TestMethod]
        public void DecodeBooleanLowercaseF()
        {
            var dr = new DecodeReturn("f");
            Code.Decode(dr);
            Assert.IsTrue(dr.B.HasValue);
            Assert.IsFalse(dr.B.Value);
            Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
        }

        [TestMethod]
        public void DecodeBooleanUppercaseF()
        {
            var dr = new DecodeReturn("F");
            Code.Decode(dr);
            Assert.IsTrue(dr.B.HasValue);
            Assert.IsFalse(dr.B.Value);
            Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
        }

        [TestMethod]
        public void DecodeBooleanLowercaseFalse()
        {
            var dr = new DecodeReturn("false");
            Code.Decode(dr);
            Assert.IsTrue(dr.B.HasValue);
            Assert.IsFalse(dr.B.Value);
            Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
        }

        public void DecodeBooleanPropercaseFalse()
        {
            var dr = new DecodeReturn("False");
            Code.Decode(dr);
            Assert.IsTrue(dr.B.HasValue);
            Assert.IsFalse(dr.B.Value);
            Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
        }

        [TestMethod]
        public void DecodeBooleanUppercaseFalse()
        {
            var dr = new DecodeReturn("FALSE");
            Code.Decode(dr);
            Assert.IsTrue(dr.B.HasValue);
            Assert.IsFalse(dr.B.Value);
            Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
        }

        #endregion // FALSE

        #endregion // Boolean
        #region Char

        [TestMethod]
        public void DecodeCharLowercaseA()
        {
            var dr = new DecodeReturn("'a'");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_CHAR);
            Assert.IsTrue(dr.C.HasValue);
            Assert.AreEqual(dr.C.Value, 'a');
        }

        [TestMethod]
        public void DecodeCharBackspace()
        {
            var dr = new DecodeReturn("'\b'");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_CHAR);
            Assert.IsTrue(dr.C.HasValue);
            Assert.AreEqual(dr.C.Value, '\b');
        }

        [TestMethod]
        public void DecodeCharTab()
        {
            var dr = new DecodeReturn("'\t'");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_CHAR);
            Assert.IsTrue(dr.C.HasValue);
            Assert.AreEqual(dr.C.Value, '\t');
        }

        [TestMethod]
        public void DecodeCharNewline()
        {
            var dr = new DecodeReturn("'\n'");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_CHAR);
            Assert.IsTrue(dr.C.HasValue);
            Assert.AreEqual(dr.C.Value, '\n');
        }

        [TestMethod]
        public void DecodeCharSingleQuote()
        {
            var dr = new DecodeReturn("'\''");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_CHAR);
            Assert.IsTrue(dr.C.HasValue);
            Assert.AreEqual(dr.C.Value, '\'');
        }

        [TestMethod]
        public void DecodeCharDoubleQuote()
        {
            var dr = new DecodeReturn("'\"'");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_CHAR);
            Assert.IsTrue(dr.C.HasValue);
            Assert.AreEqual(dr.C.Value, '\"');
        }

        [TestMethod]
        [Description("This one differs somewhat from the others, because we need to escape the backslash twice to create the encoded character.")]
        public void DecodedTypeFromCharBackslash()
        {
            var dr = new DecodeReturn("'\\\\'");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_CHAR);
            Assert.IsTrue(dr.C.HasValue);
            Assert.AreEqual(dr.C.Value, '\\');
        }

        [TestMethod]
        public void DecodedTypeFromCharUnicodeZero()
        {
            var dr = new DecodeReturn("'\x0000'");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_CHAR);
            Assert.IsTrue(dr.C.HasValue);
            Assert.AreEqual(dr.C.Value, '\0');
        }

        [TestMethod]
        public void DecodedTypeFromCharUnicodeAmperstand()
        {
            var dr = new DecodeReturn("'\u0026'");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_CHAR);
            Assert.IsTrue(dr.C.HasValue);
            Assert.AreEqual(dr.C.Value, '\u0026');
            Assert.AreEqual(dr.C.Value, '&');
        }

        #endregion // Char
        #region SByte (The Code class only deals with signed bytes, as a way of optimizing a numerical domain.)

        [TestMethod]
        public void DecodeSByteOne()
        {
            var dr = new DecodeReturn("b1|"); // NOTE: sbyte encoded with 'b'
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_BYTE);
            Assert.AreEqual(dr.L, 1);
        }

        [TestMethod]
        public void DecodeSByteMax()
        {
            var dr = new DecodeReturn("b127|"); // NOTE: sbyte encoded with 'b'
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_BYTE);
            Assert.AreEqual(dr.L, SByte.MaxValue);
        }

        [TestMethod]
        public void DecodeSByteMin()
        {
            var dr = new DecodeReturn("b-128|"); // NOTE: sbyte encoded with 'b'
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_BYTE);
            Assert.AreEqual(dr.L, SByte.MinValue);
        }

        #endregion // SByte (The Code class only deals with signed bytes, as a way of optimizing a numerical domain.)
        #region Short

        [TestMethod]
        public void DecodeShortOne()
        {
            var dr = new DecodeReturn("s1|");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_SHORT);
            Assert.AreEqual(dr.L, 1);
        }

        [TestMethod]
        public void DecodeShortMaxValue()
        {
            //var sh = short.MaxValue;
            //context.WriteLine(sh.ToString());
            var dr = new DecodeReturn("s32767|");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_SHORT);
            Assert.AreEqual(dr.L, short.MaxValue);
        }

        [TestMethod]
        public void DecodeShortMinValue()
        {
            var sh = short.MinValue;
            context.WriteLine(sh.ToString());
            var dr = new DecodeReturn("s-32768|");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_SHORT);
            Assert.AreEqual(dr.L, short.MinValue);
        }

        #endregion // Short
        #region Int32

        [TestMethod]
        public void DecodedTypeFromIntegerOne()
        {
            var dr = new DecodeReturn("i1|");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_INTEGER);
            Assert.AreEqual(dr.L, 1);
        }

        [TestMethod]
        public void DecodedTypeFromIntegerMaxValue()
        {
            //var i = int.MaxValue;
            //context.WriteLine(i.ToString());
            var dr = new DecodeReturn("i2147483647|");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_INTEGER);
            Assert.AreEqual(dr.L, int.MaxValue);
        }

        [TestMethod]
        public void DecodedTypeFromIntegerMinValue()
        {
            var i = int.MinValue;
            context.WriteLine(i.ToString());
            var dr = new DecodeReturn("i-2147483648|");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_INTEGER);
            Assert.AreEqual(dr.L, int.MinValue);
        }

        #endregion // Int32
        #region Int64

        [TestMethod]
        public void DecodeLongOne()
        {
            var dr = new DecodeReturn("l1|");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_LONG);
            Assert.AreEqual(dr.L, 1);
        }

        [TestMethod]
        public void DecodeLongMaxValue()
        {
            //var l = long.MaxValue;
            //context.WriteLine(l.ToString());
            var dr = new DecodeReturn("l9223372036854775807|");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_LONG);
            Assert.AreEqual(dr.L, long.MaxValue);
        }

        [TestMethod]
        public void DecodeLongMinValue()
        {
            //var l = long.MinValue;
            //context.WriteLine(l.ToString());
            var dr = new DecodeReturn("l-9223372036854775808|");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_LONG);
            Assert.AreEqual(dr.L, long.MinValue);
        }

        #endregion // Int32
        #region Float (Single)

        [TestMethod]
        public void DecodeFloatOnePointOne()
        {
            var f = 1.1f;
            var s = "f" + BitConverter.DoubleToInt64Bits(f) + "|" + f.ToString("R") + "|";
            //var s = "f4607632778870128640|1.1|";
            var dr = new DecodeReturn(s);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_FLOAT);
            Assert.AreEqual(dr.D, 1.1f);
        }

        [TestMethod]
        [Description("This is an example of decoding the human readable version of a float when the Int64 encoding is not provided.")]
        public void DecodeFloatOnePointOneHumanReadableOnly()
        {
            var f = 1.1f;
            var s = "f|" + f.ToString("R") + "|";
            var dr = new DecodeReturn(s);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_FLOAT);
            Assert.AreEqual(dr.D, 1.1f);
        }

        [TestMethod]
        [Description("The encoded version of a float will be EXACTLY the same as the original, because we preserve all bits.")]
        public void DecodeFloatMaxValue()
        {
            var f = float.MaxValue;
            var s = "f" + BitConverter.DoubleToInt64Bits(f) + "|" + f.ToString("R") + "|";
            context.WriteLine(s);
            var dr = new DecodeReturn(s);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_FLOAT);
            Assert.AreEqual(dr.D, float.MaxValue);
        }

        [TestMethod]
        [Description("The human-readable version of a float is guaranteed to be equal to the original value if roundtrip formatting is used (i.e. ToString(\"R\").")]
        public void DecodeFloatMaxValueHumanReadableOnly()
        {
            var f = float.Parse(float.MaxValue.ToString("R"));
            var s = "f" + BitConverter.DoubleToInt64Bits(f) + "|" + f.ToString("R") + "|";
            context.WriteLine(s);
            var dr = new DecodeReturn(s);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_FLOAT);
            Assert.AreEqual(dr.D, f);
        }

        [TestMethod]
        [Description("The encoded version of a float will be EXACTLY the same as the original, because we preserve all bits.")]
        public void DecodeFloatMinValue()
        {
            var f = float.MinValue;
            var s = "f" + BitConverter.DoubleToInt64Bits(f) + "|" + f.ToString("R") + "|";
            context.WriteLine(s);
            var dr = new DecodeReturn("f-4039728866288205824|-3.40282347E+38|");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_FLOAT);
            Assert.AreEqual(dr.D, f);
        }

        [TestMethod]
        [Description("The human-readable version of a float is guaranteed to be equal to the original value if roundtrip formatting is used (i.e. ToString(\"R\").")]
        public void DecodeFloatMinValueHumanReadableOnly()
        {
            var f = float.Parse(float.MinValue.ToString("R"));
            var s = "f|" + f.ToString("R") + "|";
            //var s = "f|-3.40282347E+38|";
            context.WriteLine(s);
            var dr = new DecodeReturn(s);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_FLOAT);
            Assert.AreEqual(dr.D, f);
        }

        #endregion // Float (Single)
        #region Double

        [TestMethod]
        public void DecodeDoubleOnePointOne()
        {
            var d = 1.1;
            var s = "d" + BitConverter.DoubleToInt64Bits(d) + "|" + d.ToString("R") + "|";
            context.WriteLine(s);
            //var s = "d4607632778762754458|1.1|";
            var dr = new DecodeReturn(s);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_DOUBLE);
            Assert.AreEqual(dr.D, d);
        }

        [TestMethod]
        [Description("This is an example of decoding the human readable version of a double when the Int64 encoding is not provided.")]
        public void DecodeDoubleOnePointOneHumanReadableOnly()
        {
            var s = "d|1.1|";
            var dr = new DecodeReturn(s);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_DOUBLE);
            Assert.AreEqual(dr.D, 1.1);
        }

        [TestMethod]
        [Description("The encoded version of a double will be EXACTLY the same as the original, because we preserve all bits.")]
        public void DecodeDoubleMaxValue()
        {
            var d = double.MaxValue;
            //var s = "d" + BitConverter.DoubleToInt64Bits(d) + "|" + d + "|";
            var s = "d9218868437227405311|1.7976931348623157E+308|";
            context.WriteLine(Code.Encode(double.MaxValue));
            var dr = new DecodeReturn(s);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_DOUBLE);
            Assert.AreEqual(dr.D, d);
        }

        [TestMethod]
        [Description("The human-readable version of a double is guaranteed to be equal to the original value if roundtrip formatting is specified (i.e. ToString('R')).")]
        public void DecodeDoubleMaxValueHumanReadableOnly()
        {
            var d = double.MaxValue.ToString("R");
            context.WriteLine(d);
            var s = "d|" + d + "|";
            context.WriteLine(s);
            var dr = new DecodeReturn(s);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_DOUBLE);
            Assert.AreEqual(dr.D, double.Parse(d));
        }

        [TestMethod]
        [Description("The encoded version of a double will be EXACTLY the same as the original, because we preserve all bits.")]
        public void DecodeDoubleMinValue()
        {
            var d = float.MinValue;
            var s = "d" + BitConverter.DoubleToInt64Bits(d) + "|" + d.ToString("R") + "|";
            context.WriteLine(Code.Encode(double.MinValue));
            context.WriteLine(s);
            var dr = new DecodeReturn("d-4503599627370497|-1.7976931348623157E+308|");
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_DOUBLE);
            Assert.AreEqual(dr.D, double.MinValue);
        }

        [TestMethod]
        [Description("The human-readable version of a double is guaranteed to be equal to the original value if roundtrip formatting is specified (i.e. ToString('R')).")]
        public void DecodeDoubleMinValueHumanReadableOnly()
        {
            //var s = "d|" + double.MinValue.ToString("R") + "|";
            //context.WriteLine(s);
            var s = "d|-1.7976931348623157E+308|";
            context.WriteLine(s);
            var dr = new DecodeReturn(s);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_DOUBLE);
            Assert.AreEqual(dr.D, double.Parse(double.MinValue.ToString("R")));
        }

        #endregion // Double

        #endregion // Decoding

        #region Encoding

        #region Boolean

        [TestMethod]
        public void EncodeBooleanTrue()
        {
            var s = Code.Encode(false);
            Assert.AreEqual(s, "F");
        }

        [TestMethod]
        public void EncodeBooleanFalse()
        {
            var s = Code.Encode(true);
            Assert.AreEqual(s, "T");
        }

        #endregion // Boolean
        #region Char

        [TestMethod]
        public void EncodeCharLowercaseA()
        {
            var s = Code.Encode('a');
            Assert.AreEqual(s, "'a'");
        }

        [TestMethod]
        public void EncodeCharUppercaseA()
        {
            var s = Code.Encode('A');
            Assert.AreEqual(s, "'A'");
        }

        [TestMethod]
        public void EncodeCharBackspace()
        {
            var s = Code.Encode('\b');
            Assert.AreEqual(s, "'\\b'");
        }

        [TestMethod]
        public void EncodeCharTab()
        {
            var s = Code.Encode('\t');
            Assert.AreEqual(s, "'\\t'");
        }

        [TestMethod]
        public void EncodeCharNewline()
        {
            var s = Code.Encode('\n');
            Assert.AreEqual(s, "'\\n'");
        }

        [TestMethod]
        public void EncodeCharSingleQuote()
        {
            var s = Code.Encode('\'');
            Assert.AreEqual(s, "'\\''");
        }

        [TestMethod]
        public void EncodeCharDoubleQuote()
        {
            var s = Code.Encode('\"');
            Assert.AreEqual(s, "'\\\"'");
        }

        [TestMethod]
        public void EncodeCharBackslash()
        {
            var s = Code.Encode('\\');
            Assert.AreEqual(s, @"'\\'");
        }

        [TestMethod]
        public void EncodeCharUnicodeZero()
        {
            var s = Code.Encode('\x0000');
            Assert.AreEqual(s, "'\\0'");
            Assert.AreEqual(s, @"'\0'");
        }

        #endregion // Char
        #region SByte

        [TestMethod]
        public void EncodeSByteOne()
        {
            var v = (sbyte) 1;
            var s = Code.Encode(v);
            Assert.AreEqual(s, "b1|");
        }

        [TestMethod]
        public void EncodeSByteMax()
        {
            var v = sbyte.MaxValue;
            var s = Code.Encode(v);
            Assert.AreEqual(s, "b127|");
        }

        [TestMethod]
        public void EncodeSByteMin()
        {
            var v = sbyte.MinValue;
            var s = Code.Encode(v);
            Assert.AreEqual(s, "b-128|");
        }

        #endregion // SByte
        #region Short

        [TestMethod]
        public void EncodeShortOne()
        {
            var v = (short)1;
            var s = Code.Encode(v);
            Assert.AreEqual(s, "s1|");
        }

        [TestMethod]
        public void EncodeShortMax()
        {
            var v = short.MaxValue;
            context.WriteLine(v.ToString());
            var s = Code.Encode(v);
            Assert.AreEqual(s, "s32767|");
        }

        [TestMethod]
        public void EncodeShortMin()
        {
            var v = short.MinValue;
            var s = Code.Encode(v);
            Assert.AreEqual(s, "s-32768|");
        }

        #endregion // Short
        #region Int32

        [TestMethod]
        public void EncodeIntOne()
        {
            var v = 1;
            var s = Code.Encode(v);
            Assert.AreEqual(s, "i1|");
        }

        [TestMethod]
        public void EncodeIntMax()
        {
            var v = int.MaxValue;
            var s = Code.Encode(v);
            Assert.AreEqual(s, "i2147483647|");
        }

        [TestMethod]
        public void EncodeIntMin()
        {
            var v = int.MinValue;
            var s = Code.Encode(v);
            Assert.AreEqual(s, "i-2147483648|");
        }

        #endregion // Int32
        #region Int64

        [TestMethod]
        public void EncodeLongOne()
        {
            var v = (long)1;
            var s = Code.Encode(v);
            Assert.AreEqual(s, "l1|");
        }

        [TestMethod]
        public void EncodeLongMax()
        {
            var v = long.MaxValue;
            var s = Code.Encode(v);
            Assert.AreEqual(s, "l9223372036854775807|");
        }

        [TestMethod]
        public void EncodeLongMin()
        {
            var v = long.MinValue;
            var s = Code.Encode(v);
            Assert.AreEqual(s, "l-9223372036854775808|");
        }

        #endregion // Int64
        #region Float (Single)

        [TestMethod]
        public void EncodeFloatOnePointOne()
        {
            var v = 1.1f;
            var s = Code.Encode(v);
            Assert.AreEqual(s, "f4607632778870128640|1.1|");
        }

        [TestMethod]
        public void EncodeFloatMax()
        {
            var v = float.MaxValue;
            var s = Code.Encode(v);
            Assert.AreEqual(s, "f5183643170566569984|3.40282347E+38|");
        }

        [TestMethod]
        public void EncodeFloatMin()
        {
            var v = float.MinValue;
            var s = Code.Encode(v);
            Assert.AreEqual(s, "f-4039728866288205824|-3.40282347E+38|");
        }

        #endregion // Float (Single)
        #region Double

        [TestMethod]
        public void EncodeDoubleOnePointOne()
        {
            var v = 1.1;
            var s = Code.Encode(v);
            Assert.AreEqual(s, "d4607632778762754458|1.1|");
        }

        [TestMethod]
        public void EncodeDoubleMax()
        {
            var v = double.MaxValue;
            var s = Code.Encode(v);
            Assert.AreEqual(s, "d9218868437227405311|1.7976931348623157E+308|");
        }

        [TestMethod]
        public void EncodeDoubleMin()
        {
            var v = double.MinValue;
            var s = Code.Encode(v);
            Assert.AreEqual(s, "d-4503599627370497|-1.7976931348623157E+308|");
        }

        #endregion // Double

        #endregion // Encoding

        #region CheckPreamble

        [TestMethod]
        public void CheckPreambleOnly()
        {
            var s = "Hello World!";
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(s))))
            {
                var dr = Code.CheckPreamble("Hello World!", null, reader);
                Assert.IsNotNull(dr);
                Assert.AreEqual(dr.LineNumber, 1);
            }
        }

        [TestMethod]
        public void CheckPreambleAfterNewline()
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine();
            sb = sb.AppendLine("Hello World!");
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()))))
            {
                var dr = Code.CheckPreamble("Hello World!", null, reader);
                Assert.IsNotNull(dr);
                Assert.AreEqual(dr.LineNumber, 2);
            }
        }

        [TestMethod]
        public void CheckPreambleAndOneBooleanValue()
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine("Hello World: T");
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()))))
            {
                var dr = Code.CheckPreamble("Hello World:", null, reader);
                Assert.IsNotNull(dr);
                Assert.AreEqual(dr.LineNumber, 1);
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
                Assert.AreEqual(dr.B, true);
            }
        }

        [TestMethod]
        public void CheckPreambleAndTwoBooleanValues()
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine("Hello World: T F");
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()))))
            {
                var dr = Code.CheckPreamble("Hello World:", null, reader);
                Assert.IsNotNull(dr);
                Assert.AreEqual(dr.LineNumber, 1);
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
                Assert.AreEqual(dr.B, true);
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
                Assert.AreEqual(dr.B, false);
            }
        }

        [TestMethod]
        public void CheckPreambleAndOneCharValue()
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine("Hello World: '\\n'");
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()))))
            {
                var dr = Code.CheckPreamble("Hello World:", null, reader);
                Assert.IsNotNull(dr);
                Assert.AreEqual(dr.LineNumber, 1);
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_CHAR);
                Assert.IsTrue(dr.C.HasValue);
                Assert.AreEqual(dr.C, '\n');
            }
        }

        [TestMethod]
        public void CheckPreambleAndTwoCharValues()
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine("Hello World: '\\n' '\\t'");
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()))))
            {
                var dr = Code.CheckPreamble("Hello World:", null, reader);
                Assert.IsNotNull(dr);
                Assert.AreEqual(dr.LineNumber, 1);
                // Newline
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_CHAR);
                Assert.IsTrue(dr.C.HasValue);
                Assert.AreEqual(dr.C, '\n');
                // Tab
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_CHAR);
                Assert.IsTrue(dr.C.HasValue);
                Assert.AreEqual(dr.C, '\t');
            }
        }

        [TestMethod]
        public void CheckPreambleAndTwoSBytes()
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine("Hello World: b127| b-128|");
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()))))
            {
                var dr = Code.CheckPreamble("Hello World:", null, reader);
                Assert.IsNotNull(dr);
                Assert.AreEqual(dr.LineNumber, 1);
                // SByte.MaxValue
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_BYTE);
                Assert.AreEqual(dr.L, SByte.MaxValue);
                // SByte.MinValue
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_BYTE);
                Assert.AreEqual(dr.L, SByte.MinValue);
            }
        }

        [TestMethod]
        public void CheckPreambleAndTwoShorts()
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine("Hello World: s32767| s-32768|");
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()))))
            {
                var dr = Code.CheckPreamble("Hello World:", null, reader);
                Assert.IsNotNull(dr);
                Assert.AreEqual(dr.LineNumber, 1);
                // Newline
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_SHORT);
                Assert.AreEqual(dr.L, short.MaxValue);
                // Tab
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_SHORT);
                Assert.AreEqual(dr.L, short.MinValue);
            }
        }

        [TestMethod]
        public void CheckPreambleAndTwoIntegers()
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine("Hello World: i2147483647| i-2147483648|");
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()))))
            {
                var dr = Code.CheckPreamble("Hello World:", null, reader);
                Assert.IsNotNull(dr);
                Assert.AreEqual(dr.LineNumber, 1);
                // Newline
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_INT);
                Assert.AreEqual(dr.L, int.MaxValue);
                // Tab
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_INT);
                Assert.AreEqual(dr.L, int.MinValue);
            }
        }

        [TestMethod]
        public void CheckPreambleAndTwoLongs()
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine("Hello World: l9223372036854775807| l-9223372036854775808|");
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()))))
            {
                var dr = Code.CheckPreamble("Hello World:", null, reader);
                Assert.IsNotNull(dr);
                Assert.AreEqual(dr.LineNumber, 1);
                // Newline
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_LONG);
                Assert.AreEqual(dr.L, long.MaxValue);
                // Tab
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_LONG);
                Assert.AreEqual(dr.L, long.MinValue);
            }
        }

        [TestMethod]
        public void CheckPreambleAndTwoFloats()
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine("Hello World: f5183643170566569984|3.40282347E+38| f-4039728866288205824|-3.40282347E+38|");
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()))))
            {
                var dr = Code.CheckPreamble("Hello World:", null, reader);
                Assert.IsNotNull(dr);
                Assert.AreEqual(dr.LineNumber, 1);
                // Newline
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_FLOAT);
                Assert.AreEqual(dr.D, float.MaxValue);
                // Tab
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_FLOAT);
                Assert.AreEqual(dr.D, float.MinValue);
            }
        }

        [TestMethod]
        public void CheckPreambleAndTwoDoubles()
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine("Hello World: d9218868437227405311|1.7976931348623157E+308| d-4503599627370497|-1.7976931348623157E+308|");
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()))))
            {
                var dr = Code.CheckPreamble("Hello World:", null, reader);
                Assert.IsNotNull(dr);
                Assert.AreEqual(dr.LineNumber, 1);
                // Newline
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_DOUBLE);
                Assert.AreEqual(dr.D, double.MaxValue);
                // Tab
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_DOUBLE);
                Assert.AreEqual(dr.D, double.MinValue);
            }
        }

        #endregion // CheckPreamble

        #region ReadWithPreamble

        [TestMethod]
        public void ReadingBoolWithPreamble()
        {
            var s = "Hello World: T";
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(s))))
            {
                var v = Code.ReadBooleanWithPreamble("Hello World:", null, reader);
                Assert.IsTrue(v);
            }
        }

        [TestMethod]
        public void ReadingCharacterWithPreamble()
        {
            var s = "Hello World: '\\n'";
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(s))))
            {
                var v = Code.ReadCharacterWithPreamble("Hello World:", null, reader);
                Assert.AreEqual(v, '\n');
            }
        }

        [TestMethod]
        public void ReadingByteWithPreamble()
        {
            var s = "Hello World: b127|";
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(s))))
            {
                var v = Code.ReadByteWithPreamble("Hello World:", null, reader);
                Assert.AreEqual(v, sbyte.MaxValue);
            }
        }

        [TestMethod]
        public void ReadingShortWithPreamble()
        {
            var s = "Hello World: s-32768|";
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(s))))
            {
                var v = Code.ReadShortWithPreamble("Hello World:", null, reader);
                Assert.AreEqual(v, short.MinValue);
            }
        }

        [TestMethod]
        public void ReadingLongWithPreamble()
        {
            var s = "Hello World: l9223372036854775807|";
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(s))))
            {
                var v = Code.ReadLongWithPreamble("Hello World:", null, reader);
                Assert.AreEqual(v, long.MaxValue);
            }
        }

        [TestMethod]
        public void ReadingFloatWithPreamble()
        {
            var s = "Hello World: f5183643170566569984|3.40282347E+38|";
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(s))))
            {
                var v = Code.ReadFloatWithPreamble("Hello World:", null, reader);
                Assert.AreEqual(v, float.MaxValue);
            }
        }

        [TestMethod]
        public void ReadingDoubleWithPreamble()
        {
            var s = "Hello World: d9218868437227405311|1.7976931348623157E+308|";
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(s))))
            {
                var v = Code.ReadDoubleWithPreamble("Hello World:", null, reader);
                Assert.AreEqual(v, double.MaxValue);
            }
        }

        #endregion // ReadWithPreamble

        #region Multiline (Block)

        [TestMethod]
        public void DecodeMultilineBooleanValuesSimple()
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine("T");
            sb = sb.AppendLine("F");

            var dr = new DecodeReturn(sb.ToString());
            Assert.IsNotNull(dr);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
            Assert.AreEqual(dr.B, true);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
            Assert.AreEqual(dr.B, false);
        }

        [TestMethod]
        public void DecodeMultilineIntegerValuesSimple()
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine("i1|");
            sb = sb.AppendLine("i2|");

            var dr = new DecodeReturn(sb.ToString());
            Assert.IsNotNull(dr);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_INT);
            Assert.AreEqual(dr.L, 1);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_INT);
            Assert.AreEqual(dr.L, 2);
        }

        [TestMethod]
        public void DecodeMultilineBooleansWithPreamble()
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine("Hello World: T");
            sb = sb.AppendLine("F");
            sb = sb.AppendLine(); // A blank line (or EOF) marks the end of the block
            sb = sb.AppendLine("Another Preamble:..."); // This should not be retained after CheckPreamble(...)
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()))))
            {
                var dr = Code.CheckPreamble("Hello World:", null, reader, /* multiline */ true);
                Assert.IsNotNull(dr);
                Assert.AreEqual(dr.LineNumber, 1);
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
                Assert.AreEqual(dr.B, true);
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
                Assert.AreEqual(dr.B, false);
            }
        }

        [TestMethod]
        public void DecodeMultilineMixedTypesWithPreamble()
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine("Hello World: T"); // Boolean
            sb = sb.AppendLine("i1| F"); // Integer and Boolean
            sb = sb.AppendLine("f|1.1|"); // Float (human-readable only)
            sb = sb.AppendLine(); // A blank line (or EOF) marks the end of the block
            sb = sb.AppendLine("Another Preamble:..."); // This should not be retained after CheckPreamble(...)

            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()))))
            {
                var dr = Code.CheckPreamble("Hello World:", null, reader, /* multiline */ true);
                Assert.IsNotNull(dr);
                Assert.AreEqual(dr.LineNumber, 1);
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
                Assert.AreEqual(dr.B, true);
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_INT);
                Assert.AreEqual(dr.L, 1);
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
                Assert.AreEqual(dr.B, false);
                Code.Decode(dr);
                Assert.AreEqual(dr.D, 1.1f);
            }
        }

        [TestMethod]
        public void DecodeMultilineBooleansWithPreambleEOF()
        {
            var sb = new StringBuilder();
            sb = sb.AppendLine("Hello World: T");
            sb = sb.AppendLine("F"); // This uses EOF instead of blank line to mark end of block
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()))))
            {
                var dr = Code.CheckPreamble("Hello World:", null, reader, /* multiline */ true);
                Assert.IsNotNull(dr);
                Assert.AreEqual(dr.LineNumber, 1);
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
                Assert.AreEqual(dr.B, true);
                Code.Decode(dr);
                Assert.AreEqual(dr.Type, DecodeReturn.T_BOOLEAN);
                Assert.AreEqual(dr.B, false);
            }
        }

        #endregion // Multiline (Block)

        #region Strings

        [TestMethod]
        public void EncodeStringSimple()
        {
            var q = "\"";
            var input = "Hello World!";
            var output = Code.Encode(input);
            Assert.AreEqual(q + input + q, output);
            var dr = new DecodeReturn(output);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_STRING);
            Assert.AreEqual(dr.S, input);
        }

        [TestMethod]
        public void EncodeStringWithNewline()
        {
            var q = "\"";
            var input = "Hello World!\n";
            var output = Code.Encode(input);
            Assert.AreEqual(q + @"Hello World!\n" + q, output);
            var dr = new DecodeReturn(output);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_STRING);
            Assert.AreEqual(dr.S, input);
        }

        [TestMethod]
        public void EncodeStringWithTab()
        {
            var q = "\"";
            var input = "Hello World! \t Goodbye!";
            var output = Code.Encode(input);
            Assert.AreEqual(q + @"Hello World! \t Goodbye!" + q, output);
            var dr = new DecodeReturn(output);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_STRING);
            Assert.AreEqual(dr.S, input);
        }

        [TestMethod]
        public void EncodeStringWithBackspace()
        {
            var q = "\"";
            var input = "Hello World! \b Goodbye!";
            var output = Code.Encode(input);
            Assert.AreEqual(q + @"Hello World! \b Goodbye!" + q, output);
            var dr = new DecodeReturn(output);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_STRING);
            Assert.AreEqual(dr.S, input);
        }

        [TestMethod]
        public void EncodeStringWithBackslash()
        {
            var q = "\"";
            var input = "Hello World! \\ Goodbye!";
            var output = Code.Encode(input);
            Assert.AreEqual(q + @"Hello World! \\ Goodbye!" + q, output);
            var dr = new DecodeReturn(output);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_STRING);
            Assert.AreEqual(dr.S, input);
        }

        [TestMethod]
        public void EncodeStringWithUnicodeZero()
        {
            var q = "\"";
            var input = "Hello World! \x0000 Goodbye!";
            var output = Code.Encode(input);
            Assert.AreEqual(q + @"Hello World! \0 Goodbye!" + q, output);
            var dr = new DecodeReturn(output);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_STRING);
            Assert.AreEqual(dr.S, input);
        }

        [TestMethod]
        public void EncodeStringWithSingleQuote()
        {
            var q = "\"";
            var input = "Hello World! 'a' Goodbye!";
            var output = Code.Encode(input);
            Assert.AreEqual(q + "Hello World! 'a' Goodbye!" + q, output);
            var dr = new DecodeReturn(output);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_STRING);
            Assert.AreEqual(dr.S, input);
        }

        [TestMethod]
        public void EncodeStringWithDoubleQuote()
        {
            var q = "\"";
            var input = "Hello World! \"a\" Goodbye!";
            var output = Code.Encode(input);
            Assert.AreEqual(q + "Hello World! \\\"a\\\" Goodbye!" + q, output);
            var dr = new DecodeReturn(output);
            Code.Decode(dr);
            Assert.AreEqual(dr.Type, DecodeReturn.T_STRING);
            Assert.AreEqual(dr.S, input);
        }

        #endregion // Strings
    }
}