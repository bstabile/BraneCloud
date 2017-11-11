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
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Select
{
    /**
     * Returns an individual using a form of simulated annealing.
     *
     * <p>This works as follows.  If there is only one individual in the population, he
     * is selected.  Otherwise we pick a random individual from among the individuals
     * <i>other</i> than the first individual.  If that random individual is <i>fitter</i>
     * than the first individual, it is selected.  Otherwise if that random individual is
     * <i>as fit</i> as the first individual, one of the two is selected at random.  Otherwise
     * if the random individual is <i>not as fit</i> as the first individual, it is selected
     * with a probability P = e^((fit(random ind) - fit(first ind)) / t), where t is a
     * TEMPERATURE.  Otherwise the first individual is selected.
     *
     * <p>The temperature starts at a high value >> 0, and is slowly cut down by multiplying
     * it by a CUTDOWN value every generation.  When the temperature reaches 0, then the first 
     * individual is selected every time.  
     *
     * <p>The selected individual can be <i>cached</i> so the same individual is returned
     * multiple times without being recomputed.  This cache is cleared after 
     * <tt>prepareToProduce(...)</tt> is called.  Note that this option is not appropriate
     * for Steady State Evolution, which only calls <tt>prepareToProduce(...)</tt> once.

     <p><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br>
     Always 1.

     <p><b>Parameters</b><br>
     <table>
     <tr><td valign=top><i>base.</i><tt>cache</tt><br>
     <font size=-1> bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
     <td valign=top>(should we cache the individual?)</td></tr>
     <tr><td valign=top><i>base.</i><tt>temperature</tt><br>
     <font size=-1> double > 0 (or undefined)</font></td>
     <td valign=top>(annealing start temperature)</td></tr>
     <tr><td valign=top><i>base.</i><tt>cutdown</tt><br>
     <font size=-1> 0.0 &lt;= double &gt;= 1.0 (default is 0.95)</font></td>
     <td valign=top>(annealing cutdown)</td></tr>
     </table>

     <p><b>Default Base</b><br>
     select.annealed

     * @author Sean Luke
     * @version 1.0 
     */

    [ECConfiguration("ec.select.AnnealedSelection")]
    public class AnnealedSelection : SelectionMethod
    {
        /** Default base */
        public const string P_ANNEALED = "annealed";
        public const string P_CACHE = "cache";
        public const string P_TEMPERATURE = "temperature";
        public const string P_CUTDOWN = "cutdown";

        public const double V_CUTDOWN = 0.95;

        bool _cache;
        int _best;
        double _temperature;
        double _t;
        double _cutdown;

        public override IParameter DefaultBase => SelectDefaults.ParamBase.Push(P_ANNEALED);

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            IParameter def = DefaultBase;

            _cache = state.Parameters.GetBoolean(paramBase.Push(P_CACHE), def.Push(P_CACHE), false);
            _temperature = state.Parameters.GetDoubleWithDefault(paramBase.Push(P_TEMPERATURE), def.Push(P_TEMPERATURE), 0);
            if (_temperature < 0)
            {
                state.Output.Fatal("TopSelection temperature, if defined, must be >= 0",
                    paramBase.Push(P_TEMPERATURE), def.Push(P_TEMPERATURE));
            }
            _cutdown = state.Parameters.GetDoubleWithDefault(paramBase.Push(P_CUTDOWN), def.Push(P_CUTDOWN), 0.95);
            if (_cutdown < 0 || _cutdown > 1)
            {
                state.Output.Fatal("TopSelection cutdown, if defined, must be between 0 and 1.  Default is 0.95.",
                    paramBase.Push(P_TEMPERATURE), def.Push(P_TEMPERATURE));
            }
        }

        public override void PrepareToProduce(IEvolutionState state, int subpop, int thread)
        {
            base.PrepareToProduce(state, subpop, thread);

            if (_cache)
                _best = -1;
        }

        public void CacheBest(int subpop, IEvolutionState state, int thread)
        {
            IList<Individual> oldinds = state.Population.Subpops[subpop].Individuals;
            int len = oldinds.Count;

            if (len == 1) // uh oh
                _best = 0;
            else
            {
                int candidate = state.Random[thread].NextInt(len - 1) + 1;

                Individual first = oldinds[0];
                Individual next = oldinds[candidate];

                if (next.Fitness.BetterThan(first.Fitness))
                    _best = candidate;

                else if (next.Fitness.EquivalentTo(first.Fitness) && state.Random[thread].NextBoolean())
                    _best = candidate;

                // he's worse           
                else if (state.Random[thread]
                    .NextBoolean(Math.Exp((next.Fitness.Value - first.Fitness.Value) / _t)))
                    _best = candidate;

                else _best = 0;
            }

            // note that we do NOT do temperature = temperature * cutdown,
            // which would ordinarily make perfect sense, except that we're
            // cloning from a prototype.
            _t = _temperature * Math.Pow(_cutdown, state.Generation);
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