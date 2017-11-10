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
using System.Linq;
using BraneCloud.Evolution.EC.Vector;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.DE
{	
    /// <summary> 
    /// Best1BinDEBreeder implements the DE/best/1/bin Differential Evolution algorithm.
    /// The code relies (with permission from the original authors) on the DE algorithms posted at
    /// http://www.icsi.berkeley.edu/~storn/code.html .  For more information on
    /// Differential Evolution, please refer to the aforementioned webpage.
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.de.Best1BinDEBreeder")]
    public class Best1BinDEBreeder : DEBreeder
    {
        #region Constants

        /// <summary>
        /// Limits on uniform noise for F
        /// </summary>
        public double F_NOISE;

        public const string P_FNOISE = "f-noise";

        #endregion // Constants
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            F_NOISE = state.Parameters.GetDouble(paramBase.Push(P_FNOISE), null, 0.0);
            if (F_NOISE < 0.0)
                state.Output.Fatal("Parameter not found, or its value is below 0.0.", paramBase.Push(P_FNOISE), null);
        }

        #endregion // Setup
        #region Operations

        public override DoubleVectorIndividual CreateIndividual(IEvolutionState state, int subpop, int index, int thread)
        {
            var inds = state.Population.Subpops[subpop].Individuals.ToArray();

            var v = (DoubleVectorIndividual)
                state.Population.Subpops[subpop].Species.NewIndividual(state, thread);

            var retry = -1;
            do
            {
                retry++;

                // select three indexes different from each other and from that of the current parent
                int r0, r1, r2;
                // do
                {
                    r0 = BestSoFarIndex[subpop];
                }
                // while( r0 == index );
                do
                {
                    r1 = state.Random[thread].NextInt(inds.Length);
                } while (r1 == r0 || r1 == index);
                do
                {
                    r2 = state.Random[thread].NextInt(inds.Length);
                } while (r2 == r1 || r2 == r0 || r2 == index);

                var g0 = (DoubleVectorIndividual) (inds[r0]);
                var g1 = (DoubleVectorIndividual) (inds[r1]);
                var g2 = (DoubleVectorIndividual) (inds[r2]);

                for (var i = 0; i < v.genome.Length; i++)
                    v.genome[i] = g0.genome[i] +
                                  (F + state.Random[thread].NextDouble() * F_NOISE - (F_NOISE / 2.0)) *
                                  (g1.genome[i] - g2.genome[i]);
            }
            while (!Valid(v) && retry < Retries);

            if (retry >= Retries && !Valid(v))  // we reached our maximum
            {
                // completely reset and be done with it
                v.Reset(state, thread);
            }


            return Crossover(state, (DoubleVectorIndividual)inds[index], v, thread);
        }

        #endregion // Operations
    }
}