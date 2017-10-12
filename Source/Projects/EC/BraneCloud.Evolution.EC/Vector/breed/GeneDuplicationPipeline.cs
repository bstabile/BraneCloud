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
    /// GeneDuplicationPipeline is designed to duplicate a sequence of genes from the chromosome and append
    /// them to the end of the chromosome.  The sequence of genes copied are randomly determined.  That is to
    /// say a random begining index is selected and a random ending index is selected from the chromosome.  Then
    /// this area is then copied (begining inclusive, ending exclusive) and appended to the end of the chromosome.
    /// Since randomness is a factor several checks are performed to make sure the begining and ending indicies are
    /// valid.  For example, since the ending index is exclusive, the ending index cannot equal the begining index (a
    /// new ending index would be randomly seleceted in this case).  Likewise the begining index cannot be larger than the
    /// ending index (they would be swapped in this case).
    /// 
    /// <p/><b>Default Base</b><br/>
    /// ec.vector.breed.GeneDuplicationPipeline
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.vector.breed.GeneDuplicationPipeline")]
    public class GeneDuplicationPipeline : BreedingPipeline
    {
        #region Constants

        public const string P_DUPLICATION = "duplicate";
        public const int NUM_SOURCES = 1;

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return VectorDefaults.ParamBase.Push(P_DUPLICATION); }
        }

        public override int NumSources
        {
            get { return NUM_SOURCES; }
        }

        #endregion // Properties
        #region Operations

        public override int Produce(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {

            // grab individuals from our source and stick 'em right into inds.
            // we'll modify them from there
            var n = Sources[0].Produce(min, max, start, subpop, inds, state, thread);


            // should we bother?
            if (!state.Random[thread].NextBoolean(Likelihood))
                return Reproduce(n, start, subpop, inds, state, thread, false);  // DON'T produce children from source -- we already did


            // now let's mutate 'em
            for (var q = start; q < n + start; q++)
            {
                if (Sources[0] is SelectionMethod)
                    inds[q] = (Individual)inds[q].Clone();

                //duplicate from the genome between a random begin and end point,
                //and put that at the end of the new genome.
                var ind = (VectorIndividual)(inds[q]);

                var len = ind.GenomeLength;

                //zero length individual, just return
                if (len == 0)
                {
                    return n;
                }

                var end = 0;
                var begin = state.Random[thread].NextInt(len + 1);
                do
                {
                    end = state.Random[thread].NextInt(len + 1);
                }
                while (begin == end);  //because the end is exclusive, start cannot be
                //equal to end.


                if (end < begin)
                {
                    var temp = end;  //swap if necessary
                    end = begin;
                    begin = temp;
                }

                // copy the original into a new array.
                var original = new Object[2];
                ind.Split(new[] { 0, len }, original);

                // copy the splice into a new array
                var splice = new Object[3];
                ind.Split(new[] { begin, end }, splice);

                // clone the genes in splice[1] (which we'll concatenate back in) in case we're using GeneVectorIndividual
                ind.CloneGenes(splice[1]);

                // appends the pieces together with the splice at the end.
                ind.Join(new[] { original[1], splice[1] });
            }
            return n;  // number of individuals produced, 1 here.
        }

        #endregion // Operations
    }
}