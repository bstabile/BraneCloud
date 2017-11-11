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

using System.Collections.Generic;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.SingleState
{
    /**
     * A very simple single-threaded breeder with optional elitism.
     *
     
     <p><b>Parameters</b><br>
     <table>
     <tr><td valign=top><tt><i>base</i>.elite.<i>i</i></tt><br>
     <font size=-1>int >= 0 (default=0)</font></td>
     <td valign=top>(will subpopulation <i>i</i> include ONE elite individual?)</td></tr>
     <tr><td valign=top><tt><i>base</i>.reduce-by.<i>i</i></tt><br>
     <font size=-1>int >= 0 (default=0)</font></td>
     <td valign=top>(how many to reduce subpopulation <i>i</i> by each generation)</td></tr>
     <tr><td valign=top><tt><i>base</i>.minimum-size.<i>i</i></tt><br>
     <font size=-1>int >= 2 (default=2)</font></td>
     <td valign=top>(the minimum size for subpopulation <i>i</i> regardless of reduction)</td></tr>
     <tr><td valign=top><tt><i>base</i>.reevaluate-elites.<i>i</i></tt><br>
     <font size=-1>boolean (default = false)</font></td>
     <td valign=top>(should we reevaluate the elites of subpopulation <i>i</i> each generation?)</td></tr>
     <tr><td valign=top><tt><i>base</i>.sequential</tt><br>
     <font size=-1>boolean (default = false)</font></td>
     <td valign=top>(should we breed just one subpopulation each generation (as opposed to all of them)?)</td></tr>
     </table>

     <p><b>Default Base</b><br>
     ec.subpop

     <p><b>Parameter bases</b><br>
     <table>
     <tr><td valign=top><i>base</i>.<tt>species</tt></td>
     <td>species (the subpopulations' species)</td></tr>

     *
     *
     * @author Ermo Wei and David Freelan
     * @version 1.0 
     */

    [ECConfiguration("ec.singlestate.SingleStateBreeder")]
    public class SingleStateBreeder : Breeder
    {
        public const string P_ELITE = "elite";
        public const string P_EXPANDED_SUBPOP_SIZE = "expanded-subpop-size";
        public const int V_SUBPOP_NOT_RESIZED = -1;

        public bool[] Elite { get; set; }
        public int[] ExpandedSubpopSize { get; set; }
        public bool[] StubsFilled { get; set; }

        public override Population BreedPopulation(IEvolutionState state)
        {
            Population pop = state.Population;
            for (int x = 0; x < pop.Subpops.Count; x++)
                BreedSubpop(state, pop.Subpops[x], x);
            return pop;
        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            IParameter p = new Parameter(Initializer.P_POP).Push(Population.P_SIZE);
            int size = state.Parameters.GetInt(p, null,
                1); // if size is wrong, we'll let Population complain about it -- for us, we'll just make 0-sized arrays and drop out.

            int defaultSubpop =
                state.Parameters.GetInt(new Parameter(Initializer.P_POP).Push(Population.P_DEFAULT_SUBPOP), null, 0);

            Elite = new bool[size];

            for (int x = 0; x < size; x++)
            {
                // get elites
                if (state.Parameters.ParameterExists(paramBase.Push(P_ELITE).Push("" + x), null))
                {
                    Elite[x] = state.Parameters.GetBoolean(paramBase.Push(P_ELITE).Push("" + x), null, false);
                }
                else if (defaultSubpop >= 0 &&
                         state.Parameters.ParameterExists(paramBase.Push(P_ELITE).Push("" + defaultSubpop), null))
                {
                    Elite[x] = state.Parameters.GetBoolean(paramBase.Push(P_ELITE).Push("" + defaultSubpop), null,
                        false);
                }
                else // no elitism
                {
                    state.Output.Warning("Elites not defined for subpopulation " + x + ".  Assuming false.");
                    Elite[x] = false;
                }
            }

            ExpandedSubpopSize = new int[size];

            for (int x = 0; x < size; x++)
            {
                // get expanded subpops
                if (state.Parameters.ParameterExists(paramBase.Push(P_EXPANDED_SUBPOP_SIZE).Push("" + x), null))
                {
                    ExpandedSubpopSize[x] =
                        state.Parameters.GetInt(paramBase.Push(P_EXPANDED_SUBPOP_SIZE).Push("" + x), null, 1);
                }
                else if (defaultSubpop >= 0 &&
                         state.Parameters.ParameterExists(
                             paramBase.Push(P_EXPANDED_SUBPOP_SIZE).Push("" + defaultSubpop), null))
                {
                    ExpandedSubpopSize[x] =
                        state.Parameters.GetInt(paramBase.Push(P_EXPANDED_SUBPOP_SIZE).Push("" + defaultSubpop), null,
                            1);
                }
                else
                {
                    state.Output.Warning("Expanded subpopulation size not defined for subpopulation " + x +
                                         ".  Assuming populations are not changed.");
                    ExpandedSubpopSize[x] = V_SUBPOP_NOT_RESIZED;
                }
            }

            StubsFilled = new bool[size];

        }

        public void BreedSubpop(IEvolutionState state, Subpopulation subpop, int index)
        {
            BreedingSource bp = (BreedingSource) subpop.Species.Pipe_Prototype;
            if (!StubsFilled[index])
                bp.FillStubs(state, null);
            StubsFilled[index] = true;

            bp.PrepareToProduce(state, index, 0);

            // maybe resize?
            IList<Individual> newIndividuals = null;
            int newlen = subpop.Individuals.Count;
            if (ExpandedSubpopSize[index] != V_SUBPOP_NOT_RESIZED)
            {
                newlen = ExpandedSubpopSize[index];
            }

            newIndividuals = new List<Individual>();

            IList<Individual> individuals = subpop.Individuals;
            int len = individuals.Count;

            if (Elite[index])
            {
                // We need to do some elitism: we put the BEST individual in the first slot
                Individual best = individuals[0];
                for (int i = 1; i < len; i++)
                {
                    Individual ind = individuals[i];
                    if (ind.Fitness.BetterThan(best.Fitness))
                        best = ind;
                }
                newIndividuals.Add(best);
            }

            // start breedin'!
            while (newIndividuals.Count < newlen)
            {
                // we don't allocate a hash table every time, so we pass in null
                bp.Produce(1, newlen - newIndividuals.Count, index, newIndividuals, state, 0, null);
            }

            subpop.Individuals = newIndividuals;
            bp.FinishProducing(state, index, 0);
        }
    }
}