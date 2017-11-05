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
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.MultiObjective.SPEA2
{
    /// <summary>
    /// Replaces earlier class by: Robert Hubley, with revisions by Gabriel Balan and Keith Sullivan
    /// 
    /// This subclass of SimpleBreeder overrides the loadElites method to build an archive in the top elites[subpopnum]
    /// of each subpopulation.  It computes the sparsity metric, then constructs the archive.
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.multiobjective.spea2.SPEA2Breeder")]
    public class SPEA2Breeder : SimpleBreeder
    {
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            for (int i = 0; i < state.Population.Subpops.Length; i++)
                if (ReduceBy[i] != 0)
                    state.Output.Fatal("SPEA2Breeder does not support population reduction.", paramBase.Push(P_REDUCE_BY).Push("" + i), null);

            if (SequentialBreeding) // uh oh, haven't tested with this
                state.Output.Fatal("SPEA2Breeder does not support sequential evaluation.",
                                   paramBase.Push(P_SEQUENTIAL_BREEDING));

            if (!ClonePipelineAndPopulation)
                state.Output.Fatal("ClonePipelineAndPopulation must be true for SPEA2Breeder.");
        }

        #endregion // Setup
        #region Operations

        protected override void LoadElites(IEvolutionState state, Population newpop)
        {
            // are our elites small enough?
            for (var x = 0; x < state.Population.Subpops.Length; x++)
                if (NumElites(state, x) > state.Population.Subpops[x].Individuals.Length)
                    state.Output.Error("The number of elites for subpopulation " + x + " exceeds the actual size of the subpopulation",
                        new Parameter(EvolutionState.P_BREEDER).Push(P_ELITE).Push("" + x));
            state.Output.ExitIfErrors();

            // do it
            for (var sub = 0; sub < state.Population.Subpops.Length; sub++)
            {
                var newInds = newpop.Subpops[sub].Individuals;  // The new population after we are done picking the elites                 
                var oldInds = state.Population.Subpops[sub].Individuals;   // The old population from which to pick elites

                BuildArchive(state, oldInds, newInds, NumElites(state, sub));
            }

            // optionally force reevaluation
            UnmarkElitesEvaluated(state, newpop);
        }

        public double[] CalculateDistancesFromIndividual(Individual ind, Individual[] inds)
        {
            var d = new double[inds.Length];
            for (var i = 0; i < inds.Length; i++)
                d[i] = ((SPEA2MultiObjectiveFitness)ind.Fitness).SumSquaredObjectiveDistance((SPEA2MultiObjectiveFitness)inds[i].Fitness);
            // now sort
            Array.Sort(d);
            return d;
        }

        public void BuildArchive(IEvolutionState state, Individual[] oldInds, Individual[] newInds, int archiveSize)
        {
            // step 1: load the archive with the pareto-nondominated front
            var archive = new List<Individual>();
            var nonFront = new List<Individual>();
            MultiObjectiveFitness.PartitionIntoParetoFront(oldInds, archive, nonFront);
            var currentArchiveSize = archive.Count;

            // step 2: if the archive isn't full, load the remainder with the fittest individuals (using customFitnessMetric) that aren't in the archive yet
            if (currentArchiveSize < archiveSize)
            {
                // BRS : The following uses Individual's IComparable implementation based on Fitness.
                // The fitter individuals will be earlier
                nonFront.Sort();
                var len = (archiveSize - currentArchiveSize);
                for (var i = 0; i < len; i++)
                {
                    archive.Add(nonFront[i]);
                    currentArchiveSize++;
                }
            }

            // step 3: if the archive is OVERFULL, iterate as follows:
            //              step 3a: remove the k-closest individual in the archive
            //var evaluator = ((ISPEA2Evaluator)(state.Evaluator));
            //var inds = archive.ToArray();

            while (currentArchiveSize > archiveSize)
            {
                var closest = archive[0];
                var closestIndex = 0;
                var closestD = CalculateDistancesFromIndividual(closest, oldInds);

                for (var i = 1; i < currentArchiveSize; i++)
                {
                    var competitor = archive[i];
                    var competitorD = CalculateDistancesFromIndividual(competitor, oldInds);

                    for (var k = 0; k < oldInds.Length; k++)
                    {
                        if (closestD[i] > competitorD[i])
                        {
                            closest = competitor;
                            closestD = competitorD;
                            closestIndex = k;
                            break;
                        }
                        else if (closestD[i] < competitorD[i])
                        { break; }
                    }
                }

                // remove him destructively -- put the top guy in his place and remove the top guy.  This is O(1)
                archive[closestIndex] = archive[archive.Count - 1];
                archive.RemoveAt(archive.Count - 1);

                currentArchiveSize--;
            }

            // step 4: put clones of the archive in the new individuals
            var arr = archive.ToArray();
            for (var i = 0; i < archiveSize; i++)
                newInds[newInds.Length - archiveSize + i] = (Individual)arr[i].Clone();
        }

        #endregion // Operations
    }
}