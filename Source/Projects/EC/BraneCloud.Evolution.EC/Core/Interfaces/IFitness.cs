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
using System.Collections.Generic;
using System.IO;

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC
{
    [ECConfiguration("ec.IFitness")]
    public interface IFitness : IPrototype, IComparable
    {
        /// <summary>
        /// Auxiliary variable, used by coevolutionary processes, to compute the
        /// number of trials used to compute this Fitness value.  By default trials=null and stays that way. 
        /// If you set this variable, all of the elements of the ArrayList must be immutable -- once they're
        /// set they never change internally.
        /// </summary>
        List<double> Trials { get; set; }

        /// <summary>
        /// Auxiliary variable, used by coevolutionary processes, to store the individuals
        /// involved in producing this given Fitness value.  By default context=null and stays that way.
        /// Note that individuals stored here may possibly not themselves have Fitness values to avoid
        /// circularity when cloning.
        /// </summary>
        Individual[] Context { get; set; }

        /// <summary>
        /// Merges the other fitness into this fitness.  May destroy the other Fitness in the process.
        /// This method is typically called by coevolution in combination with distributed evauation where
        /// the Individual may be sent to various different sites to have trials performed on it, and
        /// the results must be merged together to form a relevant fitness.  By default merging occurs as follows.
        /// First, the trials arrays are concatenated.  Then whoever has the best trial has his context retained:
        /// this Fitness is determined by calling contextIsBetterThan(other).  By default that method assumes
        /// that trials are Doubles, and that higher values are better.  You will wish to override that method 
        /// if trials are different.  In coevolution nothing
        /// else needs to be merged usually, though you may need to override this to handle other things specially.
        /// 
        /// <p/>This method only works properly if the other Fitness had its trials deleted before it was sent off
        /// for evaluation on a remote machine: thus all of the trials are new and can be concatenated in.  This
        /// is what sim.eval.Job presently does in its method copyIndividualsForward().
        /// </summary>
        void Merge(IEvolutionState state, IFitness other);

        void SetContext(Individual[] cont, int index);

        void SetContext(Individual[] cont);

        /// <summary>
        /// Treat the Individual[] you receive from this as read-only.
        /// </summary>
        /// <returns>An array of Individuals</returns>
        Individual[] GetContext();


        /// <summary>
        /// Sets the fitness to be the same value as the best of the provided fitnesses.  This method calls
        /// SetToMeanOf(...), so if that method is unimplemented, this method will also fail. 
        /// </summary>
        void SetToBestOf(IEvolutionState state, IFitness[] fitnesses);

        /// <summary>
        /// Sets the fitness to be the same value as the mean of the provided fitnesses.  The default
        /// version of this method exits with an "unimplemented" error; you should override this.
        /// </summary>
        void SetToMeanOf(IEvolutionState state, IFitness[] fitnesses);

        /// <summary>
        /// Sets the fitness to be the median of the provided fitnesses.  This method calls
        /// SetToMeanOf(...), so if that method is unimplemented, this method will also fail.
        /// </summary>
        void SetToMedianOf(IEvolutionState state, IFitness[] fitnesses);

        /// <summary>
        /// Should return true if this is a good enough fitness to end the run.
        /// </summary>
        bool IsIdeal { get; }

        /// <summary>
        /// Should return an absolute fitness value ranging from negative
        /// infinity to infinity, NOT inclusive (thus infinity, negative
        /// infinity, and NaN are NOT valid fitness values).  This should
        /// be interpreted as: negative infinity is worse than the WORST
        /// possible fitness, and positive infinity is better than the IDEAL
        /// fitness.
        /// 
        /// <p/>You are free to restrict this range any way you like: for example,
        /// your fitness values might fall in the range [-5.32, 2.3]
        /// 
        /// <p/>Selection methods relying on fitness proportionate information will
        /// <b>assume the fitness is non-negative</b> and should throw an error
        /// if it is not.  Thus if you plan on using FitProportionateSelection, 
        /// BestSelection, or
        /// GreedyOverselection, for example, your fitnesses should assume that 0
        /// is the worst fitness and positive fitness are better.  If you're using
        /// other selection methods (Tournament selection, various ES selection
        /// procedures, etc.) your fitness values can be anything.
        /// 
        /// <p/>Similarly, if you're writing a selection method and it needs positive
        /// fitnesses, you should check for negative values and issue an error; and
        /// if your selection method doesn't need an <i>absolute</i> fitness
        /// value, it should use the equivalentTo() and betterThan() methods instead.
        /// 
        /// <p/> If your fitness scheme does not use a metric quantifiable to
        /// a single positive value (for example, MultiObjectiveFitness), you should 
        /// perform some reasonable translation.
        /// </summary>
        double Value { get; }

        /// <summary>
        /// Should return true if this fitness is in the same equivalence class
        /// as _fitness, that is, neither is clearly better or worse than the
        /// other.  You may assume that _fitness is of the same class as yourself.
        /// For any two fitnesses fit1 and fit2 of the same class,
        /// it must be the case that fit1.equivalentTo(fit2) == fit2.equivalentTo(fit1),
        /// and that only one of fit1.BetterThan(fit2), fit1.equivalentTo(fit2),
        /// and fit2.BetterThan(fit1) can be true.
        /// </summary>
        /// <remarks>ECJ legacy comparison method</remarks>
        bool EquivalentTo(IFitness other);

        /// <summary>
        /// Should return true if this fitness is clearly better than _fitness;
        /// You may assume that _fitness is of the same class as yourself. 
        /// For any two fitnesses fit1 and fit2 of the same class,
        /// it must be the case that fit1.equivalentTo(fit2) == fit2.equivalentTo(fit1),
        /// and that only one of fit1.BetterThan(fit2), fit1.equivalentTo(fit2),
        /// and fit2.BetterThan(fit1) can be true.
        /// </summary>
        /// <remarks>ECJ legacy comparison method</remarks>
        bool BetterThan(IFitness other);

        /// <summary>
        /// Given another Fitness, 
        /// returns true if the trial which produced my current context is "better" in fitness than
        /// the trial which produced his current context, and thus should be retained in lieu of his.
        /// This method by default assumes that trials are Doubles, and that higher Doubles are better.
        /// If you are using distributed evaluation and coevolution and your tirals are otherwise, you
        /// need to override this method.
        /// </summary>
        bool ContextIsBetterThan(IFitness other);

        /// <summary>
        /// This method currently defers to the original ECJ methods "BetterThan" and "EquivalentTo"
        /// for comparison. Those are both abstract here, and must be overridden to meet specific needs.
        /// This simply makes the ultimate implementation conform to the "expected" .NET comparison style.
        /// </summary>
        /// <remarks>
        /// If the legacy "methods of comparison" go away at some point,
        /// this MUST become abstract to allow for concrete implementation.
        /// </remarks>
        int CompareTo(IFitness other);

        /// <summary>
        /// Should print the fitness out fashion pleasing for humans to read, 
        /// using state.Output.PrintLn(...,log).  The default version
        /// of this method calls FitnessToStringForHumans(), adds context (collaborators) if any,
        /// and PrintLns the resultant string.
        /// </summary>
        void PrintFitnessForHumans(IEvolutionState state, int logNum);

        /// <summary>
        /// Should print the fitness out in a computer-readable fashion.
        /// </summary>
        void PrintFitness(IEvolutionState state, int logNum);

        /// <summary>
        /// Should print the fitness out in a computer-readable fashion, 
        /// using writer.PrintLn(...).  You might use
        /// ec.util.Code to encode fitness values.  The default version
        /// of this method calls FitnessToString() and PrintLn's the
        /// resultant string.
        /// </summary>
        void PrintFitness(IEvolutionState state, StreamWriter writer);

        /// <summary>
        /// Reads in the fitness from a form outputted by FitnessToString() and thus
        /// PrintFitnessForHumans(...).  
        /// The default version exits the program with an "unimplemented" error; you should override this, and be
        /// certain to also write the 'trials' variable as well.
        /// </summary>
        void ReadFitness(IEvolutionState state, StreamReader reader);

        /// <summary>
        /// Reads the binary form of an individual from a DataInput.  This is not for serialization:
        /// the object should only read in the data written out via printIndividual(state,dataInput).  
        /// The default version exits the program with an "unimplemented" error; 
        /// you should override this, and be certain to also write the 'trials' variable as well.
        /// </summary>
        void ReadFitness(IEvolutionState state, BinaryReader reader);

        /// <summary>
        /// Writes the binary form of an individual out to a DataOutput.  This is not for serialization:
        /// the object should only write out the data relevant to the object sufficient to rebuild it from a DataInput.
        /// The default version exits the program with an "unimplemented" error; you should override this, and be
        /// certain to also write the 'trials' variable as well.        
        /// </summary>
        void WriteFitness(IEvolutionState state, BinaryWriter writer);

        // BRS : TODO : these could actually disappear in favor of "ToString()" if clients are changed

        /// <summary>
        /// Print to a string the fitness in a fashion readable by humans, and not intended
        /// to be parsed in again.  The default form
        /// simply calls ToString(), but you'll probably want to override this to something else. 
        /// </summary>
        string FitnessToStringForHumans();

        /// <summary>
        /// Print to a string the fitness in a fashion intended
        /// to be parsed in again via ReadFitness(...).
        /// The fitness and evaluated flag should not be included.  The default form
        /// simply calls ToString(), which is almost certainly wrong, 
        /// and you'll probably want to override this to something else.  When overriding, you
        /// may wish to check to see if the 'trials' variable is non-null, and issue an error if so.
        /// </summary>
        string FitnessToString();

        /// <summary>
        ///  Writes trials out to DataOutput
        /// </summary>
        void WriteTrials(IEvolutionState state, BinaryWriter writer);

        /// <summary>
        /// Reads trials in from a BinaryReader.
        /// </summary>
        void ReadTrials(IEvolutionState state, BinaryReader reader);
    }
}