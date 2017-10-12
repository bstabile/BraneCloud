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
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.SteadyState;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Simple
{
    /// <summary> 
    /// A basic Statistics class suitable for simple problem applications.
    /// 
    /// SimpleStatistics prints out the best individual, per subpop,
    /// each generation.  At the end of a run, it also prints out the best
    /// individual of the run.  SimpleStatistics outputs this data to a log
    /// which may either be a provided file or stdout.  Compressed files will
    /// be overridden on restart from checkpoint; uncompressed files will be 
    /// appended on restart.
    /// 
    /// <p/>SimpleStatistics implements a simple version of steady-state statistics:
    /// if it quits before a generation boundary,
    /// it will include the best individual discovered, even if the individual was discovered
    /// after the last boundary.  This is done by using individualsEvaluatedStatistics(...)
    /// to update best-individual-of-generation in addition to doing it in
    /// postEvaluationStatistics(...).
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>gzip</tt><br/>
    /// <font size="-1">boolean</font></td>
    /// <td valign="top">(whether or not to compress the file (.gz suffix added)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>file</tt><br/>
    /// <font size="-1">String (a filename), or nonexistant (signifies stdout)</font></td>
    /// <td valign="top">(the log for statistics)</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.simple.SimpleStatistics")]
    public class SimpleStatistics : Statistics, ISteadyStateStatistics
    {
        #region Constants

        /// <summary>
        /// Log file parameter 
        /// </summary>
        public const string P_STATISTICS_FILE = "file";
        
        /// <summary>
        /// Compress? 
        /// </summary>
        public const string P_COMPRESS = "gzip";

        #endregion // Constants
        #region Properties

        /// <summary>
        /// The Statistics' log 
        /// </summary>
        public int StatisticsLog { get; set; }

        /// <summary>
        /// The best individual we've found so far 
        /// </summary>
        public Individual[] BestOfRun { get; set; }

        /// <summary>
        /// Should we compress the file?
        /// </summary>
        public bool Compress { get; set; }

        #endregion // Properties
        #region Setup

        public SimpleStatistics()
        {
            BestOfRun = null;
            StatisticsLog = 0; /* stdout */
        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            Compress = state.Parameters.GetBoolean(paramBase.Push(P_COMPRESS), null, false);

            var statisticsFile = state.Parameters.GetFile(paramBase.Push(P_STATISTICS_FILE), null);

            if (statisticsFile != null)
                try
                {
                    StatisticsLog = state.Output.AddLog(statisticsFile, !Compress, Compress);
                }
                catch (IOException i)
                {
                    state.Output.Fatal("An IOException occurred while trying to create the log " + statisticsFile + ":\n" + i);
                }
        }

        #endregion // Setup
        #region Operations

        public override void PostInitializationStatistics(IEvolutionState state)
        {
            base.PostInitializationStatistics(state);

            // set up our Best_Of_Run array -- can't do this in Setup, because
            // we don't know if the number of subpops has been determined yet
            BestOfRun = new Individual[state.Population.Subpops.Length];
        }

        /// <summary>
        /// Logs the best individual of the generation. 
        /// </summary>
        public override void PostEvaluationStatistics(IEvolutionState state)
        {
            base.PostEvaluationStatistics(state);

            // for now we just print the best fitness per subpop.
            var bestI = new Individual[state.Population.Subpops.Length]; // quiets compiler complaints
            for (var x = 0; x < state.Population.Subpops.Length; x++)
            {
                bestI[x] = state.Population.Subpops[x].Individuals[0];
                for (var y = 1; y < state.Population.Subpops[x].Individuals.Length; y++)
                    if (state.Population.Subpops[x].Individuals[y].Fitness.BetterThan(bestI[x].Fitness))
                        bestI[x] = state.Population.Subpops[x].Individuals[y];

                // now test to see if it's the new Best_Of_Run
                if (BestOfRun[x] == null || bestI[x].Fitness.BetterThan(BestOfRun[x].Fitness))
                    BestOfRun[x] = (Individual)bestI[x].Clone();
            }

            // print the best-of-generation individual
            state.Output.PrintLn("\nGeneration: " + state.Generation, StatisticsLog);
            state.Output.PrintLn("Best Individual:", StatisticsLog);
            for (var x = 0; x < state.Population.Subpops.Length; x++)
            {
                state.Output.PrintLn("Subpopulation " + x + ":", StatisticsLog);
                bestI[x].PrintIndividualForHumans(state, StatisticsLog);
                if (bestI[x].Evaluated)
                    state.Output.Message("Subpop " + x + " best fitness of generation: " +
                                         bestI[x].Fitness.FitnessToStringForHumans());
                else
                    state.Output.Message("Subpop " + x + " not evaluated.");
                // can happen if we're doing sequential coevolution            
            }
        }

        /// <summary>
        /// Logs the best individual of the run. 
        /// </summary>
        public override void FinalStatistics(IEvolutionState state, int result)
        {
            base.FinalStatistics(state, result);

            // for now we just print the best fitness 

            state.Output.PrintLn("\nBest Individual of Run:", StatisticsLog);

            for (var x = 0; x < state.Population.Subpops.Length; x++)
            {
                BestOfRun[x].PrintIndividualForHumans(state, StatisticsLog);
                state.Output.Message("Subpop " + x + " best fitness of run: " + BestOfRun[x].Fitness.FitnessToStringForHumans());

                // finally describe the winner if there is a description
                if (state.Evaluator.p_problem is ISimpleProblem)
                    ((ISimpleProblem)state.Evaluator.p_problem.Clone()).Describe(state, BestOfRun[x], x, 0, StatisticsLog);
            }
        }

        #endregion // Operations
    }
}