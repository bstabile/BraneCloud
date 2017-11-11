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
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Support;

namespace BraneCloud.Evolution.EC.GP.Breed
{
    /// <summary> 
    /// MutateAllNodesPipeline implements the AllNodes mutation algorithm described in Kumar Chellapilla,
    /// "A Preliminary Investigation into Evolving Modular Programs without Subtree Crossover", GP98.
    /// 
    /// <p/>MutateAllNodesPipeline chooses a subtree and for each node <i>n</i>
    /// in that subtree, it replaces <i>n</i> with a randomly-picked node of the same
    /// arity and type constraints.  Thus the original topological structure is
    /// the same but the nodes are different.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// ...as many as the source produces
    /// <p/><b>Number of Sources</b><br/>
    /// 1
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>ns</tt>.0<br/>
    /// <font size="-1">classname, inherits and != GPNodeSelector</font></td>
    /// <td valign="top">(GPNodeSelector for tree)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>tree.0</tt><br/>
    /// <font size="-1">0 &lt; int &lt; (num trees in individuals), if exists</font></td>
    /// <td valign="top">(tree chosen for mutation; if parameter doesn't exist, tree is picked at random)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// gp.breed.mutate-all-nodes
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"/><i>base</i>.<tt>ns</tt><br/>
    /// <td>The GPNodeSelector selector</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.breed.MutateAllNodesPipeline")]
    public class MutateAllNodesPipeline : GPBreedingPipeline
    {
        #region Constants

        private const long SerialVersionUID = 1;

        public const string P_MUTATEALLNODES = "mutate-all-nodes";
        public const int NUM_SOURCES = 1;
        public const string KEY_PARENTS = "parents";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase => GPBreedDefaults.ParamBase.Push(P_MUTATEALLNODES); 

        /// <summary>
        /// How the pipeline chooses a subtree to mutate 
        /// </summary>
        public IGPNodeSelector NodeSelect { get; set; }

        public override int NumSources => NUM_SOURCES; 

        /// <summary>
        /// Is our tree fixed?  If not, this is -1 
        /// </summary>
        public int Tree { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;

            var p = paramBase.Push(P_NODESELECTOR).Push("" + 0);
            NodeSelect = (IGPNodeSelector)(state.Parameters.GetInstanceForParameter(p, def.Push(P_NODESELECTOR).Push("" + 0), typeof(IGPNodeSelector)));
            NodeSelect.Setup(state, p);

            Tree = TREE_UNFIXED;
            if (state.Parameters.ParameterExists(paramBase.Push(P_TREE).Push("" + 0), def.Push(P_TREE).Push("" + 0)))
            {
                Tree = state.Parameters.GetInt(paramBase.Push(P_TREE).Push("" + 0), def.Push(P_TREE).Push("" + 0), 0);
                if (Tree == -1)
                    state.Output.Fatal("Tree fixed value, if defined, must be >= 0");
            }
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Returns a node which is swap-compatible with returntype, 
        /// and whose arguments are swap-compatible with the current children of original.  
        /// You need to clone this node. 
        /// </summary>
        private static GPNode PickCompatibleNode(GPNode original, GPFunctionSet funcs, IEvolutionState state, GPType returntype, int thread)
        {
            // an expensive procedure: we will linearly search for a valid node
            var numValidNodes = 0;

            var type = returntype.Type;
            var initializer = ((GPInitializer)state.Initializer);
            var len = original.Constraints(initializer).ChildTypes.Length;
            bool failed;

            if (initializer.NumAtomicTypes + initializer.NumSetTypes == 1)
                // easy
                numValidNodes = funcs.NodesByArity[type][len].Length;
            else
                for (var x = 0; x < funcs.NodesByArity[type][len].Length; x++)
                // ugh, the hard way -- nodes swap-compatible with type, and of arity len
                {
                    failed = funcs.NodesByArity[type][len][x]
                        .Constraints(initializer).ChildTypes
                        .Where((t, y) => !t.CompatibleWith(initializer, original.Children[y]
                                                                            .Constraints(initializer).ReturnType)).Any();

                    if (!failed)
                        numValidNodes++;
                }

            // we must have at least success -- the node itself.  Otherwise we're
            // in deep doo-doo.

            // now pick a random node number
            var nodenum = state.Random[thread].NextInt(numValidNodes);

            // find and return that node
            var prosnode = 0;

            if (numValidNodes == funcs.NodesByArity[type][len].Length)
                // easy
                return funcs.NodesByArity[type][len][nodenum];

            for (var x = 0; x < funcs.NodesByArity[type][len].Length; x++)
            // ugh, the hard way -- nodes swap-compatible with type, and of arity len
            {
                failed = funcs.NodesByArity[type][len][x]
                    .Constraints(initializer).ChildTypes
                    .Where((t, y) => !t.CompatibleWith(initializer, original.Children[y]
                                                                        .Constraints(initializer).ReturnType)).Any();

                if (failed) continue;
                if (prosnode == nodenum)
                    // got it!
                    return funcs.NodesByArity[type][len][x];
                prosnode++;
            }

            // should never be able to get here
            throw new ApplicationException(); // whoops!
        }

        /// <summary>
        /// Returns a brand-new tree which is swap-compatible with returntype, 
        /// created by making nodes "compatible" with the equivalent nodes in the tree rooted at original.  
        /// You need to set the parent and argumentposition of the root yourself.
        /// </summary>
        private static GPNode GenerateCompatibleTree(GPNode original, GPFunctionSet funcs, IEvolutionState state, GPType returntype, int thread)
        {
            // pick a new node and clone it
            var node = PickCompatibleNode(original, funcs, state, returntype, thread).LightClone();

            // reset it
            node.ResetNode(state, thread);

            // fill in its children
            var initializer = ((GPInitializer)state.Initializer);
            for (var x = 0; x < node.Children.Length; x++)
            {
                node.Children[x] = GenerateCompatibleTree(original.Children[x], funcs, state, original.Constraints(initializer).ChildTypes[x], thread);
                node.Children[x].Parent = node;
                node.Children[x].ArgPosition = (sbyte)x;
            }
            return node;
        }

        public override int Produce(
            int min, 
            int max, 
            int subpop, 
            IList<Individual> inds, 
            IEvolutionState state, 
            int thread,
            IDictionary<string, object> misc)
        {
            int start = inds.Count;

            // grab n individuals from our source and stick 'em right into inds.
            // we'll modify them from there
            var n = Sources[0].Produce(min, max,subpop, inds, state, thread, misc);

            // should we bother?
            if (!state.Random[thread].NextBoolean(Likelihood))
            {
                return n;
            }

            IntBag[] parentparents = null;
            IntBag[] preserveParents = null;
            if (misc != null && misc.ContainsKey(KEY_PARENTS))
            {
                preserveParents = (IntBag[])misc[KEY_PARENTS];
                parentparents = new IntBag[2];
                misc[KEY_PARENTS] = parentparents;
            }

            var initializer = ((GPInitializer)state.Initializer);

            // now let's mutate 'em
            for (var q = start; q < n + start; q++)
            {
                var i = (GPIndividual)inds[q];

                if (Tree != TREE_UNFIXED && (Tree < 0 || Tree >= i.Trees.Length))
                    // uh oh
                    state.Output.Fatal("MutateAllNodesPipeline attempted to fix Tree.0 to a value which was out of bounds of the array of the individual's Trees.  Check the pipeline's fixed tree values -- they may be negative or greater than the number of trees in an individual");

                int t;
                // pick random tree
                if (Tree == TREE_UNFIXED)
                    t = i.Trees.Length > 1 ? state.Random[thread].NextInt(i.Trees.Length) : 0;
                else
                    t = Tree;

                // prepare the NodeSelector
                NodeSelect.Reset();

                // pick a node

                // pick a node in individual 1
                var p1 = NodeSelect.PickNode(state, subpop, thread, i, i.Trees[t]);

                // generate a tree with a new root but the same children,
                // which we will replace p1 with

                var type = p1.ParentType(initializer);

                var p2 = GenerateCompatibleTree(p1, i.Trees[t].Constraints(initializer).FunctionSet, state, type, thread);
                // we'll need to set p2.ArgPosition and p2.Parent further down


                    p2.Parent = p1.Parent;
                    p2.ArgPosition = p1.ArgPosition;
                    if (p2.Parent is GPNode)
                        ((GPNode)(p2.Parent)).Children[p2.ArgPosition] = p2;
                    else
                        ((GPTree)(p2.Parent)).Child = p2;
                    i.Evaluated = false; // we've modified it

                // add the new individual, replacing its previous source
                inds[q] = i;
                if (preserveParents != null)
                {
                    parentparents[0].AddAll(parentparents[1]);
                    preserveParents[q] = new IntBag(parentparents[0]);
                }
            }
            return n;
        }

        #endregion // Operations
        #region Cloning

        public override object Clone()
        {
            var c = (MutateAllNodesPipeline) (base.Clone());
            
            // deep-cloned stuff
            c.NodeSelect = (IGPNodeSelector) (NodeSelect.Clone());
            return c;
        }

        #endregion // Cloning
    }
}