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

using BraneCloud.Evolution.EC.GP.GE;
using BraneCloud.Evolution.EC.Vector;
using BraneCloud.Evolution.EC.Vector.Breed;

namespace BraneCloud.Evolution.EC.GP
{
    /**
       GECrossoverPipeline is just like ListCrossoverPipeline, except that it will additionally
       check to verify that the first crossover point is within the range of consumed genes
       in each parent.  This is not uncommon in the GE literature.
       
       <p>For simplicity, GECrossoverPipeline shares the same default base as ListCrossoverPipeline,
       since it adds no new parameters.
       
       <p><b>Number of Sources</b><br>
       2

       <p><b>Default Base</b><br>
       vector.list-xover
    **/

    public class GECrossoverPipeline : ListCrossoverPipeline
    {
        public object ComputeValidationData(EvolutionState state, VectorIndividual[] parents, int thread)
        {
            if (!(parents[0] is GEIndividual) ||
                !(parents[1] is GEIndividual))
                state.Output.Fatal("Non GEIndividuals used with GECrossoverPipeline.", null, null);

            return new[]
            {
                ((GESpecies) parents[0].Species).Consumed(state, (GEIndividual) parents[0], thread),
                ((GESpecies) parents[1].Species).Consumed(state, (GEIndividual) parents[1], thread)
            };
        }

        public bool IsValidated(int[][] split, object validationData)
        {
            int[] consumed = (int[]) validationData;

            return split[0][0] < consumed[0] && split[1][0] < consumed[1];
        }
    }
}
    
    
    
    
    
    
