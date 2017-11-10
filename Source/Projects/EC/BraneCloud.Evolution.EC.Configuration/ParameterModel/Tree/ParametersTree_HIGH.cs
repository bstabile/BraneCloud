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
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using BraneCloud.Evolution.EC.Support;

namespace BraneCloud.Evolution.EC.Configuration
{
    public partial class ParametersTree : PropertiesClass, ITree
    {
        #region IParameterNamespace

        public string Name { get; protected set; }
        public string FullName { get; protected set; }

        #endregion // IParameterNamespace

        #region IParameterSource

        public bool SourceExists
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary> 
        /// Returns true if either parameter or defaultParameter exists in the database
        /// </summary>
        public virtual bool ParameterExists(IParameter parameter, IParameter defaultParameter)
        {
            lock (this)
            {
                PrintGotten(parameter, defaultParameter, true);
                if (ParameterExists(parameter, null))
                    return true;
                if (ParameterExists(defaultParameter, null))
                    return true;
                return false;
            }
        }

        public string ToString(string initialIndent)
        {
            throw new NotImplementedException();
        }

        public XElement ToXml()
        {
            throw new NotImplementedException();
        }

        #endregion // IParameterSource

        #region Constants   **********************************************************************************

        public const string C_HERE = "$";
        public const string C_CLASS = "@";
        public const string UNKNOWN_VALUE = "";
        public const string PRINT_PARAMS = "print-params";
        public const int PS_UNKNOWN = -1;
        public const int PS_NONE = 0;
        public const int PS_PRINT_PARAMS = 1;
        public int printState = PS_UNKNOWN;

        #endregion // Constants

        #region Fields Internal   ****************************************************************************

        internal List<IParameterDatabase> Parents;
        internal FileInfo directory;
        internal string filename;
        internal bool checkState;
        internal Dictionary<string, bool> gotten;
        internal Dictionary<string, bool> accessed;
        internal List<ParameterDatabaseListener> listeners;

        #endregion // Fields Internal

        #region Fields Public   ******************************************************************************

        public Dictionary<string, bool> Gotten { get { return gotten; } }
        public Dictionary<string, bool> Accessed { get { return accessed; } }
        public List<ParameterDatabaseListener> Listeners { get { return listeners; } }

        #endregion // Fields Public

        #region Get Instance and Type   *********************************************************************

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a full Class name, and the class must be a descendent of but not
        /// equal to <i>mustCastTosuperclass </i>. Loads the class and returns an
        /// instance (constructed with the default constructor), or throws a
        /// ParamClassLoadException if there is no such Class. If the parameter is
        /// not found, the defaultParameter is used. The parameter chosen is marked
        /// "used".
        /// </summary>
        public virtual object GetInstanceForParameter(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseType)
        {
            PrintGotten(parameter, defaultParameter, false);
            IParameter p;
            if (_exists(parameter))
                p = parameter;
            else if (_exists(defaultParameter))
                p = defaultParameter;
            else
            {
                throw new ParamClassLoadException("No type name provided.\nPARAMETER: " + parameter + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter));
            }
            try
            {
                var c = Type.GetType(Take(p));
                if (!mustCastToBaseType.IsAssignableFrom(c))
                {
                    throw new ParamClassLoadException("The type " + c.FullName + "\ndoes not cast into the base type " + mustCastToBaseType.FullName 
                        + "\nPARAMETER: " + parameter + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter));
                }
                if (mustCastToBaseType == c)
                {
                    throw new ParamClassLoadException("The type " + c.FullName + "\nmust not be the same as the required base type " + mustCastToBaseType.FullName 
                        + "\nPARAMETER: " + parameter + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter));
                }
                return Activator.CreateInstance(c);
            }
            // BRS : 2009-03-15
            // Calling Type.GetType(string s) throws TypeLoadException if the typeName (s) is invalid
            catch (TypeLoadException e)
            {
                throw new ParamClassLoadException("Type not found: " + Take(p) + "\nPARAMETER: " + parameter 
                    + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" + e);
            }
            // Since we've made it this far we already know that Type.GetType(string s) has given us a valid Type
            // Calling Activator.CreateInstance(Type t) throws TargetInvocationException if the constructor fails
            catch (TargetInvocationException e)
            {
                throw new ParamClassLoadException("Could not load type: " 
                    
                    + Take(p) + "\nPARAMETER: " + parameter 
                    + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter) 
                    + "\nEXCEPTION: \n\n" + e);
            }
            catch (MemberAccessException e)
            {
                throw new ParamClassLoadException("The requested type is an interface or an abstract type: " 
                    + Take(p) + "\nPARAMETER: " + parameter 
                    + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter) 
                    + "\nEXCEPTION: \n\n" + e);
            }
            catch (Exception e)
            {
                throw new ParamClassLoadException("The requested type cannot be initialized with the default initializer: " 
                    + Take(p) + "\nPARAMETER: " + parameter 
                    + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter) 
                    + "\nEXCEPTION: \n\n" + e);
            }
        }
        
        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a full Class name, and the class must be a descendent, or equal
        /// to, <i>mustCastTosuperclass </i>. Loads the class and returns an instance
        /// (constructed with the default constructor), or throws a
        /// ParamClassLoadException if there is no such Class. The parameter chosen
        /// is marked "used".
        /// </summary>
        public virtual object GetInstanceForParameterEq(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseType)
        {
            PrintGotten(parameter, defaultParameter, false);
            IParameter p;
            if (_exists(parameter))
                p = parameter;
            else if (_exists(defaultParameter))
                p = defaultParameter;
            else
            {
                throw new ParamClassLoadException("No type name provided.\nPARAMETER: " + parameter 
                    + "\n     ALSO: " + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter));
            }
            try
            {
                var c = Type.GetType(Take(p));
                if (!mustCastToBaseType.IsAssignableFrom(c))
                {
                    throw new ParamClassLoadException("The type " + c.FullName + "\ndoes not cast into the base type " 
                        + mustCastToBaseType.FullName + "\nPARAMETER: " + parameter 
                        + "\n     ALSO: " + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter));
                }
                return Activator.CreateInstance(c);
            }
            // BRS : 2009-03-15
            // Calling Type.GetType(string s) throws TypeLoadException if the typeName (s) is invalid
            catch (TypeLoadException e)
            {
                throw new ParamClassLoadException("Type not found: " + Take(p) + "\nPARAMETER: " + parameter + "\n     ALSO: " 
                    + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" + e);
            }
            catch (TargetInvocationException e)
            {
                throw new ParamClassLoadException("Could not load type: " + Take(p) + "\nPARAMETER: " + parameter + "\n     ALSO: " 
                    + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" + e);
            }
            catch (MemberAccessException e)
            {
                throw new ParamClassLoadException("The requested type is an interface or an abstract type: " + Take(p) + "\nPARAMETER: " 
                    + parameter + "\n     ALSO: " + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" + e);
            }
            catch (Exception e)
            {
                throw new ParamClassLoadException("The requested type cannot be initialized with the default initializer: " 
                    + Take(p) + "\nPARAMETER: " + parameter + "\n     ALSO: " + (defaultParameter == null ? "" : "\n     ALSO: " 
                    + defaultParameter) + "\nEXCEPTION: \n\n" + e);
            }
        }
        
        /// <summary> 
        /// Searches down through databases to find a given parameter. The value
        /// associated with this parameter must be a full Class name, and the class
        /// must be a descendent of but not equal to <i>mustCastTosuperclass </i>.
        /// Loads and returns the associated Class, or throws a
        /// ParamClassLoadException if there is no such Class. If the parameter is
        /// not found, the defaultParameter is used. The parameter chosen is marked
        /// "used".
        /// </summary>
        public virtual Type GetTypeForParameter(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseType)
        {
            PrintGotten(parameter, defaultParameter, false);
            IParameter p;
            if (_exists(parameter))
                p = parameter;
            else if (_exists(defaultParameter))
                p = defaultParameter;
            else
            {
                throw new ParamClassLoadException("No type name provided.\nPARAMETER: " + parameter + "\n     ALSO: " 
                    + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter));
            }
            try
            {
                var c = Type.GetType(Take(p));
                if (!mustCastToBaseType.IsAssignableFrom(c))
                {
                    throw new ParamClassLoadException("The type " + c.FullName + "\ndoes not cast into the base type " 
                        + mustCastToBaseType.FullName + "\nPARAMETER: " + parameter + "\n     ALSO: " 
                        + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter));
                }
                return c;
            }
            // BRS : 2009-03-15
            // Calling Type.GetType(string s) throws TypeLoadException if the typeName (s) is invalid
            catch (TypeLoadException e)
            {
                throw new ParamClassLoadException("Type not found: " + Take(p) + "\nPARAMETER: " + parameter + "\n     ALSO: " 
                    + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" + e);
            }
            catch (Exception e)
            {
                throw new ParamClassLoadException("Could not load type: " + Take(p) + "\nPARAMETER: " + parameter + "\n     ALSO: " 
                    + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" + e);
            }
        }

        #endregion //Get Instance and Type

        #region Get Boolean   ********************************************************************************

        /// <summary> 
        /// Searches down through databases to find a given parameter; If the
        /// parameter does not exist, defaultValue is returned. If the parameter
        /// exists, and it is set to "false" (case insensitive), false is returned.
        /// Else true is returned. The parameter chosen is marked "used" if it
        /// exists.
        /// </summary>
        public virtual bool GetBoolean(IParameter parameter, IParameter defaultParameter, bool defaultValue)
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetBoolean(parameter, defaultValue);

            return GetBoolean(defaultParameter, defaultValue);
        }
        
        #endregion // Get Boolean

        #region Get Int   ************************************************************************************

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be an integer. It returns the value, else throws a
        /// NumberFormatException exception if there is an error in parsing the
        /// parameter. The parameter chosen is marked "used" if it exists. Integers
        /// may be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual int GetInt(IParameter parameter, IParameter defaultParameter)
        {
            PrintGotten(parameter, defaultParameter, false);
            if (_exists(parameter))
                return GetInt(parameter);
            if (_exists(defaultParameter))
                return GetInt(defaultParameter);

            throw new FormatException("Integer does not exist for either parameter " + parameter + "\nor\n" + defaultParameter);
        }
        
        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be an integer >= minValue. It returns the value, or minValue-1 if
        /// the value is out of range or if there is an error in parsing the
        /// parameter. The parameter chosen is marked "used" if it exists. Integers
        /// may be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual int GetInt(IParameter parameter, IParameter defaultParameter, int minValue)
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetInt(parameter, minValue);

            return GetInt(defaultParameter, minValue);
        }
        
        /// <summary> 
        /// Searches down through databases to find a given parameter, which must be
        /// an integer. If there is an error in parsing the parameter, then default
        /// is returned. The parameter chosen is marked "used" if it exists. Integers
        /// may be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual int GetIntWithDefault(IParameter parameter, IParameter defaultParameter, int defaultValue)
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetIntWithDefault(parameter, defaultValue);

            return GetIntWithDefault(defaultParameter, defaultValue);
        }
        
        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be an integer >= minValue and &lt;= maxValue. It returns the value, or
        /// minValue-1 if the value is out of range or if there is an error in
        /// parsing the parameter. The parameter chosen is marked "used" if it
        /// exists. Integers may be in decimal or (if preceded with an X or x) in
        /// hexadecimal.
        /// </summary>
        public virtual int GetIntWithMax(IParameter parameter, IParameter defaultParameter, int minValue, int maxValue)
        {
            PrintGotten(parameter, defaultParameter, false);
            if (_exists(parameter))
                return GetIntWithMax(parameter, minValue, maxValue);

            return GetIntWithMax(defaultParameter, minValue, maxValue);
        }
        
        #endregion // Get Int

        #region Get Float   **********************************************************************************

        /// <summary>
        /// Searches down through databases to find a given parameter, whose value
        /// must be a float. It returns the value, else throws a
        /// NumberFormatException exception if there is an error in parsing the
        /// parameter. The parameter chosen is marked "used" if it exists.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="defaultParameter"></param>
        /// <returns></returns>
        public float GetFloat(IParameter parameter, IParameter defaultParameter) // throws NumberFormatException 
        {
            PrintGotten(parameter, defaultParameter, false);
            if (_exists(parameter))
                return GetFloat(parameter);
            if (_exists(defaultParameter))
                return GetFloat(defaultParameter);

            throw new FormatException("Float does not exist for either parameter "
                                      + parameter + "\nor\n" + defaultParameter);
        }

        /// <summary>
        /// Searches down through databases to find a given parameter, whose value
        /// must be a float >= minValue. If not, this method returns minvalue-1, else
        /// it returns the parameter value. The parameter chosen is marked "used" if
        /// it exists.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="defaultParameter"></param>
        /// <param name="minValue"></param>
        /// <returns></returns>
        public virtual float GetFloat(IParameter parameter, IParameter defaultParameter, double minValue)
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetFloat(parameter, minValue);

            return GetFloat(defaultParameter, minValue);
        }
        
        /// <summary> 
        /// Searches down through databases to find a given parameter, which must be
        /// a float. If there is an error in parsing the parameter, then default is
        /// returned. The parameter chosen is marked "used" if it exists.
        /// </summary>
        public virtual float GetFloatWithDefault(IParameter parameter, IParameter defaultParameter, double defaultValue)
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetFloatWithDefault(parameter, defaultValue);

            return GetFloatWithDefault(defaultParameter, defaultValue);
        }

        /// <summary>
        /// Searches down through databases to find a given parameter, whose value
        /// must be a float >= minValue and &lt;= maxValue. If not, this method returns
        /// minvalue-1, else it returns the parameter value. The parameter chosen is
        /// marked "used" if it exists.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="defaultParameter"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public float GetFloatWithMax(IParameter parameter, IParameter defaultParameter, double minValue, double maxValue)
        {
            PrintGotten(parameter, defaultParameter, false);
            if (_exists(parameter))
                return GetFloat(parameter, minValue, maxValue);

            return GetFloat(defaultParameter, minValue, maxValue);
        }
        
        #endregion // Get Float

        #region Get Double   *********************************************************************************

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be an double. It returns the value, else throws a
        /// NumberFormatException exception if there is an error in parsing the
        /// parameter. The parameter chosen is marked "used" if it exists. Integers
        /// may be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual double GetDouble(IParameter parameter, IParameter defaultParameter)
        {
            PrintGotten(parameter, defaultParameter, false);
            if (_exists(parameter))
                return GetDouble(parameter);
            if (_exists(defaultParameter))
                return GetDouble(defaultParameter);

            throw new FormatException("Double does not exist for either parameter " + parameter + "\nor\n" + defaultParameter);
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a double >= minValue. If not, this method returns minvalue-1,
        /// else it returns the parameter value. The parameter chosen is marked
        /// "used" if it exists.
        /// </summary>
        public virtual double GetDouble(IParameter parameter, IParameter defaultParameter, double minValue)
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetDouble(parameter, minValue);

            return GetDouble(defaultParameter, minValue);
        }
        
        /// <summary> 
        /// Searches down through databases to find a given parameter, which must be
        /// a float. If there is an error in parsing the parameter, then default is
        /// returned. The parameter chosen is marked "used" if it exists.
        /// </summary>
        public virtual double GetDoubleWithDefault(IParameter parameter, IParameter defaultParameter, double defaultValue)
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetDoubleWithDefault(parameter, defaultValue);

            return GetDoubleWithDefault(defaultParameter, defaultValue);
        }
        
        /// <summary>
        /// Searches down through databases to find a given parameter, whose value
        /// must be a double >= minValue and &lt;= maxValue. If not, this method returns
        /// minvalue-1, else it returns the parameter value. The parameter chosen is
        /// marked "used" if it exists.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="defaultParameter"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public double GetDoubleWithMax(IParameter parameter, IParameter defaultParameter, double minValue, double maxValue)
        {
            PrintGotten(parameter, defaultParameter, false);
            if (_exists(parameter))
                return GetDouble(parameter, minValue, maxValue);

            return GetDouble(defaultParameter, minValue, maxValue);
        }

        const int ARRAY_NO_EXPECTED_LENGTH = -1;

        double[] GetDoublesWithMax(IParameter parameter, double minValue, double maxValue, int expectedLength)
        {
            if (_exists(parameter))
            {
                DoubleBag bag = new DoubleBag();
                // TODO: BRS: This has to be a stream, so find out if the parameter 
                // is supposed to be a file or just a string that needs to be wrapped in StringStream.
                // For now, we're going to assume it's a filePath.
                //Scanner scanner = new Scanner(get(parameter));
                Scanner scanner = new Scanner(GetResource(parameter));
                while (scanner.HasNextDouble())
                {
                    if (expectedLength != ARRAY_NO_EXPECTED_LENGTH && bag.Size >= expectedLength)
                        return null;  // too big

                    double val = scanner.NextDouble();
                    //if (val != val || val > maxValue || val < minValue)
                    if (double.IsNaN(val) || double.IsPositiveInfinity(val) || double.IsNegativeInfinity(val))
                        return null;
                    else
                    { bag.Add(val); }
                }
                if (scanner.HasNext())
                    return null;  // too long, or garbage afterwards
                if (expectedLength != ARRAY_NO_EXPECTED_LENGTH && bag.Size != expectedLength)
                    return null;
                if (bag.Size == 0)
                    return null;            // 0 lengths not permitted
                return bag.ToArray();
            }
            else
            {
                return null;
            }
        }


        double[] GetDoublesWithMax(IParameter parameter, double minValue, double maxValue)
        {
            return GetDoublesWithMax(parameter, minValue, maxValue, ARRAY_NO_EXPECTED_LENGTH);
        }

        double[] GetDoubles(IParameter parameter, double minValue, int expectedLength)
        {
            return GetDoublesWithMax(parameter, minValue, double.PositiveInfinity, expectedLength);
        }

        double[] GetDoubles(IParameter parameter, double minValue)
        {
            return GetDoublesWithMax(parameter, minValue, double.PositiveInfinity, ARRAY_NO_EXPECTED_LENGTH);
        }

        double[] GetDoublesUnconstrained(IParameter parameter, int expectedLength)
        {
            return GetDoublesWithMax(parameter, double.NegativeInfinity, double.PositiveInfinity, expectedLength);
        }

        double[] GetDoublesUnconstrained(IParameter parameter)
        {
            return GetDoublesWithMax(parameter, double.NegativeInfinity, double.PositiveInfinity, ARRAY_NO_EXPECTED_LENGTH);
        }



        /**
         * Searches down through databases to find a given parameter, whose value
         * must be a space- or tab-delimited list of doubles, each of which is >= minValue and <= maxValue,
         * and which must be exactly expectedLength (> 0) long.  If the parameter does not exist,
         * or any of its doubles are out of bounds, or the list is not long enough or is  
         * too long or has garbage at the end of it, then this method returns null.
         * Otherwise the method returns the doubles in question.  The doubles may not
         * be NaN, +Infinity, or -Infinity. The parameter chosen is
         * marked "used" if it exists.
         */

        public double[] GetDoublesWithMax(IParameter parameter, IParameter defaultParameter, double minValue, double maxValue, int expectedLength)
        {
            PrintGotten(parameter, defaultParameter, false);
            if (_exists(parameter))
                return GetDoublesWithMax(parameter, minValue, maxValue, expectedLength);
            else
                return GetDoublesWithMax(defaultParameter, minValue, maxValue, expectedLength);
        }

        /**
         * Searches down through databases to find a given parameter, whose value
         * must be a space- or tab-delimited list of doubles, each of which is >= minValue and <= maxValue,
         * and which must be at least 1 number long.  If the parameter does not exist,
         * or any of its doubles are out of bounds, or the list is not long enough or is  
         * too long or has garbage at the end of it, then this method returns null.
         * Otherwise the method returns the doubles in question.  The doubles may not
         * be NaN, +Infinity, or -Infinity. The parameter chosen is
         * marked "used" if it exists.
         */

        public double[] GetDoublesWithMax(IParameter parameter, IParameter defaultParameter, double minValue, double maxValue)
        {
            PrintGotten(parameter, defaultParameter, false);
            if (_exists(parameter))
                return GetDoublesWithMax(parameter, minValue, maxValue);
            else
                return GetDoublesWithMax(defaultParameter, minValue, maxValue);
        }

        /**
         * Searches down through databases to find a given parameter, whose value
         * must be a space- or tab-delimited list of doubles, each of which is >= minValue,
         * and which must be exactly expectedLength (> 0) long.  If the parameter does not exist,
         * or any of its doubles are out of bounds, or the list is not long enough or is  
         * too long or has garbage at the end of it, then this method returns null.
         * Otherwise the method returns the doubles in question.  The doubles may not
         * be NaN, +Infinity, or -Infinity. The parameter chosen is
         * marked "used" if it exists.
         */

        public double[] GetDoubles(IParameter parameter, IParameter defaultParameter, double minValue, int expectedLength)
        {
            PrintGotten(parameter, defaultParameter, false);
            if (_exists(parameter))
                return GetDoubles(parameter, minValue, expectedLength);
            else
                return GetDoubles(defaultParameter, minValue, expectedLength);
        }

        /**
         * Searches down through databases to find a given parameter, whose value
         * must be a space- or tab-delimited list of doubles, each of which is >= minValue,
         * and which must be at least 1 number long.  If the parameter does not exist,
         * or any of its doubles are out of bounds, or the list is not long enough or is  
         * too long or has garbage at the end of it, then this method returns null.
         * Otherwise the method returns the doubles in question.  The doubles may not
         * be NaN, +Infinity, or -Infinity. The parameter chosen is
         * marked "used" if it exists.
         */

        public double[] GetDoubles(IParameter parameter, IParameter defaultParameter, double minValue)
        {
            PrintGotten(parameter, defaultParameter, false);
            if (_exists(parameter))
                return GetDoubles(parameter, minValue);
            else
                return GetDoubles(defaultParameter, minValue);
        }

        /**
         * Searches down through databases to find a given parameter, whose value
         * must be a space- or tab-delimited list of doubles,
         * and which must be exactly expectedLength (> 0) long.  If the parameter does not exist,
         * or the list is not long enough or is  
         * too long or has garbage at the end of it, then this method returns null.
         * Otherwise the method returns the doubles in question.  The doubles may not
         * be NaN, +Infinity, or -Infinity. The parameter chosen is
         * marked "used" if it exists.
         */

        public double[] GetDoublesUnconstrained(IParameter parameter, IParameter defaultParameter, int expectedLength)
        {
            PrintGotten(parameter, defaultParameter, false);
            if (_exists(parameter))
                return GetDoublesUnconstrained(parameter, expectedLength);
            else
                return GetDoublesUnconstrained(defaultParameter, expectedLength);
        }

        /**
         * Searches down through databases to find a given parameter, whose value
         * must be a space- or tab-delimited list of doubles,
         * and which must be at least 1 number long.  If the parameter does not exist,
         * or the list is not long enough or is  
         * too long or has garbage at the end of it, then this method returns null.
         * Otherwise the method returns the doubles in question.  The doubles may not
         * be NaN, +Infinity, or -Infinity. The parameter chosen is
         * marked "used" if it exists.
         */

        public double[] GetDoublesUnconstrained(IParameter parameter, IParameter defaultParameter)
        {
            PrintGotten(parameter, defaultParameter, false);
            if (_exists(parameter))
                return GetDoublesUnconstrained(parameter);
            else
                return GetDoublesUnconstrained(defaultParameter);
        }

        #endregion // Get Double

        #region Get Long   ***********************************************************************************

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a long. It returns the value, else throws a NumberFormatException
        /// exception if there is an error in parsing the parameter. The parameter
        /// chosen is marked "used" if it exists. Longs may be in decimal or (if
        /// preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual long GetLong(IParameter parameter, IParameter defaultParameter)
        {
            PrintGotten(parameter, defaultParameter, false);
            if (_exists(parameter))
                return GetLong(parameter);
            if (_exists(defaultParameter))
                return GetLong(defaultParameter);

            throw new FormatException("Long does not exist for either parameter " + parameter + "\nor\n" + defaultParameter);
        }
        
        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a long >= minValue. If not, this method returns errValue, else it
        /// returns the parameter value. The parameter chosen is marked "used" if it
        /// exists. Longs may be in decimal or (if preceded with an X or x) in
        /// hexadecimal.
        /// </summary>	
        public virtual long GetLong(IParameter parameter, IParameter defaultParameter, long minValue)
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetLong(parameter, minValue);

            return GetLong(defaultParameter, minValue);
        }
        
        /// <summary> 
        /// Searches down through databases to find a given parameter, which must be
        /// a long. If there is an error in parsing the parameter, then default is
        /// returned. The parameter chosen is marked "used" if it exists. Longs may
        /// be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual long GetLongWithDefault(IParameter parameter, IParameter defaultParameter, long defaultValue)
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetLongWithDefault(parameter, defaultValue);

            return GetLongWithDefault(defaultParameter, defaultValue);
        }
        
        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a long >= minValue and = &lt; maxValue. If not, this method returns
        /// errValue, else it returns the parameter value. The parameter chosen is
        /// marked "used" if it exists. Longs may be in decimal or (if preceded with
        /// an X or x) in hexadecimal.
        /// </summary>
        public virtual long GetLongWithMax(IParameter parameter, IParameter defaultParameter, long minValue, long maxValue)
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetLongWithMax(parameter, defaultParameter, minValue, maxValue);

            return GetLongWithMax(defaultParameter, minValue, maxValue);
        }

        #endregion // Get Long

        #region GetValueType<T>   ***********************************************************************************

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a value type. It returns the value, else throws a NumberFormatException
        /// exception if there is an error in parsing the parameter. The parameter
        /// chosen is marked "used" if it exists. Value type may be in decimal or (if
        /// preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual T GetValueType<T>(IParameter parameter, IParameter defaultParameter)
             where T : struct, IConvertible, IComparable
        {
            PrintGotten(parameter, defaultParameter, false);
            if (_exists(parameter))
                return GetValueType<T>(parameter);
            if (_exists(defaultParameter))
                return GetValueType<T>(defaultParameter);

            throw new FormatException("Long does not exist for either parameter " + parameter + "\nor\n" + defaultParameter);
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a value type >= minValue. If not, this method returns errValue, else it
        /// returns the parameter value. The parameter chosen is marked "used" if it
        /// exists. Value type may be in decimal or (if preceded with an X or x) in
        /// hexadecimal.
        /// </summary>	
        public virtual T GetValueType<T>(IParameter parameter, IParameter defaultParameter, T minValue)
             where T : struct, IConvertible, IComparable
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetValueType<T>(parameter, minValue);

            return GetValueType<T>(defaultParameter, minValue);
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, which must be
        /// a value type. If there is an error in parsing the parameter, then default is
        /// returned. The parameter chosen is marked "used" if it exists. Value type may
        /// be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual T GetValueTypeWithDefault<T>(IParameter parameter, IParameter defaultParameter, T defaultValue)
             where T : struct, IConvertible, IComparable
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetValueTypeWithDefault(parameter, defaultValue);

            return GetValueTypeWithDefault(defaultParameter, defaultValue);
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a value type >= minValue and = &lt; maxValue. If not, this method returns
        /// errValue, else it returns the parameter value. The parameter chosen is
        /// marked "used" if it exists. Value type may be in decimal or (if preceded with
        /// an X or x) in hexadecimal.
        /// </summary>
        public virtual T GetValueTypeWithMax<T>(IParameter parameter, IParameter defaultParameter, T minValue, T maxValue)
             where T : struct, IConvertible, IComparable
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetValueTypeWithMax(parameter, defaultParameter, minValue, maxValue);

            return GetValueTypeWithMax(defaultParameter, minValue, maxValue);
        }

        #endregion // Get Long

        #region Get File   ***********************************************************************************

        /// <summary> 
        /// Searches down through the databases to find a given parameter, whose
        /// value must be an absolute or relative path name. If it is absolute, a
        /// File is made based on the path name. If it is relative, a file is made by
        /// resolving the path name with respect to the directory in which the file
        /// was which defined this ParameterDatabase in the ParameterDatabase
        /// hierarchy. If the parameter is not found, this returns null. The File is
        /// not checked for validity. The parameter chosen is marked "used" if it
        /// exists.
        /// </summary>
        public virtual FileInfo GetFile(IParameter parameter, IParameter defaultParameter)
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetFile(parameter);

            return GetFile(defaultParameter);
        }
        
        #endregion // Get File

        #region Get String   *********************************************************************************

        /// <summary> Searches down through databases to find a given parameter. Returns the
        /// parameter's value (trimmed) or null if not found or if the trimmed result
        /// is empty. The parameter chosen is marked "used" if it exists.
        /// </summary>		
        public virtual string GetString(IParameter parameter, IParameter defaultParameter)
        {
            lock (this)
            {
                PrintGotten(parameter, defaultParameter, false);

                if (_exists(parameter))
                    return GetString(parameter);

                return GetString(defaultParameter);
            }
        }
        
        /// <summary> 
        /// Searches down through databases to find a given parameter. Returns the
        /// parameter's value trimmed of whitespace, or defaultValue.trim() if the
        /// result is not found or the trimmed result is empty.
        /// </summary>
        public virtual string GetStringWithDefault(IParameter parameter, IParameter defaultParameter, string defaultValue)
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetStringWithDefault(parameter, defaultValue);

            return GetStringWithDefault(defaultParameter, defaultValue);
        }
        
        #endregion // Get String

        #region Events   *************************************************************************************

        public event ParameterDatabaseListenerDelegate ParameterDatabaseListenerDelegateVar;
        public virtual void  AddListener(ParameterDatabaseListener l)
        {
            lock (this) { listeners.Add(l); }
        }	
        public virtual void  RemoveListener(ParameterDatabaseListener l)
        {
            lock (this) { listeners.Remove(l); }
        }
        
        #endregion // Events

        #region Get, Set, Remove   ***************************************************************************

        /// <summary> 
        /// Sets a parameter in the topmost database to a given value, trimmed of whitespace.
        /// </summary>
        public virtual void SetParameter(IParameter parameter, string paramValue)
        {
            lock (this)
            {
                var val = paramValue.Trim();
                //object tempObject = this[parameter.Param];
                this[parameter.Param] = val;
                FireParameterSet(parameter, val);
            }
        }

        /// <summary>
        /// Removes a parameter from the topmost database. 
        /// </summary>
        public void Remove(IParameter parameter)
        {
            lock (this)
            {
                if (parameter.Param.Equals(PRINT_PARAMS))
                    printState = PS_UNKNOWN;
                Remove(parameter.Param);
            }
        }

        /// <summary>
        /// Removes a parameter from the database and all its parent databases. 
        /// </summary>
        public virtual void RemoveDeeply(IParameter parameter)
        {
            lock (this)
            {
                _removeDeeply(parameter);
                Uncheck();
            }
        }

        public virtual HashSet<IParameter> GetShadowedValues(IParameter parameter)
        {
            var vals = new HashSet<IParameter>();
            vals = _getShadowedValues(parameter, vals);
            Uncheck();
            return vals;
        }

        #endregion // Set, Take, Remove, Get (low-level)

        #region File   ***************************************************************************************

        /// <summary> 
        /// Searches down through databases to find the directory for the database
        /// which holds a given parameter. Returns the directory name or null if not found.
        /// </summary>		
        public virtual FileInfo DirectoryFor(IParameter parameter)
        {
            var result = _directoryFor(parameter);
            Uncheck();
            return result;
        }
        
        /// <summary> 
        /// Searches down through databases to find the parameter file 
        /// which holds a given parameter. Returns the filename or null if not found.
        /// </summary>		
        public virtual FileInfo FileFor(IParameter parameter)
        {
            var result = _fileFor(parameter);
            Uncheck();
            return result;
        }

        #endregion // File

        #region Resource

        public Stream GetResource(IParameter parameter, IParameter defaultParameter)
        {
            PrintGotten(parameter, defaultParameter, false);
            if (_exists(parameter))
                return GetResource(parameter);
            return GetResource(defaultParameter);
        }

        public Stream GetResource(IParameter parameter)
        {
            // TODO : Fix this!
            //throw new NotImplementedException();
            try
            {
                if (_exists(parameter))
                {
                    var p = GetString(parameter);
                    if (p == null)
                        return null;
                    if (p.StartsWith(C_HERE))
                        return new FileStream(GetFile(parameter).FullName, FileMode.Open);

                    // BRS: I think what below is trying to do is retrieve a resource from
                    //      an assembly. So we would have to determine which assembly using
                    //      Assembly.GetAssembly(typeof(T)) and then get the resource stream.

                    if (p.StartsWith(C_CLASS))
                    {
                        // TODO: Implement returning an embedded resource as a stream.
                        throw new NotImplementedException("This may be trying to access an embedded resource (not currently supported).");
                        //    var i = IndexOfFirstWhitespace(p);
                        //    if (i == -1)
                        //        return null;
                        //    var classname = p.Substring(0, i);
                        //    var filename = p.Substring(i).Trim();
                        //    return Type.GetType(classname).getResourceAsStream(filename);
                    }

                    var f = new FileInfo(p);
                    if (Path.IsPathRooted(f.FullName))
                        return new FileStream(f.FullName, FileMode.Open);

                    // BRS: Not sure what this is all about!
                    //Type c = GetLocation(parameter.Param).relativeClass;
                    //String rp = GetLocation(parameter.Param).relativePath;
                    //if (c != null)
                    //{
                    //    return c.GetResourceAsStream(new File(new File(rp).GetParent(), p).GetPath());
                    //}

                    return new FileStream(DirectoryFor(parameter).FullName, FileMode.Open);
                }
                return null;
            }
            catch (FileNotFoundException ex1) { return null; }
            catch (TypeLoadException ex2) { return null; }
            catch (Exception ex3)
            {
                return null;
            }
        }

        #endregion // Resource

        #region Constructors *********************************************************************************

        /// <summary>
        /// Creates an empty parameter database. 
        /// </summary>
        public ParametersTree()
        {
            accessed = new Dictionary<string, bool>();
            gotten = new Dictionary<string, bool>();
            directory = new FileInfo("."); // uses the user path
            filename = "";
            Parents = new List<IParameterDatabase>(10);
            checkState = false; // unnecessary
            listeners = new List<ParameterDatabaseListener>(10);
        }
        
        /// <summary>
        /// Creates a new parameter database from the given Dictionary.  
        /// Both the keys and values will be run through ToString() before adding to the database.   
        /// Keys are parameters.  Values are the values of the parameters.  
        /// Beware that a ParameterDatabase is itself a Dictionary; but if you pass one in here you 
        /// will only get the lowest-level elements.  If parent.n are defined, Parents will 
        /// be attempted to be loaded -- that's the reason for the FileNotFoundException and IOException.  
        /// </summary>
        public ParametersTree(IDictionary map) : this()
        {
            var keys = map.Keys.GetEnumerator();
            while (keys.MoveNext())
            {
                var obj = keys.Current;
                SetParameter(new Parameter("" + obj), "" + map[obj]);
            }
            
            // load Parents
            for (var x = 0; ; x++)
            {
                var s = GetProperty("parent." + x);
                if (s == null)
                    return ; // we're done
                
                // BRS : 2009-03-15
                // if (new FileInfo(s).isAbsolute())
                if (Path.IsPathRooted(s))
                    // it's an absolute file definition
                    Parents.Add(new ParameterDatabase(new FileInfo(s)));
                else
                    throw new FileNotFoundException("Attempt to load a relative file, but there's no parent file: " + s);
            }
        }
        
        /// <summary>
        /// Creates a new parameter database loaded from the given stream.  Non-relative Parents are not permitted.
        /// If parent.n are defined, Parents will be attempted to be loaded -- that's 
        /// the reason for the FileNotFoundException and IOException. 
        /// </summary>
        public ParametersTree(Stream stream) : this()
        {
            Load(stream);

            listeners = new List<ParameterDatabaseListener>();

            // load Parents
            for (var x = 0; ; x++)
            {
                var s = GetProperty("parent." + x);
                if (s == null)
                    return; // we're done

                // BRS : 2009-03-15
                // if (new FileInfo(s).isAbsolute())
                if (Path.IsPathRooted(s))
                    // it's an absolute file definition
                    Parents.Add(new ParameterDatabase(new FileInfo(s)));
                else
                    throw new FileNotFoundException("Attempt to load a relative file, but there's no parent file: " + s);
            }
        }
                
        /// <summary> 
        /// Creates a new parameter database tree from a given database file and its parent files.
        /// </summary>
        public ParametersTree(FileInfo fileInfo) : this()
        {
            // BRS : TODO : Requires further conversion.
            filename = fileInfo.Name;
            directory = new FileInfo(fileInfo.DirectoryName); // get the directory filename is in

            Load(new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read));

            listeners = new List<ParameterDatabaseListener>(10);
            
            // load Parents
            for (var x = 0; ; x++)
            {
                var s = GetProperty("parent." + x);
                if (s == null)
                    return ; // we're done

                // BRS : 2009-03-15
                // if (new FileInfo(s).isAbsolute())
                if (Path.IsPathRooted(s))
                    // it's an absolute file definition
                    Parents.Add(new ParameterDatabase(new FileInfo(s)));
                // it's relative to my path
                else
                    Parents.Add(new ParameterDatabase(new FileInfo(Path.Combine(fileInfo.DirectoryName, s))));
            }
        }
        
        /// <summary> 
        /// Creates a new parameter database from a given database file and argv
        /// list. The top-level database is completely empty, pointing to a second
        /// database which contains the parameter entries stored in args, which
        /// points to a tree of databases constructed using
        /// ParameterDatabase(filename).
        /// </summary>
        public ParametersTree(FileInfo fileInfo, string[] args)
            : this()
        {
            filename = fileInfo.Name;
            directory = new FileInfo(fileInfo.DirectoryName); // get the directory
            // filename is in
            
            // Create the Parameter Database tree for the files
            var files = new ParameterDatabase(fileInfo);
            
            // Create the Parameter Database for the arguments
            var a = new ParameterDatabase();
            a.Parents.Add(files);
            for (var x = 0; x < args.Length - 1; x++)
            {
                if (args[x].Equals("-p"))
                {
                    var s = args[x + 1].Trim();
                    if (s.Length == 0) continue;
                    var eq = s.IndexOf('=');
                    if (eq <= 0) continue;
                    this[s.Substring(0, eq)] = s.Substring(eq + 1);
                }
            }
            
            // Set me up
            Parents.Add(a);
            listeners = new List<ParameterDatabaseListener>(10);
        }

        #endregion // Constructors

        #region List (Print) *********************************************************************************

        /// <summary> 
        /// Prints out all the parameters marked as used, plus their values. If a
        /// parameter was listed as "used" but not's actually in the database, the
        /// value printed is UNKNOWN_VALUE (set to "?????")
        /// </summary>		
        public virtual void ListGotten(StreamWriter p)
        {
            lock (this)
            {
                var vec = new ArrayList(10);
                var e = gotten.Keys.GetEnumerator();

                while (e.MoveNext())
                {
                    vec.Add(e.Current);
                }

                // sort the keys
                var array = vec.ToArray();

                vec.Sort();

                // Uncheck and print each item
                for (var x = 0; x < array.Length; x++)
                {
                    var s = (string)array[x];
                    string v = null;
                    if (s != null)
                    {
                        v = _get(s);
                        Uncheck();
                    }
                    if (v == null)
                        v = UNKNOWN_VALUE;

                    p.WriteLine(s + " = " + v);
                }
                p.Flush();
            }
        }

        /// <summary>
        /// Prints out all the parameters NOT marked as used, plus their values. 
        /// </summary>		
        public virtual void ListNotGotten(StreamWriter p)
        {
            lock (this)
            {
                var vec = new List<string>(10);
                var all = new Dictionary<string, string>();

                _list(null, false, null, all); // grab all the nonshadowed keys

                //var e = gotten.Keys.GetEnumerator();

                //while (e.MoveNext())
                //{
                //    all.Remove(e.Current);
                //}

                foreach (var key in gotten.Keys)
                {
                    all.Remove(key);
                }

                //var i = all.Keys.GetEnumerator();

                //while (i.MoveNext())
                //{
                //    vec.Add(i.Current);
                //}

                foreach (var key in all.Keys)
                {
                    vec.Add(key);
                }

                // sort the keys
                var array = vec.ToArray();

                vec.Sort();

                // Uncheck and print each item
                for (var x = 0; x < array.Length; x++)
                {
                    var s = array[x];
                    string v = null;
                    if (s != null)
                    {
                        v = _get(s);
                        Uncheck();
                    }
                    if (v == null)
                        v = UNKNOWN_VALUE;

                    p.WriteLine(s + " = " + v);
                }
                p.Flush();
            }
        }

        /// <summary>
        /// Prints out all the parameters NOT marked as used, plus their values. 
        /// </summary>
        public virtual void ListNotAccessed(StreamWriter p)
        {
            lock (this)
            {
                var vec = new List<string>(10);
                var all = new Dictionary<string, string>();
                _list(null, false, null, all); // grab all the nonshadowed keys

                //var e = accessed.Keys.GetEnumerator();
                //while (e.MoveNext())
                //{
                //    all.Remove(e.Current);
                //}

                foreach (var key in accessed.Keys)
                {
                    all.Remove(key);
                }

                //var i = all.Keys.GetEnumerator();
                //while (i.MoveNext())
                //{
                //    vec.Add(i.Current);
                //}

                foreach (var key in all.Keys)
                {
                    vec.Add(key);
                }

                // sort the keys
                var array = vec.ToArray();

                vec.Sort();

                // Uncheck and print each item
                for (var x = 0; x < array.Length; x++)
                {
                    var s = array[x];
                    string v = null;
                    if (s != null)
                    {
                        v = _get(s);
                        Uncheck();
                    }
                    if (v == null)
                        v = UNKNOWN_VALUE;

                    p.WriteLine(s + " = " + v);
                }
                p.Flush();
            }
        }

        /// <summary> 
        /// Prints out all the parameters marked as accessed ("gotten" by some
        /// getFoo(...) method), plus their values. If this method ever prints
        /// UNKNOWN_VALUE ("?????"), that's a bug.
        /// </summary>
        public virtual void ListAccessed(StreamWriter p)
        {
            lock (this)
            {
                var vec = new List<string>(10);

                //var e = accessed.Keys.GetEnumerator();
                //while (e.MoveNext())
                //{
                //    vec.Add(e.Current);
                //}

                foreach (var key in accessed.Keys)
                {
                    vec.Add(key);
                }

                // sort the keys
                var array = vec.ToArray();

                vec.Sort();

                // Uncheck and print each item
                for (var x = 0; x < array.Length; x++)
                {
                    var s = array[x];
                    string v = null;
                    if (s != null)
                    {
                        v = _get(s);
                        Uncheck();
                    }
                    if (v == null)
                        v = UNKNOWN_VALUE;

                    p.WriteLine(s + " = " + v);
                }
                p.Flush();
            }
        }

        /// <summary> 
        /// Prints out all the parameters in the database. Useful for debugging. If
        /// listShadowed is true, each parameter is printed with the parameter
        /// database it's located in. If listShadowed is false, only active
        /// parameters are listed, and they're all given in one big chunk.
        /// </summary>
        // BRS : 2009-03-15 : Commented out duplicate method
        //public virtual void  list(StreamWriter p, bool listShadowed)
        //{
        //    list(new StreamWriter(p.BaseStream, Encoding.Default), listShadowed);
        //}

        /// <summary> Prints out all the parameters in the database, but not shadowed
        /// parameters.
        /// </summary>
        // BRS : 2009-03-15 : Commented out duplicate method
        //public void  list(StreamWriter p)
        //{
        //    list(new StreamWriter(p.BaseStream, Encoding.Default), false);
        //}

        /// <summary> 
        /// Prints out all the parameters in the database, but not shadowed parameters.
        /// </summary>
        public void List(StreamWriter p)
        {
            List(p, false);
        }

        /// <summary> 
        /// Prints out all the parameters in the database. Useful for debugging. If
        /// listShadowed is true, each parameter is printed with the parameter
        /// database it's located in. If listShadowed is false, only active
        /// parameters are listed, and they're all given in one big chunk.
        /// </summary>
        public virtual void List(StreamWriter p, bool listShadowed)
        {
            if (listShadowed)
                _list(p, true, "root", null);
            else
            {
                var gather = new Dictionary<string, string>();
                _list(null, false, "root", gather);

                var vec = new List<string>(10);

                //var e = gather.Keys.GetEnumerator();

                //while (e.MoveNext())
                //{
                //    vec.Add(e.Current);
                //}

                foreach (var key in gather.Keys)
                {
                    vec.Add(key);
                }

                // sort the keys
                vec.Sort();

                // Uncheck and print each item
                for (var x = 0; x < vec.Count; x++)
                {
                    var s = vec[x];
                    string v = null;
                    if (s != null)
                        v = gather[s];
                    if (v == null)
                        v = UNKNOWN_VALUE;
                    if (p != null)
                    {
                        p.WriteLine(s + " = " + v);
                    }
                }
            }
            if (p != null)
                p.Flush();
        }

        /// <summary>
        /// Private helper function. 
        /// </summary>
        internal virtual void _list(StreamWriter writer, bool listShadowed, string prefix, IDictionary<string, string> gather)
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
                    ((ParameterDatabase)Parents[x])._list(writer, true, prefix + "." + x, gather);
            }
            else
            {
                // load in reverse order so things get properly overwritten
                var size = Parents.Count;
                for (var x = size - 1; x >= 0; x--)
                    ((ParameterDatabase)Parents[x])._list(writer, false, prefix, gather);

                //var e = Keys.GetEnumerator();
                //while (e.MoveNext())
                //{
                //    var key = (string)e.Current;
                //    gather[key] = (string) this[key];
                //}

                foreach (var key in Keys)
                {
                    gather[(string)key] = (string)this[key];
                }
            }
            if (writer != null)
                writer.Flush();
        }

        public override string ToString()
        {
            var s = CollectionsSupport.CollectionToString(this);
            if (Parents.Count > 0)
            {
                s += " : (";
                for (var x = 0; x < Parents.Count; x++)
                {
                    if (x > 0)
                        s += ", ";
                    s += Parents[x];
                }
                s += ")";
            }
            return s;
        }

        #endregion // List (Print)
        
        #region Tree Model    ********************************************************************************

        public virtual void AddParent(IParameterDatabase database)
        {
            Parents.Add(database);
        }

        #endregion // Tree Model 

        /// <summary>
        /// Test the ParameterDatabase 
        /// </summary>
        [STAThread]
        public static void  Main(string[] args)
        {
            var pd = new ParameterDatabase(new FileInfo(args[0]), args);
            pd.SetParameter(new Parameter("Hi there"), "Whatever");
            pd.SetParameter(new Parameter(new[]{"1", "2", "3"}), " Whatever ");
            pd.SetParameter(new Parameter(new[]{"a", "b", "c"}).Pop().Push("d"), "Whatever");
            
            Console.Error.WriteLine("\n\n PRINTING ALL PARAMETERS \n\n");
            var temp_writer = new StreamWriter(Console.OpenStandardError(), Encoding.Default) { AutoFlush = true };
            pd.List(temp_writer, true);

            Console.Error.WriteLine("\n\n PRINTING ONLY VALID PARAMETERS \n\n");
            var temp_writer2 = new StreamWriter(Console.OpenStandardError(), Encoding.Default) { AutoFlush = true };
            pd.List(temp_writer2, false);
        }
    }
}