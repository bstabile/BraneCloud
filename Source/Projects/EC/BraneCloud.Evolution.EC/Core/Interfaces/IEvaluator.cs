using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Eval;

namespace BraneCloud.Evolution.EC
{
    public interface IEvaluator : ISingleton
    {
        IProblem p_problem { get; set; }
        IMasterProblem MasterProblem { get; set; }

        //void Setup(IEvolutionState state, IParameter paramBase);

        /// <summary>
        /// Evaluates the fitness of an entire population.  You will
        /// have to determine how to handle multiple threads on your own,
        /// as this is a very domain-specific thing. 
        /// </summary>
        void EvaluatePopulation(IEvolutionState state);

        /// <summary>
        /// Returns true if an ideal individual has been found or some
        /// other run result has shortcircuited the run so that it should
        /// end prematurely right now. 
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