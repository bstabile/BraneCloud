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
    /// RehangPipeline picks a nonterminal node other than the root and "rehangs" it as
    /// a new root. Imagine if the tree were nodes connected with string.
    /// Grab the new node and pick it up, letting the other nodes hang
    /// underneath it as a new "root".  That's in effect what you're doing.
    /// 
    /// <p/><b>Important Note</b>: Because it must be free of any constraints
    /// by nature, RehangPipeline does not work with strong typing.  You must
    /// not have more than one type defined in order to use RehangPipeline.  
    /// 
    /// <p/>RehangPipeline picks a random tree, then picks randomly from
    /// all the nonterminals in the tree other than the root, and rehangs the
    /// chosen nonterminal 
    /// as the new root. If its chosen tree has no nonterminals, it repeats
    /// the choose-tree process.  If after <i>tries</i> times
    /// it has failed to find a tree with nonterminals (other than the root),
    /// it gives up and simply
    /// copies the individual.  As you might guess, determining if a tree has
    /// nonterminals is very fast, so <i>tries</i> can be pretty large with
    /// little to no detriment to evolution speed.
    /// 
    /// <p/>"Rehanging" is complicated to describe.  First, you pick a random
    /// child of your chosen nonterminal <i>n</i>, 
    /// and remove this subtree from the tree.
    /// Call this subtree <i>T</i>.  Next, you set the nonterminal as a new root; its
    /// former parent <i>p</i> now fills the slot left behind by the missing subtree.
    /// The <i>p</i>'s former parent <i>q</i> now fills the slot left behind by 
    /// <i>n</i>.  <i>q</i>'s former parent <i>r</i> now fills the slot left behind
    /// by <i>p</i>, and so on.  This proceeds all the way up to the old root, which
    /// will be left with one empty slot (where its former child was that is now its new
    /// parent).  This slot is then filled with <i>T</i>
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
    /// gp.breed.rehang
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.breed.RehangPipeline")]
    public class RehangPipeline : GPBreedingPipeline
    {
        #region Constants

        public const string P_REHANG = "rehang";
        public const string P_NUM_TRIES = "tries";
        public const int NUM_SOURCES = 1;

        #endregion // Constants
        #region Static

        private static int NumRehangableNodes(GPNode root, int soFar)
        {
            // we don't include the tree root
            return root.Children.Aggregate(soFar, (current, t) => NumRehangableNodesDirtyWork(t, current));
        }

        private static int NumRehangableNodesDirtyWork(GPNode root, int soFar)
        {
            if (root.Children.Length > 0)
                soFar++; // rehangable
            return root.Children.Aggregate(soFar, (current, t) => NumRehangableNodesDirtyWork(t, current));
        }

        private static void Rehang(IEvolutionState state, int thread, GPNode pivot, GPNode root)
        {
            // pivot must not be root
            if (pivot == root)
                // uh oh
                throw new ApplicationException("Oops, pivot==root in ec.gp.breed.Rehang.rehang(...)");

            // snip off a random child from the pivot
            var spot = state.Random[thread].NextInt(pivot.Children.Length);
            var cut = pivot.Children[spot];

            // rehang pivot as new root and set it up
            var newPivot = (GPNode)(pivot.Parent);
            ((GPTree)root.Parent).Child = pivot;
            pivot.Parent = root.Parent;

            var newSpot = pivot.ArgPosition;
            pivot.ArgPosition = 0;
            var oldPivot = pivot;
            pivot = newPivot;

            // rehang the intermediate nodes
            while (pivot != root)
            {
                newPivot = (GPNode)pivot.Parent;
                pivot.Parent = oldPivot;
                oldPivot.Children[spot] = pivot;

                var tmpSpot = pivot.ArgPosition;
                pivot.ArgPosition = spot;
                spot = newSpot;
                newSpot = tmpSpot;

                oldPivot = pivot;
                pivot = newPivot;
            }

            // rehang the root and set the cut
            pivot.Parent = oldPivot;
            oldPivot.Children[spot] = pivot;
            pivot.ArgPosition = spot;
            cut.Parent = pivot;
            cut.ArgPosition = newSpot;
            pivot.Children[newSpot] = cut;
        }

        #endregion // Static
        #region Fields

        private GPNode _rehangableNode;

        #endregion // Fields
        #region Properties

        public override IParameter DefaultBase => GPBreedDefaults.ParamBase.Push(P_REHANG);

        /// <summary>
        /// The number of times the pipeline tries to find a tree with a
        /// nonterminal before giving up and just copying the individual. 
        /// </summary>
        public int NumTries { get; set; }

        /// <summary>
        /// Is our tree fixed?  If not, this is -1 
        /// </summary>
        public int Tree { get; set; }

        public override int NumSources => NUM_SOURCES;

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;

            NumTries = state.Parameters.GetInt(paramBase.Push(P_NUM_TRIES), def.Push(P_NUM_TRIES), 1);
            if (NumTries == 0)
                state.Output.Fatal("RehangPipeline has an invalid number of tries (it must be >= 1).",
                                                    paramBase.Push(P_NUM_TRIES), def.Push(P_NUM_TRIES));

            if (((GPInitializer)state.Initializer).NumAtomicTypes + ((GPInitializer)state.Initializer).NumSetTypes > 1)
                state.Output.Fatal("RehangPipeline only works when there is only one type (the system is typeless)", paramBase, def);

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

        private int PickRehangableNode(GPNode root, int num)
        {
            // we don't include the tree root
            foreach (var t in root.Children)
            {
                num = PickRehangableNodeDirtyWork(t, num);
                if (num == -1)
                    break; // someone found it
            }
            return num;
        }

        /// <summary>
        /// sticks the node in 
        /// </summary>
        private int PickRehangableNodeDirtyWork(GPNode root, int num)
        {
            if (root.Children.Length > 0)
            // rehangable
            {
                num--;
                if (num == -1)
                // found it
                {
                    _rehangableNode = root;
                    return num;
                }
            }
            foreach (var t in root.Children)
            {
                num = PickRehangableNodeDirtyWork(t, num);
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

            // now let's rehang 'em
            for (var q = start; q < n + start; q++)
            {
                var i = (GPIndividual) inds[q];
                
                if (Tree != TREE_UNFIXED && (Tree < 0 || Tree >= i.Trees.Length))
                // uh oh
                    state.Output.Fatal("RehangPipeline attempted to fix tree.0 to a value which was out of bounds of the array of the individual's trees.  Check the pipeline's fixed tree values -- they may be negative or greater than the number of trees in an individual");
                
                for (var x = 0; x < NumTries; x++)
                {
                    int t;
                    // pick random tree
                    if (Tree == TREE_UNFIXED)
                        t = i.Trees.Length > 1 ? state.Random[thread].NextInt(i.Trees.Length) : 0;
                    else
                        t = Tree;
                    
                    // is the tree rehangable?              
                    if (i.Trees[t].Child.Children.Length == 0)
                        continue; // uh oh, try again
                    var rehangable = i.Trees[t].Child.Children.Any(t1 => t1.Children.Length > 0);
                    if (!rehangable)
                        continue; // the root's children are all terminals
                    
                    var numrehang = NumRehangableNodes(i.Trees[t].Child, 0);
                    PickRehangableNode(i.Trees[t].Child, state.Random[thread].NextInt(numrehang));
                    
                    Rehang(state, thread, _rehangableNode, i.Trees[t].Child);
                    
                    i.Evaluated = false;
                }
                
                // add the new individual, replacing its previous source
                inds[q] = i;
            }
            return n;
        }

        #endregion // Operations
    }
}