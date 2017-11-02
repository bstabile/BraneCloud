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
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.GP.Koza;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.Problems.Tutorial4
{
    [ECConfiguration("ec.problems.tutorial4.MultiValuedRegression")]
    public class MultiValuedRegression : GPProblem, ISimpleProblem
    {
        private const long SerialVersionUID = 1;

        public double currentX;
        public double currentY;

        public override object Clone()
        {
            var newobj = (MultiValuedRegression)(base.Clone());
            newobj.Input = (DoubleData)Input.Clone();
            return newobj;
        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // very important, remember this
            base.Setup(state, paramBase);

            // verify our input is the right class (or subclasses from it)
            if (!(Input is DoubleData))
            state.Output.Fatal("GPData class must subclass from " + typeof(DoubleData).Name,
            paramBase.Push(P_DATA), null);
        }

    public void Evaluate(IEvolutionState state,
            Individual ind,
            int subpop,
            int threadnum)
        {
            if (!ind.Evaluated)  // don't bother reevaluating
            {
                var hits = 0;
                var sum = 0.0;
                for (var y = 0; y < 10; y++)
                {
                    currentX = state.Random[threadnum].NextDouble();
                    currentY = state.Random[threadnum].NextDouble();
                    var expectedResult = currentX * currentX * currentY + currentX * currentY + currentY;
                    ((GPIndividual)ind).Trees[0].Child.Eval(state, threadnum, Input, Stack, ((GPIndividual)ind), this);

                    var result = Math.Abs(expectedResult - ((DoubleData)Input).x);
                    if (result <= 0.01) hits++;
                    sum += result;
                }

                // the fitness better be KozaFitness!
                var f = (KozaFitness)ind.Fitness;
                f.SetStandardizedFitness(state, (float)sum);
                f.Hits = hits;
                ind.Evaluated = true;
            }
        }
    }
}