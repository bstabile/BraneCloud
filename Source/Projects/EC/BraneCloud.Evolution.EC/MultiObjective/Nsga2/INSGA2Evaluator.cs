using System.Collections.Generic;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Eval;

namespace BraneCloud.Evolution.EC.MultiObjective.NSGA2
{
    public interface INSGA2Evaluator
    {
        /// <summary>
        /// The original population size is stored here so NSGA2 knows how large to create the archive
        /// (it's the size of the original population -- keep in mind that NSGA2Breeder had made the 
        /// population larger to include the children.
        /// </summary>
        int[] OriginalPopSize { get; set; }

        bool CloneProblem { get; set; }
        IProblem p_problem { get; set; }
        IMasterProblem MasterProblem { get; set; }

        void Setup(IEvolutionState state, IParameter paramBase);

        /// <summary>
        /// Evaluates the population, then builds the archive and reduces the population to just the archive.
        /// </summary>
        /// <param name="state"></param>
        void EvaluatePopulation(IEvolutionState state);

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

        /// <summary>
        /// The SimpleEvaluator determines that a run is complete by asking
        /// each individual in each population if he's optimal; if he 
        /// finds an individual somewhere that's optimal,
        /// he signals that the run is complete. 
        /// </summary>
        bool RunComplete(IEvolutionState state);

        /// <summary>
        /// Called to set up remote evaluation network contacts when the run is started.  
        /// Mostly used for client/server evaluation (see MasterProblem).  
        /// By default calls p_problem.initializeContacts(state) 
        /// </summary>
        void InitializeContacts(IEvolutionState state);

        /// <summary>
        /// Called to reinitialize remote evaluation network contacts when the run is restarted from checkpoint.  
        /// Mostly used for client/server evaluation (see MasterProblem).  
        /// By default calls p_problem.ReinitializeContacts(state) 
        /// </summary>
        void ReinitializeContacts(IEvolutionState state);

        /// <summary>
        /// Called to shut down remote evaluation network contacts when the run is completed.  
        /// Mostly used for client/server evaluation (see MasterProblem).  
        /// By default calls p_problem.CloseContacts(state,result) 
        /// </summary>
        void CloseContacts(IEvolutionState state, int result);
    }
}