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
using System.Windows.Forms;
using BraneCloud.Evolution.EC.Support;

namespace BraneCloud.Evolution.EC.Configuration
{
    public partial class ParameterDatabase : PropertiesClass, IParameterDatabase
    {
        private readonly object _syncLock = new object();

        #region IParseParameter   ********************************************************************************

        #region Get Boolean

        /// <summary> 
        /// Searches down through databases to find a given parameter; If the
        /// parameter does not exist, defaultValue is returned. If the parameter
        /// exists, and it is set to "false" (case insensitive), false is returned.
        /// Else true is returned. The parameter chosen is marked "used" if it
        /// exists.
        /// </summary>
        private bool GetBoolean(IParameter parameter, bool defaultValue)
        {
            lock (_syncLock)
            {
                if (!_exists(parameter))
                    return defaultValue;
                return (!Take(parameter).ToUpper().Equals("false".ToUpper()));
            }
        }

        #endregion // Get Boolean

        #region Get Double

        private double GetDouble(IParameter parameter) // throws NumberFormatException
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    try
                    {
                        return Double.Parse(Take(parameter));
                    }
                    catch (FormatException)
                    {
                        throw new FormatException("Bad double (" + Take(parameter) + " ) for parameter " + parameter);
                    }
                }
                throw new FormatException("Double does not exist for parameter " + parameter);
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a double >= minValue. If not, this method returns minvalue-1,
        /// else it returns the parameter value. The parameter chosen is marked
        /// "used" if it exists.
        /// </summary>
        private double GetDouble(IParameter parameter, double minValue)
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    try
                    {
                        var i = Double.Parse(Take(parameter));

                        if (i < minValue)
                            return (minValue - 1);
                        return i;
                    }
                    catch (FormatException)
                    {
                        return (minValue - 1);
                    }
                }
                return (minValue - 1);
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a double >= minValue and &lt;= maxValue. If not, this method returns
        /// minvalue-1, else it returns the parameter value. The parameter chosen is
        /// marked "used" if it exists.
        /// </summary>
        private double GetDouble(IParameter parameter, double minValue, double maxValue)
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    try
                    {
                        var i = Double.Parse(Take(parameter));

                        if (i < minValue)
                            return (minValue - 1);
                        if (i > maxValue)
                            return (minValue - 1);
                        return i;
                    }
                    catch (FormatException)
                    {
                        return (minValue - 1);
                    }
                }
                return (minValue - 1);
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, which must be
        /// a float. If there is an error in parsing the parameter, then default is
        /// returned. The parameter chosen is marked "used" if it exists.
        /// </summary>
        private double GetDoubleWithDefault(IParameter parameter, double defaultValue)
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    try
                    {
                        return Double.Parse(Take(parameter));
                    }
                    catch (FormatException)
                    {
                        return defaultValue;
                    }
                }
                return defaultValue;
            }
        }

        #endregion // Get Double

        #region Get Float

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a float >= minValue. If not, this method returns minvalue-1, else
        /// it returns the parameter value. The parameter chosen is marked "used" if
        /// it exists.
        /// </summary>
        private float GetFloat(IParameter parameter) // throws NumberFormatException
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    try
                    {
                        return Single.Parse(Take(parameter));
                    }
                    catch (FormatException)
                    {
                        throw new FormatException("Bad float (" + Take(parameter) + " ) for parameter " + parameter);
                    }
                }
                throw new FormatException("Float does not exist for parameter " + parameter);
            }
        }

        /* public in ECJ */
        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a float >= minValue. If not, this method returns minvalue-1, else
        /// it returns the parameter value. The parameter chosen is marked "used" if
        /// it exists.
        /// </summary>	
        protected virtual float GetFloat(IParameter parameter, double minValue)
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    try
                    {
                        var i = Single.Parse(Take(parameter));

                        if (i < minValue)
                        {
                            return (float) (minValue - 1);
                        }
                        return i;
                    }
                    catch (FormatException)
                    {
                        return (float) (minValue - 1);
                    }
                }
                return (float) (minValue - 1);
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, which must be
        /// a float. If there is an error in parsing the parameter, then default is
        /// returned. The parameter chosen is marked "used" if it exists.
        /// </summary>
        private float GetFloatWithDefault(IParameter parameter, double defaultValue)
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    try
                    {
                        return Single.Parse(Take(parameter));
                    }
                    catch (FormatException)
                    {
                        return (float) (defaultValue);
                    }
                }
                return (float) (defaultValue);
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a float >= minValue and &lt;= maxValue. If not, this method returns
        /// minvalue-1, else it returns the parameter value. The parameter chosen is
        /// marked "used" if it exists.
        /// </summary>		
        private float GetFloat(IParameter parameter, double minValue, double maxValue)
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    try
                    {
                        var i = Single.Parse(Take(parameter));

                        if (i < minValue)
                        {
                            return (float) (minValue - 1);
                        }
                        if (i > maxValue)
                        {
                            return (float) (minValue - 1);
                        }
                        return i;
                    }
                    catch (FormatException)
                    {
                        return (float) (minValue - 1);
                    }
                }
                return (float) (minValue - 1);
            }
        }

        #endregion // Get Float

        #region Get Int

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be an integer. It returns the value, else throws a
        /// NumberFormatException exception if there is an error in parsing the
        /// parameter. The parameter chosen is marked "used" if it exists. Integers
        /// may be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        protected virtual int GetInt(IParameter parameter)
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    try
                    {
                        return ParseInt(Take(parameter));
                    }
                    catch (FormatException)
                    {
                        throw new FormatException("Bad integer (" + Take(parameter) + " ) for parameter " + parameter);
                    }
                }
                throw new FormatException("Integer does not exist for parameter " + parameter);
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be an integer >= minValue. It returns the value, or minValue-1 if
        /// the value is out of range or if there is an error in parsing the
        /// parameter. The parameter chosen is marked "used" if it exists. Integers
        /// may be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        protected virtual int GetInt(IParameter parameter, int minValue)
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    try
                    {
                        var i = ParseInt(Take(parameter));
                        if (i < minValue)
                            return minValue - 1;
                        return i;
                    }
                    catch (FormatException)
                    {
                        return minValue - 1;
                    }
                }
                return minValue - 1;
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, which must be
        /// an integer. If there is an error in parsing the parameter, then default
        /// is returned. The parameter chosen is marked "used" if it exists. Integers
        /// may be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        protected virtual int GetIntWithDefault(IParameter parameter, int defaultValue)
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    try
                    {
                        return ParseInt(Take(parameter));
                    }
                    catch (FormatException)
                    {
                        return defaultValue;
                    }
                }
                return defaultValue;
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be an integer >= minValue and &lt;= maxValue. It returns the value, or
        /// minValue-1 if the value is out of range or if there is an error in
        /// parsing the parameter. The parameter chosen is marked "used" if it
        /// exists. Integers may be in decimal or (if preceded with an X or x) in
        /// hexadecimal.
        /// </summary>
        protected virtual int GetIntWithMax(IParameter parameter, int minValue, int maxValue)
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    try
                    {
                        var i = ParseInt(Take(parameter));
                        if (i < minValue)
                            return minValue - 1;
                        if (i > maxValue)
                            return minValue - 1;
                        return i;
                    }
                    catch (FormatException)
                    {
                        return minValue - 1;
                    }
                }
                return minValue - 1;
            }
        }

        /// <summary> 
        /// Parses an integer from a string, either in decimal or (if starting with an x) in hex.
        /// We assume that the string has been trimmed already.
        /// </summary>
        protected virtual int ParseInt(string text)
        {
            lock (_syncLock)
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
                            if (d == (int) d) return (int) d; // looking fine
                            throw ex; // Let the original exception be caught somewhere else
                        }
                        catch (FormatException)
                        {
                            throw ex; // re-throw the original exception.
                        }
                    }
                }
            }
        }

        #endregion // Get Int

        #region Get Long

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a long. It returns the value, else throws a NumberFormatException
        /// exception if there is an error in parsing the parameter. The parameter
        /// chosen is marked "used" if it exists. Longs may be in decimal or (if
        /// preceded with an X or x) in hexadecimal.
        /// </summary>
        private long GetLong(IParameter parameter)
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    try
                    {
                        return ParseLong(Take(parameter));
                    }
                    catch (FormatException)
                    {
                        throw new FormatException("Bad long (" + Take(parameter) + " ) for parameter " + parameter);
                    }
                }
                throw new FormatException("Long does not exist for parameter " + parameter);
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a long >= minValue. If not, this method returns errValue, else it
        /// returns the parameter value. The parameter chosen is marked "used" if it
        /// exists. Longs may be in decimal or (if preceded with an X or x) in
        /// hexadecimal.
        /// </summary>
        private long GetLong(IParameter parameter, long minValue)
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    try
                    {
                        var i = ParseLong(Take(parameter));
                        if (i < minValue)
                            return minValue - 1;
                        return i;
                    }
                    catch (FormatException)
                    {
                        return minValue - 1;
                    }
                }
                return (minValue - 1);
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, which must be
        /// a long. If there is an error in parsing the parameter, then default is
        /// returned. The parameter chosen is marked "used" if it exists. Longs may
        /// be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        private long GetLongWithDefault(IParameter parameter, long defaultValue)
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    try
                    {
                        return ParseLong(Take(parameter));
                    }
                    catch (FormatException)
                    {
                        return defaultValue;
                    }
                }
                return defaultValue;
            }
        }

        /// <summary> 
        /// Use getLongWithMax(...) instead. Searches down through databases to find
        /// a given parameter, whose value must be a long >= minValue and = &lt;
        /// maxValue. If not, this method returns errValue, else it returns the
        /// parameter value. The parameter chosen is marked "used" if it exists.
        /// Longs may be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        private long GetLongWithMax(IParameter parameter, long minValue, long maxValue)
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    try
                    {
                        var i = ParseLong(Take(parameter));
                        if (i < minValue)
                            return minValue - 1;
                        if (i > maxValue)
                            return minValue - 1;
                        return i;
                    }
                    catch (FormatException)
                    {
                        return minValue - 1;
                    }
                }
                return (minValue - 1);
            }
        }

        /// <summary> 
        /// Parses a long from a string, either in decimal or (if starting with an x) in hex.
        /// We assume that the string has been trimmed already.
        /// </summary>
        private long ParseLong(string text)
        {
            lock (_syncLock)
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
                            if (d == (long) d) return (long) d; // looking fine
                            throw ex; // re-throw if that didn't work
                        }
                        catch (FormatException)
                        {
                            throw ex; // re-throw original
                        }
                    }
                }
            }
        }

        #endregion // Get Long

        #region Get File

        /// <summary> Searches down through the databases to find a given parameter, whose
        /// value must be an absolute or relative path name. If the parameter begins
        /// with a "$", a file is made based on the relative path name and returned
        /// directly. Otherwise, if it is absolute, a File is made based on the path
        /// name, or if it is relative, a file is made by resolving the path name
        /// with respect to the directory in which the file was which defined this
        /// ParameterDatabase in the ParameterDatabase hierarchy. If the parameter is
        /// not found, this returns null. The File is not checked for validity. The
        /// parameter chosen is marked "used" if it exists.
        /// </summary>
        public FileInfo GetFile(IParameter parameter)
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    var p = Take(parameter);
                    if (p == null)
                        return null;
                    if (p.StartsWith(C_HERE))
                        return new FileInfo(p.Substring(C_HERE.Length));
                    if (p.StartsWith(C_CLASS))
                        return null;  // can't start with that.


                    // BRS : 2009-03-15
                    // if (f.isAbsolute())
                    return Path.IsPathRooted(p) ? new FileInfo(p) : new FileInfo(DirectoryFor(parameter).FullName + "\\" + p);
                }
                return null;
            }
        }

        /// <summary> Searches down through the databases to find a given parameter, whose
        /// value must be an absolute or relative path name. If the parameter begins
        /// with a "$", a Directory is made based on the relative path name and returned
        /// directly. Otherwise, if it is absolute, a Directory is made based on the path
        /// name, or if it is relative, a Directory is made by resolving the path name
        /// with respect to the directory in which the file was which defined this
        /// ParameterDatabase in the ParameterDatabase hierarchy. If the parameter is
        /// not found, this returns null. The Directory is not checked for validity. The
        /// parameter chosen is marked "used" if it exists.
        /// </summary>
        public DirectoryInfo GetDirectory(IParameter parameter)
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    var p = Take(parameter);
                    if (p == null)
                        return null;
                    if (p.StartsWith(C_HERE))
                        return new DirectoryInfo(p.Substring(C_HERE.Length));
                    if (p.StartsWith(C_CLASS))
                        return null;  // can't start with that.


                    // BRS : 2009-03-15
                    // if (f.isAbsolute())
                    return Path.IsPathRooted(p) ? new DirectoryInfo(p) : new DirectoryInfo(DirectoryFor(parameter).FullName + "\\" + p);
                }
                return null;
            }
        }

        #endregion // Get File

        #region Get String

        /// <summary> 
        /// Searches down through databases to find a given parameter. Returns the
        /// parameter's value (trimmed) or null if not found or if the trimmed result
        /// is empty. The parameter chosen is marked "used" if it exists.
        /// </summary>
        private string GetString(IParameter parameter)
        {
            lock(_syncLock)
            {
                if (_exists(parameter))
                    return Take(parameter);

                return null;
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter. Returns the
        /// parameter's value trimmed of whitespace, or defaultValue.trim() if the
        /// result is not found or the trimmed result is empty.
        /// </summary>
        private string GetStringWithDefault(IParameter parameter, string defaultValue)
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    var result = Take(parameter);
                    if (result == null)
                    {
                        if (defaultValue == null)
                            return null;

                        result = defaultValue.Trim();
                    }
                    else
                    {
                        result = result.Trim();
                        if (result.Length == 0)
                        {
                            if (defaultValue == null)
                                return null;

                            result = defaultValue.Trim();
                        }
                    }
                    return result;
                }
                if (defaultValue == null)
                    return null;

                return defaultValue.Trim();
            }
        }

        #endregion // Get String

        #region GetValueType<T>

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a value type. It returns the value, else throws a NumberFormatException
        /// exception if there is an error in parsing the parameter. The parameter
        /// chosen is marked "used" if it exists. Value types may be in decimal or (if
        /// preceded with an X or x) in hexadecimal.
        /// </summary>
        private T GetValueType<T>(IParameter parameter)
            where T : struct, IConvertible, IComparable
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    return ParseValueType<T>(Take(parameter));
                }
                throw new InvalidOperationException(
                    String.Format("Parameter '{0}' could not be found.", parameter.Param));
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a value type >= minValue. If not, this method returns errValue, else it
        /// returns the parameter value. The parameter chosen is marked "used" if it
        /// exists. Value types may be in decimal or (if preceded with an X or x) in
        /// hexadecimal.
        /// </summary>
        private T GetValueType<T>(IParameter parameter, T minValue)
            where T : struct, IConvertible, IComparable
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    var i = ParseValueType<T>(Take(parameter));
                    if (i.CompareTo(minValue) < 0)
                        return minValue;
                    return i;
                }
                throw new InvalidOperationException(
                    String.Format("Parameter '{0}' could not be found.", parameter.Param));
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, which must be
        /// a value type. If there is an error in parsing the parameter, then default is
        /// returned. The parameter chosen is marked "used" if it exists. Value types may
        /// be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        private T GetValueTypeWithDefault<T>(IParameter parameter, T defaultValue)
            where T : struct, IConvertible, IComparable
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    try
                    {
                        return ParseValueType<T>(Take(parameter));
                    }
                    catch (FormatException)
                    {
                        return defaultValue;
                    }
                }
                return defaultValue;
            }
        }

        /// <summary> 
        /// Use getLongWithMax(...) instead. Searches down through databases to find
        /// a given parameter, whose value must be a value type >= minValue and = &lt;
        /// maxValue. If not, this method returns errValue, else it returns the
        /// parameter value. The parameter chosen is marked "used" if it exists.
        /// Value types may be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        private T GetValueTypeWithMax<T>(IParameter parameter, T minValue, T maxValue)
            where T : struct, IConvertible, IComparable
        {
            lock (_syncLock)
            {
                if (_exists(parameter))
                {
                    var i = ParseValueType<T>(Take(parameter));
                    if (i.CompareTo(minValue) < 0)
                        return minValue;
                    if (i.CompareTo(maxValue) > 0)
                        return maxValue;
                    return i;
                }
                throw new InvalidOperationException(
                    String.Format("Parameter '{0}' could not be found.", parameter.Param));
            }
        }

        /// <summary> 
        /// Parses a value type from a string, either in decimal or (if starting with an x) in hex.
        /// We assume that the string has been trimmed already.
        /// </summary>
        private T ParseValueType<T>(string text)
            where T : struct, IConvertible, IComparable
        {
            lock (_syncLock)
            {
                return (T)Convert.ChangeType(text, typeof(T));
            }
        }

        #endregion // GetIntegral<T>

        #endregion // IParseParameter
        #region ITrackParameter   ********************************************************************************

        #region Output

        /// <summary>
        /// Private helper function. 
        /// </summary>
        internal virtual void _list(StreamWriter writer, bool listShadowed, string prefix, IDictionary<string, string> gather)
        {
            lock (_syncLock)
            {
                if (listShadowed)
                {
                    // Print out my header
                    if (writer != null)
                    {
                        writer.WriteLine("\n########" + prefix);
                    }

                    WriteList(writer); // defined on underlying Properties class
                    var size = Parents.Count;
                    for (var x = 0; x < size; x++)
                        ((ParameterDatabase) Parents[x])._list(writer, true, prefix + "." + x, gather);
                }
                else
                {
                    // load in reverse order so things get properly overwritten
                    var size = Parents.Count;
                    for (var x = size - 1; x >= 0; x--)
                        ((ParameterDatabase) Parents[x])._list(writer, false, prefix, gather);
                    //var e = Keys.GetEnumerator();
                    //while (e.MoveNext())
                    //{
                    //    var key = (string) e.Current;
                    //    gather[key] = this[key];
                    //}
                    foreach (var key in Keys)
                    {
                        gather[(string) key] = (string) this[key];
                    }
                }
                if (writer != null)
                    writer.Flush();
            }
        }

        /// <summary>
        /// P: Successfully retrieved parameter
        /// !P: Unsuccessfully retrieved parameter
        /// &lt;P: Would have retrieved parameter
        /// E: Successfully tested for existence of parameter
        /// !E: Unsuccessfully tested for existence of parameter
        /// &lt;E: Would have tested for existence of parameter
        /// </summary>
        private void PrintGotten(IParameter parameter, IParameter defaultParameter, bool exists)
        {
            lock (_syncLock)
            {
                if (PrintState == PS_UNKNOWN)
                {
                    IParameter p = new Parameter(PRINT_PARAMS);
                    var jp = Take(p);
                    if (jp == null || jp.ToUpper().CompareTo("false".ToUpper()) == 0)
                        PrintState = PS_NONE;
                    else
                        PrintState = PS_PRINT_PARAMS;
                    Uncheck();
                    PrintGotten(p, null, false);
                }

                if (PrintState == PS_PRINT_PARAMS)
                {
                    var p = "P: ";
                    if (exists)
                        p = "E: ";

                    if (parameter == null && defaultParameter == null)
                        return;
                    if (parameter == null)
                    {
                        var result = _get(defaultParameter.Param);
                        Uncheck();
                        if (result == null)
                            // null parameter, didn't find defaultParameter
                            Console.Error.WriteLine("\t!" + p + defaultParameter.Param);
                        else
                            // null parameter, found defaultParameter
                            Console.Error.WriteLine("\t " + p + defaultParameter.Param + " = " + result);
                    }
                    else if (defaultParameter == null)
                    {
                        var result = _get(parameter.Param);
                        Uncheck();
                        if (result == null)
                            // null defaultParameter, didn't find parameter
                            Console.Error.WriteLine("\t!" + p + parameter.Param);
                        else
                            // null defaultParameter, found parameter
                            Console.Error.WriteLine("\t " + p + parameter.Param + " = " + result);
                    }
                    else
                    {
                        var result = _get(parameter.Param);
                        Uncheck();
                        if (result == null)
                        {
                            // didn't find parameter
                            Console.Error.WriteLine("\t!" + p + parameter.Param);
                            result = _get(defaultParameter.Param);
                            Uncheck();
                            if (result == null)
                                // didn't find defaultParameter
                                Console.Error.WriteLine("\t!" + p + defaultParameter.Param);
                            else
                                // found defaultParameter
                                Console.Error.WriteLine("\t " + p + defaultParameter.Param + " = " + result);
                        }
                        else
                        {
                            // found parameter
                            Console.Error.WriteLine("\t " + p + parameter.Param + " = " + result);
                            Console.Error.WriteLine("\t<" + p + defaultParameter.Param);
                        }
                    }
                }
            }
        }

        #endregion
        #region Events

        protected virtual void On(ParameterDatabaseEvent eventParam)
        {
            lock (_syncLock)
            {
                if (ParameterDatabaseListenerDelegateVar != null)
                    ParameterDatabaseListenerDelegateVar(this, eventParam);
            }
        }

        /// <summary> 
        /// Fires a parameter set event.
        /// </summary>
        protected virtual void FireParameterSet(IParameter parameter, string paramValue)
        {
            lock(_syncLock)
            {
                var it = _listeners.GetEnumerator();

                while (it.MoveNext())
                {
                    var l = it.Current;
                    l.ParameterSet(this, new ParameterDatabaseEvent(this, parameter, paramValue, ParameterDatabaseEvent.SET));
                }
            }
        }

        /// <summary> 
        /// Fires a parameter accessed event.
        /// </summary>
        protected virtual void FireParameterAccessed(IParameter parameter, string paramValue)
        {
            lock(_syncLock)
            {
                var it = _listeners.GetEnumerator();

                while (it.MoveNext())
                {
                    var l = it.Current;
                    l.ParameterSet(this, new ParameterDatabaseEvent(this, parameter, paramValue, ParameterDatabaseEvent.ACCESSED));
                }
            }
        }


        #endregion // Events

        #endregion // ITrackParameter
        #region IParameterSource  ********************************************************************************

        #region Get, Take, Remove, Exists

        private bool _exists(IParameter parameter)
        {
            lock(_syncLock)
            {
                if (parameter == null)
                    return false;
                var result = _get(parameter.Param);
                Uncheck();

                _accessed[parameter.Param] = true;
                return (result != null);
            }
        }

        private string Take(IParameter parameter)
        {
            lock(_syncLock)
            {
                var result = _get(parameter.Param);
                Uncheck();

                // set hashtable appropriately
                if (parameter != null)
                    _accessed[parameter.Param] = true;
                if (parameter != null)
                    _gotten[parameter.Param] = true;

                return result;
            }
        }

        /// <summary>
        /// Private helper function 
        /// </summary>
        private string _get(string parameter)
        {
            lock(_syncLock)
            {
                if (parameter == null)
                {
                    return null;
                }
                if (CheckState)
                    return null; // we already searched this path
                CheckState = true;

                var result = GetProperty(parameter);
                if (result == null)
                {
                    var size = Parents.Count;
                    for (var x = 0; x < size; x++)
                    {
                        result = ((ParameterDatabase)(Parents[x]))._get(parameter);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
                // preprocess
                else
                {
                    result = result.Trim();
                    if (result.Length == 0)
                        result = null;
                }
                return result;
            }
        }

        /// <summary>
        /// Private helper function 
        /// </summary>
        private void _removeDeeply(IParameter parameter)
        {
            lock(_syncLock)
            {
                if (CheckState)
                    return; // already removed from this path
                CheckState = true;
                Remove(parameter);
                var size = Parents.Count;
                for (var x = 0; x < size; x++)
                    ((ParameterDatabase)Parents[x]).RemoveDeeply(parameter);
            }
        }

        private HashSet<IParameter> _getShadowedValues(IParameter parameter, HashSet<IParameter> vals)
        {
            lock (_syncLock)
            {
                if (parameter == null)
                {
                    return vals;
                }

                if (CheckState)
                {
                    return vals;
                }

                CheckState = true;
                var result = GetProperty(parameter.Param);
                if (result != null)
                {
                    result = result.Trim();
                    if (result.Length != 0)
                        vals.Add(new Parameter(result));
                }

                var size = Parents.Count;
                for (var i = 0; i < size; ++i)
                {
                    ((ParameterDatabase) Parents[i])._getShadowedValues(parameter, vals);
                }

                return vals;
            }
        }

        /// <summary>
        /// Clears the checked flag 
        /// </summary>
        private void Uncheck()
        {
            lock(_syncLock)
            {
                if (!CheckState)
                    return; // we already unchecked this path -- this is dangerous if Parents are used without children
                CheckState = false;
                var size = Parents.Count;
                for (var x = 0; x < size; x++)
                    ((ParameterDatabase)(Parents[x])).Uncheck();
            }
        }

        #endregion // Get, Take, Remove, Exists
        #region Tree Model

        /// <summary> 
        /// Builds a TreeModel from the available property keys.   
        /// </summary>
        private TreeNode BuildTreeModel()
        {
            lock (_syncLock)
            {
                var sep = Path.DirectorySeparatorChar.ToString();
                var root = new ParameterDatabaseTreeNode(Directory.FullName + sep + Filename);
                var model = new ParameterDatabaseTreeModel(root);

                _buildTreeModel(model, root);

                model.Sort(root, new AnonymousClassComparator());

                // In order to add elements to the tree model, the leaves need to be
                // visible. This is because some properties have values *and* sub-
                // properties. Otherwise, if the nodes representing these properties did
                // not yet have children, then they would be invisible and the tree model
                // would be unable to add child nodes to them.
                model.VisibleLeaves = false;

                //        addListener(new ParameterDatabaseAdapter() {
                //           public void ParameterSet(ParameterDatabaseEvent evt) {
                //               model.setVisibleLeaves(true);
                //               _addNodeForParameter(model, root, evt.getParameter().Param);
                //               model.setVisibleLeaves(false);
                //           }
                //        });

                return model;
            }
        }

        private class AnonymousClassComparator : IComparer
        {
            public int Compare(object o1, object o2)
            {
                var t1 = (ParameterDatabaseTreeNode)o1;
                var t2 = (ParameterDatabaseTreeNode)o2;

                return ((IComparable)t1.Tag).CompareTo(t2.Tag);
            }
        }

        private void _buildTreeModel(TreeNode model, TreeNode root)
        {
            lock (_syncLock)
            {
                var e = Keys.GetEnumerator();
                while (e.MoveNext())
                {
                    _addNodeForParameter(model, root, (String) e.Current);
                }

                var size = Parents.Count;
                for (var i = 0; i < size; ++i)
                {
                    var parentDB = (ParameterDatabase) Parents[i];
                    parentDB._buildTreeModel(model, root);
                }
            }
        }

        private void _addNodeForParameter(TreeNode model, TreeNode root, string key)
        {
            lock (_syncLock)
            {
                if (key.IndexOf("parent.") == -1)
                {

                    var tok = new Tokenizer(key, ".");
                    var path = new string[tok.Count];
                    var t = 0;
                    while (tok.HasMoreTokens())
                    {
                        path[t++] = tok.NextToken();
                    }
                    var parent = root;

                    for (var i = 0; i < path.Length; ++i)
                    {
                        var children = model is ParameterDatabaseTreeModel
                                           ? ((ParameterDatabaseTreeModel) model).GetChildCount(parent)
                                           : parent.Nodes.Count;
                        if (children > 0)
                        {
                            var c = 0;
                            for (; c < children; ++c)
                            {
                                var child = parent.Nodes[c];
                                if (child.Tag.Equals(path[i]))
                                {
                                    parent = child;
                                    break;
                                }
                            }

                            if (c == children)
                            {
                                TreeNode child = new ParameterDatabaseTreeNode(path[i]);
                                parent.Nodes.Insert(parent.Nodes.Count, child);
                                parent = child;
                            }
                        }
                            // If the parent has no children, just add the node.
                        else
                        {
                            TreeNode child = new ParameterDatabaseTreeNode(path[i]);
                            parent.Nodes.Insert(0, child);
                            parent = child;
                        }
                    }
                }
            }
        }

        /// <summary> 
        /// Parses and adds s to the database. Returns true if there was actually something to parse.
        /// </summary>
        private bool ParseCommandLineArg(string s)
        {
            lock (_syncLock)
            {
                s = s.Trim();
                if (s.Length == 0)
                    return false;
                if (s[0] == '#')
                    return false;
                var eq = s.IndexOf('=');
                if (eq < 0)
                    return false;
                object tempObject = this[s.Substring(0, (eq) - (0))];
                this[s.Substring(0, (eq) - (0))] = s.Substring(eq + 1);
                return true;
            }
        }

        #endregion // Tree Model
        #region File System

        /// <summary>
        /// Private helper function 
        /// </summary>
        private FileInfo _directoryFor(IParameter parameter)
        {
            lock(_syncLock)
            {
                if (CheckState)
                    return null; // we already searched this path
                CheckState = true;
                FileInfo result = null;
                var p = GetProperty(parameter.Param);
                if (p == null)
                {
                    var size = Parents.Count;
                    for (var x = 0; x < size; x++)
                    {
                        result = ((ParameterDatabase)Parents[x])._directoryFor(parameter);
                        if (result != null)
                            return result;
                    }
                    return result;
                }
                return Directory;
            }
        }

        private FileInfo _fileFor(IParameter parameter)
        {
            lock(_syncLock)
            {
                if (CheckState)
                    return null;

                CheckState = true;
                FileInfo result = null;
                var p = GetProperty(parameter.Param);
                if (p == null)
                {
                    var size = Parents.Count;
                    for (var i = 0; i < size; ++i)
                    {
                        result = ((ParameterDatabase)Parents[i])._fileFor(parameter);
                        if (result != null)
                            return result;
                    }
                    return result;
                }
                return new FileInfo(Directory.FullName + "\\" + Filename);
            }
        }

        #endregion


        #endregion // IParameterSource
    }
}