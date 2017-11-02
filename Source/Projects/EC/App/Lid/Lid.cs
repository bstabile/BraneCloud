
using System;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.App.Lid
{
    /**
     * Lid implements Daida's Lid problem. See the README.txt.
     *
     <p><b>Parameters</b><br>
     <table>
     <tr><td valign=top><i>base</i>.<tt>data</tt><br>
     <font size=-1>classname, inherits or == ec.app.lid.LidData</font></td>
     <td valign=top>(the class for the prototypical GPData object for the Lid problem)</td></tr>
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

    [ECConfiguration("ec.app.lid.Lid")]
    public class Lid : GPProblem, ISimpleProblem
    {

        static String P_TARGET_DEPTH = "targetDepth";
        static String P_TARGET_TERMINALS = "targetTerminals";
        static String P_WEIGHT_DEPTH = "weightDepth";

        int maxWeight = 100;
        int targetDepth;
        int targetTerminals;
        int actualDepth;
        int actualTerminals;

        int weightDepth;
        int weightTerminals;

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // very important, remember this
            base.Setup(state, paramBase);

            // load our targets
            targetDepth = state.Parameters.GetInt(paramBase.Push(P_TARGET_DEPTH), null, 1);
            if (targetDepth == 0)
                state.Output.Error("The target depth must be > 0",
                    paramBase.Push(P_TARGET_DEPTH));
            targetTerminals = state.Parameters.GetInt(paramBase.Push(P_TARGET_TERMINALS), null, 1);
            if (targetTerminals == 0)
                state.Output.Error("The target terminals must be > 0",
                    paramBase.Push(P_TARGET_TERMINALS));
            weightDepth = state.Parameters.GetInt(paramBase.Push(P_WEIGHT_DEPTH), null, 0);
            if (weightDepth < 0 || weightDepth > maxWeight)
                state.Output.Error("The depth-weighting must be in [0, maxWeight]",
                    paramBase.Push(P_WEIGHT_DEPTH));
            weightTerminals = maxWeight - weightDepth;
            Console.WriteLine("target depth " + targetDepth + " targetTerminals " + targetTerminals);
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

                // Note ECJ GPNode counts the root as being depth
                // 1. Daida et al count it as depth 0 (p. 1669).
                actualDepth = ((GPIndividual) ind).Trees[0].Child.Depth - 1;

                actualTerminals = ((GPIndividual) ind).Trees[0].Child.NumNodes(GPNode.NODESEARCH_TERMINALS);

                double scoreDepth = weightDepth * (1.0 - Math.Abs(targetDepth - actualDepth) / (double) targetDepth);
                double scoreTerminals = 0.0;
                if (targetDepth == actualDepth)
                {
                    scoreTerminals = weightTerminals *
                                     (1.0 - Math.Abs(targetTerminals - actualTerminals) / (double) targetTerminals);
                }

                double score = scoreTerminals + scoreDepth;

                SimpleFitness f = (SimpleFitness) ind.Fitness;
                f.SetFitness(state, (float) score, false);
                ind.Evaluated = true;
            }
        }

        public override void Describe(
            IEvolutionState state,
            Individual ind,
            int subpopulation,
            int threadnum,
            int log)
        {
            // trees[0].child is the root
            // Note ECJ GPNode counts the root as being depth
            // 1. Daida et al count it as depth 0. We'll print both.
            actualDepth = ((GPIndividual) ind).Trees[0].Child.Depth - 1;
            actualTerminals = ((GPIndividual) ind).Trees[0].Child.NumNodes(GPNode.NODESEARCH_TERMINALS);
            state.Output.PrintLn(
                "\n\nBest Individual: in ECJ terms depth = " + (actualDepth + 1) + "; in Lid terms depth = " +
                actualDepth +
                "; number of terminals = " + actualTerminals, log);
        }
    }
}