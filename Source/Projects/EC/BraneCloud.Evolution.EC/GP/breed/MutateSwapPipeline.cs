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
    /// MutateSwapPipeline works very similarly to the Swap algorithm described in  Kumar Chellapilla,
    /// "A Preliminary Investigation into Evolving Modular Programs without Subtree Crossover", GP98.
    /// 
    /// <p/>MutateSwapPipeline picks a random tree, then picks
    /// randomly from all the swappable nodes in the tree, and swaps two of its subtrees.  
    /// If its chosen tree has no swappable nodes, it repeats
    /// the choose-tree process.  If after <i>tries</i> times
    /// it has failed to find a tree with swappable nodes, it gives up and simply
    /// copies the individual.
    /// 
    /// <p/>"Swapping" means to take a node <i>n</i>, and choose two children
    /// nodes of <i>n</i>, <i>x</i> and <i>y</i>, such that <i>x</i>'s return
    /// type is swap-compatible with <i>y</i>'s slot, and <i>y</i>'s return
    /// type is swap-compatible with <i>x</i>'s slot.  The subtrees rooted at
    /// <i>x</i> and <i>y</i> are swapped.
    /// 
    /// <p/>A "Swappable" node means a node which is capable of swapping
    /// given the existing function set.  In general to swap a node <i>foo</i>,
    /// it must have at least two children whose return types are type-compatible
    /// with each other's slots in <i>foo</i>.
    /// 
    /// <p/>This method is very expensive in searching nodes for
    /// "swappability".  However, if the number of types is 1 (the
    /// GP run is typeless) then the type-constraint-checking
    /// code is bypassed and the method runs a little faster.
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
    /// gp.breed.mutate-swap
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.breed.MutateSwapPipeline")]
    public class MutateSwapPipeline : GPBreedingPipeline
    {
        #region Constants

        public const string P_MUTATESWAP = "mutate-swap";
        public const string P_NUM_TRIES = "tries";
        public const int NUM_SOURCES = 1;

        #endregion // Constants
        #region Static

        /// <summary>
        /// This very expensive method (for types) might be improved in various ways I guess. 
        /// </summary>
        private static bool Swappable(GPInitializer initializer, GPNode node)
        {
            if (node.Children.Length < 2)
                return false; // fast check

            if (initializer.NumAtomicTypes + initializer.NumSetTypes == 1)
                return true; // next fast check

            // we're typed, so now we have to check our type compatibility
            for (var x = 0; x < node.Constraints(initializer).ChildTypes.Length - 1; x++)
                for (var y = x + 1; y < node.Constraints(initializer).ChildTypes.Length; y++)
                    if (node.Children[x].Constraints(initializer).ReturnType
                        .CompatibleWith(initializer, node.Constraints(initializer).ChildTypes[y])
                        && node.Children[y].Constraints(initializer).ReturnType
                        .CompatibleWith(initializer, node.Constraints(initializer).ChildTypes[x]))
                        // whew!
                        return true;
            return false;
        }

        private static void SwapSomething(GPNode node, IEvolutionState state, int thread)
        {
            if (((GPInitializer)state.Initializer).NumAtomicTypes + ((GPInitializer)state.Initializer).NumSetTypes == 1)
                // typeless
                SwapSomethingTypeless(node, state, thread);
            else
                SwapSomethingDirtyWork(node, state, thread);
        }

        private static void SwapSomethingTypeless(GPNode node, IEvolutionState state, int thread)
        {
            // assumes that number of child nodes >= 2

            // pick a random first node
            var x = state.Random[thread].NextInt(node.Children.Length);
            // pick a random second node
            var y = state.Random[thread].NextInt(node.Children.Length - 1);
            if (y >= x)
                y++; // adjust for first node

            // swap the nodes

            var tmp = node.Children[x];
            node.Children[x] = node.Children[y];
            node.Children[y] = tmp;
            node.Children[x].ArgPosition = (sbyte)x;
            node.Children[y].ArgPosition = (sbyte)y;
            // no need to set parent -- it's the same parent of course
        }

        private static void SwapSomethingDirtyWork(GPNode node, IEvolutionState state, int thread)
        {
            var numSwappable = 0;
            var initializer = ((GPInitializer)state.Initializer);
            for (var x = 0; x < node.Constraints(initializer).ChildTypes.Length - 1; x++)
                for (var y = x + 1; y < node.Constraints(initializer).ChildTypes.Length; y++)
                    if (node.Children[x].Constraints(initializer).ReturnType
                        .CompatibleWith(initializer, node.Constraints(initializer).ChildTypes[y])
                        && node.Children[y].Constraints(initializer).ReturnType
                        .CompatibleWith(initializer, node.Constraints(initializer).ChildTypes[x]))
                        // whew!
                        numSwappable++;

            // pick a random item to swap -- numSwappable is assumed to be > 0
            var swapItem = state.Random[thread].NextInt(numSwappable);

            numSwappable = 0;
            // find it

            for (var x = 0; x < node.Constraints(initializer).ChildTypes.Length - 1; x++)
                for (var y = x + 1; y < node.Constraints(initializer).ChildTypes.Length; y++)
                    if (node.Children[x].Constraints(initializer).ReturnType
                        .CompatibleWith(initializer, node.Constraints(initializer).ChildTypes[y])
                        && node.Children[y].Constraints(initializer).ReturnType
                        .CompatibleWith(initializer, node.Constraints(initializer).ChildTypes[x]))
                    {
                        if (numSwappable == swapItem)
                        // found it
                        {
                            // swap the children
                            var tmp = node.Children[x];
                            node.Children[x] = node.Children[y];
                            node.Children[y] = tmp;
                            node.Children[x].ArgPosition = (sbyte)x;
                            node.Children[y].ArgPosition = (sbyte)y;
                            // no need to set parent -- it's the same parent of course
                            return;
                        }
                        numSwappable++;
                    }
        }

        private static int NumSwappableNodes(GPInitializer initializer, GPNode root, int soFar)
        {
            if (Swappable(initializer, root))
                soFar++;
            return root.Children.Aggregate(soFar, (current, t) => NumSwappableNodes(initializer, t, current));
        }

        #endregion // Static
        #region Fields

        private GPNode _swappableNode;

        #endregion // Fields
        #region Properties

        public override IParameter DefaultBase
        {
            get { return GPBreedDefaults.ParamBase.Push(P_MUTATESWAP); }
        }

        /// <summary>
        /// The number of times the pipeline tries to build a valid mutated
        /// tree before it gives up and just passes on the original 
        /// </summary>
        public int NumTries { get; set; }

        /// <summary>
        /// Is our tree fixed?  If not, this is -1 
        /// </summary>
        public int Tree { get; set; }

        public override int NumSources
        {
            get { return NUM_SOURCES; }
        }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;

            NumTries = state.Parameters.GetInt(paramBase.Push(P_NUM_TRIES), def.Push(P_NUM_TRIES), 1);
            if (NumTries == 0)
                state.Output.Fatal("MutateSwapPipeline has an invalid number of tries (it must be >= 1).",
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
        private int PickSwappableNode(GPInitializer initializer, GPNode root, int num)
        {
            if (Swappable(initializer, root))
            {
                num--;
                if (num == -1)
                // found it
                {
                    _swappableNode = root;
                    return num;
                }
            }
            foreach (var t in root.Children)
            {
                num = PickSwappableNode(initializer, t, num);
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

            // now let's mutate 'em
            for (var q = start; q < n + start; q++)
            {
                var i = (GPIndividual)inds[q];

                if (Tree != TREE_UNFIXED && (Tree < 0 || Tree >= i.Trees.Length))
                    // uh oh
                    state.Output.Fatal("MutateSwapPipeline attempted to fix tree.0 to a value which was out of bounds"
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

                    // is the tree swappable?      
                    var initializer = ((GPInitializer)state.Initializer);
                    var numswap = NumSwappableNodes(initializer, i.Trees[t].Child, 0);
                    if (numswap == 0)
                        continue; // uh oh, try again

                    // swap the node, or if we're unsuccessful, just leave it alone
                    PickSwappableNode(initializer, i.Trees[t].Child, state.Random[thread].NextInt(numswap));

                    // node is now in swappableNode, swap it
                    SwapSomething(_swappableNode, state, thread);

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