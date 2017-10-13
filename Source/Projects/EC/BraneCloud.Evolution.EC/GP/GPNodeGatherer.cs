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
    /// GPNodeGatherer is a small container object for the GPNode.NodeInPosition(...)
    /// method and GPNode.NumNodes(...) method.  It may be safely reused without being reinitialized.
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.GPNodeGatherer")]
    public class GPNodeGatherer
    {
        #region Properties

        public GPNode Node { get; set; }

        #endregion // Properties
        #region Operations

        /// <summary>
        /// Returns true if thisNode is the kind of node to be considered in the
        /// gather count for nodeInPosition(...) and GPNode.NumNodes(GPNodeGatherer).
        /// The default form simply returns true.  
        /// </summary>
        public virtual bool Test(GPNode thisNode)
        {
            return true;
        }

        #endregion // Operations
    }
}