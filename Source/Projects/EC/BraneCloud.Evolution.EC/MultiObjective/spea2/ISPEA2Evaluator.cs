using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Eval;

namespace BraneCloud.Evolution.EC.MultiObjective.SPEA2
{
    public interface ISPEA2Evaluator
    {
        /// <summary>
        /// A simple evaluator that doesn't do any coevolutionary
        /// evaluation.  Basically it applies evaluation pipelines,
        /// one per thread, to various subchunks of a new population. 
        /// </summary>
        void EvaluatePopulation(IEvolutionState state);

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
        IProblem p_problem { get; set; }
        IMasterProblem MasterProblem { get; set; }

        /// <summary>
        /// Checks to make sure that the Problem implements ISimpleProblem.
        /// </summary>
        void Setup(IEvolutionState state, IParameter paramBase);

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