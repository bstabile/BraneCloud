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

using BraneCloud.Evolution.EC.Vector;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.DE
{
    /// <summary> 
    /// DEBreeder provides a straightforward Differential Evolution (DE) breeder
    /// for the ECJ system.  The code relies (with permission from the original
    /// authors) on the DE algorithms posted at
    /// http://www.icsi.berkeley.edu/~storn/code.html .  For more information on
    /// Differential Evolution, please refer to the aforementioned webpage.
    /// <p/>The default breeding code in DEBreeder is a simple adaptive breeder communicated personally
    /// by Dr. Kenneth Price.  The algorithm might also be explored in the recent book
    /// "Differential Evolution: A Practical Approach to Global Optimization"
    /// by Kenneth Price, Rainer Storn, and Jouni Lampinen.
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.de.DEBreeder")]
    public class DEBreeder : Breeder
    {
        #region Constants

        public const double CR_UNSPECIFIED = -1;

        public const String P_F = "f";
        public const String P_Cr = "cr";

        #endregion // Constants
        #region Properties

        /// <summary>
        /// Scaling factor for mutation.
        /// </summary>
        public double F { get { return _f; } set { _f = value; } }
        private double _f;

        /// <summary>
        /// Probability of crossover per gene.
        /// </summary>
        public double Cr
        {
            get { return _cr; }
            set { _cr = value; }
        }
        private double _cr = CR_UNSPECIFIED;

        /// <summary>
        /// The previous population is stored in order to have parents compete directly with their children
        /// </summary>
        public Population PreviousPopulation { get; set; }

        /// <summary>
        /// The best individuals in each population (required by some DE breeders).  
        /// It's not required by DEBreeder's algorithm.
        /// </summary>
        public int[] BestSoFarIndex { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            if (!state.Parameters.ParameterExists(paramBase.Push(P_Cr), null))  // it wasn't specified -- hope we know what we're doing
                _cr = CR_UNSPECIFIED;
            else
            {
                _cr = state.Parameters.GetDouble(paramBase.Push(P_Cr), null, 0.0);
                if (_cr < 0.0 || _cr > 1.0)
                    state.Output.Fatal("Parameter not found, or its value is outside of [0.0,1.0].", paramBase.Push(P_Cr), null);
            }

            F = state.Parameters.GetDouble(paramBase.Push(P_F), null, 0.0);
            if (F < 0.0 || F > 1.0)
                state.Output.Fatal("Parameter not found, or its value is outside of [0.0,1.0].", paramBase.Push(P_F), null);
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// This function is called just before chldren are to be bred
        /// </summary>
        public virtual void PrepareDEBreeder(IEvolutionState state)
        {
            // update the bestSoFar for each population
            if (BestSoFarIndex == null || state.Population.Subpops.Length != BestSoFarIndex.Length)
                BestSoFarIndex = new int[state.Population.Subpops.Length];

            for (var subpop = 0; subpop < state.Population.Subpops.Length; subpop++)
            {
                var inds = state.Population.Subpops[subpop].Individuals;
                BestSoFarIndex[subpop] = 0;
                for (var j = 1; j < inds.Length; j++)
                    if (inds[j].Fitness.BetterThan(inds[BestSoFarIndex[subpop]].Fitness))
                        BestSoFarIndex[subpop] = j;
            }
        }

        public override Population BreedPopulation(IEvolutionState state)
        {
            // double check that we're using DEEvaluator
            if (!(state.Evaluator is IDEEvaluator))
                state.Output.WarnOnce("DEEvaluator not used, but DEBreeder used.  This is almost certainly wrong.");

            // prepare the breeder (some global statistics might need to be computed here)
            PrepareDEBreeder(state);

            // create the new population
            var newpop = (Population)state.Population.EmptyClone();

            // breed the children
            for (var subpop = 0; subpop < state.Population.Subpops.Length; subpop++)
            {
                if (state.Population.Subpops[subpop].Individuals.Length < 4)  // Magic number, sorry.  createIndividual() requires at least 4 individuals in the pop
                    state.Output.Fatal("Subpopulation " + subpop + " has fewer than four individuals, and so cannot be used with DEBreeder.");

                var inds = newpop.Subpops[subpop].Individuals;
                for (var i = 0; i < inds.Length; i++)
                {
                    newpop.Subpops[subpop].Individuals[i] = CreateIndividual(state, subpop, i, 0);  // unthreaded for now
                }
            }

            // store the current population for competition with the new children
            PreviousPopulation = state.Population;
            return newpop;
        }

        /// <summary>
        /// Tests the Individual to see if its values are in range.
        /// </summary>
        public bool Valid(DoubleVectorIndividual ind)
        {
            var species = (FloatVectorSpecies)(ind.Species);
            return (!(species.MutationIsBounded && !ind.IsInRange));
        }

        public virtual DoubleVectorIndividual CreateIndividual(IEvolutionState state, int subpop, int index, int thread)
        {
            var inds = state.Population.Subpops[subpop].Individuals;

            var v = (DoubleVectorIndividual)
                (state.Population.Subpops[subpop].Species.NewIndividual(state, thread));

            do
            {
                // select three indexes different from each other and from that of the current parent
                int r0, r1, r2;
                do
                {
                    r0 = state.Random[thread].NextInt(inds.Length);
                }
                while (r0 == index);
                do
                {
                    r1 = state.Random[thread].NextInt(inds.Length);
                }
                while (r1 == r0 || r1 == index);
                do
                {
                    r2 = state.Random[thread].NextInt(inds.Length);
                }
                while (r2 == r1 || r2 == r0 || r2 == index);

                var g0 = (DoubleVectorIndividual)(inds[r0]);
                var g1 = (DoubleVectorIndividual)(inds[r1]);
                var g2 = (DoubleVectorIndividual)(inds[r2]);

                for (var i = 0; i < v.genome.Length; i++)
                    v.genome[i] = g0.genome[i] + F * (g1.genome[i] - g2.genome[i]);
            }
            while (!Valid(v));

            return Crossover(state, (DoubleVectorIndividual)(inds[index]), v, thread);
        }

        /// <summary>
        /// Crosses over child with target, storing the result in child and returning it.  The default
        /// procedure copies each value from the target, with independent probability CROSSOVER, into
        /// the child.  The crossover guarantees that at least one child value, chosen at random, will
        /// not be overwritten.  Override this method to perform some other kind of crossover.
        /// </summary>
        public DoubleVectorIndividual Crossover(IEvolutionState state, DoubleVectorIndividual target, DoubleVectorIndividual child, int thread)
        {
            if (_cr == CR_UNSPECIFIED)
                state.Output.WarnOnce("Differential Evolution Parameter cr unspecified.  Assuming cr = 0.5");

            // first, hold one value in abeyance
            var index = state.Random[thread].NextInt(child.genome.Length);
            var val = child.genome[index];

            // do the crossover
            for (var i = 0; i < child.genome.Length; i++)
            {
                if (state.Random[thread].NextDouble() < _cr)
                    child.genome[i] = target.genome[i];
            }

            // reset the one value so it's not just a duplicate copy
            child.genome[index] = val;

            return child;
        }

        #endregion // Operations
    }
}