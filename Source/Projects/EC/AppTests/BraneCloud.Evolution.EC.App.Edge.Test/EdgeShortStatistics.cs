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
using BraneCloud.Evolution.EC.GP.Koza;

namespace BraneCloud.Evolution.EC.App.Edge.Test
{
    [ECConfiguration("ec.app.edge.EdgeShortStatistics")]
    public class EdgeShortStatistics : KozaShortStatistics
    {
        public override void PostEvaluationStatistics(IEvolutionState state)
        {
            // compute and print out the other statistics -- we depend on it!
            base.PostEvaluationStatistics(state);

            // we have only one population, so this is kosher
            state.Output.Print(((Edge)(state.Evaluator.p_problem.Clone())).
                DescribeShortGeneralized(BestSoFar[0], state, 0, 0),
                StatisticsLog);
        }

    }
}