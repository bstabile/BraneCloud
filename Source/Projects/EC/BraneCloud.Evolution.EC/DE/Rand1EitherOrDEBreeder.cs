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
    /// Rand1EitherOrDEBreeder is a differential evolution breeding operator.
    /// The code is derived from a DE algorithm, known as DE/rand/1/either-or, 
    /// found on page 141 of
    /// "Differential Evolution: A Practical Approach to Global Optimization"
    /// by Kenneth Price, Rainer Storn, and Jouni Lampinen.
    /// 
    /// <p/>Rand1EitherOrDEBreeder requires that all individuals be DoubleVectorIndividuals.
    /// 
    /// <p/>In short, the algorithm is as follows.  For each individual in the population, we produce a child
    /// by selecting three (different) individuals, none the original individual, called r0, r1, and r2.
    /// We then create an individal c, defined either c = r0 + F * (r1 - r2), or as c = r0 + 0.5 * (F+1) * (r1 + r2 - 2 * r0),
    /// depending on a coin flip of probability "PF" (if 'true', the first equation is used, else the second).
    /// Unlike the other DEBreeders in this package, we do *not* cross over the child with the original individual.
    /// In fact, if the crossover probability is specified, Rand1EitherOrDEBreeder will issue a warning that it's
    /// not using it.
    /// 
    /// <p/>This class should be used in conjunction with 
    /// DEEvaluator, which allows the children to enter the population only if they're superior to their
    /// parents (the original individuals).  If so, they replace their parents.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>pf</tt><br/>
    /// <font size="-1">0.0 &lt;= double &lt;= 1.0 </font></td>
    /// <td valign="top">The "PF" probability of mutation type</td></tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.de.Rand1EitherOrDEBreeder")]
    public class Rand1EitherOrDEBreeder : DEBreeder
    {
        #region Constants

        public const string P_PF = "pf";

        #endregion // Constants
        #region Properties

        public double PF { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            PF = state.Parameters.GetDouble(paramBase.Push(P_PF), null, 0.0);
            if (PF < 0.0 || PF > 1.0)
                state.Output.Fatal("Parameter not found, or its value is outside of [0.0,1.0].", paramBase.Push(P_PF), null);

            if (state.Parameters.ParameterExists(paramBase.Push(P_Cr), null))
                state.Output.Warning("Crossover parameter specified, but Rand1EitherOrDEBreeder does not use crossover.", paramBase.Push(P_Cr));
        }

        #endregion // Setup
        #region Operations

        public override DoubleVectorIndividual CreateIndividual(IEvolutionState state, int subpop, int index, int thread)
        {
            var inds = state.Population.Subpops[subpop].Individuals;

            var v = (DoubleVectorIndividual)
                state.Population.Subpops[subpop].Species.NewIndividual(state, thread);

            var retry = -1;
            do
            {
                retry++;

                // select three indexes different from each other and from that of the current parent
                int r0, r1, r2;
                do
                {
                    r0 = state.Random[thread].NextInt(inds.Count);
                }
                while (r0 == index);
                do
                {
                    r1 = state.Random[thread].NextInt(inds.Count);
                }
                while (r1 == r0 || r1 == index);
                do
                {
                    r2 = state.Random[thread].NextInt(inds.Count);
                }
                while (r2 == r1 || r2 == r0 || r2 == index);

                var g0 = (DoubleVectorIndividual)inds[r0];
                var g1 = (DoubleVectorIndividual)inds[r1];
                var g2 = (DoubleVectorIndividual)inds[r2];

                for (int i = 0; i < v.genome.Length; i++)
                    if (state.Random[thread].NextBoolean(PF))
                        v.genome[i] = g0.genome[i] + F * (g1.genome[i] - g2.genome[i]);
                    else
                        v.genome[i] = g0.genome[i] + 0.5 * (F + 1) * (g1.genome[i] + g2.genome[i] - 2 * g0.genome[i]);
            }
            while (!Valid(v) && retry < Retries);
            if (retry >= Retries && !Valid(v))  // we reached our maximum
            {
                // completely reset and be done with it
                v.Reset(state, thread);
            }

            return v;       // no crossover is performed
        }

        #endregion // Operations
    }
}