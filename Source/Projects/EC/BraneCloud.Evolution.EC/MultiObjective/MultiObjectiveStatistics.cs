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
using System.Linq;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.MultiObjective
{
    /// <summary>
    /// MultiObjectiveStatistics are a SimpleStatistics subclass which overrides the finalStatistics
    /// method to output the current Pareto Front in various ways:
    /// 
    /// <ul/>
    /// <li/><p/>Every individual in the Pareto Front is written to the end of the statistics log.
    /// <li/><p/>A summary of the objective values of the Pareto Front is written to stdout.
    /// <li/><p/>The objective values of the Pareto Front are written in tabular form to a special
    /// Pareto Front file specified with the parameters below.  This file can be easily read by
    /// gnuplot or Excel etc. to display the Front (if it's 2D or perhaps 3D).
    /// 
    /// <p/>
    /// <b>Parameters</b><br/>
    /// <table>
    /// <tr>
    /// <td valign="top"><i>base</i>.<tt>front</tt><br/>
    /// <font size="-1">String (a filename)</font></td>
    /// <td valign="top">(The Pareto Front file, if any)</td>
    /// </tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.multiobjective.MultiObjectiveStatistics")]
    public class MultiObjectiveStatistics : SimpleStatistics
    {
        #region Constants

        /// <summary>
        /// Front file parameter
        /// </summary>
        public const string P_PARETO_FRONT_FILE = "front";
        public const string P_SILENT_FRONT_FILE = "silent.front";

        #endregion // Constants
        #region Properties

        public bool SilentFront { get; set; }

        public int FrontLog { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            SilentFront = state.Parameters.GetBoolean(paramBase.Push(P_SILENT), null, false);
            // yes, we're stating it a second time.  It's correct logic.
            SilentFront = state.Parameters.GetBoolean(paramBase.Push(P_SILENT_FRONT_FILE), null, SilentFront);

            var frontFile = state.Parameters.GetFile(paramBase.Push(P_PARETO_FRONT_FILE), null);

            if (SilentFront)
            {
                FrontLog = Output.NO_LOGS;
            }
            else if (frontFile != null)
                try
                {
                    FrontLog = state.Output.AddLog(frontFile, !Compress, Compress);
                }
                catch (IOException i)
                {
                    state.Output.Fatal("An IOException occurred while trying to create the log " + frontFile + ":\n" + i);
                }
            else state.Output.Warning("No Pareto Front statistics file specified, printing to stdout at end.", paramBase.Push(P_PARETO_FRONT_FILE));
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Logs the best individual of the run.
        /// </summary>
        public override void FinalStatistics(IEvolutionState state, int result)
        {
            BypassFinalStatistics(state, result);  // just call base.base.finalStatistics(...)

            if (DoFinal) state.Output.PrintLn("\n\n\n PARETO FRONTS", StatisticsLog);
            for (var s = 0; s < state.Population.Subpops.Length; s++)
            {
                if (DoFinal)
                    state.Output.PrintLn("\n\nPareto Front of Subpopulation " + s, StatisticsLog);

                // build front
                var front = MultiObjectiveFitness.PartitionIntoParetoFront(state.Population.Subpops[s].Individuals, null, null);

                // sort by objective[0]
                var sortedFront = front.ToArray();
                QuickSort.QSort(sortedFront, new MultiObjectiveFitnessComparator());


                // print out front to statistics log
                if (DoFinal)
                    foreach (var t in sortedFront)
                        ((Individual)t).PrintIndividualForHumans(state, StatisticsLog);

                // write short version of front out to disk
                if (FrontLog >= 0)
                {
                    if (state.Population.Subpops.Length > 1)
                        state.Output.PrintLn("Subpopulation " + s, FrontLog);
                    foreach (var t in sortedFront)
                    {
                        var ind = (Individual)t;
                        var mof = (MultiObjectiveFitness)ind.Fitness;
                        var objectives = mof.GetObjectives();

                        var line = objectives.Aggregate("", (current, t1) => current + (t1 + " "));
                        state.Output.PrintLn(line, FrontLog);
                    }
                }
            }
        }

        #endregion // Operations
    }
}