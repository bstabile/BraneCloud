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

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.SingleState
{
    public class SingleStateEvolutionState : EvolutionState
    {
        public const string P_STATISTICS_MODULO = "stats-modulo";
        public const string P_EXCHANGE_MODULO = "exchange-modulo";

        /** In how many iterations do we collect statistics */
        public int StatisticsModulo { get; set; } = 1;

        /** In how many iterations do we perform an exchange */
        public int ExchangeModulo { get; set; } = 1;

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            IParameter p = new Parameter(P_STATISTICS_MODULO);
            StatisticsModulo = Parameters.GetInt(p, null, 1);
            if (StatisticsModulo == 0)
                Output.Fatal("The statistics modulo must be an integer > 0.", p);

            p = new Parameter(P_EXCHANGE_MODULO);
            ExchangeModulo = Parameters.GetInt(p, null, 1);
            if (ExchangeModulo == 0)
                Output.Fatal("The exchange modulo must be an integer > 0.", p);

            if (StatisticsModulo > ExchangeModulo ||
                ExchangeModulo % StatisticsModulo != 0)
                Output.Fatal("The exchange modulo should to be a multiple of the statistics modulo.", p);

            p = new Parameter(P_EXCHANGE_MODULO);
            if (StatisticsModulo > CheckpointModulo ||
                CheckpointModulo % StatisticsModulo != 0)
                Output.Fatal("The checkpoint modulo should to be a multiple of the statistics modulo.", p);
        }

        public override void StartFresh()
        {
            Output.Message("Setting up");
            Setup(this, null); // a garbage Parameter

            // POPULATION INITIALIZATION
            Output.Message("Initializing Generation 0");
            Statistics.PreInitializationStatistics(this);
            Population = Initializer.InitialPopulation(this, 0); // unthreaded
            Statistics.PostInitializationStatistics(this);

            // Compute generations from evaluations if necessary
            if (NumEvaluations > UNDEFINED)
            {
                // compute a generation's number of individuals
                int generationSize = 0;
                for (int sub = 0; sub < Population.Subpops.Count; sub++)
                {
                    generationSize +=
                        Population.Subpops[sub].Individuals
                            .Count; // so our sum total 'generationSize' will be the initial total number of individuals
                }

                if (NumEvaluations < generationSize)
                {
                    NumEvaluations = generationSize;
                    NumGenerations = 1;
                    Output.Warning("Using evaluations, but evaluations is less than the initial total population size ("
                                   + generationSize + ").  Setting to the populatiion size.");
                }
                else
                {
                    if (NumEvaluations % generationSize != 0)
                        Output.Warning(
                            "Using evaluations, but initial total population size does not divide evenly into it.  Modifying evaluations to a smaller value ("
                            + ((NumEvaluations / generationSize) * generationSize) +
                            ") which divides evenly."); // note integer division
                    NumGenerations = (int) (NumEvaluations / generationSize); // note integer division
                    NumEvaluations = NumGenerations * generationSize;
                }
                Output.Message("Generations will be " + NumGenerations);
            }

            // INITIALIZE CONTACTS -- done after initialization to allow
            // a hook for the user to do things in Initializer before
            // an attempt is made to connect to island models etc.
            Exchanger.InitializeContacts(this);
            Evaluator.InitializeContacts(this);
        }

        public override int Evolve()
        {
            bool isExchangeBorder = false;
            bool isStatisticsBorder = (Generation % StatisticsModulo == 0);
            if (isStatisticsBorder)
            {
                isExchangeBorder = (Generation % ExchangeModulo == 0);
            }

            if (isStatisticsBorder)
                Output.Message("Generation " + Generation + "\tEvaluations So Far " + Evaluations);

            // EVALUATION
            if (isStatisticsBorder)
                Statistics.PreEvaluationStatistics(this);
            Evaluator.EvaluatePopulation(this);
            if (isStatisticsBorder)
                Statistics.PostEvaluationStatistics(this);

            // SHOULD WE QUIT?
            string runCompleteMessage = Evaluator.RunComplete(this);
            if ((runCompleteMessage != null) && QuitOnRunComplete)
            {
                Output.Message(runCompleteMessage);
                return R_SUCCESS;
            }

            // SHOULD WE QUIT?
            if ((NumGenerations != UNDEFINED && Generation >= NumGenerations - 1) ||
                (NumEvaluations != UNDEFINED && Evaluations >= NumEvaluations))
            {
                return R_FAILURE;
            }

            // INCREMENT GENERATION AND CHECKPOINT
            Generation++;

            // PRE-BREEDING EXCHANGING
            if (isExchangeBorder)
            {
                Statistics.PrePreBreedingExchangeStatistics(this);
                Population = Exchanger.PreBreedingExchangePopulation(this);
                Statistics.PostPreBreedingExchangeStatistics(this);

                string exchangerWantsToShutdown = Exchanger.RunComplete(this);
                if (exchangerWantsToShutdown != null)
                {
                    Output.Message(exchangerWantsToShutdown);
                    return R_SUCCESS;
                }
            }

            // BREEDING
            if (isStatisticsBorder)
                Statistics.PreBreedingStatistics(this);
            Population = Breeder.BreedPopulation(this);
            if (isStatisticsBorder)
                Statistics.PostBreedingStatistics(this);

            // POST-BREEDING EXCHANGING
            if (isExchangeBorder)
            {
                Statistics.PrePostBreedingExchangeStatistics(this);
                Population = Exchanger.PostBreedingExchangePopulation(this);
                Statistics.PostPostBreedingExchangeStatistics(this);
            }

            if (isStatisticsBorder && Checkpoint && (Generation - 1) % CheckpointModulo == 0)
            {
                Output.Message("Checkpointing");
                Statistics.PreCheckpointStatistics(this);
                Util.Checkpoint.SetCheckpoint(this);
                Statistics.PostCheckpointStatistics(this);
            }

            return R_NOTDONE;
        }

        /**
         * @param result
         */
        public override void Finish(int result)
        {
            Output.Message("Total Evaluations " + Evaluations);
            Statistics.FinalStatistics(this, result);
            Finisher.FinishPopulation(this, result);
            Exchanger.CloseContacts(this, result);
            Evaluator.CloseContacts(this, result);
        }

    }
}