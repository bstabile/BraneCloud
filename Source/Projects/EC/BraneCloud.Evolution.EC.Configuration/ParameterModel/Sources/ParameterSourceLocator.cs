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
    public class ParameterSourceLocator
    {
        public ParameterSourceLocator(string path)
        {
            Path = path;
            Type = ParameterSourceType.Default;
            Description = "";
        }

        public ParameterSourceLocator(string path, ParameterSourceType type)
        {
            Path = path;
            Type = type;
            Description = "";
        }

        public ParameterSourceLocator(string path, ParameterSourceType type, string description)
        {
            Path = path;
            Type = type;
            Description = description;
        }

        public string Path { get; protected set; }
        public ParameterSourceType Type { get; protected set; }
        public string Description { get; protected set; }
    }
}