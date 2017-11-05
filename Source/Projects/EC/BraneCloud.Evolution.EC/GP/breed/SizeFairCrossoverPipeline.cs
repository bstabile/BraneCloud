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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.MultiObjective;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.GP.Breed
{
    /**
         * SizeFairCrossover works similarly to one written in the paper
         * "Size Fair and Homologous Tree Genetic Programming Crossovers" by Langdon (1998). 
         
         * <p>SizeFairCrossover tries <i>tries</i> times to find a tree
         * that has at least one fair size node based on size fair or Homologous 
         * implementation.  If it cannot
         * find a valid tree in <i>tries</i> times, it gives up and simply
         * copies the individual.
         
         * <p>This pipeline typically produces up to 2 new individuals (the two newly-
         * swapped individuals) per produce(...) call.  If the system only
         * needs a single individual, the pipeline will throw one of the
         * new individuals away.  The user can also have the pipeline always
         * throw away the second new individual instead of adding it to the population.
         * In this case, the pipeline will only typically 
         * produce 1 new individual per produce(...) call.
         
         <p><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br>
         2 * minimum typical number of individuals produced by each source, unless TossSecondParent
         is set, in which case it's simply the minimum typical number.

         <p><b>Number of Sources</b><br>
         2

         <p><b>Parameters</b><br>
         <table>
         <tr><td valign=top><i>base</i>.<tt>tries</tt><br>
         <font size=-1>int &gt;= 1</font></td>
         <td valign=top>(number of times to try finding valid pairs of nodes)</td></tr>

         <tr><td valign=top><i>base</i>.<tt>maxdepth</tt><br>
         <font size=-1>int &gt;= 1</font></td>
         <td valign=top>(maximum valid depth of a crossed-over subtree)</td></tr>
         
         <tr><td valign=top><i>base</i>.<tt>tree.0</tt><br>
         <font size=-1>0 &lt; int &lt; (num trees in individuals), if exists</font></td>
         <td valign=top>(first tree for the crossover; if parameter doesn't exist, tree is picked at random)</td></tr>

         <tr><td valign=top><i>base</i>.<tt>tree.1</tt><br>
         <font size=-1>0 &lt; int &lt; (num trees in individuals), if exists</font></td>
         <td valign=top>(second tree for the crossover; if parameter doesn't exist, tree is picked at random.  This tree <b>must</b> have the same GPTreeConstraints as <tt>tree.0</tt>, if <tt>tree.0</tt> is defined.)</td></tr>

         <tr><td valign=top><i>base</i>.<tt>ns.</tt><i>n</i><br>
         <font size=-1>classname, inherits and != GPNodeSelector,<br>
         or String <tt>same<tt></font></td>
         <td valign=top>(GPNodeSelector for parent <i>n</i> (n is 0 or 1) If, for <tt>ns.1</tt> the value is <tt>same</tt>, then <tt>ns.1</tt> a copy of whatever <tt>ns.0</tt> is.  Note that the default version has no <i>n</i>)</td></tr>

         <tr><td valign=top><i>base</i>.<tt>toss</tt><br>
         <font size=-1>bool = <tt>true</tt> or <tt>false</tt> (default)</font>/td>
         <td valign=top>(after crossing over with the first new individual, should its second sibling individual be thrown away instead of adding it to the population?)</td></tr>
         
         <tr><td valign=top><i>base</i>.<tt>Homologous</tt><br>
         <font size=-1>bool = <tt>true</tt> or <tt>false</tt> (default)</font>/td>
         <td valign=top>(Is the implementation Homologous (as opposed to size-fair)?)</td></tr>
         </table>

         <p><b>Default Base</b><br>
         gp.breed.size-fair

         <p><b>Parameter bases</b><br>
         <table>
         <tr><td valign=top><i>base</i>.<tt>ns.</tt><i>n</i><br>
         <td>nodeselect<i>n</i> (<i>n</i> is 0 or 1)</td></tr>

         </table>

         * @author Uday Kamath and Sean Luke
         * @version 1.0 
         */

    [ECConfiguration("ec.gp.breed.SizeFairCrossoverPipeline")]
    public class SizeFairCrossoverPipeline : GPBreedingPipeline
    {
        #region Constants

        private const long serialVersionUID = 1;

        public const String P_NUM_TRIES = "tries";
        public const String P_MAXDEPTH = "maxdepth";
        public const String P_SIZEFAIR = "size-fair";
        public const String P_TOSS = "toss";
        public const String P_HOMOLOGOUS = "homologous";
        public const int INDS_PRODUCED = 2;
        public const int NUM_SOURCES = 2;

        #endregion // Constants

        #region Properties

        /** How the pipeline selects a node from individual 1 */
        public IGPNodeSelector NodeSelect1 { get; set; }

        /** How the pipeline selects a node from individual 2 */
        public IGPNodeSelector NodeSelect2 { get; set; }

        /** Is the first tree fixed? If not, this is -1 */
        public int Tree1 { get; set; }

        /** Is the second tree fixed? If not, this is -1 */
        public int Tree2 { get; set; }

        /** How many times the pipeline attempts to pick nodes until it gives up. */
        public int NumTries { get; set; }

        /**
         * The deepest tree the pipeline is allowed to form. Single terminal trees
         * are depth 1.
         */
        public int MaxDepth { get; set; }

        /** Should the pipeline discard the second parent after crossing over? */
        public bool TossSecondParent { get; set; }

        /** Temporary holding place for Parents */
        public GPIndividual[] Parents { get; set; }

        public bool Homologous { get; set; }

        public override IParameter DefaultBase => GPBreedDefaults.ParamBase.Push(P_SIZEFAIR);

        public override int NumSources => NUM_SOURCES;

        #endregion // Properties

        public SizeFairCrossoverPipeline()
        {
            Parents = new GPIndividual[2];
        }



        public override Object Clone()
        {
            SizeFairCrossoverPipeline c = (SizeFairCrossoverPipeline) (base.Clone());

            // deep-cloned stuff
            c.NodeSelect1 = (IGPNodeSelector) (NodeSelect1.Clone());
            c.NodeSelect2 = (IGPNodeSelector) (NodeSelect2.Clone());
            c.Parents = (GPIndividual[]) Parents.Clone();

            return c;
        }

        public void Setup(IEvolutionState state, Parameter paramBase)
        {
            base.Setup(state, paramBase);

            IParameter def = DefaultBase;
            IParameter p = paramBase.Push(P_NODESELECTOR).Push("0");
            IParameter d = def.Push(P_NODESELECTOR).Push("0");

            NodeSelect1 = (IGPNodeSelector) state.Parameters.GetInstanceForParameter(p, d, typeof(IGPNodeSelector));
            NodeSelect1.Setup(state, p);

            p = paramBase.Push(P_NODESELECTOR).Push("1");
            d = def.Push(P_NODESELECTOR).Push("1");

            if (state.Parameters.ParameterExists(p, d) && state.Parameters.GetString(p, d).Equals(V_SAME))
            {
                // can't just copy it this time; the selectors
                // use internal caches. So we have to clone it no matter what
                NodeSelect2 = (IGPNodeSelector) NodeSelect1.Clone();
            }
            else
            {
                NodeSelect2 = (IGPNodeSelector) state.Parameters.GetInstanceForParameter(p, d, typeof(IGPNodeSelector));
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
            TossSecondParent = state.Parameters.GetBoolean(paramBase.Push(P_TOSS), def.Push(P_TOSS), false);
            if (state.Parameters.ParameterExists(paramBase.Push(P_HOMOLOGOUS), null))
            {
                //get the parameter
                Homologous = state.Parameters.GetBoolean(paramBase.Push(P_HOMOLOGOUS), null, false);
            }
        }

        /**
         * Returns 2 * minimum number of typical individuals produced by any
         * sources, else 1* minimum number if TossSecondParent is true.
         */
        public override int TypicalIndsProduced => TossSecondParent ? MinChildProduction : MinChildProduction * 2;
        

        /** Returns true if inner1 can feasibly be swapped into inner2's position. */

        public bool VerifyPoints(GPInitializer initializer,
            GPNode inner1, GPNode inner2)
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

        public override int Produce(int min, int max, int start,
            int subpopulation, Individual[] inds,
            IEvolutionState state, int thread)
        {
            // how many individuals should we make?
            int n = TypicalIndsProduced;
            if (n < min)
                n = min;
            if (n > max)
                n = max;

            // should we bother?
            if (!state.Random[thread].NextBoolean(Likelihood))
                return Reproduce(n, start, subpopulation, inds, state, thread,
                    true); // DO produce children from source -- we've not done so already

            GPInitializer initializer = (GPInitializer) state.Initializer;

            for (int q = start; q < n + start; /* no increment */) // keep on going until we're filled up
            {
                // grab two individuals from our sources
                if (Sources[0] == Sources[1]) // grab from the same source
                    Sources[0].Produce(2, 2, 0, subpopulation, Parents, state, thread);
                else // grab from different sources
                {
                    Sources[0].Produce(1, 1, 0, subpopulation, Parents, state, thread);
                    Sources[1].Produce(1, 1, 1, subpopulation, Parents, state, thread);
                }


                // at this point, Parents[] contains our two selected individuals

                // are our tree values valid?
                if (Tree1 != TREE_UNFIXED && (Tree1 < 0 || Tree1 >= Parents[0].Trees.Length))
                    // uh oh
                    state.Output.Fatal(
                        "GP Crossover Pipeline attempted to fix tree.0 to a value which was out of bounds of the array of the individual's trees.  Check the pipeline's fixed tree values -- they may be negative or greater than the number of trees in an individual");
                if (Tree2 != TREE_UNFIXED && (Tree2 < 0 || Tree2 >= Parents[1].Trees.Length))
                    // uh oh
                    state.Output.Fatal(
                        "GP Crossover Pipeline attempted to fix tree.1 to a value which was out of bounds of the array of the individual's trees.  Check the pipeline's fixed tree values -- they may be negative or greater than the number of trees in an individual");

                int t1;
                int t2;
                if (Tree1 == TREE_UNFIXED || Tree2 == TREE_UNFIXED)
                {
                    do
                        // pick random trees -- their GPTreeConstraints must be the same
                    {
                        if (Tree1 == TREE_UNFIXED)
                            if (Parents[0].Trees.Length > 1)
                                t1 = state.Random[thread].NextInt(Parents[0].Trees.Length);
                            else
                                t1 = 0;
                        else
                            t1 = Tree1;

                        if (Tree2 == TREE_UNFIXED)
                            if (Parents[1].Trees.Length > 1)
                                t2 = state.Random[thread].NextInt(Parents[1].Trees.Length);
                            else
                                t2 = 0;
                        else
                            t2 = Tree2;
                    } while (Parents[0].Trees[t1].Constraints(initializer) !=
                             Parents[1].Trees[t2].Constraints(initializer));
                }
                else
                {
                    t1 = Tree1;
                    t2 = Tree2;
                    // make sure the constraints are okay
                    if (Parents[0].Trees[t1].Constraints(initializer) !=
                        Parents[1].Trees[t2].Constraints(initializer)) // uh oh
                        state.Output.Fatal(
                            "GP Crossover Pipeline's two tree choices are both specified by the user -- but their GPTreeConstraints are not the same");
                }

                bool res1 = false;
                bool res2 = false;

                // BRS: This is kind of stupid to name it this way!
                GPTree currTree = Parents[1].Trees[t2];

                // pick some nodes
                GPNode p1 = null;
                GPNode p2 = null;

                // lets walk on parent2 all nodes to get subtrees for each node, doing it once for O(N) and not O(N^2)
                // because depth etc are computed and not stored
                ArrayList nodeToSubtrees = new ArrayList();
                // also Hashtable for size to List() of nodes in that size for O(1) lookup
                Hashtable sizeToNodes = new Hashtable();
                TraverseTreeForDepth(currTree.Child, nodeToSubtrees, sizeToNodes);
                // sort the ArrayList with comparator that sorts by subtrees
                nodeToSubtrees.Sort(new NodeComparator());

                for (int x = 0; x < NumTries; x++)
                {
                    // pick a node in individual 1
                    p1 = NodeSelect1.PickNode(state, subpopulation, thread, Parents[0], Parents[0].Trees[t1]);
                    // now lets find "similar" in parent 2                          
                    p2 = FindFairSizeNode(nodeToSubtrees, sizeToNodes, p1, currTree, state, thread);


                    // check for depth and swap-compatibility limits
                    res1 = VerifyPoints(initializer, p2, p1); // p2 can fill p1's spot -- order is important!
                    if (n - (q - start) < 2 || TossSecondParent)
                        res2 = true;
                    else
                        res2 = VerifyPoints(initializer, p1, p2); // p1 can fill p2's spot --  order is important!

                    // did we get something that had both nodes verified?
                    // we reject if EITHER of them is invalid. This is what lil-gp
                    // does.
                    // Koza only has numTries set to 1, so it's compatible as well.
                    if (res1 && res2)
                        break;
                }

                // at this point, res1 AND res2 are valid, OR
                // either res1 OR res2 is valid and we ran out of tries, OR
                // neither res1 nor res2 is valid and we rand out of tries.
                // So now we will transfer to a tree which has res1 or res2
                // valid, otherwise it'll just get replicated. This is
                // compatible with both Koza and lil-gp.

                // at this point I could check to see if my sources were breeding
                // pipelines -- but I'm too lazy to write that code (it's a little
                // complicated) to just swap one individual over or both over,
                // -- it might still entail some copying. Perhaps in the future.
                // It would make things faster perhaps, not requiring all that
                // cloning.

                // Create some new individuals based on the old ones -- since
                // GPTree doesn't deep-clone, this should be just fine. Perhaps we
                // should change this to proto off of the main species prototype,
                // but
                // we have to then copy so much stuff over; it's not worth it.

                GPIndividual j1 = (GPIndividual) (Parents[0].LightClone());
                GPIndividual j2 = null;
                if (n - (q - start) >= 2 && !TossSecondParent)
                    j2 = (GPIndividual) (Parents[1].LightClone());

                // Fill in various tree information that didn't get filled in there
                j1.Trees = new GPTree[Parents[0].Trees.Length];
                if (n - (q - start) >= 2 && !TossSecondParent)
                    j2.Trees = new GPTree[Parents[1].Trees.Length];

                // at this point, p1 or p2, or both, may be null.
                // If not, swap one in. Else just copy the parent.

                for (int x = 0; x < j1.Trees.Length; x++)
                {
                    if (x == t1 && res1) // we've got a tree with a kicking cross
                        // position!
                    {
                        j1.Trees[x] = (GPTree) (Parents[0].Trees[x].LightClone());
                        j1.Trees[x].Owner = j1;
                        j1.Trees[x].Child = Parents[0].Trees[x].Child.CloneReplacing(p2, p1);
                        j1.Trees[x].Child.Parent = j1.Trees[x];
                        j1.Trees[x].Child.ArgPosition = 0;
                        j1.Evaluated = false;
                    } // it's changed
                    else
                    {
                        j1.Trees[x] = (GPTree) (Parents[0].Trees[x].LightClone());
                        j1.Trees[x].Owner = j1;
                        j1.Trees[x].Child = (GPNode) (Parents[0].Trees[x].Child.Clone());
                        j1.Trees[x].Child.Parent = j1.Trees[x];
                        j1.Trees[x].Child.ArgPosition = 0;
                    }
                }

                if (n - (q - start) >= 2 && !TossSecondParent)
                    for (int x = 0; x < j2.Trees.Length; x++)
                    {
                        if (x == t2 && res2) // we've got a tree with a kicking
                            // cross position!
                        {
                            j2.Trees[x] = (GPTree) (Parents[1].Trees[x].LightClone());
                            j2.Trees[x].Owner = j2;
                            j2.Trees[x].Child = Parents[1].Trees[x].Child.CloneReplacing(p1, p2);
                            j2.Trees[x].Child.Parent = j2.Trees[x];
                            j2.Trees[x].Child.ArgPosition = 0;
                            j2.Evaluated = false;
                        } // it's changed
                        else
                        {
                            j2.Trees[x] = (GPTree) Parents[1].Trees[x].LightClone();
                            j2.Trees[x].Owner = j2;
                            j2.Trees[x].Child = (GPNode) Parents[1].Trees[x].Child.Clone();
                            j2.Trees[x].Child.Parent = j2.Trees[x];
                            j2.Trees[x].Child.ArgPosition = 0;
                        }
                    }

                // add the individuals to the population
                inds[q] = j1;
                q++;
                if (q < n + start && !TossSecondParent)
                {
                    inds[q] = j2;
                    q++;
                }
            }
            return n;
        }

        /**
         * This method finds a node using the logic given in the langdon paper.
         * @param nodeToSubtrees For Tree of Parent2 all precomputed stats about depth,subtrees etc
         * @param sizeToNodes Quick lookup for LinkedList of size to Nodes
         * @param parent1SelectedNode Node selected in parent1
         * @param Tree2 Tree of parent2
         * @param state Evolution State passed for getting access to Random Object of MersenneTwiser
         * @param thread thread number
         */
        protected GPNode FindFairSizeNode(ArrayList nodeToSubtrees,
            Hashtable sizeToNodes,
            GPNode parent1SelectedNode,
            GPTree tree2,
            IEvolutionState state,
            int thread)
        {
            GPNode selectedNode = null;
            // get the size of subtrees of parent1
            int parent1SubTrees = parent1SelectedNode.NumNodes(GPNode.NODESEARCH_NONTERMINALS);
            // the maximum length in mate we are looking for
            int maxmatesublen = parent1SubTrees == 0 ? 0 : 2 * parent1SubTrees + 1;

            // lets see if for all lengths we have trees corresponding
            bool[] mateSizeAvailable = new bool[maxmatesublen + 1];
            // initialize the array to false
            for (int i = 0; i < maxmatesublen; i++)
                mateSizeAvailable[i] = false;
            // check for ones we have
            for (int i = 0; i < nodeToSubtrees.Count; i++)
            {
                NodeInfo nodeInfo = (NodeInfo) nodeToSubtrees[i];
                // get the length of trees
                int subtree = nodeInfo.NumberOfSubTreesBeneath;
                if (subtree <= maxmatesublen)
                    mateSizeAvailable[subtree] = true;
            }
            // choose matesublen so mean size change=0 if possible
            int countOfPositives = 0;
            int countOfNegatives = 0;
            int sumOfPositives = 0;
            int sumOfNegatives = 0;
            int l;
            for (l = 1; l < parent1SubTrees; l++)
                if (mateSizeAvailable[l])
                {
                    countOfNegatives++;
                    sumOfNegatives += parent1SubTrees - l;
                }
            for (l = parent1SubTrees + 1; l <= maxmatesublen; l++)
                if (mateSizeAvailable[l])
                {
                    countOfPositives++;
                    sumOfPositives += l - parent1SubTrees;
                }
            // if they are missing use the same
            int mateSublengthSelected = 0;
            if (sumOfPositives == 0 || sumOfNegatives == 0)
            {
                //if so then check if mate has the length and use that
                if (mateSizeAvailable[parent1SubTrees])
                {
                    mateSublengthSelected = parent1SubTrees;
                }
                //else we go with zero
            }
            else
            {
                // probability of same is dependent on do we find same sub trees
                // else 0.0
                double pzero = (mateSizeAvailable[parent1SubTrees]) ? 1.0 / parent1SubTrees : 0.0;
                // positive probability
                double ppositive = (1.0 - pzero) /
                                   (countOfPositives +
                                    ((double) (countOfNegatives * sumOfPositives) / (sumOfNegatives)));
                // negative probability
                double pnegative = (1.0 - pzero) /
                                   (countOfNegatives +
                                    ((double) (countOfPositives * sumOfNegatives) / (sumOfPositives)));
                // total probability, just for making sure math is right ;-)
                double total = countOfNegatives * pnegative + pzero + countOfPositives * ppositive;
                // putting an assert for floating point calculations, similar to what langdon does
                // assert(total<1.01&&total>.99);
                // now create a Roulette Wheel
                RouletteWheelSelector wheel = new RouletteWheelSelector(maxmatesublen);
                // add probabilities to the wheel
                // all below the length of parent node get pnegative
                // all above get ppositive and one on node gets pzero
                for (l = 1; l < parent1SubTrees; l++)
                    if (mateSizeAvailable[l])
                        wheel.Add(pnegative, l);
                if (mateSizeAvailable[parent1SubTrees])
                    wheel.Add(pzero, parent1SubTrees);
                for (l = parent1SubTrees + 1; l <= maxmatesublen; l++)
                    if (mateSizeAvailable[l])
                        wheel.Add(ppositive, l);
                // spin the wheel
                mateSublengthSelected = wheel.Roulette(state, thread);
            }
            // now we have length chosen, but there can be many nodes with that
            //
            LinkedList<NodeInfo> listOfNodes = (LinkedList<NodeInfo>) sizeToNodes[mateSublengthSelected];
            if (listOfNodes == null)
            {
                state.Output.Fatal("In SizeFairCrossoverPipeline, nodes for tree length " + mateSublengthSelected + " is null, indicates some serious error");
            }
            // in size fair we choose the elements at random for given length
            int chosenNode = 0;
            // if using fair size get random from the list
            if (!Homologous)
            {
                chosenNode = state.Random[thread].NextInt(listOfNodes.Count);
            }
            // if Homologous
            else
            {
                if (listOfNodes.Count > 1)
                {
                    GPInitializer initializer = ((GPInitializer) state.Initializer);
                    int currentMinDistance = Int32.MaxValue;
                    for (int i = 0; i < listOfNodes.Count; i++)
                    {
                        // get the GP node
                        GPNode selectedMateNode = listOfNodes.ElementAt(i).Node;
                        // now lets traverse selected and parent 1 to see divergence
                        GPNode currentMateNode = selectedMateNode;
                        GPNode currentParent1Node = parent1SelectedNode;
                        // found a match?
                        bool foundAMatchInAncestor = false;
                        int distance = 0;
                        while (currentMateNode.Parent != null &&
                               currentMateNode.Parent is GPNode &&
                               currentParent1Node.Parent != null &&
                               currentParent1Node.Parent is GPNode &&
                               !foundAMatchInAncestor)
                        {
                            GPNode parent1 = (GPNode) currentParent1Node.Parent;
                            GPNode parent2 = (GPNode) currentMateNode.Parent;
                            // if there is match between compatibility of Parents break
                            if (parent1.SwapCompatibleWith(initializer, parent2))
                            {
                                foundAMatchInAncestor = true;
                                break;
                            }
                            else
                            {
                                // need to go one level above of both
                                currentMateNode = parent2;
                                currentParent1Node = parent1;
                                //increment the distance
                                distance = distance + 1;
                            }
                        }
                        // find the one with least distance
                        if (distance < currentMinDistance)
                        {
                            currentMinDistance = distance;
                            chosenNode = i;
                        }
                    }
                }
                // else take the first node, no choice
            }
            NodeInfo nodeInfoSelected = listOfNodes.ElementAt(chosenNode);
            selectedNode = nodeInfoSelected.Node;

            return selectedNode;
        }

        /**
         * Recursively travel the tree so that depth and subtree below are computed
         * only once and can be reused later.
         * 
         * @param node
         * @param nodeToDepth
         */
        public void TraverseTreeForDepth(GPNode node,
            ArrayList nodeToDepth,
            Hashtable sizeToNodes)
        {
            GPNode[] children = node.Children;
            NodeInfo nodeInfo = new NodeInfo(node, node.NumNodes(GP.GPNode.NODESEARCH_NONTERMINALS));
            nodeToDepth.Add(nodeInfo);
            // check to see if there is list in map for that size
            LinkedList<NodeInfo> listForSize = (LinkedList<NodeInfo>) sizeToNodes[nodeInfo.NumberOfSubTreesBeneath];
            if (listForSize == null)
            {
                listForSize = new LinkedList<NodeInfo>();
                sizeToNodes[nodeInfo.NumberOfSubTreesBeneath] = listForSize;
            }
            // add it to the list no matter what
            listForSize.AddLast(nodeInfo);
            // recurse
            if (children.Length > 0)
            {
                foreach (GPNode t in children)
                {
                    TraverseTreeForDepth(t, nodeToDepth, sizeToNodes);
                }
            }
        }




        /**
         * Inner class to do a quick Roulette Wheel Selection
         *  
         */
        class RouletteWheelSelector
        {
            readonly int[] _length;
            readonly double[] _probability;
            int _currentIndex;
            int _maxLength;

            public RouletteWheelSelector(int size)
            {
                _length = new int[size];
                _probability = new double[size];
            }

            public void Add(double currentProbability, int currentLength)
            {
                _length[_currentIndex] = currentLength;
                _probability[_currentIndex] = currentProbability;
                _currentIndex = _currentIndex + 1;
                if (currentLength > _maxLength) _maxLength = currentLength;
            }

            public int Roulette(IEvolutionState state, int thread)
            {
                int winner = 0;
                int selectedLength = 0;
                // accumulate
                for (int i = 1; i < _currentIndex; i++)
                {
                    _probability[i] += _probability[i - 1];
                }

                int bot = 0; // binary chop search
                int top = _currentIndex - 1;
                double f = state.Random[thread].NextDouble() * _probability[top];

                for (int loop = 0; loop < 20; loop++)
                {
                    int index = (top + bot) / 2;
                    if (index > 0 && f < _probability[index - 1])
                        top = index - 1;
                    else if (f > _probability[index])
                        bot = index + 1;
                    else
                    {
                        if (f == _probability[index] && index + 1 < _currentIndex)
                            winner = index + 1;
                        else
                            winner = index;
                        break;
                    }
                }
                // check for bounds
                if (winner < 0 || winner >= _currentIndex)
                {
                    state.Output.Fatal(
                        "roulette() method  winner " + winner + " out of range 0..." + (_currentIndex - 1));
                    winner = 0; //safe default
                }
                if (_length[winner] < 1 || _length[winner] > _maxLength)
                {
                    state.Output.Fatal("roulette() method " + _length[winner] + " is  out of range 1..." + _maxLength);
                    // range is specified on creation
                    return _maxLength; //safe default
                }
                selectedLength = _length[winner];
                return selectedLength;
            }

        }

        /**
         *Used for O(1) information of number of subtrees
         *
         */
        class NodeInfo
        {
            // numberOfSubTrees beneath
            public int NumberOfSubTreesBeneath { get; private set; }

            // actual node
            public GPNode Node { get; }

            public NodeInfo(GPNode node, int numberOfSubtrees)
            {
                this.Node = node;
                this.NumberOfSubTreesBeneath = numberOfSubtrees;
            }

            public void SetSubtrees(int totalSubtrees)
            {
                this.NumberOfSubTreesBeneath = totalSubtrees;
            }

            public int GetSubtrees()
            {
                return NumberOfSubTreesBeneath;
            }

            public GPNode GetNode()
            {
                return Node;
            }

        }
        class NodeComparator : IComparer
        {
            public int Compare(Object o1, Object o2)
            {
                NodeInfo node1 = o1 as NodeInfo;
                NodeInfo node2 = o2 as NodeInfo;

                if (node1 == null || node2 == null)
                    return -1;

                if (node1.NumberOfSubTreesBeneath > node2.NumberOfSubTreesBeneath)
                    return 1;
                if (node1.NumberOfSubTreesBeneath < node2.NumberOfSubTreesBeneath)
                    return -1;
                return 0;
            }
        }
    }

}