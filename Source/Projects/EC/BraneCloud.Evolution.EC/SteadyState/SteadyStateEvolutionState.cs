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
using System.Collections;

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.SteadyState
{
    /// <summary>
    /// This subclass of EvolutionState implements basic Steady-State Evolution and (in distributed form)
    /// Asynchronous Evolution. The procedure is as follows.  We begin with an empty Population and one by
    /// one create new Indivdiuals and send them off to be evaluated.  In basic Steady-State Evolution the
    /// individuals are immediately evaluated and we wait for them; but in Asynchronous Evolution the individuals are evaluated
    /// for however long it takes and we don't wait for them to finish.  When individuals return they are
    /// added to the Population until it is full.  No duplicate individuals are allowed.
    /// 
    /// <p/>At this point the system switches to its "steady state": individuals are bred from the population
    /// one by one, and sent off to be evaluated.  Once again, in basic Steady-State Evolution the
    /// individuals are immediately evaluated and we wait for them; but in Asynchronous Evolution the individuals are evaluated
    /// for however long it takes and we don't wait for them to finish.  When an individual returns, we
    /// mark an individual in the Population for death, then replace it with the new returning individual.
    /// Note that during the steady-state, Asynchronous Evolution could be still sending back some "new" individuals
    /// created during the initialization phase, not "bred" individuals.
    /// 
    /// <p/>The determination of how an individual is marked for death is done by the SteadyStateBreeder.
    /// 
    /// <p/>SteadyStateEvolutionState will run either for some N "generations" or for some M evaluations of
    /// individuals.   A "generation" is defined as a Population's worth of evaluations.   If you do not
    /// specify the number of evaluations (the M), then SteadyStateEvolutionState will use the standard
    /// generations parameter defined in EvolutionState.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><tt>evaluations</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(maximal number of evaluations to run.)</td></tr>
    /// <tr><td valign="top"><tt>steady.replacement-probability</tt><br/>
    /// <font size="-1">0.0 &lt;= double &lt;= 1.0 (default is 1.0)</font></td>
    /// <td valign="top"> (probability that an incoming individual will unilaterally replace the individual marked
    /// for death, as opposed to replacing it only if the incoming individual is superior in fitness)</td></tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.steadystate.SteadyStateEvolutionState")]
    public class SteadyStateEvolutionState : EvolutionState
    {
        #region Constants

        public const string P_REPLACEMENT_PROBABILITY = "replacement-probability";
        
        #endregion // Constants
        #region Properties

        /// <summary>
        /// Did we just start a new generation? 
        /// </summary>
        public bool GenerationBoundary { get; set; }

        /// <summary>
        /// How big is a generation? Set to the size of subpop 0 of the initial population. 
        /// </summary>
        public int GenerationSize { get; set; }

        /// <summary>
        /// When a new individual arrives, with what probability should it directly replace the existing
        /// "marked for death" individual, as opposed to only replacing it if it's superior?
        /// </summary>
        public double ReplacementProbability { get; set; }

        /// <summary>
        /// How many Evaluations have we run so far? 
        /// </summary>
        public long Evaluations { get; set; }

        #endregion // Properties
        #region Fields

        /// <summary>
        /// First time calling evolve?
        /// </summary>
        protected bool FirstTime;

        /// <summary>
        /// How many individuals have we added to the initial population? 
        /// </summary>
        private int[] IndividualCount;

        /// <summary>
        /// Hash table to check for duplicate individuals. 
        /// </summary>
        private Hashtable[] IndividualHash;

        /// <summary>
        /// Holds which subpop we are currently operating on. 
        /// </summary>
        private int WhichSubpop;

        private bool _justCalledPostEvaluationStatistics;

        #endregion
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            // double check that we have valid Evaluators and Breeders and exchangers
            if (!(Breeder is SteadyStateBreeder))
                state.Output.Error("You've chosen to use Steady-State Evolution, but your Breeder is not of the class SteadyStateBreeder.", paramBase);
            if (!(Evaluator is ISteadyStateEvaluator))
                state.Output.Error("You've chosen to use Steady-State Evolution, but your Evaluator is not of the class SteadyStateEvaluator.", paramBase);
            if (!(Exchanger is ISteadyStateExchanger))
                state.Output.Error("You've chosen to use Steady-State Evolution, but your exchanger does not implement the ISteadyStateExchanger.", paramBase);

            CheckStatistics(state, Statistics, paramBase);

            if (Parameters.ParameterExists(SteadyStateDefaults.ParamBase.Push(P_REPLACEMENT_PROBABILITY), null))
            {
                ReplacementProbability = Parameters.GetDoubleWithMax(SteadyStateDefaults.ParamBase.Push(P_REPLACEMENT_PROBABILITY), null, 0.0, 1.0);
                if (ReplacementProbability < 0.0) // uh oh
                    state.Output.Error("Replacement probability must be between 0.0 and 1.0 inclusive.",
                        SteadyStateDefaults.ParamBase.Push(P_REPLACEMENT_PROBABILITY), null);
            }
            else
            {
                ReplacementProbability = 1.0;  // always replace
                state.Output.Message("Replacement probability not defined: using 1.0 (always replace)");
            }
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Recursively prints out warnings for all Statistics that are not of steadystate Statistics form.
        /// </summary>
        internal virtual void CheckStatistics(IEvolutionState state, Statistics stat, IParameter paramBase)
        {
            if (!(stat is ISteadyStateStatistics))
                state.Output.Warning("You've chosen to use Steady-State Evolution, but your Statistics does not implement the ISteadyStateStatistics.", paramBase);
            for (var x = 0; x < stat.Children.Length; x++)
                if (stat.Children[x] != null)
                    CheckStatistics(state, stat.Children[x], paramBase.Push("child").Push("" + x));
        }

        public override void StartFresh()
        {
            Output.Message("Setting up");
            Setup(this, null); // a garbage Parameter

            // POPULATION INITIALIZATION
            Output.Message("Initializing Generation 0");
            Statistics.PreInitializationStatistics(this);
            Population = Initializer.SetupPopulation(this, 0); // unthreaded

            GenerationSize = 0;
            GenerationBoundary = false;
            FirstTime = true;
            Evaluations = 0;
            WhichSubpop = -1;

            IndividualHash = new Hashtable[Population.Subpops.Length];
            for (var i = 0; i < Population.Subpops.Length; i++)
            {
                IndividualHash[i] = new Hashtable();
            }

            IndividualCount = new int[Population.Subpops.Length];
            for (var sub = 0; sub < Population.Subpops.Length; sub++)
            {
                IndividualCount[sub] = 0;
                GenerationSize += Population.Subpops[sub].Individuals.Length; // so our sum total 'GenerationSize' will be the initial total number of individuals
            }

            if (NumEvaluations > UNDEFINED && NumEvaluations < GenerationSize)
                Output.Fatal("Number of Evaluations desired is smaller than the initial population of individuals");

            // INITIALIZE CONTACTS -- done after initialization to allow
            // a hook for the user to do things in Initializer before
            // an attempt is made to connect to island models etc.
            Exchanger.InitializeContacts(this);
            Evaluator.InitializeContacts(this);
        }

        public override int Evolve()
        {
            if (GenerationBoundary && Generation > 0)
            {
                Output.Message("Generation " + Generation + "\tEvaluations " + Evaluations);
                Statistics.GenerationBoundaryStatistics(this);
                Statistics.PostEvaluationStatistics(this);
                _justCalledPostEvaluationStatistics = true;
            }
            else
            {
                _justCalledPostEvaluationStatistics = false;
            }

            if (FirstTime)
            {
                if (Statistics is ISteadyStateStatistics)
                    ((ISteadyStateStatistics)Statistics).EnteringInitialPopulationStatistics(this);
                Statistics.PostInitializationStatistics(this);
                ((SteadyStateBreeder)Breeder).PrepareToBreed(this, 0); // unthreaded 
                ((ISteadyStateEvaluator)Evaluator).PrepareToEvaluate(this, 0); // unthreaded 
                FirstTime = false;
            }

            WhichSubpop = (WhichSubpop + 1) % Population.Subpops.Length;  // round robin selection

            // is the current subpop full? 
            var partiallyFullSubpop = (IndividualCount[WhichSubpop] < Population.Subpops[WhichSubpop].Individuals.Length);

            // MAIN EVOLVE LOOP 
            Individual ind = null;
            if (((ISteadyStateEvaluator)Evaluator).CanEvaluate)   // are we ready to evaluate? 
            {
                var numDuplicateRetries = Population.Subpops[WhichSubpop].NumDuplicateRetries;

                for (var tries = 0; tries <= numDuplicateRetries; tries++)  // see Subpopulation
                {
                    if (partiallyFullSubpop)   // is population full?
                    {
                        ind = Population.Subpops[WhichSubpop].Species.NewIndividual(this, 0);  // unthreaded 
                    }
                    else
                    {
                        ind = ((SteadyStateBreeder)Breeder).BreedIndividual(this, WhichSubpop, 0);
                        Statistics.IndividualsBredStatistics(this, new[] { ind });
                    }

                    if (numDuplicateRetries >= 1)
                    {
                        var o = IndividualHash[WhichSubpop][ind];
                        if (o == null)
                        {
                            IndividualHash[WhichSubpop][ind] = ind;
                            break;
                        }
                    }
                } // tried to cut down the duplicates 

                // evaluate the new individual
                ((ISteadyStateEvaluator)Evaluator).EvaluateIndividual(this, ind, WhichSubpop);
            }

            ind = ((ISteadyStateEvaluator)Evaluator).GetNextEvaluatedIndividual();
            if (ind != null)   // do we have an evaluated individual? 
            {
                var subpop = ((ISteadyStateEvaluator)Evaluator).GetSubpopulationOfEvaluatedIndividual();

                if (partiallyFullSubpop) // is subpopulation full? 
                {
                    Population.Subpops[subpop].Individuals[IndividualCount[subpop]++] = ind;

                    // STATISTICS FOR GENERATION ZERO 
                    if (IndividualCount[subpop] == Population.Subpops[subpop].Individuals.Length)
                        if (Statistics is ISteadyStateStatistics)
                            ((ISteadyStateStatistics)Statistics).EnteringSteadyStateStatistics(subpop, this);
                }
                else
                {
                    // mark individual for death 
                    var deadIndex = ((SteadyStateBreeder)Breeder).Deselectors[subpop].Produce(subpop, this, 0);
                    var deadInd = Population.Subpops[subpop].Individuals[deadIndex];

                    // replace dead individual with new individual 
                    if (ind.Fitness.BetterThan(deadInd.Fitness) || // it's better, we want it
                        Random[0].NextDouble() < ReplacementProbability) // it's not better but maybe we replace it directly anyway
                    {
                        Population.Subpops[subpop].Individuals[deadIndex] = ind;
                    }

                    // update duplicate hash table 
                    IndividualHash[subpop].Remove(deadInd);

                    if (Statistics is ISteadyStateStatistics)
                        ((ISteadyStateStatistics)Statistics).IndividualsEvaluatedStatistics(this,
                            new[] { ind }, new[] { deadInd }, new[] { subpop }, new[] { deadIndex });
                }

                // INCREMENT NUMBER OF COMPLETED EVALUATIONS
                Evaluations++;

                // COMPUTE GENERATION BOUNDARY
                GenerationBoundary = (Evaluations % GenerationSize == 0);
            }
            else
            {
                GenerationBoundary = false;
            }

            // SHOULD WE QUIT?
            if (!partiallyFullSubpop && Evaluator.RunComplete(this) && QuitOnRunComplete)
            {
                Output.Message("Found Ideal Individual");
                return R_SUCCESS;
            }

            if (NumEvaluations > UNDEFINED && Evaluations >= NumEvaluations ||  // using numEvaluations
                NumEvaluations <= UNDEFINED && GenerationBoundary && Generation == NumGenerations - 1)  // not using numEvaluations
            {
                return R_FAILURE;
            }


            // EXCHANGING
            if (GenerationBoundary)
            {
                // PRE-BREED EXCHANGE 
                Statistics.PrePreBreedingExchangeStatistics(this);
                Population = Exchanger.PreBreedingExchangePopulation(this);
                Statistics.PostPreBreedingExchangeStatistics(this);
                var exchangerWantsToShutdown = Exchanger.RunComplete(this);
                if (exchangerWantsToShutdown != null)
                {
                    Output.Message(exchangerWantsToShutdown);
                    return R_SUCCESS;
                }

                // POST BREED EXCHANGE
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
            }
            return R_NOTDONE;
        }

        public override void Finish(int result)
        {
            /* finish up -- we completed. */
            ((SteadyStateBreeder)Breeder).FinishPipelines(this);
            if (!_justCalledPostEvaluationStatistics)
                Statistics.PostEvaluationStatistics(this);
            Statistics.FinalStatistics(this, result);
            Finisher.FinishPopulation(this, result);
            Exchanger.CloseContacts(this, result);
            Evaluator.CloseContacts(this, result);
        }

        #endregion // Operations
    }
}