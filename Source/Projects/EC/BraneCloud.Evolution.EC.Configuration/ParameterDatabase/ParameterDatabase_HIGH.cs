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
using System.Text;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;
using BraneCloud.Evolution.EC.Support;

namespace BraneCloud.Evolution.EC.Configuration
{	
    // BRS: Most of the methods in ParameterDatabase are synchronized using _syncLock.

    /// <summary> 
    /// This extension of the Properties class allows you to set, get, and delete
    /// Parameters in a hierarchical tree-like database. The database consists of a
    /// list of Parameters, plus an array of "parent databases" which it falls back
    /// on when it can't find the Parameter you're looking for. Parents may also have
    /// arrays of Parents, and so on..
    /// 
    /// The parameters are loaded from a Java property-list file, which is basically
    /// a collection of parameter=value pairs, one per line. Empty lines and lines
    /// beginning with # are ignored. These parameters and their values are
    /// <b>case-sensitive </b>, and whitespace is trimmed I believe.
    /// 
    /// An optional set of parameters, "parent. <i>n </i>", where <i>n </i> are
    /// consecutive integers starting at 0, define the filenames of the database's Parents.
    /// 
    /// An optional set of parameters, "print-params", specifies whether or not
    /// parameters should be printed as they are used (through one of the get(...)
    /// methods). If print-params is unset, or set to false or FALSE, nothing is
    /// printed. If set to non-false, then the parameters are printed prepended with a "P:"
    /// when their values are requested,  "E:" when their existence is tested.  Prior to the
    /// "P:" or "E:" you may see a "!" (meaning that the parameter isn't in the database),
    /// or a "&lt;" (meaning that the parameter was a default parameter which was never
    /// looked up because the primary parameter contained the value).
    /// 
    /// When you create a ParameterDatabase using new ParameterDatabase(), it is created thus:
    /// 
    /// DATABASE: database
    /// FROM: (empty)
    /// 
    /// When you create a ParameterDatabase using new ParameterDatabase( <i>file</i>), 
    /// it is created by loading the database file, and its parent file tree, thus:
    /// 
    /// DATABASE: database - parent0 +- parent0 +- parent0 +- ....
    /// FROM:     (empty)    (file)    (parent.0) (parent.0)  ....
    ///                                         +- parent1 +- ....
    ///                                           (parent.1)  
    ///                                         ....
    ///                                 parent1 +- ....
    ///                                (parent.1)
    ///                               ....
    /// 
    /// When you create a ParameterDatabase using new ParameterDatabase( file, argv ), 
    /// the preferred way, it is created thus:
    /// 
    /// DATABASE: database - parent0 +- parent0 +- parent0 +- parent0 +- ....
    /// FROM:     (empty)    (argv)     (file)    (parent.0) (parent.0)  ....
    ///                                                       parent1 +- ....
    ///                                                      (parent.1)
    ///                                                    ....
    ///                                         +- parent1 +- ....
    ///                                           (parent.1)
    ///                                         ....
    /// 
    /// ...that is, the actual top database is empty, and stores parameters added
    /// programmatically; its parent is a database formed from arguments passed in on
    /// the command line; <i>its </i> parent is the parameter database which actually
    /// loads from foo. This allows you to programmatically add parameters which
    /// override those in foo, then delete them, thus bringing foo's parameters back
    /// in view.
    /// 
    /// Once a parameter database is loaded, you query it with the <tt>get</tt>
    /// methods. The database, then its Parents, are searched until a match is found
    /// for your parameter. The search rules are thus: (1) the root database is
    /// searched first. (2) If a database being searched doesn't contain the data, it
    /// searches its Parents recursively, starting with parent 0, then moving up,
    /// until all searches are exhausted or something was found. (3) No database is
    /// searched twice.
    /// 
    /// The various <tt>get</tt> methods all take two parameters.  The first
    /// parameter is fetched and retrieved first.  If that fails, the second one
    /// (known as the <i>default parameter</i>) is fetched and retrieved.  You
    /// can pass in <tt>null</tt> for the default parameter if you don't have one.
    /// 
    /// You can test a parameter for existence with the <tt>exists</tt> methods.
    /// 
    /// You can set a parameter (in the topmost database <i>only </i> with the
    /// <tt>set</tt> command. The <tt>remove</tt> command removes a parameter
    /// from the topmost database only. The <tt>removeDeeply</tt> command removes
    /// that parameter from every database.
    /// 
    /// The values stored in a parameter database must not contain "#", "=",
    /// non-ascii values, or whitespace.
    /// </summary>
    [Serializable]
    public partial class ParameterDatabase : PropertiesClass, IParameterDatabase
    {
        /// <inheritdoc />
        public IParameterDatabase DeepClone()
        {
            var bs = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bs.Serialize(ms, this);
                ms.Position = 0;
                return (ParameterDatabase)bs.Deserialize(ms);
            }
        }

        #region Constants           **************************************************************************

        public const string C_HERE = "$";
        public const string C_CLASS = "@";
        public const string UNKNOWN_VALUE = "";
        public const string PRINT_PARAMS = "print-params";
        public const int PS_UNKNOWN = -1;
        public const int PS_NONE = 0;
        public const int PS_PRINT_PARAMS = 1;
        public int PrintState = PS_UNKNOWN;

        #endregion // Constants
        #region Constructors        **************************************************************************

        /// <summary>
        /// Creates an empty parameter database. 
        /// </summary>
        public ParameterDatabase()
        {
            Directory = new FileInfo("."); // uses the user path
            //filename = "";
            Label = "Basic Database"; 
            Parents = new List<IParameterDatabase>(10);
            CheckState = false; // unnecessary
        }

        /// <summary>
        /// Creates a new parameter database from the given Dictionary.  
        /// Both the keys and values will be run through ToString() before adding to the database.   
        /// Keys are parameters.  Values are the values of the parameters.  
        /// Beware that a ParameterDatabase is itself a Dictionary; but if you pass one in here you 
        /// will only get the lowest-level elements.  If parent.n are defined, Parents will 
        /// be attempted to be loaded -- that's the reason for the FileNotFoundException and IOException.  
        /// </summary>
        public ParameterDatabase(IDictionary map)
            : this()
        {
            Label = "Dictionary: " + map.GetHashCode();

            // TODO: Use foreach here?
            //foreach (DictionaryEntry de in map)
            //{
            //    SetParameter(new Parameter("" + de.Key), "" + de.Value);
            //}

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
        /// Eliminates .. and . from a relative path without converting it
        /// according to the file system. For example,
        /// "hello/there/../how/./are/you/yo/../../hey" becomes
        /// "hello/how/are/hey".  This is useful for making proper
        /// path names for jar files.
        /// </summary>
        private static String SimplifyPath(String pathname)
        {
            // TODO : Implement this!
            throw new NotImplementedException();
            //var path = new FileInfo(pathname);
            //var a = new List<string>();
            //while (path != null)
            //{
            //    var n = path.Name;
            //    a.Add(n);
            //    path = path.getParentFile();
            //}

            //var b = new List<string>();
            //for (var i = a.Count - 1; i >= 0; i--)
            //{
            //    var n = (String) (a[i]);
            //    if (n.Equals("."))
            //    {
            //    } // do nothing
            //    else if (n.Equals("..") &&
            //             b.Count != 0 && !b[0].Equals(".."))
            //        b.Remove(b.Count - 1);
            //    else b.Add(n);
            //}

            //if (b.Count == 0) return "";

            //path = new FileInfo((String) (b[0]));
            //for (var i = 1; i < b.Count; i++)
            //{
            //    path = new FileInfo(path, (String) (b[i]));
            //}
            //return path.FullName;
        }

        /// <summary>
        /// Creates a new parameter database from a given database file and argv
        /// list. The top-level database is completely empty, pointing to a second
        /// database which contains the parameter entries stored in args, which
        /// points to a tree of databases constructed using
        /// ParameterDatabase(filename).
        /// </summary>
        public ParameterDatabase(string pathNameRelativeToClassFile, Type cls, IList<string> args) // throws FileNotFoundException, IOException 
            : this()
        {
            // TODO : TEST THIS!
            Label = "" + cls + " : " + pathNameRelativeToClassFile;

            var files = new ParameterDatabase(pathNameRelativeToClassFile, cls);

            // Create the Parameter Database for the arguments
            var a = new ParameterDatabase
                        {
                            RelativeClass = cls,
                            RelativePath = SimplifyPath(pathNameRelativeToClassFile)
                        };

            a.Parents.Add(files);
            var hasArgs = false;
            for (var x = 0; x < args.Count - 1; x++)
            {
                if (args[x].Equals("-p"))
                {
                    var s = args[x + 1].Trim();
                    if (s.Length == 0) continue; // failure
                    var eq = s.IndexOf('='); // look for the '='
                    if (eq <= 0) continue; // '=' isn't there, or it's the first char: failure                      
                    this[s.Substring(0, eq)] = s.Substring(eq + 1);
                    if (!hasArgs)
                    {
                        Label = Label + "    Args:  ";
                        hasArgs = true;
                    }
                    Label = Label + s + "  ";
                }
            }

            // Set me up
            RelativeClass = cls;
            RelativePath = SimplifyPath(pathNameRelativeToClassFile);

            Parents.Add(a);
        }

        /// <summary>
        /// Creates a new parameter database loaded from a parameter file located relative to a class file,
        /// wherever the class file may be (such as in a jar).
        /// This approach uses resourceLocation.getResourceAsStream() to load the parameter file.
        /// If parent.n are defined, parents will be attempted to be loaded -- that's
        /// the reason for the FileNotFoundException and IOException.
        /// </summary>
        public ParameterDatabase(string pathNameRelativeToClassFile, Type cls) // throws FileNotFoundException, IOException 
            : this()
        {
            // TODO : Implement this!
            throw new NotImplementedException();
            //Label = "" + cls + " : " + pathNameRelativeToClassFile;
            //RelativeClass = cls;
            //RelativePath = SimplifyPath(pathNameRelativeToClassFile);
            //Load(cls.GetResourceAsStream(RelativePath));

            ////listeners = new Vector();

            //// load parents
            //for (int x = 0 ; ; x++) 
            //    {
            //    String s = GetProperty("parent." + x);
            //    if (s == null)
            //        return; // we're done

            //    if (Path.IsPathRooted(new FileInfo(s).FullName)) // it's an absolute file definition
            //        Parents.Add(new ParameterDatabase(new FileInfo(s)));
            //    else if (s.StartsWith(C_CLASS))
            //        {
            //        int i = IndexOfFirstWhitespace(s);
            //        if (i == -1) throw new FileNotFoundException("Could not parse file into filename and classname:\n\tparent." + x + " = " + s);
            //        String classname = s.Substring(0,i);
            //        String filename = s.Substring(i).Trim();
            //        try
            //            {
            //            Parents.Add(new ParameterDatabase(filename, Type.GetType(classname.Substring(1).Trim())));
            //            }
            //        catch (TypeLoadException ex)
            //            {
            //            throw new FileNotFoundException("Could not parse file into filename and classname:\n\tparent." + x + " = " + s);
            //            }
            //        }
            //    else
            //        {
            //        String path = new File(new FileInfo(pathNameRelativeToClassFile).GetParent(), s).ToString();
            //        Parents.Add(new ParameterDatabase(path, cls));
            //        }
            //    }
        }

        /// <summary>
        /// Creates a new parameter database loaded from the given stream.  Non-relative Parents are not permitted.
        /// If parent.n are defined, Parents will be attempted to be loaded -- that's 
        /// the reason for the FileNotFoundException and IOException. 
        /// </summary>
        public ParameterDatabase(Stream stream)
            : this()
        {
            // TODO : Test this!

            Label = "Stream: " + stream.GetHashCode();
            Load(stream);

            _listeners = new List<ParameterDatabaseListener>();

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
                else if (s.StartsWith(C_CLASS))
                {
                    var i = IndexOfFirstWhitespace(s);
                    if (i == -1) throw new FileNotFoundException("Could not parse file into filename and classname:\n\tparent." + x + " = " + s);
                    var classname = s.Substring(0, i);
                    var filename = s.Substring(i).Trim();
                    try
                    {
                        Parents.Add(new ParameterDatabase(filename, Type.GetType(classname)));
                    }
                    catch (TypeLoadException ex)
                    {
                        throw new FileNotFoundException("Could not parse file into filename and classname:\n\tparent." + x + " = " + s);
                    }
                }
                else
                    throw new FileNotFoundException("Attempt to load a relative file, but there's no parent file: " + s);
            }
        }

        /// <summary> 
        /// Creates a new parameter database tree from a given database file and its parent files.
        /// </summary>
        public ParameterDatabase(FileInfo fileInfo)
            : this()
        {
            // BRS : TODO : Requires further conversion and testing.
            Filename = fileInfo.Name;
            Label = "File: " + fileInfo.FullName;
            Directory = new FileInfo(fileInfo.DirectoryName); // get the directory filename is in

            Load(new FileStream(fileInfo.FullName, FileMode.Open));

            _listeners = new List<ParameterDatabaseListener>(10);

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
                else if (s.StartsWith(C_CLASS))
                {
                    var i = IndexOfFirstWhitespace(s);
                    if (i == -1)
                        throw new FileNotFoundException("Could not parse file into filename and classname:\n\tparent." +
                                                        x + " = " + s);
                    var classname = s.Substring(0, i);
                    var fname = s.Substring(i).Trim();
                    try
                    {
                        Parents.Add(new ParameterDatabase(fname, Type.GetType(classname.Substring(1).Trim())));
                    }
                    catch (TypeLoadException ex)
                    {
                        throw new FileNotFoundException("Could not parse file into filename and classname:\n\tparent." +
                                                        x + " = " + s);
                    }
                } // it's relative to my path
                else
                {
                    Parents.Add(new ParameterDatabase(new FileInfo(Path.Combine(fileInfo.DirectoryName, s))));
                }
            }
        }

        /// <summary> 
        /// Creates a new parameter database from a given database file and argv
        /// list. The top-level database is completely empty, pointing to a second
        /// database which contains the parameter entries stored in args, which
        /// points to a tree of databases constructed using
        /// ParameterDatabase(filename).
        /// </summary>
        public ParameterDatabase(FileInfo fileInfo, IList<string> args)
            : this()
        {
            Filename = fileInfo.Name;
            Label = "File: " + fileInfo.FullName;
            Directory = new FileInfo(fileInfo.DirectoryName); // get the directory
            // filename is in

            // Create the Parameter Database tree for the files
            var files = new ParameterDatabase(fileInfo);

            // Create the Parameter Database for the arguments
            var a = new ParameterDatabase();
            a.Parents.Add(files);
            var hasArgs = false;
            for (var x = 0; x < args.Count - 1; x++)
            {
                if (!args[x].Equals("-p")) continue;
                var s = args[x + 1].Trim();
                if (s.Length == 0) continue;
                var eq = s.IndexOf('=');
                if (eq <= 0) continue;
                this[s.Substring(0, eq)] = s.Substring(eq + 1);
                if (!hasArgs)
                {
                    Label = Label + "    Args:  ";
                    hasArgs = true;
                }
                Label = Label + s + "  ";
            }

            // Set me up
            Parents.Add(a);
            _listeners = new List<ParameterDatabaseListener>(10);
        }

        public ParameterDatabase(XElement xml)
        {
            // TODO : Implement this!
            throw new NotImplementedException();
        }

        #endregion // Constructors
        #region Object Overrides    **************************************************************************

        public override string ToString()
        {
            lock (_syncLock)
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
        }

        #endregion // Object Overrides

        #region ITrackParameter     **************************************************************************

        private readonly Dictionary<string, bool> _gotten = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> _accessed = new Dictionary<string, bool>();
        private readonly List<ParameterDatabaseListener> _listeners = new List<ParameterDatabaseListener>(10);

        /// <summary>
        /// List of parameters which were requested and ones which furthermore were fulfilled.
        /// </summary>
        public Dictionary<string, bool> Gotten { get { return _gotten; } }
        public Dictionary<string, bool> Accessed { get { return _accessed; } }
        public List<ParameterDatabaseListener> Listeners { get { return _listeners; } }

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
                var vec = new List<string>(10);
                var e = _gotten.Keys.GetEnumerator();

                while (e.MoveNext())
                {
                    if (e.Current != null)
                        vec.Add(e.Current);
                }

                // sort the keys
                var array = vec.ToArray();

                vec.Sort();

                // Uncheck and print each item
                foreach (var t in array)
                {
                    var s = t;
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

                var e = _gotten.Keys.GetEnumerator();

                while (e.MoveNext())
                {
                    if (e.Current != null)
                        all.Remove(e.Current);
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
                foreach (var t in array)
                {
                    var s = t;
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

                var e = _accessed.Keys.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current != null)
                        all.Remove(e.Current);
                }

                var i = all.Keys.GetEnumerator();
                while (i.MoveNext())
                {
                    vec.Add(i.Current);
                }

                // sort the keys
                var array = vec.ToArray();

                vec.Sort();

                // Uncheck and print each item
                foreach (var t in array)
                {
                    var s = t;
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
                var vec = new ArrayList(10);

                var e = _accessed.Keys.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current != null)
                        vec.Add(e.Current);
                }

                // sort the keys
                var array = vec.ToArray();

                vec.Sort();

                // Uncheck and print each item
                foreach (var t in array)
                {
                    var s = (string)t;
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

                var vec = new ArrayList(10);
                var e = gather.Keys.GetEnumerator();

                while (e.MoveNext())
                {
                    vec.Add(e.Current);
                }

                // sort the keys
                vec.Sort();

                // Uncheck and print each item
                foreach (var t in vec)
                {
                    var s = (string)t;
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

        #endregion // List (Print)

        #region Listeners   *************************************************************************************

        public event ParameterDatabaseListenerDelegate ParameterDatabaseListenerDelegateVar;
        public virtual void AddListener(ParameterDatabaseListener l)
        {
            lock (this) { _listeners.Add(l); }
        }
        public virtual void RemoveListener(ParameterDatabaseListener l)
        {
            lock (this) { _listeners.Remove(l); }
        }

        #endregion // Listeners

        #endregion // ITrackParameter
        #region IParseParameter     **************************************************************************

        #region Get Instance and Class   *********************************************************************

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a full Class name, and the class must be a descendent of but not
        /// equal to <i>mustCastToBaseClass </i>. Loads the class and returns an
        /// instance (constructed with the default constructor), or throws a
        /// ParamClassLoadException if there is no such Class. If the parameter is
        /// not found, the defaultParameter is used. The parameter chosen is marked "used".
        /// </summary>
        public virtual object GetInstanceForParameter(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseType)
        {
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);
                IParameter p;
                if (_exists(parameter))
                    p = parameter;
                else if (_exists(defaultParameter))
                    p = defaultParameter;
                else
                {
                    throw new ParamClassLoadException("No type name provided.\nPARAMETER: " + parameter +
                                                      (defaultParameter == null
                                                           ? ""
                                                           : "\n     ALSO: " + defaultParameter));
                }
                try
                {
                    var param = Take(p);
                    var t = ECActivator.GetType(param);
                    if (!mustCastToBaseType.IsAssignableFrom(t))
                    {
                        throw new ParamClassLoadException("The type " + t.FullName +
                                                          "\ndoes not cast into the base type " +
                                                          mustCastToBaseType.FullName
                                                          + "\nPARAMETER: " + parameter +
                                                          (defaultParameter == null
                                                               ? ""
                                                               : "\n     ALSO: " + defaultParameter));
                    }
                    if (mustCastToBaseType == t)
                    {
                        throw new ParamClassLoadException("The type " + t.FullName +
                                                          "\nmust not be the same as the required base type " +
                                                          mustCastToBaseType.FullName
                                                          + "\nPARAMETER: " + parameter +
                                                          (defaultParameter == null
                                                               ? ""
                                                               : "\n     ALSO: " + defaultParameter));
                    }
                    return Activator.CreateInstance(t);

                }
                    // BRS : 2009-03-15
                    // Calling Type.GetType(string s) throws TypeLoadException if the typeName (s) is invalid
                catch (TypeLoadException e)
                {
                    throw new ParamClassLoadException("Type not found: " + Take(p) + "\nPARAMETER: " + parameter
                                                      +
                                                      (defaultParameter == null
                                                           ? ""
                                                           : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" +
                                                      e);
                }
                    // Since we've made it this far we already know that Type.GetType(string s) has given us a valid Type
                    // Calling Activator.CreateInstance(Type t) throws TargetInvocationException if the constructor fails
                catch (TargetInvocationException e)
                {
                    throw new ParamClassLoadException("Could not load type: "

                                                      + Take(p) + "\nPARAMETER: " + parameter
                                                      +
                                                      (defaultParameter == null
                                                           ? ""
                                                           : "\n     ALSO: " + defaultParameter)
                                                      + "\nEXCEPTION: \n\n" + e);
                }
                catch (MemberAccessException e)
                {
                    throw new ParamClassLoadException("The requested type is an interface or an abstract type: "
                                                      + Take(p) + "\nPARAMETER: " + parameter
                                                      +
                                                      (defaultParameter == null
                                                           ? ""
                                                           : "\n     ALSO: " + defaultParameter)
                                                      + "\nEXCEPTION: \n\n" + e);
                }
                catch (Exception e)
                {
                    throw new ParamClassLoadException("The requested type cannot be initialized with the default initializer: "
                                                      + Take(p) + "\nPARAMETER: " + parameter
                                                      +
                                                      (defaultParameter == null
                                                           ? ""
                                                           : "\n     ALSO: " + defaultParameter)
                                                      + "\nEXCEPTION: \n\n" + e);
                }
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a full Class name, and the class must be a descendent, or equal
        /// to, <i>mustCastToBaseclass </i>. Loads the class and returns an instance
        /// (constructed with the default constructor), or throws a
        /// ParamClassLoadException if there is no such Class. The parameter chosen is marked "used".
        /// </summary>
        public virtual object GetInstanceForParameterEq(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseType)
        {
            lock (_syncLock)
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
                                                      + "\n     ALSO: " +
                                                      (defaultParameter == null
                                                           ? ""
                                                           : "\n     ALSO: " + defaultParameter));
                }
                try
                {
                    var param = Take(p);
                    var t = ECActivator.GetType(param);
                    //var c = Type.GetType(Take(p));
                    if (!mustCastToBaseType.IsAssignableFrom(t))
                    {
                        throw new ParamClassLoadException("The type " + t.FullName +
                                                          "\ndoes not cast into the base type "
                                                          + mustCastToBaseType.FullName + "\nPARAMETER: " + parameter
                                                          + "\n     ALSO: " +
                                                          (defaultParameter == null
                                                               ? ""
                                                               : "\n     ALSO: " + defaultParameter));
                    }
                    return Activator.CreateInstance(t);
                }
                    // BRS : 2009-03-15
                    // Calling Type.GetType(string s) throws TypeLoadException if the typeName (s) is invalid
                catch (TypeLoadException e)
                {
                    throw new ParamClassLoadException("Type not found: " + Take(p) + "\nPARAMETER: " + parameter +
                                                      "\n     ALSO: "
                                                      +
                                                      (defaultParameter == null
                                                           ? ""
                                                           : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" +
                                                      e);
                }
                catch (TargetInvocationException e)
                {
                    throw new ParamClassLoadException("Could not load type: " + Take(p) + "\nPARAMETER: " + parameter +
                                                      "\n     ALSO: "
                                                      +
                                                      (defaultParameter == null
                                                           ? ""
                                                           : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" +
                                                      e);
                }
                catch (MemberAccessException e)
                {
                    throw new ParamClassLoadException("The requested type is an interface or an abstract class: " +
                                                      Take(p) + "\nPARAMETER: "
                                                      + parameter + "\n     ALSO: " +
                                                      (defaultParameter == null
                                                           ? ""
                                                           : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" +
                                                      e);
                }
                catch (Exception e)
                {
                    throw new ParamClassLoadException("The requested type cannot be initialized with the default initializer: "
                                                      + Take(p) + "\nPARAMETER: " + parameter + "\n     ALSO: " +
                                                      (defaultParameter == null
                                                           ? ""
                                                           : "\n     ALSO: "
                                                             + defaultParameter) + "\nEXCEPTION: \n\n" + e);
                }
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter. The value
        /// associated with this parameter must be a full Class name, and the class
        /// must be a descendent of but not equal to <i>mustCastToBaseClass </i>.
        /// Loads and returns the associated Class, or throws a
        /// ParamClassLoadException if there is no such Class. If the parameter is
        /// not found, the defaultParameter is used. The parameter chosen is marked "used".
        /// </summary>
        public virtual object GetTypeForParameter(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseType)
        {
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);
                IParameter p;
                if (_exists(parameter))
                    p = parameter;
                else if (_exists(defaultParameter))
                    p = defaultParameter;
                else
                {
                    throw new ParamClassLoadException("No type name provided.\nPARAMETER: " + parameter +
                                                      "\n     ALSO: "
                                                      +
                                                      (defaultParameter == null
                                                           ? ""
                                                           : "\n     ALSO: " + defaultParameter));
                }
                try
                {
                    var param = Take(p);
                    var t = ECActivator.GetType(param);
                    //var c = Type.GetType(Take(p));
                    if (!mustCastToBaseType.IsAssignableFrom(t))
                    {
                        throw new ParamClassLoadException("The type " + t.FullName +
                                                          "\ndoes not cast into the base type "
                                                          + mustCastToBaseType.FullName + "\nPARAMETER: " + parameter +
                                                          "\n     ALSO: "
                                                          +
                                                          (defaultParameter == null
                                                               ? ""
                                                               : "\n     ALSO: " + defaultParameter));
                    }
                    return t;
                }
                    // BRS : 2009-03-15
                    // Calling Type.GetType(string s) throws TypeLoadException if the typeName (s) is invalid
                catch (TypeLoadException e)
                {
                    throw new ParamClassLoadException("Type not found: " + Take(p) + "\nPARAMETER: " + parameter +
                                                      "\n     ALSO: "
                                                      +
                                                      (defaultParameter == null
                                                           ? ""
                                                           : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" +
                                                      e);
                }
                catch (Exception e)
                {
                    throw new ParamClassLoadException("Could not load type: " + Take(p) + "\nPARAMETER: " + parameter +
                                                      "\n     ALSO: "
                                                      +
                                                      (defaultParameter == null
                                                           ? ""
                                                           : "\n     ALSO: " + defaultParameter) + "\nEXCEPTION: \n\n" +
                                                      e);
                }
            }
        }

        #endregion //Get Instance and Class

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
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);

                if (_exists(parameter))
                    return GetBoolean(parameter, defaultValue);

                return GetBoolean(defaultParameter, defaultValue);
            }
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
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);
                if (_exists(parameter))
                    return GetInt(parameter);
                if (_exists(defaultParameter))
                    return GetInt(defaultParameter);

                throw new FormatException("Integer does not exist for either parameter " + parameter + "\nor\n" +
                                          defaultParameter);
            }
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
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);

                if (_exists(parameter))
                    return GetInt(parameter, minValue);

                return GetInt(defaultParameter, minValue);
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, which must be
        /// an integer. If there is an error in parsing the parameter, then default
        /// is returned. The parameter chosen is marked "used" if it exists. Integers
        /// may be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual int GetIntWithDefault(IParameter parameter, IParameter defaultParameter, int defaultValue)
        {
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);

                if (_exists(parameter))
                    return GetIntWithDefault(parameter, defaultValue);

                return GetIntWithDefault(defaultParameter, defaultValue);
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
        public virtual int GetIntWithMax(IParameter parameter, IParameter defaultParameter, int minValue, int maxValue)
        {
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);
                if (_exists(parameter))
                    return GetIntWithMax(parameter, minValue, maxValue);

                return GetIntWithMax(defaultParameter, minValue, maxValue);
            }
        }

        #endregion // Get Int

        #region Get Float   **********************************************************************************

        /// <summary>
        /// Searches down through databases to find a given parameter, whose value
        /// must be a float. It returns the value, else throws a
        /// NumberFormatException exception if there is an error in parsing the
        /// parameter. The parameter chosen is marked "used" if it exists.
        /// </summary>
        public float GetFloat(IParameter parameter, IParameter defaultParameter) // throws NumberFormatException 
        {
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);
                if (_exists(parameter))
                    return GetFloat(parameter);
                if (_exists(defaultParameter))
                    return GetFloat(defaultParameter);

                throw new FormatException("Float does not exist for either parameter "
                                          + parameter + "\nor\n" + defaultParameter);
            }
        }

        /// <summary>
        /// Searches down through databases to find a given parameter, whose value
        /// must be a float >= minValue. If not, this method returns minvalue-1, else
        /// it returns the parameter value. The parameter chosen is marked "used" if
        /// it exists.
        /// </summary>
        public virtual float GetFloat(IParameter parameter, IParameter defaultParameter, double minValue)
        {
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);

                if (_exists(parameter))
                    return GetFloat(parameter, minValue);

                return GetFloat(defaultParameter, minValue);
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, which must be
        /// a float. If there is an error in parsing the parameter, then default is
        /// returned. The parameter chosen is marked "used" if it exists.
        /// </summary>
        public virtual float GetFloatWithDefault(IParameter parameter, IParameter defaultParameter, double defaultValue)
        {
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);

                if (_exists(parameter))
                    return GetFloatWithDefault(parameter, defaultValue);

                return GetFloatWithDefault(defaultParameter, defaultValue);
            }
        }

        /// <summary>
        /// Searches down through databases to find a given parameter, whose value
        /// must be a float >= minValue and &lt;= maxValue. If not, this method returns
        /// minvalue-1, else it returns the parameter value. The parameter chosen is
        /// marked "used" if it exists.
        /// </summary>
        public float GetFloatWithMax(IParameter parameter, IParameter defaultParameter, double minValue, double maxValue)
        {
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);
                if (_exists(parameter))
                    return GetFloat(parameter, minValue, maxValue);

                return GetFloat(defaultParameter, minValue, maxValue);
            }
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
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);
                if (_exists(parameter))
                    return GetLong(parameter);
                if (_exists(defaultParameter))
                    return GetLong(defaultParameter);

                throw new FormatException("Long does not exist for either parameter " + parameter + "\nor\n" +
                                          defaultParameter);
            }
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
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);

                if (_exists(parameter))
                    return GetLong(parameter, minValue);

                return GetLong(defaultParameter, minValue);
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, which must be
        /// a long. If there is an error in parsing the parameter, then default is
        /// returned. The parameter chosen is marked "used" if it exists. Longs may
        /// be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual long GetLongWithDefault(IParameter parameter, IParameter defaultParameter, long defaultValue)
        {
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);

                if (_exists(parameter))
                    return GetLongWithDefault(parameter, defaultValue);

                return GetLongWithDefault(defaultParameter, defaultValue);
            }
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
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);

                if (_exists(parameter))
                    return GetLongWithMax(parameter, minValue, maxValue);

                return GetLongWithMax(defaultParameter, minValue, maxValue);
            }
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
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);

                if (_exists(parameter))
                    return GetFile(parameter);

                return GetFile(defaultParameter);
            }
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

        #region GetValueType<T>   ***********************************************************************************

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a struct. It returns the value, else throws a NumberFormatException
        /// exception if there is an error in parsing the parameter. The parameter
        /// chosen is marked "used" if it exists. Longs may be in decimal or (if
        /// preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual T GetValueType<T>(IParameter parameter, IParameter defaultParameter)
            where T : struct, IConvertible, IComparable
        {
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);
                if (_exists(parameter))
                    return GetValueType<T>(parameter);
                if (_exists(defaultParameter))
                    return GetValueType<T>(defaultParameter);

                throw new FormatException("Long does not exist for either parameter " + parameter + "\nor\n" +
                                          defaultParameter);
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a value type >= minValue. If not, this method returns errValue, else it
        /// returns the parameter value. The parameter chosen is marked "used" if it
        /// exists. Value types may be in decimal or (if preceded with an X or x) in
        /// hexadecimal.
        /// </summary>	
        public virtual T GetValueType<T>(IParameter parameter, IParameter defaultParameter, T minValue)
            where T : struct, IConvertible, IComparable
        {
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);

                if (_exists(parameter))
                    return GetValueType<T>(parameter, minValue);

                return GetValueType<T>(defaultParameter, minValue);
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, which must be
        /// a value type. If there is an error in parsing the parameter, then default is
        /// returned. The parameter chosen is marked "used" if it exists. Value types may
        /// be in decimal or (if preceded with an X or x) in hexadecimal.
        /// </summary>
        public virtual T GetValueTypeWithDefault<T>(IParameter parameter, IParameter defaultParameter, T defaultValue)
            where T : struct, IConvertible, IComparable
        {
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);

                if (_exists(parameter))
                    return GetValueTypeWithDefault<T>(parameter, defaultValue);

                return GetValueTypeWithDefault<T>(defaultParameter, defaultValue);
            }
        }

        /// <summary> 
        /// Searches down through databases to find a given parameter, whose value
        /// must be a value type >= minValue and = &lt; maxValue. If not, this method returns
        /// errValue, else it returns the parameter value. The parameter chosen is
        /// marked "used" if it exists. Value types may be in decimal or (if preceded with
        /// an X or x) in hexadecimal.
        /// </summary>
        public virtual T GetValueTypeWithMax<T>(IParameter parameter, IParameter defaultParameter, T minValue, T maxValue)
            where T : struct, IConvertible, IComparable
        {
            lock (_syncLock)
            {
                PrintGotten(parameter, defaultParameter, false);

                if (_exists(parameter))
                    return GetValueTypeWithMax(parameter, minValue, maxValue);

                return GetValueTypeWithMax(defaultParameter, minValue, maxValue);
            }
        }

        #endregion // GetValueType<T>

        #endregion // IParseParameter
        #region IParameterSource    **************************************************************************

        #region Fields Internal 

        /// <summary>
        /// The parents of this database.
        /// </summary>
        internal readonly List<IParameterDatabase> Parents = new List<IParameterDatabase>();

        /// <summary>
        /// If the database was loaded via a file, this holds the directory of the database.
        /// </summary>
        internal FileInfo Directory;

        internal string Filename;

        /// <summary>
        /// A flag (unchecked by Uncheck()) for not hitting the same database twice in a graph search
        /// </summary>
        internal bool CheckState;

        /// <summary>
        /// If the database was loaded via GetResource(), this holds the type that was loaded.
        /// </summary>
        internal Type RelativeClass { get; set; }

        /// <summary>
        /// If the database was loaded via GetResource(), this holds the relative path used in that load.
        /// </summary>
        internal string RelativePath { get; set; }       
        
        #endregion // Fields Internal
        #region Public Properties

        /// <summary>
        /// A descriptive name of the parameter database.
        /// </summary>
        public string Label { get; set; }

        public bool SourceExists { get { throw new NotImplementedException(); } }

        #endregion // Public Properties
        #region File

        /// <summary> 
        /// Searches down through databases to find the directory for the database
        /// which holds a given parameter. Returns the directory name or null if not found.
        /// </summary>		
        public virtual FileInfo DirectoryFor(IParameter parameter)
        {
            lock (_syncLock)
            {
                var result = _directoryFor(parameter);
                Uncheck();
                return result;
            }
        }

        /// <summary> 
        /// Searches down through databases to find the parameter file 
        /// which holds a given parameter. Returns the filename or null if not found.
        /// </summary>		
        public virtual FileInfo FileFor(IParameter parameter)
        {
            lock (_syncLock)
            {
                var result = _fileFor(parameter);
                Uncheck();
                return result;
            }
        }

        /**
         * Searches down through the databases to find a given parameter, whose
         * value must be an absolute or relative path name. If it is absolute, a
         * file is made based on the path name, and an InputStream is opened on 
         * the file and returned.  If the path name begins with "$", then an
         * InputStream is opened on a file relative to the directory where the
         * system was started.  Otherwise if the path name is relative, an InputStream is made by
         * resolving the path name with respect to the directory in which the file
         * was which defined this ParameterDatabase in the ParameterDatabase
         * hierarchy, be it in the file system or in a jar file.  If the parameter is not found, 
         * this returns null.  If no such file exists, null is also returned.
         * The parameter chosen is marked "used" if it exists.
         */

        public Stream GetResource(IParameter parameter, IParameter defaultParameter)
        {
            PrintGotten(parameter, defaultParameter, false);
            if (_exists(parameter))
                return GetResource(parameter);
            return GetResource(defaultParameter);
        }

        /** Private helper function */
        private static int IndexOfFirstWhitespace(String s)
        {
            var len = s.Length;
            for (var i = 0; i < len; i++)
                if (char.IsWhiteSpace(s[i]))
                    return i;
            return -1;
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

                    //if (p.StartsWith(C_CLASS))
                    //{
                    //    var i = IndexOfFirstWhitespace(p);
                    //    if (i == -1)
                    //        return null;
                    //    var classname = p.Substring(0, i);
                    //    var filename = p.Substring(i).Trim();
                    //    return Type.GetType(classname).getResourceAsStream(filename);
                    //}

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

        public IParameterDatabase GetLocation(IParameter parameter)
        {
            return GetLocation(parameter.Param);
        }

        //[Synchronized]
        public IParameterDatabase GetLocation(string parameter)
        {
            var loc = _getLocation(parameter);
            Uncheck();
            return loc;
        }

        /// <summary>
        /// Returns a String describing the location of the ParameterDatabase holding this parameter, or "" if there is none.
        /// </summary>
        public String GetLabel()
        {
            return Label;
            /*        File file = fileFor(parameter);
                      if (file == null) return "";
                      try { return file.getCanonicalPath(); }
                      catch (IOException e) { return ""; }
            */
        }

        #endregion // File
        #region TreeDictionary

        public virtual void AddParent(IParameterDatabase database)
        {
            Parents.Add(database);
        }

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
                    PrintState = PS_UNKNOWN;
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

        /// <summary> 
        /// Returns true if either parameter or defaultParameter exists in the database
        /// </summary>
        public virtual bool ParameterExists(IParameter parameter, IParameter defaultParameter)
        {
            lock (this)
            {
                PrintGotten(parameter, defaultParameter, true);
                if (_exists(parameter)) // returns false for null arg
                    return true;
                if (_exists(defaultParameter)) // returns false for null arg
                    return true;
                return false;
            }
        }

        #endregion

        public string ToString(string initialIndent) { throw new NotImplementedException(); }
        public XElement ToXml() { throw new NotImplementedException(); }

        #endregion // IParameterSource

        #region Main (for testing)

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
            var tempWriter = new StreamWriter(Console.OpenStandardError(), Encoding.Default) { AutoFlush = true };
            pd.List(tempWriter, true);

            Console.Error.WriteLine("\n\n PRINTING ONLY VALID PARAMETERS \n\n");
            var tempWriter2 = new StreamWriter(Console.OpenStandardError(), Encoding.Default) { AutoFlush = true };
            pd.List(tempWriter2, false);
        }

        #endregion // Main (for testing)
    }
}