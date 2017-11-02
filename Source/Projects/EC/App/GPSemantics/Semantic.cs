using System;
using System.Collections;
using System.Linq;
using BraneCloud.Evolution.EC.App.GPSemantics.Func;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.App.GPSemantics
{
    /**
     * Semantic.cs
     * 
     * Implements Goldberg and O'Reilly's semantic Order and Majority
     * problems. See the README.
     *
     <p><b>Parameters</b><br>
     <table>
     <tr><td valign=top><i>base</i>.<tt>data</tt><br>
     <font size=-1>classname, inherits or == ec.gp.gpdata</font></td>
     <td valign=top>(the class for the prototypical GPData object)</td></tr>
     </table>

     <p><b>Parameter bases</b><br>
     <table>
     <tr><td valign=top><i>base</i>.<tt>data</tt></td>
     <td>species (the GPData object)</td></tr>
     </table>
     *
     * Class representing Semantic Order and Majority problems. This
     * problem has a size: typical size is 16, which means the terminals
     * are [X0, N0, ... X16, N16]. For Order, fitness is 1 for every time
     * when Xi occurs before Ni, in an inorder traversal. For Majority,
     * fitness is 1 for every time when Xi occurs more often than Ni.
     *
     * @author James McDermott
     * @version 1.0
     */

    [ECConfiguration("ec.app.gpsemantics.Semantic")]
    public class Semantic : GPProblem, ISimpleProblem
    {

        const string P_PROBLEM_NAME = "problem_name";
        const string P_SIZE = "size";
        const string P_ORDER = "Order";
        const string P_MAJORITY = "Majority";

        string _problemName;
        int _problemSize;

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // very important, remember this
            base.Setup(state, paramBase);
            IParameter fsSize = new Parameter(GPDefaults.P_GP).Push(GPInitializer.P_FUNCTIONSETS).Push("" + 0)
                .Push(GPFunctionSet.P_SIZE);
            int numFuncs = state.Parameters.GetInt(fsSize, null, 1);
            _problemSize = (numFuncs - 1) / 2;
            _problemName = state.Parameters.GetString(paramBase.Push(P_PROBLEM_NAME), paramBase.Push(P_ORDER));
            if (!_problemName.Equals(P_ORDER) && !_problemName.Equals(P_MAJORITY))
                state.Output.Error("The problem name is unrecognized",
                    paramBase.Push(P_PROBLEM_NAME));

            Console.WriteLine("Problem name " + _problemName);
            Console.WriteLine("Problem size " + _problemSize);
            state.Output.ExitIfErrors();
        }

        public void Evaluate(IEvolutionState state,
            Individual ind,
            int subpopulation,
            int threadnum)
        {
            if (!ind.Evaluated) // don't bother reevaluating
            {
                // trees[0].Child is the root

                ArrayList output = GetSemanticOutput(((GPIndividual) ind).Trees[0]);

                double score = 0.0;
                for (int i = 0; i < output.Count; i++)
                {
                    SemanticNode n = (SemanticNode) output[i];
                    if (n.Value == 'X')
                    {
                        score += 1;
                    }
                }

                SimpleFitness f = (SimpleFitness) ind.Fitness;
                f.SetFitness(state, (float) score, false);
                ind.Evaluated = true;
            }
        }

        /**
         * @param t Tree to be "executed"
         * @return expressed output
         */
        ArrayList GetSemanticOutput(GPTree t)
        {
            ArrayList p = new ArrayList();
            ArrayList nodes = new ArrayList();

            // Is there a better way to get all the nodes in a depth-first
            // traversal? Note that the paper specifies inorder traversal,
            // but since we're only getting the terminals, preorder,
            // inorder, and postorder are equivalent.
            int nterminals = t.Child.NumNodes(GPNode.NODESEARCH_TERMINALS);
            for (int i = 0; i < nterminals; i++)
            {
                GPNodeGatherer g = new GPNodeGatherer();
                t.Child.NodeInPosition(i, g, GPNode.NODESEARCH_TERMINALS);
                nodes.Add(g.Node);
            }

            if (_problemName.Equals(P_ORDER))
            {
                // Order: first occurence counts
                for (int i = 0; i < nodes.Count; i++)
                {
                    SemanticNode node = (SemanticNode) nodes[i];
                    if (!NodeSameIndexExists(p, node.Index))
                    {
                        p.Add(node);
                    }
                }
            }
            else
            {
                // Majority: most common counts
                for (int n = 0; n < _problemSize; n++)
                {
                    int xCount = 0;
                    int nCount = 0;
                    int lastXNode = -1;
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        SemanticNode node = (SemanticNode) nodes[i];
                        if (node.Value == 'X' && node.Index == n)
                        {
                            xCount += 1;
                            lastXNode = i;
                        }
                        else if (node.Value == 'N' && node.Index == n)
                        {
                            nCount += 1;
                        }
                    }
                    if (xCount >= nCount && xCount > 0)
                    {
                        p.Add((SemanticNode) nodes[lastXNode]);
                    }
                }
            }
            return p;
        }


        /**
         * Given a list and an index, check whether a node of that index
         * exists in the list.
         *
         * @param p List of nodes
         * @param n index
         * @return whether node of index n exists in p.
         */
        bool NodeSameIndexExists(ArrayList p, int n)
        {
            return p.Cast<object>().Any(t => ((SemanticNode) t).Index == n);
        }

        String phenotypeToString(ArrayList p)
        {
            String retval = "";
            foreach (object t in p)
            {
                retval += t + " ";
            }
            return retval;
        }

        // In one paper, there is a parameter for scaling, ie the fitness
        // contribution of each Xi can be uniform, or linearly or
        // exponentially scaled. We don't do that in this version.

        public void Describe(
            EvolutionState state,
            Individual ind,
            int subpopulation,
            int threadnum,
            int log)
        {
            state.Output.PrintLn(
                "\n\nBest Individual: output = " + phenotypeToString(GetSemanticOutput(((GPIndividual) ind).Trees[0])),
                log);
        }
    }

}
