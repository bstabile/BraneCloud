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

namespace BraneCloud.Evolution.EC
{
    /// <summary> 
    /// Species is a prototype which defines the features for a set of individuals
    /// in the population.  Typically, individuals may breed if they belong to the
    /// same species (but it's not a hard-and-fast rule).  Each Subpopulation has
    /// one Species object which defines the species for individuals in that
    /// Subpopulation.
    /// 
    /// <p/>Species are generally responsible for creating individuals, through
    /// their newIndividual(...) method.  This method usually clones its prototypical
    /// individual and makes some additional modifications to the clone, then returns it.
    /// Note that the prototypical individual does <b>not need to be a complete individual</b> --
    /// for example, GPSpecies holds a GPIndividual which doesn't have any trees (the tree
    /// roots are null).
    /// 
    /// <p/>Species also holds a prototypical breeding pipeline meant to breed
    /// this individual.  To breed individuals of this species, clone the pipeline
    /// and use the clone.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>ind</tt><br/>
    /// <font size="-1">classname, inherits and != ec.Individual</font></td>
    /// <td valign="top">(the class for the prototypical individual for the species)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>numpipes</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(total number of breeding pipelines for the species)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>pipe</tt><br/>
    /// <font size="-1">classname, inherits and != ec.BreedingPipeline</font></td>
    /// <td valign="top">(the class for the prototypical Breeding Pipeline)</td></tr>
    /// </table>
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>ind</tt></td>
    /// <td>I_Prototype (the prototypical individual)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>pipe</tt></td>
    /// <td>Pipe_Prototype (breeding pipeline prototype)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>fitness</tt></td>
    /// <td>F_Prototype (the prototypical fitness)</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.Species")]
    public abstract class Species : IPrototype
    {
        #region Constants

        public const string P_INDIVIDUAL = "ind";
        public const string P_PIPE = "pipe";
        public const string P_FITNESS = "fitness";

        #endregion // Constants
        #region Properties

        public abstract IParameter DefaultBase { get; }

        /// <summary>
        /// The prototypical individual for this species. 
        /// </summary>
        public Individual I_Prototype { get; set; }

        /// <summary>
        /// The prototypical breeding pipeline for this species. 
        /// </summary>
        public BreedingPipeline Pipe_Prototype { get; set; }

        /// <summary>
        /// The prototypical fitness for individuals of this species. 
        /// </summary>
        public Fitness F_Prototype { get; set; }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// The default version of Setup(...) loads requested pipelines and calls Setup(...) on them and normalizes their probabilities.  
        /// If your individual prototype might need to know special things about the species (like parameters stored in it),
        /// then when you override this Setup method, you'll need to set those parameters BEFORE you call super.Setup(...),
        /// because the Setup(...) code in Species sets up the prototype.
        /// </summary>
        /// <seealso cref="IPrototype.Setup(IEvolutionState,IParameter)" />		
        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            var def = DefaultBase;

            // load the breeding pipeline
            Pipe_Prototype = (BreedingPipeline)(state.Parameters.GetInstanceForParameter(paramBase.Push(P_PIPE), def.Push(P_PIPE), typeof(BreedingPipeline)));
            Pipe_Prototype.Setup(state, paramBase.Push(P_PIPE));

            // I promised over in BreedingSource.java that this method would get called.
            state.Output.ExitIfErrors();

            // load our individual prototype
            I_Prototype = (Individual)(state.Parameters.GetInstanceForParameter(paramBase.Push(P_INDIVIDUAL), def.Push(P_INDIVIDUAL), typeof(Individual)));
            // set the species to me before setting up the individual, so they know who I am
            I_Prototype.Species = this;
            I_Prototype.Setup(state, paramBase.Push(P_INDIVIDUAL));

            // load our fitness
            F_Prototype = (Fitness)state.Parameters.GetInstanceForParameter(paramBase.Push(P_FITNESS), def.Push(P_FITNESS), typeof(Fitness));
            F_Prototype.Setup(state, paramBase.Push(P_FITNESS));
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Provides a brand-new individual to fill in a population.  The default form
        /// simply calls clone(), creates a fitness, sets evaluated to false, and sets
        /// the species.  If you need to make a more custom genotype (as is the case
        /// for GPSpecies, which requires a light rather than deep clone), 
        /// you will need to override this method as you see fit.
        /// </summary>		
        public virtual Individual NewIndividual(IEvolutionState state, int thread)
        {
            var newind = (Individual)I_Prototype.Clone();

            // Set the fitness
            newind.Fitness = (Fitness)F_Prototype.Clone();
            newind.Evaluated = false;

            // Set the species to me
            newind.Species = this;

            // ...and we're ready!
            return newind;
        }

        /// <summary>
        /// Provides an individual read from a stream, including
        /// the fitness; the individual will
        /// appear as it was written by printIndividual(...).  Doesn't 
        /// close the stream.  Sets evaluated to false and sets the species.
        /// If you need to make a more custom mechanism (as is the case
        /// for GPSpecies, which requires a light rather than deep clone), 
        /// you will need to override this method as you see fit.
        /// </summary>		
        public virtual Individual NewIndividual(IEvolutionState state, StreamReader reader)
        {
            var newind = (Individual)(I_Prototype.Clone());

            // Set the fitness
            newind.Fitness = (Fitness)(F_Prototype.Clone());
            newind.Evaluated = false; // for sanity's sake, though it's a useless line

            // load that sucker
            newind.ReadIndividual(state, reader);

            // Set the species to me
            newind.Species = this;

            // and we're ready!
            return newind;
        }

        /// <summary>
        /// Provides an individual read from a DataInput source, including
        /// the fitness.  Doesn't 
        /// close the DataInput.  Sets evaluated to false and sets the species.
        /// If you need to make a more custom mechanism (as is the case
        /// for GPSpecies, which requires a light rather than deep clone), 
        /// you will need to override this method as you see fit.
        /// </summary>		
        public virtual Individual NewIndividual(IEvolutionState state, BinaryReader dataInput)
        {
            var newInd = (Individual)(I_Prototype.Clone());

            // Set the fitness
            newInd.Fitness = (Fitness)(F_Prototype.Clone());
            newInd.Evaluated = false; // for sanity's sake, though it's a useless line

            // Set the species to me
            newInd.Species = this;

            // load that sucker
            newInd.ReadIndividual(state, dataInput);

            // and we're ready!
            return newInd;
        }

        #endregion // Operations
        #region Cloning

        public virtual object Clone()
        {
            try
            {
                var myobj = (Species) (base.MemberwiseClone());
                myobj.I_Prototype = (Individual) I_Prototype.Clone();
                myobj.F_Prototype = (Fitness) F_Prototype.Clone();
                myobj.Pipe_Prototype = (BreedingPipeline) Pipe_Prototype.Clone();
                return myobj;
            }
            catch (Exception)
            {
                throw new ApplicationException();
            } // never happens
        }

        #endregion // Cloning
    }
}