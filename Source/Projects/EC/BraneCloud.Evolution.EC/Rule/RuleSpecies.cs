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

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Rule
{
    /// <summary> 
    /// RuleSpecies is a simple individual which is suitable as a species
    /// for rule sets subpops.  RuleSpecies' individuals must be RuleIndividuals,
    /// and often their pipelines are RuleBreedingPipelines (at any rate,
    /// the pipelines will have to return members of RuleSpecies!).
    /// 
    /// <p/><b>Default Base</b><br/>
    /// rule.Species
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.rule.RuleSpecies")]
    public class RuleSpecies : Species
    {
        #region Constants

        public const string P_RULESPECIES = "species";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return RuleDefaults.ParamBase.Push(P_RULESPECIES); }
        }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            // check to make sure that our individual prototype is a RuleIndividual
            if (!(I_Prototype is RuleIndividual))
                state.Output.Fatal("The Individual class for the Species " + GetType().FullName
                                + " is must be a subclass of ec.rule.RuleIndividual.", paramBase);
        }

        #endregion // Setup
        #region Operations

        public override Individual NewIndividual(IEvolutionState state, int thread)
        {
            var newind = (RuleIndividual)(base.NewIndividual(state, thread));

            newind.Reset(state, thread);

            return newind;
        }

        #endregion // Operations
    }
}