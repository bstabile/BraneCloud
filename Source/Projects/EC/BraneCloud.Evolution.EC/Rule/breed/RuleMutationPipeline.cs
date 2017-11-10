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
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Rule.Breed
{	
    /// <summary> 
    /// RuleMutationPipeline is a BreedingPipeline which implements a simple default Mutation
    /// for RuleIndividuals.  Normally it takes an individual and returns a mutated 
    /// child individual. RuleMutationPipeline works by calling Mutate(...) on each RuleSet in the 
    /// parent individual.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>Produce(...)</tt> call</b><br/>
    /// 1
    /// <p/><b>Number of Sources</b><br/>
    /// 1
    /// <p/><b>Default Base</b><br/>
    /// rule.mutate (not that it matters)
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.rule.breed.RuleMutationPipeline")]
    public class RuleMutationPipeline : BreedingPipeline
    {
        #region Constants

        public const string P_MUTATION = "mutate";
        public const int INDS_PRODUCED = 1;
        public const int NUM_SOURCES = 1;

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return RuleDefaults.ParamBase.Push(P_MUTATION); }
        }

        /// <summary>Returns 1 </summary>
        public override int NumSources
        {
            get { return NUM_SOURCES; }
        }

        /// <summary>Returns 1 </summary>
        // DO I need to change this?
        public override int TypicalIndsProduced
        {
            get { return INDS_PRODUCED; }
        }

        #endregion // Properties
        #region Operations

        public override int Produce(
            int min, 
            int max, 
            int subpop, 
            IList<Individual> inds, 
            IEvolutionState state, 
            int thread, 
            IDictionary<string, object> misc)
        {
            int start = inds.Count;

            // grab n individuals from our source and stick 'em right into inds.
            // we'll modify them from there
            var n = Sources[0].Produce(min, max, subpop, inds, state, thread, misc);

            // should we bother?
            if (!state.Random[thread].NextBoolean(Likelihood))
            {
                return n;
            }

            // mutate 'em
            for (var q = start; q < n + start; q++)
            {

                ((RuleIndividual)inds[q]).PreprocessIndividual(state, thread);

                //int len = ((RuleIndividual) inds[q]).Rulesets.Length;

                //for (var x = 0; x < len; x++)
                //{
                //    ((RuleIndividual) inds[q]).Rulesets[x].Mutate(state, thread);
                //}

                ((RuleIndividual)inds[q]).Mutate(state, thread);
                ((RuleIndividual)inds[q]).PostprocessIndividual(state, thread);

                ((RuleIndividual)inds[q]).Evaluated = false;
            }

            return n;
        }

        #endregion // Operations
    }
}