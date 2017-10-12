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
using System.Xml.Linq;

namespace BraneCloud.Evolution.EC.Configuration.ParameterModel.Sources.Xml
{
    public class ParameterXmlReader
    {
        public static ParameterDatabase Read(string filePath)
        {
            //var fi = new FileInfo(filePath);
            //if (fi.Exists)
            //    throw new FileNotFoundException("The specified file could not be found.", filePath);
            //if (fi.Extension != ".xml")
            //    throw new FileLoadException("Invalid file extension. Expecting '.xml'", filePath);
            throw new NotImplementedException();
        }

        public static ParameterDatabase ParseXElement(XElement xml)
        {
            //if (xml == null)
            //    throw new ArgumentNullException("xml");
            //var pdb = new ParameterDatabase();
            //if (xml.)
            throw new NotImplementedException();
        }
    }
}