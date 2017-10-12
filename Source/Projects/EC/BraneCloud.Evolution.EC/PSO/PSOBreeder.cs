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
    /// <summary> 
    /// PSOBreeder
    /// 
    /// <p/>The PSOBreeder performs the calculations to determine new particle locations
    /// and performs the bookkeeping to keep track of personal, neighborhood, and global
    /// best solutions.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>debug-info</tt><br/>
    /// <font size="-1">boolean</font></td>
    /// <td valign="top">(whether the system should display information useful for debugging purposes)<br/>
    /// </td></tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.pso.PSOBreeder")]
    public class PSOBreeder : Breeder
    {
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // intentionally empty
        }

        #endregion // Setup
        #region Operations

        public override Population BreedPopulation(IEvolutionState state)
        {
            var subpop = (PSOSubpopulation)state.Population.Subpops[0];

            // update bests
            AssignPersonalBests(subpop);
            AssignNeighborhoodBests(subpop);
            AssignGlobalBest(subpop);

            // make a temporary copy of locations so we can modify the current location on the fly
            var tempClone = new DoubleVectorIndividual[subpop.Individuals.Length];
            Array.Copy(subpop.Individuals, 0, tempClone, 0, subpop.Individuals.Length);

            // update particles             
            for (var i = 0; i < subpop.Individuals.Length; i++)
            {
                var ind = (DoubleVectorIndividual)subpop.Individuals[i].Clone();
                var prevInd = subpop.PreviousIndividuals[i];
                // the individual's personal best
                var pBest = subpop.PersonalBests[i];
                // the individual's neighborhood best
                var nBest = subpop.NeighborhoodBests[i];
                // the individuals's global best
                var gBest = subpop.GlobalBest;

                // calculate update for each dimension in the genome
                for (var j = 0; j < ind.GenomeLength; j++)
                {
                    var velocity = ind.genome[j] - prevInd.genome[j];
                    var pDelta = pBest.genome[j] - ind.genome[j]; // difference to personal best
                    var nDelta = nBest.genome[j] - ind.genome[j]; // difference to neighborhood best
                    var gDelta = gBest.genome[j] - ind.genome[j]; // difference to global best
                    var pWeight = state.Random[0].NextDouble(); // weight for personal best
                    var nWeight = state.Random[0].NextDouble(); // weight for neighborhood best
                    var gWeight = state.Random[0].NextDouble(); // weight for global best

                    ind.genome[j] += velocity * subpop.VelocityMultiplier + subpop.pFactor * pWeight * pDelta + subpop.nFactor * nWeight * nDelta + subpop.gFactor * gWeight * gDelta;
                }

                subpop.Individuals[i] = ind;

                if (subpop.ClampRange)
                    ind.Clamp();
            }

            // update previous locations
            subpop.PreviousIndividuals = tempClone;

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
            for (var j = 0; j < subpop.Individuals.Length; j++)
            {
                var hoodBest = subpop.NeighborhoodBests[j];
                var start = (j - subpop.NeighborhoodSize / 2);
                if (start < 0)
                    start += subpop.Individuals.Length;

                for (var i = 0; i < subpop.NeighborhoodSize; i++)
                {
                    var ind = (DoubleVectorIndividual)subpop.Individuals[(start + i) % subpop.Individuals.Length];
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