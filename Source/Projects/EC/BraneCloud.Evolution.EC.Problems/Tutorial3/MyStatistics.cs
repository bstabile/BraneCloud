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

using System.IO;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.Problems.Tutorial3
{
    [ECConfiguration("ec.problems.tutorial3.MyStatistics")]
    public class MyStatistics : Statistics
    {
        // The parameter string and log number of the file for our readable population
        public const string P_POPFILE = "pop-file";
        public int popLog;

        // The parameter string and log number of the file for our best-genome-#3 individual
        public const string P_INFOFILE = "info-file";
        public int infoLog;

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // DO NOT FORGET to call super.Setup(...) !!
            base.Setup(state, paramBase);

            // set up popFile
            var popFile = state.Parameters.GetFile(paramBase.Push(P_POPFILE), null);
            if (popFile != null) try
                {
                    popLog = state.Output.AddLog(popFile, true);
                }
                catch (IOException i)
                {
                    state.Output.Fatal("An IOException occurred while trying to create the log " +
                        popFile + ":\n" + i);
                }

            // set up infoFile
            var infoFile = state.Parameters.GetFile(paramBase.Push(P_INFOFILE), null);
            if (infoFile != null) try
                {
                    infoLog = state.Output.AddLog(infoFile, true);
                }
                catch (IOException i)
                {
                    state.Output.Fatal("An IOException occurred while trying to create the log " + infoFile + ":\n" + i);
                }

        }

        public override void PostEvaluationStatistics(IEvolutionState state)
        {
            // be certain to call the hook on super!
            base.PostEvaluationStatistics(state);

            // write out a warning that the next generation is coming 
            state.Output.PrintLn("-----------------------\nGENERATION " +
                state.Generation + "\n-----------------------", popLog);

            // print out the population 
            state.Population.PrintPopulation(state, popLog);

            // print out best genome #3 individual in subpop 0
            var best = 0;
            var bestVal = ((DoubleVectorIndividual)state.Population.Subpops[0].Individuals[0]).genome[3];
            for (var y = 1; y < state.Population.Subpops[0].Individuals.Length; y++)
            {
                // We'll be unsafe and assume the individual is a DoubleVectorIndividual
                var val = ((DoubleVectorIndividual)state.Population.Subpops[0].Individuals[y]).genome[3];
                if (val <= bestVal) continue;
                best = y;
                bestVal = val;
            }
            state.Population.Subpops[0].Individuals[best].PrintIndividualForHumans(state, infoLog);
        }
    }
}