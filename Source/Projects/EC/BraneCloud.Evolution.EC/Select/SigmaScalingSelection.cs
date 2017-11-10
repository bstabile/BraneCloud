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
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.Select
{
    /// <summary>
    /// Similar to FitProportionateSelection, but with adjustments to scale up/exaggerate differences 
    /// in fitness for selection when true fitness values are very close to  eachother across the population. 
    /// This addreses a common problem with FitProportionateSelection  wherein selection approaches 
    /// random selection during  late runs when fitness values do not differ by much.
    /// 
    /// <p/>
    /// Like FitProportionateSelection this is not appropriate for steady-state evolution.
    /// If you're not familiar with the relative advantages of  selection methods and just want a good one,
    /// use TournamentSelection instead. Not appropriate for multiobjective fitnesses.
    /// 
    /// <p/><b><font color="red">
    /// Note: Fitnesses must be non-negative.  0 is assumed to be the worst fitness.
    /// </font></b>
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// Always 1.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>scaled-fitness-floor</tt><br/>
    /// <font size="-1">double = some small number (defaults to 0.1)</font></td>
    /// <td valign="top">(The sigma scaling formula sometimes returns negative values. 
    /// This is unacceptable for fitness proportionate style selection so we must substitute 
    /// the fitnessFloor (some value >= 0) for the sigma scaled fitness when that sigma scaled fitness &lt;= fitnessFloor.)</td></tr>
    /// </table> 
    /// 
    /// <p/><b>Default Base</b><br/>
    /// select.sigma-scaling
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.select.SigmaScalingSelection")]
    public class SigmaScalingSelection : FitProportionateSelection
    {
        #region Constants

        /// <summary>
        /// Default base.
        /// </summary>
        public const string P_SIGMA_SCALING = "sigma-scaling";

        /// <summary>
        /// Scaled fitness floor.
        /// Used as a cut-off point when negative valued scaled fitnesses are encountered 
        /// (negative fitness values are not compatible with fitness proportionate style selection methods)
        /// </summary>
        public const string P_SCALED_FITNESS_FLOOR = "scaled-fitness-floor";

        #endregion // Constants
        #region Fields

        /// <summary>
        /// Floor for sigma scaled fitnesses.
        /// </summary>
        private double _fitnessFloor;

        #endregion // Fields
        #region Properties

        public override IParameter DefaultBase => SelectDefaults.ParamBase.Push(P_SIGMA_SCALING);

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;

            _fitnessFloor = state.Parameters.GetDoubleWithDefault(
                paramBase.Push(P_SCALED_FITNESS_FLOOR), def.Push(P_SCALED_FITNESS_FLOOR), 0.1); // default scaled fitness floor of 0.1 according to Tanese (1989)

            if (_fitnessFloor < 0)
            {
                //Hey! you gotta cool!  Set your cooling rate to a positive value!
                state.Output.Fatal("The scaled-fitness-floor must be a non-negative value.",
                    paramBase.Push(P_SCALED_FITNESS_FLOOR), def.Push(P_SCALED_FITNESS_FLOOR));
            }
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Completely override FitProportionateSelection.prepareToProduce
        /// </summary>
        public override void PrepareToProduce(IEvolutionState s, int subpop, int thread)
        {
            base.PrepareToProduce(s, subpop, thread);

            // load fitnesses
            Fitnesses = new double[s.Population.Subpops[subpop].Individuals.Count];

            double meanSum = 0;
            double squaredDeviationsSum = 0;

            for (var x = 0; x < Fitnesses.Length; x++)
            {
                Fitnesses[x] = s.Population.Subpops[subpop].Individuals[x].Fitness.Value;
                if (Fitnesses[x] < 0) // uh oh
                    s.Output.Fatal("Discovered a negative fitness value.  SigmaScalingSelection requires that all fitness values be non-negative(offending subpopulation #"
                        + subpop + ")");
            }

            // Calculate meanFitness
            for (var x = 0; x < Fitnesses.Length; x++)
            {
                meanSum = meanSum + Fitnesses[x];
            }
            var meanFitness = meanSum / Fitnesses.Length;

            // Calculate sum of squared deviations
            for (var x = 0; x < Fitnesses.Length; x++)
            {
                squaredDeviationsSum = squaredDeviationsSum + Math.Pow(Fitnesses[x] - meanFitness, 2);
            }
            var sigma = Math.Sqrt(squaredDeviationsSum / (Fitnesses.Length - 1));

            // Fill fitnesses[] with sigma scaled fitness values
            for (var x = 0; x < Fitnesses.Length; x++)
            {
                Fitnesses[x] = (double)SigmaScaledValue(Fitnesses[x], meanFitness, sigma, s); // adjust the fitness proportion according to sigma scaling.

                // Sigma scaling formula can return negative values, this is unacceptable for fitness proportionate style selection...
                // so we must substitute the fitnessFloor (some value >= 0) when a sigma scaled fitness <= fitnessFloor is encountered.
                if (Fitnesses[x] < _fitnessFloor)
                    Fitnesses[x] = _fitnessFloor;
            }

            // organize the distribution.  All zeros in fitness is fine
            RandomChoice.OrganizeDistribution(Fitnesses, true);
        }

        private static double SigmaScaledValue(double fitness, double meanFitness, double sigma, IEvolutionState s)
        {
            if (!sigma.Equals(0))
                return 1 + (fitness - meanFitness) / (2 * sigma);
            return 1.0;
        }

        #endregion // Operations
    }
}