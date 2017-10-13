﻿/*
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

namespace BraneCloud.Evolution.EC.Configuration
{
    public interface IParseParameter
    {
        object GetInstanceForParameter(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseType);
        object GetInstanceForParameterEq(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseType);
        object GetTypeForParameter(IParameter parameter, IParameter defaultParameter, Type mustCastToBaseType);

        bool GetBoolean(IParameter parameter, IParameter defaultParameter, bool defaultValue);

        int GetInt(IParameter parameter, IParameter defaultParameter);
        int GetInt(IParameter parameter, IParameter defaultParameter, int minValue);
        int GetIntWithDefault(IParameter parameter, IParameter defaultParameter, int defaultValue);
        int GetIntWithMax(IParameter parameter, IParameter defaultParameter, int minValue, int maxValue);

        float GetFloat(IParameter parameter, IParameter defaultParameter, double minValue);
        float GetFloatWithDefault(IParameter parameter, IParameter defaultParameter, double defaultValue);
        float GetFloatWithMax(IParameter parameter, IParameter defaultParameter, double minValue, double maxValue);

        double GetDouble(IParameter parameter, IParameter defaultParameter);
        double GetDouble(IParameter parameter, IParameter defaultParameter, double minValue);
        double GetDoubleWithDefault(IParameter parameter, IParameter defaultParameter, double defaultValue);
        double GetDoubleWithMax(IParameter parameter, IParameter defaultParameter, double minValue, double maxValue);

        long GetLong(IParameter parameter, IParameter defaultParameter);
        long GetLong(IParameter parameter, IParameter defaultParameter, long minValue);
        long GetLongWithDefault(IParameter parameter, IParameter defaultParameter, long defaultValue);
        long GetLongWithMax(IParameter parameter, IParameter defaultParameter, long minValue, long maxValue);

        T GetValueType<T>(IParameter parameter, IParameter defaultParameter) where T : struct, IConvertible, IComparable;
        T GetValueType<T>(IParameter parameter, IParameter defaultParameter, T minValue) where T : struct, IConvertible, IComparable;
        T GetValueTypeWithDefault<T>(IParameter parameter, IParameter defaultParameter, T defaultValue) where T : struct, IConvertible, IComparable;
        T GetValueTypeWithMax<T>(IParameter parameter, IParameter defaultParameter, T minValue, T maxValue) where T : struct, IConvertible, IComparable;

        FileInfo GetFile(IParameter parameter, IParameter defaultParameter);

        string GetString(IParameter parameter, IParameter defaultParameter);
        string GetStringWithDefault(IParameter parameter, IParameter defaultParameter, string defaultValue);
    }
}