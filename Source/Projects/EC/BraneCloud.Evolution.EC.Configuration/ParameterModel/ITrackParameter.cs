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

namespace BraneCloud.Evolution.EC.Configuration
{
    public interface ITrackParameter
    {
        // The following are now part of ITrackDictionaryUsage
        //Dictionary<string, bool> Accessed { get; }
        //Dictionary<string, bool> Gotten { get; }

        void List(StreamWriter p);
        void List(StreamWriter p, bool listShadowed);

        void ListAccessed(StreamWriter p);
        void ListNotAccessed(StreamWriter p);
        void ListGotten(StreamWriter p);
        void ListNotGotten(StreamWriter p);

        #region Listeners

        List<ParameterDatabaseListener> Listeners { get; }
        event ParameterDatabaseListenerDelegate ParameterDatabaseListenerDelegateVar;
        void AddListener(ParameterDatabaseListener listener);
        void RemoveListener(ParameterDatabaseListener listener);

        #endregion // Listeners
    }
}