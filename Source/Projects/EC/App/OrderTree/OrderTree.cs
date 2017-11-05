using System;
using BraneCloud.Evolution.EC.App.OrderTree.Func;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.App.OrderTree
{
    /**
     * OrderTree implements the OrderTree problem of Hoang et al. See the
     * README.txt.  Note that although this is a tunable problem, tuning
     * is achieved by setting the function and terminal sets. No need for
     * a size parameter.
     *
     <p><b>Parameters</b><br>
     <table>
     <tr><td valign=top><i>base</i>.<tt>data</tt><br>
     <font size=-1>classname, inherits or == ec.app.ordertree.OrderTreeData</font></td>
     <td valign=top>(the class for the prototypical GPData object for the OrderTree problem)</td></tr>
     </table>

     <tr><td valign=top><i>base</i>.<tt>contribution-type</tt><br>
     <font size=-1>Integer specifying the amount of nonlinearity in fitness contributions</font></td>
     <td valign=top>0: add unit; 1: add node value; 2: add node value squared; 3: add 3^node value</td></tr>
     </table>

     <p><b>IParameter paramBases</b><br>
     <table>
     <tr><td valign=top><i>base</i>.<tt>data</tt></td>
     <td>species (the GPData object)</td></tr>
     </table>
     *
     * @author James McDermott
     * @version 1.0
     */

    [ECConfiguration("ec.app.ordertree.OrderTree")]
    public class OrderTree : GPProblem, ISimpleProblem
    {
        const string P_CONTRIBUTION_TYPE = "contribution-type";
        const int CONTRIBUTION_UNIT = 0;
        const int CONTRIBUTION_VALUE = 1;
        const int CONTRIBUTION_SQUARE = 2;
        const int CONTRIBUTION_EXPONENTIAL = 3;

        double _fitness;
        int _fitnessContributionType;

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // very important, remember this
            base.Setup(state, paramBase);

            _fitnessContributionType = state.Parameters.GetInt(paramBase.Push(P_CONTRIBUTION_TYPE), null, 1);
            if (_fitnessContributionType < CONTRIBUTION_UNIT || _fitnessContributionType > CONTRIBUTION_EXPONENTIAL)
                state.Output.Fatal("Fitness Contribution Type must be an integer greater than 0 and less th an 4",
                    paramBase.Push(P_CONTRIBUTION_TYPE));

            state.Output.ExitIfErrors();
        }

        public void Evaluate(IEvolutionState state,
            Individual ind,
            int subpopulation,
            int threadnum)
        {
            if (!ind.Evaluated) // don't bother reevaluating
            {
                // trees[0].child is the root
                _fitness = 0.0;
                NodeCal(((GPIndividual) ind).Trees[0].Child, state);

                SimpleFitness f = (SimpleFitness) ind.Fitness;
                f.SetFitness(state, _fitness, false);
                ind.Evaluated = true;
            }
        }

        double FitnessContribution(double value, IEvolutionState state)
        {
            switch (_fitnessContributionType)
            {
                case CONTRIBUTION_UNIT: return 1.0;
                case CONTRIBUTION_VALUE: return value;
                case CONTRIBUTION_SQUARE: return value * value;
                case CONTRIBUTION_EXPONENTIAL: return Math.Pow(3.0, value);
                default:
                    state.Output.Fatal("Unexpected fitness contribution type.");
                    return -1.0;
            }
        }

        void NodeCal(GPNode p, IEvolutionState state)
        {
            int pval = ((OrderTreeNode) p).Value;
            for (int i = 0; i < p.Children.Length; i++)
            {
                GPNode c = p.Children[i];
                int cval = ((OrderTreeNode) c).Value;
                if (pval < cval)
                {
                    // direct fitness contribution
                    _fitness += FitnessContribution(cval, state);
                    NodeCal(c, state);
                }
                else if (pval == cval)
                {
                    // neutral-left-walk
                    bool found = false;
                    while (c.Children.Length > 0 && cval == pval && !found)
                    {
                        c = c.Children[0];
                        cval = ((OrderTreeNode) c).Value;
                        if (pval < cval)
                        {
                            found = true;
                        }
                    }
                    if (found)
                    {
                        _fitness += FitnessContribution(cval, state);
                        NodeCal(c, state);
                    }
                }
            }
        }
    }
}