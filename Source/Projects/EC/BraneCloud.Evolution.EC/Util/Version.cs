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

        public const string name = "BrainCloud.EC";
        public const string version = "2.0.0.0";
        public const string versionECJ = "20";
        public const string copyright = "2011";
        public const string author = "Bennett R. Stabile";
        public const string authorECJ = "Sean Luke";
        public const string contributors = "L. Panait, G. Balan, S. Paus, Z. Skolicki, R. Kicinger, E. Popovici,";
        public const string contributors2 = "J. Harrison, J. Bassett, R. Hubley, A. Desai, and A. Chircop";
        public const string authorEmail = "bennett.stabile@gmail.com";
        public const string authorURL = "http://braincloud.codeplex.com";
        public const string date = "May 14, 2011";
        public const string minClrVersion = "4.0";

        #endregion // Constants
        
        public static string Message()
        {
            var netVersion = Environment.Version;

            return "\n| " + name 
                + "\n| Evolutionary Computation System for .NET based on ECJ"
                + "\n| Version " + version
                + "\n| Derivative Work by " + author
                + "\n| URL: " + authorURL 
                + "\n| Mail: " + authorEmail 
                + "\n| Date: " + date
                + "\n| ECJ Original Work By " + authorECJ
                + "\n| ECJ Contributors: " + contributors 
                + "\n|                   " + contributors2
                + "\n\n| Required .NET Framework: " + minClrVersion
                + "\n| Detected .NET Framework: " + netVersion + "\n";
        }
    }
}