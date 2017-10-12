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

using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.App.Gui
{
/*
public class SimpleBarChartStatistics : BarChartStatistics 
    { //Hovden

    private int[] seriesID;
    
    public void Setup(EvolutionState state, Parameter paramBase) 
    {
        base.Setup(state, paramBase);
        int numSubpops = state.Parameters.GetInt(new Parameter("pop.subpops"),null);
        
        seriesID = new int[numSubpops];
        
        for (int i = 0; i < numSubpops; ++i) {
            seriesID[i] = i; //series id identifies each sub pop
            }
        }
    
    public void PostEvaluationStatistics(EvolutionState state) {
        base.PostEvaluationStatistics(state);
        
        for (var subpop = 0; subpop < state.Population.Subpops.Length; ++subpop) {
            var bestFit = state.Population.Subpops[subpop].Individuals[0].Fitness;
            for (var i = 1; i < state.Population.Subpops[subpop].Individuals.Length; ++i) {
                var fit = state.Population.Subpops[subpop].Individuals[i].Fitness;
                if (fit.BetterThan(bestFit))
                    bestFit = fit;
                
                //Best individual is found, make a bar graph
                makeBar(seriesID[subpop], ((DoubleVectorIndividual)state.Population.Subpops[subpop].Individuals[i]).genome);
                }
            }
        }
    }
 */
}