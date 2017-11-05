using System.Collections.Generic;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Eval;

namespace BraneCloud.Evolution.EC.MultiObjective.NSGA2
{
    public interface INSGA2Evaluator : IEvaluator
    {
        /// <summary>
        /// The original population size is stored here so NSGA2 knows how large to create the archive
        /// (it's the size of the original population -- keep in mind that NSGA2Breeder had made the 
        /// population larger to include the children.
        /// </summary>
        int[] OriginalPopSize { get; set; }

        bool CloneProblem { get; set; }

        /// <summary>
        /// Build the auxiliary fitness data and reduce the subpopulation to just the archive, which is returned.
        /// </summary>
        Individual[] BuildArchive(IEvolutionState state, int subpop);

        /// <summary>
        /// Divides inds into ranks and assigns each individual's rank to be the rank it was placed into.
        /// Each front is a List.
        /// </summary>
        /// <param name="subpop"></param>
        /// <returns></returns>
        IList<IList<Individual>> AssignFrontRanks(Subpopulation subpop);

        /// <summary>
        /// Computes and assigns the sparsity values of a given front.
        /// </summary>
        /// <param name="front"></param>
        void AssignSparsity(Individual[] front);


    }
}