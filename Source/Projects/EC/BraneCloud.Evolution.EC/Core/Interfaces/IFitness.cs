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
using System.IO;

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC
{
    [ECConfiguration("ec.IFitness")]
    public interface IFitness : IPrototype, IComparable
    {
        List<double> Trials { get; set; }
        void Merge(IEvolutionState state, IFitness other);

        /// <summary>
        /// Auxiliary variable, used by coevolutionary processes, to store the individuals
        /// involved in producing this given Fitness value.  By default context=null and stays that way.
        /// Note that individuals stored here may possibly not themselves have Fitness values to avoid
        /// circularity when cloning.
        /// </summary>
        Individual[] Context { get; set; }
        void SetContext(Individual[] cont, int index);
        void SetContext(Individual[] cont);
        Individual[] GetContext();

        bool IsIdeal { get; }
        float Value { get; }

        // the following are the ECJ legacy comparison methods
        bool EquivalentTo(IFitness other);
        bool BetterThan(IFitness other);

        void PrintFitnessForHumans(IEvolutionState state, int logNum);

        void PrintFitness(IEvolutionState state, int logNum);

        void PrintFitness(IEvolutionState state, StreamWriter writer);

        void ReadFitness(IEvolutionState state, StreamReader reader);
        void ReadFitness(IEvolutionState state, BinaryReader reader);

        void WriteFitness(IEvolutionState state, BinaryWriter writer);

        // BRS : TODO : these could actually disappear in favor of "ToString()" if clients are changed
        string FitnessToStringForHumans();
        string FitnessToString();
    }
}