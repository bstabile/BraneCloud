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
    /// exceeds the parameter value numGenerations.  Each generation the system
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
                Output.Message("Generation " + Generation);
            
            // EVALUATION
            Statistics.PreEvaluationStatistics(this);
            Evaluator.EvaluatePopulation(this);
            Statistics.PostEvaluationStatistics(this);
            
            // SHOULD WE QUIT?
            if (Evaluator.RunComplete(this) && QuitOnRunComplete)
            {
                Output.Message("Found Ideal Individual");
                return R_SUCCESS;
            }
            
            // SHOULD WE QUIT?
            if (Generation == NumGenerations - 1)
            {
                return R_FAILURE;
            }
            
            // PRE-BREEDING EXCHANGING
            Statistics.PrePreBreedingExchangeStatistics(this);
            Population = Exchanger.PreBreedingExchangePopulation(this);
            Statistics.PostPreBreedingExchangeStatistics(this);
            
            var exchangerWantsToShutdown = Exchanger.RunComplete(this);
            if (exchangerWantsToShutdown != null)
            {
                Output.Message(exchangerWantsToShutdown);
                /*
                * Don't really know what to return here.  The only place I could
                * find where runComplete ever returns non-null is 
                * IslandExchange.  However, that can return non-null whether or
                * not the ideal individual was found (for example, if there was
                * a communication error with the server).
                * 
                * Since the original version of this code didn't care, and the
                * result was initialized to R_SUCCESS before the while loop, I'm 
                * just going to return R_SUCCESS here. 
                */
                
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
            
            // INCREMENT GENERATION AND CHECKPOINT
            Generation++;
            if (Checkpoint && Generation % CheckpointModulo == 0)
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
            /* finish up -- we completed. */
            Statistics.FinalStatistics(this, result);
            Finisher.FinishPopulation(this, result);
            Exchanger.CloseContacts(this, result);
            Evaluator.CloseContacts(this, result);
        }

        #endregion // Operations
    }
}