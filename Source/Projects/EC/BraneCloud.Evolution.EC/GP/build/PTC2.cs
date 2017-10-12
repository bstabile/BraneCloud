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
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.GP.Build
{
    /// <summary> 
    /// PTC2 implements the "Strongly-typed Probabilistic Tree Creation 2 (PTC2)" algorithm described in 
    /// 
    /// <p/>Luke, Sean. 2000. <i>Issues in Scaling Genetic Programming: Breeding Strategies, Tree Generation, and Code Bloat.</i> Ph.D. Dissertation, Department of Computer Science, University of Maryland, College Park, Maryland. 
    /// 
    /// <p/> ...and also in
    /// 
    /// <p/>Luke, Sean. 2000. Two fast tree-creation algorithms for genetic programming. In <i>IEEE Transactions on Evolutionary Computation</i> 4:3 (September 2000), 274-283. IEEE. 
    /// 
    /// <p/> Both can be found at <a href="http://www.cs.gmu.edu/~sean/papers/">http://www.cs.gmu.edu/~sean/papers/</a>
    /// 
    /// <p/> PTC2 requires that your function set to implement IPTCFunctionSet.  The
    /// provided function set, PTCFunctionSet, does exactly this.
    /// 
    /// <p/>The Strongly-typed PTC2 algorithm roughly works as follows: 
    /// the user provides a requested tree size, and PTC2 attempts to build
    /// a tree of that size or that size plus the maximum arity of a nonterminal
    /// in the function set.  PTC2 works roughly like this:
    /// 
    /// <ol><li/>If the tree size requested is 1, pick a random terminal and return it.
    /// <li/> Else pick a random nonterminal as the root and put each of its unfilled child positions into the queue <i>Q</i>.
    /// <li/> Loop until the size of <i>Q</i>, plus the size of the nodes in the tree so far, equals or exceeds the requested tree size:
    /// <ol><li/>Remove a random position from <i>Q</i>.
    /// <li/>Fill the position with a random nonterminal <i>n</i>.
    /// <li/>Put each of <i>n's</i> unfilled child positions into <i>Q</i>.
    /// </ol>
    /// <li/>For each position in <i>Q</i>, fill the position with a randomly-chosen terminal.
    /// </ol>
    /// 
    /// <p/> Generally speaking, PTC2 picks a random position in the horizon of the tree (unfiled child node positions), fills it with a nonterminal, thus extending the horizon, and repeats this until the number of nodes (nonterminals) in the tree, plus the number of unfilled node positions, is >= the requested tree size.  Then the remaining horizon is filled with terminals.
    /// 
    /// <p/> The user-provided requested tree size is either provided directly to the PTC2 algorithm, or if the size is NOSIZEGIVEN, then PTC2 will pick one at random from the GPNodeBuilder probability distribution system (using either max-depth and min-depth, or using num-sizes).
    /// 
    /// <p/> PTC2 also has provisions for picking nonterminals with a certain probability over other nonterminals of the same return type (and terminals over other terminals likewise), hence its name.  To change the probability of picking various terminals or nonterminals, you modify your IPTCFunctionSet function set.
    /// 
    /// <p/>PTC2 further has a maximum depth, which you should set to some fairly big value.  If your maximum depth is small enough that PTC2 often creates trees which bump up against it, then PTC2 will only generate terminals at that depth position.  If the depth is *really* small, it's possible that this means PTC2 will generate trees smaller than you had requested.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>max-depth</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">maximum allowable tree depth (usually a big value)</td></tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.build.PTC2")]
    public class PTC2 : GPNodeBuilder
    {
        #region Constants

        public const string P_PTC2 = "ptc2";
        public const string P_MAXDEPTH = "max-depth";

        public const int MIN_QUEUE_SIZE = 32;

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return GPBuildDefaults.ParamBase.Push(P_PTC2); }
        }

        /// <summary>
        /// The largest maximum tree depth GROW can specify -- should be big. 
        /// </summary>
        public int MaxDepth { get; set; }

        // these are all initialized in enqueue
        public GPNode[] s_node { get; set; }
        public int[] s_argpos { get; set; }
        public int[] s_depth { get; set; }
        public int s_size { get; set; }

        public GPNode DequeueNode { get; set; }
        public int DequeueArgpos { get; set; }
        public int DequeueDepth { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;

            // we use size distributions -- did the user specify any?
            if (!CanPick())
                state.Output.Fatal("PTC2 needs a distribution of tree sizes to pick from.  You can do this by either setting a distribution (with "
                                    + P_NUMSIZES + ") or with " + P_MINSIZE + " and " + P_MAXSIZE + ".", paramBase, def);

            MaxDepth = state.Parameters.GetInt(paramBase.Push(P_MAXDEPTH), def.Push(P_MAXDEPTH), 1);
            if (MaxDepth < 1)
                state.Output.Fatal("Maximum depth must be >= 1", paramBase.Push(P_MAXDEPTH), def.Push(P_MAXDEPTH));
        }

        #endregion // Setup
        #region Operations

        private void Enqueue(GPNode n, int argpos, int depth)
        {
            if (s_node == null)
            {
                s_node = new GPNode[MIN_QUEUE_SIZE];
                s_argpos = new int[MIN_QUEUE_SIZE];
                s_depth = new int[MIN_QUEUE_SIZE];
                s_size = 0;
            }
            else if (s_size == s_node.Length)
            // need to double them
            {
                var newSNode = new GPNode[s_size * 2];
                Array.Copy(s_node, 0, newSNode, 0, s_size);
                s_node = newSNode;

                var newSArgpos = new int[s_size * 2];
                Array.Copy(s_argpos, 0, newSArgpos, 0, s_size);
                s_argpos = newSArgpos;

                var newSDepth = new int[s_size * 2];
                Array.Copy(s_depth, 0, newSDepth, 0, s_size);
                s_depth = newSDepth;
            }

            // okay, let's boogie!
            s_node[s_size] = n;
            s_argpos[s_size] = argpos;
            s_depth[s_size] = depth;
            s_size++;
        }

        /// <summary>
        /// stashes in dequeue_*
        /// </summary>
        /// <param name="state"></param>
        /// <param name="thread"></param>
        private void RandomDequeue(IEvolutionState state, int thread)
        {
            var r = state.Random[thread].NextInt(s_size);
            s_size -= 1;
            // put items r into spot dequeue_*
            DequeueNode = s_node[r];
            DequeueArgpos = s_argpos[r];
            DequeueDepth = s_depth[r];
            // put items s_size into spot r
            s_node[r] = s_node[s_size];
            s_argpos[r] = s_argpos[s_size];
            s_depth[r] = s_depth[s_size];
        }

        public override GPNode NewRootedTree(IEvolutionState state, GPType type, int thread,
                        IGPNodeParent parent, GPFunctionSet funcs, int argPosition, int requestedSize)
        {
            // ptc2 can mess up if there are no available terminals for a given type.  If this occurs,
            // and we find ourselves unable to pick a terminal when we want to do so, we will issue a warning,
            // and pick a nonterminal, violating the ptc2 size and depth contracts.  This can lead to pathological situations
            // where the system will continue to go on and on unable to stop because it can't pick a terminal,
            // resulting in running out of memory or some such.  But there are cases where we'd want to let
            // this work itself out.
            var triedTerminals = false;

            if (!(funcs is IPTCFunctionSet))
                state.Output.Fatal("Set " + funcs.Name
                    + " is not of the class ec.gp.build.IPTCFunctionSet, and so cannot be used with PTC Nodebuilders.");

            var pfuncs = (IPTCFunctionSet)funcs;

            // pick a size from the distribution
            if (requestedSize == NOSIZEGIVEN)
                requestedSize = PickSize(state, thread);

            GPNode root;

            var t = type.Type;
            var terminals = funcs.Terminals[t];
            var nonterminals = funcs.Nonterminals[t];
            var nodes = funcs.Nodes[t];

            if (nodes.Length == 0)
                ErrorAboutNoNodeWithType(type, state); // total failure



            // return a terminal
            // Now pick a terminal if our size is 1
            // OR if there are NO nonterminals!
            // [first set triedTerminals]
            // AND if there are available terminals
            if ((requestedSize == 1 || WarnAboutNonterminal(nonterminals.Length == 0, type, false, state))
                                    && (triedTerminals = true) && terminals.Length != 0)
            {
                root = terminals[RandomChoice.PickFromDistribution(pfuncs.TerminalProbabilities(t),
                                    state.Random[thread].NextFloat())].LightClone();

                root.ResetNode(state, thread); // give ERCs a chance to randomize
                root.ArgPosition = (sbyte)argPosition;
                root.Parent = parent;
            }
            // return a nonterminal-rooted tree
            else
            {
                if (triedTerminals)
                    WarnAboutNoTerminalWithType(type, false, state); // we tried terminals and we're here because there were none!

                // pick a nonterminal
                root = nonterminals[RandomChoice.PickFromDistribution(pfuncs.NonterminalProbabilities(t),
                                           state.Random[thread].NextFloat())].LightClone();

                root.ResetNode(state, thread); // give ERCs a chance to randomize
                root.ArgPosition = (sbyte)argPosition;
                root.Parent = parent;

                // set the depth, size, and enqueuing, and reset the random dequeue

                s_size = 0; // pretty critical!
                var s = 1;
                var initializer = ((GPInitializer)state.Initializer);
                var childtypes = root.Constraints(initializer).ChildTypes;

                for (var x = 0; x < childtypes.Length; x++)
                    Enqueue(root, x, 1); /* depth 1 */

                while (s_size > 0)
                {
                    triedTerminals = false;
                    RandomDequeue(state, thread);
                    type = DequeueNode.Constraints(initializer).ChildTypes[DequeueArgpos];

                    var y = type.Type;
                    terminals = funcs.Terminals[y];
                    nonterminals = funcs.Nonterminals[y];
                    nodes = funcs.Nodes[y];

                    if (nodes.Length == 0)
                        ErrorAboutNoNodeWithType(type, state); // total failure

                    // pick a terminal 
                    // if we need no nonterminal nodes
                    // OR if we're at max depth and must pick a terminal
                    // OR if there are NO nonterminals!
                    // [first set triedTerminals]
                    // AND if there are available terminals
                    if ((s_size + s >= requestedSize || DequeueDepth == MaxDepth
                                                     || WarnAboutNonterminal(nonterminals.Length == 0, type, false, state))
                                                     && (triedTerminals = true) && terminals.Length != 0)
                    {
                        var n = terminals[RandomChoice.PickFromDistribution(pfuncs.TerminalProbabilities(y),
                                            state.Random[thread].NextFloat())].LightClone();

                        DequeueNode.Children[DequeueArgpos] = n;
                        n.ResetNode(state, thread); // give ERCs a chance to randomize
                        n.ArgPosition = (sbyte)DequeueArgpos;
                        n.Parent = DequeueNode;
                    }
                    // pick a nonterminal and enqueue its children
                    else
                    {
                        if (triedTerminals)
                            WarnAboutNoTerminalWithType(type, false, state); // we tried terminals and we're here because there were none!

                        var n = nonterminals[RandomChoice.PickFromDistribution(pfuncs.NonterminalProbabilities(y),
                                                    state.Random[thread].NextFloat())].LightClone();

                        DequeueNode.Children[DequeueArgpos] = n;
                        n.ResetNode(state, thread); // give ERCs a chance to randomize
                        n.ArgPosition = (sbyte)DequeueArgpos;
                        n.Parent = DequeueNode;

                        childtypes = n.Constraints(initializer).ChildTypes;
                        for (var x = 0; x < childtypes.Length; x++)
                            Enqueue(n, x, DequeueDepth + 1);
                    }
                    s++;
                }
            }

            return root;
        }

        #endregion // Operations
    }
}