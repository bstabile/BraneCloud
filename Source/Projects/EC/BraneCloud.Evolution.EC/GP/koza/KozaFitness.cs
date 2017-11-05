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

using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.GP.Koza
{
    // BRS : TODO : It would be wise to go over this carefully! 
    //       There are too many accessors that make it very unclear what a client is retrieving.


    /// <summary> 
    /// KozaFitness is a Fitness which stores an individual's fitness as described in
    /// Koza I.  Well, almost.  In KozaFitness, standardized fitness and raw fitness
    /// are considered the same (there are different methods for them, but they return
    /// the same thing).  Standardized fitness ranges from 0.0 inclusive (the best)
    /// to infinity exclusive (the worst).  Adjusted fitness converts this, using
    /// the formula adj_f = 1/(1+f), into a scale from 0.0 exclusive (worst) to 1.0
    /// inclusive (best).  While it's the standardized fitness that is stored, it
    /// is the adjusted fitness that is printed out.
    /// This is all just convenience stuff anyway; selection methods
    /// generally don't use these fitness values but instead use the betterThan
    /// and equalTo methods.
    /// 
    /// <p/><b>Default Base</b><br/>
    /// gp.koza.Fitness
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.koza.KozaFitness")]
    public class KozaFitness : Fitness
    {
        #region Constants

        public const string P_KOZAFITNESS = "fitness";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase => GPKozaDefaults.ParamBase.Push(P_KOZAFITNESS);

        /// <summary>
        /// This auxillary measure is used in some problems for additional
        /// information.  It's a traditional feature of Koza-style GP, and so
        /// although I think it's not very useful, I'll leave it in anyway.
        /// </summary>
        public int Hits { get; set; }

        /// <summary>
        /// Returns the adjusted fitness metric, which recasts the
        /// fitness to the half-open interval (0,1], where 1 is ideal and
        /// 0 is worst.  Same as AdjustedFitness.        
        /// </summary>
        /// <remarks>
        /// NOTE: There is no setter for Value. Each Fitness subclass needs to define its own way of handling this.
        /// </remarks>
        public override double Value => AdjustedFitness;

        /// <summary>
        /// Returns the standardized fitness metric.
        /// </summary>
        /// <value></value>
        public double StandardizedFitness => _standardizedFitness;
        
        /// <summary>
        /// This ranges from 0 (best) to infinity (worst).
        /// I define it here as equivalent to the standardized fitness.
        /// </summary>
        protected double _standardizedFitness;

        /// <summary>
        /// Returns the adjusted fitness metric, which recasts the fitness
        /// to the half-open interval (0,1], where 1 is ideal and 0 is worst.
        /// This metric is used when printing the fitness out.
        /// </summary>
        public double AdjustedFitness => 1.0 / (1.0 + _standardizedFitness);

        /// <summary>
        /// Set the standardized fitness in the half-open interval [0.0,infinity)
        /// which is defined as 0.0 being the IDEAL and infinity being worse than the worst possible.
        /// This is the GP tradition.  The Value property instead will output
        /// the equivalent of Standardized Fitness.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="f"></param>
        public void SetStandardizedFitness(IEvolutionState state, double f)
        {
            if (f < 0.0 || f.Equals(double.PositiveInfinity) || double.IsNaN(f))
            {
                state.Output.Warning("Bad fitness (may not be < 0, NaN, or infinity): " + f + ", setting to 0.");
                _standardizedFitness = 0;
            }
            else _standardizedFitness = f;
        }

        /// <summary>
        /// should always be == 0.0, less than 0.0 is illegal, but just in case...
        /// </summary>
        public override bool IsIdeal => _standardizedFitness <= 0.0;

        // BRS: See the Value property
        ///// <summary>
        ///// Returns the adjusted fitness metric, which recasts the
        ///// fitness to the half-open interval (0,1], where 1 is ideal and
        ///// 0 is worst.  Same as AdjustedFitness.
        ///// </summary>
        //public double Fitness
        //{
        //    get { return AdjustedFitness; }
        //}

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase) { }

        #endregion // Setup
        #region Comparison

        public override bool EquivalentTo(IFitness fitness)
        {
            return fitness.Value.Equals(Value);
        }

        public override bool BetterThan(IFitness fitness)
        {
            return fitness.Value < Value;
        }

        #endregion // Comparison
        #region Operations

        public void SetToMeanOf(EvolutionState state, Fitness[] fitnesses)
        {
            // this is not numerically stable.  Perhaps we should have a numerically stable algorithm for sums
            // we're presuming it's not a very large number of elements, so it's probably not a big deal,
            // since this function is meant to be used mostly for gathering trials together.
            double f = 0;
            long h = 0;
            for (var i = 0; i < fitnesses.Length; i++)
            {
                var fit = (KozaFitness)fitnesses[i];
                f += fit._standardizedFitness;
                h += fit.Hits;
            }
            f /= fitnesses.Length;
            h /= fitnesses.Length;
            _standardizedFitness = (double)f;
            Hits = (int)h;
        }

        #endregion // Operations
        #region ToString

        public override string FitnessToString()
        {
            return FITNESS_PREAMBLE + Code.Encode(_standardizedFitness) + Code.Encode(Hits);
        }

        public override string FitnessToStringForHumans()
        {
            return FITNESS_PREAMBLE + "Standardized=" + _standardizedFitness + " Adjusted=" + AdjustedFitness + " Hits=" + Hits;
        }

        #endregion // ToString
        #region IO

        public override void ReadFitness(IEvolutionState state, StreamReader reader)
        {
            var d = Code.CheckPreamble(FITNESS_PREAMBLE, state, reader);

            // extract fitness
            Code.Decode(d);
            if (d.Type != DecodeReturn.T_DOUBLE)
                state.Output.Fatal("Reading Line " + d.LineNumber + ": " + "Bad Fitness.");
            _standardizedFitness = (double)d.D;

            // extract hits
            Code.Decode(d);
            if (d.Type != DecodeReturn.T_INT)
                state.Output.Fatal("Reading Line " + d.LineNumber + ": " + "Bad Fitness.");
            Hits = (int)d.L;
        }

        public override void WriteFitness(IEvolutionState state, BinaryWriter writer)
        {
            writer.Write(_standardizedFitness);
            writer.Write(Hits);
            WriteTrials(state, writer);
        }

        public override void ReadFitness(IEvolutionState state, BinaryReader reader)
        {
            _standardizedFitness = reader.ReadDouble();
            Hits = reader.ReadInt32();
            ReadTrials(state, reader);
        }

        #endregion // IO
    }
}