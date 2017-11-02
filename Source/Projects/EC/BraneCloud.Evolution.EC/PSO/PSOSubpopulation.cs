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
using BraneCloud.Evolution.EC.Vector;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.PSO
{    
    /// <summary> 
    /// PSOSubpopulation
    /// 
    /// <p/>Particle Swarm Optimization (PSO) is a population-oriented stochastic search 
    /// technique similar to genetic algorithms, evolutionary strategies, and other evolutionary
    /// computation algorithms. The technique discovers solutions for N-dimensional 
    /// parameterized problems: basically it discovers the point in N-dimensional space which
    /// maximizes some quality function. 
    /// 
    /// <p/>PSOSubpopulation handles initialization and input/output of the swarm.   
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// 
    /// <tr><td valign="top"><i>base</i>.<tt>neighborhood-size</tt><br/>
    /// <font size="-1">integer</font></td>
    /// <td valign="top">(the number of Individuals per neighborhood)<br/></td></tr>
    /// 
    /// <tr><td valign="top"><i>base</i>.<tt>clamp</tt><br/>
    /// <font size="-1">boolean</font></td>
    /// <td valign="top">(clamp the individual to stay within the bounds or not)<br/></td></tr>
    /// 
    /// <tr><td valign="top"><i>base</i>.<tt>initial-velocity-scale</tt><br/>
    /// <font size="-1">double</font></td>
    /// <td valign="top">(particles are initialized with a random velocity and this value provides bounds. 
    /// A value of 1.0 means that the velocity will be within +/- the range of the genotype.)<br/></td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>velocity-multiplier</tt><br/>
    /// <font size="-1">double</font></td>
    /// <td valign="top">(particle velocities are multiplied by this value before the particle is updated. 
    /// Increasing this value helps particles to escape local optima, but slows convergence. The default 
    /// value of 1.5 is geared toward multi-modal landscapes.)<br/></td></tr>
    /// </table>
    /// 
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>data</tt></td>
    /// <td>Subpopulation</td></tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.pso.PSOSubpopulation")]
    public class PSOSubpopulation : Subpopulation
    {
        #region Constants

        public const string P_NEIGHBORHOOD_SIZE = "neighborhood-size";
        public const string P_CLAMP_RANGE = "clamp";
        public const string P_INITIAL_VELOCITY_SCALE = "initial-velocity-scale";
        public const string P_VELOCITY_MULTIPLIER = "velocity-multiplier";

        public const string GLOBAL_BEST_PREAMBLE = "Global-Best Individual: ";
        public const string NEIGHBORHOOD_BEST_PREAMBLE = "Neighborhood Best Individuals: ";
        public const string PERSONAL_BEST_PREAMBLE = "Personal Best Individuals ";
        public const string PREVIOUS_INDIVIDUAL_PREAMBLE = "Previous Individuals ";
        public const string INDIVIDUAL_EXISTS_PREAMBLE = "Exists: ";


        public const string P_P_FACTOR = "personal-factor";
        public const string P_N_FACTOR = "neighborhood-factor";
        public const string P_G_FACTOR = "global-factor";

        #endregion // Constants
        #region Properties

        public int NeighborhoodSize { get; set; }

        public double InitialVelocityScale { get; set; }

        public double VelocityMultiplier { get; set; }

        public bool ClampRange { get; set; }

        public DoubleVectorIndividual GlobalBest { get; set; }
        public DoubleVectorIndividual[] NeighborhoodBests { get; set; }
        public DoubleVectorIndividual[] PersonalBests { get; set; }
        public DoubleVectorIndividual[] PreviousIndividuals { get; set; }

        public double pFactor { get; set; }
        public double nFactor { get; set; }
        public double gFactor { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            if (!(Species is FloatVectorSpecies))
                state.Output.Error("PSOSubpopulation requires that its Species is ec.vector.FloatVectorSpecies or a subclass.  Yours is: "
                    + Species.GetType(), null, null);
            if (!(Species.I_Prototype is DoubleVectorIndividual))
                state.Output.Error("PSOSubpopulation requires that its Species' prototypical individual be is ec.vector.DoubleVectorSpecies or a subclass.  Yours is: " + Species.GetType(), null, null);

            NeighborhoodBests = new DoubleVectorIndividual[Individuals.Length];
            PersonalBests = new DoubleVectorIndividual[Individuals.Length];
            PreviousIndividuals = new DoubleVectorIndividual[Individuals.Length];

            NeighborhoodSize = state.Parameters.GetInt(paramBase.Push(P_NEIGHBORHOOD_SIZE), null);
            ClampRange = state.Parameters.GetBoolean(paramBase.Push(P_CLAMP_RANGE), null, false);
            InitialVelocityScale = state.Parameters.GetDouble(paramBase.Push(P_INITIAL_VELOCITY_SCALE), null, 0);
            VelocityMultiplier = state.Parameters.GetDouble(paramBase.Push(P_VELOCITY_MULTIPLIER), null, 0.1);

            pFactor = state.Parameters.GetDoubleWithDefault(paramBase.Push(P_P_FACTOR), null, 1);
            nFactor = state.Parameters.GetDoubleWithDefault(paramBase.Push(P_N_FACTOR), null, 1);
            gFactor = state.Parameters.GetDoubleWithDefault(paramBase.Push(P_G_FACTOR), null, 1);
        }

        #endregion // Setup
        #region Operations

        protected virtual void Clear(DoubleVectorIndividual[] inds)
        {
            for (var x = 0; x < inds.Length; x++) { inds[x] = null; }
        }

        public override void Populate(IEvolutionState state, int thread)
        {
            base.Populate(state, thread);

            if (!LoadInds)
            // we're generating new Individuals, not reading them from a file
            {
                Clear(NeighborhoodBests);
                Clear(PersonalBests);

                var fvSpecies = (FloatVectorSpecies)Species;
                /* double range = fvSpecies.maxGene - fvSpecies.minGene; */

                for (var i = 0; i < Individuals.Length; i++)
                {
                    var prevInd = (DoubleVectorIndividual)Individuals[i].Clone();

                    // pick a genome near prevInd but not outside the box
                    for (var j = 0; j < prevInd.GenomeLength; j++)
                    {
                        var val = prevInd.genome[j];
                        var range = fvSpecies.GetMaxGene(j) - fvSpecies.GetMinGene(j);
                        do
                            prevInd.genome[j] = val + (range * InitialVelocityScale) * (state.Random[thread].NextDouble() * 2.0 - 1.0);
                        while (prevInd.genome[j] < fvSpecies.GetMinGene(j) || prevInd.genome[j] > fvSpecies.GetMaxGene(j));
                    }
                    PreviousIndividuals[i] = prevInd;
                }
            }
        }

        #endregion // Operations
        #region IO

        /// <summary>
        /// Overridden to include the global best, neighborhood bests, personal bests, and previous Individuals in the stream.
        /// The neighborhood size, clamp range, and initial velocity scale are not included -- it's assumed you're using the
        /// same values for them on reading, or understand that the values are revised. 
        /// </summary>
        public override void PrintSubpopulationForHumans(IEvolutionState state, int log)
        {
            // global best
            state.Output.PrintLn(GLOBAL_BEST_PREAMBLE, log);
            if (GlobalBest == null)
                state.Output.PrintLn(INDIVIDUAL_EXISTS_PREAMBLE + "false", log);
            else
            {
                state.Output.PrintLn(INDIVIDUAL_EXISTS_PREAMBLE + "true", log);
                GlobalBest.PrintIndividualForHumans(state, log);
            }

            // NeighborhoodBests
            state.Output.PrintLn(NEIGHBORHOOD_BEST_PREAMBLE, log);
            for (var i = 0; i < Individuals.Length; i++)
                if (NeighborhoodBests[i] == null)
                    state.Output.PrintLn(INDIVIDUAL_EXISTS_PREAMBLE + "false", log);
                else
                {
                    state.Output.PrintLn(INDIVIDUAL_EXISTS_PREAMBLE + "true", log);
                    NeighborhoodBests[i].PrintIndividualForHumans(state, log);
                }

            // PersonalBests
            state.Output.PrintLn(PERSONAL_BEST_PREAMBLE, log);
            for (var i = 0; i < Individuals.Length; i++)
                if (PersonalBests[i] == null)
                    state.Output.PrintLn(INDIVIDUAL_EXISTS_PREAMBLE + "false", log);
                else
                {
                    state.Output.PrintLn(INDIVIDUAL_EXISTS_PREAMBLE + "true", log);
                    PersonalBests[i].PrintIndividualForHumans(state, log);
                }

            // NeighborhoodBests
            state.Output.PrintLn(PREVIOUS_INDIVIDUAL_PREAMBLE, log);
            for (var i = 0; i < Individuals.Length; i++)
                if (PreviousIndividuals[i] == null)
                    state.Output.PrintLn(INDIVIDUAL_EXISTS_PREAMBLE + "false", log);
                else
                {
                    state.Output.PrintLn(INDIVIDUAL_EXISTS_PREAMBLE + "true", log);
                    PreviousIndividuals[i].PrintIndividualForHumans(state, log);
                }

            base.PrintSubpopulationForHumans(state, log);
        }

        /// <summary>
        /// Overridden to include the global best, neighborhood bests, personal bests, and previous Individuals in the stream.
        /// The neighborhood size, clamp range, and initial velocity scale are not included -- it's assumed you're using the
        /// same values for them on reading, or understand that the values are revised. 
        /// </summary>
        public override void PrintSubpopulation(IEvolutionState state, int log)
        {
            // global best
            state.Output.PrintLn(GLOBAL_BEST_PREAMBLE, log);
            if (GlobalBest == null)
                state.Output.PrintLn(INDIVIDUAL_EXISTS_PREAMBLE + Code.Encode(false), log);
            else
            {
                state.Output.PrintLn(INDIVIDUAL_EXISTS_PREAMBLE + Code.Encode(true), log);
                GlobalBest.PrintIndividual(state, log);
            }

            // NeighborhoodBests
            state.Output.PrintLn(NEIGHBORHOOD_BEST_PREAMBLE, log);
            for (var i = 0; i < Individuals.Length; i++)
                if (NeighborhoodBests[i] == null)
                    state.Output.PrintLn(INDIVIDUAL_EXISTS_PREAMBLE + Code.Encode(false), log);
                else
                {
                    state.Output.PrintLn(INDIVIDUAL_EXISTS_PREAMBLE + Code.Encode(true), log);
                    NeighborhoodBests[i].PrintIndividual(state, log);
                }

            // PersonalBests
            state.Output.PrintLn(PERSONAL_BEST_PREAMBLE, log);
            for (var i = 0; i < Individuals.Length; i++)
                if (PersonalBests[i] == null)
                    state.Output.PrintLn(INDIVIDUAL_EXISTS_PREAMBLE + Code.Encode(false), log);
                else
                {
                    state.Output.PrintLn(INDIVIDUAL_EXISTS_PREAMBLE + Code.Encode(true), log);
                    PersonalBests[i].PrintIndividual(state, log);
                }

            // NeighborhoodBests
            state.Output.PrintLn(PREVIOUS_INDIVIDUAL_PREAMBLE, log);
            for (var i = 0; i < Individuals.Length; i++)
                if (PreviousIndividuals[i] == null)
                    state.Output.PrintLn(INDIVIDUAL_EXISTS_PREAMBLE + Code.Encode(false), log);
                else
                {
                    state.Output.PrintLn(INDIVIDUAL_EXISTS_PREAMBLE + Code.Encode(true), log);
                    PreviousIndividuals[i].PrintIndividual(state, log);
                }

            base.PrintSubpopulation(state, log);
        }

        /// <summary>
        /// Overridden to include the global best, neighborhood bests, personal bests, and previous Individuals in the stream.
        /// The neighborhood size, clamp range, and initial velocity scale are not included -- it's assumed you're using the
        /// same values for them on reading, or understand that the values are revised. 
        /// </summary>
        public override void PrintSubpopulation(IEvolutionState state, StreamWriter writer)
        {
            // global best
            writer.WriteLine(GLOBAL_BEST_PREAMBLE);
            if (GlobalBest == null)
            {
                writer.WriteLine(INDIVIDUAL_EXISTS_PREAMBLE + Code.Encode(false));
            }
            else
            {
                writer.WriteLine(INDIVIDUAL_EXISTS_PREAMBLE + Code.Encode(true));
                GlobalBest.PrintIndividual(state, writer);
            }

            // NeighborhoodBests
            writer.WriteLine(NEIGHBORHOOD_BEST_PREAMBLE);
            for (var i = 0; i < Individuals.Length; i++)
                if (NeighborhoodBests[i] == null)
                {
                    writer.WriteLine(INDIVIDUAL_EXISTS_PREAMBLE + Code.Encode(false));
                }
                else
                {
                    writer.WriteLine(INDIVIDUAL_EXISTS_PREAMBLE + Code.Encode(true));
                    NeighborhoodBests[i].PrintIndividual(state, writer);
                }

            // PersonalBests
            writer.WriteLine(PERSONAL_BEST_PREAMBLE);
            for (var i = 0; i < Individuals.Length; i++)
                if (PersonalBests[i] == null)
                {
                    writer.WriteLine(INDIVIDUAL_EXISTS_PREAMBLE + Code.Encode(false));
                }
                else
                {
                    writer.WriteLine(INDIVIDUAL_EXISTS_PREAMBLE + Code.Encode(true));
                    PersonalBests[i].PrintIndividual(state, writer);
                }

            // NeighborhoodBests
            writer.WriteLine(PREVIOUS_INDIVIDUAL_PREAMBLE);
            for (var i = 0; i < Individuals.Length; i++)
                if (PreviousIndividuals[i] == null)
                {
                    writer.WriteLine(INDIVIDUAL_EXISTS_PREAMBLE + Code.Encode(false));
                }
                else
                {
                    writer.WriteLine(INDIVIDUAL_EXISTS_PREAMBLE + Code.Encode(true));
                    PreviousIndividuals[i].PrintIndividual(state, writer);
                }

            base.PrintSubpopulation(state, writer);
        }

        internal virtual DoubleVectorIndividual PossiblyReadIndividual(IEvolutionState state, StreamReader reader)
        {
            if (Code.ReadBooleanWithPreamble(INDIVIDUAL_EXISTS_PREAMBLE, state, reader))
                return (DoubleVectorIndividual)Species.NewIndividual(state, reader);

            return null;
        }

        /// <summary>
        /// Overridden to include the global best, neighborhood bests, personal bests, and previous Individuals in the stream.
        /// The neighborhood size, clamp range, and initial velocity scale are not included -- it's assumed you're using the
        /// same values for them on reading, or understand that the values are revised. 
        /// </summary>
        public override void ReadSubpopulation(IEvolutionState state, StreamReader reader)
        {
            // global best
            Code.CheckPreamble(GLOBAL_BEST_PREAMBLE, state, reader);
            GlobalBest = PossiblyReadIndividual(state, reader);

            // NeighborhoodBests
            Code.CheckPreamble(NEIGHBORHOOD_BEST_PREAMBLE, state, reader);
            for (var i = 0; i < Individuals.Length; i++)
                NeighborhoodBests[i] = PossiblyReadIndividual(state, reader);

            // PersonalBests
            Code.CheckPreamble(PERSONAL_BEST_PREAMBLE, state, reader);
            for (var i = 0; i < Individuals.Length; i++)
                PersonalBests[i] = PossiblyReadIndividual(state, reader);

            // NeighborhoodBests
            Code.CheckPreamble(PREVIOUS_INDIVIDUAL_PREAMBLE, state, reader);
            for (var i = 0; i < Individuals.Length; i++)
                PreviousIndividuals[i] = PossiblyReadIndividual(state, reader);

            base.ReadSubpopulation(state, reader);
        }

        /// <summary>
        /// Overridden to include the global best, neighborhood bests, personal bests, and previous Individuals in the stream.
        /// The neighborhood size, clamp range, and initial velocity scale are not included -- it's assumed you're using the
        /// same values for them on reading, or understand that the values are revised. 
        /// </summary>
        public override void WriteSubpopulation(IEvolutionState state, BinaryWriter writer)
        {
            // global best
            if (GlobalBest == null)
                writer.Write(false);
            else
            {
                writer.Write(true);
                GlobalBest.WriteIndividual(state, writer);
            }

            // NeighborhoodBests
            for (var i = 0; i < Individuals.Length; i++)
                if (NeighborhoodBests[i] == null)
                    writer.Write(false);
                else
                {
                    writer.Write(true);
                    NeighborhoodBests[i].WriteIndividual(state, writer);
                }

            // PersonalBests
            for (var i = 0; i < Individuals.Length; i++)
                if (PersonalBests[i] == null)
                    writer.Write(false);
                else
                {
                    writer.Write(true);
                    PersonalBests[i].WriteIndividual(state, writer);
                }

            // previous Individuals
            for (var i = 0; i < Individuals.Length; i++)
                if (PreviousIndividuals[i] == null)
                    writer.Write(false);
                else
                {
                    writer.Write(true);
                    PreviousIndividuals[i].WriteIndividual(state, writer);
                }

            base.WriteSubpopulation(state, writer);
        }

        /// <summary>
        /// Overridden to include the global best, neighborhood bests, personal bests, and previous Individuals in the stream.
        /// The neighborhood size, clamp range, and initial velocity scale are not included -- it's assumed you're using the
        /// same values for them on reading, or understand that the values are revised. 
        /// </summary>
        public override void ReadSubpopulation(IEvolutionState state, BinaryReader reader)
        {
            // global best
            GlobalBest = (reader.ReadBoolean() ? (DoubleVectorIndividual)Species.NewIndividual(state, reader) : null);

            // NeighborhoodBests
            for (var i = 0; i < Individuals.Length; i++)
                NeighborhoodBests[i] = (reader.ReadBoolean() ? (DoubleVectorIndividual)Species.NewIndividual(state, reader) : null);

            // PersonalBests
            for (var i = 0; i < Individuals.Length; i++)
                PersonalBests[i] = (reader.ReadBoolean() ? (DoubleVectorIndividual)Species.NewIndividual(state, reader) : null);

            // previous Individuals
            for (var i = 0; i < Individuals.Length; i++)
                PreviousIndividuals[i] = (reader.ReadBoolean() ? (DoubleVectorIndividual)Species.NewIndividual(state, reader) : null);

            base.ReadSubpopulation(state, reader);
        }

        #endregion // IO
    }
}