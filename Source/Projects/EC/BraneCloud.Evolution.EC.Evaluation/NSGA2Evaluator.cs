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
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.MultiObjective;
using BraneCloud.Evolution.EC.MultiObjective.NSGA2;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.Evaluation
{
    /// <summary>
    /// The NSGA2Evaluator is a simple generational evaluator which
    /// evaluates every single member of the population (in a multithreaded fashion).
    /// Then it reduces the population size to an <i>archive</i> consisting of the
    /// best front ranks.  When there isn't enough space to fit another front rank,
    /// individuals in that final front rank vie for the remaining slots in the archive
    /// based on their sparsity.
    /// 
    /// <p/>The evaluator is also responsible for calculating the rank and
    /// sparsity values stored in the NSGA2MultiObjectiveFitness class and used largely
    /// for statistical information.
    /// 
    /// <p/>NSGA-II has fixed archive size (the population size), and so ignores the 'elites'
    /// declaration.  However it will adhere to the 'reevaluate-elites' parameter in SimpleBreeder
    /// to determine whether to force fitness reevaluation.
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.multiobjective.nsga2.NSGA2Evaluator")]
    public class NSGA2Evaluator : SimpleEvaluator, INSGA2Evaluator
    {
        #region Properties

        /// <summary>
        /// The original population size is stored here so NSGA2 knows how large to create the archive
        /// (it's the size of the original population -- keep in mind that NSGA2Breeder had made the 
        /// population larger to include the children.
        /// </summary>
        public int[] OriginalPopSize { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            IParameter p = new Parameter(Initializer.P_POP);
            var subpopsLength = state.Parameters.GetInt(p.Push(Population.P_SIZE), null, 1);
            IParameter p_subpop;
            OriginalPopSize = new int[subpopsLength];

            for (var i = 0; i < subpopsLength; i++)
            {
                p_subpop = p.Push(Population.P_SUBPOP).Push("" + i).Push(Subpopulation.P_SUBPOPSIZE);
                OriginalPopSize[i] = state.Parameters.GetInt(p_subpop, null, 1);
            }
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Evaluates the population, then builds the archive and reduces the population to just the archive.
        /// </summary>
        /// <param name="state"></param>
        public override void EvaluatePopulation(IEvolutionState state)
        {
            base.EvaluatePopulation(state);
            for (var x = 0; x < state.Population.Subpops.Count; x++)
                state.Population.Subpops[x].Individuals = BuildArchive(state, x);
        }

        /// <summary>
        /// Build the auxiliary fitness data and reduce the subpopulation to just the archive, which is returned.
        /// </summary>
        public Individual[] BuildArchive(IEvolutionState state, int subpop)
        {
            var ranks = AssignFrontRanks(state.Population.Subpops[subpop]);

            var newSubpopulation = new List<Individual>();
            var size = ranks.Count;
            for (var i = 0; i < size; i++)
            {
                var rank = ranks[i].ToArray();
                AssignSparsity(rank);
                if (rank.Length + newSubpopulation.Count >= OriginalPopSize[subpop])
                {
                    // first sort the rank by sparsity
                    QuickSort.QSort(rank, new NSGA2MultiObjectiveFitnessComparator());

                    // then put the m sparsest individuals in the new population
                    var m = OriginalPopSize[subpop] - newSubpopulation.Count;
                    for (var j = 0; j < m; j++)
                        newSubpopulation.Add(rank[j]);

                    // and bail
                    break;
                }

                // dump in everyone
                newSubpopulation.AddRange(rank);
            }

            var archive = newSubpopulation.ToArray();

            // maybe force reevaluation
            var breeder = (NSGA2Breeder) state.Breeder;
            if (breeder.ReevaluateElites[subpop])
                foreach (var t in archive)
                    t.Evaluated = false;

            return archive;
        }

        /// <summary>
        /// Divides inds into ranks and assigns each individual's rank to be the rank it was placed into.
        /// Each front is an ArrayList.
        /// </summary>
        /// <param name="subpop"></param>
        /// <returns></returns>
        public IList<IList<Individual>> AssignFrontRanks(Subpopulation subpop)
        {
            var inds = subpop.Individuals;
            var frontsByRank = MultiObjectiveFitness.PartitionIntoRanks(inds);

            var numRanks = frontsByRank.Count;
            for (var rank = 0; rank < numRanks; rank++)
            {
                var front = frontsByRank[rank];
                var numInds = front.Count;
                for (var ind = 0; ind < numInds; ind++)
                    ((NSGA2MultiObjectiveFitness) front[ind].Fitness).Rank = rank;
            }
            return frontsByRank;
        }

        /// <summary>
        /// Computes and assigns the sparsity values of a given front.
        /// </summary>
        /// <param name="front"></param>
        public void AssignSparsity(Individual[] front)
        {
            var numObjectives = ((NSGA2MultiObjectiveFitness)front[0].Fitness).GetObjectives().Length;

            for (var i = 0; i < front.Length; i++)
                ((NSGA2MultiObjectiveFitness) front[i].Fitness).Sparsity = 0;

            for (var i = 0; i < numObjectives; i++)
            {
                var o = i;
                // 1. Sort front by each objective.
                // 2. Sum the manhattan distance of an individual's neighbours over
                // each objective.
                // NOTE: No matter which objectives objective you sort by, the
                // first and last individuals will always be the same (they maybe
                // interchanged though). This is because a Pareto front's
                // objective values are strictly increasing/decreasing.
                QuickSort.QSort(front, new MultiObjectiveFitnessComparator());

                // Compute and assign sparsity.
                // the first and last individuals are the sparsest.
                ((NSGA2MultiObjectiveFitness) front[0].Fitness).Sparsity = Double.PositiveInfinity;
                ((NSGA2MultiObjectiveFitness) front[front.Length - 1].Fitness).Sparsity = Double.PositiveInfinity;
                for (var j = 1; j < front.Length - 1; j++)
                {
                    var f_j = (NSGA2MultiObjectiveFitness) front[j].Fitness;
                    var f_jplus1 = (NSGA2MultiObjectiveFitness) front[j + 1].Fitness;
                    var f_jminus1 = (NSGA2MultiObjectiveFitness) front[j - 1].Fitness;

                    // store the NSGA2Sparsity in sparsity
                    f_j.Sparsity += (f_jplus1.GetObjective(o) - f_jminus1.GetObjective(o)) / (f_j.MaxObjective[o] - f_j.MinObjective[o]);
                }
            }
        }

        #endregion // Operations
    }
}