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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.GP.GE
{
    /// <summary>
    /// The abstract superclass of nodes used by GrammarParser to construct a parse graph to generate
    /// GEIndividuals.  GrammarNode has a *head*, which typically holds the name of the node,
    /// and an array of *children*, which are themselves GrammarNodes.
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.ge.GrammarNode")]
    public abstract class GrammarNode
    {
        /// <summary>
        /// May be empty but it's not very expensive
        /// </summary>
        public IList<GrammarNode> Children { get; set; } = new List<GrammarNode>();

        public GrammarNode(string head)
        {
            Head = head;
        }

        public string Head
        {
            get; 
            protected set; 
        }
    }
}