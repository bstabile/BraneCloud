using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.Problems.Mona
{
    [ECConfiguration("ec.problems.mona.MonaStatistics")]
    public class MonaStatistics : Statistics
    {
        public Individual BestOfRun { get; set; }

        /** Logs the best individual of the generation. */
        public override void PostEvaluationStatistics(IEvolutionState state)
        {
            base.PostEvaluationStatistics(state);

            bool newBest = false;
            for (int y = 0; y < state.Population.Subpops[0].Individuals.Length; y++)
                if (BestOfRun == null ||
                    state.Population.Subpops[0].Individuals[y].Fitness.BetterThan(BestOfRun.Fitness))
                {
                    BestOfRun = (Individual) state.Population.Subpops[0].Individuals[y].Clone();
                    newBest = true;
                }

            if (newBest)
            {
                ((ISimpleProblem) state.Evaluator.p_problem.Clone()).Describe(state, BestOfRun, 0, 0, 0);
            }
        }

        /** Logs the best individual of the run. */
        public override void FinalStatistics(IEvolutionState state, int result)
        {
            base.FinalStatistics(state, result);

            ((ISimpleProblem) state.Evaluator.p_problem.Clone()).Describe(state, BestOfRun, 0, 0, 0);
        }
    }
}
