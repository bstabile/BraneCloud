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
using System.Linq;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.Vector.Breed
{
    /// <summary>
    /// MultipleVectorCrossoverPipeline is a BreedingPipeline which implements a uniform
    /// (any point) crossover between multiple vectors. It is intended to be used with
    /// three or more vectors. It takes n parent individuals and returns n crossed over
    /// individuals. The number of parents and consequently children is specified by the
    /// number of sources parameter. 
    /// <p/>The standard vector crossover probability is used for this crossover type. 
    /// <br/> <i> Note</i> : It is necessary to set the crossover-type parameter to 'any' 
    /// in order to use this pipeline.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// number of parents
    /// 
    /// <p/><b>Number of Sources</b><br/>
    /// variable (generally 3 or more)
    /// 
    /// <p/><b>Default Base</b><br/>
    /// vector.multixover
    /// 
    /// This class is MUCH MUCH longer than it need be.  We could just do it by using 
    /// ECJ's generic split and join operations, but only rely on that in the default
    /// case, and instead use faster per-array operations.
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.vector.breed.MultipleVectorCrossoverPipeline")]
    public class MultipleVectorCrossoverPipeline : BreedingPipeline
    {
        #region Constants

        /// <summary>
        /// Default base.
        /// </summary>
        public const string P_CROSSOVER = "multixover";

        #endregion // Constants

        #region Properties

        public override IParameter DefaultBase => VectorDefaults.ParamBase.Push(P_CROSSOVER);

        /// <summary>
        /// Returns the number of parents.
        /// </summary>
        public override int NumSources => DYNAMIC_SOURCES;

        /// <summary>
        /// Returns the minimum number of children that are produced per crossover.
        /// </summary>
        public override int TypicalIndsProduced => MinChildProduction * Sources.Length; // minChild is always 1     

        /// <summary>
        /// Temporary holding place for parents.
        /// </summary>
        protected IList<Individual> Parents { get; set; } = new List<Individual>();

        #endregion // Properties

        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            IParameter def = DefaultBase;

            if (Sources.Length <= 2) // uh oh
                state.Output.Fatal("num-sources must be provided and > 2 for MultipleVectorCrossoverPipeline",
                    paramBase.Push(P_NUMSOURCES), def.Push(P_NUMSOURCES));

            Parents = new List<Individual>();
        }

        #endregion // Setup

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

            // how many individuals should we make?
            var n = TypicalIndsProduced;
            if (n < min) n = min;
            if (n > max) n = max;

            // should we bother?
            if (!state.Random[thread].NextBoolean(Likelihood))
            {
                // just load from source 0
                Sources[0].Produce(n, n, subpop, inds, state, thread, misc);
                return n;
            }

            Parents.Clear();
            // fill up parents: 
            for (var i = 0; i < Sources.Length; i++) // parents.length == sources.length
            {
                // produce one parent from each source 
                Sources[i].Produce(1, 1, subpop, Parents, state, thread, misc);
            }

            // We assume all of the species are the same species ... 
            var species = (VectorSpecies) ((VectorIndividual) Parents[0]).Species;

            // an array of the split points (width = 1)
            var points = new int[((VectorIndividual) Parents[0]).GenomeLength - 1];
            for (var i = 0; i < points.Length; i++)
            {
                points[i] = i + 1; // first split point/index = 1
            }

            // split all the parents into object arrays 
            var pieces = TensorFactory.Create<object>(Parents.Count, ((VectorIndividual) Parents[0]).GenomeLength);

            // splitting...
            for (int i = 0; i < Parents.Count; i++)
            {
                if (((VectorIndividual) Parents[i]).GenomeLength != ((VectorIndividual) Parents[0]).GenomeLength)
                    state.Output.Fatal("All vectors must be of the same length for crossover!");
                else
                    ((VectorIndividual) Parents[i]).Split(points, pieces[i]);
            }


            // crossing them over now
            for (var i = 0; i < pieces[0].Length; i++)
            {
                if (state.Random[thread].NextBoolean(species.CrossoverProbability))
                {
                    // shuffle
                    for (var j = pieces.Length - 1; j > 0; j--) // no need to shuffle first index at the end
                    {
                        // find parent to swap piece with
                        var parent2 = state.Random[thread].NextInt(j); // not inclusive; don't want to swap with self
                        // swap
                        var temp = pieces[j][i];
                        pieces[j][i] = pieces[parent2][i];
                        pieces[parent2][i] = temp;
                    }
                }
            }

            // join them and add them to the population starting at the start location
            for (int i = 0, q = start; i < Parents.Count; i++, q++)
            {
                ((VectorIndividual)Parents[i]).Join(pieces[i]);
                Parents[i].Evaluated = false;
                //if (q < inds.Count) // just in case
                //{
                //    inds[q] = Parents[i];
                //}
                // by Ermo. The comment code seems to be wrong. inds are empty, which means indes.size() returns 0.
                // I think it should be changed to following code
                // Sean -- right?
                inds.Add(Parents[i]);
            }
            return n;
        }


        #endregion // Operations
        #region Cloning

        public override object Clone()
        {
            var c = (MultipleVectorCrossoverPipeline)base.Clone();

            // deep-cloned stuff
            c.Parents = Parents.ToList();

            return c;
        }

        #endregion // Cloning
    }
}