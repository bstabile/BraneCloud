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

namespace BraneCloud.Evolution.EC.GP.Build
{
    /// <summary> 
    /// RandomBranch implements the <tt>Random_Branch</tt> tree generation
    /// method described in 
    /// <p/> Chellapilla, K. 1998.  Evolving Modular Programs without Crossover.
    /// in <i>Proceedings of the Third Annual Genetic Programming Conference</i>
    /// (GP98), J.R. Koza <i>et al</i>, editors.  San Fransisco: Morgan Kaufmann.
    /// 23--31.
    /// 
    /// <p/> This algorithm attempts to create a tree of size <tt>requestedSize</tt>,
    /// or "slightly less".
    /// 
    /// If the pipeline does not specify a size it wants (it uses <tt>NOSIZEGIVEN</tt>),
    /// the algorithm picks a size at random from either [minSize...maxSize] or from
    /// sizeDistribution (one of the two <b>must</b> be defined), and attempts to create
    /// a tree of that size or "slightly less".
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.build.RandomBranch")]
    public class RandomBranch : GPNodeBuilder
    {
        #region Constants

        public const string P_RANDOMBRANCH = "random-branch";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return GPBuildDefaults.ParamBase.Push(P_RANDOMBRANCH); }
        }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            // we use size distributions -- did the user specify any?
            if (!CanPick())
                state.Output.Fatal("RandomBranch requires some kind of size distribution set, either with "
                    + P_MINSIZE + "/" + P_MAXSIZE + ", or with " + P_NUMSIZES + ".", paramBase, DefaultBase);
        }

        #endregion // Setup
        #region Operations

        public override GPNode NewRootedTree(IEvolutionState state, GPType type, int thread, IGPNodeParent parent,
                                                            GPFunctionSet funcs, int argPosition, int requestedSize)
        {
            if (requestedSize == NOSIZEGIVEN)
                // pick from the distribution
                return randomBranch(state, type, PickSize(state, thread), thread, parent, argPosition, funcs);
            if (requestedSize < 1)
                state.Output.Fatal("ec.gp.build.RandomBranch requested to build a tree, but a requested size was given that is < 1.");
            return randomBranch(state, type, requestedSize, thread, parent, argPosition, funcs);
        }

        private GPNode randomBranch(IEvolutionState state, GPType type, int maxLength, int thread, IGPNodeParent parent, int argPosition, GPFunctionSet funcs)
        {
            // randomBranch can mess up if there are no available terminals for a given type.  If this occurs,
            // and we find ourselves unable to pick a terminal when we want to do so, we will issue a warning,
            // and pick a nonterminal, violating the maximum-size contract.  This can lead to pathological situations
            // where the system will continue to go on and on unable to stop because it can't pick a terminal,
            // resulting in running out of memory or some such.  But there are cases where we'd want to let
            // this work itself out.
            var triedTerminals = false;

            var t = type.Type;
            var terminals = funcs.Terminals[t];
            var nonterminals = funcs.Nonterminals[t];
            var nodes = funcs.Nodes[t];

            if (nodes.Length == 0)
                ErrorAboutNoNodeWithType(type, state); // total failure

            // if the desired length is 1
            // OR if there are NO nonterminals!
            // [first set triedTerminals]
            // AND if there are available terminals
            if ((maxLength == 1 || WarnAboutNonterminal(nonterminals.Length == 0, type, false, state))
                                                    // this will freak out the static checkers
                                                    && (triedTerminals = true) 
                                                    && terminals.Length != 0)
            {
                var n = terminals[state.Random[thread].NextInt(terminals.Length)].LightClone();
                n.ResetNode(state, thread); // give ERCs a chance to randomize
                n.ArgPosition = (sbyte)argPosition;
                n.Parent = parent;
                return n;
            }

            if (triedTerminals)
                WarnAboutNoTerminalWithType(type, false, state); // we tried terminals and we're here because there were none!

            // grab all the nodes whose arity is <= maxlength-1
            var len = funcs.NonterminalsUnderArity[type.Type].Length - 1;
            if (len > maxLength - 1)
                len = maxLength - 1;
            var okayNonterms = funcs.NonterminalsUnderArity[type.Type][len];

            if (okayNonterms.Length == 0)
            // no nodes, pick a terminal
            {
                if (terminals.Length == 0)
                    ErrorAboutNoNodeWithType(type, state); // total failure

                var n = terminals[state.Random[thread].NextInt(terminals.Length)].LightClone();
                n.ResetNode(state, thread); // give ERCs a chance to randomize
                n.ArgPosition = (sbyte)argPosition;
                n.Parent = parent;
                return n;
            }
            // we've got nonterminals, pick one at random
            else
            {
                var n = okayNonterms[state.Random[thread].NextInt(okayNonterms.Length)].LightClone();
                n.ResetNode(state, thread); // give ERCs a chance to randomize
                n.ArgPosition = (sbyte)argPosition;
                n.Parent = parent;

                // Populate the node...
                var childtypes = n.Constraints((GPInitializer)state.Initializer).ChildTypes;
                for (var x = 0; x < childtypes.Length; x++)
                    n.Children[x] = randomBranch(state, childtypes[x], (maxLength - 1) / childtypes.Length, thread, n, x, funcs);
                return n;
            }
        }

        #endregion // Operations
    }
}