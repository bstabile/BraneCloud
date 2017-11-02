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
using System.Text;
using BraneCloud.Evolution.EC.Configuration;

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
        #region Fields

        /// <summary>
        /// Temporary holding place for parents.
        /// </summary>
        VectorIndividual[] _parents;

        #endregion // Fields
        #region Properties

        public override IParameter DefaultBase
        {
            get { return VectorDefaults.ParamBase.Push(P_CROSSOVER); }
        }

        /// <summary>
        /// Returns the number of parents.
        /// </summary>
        public override int NumSources
        {
            get { return DYNAMIC_SOURCES; }
        }

        /// <summary>
        /// Returns the minimum number of children that are produced per crossover.
        /// </summary>
        public override int TypicalIndsProduced
        {
            get
            {
                return MinChildProduction * Sources.Length; // minChild is always 1     
            }
        }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            _parents = new VectorIndividual[Sources.Length];
        }

        #endregion // Setup
        #region Operations

        public override int Produce(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            // how many individuals should we make?
            var n = TypicalIndsProduced;
            if (n < min) n = min;
            if (n > max) n = max;

            // should we bother?
            if (!state.Random[thread].NextBoolean(Likelihood))
                return Reproduce(n, start, subpop, inds, state, thread, true);  // DO produce children from source -- we've not done so already

            if (inds[0] is BitVectorIndividual)
                n = MultipleBitVectorCrossover(min, max, start, subpop, inds, state, thread); // redundant reassignment

            else if (inds[0] is ByteVectorIndividual)
                n = MultipleByteVectorCrossover(min, max, start, subpop, inds, state, thread);

            else if (inds[0] is DoubleVectorIndividual)
                n = MultipleDoubleVectorCrossover(min, max, start, subpop, inds, state, thread);

            else if (inds[0] is FloatVectorIndividual)
                n = MultipleFloatVectorCrossover(min, max, start, subpop, inds, state, thread);

            else if (inds[0] is IntegerVectorIndividual)
                n = MultipleIntegerVectorCrossover(min, max, start, subpop, inds, state, thread);

            else if (inds[0] is GeneVectorIndividual)
                n = MultipleGeneVectorCrossover(min, max, start, subpop, inds, state, thread);

            else if (inds[0] is LongVectorIndividual)
                n = MultipleLongVectorCrossover(min, max, start, subpop, inds, state, thread);

            else if (inds[0] is ShortVectorIndividual)
                n = MultipleShortVectorCrossover(min, max, start, subpop, inds, state, thread);

            else // default crossover -- shouldn't need this unless a new vector type is added
            {
                // check how many sources are provided
                if (Sources.Length <= 2)
                    // this method shouldn't be called for just two parents 
                    state.Output.Error("Only two parents specified!");

                // fill up parents: 
                for (var i = 0; i < _parents.Length; i++) // parents.length == sources.length
                {
                    // produce one parent from each source 
                    Sources[i].Produce(1, 1, i, subpop, _parents, state, thread);
                    if (!(Sources[i] is BreedingPipeline))  // it's a selection method probably
                        _parents[i] = (VectorIndividual)_parents[i].Clone();
                }

                //... some required intermediary steps ....

                // assuming all of the species are the same species ... 
                var species = (VectorSpecies)_parents[0].Species;

                // an array of the split points (width = 1)
                var points = new int[_parents[0].GenomeLength - 1];
                for (var i = 0; i < points.Length; i++)
                {
                    points[i] = i + 1;    // first split point/index = 1
                }

                // split all the parents into object arrays 
                var pieces = new Object[_parents.Length][];
                for (var x = 0; x < _parents.Length; x++) pieces[x] = new object[_parents[0].GenomeLength];
                // splitting...
                for (var i = 0; i < _parents.Length; i++)
                {
                    if (_parents[i].GenomeLength != _parents[0].GenomeLength)
                        state.Output.Fatal("All vectors must be of the same length for crossover!");
                    else
                        _parents[i].Split(points, pieces[i]);
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
                for (int i = 0, q = start; i < _parents.Length; i++, q++)
                {
                    _parents[i].Join(pieces[i]);
                    _parents[i].Evaluated = false;
                    if (q < inds.Length) // just in case
                    {
                        inds[q] = _parents[i];
                    }
                }
            }
            return n;
        }

        /// <summary>
        /// Crosses over the Bit Vector Individuals using a uniform crossover method. 
        /// There is no need to call this method separately; produce(...) calls it
        /// whenever necessary by default.
        /// </summary>
        public int MultipleBitVectorCrossover(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            if (!(inds[0] is BitVectorIndividual))
                state.Output.Fatal("Trying to produce bit vector individuals when you can't!");


            // check how many sources are provided
            if (Sources.Length <= 2)
                // this method shouldn't be called for just two parents 
                state.Output.Error("Only two parents specified!");


            // how many individuals should we make?
            var n = TypicalIndsProduced;
            if (n < min) n = min;
            if (n > max) n = max;

            // fill up parents: 
            for (var i = 0; i < _parents.Length; i++) // parents.length == sources.length
            {
                // produce one parent from each source 
                Sources[i].Produce(1, 1, i, subpop, _parents, state, thread);
                if (!(Sources[i] is BreedingPipeline))  // it's a selection method probably
                    _parents[i] = (BitVectorIndividual)_parents[i].Clone();
            }

            var species = (VectorSpecies)inds[0].Species; // doesn't really matter if 
            //this is dblvector or vector as long as we
            // can get the crossover probability

            // crossover
            for (var i = 0; i < _parents[0].GenomeLength; i++)
            {
                if (state.Random[thread].NextBoolean(species.CrossoverProbability))
                {
                    for (var j = _parents.Length - 1; j > 0; j--)
                    {
                        var swapIndex = state.Random[thread].NextInt(j); // not inclusive; don't want to swap with self                     
                        var temp = ((BitVectorIndividual)_parents[j]).genome[i]; // modifying genomes directly. it's okay since they're clones
                        ((BitVectorIndividual)_parents[j]).genome[i] =
                            ((BitVectorIndividual)_parents[swapIndex]).genome[i];
                        ((BitVectorIndividual)_parents[swapIndex]).genome[i] = temp;
                    }
                }
            }

            // add to population
            for (int i = 0, q = start; i < _parents.Length; i++, q++)
            {
                _parents[i].Evaluated = false;
                if (q < inds.Length) // just in case
                {
                    inds[q] = _parents[i];
                }
            }
            return n;
        }

        /// <summary>
        /// Crosses over the Byte Vector Individuals using a uniform crossover method. 
        /// There is no need to call this method separately; produce(...) calls it
        /// whenever necessary by default. 
        /// </summary>
        public int MultipleByteVectorCrossover(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            if (!(inds[0] is ByteVectorIndividual))
                state.Output.Fatal("Trying to produce byte vector individuals when you can't!");

            // check how many sources are provided
            if (Sources.Length <= 2)
                // this method shouldn't be called for just two parents 
                state.Output.Error("Only two parents specified!");

            // how many individuals should we make?
            var n = TypicalIndsProduced;
            if (n < min) n = min;
            if (n > max) n = max;

            // fill up parents: 
            for (var i = 0; i < _parents.Length; i++) // parents.length == sources.length
            {
                // produce one parent from each source 
                Sources[i].Produce(1, 1, i, subpop, _parents, state, thread);
                if (!(Sources[i] is BreedingPipeline))  // it's a selection method probably
                    _parents[i] = (ByteVectorIndividual)(_parents[i].Clone());
            }

            var species = (VectorSpecies)inds[0].Species; // doesn't really matter if 
            //this is dblvector or vector as long as we
            // can get the crossover probability

            // crossover
            for (var i = 0; i < _parents[0].GenomeLength; i++)
            {
                if (state.Random[thread].NextBoolean(species.CrossoverProbability))
                {
                    for (var j = _parents.Length - 1; j > 0; j--)
                    {
                        var swapIndex = state.Random[thread].NextInt(j); // not inclusive; don't want to swap with self                     
                        var temp = ((ByteVectorIndividual)_parents[j]).genome[i]; // modifying genomes directly. it's okay since they're clones
                        ((ByteVectorIndividual)_parents[j]).genome[i] = ((ByteVectorIndividual)_parents[swapIndex]).genome[i];
                        ((ByteVectorIndividual)_parents[swapIndex]).genome[i] = temp;
                    }
                }
            }

            // add to population
            for (int i = 0, q = start; i < _parents.Length; i++, q++)
            {
                _parents[i].Evaluated = false;
                if (q < inds.Length) // just in case
                {
                    inds[q] = _parents[i];
                }
            }
            return n;
        }

        /// <summary>
        /// Crosses over the Double Vector Individuals using a uniform crossover method. 
        /// There is no need to call this method separately; produce(...) calls it
        /// whenever necessary by default. 
        /// </summary>
        public int MultipleDoubleVectorCrossover(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            if (!(inds[0] is DoubleVectorIndividual))
                state.Output.Fatal("Trying to produce double vector individuals when you can't!");

            // check how many sources are provided
            if (Sources.Length <= 2)
                // this method shouldn't be called for just two parents 
                state.Output.Error("Only two parents specified!");

            // how many individuals should we make?
            var n = TypicalIndsProduced;
            if (n < min) n = min;
            if (n > max) n = max;

            // fill up parents: 
            for (var i = 0; i < _parents.Length; i++) // parents.length == sources.length
            {
                // produce one parent from each source 
                Sources[i].Produce(1, 1, i, subpop, _parents, state, thread);
                if (!(Sources[i] is BreedingPipeline))  // it's a selection method probably
                    _parents[i] = (DoubleVectorIndividual)(_parents[i].Clone());
            }

            var species = (VectorSpecies)inds[0].Species; // doesn't really matter if 
            //this is dblvector or vector as long as we
            // can get the crossover probability

            // crossover
            for (var i = 0; i < _parents[0].GenomeLength; i++)
            {
                if (state.Random[thread].NextBoolean(species.CrossoverProbability))
                {
                    for (var j = _parents.Length - 1; j > 0; j--)
                    {
                        var swapIndex = state.Random[thread].NextInt(j); // not inclusive; don't want to swap with self                     
                        var temp = ((DoubleVectorIndividual)_parents[j]).genome[i]; // modifying genomes directly. it's okay since they're clones
                        ((DoubleVectorIndividual)_parents[j]).genome[i] = ((DoubleVectorIndividual)_parents[swapIndex]).genome[i];
                        ((DoubleVectorIndividual)_parents[swapIndex]).genome[i] = temp;
                    }
                }
            }

            // add to population
            for (int i = 0, q = start; i < _parents.Length; i++, q++)
            {
                _parents[i].Evaluated = false;
                if (q < inds.Length) // just in case
                {
                    inds[q] = _parents[i];
                }
            }
            return n;
        }

        /// <summary>
        /// Crosses over the Float Vector Individuals using a uniform crossover method. 
        /// There is no need to call this method separately; produce(...) calls it
        /// whenever necessary by default.
        /// </summary>
        public int MultipleFloatVectorCrossover(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            if (!(inds[0] is FloatVectorIndividual))
                state.Output.Fatal("Trying to produce float vector individuals when you can't!");

            // check how many sources are provided
            if (Sources.Length <= 2)
                // this method shouldn't be called for just two parents 
                state.Output.Error("Only two parents specified!");

            // how many individuals should we make?
            var n = TypicalIndsProduced;
            if (n < min) n = min;
            if (n > max) n = max;

            // fill up parents: 
            for (var i = 0; i < _parents.Length; i++) // parents.length == sources.length
            {
                // produce one parent from each source 
                Sources[i].Produce(1, 1, i, subpop, _parents, state, thread);
                if (!(Sources[i] is BreedingPipeline))  // it's a selection method probably
                    _parents[i] = (FloatVectorIndividual)(_parents[i].Clone());
            }

            var species = (VectorSpecies)inds[0].Species; // doesn't really matter if 
            //this is dblvector or vector as long as we
            // can get the crossover probability

            // crossover
            for (var i = 0; i < _parents[0].GenomeLength; i++)
            {
                if (state.Random[thread].NextBoolean(species.CrossoverProbability))
                {
                    for (var j = _parents.Length - 1; j > 0; j--)
                    {
                        var swapIndex = state.Random[thread].NextInt(j); // not inclusive; don't want to swap with self                     
                        var temp = ((FloatVectorIndividual)_parents[j]).genome[i]; // modifying genomes directly. it's okay since they're clones
                        ((FloatVectorIndividual)_parents[j]).genome[i] = ((FloatVectorIndividual)_parents[swapIndex]).genome[i];
                        ((FloatVectorIndividual)_parents[swapIndex]).genome[i] = temp;
                    }
                }
            }

            // add to population
            for (int i = 0, q = start; i < _parents.Length; i++, q++)
            {
                _parents[i].Evaluated = false;
                if (q < inds.Length) // just in case
                {
                    inds[q] = _parents[i];
                }
            }
            return n;
        }

        /// <summary>
        /// Crosses over the Gene Vector Individuals using a uniform crossover method. 
        /// There is no need to call this method separately; produce(...) calls it
        /// whenever necessary by default.
        /// </summary>
        public int MultipleGeneVectorCrossover(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            if (!(inds[0] is GeneVectorIndividual))
                state.Output.Fatal("Trying to produce gene vector individuals when you can't!");

            // check how many sources are provided
            if (Sources.Length <= 2)
                // this method shouldn't be called for just two parents 
                state.Output.Error("Only two parents specified!");

            // how many individuals should we make?
            var n = TypicalIndsProduced;
            if (n < min) n = min;
            if (n > max) n = max;

            // fill up parents: 
            for (var i = 0; i < _parents.Length; i++) // parents.length == sources.length
            {
                // produce one parent from each source 
                Sources[i].Produce(1, 1, i, subpop, _parents, state, thread);
                if (!(Sources[i] is BreedingPipeline))  // it's a selection method probably
                    _parents[i] = (GeneVectorIndividual)_parents[i].Clone();
            }

            var species = (VectorSpecies)inds[0].Species; // doesn't really matter if 
            //this is dblvector or vector as long as we
            // can get the crossover probability

            // crossover
            for (var i = 0; i < _parents[0].GenomeLength; i++)
            {
                if (state.Random[thread].NextBoolean(species.CrossoverProbability))
                {
                    for (var j = _parents.Length - 1; j > 0; j--)
                    {
                        var swapIndex = state.Random[thread].NextInt(j); // not inclusive; don't want to swap with self                     
                        Gene temp = ((GeneVectorIndividual)_parents[j]).genome[i]; // modifying genomes directly. it's okay since they're clones
                        ((GeneVectorIndividual)_parents[j]).genome[i] = ((GeneVectorIndividual)_parents[swapIndex]).genome[i];
                        ((GeneVectorIndividual)_parents[swapIndex]).genome[i] = temp;
                    }
                }
            }

            // add to population
            for (int i = 0, q = start; i < _parents.Length; i++, q++)
            {
                _parents[i].Evaluated = false;
                if (q < inds.Length) // just in case
                {
                    inds[q] = _parents[i];
                }
            }
            return n;
        }

        /// <summary>
        /// Crosses over the Integer Vector Individuals using a uniform crossover method.  
        /// There is no need to call this method separately; produce(...) calls it
        /// whenever necessary by default.  
        /// </summary>
        public int MultipleIntegerVectorCrossover(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            if (!(inds[0] is IntegerVectorIndividual))
                state.Output.Fatal("Trying to produce integer vector individuals when you can't!");

            // check how many sources are provided
            if (Sources.Length <= 2)
                // this method shouldn't be called for just two parents 
                state.Output.Error("Only two parents specified!");

            // how many individuals should we make?
            var n = TypicalIndsProduced;
            if (n < min) n = min;
            if (n > max) n = max;

            // fill up parents: 
            for (var i = 0; i < _parents.Length; i++) // parents.length == sources.length
            {
                // produce one parent from each source      
                Sources[i].Produce(1, 1, i, subpop, _parents, state, thread);
                if (!(Sources[i] is BreedingPipeline))  // it's a selection method probably
                    _parents[i] = (IntegerVectorIndividual)_parents[i].Clone();
            }

            var species = (VectorSpecies)inds[0].Species; // doesn't really matter if 
            //this is dblvector or vector as long as we
            // can get the crossover probability

            // crossover
            for (var i = 0; i < _parents[0].GenomeLength; i++)
            {
                if (state.Random[thread].NextBoolean(species.CrossoverProbability))
                {
                    for (var j = _parents.Length - 1; j > 0; j--)
                    {
                        var swapIndex = state.Random[thread].NextInt(j); // not inclusive; don't want to swap with self                     
                        var temp = ((IntegerVectorIndividual)_parents[j]).genome[i]; // modifying genomes directly. it's okay since they're clones
                        ((IntegerVectorIndividual)_parents[j]).genome[i] = ((IntegerVectorIndividual)_parents[swapIndex]).genome[i];
                        ((IntegerVectorIndividual)_parents[swapIndex]).genome[i] = temp;
                    }
                }
            }

            // add to population
            for (int i = 0, q = start; i < _parents.Length; i++, q++)
            {
                _parents[i].Evaluated = false;
                if (q < inds.Length) // just in case
                {
                    inds[q] = _parents[i];
                }
            }
            return n;
        }

        /// <summary>
        /// Crosses over the Long Vector Individuals using a uniform crossover method.
        /// There is no need to call this method separately; produce(...) calls it
        /// whenever necessary by default. 
        /// </summary>
        public int MultipleLongVectorCrossover(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            if (!(inds[0] is LongVectorIndividual))
                state.Output.Fatal("Trying to produce long vector individuals when you can't!");

            // check how many sources are provided
            if (Sources.Length <= 2)
                // this method shouldn't be called for just two parents 
                state.Output.Error("Only two parents specified!");

            // how many individuals should we make?
            var n = TypicalIndsProduced;
            if (n < min) n = min;
            if (n > max) n = max;

            // fill up parents: 
            for (var i = 0; i < _parents.Length; i++) // parents.length == sources.length
            {
                // produce one parent from each source 
                Sources[i].Produce(1, 1, i, subpop, _parents, state, thread);
                if (!(Sources[i] is BreedingPipeline))  // it's a selection method probably
                    _parents[i] = (LongVectorIndividual)(_parents[i].Clone());
            }

            var species = (VectorSpecies)inds[0].Species; // doesn't really matter if 
            //this is dblvector or vector as long as we
            // can get the crossover probability

            // crossover
            for (var i = 0; i < _parents[0].GenomeLength; i++)
            {
                if (state.Random[thread].NextBoolean(species.CrossoverProbability))
                {
                    for (var j = _parents.Length - 1; j > 0; j--)
                    {
                        var swapIndex = state.Random[thread].NextInt(j); // not inclusive; don't want to swap with self                     
                        var temp = ((LongVectorIndividual)_parents[j]).genome[i]; // modifying genomes directly. it's okay since they're clones
                        ((LongVectorIndividual)_parents[j]).genome[i] = ((LongVectorIndividual)_parents[swapIndex]).genome[i];
                        ((LongVectorIndividual)_parents[swapIndex]).genome[i] = temp;
                    }
                }
            }

            // add to population
            for (int i = 0, q = start; i < _parents.Length; i++, q++)
            {
                _parents[i].Evaluated = false;
                if (q < inds.Length) // just in case
                {
                    inds[q] = _parents[i];
                }
            }
            return n;
        }

        /// <summary>
        /// Crosses over the Short Vector Individuals using a uniform crossover method.
        /// There is no need to call this method separately; produce(...) calls it
        /// whenever necessary by default.
        /// </summary>
        public int MultipleShortVectorCrossover(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            if (!(inds[0] is ShortVectorIndividual))
                state.Output.Fatal("Trying to produce short vector individuals when you can't!");

            // check how many sources are provided
            if (Sources.Length <= 2)
                // this method shouldn't be called for just two parents 
                state.Output.Error("Only two parents specified!");

            // how many individuals should we make?
            var n = TypicalIndsProduced;
            if (n < min) n = min;
            if (n > max) n = max;

            // fill up parents: 
            for (var i = 0; i < _parents.Length; i++) // parents.length == sources.length
            {
                // produce one parent from each source 
                Sources[i].Produce(1, 1, i, subpop, _parents, state, thread);
                if (!(Sources[i] is BreedingPipeline))  // it's a selection method probably
                    _parents[i] = (ShortVectorIndividual)_parents[i].Clone();
            }

            var species = (VectorSpecies)inds[0].Species; // doesn't really matter if 
            //this is dblvector or vector as long as we
            // can get the crossover probability

            // crossover
            for (var i = 0; i < _parents[0].GenomeLength; i++)
            {
                if (state.Random[thread].NextBoolean(species.CrossoverProbability))
                {
                    for (var j = _parents.Length - 1; j > 0; j--)
                    {
                        var swapIndex = state.Random[thread].NextInt(j); // not inclusive; don't want to swap with self                     
                        var temp = ((ShortVectorIndividual)_parents[j]).genome[i]; // modifying genomes directly. it's okay since they're clones
                        ((ShortVectorIndividual)_parents[j]).genome[i] = ((ShortVectorIndividual)_parents[swapIndex]).genome[i];
                        ((ShortVectorIndividual)_parents[swapIndex]).genome[i] = temp;
                    }
                }
            }

            // add to population
            for (int i = 0, q = start; i < _parents.Length; i++, q++)
            {
                _parents[i].Evaluated = false;
                if (q < inds.Length) // just in case
                {
                    inds[q] = _parents[i];
                }
            }
            return n;
        }

        #endregion // Operations
        #region Cloning

        public override object Clone()
        {
            var c = (MultipleVectorCrossoverPipeline)base.Clone();

            // deep-cloned stuff
            c._parents = (VectorIndividual[])_parents.Clone();

            return c;
        }

        #endregion // Cloning
    }
}