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

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC
{
    /// <summary> 
    /// The Initializer is a singleton object whose job is to initialize the
    /// population at the beginning of the run.  It does this by providing
    /// a population through the initialPopulation(...) method.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><tt>pop</tt><br/>
    /// <font size="-1">classname, inherits or = ec.Population</font></td>
    /// <td valign="top">(the class for a new population)</td></tr>
    /// </table>
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><tt>pop</tt></td>
    /// <td>The base for a new population's set up parameters</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.Initializer")]
    public abstract class Initializer : ISingleton
    {
        #region Constants

        /// <summary>
        /// Parameter for a new population. 
        /// </summary>
        public const string P_POP = "pop";

        #endregion // Constants
        #region Setup

        public abstract void Setup(IEvolutionState param1, IParameter param2);

        /// <summary>
        /// Creates and returns a new initial population for the evolutionary run.
        /// This is commonly done by creating a Population, setting it up (call
        /// Setup() on it!), and calling its Populate() method. This method
        /// will likely only be called once in a run. 
        /// </summary>
        public abstract Population InitialPopulation(IEvolutionState state, int thread);

        public abstract Population SetupPopulation(IEvolutionState state, int thread);

        #endregion // Setup
    }
}