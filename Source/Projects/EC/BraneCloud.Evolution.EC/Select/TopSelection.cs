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

namespace BraneCloud.Evolution.EC.Select
{
    /**
     * Returns the single fittest individual in the population, breaking ties randomly.
     *
     * <p>The individual can be <i>cached</i> so it is not recomputed every single time;
     * the cache is cleared after <tt>prepareToProduce(...)</tt> is called.  Note that this
     * means that if there are multiple individuals with the top fitness, and we're caching,
     * only one of them will be returned throughout the series of multiple produce(...) calls.
     *
     <p><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br>
     Always 1.

     <p><b>Parameters</b><br>
     <table>
     <tr><td valign=top><i>base.</i><tt>cache</tt><br>
     <font size=-1> bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
     <td valign=top>(should we cache the individual?)</td></tr>
     </table>

     <p><b>Default Base</b><br>
     select.top

     * @author Sean Luke
     * @version 1.0 
     */

    [ECConfiguration("ec.select.TopSelection")]
    public class TopSelection : SelectionMethod
    {
        /** Default base */
        public const string P_TOP = "top";

        public const string P_CACHE = "cache";

        bool _cache;
        int _best;

        public override IParameter DefaultBase => SelectDefaults.ParamBase.Push(P_TOP);

        // don't need clone etc. 

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            IParameter def = DefaultBase;

            _cache = state.Parameters.GetBoolean(paramBase.Push(P_CACHE), def.Push(P_CACHE), false);
        }

        public override void PrepareToProduce(IEvolutionState s, int subpop, int thread)
        {
            base.PrepareToProduce(s, subpop, thread);

            if (_cache)
                _best = -1;
        }

        public void CacheBest(int subpopulation, IEvolutionState state, int thread)
        {
            IList<Individual> oldinds = state.Population.Subpops[subpopulation].Individuals;
            int len = oldinds.Count;

            int b = 0; // this is the INDEX of the best known individual
            Individual bi = oldinds[b]; // this is the best known individual            
            int ties = 1;

            for (int i = 1; i < len; i++)
            {
                Individual ni = oldinds[i];

                // if he's better, definitely adopt him and reset the ties
                if (ni.Fitness.BetterThan(bi.Fitness))
                {
                    bi = ni;
                    b = i;
                    ties = 1;
                }
                // if he's the same, adopt him with 1/n probability
                else if (ni.Fitness.EquivalentTo(bi.Fitness))
                {
                    ties++;
                    if (state.Random[thread].NextBoolean(1.0 / ties))
                    {
                        bi = ni;
                        b = i;
                    }
                }
            }
            _best = b;
        }

        public override int Produce(int subpop, IEvolutionState state, int thread)
        {
            if (_cache && _best >= 0)
            {
                // do nothing, it's cached
            }
            else
            {
                CacheBest(subpop, state, thread);
            }
            return _best;
        }
    }
}