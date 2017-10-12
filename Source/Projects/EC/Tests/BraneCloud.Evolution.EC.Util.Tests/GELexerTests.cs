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

using System.Collections.Generic;
//using System.Text.RegularExpressions;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BraneCloud.Evolution.Util.RegularExpressions;

namespace BraneCloud.Evolution.EC.Util.Tests
{
    /// <summary>
    /// Summary description for GELexerTests
    /// </summary>
    [TestClass]
    public class GELexerTests
    {
        #region Housekeeping

        public GELexerTests()
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

        #region General Regex Tests

        [TestMethod]
        public void ValidateEmailAddress()
        {
            const string s = "foo@bar.baz";
            const string pattern = @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))" +
                                   @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$";

            context.WriteLine("Pattern: \n{0}", pattern);
            // Return true if strIn is in valid e-mail format.
            var isMatch = Regex.IsMatch(s, pattern);

            Assert.IsTrue(isMatch);
        }

        [TestMethod]
        public void ConvertDateFromMonthFirstToYearFirst()
        {
            var date = "12/31/1997";
            var newDate = RegexUtilities.ConvertDateFromMonthFirstToYearFirst(date, '/', '-');
            Assert.AreEqual("1997-12-31", newDate);

            date = "12-31-97";
            newDate = RegexUtilities.ConvertDateFromMonthFirstToYearFirst(date, '-', '/');
            Assert.AreEqual("97/12/31", newDate);

            date = "The party lasted from 12/31/1997 to 1/1/2011";
            newDate = RegexUtilities.ConvertDateFromMonthFirstToYearFirst(date, '/', '-');
            Assert.AreEqual("The party lasted from 1997-12-31 to 2011-1-1", newDate);
        }

        #endregion // General Regex Tests

        #region GELexer Sanity Checks

        [TestMethod]
        [Description("This is the bare minimum that the tokenizer should be capable of handling.")]
        public void SimpleAlphanumericExplicit()
        {
            var input = "A B C D";
            var p1 = "A";
            var p2 = "B";
            var p3 = "C";
            var p4 = "D";
            var patterns = new List<string>();
            patterns.Add(p1);
            patterns.Add(p2);
            patterns.Add(p3);
            patterns.Add(p4);

            var lexer = new GELexer(input, patterns.ToArray());

            var t1 = lexer.NextToken();
            var t2 = lexer.NextToken();
            var t3 = lexer.NextToken();
            var t4 = lexer.NextToken();

            Assert.AreEqual(t1, "A");
            Assert.AreEqual(t2, "B");
            Assert.AreEqual(t3, "C");
            Assert.AreEqual(t4, "D");
        }

        [TestMethod]
        [Description("This is the bare minimum that the tokenizer should be capable of handling.")]
        public void SimpleAlphanumericAlternateOrdering()
        {
            var input = "A B C D";
            var p1 = "A";
            var p2 = "C"; // swapped
            var p3 = "B"; // swapped
            var p4 = "D";
            var patterns = new List<string>();
            patterns.Add(p1);
            patterns.Add(p2);
            patterns.Add(p3);
            patterns.Add(p4);

            var lexer = new GELexer(input, patterns.ToArray());

            var t1 = lexer.NextToken();
            var t2 = lexer.NextToken();
            var t3 = lexer.NextToken();
            var t4 = lexer.NextToken();

            Assert.AreEqual(t1, "A");
            Assert.AreEqual(t2, "B"); // swapped in regex array, should still find it in correct order
            Assert.AreEqual(t3, "C"); // swapped in regex array, should still find it in correct order
            Assert.AreEqual(t4, "D");
        }

        [TestMethod]
        [Description("Minimal tokenizing with a little surrounding whitespace in patterns.")]
        public void SimpleAlphanumericWithSurroundingWhitespace()
        {
            var input = "A B C D";
            var p1 = "\\s*A\\s*";
            var p2 = "\\s*C\\s*"; // swapped
            var p3 = "\\s*B\\s*"; // swapped
            var p4 = "\\s*D\\s*";
            var patterns = new List<string>();
            patterns.Add(p1);
            patterns.Add(p2);
            patterns.Add(p3);
            patterns.Add(p4);

            var lexer = new GELexer(input, patterns.ToArray());

            var t1 = lexer.NextToken();
            var t2 = lexer.NextToken();
            var t3 = lexer.NextToken();
            var t4 = lexer.NextToken();

            Assert.AreEqual(t1, "A");
            Assert.AreEqual(t2, "B"); // swapped in regex array, should still find it in correct order
            Assert.AreEqual(t3, "C"); // swapped in regex array, should still find it in correct order
            Assert.AreEqual(t4, "D");
        }

        [TestMethod]
        [Description("Minimal tokenizing with a little surrounding whitespace in patterns.")]
        public void SimpleAlphanumericWithEmbeddedWhitespaceGreedy()
        {
            var input = "A B C D";
            var p1 = "\\s*A\\s*";
            var p2 = "\\s*B\\s*C\\s*"; // This should overshadow the single match that follows
            var p3 = "\\s*C\\s*"; // We "hop over" this one because of the previous two part match
            var p4 = "\\s*D\\s*";
            var patterns = new List<string>();
            patterns.Add(p1);
            patterns.Add(p2);
            patterns.Add(p3);
            patterns.Add(p4);

            var lexer = new GELexer(input, patterns.ToArray());

            // should only get 3 tokens instead of 4
            var t1 = lexer.NextToken();
            var t2 = lexer.NextToken();
            var t3 = lexer.NextToken();

            Assert.AreEqual(t1, "A");
            Assert.AreEqual(t2, "B C");
            Assert.AreEqual(t3, "D");
        }

        [TestMethod]
        [Description("Minimal tokenizing with a little surrounding whitespace in patterns.")]
        public void SimpleAlphanumericWithEmbeddedWhitespaceAlternateOrder()
        {
            var input = "A B C D"; // This should overshadow the single match in p3
            var p1 = "\\s*B\\s*C\\s*";
            var p2 = "\\s*A\\s*";
            var p3 = "\\s*C\\s*"; // We "hop over" this one because of the previous two part match in p1
            var p4 = "\\s*D\\s*";
            var patterns = new List<string>();
            patterns.Add(p1);
            patterns.Add(p2);
            patterns.Add(p3);
            patterns.Add(p4);

            var lexer = new GELexer(input, patterns.ToArray());

            // should only get 3 tokens instead of 4
            var t1 = lexer.NextToken();
            var t2 = lexer.NextToken();
            var t3 = lexer.NextToken();

            Assert.AreEqual(t1, "A");
            Assert.AreEqual(t2, "B C");
            Assert.AreEqual(t3, "D");
        }

        [TestMethod]
        [Description("Minimal tokenizing with a little surrounding whitespace in patterns.")]
        public void SimpleAlphanumericWithEmbeddedWhitespaceInterception()
        {
            var input = "A B C D";
            var p1 = "\\s*A\\s*";
            var p2 = "\\s*B\\s*"; // This should overshadow the "double" match that follows
            var p3 = "\\s*B\\s*C\\s*"; // We "hop over" this one because we "intercept the single token above
            var p4 = "\\s*C\\s*";
            var p5 = "\\s*D\\s*";
            var patterns = new List<string>();
            patterns.Add(p1);
            patterns.Add(p2);
            patterns.Add(p3);
            patterns.Add(p4);
            patterns.Add(p5);

            var lexer = new GELexer(input, patterns.ToArray());

            // We get 4 tokens, but the double pattern is passed over
            var t1 = lexer.NextToken();
            var t2 = lexer.NextToken();
            var t3 = lexer.NextToken();
            var t4 = lexer.NextToken();

            Assert.AreEqual(t1, "A");
            Assert.AreEqual(t2, "B");
            Assert.AreEqual(t3, "C");
            Assert.AreEqual(t4, "D");
        }

        #endregion // GELexer Sanity Checks

        #region Real-World Checks

        #region Ant

        [TestMethod]
        [Description("Tokenizing the grammar found in 'ant.grammar' (ec.app.ant)")]
        public void TokenizeAntGrammar()
        {
            var strings = new[]
                              {
                                  "<prog> ::= <op>",
                                  "<op> ::= (if-food-ahead <op> <op>)",
                                  "<op> ::=  (progn2 <op> <op>)",
                                  "<op> ::= (progn3 <op> <op> <op>)",
                                  "<op> ::= (left) | (right) | (move)"
                              };
            var patterns = DEFAULT_REGEXES;

            var rule1 = strings[0];
            var rule2 = strings[1];
            var rule3 = strings[2];
            var rule4 = strings[3];
            var rule5 = strings[4];

            // Rule 1
            var lexer = new GELexer(rule1, patterns);
            Assert.AreEqual(lexer.NextToken(), "<prog>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.IsNull(lexer.NextToken());
            // Rule 2
            lexer = new GELexer(rule2, patterns);
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "if-food-ahead");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.IsNull(lexer.NextToken());
            // Rule 3
            lexer = new GELexer(rule3, patterns);
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "progn2");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.IsNull(lexer.NextToken());
            // Rule 4
            lexer = new GELexer(rule4, patterns);
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "progn3");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.IsNull(lexer.NextToken());
            // Rule 5
            lexer = new GELexer(rule5, patterns);
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "left");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "right");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "move");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.IsNull(lexer.NextToken());
        }

        #endregion // Ant
        #region Lawnmower

        [TestMethod]
        [Description("Tokenizing the grammar found in 'lawnmower.grammar' (ec.app.lawnmower)")]
        public void TokenizeLawnmowerGrammar()
        {
            var strings = new[]
                              {
                                  "<start> ::= <op>",
                                  "<op> ::= (progn2 <op> <op>) | (v8a <op> <op>) | (frog <op>)",
                                  "<op> ::= (mow) | (left) | (ERC) | (ADF0) | (ADF1 <op>)"
                              };
            var patterns = DEFAULT_REGEXES;

            var rule1 = strings[0];
            var rule2 = strings[1];
            var rule3 = strings[2];

            // Rule 1
            var lexer = new GELexer(rule1, patterns);
            Assert.AreEqual(lexer.NextToken(), "<start>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.IsNull(lexer.NextToken());
            // Rule 2
            lexer = new GELexer(rule2, patterns);
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "progn2");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "v8a");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "frog");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.IsNull(lexer.NextToken());
            // Rule 3
            lexer = new GELexer(rule3, patterns);
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "mow");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "left");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "ERC");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "ADF0");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "ADF1");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        [Description("Tokenizing the grammar found in 'adf0.grammar' (ec.app.lawnmower)")]
        public void TokenizeLawnmowerADF0Grammar()
        {
            var strings = new[]
                              {
                                  "<start> ::= <op>",
                                  "<op> ::= (progn2 <op> <op>) | (v8a <op> <op>)",
                                  "<op> ::= (mow) | (left) | (ERC)"
                              };
            var patterns = DEFAULT_REGEXES;

            var rule1 = strings[0];
            var rule2 = strings[1];
            var rule3 = strings[2];

            // Rule 1
            var lexer = new GELexer(rule1, patterns);
            Assert.AreEqual(lexer.NextToken(), "<start>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.IsNull(lexer.NextToken());
            // Rule 2
            lexer = new GELexer(rule2, patterns);
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "progn2");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "v8a");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.IsNull(lexer.NextToken());
            // Rule 3
            lexer = new GELexer(rule3, patterns);
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "mow");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "left");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "ERC");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.IsNull(lexer.NextToken());
        }

        [TestMethod]
        [Description("Tokenizing the grammar found in 'adf1.grammar' (ec.app.lawnmower)")]
        public void TokenizeLawnmowerADF1Grammar()
        {
            var strings = new[]
                              {
                                  "<start> ::= <op>",
                                  "<op> ::= (progn2 <op> <op>) | (v8a <op> <op>)",
                                  "<op> ::= (mow) | (left) | (ERC) | (ADF0) | (ARG0)"
                              };
            var patterns = DEFAULT_REGEXES;

            var rule1 = strings[0];
            var rule2 = strings[1];
            var rule3 = strings[2];

            // Rule 1
            var lexer = new GELexer(rule1, patterns);
            Assert.AreEqual(lexer.NextToken(), "<start>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.IsNull(lexer.NextToken());
            // Rule 2
            lexer = new GELexer(rule2, patterns);
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "progn2");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "v8a");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.IsNull(lexer.NextToken());
            // Rule 3
            lexer = new GELexer(rule3, patterns);
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "mow");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "left");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "ERC");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "ADF0");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "ARG0");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.IsNull(lexer.NextToken());
        }

        #endregion // Lawnmower
        #region Regression

        [TestMethod]
        [Description("Tokenizing the grammar found in 'regression.grammar' (ec.app.regression)")]
        public void TokenizeRegressionGrammar()
        {
            var strings = new[]
                              {
                                  "# grammar of the regression problem including ERCs",
                                  "<start> ::= <op>",
                                  "<op> ::= (+ <op><op>)|(-<op><op>) |(* <op> <op>) | (% <op> <op>)",
                                  "<op>::=(sin <op>) | (cos <op>) | (exp <op>) | (rlog <op>)",
                                  "<op>::=(x) | (   ERC     )"
                              };
            var patterns = DEFAULT_REGEXES;

            var rule1 = strings[0];
            var rule2 = strings[1];
            var rule3 = strings[2];
            var rule4 = strings[3];
            var rule5 = strings[4];

            // Rule 1
            var lexer = new GELexer(rule1, patterns);
            Assert.AreEqual(lexer.NextToken(), "# grammar of the regression problem including ERCs");
            Assert.IsNull(lexer.NextToken());
            // Rule 2
            lexer = new GELexer(rule2, patterns);
            Assert.AreEqual(lexer.NextToken(), "<start>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.IsNull(lexer.NextToken());
            // Rule 3
            lexer = new GELexer(rule3, patterns);
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "+");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "-");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "*");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "%");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.IsNull(lexer.NextToken());
            // Rule 4
            lexer = new GELexer(rule4, patterns);
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "sin");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "cos");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "exp");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "rlog");
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.IsNull(lexer.NextToken());
            // Rule 5
            lexer = new GELexer(rule5, patterns);
            Assert.AreEqual(lexer.NextToken(), "<op>");
            Assert.AreEqual(lexer.NextToken(), "::=");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "x");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.AreEqual(lexer.NextToken(), "|");
            Assert.AreEqual(lexer.NextToken(), "(");
            Assert.AreEqual(lexer.NextToken(), "ERC");
            Assert.AreEqual(lexer.NextToken(), ")");
            Assert.IsNull(lexer.NextToken());
        }

        #endregion // Regression

        /// <summary>
        /// The default regular expressions for tokens in the parser.  If you'd
        /// like to change minor features of the regular expressions, override the
        /// GetRegexes() method in a subclass to return a different array.  Note that
        /// if you INSERT a new regular expression into the middle of these, the values
        /// of the various token constants ("LPAREN", "RULE", etc.) will be wrong, so you
        /// will need to override or modify the methods which use them.
        /// </summary>
        public static readonly string[] DEFAULT_REGEXES = 
        {
            "\\s*#[^\\n\\r]*",               // COMMENT: matches a #foo up to but not including the newline.  Should appear first.
            "\\s*\\(",                       // LPAREN: matches a (
            "\\s*\\)",                       // RPAREN: matches a )
            "\\s*<[^<>()\\s]*>",             // RULE: matches a rule of the form <foo>.  No <, >, (, ), |, or spaces may appear in foo
            "\\s*[|]",                       // PIPE: matches a |
            "\\s*::=",                       // EQUALS: matches a ::=
            "\\s*::=",                       // NUMERIC_CONSTANT: does nothing right now, so set to be identical to EQUALS.  Reserved for future use.
            "\\s*::=",                       // BOOLEAN_CONSTANT: does nothing right now, so set to be identical to EQUALS.  Reserved for future use.
            "\\s*::=",                       // STRING_CONSTANT: does nothing right now, so set to be identical to EQUALS.  Reserved for future use.
            "\\s*[^<>()|\\s]+",              // FUNCTION (must appear after RULE and PIPE): matches a rule of the form foo.  No <, >, (, ), |, or spaces may appear in foo, and foo must have at least one character.
        };

        #endregion // Real-World Checks

    }
}