using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Eval;

namespace BraneCloud.Evolution.EC.SteadyState
{
    public interface ISteadyStateEvaluator : IEvaluator
    {
        /// <summary>
        /// Returns true if we're ready to evaluate an individual.  Ordinarily this is ALWAYS true,
        /// except in the asynchronous evolution situation, where we may not have a processor ready yet.
        /// </summary>
        bool CanEvaluate { get; }

        bool CloneProblem { get; set; }

        void PrepareToEvaluate(IEvolutionState state, int thread);

        /// <summary>
        /// Submits an individual to be evaluated by the Problem, and adds it and its subpopulation to the queue.
        /// </summary>
        void EvaluateIndividual(IEvolutionState state, Individual ind, int subpop);

        /// <summary>
        /// Returns an evaluated individual is in the queue and ready to come back to us. 
        /// Ordinarily this is ALWAYS true at the point that we call it, except in the asynchronous 
        /// evolution situation, where we may not have a job completed yet, in which case NULL is
        /// returned. Once an individual is returned by this function, no other individual will
        /// be returned until the system is ready to provide us with another one.  NULL will
        /// be returned otherwise.
        /// </summary>
        Individual GetNextEvaluatedIndividual();

        /// <summary>
        /// Returns the subpopulation of the last evaluated individual returned by getNextEvaluatedIndividual, 
        /// or potentially -1 if getNextEvaluatedIndividual was never called or hasn't returned an individual yet.
        /// </summary>
        int GetSubpopulationOfEvaluatedIndividual();

        /// <summary>
        /// The SimpleEvaluator determines that a run is complete by asking
        /// each individual in each population if he's optimal; if he 
        /// finds an individual somewhere that's optimal,
        /// he signals that the run is complete. 
        /// </summary>
        bool IsIdeal(IEvolutionState state, Individual ind);

    }
}