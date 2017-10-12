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
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Util
{
    /// <summary> 
    /// DecodeReturn is used by Code to provide varied information returned
    /// when decoding. 
    /// You start the decoding process by initializing the DecodeReturn
    /// on a string you want to decode items out of.  Then you repeatedly
    /// pass the DecodeReturn to Code.Decode(...), and each time the
    /// DecodeReturn will contain information about the next token, namely,
    /// its type, the data of the token (depending on type, this
    /// can be in one of three slots, B, C, D, L, or S), and the start position
    /// for reading the next token.
    /// 
    /// B = Boolean
    /// C = Char
    /// D = Double (or Float)
    /// L = Long (or SByte, Short, Int32)
    /// S = String
    /// 
    /// <p/>In case of an error, type is set to DecodeReturn.T_ERROR,
    /// pos is kept at the token where the error occured, and
    /// s is set to an error message.
    /// </summary>	
    [ECConfiguration("ec.util.DecodeReturn")]
    public class DecodeReturn
    {
        #region Constants

        /// <summary>
        /// The actual error is stored in the String slot. 
        /// </summary>
        public const sbyte T_ERROR = -1;
        
        public const sbyte T_BOOLEAN = 0;
        public const sbyte T_BYTE = 1;

        public const sbyte T_CHAR = 2;
        /// <summary>
        /// Same as T_CHAR. 
        /// </summary>
        public const sbyte T_CHARACTER = 2;

        public const sbyte T_SHORT = 3;

        public const sbyte T_INT = 4;

        /// <summary>
        /// Same as T_INT. 
        /// </summary>
        public const sbyte T_INTEGER = 4;

        public const sbyte T_LONG = 5;
        public const sbyte T_FLOAT = 6;
        public const sbyte T_DOUBLE = 7;
        public const sbyte T_STRING = 8;

        #endregion // Constants
        #region Properties

        /// <summary>
        /// The Line number, if it has been posted. 
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// The DecodeReturn type. 
        /// </summary>
        public sbyte Type { get; set; }

        /// <summary>
        /// The DecodeReturn string that's read from. 
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// The DecodeReturn new position in the string.  Set this yourself.
        /// New values get set here automatically. 
        /// </summary>
        public int Pos { get; set; }

        /// <summary>
        /// Stores boolean as a nullable value.
        /// </summary>
        public bool? B
        {
            get { return _b; }
            set { _b = value; }
        }
        private bool? _b = new bool?();

        /// <summary>
        /// Stores a <see cref="Char"/> as a nullable value.
        /// </summary>
        public char? C
        {
            get { return _c; }
            set { _c = value; }
        }
        private char? _c = new char?();

        /// <summary>
        /// Stores bytes, chars, shorts, ints, longs. 
        /// </summary>
        public long L { get; set; }

        /// <summary>
        /// Stores floats, doubles. 
        /// </summary>
        public double D { get; set; }

        /// <summary>
        /// Stores strings, error messages. 
        /// </summary>
        public string S { get; set; }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// Use this to make a new DecodeReturn starting at position 0. 
        /// </summary>
        public DecodeReturn(string data)
        {
            LineNumber = 0;
            Data = data; 
            Pos = 0;
        }
        
        /// <summary>
        /// Use this to make a new DecodeReturn starting at some position. 
        /// </summary>
        public DecodeReturn(string data, int pos)
        {
            LineNumber = 0;
            Data = data; 
            Pos = pos;
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Sets the DecodeReturn to begin scanning at _pos, which should be valid. 
        /// </summary>
        public virtual DecodeReturn ScanAt(int pos)
        {
            Pos = Math.Min(Math.Max(pos, 0), Data.Length);
            return this;
        }
        
        /// <summary>
        /// Use this to reuse your DecodeReturn for another string. 
        /// </summary>
        public virtual DecodeReturn Reset(string data)
        {
            Data = data; 
            Pos = 0; 
            return this;
        }
        
        /// <summary>
        /// Use this to reuse your DecodeReturn for another string. 
        /// </summary>
        public virtual DecodeReturn Reset(string data, int pos)
        {
            Data = data; 
            Pos = pos; 
            return this;
        }

        #endregion // Operations
    }
}