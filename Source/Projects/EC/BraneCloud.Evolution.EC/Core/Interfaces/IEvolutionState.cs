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
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC
{
    [ECConfiguration("ec.IEvolutionState")]
    public interface IEvolutionState : ISingleton
    {
        IParameterDatabase Parameters { get; set; }
        IMersenneTwister[] Random { get; set; }
        Output Output { get; set; }

        int BreedThreads { get; set; }
        int EvalThreads { get; set; }

        bool Checkpoint { get; set; }
        DirectoryInfo CheckpointDirectory { get; set; }
        string CheckpointPrefix { get; set; }
        int CheckpointModulo { get; set; }

        int RandomSeedOffset { get; set; }
        bool QuitOnRunComplete { get; set; }
        string[] RuntimeArguments { get; set; }

        int Generation { get; set; }
        int NumGenerations { get; set; }

        object[] Job { get; set; }

        Population Population { get; set; }
        Initializer Initializer { get; set; }
        IFinisher Finisher { get; set; }
        Breeder Breeder { get; set; }
        IEvaluator Evaluator { get; set; }
        Statistics Statistics { get; set; }
        Exchanger Exchanger { get; set; }

        void ResetFromCheckpoint();
        void Finish(int result);
        void StartFromCheckpoint();
        void StartFresh();
        int Evolve();
        void IncrementEvaluations(int val);
        void Run(int condition);
    }
}