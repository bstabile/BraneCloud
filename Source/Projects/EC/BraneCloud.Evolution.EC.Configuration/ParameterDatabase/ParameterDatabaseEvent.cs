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
	
	/// <author>  spaus
	/// </author>
	[Serializable]
	public class ParameterDatabaseEvent : EventArgs
	{
		/// <returns> the Parameter associated with the event
		/// </returns>
		virtual public IParameter Parameter
		{
			get { return _parameter; }
		}
		/// <returns> the value of the Parameter associated with the event.
		/// </returns>
		virtual public string Value
		{
			get { return _valueRenamed; }
		}
		/// <returns> the type of the event.
		/// </returns>
		virtual public int Type
		{
			get { return _type; }
		}
		
		public const int SET = 0;
		public const int ACCESSED = 1;
		
		private IParameter _parameter;
		private string _valueRenamed;
		private int _type;
		
		/// <summary> 
		/// For ParameterDatabase events.
		/// </summary>
		/// <param name="source">the ParameterDatabase </param>
		/// <param name="parameter">the Parameter associated with the event</param>
        /// <param name="paramValue">the value of the Parameter associated with the event</param>
		/// <param name="type">the type of the event</param>
		public ParameterDatabaseEvent(object source, IParameter parameter, string paramValue, int type)
		{
			_parameter = parameter;
            _valueRenamed = paramValue;
			_type = type;
		}
	}
}