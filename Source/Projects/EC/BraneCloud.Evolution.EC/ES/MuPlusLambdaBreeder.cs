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

namespace BraneCloud.Evolution.EC.ES
{
    /// <summary> 
    /// MuPlusLambdaBreeder is a subclass of MuCommaLambdaBreeder which, together with
    /// ESSelection, implements the (mu + lambda) breeding strategy and gathers
    /// the comparison data you can use to implement a 1/5-rule mutation mechanism.
    /// Note that MuPlusLambdaBreeder increases subpop sizes by their mu
    /// values in the second generation and keep them at that size thereafter.
    /// See MuCommaLambdaBreeder for information about how to set mu and lambda.
    /// </summary>    
    [Serializable]
    [ECConfiguration("ec.es.MuPlusLambdaBreeder")]
    public class MuPlusLambdaBreeder : MuCommaLambdaBreeder
    {
        #region Operations

        /// <summary>
        /// Sets all subpops in pop to the expected mu+lambda size.  Does not fill new slots with individuals. 
        /// </summary>
        public virtual Population SetToMuPlusLambda(Population pop, IEvolutionState state)
        {
            for (var x = 0; x < pop.Subpops.Length; x++)
            {
                var s = Mu[x] + Lambda[x];

                // check to see if the array's big enough
                if (pop.Subpops[x].Individuals.Length != s)
                // need to increase
                {
                    var newinds = new Individual[s];
                    Array.Copy(pop.Subpops[x].Individuals, 0, newinds, 0,
                        s < pop.Subpops[x].Individuals.Length ? s : pop.Subpops[x].Individuals.Length);

                    pop.Subpops[x].Individuals = newinds;
                }
            }
            return pop;
        }

        public override Population PostProcess(Population newpop, Population oldpop, IEvolutionState state)
        {
            // first we need to expand newpop to mu+lambda in size
            newpop = SetToMuPlusLambda(newpop, state);

            // now we need to dump the old population into the high end of the new population

            for (var x = 0; x < newpop.Subpops.Length; x++)
            {
                for (var y = 0; y < Mu[x]; y++)
                {
                    newpop.Subpops[x].Individuals[y + Lambda[x]] = (Individual)(oldpop.Subpops[x].Individuals[y].Clone());
                }
            }
            return newpop;
        }

        #endregion // Operations
    }
}