using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.GP
{
    public interface IGPProblem
    {
        /// <summary>
        /// GPProblem defines a default base so your subclass doesn't absolutely have to. 
        /// </summary>
        IParameter DefaultBase { get; }

        /// <summary>
        /// The GPProblem's Stack 
        /// </summary>
        ADFStack Stack { get; set; }

        /// <summary>
        /// The GPProblems' GPData 
        /// </summary>
        GPData Input { get; set; }

        /// <summary>
        /// Asynchronous Steady-State EC only: Returns true if the problem is ready to evaluate.  
        /// In most cases, the default is true.  
        /// </summary>
        bool CanEvaluate { get; }

        void Setup(IEvolutionState state, IParameter paramBase);
        object Clone();

        /// <summary>
        /// Called to set up remote evaluation network contacts when the run is started.  By default does nothing. 
        /// </summary>
        void InitializeContacts(IEvolutionState state);

        /// <summary>
        /// Called to reinitialize remote evaluation network contacts when the run is restarted from checkpoint.  
        /// By default does nothing. 
        /// </summary>
        void ReinitializeContacts(IEvolutionState state);

        /// <summary>
        /// Called to shut down remote evaluation network contacts when the run is completed.  
        /// By default does nothing. 
        /// </summary>
        void CloseContacts(IEvolutionState state, int result);

        /// <summary>
        /// May be called by the Evaluator prior to a series of individuals to 
        /// evaluate, and then ended with a finishEvaluating(...).  If this is the
        /// case then the Problem is free to delay modifying the individuals or their
        /// fitnesses until at finishEvaluating(...).  If no prepareToEvaluate(...)
        /// is called prior to evaluation, the Problem must complete its modification
        /// of the individuals and their fitnesses as they are evaluated as stipulated
        /// in the relevant evaluate(...) documentation for ISimpleProblem 
        /// or IGroupedProblem.  The default method does nothing.  Note that
        /// prepareToEvaluate() can be called *multiple times* prior to finishEvaluating()
        /// being called -- in this case, the subsequent calls may be ignored. 
        /// </summary>
        void PrepareToEvaluate(IEvolutionState state, int threadNum);

        /// <summary>
        /// Will be called by the Evaluator after prepareToEvaluate(...) is called
        /// and then a series of individuals are evaluated.  However individuals may
        /// be evaluated without prepareToEvaluate or finishEvaluating being called
        /// at all.  See the documentation for prepareToEvaluate for more information. 
        /// The default method does nothing.
        /// </summary>
        void FinishEvaluating(IEvolutionState state, int threadNum);

        /// <summary>
        /// Part of ISimpleProblem.  Included here so you don't have to write the default version, which usually does nothing.
        /// </summary>
        void Describe(IEvolutionState state, Individual ind, int subpop, int threadnum, int log);
    }
}