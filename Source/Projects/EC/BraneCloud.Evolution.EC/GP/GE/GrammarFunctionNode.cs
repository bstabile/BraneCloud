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
    /// A GrammarNode representing a GPNode in the GE Grammar.  The head of the GrammarFunctionNode
    /// is the name of the GPNode in the grammar; and the children are various arguments to the node
    /// as defined by the grammar.  These are returned  by getArgument(...) and getNumArguments().
    /// The GrammarFunctionNode holds a prototypical GPNode from which clones can be made.
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.ge.GrammarFunctionNode")]
    public class GrammarFunctionNode : GrammarNode
    {
        #region Fields

        readonly GPNode _prototype;

        #endregion // Fields
        #region Setup

        /// <summary>
        /// Determines the GPNode from the function set by the name.  If there is more than
        /// one such node (which shouldn't be the case) then only the first such node is
        /// used.  Stores the prototype. 
        /// </summary>
        public GrammarFunctionNode(GPFunctionSet gpfs, String name)
            : base(name)
        {
            _prototype = ((GPNode[])gpfs.NodesByName[name])[0];
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Adds a given argument to the node.
        /// </summary>
        public void AddArgument(GrammarNode arg)
        {
            Children.Add(arg);
        }

        /// <summary>
        /// Returns the number of arguments.
        /// </summary>
        public int GetNumArguments()
        {
            return Children.Count;
        }

        /// <summary>
        /// Return a given argument.
        /// </summary>
        public GrammarNode GetArgument(int index) { return Children[index]; }

        /// <summary>
        /// Returns the prototype without cloning it first.  Be certain to clone before using.
        /// </summary>
        /// <returns></returns>
        public GPNode GetGPNodePrototype() { return _prototype; }

        #endregion // Operations
        #region ToString

        public override string ToString()
        {
            var ret = "(" + Head + " ";
            var i = Children.GetEnumerator();
            var first = true;
            while (i.MoveNext())
            {
                ret = ret + (first ? "" : " ") + i.Current.Head;
                first = false;
            }
            return ret + ")";
        }

        #endregion // ToString
    }
}