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

namespace BraneCloud.Evolution.EC.GP.Breed
{
    /// <summary> 
    /// MutateERCPipeline works very similarly to the "Gaussian" algorithm described in Kumar Chellapilla,
    /// "A Preliminary Investigation into Evolving Modular Programs without Subtree Crossover", GP98.
    /// 
    /// <p/>MutateERCPipeline picks a random node from a random tree in the individual,
    /// using its node selector.  It then proceeds to "mutate" every ERC (ephemeral
    /// random constant) located in the subtree rooted at that node.  It does this
    /// by calling each ERC's <tt>mutateERC()</tt> method.  The default form of <tt>mutateERC()</tt>
    /// method is to simply call <tt>resetNode()</tt>, thus randomizing the ERC;
    /// you may want to override this default to provide more useful mutations,
    /// such as adding gaussian noise.
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
    /// gp.breed.mutate-erc
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"/><i>base</i>.<tt>ns</tt><br/>
    /// <td>The GPNodeSelector selector</td></tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.breed.MutateERCPipeline")]
    public class MutateERCPipeline : GPBreedingPipeline
    {
        #region Constants

        public const string P_MUTATEERC = "mutate-erc";
        public const int NUM_SOURCES = 1;

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return GPBreedDefaults.ParamBase.Push(P_MUTATEERC); }
        }

        /// <summary>
        /// How the pipeline chooses a subtree to mutate 
        /// </summary>
        public IGPNodeSelector NodeSelect { get; set; }

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

            var p = paramBase.Push(P_NODESELECTOR).Push("" + 0);
            var def = DefaultBase;

            NodeSelect = (IGPNodeSelector)
                (state.Parameters.GetInstanceForParameter(p, def.Push(P_NODESELECTOR).Push("" + 0), typeof(IGPNodeSelector)));

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

        public void MutateERCs(GPNode node, IEvolutionState state, int thread)
        {
            // is node an erc?
            if (node is ERC)
                ((ERC)node).MutateERC(state, thread);

            // mutate children
            foreach (var t in node.Children)
                MutateERCs(t, state, thread);
        }

        public override int Produce(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            // grab n individuals from our source and stick 'em right into inds.
            // we'll modify them from there
            var n = Sources[0].Produce(min, max, start, subpop, inds, state, thread);

            // should we bother?
            if (!state.Random[thread].NextBoolean(Likelihood))
                return Reproduce(n, start, subpop, inds, state, thread, false);  // DON'T produce children from source -- we already did

            // now let's mutate 'em
            for (var q = start; q < n + start; q++)
            {
                var i = (GPIndividual)inds[q];

                if (Tree != TREE_UNFIXED && (Tree < 0 || Tree >= i.Trees.Length))
                    // uh oh
                    state.Output.Fatal("MutateERCPipeline attempted to fix tree.0 to a value which was"
                                    + " out of bounds of the array of the individual's trees. "
                                    + " Check the pipeline's fixed tree values -- they may be"
                                    + " negative or greater than the number of trees in an individual");

                int t;
                // pick random tree
                if (Tree == TREE_UNFIXED)
                    t = i.Trees.Length > 1 ? state.Random[thread].NextInt(i.Trees.Length) : 0;
                else
                    t = Tree;

                GPIndividual j;
                if (Sources[0] is BreedingPipeline)
                // it's already a copy, so just smash the tree in
                {
                    j = i;
                }
                // need to copy it
                else
                {
                    j = i.LightClone();

                    // Fill in various tree information that didn't get filled in there
                    j.Trees = new GPTree[i.Trees.Length];

                    for (var x = 0; x < j.Trees.Length; x++)
                    {
                        j.Trees[x] = i.Trees[x].LightClone();
                        j.Trees[x].Owner = j;
                        j.Trees[x].Child = (GPNode)i.Trees[x].Child.Clone();
                        j.Trees[x].Child.Parent = j.Trees[x];
                        j.Trees[x].Child.ArgPosition = 0;
                    }
                }
                j.Evaluated = false;

                // prepare the NodeSelector
                NodeSelect.Reset();

                // Now pick a random node

                var p = NodeSelect.PickNode(state, subpop, thread, j, j.Trees[t]);

                // mutate all the ERCs in p1's subtree

                MutateERCs(p, state, thread);

                // add the new individual, replacing its previous source
                inds[q] = j;
            }
            return n;
        }

        #endregion // Operations
        #region Cloning

        public override object Clone()
        {
            var c = (MutateERCPipeline) (base.Clone());
            
            // deep-cloned stuff
            c.NodeSelect = (IGPNodeSelector) (NodeSelect.Clone());
            return c;
        }

        #endregion // Cloning
    }
}