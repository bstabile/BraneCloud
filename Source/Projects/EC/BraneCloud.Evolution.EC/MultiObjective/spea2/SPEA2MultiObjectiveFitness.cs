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
using System.IO;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.MultiObjective.SPEA2
{
    /// <summary>
    /// Replaces earlier class by: Robert Hubley, with revisions by Gabriel Balan and Keith Sullivan
    /// 
    /// SPEA2MultiObjectiveFitness is a subclass of MultiObjectiveFitness which adds three auxiliary fitness
    /// measures used in SPEA2: strength S(i), kthNNDistance D(i), and a fitness value R(i) + D(i). 
    /// Note that so-called "raw fitness" (what Sean calls "Wimpiness" in Essentials of Metaheuristics) is not retained.
    /// <p/>The fitness comparison operators solely use the 'fitness' value R(i) + D(i).
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.multiobjective.spea2.SPEA2MultiObjectiveFitness")]
    public class SPEA2MultiObjectiveFitness : MultiObjectiveFitness
    {
        #region Constants

        public const string SPEA2_FITNESS_PREAMBLE = "Fitness: ";
        public const string SPEA2_STRENGTH_PREAMBLE = "Strength: ";
        public const string SPEA2_DISTANCE_PREAMBLE = "Distance: ";

        #endregion // Constants
        #region Properties

        /// <summary>
        /// SPEA2 strength (# of nodes it dominates).
        /// S(i).
        /// </summary>
        public double Strength { get; set; }

        /// <summary>
        /// SPEA2 NN distance.
        /// D(i).
        /// </summary>
        public double KthNNDistance { get; set; }

        /// <summary>
        /// Final SPEA2 fitness.  Equals the raw fitness R(i) plus the kthNNDistance D(i).
        /// </summary>
        public double Fitness { get; set; }

        #endregion // Properties
        #region Operations

        public override string[] GetAuxilliaryFitnessNames() { return new[] { "Strength", "Raw Fitness", "Kth NN Distance" }; }
        public override double[] GetAuxilliaryFitnessValues() { return new[] { Strength, Fitness, KthNNDistance }; }

        #endregion // Operations
        #region Comparison

        /// <summary>
        /// The selection criteria in SPEA2 uses the computed fitness, and not pareto dominance.
        /// </summary>
        public bool EquivalentTo(Fitness fitness)
        {
            return Fitness == ((SPEA2MultiObjectiveFitness)fitness).Fitness;
        }

        /// <summary>
        /// The selection criteria in SPEA2 uses the computed fitness, and not pareto dominance.
        /// </summary>
        public bool BetterThan(Fitness fitness)
        {
            return Fitness < ((SPEA2MultiObjectiveFitness)fitness).Fitness;
        }

        #endregion // Comparison
        #region ToString

        public override string FitnessToString()
        {
            return base.FitnessToString() + "\n" + SPEA2_FITNESS_PREAMBLE + Code.Encode(Fitness) + "\n" + SPEA2_STRENGTH_PREAMBLE + Code.Encode(Strength) + "\n" + SPEA2_DISTANCE_PREAMBLE + Code.Encode(KthNNDistance);
        }

        public override string FitnessToStringForHumans()
        {
            return base.FitnessToStringForHumans() + "\n" + SPEA2_STRENGTH_PREAMBLE + Strength + SPEA2_DISTANCE_PREAMBLE + KthNNDistance + " " + SPEA2_FITNESS_PREAMBLE + Fitness;
        }

        #endregion // ToString
        #region IO

        public override void ReadFitness(IEvolutionState state, StreamReader reader)
        {
            base.ReadFitness(state, reader);
            Fitness = Code.ReadDoubleWithPreamble(SPEA2_FITNESS_PREAMBLE, state, reader);
            Strength = Code.ReadDoubleWithPreamble(SPEA2_STRENGTH_PREAMBLE, state, reader);
            KthNNDistance = Code.ReadDoubleWithPreamble(SPEA2_DISTANCE_PREAMBLE, state, reader);
        }

        public override void WriteFitness(IEvolutionState state, BinaryWriter writer)
        {
            base.WriteFitness(state, writer);
            writer.Write(Fitness);
            writer.Write(Strength);
            writer.Write(Fitness);
            writer.Write(KthNNDistance);
            WriteTrials(state, writer);
        }

        public override void ReadFitness(IEvolutionState state, BinaryReader dataInput)
        {
            base.ReadFitness(state, dataInput);
            Fitness = dataInput.ReadDouble();
            Strength = dataInput.ReadDouble();
            Fitness = dataInput.ReadDouble();
            KthNNDistance = dataInput.ReadDouble();
            ReadTrials(state, dataInput);
        }

        #endregion // IO
    }
}