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
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.SteadyState;

namespace BraneCloud.Evolution.EC.Parsimony
{
    /// <summary> 
    /// This selection method adds parsimony pressure to the regular tournament selection.  
    /// The comparison of individuals is based on fitness with probability <i>prob</i>, 
    /// and it is based on size with probability <i>1-prob</i>.  
    /// For each pairwise comparsion of individuals, the ProportionalTournamentSelection 
    /// randomly decides whether to compare based on fitness or size. 
    /// <p/>
    /// <b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// Always 1.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>size</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(the tournament size)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>pick-worst</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">(should we pick the <i>worst</i> individual in the tournament instead of the <i>best</i>?)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>fitness-prob</tt><br/>
    /// <font size="-1"> double &gt;= 0 and &lt;= 1</font></td>
    /// <td valign="top">(the probability of comparing individuals based on fitness, rather than size)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// select.proportional-tournament
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.parsimony.ProportionalTournamentSelection")]
    public class ProportionalTournamentSelection : TournamentSelection
    {
        #region Constants

        /// <summary>
        /// Default base
        /// </summary>
        public const string P_PROPORTIONAL_TOURNAMENT = "proportional-tournament";

        /// <summary>
        /// The parameter for the probability of having the tournament based on fitness
        /// </summary>
        public const string P_PROBABILITY = "fitness-prob";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return SelectDefaults.ParamBase.Push(P_PROPORTIONAL_TOURNAMENT); }
        }

        /// <summary>
        /// The probability of having the tournament based on fitness.
        /// </summary>
        public double FitnessPressureProb { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;

            FitnessPressureProb = state.Parameters.GetDouble(paramBase.Push(P_PROBABILITY), def.Push(P_PROBABILITY), 0.0);
            if (FitnessPressureProb < 0.0 || FitnessPressureProb > 1.0)
                state.Output.Fatal("Probability must be between 0.0 and 1.0",
                    paramBase.Push(P_PROBABILITY), def.Push(P_PROBABILITY));
        }

        #endregion // Setup
        #region Comparison

        public override bool BetterThan(Individual first, Individual second, int subpopulation, IEvolutionState state, int thread)
        {
            if (state.Random[thread].NextBoolean(FitnessPressureProb))
                return first.Fitness.BetterThan(second.Fitness);

            return first.Size < second.Size;
        }

        #endregion // Comparison
    }
}