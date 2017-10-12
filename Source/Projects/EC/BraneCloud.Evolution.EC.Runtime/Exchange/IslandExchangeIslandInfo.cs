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
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Runtime.Exchange
{
    /// <summary>
    /// A class indicating all the information the server knows about
    /// a given island, including its mod, size, Offset, and all the
    /// migrating islands it hooks to, etc. 
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.exchange.IslandExchangeIslandInfo")]
    public class IslandExchangeIslandInfo
    {
        #region Properties

        /// <summary>
        /// How often to send individuals. 
        /// </summary>
        public int Modulo { get; set; }

        /// <summary>
        /// The Mailbox capacity (for each of the subpops). 
        /// </summary>
        public int MailboxCapacity { get; set; }

        /// <summary>
        /// What generation to start sending individuals. 
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// How many individuals to send.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// To how many islands to send individuals.
        /// </summary>
        public int NumMig { get; set; }

        /// <summary>
        /// The ids of the contries to send individuals to.
        /// </summary>
        public string[] MigratingIslandIds { get; set; }

        /// <summary>
        /// How many islands will send individuals to the Mailbox.
        /// </summary>
        public int NumIncoming { get; set; }

        // also later filled in:

        /// <summary>
        /// The address of the Mailbox where to receive information.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The port of the Mailbox.
        /// </summary>
        public int Port { get; set; }

        #endregion // Properties
    }
}