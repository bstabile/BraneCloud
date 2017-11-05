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

using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Util
{
    /// <summary> 
    /// Code provides some simple wrapper functions for encoding and decoding
    /// basic data types for storage in a pseudo-source code string format.  
    /// This differs from just "printing" them to string 
    /// in that the actual precision of the object is maintained.
    /// Code attempts to keep the representations as near to runtime values as possible --
    /// the exceptions being primarily floats and doubles, which are encoded as
    /// ints and longs.  Encoding of objects and arrays is not supported.  You'll
    /// have to handle that yourself.  Strings are supported.
    /// 
    /// <p/>Everything is case-SENSITIVE.  Here's the breakdown.
    /// 
    /// <p/><table>
    /// <tr><td><b>Type</b></td><td><b>Format</b></td></tr>
    /// <tr><td>boolean</td><td><tt>true</tt> or <tt>false</tt> (old style, case sensitive) or <tt>T</tt> or <tt>F</tt> (new style, case sensitive)</td></tr>
    /// <tr><td>byte</td><td><tt>b<i>byte</i>|</tt></td></tr>
    /// <tr><td>short</td><td><tt>s<i>short</i>|</tt></td></tr>
    /// <tr><td>int</td><td><tt>i<i>int</i>|</tt></td></tr>
    /// <tr><td>long</td><td><tt>l<i>long</i>|</tt></td></tr>
    /// <tr><td>float</td><td><tt>f<i>floatConvertedToIntForStorage</i>|<i>humanReadableFloat</i>|</tt> or (only for reading in) f|<i>humanReadableFloat</i>|</td></tr>
    /// <tr><td>float</td><td><tt>d<i>doubleConvertedToLongForStorage</i>|<i>humanReadableDouble</i>|</tt> or (only for reading in) d|<i>humanReadableDouble</i>|</td></tr>
    /// <tr><td>char</td><td>standard Java char, except that the only valid escape sequences are: \0 \t \n \b \' \" \ u <i>unicodeHex</i></td></tr>
    /// <tr><td>string</td><td>standard Java string with \ u ...\ u Unicode escapes, except that the only other valid escape sequences are: <i> \0 \t \n \b \' \" </i></td></tr>
    /// </table>
    /// </summary>
    [ECConfiguration("ec.util.Code")]
    public static class Code
    {
        /// <summary>
        /// Encodes a boolean. 
        /// </summary>
        public static string Encode(bool b)
        {
            return b ? "T" : "F";
        }
        
        /// <summary>
        /// Encodes a byte. 
        /// </summary>
        public static string Encode(sbyte b)
        {
            return "b" + b + "|";
        }
        
        /// <summary>
        /// Encodes a character. 
        /// </summary>
        public static string Encode(char c)
        {
            if (c >= 32 && c < 127 && c != '\\' && c != '\'' && c != '\"')
            // we can safely print it
                return "'" + Convert.ToString(c) + "'";

            // print it with an escape character
            switch (c)
            {
                case '\b':
                    return "'\\b'";
                case '\n':
                    return "'\\n'";
                case '\t':
                    return "'\\t'";
                case '\'':
                    return "'\\''";
                case '\"':
                    return "'\\\"'";
                case '\\':
                    return "'\\\\'";
                case '\x0000':
                    return "'\\0'";
                default:
                    {
                        var s = Convert.ToString((int) c, 16);
                        // pad with 0's
                        switch (s.Length)
                        {
                            case 1:  s = "'\\u000" + s + "'"; break;
                            case 2:  s = "'\\u00" + s + "'"; break;
                            case 3:  s = "'\\u0" + s + "'"; break;
                            case 4:  s = "'\\u" + s + "'"; break;
                            default:
                                throw new InvalidOperationException("Default case should never occur");
                        }
                        return s;
                    }
            }
        }

        /// <summary>
        /// Encodes a short. 
        /// </summary>
        public static string Encode(short s)
        {
            return "s" + s + "|";
        }
        
        /// <summary>
        /// Encodes an int. 
        /// </summary>
        public static string Encode(int i)
        {
            return "i" + Convert.ToString(i) + "|";
        }
        
        /// <summary>
        /// Encodes a long. 
        /// </summary>
        public static string Encode(long l)
        {
            return "l" + Convert.ToString(l) + "|";
        }
        
        /// <summary>
        /// Encodes a float. Note that the human-readable roundtrip format is specified: i.e. ToString("R").
        /// </summary>
        /// <remarks>
        /// Unfortunately, to get this to work correctly,
        /// I had to remove the cast to Int32 after converting to Int64 bits.
        /// The corresponding "Decode" logic was changed for the same reason.
        /// This just makes the "representation" a bit less compact.
        /// </remarks>
        public static string Encode(float f)
        {
            return "f" + BitConverter.DoubleToInt64Bits(f) + "|" + f.ToString("R") + "|";
        }
        
        /// <summary>
        /// Encodes a double. Note that the human-readable roundtrip format is specified: i.e. ToString("R").
        /// </summary>
        public static string Encode(double d)
        {
            return "d" + BitConverter.DoubleToInt64Bits(d) + "|" + d.ToString("R") + "|";
        }
        
        /// <summary>
        /// Encodes a String. 
        /// </summary>
        public static string Encode(string s)
        {
            var inUnicode = false;
            var l = s.Length;
            var sb = new StringBuilder(l);
            sb.Append("\"");
            for (var x = 0; x < l; x++)
            {
                var c = s[x];
                if (c >= 32 && c < 127 && c != '\\' && c != '"') // we allow spaces
                // we can safely print it
                {
                    if (inUnicode)
                    {
                        sb.Append("\\u"); 
                        inUnicode = false;
                    }
                    sb.Append(c);
                }
                else
                {
                    // print it with an escape character
                    if (c == '\b')
                    {
                        if (inUnicode)
                        {
                            sb.Append("\\u"); 
                            inUnicode = false;
                        }
                        sb.Append("\\b");
                    }
                    else if (c == '\n')
                    {
                        if (inUnicode)
                        {
                            sb.Append("\\u"); 
                            inUnicode = false;
                        }
                        sb.Append("\\n");
                    }
                    else if (c == '\t')
                    {
                        if (inUnicode)
                        {
                            sb.Append("\\u"); 
                            inUnicode = false;
                        }
                        sb.Append("\\t");
                    }
                    else if (c == '"')
                    {
                        if (inUnicode)
                        {
                            sb.Append("\\u"); 
                            inUnicode = false;
                        }
                        sb.Append("\\\"");
                    }
                    else if (c == '\\')
                    {
                        if (inUnicode)
                        {
                            sb.Append("\\u"); 
                            inUnicode = false;
                        }
                        sb.Append("\\\\");
                    }
                    else if (c == '\x0000')
                    {
                        if (inUnicode)
                        {
                            sb.Append("\\u"); 
                            inUnicode = false;
                        }
                        sb.Append("\\0");
                    }
                    else
                    {
                        if (!inUnicode)
                        {
                            sb.Append("\\u"); 
                            inUnicode = true;
                        }
                        var ss = Convert.ToString((int) c, 16);
                        // pad with 0's  -- parser might freak out otherwise (ecj)
                        switch (ss.Length)
                        {
                            case 1:  sb.Append("000" + ss); break;
                            case 2:  sb.Append("00" + ss); break;
                            case 3:  sb.Append("0" + ss); break;
                            case 4:  sb.Append(ss); break;
                            default:
                                throw new InvalidOperationException("Default case should never occur");
                        }
                    }
                }
            }
            if (inUnicode)
                sb.Append("\\u");
            sb.Append("\"");
            return sb.ToString();
        }
        
        /// <summary>
        /// Decodes the next item out of a DecodeReturn and modifies the DecodeReturn to hold the results.  
        /// See DecodeReturn for more explanations about how to interpret the results. 
        /// </summary>
        public static void Decode(DecodeReturn d)
        {
            var dat = d.Data;
            var x = d.Pos;
            var len = d.Data.Length;
            
            // look for whitespace or ( or )
            for (; x < len; x++)
                if (!Char.IsWhiteSpace(dat[x]) && dat[x] != '(' && dat[x] != ')')
                    break;
            
            // am I at the end of my rope?
            if (x == len)
            {
                d.Type = DecodeReturn.T_ERROR; d.S = "Out of tokens"; 
                return ;
            }
            
            // what type am I?
            switch (dat[x])
            {
                case 't': // boolean (true)
                    if (x + 3 < len && dat[x + 1] == 'r' && dat[x + 2] == 'u' && dat[x + 3] == 'e')
                    {
                        d.Type = DecodeReturn.T_BOOLEAN;
                        d.B = true;
                        d.Pos = x + 4;
                        return;
                    }
                    else
                    {
                        d.Type = DecodeReturn.T_ERROR;
                        d.S = "Expected a (true) boolean";
                        return;
                    }
                case 'T': // boolean (true)
                    {
                        d.Type = DecodeReturn.T_BOOLEAN;
                        d.B = true;
                        d.Pos = x + 1;
                        return;
                    }
                case 'F': // boolean (false)
                    {
                        d.Type = DecodeReturn.T_BOOLEAN;
                        d.B = false;
                        d.Pos = x + 1;
                        return;
                    }
                case 'f': // float or boolean
                    if ( len == 1 || Char.IsWhiteSpace(dat[x+1]) || dat[x+1] == ')')  // Then we must have a lowercase boolean ("false")?
                    {
                        d.Type = DecodeReturn.T_BOOLEAN;
                        d.B = false;
                        d.Pos = x + 1;
                        return;
                    }
                    if (x + 4 < len && dat[x + 1] == 'a' && dat[x + 2] == 'l' && dat[x + 3] == 's' && dat[x + 4] == 'e')
                    {
                        d.Type = DecodeReturn.T_BOOLEAN; d.B = false; d.Pos = x + 5; 
                        return ;
                    }
                    else
                    {
                        var readHuman = false;
                        string sf = null;
                        var initial = x + 1;
                        
                        // look for next '|'
                        for (; x < len; x++)
                            if (dat[x] == '|')
                                break;
                        
                        if (x == initial)
                            readHuman = true;
                        
                        if (x >= len)
                        {
                            d.Type = DecodeReturn.T_ERROR; d.S = "Expected a float"; 
                            return ;
                        }
                        
                        if (!readHuman)
                            sf = dat.Substring(initial, (x) - (initial));
                        x++;
                        
                        // look for next '|'
                        var initial2 = x; // x is now just past first |
                        for (; x < len; x++)
                            if (dat[x] == '|')
                                break;
                        
                        if (x >= len)
                        {
                            d.Type = DecodeReturn.T_ERROR; d.S = "Expected a float"; 
                            return ;
                        }
                        if (readHuman)
                            sf = dat.Substring(initial2, (x) - (initial2));
                        
                        float f;
                        try
                        {
                            if (readHuman)
                                f = Single.Parse(sf);
                            else
                            {
                                f = (float)BitConverter.Int64BitsToDouble(Int64.Parse(sf));
                            }
                        }
                        catch (FormatException)
                        {
                            d.Type = DecodeReturn.T_ERROR; d.S = "Expected a float"; 
                            return ;
                        }
                        d.Type = DecodeReturn.T_FLOAT;
                        d.D = f;
                        d.Pos = x + 1;
                        return ;
                    }
                case 'd':  // double
                    {
                        var readHuman = false;
                        string sf = null;
                        var initial = x + 1;
                        
                        // look for next '|'
                        for (; x < len; x++)
                            if (dat[x] == '|')
                                break;
                        
                        if (x == initial)
                            readHuman = true;
                        
                        if (x >= len)
                        {
                            d.Type = DecodeReturn.T_ERROR; d.S = "Expected a double"; 
                            return ;
                        }
                        
                        if (!readHuman)
                            sf = dat.Substring(initial, (x) - (initial));
                        x++;
                        
                        // look for next '|'
                        var initial2 = x; // x is now just past first |
                        for (; x < len; x++)
                            if (dat[x] == '|')
                                break;
                        
                        if (x >= len)
                        {
                            d.Type = DecodeReturn.T_ERROR; d.S = "Expected a double"; 
                            return ;
                        }
                        if (readHuman)
                            sf = dat.Substring(initial2, (x) - (initial2));
                        
                        double f;
                        try
                        {
                            if (readHuman)
                                f = Double.Parse(sf);
                            else
                            {
                                f = BitConverter.Int64BitsToDouble(Int64.Parse(sf));
                            }
                        }
                        catch (FormatException)
                        {
                            d.Type = DecodeReturn.T_ERROR; d.S = "Expected a double"; 
                            return ;
                        }
                        d.Type = DecodeReturn.T_DOUBLE;
                        d.D = f;
                        d.Pos = x + 1;
                        return ;
                    }
                case 'b':  // byte
                    {
                        var initial = x + 1;
                        
                        // look for next '|'
                        for (; x < len; x++)
                            if (dat[x] == '|')
                                break;
                        
                        if (x >= len)
                        {
                            d.Type = DecodeReturn.T_ERROR; d.S = "Expected a byte"; 
                            return ;
                        }
                        var sf = dat.Substring(initial, (x) - (initial));
                        
                        sbyte f;
                        try
                        {
                            f = SByte.Parse(sf);
                        }
                        catch (FormatException)
                        {
                            d.Type = DecodeReturn.T_ERROR; d.S = "Expected a byte"; 
                            return ;
                        }
                        d.Type = DecodeReturn.T_BYTE;
                        d.L = f;
                        d.Pos = x + 1;
                        return ;
                    }
                case 's':  // short
                    {
                        var initial = x + 1;
                        
                        // look for next '|'
                        for (; x < len; x++)
                            if (dat[x] == '|')
                                break;
                        
                        if (x >= len)
                        {
                            d.Type = DecodeReturn.T_ERROR; d.S = "Expected a short"; 
                            return ;
                        }
                        var sf = dat.Substring(initial, (x) - (initial));
                        
                        short f;
                        try
                        {
                            f = Int16.Parse(sf);
                        }
                        catch (FormatException)
                        {
                            d.Type = DecodeReturn.T_ERROR; d.S = "Expected a short"; 
                            return ;
                        }
                        d.Type = DecodeReturn.T_SHORT;
                        d.L = f;
                        d.Pos = x + 1;
                        return ;
                    }
                case 'i':  // int
                    {
                        var initial = x + 1;
                        
                        // look for next '|'
                        for (; x < len; x++)
                            if (dat[x] == '|')
                                break;
                        
                        if (x >= len)
                        {
                            d.Type = DecodeReturn.T_ERROR; d.S = "Expected an int"; 
                            return ;
                        }
                        var sf = dat.Substring(initial, (x) - (initial));
                        
                        int f;
                        try
                        {
                            f = int.Parse(sf);
                        }
                        catch (FormatException)
                        {
                            d.Type = DecodeReturn.T_ERROR; d.S = "Expected an int"; 
                            return ;
                        }
                        d.Type = DecodeReturn.T_INT;
                        d.L = f;
                        d.Pos = x + 1;
                        return ;
                    }
                case 'l':  // long
                    {
                        var initial = x + 1;
                        
                        // look for next '|'
                        for (; x < len; x++)
                            if (dat[x] == '|')
                                break;
                        
                        if (x >= len)
                        {
                            d.Type = DecodeReturn.T_ERROR; d.S = "Expected a long"; 
                            return ;
                        }
                        var sf = dat.Substring(initial, (x) - (initial));
                        
                        long f;
                        try
                        {
                            f = Int64.Parse(sf);
                        }
                        catch (FormatException)
                        {
                            d.Type = DecodeReturn.T_ERROR; d.S = "Expected a long"; 
                            return ;
                        }
                        d.Type = DecodeReturn.T_LONG;
                        d.L = f;
                        d.Pos = x + 1;
                        return ;
                    }
                case '"':  // string
                    {
                        var sb = new StringBuilder();
                        var inUnicode = false;
                        
                        x++;
                        for (; x < len; x++)
                        {
                            var c = dat[x];
                            if (c == '"')
                            {
                                // done with the string
                                if (inUnicode)
                                // uh oh
                                {
                                    d.Type = DecodeReturn.T_ERROR; d.S = "Forgot to terminate Unicode with a '\\u' in the string"; 
                                    return ;
                                }
                                d.Type = DecodeReturn.T_STRING;
                                d.S = sb.ToString();
                                d.Pos = x + 1;
                                return ;
                            }
                            else if (c == '\\') // escape
                            {
                                x++;
                                if (x >= len)
                                {
                                    d.Type = DecodeReturn.T_ERROR; d.S = "Unterminated String"; 
                                    return ;
                                }
                                if (dat[x] != 'u' && inUnicode)
                                {
                                    d.Type = DecodeReturn.T_ERROR; d.S = "Escape character in Unicode sequence"; 
                                    return ;
                                }
                                
                                switch (dat[x])
                                {
                                    case 'u'  :  inUnicode = !inUnicode; break;
                                    case 'b'  :  sb.Append('\b'); break;
                                    case 'n'  :  sb.Append('\n'); break;
                                    case '"'  :  sb.Append('"'); break;
                                    case '\'' :  sb.Append('\''); break;
                                    case 't'  :  sb.Append('\t'); break;									
                                    case '\\' :  sb.Append('\\'); break;
                                    case '0'  :  sb.Append('\x0000'); break;
                                    default: 
                                    {
                                        d.Type = DecodeReturn.T_ERROR; d.S = "Bad escape char in String"; 
                                        return ;
                                    }
                                }
                            }
                            else if (inUnicode)
                            {
                                if (x + 3 >= len)
                                {
                                    d.Type = DecodeReturn.T_ERROR; d.S = "Unterminated String"; 
                                    return ;
                                }
                                try
                                {
                                    sb.Append((char) (int.Parse("0x" + c + dat[x + 1] + dat[x + 2] + dat[x + 3])));
                                    ;
                                    x += 3;
                                }
                                catch (System.FormatException)
                                {
                                    d.Type = DecodeReturn.T_ERROR; d.S = "Bad Unicode in String"; 
                                    return ;
                                }
                            }
                            else
                                sb.Append(c);
                        }
                        d.Type = DecodeReturn.T_ERROR; d.S = "Unterminated String"; 
                        return ;
                    }
                case '\'':  // char
                    {
                        x++;
                        if (x >= len)
                        {
                            d.Type = DecodeReturn.T_ERROR; d.S = "Unterminated char"; return ;
                        }
                        var c = dat[x];
                        if (c == '\\')
                        {
                            x++;
                            if (x >= len)
                            {
                                d.Type = DecodeReturn.T_ERROR; d.S = "Unterminated char"; 
                                return ;
                            }
                            switch (dat[x])
                            {
                                case 'u': 
                                    if (x + 4 >= len)
                                    {
                                        d.Type = DecodeReturn.T_ERROR; d.S = "Unterminated char"; 
                                        return ;
                                    }
                                    try
                                    {
                                        c = (char) (int.Parse("0x" + dat[x + 1] + dat[x + 2] + dat[x + 3] + dat[x + 4]));
                                    }
                                    catch (System.FormatException)
                                    {
                                        d.Type = DecodeReturn.T_ERROR; d.S = "Bad Unicode in char"; 
                                        return ;
                                    }
                                    x += 5;
                                    break;
                                case 'b'  :  c = '\b'; x++; break;
                                case 'n'  :  c = '\n'; x++; break;
                                case '"'  :  c = '"'; x++; break;
                                case '\'' :  c = '\''; x++; break;
                                case 't'  :  c = '\t'; x++; break;
                                case '\\' :  c = '\\'; x++; break;
                                case '0'  :  c = '\x0000'; x++; break;
                                default: 
                                {
                                    d.Type = DecodeReturn.T_ERROR; d.S = "Bad escape char in char"; 
                                    return ;
                                }
                                
                            }
                            if (dat[x] != '\'')
                            {
                                d.Type = DecodeReturn.T_ERROR; d.S = "Bad char"; 
                                return ;
                            }
                            d.Type = DecodeReturn.T_CHAR;
                            d.C = c;
                            d.Pos = x + 1;
                            return ;
                        }
                        else
                        {
                            x++;
                            if (x >= len)
                            {
                                d.Type = DecodeReturn.T_ERROR; d.S = "Unterminated char"; 
                                return ;
                            }
                            if (dat[x] != '\'')
                            {
                                d.Type = DecodeReturn.T_ERROR; d.S = "Bad char"; 
                                return ;
                            }
                            d.Type = DecodeReturn.T_CHAR;
                            d.C = c;
                            d.Pos = x + 1;
                            return ;
                        }
                    }				
                default: 
                    d.Type = DecodeReturn.T_ERROR; d.S = "Unknown token"; 
                    return ;
            }
        }
        
        /// <summary>
        /// Finds the next nonblank line, then trims the line and checks the preamble.  
        /// Returns a DecodeReturn on the line if successful, else posts a fatal error.
        /// Sets the DecodeReturn's line number.  The DecodeReturn has not yet been decoded.  
        /// You'll need to do that with Code.Decode(...) 
        /// </summary>
        public static DecodeReturn CheckPreamble(string preamble, IEvolutionState state, StreamReader reader)
        {
            return CheckPreamble(preamble, state, reader, false); // multiline = false
            //// BRS : TODO : Added Offset because the reader does not provide "linenumber". Check the actual behavior.
            //var offset = reader.BaseStream.Position;
            //var linenumber = 0; // throw it away later
            //try
            //{
            //    // get non-blank line
            //    var s = "";
            //    while (s != null && s.Trim().Equals(""))
            //    {
            //        //linenumber = reader.GetLineNumber();
            //        s = reader.ReadLine();
            //        linenumber++;
            //    }

            //    // check the preamble
            //    if (s == null || !(s = s.Trim()).StartsWith(preamble))
            //    // uh oh
            //        state.Output.Fatal("Stream Offset: " + offset + " Line: " + linenumber + " has bad preamble.  Expected '" + preamble + "'. -->" + s);
            //    var d = new DecodeReturn(s, preamble.Length) {LineNumber = linenumber};
            //    return d;
            //}
            //catch (IOException e)
            //{
            //    state.Output.Fatal("On line " + linenumber + "an IO error occurred:\n\n" + e);
            //    return null; // never happens
            //}
        }

        /// <summary>
        /// Finds the next nonblank line, then trims the line and checks the preamble.  
        /// Returns a DecodeReturn on the line if successful, else posts a fatal error.
        /// Sets the DecodeReturn's line number.  The DecodeReturn has not yet been decoded.  
        /// You'll need to do that with Code.Decode(...) 
        /// If the "multiline" argument is true, this will append additional lines 
        /// up to the first one that is null or empty (blank). 
        /// Note that this multiline capability provides the means to reconstitute large blocks of data if needed. 
        /// The user must ensure that a blank line (or EOF) marks the end of such blocks.
        /// </summary>
        public static DecodeReturn CheckPreamble(string preamble, IEvolutionState state, StreamReader reader, bool multiline)
        {
            // BRS : TODO : Added Offset because the reader does not provide "linenumber". Check the actual behavior.
            var offset = reader.BaseStream.Position;
            var linenumber = 0; // throw it away later
            try
            {
                // get non-blank line
                var s = "";
                while (s != null && s.Trim().Equals(""))
                {
                    //linenumber = reader.GetLineNumber();
                    s = reader.ReadLine();
                    linenumber++;
                }

                // check the preamble
                if (s == null || !(s = s.Trim()).StartsWith(preamble))
                    // uh oh
                    state.Output.Fatal("Stream Offset: " + offset + " Line: " + linenumber + " has bad preamble.  Expected '" + preamble + "'. -->" + s);

                // At this point we are exactly the same as the single line version
                if (!multiline)
                {
                    var d = new DecodeReturn(s, preamble.Length) {LineNumber = linenumber};
                    return d;
                }
                // multiline has been requested...
                var sb = new StringBuilder(s); // Aggregate lines up to the first blank line following the preamble
                while (!String.IsNullOrEmpty(s = reader.ReadLine()))
                {
                    sb = sb.AppendLine(s);
                }
                return new DecodeReturn(sb.ToString(), preamble.Length) {LineNumber = linenumber};
            }
            catch (IOException e)
            {
                state.Output.Fatal("On line " + linenumber + "an IO error occurred:\n\n" + e);
                return null; // never happens
            }
        }

        /** Finds the next nonblank line, skips past an expected preamble, and reads in a string if there is one, and returns it.
            Generates an error otherwise. */
        public static string ReadStringWithPreamble(string preamble, IEvolutionState state, StreamReader reader)
        {
            DecodeReturn d = CheckPreamble(preamble, state, reader);
            Decode(d);
            if (d.Type != DecodeReturn.T_STRING)
                state.Output.Fatal("Line " + d.LineNumber +
                                   " has no string after preamble '" + preamble + "'\n-->" + d.Data);
            return d.S;
        }

        /// <summary>
        /// Finds the next nonblank line, skips past an expected preamble, 
        /// and reads in an sbyte value if there is one, and returns it.
        /// Generates an error otherwise. 
        /// </summary>
        public static sbyte ReadByteWithPreamble(string preamble, IEvolutionState state, StreamReader reader)
        {
            var d = CheckPreamble(preamble, state, reader);
            Decode(d);
            if (d.Type != DecodeReturn.T_BYTE)
                state.Output.Fatal("Line " + d.LineNumber + "has no sbyte after preamble '" + preamble + "'. -->" + d.Data);
            return (sbyte)d.L;
        }

        /// <summary>
        /// Finds the next nonblank line, skips past an expected preamble, 
        /// and reads in an short value if there is one, and returns it.
        /// Generates an error otherwise. 
        /// </summary>
        public static short ReadShortWithPreamble(string preamble, IEvolutionState state, StreamReader reader)
        {
            var d = CheckPreamble(preamble, state, reader);
            Decode(d);
            if (d.Type != DecodeReturn.T_SHORT)
                state.Output.Fatal("Line " + d.LineNumber + "has no short value after preamble '" + preamble + "'. -->" + d.Data);
            return (short)d.L;
        }

        /// <summary>
        /// Finds the next nonblank line, skips past an expected preamble, 
        /// and reads in an integer value if there is one, and returns it.
        /// Generates an error otherwise. 
        /// </summary>
        public static int ReadIntWithPreamble(string preamble, IEvolutionState state, StreamReader reader)
        {
            var d = CheckPreamble(preamble, state, reader);
            Decode(d);
            if (d.Type != DecodeReturn.T_INT)
                state.Output.Fatal("Line " + d.LineNumber + "has no Int32 after preamble '" + preamble + "'. -->" + d.Data);
            return (int) (d.L);
        }

        /// <summary>
        /// Finds the next nonblank line, skips past an expected preamble, 
        /// and reads in a long value if there is one, and returns it.
        /// Generates an error otherwise. 
        /// </summary>
        public static long ReadLongWithPreamble(string preamble, IEvolutionState state, StreamReader reader)
        {
            var d = CheckPreamble(preamble, state, reader);
            Decode(d);
            if (d.Type != DecodeReturn.T_LONG)
                state.Output.Fatal("Line " + d.LineNumber + "has no Int64 after preamble '" + preamble + "'. -->" + d.Data);
            return d.L;
        }
                
        /// <summary>
        /// Finds the next nonblank line, skips past an expected preamble, 
        /// and reads in a float value if there is one, and returns it. 
        /// Generates an error otherwise. 
        /// </summary>
        public static float ReadFloatWithPreamble(string preamble, IEvolutionState state, StreamReader reader)
        {
            var d = CheckPreamble(preamble, state, reader);
            Decode(d);
            if (d.Type != DecodeReturn.T_FLOAT)
                state.Output.Fatal("Line " + d.LineNumber + "has no floating point number after preamble '" + preamble + "'. -->" + d.Data);
            //UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions.
            return (float) (d.D);
        }
        
        /// <summary>
        /// Finds the next nonblank line, skips past an expected preamble, 
        /// and reads in a double value if there is one, and returns it. 
        /// Generates an error otherwise. 
        /// </summary>
        public static double ReadDoubleWithPreamble(string preamble, IEvolutionState state, StreamReader reader)
        {
            var d = CheckPreamble(preamble, state, reader);
            Decode(d);
            if (d.Type != DecodeReturn.T_DOUBLE)
                state.Output.Fatal("Line " + d.LineNumber + "has no double floating point number after preamble '" + preamble + "'. -->" + d.Data);
            return d.D;
        }
        
        /// <summary>
        /// Finds the next nonblank line, skips past an expected preamble, 
        /// and reads in a boolean value ("true" or "false") if there is one, and returns it. 
        /// Generates an error otherwise. 
        /// </summary>
        public static bool ReadBooleanWithPreamble(string preamble, IEvolutionState state, StreamReader reader)
        {
            var d = CheckPreamble(preamble, state, reader);
            Decode(d);
            if (d.Type != DecodeReturn.T_BOOLEAN || !d.B.HasValue)
                state.Output.Fatal("Line " + d.LineNumber + "has no boolean value ('true' or 'false') after preamble '" + preamble + "'. -->" + d.Data);
            return d.B.Value;
        }

        /// <summary>
        /// Finds the next nonblank line, skips past an expected preamble, 
        /// and reads in a character value (eg. "'a'" or "'\\n'") if there is one, and returns it. 
        /// Generates an error otherwise. 
        /// </summary>
        public static char ReadCharacterWithPreamble(string preamble, IEvolutionState state, StreamReader reader)
        {
            var d = CheckPreamble(preamble, state, reader);
            Decode(d);
            if (d.Type != DecodeReturn.T_CHAR || !d.C.HasValue)
            {
                state.Output.Fatal("Line " + d.LineNumber + "has no char value after preamble '" + preamble + "'. -->" +
                                   d.Data);
                return ' ';
            }
            return d.C.Value;
        }
    }
       
    /*
    (BeanShell testing for decoding)
    
    s = "      true false s-12| i232342|b22|f234123|3234.1| d234111231|4342.31|"
    
    
    s = "\"Hello\" true false s-12| i232342|b22|f234123|3234.1| d234111231|4342.31| ' ' '\\'' '\\n' \"Hello\\u0000\\uWorld\""
    c = new ec.util.Code();
    r = new ec.util.DecodeReturn(s);
    
    c.decode(r);
    System.out.PrintLn(r.Type);
    System.out.PrintLn(r.l);
    
    System.out.PrintLn(r.d);
    System.out.PrintLn(r.s);
    */
}