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

namespace BraneCloud.Evolution.EC.GP.Koza
{
    /// <summary>
    /// KozaBuilder is an abstract superclass of three tree builders: GROW, FULL, and RAMPED HALF-AND-HALF,
    /// all described in I/II.  As all three classes specify a minimum and maximum depth, these instance
    /// variables and Setup methods appear here; but they are described in detail in the relevant subclasses
    /// (GrowBuilder, HalfBuilder, and FullBuilder).
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>min-depth</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(smallest "maximum" depth the builder may use for building a tree.  2 is the default.)</td></tr>
    /// 
    /// <tr><td valign="top"><i>base</i>.<tt>max-depth</tt><br/>
    /// <font size="-1">int &gt;= <i>base</i>.<tt>min-depth</tt></font></td>
    /// <td valign="top">(largest "maximum" depth the builder may use for building a tree. 6 is the default.)</td></tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.koza.KozaBuilder")]
    public abstract class KozaBuilder : GPNodeBuilder
    {
        #region Constants

        public const string P_MAXDEPTH = "max-depth";
        public const string P_MINDEPTH = "min-depth";

        #endregion // Constants
        #region Properties

        /// <summary>
        /// The largest maximum tree depth RAMPED HALF-AND-HALF can specify. 
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// The smallest maximum tree depth RAMPED HALF-AND-HALF can specify. 
        /// </summary>
        public int MinDepth { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            
            var def = DefaultBase;
            
            // load maxdepth and mindepth, check that maxdepth>0, mindepth>0, maxdepth>=mindepth
            MaxDepth = state.Parameters.GetInt(paramBase.Push(P_MAXDEPTH), def.Push(P_MAXDEPTH), 1);
            if (MaxDepth <= 0)
                state.Output.Fatal("The Max Depth for a KozaBuilder must be at least 1.", paramBase.Push(P_MAXDEPTH), def.Push(P_MAXDEPTH));
            
            MinDepth = state.Parameters.GetInt(paramBase.Push(P_MINDEPTH), def.Push(P_MINDEPTH), 1);
            if (MinDepth <= 0)
                state.Output.Fatal("The Min Depth for a KozaBuilder must be at least 1.", paramBase.Push(P_MINDEPTH), def.Push(P_MINDEPTH));
            
            if (MaxDepth < MinDepth)
                state.Output.Fatal("Max Depth must be >= Min Depth for a KozaBuilder", paramBase.Push(P_MAXDEPTH), def.Push(P_MAXDEPTH));
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// A private recursive method which builds a FULL-style tree for NewRootedTree(...) 
        /// </summary>
        protected internal virtual GPNode FullNode(IEvolutionState state, int current, int max, GPType type, int thread,
                                                                IGPNodeParent parent, int argPosition, GPFunctionSet funcs)
        {
            // fullNode can mess up if there are no available terminals for a given type.  If this occurs,
            // and we find ourselves unable to pick a terminal when we want to do so, we will issue a warning,
            // and pick a nonterminal, violating the "FULL" contract.  This can lead to pathological situations
            // where the system will continue to go on and on unable to stop because it can't pick a terminal,
            // resulting in running out of memory or some such.  But there are cases where we'd want to let
            // this work itself out.
            var triedTerminals = false; // did we try -- and fail -- to fetch a terminal?

            var t = type.Type;
            var terminals = funcs.Terminals[t];
            var nonterminals = funcs.Nonterminals[t];
            var nodes = funcs.Nodes[t];

            if (nodes.Length == 0)
                ErrorAboutNoNodeWithType(type, state); // total failure

            // pick a terminal when we're at max depth or if there are NO nonterminals
            if ((current + 1 >= max || WarnAboutNonterminal(nonterminals.Length == 0, type, false, state))
                                                    // this will freak out the static checkers
                                                    && (triedTerminals = true) // [first set triedTerminals]
                                                    && terminals.Length != 0)  // AND if there are available terminals
            {
                var n = terminals[state.Random[thread].NextInt(terminals.Length)].LightClone();
                n.ResetNode(state, thread); // give ERCs a chance to randomize
                n.ArgPosition = (sbyte)argPosition;
                n.Parent = parent;
                return n;
            }
            // else force a nonterminal unless we have no choice
            else
            {
                if (triedTerminals)
                    WarnAboutNoTerminalWithType(type, false, state); // we tried terminals and we're here because there were none!

                var nodesToPick = funcs.Nonterminals[type.Type];
                if (nodesToPick == null || nodesToPick.Length == 0)
                    // no nonterminals, hope the guy knows what he's doing!
                    nodesToPick = funcs.Terminals[type.Type]; // this can only happen with the warning about nonterminals above

                var n = nodesToPick[state.Random[thread].NextInt(nodesToPick.Length)].LightClone();
                n.ResetNode(state, thread); // give ERCs a chance to randomize
                n.ArgPosition = (sbyte)argPosition;
                n.Parent = parent;

                // Populate the node...
                var childtypes = n.Constraints(((GPInitializer)state.Initializer)).ChildTypes;
                for (var x = 0; x < childtypes.Length; x++)
                    n.Children[x] = FullNode(state, current + 1, max, childtypes[x], thread, n, x, funcs);

                return n;
            }
        }

        /// <summary>
        /// A private function which recursively returns a GROW tree to NewRootedTree(...) 
        /// </summary>
        protected internal virtual GPNode GrowNode(IEvolutionState state, int current, int max, GPType type, int thread,
                                                                IGPNodeParent parent, int argPosition, GPFunctionSet funcs)
        {
            // growNode can mess up if there are no available terminals for a given type.  If this occurs,
            // and we find ourselves unable to pick a terminal when we want to do so, we will issue a warning,
            // and pick a nonterminal, violating the maximum-depth contract.  This can lead to pathological situations
            // where the system will continue to go on and on unable to stop because it can't pick a terminal,
            // resulting in running out of memory or some such.  But there are cases where we'd want to let
            // this work itself out.
            var triedTerminals = false;

            var t = type.Type;
            GPNode[] terminals = funcs.Terminals[t];
            //GPNode[] nonterminals = funcs.Nonterminals[t];
            GPNode[] nodes = funcs.Nodes[t];

            if (nodes.Length == 0)
                ErrorAboutNoNodeWithType(type, state); // total failure

            // pick a terminal when we're at max depth or if there are NO nonterminals
            if (current + 1 >= max
                // this will freak out the static checkers
                && (triedTerminals = true) // [first set triedTerminals]
                && terminals.Length != 0)  // AND if there are available terminals
            {
                var n = terminals[state.Random[thread].NextInt(terminals.Length)].LightClone();
                n.ResetNode(state, thread); // give ERCs a chance to randomize
                n.ArgPosition = (sbyte)argPosition;
                n.Parent = parent;
                return n;
            }
            // else pick a random node
            else
            {
                if (triedTerminals)
                    WarnAboutNoTerminalWithType(type, false, state); // we tried terminals and we're here because there were none!

                var n = nodes[state.Random[thread].NextInt(nodes.Length)].LightClone();
                n.ResetNode(state, thread); // give ERCs a chance to randomize
                n.ArgPosition = (sbyte)argPosition;
                n.Parent = parent;

                // Populate the node...
                var childtypes = n.Constraints(((GPInitializer)state.Initializer)).ChildTypes;
                for (var x = 0; x < childtypes.Length; x++)
                    n.Children[x] = GrowNode(state, current + 1, max, childtypes[x], thread, n, x, funcs);

                return n;
            }
        }
        
        #endregion // Operations
    }
}