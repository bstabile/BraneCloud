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
using System.Collections.Specialized;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Support;

namespace BraneCloud.Evolution.EC.Util
{
    /// <summary> 
    /// Version is a static class which stores version information for this evolutionary computation system.
    /// </summary>
    [ECConfiguration("ec.util.ECVersion")]
    public class ECVersion
    {
        #region Constants

        public const string name = "BrainCloud.Evolution.EC";
        public const String version = "21";
        public const String copyright = "2013";
        public const String authorDotNet = "Bennett R. Stabile";
        public const String authorECJ = "Sean Luke";
        public const String contributors = "L. Panait, G. Balan, S. Paus, Z. Skolicki, R. Kicinger,";
        public const String contributors2 = "E. Popovici, K. Sullivan, J. Harrison, J. Bassett, R. Hubley,";
        public const String contributors3 = "A. Desai, A. Chircop, J. Compton, W. Haddon, S. Donnelly,";
        public const String contributors4 = "B. Jamil, J. Zelibor, E. Kangas, F. Abidi, H. Mooers,";
        public const String contributors5 = "J. O'Beirne, L. Manzoni, K. Talukder, and J. McDermott";
        public const String authorEmail0 = "ecj-help";
        public const String authorEmail1 = "cs.gmu.edu";
        public const String authorEmail2 = "(better: join ECJ-INTEREST at URL above)";
        public const String authorURL = "http://cs.gmu.edu/~eclab/projects/ecj/";
        public const String date = "May 1, 2013";
        public const string minClrVersion = "4.0";

        #endregion // Constants
        
        public static string Message()
        {
            var netVersion = Environment.Version;

            return "\n| " + name 
                + "\n| Evolutionary Computation System for .NET based on ECJ (GMU)"
                + "\n| Version " + version
                + "\n| Derivative Work by " + authorDotNet
                + "\n| URL: " + authorURL 
                + "\n| Mail: " + authorEmail0 + "@" + authorEmail1 + authorEmail2
                + "\n| Date: " + date
                + "\n| ECJ Original Work By " + authorECJ
                + "\n| ECJ Contributors: " + contributors 
                + "\n|                   " + contributors2
                + "\n|                   " + contributors3
                + "\n|                   " + contributors4
                + "\n|                   " + contributors5
                + "\n\n| Required .NET Framework: " + minClrVersion
                + "\n| Detected .NET Framework: " + netVersion + "\n";
        }
    }
}