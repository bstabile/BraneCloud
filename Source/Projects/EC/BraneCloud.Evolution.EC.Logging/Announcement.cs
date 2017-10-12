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

namespace BraneCloud.Evolution.EC.Logging
{
    /// <summary> 
    /// Announcements are messages which are stored by ec.util.Output in
    /// memory, in addition to being logged out to files.  The purpose of
    /// this is that announcements saved in the checkpointing process.
    /// You can turn off the memory-storage of announcements with an argument
    /// passed to ec.Evolve when you start the run.
    /// </summary>	
    [Serializable]
    public class Announcement
    {
        /// <summary>The announcement's...anouncement.</summary>
        public string Text;
        
        public Announcement(string t)
        {
            Text = t;
        }
    }
}