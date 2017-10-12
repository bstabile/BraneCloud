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

namespace BraneCloud.Evolution.EC.App.Tutorial3
{
    [ECConfiguration("ec.app.tutorial3.OurSelection")]
    public class OurSelection : SelectionMethod
    {
        // We have to specify a default base
        public const string P_OURSELECTION = "our-selection";

        public override IParameter DefaultBase
        {
            get { return new Parameter(P_OURSELECTION); }
        }

        public const string P_MIDDLEPROBABILITY = "middle-probability";  // our parameter name

        public double middleProbability;

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);   // always call super.Setup(...) first if it exists!

            var def = DefaultBase;

            // gets a double between min (0.0) and max (1.0), from the parameter
            // database, returning a value of min-1 (-1.0) if the parameter doesn't exist or was 
            // outside this range.
            middleProbability = state.Parameters.GetDoubleWithMax(paramBase.Push(P_MIDDLEPROBABILITY),
                def.Push(P_MIDDLEPROBABILITY), 0.0, 1.0);
            if (middleProbability < 0.0)
                state.Output.Fatal("Middle-Probability must be between 0.0 and 1.0",
                    paramBase.Push(P_MIDDLEPROBABILITY), def.Push(P_MIDDLEPROBABILITY));
        }

        public override int Produce(int subpopulation, IEvolutionState state, int thread)
        {
            //toss a coin
            if (state.Random[thread].NextBoolean(middleProbability))
            {
                //pick three individuals, return the middle one
                Individual[] inds = state.Population.Subpops[subpopulation].Individuals;
                int one = state.Random[thread].NextInt(inds.Length);
                int two = state.Random[thread].NextInt(inds.Length);
                int three = state.Random[thread].NextInt(inds.Length);
                // generally the betterThan(...) method imposes an ordering,
                // so you shouldn't see any cycles here except in very unusual domains...
                if (inds[two].Fitness.BetterThan(inds[one].Fitness))
                {
                    if (inds[three].Fitness.BetterThan(inds[two].Fitness)) //  1 < 2 < 3
                        return two;
                    else if (inds[three].Fitness.BetterThan(inds[one].Fitness)) //  1 < 3 < 2
                        return three;
                    else //  3 < 1 < 2
                        return one;
                }
                else if (inds[three].Fitness.BetterThan(inds[one].Fitness)) //  2 < 1 < 3
                    return one;
                else if (inds[three].Fitness.BetterThan(inds[two].Fitness)) //  2 < 3 < 1
                    return three;
                else //  3 < 2 < 1
                    return two;
            }
            else        //select a random individual's index
            {
                return state.Random[thread].NextInt(
                    state.Population.Subpops[subpopulation].Individuals.Length);
            }
        }
    }  // close the class
}