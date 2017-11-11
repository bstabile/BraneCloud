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
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.EDA.DOvS
{
    /**
     * DOVSFitness is a subclass of Fitness which implements contains important
     * statistics about simulation results of the individual. These statistics will
     * be used to determine the total simulation number that are necessary for a
     * individual. And we hope after such number of simulations are done, we have
     * high confidence of the fitness value of the individual.
     * 
     * 
     * <p>
     * <b>Default Base</b><br>
     * dovs.fitness
     * 
     * @author Ermo Wei and David Freelan
     */

    [ECConfiguration("ec.eda.dovs.DOVSFitness")]
    public class DOVSFitness : SimpleFitness
    {
        /** Sum of the all the squared fitness value with all the evaluation. */
        public double SumSquared { get; set; }

    /** Sum of the all the fitness value with all the evaluation. */
        public double Sum { get; set; }

        /** Mean fitness value of the current individual. */
        public double Mean { get; set; }

        /** Number of evaluation have been performed on this individual. */
        public int NumOfObservations { get; set; }

        /** Variance of the fitness value of the current individual. */
        public double Variance { get; set; }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase); // unnecessary but what the heck

            SumSquared = 0;
            Sum = 0;
            Mean = 0;
            NumOfObservations = 0;
            Variance = 0;
        }

        /** Reset the fitness to initial status. */
        public void Reset()
        {
            SumSquared = 0;
            Sum = 0;
            Mean = 0;
            NumOfObservations = 0;
            Variance = 0;
        }

        ///** Return the number of simulation have done with current individual. */
        //public int numOfObservations()
        //    {
        //    return numOfObservations;
        //    }

        /**
         * Record the result of the new simulation. This will update some of the
         * statistics of the current fitness value.
         */
        public double RecordObservation(EvolutionState state, double result)
        {

            Sum += result;
            SumSquared += result * result;
            NumOfObservations++;
            Mean = Sum / NumOfObservations;
            if (NumOfObservations == 1)
            {
                Variance = 0;
            }
            else
            {
                Variance = (SumSquared - NumOfObservations * Mean * Mean) / (NumOfObservations - 1);
            }

            SetFitness(state, Mean, false);

            return Mean;
        }
    }
}