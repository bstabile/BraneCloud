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
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BraneCloud.Evolution.EC.Configuration
{
    /// <summary>
    /// The <code>Properties</code> class represents a persistent set of
    /// properties. The <code>Properties</code> can be saved to a stream
    /// or loaded from a stream. Each key and its corresponding value in
    /// the property list is a string.
    /// <p/>
    /// A property list can contain another property list as its
    /// "defaults"; this second property list is searched if
    /// the property key is not found in the original property list.
    /// <p/>
    /// Because <code>Properties</code> inherits from <code>Hashtable</code>, the
    /// <code>put</code> and <code>putAll</code> methods can be applied to a
    /// <code>Properties</code> object. Their use is strongly discouraged as they
    /// allow the caller to insert entries whose keys or values are not
    /// <code>Strings</code>. The <code>setProperty</code> method should be used
    /// instead. If the <code>store</code> or <code>save</code> method is called
    /// on a "compromised" <code>Properties</code> object that contains a
    /// non-<code>String</code> key or value, the call will fail.
    /// <p/>
    /// <a name="encoding"></a>
    /// <p/> The {@link #load load} and {@link #store store} methods load and store
    /// properties in a simple line-oriented format specified below. This format
    /// uses the ISO 8859-1 character encoding. Characters that cannot be directly
    /// represented in this encoding can be written using
    /// <a HREF="http://java.sun.com/docs/books/jls/html/3.doc.html#100850">Unicode escapes</a>
    /// ; only a single 'u' character is allowed in an escape
    /// sequence. The native2ascii tool can be used to convert property files to and
    /// from other character encodings.
    /// <p/>
    /// <p/> The {@link #loadFromXML(InputStream)} and {@link
    /// #storeToXML(OutputStream, String, String)} methods load and store properties
    /// in a simple XML format. By default the UTF-8 character encoding is used,
    /// however a specific encoding may be specified if required. An XML properties
    /// document has the following DOCTYPE declaration:
    ///
    /// <pre>
    /// &lt;!DOCTYPE properties SYSTEM "http://java.sun.com/dtd/properties.dtd"&gt;
    /// </pre>
    /// Note that the system URI (http://java.sun.com/dtd/properties.dtd) is
    /// <i>not</i> accessed when exporting or importing properties; it merely
    /// serves as a string to uniquely identify the DTD, which is:
    /// <pre>
    /// &lt;?xml version="1.0" encoding="UTF-8"?&gt;
    ///
    /// &lt;!-- DTD for properties --&gt;
    ///
    /// &lt;!ELEMENT properties ( comment?, entry* ) &gt;
    ///
    /// &lt;!ATTLIST properties version CDATA #FIXED "1.0"&gt;
    ///
    /// &lt;!ELEMENT comment (#PCDATA) &gt;
    ///
    /// &lt;!ELEMENT entry (#PCDATA) &gt;
    ///
    /// &lt;!ATTLIST entry key CDATA #REQUIRED&gt;
    /// </pre>
    /// 
    /// @see <a HREF="../../../tooldocs/solaris/native2ascii.html">native2ascii tool for Solaris</a>
    /// @see <a HREF="../../../tooldocs/windows/native2ascii.html">native2ascii tool for Windows</a>
    /// @author Arthur van Hoff
    /// @author Michael McCloskey
    /// @version 1.84, 05/18/04
    /// @since JDK1.0
    /// </summary>
    public class PropertiesClass : Hashtable // Dictionary<Object, Object> // .NET does not have a generic Hashtable.
    {
        #region Constants

        /// <summary>
        /// use serialVersionUID from JDK 1.1.X for interoperability
        /// </summary>
        private const long SerialVersionUid = 4112578634029874840L;

        #endregion // Constants
        #region Static

        /// <summary>
        /// Convert a nibble to a hex character.
        /// </summary>
        /// <param name="nibble">The nibble to convert.</param>
        /// <returns></returns>
        private static char ToHex(int nibble)
        {
            return HexDigit[(nibble & 0xF)];
        }

        /// <summary>
        /// A table of hex digits
        /// </summary>
        private static readonly char[] HexDigit = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        /// <summary>
        /// Converts encoded &#92;uxxxx to unicode chars
        /// and changes special saved chars to their original forms
        /// </summary>
        /// <param name="inBuf"></param>
        /// <param name="off"></param>
        /// <param name="len"></param>
        /// <param name="convtBuf"></param>
        /// <returns></returns>
        private static String LoadConvert(char[] inBuf, int off, int len, char[] convtBuf)
        {
            if (convtBuf.Length < len)
            {
                var newLen = len * 2;
                if (newLen < 0)
                {
                    newLen = Int32.MaxValue;
                }
                convtBuf = new char[newLen];
            }
            var outBuf = convtBuf;
            var outLen = 0;
            var end = off + len;

            while (off < end)
            {
                var aChar = inBuf[off++];
                if (aChar == '\\')
                {
                    aChar = inBuf[off++];
                    if (aChar == 'u')
                    {
                        // Read the xxxx
                        var value = 0;
                        for (var i = 0; i < 4; i++)
                        {
                            aChar = inBuf[off++];
                            switch (aChar)
                            {
                                case '0':
                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                case '8':
                                case '9':
                                    value = (value << 4) + aChar - '0';
                                    break;
                                case 'a':
                                case 'b':
                                case 'c':
                                case 'd':
                                case 'e':
                                case 'f':
                                    value = (value << 4) + 10 + aChar - 'a';
                                    break;
                                case 'A':
                                case 'B':
                                case 'C':
                                case 'D':
                                case 'E':
                                case 'F':
                                    value = (value << 4) + 10 + aChar - 'A';
                                    break;
                                default:
                                    throw new ArgumentException("Malformed \\uxxxx encoding.");
                            }
                        }
                        outBuf[outLen++] = (char)value;
                    }
                    else
                    {
                        if (aChar == 't') aChar = '\t';
                        else if (aChar == 'r') aChar = '\r';
                        else if (aChar == 'n') aChar = '\n';
                        else if (aChar == 'f') aChar = '\f';
                        outBuf[outLen++] = aChar;
                    }
                }
                else
                {
                    outBuf[outLen++] = aChar;
                }
            }
            return new String(outBuf, 0, outLen);
        }

        /// <summary>
        /// Converts unicodes to encoded &#92;uxxxx and escapes
        /// special characters with a preceding slash
        /// </summary>
        /// <param name="theString"></param>
        /// <param name="escapeSpace"></param>
        /// <returns></returns>
        private static String SaveConvert(String theString, bool escapeSpace)
        {
            var len = theString.Length;
            var bufLen = len * 2;
            if (bufLen < 0)
            {
                bufLen = Int32.MaxValue;
            }
            var outBuffer = new StringBuilder(bufLen);

            for (var x = 0; x < len; x++)
            {
                char aChar = theString[x];
                // Handle common case first, selecting largest block that
                // avoids the specials below
                if ((aChar > 61) && (aChar < 127))
                {
                    if (aChar == '\\')
                    {
                        outBuffer.Append('\\'); outBuffer.Append('\\');
                        continue;
                    }
                    outBuffer.Append(aChar);
                    continue;
                }
                switch (aChar)
                {
                    case ' ':
                        if (x == 0 || escapeSpace)
                            outBuffer.Append('\\');
                        outBuffer.Append(' ');
                        break;
                    case '\t':
                        outBuffer.Append('\\');
                        outBuffer.Append('t');
                        break;
                    case '\n':
                        outBuffer.Append('\\');
                        outBuffer.Append('n');
                        break;
                    case '\r':
                        outBuffer.Append('\\');
                        outBuffer.Append('r');
                        break;
                    case '\f':
                        outBuffer.Append('\\');
                        outBuffer.Append('f');
                        break;
                    case '=': // Fall through
                    case ':': // Fall through
                    case '#': // Fall through
                    case '!':
                        outBuffer.Append('\\');
                        outBuffer.Append(aChar);
                        break;
                    default:
                        if ((aChar < 0x0020) || (aChar > 0x007e))
                        {
                            outBuffer.Append('\\');
                            outBuffer.Append('u');
                            outBuffer.Append(ToHex((aChar >> 12) & 0xF));
                            outBuffer.Append(ToHex((aChar >> 8) & 0xF));
                            outBuffer.Append(ToHex((aChar >> 4) & 0xF));
                            outBuffer.Append(ToHex(aChar & 0xF));
                        }
                        else
                        {
                            outBuffer.Append(aChar);
                        }
                        break;
                }
            }
            return outBuffer.ToString();
        }

        private static void WriteLn(StreamWriter bw, String s) // throws IOException  
        {
            bw.Write(s);
            bw.WriteLine();
        }

        #endregion // Static
        #region Properties

        /// <summary>
        /// A property list that contains default values for any keys not found in this property list.
        /// </summary>
        protected PropertiesClass Defaults { get; set; }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// Creates an empty property list with no default values.
        /// </summary>
        public PropertiesClass()
        {
        }

        /// <summary>
        /// Creates an empty property list with the specified defaults.
        /// </summary>
        /// <param name="defaults"></param>
        public PropertiesClass(PropertiesClass defaults)
        {
            this.Defaults = defaults;
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Calls the <tt>Hashtable</tt> method <code>put</code>. Provided for
        /// parallelism with the <tt>getProperty</tt> method. Enforces use of
        /// strings for property keys and values. The value returned is the
        /// result of the <tt>Hashtable</tt> call to <code>put</code>.
        /// </summary>
        /// <param name="key">the key to be placed into this property list.</param>
        /// <param name="value">the value corresponding to <tt>key</tt>.</param>
        /// <returns>the previous value of the specified key in this property list, or <code>null</code> if it did not have one.</returns>
        public object SetProperty(String key, String value)
        {
            lock (this)
            {
                {
                    return (this[key] = value);
                }
            }
        }

        /// <summary>
        /// Searches for the property with the specified key in this property list.
        /// If the key is not found in this property list, the default property list,
        /// and its defaults, recursively, are then checked. The method returns
        /// <code>null</code> if the property is not found.
        /// </summary>
        /// <param name="key">the property key.</param>
        /// <returns>the value in this property list with the specified key value.</returns>
        public String GetProperty(String key)
        {
            var oval = this[key];
            var sval = (oval is String) ? ((String)oval).Trim() : null;
            return ((sval == null) && (Defaults != null)) ? Defaults.GetProperty(key).Trim() : sval;
        }

        /// <summary>
        /// Searches for the property with the specified key in this property list.
        /// If the key is not found in this property list, the default property list,
        /// and its defaults, recursively, are then checked. The method returns the
        /// default value argument if the property is not found.
        /// </summary>
        /// <param name="key">the hashtable key.</param>
        /// <param name="defaultValue">a default value.</param>
        /// <returns>the value in this property list with the specified key value.</returns>
        public String GetProperty(String key, String defaultValue)
        {
            var val = GetProperty(key);
            return (val == null) ? defaultValue : val;
        }

        /// <summary>
        /// Returns an enumeration of all the keys in this property list,
        /// including distinct keys in the default property list if a key
        /// of the same name has not already been found from the main
        /// properties list.
        /// </summary>
        /// <returns>
        /// An enumeration of all the keys in this property list, including
        /// the keys in the default property list.
        /// </returns>
        public IEnumerable PropertyNames()
        {
            var h = new Hashtable();
            Enumerate(h);
            return h.Keys;
        }

        /// <summary>
        /// Reads a property list (key and element pairs) from the input
        /// stream. The stream is assumed to be using the ISO 8859-1
        /// character encoding; that is each byte is one Latin1 character.
        /// Characters not in Latin1, and certain special characters, can
        /// be represented in keys and elements using escape sequences
        /// similar to those used for character and string literals (see <a
        /// HREF="http://java.sun.com/docs/books/jls/second_edition/html/lexical.doc.html#100850">&sect;3.3</a>
        /// and <a
        /// HREF="http://java.sun.com/docs/books/jls/second_edition/html/lexical.doc.html#101089">&sect;3.10.6</a>
        /// of the <i>Java Language Specification</i>).
        /// 
        /// The differences from the character escape sequences used for
        /// characters and strings are:
        /// 
        /// <ul>
        /// <li/> Octal escapes are not recognized.
        /// 
        /// <li/> The character sequence <code>\b</code> does <i>not</i>
        /// represent a backspace character.
        /// 
        /// <li/> The method does not treat a backslash character,
        /// <code>\</code>, before a non-valid escape character as an
        /// error; the backslash is silently dropped. For example, in a
        /// Java string the sequence <code>"\z"</code> would cause a
        /// compile time error. In contrast, this method silently drops
        /// the backslash. Therefore, this method treats the two character
        /// sequence <code>"\b"</code> as equivalent to the single
        /// character <code>'b'</code>.
        /// 
        /// <li/> Escapes are not necessary for single and double quotes;
        /// however, by the rule above, single and double quote characters
        /// preceded by a backslash still yield single and double quote
        /// characters, respectively.
        /// 
        /// </ul>
        /// 
        /// An <code>IllegalArgumentException</code> is thrown if a
        /// malformed Unicode escape appears in the input.
        /// 
        /// <p/>
        /// This method processes input in terms of lines. A natural line
        /// of input is terminated either by a set of line terminator
        /// characters (<code>\n</code> or <code>\r</code> or
        /// <code>\r\n</code>) or by the end of the file. A natural line
        /// may be either a blank line, a comment line, or hold some part
        /// of a key-element pair. The logical line holding all the data
        /// for a key-element pair may be spread out across several adjacent
        /// natural lines by escaping the line terminator sequence with a
        /// backslash character, <code>\</code>. Note that a comment line
        /// cannot be extended in this manner; every natural line that is a
        /// comment must have its own comment indicator, as described
        /// below. If a logical line is continued over several natural
        /// lines, the continuation lines receive further processing, also
        /// described below. Lines are read from the input stream until
        /// end of file is reached.
        /// 
        /// <p/>
        /// A natural line that contains only white space characters is
        /// considered blank and is ignored. A comment line has an ASCII
        /// <code>'#'</code> or <code>'!'</code> as its first non-white
        /// space character; comment lines are also ignored and do not
        /// encode key-element information. In addition to line
        /// terminators, this method considers the characters space
        /// (<code>' '</code>, <code>'&#92;u0020'</code>), tab
        /// (<code>'\t'</code>, <code>'&#92;u0009'</code>), and form feed
        /// (<code>'\f'</code>, <code>'&#92;u000C'</code>) to be white
        /// space.
        /// 
        /// <p/>
        /// If a logical line is spread across several natural lines, the
        /// backslash escaping the line terminator sequence, the line
        /// terminator sequence, and any white space at the start the
        /// following line have no affect on the key or element values.
        /// The remainder of the discussion of key and element parsing will
        /// assume all the characters constituting the key and element
        /// appear on a single natural line after line continuation
        /// characters have been removed. Note that it is <i>not</i>
        /// sufficient to only examine the character preceding a line
        /// terminator sequence to see if the line terminator is
        /// escaped; there must be an odd number of contiguous backslashes
        /// for the line terminator to be escaped. Since the input is
        /// processed from left to right, a non-zero even number of
        /// 2<i>n</i> contiguous backslashes before a line terminator (or
        /// elsewhere) encodes <i>n</i> backslashes after escape
        /// processing.
        /// 
        /// <p/>
        /// The key contains all of the characters in the line starting
        /// with the first non-white space character and up to, but not
        /// including, the first unescaped <code>'='</code>,
        /// <code>':'</code>, or white space character other than a line
        /// terminator. All of these key termination characters may be
        /// included in the key by escaping them with a preceding backslash
        /// character; for example,
        /// <p/>
        /// <code>\:\=</code>
        /// <p/>
        /// would be the two-character key <code>":="</code>. Line
        /// terminator characters can be included using <code>\r</code> and
        /// <code>\n</code> escape sequences. Any white space after the
        /// key is skipped; if the first non-white space character after
        /// the key is <code>'='</code> or <code>':'</code>, then it is
        /// ignored and any white space characters after it are also
        /// skipped. All remaining characters on the line become part of
        /// the associated element string; if there are no remaining
        /// characters, the element is the empty string
        /// <code>&quot;&quot;</code>. Once the raw character sequences
        /// constituting the key and element are identified, escape
        /// processing is performed as described above.
        /// 
        /// <p/>
        /// As an example, each of the following three lines specifies the key
        /// <code>"Truth"</code> and the associated element value
        /// <code>"Beauty"</code>:
        /// <p/>
        /// <pre>
        /// Truth = Beauty
        /// Truth:Beauty
        /// Truth :Beauty
        /// </pre>
        /// As another example, the following three lines specify a single
        /// property:
        /// <p/>
        /// <pre>
        /// fruits apple, banana, pear, \
        /// cantaloupe, watermelon, \
        /// kiwi, mango
        /// </pre>
        /// The key is <code>"fruits"</code> and the associated element is:
        /// <p/>
        /// <pre>"apple, banana, pear, cantaloupe, watermelon, kiwi, mango"</pre>
        /// Note that a space appears before each <code>\</code> so that a space
        /// will appear after each comma in the final result; the <code>\</code>,
        /// line terminator, and leading white space on the continuation line are
        /// merely discarded and are <i>not</i> replaced by one or more other
        /// characters.
        /// <p/>
        /// As a third example, the line:
        /// <p/>
        /// <pre>cheeses
        /// </pre>
        /// specifies that the key is <code>"cheeses"</code> and the associated
        /// element is the empty string <code>""</code>.
        /// <p/>
        /// Throws IOException if an error occurred when reading from the input stream.
        /// Throws IllegalArgumentException if the input stream contains a malformed Unicode escape sequence.
        /// </summary>
        /// <param name="inStream">the input stream.</param>
        public void Load(Stream inStream) // throws IOException  
        {
            lock (this)
            {
                var convtBuf = new char[1024];
                var lr = new LineReader(inStream);

                int limit;

                while ((limit = lr.readLine()) >= 0)
                {
                    var c = (char)0;
                    var keyLen = 0;
                    var valueStart = limit;
                    var hasSep = false;

                    //Trace.WriteLine("line=<" + new String(lr.lineBuf, 0, limit ) + ">");
                    var precedingBackslash = false;
                    while (keyLen < limit)
                    {
                        c = lr.lineBuf[keyLen];
                        //need check if escaped.
                        if ((c == '=' || c == ':') && !precedingBackslash)
                        {
                            valueStart = keyLen + 1;
                            hasSep = true;
                            break;
                        }
                        else if ((c == ' ' || c == '\t' || c == '\f') && !precedingBackslash)
                        {
                            valueStart = keyLen + 1;
                            break;
                        }
                        if (c == '\\')
                        {
                            precedingBackslash = !precedingBackslash;
                        }
                        else
                        {
                            precedingBackslash = false;
                        }
                        keyLen++;
                    }
                    while (valueStart < limit)
                    {
                        c = lr.lineBuf[valueStart];
                        if (c != ' ' && c != '\t' && c != '\f')
                        {
                            if (!hasSep && (c == '=' || c == ':'))
                            {
                                hasSep = true;
                            }
                            else
                            {
                                break;
                            }
                        }
                        valueStart++;
                    }
                    var key = LoadConvert(lr.lineBuf, 0, keyLen, convtBuf);
                    var value = LoadConvert(lr.lineBuf, valueStart, limit - valueStart, convtBuf);
                    this[key] = value;
                }
            }
        }

        /// <summary>
        /// Writes this property list (key and element pairs) in this
        /// <code>Properties</code> table to the output stream in a format suitable
        /// for loading into a <code>Properties</code> table using the
        /// {@link #load(InputStream) load} method.
        /// The stream is written using the ISO 8859-1 character encoding.
        /// <p/>
        /// Properties from the defaults table of this <code>Properties</code>
        /// table (if any) are <i>not</i> written out by this method.
        /// <p/>
        /// If the comments argument is not null, then an ASCII <code>#</code>
        /// character, the comments string, and a line separator are first written
        /// to the output stream. Thus, the <code>comments</code> can serve as an
        /// identifying comment.
        /// <p/>
        /// Next, a comment line is always written, consisting of an ASCII
        /// <code>#</code> character, the current date and time (as if produced
        /// by the <code>ToString</code> method of <code>Date</code> for the
        /// current time), and a line separator as generated by the Writer.
        /// <p/>
        /// Then every entry in this <code>Properties</code> table is
        /// written out, one per line. For each entry the key string is
        /// written, then an ASCII <code>=</code>, then the associated
        /// element string. Each character of the key and element strings
        /// is examined to see whether it should be rendered as an escape
        /// sequence. The ASCII characters <code>\</code>, tab, form feed,
        /// newline, and carriage return are written as <code>\\</code>,
        /// <code>\t</code>, <code>\f</code> <code>\n</code>, and
        /// <code>\r</code>, respectively. Characters less than
        /// <code>&#92;u0020</code> and characters greater than
        /// <code>&#92;u007E</code> are written as
        /// <code>&#92;u</code><i>xxxx</i> for the appropriate hexadecimal
        /// value <i>xxxx</i>. For the key, all space characters are
        /// written with a preceding <code>\</code> character. For the
        /// element, leading space characters, but not embedded or trailing
        /// space characters, are written with a preceding <code>\</code>
        /// character. The key and element characters <code>#</code>,
        /// <code>!</code>, <code>=</code>, and <code>:</code> are written
        /// with a preceding backslash to ensure that they are properly loaded.
        /// <p/>
        /// After the entries have been written, the output stream is flushed. The
        /// output stream remains open after this method returns.
        /// 
        /// Throws IOException if writing this property list to the specified
        /// output stream throws an <tt>IOException</tt>.
        /// Throws ClassCastException if this <code>Properties</code> object
        /// contains any keys or values that are not <code>Strings</code>.
        /// Throws NullPointerException if <code>out</code> is null.
        /// </summary>
        /// <param name="outStream">an output stream.</param>
        /// <param name="comments">a description of the property list.</param>
        public void Store(Stream outStream, String comments) // throws IOException 
        {
            lock (this)
            {
                //BufferedWriter awriter;
                //awriter = new BufferedWriter(new StreamWriter(out , "8859_1"));
                var awriter = new StreamWriter(outStream, Encoding.UTF8 /* "8859_1" */); // BRS : ??
                if (comments != null)
                    WriteLn(awriter, "#" + comments);
                WriteLn(awriter, "#" + DateTime.Now.ToString());
                for (var e = Keys.GetEnumerator(); e.MoveNext(); )
                {
                    var key = (String)e.Current;
                    var val = (String)this[key];
                    key = SaveConvert(key, true);

                    // No need to escape embedded and trailing spaces for value, hence pass false to flag.
                    val = SaveConvert(val, false);
                    WriteLn(awriter, key + "=" + val);
                }
                awriter.Flush();
            }
        }

        #endregion // Operations
        #region IO

        /// <summary>
        /// Prints this property list out to the specified output stream.
        /// </summary>
        /// <remarks>This method is useful for debugging.</remarks>
        /// <param name="writer">an output stream.</param>
        public void WriteList(StreamWriter writer)
        {
            writer.WriteLine("-- listing properties --");
            var h = new Hashtable();
            Enumerate(h);
            for (var e = h.Keys.GetEnumerator(); e.MoveNext(); )
            {
                var key = (String)e.Current;
                var val = (String)h[key];
                if (val.Length > 40)
                {
                    val = val.Substring(0, 37) + "...";
                }
                writer.WriteLine(key + "=" + val);
            }
        }

        /// <summary>
        /// Enumerates all key/value pairs in the specified hashtable.
        /// </summary>
        /// <param name="h">the hashtable</param>
        private void Enumerate(Hashtable h)
        {
            lock (this)
            {
                if (Defaults != null)
                {
                    Defaults.Enumerate(h);
                }
                for (var e = Keys.GetEnumerator(); e.MoveNext(); )
                {
                    var key = (String)e.Current;
                    h[key] = this[key];
                }
            }
        }

        #endregion // IO

        #region LineReader (Class)

        /// <summary>
        /// read in a "logical line" from input stream, skip all comment and
        /// blank lines and filter out those leading whitespace characters 
        /// ( , and ) from the beginning of a "natural line". 
        /// Method returns the char length of the "logical line" and stores 
        /// the line in "lineBuf". 
        /// </summary>
        internal class LineReader
        {
            public LineReader(Stream inStream)
            {
                this.inStream = inStream;
            }

            byte[] inBuf = new byte[8192];
            internal char[] lineBuf = new char[1024];
            int inLimit;
            int inOff;
            Stream inStream;

            internal int readLine() // throws IOException  
            {
                var len = 0;

                var skipWhiteSpace = true;
                var isCommentLine = false;
                var isNewLine = true;
                var appendedLineBegin = false;
                var precedingBackslash = false;
                var skipLF = false;

                while (true)
                {
                    if (inOff >= inLimit)
                    {
                        inLimit = inStream.Read(inBuf, 0, inBuf.Length);
                        inOff = 0;
                        if (inLimit <= 0)
                        {
                            if (len == 0 || isCommentLine)
                            {
                                return -1;
                            }
                            return len;
                        }
                    }
                    //The line below is equivalent to calling a 
                    //ISO8859-1 decoder.
                    var c = (char)(0xff & inBuf[inOff++]);
                    if (skipLF)
                    {
                        skipLF = false;
                        if (c == '\n')
                        {
                            continue;
                        }
                    }
                    if (skipWhiteSpace)
                    {
                        if (c == ' ' || c == '\t' || c == '\f')
                        {
                            continue;
                        }
                        if (!appendedLineBegin && (c == '\r' || c == '\n'))
                        {
                            continue;
                        }
                        skipWhiteSpace = false;
                        appendedLineBegin = false;
                    }
                    if (isNewLine)
                    {
                        isNewLine = false;
                        if (c == '#' || c == '!')
                        {
                            isCommentLine = true;
                            continue;
                        }
                    }

                    if (c != '\n' && c != '\r')
                    {
                        lineBuf[len++] = c;
                        if (len == lineBuf.Length)
                        {
                            var newLength = lineBuf.Length * 2;
                            if (newLength < 0)
                            {
                                newLength = Int32.MaxValue;
                            }
                            var buf = new char[newLength];
                            Array.Copy(lineBuf, 0, buf, 0, lineBuf.Length);
                            lineBuf = buf;
                        }
                        //flip the preceding backslash flag
                        if (c == '\\')
                        {
                            precedingBackslash = !precedingBackslash;
                        }
                        else
                        {
                            precedingBackslash = false;
                        }
                    }
                    else
                    {
                        // reached EOL
                        if (isCommentLine || len == 0)
                        {
                            isCommentLine = false;
                            isNewLine = true;
                            skipWhiteSpace = true;
                            len = 0;
                            continue;
                        }
                        if (inOff >= inLimit)
                        {
                            inLimit = inStream.Read(inBuf, 0, inBuf.Length);
                            inOff = 0;
                            if (inLimit <= 0)
                            {
                                return len;
                            }
                        }
                        if (precedingBackslash)
                        {
                            len -= 1;
                            //skip the leading whitespace characters in following line
                            skipWhiteSpace = true;
                            appendedLineBegin = true;
                            precedingBackslash = false;
                            if (c == '\r')
                            {
                                skipLF = true;
                            }
                        }
                        else
                        {
                            return len;
                        }
                    }
                }
            }
        }

        #endregion // LineReader

        // * XMLUtils related stuff (not used) ****
        // * Loads all of the properties represented by the XML document on the
        // * specified input stream into this properties table.
        // *
        // * <p/>The XML document must have the following DOCTYPE declaration:
        // * <pre>
        // * &lt;!DOCTYPE properties SYSTEM "http://java.sun.com/dtd/properties.dtd"&gt;
        // * </pre>
        // * Furthermore, the document must satisfy the properties DTD described
        // * above.
        // *
        // * <p/>The specified stream remains open after this method returns.
        // *
        // * @param in the input stream from which to read the XML document.
        // * @throws IOException if reading from the specified input stream
        // * results in an <tt>IOException</tt>.
        // * @throws InvalidPropertiesFormatException Data on input stream does not
        // * constitute a valid XML document with the mandated document type.
        // * @throws NullPointerException if <code>in</code> is null.
        // * @see #storeToXML(OutputStream, String, String)
        // * @since 1.5
        // */
        //public void loadFromXML(Stream inStream)  // throws IOException , InvalidPropertiesFormatException  
        //{
        //    lock (this)
        //    {
        //        if (inStream == null)
        //            throw new ArgumentNullException();
        //        XMLUtils.load(this, inStream);
        //    }
        //}
        // /*
        // * Emits an XML document representing all of the properties contained
        // * in this table.
        // *
        // * <p/> An invocation of this method of the form <tt>props.storeToXML(os,
        // * comment)</tt> behaves in exactly the same way as the invocation
        // * <tt>props.storeToXML(os, comment, "UTF-8");</tt>.
        // *
        // * @param os the output stream on which to emit the XML document.
        // * @param comment a description of the property list, or <code>null</code>
        // * if no comment is desired.
        // * @throws IOException if writing to the specified output stream
        // * results in an <tt>IOException</tt>.
        // * @throws NullPointerException if <code>os</code> is null.
        // * @see #loadFromXML(InputStream)
        // * @since 1.5
        // */
        //public void storeToXML(Stream os, String comment) // throws IOException 
        //{
        //    lock (this)
        //    {
        //        if (os == null)
        //            throw new ArgumentNullException();
        //        storeToXML(os, comment, "UTF-8");
        //    }
        //}
        // /*
        // * Emits an XML document representing all of the properties contained
        // * in this table, using the specified encoding.
        // *
        // * <p/>The XML document will have the following DOCTYPE declaration:
        // * <pre>
        // * &lt;!DOCTYPE properties SYSTEM "http://java.sun.com/dtd/properties.dtd"&gt;
        // * </pre>
        // *
        // *<p/>If the specified comment is <code>null</code> then no comment
        // * will be stored in the document.
        // *
        // * <p/>The specified stream remains open after this method returns.
        // *
        // * @param os the output stream on which to emit the XML document.
        // * @param comment a description of the property list, or <code>null</code>
        // * if no comment is desired.
        // * @throws IOException if writing to the specified output stream
        // * results in an <tt>IOException</tt>.
        // * @throws NullPointerException if <code>os</code> is <code>null</code>,
        // * or if <code>encoding</code> is <code>null</code>.
        // * @see #loadFromXML(InputStream)
        // * @since 1.5
        // */
        //public void storeToXML(Stream os, String comment, String encoding) // throws IOException 
        //{
        //    lock (this)
        //    {
        //        if (os == null)
        //            throw new ArgumentNullException();
        //        XMLUtils.save(this, os, comment, encoding);
        //    }
        //}

    }
}