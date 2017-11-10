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

using BraneCloud.Evolution.EC.Vector;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.PSO
{
    /**
     * PSOBreeder is a simple single-threaded Breeder which performs 
     * Particle Swarm Optimization using the Particle class as individuals. 
     * PSOBreeder relies on a number of parameters which define weights for
     * various vectors computed during Particle Swarm Optimization, plus
     * a few flags:
     *
     * <ul>
     * <li> Neighborhoods for particles have a size S determined by the parameter Neighborhood-size.  It's best if S were even.
     * <li> Neighborhoods for particles are constructed in one of three ways:
     * <ul>
     * <li> random: pick S informants randomly without replacement within the subpopulation, not including the particle itself, once at the beginning of the run.
     * <li> random-each-time: pick S informants randomly without replacement within the subpopulation, not including the particle itself, every single generation.
     * <li> toroidal: pick the floor(S/2) informants to the left of the particle's location within the subpopulation and the ceiling(S/2) informants to the right of the particle's location in the subpopulation, once at the beginning of the run.
     * </ul>
     * <li> To this you can add the particle itself to the Neighborhood, with include-self. 
     * <li> The basic velocity update equation is VELOCITY <-- (VELOCITY * velocity-coefficent) + (VECTOR-TO-GLOBAL-BEST * global-coefficient) + (VECTOR-TO-NEIGHBORHOOD-BEST * informant-coefficient) + (VECTOR-TO-PERSONAL-BEST * personal-coefficient)
     * <li> The basic particle update equation is PARTICLE <-- PARTICLE + VELOCITY
     * </ul>
     *
     * <p>
     * <b>Parameters</b><br>
     * <table>
     * <tr>
     * <td valign=top><i>base</i>.<tt>velocity-coefficient</tt><br>
     *  <font size=-1>float &ge; 0</font></td>
     *  <td valign=top>(The weight for the velocity)</td>
     * </tr><tr>
     * <td valign=top><i>base</i>.<tt>personal-coefficient</tt><br>
     *  <font size=-1>float &ge; 0</font></td>
     *  <td valign=top>(The weight for the personal-best vector)</td>
     * </tr><tr>
     * <td valign=top><i>base</i>.<tt>informant-coefficient</tt><br>
     *  <font size=-1>float &ge; 0</font></td>
     *  <td valign=top>(The weight for the Neighborhood/informant-best vector)</td>
     * </tr><tr>
     * <td valign=top><i>base</i>.<tt>global-coefficient</tt><br>
     *  <font size=-1>float &ge; 0</font></td>
     *  <td valign=top>(The weight for the global-best vector)</td>
     * </tr><tr>
     * <td valign=top><i>base</i>.<tt>Neighborhood-size</tt><br>
     *  <font size=-1>int &gt; 0</font></td>
     *  <td valign=top>(The size of the Neighborhood of informants, not including the particle)</td>
     * </tr><tr>
     * <td valign=top><i>base</i>.<tt>Neighborhood-style</tt><br>
     *  <font size=-1>String, one of: random toroidal random-each-time</font></td>
     *  <td valign=top>(The method of generating the Neighborhood of informants, not including the particle)</td>
     * </tr><tr>
     * <td valign=top><i>base</i>.<tt>include-self</tt><br>
     *  <font size=-1>true or false (default)</font></td>
     *  <td valign=top>(Whether to include the particle itself as a member of the Neighborhood after building the Neighborhood)</td>
     * </tr>
     *
     * </table>
     *
     * @author Khaled Ahsan Talukder
     */

    [Serializable]
    [ECConfiguration("ec.pso.PSOBreeder")]
    public class PSOBreeder : Breeder
    {
        #region Constants

        public const int C_NEIGHBORHOOD_RANDOM = 0;
        public const int C_NEIGHBORHOOD_TOROIDAL = 1;
        public const int C_NEIGHBORHOOD_RANDOM_EACH_TIME = 2;

        public const string P_VELOCITY_COEFFICIENT = "velocity-coefficient" ;
        public const string P_PERSONAL_COEFFICIENT = "personal-coefficient" ;
        public const string P_INFORMANT_COEFFICIENT = "informant-coefficient" ;
        public const string P_GLOBAL_COEFFICIENT = "global-coefficient" ;
        public const string P_INCLUDE_SELF = "include-self" ;
        public const string P_NEIGHBORHOOD = "Neighborhood-style" ;
        public const string P_NEIGHBORHOOD_SIZE = "Neighborhood-size" ;
        public const string V_NEIGHBORHOOD_RANDOM = "random";
        public const string V_NEIGHBORHOOD_TOROIDAL = "toroidal";
        public const string V_NEIGHBORHOOD_RANDOM_EACH_TIME = "random-each-time";

        #endregion

        #region Properties

        public int Neighborhood { get; set; } = C_NEIGHBORHOOD_RANDOM;        // default Neighborhood scheme
        public double VelCoeff { get; set; } = 0.5;          //  coefficient for the velocity
        public double PersonalCoeff { get; set; } = 0.5;             //  coefficient for self
        public double InformantCoeff { get; set; } = 0.5;            //  coefficient for informants/neighbours
        public double GlobalCoeff { get; set; } = 0.5;               //  coefficient for global best, this is not done in the standard PSO
        public int NeighborhoodSize { get; set; } = 3;
        public bool IncludeSelf { get; set; } = false;

        public double[][] GlobalBest { get; set; } = null; // one for each subpopulation
        public IFitness[] GlobalBestFitness { get; set; } = null;

        #endregion

        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            VelCoeff = state.Parameters.GetDouble(paramBase.Push(P_VELOCITY_COEFFICIENT), null, 0.0);
            if (VelCoeff < 0.0)
                state.Output.Fatal("Parameter not found, or its value is less than 0.",
                    paramBase.Push(P_VELOCITY_COEFFICIENT), null);

            PersonalCoeff = state.Parameters.GetDouble(paramBase.Push(P_PERSONAL_COEFFICIENT), null, 0.0);
            if (PersonalCoeff < 0.0)
                state.Output.Fatal("Parameter not found, or its value is less than 0.",
                    paramBase.Push(P_PERSONAL_COEFFICIENT), null);

            InformantCoeff = state.Parameters.GetDouble(paramBase.Push(P_INFORMANT_COEFFICIENT), null, 0.0);
            if (InformantCoeff < 0.0)
                state.Output.Fatal("Parameter not found, or its value is less than 0.",
                    paramBase.Push(P_INFORMANT_COEFFICIENT), null);

            GlobalCoeff = state.Parameters.GetDouble(paramBase.Push(P_GLOBAL_COEFFICIENT), null, 0.0);
            if (GlobalCoeff < 0.0)
                state.Output.Fatal("Parameter not found, or its value is less than 0.",
                    paramBase.Push(P_GLOBAL_COEFFICIENT), null);

            NeighborhoodSize = state.Parameters.GetInt(paramBase.Push(P_NEIGHBORHOOD_SIZE), null, 1);
            if (NeighborhoodSize <= 0)
                state.Output.Fatal("Neighbourhood size must be a value >= 1.", paramBase.Push(P_NEIGHBORHOOD_SIZE),
                    null);

            string sch = state.Parameters.GetString(paramBase.Push(P_NEIGHBORHOOD), null);
            if (V_NEIGHBORHOOD_RANDOM.Equals(sch))
            {
                Neighborhood = C_NEIGHBORHOOD_RANDOM; // default anyway
            }
            else if (V_NEIGHBORHOOD_TOROIDAL.Equals(sch))
            {
                Neighborhood = C_NEIGHBORHOOD_TOROIDAL;
            }
            else if (V_NEIGHBORHOOD_RANDOM_EACH_TIME.Equals(sch))
            {
                Neighborhood = C_NEIGHBORHOOD_RANDOM_EACH_TIME;
            }
            else
                state.Output.Fatal("Neighborhood style must be either 'random', 'toroidal', or 'random-each-time'.",
                    paramBase.Push(P_NEIGHBORHOOD), null);

            IncludeSelf = state.Parameters.GetBoolean(paramBase.Push(P_INCLUDE_SELF), null, false);
        }

        #endregion // Setup
        #region Operations

        public override Population BreedPopulation(IEvolutionState state)
        {
            // initialize the global best
            if (GlobalBest == null)
            {
                GlobalBest = new double[state.Population.Subpops.Count][];
                GlobalBestFitness = new IFitness[state.Population.Subpops.Count];
            }

            // update global best, neighborhood best, and personal best 
            for (int subpop = 0; subpop < state.Population.Subpops.Count; subpop++)
            {
                for (int ind = 0; ind < state.Population.Subpops[subpop].Individuals.Count; ind++)
                {
                    if (GlobalBestFitness[subpop] == null ||
                        state.Population.Subpops[subpop].Individuals[ind].Fitness.BetterThan(GlobalBestFitness[subpop]))
                    {
                        GlobalBest[subpop] = ((DoubleVectorIndividual)state.Population.Subpops[subpop].Individuals[ind]).genome;
                        GlobalBestFitness[subpop] = state.Population.Subpops[subpop].Individuals[ind].Fitness;
                    }
                    ((Particle)state.Population.Subpops[subpop].Individuals[ind]).Update(state, subpop, ind, 0);
                }
                // clone global best
                GlobalBest[subpop] = (double[])(GlobalBest[subpop].Clone());
                GlobalBestFitness[subpop] = (Fitness)(GlobalBestFitness[subpop].Clone());
            }


            // now move the particles
            for (int subpop = 0; subpop < state.Population.Subpops.Count; subpop++)
            {
                for (int ind = 0; ind < state.Population.Subpops[subpop].Individuals.Count; ind++)
                    // tweak in place, destructively
                    ((Particle)state.Population.Subpops[subpop].Individuals[ind]).Tweak(state, GlobalBest[subpop],
                        VelCoeff, PersonalCoeff, InformantCoeff, GlobalCoeff, 0);
            }

            // we return the same population
            return state.Population;
        }

        public virtual void AssignPersonalBests(PSOSubpopulation subpop)
        {
            for (var i = 0; i < subpop.PersonalBests.Length; i++)
                if ((subpop.PersonalBests[i] == null) || subpop.Individuals[i].Fitness.BetterThan(subpop.PersonalBests[i].Fitness))
                    subpop.PersonalBests[i] = (DoubleVectorIndividual)subpop.Individuals[i].Clone();
        }

        public virtual void AssignNeighborhoodBests(PSOSubpopulation subpop)
        {
            for (var j = 0; j < subpop.Individuals.Count; j++)
            {
                var hoodBest = subpop.NeighborhoodBests[j];
                var start = (j - subpop.NeighborhoodSize / 2);
                if (start < 0)
                    start += subpop.Individuals.Count;

                for (var i = 0; i < subpop.NeighborhoodSize; i++)
                {
                    var ind = (DoubleVectorIndividual)subpop.Individuals[(start + i) % subpop.Individuals.Count];
                    if ((hoodBest == null) || ind.Fitness.BetterThan(hoodBest.Fitness))
                        hoodBest = ind;
                }

                if (hoodBest != subpop.NeighborhoodBests[j])
                    subpop.NeighborhoodBests[j] = (DoubleVectorIndividual)hoodBest.Clone();
            }
        }

        public virtual void AssignGlobalBest(PSOSubpopulation subpop)
        {
            var globalBest = subpop.GlobalBest;
            foreach (var t in subpop.Individuals)
            {
                var ind = (DoubleVectorIndividual)t;
                if ((globalBest == null) || ind.Fitness.BetterThan(globalBest.Fitness))
                    globalBest = ind;
            }
            if (globalBest != subpop.GlobalBest)
                subpop.GlobalBest = (DoubleVectorIndividual)globalBest.Clone();
        }

        #endregion // Operations
    }
}