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
    public interface ITree : IParameterSource, IParseParameter, ITrackParameter
    {
        // BRS : TODO : Change to Strong Type
        HashSet<IParameter> GetShadowedValues(IParameter parameter);

        FileInfo DirectoryFor(IParameter parameter);
        FileInfo FileFor(IParameter parameter);

        void Remove(IParameter parameter);
        void RemoveDeeply(IParameter parameter);

        void AddParent(IParameterDatabase database);
    }
}