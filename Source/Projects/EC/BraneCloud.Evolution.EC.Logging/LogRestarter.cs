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
	/// <summary> A LogRestarter is an abstract superclass of objects which are
	/// capable of restarting logs after a computer failure.   
	/// LogRestarters subclasses are generally used
	/// internally in Logs only; you shouldn't need to deal with them.
	/// 
	/// </summary>	
	[Serializable]
	public abstract class LogRestarter
	{
		/* recreate the writer for, and properly reopen a log
		upon a system restart from a checkpoint */
		public abstract Log Restart(Log l);
		
		/* close an existing log file and reopen it (non-appending),
		if that' appropriate for this kind of log.  Otherwise,
		don't do anything. */
		public abstract Log Reopen(Log l);
	}
}