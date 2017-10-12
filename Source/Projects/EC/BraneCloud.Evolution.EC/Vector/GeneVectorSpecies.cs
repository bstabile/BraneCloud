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

namespace BraneCloud.Evolution.EC.Vector
{	
    /// <summary> 
    /// GeneVectorSpecies is a subclass of VectorSpecies with special
    /// constraints for GeneVectorIndividuals.
    /// 
    /// <p/>At present there is exactly one item stored in GeneVectorSpecies:
    /// the prototypical VectorGene that populates the genome array stored in a
    /// GeneVectorIndividual.
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.vector.GeneVectorSpecies")]
    public class GeneVectorSpecies : VectorSpecies
    {
        #region Constants

        public const string P_GENE = "gene";

        #endregion // Constants
        #region Properties

        public VectorGene GenePrototype { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            var def = DefaultBase;

            GenePrototype = (VectorGene)(state.Parameters.GetInstanceForParameterEq(paramBase.Push(P_GENE), def.Push(P_GENE), typeof(VectorGene)));
            GenePrototype.Setup(state, paramBase.Push(P_GENE));

            // make sure that super.Setup is done AFTER we've loaded our gene prototype.
            base.Setup(state, paramBase);
        }

        #endregion // Setup
    }
}