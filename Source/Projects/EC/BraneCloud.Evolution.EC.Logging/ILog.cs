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

using System.IO;

namespace BraneCloud.Evolution.EC.Logging
{
    public interface ILog
    {
        StreamWriter Writer { get; set; }
        FileInfo FileInfo { get; set; }
        LogRestarter Restarter { get; set; }

        /// <summary>
        /// Should we write to this log at all?
        /// </summary>
        bool Silent { get; set; }
        bool PostAnnouncements { get; set; }
        bool RepostAnnouncementsOnRestart { get; set; }
        bool AppendOnRestart { get; set; }
        bool IsLoggingToSystemOut { get; set; }

        Log Restart();
        Log Reopen();
    }
}