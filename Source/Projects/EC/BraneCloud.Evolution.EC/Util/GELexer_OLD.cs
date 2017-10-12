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
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Util
{
    ///// <summary>
    ///// A simple line-by-line String tokenizer.  You provide Lexer with a String or other
    ///// CharSequence as input, plus an array of regular expressions.  Each time you call
    ///// nextToken(...), the Lexer matches the next token against the regular expressions
    ///// and returns it.  The regular expressions are checked in order, and the first one
    ///// that matches is the winner.
    ///// </summary>
    //[ECConfiguration("ec.util.GELexer")]
    //public class GELexer_OLD : IGELexer
    //{
    //    /// <summary>
    //    /// An index which indicates that no further tokens were found.  
    //    /// This could be due to the end of the string or due to a bad string.  
    //    /// You'll need to check the index to determine for sure.
    //    /// </summary>
    //    public const int FAILURE = -1;
    //    string _input;
    //    int _position = 0;
    //    Regex[] _matchers;
    //    string[] _regexps;
    //    int _matchingIndex = FAILURE;

    //    /// <summary>
    //    /// Builds a Lexer for the given input with the provided regular expressions.  The regular expressions
    //    /// will be checked in order against the input, and the first one which matches will be assumed to be the token.
    //    /// </summary>
    //    public GELexer_OLD(/* CharSequence */ string input, string[] regexps)
    //{
    //        _regexps = regexps;
    //        _matchers = new Regex[regexps.Length];
    //        for (var i = 0; i < regexps.Length; i++)
    //            _matchers[i] = new Regex(regexps[i]); // not DOTALL
    //        _input = input;
    //}

    //    /// <summary>
    //    /// Returns the next token as a string.  If <b>trim</b> is true, then the string is first trimmed of whitespace.
    //    /// </summary>
    //    public String NextToken(bool trim)
    //    {
           
    //        //for (var i = 0; i < _regexps.Length; i++)
    //        //{
    //        //    if (!_matchers[i].Region(_position, _input.Length).lookingAt()) continue;
    //        //    _position = _matchers[i].End;
    //        //    _matchingIndex = i;
    //        //    return (trim ? _matchers[i].Group.trim() : _matchers[i].Group());
    //        //}
    //        //// we failed
    //        //_matchingIndex = -1;
    //        return null; 
    //    }

    //    /// <summary>
    //    /// Returns the next token as a string.  The string is first trimmed of whitespace.
    //    /// </summary>
    //    public String NextToken() { return NextToken(true); }

    //    /// <summary>
    //    /// Returns the index of the regular expression which matched the most recent token.
    //    /// </summary>
    //    public int MatchingIndex
    //    {
    //        get { return _matchingIndex; }
    //    }

    //    /// <summary>
    //    /// Returns the regular expression which matched the most recent token.
    //    /// </summary>
    //    public string MatchingRule
    //    {
    //        get
    //        {
    //            if (_matchingIndex == -1) return null;
    //            return _regexps[_matchingIndex];
    //        }
    //    }

    //    /// <summary>
    //    /// Returns the position in the String just beyond the most recent token.
    //    /// </summary>
    //    public int MatchingPosition
    //    {
    //        get { return _position; }
    //    }
    //}
}