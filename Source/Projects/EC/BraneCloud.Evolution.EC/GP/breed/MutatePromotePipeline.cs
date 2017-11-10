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

namespace BraneCloud.Evolution.EC.GP.Breed
{
    /// <summary> 
    /// MutatePromotePipeline works very similarly to the PromoteNode algorithm described in  Kumar Chellapilla,
    /// "A Preliminary Investigation into Evolving Modular Programs without Subtree Crossover", GP98, 
    /// and is also similar to the "deletion" operator found in Una-May O'Reilly's thesis,
    /// <a href="http://www.ai.mit.edu/people/unamay/thesis.html"> "An Analysis of Genetic Programming"</a>.
    /// 
    /// <p/>MutatePromotePipeline tries <i>tries</i> times to find a tree
    /// that has at least one promotable node.  It then picks randomly from
    /// all the promotable nodes in the tree, and promotes one.  If it cannot
    /// find a valid tree in <i>tries</i> times, it gives up and simply
    /// copies the individual.
    /// 
    /// <p/>"Promotion" means to take a node <i>n</i> whose parent is <i>m</i>,
    /// and replacing the subtree rooted at <i>m</i> with the subtree rooted at <i>n</i>.
    /// 
    /// <p/>A "Promotable" node means a node which is capable of promotion
    /// given the existing type constraints.  In general to promote a node <i>foo</i>,
    /// <i>foo</i> must have a parent node, and must be type-compatible with the
    /// child slot that its parent fills.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// ...as many as the source produces
    /// <p/><b>Number of Sources</b><br/>
    /// 1
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>tries</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(number of times to try finding valid pairs of nodes)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>tree.0</tt><br/>
    /// <font size="-1">0 &lt; int &lt; (num trees in individuals), if exists</font></td>
    /// <td valign="top">(tree chosen for mutation; if parameter doesn't exist, tree is picked at random)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// gp.breed.mutate-demote
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.breed.MutatePromotePipeline")]
    public class MutatePromotePipeline : GPBreedingPipeline
    {
        #region Constants

        public const string P_MUTATEPROMOTE = "mutate-promote";
        public const string P_NUM_TRIES = "tries";
        public const int NUM_SOURCES = 1;

        #endregion // Constants
        #region Static

        private static bool Promotable(GPInitializer initializer, GPNode node)
        {
            // A node is promotable if:
            // 1: its parent is a GPNode
            if (!(node.Parent is GPNode))
                return false;
            var parent = (GPNode)(node.Parent);

            GPType t;
            if (parent.Parent is GPNode)
                // ugh, expensive
                t = ((GPNode)parent.Parent).Constraints(initializer).ChildTypes[parent.ArgPosition];
            else
                t = ((GPTree)parent.Parent).Constraints(initializer).TreeType;

            // 2: the node's returntype is type-compatible with its GRANDparent's return slot
            return node.Constraints(initializer).ReturnType.CompatibleWith(initializer, t);
        }

        private static void PromoteSomething(GPNode node)
        {
            // the node's parent MUST be a GPNode -- we've checked that already
            var parent = (GPNode)node.Parent;

            node.Parent = parent.Parent;
            node.ArgPosition = parent.ArgPosition;

            if (parent.Parent is GPNode)
                ((GPNode)parent.Parent).Children[parent.ArgPosition] = node;
            else
                ((GPTree)parent.Parent).Child = node;
            return;
        }

        private static int NumPromotableNodes(GPInitializer initializer, GPNode root, int soFar)
        {
            if (Promotable(initializer, root))
                soFar++;
            return root.Children.Aggregate(soFar, (current, t) => NumPromotableNodes(initializer, t, current));
        }

        #endregion // Static
        #region Fields

        private GPNode _promotableNode;

        #endregion // Fields
        #region Properties

        public override IParameter DefaultBase => GPBreedDefaults.ParamBase.Push(P_MUTATEPROMOTE); 

        /// <summary>
        /// Is our tree fixed?  If not, this is -1 
        /// </summary>
        public int Tree { get; set; }

        /// <summary>
        /// The number of times the pipeline tries to build a valid mutated
        /// tree before it gives up and just passes on the original 
        /// </summary>
        public int NumTries { get; set; }

        public override int NumSources => NUM_SOURCES; 
        

        #endregion // Properties
        #region Setup

        public override void  Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            
            var def = DefaultBase;
            
            NumTries = state.Parameters.GetInt(paramBase.Push(P_NUM_TRIES), def.Push(P_NUM_TRIES), 1);
            if (NumTries == 0)
                state.Output.Fatal("MutatePromotePipeline has an invalid number of tries (it must be >= 1).", 
                                    paramBase.Push(P_NUM_TRIES), def.Push(P_NUM_TRIES));
            
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
        /// sticks the node in 
        /// </summary>
        private int PickPromotableNode(GPInitializer initializer, GPNode root, int num)
        {
            if (Promotable(initializer, root))
            {
                num--;
                if (num == -1)
                // found it
                {
                    _promotableNode = root;
                    return num;
                }
            }
            foreach (var t in root.Children)
            {
                num = PickPromotableNode(initializer, t, num);
                if (num == -1)
                    break; // someone found it
            }
            return num;
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
            var n = Sources[0].Produce(min, max, subpop, inds, state, thread, misc);

            // should we bother?
            if (!state.Random[thread].NextBoolean(Likelihood))
            {
                return n;
            }

            var initializer = (GPInitializer)state.Initializer;

            // now let's mutate 'em
            for (var q = start; q < n + start; q++)
            {
                var i = (GPIndividual)inds[q];

                if (Tree != TREE_UNFIXED && (Tree < 0 || Tree >= i.Trees.Length))
                    // uh oh
                    state.Output.Fatal("MutatePromotePipeline attempted to fix tree.0 to a value which was out of bounds"
                        + " of the array of the individual's trees.  Check the pipeline's fixed tree values"
                        + " -- they may be negative or greater than the number of trees in an individual");

                for (var x = 0; x < NumTries; x++)
                {
                    int t;
                    // pick random tree
                    if (Tree == TREE_UNFIXED)
                        t = i.Trees.Length > 1 ? state.Random[thread].NextInt(i.Trees.Length) : 0;
                    else
                        t = Tree;

                    // is the tree promotable?
                    var numpromote = NumPromotableNodes(initializer, i.Trees[t].Child, 0);
                    if (numpromote == 0)
                        continue; // uh oh, try again

                    // promote the node, or if we're unsuccessful, just leave it alone
                    PickPromotableNode(initializer, i.Trees[t].Child, state.Random[thread].NextInt(numpromote));

                    // promote it
                    PromoteSomething(_promotableNode);
                    i.Evaluated = false;
                    break;
                }

                // add the new individual, replacing its previous source
                inds[q] = i;
            }
            return n;
        }
        
        #endregion // Operations
    }
}