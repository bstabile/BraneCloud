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

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Breed
{
    /**
     * CheckingPipeline is a BreedingPipeline which just passes through the
     * individuals it receives from its source 0, but only if those individuals
     * ALL pass a validation check (the method allValid(), which you must override).
     * It tries to find valid individuals some num-times times, and if it cannot, it
     * instead reproduces individuals from its source 1 and returns them instead.
     *
     * <p>In some cases you may wish instead to produce individuals which
     * are individually checked for validity, rather than together.  The easiest way
     * to do this is to add the CheckingPipeline as a child to a ForceBreedingPipeline
     * which has been set with num-inds=1.
     *
     <p><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br>
     ...as many as either child produces

     <p><b>Number of Sources</b><br>
     2

     <p><b>Parameters</b><br>
     <table>
     <tr><td valign=top><i>base.</i><tt>num-times</tt><br>
     <font size=-1>int >= 1</font></td>
     <td valign=top>(number of times we try to get children from source 0 before giving up and using source 1)</td></tr>

     </table>
     <p><b>Default Base</b><br>
     breed.check
     */

    public class CheckingPipeline : BreedingPipeline
    {
        public const string P_CHECK = "check";
        public const string P_NUMTIMES = "num-times";
        public const int NUM_SOURCES = 2;

        public override IParameter DefaultBase => BreedDefaults.ParamBase.Push(P_CHECK);


        public override int NumSources => NUM_SOURCES;

        int _numTimes;

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            IParameter def = DefaultBase;
            _numTimes = state.Parameters.GetInt(paramBase.Push(P_NUMTIMES), def.Push(P_NUMTIMES), 1);
            if (_numTimes < 1)
                state.Output.Fatal("CheckingPipeline must have a num-times value >= 1.",
                    paramBase.Push(P_NUMTIMES),
                    def.Push(P_NUMTIMES));
            if (Likelihood != 1.0)
                state.Output.Warning(
                    "CheckingPipeline given a likelihood other than 1.0.  This is nonsensical and will be ignored.",
                    paramBase.Push(P_LIKELIHOOD),
                    def.Push(P_LIKELIHOOD));
        }

        public bool AllValid(Individual[] inds, int numInds, int subpopulation, IEvolutionState state, int thread)
        {
            return true;
        }

        public override int Produce(
            int min,
            int max,
            int start,
            int subpopulation,
            Individual[] inds,
            IEvolutionState state,
            int thread)
        {
            Individual[] inds2 = new Individual[max];

            for (int i = 0; i < _numTimes; i++)
            {
                // grab individuals from our source and stick 'em into inds2 at position 0
                int n0 = Sources[0].Produce(min, max, 0, subpopulation, inds2, state, thread);

                // check for validity
                if (!AllValid(inds2, n0, subpopulation, state, thread))
                    continue; // failure, try again

                // success!  Copy to inds and possibly clone
                Array.Copy(inds2, 0, inds, start, n0);
                if (Sources[0] is SelectionMethod)
                    for (int q = start; q < n0 + start; q++)
                        inds[q] = (Individual) inds[q].Clone();
                return n0;
            }

            // big-time failure!  Grab from the other source
            int n1 = Sources[1].Produce(min, max, start, subpopulation, inds, state, thread);
            if (Sources[0] is SelectionMethod)
                for (int q = start; q < n1 + start; q++)
                    inds[q] = (Individual) inds[q].Clone();
            return n1;
        }
    }
}









