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
using System.Xml.Linq;
using BraneCloud.Evolution.Collections.Generic;

namespace BraneCloud.Evolution.EC.Configuration
{
    /// <summary>
    /// This class is essentially a reverse tree of properties. 
    /// That is, in keeping with the configuration semantics of ECJ,
    /// it starts with the "child" as the root of a hierarchy of properties,
    /// and branches "up" to one or more parents, overriding matching properties
    /// (the keys) that may exist in each of them. The easiest way to think about this 
    /// is to envision properties as being defined in a rooted file-system tree.
    /// The file that acts as the root in this class is basically a leaf node 
    /// in the hierarchy of files located beneath some file system root directory.
    /// The reason that no file system operations are included in the class
    /// is so that a different representation of the underlying storage mechanism
    /// can be used. For example, it is easy to envision an XML-based configuration
    /// storage mechanism that uses namespaces and nested elements to define the hierarchy. 
    /// That is why the "Name" and "FullName" properties are included, 
    /// but are not interpreted internally. These property names happen to match
    /// property names used in <see cref="FileInfo"/> and <see cref="Directory"/> types, 
    /// but they are intended to provide information relevant to a scheme defined elsewhere
    /// (inversion of control, if you will).
    /// </summary>
    public partial class FileDictionaryTree : DictionaryTree<string, string>, IParameterSource, IParameterSourceBuilder
    {
        protected FileSystemInfo _root;

        #region Constructors

        /// <summary>
        /// The default constructor creates a new DirectoryInfo instance internally, initialized to the current directory.
        /// </summary>
        public FileDictionaryTree()
        {

        }

        public FileDictionaryTree(string name)
        {
            Name = name;
        }

        public FileDictionaryTree(string name, string fullName)
        {
            Name = name;
            FullName = fullName;
        }

        /// <summary>
        /// This constructor accepts an enumerable of initial properties for loading into the local dictionary.
        /// The internal DirectoryInfo instance is created pointing to the current directory. This obviously
        /// has no specific meaning.
        /// </summary>
        public FileDictionaryTree(IEnumerable<KeyValuePair<string, string>> initialProperties) : this()
        {
            foreach (var pair in initialProperties)
                LocalEntries.Add(pair.Key, pair.Value);
        }

        /// <summary>
        /// This constructor accepts an enumerable of initial properties for loading into the local dictionary.
        /// The internal DirectoryInfo instance is created pointing to the current directory. This obviously
        /// has no specific meaning.
        /// </summary>
        public FileDictionaryTree(string name, IEnumerable<KeyValuePair<string, string>> initialProperties)
            : this(name)
        {
            foreach (var pair in initialProperties)
                LocalEntries.Add(pair.Key, pair.Value);
        }

        /// <summary>
        /// This constructor accepts an enumerable of initial properties for loading into the local dictionary.
        /// The internal DirectoryInfo instance is created pointing to the current directory. This obviously
        /// has no specific meaning.
        /// </summary>
        public FileDictionaryTree(string name, string fullName, IEnumerable<KeyValuePair<string, string>> initialProperties)
            : this(name, fullName)
        {
            foreach (var pair in initialProperties)
                LocalEntries.Add(pair.Key, pair.Value);
        }

        /// <summary>
        /// This constructor accepts an enumerable of initial properties for loading into the local dictionary,
        /// and another enumerable of default properties for loading into the "Defaults" dictionary.
        /// </summary>
        public FileDictionaryTree(IEnumerable<KeyValuePair<string, string>> initialProperties, 
            IEnumerable<KeyValuePair<string, string>> defaultProperties)
            : this(initialProperties)
        {
            foreach (var pair in defaultProperties)
                LocalDefaults.Add(pair.Key, pair.Value);
        }

        /// <summary>
        /// This constructor accepts an enumerable of initial properties for loading into the local dictionary,
        /// and another enumerable of default properties for loading into the "Defaults" dictionary.
        /// </summary>
        public FileDictionaryTree(string name, IEnumerable<KeyValuePair<string, string>> initialProperties,
            IEnumerable<KeyValuePair<string, string>> defaultProperties)
            : this(name, initialProperties)
        {
            foreach (var pair in defaultProperties)
                LocalDefaults.Add(pair.Key, pair.Value);
        }

        /// <summary>
        /// This constructor accepts an enumerable of initial properties for loading into the local dictionary,
        /// and another enumerable of default properties for loading into the "Defaults" dictionary.
        /// </summary>
        public FileDictionaryTree(string name, string fullName, IEnumerable<KeyValuePair<string, string>> initialProperties,
            IEnumerable<KeyValuePair<string, string>> defaultProperties)
            : this(name, fullName, initialProperties)
        {
            foreach (var pair in defaultProperties)
                LocalDefaults.Add(pair.Key, pair.Value);
        }

        /// <summary>
        /// This constructor accepts enumerable initial properties, defaults, and parents with which to create a new instance.
        /// </summary>
        public FileDictionaryTree(IEnumerable<KeyValuePair<string, string>> initialProperties, 
            IEnumerable<KeyValuePair<string, string>> defaultProperties,
            IEnumerable<FileDictionaryTree> parents) 
            : this(initialProperties, defaultProperties)
        {
            foreach (var parent in parents)
                Parents.Add(new FileDictionaryTree(parent));
        }

        /// <summary>
        /// This constructor accepts enumerable initial properties, defaults, and parents with which to create a new instance.
        /// </summary>
        public FileDictionaryTree(string name, IEnumerable<KeyValuePair<string, string>> initialProperties,
            IEnumerable<KeyValuePair<string, string>> defaultProperties,
            IEnumerable<FileDictionaryTree> parents)
            : this(name, initialProperties, defaultProperties)
        {
            foreach (var parent in parents)
                Parents.Add(new FileDictionaryTree(parent));
        }

        /// <summary>
        /// This constructor accepts enumerable initial properties, defaults, and parents with which to create a new instance.
        /// </summary>
        public FileDictionaryTree(string name, string fullName, IEnumerable<KeyValuePair<string, string>> initialProperties,
            IEnumerable<KeyValuePair<string, string>> defaultProperties,
            IEnumerable<FileDictionaryTree> parents)
            : this(name, fullName, initialProperties, defaultProperties)
        {
            foreach (var parent in parents)
                Parents.Add(new FileDictionaryTree(parent));
        }

        /// <summary>
        /// This copy constructory creates a new instance that is identical to another instance, 
        /// but with its own identity. That is, this is a deep-clone of the original (including 
        /// recursive cloning of parents). 
        /// Note that the new instance has the same "Name" and "FullName", even though its object identity is unique.
        /// </summary>
        /// <param name="other">The instance to copy.</param>
        public FileDictionaryTree(FileDictionaryTree other)
            : this(other.Name, other.FullName, other.LocalEntries, other.LocalDefaults)
        {
            foreach (var parent in other.Parents)
                Parents.Add(new FileDictionaryTree(parent));
        }

        /// <summary>
        /// This constructor creates a new instance with exact copies of the original's parents.
        /// But the constructed instance itself contains no initial properties or defaults.
        /// </summary>
        /// <param name="parents"></param>
        public FileDictionaryTree(IEnumerable<FileDictionaryTree> parents)
        {
            foreach (var parent in parents)
            {
                Parents.Add(new FileDictionaryTree(parent));
            }
        }

        /// <summary>
        /// This constructor creates a new instance with exact copies of the original's parents.
        /// But the constructed instance itself contains no initial properties or defaults.
        /// </summary>
        public FileDictionaryTree(string name, IEnumerable<FileDictionaryTree> parents)
            : this(name)
        {
            foreach (var parent in parents)
            {
                Parents.Add(new FileDictionaryTree(parent));
            }
        }

        /// <summary>
        /// This constructor creates a new instance with exact copies of the original's parents.
        /// But the constructed instance itself contains no initial properties or defaults.
        /// </summary>
        public FileDictionaryTree(string name, string fullName, IEnumerable<FileDictionaryTree> parents)
            : this(name, fullName)
        {
            foreach (var parent in parents)
            {
                Parents.Add(new FileDictionaryTree(parent));
            }
        }

        public FileDictionaryTree(FileInfo fileInfo)
        {
            Build(fileInfo);
        }

        #endregion // Constructors

        #region IParameterSource

        public bool SourceExists { get { return _root.Exists; } }

        /// <summary>
        /// This method simply searches for the parameter
        /// in the local Primary dictionary, and in the
        /// Primary dictionary of all parents. It does NOT
        /// search the Defaults dictionaries.
        /// </summary>
        /// <param name="parameter">The parameter for which to search.</param>
        /// <param name="defaultParameter">The default parameter for which to search if the primary parameter is not found.</param>
        /// <returns>A boolean indicating if the parameter or the default parameter was found.</returns>
        public bool ParameterExists(IParameter parameter, IParameter defaultParameter)
        {
            if (ContainsKey(parameter.Param))
                return true;
            return ContainsDefaultKey(defaultParameter.Param);
        }

        /// <summary>
        /// This method inserts or overwrites a parameter and its value in the Primary dictionary.
        /// Note that this only affects the top-level Primary dictionary. If parents contain entries
        /// for the same parameter, those entries remain unaffected and will "shadow" whatever
        /// entry is created or overwritten here. In other words, if this entry is later removed
        /// from the dictionary at this level, then the parent entries will come back into view.
        /// </summary>
        /// <param name="parameter">The parameter that will be used as the key.</param>
        /// <param name="paramValue">The parameter value that will be associated with the key.</param>
        public void SetParameter(IParameter parameter, string paramValue)
        {
            if (LocalEntries.ContainsKey(parameter.Param)) // update existing entry
                LocalEntries[parameter.Param] = paramValue;
            else
                LocalEntries.Add(parameter.Param, paramValue); // add new entry
        }

        public override string ToString()
        {
            return ToString("");
        }

        public string ToString(string initialIndent)
        {
            var sb = new StringBuilder();
            sb = sb.AppendFormat("{0}{1} : {2}\n", initialIndent, "ParameterNamespace", FullName);
            sb = sb.AppendFormat("\t{0}{1}:\n", initialIndent, "LocalEntries");
            sb = LocalEntries.Aggregate(sb, (current, entry) => current.AppendFormat("\t\t{0}{1} = {2}\n", initialIndent, entry.Key, entry.Value));
            sb = Parents.Cast<FileDictionaryTree>().Aggregate(sb, (current, parent) => current.Append(parent.ToString(initialIndent + "\t")));
            return sb.ToString();
        }
        
        public XElement ToXml()
        {
            var entries = LocalEntries.Select(
                entry => new XElement("parameter",
                    new XAttribute("name", entry.Key),
                    new XAttribute("value", entry.Value))).ToList();

            var defaults = LocalDefaults.Select(
                def => new XElement("default",
                    new XAttribute("name", def.Key),
                    new XAttribute("value", def.Value))).ToList();

            var parents = Parents.OfType<FileDictionaryTree>().Select(parent => parent.ToXml()).ToList();

            var root = new XElement("dictionary");
            if (!string.IsNullOrEmpty(Name))
                root.Add(new XAttribute("name", Name));
            if (!string.IsNullOrEmpty(FullName))
                root.Add(new XAttribute("fullName", FullName));
            if (entries.Count > 0)
                root.Add(entries);
            if (defaults.Count > 0)
                root.Add(defaults);
            if (parents.Count > 0)
                root.Add(new XElement("parents", new XAttribute("count", parents.Count), parents));
            return root;
            //return new XElement("dictionary", new XAttribute("name", FullName), 
            //    new XElement("entries", new XAttribute("count", LocalEntries.Count), entries),
            //    new XElement("defaults", new XAttribute("count", LocalDefaults.Count), defaults),
            //    new XElement("parents", new XAttribute("count", Parents.Count), parents));
        }

        #endregion // IParameterSource
        #region IParamteterSourceBuilder

        public void Build(ParameterSourceLocator sourceLocator)
        {
            if (sourceLocator.Type != ParameterSourceType.PropertyFile)
                throw new ArgumentException(String.Format(
                    "The locator must be of type '{0}'. The type provided was '{1}'.",
                    ParameterSourceType.PropertyFile,
                    sourceLocator.Type));

            Build(new FileInfo(sourceLocator.Path));
        }

        #endregion // IParamteterSourceBuilder

        #region Build

        public void Build(FileInfo fileInfo)
        {
            Clear(); // This clears all local primary and default entries, as well as the list of parents.

            if (fileInfo == null)
                throw new ArgumentNullException("fileInfo", "The argument must not be null.");
            if (!fileInfo.Exists)
                throw new FileNotFoundException("The specified file does not exist.", fileInfo.FullName);

            _root = fileInfo;
            Name = _root.Name;
            FullName = _root.FullName;

            var propFile = new File(fileInfo);

            foreach (var entry in propFile.Properties)
            {
                LocalEntries.Add(entry.Key, entry.Value);
            }
            BuildParents();
        }

        protected void BuildParents()
        {
            var x = 0;

            string relPath;
            while (LocalEntries.TryGetValue("parent." + x, out relPath))
            {
                if (_root != null && !String.IsNullOrEmpty(_root.FullName) && !String.IsNullOrEmpty(relPath))
                {
                    var rootDir = Path.GetDirectoryName(_root.FullName);
                    if (!String.IsNullOrEmpty(rootDir))
                    {
                        var absPath = Path.GetFullPath(Path.Combine(rootDir, relPath));
                        var fi = new FileInfo(absPath);
                        if (fi.Exists)
                        {
                            var parent = new FileDictionaryTree(fi);
                            Parents.Add(parent);
                        }
                    }
                }
                x++; // increment for the next parent
            }
        }

        #endregion // Build
        #region IParseParameter

        public object GetInstanceForParameter(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseType)
        {
            throw new NotImplementedException();
        }
        public object GetInstanceForParameterEq(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseType)
        {
            throw new NotImplementedException();
        }
        public object GetTypeForParameter(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseType)
        {
            throw new NotImplementedException();
        }

        /// <summary> 
        /// Searches down through the tree to find a given parameter; If the
        /// parameter does not exist, defaultValue is returned. If the parameter
        /// exists, and it is set to "false" (case insensitive), false is returned.
        /// Else true is returned. The parameter chosen is marked "used" if it
        /// exists.
        /// </summary>
        public bool GetBoolean(IParameter parameter, IParameter defaultParameter, bool defaultValue)
        {
            string val;
            bool result;
            if (TryGetValue(parameter.Param, out val)) // Try local and parent primary dictionaries
            {
                if (bool.TryParse(val, out result))
                    return result;
            }
            if (TryGetDefault(defaultParameter.Param, out val)) // Try local and parent default dictionaries
            {
                if (bool.TryParse(val, out result))
                    return result;
            }
            return defaultValue;
        }

        public int GetInt(IParameter parameter, IParameter defaultParameter)
        {
            string val;
            int result;
            if (TryGetValue(parameter.Param, out val)) // Try local and parent primary dictionaries
            {
                if (int.TryParse(val, out result))
                    return result;
            }
            if (TryGetDefault(defaultParameter.Param, out val)) // Try local and parent default dictionaries
            {
                if (int.TryParse(val, out result))
                    return result;
            }
            return default(int);
        }
        public int GetInt(IParameter parameter, IParameter defaultParameter, int minValue)
        {
            string val;
            int result = minValue;
            if (TryGetValue(parameter.Param, out val)) // Try local and parent primary dictionaries
            {
                if (int.TryParse(val, out result))
                    return result > minValue ? result : minValue;
            }
            if (TryGetDefault(defaultParameter.Param, out val)) // Try local and parent default dictionaries
            {
                if (int.TryParse(val, out result))
                    return result > minValue ? result : minValue;
            }
            return Math.Max(result, minValue);
        }
        public int GetIntWithDefault(IParameter parameter, IParameter defaultParameter, int defaultValue)
        {
            string val;
            int result;
            if (TryGetValue(parameter.Param, out val)) // Try local and parent primary dictionaries
            {
                if (int.TryParse(val, out result))
                    return result;
            }
            if (TryGetDefault(defaultParameter.Param, out val)) // Try local and parent default dictionaries
            {
                if (int.TryParse(val, out result))
                    return result;
            }
            return defaultValue;
        }
        public int GetIntWithMax(IParameter parameter, IParameter defaultParameter, int minValue, int maxValue)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(IParameter parameter, IParameter defaultParameter, double minValue)
        {
            throw new NotImplementedException();
        }
        public float GetFloatWithDefault(IParameter parameter, IParameter defaultParameter, double defaultValue)
        {
            throw new NotImplementedException();
        }
        public float GetFloatWithMax(IParameter parameter, IParameter defaultParameter, double minValue, double maxValue)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(IParameter parameter, IParameter defaultParameter)
        {
            throw new NotImplementedException();
        }
        public double GetDouble(IParameter parameter, IParameter defaultParameter, double minValue)
        {
            throw new NotImplementedException();
        }
        public double GetDoubleWithDefault(IParameter parameter, IParameter defaultParameter, double defaultValue)
        {
            throw new NotImplementedException();
        }
        public double GetDoubleWithMax(IParameter parameter, IParameter defaultParameter, double minValue, double maxValue)
        {
            throw new NotImplementedException();
        }

        public long GetLong(IParameter parameter, IParameter defaultParameter)
        {
            throw new NotImplementedException();
        }
        public long GetLong(IParameter parameter, IParameter defaultParameter, long minValue)
        {
            throw new NotImplementedException();
        }
        public long GetLongWithDefault(IParameter parameter, IParameter defaultParameter, long defaultValue)
        {
            throw new NotImplementedException();
        }
        public long GetLongWithMax(IParameter parameter, IParameter defaultParameter, long minValue, long maxValue)
        {
            throw new NotImplementedException();
        }

        public FileInfo GetFile(IParameter parameter, IParameter defaultParameter)
        {
            throw new NotImplementedException();
        }

        public string GetString(IParameter parameter, IParameter defaultParameter)
        {
            throw new NotImplementedException();
        }
        public string GetStringWithDefault(IParameter parameter, IParameter defaultParameter, string defaultValue)
        {
            throw new NotImplementedException();
        }

        #endregion // IParseParameter

        #region Raw Parsing of Standard Types

        /// <summary>
        /// Parses a <see cref="System.Boolean"/> value from a string.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        /// <returns>A boolean value.</returns>
        public static bool ParseBoolean(string text)
        {
            try
            {
                return Boolean.Parse(text.ToLower());
            }
            catch (Exception)
            {               
                throw;
            }
        }

        /// <summary> 
        /// Parses an Int32 from a string, either in decimal or (if starting with an x) in hex.
        /// We assume that the string has been trimmed already.
        /// </summary>
        public static int ParseInt32(string text)
        {
            char c;
            if (!string.IsNullOrEmpty(text) && ((text[0] == (c = 'x')) || c == 'X'))
            {
                // it's a hex int, load it as hex
                return Convert.ToInt32(text.Substring(1), 16);
            }
            else
            {
                try
                {
                    // it's decimal
                    return int.Parse(text);
                }
                catch (FormatException ex)
                {
                    // maybe it's a double ending in .0, which should be okay
                    try
                    {
                        var d = Double.Parse(text);
                        if (d == (int)d) return (int)d; // looking fine
                        throw ex; // Let the original exception be caught somewhere else
                    }
                    catch (FormatException)
                    {
                        throw ex; // re-throw the original exception.
                    }
                }
            }
        }

        /// <summary> 
        /// Parses a <see cref="System.Int64"/> value from a string, 
        /// either in decimal or (if starting with an x) in hex.
        /// We assume that the string has been trimmed already.
        /// </summary>
        public static long ParseInt64(string text)
        {
            char c;
            if (!string.IsNullOrEmpty(text) && ((text[0] == (c = 'x')) || c == 'X'))
            {
                // it's a hex int, load it as hex
                return Convert.ToInt64(text.Substring(1), 16);
            }
            else
            {
                try
                {
                    // it's decimal
                    return Int64.Parse(text);
                }
                catch (FormatException ex)
                {
                    // maybe it's a double ending in .0, which should be okay
                    try
                    {
                        var d = Double.Parse(text);
                        if (d == (long)d) return (long)d; // looking fine
                        throw ex; // re-throw if that didn't work
                    }
                    catch (FormatException)
                    {
                        throw ex; // re-throw original
                    }
                }
            }
        }

        /// <summary> 
        /// Parses a <see cref="System.Single"/> value from a string.
        /// We assume that the string has been trimmed already.
        /// </summary>		
        private static float ParseSingle(string text)
        {
            try
            {
                var i = Single.Parse(text);
                return i;
            }
            catch (FormatException ex)
            {
                throw ex; // re-throw the original exception.
            }
        }

        /// <summary> 
        /// Parses a <see cref="System.Double"/> value from a string.
        /// We assume that the string has been trimmed already.
        /// </summary>
        private static double ParseDouble(string text)
        {
            var i = Double.Parse(text);
            return i;
        }

        /// <summary>
        /// Parses a <see cref="System.Type"/> from a string.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        /// <returns>A Type.</returns>
        private static Type ParseType(string text)
        {
            throw new NotImplementedException();
        }

        #endregion //  Raw Parsing

        /// <summary>
        /// This method creates a deep clone of the current instance.
        /// </summary>
        /// <returns></returns>
        public FileDictionaryTree Clone()
        {
            return new FileDictionaryTree(this);
        }
    }
}