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
using System.Collections.Generic;
using BraneCloud.Evolution.EC.Eval;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.SteadyState
{
    /// <summary>
    /// This subclass of Evaluator performs the evaluation portion of Steady-State Evolution and (in distributed form)
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
    /// <p/>When SteadyStateEvaluator sends indivduals off to be evaluated, it stores them in an internal queue, along
    /// with the subpopulation in which they were destined.  This tuple is defined by QueueIndividual.java
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.steadystate.SteadyStateEvaluator")]
    public class SteadyStateEvaluator : SimpleEvaluator, ISteadyStateEvaluator
    {
        #region Fields

        readonly List<QueueIndividual> _queue = new List<QueueIndividual>();

        /// <summary>
        /// Holds the subpopulation currently being evaluated.
        /// </summary>
        int _subpopulationBeingEvaluated = -1;

        /// <summary>
        /// Our problem.
        /// </summary>
        ISimpleProblem _problem;

        #endregion // Fields
        #region Properties

        /// <summary>
        /// Returns true if we're ready to evaluate an individual.  Ordinarily this is ALWAYS true,
        /// except in the asynchronous evolution situation, where we may not have a processor ready yet.
        /// </summary>
        public bool CanEvaluate
        {
            get
            {
                if (_problem is IMasterProblem)
                    return ((IProblem)_problem).CanEvaluate;

                return true;
            }
        }

        #endregion // Properties
        #region Setup

        public void Setup(EvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            if (!CloneProblem)
                state.Output.Fatal(
                    "cloneProblem must be true for SteadyStateEvaluator -- we'll use only one Problem anyway.");
        }

        #endregion // Setup
        #region Operations

        public void PrepareToEvaluate(EvolutionState state, int thread)
        {
            _problem = (ISimpleProblem)p_problem.Clone();

            //   We only call prepareToEvaluate during Asynchronous Evolution.
            if (_problem is IMasterProblem)
                ((IProblem)_problem).PrepareToEvaluate(state, thread);
        }

        /// <summary>
        /// Submits an individual to be evaluated by the Problem, and adds it and its subpopulation to the queue.
        /// </summary>
        public void EvaluateIndividual(IEvolutionState state, Individual ind, int subpop)
        {
            _problem.Evaluate(state, ind, subpop, 0);
            _queue.Add(new QueueIndividual(ind, subpop));
        }

        /// <summary>
        /// Returns an evaluated individual is in the queue and ready to come back to us. 
        /// Ordinarily this is ALWAYS true at the point that we call it, except in the asynchronous 
        /// evolution situation, where we may not have a job completed yet, in which case NULL is
        /// returned. Once an individual is returned by this function, no other individual will
        /// be returned until the system is ready to provide us with another one.  NULL will
        /// be returned otherwise.
        /// </summary>
        public Individual GetNextEvaluatedIndividual()
        {
            QueueIndividual qind = null;

            if (_problem is IMasterProblem)
            {
                if (((IMasterProblem)_problem).EvaluatedIndividualAvailable)
                    qind = ((IMasterProblem)_problem).GetNextEvaluatedIndividual();
            }
            else
            {
                qind = (QueueIndividual)_queue[0];
                _queue.RemoveAt(0);
            }

            if (qind == null) return null;

            _subpopulationBeingEvaluated = qind.Subpop;
            return qind.Ind;
        }

        /// <summary>
        /// Returns the subpopulation of the last evaluated individual returned by getNextEvaluatedIndividual, 
        /// or potentially -1 if getNextEvaluatedIndividual was never called or hasn't returned an individual yet.
        /// </summary>
        public int GetSubpopulationOfEvaluatedIndividual()
        {
            return _subpopulationBeingEvaluated;
        }

        /// <summary>
        /// The SimpleEvaluator determines that a run is complete by asking
        /// each individual in each population if he's optimal; if he 
        /// finds an individual somewhere that's optimal,
        /// he signals that the run is complete. 
        /// </summary>
        public bool RunComplete(IEvolutionState state, Individual ind)
        {
            return ind.Fitness.IsIdeal;
        }
        #endregion // Operations
    }
}