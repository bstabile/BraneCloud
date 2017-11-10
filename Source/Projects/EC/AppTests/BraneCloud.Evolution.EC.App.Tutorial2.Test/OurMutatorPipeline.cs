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

using System.Collections.Generic;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.App.Tutorial2.Test
{
    /// <summary>
    /// OurMutatorPipeline is a BreedingPipeline which negates the sign of genes.
    /// The individuals must be IntegerVectorIndividuals.  Because we're lazy,
    /// we'll use the individual's species' mutation-probability parameter to tell
    /// us whether or not to mutate a given gene.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// (however many its source produces)
    /// 
    /// <p/><b>Number of Sources</b><br/>
    /// 1
    /// </summary>
    [ECConfiguration("ec.app.tutorial2.OurMutatorPipeline")]
    public class OurMutatorPipeline : BreedingPipeline
    {
        public const string P_OURMUTATION = "our-mutation";

        /// <summary>
        /// We have to specify a default base, even though we never use it 
        /// </summary>
        /// <value></value>
        public override IParameter DefaultBase
        {
            get { return VectorDefaults.ParamBase.Push(P_OURMUTATION); }
        }

        public const int NUM_SOURCES = 1;

        /// <summary>
        /// Return 1 -- we only use one source
        /// </summary>
        /// <value></value>
        public override int NumSources
        {
            get { return NUM_SOURCES; }
        }

        // We're supposed to create a most _max_ and at least _min_ individuals,
        // drawn from our source and mutated, and stick them into slots in inds[]
        // starting with the slot inds[start].  Let's do this by telling our 
        // source to stick those individuals into inds[] and then mutating them
        // right there.
        public override int Produce(
            int min,
            int max,
            int subpopulation,
            IList<Individual> inds,
            IEvolutionState state,
            int thread,
            IDictionary<string, object> misc)
        {
            int start = inds.Count;

            // grab individuals from our source and stick 'em right into inds.
            // we'll modify them from there
            var n = Sources[0].Produce(min, max, subpopulation, inds, state, thread, misc);


            // should we bother?
            if (!state.Random[thread].NextBoolean(Likelihood))
            {
                return n;
            }


            // Check to make sure that the individuals are IntegerVectorIndividuals and
            // grab their species.  For efficiency's sake, we assume that all the 
            // individuals in inds[] are the same type of individual and that they all
            // share the same common species -- this is a safe assumption because they're 
            // all breeding from the same subpopulation.

            if (!(inds[start] is IntegerVectorIndividual)) // uh oh, wrong kind of individual
                state.Output.Fatal("OurMutatorPipeline didn't get an IntegerVectorIndividual." +
                    "The offending individual is in subpopulation " + subpopulation + " and it's:" + inds[start]);
            var species = (IntegerVectorSpecies)(inds[start].Species);

            // mutate 'em!
            for (var q = start; q < n + start; q++)
            {
                var i = (IntegerVectorIndividual)inds[q];
                for (var x = 0; x < i.genome.Length; x++)
                    if (state.Random[thread].NextBoolean(species.MutationProbability[x]))
                        i.genome[x] = -i.genome[x];
                // it's a "new" individual, so it's no longer been evaluated
                i.Evaluated = false;
            }

            return n;
        }
    }
}