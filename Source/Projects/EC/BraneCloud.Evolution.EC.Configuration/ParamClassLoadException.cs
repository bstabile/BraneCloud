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

namespace BraneCloud.Evolution.EC.Configuration
{
	/// <summary> This exception is thrown by the Parameter Database when it fails to
	/// locate and load a class specified by a given parameter as requested.
	/// Most commonly this results in the program exiting with an error, so
	/// it is defined as a RuntimeException so you don't have to catch it
	/// or declare that you throw it.
	/// 
	/// </summary>
	
	[Serializable]
	public class ParamClassLoadException : SystemException
	{
		public ParamClassLoadException(string s) : base("\n" + s)
		{
		}
	}
}