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
using System.IO;

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.GP
{
    /// <summary> 
    /// GPSpecies is a simple individual which is suitable as a species
    /// for GP subpops.  GPSpecies' individuals must be GPIndividuals,
    /// and often their pipelines are GPBreedingPipelines (at any rate,
    /// the pipelines will have to return members of GPSpecies!).
    /// 
    /// <p/><b>Default Base</b><br/>
    /// gp.Species
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.GPSpecies")]
    public class GPSpecies : Species
    {
        #region Constants

        public const string P_GPSPECIES = "species";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return GPDefaults.ParamBase.Push(P_GPSPECIES); }
        }

        #endregion // Properties
        #region Setup

        public override void  Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            
            // check to make sure that our individual prototype is a GPIndividual
            if (!(I_Prototype is GPIndividual))
                state.Output.Fatal("The Individual class for the Species " + GetType().FullName + " must be a subclass of ec.gp.GPIndividual.", paramBase);
        }

        #endregion // Setup
        #region NewIndividual

        public override Individual NewIndividual(IEvolutionState state, int thread)
        {
            var newind = ((GPIndividual) (I_Prototype)).LightClone();
            
            // Initialize the trees
            for (var x = 0; x < newind.Trees.Length; x++)
                newind.Trees[x].BuildTree(state, thread);
            
            // Set the fitness
            newind.Fitness = (Fitness) (F_Prototype.Clone());
            newind.Evaluated = false;
            
            // Set the species to me
            newind.Species = this;
            
            // ...and we're ready!
            return newind;
        }
                
        /// <summary>
        /// A custom version of newIndividual() which guarantees that the
        /// prototype is light-cloned before ReadIndividual is issued.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public override Individual NewIndividual(IEvolutionState state, StreamReader reader)
        {
            var newind = ((GPIndividual) I_Prototype).LightClone();
            
            // Set the fitness -- must be done BEFORE loading!
            newind.Fitness = (Fitness) (F_Prototype.Clone());
            newind.Evaluated = false; // for sanity's sake, though it's a useless line
            
            // load that sucker
            newind.ReadIndividual(state, reader);
            
            // Set the species to me
            newind.Species = this;
            
            // and we're ready!
            return newind;
        }
                
        /// <summary>
        /// A custom version of newIndividual() which guarantees that the
        /// prototype is light-cloned before ReadIndividual is issued.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="dataInput"></param>
        /// <returns></returns>
        public override Individual NewIndividual(IEvolutionState state, BinaryReader dataInput)
        {
            var newind = ((GPIndividual) I_Prototype).LightClone();
            
            // Set the fitness -- must be done BEFORE loading!
            newind.Fitness = (Fitness) (F_Prototype.Clone());
            newind.Evaluated = false; // for sanity's sake, though it's a useless line
            
            // Set the species to me
            newind.Species = this;
            
            // load that sucker
            newind.ReadIndividual(state, dataInput);
            
            // and we're ready!
            return newind;
        }

        #endregion // NewIndividual
    }
}