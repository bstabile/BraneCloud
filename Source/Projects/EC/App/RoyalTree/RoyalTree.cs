using System.Linq;
using BraneCloud.Evolution.EC.App.RoyalTree.Func;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.App.RoyalTree
{
    /**
     * RoyalTree implements Punch's RoyalTree problem. See the README.txt.
     *
     <p><b>Parameters</b><br>
     <table>
     <tr><td valign=top><i>base</i>.<tt>data</tt><br>
     <font size=-1>classname, inherits or == ec.app.royaltree.RoyalTreeData</font></td>
     <td valign=top>(the class for the prototypical GPData object for the RoyalTree problem)</td></tr>
     </table>

     <p><b>Parameter bases</b><br>
     <table>
     <tr><td valign=top><i>base</i>.<tt>data</tt></td>
     <td>species (the GPData object)</td></tr>
     </table>
     *
     * @author James McDermott
     * @version 1.0
     */

    [ECConfiguration("ec.app.royaltree.RoyalTree")]
    public class RoyalTree : GPProblem, ISimpleProblem
    {

        public void Evaluate(IEvolutionState state,
            Individual ind,
            int subpopulation,
            int threadnum)
        {
            if (!ind.Evaluated) // don't bother reevaluating
            {
                // trees[0].child is the root
                double score = Fitness(((GPIndividual) ind).Trees[0].Child, state);

                var f = (SimpleFitness) ind.Fitness;
                f.SetFitness(state, score, false);
                ind.Evaluated = true;
            }
        }

        double Fitness(GPNode node, IEvolutionState state)
        {
            double completeBonus = 2.0,
                partialBonus = 1.0,
                fullBonus = 2.0,
                penalty = 1.0 / 3;

            char nodeFn = ((RoyalTreeNode) node).Value;
            if (nodeFn == 'X')
            {
                return 1.0;
            }

            double retval = 0.0;
            bool nodeIsPerfect = true;
            foreach (GPNode child in node.Children)
            {
                char childFn = ((RoyalTreeNode) child).Value;

                if (IsPerfect(nodeFn, child, state))
                {
                    retval += fullBonus * Fitness(child, state);
                }
                else if (IsSuccessor(nodeFn, childFn, state))
                {
                    retval += partialBonus * Fitness(child, state);
                    nodeIsPerfect = false;
                }
                else
                {
                    retval += penalty * Fitness(child, state);
                    nodeIsPerfect = false;
                }
            }

            // Only if every child is a perfect subtree of the appropriate
            // type does this node get completeBonus.
            if (nodeIsPerfect)
            {
                retval *= completeBonus;
            }
            return retval;
        }


        // doesn't need to be cloned
        readonly char[] _successors = new char[256]; // we assume we only have letters, and 0 means "no sucessor"

        public RoyalTree()
        {
            string SUCCESSORS = "XABCDEFGHIJ";
            for (int i = 0; i < SUCCESSORS.Length - 1; i++)
                _successors[SUCCESSORS[i]] = SUCCESSORS[i + 1];
        }

        /**
         * @param p parent
         * @param q child
         * @return whether q is the correct "successor", eg p = B and q = A
         */
        bool IsSuccessor(char p, char q, IEvolutionState state)
        {
            return _successors[p] == q;
        }

        /**
         * Calculate whether the tree rooted at n is a perfect subtree
         * of the appropriate type given the current parent.
         * @param parent
         * @param n root of the sub-tree to be tested.
         * @return whether it is a perfect subtree of the right type.
         */
        bool IsPerfect(char parent, GPNode node, IEvolutionState state)
        {
            char nodeFn = ((RoyalTreeNode) node).Value;
            if (!IsSuccessor(parent, nodeFn, state))
            {
                return false;
            }
            return node.Children.All(child => IsPerfect(nodeFn, child, state));
        }
    }
}