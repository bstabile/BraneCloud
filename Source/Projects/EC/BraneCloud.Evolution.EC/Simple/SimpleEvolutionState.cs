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

using System;

using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Simple
{
    /// <summary> 
    /// A SimpleEvolutionState is an EvolutionState which implements a simple form of generational evolution.
    /// 
    /// <p/>First, all the individuals in the population are created.
    /// <b>(A)</b>Then all individuals in the population are evaluated.
    /// Then the population is replaced in its entirety with a new population
    /// of individuals bred from the old population.  Goto <b>(A)</b>.
    /// 
    /// <p/>Evolution stops when an ideal individual is found (if QuitOnRunComplete
    /// is set to true), or when the number of generations (loops of <b>(A)</b>)
    /// exceeds the parameter value NumGenerations.  Each generation the system
    /// will perform garbage collection and checkpointing, if the appropriate
    /// parameters were set.
    /// 
    /// <p/>This approach can be readily used for most applications of Genetic Algorithms and Genetic Programming.
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.simple.SimpleEvolutionState")]
    public class SimpleEvolutionState : EvolutionState
    {
        #region Operations

        /// <summary> </summary>
        public override void  StartFresh()
        {
            Output.Message("Setting up");
            Setup(this, null); // a garbage Parameter
            
            // POPULATION INITIALIZATION
            Output.Message("Initializing Generation 0");
            Statistics.PreInitializationStatistics(this);
            Population = Initializer.InitialPopulation(this, 0); // unthreaded
            Statistics.PostInitializationStatistics(this);

            // INITIALIZE CONTACTS -- done after initialization to allow
            // a hook for the user to do things in Initializer before
            // an attempt is made to connect to island models etc.
            Exchanger.InitializeContacts(this);
            Evaluator.InitializeContacts(this);
        }
        
        public override int Evolve()
        {
            if (Generation > 0)
                Output.Message("Generation " + Generation + "\tEvaluations So Far " + Evaluations);

            // EVALUATION
            Statistics.PreEvaluationStatistics(this);
            Evaluator.EvaluatePopulation(this);
            Statistics.PostEvaluationStatistics(this);

            // SHOULD WE QUIT?
            string runCompleteMessage = Evaluator.RunComplete(this);
            if (runCompleteMessage != null && QuitOnRunComplete)
            {
                Output.Message(runCompleteMessage);
                return R_SUCCESS;
            }

            // SHOULD WE QUIT?
            if (NumGenerations != UNDEFINED && Generation >= NumGenerations - 1 ||
                NumEvaluations != UNDEFINED && Evaluations >= NumEvaluations)
            {
                return R_FAILURE;
            }

            // INCREMENT GENERATION AND CHECKPOINT
            Generation++;

            // PRE-BREEDING EXCHANGING
            Statistics.PrePreBreedingExchangeStatistics(this);
            Population = Exchanger.PreBreedingExchangePopulation(this);
            Statistics.PostPreBreedingExchangeStatistics(this);
            
            var exchangerWantsToShutdown = Exchanger.RunComplete(this);
            if (exchangerWantsToShutdown != null)
            {
                Output.Message(exchangerWantsToShutdown);
                return R_SUCCESS;
            }
            
            // BREEDING
            Statistics.PreBreedingStatistics(this);
            
            Population = Breeder.BreedPopulation(this);
            
            // POST-BREEDING EXCHANGING
            Statistics.PostBreedingStatistics(this);
            
            // POST-BREEDING EXCHANGING
            Statistics.PrePostBreedingExchangeStatistics(this);
            Population = Exchanger.PostBreedingExchangePopulation(this);
            Statistics.PostPostBreedingExchangeStatistics(this);
            
            if (Checkpoint && (Generation - 1) % CheckpointModulo == 0)
            {
                Output.Message("Checkpointing");
                Statistics.PreCheckpointStatistics(this);
                EC.Util.Checkpoint.SetCheckpoint(this);
                Statistics.PostCheckpointStatistics(this);
            }
            
            return R_NOTDONE;
        }
        
        public override void Finish(int result)
        {
            Output.Message("Total Evaluations " + Evaluations);
            /* finish up -- we completed. */
            Statistics.FinalStatistics(this, result);
            Finisher.FinishPopulation(this, result);
            Exchanger.CloseContacts(this, result);
            Evaluator.CloseContacts(this, result);
        }

        #endregion // Operations
    }
}