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

namespace BraneCloud.Evolution.EC.GP
{
    /// <summary> 
    /// GPData is the parent class of data transferred between GPNodes.
    /// If performed correctly, there need be only one GPData instance 
    /// ever created in the evaluation of many individuals. 
    ///
    /// <p/>You can use GPData as-is if you have absolutely no data to
    /// transfer between individuals.Otherwise, you need to subclas
    /// GPData, add your own instance variables, and then override
    /// the copyTo(...) method and, depending on whether the data has
    /// pointers in it(like arrays), the clone() method as well.
    ///
    /// <p/><b>Default Base</b><br/>
    /// gp.Data
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.GPData")]
    public class GPData : IPrototype
    {
        #region Constants

        public const string P_GPDATA = "data";

        #endregion // Constants
        #region Properties

        public virtual IParameter DefaultBase => GPDefaults.ParamBase.Push(P_GPDATA); 
        

        #endregion // Properties
        #region Setup

        public virtual void  Setup(IEvolutionState state, IParameter paramBase)
        {
        }

        #endregion // Setup
        #region Cloning and Copying

        /// <summary>
        /// Modifies gpd so that gpd is equivalent to us. You may
        /// safely assume that gpd is of the same class as we are. 
        /// Do not share references with the other object, except to
        /// read-only data: instead, copy any read-write data as necessary.
        /// </summary>
        public virtual void CopyTo(GPData gpd) { }

        public virtual object Clone()
        {
            try
            {
                return MemberwiseClone();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Cloning Error!", ex);
            } // never happens
        }

        #endregion // Cloning and Copying
    }
}