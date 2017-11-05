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

namespace BraneCloud.Evolution.EC.GP.GE
{
    /// <summary>
    /// A GrammarNode representing a Rule in the GE Grammar.  The head of the GrammarRuleNode
    /// is the name of the rule; and the children are the various choices.  These are returned
    /// by getChoice(...) and getNumChoices().  The merge(...) method unifies this GrammarRuleNode
    /// with the choices of another node.
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.ge.GrammarRuleNode")]
    public class GrammarRuleNode : GrammarNode
    {
        #region Setup

        public GrammarRuleNode(String head)
            : base(head)
        {
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Adds a choice to the children of this node.
        /// </summary>
        public void AddChoice(GrammarNode choice)
        {
            Children.Add(choice);
        }

        /// <summary>
        /// Returns the current number of choices to the node.
        /// </summary>
        public int GetNumChoices() { return Children.Count; }

        /// <summary>
        /// Returns a given choice.
        /// </summary>
        public GrammarNode GetChoice(int index) { return Children[index]; }

        /// <summary>
        /// Adds to this node all the choices of another node.
        /// </summary>
        public void Merge(GrammarRuleNode other)
        {
            var n = other.GetNumChoices();
            for (var i = 0; i < n; i++)
                AddChoice(other.GetChoice(i));
        }

        #endregion // Operations
        #region ToString

        public override string ToString()
        {
            var ret = "" + Head + " ::= ";
            var i = Children.GetEnumerator();
            var first = true;
            while (i.MoveNext())
            {
                ret = ret + (first ? "" : "| ") + i.Current.Head;
                first = false;
            }
            return ret;
        }

        #endregion // ToString
    }
}