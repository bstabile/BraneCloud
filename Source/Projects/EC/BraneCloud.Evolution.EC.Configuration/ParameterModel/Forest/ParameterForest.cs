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
using System.Linq;
using System.Text;

namespace BraneCloud.Evolution.EC.Configuration
{
    public class ParameterForest : IParameterForest
    {
        public ParameterForest()
        {
            Sources = new Dictionary<string, IParameterSource>();
            Nodes = new Dictionary<string, File>();
            Trees = new Dictionary<string, FileDictionaryTree>();
        }
        public IDictionary<string, IParameterSource> Sources { get; protected set; }
        public IDictionary<string, File> Nodes { get; protected set; }
        public IDictionary<string, FileDictionaryTree> Trees { get; protected set; }
    }
}