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
    /// MutationPipeline is a GPBreedingPipeline which 
    /// implements a strongly-typed version of the 
    /// "Point Mutation" operator as described in Koza I.
    /// Actually, that's not quite true.  Koza doesn't have any Tree depth restrictions
    /// on his mutation operator.  This one does -- if the Tree gets deeper than
    /// the maximum Tree depth, then the new subTree is rejected and another one is
    /// tried.  Similar to how the Crosssover operator is implemented.
    /// 
    /// <p/>Mutated trees are restricted to being <tt>maxdepth</tt> depth at
    /// most and at most <tt>maxsize</tt> number of nodes.  If in
    /// <tt>tries</tt> attemptes, the pipeline cannot come up with a
    /// mutated tree within the depth limit, then it simply copies the
    /// original individual wholesale with no mutation.    
    ///  
    /// <p/>One additional feature: if <tt>equal</tt> is true, then MutationPipeline
    /// will attempt to replace the subTree with a Tree of approximately equal size.
    /// How this is done exactly, and how close it is, is entirely up to the pipeline's
    /// Tree builder -- for example, Grow/Full/HalfBuilder don't support this at all, while
    /// RandomBranch will replace it with a Tree of the same size or "slightly smaller"
    /// as described in the algorithm.
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// ...as many as the child produces
    /// <p/><b>Number of Sources</b><br/>
    /// 1
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>tries</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(number of times to try finding valid pairs of nodes)</td></tr>
    /// 
    /// <tr><td valign="top"><i>base</i>.<tt>maxsize</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(maximum valid size, in nodes, of a crossed-over subtree)</td></tr>
    ///
    /// <tr><td valign="top"><i>base</i>.<tt>maxdepth</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(maximum valid depth of a crossed-over subTree)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>ns</tt><br/>
    /// <font size="-1">classname, inherits and != GPNodeSelector</font></td>
    /// <td valign="top">(GPNodeSelector for Tree)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>build</tt>.0<br/>
    /// <font size="-1">classname, inherits and != GPNodeBuilder</font></td>
    /// <td valign="top">(GPNodeBuilder for new subTree)</td></tr>
    /// <tr><td valign="top"><tt>equal</tt><br/>
    /// <font size="-1"/>bool = <tt>true</tt> or <tt>false</tt> (default)</td>
    /// <td valign="top">(do we attempt to replace the subTree with a new one of roughly the same size?)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>Tree.0</tt><br/>
    /// <font size="-1">0 &lt; int &lt; (num Trees in individuals), if exists</font></td>
    /// <td valign="top">(Tree chosen for mutation; if parameter doesn't exist, Tree is picked at random)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// gp.koza.mutate
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"/><i>base</i>.<tt>ns</tt><br/>
    /// <td>NodeSelect</td></tr>
    /// <tr><td valign="top"/><i>base</i>.<tt>build</tt><br/>
    /// <td>Builder</td></tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.koza.MutationPipeline")]
    public class MutationPipeline : GPBreedingPipeline
    {
        #region Constants

        private const long SerialVersionUID = 1;

        public const string P_NUM_TRIES = "tries";
        public const string P_MAXDEPTH = "maxdepth";
        public const string P_MAXSIZE = "maxsize";        
        public const string P_MUTATION = "mutate";
        public const string P_BUILDER = "build";
        public const string P_EQUALSIZE = "equal";
        public const int INDS_PRODUCED = 1;
        public const int NUM_SOURCES = 1;
        public const int NO_SIZE_LIMIT = -1;

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return GPKozaDefaults.ParamBase.Push(P_MUTATION); }
        }

        public override int NumSources
        {
            get { return NUM_SOURCES; }
        }

        /// <summary>
        /// How the pipeline chooses a subTree to mutate 
        /// </summary>
        public IGPNodeSelector NodeSelect { get; set; }

        /// <summary>
        /// How the pipeline builds a new subTree 
        /// </summary>
        public GPNodeBuilder Builder { get; set; }

        /// <summary>
        /// The number of times the pipeline tries to build a valid mutated
        /// Tree before it gives up and just passes on the original 
        /// </summary>
        public int NumTries { get; set; }

        /// <summary>
        /// The maximum depth of a mutated Tree 
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// The largest tree (measured as a nodecount) the pipeline is allowed to form.
        /// </summary>
        public int MaxSize { get; set; }

        /// <summary>
        /// Do we try to replace the subTree with another of the same size? 
        /// </summary>
        public bool EqualSize { get; set; }

        /// <summary>
        /// Is our Tree fixed?  If not, this is -1 
        /// </summary>
        public int Tree { get; set; }

        #endregion // Properties
        #region Cloning

        public override object Clone()
        {
            var c = (MutationPipeline)base.Clone();

            // deep-cloned stuff
            c.NodeSelect = (IGPNodeSelector)NodeSelect.Clone();

            return c;
        }

        #endregion // Cloning
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;
            var p = paramBase.Push(P_NODESELECTOR).Push("" + 0);
            var d = def.Push(P_NODESELECTOR).Push("" + 0);

            NodeSelect = (IGPNodeSelector)(state.Parameters.GetInstanceForParameter(p, d, typeof(IGPNodeSelector)));
            NodeSelect.Setup(state, p);

            p = paramBase.Push(P_BUILDER).Push("" + 0);
            d = def.Push(P_BUILDER).Push("" + 0);

            Builder = (GPNodeBuilder)(state.Parameters.GetInstanceForParameter(p, d, typeof(GPNodeBuilder)));
            Builder.Setup(state, p);

            NumTries = state.Parameters.GetInt(paramBase.Push(P_NUM_TRIES), def.Push(P_NUM_TRIES), 1);
            if (NumTries == 0)
                state.Output.Fatal("Mutation Pipeline has an invalid number of tries (it must be >= 1).",
                                                        paramBase.Push(P_NUM_TRIES), def.Push(P_NUM_TRIES));

            MaxDepth = state.Parameters.GetInt(paramBase.Push(P_MAXDEPTH), def.Push(P_MAXDEPTH), 1);
            if (MaxDepth == 0)
            {
                state.Output.Fatal("The Mutation Pipeline " + paramBase + "has an invalid maximum depth (it must be >= 1).",
                                                              paramBase.Push(P_MAXDEPTH), def.Push(P_MAXDEPTH));
            }

            MaxSize = NO_SIZE_LIMIT;
            if (state.Parameters.ParameterExists(paramBase.Push(P_MAXSIZE), def.Push(P_MAXSIZE)))
            {
                MaxSize = state.Parameters.GetInt(paramBase.Push(P_MAXSIZE), def.Push(P_MAXSIZE), 1);
                if (MaxSize < 1)
                    state.Output.Fatal("Maximum tree size, if defined, must be >= 1");
            }

            EqualSize = state.Parameters.GetBoolean(paramBase.Push(P_EQUALSIZE), def.Push(P_EQUALSIZE), false);

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
        /// Returns true if inner1 can feasibly be swapped into inner2's position 
        /// </summary>        
        private bool VerifyPoints(GPNode inner1, GPNode inner2)
        {
            // We know they're swap-compatible since we generated inner1
            // to be exactly that.  So don't bother.

            // next check to see if inner1 can fit in inner2's spot
            if (inner1.Depth + inner2.AtDepth() > MaxDepth)
                return false;

            // check for size
            if (MaxSize != NO_SIZE_LIMIT)
            {
                // first easy check
                var inner1Size = inner1.NumNodes(GPNode.NODESEARCH_ALL);
                var inner2Size = inner2.NumNodes(GPNode.NODESEARCH_ALL);
                if (inner1Size > inner2Size)  // need to test further
                {
                    // let's keep on going for the more complex test
                    GPNode root2 = ((GPTree)(inner2.RootParent())).Child;
                    var root2Size = root2.NumNodes(GPNode.NODESEARCH_ALL);
                    if (root2Size - inner2Size + inner1Size > MaxSize)  // take root2, remove inner2 and swap in inner1.  Is it still small enough?
                        return false;
                }
            }

            // checks done!
            return true;
        }

        public override int Produce(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            // grab individuals from our source and stick 'em right into inds.
            // we'll modify them from there
            var n = Sources[0].Produce(min, max, start, subpop, inds, state, thread);

            // should we bother?
            if (!state.Random[thread].NextBoolean(Likelihood))
                return Reproduce(n, start, subpop, inds, state, thread, false);  // DON'T produce children from source -- we already did

            var initializer = ((GPInitializer)state.Initializer);

            // now let's mutate 'em
            for (var q = start; q < n + start; q++)
            {
                var i = (GPIndividual)inds[q];

                if (Tree != TREE_UNFIXED && (Tree < 0 || Tree >= i.Trees.Length))
                    // uh oh
                    state.Output.Fatal("GP Mutation Pipeline attempted to fix Tree.0 to a value which was out of bounds"
                        + " of the array of the individual's Trees.  Check the pipeline's fixed Tree values"
                        + " -- they may be negative or greater than the number of Trees in an individual");


                int t;
                // pick random Tree
                if (Tree == TREE_UNFIXED)
                    if (i.Trees.Length > 1)
                        t = state.Random[thread].NextInt(i.Trees.Length);
                    else
                        t = 0;
                else
                    t = Tree;

                // validity result...
                var res = false;

                // prepare the NodeSelector
                NodeSelect.Reset();

                // pick a node

                GPNode p1 = null; // the node we pick
                GPNode p2 = null;

                for (var x = 0; x < NumTries; x++)
                {
                    // pick a node in individual 1
                    p1 = NodeSelect.PickNode(state, subpop, thread, i, i.Trees[t]);

                    // generate a Tree swap-compatible with p1's position


                    var size = GPNodeBuilder.NOSIZEGIVEN;
                    if (EqualSize)
                        size = p1.NumNodes(GPNode.NODESEARCH_ALL);

                    p2 = Builder.NewRootedTree(state, p1.ParentType(initializer), thread, p1.Parent,
                                i.Trees[t].Constraints(initializer).FunctionSet, p1.ArgPosition, size);

                    // check for depth and swap-compatibility limits
                    res = VerifyPoints(p2, p1); // p2 can fit in p1's spot  -- the order is important!

                    // did we get something that had both nodes verified?
                    if (res)
                        break;
                }

                GPIndividual j;

                if (Sources[0] is BreedingPipeline)
                // it's already a copy, so just smash the Tree in
                {
                    j = i;
                    if (res)
                    // we're in business
                    {
                        p2.Parent = p1.Parent;
                        p2.ArgPosition = p1.ArgPosition;
                        if (p2.Parent is GPNode)
                            ((GPNode)p2.Parent).Children[p2.ArgPosition] = p2;
                        else
                            ((GPTree)p2.Parent).Child = p2;
                        j.Evaluated = false; // we've modified it
                    }
                }
                // need to clone the individual
                else
                {
                    j = i.LightClone();

                    // Fill in various Tree information that didn't get filled in there
                    j.Trees = new GPTree[i.Trees.Length];

                    // at this point, p1 or p2, or both, may be null.
                    // If not, swap one in.  Else just copy the parent.
                    for (var x = 0; x < j.Trees.Length; x++)
                    {
                        if (x == t && res)
                        // we've got a Tree with a kicking cross position!
                        {
                            j.Trees[x] = i.Trees[x].LightClone();
                            j.Trees[x].Owner = j;
                            j.Trees[x].Child = i.Trees[x].Child.CloneReplacingNoSubclone(p2, p1);
                            j.Trees[x].Child.Parent = j.Trees[x];
                            j.Trees[x].Child.ArgPosition = 0;
                            j.Evaluated = false;
                        }
                        // it's changed
                        else
                        {
                            j.Trees[x] = i.Trees[x].LightClone();
                            j.Trees[x].Owner = j;
                            j.Trees[x].Child = (GPNode)i.Trees[x].Child.Clone();
                            j.Trees[x].Child.Parent = j.Trees[x];
                            j.Trees[x].Child.ArgPosition = 0;
                        }
                    }
                }

                // add the new individual, replacing its previous source
                inds[q] = j;
            }
            return n;
        }

        #endregion // Operations
    }
}