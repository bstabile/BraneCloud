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
using System.IO;
using System.Linq;
using System.Text;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.MultiObjective.NSGA2
{
    /// <summary>
    /// NSGA2MultiObjectiveFitness is a subclass of MultiObjeciveFitness which
    /// adds auxiliary fitness measures (sparsity, rank) largely used by MultiObjectiveStatistics.
    /// It also redefines the comparison measures to compare based on rank, and break ties
    /// based on sparsity. 
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.multiobjective.nsga2.NSGA2MultiObjectiveFitness")]
    public class NSGA2MultiObjectiveFitness : MultiObjectiveFitness
    {
        #region Constants

        public const string NSGA2_RANK_PREAMBLE = "Rank: ";
        public const string NSGA2_SPARSITY_PREAMBLE = "Sparsity: ";

        #endregion // Constants
        #region Static

        public override string[] GetAuxilliaryFitnessNames() { return new[] { "Rank", "Sparsity" }; }
        public override double[] GetAuxilliaryFitnessValues() { return new[] { Rank, Sparsity }; }

        #endregion // Static
        #region Properties

        /// <summary>
        /// Pareto front rank measure (lower ranks are better).
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Sparsity along front rank measure (higher sparsity is better).
        /// </summary>
        public double Sparsity { get; set; }

        #endregion // Properties
        #region Comparison

        public override bool EquivalentTo(IFitness fitness)
        {
            var other = (NSGA2MultiObjectiveFitness)fitness;
            return (Rank == ((NSGA2MultiObjectiveFitness)fitness).Rank) &&
                (Sparsity == other.Sparsity);
        }

        /// <summary>
        /// We specify the tournament selection criteria, Rank (lower
        /// values are better) and Sparsity (higher values are better)
        /// </summary>
        /// <param name="fitness"></param>
        /// <returns></returns>
        public override bool BetterThan(IFitness fitness)
        {
            var other = (NSGA2MultiObjectiveFitness)fitness;
            // Rank should always be minimized.
            if (Rank < ((NSGA2MultiObjectiveFitness)fitness).Rank)
                return true;
            if (Rank > ((NSGA2MultiObjectiveFitness)fitness).Rank)
                return false;

            // otherwise try sparsity
            return (Sparsity > other.Sparsity);
        }

        #endregion // Comparison
        #region ToString

        public override string FitnessToString()
        {
            return base.FitnessToString() + "\n" + NSGA2_RANK_PREAMBLE + Code.Encode(Rank) + "\n" + NSGA2_SPARSITY_PREAMBLE + Code.Encode(Sparsity);
        }

        public override string FitnessToStringForHumans()
        {
            return base.FitnessToStringForHumans() + "\n" + NSGA2_RANK_PREAMBLE + Rank + "\n" + NSGA2_SPARSITY_PREAMBLE + Sparsity;
        }

        #endregion // ToString
        #region IO

        public override void ReadFitness(IEvolutionState state, StreamReader reader)
        {
            base.ReadFitness(state, reader);
            Rank = Code.ReadIntWithPreamble(NSGA2_RANK_PREAMBLE, state, reader);
            Sparsity = Code.ReadDoubleWithPreamble(NSGA2_SPARSITY_PREAMBLE, state, reader);
        }

        public override void WriteFitness(IEvolutionState state, BinaryWriter writer)
        {
            base.WriteFitness(state, writer);
            writer.Write(Rank);
            writer.Write(Sparsity);
            WriteTrials(state, writer);
        }

        public override void ReadFitness(IEvolutionState state, BinaryReader reader)
        {
            base.ReadFitness(state, reader);
            Rank = reader.ReadInt32();
            Sparsity = reader.ReadDouble();
            ReadTrials(state, reader);
        }

        #endregion // IO
    }
}