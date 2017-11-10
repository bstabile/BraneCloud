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
using System.Collections;
using System.Collections.Generic;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.Breed
{
    /**
     * UniquePipeline is a BreedingPipeline which tries very hard to guarantee that all
     * the individuals it produces are unique from members of the original subpopulation.
     *
     <p><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br>
     ...as many as the child produces

     <p><b>Number of Sources</b><br>
     1

     <p><b>Parameters</b><br>
     <table>
     <tr><td valign=top><i>base.</i><tt>generate-max</tt><br>
     <font size=-1>bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
     <td valign=top>(do we always generate the maximum number of possible individuals, or at least the minimum number?)</td></tr>
     <tr><td valign=top><i>base.</i><tt>duplicate-retries</tt><br>
     <font size=-1>int >= 0</font></td>
     <td valign=top>(number of times we try to find a duplicate individual before giving up and just filling the remainder with non-duplicate individuals)</td></tr>
     </table>
     <p><b>Default Base</b><br>
     breed.unique

     * @author Sean Luke
     * @version 1.0 
     */

    public class UniquePipeline : BreedingPipeline
    {
        public const string P_UNIQUE = "unique";
        public const string P_GEN_MAX = "generate-max";
        public const string P_RETRIES = "duplicate-retries";

        public const int NUM_SOURCES = 1;

        public HashSet<Individual> Set { get; set; } = new HashSet<Individual>();

        public override IParameter DefaultBase => BreedDefaults.ParamBase.Push(P_UNIQUE);

        public override int NumSources => NUM_SOURCES;

        public bool ResetEachGeneration { get; set; }
        public int NumDuplicateRetries { get; set; }

        private bool _generateMax;

        public override object Clone()
        {
            UniquePipeline c = (UniquePipeline) base.Clone();
            c.Set = Set.DeepClone(); // Individual.Clone() to populate new HashSet
            return c;
        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            IParameter def = DefaultBase;
            _generateMax = state.Parameters.GetBoolean(paramBase.Push(P_GEN_MAX), def.Push(P_GEN_MAX), false);

            if (!Likelihood.Equals(1.0))
                state.Output.Warning(
                    "UniquePipeline given a likelihood other than 1.0.  This is nonsensical and will be ignored.",
                    paramBase.Push(P_LIKELIHOOD),
                    def.Push(P_LIKELIHOOD));

            // How often do we retry if we find a duplicate?
            NumDuplicateRetries = state.Parameters.GetInt(
                paramBase.Push(P_RETRIES), def.Push(P_RETRIES), 0);
            if (NumDuplicateRetries < 0)
                state.Output.Fatal(
                    "The number of retries for duplicates must be an integer >= 0.\n",
                    paramBase.Push(P_RETRIES), def.Push(P_RETRIES));
        }



        public override void PrepareToProduce(
            IEvolutionState state,
            int subpopulation,
            int thread)
        {
            Set.Clear();
            IList<Individual> inds = state.Population.Subpops[subpopulation].Individuals;
            foreach (Individual i in inds)
                Set.Add(i);
        }

        int RemoveDuplicates(IList<Individual> inds, int start, int num)
        {
            for (int i = start; i < start + num; i++)
            {
                if (Set.Contains(inds[i])) // swap in from top
                {
                    inds[i] = inds[start + num - 1];
                    inds[start + num - 1] = null;
                    num--;
                    i--; // try again
                }
            }
            return num;
        }

        public override int Produce(
            int min,
            int max,
            int subpop,
            IList<Individual> inds,
            IEvolutionState state,
            int thread,
            IDictionary<string, object> misc)
        {
            int start = 0;

            int n = 0; // unique individuals we've built so far
            int remainder = (_generateMax ? max : min);
            for (int retry = 0; retry < NumDuplicateRetries + 1; retry++)
            {
                // grab individuals from our source and stick 'em right into inds.
                // we'll verify them from there
                int newmin = Math.Min(Math.Max(min - n, 1), max - n);
                int num = Sources[0].Produce(newmin, max - n, subpop, inds, state, thread, misc);

                int total = RemoveDuplicates(inds, start + n, num); // unique individuals out of the num
                n += total; // we'll keep those
            }

            if (n < remainder) // never succeeded to build unique individuals, just make some non-unique ones
                n += Sources[0].Produce(remainder - n, max - n, subpop, inds, state, thread, misc);

            return n;
        }
    }
}