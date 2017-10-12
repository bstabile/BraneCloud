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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BraneCloud.Evolution.Archetype;

namespace BraneCloud.Evolution.EC.Configuration
{
    public class File : INamespace
    {
        //private const string RegexSloppy = @"^\s*[^#]((\s)*(?<Key>([^\=^\s^\n]+))[\s^\n]*\=(\s)*(?<Value>([^\n^\s]*(\n){0,1})))";
        private const string RegexStrict = @"^\s*[^#]((\s)*(?<Key>([^\=^\s^\n]+))[\s^\n]*\=(\s)*(?<Value>([^\n^\s]*(\n){0,1})))";

        public File(FileInfo fileInfo)
        {
            Info = fileInfo;
        }

        // The following properties just help make things a little cleaner for the client (for binding etc.)
        #region IParameterNamespace

        public string Name { get { return _info == null ? "" : _info.Name; } }
        public string FullName { get { return _info == null ? "" : _info.FullName; } }
        public bool Exists { get { return _info.Exists; } }

        #endregion // IParameterNamespace

        public FileInfo Info
        {
            get { return _info; }
            set
            {
                if (value == null) throw new ArgumentNullException("fileInfo", "A valid FileInfo instance was not provided.");
                if (!value.Exists)
                    throw new FileNotFoundException("The specified file could not be found.", value.FullName);
                if (_info != null && _info.Equals(value)) return;

                _info = value;
                Properties = ParseForProperties(value);
            }
        }
        private FileInfo _info;

        public IDictionary<string, string> Properties { get; protected set; }

        #region ParseForProperties   **********************************************************************************************

        /// <summary>
        /// This method parses a <see cref="Stream" /> created 
        /// from a <see cref="FileInfo" /> instance for properties 
        /// in the canonical format: "key = value". But it also will recognize
        /// properties specificed in the less formal format: "key value".
        /// Arbitrary whitespace is allowed between tokens, but the key must
        /// not include embedded whitespace for obvious reasons.
        /// </summary>
        /// <remarks>
        /// This method defers to the static method <see cref="ParseForPropertiesAllowSpaceDelim" />.
        /// </remarks>
        /// <param name="fileInfo">
        /// A <see cref="FileInfo" /> that will be used to open a <see cref="Stream" /> that will be parsed for properties.
        /// </param>
        /// <returns>
        /// An <see cref="IDictionary{string, string}" /> containing the properties that have been parsed.
        /// </returns>
        public static IDictionary<string, string> ParseForProperties(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException("fileInfo", "The argument provided was null.");
            if (!fileInfo.Exists) throw new FileNotFoundException("The specified file could not be found.", fileInfo.FullName);
            using (var reader = new StreamReader(fileInfo.FullName))
            {
                return ParseForPropertiesAllowSpaceDelim(reader);
            }
        }
        
        /// <summary>
        /// This method parses a <see cref="Stream" /> for properties 
        /// in the canonical format: "key = value". But it also will recognize
        /// properties specificed in the less formal format: "key value".
        /// Arbitrary whitespace is allowed between tokens, but the key must
        /// not include embedded whitespace for obvious reasons.
        /// </summary>
        /// <remarks>
        /// This method defers to the static method <see cref="ParseForPropertiesAllowSpaceDelim" />.
        /// </remarks>
        /// <param name="stream">
        /// A <see cref="Stream" /> that will be parsed for properties.
        /// </param>
        /// <returns>
        /// An <see cref="IDictionary{string, string}" /> containing the properties that have been parsed.
        /// </returns>
        public static IDictionary<string, string> ParseForProperties(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream", "The argument provided was null.");
            if (stream.Length == 0) throw new EndOfStreamException("The stream provided has zero length.");
            using (var reader = new StreamReader(stream))
            {
                return ParseForPropertiesAllowSpaceDelim(reader);
            }
        }

        /// <summary>
        /// This method parses a <see cref="TextReader" /> for properties 
        /// in the canonical format: "key = value". But it also will recognize
        /// properties specificed in the less formal format: "key value".
        /// Arbitrary whitespace is allowed between tokens, but the key must
        /// not include embedded whitespace for obvious reasons.
        /// </summary>
        /// <param name="reader">A <see cref="TextReader" /> that will be parsed for properties.</param>
        /// <returns>An <see cref="IDictionary{string, string}" /> containing the properties that have been parsed.</returns>
        public static IDictionary<string, string> ParseForPropertiesAllowSpaceDelim(TextReader reader)
        {
            if (reader == null) return null;
            var dict = new Dictionary<string, string>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                if (line.Contains("="))
                {
                    var a = line.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (a.Length != 2) continue;
                    a[0] = a[0].Trim();
                    a[1] = a[1].Trim();

                    if (dict.ContainsKey(a[0]))
                        dict[a[0]] = a[1];
                    else
                        dict.Add(a[0], a[1]);
                }
                else
                {
                    var a = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (a.Length < 2) continue;

                    if (dict.ContainsKey(a[0]))
                        dict[a[0].Trim()] = a[1].Trim();
                    else
                        dict.Add(a[0].Trim(), a[1].Trim());
                }
            }
            return dict;
        }

        /// <summary>
        /// This method parses a <see cref="Stream" /> created 
        /// from a specified <see cref="FileInfo" /> instance 
        /// for properties in the canonical format: "key = value".
        /// Arbitrary whitespace between tokens is allowed, but the use of whitespace 
        /// within the key token is not recommended. That is because it would conflict 
        /// with the semantics of the less formal method: <see cref="ParseForPropertiesAllowSpaceDelim" />.
        /// The latter will recognize "key value" property specifications (using a whitespace delimeter),
        /// in addition to the canonical format required here (using the "assignment" delimeter).
        /// </summary>
        /// <param name="fileInfo">
        /// A <see cref="FileInfo" /> that will be used to create a <see cref="Stream" /> that will be parsed for properties.
        /// </param>
        /// <returns>
        /// An <see cref="IDictionary{string, string}" /> containing the properties that have been parsed.
        /// </returns>
        public static IDictionary<string, string> ParseForPropertiesStrict(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException("fileInfo", "The argument provided was null.");
            if (!fileInfo.Exists) throw new FileNotFoundException("The specified file could not be found.", fileInfo.FullName);
            using (var reader = new StreamReader(fileInfo.FullName))
            {
                return ParseForPropertiesStrict(reader);
            }
        }

        /// <summary>
        /// This method parses a <see cref="Stream" /> for properties in the canonical format: "key = value".
        /// Arbitrary whitespace between tokens is allowed, but the use of whitespace 
        /// within the key token is not recommended. That is because it would conflict 
        /// with the semantics of the less formal method: <see cref="ParseForPropertiesAllowSpaceDelim" />.
        /// The latter will recognize "key value" property specifications (using a whitespace delimeter),
        /// in addition to the canonical format required here (using the "assignment" delimeter).
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream" /> that will be parsed for properties.
        /// </param>
        /// <returns>
        /// An <see cref="IDictionary{string, string}" /> containing the properties that have been parsed.
        /// </returns>
        public static IDictionary<string, string> ParseForPropertiesStrict(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream", "The argument provided was null.");
            if (stream.Length == 0) throw new EndOfStreamException("The stream provided has zero length.");
            using (var reader = new StreamReader(stream))
            {
                return ParseForPropertiesStrict(reader);
            }
        }

        /// <summary>
        /// This method parses for properties in the canonical format: "key = value".
        /// Arbitrary whitespace between tokens is allowed, but the use of whitespace 
        /// within the key token is not recommended. That is because it would conflict 
        /// with the semantics of the less formal method: <see cref="ParseForPropertiesAllowSpaceDelim" />.
        /// The latter will recognize "key value" property specifications (using a whitespace delimeter),
        /// in addition to the canonical format required here (using the "assignment" delimeter).
        /// </summary>
        /// <remarks>
        /// The caller is responsible for closing the underlying <see cref="TextReader" /> or <see cref="Stream" />
        /// </remarks>
        /// <param name="reader">
        /// A <see cref="TextReader" /> that will be parsed for properties.
        /// </param>
        /// <returns>
        /// An <see cref="IDictionary{string, string}" /> containing the properties that have been parsed.
        /// </returns>
        public static IDictionary<string, string> ParseForPropertiesStrict(TextReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader", "The argument provided was null.");
            var dict = new Dictionary<string, string>();

            var matches = Regex.Matches(reader.ReadToEnd(), RegexStrict, RegexOptions.Multiline);
            foreach (Match m in matches)
            {
                var arr = m.Value.Split('=');
                if (arr.Length == 2)
                {
                    var k = arr[0].Trim();
                    var v = arr[1].Trim();
                    if (dict.ContainsKey(k))
                    {
                        dict.Add(k + "_", v);
                    }
                    else
                    {
                        dict.Add(k, v);
                    }
                }
            }
            return dict;
        }

        #endregion // ParseForProperties
    }
}