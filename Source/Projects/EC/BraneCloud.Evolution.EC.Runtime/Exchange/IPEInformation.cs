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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BraneCloud.Evolution.EC.Runtime.Exchange
{
    /// <summary>
    /// This is a data container used for Interpopulation Exchange.
    /// </summary>
    [Serializable]
    public class IPEInformation
    {
        #region Properties

        /// <summary>
        /// The selection method for Immigrants.
        /// </summary>
        public SelectionMethod ImmigrantsSelectionMethod { get; set; }

        /// <summary>
        /// The selection method for Individuals to die.
        /// </summary>
        public SelectionMethod IndsToDieSelectionMethod { get; set; }

        /// <summary>
        /// The number of destination subpops.
        /// </summary>
        public int NumDest { get; set; }

        /// <summary>
        /// The subpops where individuals need to be sent.
        /// </summary>
        public int[] Destinations { get; set; }

        /// <summary>
        /// The modulo
        /// </summary>
        public int Modulo { get; set; }

        /// <summary>
        /// The start (offset).
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// The size.
        /// </summary>
        public int Size { get; set; }
        
        #endregion // Properties
    }

}