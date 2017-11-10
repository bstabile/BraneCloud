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
            "\\s*#[^\\n\\r]*", // COMMENT: matches a #foo up to but not including the newline.  Should appear first.
            "\\s*\\(", // LPAREN: matches a (
            "\\s*\\)", // RPAREN: matches a )
            "\\s*<[^<>()\\s]*>", // RULE: matches a rule of the form <foo>.  No <, >, (, ), |, or spaces may appear in foo
            "\\s*[|]", // PIPE: matches a |
            "\\s*::=", // EQUALS: matches a ::=
            "\\s*::=", // NUMERIC_CONSTANT: does nothing right now, so set to be identical to EQUALS.  Reserved for future use.
            "\\s*::=", // BOOLEAN_CONSTANT: does nothing right now, so set to be identical to EQUALS.  Reserved for future use.
            "\\s*::=", // STRING_CONSTANT: does nothing right now, so set to be identical to EQUALS.  Reserved for future use.
            "\\s*[^<>()|\\s]+", // FUNCTION (must appear after RULE and PIPE): matches a rule of the form foo.  No <, >, (, ), |, or spaces may appear in foo, and foo must have at least one character.
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

        /** 
         * Lots of stuffs to enumerate/analyze the grammar tree, 
         * these are needed to generate the predictive parse table.
         */
        // The list of production rules after flattenning the grammar tree
        ArrayList productionRuleList = new ArrayList();

        // Assign integer index to each of the rules, starting from 0
        Hashtable indexToRule = new Hashtable();

        // Reverse map of the above Hashtable indexToRule
        Hashtable ruleToIndex = new Hashtable();

        // Function heads' (i.e. terminals') indices
        Hashtable functionHeadToIndex = new Hashtable();

        // Rule heads' (i.e. non-terminals') indices
        Hashtable ruleHeadToIndex = new Hashtable();

        // Absolute production rule indices to relative indices (w.r.t. sub-rules)
        Hashtable absIndexToRelIndex = new Hashtable();

        /** 
         * The hash-map for the so called FIRST-SET, FOLLOW-SET and PREDICT-SET 
         * for each of the production rules. 
         */
        Hashtable ruleToFirstSet = new Hashtable();

        Hashtable ruleToFollowSet = new Hashtable();
        Hashtable ruleToPredictSet = new Hashtable();

        /** 
         * The predictive parse table to parse the lisp tree, 
         * this is what we are looking for.
         */
        int[][] predictiveParseTable;

        #endregion // Fields

        #region Properties

        public IParameter DefaultBase => GEDefaults.ParamBase.Push(P_PARSER);

        /// <summary>
        /// Returns the regular expressions to use for tokenizing these rules.  By default DEFAULT_REGEXES are returned.
        /// </summary>
        public string[] DefaultRegexes => DEFAULT_REGEXES;

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
        GrammarRuleNode GetRule(Hashtable rules, String head)
        {
            if (rules.ContainsKey(head))
                return (GrammarRuleNode) rules[head];
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
                        state.Output.Fatal("GE Grammar Error - Unexpected token for rule: " + retResult.Head +
                                           "Expecting '('.");
                    }
                    token = lexer.NextToken();
                    if (lexer.MatchingIndex != FUNCTION) //now expecting function
                    {
                        state.Output.Fatal("GE Grammar Error - Expecting a function name after first '(' for rule: " +
                                           retResult.Head + " Error: " + token);
                    }
                    else
                    {
                        if (!gpfs.NodesByName.ContainsKey(token))
                        {
                            state.Output.Fatal("GPNode " + token + " is not defined in the function set.");
                        }
                        var grammarfuncnode = new GrammarFunctionNode(gpfs, token);
                        token = lexer.NextToken();
                        while (lexer.MatchingIndex != RPAREN)
                        {
                            if (lexer.MatchingIndex != RULE) //this better be the name of a rule node
                            {
                                state.Output.Fatal(
                                    "GE Grammar Error - Expecting a rule name as argument for function definition: "
                                    + grammarfuncnode.Head + " Error on : " + token);
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
                        state.Output.Fatal("GE Grammar Error - Expecting either '|' delimiter or newline. Error on : " +
                                           token);
                    }
                }
            } while (lexer.MatchingIndex == PIPE);
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
                    GrammarRuleNode rule = ParseRule(state, new GELexer(line.Trim(), DEFAULT_REGEXES), gpfs);
                    if (rule != null && _root == null) _root = rule;
                }
            }
            catch (IOException)
            {
            } // do nothing
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
                var rule = (GrammarRuleNode) (i.Current);
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

        /**
         * Run BFS to enumerate the whole grammar tree into all necessary
         * indices lists/hash-maps, we *need* to run BFS because the decoding of 
         * the "GE array to tree" works in a BFS fashion, so we need to stick with that;
         * After enumeration, we will have four data-structures like these --
         *
         * (1) productionRuleList (a flattened grammar tree):
         *      grammar-tree ==> {rule-0, rule-1, ,,, rule-(n-1)}
         *
         * (2) ruleToIndex:
         *      rule-0 --> 0
         *      rule-1 --> 1
         *      ,
         *      ,
         *      rule-(n-1) --> (n-1)
         *
         * (3) indexToRule (reverse of ruleToIndex):
         *      0 --> rule-0
         *      1 --> rule-1
         *      ,
         *      ,
         *      n-1 --> rule-(n-1)
         *
         * and then, last but not the least, the relative rule index --
         * (4) absIndexToRelIndex: 
         *      if we have two rules like "<A> -> <B> | <C>" and "<C> -> <D> | <E>" then,
         *              [rule]          [absIndex]      [relIndex] 
         *              <A> -> <B> -->  [0]     -->     [0]
         *              <A> -> <C> -->  [1]     -->     [1] 
         *              <C> -> <D> -->  [2]     -->     [0]
         *              <C> -> <E> -->  [3]     -->     [1] etc,
         */
        public void EnumerateGrammarTree(GrammarNode gn)
        {
            throw new NotImplementedException();
            /*
            // The BFS queue
            Queue q = new LinkedList<>();
            int gnIndex = 0;
            int fIndex = 0;
            int rIndex = 0;
            ruleHeadToIndex.Put(gn.GetHead(), rIndex++);
            q.Add(gn);
            while (!q.IsEmpty())
            {
                GrammarNode temp = (GrammarNode) q.Remove();
                for (int i = 0; i < temp.Children.Count; i++)
                {
                    GrammarRuleNode grn = new GrammarRuleNode(temp.Head);
                    GrammarNode child = ((GrammarRuleNode) temp).GetChoice(i);
                    grn.Children.Add(child);
                    productionRuleList.Add(grn);
                    indexToRule.Put(gnIndex, grn);
                    ruleToIndex.Put(grn, gnIndex);
                    gnIndex++;
                    if (child is GrammarRuleNode)
                    {
                        ruleHeadToIndex.Put(child.GetHead(), rIndex++);
                        q.add(child);
                    }
                    else if (child is GrammarFunctionNode)
                        functionHeadToIndex.Put(child.GetHead(), fIndex++);
                }
            }
            // Now to the absolute index to relative index mapping
            String oldHead = ((GrammarNode) indexToRule.Get(Integer.valueOf(0))).GetHead();
            absIndexToRelIndex.Put(new Integer(0), new Integer(0));
            for (int absIndex = 1, relIndex = 1; absIndex < indexToRule.size(); absIndex++)
            {
                String currentHead = ((GrammarNode) indexToRule.Get(new Integer(absIndex))).GetHead();
                if (!currentHead.Equals(oldHead))
                    relIndex = 0;
                absIndexToRelIndex.Put(new Integer(absIndex), new Integer(relIndex++));
                oldHead = currentHead;
            }
            */
        }

/**
 * Generate the FIRST-SET for each production rule and store them in the
 * global hash-table, this runs a DFS on the grammar tree, the returned ArrayList
 * is discarded and the FIRST-SETs are organized in a hash-map called 
 * "ruleToFirstSet" as follows -- 
 *
 *      rule-0 --> {FIRST-SET-0}
 *      rule-1 --> {FIRST-SET-1}
 *      ,
 *      ,
 *      rule-(n-1) --> {FIRST-SET-(n-1)}
 */
        public ArrayList GatherFirstSets(GrammarNode gn, GrammarNode parent)
        {
            throw new NotImplementedException();
            /*
            ArrayList firstSet = new ArrayList();
            if (gn is GrammarRuleNode)
            {
                for (int i = 0; i < ((GrammarRuleNode) gn).GetNumChoices(); i++)
                {
                    ArrayList set =
                        GatherFirstSets(((GrammarRuleNode) gn).GetChoice(i), gn);
                    firstSet.AddAll(set);
                }
                if (parent != null)
                {
                    GrammarNode treeEdge = new GrammarRuleNode(parent.GetHead());
                    treeEdge.Children.Add(gn);
                    ruleToFirstSet.Put(treeEdge, firstSet);
                }
            }
            else if (gn is GrammarFunctionNode)
            {
                firstSet.Add(gn.GetHead());
                GrammarNode treeEdge = new GrammarRuleNode(parent.GetHead());
                treeEdge.Children.Add(gn);
                ruleToFirstSet.Put(treeEdge, firstSet);
            }
            return firstSet;
            */
        }

        /**
         * We do not have any example grammar to test with FOLLOW-SETs,
         * so the FOLLOW-SET is empty, we need to test with a grammar 
         * that contains post-fix notations;
         *
         * this needs to be implemented properly with a new grammar.
         */
        public ArrayList GatherFollowSets(GrammarNode gn, GrammarNode parent)
        {
            ArrayList followSet = new ArrayList();
            return followSet;
        }

        /** 
         * Populate the PREDICT-SET from the FIRST-SETs and the FOLLOW-SETs, 
         * as we do not have FOLLOW-SET, so FIRST-SET == PREDICT-SET;
         * 
         * this needs to be implemented, when the FOLLOW-SETs are done properly.
         */
        public void GatherPredictSets(GrammarNode gn, GrammarNode parent)
        {
            throw new NotImplementedException();
            /*
            // gather FIRST-SET
            GatherFirstSets(gn, null);
            // gather FOLLOW-SET
            GatherFollowSets(gn, null);
            // then, gather PREDICT-SET
            if (ruleToFollowSet.IsEmpty())
            {
                ruleToPredictSet = (Hashtable) ruleToFirstSet.Clone();
            }
            else
            {
                ; // not implemented yet
            }
            */
        }

        /**
         * Now populate the predictive-parse table, this procedure reads
         * hash-maps/tables for the grammar-rule indices, PREDICT-SETs etc, 
         * and assigns the corresponding values in the predictive-parse table. 
         */
        public void PopulatePredictiveParseTable(GrammarNode gn)
        {
            throw new NotImplementedException();
            /*
            // calculate the predict sets
            GatherPredictSets(gn, null);
            // now make the predictive parse table
            predictiveParseTable = TensorFactory.Create<int>(ruleHeadToIndex.Count, functionHeadToIndex.Count);
            Iterator it = ruleToPredictSet.EntrySet().iterator();
            while (it.HasNext())
            {
                Map.Entry pairs = (Map.Entry) it.Next();
                GrammarNode action = (GrammarNode) pairs.getKey();
                String ruleHead = action.GetHead();
                int ruleIndex = ((Integer) ruleHeadToIndex.Get(ruleHead)).intValue();
                ArrayList functionHeads = (ArrayList) pairs.getValue();
                for (int i = 0; i < functionHeads.Count; i++)
                {
                    String functionHead = (String) functionHeads.Get(i);
                    int functionHeadIndex = ((Integer) functionHeadToIndex.Get(functionHead)).intValue();
                    predictiveParseTable[ruleIndex][functionHeadIndex]
                        = ((Integer) ruleToIndex.Get(action)).intValue();
                }
            }
            */
        }

        #endregion // Operations

        #region Cloning

        public Object Clone()
        {
            try
            {
                GrammarParser other = (GrammarParser) base.MemberwiseClone();
                other._rules = (Hashtable) _rules.Clone();
                // we'll pointer-copy the root
                return other;
            }
            catch (CloneNotSupportedException e)
            {
                return null; // never happens
            }
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
            var state = new EvolutionState {Output = new Output(true)};
            state.Output.AddLog(Log.D_STDOUT, false);
            state.Output.AddLog(Log.D_STDERR, true);

            var gp = new GrammarParser();
            gp.ParseRules(state,
                new StreamReader(new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.Read)), null);
            gp.ValidateRules();
            Console.WriteLine(gp);
        }

        #endregion // Testing
    }
}