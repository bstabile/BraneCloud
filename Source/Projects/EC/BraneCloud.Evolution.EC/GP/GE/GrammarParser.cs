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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.GP.GE
{
    [Serializable]
    [ECConfiguration("ec.gp.ge.GrammarParser")]
    public class GrammarParser : IPrototype
    {
        #region Constants

        public const string P_PARSER = "parser";

        protected const int COMMENT = 0;
        protected const int LPAREN = 1;
        protected const int RPAREN = 2;
        protected const int RULE = 3;
        protected const int PIPE = 4;
        protected const int EQUALS = 5;

        // the following three are reserved for future use
        protected const int NUMERIC_CONSTANT = 6;
        protected const int BOOLEAN_CONSTANT = 7;
        protected const int STRING_CONSTANT = 8;

        /// <summary>
        /// And now we continue with our regularly scheduled program.
        /// </summary>
        protected const int FUNCTION = 9;

        #endregion // Constants
        #region Static

        // NOTE: SEE BELOW FOR THE .NET VERSION OF DEFAULT_REGEXES
        ///// <summary>
        ///// The default regular expressions for tokens in the parser.  If you'd
        ///// like to change minor features of the regular expressions, override the
        ///// GetRegexes() method in a subclass to return a different array.  Note that
        ///// if you INSERT a new regular expression into the middle of these, the values
        ///// of the various token constants ("LPAREN", "RULE", etc.) will be wrong, so you
        ///// will need to override or modify the methods which use them.
        ///// </summary>
        //public static readonly string[] DEFAULT_REGEXES = 
        //{
        //    "\\p{Blank}*#[^\\n\\r]*",               // COMMENT: matches a #foo up to but not including the newline.  Should appear first.
        //    "\\p{Blank}*\\(",                       // LPAREN: matches a (
        //    "\\p{Blank}*\\)",                       // RPAREN: matches a )
        //    "\\p{Blank}*<[^<>()\\p{Space}]*>",      // RULE: matches a rule of the form <foo>.  No <, >, (, ), |, or spaces may appear in foo
        //    "\\p{Blank}*[|]",                       // PIPE: matches a |
        //    "\\p{Blank}*::=",                       // EQUALS: matches a ::=
        //    "\\p{Blank}*::=",                       // NUMERIC_CONSTANT: does nothing right now, so set to be identical to EQUALS.  Reserved for future use.
        //    "\\p{Blank}*::=",                       // BOOLEAN_CONSTANT: does nothing right now, so set to be identical to EQUALS.  Reserved for future use.
        //    "\\p{Blank}*::=",                       // STRING_CONSTANT: does nothing right now, so set to be identical to EQUALS.  Reserved for future use.
        //    "\\p{Blank}*[^<>()|\\p{Space}]+",       // FUNCTION (must appear after RULE and PIPE): matches a rule of the form foo.  No <, >, (, ), |, or spaces may appear in foo, and foo must have at least one character.
        //};

        /// <summary>
        /// The default regular expressions for tokens in the parser.  If you'd
        /// like to change minor features of the regular expressions, override the
        /// GetRegexes() method in a subclass to return a different array.  Note that
        /// if you INSERT a new regular expression into the middle of these, the values
        /// of the various token constants ("LPAREN", "RULE", etc.) will be wrong, so you
        /// will need to override or modify the methods which use them.
        /// </summary>
        /// <remarks>
        /// This is slightly different from the ECJ version, but not in any important way. 
        /// The only change is that the "\\p{Blank}" (unicode property) is replaced by "\\s" to match whitespace.
        /// After testing this together with the regex changes in GELexer, everything seems to tokenize just fine. 
        /// </remarks>
        public static readonly string[] DEFAULT_REGEXES = 
        {
            "\\s*#[^\\n\\r]*",        // COMMENT: matches a #foo up to but not including the newline.  Should appear first.
            "\\s*\\(",                // LPAREN: matches a (
            "\\s*\\)",                // RPAREN: matches a )
            "\\s*<[^<>()\\s]*>",      // RULE: matches a rule of the form <foo>.  No <, >, (, ), |, or spaces may appear in foo
            "\\s*[|]",                // PIPE: matches a |
            "\\s*::=",                // EQUALS: matches a ::=
            "\\s*::=",                // NUMERIC_CONSTANT: does nothing right now, so set to be identical to EQUALS.  Reserved for future use.
            "\\s*::=",                // BOOLEAN_CONSTANT: does nothing right now, so set to be identical to EQUALS.  Reserved for future use.
            "\\s*::=",                // STRING_CONSTANT: does nothing right now, so set to be identical to EQUALS.  Reserved for future use.
            "\\s*[^<>()|\\s]+",       // FUNCTION (must appear after RULE and PIPE): matches a rule of the form foo.  No <, >, (, ), |, or spaces may appear in foo, and foo must have at least one character.
        };

        #endregion // Static
        #region Fields

        /// <summary>
        /// The parsed rules, hashed by name.
        /// </summary>
        Hashtable _rules = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// The resulting parse graph.
        /// </summary>
        GrammarRuleNode _root = null;

        #endregion // Fields
        #region Properties

        public IParameter DefaultBase
        {
            get { return GEDefaults.ParamBase.Push(P_PARSER); }
        }

        /// <summary>
        /// Returns the regular expressions to use for tokenizing these rules.  By default DEFAULT_REGEXES are returned.
        /// </summary>
        public string[] DefaultRegexes
        {
            get { return DEFAULT_REGEXES; }
        }

        #endregion // Properties
        #region Setup

        public void Setup(IEvolutionState state, IParameter paramBase)
        {
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Returns a rule from the hashmap.  If one does not exist, creates a rule with the
        /// given head and stores, then returns that.
        /// </summary>
        static GrammarRuleNode GetRule(Hashtable rules, String head)
        {
            if (rules.ContainsKey(head))
                return (GrammarRuleNode)(rules[head]);
            else
            {
                var node = new GrammarRuleNode(head);
                rules[head] = node;
                return node;
            }
        }

        /// <summary>
        /// Parses a rule, one rule per line, from the lexer.  
        /// Adds to the existing hashmap if there's already a rule there.
        /// </summary>
        GrammarRuleNode ParseRule(IEvolutionState state, IGELexer lexer, GPFunctionSet gpfs)
        {
            GrammarRuleNode retResult = null;

            string token = lexer.NextToken();
            if (lexer.MatchingIndex == COMMENT) return null; //ignore the comment
            if (lexer.MatchingIndex == RULE) //rule head, good, as expected...
            {
                lexer.NextToken();
                if (lexer.MatchingIndex != EQUALS)
                {
                    state.Output.Fatal("GE Grammar Error: Expecting equal sign after rule head: " + token);
                }
                retResult = GetRule(_rules, token);
                ParseProductions(state, retResult, lexer, gpfs);
            }
            else
            {
                state.Output.Fatal("GE Grammar Error - Unexpected token: Expecting rule head.: " + token);
            }
            return retResult;
            // IMPLEMENTED
            // Need to parse the rule using a recursive descent parser
            // If there was an error, then try to call state.output.error(...).
            //
            // Don't merge into any existing rule -- I do that in parseRules below.  Instead, just pull out
            // rules and hang them into your "new rule" as necessary.  
            // Use getRule(rules, "<rulename>") to extract the rule representing the current rule name which you
            // can hang inside there as necessary. 
            //
            // If you have to you can call state.output.fatal(...) which will terminate the program,
            // but piling up some errors might be useful.  I'll handle the exitIfErors() in parseRules below
            //
            // Return null if there was no rule to parse (blank line or all comments) but no errors.
            // Also return null if you called state.output.error(...).
        }

        /// <summary>
        /// Parses each of a rule's production choices.
        /// </summary>
        void ParseProductions(IEvolutionState state, GrammarRuleNode retResult, IGELexer lexer, GPFunctionSet gpfs)
        {
            GrammarFunctionNode grammarfuncnode;
            do
            {
                var token = lexer.NextToken();
                if (lexer.MatchingIndex == RULE)
                {
                    retResult.AddChoice(GetRule(_rules, token));
                    token = lexer.NextToken();
                }
                else
                {
                    if (lexer.MatchingIndex != LPAREN) //first expect '('
                    {
                        state.Output.Fatal("GE Grammar Error - Unexpected token for rule: " + retResult.Head + "Expecting '('.");
                    }
                    token = lexer.NextToken();
                    if (lexer.MatchingIndex != FUNCTION) //now expecting function
                    {
                        state.Output.Fatal("GE Grammar Error - Expecting a function name after first '(' for rule: " + retResult.Head + " Error: " + token);
                    }
                    else
                    {
                        if (!(gpfs.NodesByName.ContainsKey(token)))
                        {
                            state.Output.Fatal("GPNode " + token + " is not defined in the function set.");
                        }
                        grammarfuncnode = new GrammarFunctionNode(gpfs, token);
                        token = lexer.NextToken();
                        while (lexer.MatchingIndex != RPAREN)
                        {
                            if (lexer.MatchingIndex != RULE) //this better be the name of a rule node
                            {
                                state.Output.Fatal("GE Grammar Error - Expecting a rule name as argument for function definition: " + grammarfuncnode.Head + " Error on : " + token);
                            }
                            grammarfuncnode.AddArgument(GetRule(_rules, token));
                            token = lexer.NextToken();
                        }
                        retResult.AddChoice(grammarfuncnode);
                    }
                    //after right paren, should see either '|' or newline
                    token = lexer.NextToken();
                    if (lexer.MatchingIndex != PIPE && lexer.MatchingIndex != Constants.GE_LEXER_FAILURE)
                    {
                        state.Output.Fatal("GE Grammar Error - Expecting either '|' delimiter or newline. Error on : " + token);
                    }
                }
            }
            while (lexer.MatchingIndex == PIPE);
        }

        /// <summary>
        /// Parses the rules from a grammar and returns the resulting GrammarRuleNode root.
        /// </summary>
        public GrammarRuleNode ParseRules(IEvolutionState state, TextReader reader, GPFunctionSet gpfs)
        {
            _rules = new Hashtable();
            try
            {
                String line;
                while ((line = reader.ReadLine()) != null)
                {
                    var rule = ParseRule(state, new GELexer(line.Trim(), DEFAULT_REGEXES), gpfs);
                    if (rule != null && _root == null) _root = rule;
                }
            }
            catch (IOException) { } // do nothing
            state.Output.ExitIfErrors();
            return _root;
        }

        /// <summary>
        /// Checks that all grammar rules in ruleshashmap have at least one possible production
        /// </summary>
        /// <returns>True if grammar rules are properly defined, false otherwise</returns>
        public bool ValidateRules()
        {
            var isok = true;
            var i = _rules.Values.GetEnumerator();
            while (i.MoveNext())
            {
                var rule = (GrammarRuleNode)(i.Current);
                if (rule.GetNumChoices() < 1)
                {
                    Console.WriteLine("Grammar is bad! - Rule not defined: " + rule);
                    isok = false;
                }
            }
            if (isok)
            {
                Console.WriteLine("All rules appear properly defined!");
                return true;
            }
            return false;
        }

        #endregion // Operations
        #region Cloning

        public Object Clone()
        {
            //var other = (GrammarParser) base.Clone();
            var other = new GrammarParser();
            other._rules = (Hashtable)_rules.Clone();
            // we'll pointer-copy the root
            return other;
        }

        #endregion // Cloning
        #region ToString

        public override string ToString()
        {
            var ret = "Grammar[";
            var i = _rules.Values.GetEnumerator();
            while (i.MoveNext())
            {
                ret = ret + "\n" + i.Current;
            }
            return ret + "\n\t]";
        }

        #endregion // ToString
        #region Testing

        /// <summary>
        /// A simple testing fcility.
        /// </summary>
        public static void Main(string[] args)
        {
            // make a dummy EvolutionState that just has an output for testing
            var state = new EvolutionState { Output = new Output(true) };
            state.Output.AddLog(Log.D_STDOUT, false);
            state.Output.AddLog(Log.D_STDERR, true);

            var gp = new GrammarParser();
            gp.ParseRules(state, new StreamReader(new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.Read)), null);
            gp.ValidateRules();
            Console.WriteLine(gp);
        }

        #endregion // Testing
    }
}