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

namespace BraneCloud.Evolution.EC.Support
{
    /// <summary>
    /// The class performs token processing in strings
    /// </summary>
    public class Tokenizer : IEnumerator
    {
        /// Position over the string
        private long _currentPos;

        /// Include demiliters in the results.
        private readonly bool _includeDelims;

        /// Char representation of the String to tokenize.
        private readonly char[] _chars;

        //The tokenizer uses the default delimiter set: the space character, the tab character, the newline character, and the carriage-return character and the form-feed character
        private string _delimiters = " \t\n\r\f";

        /// <summary>
        /// Initializes a new class instance with a specified string to process
        /// </summary>
        /// <param name="source">String to tokenize</param>
        public Tokenizer(string source)
        {
            _chars = source.ToCharArray();
        }

        /// <summary>
        /// Initializes a new class instance with a specified string to process
        /// and the specified token delimiters to use
        /// </summary>
        /// <param name="source">String to tokenize</param>
        /// <param name="delimiters">String containing the delimiters</param>
        public Tokenizer(string source, string delimiters)
            : this(source)
        {
            _delimiters = delimiters;
        }


        /// <summary>
        /// Initializes a new class instance with a specified string to process, the specified token 
        /// delimiters to use, and whether the delimiters must be included in the results.
        /// </summary>
        /// <param name="source">String to tokenize</param>
        /// <param name="delimiters">String containing the delimiters</param>
        /// <param name="includeDelims">Determines if delimiters are included in the results.</param>
        public Tokenizer(string source, string delimiters, bool includeDelims)
            : this(source, delimiters)
        {
            _includeDelims = includeDelims;
        }


        /// <summary>
        /// Returns the next token from the token list
        /// </summary>
        /// <returns>The string value of the token</returns>
        public string NextToken()
        {
            return NextToken(_delimiters);
        }

        /// <summary>
        /// Returns the next token from the source string, using the provided
        /// token delimiters
        /// </summary>
        /// <param name="delimiters">String containing the delimiters to use</param>
        /// <returns>The string value of the token</returns>
        public string NextToken(string delimiters)
        {
            //According to documentation, the usage of the received delimiters should be temporary (only for this call).
            //However, it seems it is not true, so the following line is necessary.
            _delimiters = delimiters;

            //at the end 
            if (_currentPos == _chars.Length)
                throw new ArgumentOutOfRangeException();

            //if over a delimiter and delimiters must be returned
            if ((Array.IndexOf(_delimiters.ToCharArray(), _chars[_currentPos]) != -1) && _includeDelims)
                return "" + _chars[_currentPos++];

            //need to get the token wo delimiters.
            return nextToken(_delimiters.ToCharArray());
        }

        //Returns the nextToken wo delimiters
        private string nextToken(char[] delimiters)
        {
            var token = "";
            var pos = _currentPos;

            //skip possible delimiters
            while (Array.IndexOf(delimiters, _chars[_currentPos]) != -1)
                //The last one is a delimiter (i.e there is no more tokens)
                if (++_currentPos == _chars.Length)
                {
                    _currentPos = pos;
                    throw new ArgumentOutOfRangeException();
                }

            //getting the token
            while (Array.IndexOf(delimiters, _chars[_currentPos]) == -1)
            {
                token += _chars[_currentPos];
                //the last one is not a delimiter
                if (++_currentPos == _chars.Length)
                    break;
            }
            return token;
        }


        /// <summary>
        /// Determines if there are more tokens to return from the source string
        /// </summary>
        /// <returns>True or false, depending if there are more tokens</returns>
        public bool HasMoreTokens()
        {
            //keeping the current pos
            var pos = _currentPos;

            try
            {
                NextToken();
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
            finally
            {
                _currentPos = pos;
            }
            return true;
        }

        /// <summary>
        /// Remaining tokens count
        /// </summary>
        public int Count
        {
            get
            {
                //keeping the current pos
                var pos = _currentPos;
                var i = 0;

                try
                {
                    while (true)
                    {
                        NextToken();
                        i++;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    _currentPos = pos;
                    return i;
                }
            }
        }

        /// <summary>
        ///  Performs the same action as NextToken.
        /// </summary>
        public object Current
        {
            get
            {
                return NextToken();
            }
        }

        /// <summary>
        //  Performs the same action as HasMoreTokens.
        /// </summary>
        /// <returns>True or false, depending if there are more tokens</returns>
        public bool MoveNext()
        {
            return HasMoreTokens();
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Reset()
        {
            ;
        }
    }
}