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
    /// Picks individuals in a population in direct proportion to their
    /// fitnesses as returned by their fitness() methods.  This is expensive to
    /// set up and bring down, so it's not appropriate for steady-state evolution.
    /// If you're not familiar with the relative advantages of 
    /// selection methods and just want a good one,
    /// use TournamentSelection instead.   Not appropriate for
    /// multiobjective fitnesses.
    /// 
    /// <p/><b><font color="red">
    /// Note: Fitnesses must be non-negative.  0 is assumed to be the worst fitness.
    /// </font></b>
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// Always 1.
    /// <p/><b>Default Base</b><br/>
    /// select.Fitness-proportionate
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.select.FitProportionateSelection")]
    public class FitProportionateSelection : SelectionMethod
    {
        #region Constants

        /// <summary>
        /// Default base. 
        /// </summary>
        public const string P_FITNESSPROPORTIONATE = "fitness-proportionate";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return SelectDefaults.ParamBase.Push(P_FITNESSPROPORTIONATE); }
        }

        /// <summary>
        /// Normalized, totalized fitnesses for the population.
        /// </summary>
        public float[] Fitnesses { get; set; }

        #endregion // Properties

        // don't need clone etc. 

        #region Operations

        public override void PrepareToProduce(IEvolutionState s, int subpop, int thread)
        {
            // load sortedFit
            Fitnesses = new float[s.Population.Subpops[subpop].Individuals.Length];
            for (var x = 0; x < Fitnesses.Length; x++)
            {
                Fitnesses[x] = s.Population.Subpops[subpop].Individuals[x].Fitness.Value;
                if (Fitnesses[x] < 0)
                    // uh oh
                    s.Output.Fatal("Discovered a negative fitness value."
                        + "  FitProportionateSelection requires that all fitness values be non-negative(offending subpop #" + subpop + ")");
            }

            // organize the distribution.  All zeros in fitness is fine
            RandomChoice.OrganizeDistribution(Fitnesses, true);
        }

        public override int Produce(int subpop, IEvolutionState state, int thread)
        {
            // Pick and return an individual from the population
            return RandomChoice.PickFromDistribution(Fitnesses, state.Random[thread].NextFloat());
        }

        public override void FinishProducing(IEvolutionState s, int subpop, int thread)
        {
            // release the distributions so we can quickly 
            // garbage-collect them if necessary
            Fitnesses = null;
        }

        #endregion // Operations
    }
}