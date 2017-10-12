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

using BraneCloud.Evolution.EC.Select;
using BraneCloud.Evolution.EC.SteadyState;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Parsimony
{
    /// <summary> 
    /// Does a simple tournament selection, limited to the subpop it's
    /// working in at the time.
    /// 
    /// <p/>Tournament selection works like this: first, <i>size</i> individuals
    /// are chosen at random from the population.  Then of those individuals,
    /// the one with the best fitness is selected.  If two individuals have the
    /// same fitness, the one with smaller size is prefered.
    /// 
    /// The default tournament size is 7.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// Always 1.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>size</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(the tournament size)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>pick-worst</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">(should we pick the <i>worst</i> individual in the tournament instead of the <i>best</i>?)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// select.lexicographic-tournament
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.parsimony.LexicographicTournamentSelection")]
    public class LexicographicTournamentSelection : TournamentSelection
    {
        #region Constants

        /// <summary>
        /// Default base 
        /// </summary>
        public new const string P_TOURNAMENT = "lexicographic-tournament";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return SelectDefaults.ParamBase.Push(P_TOURNAMENT); }
        }

        #endregion // Properties
        #region Comparison

        public bool BetterThan(Individual first, Individual second, int subpop, EvolutionState state, int thread)
        {
            return (first.Fitness.BetterThan(second.Fitness) ||
                (first.Fitness.EquivalentTo(second.Fitness) && first.Size < second.Size));
        }

        #endregion // Comparison
    }
}