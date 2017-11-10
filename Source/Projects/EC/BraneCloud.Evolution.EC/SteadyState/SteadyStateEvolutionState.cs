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

        #endregion // Properties
        #region Fields

        /// <summary>
        /// First time calling evolve?
        /// </summary>
        protected bool FirstTime;

        ///// <summary>
        ///// How many individuals have we added to the initial population? 
        ///// </summary>
        //private int[] _individualCount;

        /// <summary>
        /// Hash table to check for duplicate individuals. 
        /// </summary>
        private Hashtable[] _individualHash;

        /// <summary>
        /// Holds which subpop we are currently operating on. 
        /// </summary>
        private int _whichSubpop;

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

            CheckStatistics(state, Statistics, new Parameter(P_STATISTICS));

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
            _whichSubpop = -1;

            _individualHash = new Hashtable[Population.Subpops.Count];
            for (var i = 0; i < Population.Subpops.Count; i++)
            {
                _individualHash[i] = new Hashtable();
            }

            for (var sub = 0; sub < Population.Subpops.Count; sub++)
            {
                GenerationSize += Population.Subpops[sub].InitialSize;
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

            // WARNING: BRS: Two variables _whichSubpop and whichSubpop (instance variable and local method variable)
            _whichSubpop = (_whichSubpop + 1) % Population.Subpops.Count;  // round robin selection

            // is the current subpop full? 
            bool partiallyFullSubpop = Population.Subpops[_whichSubpop].Individuals.Count < Population.Subpops[_whichSubpop].InitialSize;

            // MAIN EVOLVE LOOP 
            Individual ind = null;
            if (((ISteadyStateEvaluator)Evaluator).CanEvaluate)   // are we ready to evaluate? 
            {
                var numDuplicateRetries = Population.Subpops[_whichSubpop].NumDuplicateRetries;

                for (var tries = 0; tries <= numDuplicateRetries; tries++)  // see Subpopulation
                {
                    if (partiallyFullSubpop)   // is population full?
                    {
                        ind = Population.Subpops[_whichSubpop].Species.NewIndividual(this, 0);  // unthreaded 
                    }
                    else
                    {
                        ind = ((SteadyStateBreeder)Breeder).BreedIndividual(this, _whichSubpop, 0);
                        Statistics.IndividualsBredStatistics(this, new[] { ind });
                    }

                    if (numDuplicateRetries >= 1)
                    {
                        var o = _individualHash[_whichSubpop][ind];
                        if (o == null)
                        {
                            _individualHash[_whichSubpop][ind] = ind;
                            break;
                        }
                    }
                } // tried to cut down the duplicates 

                // evaluate the new individual
                ((ISteadyStateEvaluator)Evaluator).EvaluateIndividual(this, ind, _whichSubpop);
            }

            ind = ((ISteadyStateEvaluator)Evaluator).GetNextEvaluatedIndividual(this);
            int whichIndIndex = -1;
            // WARNING: BRS: Two variables _whichSubpop and whichSubpop (instance field and local variable)
            int whichSubpop = -1;
            if (ind != null)   // do we have an evaluated individual? 
            {
                // COMPUTE GENERATION BOUNDARY
                GenerationBoundary = Evaluations % GenerationSize == 0;

                if (GenerationBoundary)
                {
                    Statistics.PreEvaluationStatistics(this);
                }

                var subpop = ((ISteadyStateEvaluator)Evaluator).GetSubpopulationOfEvaluatedIndividual();
                whichSubpop = subpop;

                if (partiallyFullSubpop) // is subpopulation full? 
                {
                    Population.Subpops[subpop].Individuals.Add(ind);

                    // STATISTICS FOR GENERATION ZERO 
                    if (Population.Subpops[subpop].Individuals.Count == Population.Subpops[subpop].InitialSize)
                        if (Statistics is ISteadyStateStatistics)
                            ((ISteadyStateStatistics)Statistics).EnteringSteadyStateStatistics(subpop, this);
                }
                else
                {
                    // mark individual for death 
                    var deadIndex = ((SteadyStateBreeder)Breeder).Deselectors[subpop].Produce(subpop, this, 0);
                    var deadInd = Population.Subpops[subpop].Individuals[deadIndex];

                    // maybe replace dead individual with new individual 
                    if (ind.Fitness.BetterThan(deadInd.Fitness) || // it's better, we want it
                        Random[0].NextDouble() < ReplacementProbability) // it's not better but maybe we replace it directly anyway
                    {
                        Population.Subpops[subpop].Individuals[deadIndex] = ind;
                        whichIndIndex = deadIndex;
                    }

                    // update duplicate hash table 
                    _individualHash[subpop].Remove(deadInd);

                    if (Statistics is ISteadyStateStatistics)
                        ((ISteadyStateStatistics)Statistics).IndividualsEvaluatedStatistics(this,
                            new[] { ind }, new[] { deadInd }, new[] { subpop }, new[] { deadIndex });
                }

                if (GenerationBoundary)
                {
                    Statistics.PostEvaluationStatistics(this);
                }
            }
            else
            {
                GenerationBoundary = false;
            }

            // SHOULD WE QUIT?
            if (!partiallyFullSubpop && ((ISteadyStateEvaluator)Evaluator).IsIdeal(this, ind) && QuitOnRunComplete)
            {
                Output.Message("Individual " + whichIndIndex + " of subpopulation " + whichSubpop + " has an ideal fitness.");
                FinishEvaluationStatistics();
                return R_SUCCESS;
            }

            if (Evaluator.RunCompleted != null)
            {
                Output.Message(Evaluator.RunCompleted);
                FinishEvaluationStatistics();
                return R_SUCCESS;
            }

            if ((GenerationBoundary && NumEvaluations != UNDEFINED && Generation >= NumGenerations - 1) ||
                (NumEvaluations != UNDEFINED && Evaluations >= NumEvaluations))
            {
                FinishEvaluationStatistics();
                return R_FAILURE;
            }

            if (GenerationBoundary)
            {
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
                    FinishEvaluationStatistics();
                    return R_SUCCESS;
                }

                // POST-BREEDING EXCHANGING
                Statistics.PrePostBreedingExchangeStatistics(this);
                Population = Exchanger.PostBreedingExchangePopulation(this);
                Statistics.PostPostBreedingExchangeStatistics(this);
            }
            if (Checkpoint && GenerationBoundary && (Generation - 1) % CheckpointModulo == 0)
            {
                Output.Message("Checkpointing");
                Statistics.PreCheckpointStatistics(this);
                Util.Checkpoint.SetCheckpoint(this);
                Statistics.PostCheckpointStatistics(this);
            }
            return R_NOTDONE;
        }

        public void FinishEvaluationStatistics()
        {
            bool generationBoundary = Evaluations % GenerationSize == 0;
            if (!generationBoundary)
            {
                Statistics.PostEvaluationStatistics(this);
                Output.Message("Generation " + Generation + " Was Partial");
            }
        }

        public override void Finish(int result)
        {
            Output.Message("Total Evaluations " + Evaluations);
            ((SteadyStateBreeder)Breeder).FinishPipelines(this);
            Statistics.FinalStatistics(this, result);
            Finisher.FinishPopulation(this, result);
            Exchanger.CloseContacts(this, result);
            Evaluator.CloseContacts(this, result);
        }

        #endregion // Operations
    }
}