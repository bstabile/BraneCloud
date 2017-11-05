using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Eval;

namespace BraneCloud.Evolution.EC.MultiObjective.SPEA2
{
    public interface ISPEA2Evaluator : IEvaluator
    {
        /// <summary>
        /// Computes the strength of individuals, then the raw fitness (wimpiness) and kth-closest sparsity
        /// measure.  Finally, computes the final fitness of the individuals.
        /// </summary>
        void ComputeAuxiliaryData(IEvolutionState state, Individual[] inds);

        /// <summary>
        /// Returns a matrix of sum squared distances from each individual to each other individual.
        /// </summary>
        double[][] CalculateDistances(IEvolutionState state, Individual[] inds);

        bool CloneProblem { get; set; }

    }
}