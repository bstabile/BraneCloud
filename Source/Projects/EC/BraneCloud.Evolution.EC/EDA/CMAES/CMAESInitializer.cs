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
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.EDA.CMAES
{
    /**
     * CMAESInitializer is a SimpleInitializer which ensures that the subpopulations are all set to the provided
     * or computed lambda values.
     *
     * @author Sam McKay and Sean Luke
     * @version 1.0 
     */

    [ECConfiguration("ec.eda.cmaes.CMAESInitializer")]
    public class CMAESInitializer : SimpleInitializer
    {
        private const long SerialVersionUID = 1;

        public override Population SetupPopulation(IEvolutionState state, int thread)
        {
            Population p = base.SetupPopulation(state, thread);

            // reset to lambda in size!
            for (int i = 0; i < p.Subpops.Count; i++)
            {
                Individual[] oldInds = p.Subpops[i].Individuals.ToArray();
                if (p.Subpops[i].Species is CMAESSpecies)
                {
                    int lambda = (int) (((CMAESSpecies) p.Subpops[i].Species).lambda);
                    if (lambda < oldInds.Length) // need to reduce
                    {
                        Individual[] newInds = new Individual[lambda];
                        Array.Copy(oldInds, 0, newInds, 0, lambda);
                        oldInds = newInds;
                    }
                    else if (lambda > oldInds.Length) // need to increase
                    {
                        Individual[] newInds = new Individual[lambda];
                        Array.Copy(oldInds, 0, newInds, 0, oldInds.Length);
                        for (int j = oldInds.Length; j < lambda; j++)
                            newInds[j] = p.Subpops[i].Species.NewIndividual(state, thread);
                        oldInds = newInds;
                    }
                }
                else
                    state.Output.Fatal("Species of subpopulation " + i + " is not a CMAESSpecies.  It's a " + p.Subpops[i].Species);

                p.Subpops[i].Individuals = oldInds.ToList();
                //p.Subpops[i].Individuals = new List<Individual>();
                //for (int j = 0; j < oldInds.Length; j++)
                //    p.Subpops[i].Individuals.Add(oldInds[j]); // yuck, but 1.5 doesn't have Arrays.asList
            }

            return p;
        }
    }
}