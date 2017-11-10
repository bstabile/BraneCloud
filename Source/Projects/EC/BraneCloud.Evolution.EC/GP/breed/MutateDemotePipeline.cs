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
    /// MutateDemotePipeline works very similarly to the DemoteNode algorithm described in  Kumar Chellapilla,
    /// "A Preliminary Investigation into Evolving Modular Programs without Subtree Crossover", GP98, 
    /// and is also similar to the "insertion" operator found in Una-May O'Reilly's thesis,
    /// <a href="http://www.ai.mit.edu/people/unamay/thesis.html"> "An Analysis of Genetic Programming"</a>.
    /// 
    /// <p/>MutateDemotePipeline tries picks a random tree, then picks
    /// randomly from all the demotable nodes in the tree, and demotes one.  
    /// If its chosen tree has no demotable nodes, or demoting
    /// its chosen demotable node would make the tree too deep, it repeats
    /// the choose-tree-then-choose-node process.  If after <i>tries</i> times
    /// it has failed to find a valid tree and demotable node, it gives up and simply
    /// copies the individual.
    /// 
    /// <p/>"Demotion" means to take a node <i>n</i> and insert a new node <i>m</i>
    /// between <i>n</i> and <i>n</i>'s parent.  <i>n</i> becomes a child of
    /// <i>m</i>; the place where it becomes a child is determined at random
    /// from all the type-compatible slots of <i>m</i>.  The other child slots
    /// of <i>m</i> are filled with randomly-generated terminals.  
    /// Chellapilla's version of the algorithm always
    /// places <i>n</i> in child slot 0 of <i>m</i>.  Because this would be
    /// unneccessarily restrictive on strong typing, MutateDemotePipeline instead
    /// picks the slot at random from all available valid choices.
    /// 
    /// <p/>A "Demotable" node means a node which is capable of demotion
    /// given the existing function set.  In general to demote a node <i>foo</i>,
    /// there must exist in the function set a nonterminal whose return type
    /// is type-compatible with the child slot <i>foo</i> holds in its parent;
    /// this nonterminal must also have a child slot which is type-compatible
    /// with <i>foo</i>'s return type.
    /// 
    /// <p/>This method is very expensive in searching nodes for
    /// "demotability".  However, if the number of types is 1 (the
    /// GP run is typeless) then the type-constraint-checking
    /// code is bypassed and the method runs a little faster.
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
    /// <td valign="top">(maximum valid depth of a mutated tree)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>tree.0</tt><br/>
    /// <font size="-1">0 &lt; int &lt; (num trees in individuals), if exists</font></td>
    /// <td valign="top">(tree chosen for mutation; if parameter doesn't exist, tree is picked at random)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// gp.breed.mutate-demote
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.breed.MutateDemotePipeline")]
    public class MutateDemotePipeline : GPBreedingPipeline
    {
        #region Constants

        public const string P_MUTATEDEMOTE = "mutate-demote";
        public const string P_NUM_TRIES = "tries";
        public const string P_MAXDEPTH = "maxdepth";
        public const int NUM_SOURCES = 1;

        #endregion // Constants
        #region Fields

        private GPNode _demotableNode;

        #endregion // Fields
        #region Properties

        public override IParameter DefaultBase => GPBreedDefaults.ParamBase.Push(P_MUTATEDEMOTE); 

        /// <summary>
        /// The number of times the pipeline tries to build a valid mutated
        /// tree before it gives up and just passes on the original 
        /// </summary>
        public int NumTries { get; set; }

        /// <summary>
        /// The maximum depth of a mutated tree 
        /// </summary>
        public int MaxDepth { get; set; }

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
                state.Output.Fatal("MutateDemotePipeline has an invalid number of tries (it must be >= 1).",
                    paramBase.Push(P_NUM_TRIES), def.Push(P_NUM_TRIES));

            MaxDepth = state.Parameters.GetInt(paramBase.Push(P_MAXDEPTH), def.Push(P_MAXDEPTH), 1);
            if (MaxDepth == 0)
            {
                state.Output.Fatal("The MutateDemotePipeline " + paramBase + "has an invalid maximum depth (it must be >= 1).",
                    paramBase.Push(P_MAXDEPTH), def.Push(P_MAXDEPTH));
            }

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

        private static bool Demotable(GPInitializer initializer, GPNode node, GPFunctionSet funcs)
        {
            GPType t;

            if (node.Parent is GPNode)
                // ugh, expensive
                t = ((GPNode)(node.Parent)).Constraints(initializer).ChildTypes[node.ArgPosition];
            else
                t = ((GPTree)(node.Parent)).Constraints(initializer).TreeType;

            // Now, out of the nonterminals compatible with that return type,
            // do any also have a child compatible with that return type?  This
            // will be VERY expensive

            for (var x = 0; x < funcs.Nonterminals[t.Type].Length; x++)
                if (funcs.Nonterminals[t.Type][x].Constraints(initializer).ChildTypes
                    .Any(t1 => t1.CompatibleWith(initializer, node.Constraints(initializer).ReturnType)))
                {
                    return true;
                }
            return false;
        }

        private static void DemoteSomething(GPNode node, IEvolutionState state, int thread, GPFunctionSet funcs)
        {
            // if I have just one type, do it the easy way
            if (((GPInitializer)state.Initializer).NumAtomicTypes + ((GPInitializer)state.Initializer).NumSetTypes == 1)
                DemoteSomethingTypeless(node, state, thread, funcs);
            // otherwise, I gotta do the dirty work
            else
                DemoteSomethingDirtyWork(node, state, thread, funcs);
        }

        private static void DemoteSomethingTypeless(GPNode node, IEvolutionState state, int thread, GPFunctionSet funcs)
        {
            var numDemotable = 0;

            // since we're typeless, we can demote under any nonterminal
            numDemotable = funcs.Nonterminals[0].Length;

            // pick a random item to demote -- numDemotable is assumed to be > 0
            var demoteItem = state.Random[thread].NextInt(numDemotable);

            numDemotable = 0;
            // find it

            // clone the node
            var cnode = funcs.Nonterminals[0][demoteItem].LightClone();

            var chityp = cnode.Constraints(((GPInitializer)state.Initializer)).ChildTypes;

            // choose a spot to hang the old parent under
            var choice = state.Random[thread].NextInt(cnode.Children.Length);

            for (var z = 0; z < cnode.Children.Length; z++)
                if (z == choice)
                {
                    // demote the parent, inserting cnode
                    cnode.Parent = node.Parent;
                    cnode.ArgPosition = node.ArgPosition;
                    cnode.Children[z] = node;
                    node.Parent = cnode;
                    node.ArgPosition = (sbyte)z;
                    if (cnode.Parent is GPNode)
                        ((GPNode)(cnode.Parent)).Children[cnode.ArgPosition] = cnode;
                    else
                        ((GPTree)(cnode.Parent)).Child = cnode;
                }
                else
                {
                    // hang a randomly-generated terminal off of cnode
                    var term = funcs.Terminals
                        [chityp[z].Type][state.Random[thread].NextInt(funcs.Terminals[chityp[z].Type].Length)].LightClone();

                    cnode.Children[z] = term;
                    term.Parent = cnode; // just in case
                    term.ArgPosition = (sbyte)z; // just in case
                    term.ResetNode(state, thread); // let it randomize itself if necessary
                }
        }

        private static void DemoteSomethingDirtyWork(GPNode node, IEvolutionState state, int thread, GPFunctionSet funcs)
        {
            var numDemotable = 0;

            GPType t;
            var initializer = ((GPInitializer)state.Initializer);

            if (node.Parent is GPNode)
                // ugh, expensive
                t = ((GPNode)(node.Parent)).Constraints(initializer).ChildTypes[node.ArgPosition];
            else
                t = ((GPTree)(node.Parent)).Constraints(initializer).TreeType;

            // Now, determine how many nodes we can demote this under --
            // note this doesn't select based on the total population
            // of "available child positions", but on the total population
            // of *nodes* regardless of if they have more than one possible
            // valid "child position".

            for (var x = 0; x < funcs.Nonterminals[t.Type].Length; x++)
                if (funcs.Nonterminals[t.Type][x].Constraints(initializer).ChildTypes
                    .Any(t1 => t1.CompatibleWith(initializer, node.Constraints(initializer).ReturnType)))
                {
                    numDemotable++;
                }

            // pick a random item to demote -- numDemotable is assumed to be > 0
            var demoteItem = state.Random[thread].NextInt(numDemotable);

            numDemotable = 0;
            // find it

            for (var x = 0; x < funcs.Nonterminals[t.Type].Length; x++)
                if (funcs.Nonterminals[t.Type][x].Constraints(initializer).ChildTypes
                    .Any(t1 => t1.CompatibleWith(initializer, node.Constraints(initializer).ReturnType)))
                {
                    if (numDemotable == demoteItem)
                    {
                        // clone the node
                        var cnode = funcs.Nonterminals[t.Type][x].LightClone();

                        // choose a spot to hang the old parent under
                        var retyp = node.Constraints(initializer).ReturnType;
                        var chityp = cnode.Constraints(initializer).ChildTypes;

                        var numSpots = cnode.Children.Where((t1, z) => chityp[z].CompatibleWith(initializer, retyp)).Count();
                        var choice = state.Random[thread].NextInt(numSpots);

                        numSpots = 0;
                        for (var z = 0; z < cnode.Children.Length; z++)
                            if (chityp[z].CompatibleWith(initializer, retyp))
                            {
                                if (numSpots == choice)
                                {
                                    // demote the parent, inserting cnode
                                    cnode.Parent = node.Parent;
                                    cnode.ArgPosition = node.ArgPosition;
                                    cnode.Children[z] = node;
                                    node.Parent = cnode;
                                    node.ArgPosition = (sbyte)z;
                                    if (cnode.Parent is GPNode)
                                        ((GPNode)(cnode.Parent)).Children[cnode.ArgPosition] = cnode;
                                    else
                                        ((GPTree)(cnode.Parent)).Child = cnode;

                                    // this is important to ensure that the
                                    // demotion only happens once!  Otherwise
                                    // you'll get really nasty bugs
                                    numSpots++; // notice no break
                                }
                                else
                                {
                                    // hang a randomly-generated terminal off of cnode
                                    var term = funcs.Terminals
                                        [chityp[z].Type][state.Random[thread].NextInt(funcs.Terminals[chityp[z].Type].Length)].LightClone();

                                    cnode.Children[z] = term;
                                    term.Parent = cnode; // just in case
                                    term.ArgPosition = (sbyte)z; // just in case
                                    term.ResetNode(state, thread); // let it randomize itself if necessary

                                    // increase numSpots
                                    numSpots++; // notice no break
                                }
                            }
                            else
                            {
                                // hang a randomly-generated terminal off of cnode
                                var term = funcs.Terminals
                                    [chityp[z].Type][state.Random[thread].NextInt(funcs.Terminals[chityp[z].Type].Length)].LightClone();

                                cnode.Children[z] = term;
                                term.Parent = cnode; // just in case
                                term.ArgPosition = (sbyte)z; // just in case
                                term.ResetNode(state, thread); // let it randomize itself if necessary
                            }
                        return;
                    }
                    numDemotable++;
                }
            // should never reach here
            throw new ApplicationException("Bug in demoteSomething -- should never be able to reach the end of the function");
        }

        private static int NumDemotableNodes(GPInitializer initializer, GPNode root, int soFar, GPFunctionSet funcs)
        {
            // if I have just one type, skip this and just return
            // the number of nonterminals in the tree
            if (initializer.NumAtomicTypes + initializer.NumSetTypes == 1)
                return root.NumNodes(GPNode.NODESEARCH_ALL);

            // otherwise, I gotta do the dirty work
            return NumDemotableNodesDirtyWork(initializer, root, soFar, funcs);
        }

        private static int NumDemotableNodesDirtyWork(GPInitializer initializer, GPNode root, int soFar, GPFunctionSet funcs)
        {
            if (Demotable(initializer, root, funcs))
                soFar++;
            return root.Children.Aggregate(soFar, (current, t) => NumDemotableNodesDirtyWork(initializer, t, current, funcs));
        }

        private int PickDemotableNode(GPInitializer initializer, GPNode root, int num, GPFunctionSet funcs)
        {
            // if I have just one type, skip this and just 
            // the num-th nonterminal
            if (initializer.NumAtomicTypes + initializer.NumSetTypes == 1)
            {
                _demotableNode = root.NodeInPosition(num, GPNode.NODESEARCH_ALL);
                return -1; // what PickDemotableNodeDirtyWork() returns...
            }

            // otherwise, I gotta do the dirty work
            return PickDemotableNodeDirtyWork(initializer, root, num, funcs);
        }

        /// <summary>
        /// sticks the node in
        /// </summary>
        private int PickDemotableNodeDirtyWork(GPInitializer initializer, GPNode root, int num, GPFunctionSet funcs)
        {
            if (Demotable(initializer, root, funcs))
            {
                num--;
                if (num == -1)
                // found it
                {
                    _demotableNode = root;
                    return num;
                }
            }
            foreach (var t in root.Children)
            {
                num = PickDemotableNodeDirtyWork(initializer, t, num, funcs);
                if (num == -1)
                    break; // someone found it
            }
            return num;
        }

        /// <summary>
        /// Returns true if inner1's depth + atdepth +1 is within the depth bounds 
        /// </summary>
        private bool VerifyPoint(GPNode inner1)
        {
            // We know they're swap-compatible since we generated inner1
            // to be exactly that.  So don't bother.

            // next check to see if inner1 can be demoted
            if (inner1.Depth + inner1.AtDepth() + 1 > MaxDepth)
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
                    state.Output.Fatal("MutateDemotePipeline attempted to fix tree.0 to a value"
                        + " which was out of bounds of the array of the individual's trees. "
                        + " Check the pipeline's fixed tree values -- they may be negative"
                        + " or greater than the number of trees in an individual");

                for (var x = 0; x < NumTries; x++)
                {
                    int t;
                    // pick random tree
                    if (Tree == TREE_UNFIXED)
                        if (i.Trees.Length > 1)
                            t = state.Random[thread].NextInt(i.Trees.Length);
                        else
                            t = 0;
                    else
                        t = Tree;

                    // is the tree demotable?
                    int numdemote = NumDemotableNodes(initializer, i.Trees[t].Child, 0, i.Trees[t].Constraints(initializer).FunctionSet);
                    if (numdemote == 0)
                        continue; // uh oh, try again

                    // demote the node, or if we're unsuccessful, just leave it alone
                    PickDemotableNode(initializer, i.Trees[t].Child, state.Random[thread].NextInt(numdemote),
                                                   i.Trees[t].Constraints(initializer).FunctionSet);

                    // does this node exceed the maximum depth limits?
                    if (!VerifyPoint(_demotableNode))
                        continue; // uh oh, try again

                    // demote it
                    DemoteSomething(_demotableNode, state, thread, i.Trees[t].Constraints(initializer).FunctionSet);
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