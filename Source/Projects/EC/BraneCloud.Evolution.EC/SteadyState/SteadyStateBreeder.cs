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
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.SteadyState
{
    /// <summary>
    /// This subclass of Breeder performs the evaluation portion of Steady-State Evolution and (in distributed form)
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
    /// <p/>The determination of how an individual is marked for death is done by the SteadyStateBreeder.  This is
    /// a SelectionMethod.  Note that this SelectionMethod probably should <i>not</i> be selecting for the "fittest"
    /// individuals, but rather for either random individuals (the standard approach) or for "bad" individuals.
    /// 
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><tt>deselector</tt><br/>
    /// <font size="-1">classname, inherits and != ec.SelectionMethod</font></td>
    /// <td valign="top">(The SelectionMethod used to pick individuals for death)</td></tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.steadystate.SteadyStateBreeder")]
    public class SteadyStateBreeder : SimpleBreeder
    {
        #region Constants

        private const long SerialVersionUID = 1;

        public const string P_DESELECTOR = "deselector";

        #endregion // Constants
        #region Properties

        /// <summary>
        /// If st.firstTimeAround, this acts exactly like SimpleBreeder.
        /// Else, it only breeds one new individual per subpop, to  place in position 0 of the subpop.  
        /// </summary>
        public BreedingSource[] BP { get; set; }

        /// <summary>
        /// Loaded during the first iteration of breedPopulation 
        /// </summary>
        public SelectionMethod[] Deselectors { get; set; }

        /// <summary>
        /// Do we allow duplicates? 
        /// </summary>
        public int NumDuplicateRetries { get; set; }

        #endregion // Properties
        #region Setup

        public SteadyStateBreeder()
        {
            BP = null;
            Deselectors = null;
        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            if (!ClonePipelineAndPopulation)
                state.Output.Fatal("clonePipelineAndPopulation must be true for SteadyStateBreeder -- we'll use only one Pipeline anyway.");

            var p = new Parameter(Initializer.P_POP).Push(Population.P_SIZE);
            var size = state.Parameters.GetInt(p, null, 1);

            // if size is wrong, we'll let Population complain about it  -- for us, we'll just make 0-sized arrays and drop out.
            if (size > 0)
                Deselectors = new SelectionMethod[size];

            // load the Deselectors
            for (var x = 0; x < Deselectors.Length; x++)
            {
                Deselectors[x] = (SelectionMethod)(state.Parameters.GetInstanceForParameter(
                    SteadyStateDefaults.ParamBase.Push(P_DESELECTOR).Push("" + x), null, typeof(SelectionMethod)));
                if (!(Deselectors[x] is ISteadyStateBSource))
                    state.Output.Error("Deselector for subpop " + x + " is not of ISteadyStateBSource.");
                Deselectors[x].Setup(state, SteadyStateDefaults.ParamBase.Push(P_DESELECTOR).Push("" + x));
            }
            state.Output.ExitIfErrors();

            if (SequentialBreeding) // uh oh
                state.Output.Fatal("SteadyStateBreeder does not support sequential evaluation.",
                    paramBase.Push(P_SEQUENTIAL_BREEDING));

            //// How often do we retry if we find a duplicate?
            //NumDuplicateRetries = state.Parameters.GetInt(SteadyStateDefaults.ParamBase.Push(P_RETRIES), null, 0);
            //if (NumDuplicateRetries < 0)
            //    state.Output.Fatal("The number of retries for duplicates must be an integer >= 0.\n", paramBase.Push(P_RETRIES), null);
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Called to check to see if the breeding sources are correct -- if you
        /// use this method, you must call state.Output.ExitIfErrors() immediately  afterwards. 
        /// </summary>
        public virtual void SourcesAreProperForm(SteadyStateEvolutionState state, BreedingSource[] breedingSources)
        {
            foreach (BreedingSource bp in breedingSources)
            {
                // all breeding sources are ISteadyStateBSource
                ((ISteadyStateBSource)bp).SourcesAreProperForm(state);
            }
        }

        /// <summary>
        /// Called whenever individuals have been replaced by new individuals in the population. 
        /// </summary>
        public virtual void IndividualReplaced(SteadyStateEvolutionState state, int subpop, int thread, int individual)
        {
            for (var x = 0; x < BP.Length; x++)
                ((ISteadyStateBSource)BP[x]).IndividualReplaced(state, subpop, thread, individual);
            // let the deselector know
            ((ISteadyStateBSource)Deselectors[subpop]).IndividualReplaced(state, subpop, thread, individual);
        }

        public virtual void FinishPipelines(IEvolutionState state)
        {
            for (var x = 0; x < Deselectors.Length; x++)
            {
                BP[x].FinishProducing(state, x, 0);
                Deselectors[x].FinishProducing(state, x, 0);
            }
        }

        public virtual void PrepareToBreed(IEvolutionState state, int thread)
        {
            var st = (SteadyStateEvolutionState)state;
            // set up the breeding pipelines
            BP = new BreedingSource[st.Population.Subpops.Count];
            for (var pop = 0; pop < BP.Length; pop++)
            {
                BP[pop] = (BreedingSource)st.Population.Subpops[pop].Species.Pipe_Prototype.Clone();
                if (!BP[pop].Produces(st, st.Population, pop, 0))
                    st.Output.Error("The Breeding Source of subpop " + pop + " does not produce individuals of the expected species "
                        + st.Population.Subpops[pop].Species.GetType().FullName + " and with the expected Fitness class "
                        + st.Population.Subpops[pop].Species.F_Prototype.GetType().FullName);
                BP[pop].FillStubs(state, null);
            }
            // are they of the proper form?
            SourcesAreProperForm(st, BP);
            // because I promised when calling SourcesAreProperForm
            st.Output.ExitIfErrors();

            // warm them up
            for (var pop = 0; pop < BP.Length; pop++)
            {
                BP[pop].PrepareToProduce(state, pop, 0);
                Deselectors[pop].PrepareToProduce(state, pop, 0);
            }
        }

        public virtual Individual BreedIndividual(IEvolutionState state, int subpop, int thread)
        {
            // this is inefficient but whatever...
            // BRS: It's not so inefficient if we set the capacity explicitly!
            var newind = new List<Individual>(1);

            // breed a single individual
            var newMisc = state.Population.Subpops[subpop].Species.BuildMisc(state, subpop, thread);
            BP[subpop].Produce(1, 1, subpop, newind, state, thread, newMisc);
            return newind[0];
        }

        #endregion // Operations
    }
}