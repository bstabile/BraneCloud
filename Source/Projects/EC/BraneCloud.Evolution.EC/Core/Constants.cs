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

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC
{
    [ECConfiguration("ec.Constants")]
    public static class Constants
    {
        /// <summary>
        /// The population has started fresh (not from a Checkpoint). 
        /// </summary>
        public const int C_STARTED_FRESH = 0;

        /// <summary>
        /// The population started from a Checkpoint. 
        /// </summary>
        public const int C_STARTED_FROM_CHECKPOINT = 1;

        public const int GE_LEXER_FAILURE = -1;

        /// <summary>
        /// The evolution run has quit, finding a perfect individual. 
        /// </summary>
        public const int R_SUCCESS = 0;

        /// <summary>
        /// The evolution run has quit, failing to find a perfect individual. 
        /// </summary>
        public const int R_FAILURE = 1;

        /// <summary>
        /// The evolution run has not quit. 
        /// </summary>
        public const int R_NOTDONE = 2;

        public const string P_INITIALIZER = "init";
        public const string P_FINISHER = "finish";
        public const string P_BREEDER = "breed";
        public const string P_EVALUATOR = "eval";
        public const string P_STATISTICS = "stat";
        public const string P_EXCHANGER = "exch";
        public const string P_GENERATIONS = "generations";
        public const string P_QUITONRUNCOMPLETE = "quit-on-run-complete";
        public const string P_CHECKPOINTPREFIX = "prefix";
        public const string P_CHECKPOINTMODULO = "checkpoint-modulo";
        public const string P_CHECKPOINT = "checkpoint";
    }
}