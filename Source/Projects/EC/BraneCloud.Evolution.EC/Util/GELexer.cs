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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BraneCloud.Evolution.EC.Util
{
    /// <summary>
    /// A simple line-by-line String tokenizer.  You provide Lexer with a String as input, 
    /// plus an array of regular expressions.  Each time you call nextToken(...), 
    /// the Lexer matches the next token against the regular expressions and returns it.  
    /// The regular expressions are checked in order, and the first one
    /// that matches is the winner.
    /// </summary>
    /// <remarks>
    /// This is a quick and dirty hack to make the GELexer 
    /// work in .NET just as it does in Java (hopefully).
    /// The problem is that .NET does not have the semantics of Java's "region" or "lookingAt()" regex extensions.
    /// Assuming that grammars are relatively small and simple, this should be adequate for now.
    /// Custom grammars should be tested thoroughly to ensure that all tokens are properly identified.
    /// If the matching behavior needs to be changed, consider using the constructor that accepts
    /// a <see cref="RegexOptions"/> argument, before going to the trouble of deriving a specialized
    /// class to meet your particular requirements.
    /// </remarks>
    public class GELexer : IGELexer
    {
        #region Constants

        /// <summary>
        /// This can be changed to alter the token-matching behavior of the lexer.
        /// </summary>
        public const RegexOptions DEFAULT_OPTIONS = RegexOptions.None;

        #endregion // Constants
        #region Fields

        private int _position = 0;

        #endregion // Fields
        #region Properties   *******************************************************

        /// <summary>
        /// This property is set in the appropriate constructor to control the behavior of the Regex class.
        /// For example:
        ///     <see cref="RegexOptions.Multiline"/>  (Anchors <b>^</b> and <b>$</b> apply to indivdual lines)
        ///     <see cref="RegexOptions.Singleline"/> (dot matches newline)
        /// If no options are specified, this will be set to <see cref="RegexOptions.Multiline"/>.
        /// This means that the anchors <b>^</b> and <b>$</b> match the beginning and end of each line,
        /// rather than the beginning and end of the entire input string (if it spans multiple lines).
        /// </summary>
        /// <remarks>
        /// At present, the lexer is used to tokenize individual rules in a grammar,
        /// one rule per line. If this changes so that rules can span multiple lines, the default
        /// options should be changed accordingly.
        /// </remarks>
        public RegexOptions Options { get; private set; }

        public string Input { get; private set; }
        public string[] Patterns { get; private set; }
        public Regex[] Matchers { get; private set; }

        public int MatchingPosition { get; private set; }
        public int MatchingIndex { get; private set; }
        public string MatchingRule { get; private set; }

        #endregion // Properties
        #region Setup   ************************************************************

        /// <summary>
        /// This constructor accepts the input, and an array of patterns that will be used to identify tokens. 
        /// Note that this constructor uses <i>DEFAULT_OPTIONS</i> for default token-matching behavior.
        /// </summary>
        /// <param name="input">The string that will be searched for tokens.</param>
        /// <param name="patterns">The regular expression patterns that will be used (in order) to identify tokens.</param>
        public GELexer(string input, string[] patterns)
        {
            Input = input;
            Patterns = patterns;
            Matchers = new Regex[Patterns.Length];
            Options = DEFAULT_OPTIONS;
            for (var i = 0; i < patterns.Length; i++)
                Matchers[i] = new Regex(Patterns[i], Options); // not DOTALL
        }

        /// <summary>
        /// This constructor adds a <see cref="RegexOptions"/> argument that can be used to control the matching behavior.
        /// </summary>
        /// <param name="input">The string that will be tokenized.</param>
        /// <param name="patterns">The array of regular expression patterns that will used to identify tokens.</param>
        /// <param name="regexOptions">Flags that will be used to control the behavior of token identification.</param>
        public GELexer(string input, string[] patterns, RegexOptions regexOptions)
        {
            Input = input;
            Patterns = patterns;
            Matchers = new Regex[Patterns.Length];
            Options = regexOptions;
            for (var i = 0; i < patterns.Length; i++)
                Matchers[i] = new Regex(Patterns[i], Options); // not DOTALL
        }

        #endregion // Setup
        #region Operations

        public string NextToken()
        {
            return NextToken(true);
        }

        public string NextToken(bool trim)
        {
            for (var i = 0; i < Patterns.Length; i++)
            {
                var tail = Input.Substring(_position);
                var match = Regex.Match(tail, Patterns[i]);

                // the following line is a HACK to make .NET regex behave 
                // as though it has Java's "region" and "lookingAt()" semantics
                if (!match.Success || !Input.Substring(_position).Trim().StartsWith(match.Value.Trim())) continue;

                MatchingIndex = i;
                MatchingRule = Patterns[i];
                MatchingPosition = _position + match.Index;
                _position += match.Index + match.Value.Length;
                return (trim ? match.Value.Trim() : match.Value);
            }
            // we failed
            MatchingIndex = -1;

            return null;
        }

        #endregion // Operations
    }
}