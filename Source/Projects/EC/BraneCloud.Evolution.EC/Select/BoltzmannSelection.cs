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
using System.Linq;
using System.Text;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.Select
{
    /// <summary>
    /// Similar to FitProportionateSelection, but with a Simulated Annealing style twist. BoltzmannSelection picks individuals of a population in 
    /// proportion to an adjusted version of their fitnesses instead of their actual fitnesses as returned by fitness(). The adjusted fitness is 
    /// calculated by e^(fitness/current_temperature) where current_temperature is a temperature value that decreases by a constant cooling rate as 
    /// generations of evolution pass. The current_temperature is calculated by starting-temperature - (cooling-rate * the_current_generation_number). 
    /// When the temperature dips below 1.0, annealing ceases and BoltzmannSelection reverts to normal FitProportionateSelection behavior.
    /// 
    /// <p/>
    /// Like FitProportionateSelection this is not appropriate for steady-state evolution.
    /// If you're not familiar with the relative advantages of 
    /// selection methods and just want a good one,
    /// use TournamentSelection instead. Not appropriate for
    /// multiobjective fitnesses.
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
    /// <tr><td valign="top"><i>base.</i><tt>starting-temperature</tt><br/>
    /// <font size="-1">double = some large number (defaults to 1.0)</font></td>
    /// <td valign="top">(the starting temperature for our simulated annealing style adjusted fitness proportions)</td></tr>
    /// 
    /// <tr><td valign="top"><i>base.</i><tt>cooling-rate</tt><br/>
    /// <font size="-1"> double = some smaller number (defaults to 0.0 which causes BoltzmannSelection to behave just as FitProportionateSelection would)</font></td>
    /// <td valign="top">(how slow, or fast, do you want to cool the annealing fitness proportions?)</td></tr>
    /// 
    /// </table> 
    /// 
    /// <p/><b>Default Base</b><br/>
    /// select.boltzmann
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.select.BoltzmannSelection")]
    public class BoltzmannSelection : FitProportionateSelection
    {
        #region Constants

        /// <summary>
        /// Default base
        /// </summary>
        public const string P_BOLTZMANN = "boltzmann";

        /// <summary>
        /// Starting temperature parameter.
        /// </summary>
        public const string P_STARTING_TEMPERATURE = "starting-temperature";

        /// <summary>
        /// Cooling rate parameter.
        /// </summary>
        public const string P_COOLING_RATE = "cooling-rate";

        #endregion // Constants
        #region Fields

        /// <summary>
        /// Starting temperature.
        /// </summary>
        private double _startingTemperature;

        /// <summary>
        /// Cooling rate.
        /// </summary>
        private double _coolingRate;

        #endregion // Fields
        #region Properties

        public override IParameter DefaultBase
        {
            get { return SelectDefaults.ParamBase.Push(P_BOLTZMANN); }
        }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;

            _coolingRate = state.Parameters.GetDouble(paramBase.Push(P_COOLING_RATE), def.Push(P_COOLING_RATE)); // default cooling rate of 1.0 per generation
            _startingTemperature = state.Parameters.GetDouble(paramBase.Push(P_STARTING_TEMPERATURE), def.Push(P_STARTING_TEMPERATURE)); // default starting temp is 0.0/completely cooled - will act as normal fit proportionate selection

            if (_coolingRate <= 0)
            {
                //Hey! you gotta cool! Set your cooling rate to a positive value!
                state.Output.Fatal("Cooling rate should be a positive value.", paramBase.Push(P_COOLING_RATE), def.Push(P_COOLING_RATE));
            }

            if ((_startingTemperature - _coolingRate) <= 0)
            {
                // C'mon, you should cool slowly if you want boltzmann selection to be effective.
                state.Output.Fatal("For best results, try to set your temperature to cool to 0 a more slowly. This can be acheived by increasing your starting-temperature and/or decreasing your cooling rate.\nstarting-temperatire/cooling-rate: "
                    + _startingTemperature + " / " + _coolingRate);
            }

            var totalGenerations = state.NumGenerations;
            if (totalGenerations == 0)
            {
                //Load from parameter database!!
                state.Output.Fatal("Hey now, we gotta load the total_generations from the param DB");
            }

            if ((_startingTemperature - (_coolingRate * totalGenerations)) > 0)
            {
                //Either your cooling rate is to low, or your starting temp is too high, because at this rate you will never cool to 0! (kind of essential to reaping the benefits of boltzmann selection)
                state.Output.Warning("If you want BoltzmannnSelection to be effective, your temperature should cool to 0 before all generations have passed. Make sure that (starting-temperature - (cooling-rate * generations)) <= 0.");
            }

        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Completely override FitProportionateSelection.prepareToProduce.
        /// </summary>
        public override void PrepareToProduce(IEvolutionState s, int subpop, int thread)
        {
            // load fitnesses
            Fitnesses = new float[s.Population.Subpops[subpop].Individuals.Length];
            for (var x = 0; x < Fitnesses.Length; x++)
            {
                // adjust the fitness proportion according to current temperature.                
                Fitnesses[x] = (float)BoltzmannExpectedValue(s.Population.Subpops[subpop].Individuals[x].Fitness.Value, s);
                if (Fitnesses[x] < 0) // uh oh
                    s.Output.Fatal("Discovered a negative fitness value.  BoltzmannnSelection requires that all fitness values be non-negative(offending subpopulation #"
                        + subpop + ")");
            }

            // organize the distribution.  All zeros in fitness is fine
            RandomChoice.OrganizeDistribution(Fitnesses, true);
        }

        private double BoltzmannExpectedValue(double fitness, IEvolutionState s)
        {
            var currentTemperature = _startingTemperature - (_coolingRate * s.Generation);
            if (currentTemperature < 1.0)
                return fitness;
            return Math.Exp(fitness / currentTemperature);
        }

        #endregion // Operations
    }
}