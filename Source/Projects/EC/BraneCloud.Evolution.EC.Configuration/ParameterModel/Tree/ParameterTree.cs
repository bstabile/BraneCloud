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
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Xml.Linq;
using BraneCloud.Evolution.Collections.Generic;

namespace BraneCloud.Evolution.EC.Configuration
{
    public class ParameterTree : DictionaryTree<string, string>, IParameterTree
    {
        #region Protected Helper Methods

        protected void PrintGotten(IParameter parameter, IParameter defaultParameter, bool exists)
        {
            throw new NotImplementedException();
        }

        protected virtual void _list(StreamWriter writer, bool listShadowed, string prefix, Hashtable gather)
        {
            throw new NotImplementedException();
        }

        #endregion // Protected Helper Methods

        #region ISerializable

        protected ParameterTree(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext context)
        {
            
        }
        #endregion

        #region INamespace

        // The following INamespace properties are implemented on DictionaryTree
        //public string Name { get; protected set; }
        //public string FullName { get; protected set; }

        #endregion

        #region IParameterSource

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

        /// <summary>
        /// This is the one method of IParameterSource that should be defined by a concrete implementation.
        /// Because ParameterTree itself does not define any particular aquisition logic, this method
        /// will return false unless an override is specified by a derived type. 
        /// </summary>
        public virtual bool SourceExists { get { return false; } }

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
            throw new NotImplementedException();
        }

        public int GetInt(IParameter parameter, IParameter defaultParameter)
        {
            throw new NotImplementedException();
        }
        public int GetInt(IParameter parameter, IParameter defaultParameter, int minValue)
        {
            throw new NotImplementedException();
        }
        public int GetIntWithDefault(IParameter parameter, IParameter defaultParameter, int defaultValue)
        {
            throw new NotImplementedException();
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

        public T GetValueType<T>(IParameter parameter, IParameter defaultParameter) 
            where T : struct, IConvertible, IComparable
        {
            throw new NotImplementedException();
        }
        public T GetValueType<T>(IParameter parameter, IParameter defaultParameter, T minValue) 
            where T : struct, IConvertible, IComparable
        {
            throw new NotImplementedException();
        }
        public T GetValueTypeWithDefault<T>(IParameter parameter, IParameter defaultParameter, T defaultValue) 
            where T : struct, IConvertible, IComparable
        {
            throw new NotImplementedException();
        }
        public T GetValueTypeWithMax<T>(IParameter parameter, IParameter defaultParameter, T minValue, T maxValue) 
            where T : struct, IConvertible, IComparable
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

        #region ITrackParameter

        public void List(StreamWriter p)
        {
            throw new NotImplementedException();
        }
        public void List(StreamWriter p, bool listShadowed)
        {
            throw new NotImplementedException();
        }

        public void ListAccessed(StreamWriter p)
        {
            throw new NotImplementedException();
        }
        public void ListNotAccessed(StreamWriter p)
        {
            throw new NotImplementedException();
        }
        public void ListGotten(StreamWriter p)
        {
            throw new NotImplementedException();
        }
        public void ListNotGotten(StreamWriter p)
        {
            throw new NotImplementedException();
        }

        #region Listeners

        public List<ParameterDatabaseListener> Listeners { get; protected set; }

#pragma warning disable 67
        // BRS: TODO : Don't remove this even if the compiler tells you it's unused. (I will take care of this listener nonsense later)
        public event ParameterDatabaseListenerDelegate ParameterDatabaseListenerDelegateVar;
#pragma warning restore 67

        public void AddListener(ParameterDatabaseListener listener)
        {
            throw new NotImplementedException();
        }
        public void RemoveListener(ParameterDatabaseListener listener)
        {
            throw new NotImplementedException();
        }

        #endregion // Listeners

        #endregion // ITrackParameter

        #region Object Overrides

        /// <summary>
        /// This override of the Object.ToString() method defers to the overloaded method that accepts
        /// an "initialIndent" argument. In this case, that argument is simply passed as an empty string.
        /// </summary>
        /// <returns>A string representation of the current state of the instance data.</returns>
        public override string ToString()
        {
            return ToString("");
        }

        #endregion // Object Overrides
    }
}
