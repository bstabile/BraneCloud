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
using System.Diagnostics;
using System.IO;
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Randomization;
using BraneCloud.Evolution.EC.Util;
using SharpenMinimal;

namespace BraneCloud.Evolution.EC.Simple
{
    /// <summary> 
    /// A Simple-style statistics generator, intended to be easily parseable with
    /// awk or other Unix tools.  Prints fitness information,
    /// one generation (or pseudo-generation) per line.
    /// If do-time is true, then timing information is also given.  If do-size is true, then size information is also given.
    /// No final statistics information is provided.  You can also set SimpleShortStatistics to only output every *modulus* generations
    /// to keep the tally shorter.  And you can gzip the statistics file.    
    ///  
    /// <p/> Each line represents a single generation.  
    /// The first items on a line are always:
    /// <ul>
    /// <li/> The generation number
    /// <li/> (if do-time) how long initialization took in milliseconds, or how long the previous generation took to breed to form this generation
    /// <li/> (if do-time) How long evaluation took in milliseconds this generation
    /// </ul>
    /// 
    /// <p/>Then, (if do-subpops) the following items appear, once per each subpopulation:
    /// <ul>
    /// <li/> (if do-size) The average size of an individual this generation
    /// <li/> (if do-size) The average size of an individual so far in the run
    /// <li/> (if do-size) The size of the best individual this generation
    /// <li/> (if do-size) The size of the best individual so far in the run
    /// <li/> The mean fitness of the subpopulation this generation
    /// <li/> The best fitness of the subpopulation this generation
    /// <li/> The best fitness of the subpopulation so far in the run
    /// </ul>
    /// 
    /// <p/>Then the following items appear, for the whole population:
    /// <ul>
    /// <li/> (if do-size) The average size of an individual this generation
    /// <li/> (if do-size) The average size of an individual so far in the run
    /// <li/> (if do-size) The size of the best individual this generation
    /// <li/> (if do-size) The size of the best individual so far in the run
    /// <li/> The mean fitness this generation
    /// <li/> The best fitness this generation
    /// <li/> The best fitness so far in the run    
    /// </ul>
    /// Compressed files will be overridden on restart from checkpoint; uncompressed files will be 
    /// appended on restart.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>file</tt><br/>
    /// <font size="-1">String (a filename), or nonexistant (signifies stdout)</font></td>
    /// <td valign="top">(the log for statistics)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>gzip</tt><br/>
    /// <font size="-1">boolean</font></td>
    /// <td valign="top">(whether or not to compress the file (.gz suffix added)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>modulus</tt><br/>
    /// <font size="-1">integer >= 1 (default)</font></td>
    /// <td valign="top">(How often (in generations) should we print a statistics line?)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>do-time</tt><br/>
    /// <font size="-1">bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">(print timing information?)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>do-size</tt><br/>
    /// <font size="-1">bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">(print sizing information?)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>do-subpops</tt><br/>
    /// <font size="-1">bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">(print information on a per-subpop basis as well as per-population?)</td></tr>    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.simple.SimpleShortStatistics")]
    public class SimpleShortStatistics : Statistics
    {
        #region Constants

        public const string P_STATISTICS_MODULUS = "modulus";

        /// <summary>
        /// Compress? 
        /// </summary>
        public const string P_COMPRESS = "gzip";

        public const string P_FULL = "gather-full";
        public const string P_DO_SIZE = "do-size";
        public const string P_DO_TIME = "do-time";
        public const string P_DO_SUBPOPS = "do-subpops";
        public const string P_STATISTICS_FILE = "file";

        #endregion // Constants
        #region Public Properties

        //public bool DoFull { get; set; }

        public int StatisticsLog { get; set; }
        public int Modulus { get; set; }
        public bool DoSize { get; set; }
        public bool DoTime { get; set; }
        public bool DoSubpops { get; set; }

        public Individual[] BestSoFar { get; set; }
        public long[] TotalSizeSoFar { get; set; }
        public long[] TotalIndsSoFar { get; set; }
        public long[] TotalIndsThisGen { get; set; }
        public long[] TotalSizeThisGen { get; set; }
        public double[] TotalFitnessThisGen { get; set; }

        // per-subpop best individual this generation
        public Individual[] BestOfGeneration { get; set; }

        /// <summary>
        /// Timings
        /// </summary>
        public long LastTime { get; set; }

        #endregion // Public Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            FileInfo statisticsFile = state.Parameters.GetFile(paramBase.Push(P_STATISTICS_FILE), null);

            Modulus = state.Parameters.GetIntWithDefault(paramBase.Push(P_STATISTICS_MODULUS), null, 1);

            if (SilentFile)
            {
                StatisticsLog = Output.NO_LOGS;
            }
            else if (statisticsFile != null)
            {
                try
                {
                    StatisticsLog = state.Output.AddLog(statisticsFile,
                                                        !state.Parameters.GetBoolean(paramBase.Push(P_COMPRESS), null, false),
                                                        state.Parameters.GetBoolean(paramBase.Push(P_COMPRESS), null, false));
                }
                catch (IOException i)
                {
                    state.Output.Fatal("An IOException occurred while trying to create the log " + statisticsFile + ":\n" + i);
                }
            }
            else state.Output.Warning("No statistics file specified, printing to stdout at end.", paramBase.Push(P_STATISTICS_FILE));

            DoSize = state.Parameters.GetBoolean(paramBase.Push(P_DO_SIZE), null, false);
            DoTime = state.Parameters.GetBoolean(paramBase.Push(P_DO_TIME), null, false);
            if (state.Parameters.ParameterExists(paramBase.Push(P_FULL), null))
            {
                state.Output.Warning(P_FULL + " is deprecated.  Use " + P_DO_SIZE + " and " + P_DO_TIME + " instead.  Also be warned that the table columns have been reorganized. ", paramBase.Push(P_FULL), null);
                bool gather = state.Parameters.GetBoolean(paramBase.Push(P_FULL), null, false);
                DoSize = DoSize || gather;
                DoTime = DoTime || gather;
            }
            DoSubpops = state.Parameters.GetBoolean(paramBase.Push(P_DO_SUBPOPS), null, false);
        }

        #endregion // Setup
        #region Operations

        public Individual[] GetBestSoFar() { return BestSoFar; }

        protected void PrepareStatistics(IEvolutionState state) { }

        #region On Initialization

        public override void PreInitializationStatistics(IEvolutionState state)
        {
            base.PreInitializationStatistics(state);

            var output = state.Generation % Modulus == 0;

            if (output && DoTime)
            {
                // ECJ: lastTime = System.currentTimeMillis();
                LastTime = DateTimeHelper.CurrentTimeMilliseconds;
            }
        }

        public override void PostInitializationStatistics(IEvolutionState state)
        {
            base.PostInitializationStatistics(state);

            bool output = state.Generation % Modulus == 0;

            // set up our BestSoFar array -- can't do this in Setup, because
            // we don't know if the number of subpops has been determined yet
            BestSoFar = new Individual[state.Population.Subpops.Length];

            // print out our generation number
            if (output)
            {
                state.Output.Print("0 ", StatisticsLog);
            }

            // gather timings       
            TotalSizeSoFar = new long[state.Population.Subpops.Length];
            TotalIndsSoFar = new long[state.Population.Subpops.Length];

            if (output && DoTime)
            {
                // ECJ: state.output.print("" + (System.currentTimeMillis() - lastTime) + " ", statisticslog);
                state.Output.Print("" + (DateTimeHelper.CurrentTimeMilliseconds - LastTime) + " ", StatisticsLog);
            }
        }

        #endregion // On Initialization
        #region On Breeding

        public override void PreBreedingStatistics(IEvolutionState state)
        {
            base.PreBreedingStatistics(state);
            bool output = state.Generation % Modulus == Modulus - 1;
            if (output && DoTime)
            {
                // ECJ: lastTime = System.currentTimeMillis();
                LastTime = DateTimeHelper.CurrentTimeMilliseconds;
            }
        }

        public override void PostBreedingStatistics(IEvolutionState state)
        {
            base.PostBreedingStatistics(state);
            bool output = (state.Generation % Modulus == Modulus - 1);
            if (output) state.Output.Print("" + (state.Generation + 1) + " ", StatisticsLog); // 1 because we're putting the breeding info on the same line as the generation it *produces*, and the generation number is increased *after* breeding occurs, and statistics for it

            // gather timings
            if (output && DoTime)
            {
                // ECJ: state.output.print("" + (System.currentTimeMillis() - lastTime) + " ", statisticslog);
                state.Output.Print("" + (DateTimeHelper.CurrentTimeMilliseconds - LastTime) + " ", StatisticsLog);
            }
        }

        #endregion // On Breeding
        #region On Evaluation

        public override void PreEvaluationStatistics(IEvolutionState state)
        {
            base.PreEvaluationStatistics(state);
            bool output = (state.Generation % Modulus == 0);

            if (output && DoTime)
            {
                // ECJ: lastTime = System.currentTimeMillis();
                LastTime = DateTimeHelper.CurrentTimeMilliseconds;
            }
        }

        public override void PostEvaluationStatistics(IEvolutionState state)
        {
            base.PostEvaluationStatistics(state);

            bool output = state.Generation % Modulus == 0;

            // gather timings
            if (output && DoTime)
            {
                // ECJ: state.output.print("" + (System.currentTimeMillis() - lastTime) + " ", statisticslog);
                state.Output.Print("" + (DateTimeHelper.CurrentTimeMilliseconds - LastTime) + " ", StatisticsLog);
            }

            int subpops = state.Population.Subpops.Length;				// number of supopulations
            TotalIndsThisGen = new long[subpops];						// total assessed individuals
            BestOfGeneration = new Individual[subpops];					// per-subpop best individual this generation
            TotalSizeThisGen = new long[subpops];				// per-subpop total size of individuals this generation
            TotalFitnessThisGen = new double[subpops];			// per-subpop mean fitness this generation
            var meanFitnessThisGen = new double[subpops];			// per-subpop mean fitness this generation


            PrepareStatistics(state);

            // gather per-subpopulation statistics

            for (var x = 0; x < subpops; x++)
            {
                for (var y = 0; y < state.Population.Subpops[x].Individuals.Length; y++)
                {
                    if (state.Population.Subpops[x].Individuals[y].Evaluated)		// he's got a valid fitness
                    {
                        // update sizes
                        var size = state.Population.Subpops[x].Individuals[y].Size;
                        TotalSizeThisGen[x] += size;
                        TotalSizeSoFar[x] += size;
                        TotalIndsThisGen[x] += 1;
                        TotalIndsSoFar[x] += 1;

                        // update fitness
                        if (BestOfGeneration[x] == null ||
                            state.Population.Subpops[x].Individuals[y].Fitness.BetterThan(BestOfGeneration[x].Fitness))
                        {
                            BestOfGeneration[x] = state.Population.Subpops[x].Individuals[y];
                            if (BestSoFar[x] == null || BestOfGeneration[x].Fitness.BetterThan(BestSoFar[x].Fitness))
                                BestSoFar[x] = (Individual)BestOfGeneration[x].Clone();
                        }

                        // sum up mean fitness for population
                        TotalFitnessThisGen[x] += state.Population.Subpops[x].Individuals[y].Fitness.Value;

                        // hook for KozaShortStatistics etc.
                        GatherExtraSubpopStatistics(state, x, y);
                    }
                }
                // compute mean fitness stats
                meanFitnessThisGen[x] = TotalIndsThisGen[x] > 0 ? TotalFitnessThisGen[x] / TotalIndsThisGen[x] : 0;

                // hook for KozaShortStatistics etc.
                if (output && DoSubpops) PrintExtraSubpopStatisticsBefore(state, x);

                // print out optional average size information
                if (output && DoSize && DoSubpops)
                {
                    state.Output.Print("" + (TotalIndsThisGen[x] > 0 ? (double)TotalSizeThisGen[x] / TotalIndsThisGen[x] : 0) + " ", StatisticsLog);
                    state.Output.Print("" + (TotalIndsSoFar[x] > 0 ? (double)TotalSizeSoFar[x] / TotalIndsSoFar[x] : 0) + " ", StatisticsLog);
                    state.Output.Print("" + (double)BestOfGeneration[x].Size + " ", StatisticsLog);
                    state.Output.Print("" + (double)BestSoFar[x].Size + " ", StatisticsLog);
                }

                // print out fitness information
                if (output && DoSubpops)
                {
                    state.Output.Print("" + meanFitnessThisGen[x] + " ", StatisticsLog);
                    state.Output.Print("" + BestOfGeneration[x].Fitness.Value + " ", StatisticsLog);
                    state.Output.Print("" + BestSoFar[x].Fitness.Value + " ", StatisticsLog);
                }

                // hook for KozaShortStatistics etc.
                if (output && DoSubpops) PrintExtraSubpopStatisticsAfter(state, x);
            }


            // Now gather per-Population statistics
            long popTotalInds = 0;
            long popTotalIndsSoFar = 0;
            long popTotalSize = 0;
            long popTotalSizeSoFar = 0;
            double popMeanFitness = 0;
            double popTotalFitness = 0;
            Individual popBestOfGeneration = null;
            Individual popBestSoFar = null;

            for (var x = 0; x < subpops; x++)
            {
                popTotalInds += TotalIndsThisGen[x];
                popTotalIndsSoFar += TotalIndsSoFar[x];
                popTotalSize += TotalSizeThisGen[x];
                popTotalSizeSoFar += TotalSizeSoFar[x];
                popTotalFitness += TotalFitnessThisGen[x];
                if (BestOfGeneration[x] != null && (popBestOfGeneration == null || BestOfGeneration[x].Fitness.BetterThan(popBestOfGeneration.Fitness)))
                    popBestOfGeneration = BestOfGeneration[x];
                if (BestSoFar[x] != null && (popBestSoFar == null || BestSoFar[x].Fitness.BetterThan(popBestSoFar.Fitness)))
                    popBestSoFar = BestSoFar[x];

                // hook for KozaShortStatistics etc.
                GatherExtraPopStatistics(state, x);
            }

            // build mean
            popMeanFitness = popTotalInds > 0 ? popTotalFitness / popTotalInds : 0;		// average out

            // hook for KozaShortStatistics etc.
            if (output) PrintExtraPopStatisticsBefore(state);

            // optionally print out mean size info
            if (output && DoSize)
            {
                state.Output.Print("" + (popTotalInds > 0 ? popTotalSize / popTotalInds : 0) + " ", StatisticsLog);						// mean size of pop this gen
                state.Output.Print("" + (popTotalIndsSoFar > 0 ? popTotalSizeSoFar / popTotalIndsSoFar : 0) + " ", StatisticsLog);				// mean size of pop so far
                state.Output.Print("" + (double)popBestOfGeneration.Size + " ", StatisticsLog);					// size of best ind of pop this gen
                state.Output.Print("" + (double)popBestSoFar.Size + " ", StatisticsLog);				// size of best ind of pop so far
            }

            // print out fitness info
            if (output)
            {
                state.Output.Print("" + popMeanFitness + " ", StatisticsLog);											// mean fitness of pop this gen
                state.Output.Print("" + (double)popBestOfGeneration.Fitness.Value + " ", StatisticsLog);			// best fitness of pop this gen
                state.Output.Print("" + (double)popBestSoFar.Fitness.Value + " ", StatisticsLog);		// best fitness of pop so far
            }

            // hook for KozaShortStatistics etc.
            if (output) PrintExtraPopStatisticsAfter(state);

            // we're done!
            if (output) state.Output.PrintLn("", StatisticsLog);
        }

        #endregion // On Evaluation

        #region Extra Subpop (Used by KozaShortStatistics)

        protected void GatherExtraSubpopStatistics(IEvolutionState state, int subpop, int individual) { }
        protected void PrintExtraSubpopStatisticsBefore(IEvolutionState state, int subpop) { }
        protected void PrintExtraSubpopStatisticsAfter(IEvolutionState state, int subpop) { }

        #endregion // Extra Subpop
        #region Extra Pop  (Used by KozaShortStatistics)

        protected void GatherExtraPopStatistics(IEvolutionState state, int subpop) { }
        protected void PrintExtraPopStatisticsBefore(IEvolutionState state) { }
        protected void PrintExtraPopStatisticsAfter(IEvolutionState state) { }

        #endregion // Extra Pop

        #endregion // Operations
    }
}