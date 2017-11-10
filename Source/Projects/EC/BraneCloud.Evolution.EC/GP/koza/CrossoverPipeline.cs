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
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.GP.Koza
{	
    /// <summary> 
    /// CrossoverPipeline is a GPBreedingPipeline which performs a strongly-typed
    /// version of  Koza-style "Subtree Crossover".  Two individuals are selected,
    /// then a single tree is chosen in each such that the two trees
    /// have the same GPTreeConstraints.  Then a random node is chosen
    /// in each tree such that each node's return type is type-compatible
    /// with the argument type of the slot in the parent which contains
    /// the other node.
    /// If by swapping subtrees at these nodes the two trees will not
    /// violate maximum depth constraints, then the trees perform the
    /// swap, otherwise, they repeat the hunt for random nodes.
    /// 
    /// <p/>The pipeline tries at most <i>tries</i> times to a pair
    /// of random nodes BOTH with valid swap constraints.  If it
    /// cannot find any such pairs after <i>tries</i> times, it 
    /// uses the pair of its last attempt.  If either of the nodes in the pair
    /// is valid, that node gets substituted with the other node.  Otherwise
    /// an individual invalid node isn't changed at all (it's "reproduced").
    /// 
    /// <p/><b>Compatibility with constraints.</b> 
    /// Since Koza-I/II only tries 1 time, and then follows this policy, this is
    /// compatible with Koza.  lil-gp either tries 1 time, or tries forever.
    /// Either way, this is compatible with lil-gp.  My hacked 
    /// <a href="http://www.cs.umd.edu/users/seanl/gp/">lil-gp kernel</a>
    /// either tries 1 time, <i>n</i> times, or forever.  This is compatible
    /// as well.
    /// 
    /// <p/>This pipeline typically produces up to 2 new individuals (the two newly-
    /// swapped individuals) per Produce(...) call.  If the system only
    /// needs a single individual, the pipeline will throw one of the
    /// new individuals away.  The user can also have the pipeline always
    /// throw away the second new individual instead of adding it to the population.
    /// In this case, the pipeline will only typically 
    /// produce 1 new individual per Produce(...) call.
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// 2 * minimum typical number of individuals produced by each source, unless TossSecondParent
    /// is set, in which case it's simply the minimum typical number.
    /// <p/><b>Number of Sources</b><br/>
    /// 2
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>tries</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(number of times to try finding valid pairs of nodes)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>maxdepth</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(maximum valid depth of a crossed-over subtree)</td></tr>
    /// 
    /// <tr><td valign=top><i>base</i>.<tt>maxsize</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(maximum valid size, in nodes, of a crossed-over subtree)</td></tr>
    ///
    /// <tr><td valign="top"><i>base</i>.<tt>tree.0</tt><br/>
    /// <font size="-1">0 &lt; int &lt; (num trees in individuals), if exists</font></td>
    /// <td valign="top">(first tree for the crossover; if parameter doesn't exist, tree is picked at random)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>tree.1</tt><br/>
    /// <font size="-1">0 &lt; int &lt; (num trees in individuals), if exists</font></td>
    /// <td valign="top">(second tree for the crossover; if parameter doesn't exist, tree is picked at random.  
    /// This tree <b>must</b> have the same GPTreeConstraints as <tt>tree.0</tt>, if <tt>tree.0</tt> is defined.)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>ns.</tt><i>n</i><br/>
    /// <font size="-1">classname, inherits and != GPNodeSelector,<br/>
    /// or String <tt>same</tt></font></td>
    /// <td valign="top">(GPNodeSelector for parent <i>n</i> (n is 0 or 1) If, for <tt>ns.1</tt> the value is <tt>same</tt>, 
    /// then <tt>ns.1</tt> a copy of whatever <tt>ns.0</tt> is.  Note that the default version has no <i>n</i>)</td></tr>
    /// <tr><td valign="top"/><i>base</i>.<tt>toss</tt><br/>
    /// <font size="-1">bool = <tt>true</tt> or <tt>false</tt> (default)</font>/td>
    /// <td valign="top">(after crossing over with the first new individual, should its second sibling individual be thrown 
    /// away instead of adding it to the population?)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// gp.koza.xover
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"/><i>base</i>.<tt>ns.</tt><i>n</i><br/>
    /// <td>nodeselect<i>n</i> (<i>n</i> is 0 or 1)</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.koza.CrossoverPipeline")]
    public class CrossoverPipeline : GPBreedingPipeline
    {
        #region Constants

        private const long SerialVersionUID = 1;

        public const string P_NUM_TRIES = "tries";
        public const string P_MAXDEPTH = "maxdepth";
        public const string P_MAXSIZE = "maxsize";
        public const string P_CROSSOVER = "xover";
        public const string P_TOSS = "toss";
        public const string KEY_PARENTS = "parents";

        public const int INDS_PRODUCED = 2;
        public const int NUM_SOURCES = 2;
        public const int NO_SIZE_LIMIT = -1;

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase => GPKozaDefaults.ParamBase.Push(P_CROSSOVER);

        public override int NumSources => NUM_SOURCES;

        /// <summary>
        /// How the pipeline selects a node from individual 1 
        /// </summary>
        public IGPNodeSelector NodeSelect1 { get; set; }

        /// <summary>
        /// How the pipeline selects a node from individual 2 
        /// </summary>
        public IGPNodeSelector NodeSelect2 { get; set; }

        /// <summary>
        /// Is the first tree fixed?  If not, this is -1 
        /// </summary>
        public int Tree1 { get; set; }

        /// <summary>
        /// Is the second tree fixed?  If not, this is -1 
        /// </summary>
        public int Tree2 { get; set; }

        /// <summary>
        /// How many times the pipeline attempts to pick nodes until it gives up. 
        /// </summary>
        public int NumTries { get; set; }

        /// <summary>
        /// The deepest tree the pipeline is allowed to form.  Single terminal trees are depth 1. 
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// The largest tree (measured as a nodecount) the pipeline is allowed to form.
        /// </summary>
        public int MaxSize { get; set; }

        /// <summary>
        /// Should the pipeline discard the second parent after crossing over? 
        /// </summary>
        public bool TossSecondParent { get; set; }

        /// <summary>
        /// Temporary holding place for parents 
        /// </summary>
        public IList<Individual> Parents { get; set; } = new List<Individual>();

        /// <summary>
        /// Returns 2 * minimum number of typical individuals produced by any sources, 
        /// else 1 * minimum number if TossSecondParent is true. 
        /// </summary>
        public override int TypicalIndsProduced => TossSecondParent ? MinChildProduction : MinChildProduction * 2;

        #endregion // Properties
        #region Cloning

        public override object Clone()
        {
            var c = (CrossoverPipeline) (base.Clone());
            
            // deep-cloned stuff
            c.NodeSelect1 = (IGPNodeSelector) NodeSelect1.Clone();
            c.NodeSelect2 = (IGPNodeSelector) NodeSelect2.Clone();
            c.Parents = Parents.ToList();
            
            return c;
        }

        #endregion // Cloning
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            
            var def = DefaultBase;
            var p = paramBase.Push(P_NODESELECTOR).Push("0");
            var d = def.Push(P_NODESELECTOR).Push("0");
            
            NodeSelect1 = (IGPNodeSelector) (state.Parameters.GetInstanceForParameter(p, d, typeof(IGPNodeSelector)));
            NodeSelect1.Setup(state, p);
            
            p = paramBase.Push(P_NODESELECTOR).Push("1");
            d = def.Push(P_NODESELECTOR).Push("1");
            
            if (state.Parameters.ParameterExists(p, d) && state.Parameters.GetString(p, d).Equals(V_SAME))
            // can't just copy it this time; the selectors
            // use internal caches.  So we have to clone it no matter what
                NodeSelect2 = (IGPNodeSelector) (NodeSelect1.Clone());
            else
            {
                NodeSelect2 = (IGPNodeSelector) (state.Parameters.GetInstanceForParameter(p, d, typeof(IGPNodeSelector)));
                NodeSelect2.Setup(state, p);
            }
            
            NumTries = state.Parameters.GetInt(paramBase.Push(P_NUM_TRIES), def.Push(P_NUM_TRIES), 1);
            if (NumTries == 0)
                state.Output.Fatal("GPCrossover Pipeline has an invalid number of tries (it must be >= 1).", 
                                                            paramBase.Push(P_NUM_TRIES), def.Push(P_NUM_TRIES));
            
            MaxDepth = state.Parameters.GetInt(paramBase.Push(P_MAXDEPTH), def.Push(P_MAXDEPTH), 1);
            if (MaxDepth == 0)
                state.Output.Fatal("GPCrossover Pipeline has an invalid maximum depth (it must be >= 1).", 
                                                            paramBase.Push(P_MAXDEPTH), def.Push(P_MAXDEPTH));

            MaxSize = NO_SIZE_LIMIT;
            if (state.Parameters.ParameterExists(paramBase.Push(P_MAXSIZE), def.Push(P_MAXSIZE)))
            {
                MaxSize = state.Parameters.GetInt(paramBase.Push(P_MAXSIZE), def.Push(P_MAXSIZE), 1);
                if (MaxSize < 1)
                    state.Output.Fatal("Maximum tree size, if defined, must be >= 1");
            }

            Tree1 = TREE_UNFIXED;
            if (state.Parameters.ParameterExists(paramBase.Push(P_TREE).Push("" + 0), def.Push(P_TREE).Push("" + 0)))
            {
                Tree1 = state.Parameters.GetInt(paramBase.Push(P_TREE).Push("" + 0), def.Push(P_TREE).Push("" + 0), 0);
                if (Tree1 == - 1)
                    state.Output.Fatal("Tree fixed value, if defined, must be >= 0");
            }
            
            Tree2 = TREE_UNFIXED;
            if (state.Parameters.ParameterExists(paramBase.Push(P_TREE).Push("" + 1), def.Push(P_TREE).Push("" + 1)))
            {
                Tree2 = state.Parameters.GetInt(paramBase.Push(P_TREE).Push("" + 1), def.Push(P_TREE).Push("" + 1), 0);
                if (Tree2 == -1)
                    state.Output.Fatal("Tree fixed value, if defined, must be >= 0");
            }
            TossSecondParent = state.Parameters.GetBoolean(paramBase.Push(P_TOSS), def.Push(P_TOSS), false);
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Returns true if inner1 can feasibly be swapped into inner2's position. 
        /// </summary>		
        public bool VerifyPoints(GPInitializer initializer, GPNode inner1, GPNode inner2)
        {
            // first check to see if inner1 is swap-compatible with inner2
            // on a type basis
            if (!inner1.SwapCompatibleWith(initializer, inner2))
                return false;

            // next check to see if inner1 can fit in inner2's spot
            if (inner1.Depth + inner2.AtDepth() > MaxDepth)
                return false;

            // check for size
            // NOTE: this is done twice, which is more costly than it should be.  But
            // on the other hand it allows us to toss a child without testing both times
            // and it's simpler to have it all here in the verifyPoints code.  
            if (MaxSize != NO_SIZE_LIMIT)
            {
                // first easy check
                var inner1Size = inner1.NumNodes(GPNode.NODESEARCH_ALL);
                var inner2Size = inner2.NumNodes(GPNode.NODESEARCH_ALL);
                if (inner1Size > inner2Size)  // need to test further
                {
                    // let's keep on going for the more complex test
                    var root2 = ((GPTree)(inner2.RootParent())).Child;
                    var root2Size = root2.NumNodes(GPNode.NODESEARCH_ALL);
                    if (root2Size - inner2Size + inner1Size > MaxSize)  // take root2, remove inner2 and swap in inner1.  Is it still small enough?
                        return false;
                }
            }

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

            // how many individuals should we make?
            var n = TypicalIndsProduced;
            if (n < min)
                n = min;
            if (n > max)
                n = max;

            // should we bother?
            if (!state.Random[thread].NextBoolean(Likelihood))
            {
                // just load from source 0 and clone 'em
                Sources[0].Produce(n, n, subpop, inds, state, thread, misc);
                return n;
            }

            IntBag[] parentparents = null;
            IntBag[] preserveParents = null;
            if (misc != null && misc[KEY_PARENTS] != null)
            {
                preserveParents = (IntBag[])misc[KEY_PARENTS];
                parentparents = new IntBag[2];
                misc[KEY_PARENTS] = parentparents;
            }

            var initializer = (GPInitializer)state.Initializer;

            for (var q = start; q < n + start; ) // keep on going until we're filled up
            {
                Parents.Clear();

                // grab two individuals from our sources
                if (Sources[0] == Sources[1])
                    // grab from the same source
                    Sources[0].Produce(2, 2, subpop, Parents, state, thread, misc);
                // grab from different sources
                else
                {
                    Sources[0].Produce(1, 1, subpop, Parents, state, thread, misc);
                    Sources[1].Produce(1, 1, subpop, Parents, state, thread, misc);
                }

                // at this point, Parents[] contains our two selected individuals

                // are our tree values valid?
                if (Tree1 != TREE_UNFIXED && (Tree1 < 0 || Tree1 >= ((GPIndividual)Parents[0]).Trees.Length))
                    // uh oh
                    state.Output.Fatal("GP Crossover Pipeline attempted to fix tree.0 to a value which was out of bounds"
                        + " of the array of the individual's trees.  Check the pipeline's fixed tree values"
                        + " -- they may be negative or greater than the number of trees in an individual");

                if (Tree2 != TREE_UNFIXED && (Tree2 < 0 || Tree2 >= ((GPIndividual)Parents[1]).Trees.Length))
                    // uh oh
                    state.Output.Fatal("GP Crossover Pipeline attempted to fix tree.1 to a value which was out of bounds"
                        + " of the array of the individual's trees.  Check the pipeline's fixed tree values"
                        + " -- they may be negative or greater than the number of trees in an individual");

                var t1 = 0;
                var t2 = 0;
                if (Tree1 == TREE_UNFIXED || Tree2 == TREE_UNFIXED)
                {
                    do
                    // pick random trees  -- their GPTreeConstraints must be the same
                    {
                        if (Tree1 == TREE_UNFIXED)
                            if (((GPIndividual)Parents[0]).Trees.Length > 1)
                                t1 = state.Random[thread].NextInt(((GPIndividual)Parents[0]).Trees.Length);
                            else
                                t1 = 0;
                        else
                            t1 = Tree1;

                        if (Tree2 == TREE_UNFIXED)
                            if (((GPIndividual)Parents[1]).Trees.Length > 1)
                                t2 = state.Random[thread].NextInt(((GPIndividual)Parents[1]).Trees.Length);
                            else
                                t2 = 0;
                        else
                            t2 = Tree2;
                    }
                    while (((GPIndividual)Parents[0]).Trees[t1].Constraints(initializer) != ((GPIndividual)Parents[1]).Trees[t2].Constraints(initializer));
                }
                else
                {
                    t1 = Tree1;
                    t2 = Tree2;
                    // make sure the constraints are okay
                    if (((GPIndividual)Parents[0]).Trees[t1].Constraints(initializer) != ((GPIndividual)Parents[1]).Trees[t2].Constraints(initializer))
                        // uh oh
                        state.Output.Fatal("GP Crossover Pipeline's two tree choices are both specified by the user"
                                                                + " -- but their GPTreeConstraints are not the same");
                }

                // validity results...
                var res1 = false;
                var res2 = false;

                // prepare the nodeselectors
                NodeSelect1.Reset();
                NodeSelect2.Reset();

                // pick some nodes

                GPNode p1 = null;
                GPNode p2 = null;

                for (var x = 0; x < NumTries; x++)
                {
                    // pick a node in individual 1
                    p1 = NodeSelect1.PickNode(state, subpop, thread, (GPIndividual)Parents[0], ((GPIndividual)Parents[0]).Trees[t1]);

                    // pick a node in individual 2
                    p2 = NodeSelect2.PickNode(state, subpop, thread, (GPIndividual)Parents[1], ((GPIndividual)Parents[1]).Trees[t2]);

                    // check for depth and swap-compatibility limits
                    res1 = VerifyPoints(initializer, p2, p1); // p2 can fill p1's spot -- order is important!
                    if (n - (q - start) < 2 || TossSecondParent)
                        res2 = true;
                    else
                        res2 = VerifyPoints(initializer, p1, p2); // p1 can fill p2's spot -- order is important!

                    // did we get something that had both nodes verified?
                    // we reject if EITHER of them is invalid.  This is what lil-gp does.
                    // Koza only has NumTries set to 1, so it's compatible as well.
                    if (res1 && res2)
                        break;
                }

                // at this point, res1 AND res2 are valid, OR either res1
                // OR res2 is valid and we ran out of tries, OR neither is
                // valid and we ran out of tries.  So now we will transfer
                // to a tree which has res1 or res2 valid, otherwise it'll
                // just get replicated.  This is compatible with both Koza
                // and lil-gp. 

                // at this point I could check to see if my sources were breeding
                // pipelines -- but I'm too lazy to write that code (it's a little
                // complicated) to just swap one individual over or both over,
                // -- it might still entail some copying.  Perhaps in the future.
                // It would make things faster perhaps, not requiring all that
                // cloning.

                // Create some new individuals based on the old ones -- since
                // GPTree doesn't deep-clone, this should be just fine.  Perhaps we
                // should change this to proto off of the main species prototype, but
                // we have to then copy so much stuff over; it's not worth it.

                var j1 = ((GPIndividual)Parents[0]).LightClone();
                GPIndividual j2 = null;

                if (n - (q - start) >= 2 && !TossSecondParent)
                    j2 = ((GPIndividual)Parents[1]).LightClone();

                // Fill in various tree information that didn't get filled in there
                j1.Trees = new GPTree[((GPIndividual)Parents[0]).Trees.Length];
                if (n - (q - start) >= 2 && !TossSecondParent)
                    j2.Trees = new GPTree[((GPIndividual)Parents[1]).Trees.Length];

                // at this point, p1 or p2, or both, may be null.
                // If not, swap one in.  Else just copy the parent.

                for (var x = 0; x < j1.Trees.Length; x++)
                {
                    if (x == t1 && res1)
                    // we've got a tree with a kicking cross position!
                    {
                        j1.Trees[x] = ((GPIndividual)Parents[0]).Trees[x].LightClone();
                        j1.Trees[x].Owner = j1;
                        j1.Trees[x].Child = ((GPIndividual)Parents[0]).Trees[x].Child.CloneReplacing(p2, p1);
                        j1.Trees[x].Child.Parent = j1.Trees[x];
                        j1.Trees[x].Child.ArgPosition = 0;
                        j1.Evaluated = false;
                    }
                    // it's changed
                    else
                    {
                        j1.Trees[x] = ((GPIndividual)Parents[0]).Trees[x].LightClone();
                        j1.Trees[x].Owner = j1;
                        j1.Trees[x].Child = (GPNode)((GPIndividual)Parents[0]).Trees[x].Child.Clone();
                        j1.Trees[x].Child.Parent = j1.Trees[x];
                        j1.Trees[x].Child.ArgPosition = 0;
                    }
                }

                if (n - (q - start) >= 2 && !TossSecondParent)
                    for (var x = 0; x < j2.Trees.Length; x++)
                    {
                        if (x == t2 && res2)
                        // we've got a tree with a kicking cross position!
                        {
                            j2.Trees[x] = ((GPIndividual)Parents[1]).Trees[x].LightClone();
                            j2.Trees[x].Owner = j2;
                            j2.Trees[x].Child = ((GPIndividual)Parents[1]).Trees[x].Child.CloneReplacing(p1, p2);
                            j2.Trees[x].Child.Parent = j2.Trees[x];
                            j2.Trees[x].Child.ArgPosition = 0;
                            j2.Evaluated = false;
                        }
                        // it's changed
                        else
                        {
                            j2.Trees[x] = ((GPIndividual)Parents[1]).Trees[x].LightClone();
                            j2.Trees[x].Owner = j2;
                            j2.Trees[x].Child = (GPNode)((GPIndividual)Parents[1]).Trees[x].Child.Clone();
                            j2.Trees[x].Child.Parent = j2.Trees[x];
                            j2.Trees[x].Child.ArgPosition = 0;
                        }
                    }

                // add the individuals to the population
                inds.Add(j1);
                q++;
                if (q < n + start && !TossSecondParent)
                {
                    inds.Add(j2);
                    if (preserveParents != null)
                    {
                        parentparents[0].AddAll(parentparents[1]);
                        preserveParents[q] = parentparents[0];
                    }
                    q++;
                }
            }
            return n;
        }

        #endregion // Operations
    }
}