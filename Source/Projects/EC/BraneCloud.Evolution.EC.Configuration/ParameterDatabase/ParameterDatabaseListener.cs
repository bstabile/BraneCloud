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

namespace BraneCloud.Evolution.EC.Configuration
{
    
    /// <author>  spaus
    /// </author>
    public delegate void  ParameterDatabaseListenerDelegate(object sender, ParameterDatabaseEvent ParameterDatabaseListenerDelegateParam);
    // BRS : 2009-03-15 : (EventHandler?)
    public interface ParameterDatabaseListener // :EventListener
    {
        /// <param name="evt">
        /// </param>
        void  ParameterSet(object event_sender, ParameterDatabaseEvent evt);
        
        /// <param name="evt">
        /// </param>
        void  ParameterAccessed(object event_sender, ParameterDatabaseEvent evt);
    }
}