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
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Support;

namespace BraneCloud.Evolution.EC.GP.Breed
{
    /// <summary> 
    /// InternalCrossoverPipeline picks two subtrees from somewhere within an individual,
    /// and crosses them over.  Before doing so, it checks to make sure that the
    /// subtrees come from trees with the same tree constraints, that the subtrees
    /// are swap-compatible with each other, that the new individual does not violate
    /// depth constraints, and that one subtree does not contain the other.  It tries
    /// <tt>tries</tt> times to find a valid subtree pair to cross over.  Failing this,
    /// it just copies the individual.
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
    /// <tr><td valign="top"><i>base</i>.<tt>maxdepth</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(maximum valid depth of the crossed-over individual's trees)</td></tr>
    /// 
    /// <tr><td valign="top"><i>base</i>.<tt>ns.</tt>0<br/>
    /// <font size="-1">classname, inherits and != GPNodeSelector</font></td>
    /// <td valign="top">(GPNodeSelector for subtree 0.  </td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>ns.</tt>1<br/>
    /// <font size="-1">classname, inherits and != GPNodeSelector,<br/>
    /// or String <tt>same</tt></font></td>
    /// <td valign="top">(GPNodeSelector for subtree 1.  If value is <tt>same</tt>, then <tt>ns.1</tt> a copy of whatever <tt>ns.0</tt> is)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>tree.0</tt><br/>
    /// <font size="-1">0 &lt; int &lt; (num trees in individuals), if exists</font></td>
    /// <td valign="top">(first tree for the crossover; if parameter doesn't exist, tree is picked at random)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>tree.1</tt><br/>
    /// <font size="-1">0 &lt; int &lt; (num trees in individuals), if exists</font></td>
    /// <td valign="top">(second tree for the crossover; if parameter doesn't exist, tree is picked at random.  This tree <b>must</b> have the same GPTreeConstraints as <tt>tree.0</tt>, if <tt>tree.0</tt> is defined.)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// gp.breed.internal-xover
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"/><i>base</i>.<tt>ns.</tt><i>n</i><br/>
    /// <td>nodeselect<i>n</i> (<i>n</i> is 0 or 1)</td></tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.breed.InternalCrossoverPipeline")]
    public class InternalCrossoverPipeline : GPBreedingPipeline
    {
        #region Constants

        private const long SerialVersionUID = 1;

        public const string P_INTERNALCROSSOVER = "internal-xover";
        public const string P_NUM_TRIES = "tries";
        public const string P_MAXDEPTH = "maxdepth";
        public const int NUM_SOURCES = 1;
        public const string KEY_PARENTS = "parents";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase => GPBreedDefaults.ParamBase.Push(P_INTERNALCROSSOVER); 

        /// <summary>
        /// How the pipeline chooses the first subtree 
        /// </summary>
        public IGPNodeSelector NodeSelect0 { get; set; }

        /// <summary>
        /// How the pipeline chooses the second subtree 
        /// </summary>
        public IGPNodeSelector NodeSelect1 { get; set; }

        /// <summary>
        /// How many times the pipeline attempts to pick nodes until it gives up. 
        /// </summary>
        public int NumTries { get; set; }

        /// <summary>
        /// The deepest tree the pipeline is allowed to form.  Single terminal trees are depth 1. 
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// Is the first tree fixed?  If not, this is -1 
        /// </summary>
        public int Tree1 { get; set; }

        /// <summary>
        /// Is the second tree fixed?  If not, this is -1 
        /// </summary>
        public int Tree2 { get; set; }

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
            var p = paramBase.Push(P_NODESELECTOR).Push("0");
            var d = def.Push(P_NODESELECTOR).Push("0");

            NodeSelect0 = (IGPNodeSelector)(state.Parameters.GetInstanceForParameter(p, d, typeof(IGPNodeSelector)));
            NodeSelect0.Setup(state, p);

            p = paramBase.Push(P_NODESELECTOR).Push("1");
            d = def.Push(P_NODESELECTOR).Push("1");

            if (state.Parameters.ParameterExists(p, d) && state.Parameters.GetString(p, d).Equals(V_SAME))
                // can't just copy it this time; the selectors
                // use internal caches.  So we have to clone it no matter what
                NodeSelect1 = (IGPNodeSelector)(NodeSelect0.Clone());
            else
            {
                NodeSelect1 = (IGPNodeSelector)(state.Parameters.GetInstanceForParameter(p, d, typeof(IGPNodeSelector)));
                NodeSelect1.Setup(state, p);
            }

            NumTries = state.Parameters.GetInt(paramBase.Push(P_NUM_TRIES), def.Push(P_NUM_TRIES), 1);
            if (NumTries == 0)
                state.Output.Fatal("InternalCrossover Pipeline has an invalid number of tries (it must be >= 1).",
                                                                paramBase.Push(P_NUM_TRIES), def.Push(P_NUM_TRIES));

            MaxDepth = state.Parameters.GetInt(paramBase.Push(P_MAXDEPTH), def.Push(P_MAXDEPTH), 1);
            if (MaxDepth == 0)
                state.Output.Fatal("InternalCrossover Pipeline has an invalid maximum depth (it must be >= 1).",
                                                                paramBase.Push(P_MAXDEPTH), def.Push(P_MAXDEPTH));

            Tree1 = TREE_UNFIXED;
            if (state.Parameters.ParameterExists(paramBase.Push(P_TREE).Push("" + 0), def.Push(P_TREE).Push("" + 0)))
            {
                Tree1 = state.Parameters.GetInt(paramBase.Push(P_TREE).Push("" + 0), def.Push(P_TREE).Push("" + 0), 0);
                if (Tree1 == -1)
                    state.Output.Fatal("Tree fixed value, if defined, must be >= 0");
            }

            Tree2 = TREE_UNFIXED;
            if (state.Parameters.ParameterExists(paramBase.Push(P_TREE).Push("" + 1), def.Push(P_TREE).Push("" + 1)))
            {
                Tree2 = state.Parameters.GetInt(paramBase.Push(P_TREE).Push("" + 1), def.Push(P_TREE).Push("" + 1), 0);
                if (Tree2 == -1)
                    state.Output.Fatal("Tree fixed value, if defined, must be >= 0");
            }
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Returns true if inner1 and inner2 do not contain one another 
        /// </summary>
        private static bool NoContainment(GPNode inner1, GPNode inner2)
        {
            IGPNodeParent current = inner1;
            while (current != null && current is GPNode)
            {
                if (current == inner2)
                    return false; // inner2 contains inner1
                current = ((GPNode)current).Parent;
            }
            current = inner2;
            while (current != null && current is GPNode)
            {
                if (current == inner1)
                    return false; // inner1 contains inner2
                current = ((GPNode)current).Parent;
            }
            return true;
        }

        /// <summary>
        /// Returns true if inner1 can feasibly be swapped into inner2's position. 
        /// </summary>       
        private bool VerifyPoints(GPInitializer initializer, GPNode inner1, GPNode inner2)
        {
            // first check to see if inner1 is swap-compatible with inner2
            // on a type basis
            if (!inner1.SwapCompatibleWith(initializer, inner2))
                return false;

            // next check to see if inner1 can fit in inner2's spot
            if (inner1.Depth + inner2.AtDepth() > MaxDepth)
                return false;

            // checks done!
            return true;
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

            IntBag[] parentparents = null;
            IntBag[] preserveParents = null;
            if (misc != null && misc.ContainsKey(KEY_PARENTS))
            {
                preserveParents = (IntBag[])misc[KEY_PARENTS];
                parentparents = new IntBag[2];
                misc[KEY_PARENTS] = parentparents;
            }

            // should we bother?
            if (!state.Random[thread].NextBoolean(Likelihood))
            {
                return n;
            }

            var initializer = (GPInitializer)state.Initializer;

            for (var q = start; q < n + start; q++)
            {
                var i = (GPIndividual)inds[q];

                if (Tree1 != TREE_UNFIXED && (Tree1 < 0 || Tree1 >= i.Trees.Length))
                    // uh oh
                    state.Output.Fatal("Internal Crossover Pipeline attempted to fix tree.0 to a value"
                        + " which was out of bounds of the array of the individual's trees. "
                        + " Check the pipeline's fixed tree values -- they may be negative"
                        + " or greater than the number of trees in an individual");

                if (Tree2 != TREE_UNFIXED && (Tree2 < 0 || Tree2 >= i.Trees.Length))
                    // uh oh
                    state.Output.Fatal("Internal Crossover Pipeline attempted to fix tree.0 to a value"
                        + " which was out of bounds of the array of the individual's trees. "
                        + " Check the pipeline's fixed tree values -- they may be negative"
                        + " or greater than the number of trees in an individual");

                var t1 = 0;
                var t2 = 0;
                if (Tree1 == TREE_UNFIXED || Tree2 == TREE_UNFIXED)
                {
                    do
                    // pick random trees  -- their GPTreeConstraints must be the same
                    {
                        if (Tree1 == TREE_UNFIXED)
                            if (i.Trees.Length > 1)
                                t1 = state.Random[thread].NextInt(i.Trees.Length);
                            else
                                t1 = 0;
                        else
                            t1 = Tree1;

                        if (Tree2 == TREE_UNFIXED)
                            if (i.Trees.Length > 1)
                                t2 = state.Random[thread].NextInt(i.Trees.Length);
                            else
                                t2 = 0;
                        else
                            t2 = Tree2;
                    }
                    while (i.Trees[t1].Constraints(initializer) != i.Trees[t2].Constraints(initializer));
                }
                else
                {
                    t1 = Tree1;
                    t2 = Tree2;
                    // make sure the constraints are okay
                    if (i.Trees[t1].Constraints(initializer) != i.Trees[t2].Constraints(initializer))
                        // uh oh
                        state.Output.Fatal("GP Crossover Pipeline's two tree choices are both specified by the user -- but their GPTreeConstraints are not the same");
                }


                // prepare the nodeselectors
                NodeSelect0.Reset();
                NodeSelect1.Reset();


                // pick some nodes

                GPNode p1 = null;
                GPNode p2 = null;
                var res = false;

                for (var x = 0; x < NumTries; x++)
                {
                    // pick a node in individual 1
                    p1 = NodeSelect0.PickNode(state, subpop, thread, i, i.Trees[t1]);

                    // pick a node in individual 2
                    p2 = NodeSelect1.PickNode(state, subpop, thread, i, i.Trees[t2]);

                    // make sure they're not the same node
                    res = (p1 != p2
                        && (t1 != t2 || NoContainment(p1, p2))
                        && VerifyPoints(initializer, p1, p2)
                        && VerifyPoints(initializer, p2, p1)); // 2 goes into 1
                    if (res)
                        break; // got one
                }

                // if res, then it's time to cross over!
                if (res)
                {
                    var oldparent = p1.Parent;
                    var oldArgPosition = p1.ArgPosition;

                    p1.Parent = p2.Parent;
                    p1.ArgPosition = p2.ArgPosition;
                    p2.Parent = oldparent;
                    p2.ArgPosition = oldArgPosition;

                    if (p1.Parent is GPNode)
                        ((GPNode)(p1.Parent)).Children[p1.ArgPosition] = p1;
                    else
                        ((GPTree)(p1.Parent)).Child = p1;

                    if (p2.Parent is GPNode)
                        ((GPNode)(p2.Parent)).Children[p2.ArgPosition] = p2;
                    else
                        ((GPTree)(p2.Parent)).Child = p2;

                    i.Evaluated = false; // we've modified it
                }

                // add the individuals to the population
                //inds[q] = i;
                inds.Add(i);
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
            var c = (InternalCrossoverPipeline) (base.Clone());
            
            // deep-cloned stuff
            c.NodeSelect0 = (IGPNodeSelector) (NodeSelect0.Clone());
            c.NodeSelect1 = (IGPNodeSelector) (NodeSelect1.Clone());
            return c;
        }

        #endregion // Cloning
    }
}