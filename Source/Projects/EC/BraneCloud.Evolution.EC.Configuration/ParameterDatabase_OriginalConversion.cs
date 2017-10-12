using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows.Forms;

using BraneCloud.Evolution.EC.Support;

namespace BraneCloud.Evolution.EC.Configuration
{
    /// <summary> 
    /// <p>
    /// This extension of the Properties class allows you to set, get, and delete
    /// Parameters in a hierarchical tree-like database. The database consists of a
    /// list of Parameters, plus an array of "parent databases" which it falls back
    /// on when it can't find the Parameter you're looking for. Parents may also have
    /// arrays of Parents, and so on..
    /// 
    /// <p>
    /// The parameters are loaded from a Java property-list file, which is basically
    /// a collection of parameter=value pairs, one per line. Empty lines and lines
    /// beginning with # are ignored. These parameters and their values are
    /// <b>case-sensitive </b>, and whitespace is trimmed I believe.
    /// 
    /// <p>
    /// An optional set of parameters, "parent. <i>n </i>", where <i>n </i> are
    /// consecutive integers starting at 0, define the filenames of the database's
    /// Parents.
    /// 
    /// <p>
    /// An optional set of parameters, "print-params", specifies whether or not
    /// parameters should be printed as they are used (through one of the get(...)
    /// methods). If print-params is unset, or set to false or FALSE, nothing is
    /// printed. If set to non-false, then the parameters are printed prepended with a "P:"
    /// when their values are requested,  "E:" when their existence is tested.  Prior to the
    /// "P:" or "E:" you may see a "!" (meaning that the parameter isn't in the database),
    /// or a "&lt;" (meaning that the parameter was a default parameter which was never
    /// looked up because the primary parameter contained the value).
    /// 
    /// <p>
    /// <p>
    /// When you create a ParameterDatabase using new ParameterDatabase(), it is
    /// created thus:
    /// 
    /// <p>
    /// <table border=0 cellpadding=0 cellspacing=0>
    /// <tr>
    /// <td><tt>DATABASE:</tt></td>
    /// <td><tt>&nbsp;database</tt></td>
    /// </tr>
    /// <tr>
    /// <td><tt>FROM:</tt></td>
    /// <td><tt>&nbsp;(empty)</tt></td>
    /// </tr>
    /// </table>
    /// 
    /// 
    /// <p>
    /// When you create a ParameterDatabase using new ParameterDatabase( <i>file
    /// </i>), it is created by loading the database file, and its parent file tree,
    /// thus:
    /// 
    /// <p>
    /// <table border=0 cellpadding=0 cellspacing=0>
    /// <tr>
    /// <td><tt>DATABASE:</tt></td>
    /// <td><tt>&nbsp;database</tt></td>
    /// <td><tt>&nbsp;-&gt;</tt></td>
    /// <td><tt>&nbsp;parent0</tt></td>
    /// <td><tt>&nbsp;+-&gt;</tt></td>
    /// <td><tt>&nbsp;parent0</tt></td>
    /// <td><tt>&nbsp;+-&gt;</tt></td>
    /// <td><tt>&nbsp;parent0</tt></td>
    /// <td><tt>&nbsp;+-&gt;</tt></td>
    /// <td><tt>&nbsp;....</tt></td>
    /// </tr>
    /// <tr>
    /// <td><tt>FROM:</tt></td>
    /// <td><tt>&nbsp;(empty)</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;(file)</tt></td>
    /// <td><tt>&nbsp;|</tt></td>
    /// <td><tt>&nbsp;(parent.0)</tt></td>
    /// <td><tt>&nbsp;|</tt></td>
    /// <td><tt>&nbsp;(parent.0)</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;....</tt></td>
    /// </tr>
    /// <tr>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;|</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;+-&gt;</tt></td>
    /// <td><tt>&nbsp;parent1</tt></td>
    /// <td><tt>&nbsp;+-&gt;</tt></td>
    /// <td><tt>&nbsp;....</tt></td>
    /// </tr>
    /// <tr>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;|</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;|</tt></td>
    /// <td><tt>&nbsp;(parent.1)</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// </tr>
    /// <tr>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;|</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;....</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// </tr>
    /// <tr>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;|</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// </tr>
    /// <tr>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;+-&gt;</tt></td>
    /// <td><tt>&nbsp;parent1</tt></td>
    /// <td><tt>&nbsp;+-&gt;</tt></td>
    /// <td><tt>&nbsp;....</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// </tr>
    /// <tr>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;|</tt></td>
    /// <td><tt>&nbsp;(parent.1)</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// </tr>
    /// <tr>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;....</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// </tr>
    /// </table>
    /// 
    /// 
    /// <p>
    /// When you create a ParameterDatabase using new ParameterDatabase( <i>file,argv
    /// </i>), the preferred way, it is created thus:
    /// 
    /// 
    /// <p>
    /// <table border=0 cellpadding=0 cellspacing=0>
    /// <tr>
    /// <td><tt>DATABASE:</tt></td>
    /// <td><tt>&nbsp;database</tt></td>
    /// <td><tt>&nbsp;-&gt;</tt></td>
    /// <td><tt>&nbsp;parent0</tt></td>
    /// <td><tt>&nbsp;+-&gt;</tt></td>
    /// <td><tt>&nbsp;parent0</tt></td>
    /// <td><tt>&nbsp;+-&gt;</tt></td>
    /// <td><tt>&nbsp;parent0</tt></td>
    /// <td><tt>&nbsp;+-&gt;</tt></td>
    /// <td><tt>&nbsp;parent0</tt></td>
    /// <td><tt>&nbsp;+-&gt;</tt></td>
    /// <td><tt>&nbsp;....</tt></td>
    /// </tr>
    /// <tr>
    /// <td><tt>FROM:</tt></td>
    /// <td><tt>&nbsp;(empty)</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>(argv)</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;(file)</tt></td>
    /// <td><tt>&nbsp;|</tt></td>
    /// <td><tt>&nbsp;(parent.0)</tt></td>
    /// <td><tt>&nbsp;|</tt></td>
    /// <td><tt>&nbsp;(parent.0)</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;....</tt></td>
    /// </tr>
    /// <tr>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;|</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;+-&gt;</tt></td>
    /// <td><tt>&nbsp;parent1</tt></td>
    /// <td><tt>&nbsp;+-&gt;</tt></td>
    /// <td><tt>&nbsp;....</tt></td>
    /// </tr>
    /// <tr>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;|</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;|</tt></td>
    /// <td><tt>&nbsp;(parent.1)</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// </tr>
    /// <tr>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;|</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;....</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// </tr>
    /// <tr>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;|</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// </tr>
    /// <tr>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;+-&gt;</tt></td>
    /// <td><tt>&nbsp;parent1</tt></td>
    /// <td><tt>&nbsp;+-&gt;</tt></td>
    /// <td><tt>&nbsp;....</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// </tr>
    /// <tr>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;|</tt></td>
    /// <td><tt>&nbsp;(parent.1)</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// </tr>
    /// <tr>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;....</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// <td><tt>&nbsp;</tt></td>
    /// </tr>
    /// </table>
    /// 
    /// 
    /// <p>
    /// ...that is, the actual top database is empty, and stores parameters added
    /// programmatically; its parent is a database formed from arguments passed in on
    /// the command line; <i>its </i> parent is the parameter database which actually
    /// loads from foo. This allows you to programmatically add parameters which
    /// override those in foo, then delete them, thus bringing foo's parameters back
    /// in view.
    /// 
    /// <p>
    /// Once a parameter database is loaded, you query it with the <tt>get</tt>
    /// methods. The database, then its Parents, are searched until a match is found
    /// for your parameter. The search rules are thus: (1) the root database is
    /// searched first. (2) If a database being searched doesn't contain the data, it
    /// searches its Parents recursively, starting with parent 0, then moving up,
    /// until all searches are exhausted or something was found. (3) No database is
    /// searched twice.
    /// 
    /// <p>The various <tt>get</tt> methods all take two parameters.  The first
    /// parameter is fetched and retrieved first.  If that fails, the second one
    /// (known as the <i>default parameter</i>) is fetched and retrieved.  You
    /// can pass in <tt>null</tt> for the default parameter if you don't have one.
    /// 
    /// <p>You can test a parameter for existence with the <tt>exists</tt> methods.
    /// 
    /// <p>
    /// You can set a parameter (in the topmost database <i>only </i> with the
    /// <tt>set</tt> command. The <tt>remove</tt> command removes a parameter
    /// from the topmost database only. The <tt>removeDeeply</tt> command removes
    /// that parameter from every database.
    /// 
    /// <p>
    /// The values stored in a parameter database must not contain "#", "=",
    /// non-ascii values, or whitespace.
    /// 
    /// <p>
    /// <b>Note for JDK 1.1 </b>. Finally recovering from stupendous idiocy, JDK 1.2
    /// included parseDouble() and parseFloat() commands; now you can READ A FLOAT
    /// FROM A STRING without having to create a Float object first! Anyway, you will
    /// need to modify the getFloat() method below if you're running on JDK 1.1, but
    /// understand that large numbers of calls to the method may be inefficient.
    /// Sample JDK 1.1 code is given with those methods, but is commented out.
    /// </summary>
    [Serializable]
    public class ParameterDatabase_OriginalConversion : NameValueCollection, IParameterDatabase
    {
        private class AnonymousClassComparator : IComparer
        {
            public virtual int Compare(object o1, object o2)
            {
                var t1 = (ParameterDatabaseTreeNode)o1;
                var t2 = (ParameterDatabaseTreeNode)o2;

                return ((IComparable)t1.Tag).CompareTo(t2.Tag);
            }
        }
        public const string C_HERE = "$";
        public const string UNKNOWN_VALUE = "";
        public const string PRINT_PARAMS = "print-params";
        public const int PS_UNKNOWN = -1;
        public const int PS_NONE = 0;
        public const int PS_PRINT_PARAMS = 1;
        public int printState = PS_UNKNOWN;
        internal List<IParameterDatabase> Parents;
        internal FileInfo directory;
        internal string filename;
        internal bool checkState;
        internal Dictionary<string, bool> gotten;
        internal Dictionary<string, bool> accessed;
        internal List<ParameterDatabaseListener> listeners;

        public Dictionary<string, bool> Gotten { get { return gotten; } }
        public Dictionary<string, bool> Accessed { get { return accessed; } }
        public List<ParameterDatabaseListener> Listeners { get { return listeners; } }

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a full Class name, and the class must be a descendent of but not
        /// equal to <i>mustCastTosuperclass </i>. Loads the class and returns an
        /// instance (constructed with the default constructor), or throws a
        /// ParamClassLoadException if there is no such Class. If the parameter is
        /// not found, the defaultParameter is used. The parameter chosen is marked
        /// "used".
        /// </summary>
        public virtual object GetInstanceForParameter(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseclass)
        {
            PrintGotten(parameter, defaultParameter, false);
            IParameter p;
            if (_exists(parameter))
                p = parameter;
            else if (_exists(defaultParameter))
                p = defaultParameter;
            else
            {
                throw new ParamClassLoadException("No class name provided.\nPARAMETER: " + parameter + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter));
            }
            try
            {
                Type c = Type.GetType(Take(p));
                if (!mustCastToBaseclass.IsAssignableFrom(c))
                {
                    throw new ParamClassLoadException("The class " + c.FullName + "\ndoes not cast into the superclass " + mustCastToBaseclass.FullName + "\nPARAMETER: " + parameter + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter));
                }
                if (mustCastToBaseclass == c)
                {
                    throw new ParamClassLoadException("The class " + c.FullName + "\nmust not be the same as the required superclass " + mustCastToBaseclass.FullName + "\nPARAMETER: " + parameter + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter));
                }
                return Activator.CreateInstance(c);
            }
            // BRS : 2009-03-15
            // Calling Type.GetType(string s) throws TypeLoadException if the typeName (s) is invalid
            catch (TypeLoadException e)
            {
                throw new ParamClassLoadException("Class not found: " + Take(p) + "\nPARAMETER: " + parameter + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" + e);
            }
            // Since we've made it this far we already know that Type.GetType(string s) has given us a valid Type
            // Calling Activator.CreateInstance(Type t) throws TargetInvocationException if the constructor fails
            catch (TargetInvocationException e)
            {
                throw new ParamClassLoadException("Could not load class: "

                    + Take(p) + "\nPARAMETER: " + parameter + (defaultParameter == null ? ""
                    : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" + e);
            }
            catch (MemberAccessException e)
            {
                throw new ParamClassLoadException("The requested class is an interface or an abstract class: "
                    + Take(p) + "\nPARAMETER: " + parameter + (defaultParameter == null ? ""
                    : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" + e);
            }
            catch (Exception e)
            {
                throw new ParamClassLoadException("The requested class cannot be initialized with the default initializer: "
                    + Take(p) + "\nPARAMETER: " + parameter + (defaultParameter == null ? ""
                    : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" + e);
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
        public virtual object GetInstanceForParameterEq(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseclass)
        {
            PrintGotten(parameter, defaultParameter, false);
            IParameter p;
            if (_exists(parameter))
                p = parameter;
            else if (_exists(defaultParameter))
                p = defaultParameter;
            else
            {
                throw new ParamClassLoadException("No class name provided.\nPARAMETER: " + parameter
                    + "\n     ALSO: " + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter));
            }
            try
            {
                Type c = Type.GetType(Take(p));
                if (!mustCastToBaseclass.IsAssignableFrom(c))
                {
                    throw new ParamClassLoadException("The class " + c.FullName + "\ndoes not cast into the superclass "
                        + mustCastToBaseclass.FullName + "\nPARAMETER: " + parameter
                        + "\n     ALSO: " + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter));
                }
                return Activator.CreateInstance(c);
            }
            // BRS : 2009-03-15
            // Calling Type.GetType(string s) throws TypeLoadException if the typeName (s) is invalid
            catch (TypeLoadException e)
            {
                throw new ParamClassLoadException("Class not found: " + Take(p) + "\nPARAMETER: " + parameter + "\n     ALSO: "
                    + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" + e);
            }
            catch (TargetInvocationException e)
            {
                throw new ParamClassLoadException("Could not load class: " + Take(p) + "\nPARAMETER: " + parameter + "\n     ALSO: "
                    + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" + e);
            }
            catch (MemberAccessException e)
            {
                throw new ParamClassLoadException("The requested class is an interface or an abstract class: " + Take(p) + "\nPARAMETER: "
                    + parameter + "\n     ALSO: " + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" + e);
            }
            catch (Exception e)
            {
                throw new ParamClassLoadException("The requested class cannot be initialized with the default initializer: "
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
        public virtual object GetClassForParameter(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseClass)
        {
            PrintGotten(parameter, defaultParameter, false);
            IParameter p;
            if (_exists(parameter))
                p = parameter;
            else if (_exists(defaultParameter))
                p = defaultParameter;
            else
            {
                throw new ParamClassLoadException("No class name provided.\nPARAMETER: " + parameter + "\n     ALSO: "
                    + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter));
            }
            try
            {
                Type c = Type.GetType(Take(p));
                if (!mustCastToBaseClass.IsAssignableFrom(c))
                {
                    throw new ParamClassLoadException("The class " + c.FullName + "\ndoes not cast into the superclass "
                        + mustCastToBaseClass.FullName + "\nPARAMETER: " + parameter + "\n     ALSO: "
                        + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter));
                }
                return c;
            }
            // BRS : 2009-03-15
            // Calling Type.GetType(string s) throws TypeLoadException if the typeName (s) is invalid
            catch (TypeLoadException e)
            {
                throw new ParamClassLoadException("Class not found: " + Take(p) + "\nPARAMETER: " + parameter + "\n     ALSO: "
                    + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" + e);
            }
            catch (Exception e)
            {
                throw new ParamClassLoadException("Could not load class: " + Take(p) + "\nPARAMETER: " + parameter + "\n     ALSO: "
                    + (defaultParameter == null ? "" : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" + e);
            }
        }

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

        /// <summary> 
        /// Searches down through databases to find a given parameter; If the
        /// parameter does not exist, defaultValue is returned. If the parameter
        /// exists, and it is set to "false" (case insensitive), false is returned.
        /// Else true is returned. The parameter chosen is marked "used" if it
        /// exists.
        /// </summary>
        internal virtual bool GetBoolean(IParameter parameter, bool defaultValue)
        {
            if (!_exists(parameter))
                return defaultValue;
            return (!Take(parameter).ToUpper().Equals("false".ToUpper()));
        }

        /*protected*/
        /// <summary> 
        /// Parses an integer from a string, either in decimal or (if starting with an x) in hex.
        /// We assume that the string has been trimmed already.
        /// </summary>
        public virtual int ParseInt(string text)
        {
            char c;
            if (!string.IsNullOrEmpty(text) && ((text[0] == (c = 'x')) || c == 'X'))
            {
                // it's a hex int, load it as hex
                return Convert.ToInt32(text.Substring(1), 16);
            }
            // it's decimal
            return int.Parse(text);
        }

        /*protected*/
        /// <summary> 
        /// Parses a long from a string, either in decimal or (if starting with an x) in hex.
        /// We assume that the string has been trimmed already.
        /// </summary>
        public virtual long ParseLong(string text)
        {
            char c;
            if (!string.IsNullOrEmpty(text) && ((text[0] == (c = 'x')) || c == 'X'))
            {
                // it's a hex int, load it as hex
                return Convert.ToInt64(text.Substring(1), 16);
            }
            // it's decimal
            return Int64.Parse(text);
        }

        /*protected*/
        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be an integer. It returns the value, else throws a
        /// NumberFormatException exception if there is an error in parsing the
        /// parameter. The parameter chosen is marked "used" if it exists. Integers
        /// may be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual int GetInt(IParameter parameter)
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

        /*protected*/
        /// <summary> Searches down through databases to find a given parameter, whose value
        /// must be an integer >= minValue. It returns the value, or minValue-1 if
        /// the value is out of range or if there is an error in parsing the
        /// parameter. The parameter chosen is marked "used" if it exists. Integers
        /// may be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual int GetInt(IParameter parameter, int minValue)
        {
            if (_exists(parameter))
            {
                try
                {
                    int i = ParseInt(Take(parameter));
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
        /// Searches down through databases to find a given parameter, which must be
        /// an integer. If there is an error in parsing the parameter, then default
        /// is returned. The parameter chosen is marked "used" if it exists. Integers
        /// may be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual int GetIntWithDefault(IParameter parameter, int defaultValue)
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

        /// <summary> Searches down through databases to find a given parameter, whose value
        /// must be an integer >= minValue and &lt;= maxValue. It returns the value, or
        /// minValue-1 if the value is out of range or if there is an error in
        /// parsing the parameter. The parameter chosen is marked "used" if it
        /// exists. Integers may be in decimal or (if preceded with an X or x) in
        /// hexadecimal.
        /// </summary>
        public virtual int GetIntWithMax(IParameter parameter, int minValue, int maxValue)
        {
            if (_exists(parameter))
            {
                try
                {
                    int i = ParseInt(Take(parameter));
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

        /// <summary> Searches down through databases to find a given parameter, whose value
        /// must be a float >= minValue. If not, this method returns minvalue-1, else
        /// it returns the parameter value. The parameter chosen is marked "used" if
        /// it exists.
        /// </summary>

        public virtual float GetFloat(IParameter parameter, IParameter defaultParameter, double minValue)
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetFloat(parameter, minValue);

            return GetFloat(defaultParameter, minValue);
        }

        /// <summary> Searches down through databases to find a given parameter, whose value
        /// must be a float >= minValue. If not, this method returns minvalue-1, else
        /// it returns the parameter value. The parameter chosen is marked "used" if
        /// it exists.
        /// </summary>

        public virtual float GetFloat(IParameter parameter, double minValue)
        {
            if (_exists(parameter))
            {
                try
                {
                    float i = Single.Parse(Take(parameter)); // what
                    // stupidity...

                    // For JDK 1.2 and later, this is more efficient...
                    // float i = Float.parseFloat(get(parameter));
                    // ...but we can't use it and still be compatible with JDK 1.1

                    if (i < minValue)
                    {
                        return (float)(minValue - 1);
                    }
                    return i;
                }
                catch (FormatException)
                {
                    return (float)(minValue - 1);
                }
            }
            return (float)(minValue - 1);
        }

        /// <summary> Searches down through databases to find a given parameter, which must be
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

        /// <summary> Searches down through databases to find a given parameter, which must be
        /// a float. If there is an error in parsing the parameter, then default is
        /// returned. The parameter chosen is marked "used" if it exists.
        /// </summary>
        public virtual float GetFloatWithDefault(IParameter parameter, double defaultValue)
        {
            if (_exists(parameter))
            {
                try
                {
                    // For JDK 1.2 and later, this is more efficient...
                    // return Float.parseFloat(get(parameter));
                    // ...but we can't use it and still be compatible with JDK 1.1
                    return Single.Parse(Take(parameter)); // what
                    // stupidity...
                }
                catch (FormatException)
                {
                    return (float)(defaultValue);
                }
            }
            return (float)(defaultValue);
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

        /// <summary> Searches down through databases to find a given parameter, whose value
        /// must be a float >= minValue and &lt;= maxValue. If not, this method returns
        /// minvalue-1, else it returns the parameter value. The parameter chosen is
        /// marked "used" if it exists.
        /// </summary>
        public virtual float GetFloat(IParameter parameter, IParameter defaultParameter, double minValue, double maxValue)
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetFloat(parameter, minValue, maxValue);

            return GetFloat(defaultParameter, minValue, maxValue);
        }

        /// <summary> Searches down through databases to find a given parameter, whose value
        /// must be a float >= minValue and <= maxValue. If not, this method returns
        /// minvalue-1, else it returns the parameter value. The parameter chosen is
        /// marked "used" if it exists.
        /// </summary>

        public virtual float GetFloat(IParameter parameter, double minValue, double maxValue)
        {
            if (_exists(parameter))
            {
                try
                {
                    Single i = Single.Parse(Take(parameter));

                    if (i < minValue)
                    {
                        return (float)(minValue - 1);
                    }
                    if (i > maxValue)
                    {
                        return (float)(minValue - 1);
                    }
                    return i;
                }
                catch (FormatException)
                {
                    return (float)(minValue - 1);
                }
            }
            return (float)(minValue - 1);
        }



        /// <summary> Searches down through databases to find a given parameter, whose value
        /// must be an double. It returns the value, else throws a
        /// NumberFormatException exception if there is an error in parsing the
        /// parameter. The parameter chosen is marked "used" if it exists. Integers
        /// may be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual double GetDouble(IParameter parameter, IParameter defaultParameter)
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetInt(parameter);

            if (_exists(defaultParameter))
                return GetInt(defaultParameter);

            throw new FormatException("Integer does not exist for either parameter " + parameter + "\nor\n" + defaultParameter);
        }


        /// <summary> Searches down through databases to find a given parameter, whose value
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

        /// <summary> Searches down through databases to find a given parameter, whose value
        /// must be a double >= minValue. If not, this method returns minvalue-1,
        /// else it returns the parameter value. The parameter chosen is marked
        /// "used" if it exists.
        /// </summary>

        public virtual double GetDouble(IParameter parameter, double minValue)
        {
            if (_exists(parameter))
            {
                try
                {
                    double i = Double.Parse(Take(parameter));

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

        /// <summary> Searches down through databases to find a given parameter, whose value
        /// must be a double >= minValue and &lt;= maxValue. If not, this method returns
        /// minvalue-1, else it returns the parameter value. The parameter chosen is
        /// marked "used" if it exists.
        /// </summary>

        public virtual double GetDouble(IParameter parameter, IParameter defaultParameter, double minValue, double maxValue)
        {
            PrintGotten(parameter, defaultParameter, false);
            if (_exists(parameter))
                return GetDouble(parameter, minValue, maxValue);

            return GetDouble(defaultParameter, minValue, maxValue);
        }

        /// <summary> Searches down through databases to find a given parameter, whose value
        /// must be a double >= minValue and &lt;= maxValue. If not, this method returns
        /// minvalue-1, else it returns the parameter value. The parameter chosen is
        /// marked "used" if it exists.
        /// </summary>

        public virtual double GetDouble(IParameter parameter, double minValue, double maxValue)
        {
            if (_exists(parameter))
            {
                try
                {
                    double i = Double.Parse(Take(parameter));

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

        /// <summary> Searches down through databases to find a given parameter, which must be
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

        /// <summary> Searches down through databases to find a given parameter, which must be
        /// a float. If there is an error in parsing the parameter, then default is
        /// returned. The parameter chosen is marked "used" if it exists.
        /// </summary>
        public virtual double GetDoubleWithDefault(IParameter parameter, double defaultValue)
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

        /// <summary> Searches down through databases to find a given parameter, whose value
        /// must be a long. It returns the value, else throws a NumberFormatException
        /// exception if there is an error in parsing the parameter. The parameter
        /// chosen is marked "used" if it exists. Longs may be in decimal or (if
        /// preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual long GetLong(IParameter parameter)
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

        /// <summary> Searches down through databases to find a given parameter, whose value
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

        /// <summary> Searches down through databases to find a given parameter, whose value
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

        /// <summary> Searches down through databases to find a given parameter, whose value
        /// must be a long >= minValue. If not, this method returns errValue, else it
        /// returns the parameter value. The parameter chosen is marked "used" if it
        /// exists. Longs may be in decimal or (if preceded with an X or x) in
        /// hexadecimal.
        /// </summary>
        public virtual long GetLong(IParameter parameter, long minValue)
        {
            if (_exists(parameter))
            {
                try
                {
                    long i = ParseLong(Take(parameter));
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

        /// <summary> Searches down through databases to find a given parameter, which must be
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

        /// <summary> Searches down through databases to find a given parameter, which must be
        /// a long. If there is an error in parsing the parameter, then default is
        /// returned. The parameter chosen is marked "used" if it exists. Longs may
        /// be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual long GetLongWithDefault(IParameter parameter, long defaultValue)
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

        /// <summary> Searches down through databases to find a given parameter, whose value
        /// must be a long >= minValue and = &lt; maxValue. If not, this method returns
        /// errValue, else it returns the parameter value. The parameter chosen is
        /// marked "used" if it exists. Longs may be in decimal or (if preceded with
        /// an X or x) in hexadecimal.
        /// </summary>
        public virtual long GetLongWithMax(IParameter parameter, IParameter defaultParameter, long minValue, long maxValue)
        {
            PrintGotten(parameter, defaultParameter, false);

            if (_exists(parameter))
                return GetLong(parameter, minValue, maxValue);

            return GetLong(defaultParameter, minValue, maxValue);
        }

        /// <summary> Use getLongWithMax(...) instead. Searches down through databases to find
        /// a given parameter, whose value must be a long >= minValue and = &lt;
        /// maxValue. If not, this method returns errValue, else it returns the
        /// parameter value. The parameter chosen is marked "used" if it exists.
        /// Longs may be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual long GetLongWithMax(IParameter parameter, long minValue, long maxValue)
        {
            if (_exists(parameter))
            {
                try
                {
                    long i = ParseLong(Take(parameter));
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

        /// <summary> Use getLongWithMax(...) instead. Searches down through databases to find
        /// a given parameter, whose value must be a long >= minValue and = &lt;
        /// maxValue. If not, this method returns errValue, else it returns the
        /// parameter value. The parameter chosen is marked "used" if it exists.
        /// Longs may be in decimal or (if preceded with an X or x) in hexadecimal.
        /// 
        /// </summary>
        /// <deprecated>
        /// </deprecated>
        public virtual long GetLong(IParameter parameter, IParameter defaultParameter, long minValue, long maxValue)
        {
            PrintGotten(parameter, defaultParameter, false);
            return GetLongWithMax(parameter, defaultParameter, minValue, maxValue);
        }

        /// <summary> Use getLongWithMax(...) instead. Searches down through databases to find
        /// a given parameter, whose value must be a long >= minValue and = &lt;
        /// maxValue. If not, this method returns errValue, else it returns the
        /// parameter value. The parameter chosen is marked "used" if it exists.
        /// </summary>
        /// <deprecated>
        /// </deprecated>
        public virtual long GetLong(IParameter parameter, long minValue, long maxValue)
        {
            return GetLongWithMax(parameter, minValue, maxValue);
        }

        /// <summary> Searches down through the databases to find a given parameter, whose
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

        public virtual FileInfo GetFile(IParameter parameter)
        {
            if (_exists(parameter))
            {
                string p = Take(parameter);
                if (p == null)
                    return null;
                if (p.StartsWith(C_HERE))
                    return new FileInfo(p.Substring(C_HERE.Length));

                {
                    // BRS : 2009-03-15
                    // if (f.isAbsolute())
                    if (Path.IsPathRooted(p))
                        return new FileInfo(p);

                    return new FileInfo(DirectoryFor(parameter).FullName + "\\" + p);
                }
            }
            return null;
        }

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

        /*protected*/
        /// <summary> Searches down through databases to find a given parameter. Returns the
        /// parameter's value (trimmed) or null if not found or if the trimmed result
        /// is empty. The parameter chosen is marked "used" if it exists.
        /// </summary>

        public virtual string GetString(IParameter parameter)
        {
            lock (this)
            {
                if (_exists(parameter))
                    return Take(parameter);

                return null;
            }
        }

        /// <summary> Searches down through databases to find a given parameter. Returns the
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

        /*protected*/
        /// <summary> Searches down through databases to find a given parameter. Returns the
        /// parameter's value trimmed of whitespace, or defaultValue.trim() if the
        /// result is not found or the trimmed result is empty.
        /// </summary>
        public virtual string GetStringWithDefault(IParameter parameter, string defaultValue)
        {
            if (_exists(parameter))
            {
                string result = Take(parameter);
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

        /*protected*/
        /// <summary>Clears the checked flag </summary>
        public virtual void Uncheck()
        {
            lock (this)
            {
                if (!checkState)
                    return; // we already unchecked this path -- this is dangerous if
                // Parents are used without children
                checkState = false;
                int size = Parents.Count;
                for (int x = 0; x < size; x++)
                    ((ParameterDatabase)(Parents[x])).Uncheck();
            }
        }

        public event ParameterDatabaseListenerDelegate ParameterDatabaseListenerDelegateVar;
        protected virtual void On(ParameterDatabaseEvent eventParam)
        {
            if (ParameterDatabaseListenerDelegateVar != null)
                ParameterDatabaseListenerDelegateVar(this, eventParam);
        }

        public virtual void AddListener(ParameterDatabaseListener l)
        {
            lock (this) { listeners.Add(l); }
        }

        public virtual void RemoveListener(ParameterDatabaseListener l)
        {
            lock (this) { listeners.Remove(l); }
        }

        /// <summary> Fires a parameter set event.
        /// </summary>
        public virtual void FireParameterSet(IParameter parameter, string paramValue)
        {
            lock (this)
            {
                List<ParameterDatabaseListener>.Enumerator it = listeners.GetEnumerator();

                while (it.MoveNext())
                {
                    ParameterDatabaseListener l = it.Current;
                    l.ParameterSet(this, new ParameterDatabaseEvent(this, parameter, paramValue, ParameterDatabaseEvent.SET));
                }
            }
        }

        /// <summary> Fires a parameter accessed event.
        /// </summary>
        /// <param name="parameter">
        /// </param>
        /// <param name="paramValue"></param>
        public virtual void FireParameterAccessed(IParameter parameter, string paramValue)
        {
            lock (this)
            {
                List<ParameterDatabaseListener>.Enumerator it = listeners.GetEnumerator();

                while (it.MoveNext())
                {
                    ParameterDatabaseListener l = it.Current;
                    l.ParameterSet(this, new ParameterDatabaseEvent(this, parameter, paramValue, ParameterDatabaseEvent.ACCESSED));
                }
            }
        }

        /// <summary> Sets a parameter in the topmost database to a given value, trimmed of
        /// whitespace.
        /// </summary>
        public virtual void SetParameter(IParameter parameter, string paramValue)
        {
            lock (this)
            {
                string tmp = paramValue.Trim();
                //object tempObject = this[parameter.Param];
                this[parameter.Param] = tmp;
                FireParameterSet(parameter, tmp);
            }
        }

        /// <summary> Prints out all the parameters marked as used, plus their values. If a
        /// parameter was listed as "used" but not's actually in the database, the
        /// value printed is UNKNOWN_VALUE (set to "?????")
        /// </summary>

        public virtual void ListGotten(StreamWriter p)
        {
            lock (this)
            {
                List<string> vec = new List<string>(10);
                Dictionary<string, bool>.KeyCollection.Enumerator e = gotten.Keys.GetEnumerator();

                while (e.MoveNext())
                {
                    vec.Add(e.Current);
                }

                // sort the keys
                string[] array = new string[vec.Count];
                array = vec.ToArray();

                CollectionsSupport.Sort(vec, null);

                // Uncheck and print each item
                for (int x = 0; x < array.Length; x++)
                {
                    string s = array[x];
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

        /// <summary>Prints out all the parameters NOT marked as used, plus their values. </summary>

        public virtual void ListNotGotten(StreamWriter p)
        {
            lock (this)
            {
                List<string> vec = new List<string>(10);
                Dictionary<string, IParameter> all = new Dictionary<string, IParameter>();

                _list(null, false, null, all); // grab all the nonshadowed keys

                Dictionary<string, bool>.KeyCollection.Enumerator e = gotten.Keys.GetEnumerator();

                while (e.MoveNext())
                {
                    all.Remove(e.Current);
                }

                Dictionary<string, IParameter>.KeyCollection.Enumerator i = all.Keys.GetEnumerator();
                i = all.Keys.GetEnumerator();

                while (i.MoveNext())
                {
                    vec.Add(i.Current);
                }

                // sort the keys
                string[] array = new string[vec.Count];
                array = vec.ToArray();

                CollectionsSupport.Sort(vec, null);

                // Uncheck and print each item
                for (int x = 0; x < array.Length; x++)
                {
                    string s = array[x];
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

        /// <summary>Prints out all the parameters NOT marked as used, plus their values. </summary>

        public virtual void ListNotAccessed(StreamWriter p)
        {
            lock (this)
            {
                List<string> vec = new List<string>(10);
                Dictionary<string, IParameter> all = new Dictionary<string, IParameter>();
                _list(null, false, null, all); // grab all the nonshadowed keys
                Dictionary<string, bool>.KeyCollection.Enumerator e = accessed.Keys.GetEnumerator();

                while (e.MoveNext())
                {
                    all.Remove(e.Current);
                }

                Dictionary<string, IParameter>.KeyCollection.Enumerator i = all.Keys.GetEnumerator();
                i = all.Keys.GetEnumerator();

                while (i.MoveNext())
                {
                    vec.Add(i.Current);
                }

                // sort the keys
                string[] array = new string[vec.Count];
                array = vec.ToArray();

                CollectionsSupport.Sort(vec, null);

                // Uncheck and print each item
                for (int x = 0; x < array.Length; x++)
                {
                    string s = array[x];
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

        /// <summary> Prints out all the parameters marked as accessed ("gotten" by some
        /// getFoo(...) method), plus their values. If this method ever prints
        /// UNKNOWN_VALUE ("?????"), that's a bug.
        /// </summary>

        public virtual void ListAccessed(StreamWriter p)
        {
            lock (this)
            {
                List<string> vec = new List<string>(10);
                Dictionary<string, bool>.KeyCollection.Enumerator e = accessed.Keys.GetEnumerator();

                while (e.MoveNext())
                {
                    vec.Add(e.Current);
                }

                // sort the keys
                string[] array = new string[vec.Count];
                array = vec.ToArray();

                CollectionsSupport.Sort(vec, null);

                // Uncheck and print each item
                for (int x = 0; x < array.Length; x++)
                {
                    string s = array[x];
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

        /// <summary>Returns true if parameter exist in the database </summary>
        public virtual bool Exists(IParameter parameter)
        {
            lock (this)
            {
                PrintGotten(parameter, null, true);
                return _exists(parameter);
            }
        }

        internal virtual bool _exists(IParameter parameter)
        {
            lock (this)
            {
                if (parameter == null)
                    return false;
                string result = _get(parameter.Param);
                Uncheck();

                accessed[parameter.Param] = true;
                return (result != null);
            }
        }

        /// <summary> Returns true if either parameter or defaultParameter exists in the
        /// database
        /// </summary>
        public virtual bool Exists(IParameter parameter, IParameter defaultParameter)
        {
            lock (this)
            {
                PrintGotten(parameter, defaultParameter, true);
                if (Exists(parameter))
                    return true;
                if (Exists(defaultParameter))
                    return true;
                return false;
            }
        }


        /*protected*/
        /*
P: Successfully retrieved parameter
!P: Unsuccessfully retrieved parameter
<P: Would have retrieved parameter
		
E: Successfully tested for existence of parameter
!E: Unsuccessfully tested for existence of parameter
<E: Would have tested for exidstence of parameter*/

        public virtual void PrintGotten(IParameter parameter, IParameter defaultParameter, bool exists)
        {
            if (printState == PS_UNKNOWN)
            {
                IParameter p = new Parameter(PRINT_PARAMS);
                string jp = Take(p);
                if (jp == null || jp.ToUpper().CompareTo("false".ToUpper()) == 0)
                    printState = PS_NONE;
                else
                    printState = PS_PRINT_PARAMS;
                Uncheck();
                PrintGotten(p, null, false);
            }

            if (printState == PS_PRINT_PARAMS)
            {
                string p = "P: ";
                if (exists)
                    p = "E: ";

                if (parameter == null && defaultParameter == null)
                    return;
                if (parameter == null)
                {
                    string result = _get(defaultParameter.Param);
                    Uncheck();
                    if (result == null)
                        // null parameter, didn't find defaultParameter
                        Console.Error.WriteLine("\t!" + p + defaultParameter.Param);
                    // null parameter, found defaultParameter
                    else
                        Console.Error.WriteLine("\t " + p + defaultParameter.Param + " = " + result);
                }
                else if (defaultParameter == null)
                {
                    string result = _get(parameter.Param);
                    Uncheck();
                    if (result == null)
                        // null defaultParameter, didn't find parameter
                        Console.Error.WriteLine("\t!" + p + parameter.Param);
                    // null defaultParameter, found parameter
                    else
                        Console.Error.WriteLine("\t " + p + parameter.Param + " = " + result);
                }
                else
                {
                    string result = _get(parameter.Param);
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
                        // found defaultParameter
                        else
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

        public string Take(IParameter parameter)
        {
            lock (this)
            {
                string result = _get(parameter.Param);
                Uncheck();

                // set hashtable appropriately
                accessed[parameter.Param] = true;
                gotten[parameter.Param] = true;

                return result;
            }
        }

        /// <summary>Private helper function </summary>
        internal virtual string _get(string parameter)
        {
            lock (this)
            {
                if (parameter == null)
                {
                    return null;
                }
                if (checkState)
                    return null; // we already searched this path
                checkState = true;
                string result = Get(parameter);
                if (result == null)
                {
                    int size = Parents.Count;
                    for (int x = 0; x < size; x++)
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

        /*protected*/
        internal virtual HashSet<IParameter> _getShadowedValues(IParameter parameter, HashSet<IParameter> vals)
        {
            if (parameter == null)
            {
                return vals;
            }

            if (checkState)
            {
                return vals;
            }

            checkState = true;
            string result = Get(parameter.Param);
            if (result != null)
            {
                result = result.Trim();
                if (result.Length != 0)
                    vals.Add(new Parameter(result));
            }

            int size = Parents.Count;
            for (int i = 0; i < size; ++i)
            {
                ((ParameterDatabase)Parents[i])._getShadowedValues(parameter, vals);
            }

            return vals;
        }

        public virtual HashSet<IParameter> GetShadowedValues(IParameter parameter)
        {
            HashSet<IParameter> vals = new HashSet<IParameter>();
            vals = _getShadowedValues(parameter, vals);
            Uncheck();
            return vals;
        }

        /// <summary> Searches down through databases to find the directory for the database
        /// which holds a given parameter. Returns the directory name or null if not
        /// found.
        /// </summary>

        public virtual FileInfo DirectoryFor(IParameter parameter)
        {
            FileInfo result = _directoryFor(parameter);
            Uncheck();
            return result;
        }

        /// <summary>Private helper function </summary>
        internal virtual FileInfo _directoryFor(IParameter parameter)
        {
            lock (this)
            {
                if (checkState)
                    return null; // we already searched this path
                checkState = true;
                FileInfo result = null;
                string p = Get(parameter.Param);
                if (p == null)
                {
                    int size = Parents.Count;
                    for (int x = 0; x < size; x++)
                    {
                        result = ((ParameterDatabase)(Parents[x]))._directoryFor(parameter);
                        if (result != null)
                            return result;
                    }
                    return result;
                }
                return directory;
            }
        }

        /// <summary> Searches down through databases to find the parameter file 
        /// which holds a given parameter. Returns the filename or null if not
        /// found.
        /// </summary>

        public virtual FileInfo FileFor(IParameter parameter)
        {
            FileInfo result = _fileFor(parameter);
            Uncheck();
            return result;
        }

        internal virtual FileInfo _fileFor(IParameter parameter)
        {
            lock (this)
            {
                if (checkState)
                    return null;

                checkState = true;
                FileInfo result = null;
                string p = Get(parameter.Param);
                if (p == null)
                {
                    int size = Parents.Count;
                    for (int i = 0; i < size; ++i)
                    {
                        result = ((ParameterDatabase)Parents[i])._fileFor(parameter);
                        if (result != null)
                            return result;
                    }
                    return result;
                }
                return new FileInfo(directory.FullName + "\\" + filename);
            }
        }

        /// <summary>Removes a parameter from the topmost database. </summary>
        public void Remove(IParameter parameter)
        {
            lock (this)
            {
                if (parameter.Param.Equals(PRINT_PARAMS))
                    printState = PS_UNKNOWN;
                Remove(parameter.Param);
            }
        }

        /// <summary>Removes a parameter from the database and all its parent databases. </summary>
        public virtual void RemoveDeeply(IParameter parameter)
        {
            lock (this)
            {
                _removeDeeply(parameter);
                Uncheck();
            }
        }

        /// <summary>Private helper function </summary>
        internal virtual void _removeDeeply(IParameter parameter)
        {
            lock (this)
            {
                if (checkState)
                    return; // already removed from this path
                checkState = true;
                Remove(parameter);
                int size = Parents.Count;
                for (int x = 0; x < size; x++)
                    ((ParameterDatabase)(Parents[x])).RemoveDeeply(parameter);
            }
        }

        public virtual void AddParent(IParameterDatabase database)
        {
            Parents.Add(database);
        }

        /// <summary>Creates an empty parameter database. </summary>
        public ParameterDatabase_OriginalConversion()
        {
            accessed = new Dictionary<string, bool>();
            gotten = new Dictionary<string, bool>();
            directory = new FileInfo("."); // uses the user
            // path
            filename = "";
            Parents = new List<IParameterDatabase>(10);
            checkState = false; // unnecessary
            listeners = new List<ParameterDatabaseListener>(10);
        }

        /// <summary>Creates a new parameter database from the given Dictionary.  
        /// Both the keys and values will be run through toString() before adding to the dataase.   
        /// Keys are parameters.  Values are the values of the parameters.  
        /// Beware that a ParameterDatabase is itself a Dictionary; but if you pass one in here you 
        /// will only get the lowest-level elements.  If parent.n are defined, Parents will 
        /// be attempted to be loaded -- that's the reason for the FileNotFoundException and IOException.  
        /// </summary>
        public ParameterDatabase_OriginalConversion(IDictionary map)
            : this()
        {
            IEnumerator keys = map.Keys.GetEnumerator();
            while (keys.MoveNext())
            {
                object obj = keys.Current;
                SetParameter(new Parameter("" + obj), "" + map[obj]);
            }

            // load Parents
            for (int x = 0; ; x++)
            {
                string s = Get("parent." + x);
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

        /// <summary>Creates a new parameter database loaded from the given stream.  Non-relative Parents are not permitted.
        /// If parent.n are defined, Parents will be attempted to be loaded -- that's 
        /// the reason for the FileNotFoundException and IOException. 
        /// </summary>

        public ParameterDatabase_OriginalConversion(StreamReader stream)
            : this()
        {
            // BRS : TODO : Requires further conversion.
            //throw new NotImplementedException("Requires further conversion.");
            //this = (ParameterDatabase)(new NameValueCollection(System.Configuration.ConfigurationSettings.AppSettings));

            string line;
            while ((line = stream.ReadLine()) != null)
            {
                string trimStr = line.Trim();
                if (trimStr != "" && trimStr[0] != '#')
                {
                    string key = getStringIndex(line, 0, "=");
                    string value = getStringIndex(line, 1, "=");
                    SetParameter(new Parameter(key), value);
                }
            }

            // load Parents
            for (int x = 0; ; x++)
            {
                string s = Get("parent." + x);
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


        /// <summary> Creates a new parameter database tree from a given database file and its
        /// parent files.
        /// </summary>
        public ParameterDatabase_OriginalConversion(FileInfo filename)
            : this()
        {
            // BRS : TODO : Requires further conversion.
            this.filename = filename.Name;
            string tmpdirectory = filename.DirectoryName; // get the directory
            //// filename is in
            new FileStream(filename.FullName, FileMode.Open, FileAccess.Read);
            //// => load parameters from file, what is the file format
            listeners = new List<ParameterDatabaseListener>(10);

            // load Parents
            for (int x = 0; ; x++)
            {
                string s = Get("parent." + x);
                if (s == null)
                    return; // we're done
                // BRS : 2009-03-15
                // if (new FileInfo(s).isAbsolute())
                if (Path.IsPathRooted(s))
                    // it's an absolute file definition
                    Parents.Add(new ParameterDatabase(new FileInfo(s)));
                // it's relative to my path
                else
                    Parents.Add(new ParameterDatabase(new FileInfo(filename.DirectoryName + "\\" + s)));
            }
        }

        /// <summary> Creates a new parameter database from a given database file and argv
        /// list. The top-level database is completely empty, pointing to a second
        /// database which contains the parameter entries stored in args, which
        /// points to a tree of databases constructed using
        /// ParameterDatabase(filename).
        /// </summary>

        public ParameterDatabase_OriginalConversion(FileInfo filename, string[] args)
            : this()
        {
            this.filename = filename.Name;
            directory = new FileInfo(filename.DirectoryName); // get the directory
            // filename is in

            // Create the Parameter Database tree for the files
            ParameterDatabase files = new ParameterDatabase(filename);

            // Create the Parameter Database for the arguments
            ParameterDatabase a = new ParameterDatabase();
            a.Parents.Add(files);
            for (var x = 0; x < args.Length - 1; x++)
            {
                var s = args[x + 1].Trim();
                if (s.Length == 0) continue;
                var eq = s.IndexOf('=');
                if (eq <= 0) continue;
                this[s.Substring(0, eq)] = s.Substring(eq + 1);
            }

            // Set me up
            Parents.Add(a);
            listeners = new List<ParameterDatabaseListener>(10);
        }

        /// <summary> Parses and adds s to the database. Returns true if there was actually
        /// something to parse.
        /// </summary>
        public virtual bool ParseCommandLineArg(string s)
        {
            s = s.Trim();
            if (s.Length == 0)
                return false;
            if (s[0] == '#')
                return false;
            int eq = s.IndexOf('=');
            if (eq < 0)
                return false;
            object tempObject = this[s.Substring(0, (eq) - (0))];
            this[s.Substring(0, (eq) - (0))] = s.Substring(eq + 1);
            return true;
        }

        /// <summary> Prints out all the parameters in the database. Useful for debugging. If
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

        /// <summary> Prints out all the parameters in the database, but not shadowed
        /// parameters.
        /// </summary>
        public void List(StreamWriter p)
        {
            List(p, false);
        }

        /// <summary> Prints out all the parameters in the database. Useful for debugging. If
        /// listShadowed is true, each parameter is printed with the parameter
        /// database it's located in. If listShadowed is false, only active
        /// parameters are listed, and they're all given in one big chunk.
        /// </summary>
        public virtual void List(StreamWriter p, bool listShadowed)
        {
            if (listShadowed)
                _list(p, listShadowed, "root", null);
            else
            {
                Dictionary<string, IParameter> gather = new Dictionary<string, IParameter>();
                _list(null, listShadowed, "root", gather);

                List<string> vec = new List<string>(10);
                Dictionary<string, IParameter>.KeyCollection.Enumerator e = gather.Keys.GetEnumerator();

                while (e.MoveNext())
                {
                    vec.Add(e.Current);
                }

                // sort the keys
                CollectionsSupport.Sort(vec, null);

                // Uncheck and print each item
                for (int x = 0; x < vec.Count; x++)
                {
                    string s = vec[x];
                    string v = null;
                    if (s != null)
                        v = gather[s].Param;
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

        /// <summary>Private helper function. </summary>
        internal virtual void _list(StreamWriter p, bool listShadowed, string prefix, Dictionary<string, IParameter> gather)
        {
            if (listShadowed)
            {
                // Print out my header
                if (p != null)
                {
                    p.WriteLine("\n########" + prefix);
                }
                // BRS : 2009-03-15 : Commented out the following line because the base class (NameValueCollection) does not define the method.
                // BRS : TODO : Should walk through the semantics of this one.
                //base.list(p);
                int size = Parents.Count;
                for (int x = 0; x < size; x++)
                    ((ParameterDatabase)(Parents[x]))._list(p, listShadowed, prefix + "." + x, gather);
            }
            else
            {
                // load in reverse order so things get properly overwritten
                int size = Parents.Count;
                for (int x = size - 1; x >= 0; x--)
                    ((ParameterDatabase)(Parents[x]))._list(p, listShadowed, prefix, gather);
                IEnumerator e = Keys.GetEnumerator();
                while (e.MoveNext())
                {
                    string key = (string)e.Current;
                    gather[key] = new Parameter(this[key]);
                }
            }
            if (p != null)
                p.Flush();
        }

        public override string ToString()
        {
            string s = CollectionsSupport.CollectionToString(this);
            if (Parents.Count > 0)
            {
                s += " : (";
                for (int x = 0; x < Parents.Count; x++)
                {
                    if (x > 0)
                        s += ", ";
                    s += Parents[x];
                }
                s += ")";
            }
            return s;
        }

        /// <summary> Builds a TreeModel from the available property keys.   </summary>
        /// <returns>
        /// </returns>
        public virtual TreeNode BuildTreeModel()
        {
            string sep = Path.DirectorySeparatorChar.ToString();
            ParameterDatabaseTreeNode root = new ParameterDatabaseTreeNode(this.directory.FullName + sep + this.filename);
            ParameterDatabaseTreeModel model = new ParameterDatabaseTreeModel(root);

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

        internal virtual void _buildTreeModel(TreeNode model, TreeNode root)
        {
            IEnumerator e = Keys.GetEnumerator();
            while (e.MoveNext())
            {
                _addNodeForParameter(model, root, (String)e.Current);
            }

            int size = Parents.Count;
            for (int i = 0; i < size; ++i)
            {
                ParameterDatabase parentDB = (ParameterDatabase)Parents[i];
                parentDB._buildTreeModel(model, root);
            }
        }

        /// <param name="model">
        /// </param>
        /// <param name="root">
        /// </param>
        internal virtual void _addNodeForParameter(TreeNode model, TreeNode root, string key)
        {
            if (key.IndexOf("parent.") == -1)
            {
                /* 
                * TODO split is new to 1.4.  To maintain 1.2 compatability we need
                * to use a different approach.  Just use a string tokenizer.
                */
                Tokenizer tok = new Tokenizer(key, ".");
                string[] path = new string[tok.Count];
                int t = 0;
                while (tok.HasMoreTokens())
                {
                    path[t++] = tok.NextToken();
                }
                TreeNode parent = root;

                for (int i = 0; i < path.Length; ++i)
                {
                    int children = model is ParameterDatabaseTreeModel ? ((ParameterDatabaseTreeModel)model).GetChildCount(parent)
                        : ((TreeNode)parent).Nodes.Count;
                    if (children > 0)
                    {
                        int c = 0;
                        for (; c < children; ++c)
                        {
                            TreeNode child = parent.Nodes[c];
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

        /**
      * Extract the index field from the buffer using the separator.
      * @param index the index to extract
      * @param separator the separator to use for indexing
      * @return the string value at index position in the read string
      */
        internal static string getStringIndex(string buffer, int index, string separator)
        {
            if (index < 0 || buffer == null || separator == null)
            {
                return ""; // Nothing to do for a negative value or empty buffer
            }

            // Length of <separator>
            int separatorSize = separator.Length;

            // We got to the position we will modify
            int debut = -separatorSize;

            while (index != 0)
            {
                // We search for the <separator>
                debut = buffer.IndexOf(separator, debut + separatorSize);

                // We did not find <separator>, we add it
                // since we did not reach the desired index
                if (debut == -1)
                {
                    return "";
                }

                --index; // We change the index
            }

            // We search the end index in the string
            int fin = buffer.IndexOf(separator, debut + separatorSize);
            if (fin == -1)
            {
                fin = buffer.Length;
            }

            // Extract the field
            return buffer.Substring(debut + separatorSize, fin - (debut + separatorSize));
        }

        /// <summary>Test the ParameterDatabase </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            ParameterDatabase pd = new ParameterDatabase(new FileInfo(args[0]), args);
            pd.SetParameter(new Parameter("Hi there"), "Whatever");
            pd.SetParameter(new Parameter(new[] { "1", "2", "3" }), " Whatever ");
            pd.SetParameter(new Parameter(new[] { "a", "b", "c" }).Pop().Push("d"), "Whatever");

            Console.Error.WriteLine("\n\n PRINTING ALL PARAMETERS \n\n");
            StreamWriter temp_writer = new StreamWriter(Console.OpenStandardError(), Encoding.Default) { AutoFlush = true };
            pd.List(temp_writer, true);

            Console.Error.WriteLine("\n\n PRINTING ONLY VALID PARAMETERS \n\n");
            StreamWriter temp_writer2 = new StreamWriter(Console.OpenStandardError(), Encoding.Default) { AutoFlush = true };
            pd.List(temp_writer2, false);
        }
    }
}