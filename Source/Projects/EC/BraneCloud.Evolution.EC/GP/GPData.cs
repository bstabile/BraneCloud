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
    /// <p/><b>Default Base</b><br/>
    /// gp.Data
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.GPData")]
    public abstract class GPData : IPrototype
    {
        #region Constants

        public const string P_GPDATA = "data";

        #endregion // Constants
        #region Properties

        public virtual IParameter DefaultBase
        {
            get { return GPDefaults.ParamBase.Push(P_GPDATA); }
        }

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
        /// </summary>
        public abstract void CopyTo(GPData gpd);

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