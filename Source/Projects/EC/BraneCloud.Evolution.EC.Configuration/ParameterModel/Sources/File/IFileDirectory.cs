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

using BraneCloud.Evolution.Archetype;

namespace BraneCloud.Evolution.EC.Configuration
{
    public interface IFileDirectory : INamespace
    {
        IFileDirectory this[string subdirectory] { get; }
        DirectoryInfo Info { get; }
        string FileSearchPattern { get; }
        IDictionary<string, File> Files { get; }
        IDictionary<string, File> Collect();
        IDictionary<string, File> Collect(IDictionary<string, File> registry);
        void Build();
        string ToString(string initialIndent);
        string ToXml();
    }
}