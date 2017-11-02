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
using System.Runtime.Serialization;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace BraneCloud.Evolution.EC.Configuration
{
    public interface IParameterDatabase : IParseParameter, ITrackParameter, ISerializable
    {
        /// <summary>
        /// This serializes and deserializes the instance (using BinaryFormatter) to create an exact copy.
        /// </summary>
        /// <returns>A new instance of the ParameterDatabase.</returns>
        IParameterDatabase DeepClone();

        #region ITrackParameter

        //Dictionary<string, bool> Gotten { get; }
        //Dictionary<string, bool> Accessed { get; }

        //void List(StreamWriter p);
        //void List(StreamWriter p, bool listShadowed);

        //void ListGotten(StreamWriter p);
        //void ListNotGotten(StreamWriter p);
        //void ListNotAccessed(StreamWriter p);
        //void ListAccessed(StreamWriter p);

        //List<ParameterDatabaseListener> Listeners { get; }
        //event ParameterDatabaseListenerDelegate ParameterDatabaseListenerDelegateVar;
        //void AddListener(ParameterDatabaseListener listener);
        //void RemoveListener(ParameterDatabaseListener listener);

        #endregion //  ITrackParameter
        #region IParseParameter

        //object GetInstanceForParameter(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseType);
        //object GetInstanceForParameterEq(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseType);
        //object GetTypeForParameter(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseType);

        //bool GetBoolean(IParameter parameter, IParameter defaultParameter, bool defaultValue);

        //int GetInt(IParameter parameter, IParameter defaultParameter);
        //int GetInt(IParameter parameter, IParameter defaultParameter, int minValue);
        //int GetIntWithDefault(IParameter parameter, IParameter defaultParameter, int defaultValue);
        //int GetIntWithMax(IParameter parameter, IParameter defaultParameter, int minValue, int maxValue);

        //float GetFloat(IParameter parameter, IParameter defaultParameter, double minValue);
        //float GetFloat(IParameter parameter, IParameter defaultParameter, double minValue, double maxValue);
        //float GetFloatWithDefault(IParameter parameter, IParameter defaultParameter, double defaultValue);
        //float GetFloatWithMax(IParameter parameter, IParameter defaultParameter, double minValue, double maxValue);

        //double GetDouble(IParameter parameter, IParameter defaultParameter);
        //double GetDouble(IParameter parameter, IParameter defaultParameter, double minValue);
        //double GetDouble(IParameter parameter, IParameter defaultParameter, double minValue, double maxValue);
        //double GetDoubleWithDefault(IParameter parameter, IParameter defaultParameter, double defaultValue);
        //double GetDoubleWithMax(IParameter parameter, IParameter defaultParameter, double minValue, double maxValue);

        //long GetLong(IParameter parameter, IParameter defaultParameter);
        //long GetLong(IParameter parameter, IParameter defaultParameter, long minValue);
        //long GetLong(IParameter parameter, IParameter defaultParameter, long minValue, long maxValue);
        //long GetLong(IParameter parameter, long minValue, long maxValue);
        //long GetLongWithDefault(IParameter parameter, IParameter defaultParameter, long defaultValue);
        //long GetLongWithMax(IParameter parameter, IParameter defaultParameter, long minValue, long maxValue);

        //FileInfo GetFile(IParameter parameter, IParameter defaultParameter);

        //string GetString(IParameter parameter, IParameter defaultParameter);
        //string GetStringWithDefault(IParameter parameter, IParameter defaultParameter, string defaultValue);

        #endregion //  IParseParameter
        #region IParameterSource

        string Label { get; set; }

        // BRS : TODO : Change to Strong Type
        HashSet<IParameter> GetShadowedValues(IParameter parameter);

        FileInfo DirectoryFor(IParameter parameter);
        FileInfo FileFor(IParameter parameter);

        FileInfo GetFile(IParameter parameter);
        DirectoryInfo GetDirectory(IParameter parameter);
        Stream GetResource(IParameter parameter, IParameter defaultParameter);

        void Remove(IParameter parameter);
        void RemoveDeeply(IParameter parameter);

        void AddParent(IParameterDatabase database);

        void SetParameter(IParameter parameter, string paramValue);

        bool SourceExists { get; }

        bool ParameterExists(IParameter parameter, IParameter defaultParameter);

        string ToString(string initialIndent);
        XElement ToXml();

        #endregion // IParameterSource


        // ************************* Protected and Private in ECJ *******************************************
        //bool GetBoolean(IParameter parameter, bool defaultValue);

        //int ParseInt(string stringRenamed); // Private in ECJ
        //long ParseLong(string stringRenamed); // Private in ECJ
        //int GetInt(IParameter parameter); // Protected in ECJ
        //int GetInt(IParameter parameter, int minValue); // Protected in ECJ
        //int GetIntWithDefault(IParameter parameter, int defaultValue); // Private in ECJ
        //int GetIntWithMax(IParameter parameter, int minValue, int maxValue); // Private in ECJ
        //float GetFloat(IParameter parameter, double minValue, double maxValue); // Private in ECJ
        //float GetFloat(IParameter parameter); // Private in ECJ
        //float GetFloatWithDefault(IParameter parameter, double defaultValue); // Private in ECJ
        //double GetDouble(IParameter parameter, double minValue); // Private in ECJ
        //double GetDouble(IParameter parameter, double minValue, double maxValue); // Private in ECJ
        //double GetDoubleWithDefault(IParameter parameter, double defaultValue); // Private in ECJ
        //long GetLong(IParameter parameter); // Private in ECJ
        //long GetLong(IParameter parameter, long minValue); // Private in ECJ
        //long GetLongWithDefault(IParameter parameter, long defaultValue); // Private in ECJ
        //long GetLongWithMax(IParameter parameter, long minValue, long maxValue); // Private in ECJ
        //FileInfo GetFile(IParameter parameter); // Private in ECJ
        //string GetString(IParameter parameter); // Private in ECJ
        //string GetStringWithDefault(IParameter parameter, string defaultValue); // Private in ECJ
        //void Uncheck(); Private in ECJ
        //void FireParameterSet(IParameter parameter, string valueRenamed); // Protected
        //void FireParameterAccessed(IParameter parameter, string valueRenamed); // Protected
        //void SetRenamed(IParameter parameter, string valueRenamed);
        //void PrintGotten(IParameter parameter, IParameter defaultParameter, bool exists); // Private in ECJ
        //string GetRenamed(IParameter parameter);
        //bool ParseCommandLineArg(string s);
        //TreeNode BuildTreeModel();

    }
}