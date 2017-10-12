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
using BraneCloud.Evolution.EC.Logging;

namespace BraneCloud.Evolution.EC
{
    // BRS : TODO : Decide if the ECJ comparison methods can be retired in favor of IComparable.
    // Other notable changes to ECJ 20:
    //  1. Changed "isIdealFitness" to "IsIdeal".
    //  2. Method called "Fitness()" changed to property called "Value".

    /// <summary> 
    /// Fitness is a prototype which describes the fitness of an individual.
    /// Every individual contains exactly one Fitness object.
    /// Fitness objects are compared to each other with the equivalentTo()
    /// and betterThan(), etc. methods. 
    /// 
    /// <p/>Rules: 
    /// <table>
    /// <tr><td><b>comparison</b></td><td><b>method</b></td></tr>
    /// <tr/><td>a &gt; b</td><td>a.BetterThan(b)</td>
    /// <tr/><td>a &gt;= b</td><td>a.BetterThan(b) || a.equivalentTo(b)</td>
    /// <tr/><td>a = b</td><td>a.equivalentTo(b)</td>
    /// </table>
    /// This applies even to multiobjective pareto-style dominance, eg:
    /// <ul>
    /// <li/> a dominates b :: a &gt; b
    /// <li/> a and b do not dominate each other :: a = b
    /// <li/> b dominates a :: a &lt; b
    /// </ul>
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><tt>fit</tt></td>
    /// <td>default fitness base</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.Fitness")]
    public abstract class Fitness : IFitness
    {
        #region Constants

        /// <summary>
        /// Base parameter for defaults. 
        /// </summary>
        public const string P_FITNESS = "fitness";

        /// <summary>
        /// Basic preamble for printing Fitness values out. 
        /// </summary>
        public const string FITNESS_PREAMBLE = "Fitness: ";

        #endregion // Constants
        #region Properties

        public abstract IParameter DefaultBase { get; }

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
        public abstract float Value { get; /* protected set; */ } // The actual value should be handled in each derived type!

        /// <summary>
        /// Should return true if this is a good enough fitness to end the run 
        /// </summary>
        public abstract bool IsIdeal { get; }

        /// <summary>
        /// Auxiliary variable, used by coevolutionary processes, to compute the
        /// number of trials used to compute this Fitness value.  By default Trials = null and stays that way.
        /// If you set this variable, all of the elements of the List must be immutable -- once they're
        /// set they never change internally.
        /// </summary>
        public List<double> Trials { get { return _trials; } set { _trials = value; } }

        private List<double> _trials;

        /// <summary>
        /// Auxiliary variable, used by coevolutionary processes, to store the individuals
        /// involved in producing this given Fitness value.  By default context=null and stays that way.
        /// Note that individuals stored here may possibly not themselves have Fitness values to avoid
        /// circularity when cloning.
        /// </summary>
        public Individual[] Context { get { return _context; } set { _context = value; } }
        private Individual[] _context;

        #endregion // Properties
        #region Setup

        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            // by default does nothing
        }

        #endregion // Setup
        #region Operations

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
        public void Merge(IEvolutionState state, IFitness other)
        {
            // first let's merge trials.  We assume they're Doubles

            if (other.Trials == null) return; // I win
            if (Trials == null) // he wins
            {
                Trials = other.Trials; // just steal him
                Context = other.GetContext(); // grab his context
            }
            else // gotta concatenate
            {
                // first question: who has the best context?
                if (!ContextIsBetterThan(other)) // other is beter
                    Context = other.GetContext();

                // now concatenate the trials
                Trials.AddRange(other.Trials);
            }
        }

        protected double BestTrial(List<double> l)
        {
            if (l == null || l.Count == 0) return Double.NegativeInfinity;
            var best = (Double)l[0];
            var len = l.Count;
            for (var i = 1; i < len; i++)
            {
                var next = (Double)l[i];
                if (next > best) best = next;
            }
            return best;
        }

        #region Context

        public void SetContext(Individual[] cont, int index)
        {
            var ind = cont[index];
            cont[index] = null;
            SetContext(cont);
            cont[index] = ind;
        }
        public void SetContext(Individual[] cont)
        {
            if (cont == null)
                Context = null;
            else // make sure it's deep-cloned and stripped of context itself
            {
                Context = new Individual[cont.Length];
                for (var i = 0; i < cont.Length; i++)
                {
                    if (cont[i] == null)
                    { Context[i] = null; }
                    else
                    {
                        // we first temporarily remove context so we don't have any circularity in cloning 
                        var c = cont[i].Fitness.Context;
                        cont[i].Fitness.Context = null;

                        // now clone the individual in place
                        Context[i] = (Individual)(cont[i].Clone());

                        // now put the context back
                        cont[i].Fitness.Context = c;
                    }
                }
            }
        }

        /// <summary>
        /// Treat the Individual[] you receive from this as read-only.
        /// </summary>
        /// <returns>An array of Individuals</returns>
        public Individual[] GetContext()
        {
            return Context;
        }

        /// <summary>
        /// Given another Fitness, 
        /// returns true if the trial which produced my current context is "better" in fitness than
        /// the trial which produced his current context, and thus should be retained in lieu of his.
        /// This method by default assumes that trials are Doubles, and that higher Doubles are better.
        /// If you are using distributed evaluation and coevolution and your tirals are otherwise, you
        /// need to override this method.
        /// </summary>
        public bool ContextIsBetterThan(IFitness other)
        {
            if (other.Trials == null) return true; // I win
            if (Trials == null) return false; // he wins
            return BestTrial(Trials) < BestTrial(other.Trials);
        }

        #endregion

        #endregion // Operations
        #region Comparison

        /// <summary>
        /// Should return true if this fitness is in the same equivalence class
        /// as _fitness, that is, neither is clearly better or worse than the
        /// other.  You may assume that _fitness is of the same class as yourself.
        /// For any two fitnesses fit1 and fit2 of the same class,
        /// it must be the case that fit1.equivalentTo(fit2) == fit2.equivalentTo(fit1),
        /// and that only one of fit1.BetterThan(fit2), fit1.equivalentTo(fit2),
        /// and fit2.BetterThan(fit1) can be true.
        /// </summary>
        public abstract bool EquivalentTo(IFitness other);

        /// <summary>
        /// Should return true if this fitness is clearly better than _fitness;
        /// You may assume that _fitness is of the same class as yourself. 
        /// For any two fitnesses fit1 and fit2 of the same class,
        /// it must be the case that fit1.equivalentTo(fit2) == fit2.equivalentTo(fit1),
        /// and that only one of fit1.BetterThan(fit2), fit1.equivalentTo(fit2),
        /// and fit2.BetterThan(fit1) can be true.
        /// </summary>
        public abstract bool BetterThan(IFitness other);

        /// <summary>
        /// This method currently defers to the original ECJ methods "BetterThan" and "EquivalentTo"
        /// for comparison. Those are both abstract here, and must be overridden to meet specific needs.
        /// This simply makes the ultimate implementation conform to the "expected" .NET comparison style.
        /// </summary>
        /// <remarks>
        /// If the legacy "methods of comparison" go away at some point,
        /// this MUST become abstract to allow for concrete implementation.
        /// </remarks>
        public int CompareTo(IFitness other)
        {
            // If the following is not valid, the derivation should override
            if (other == null || BetterThan(other)) return 1;
            if (EquivalentTo(other)) return 0;
            return -1;
        }

        int IComparable.CompareTo(object other)
        {
            if (!(other is IFitness))
                throw new ArgumentException(String.Format("Argument is not of the correct type. It must be typeof({0}).", GetType().Name), "other");
            return CompareTo((IFitness)other);
        }

        #endregion // Comparison
        #region Cloning

        public virtual object Clone()
        {
            try
            {
                var f = (Fitness)MemberwiseClone();
                if (f.Trials != null) f.Trials = new List<double>(Trials);  // we can do a light clone because trials must be immutable
                f.SetContext(f.GetContext()); // deep-clones and removes context just in case
                return f;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Cloning Error!", ex);
            } // never happens
        }

        #endregion // Cloning
        #region ToString

        /// <summary>
        /// Print to a string the fitness in a fashion readable by humans, and not intended
        /// to be parsed in again.  The default form
        /// simply calls ToString(), but you'll probably want to override this to something else. 
        /// </summary>
        public virtual string FitnessToStringForHumans()
        {
            return ToString();
        }

        /// <summary>
        /// Print to a string the fitness in a fashion intended
        /// to be parsed in again via ReadFitness(...).
        /// The fitness and evaluated flag should not be included.  The default form
        /// simply calls ToString(), which is almost certainly wrong, 
        /// and you'll probably want to override this to something else.  When overriding, you
        /// may wish to check to see if the 'trials' variable is non-null, and issue an error if so.
        /// </summary>
        public virtual string FitnessToString()
        {
            return ToString();
        }

        #endregion // ToString
        #region IO

        /// <summary>
        /// Should print the fitness out fashion pleasing for humans to read, 
        /// using state.Output.PrintLn(...,log).  The default version
        /// of this method calls FitnessToStringForHumans(), adds context (collaborators) if any,
        /// and PrintLns the resultant string.
        /// </summary>
        public void PrintFitnessForHumans(IEvolutionState state, int log)
        {
            var s = FitnessToStringForHumans();
            if (Context != null)
            {
                for (var i = 0; i < Context.Length; i++)
                {
                    if (Context[i] != null)
                    {
                        s += "\nCollaborator " + i + ": ";
                        // temporarily de-link the context of the collaborator to avoid loops
                        var c = Context[i].Fitness.Context;
                        Context[i].Fitness.Context = null;
                        s += Context[i].GenotypeToStringForHumans();
                        // relink
                        Context[i].Fitness.Context = c;
                    }
                    else // that's me!
                    {
                        // do nothing
                    }
                }
            }
            state.Output.PrintLn(s, log);
        }

        /// <summary>
        /// Should print the fitness out in a computer-readable fashion.
        /// </summary>
        public void PrintFitness(IEvolutionState state, int log)
        {
            state.Output.PrintLn(ToString(), log);
        }

        /// <summary>
        /// Should print the fitness out in a computer-readable fashion, 
        /// using writer.PrintLn(...).  You might use
        /// ec.util.Code to encode fitness values.  The default version
        /// of this method calls FitnessToString() and PrintLn's the
        /// resultant string.
        /// </summary>
        public virtual void PrintFitness(IEvolutionState state, StreamWriter writer)
        {
            writer.WriteLine(ToString());
        }

        /// <summary>
        /// Reads in the fitness from a form outputted by FitnessToString() and thus
        /// PrintFitnessForHumans(...).  
        /// The default version exits the program with an "unimplemented" error; you should override this, and be
        /// certain to also write the 'trials' variable as well.        /// </summary>
        public virtual void ReadFitness(IEvolutionState state, StreamReader reader)
        {
            state.Output.Fatal("readFitness(IEvolutionState, StreamReader)  not implemented in " + GetType().Name);
        }

        /// <summary>
        /// Writes the binary form of an individual out to a DataOutput.  This is not for serialization:
        /// the object should only write out the data relevant to the object sufficient to rebuild it from a DataInput.
        /// The default version exits the program with an "unimplemented" error; you should override this, and be
        /// certain to also write the 'trials' variable as well.        
        /// </summary>
        public virtual void WriteFitness(IEvolutionState state, BinaryWriter writer)
        {
            state.Output.Fatal("WriteFitness(EvolutionState, DataOutput) not implemented in " + GetType().Name);
        }

        /// <summary>
        /// Reads the binary form of an individual from a DataInput.  This is not for serialization:
        /// the object should only read in the data written out via printIndividual(state,dataInput).  
        /// The default version exits the program with an "unimplemented" error; 
        /// you should override this, and be certain to also write the 'trials' variable as well.
        /// </summary>
        public virtual void ReadFitness(IEvolutionState state, BinaryReader reader)
        {
            state.Output.Fatal("ReadFitness(IEvolutionState, BinaryReader) not implemented in " + GetType());
        }

        /// <summary>
        ///  Writes trials out to DataOutput
        /// </summary>
        public void WriteTrials(IEvolutionState state, BinaryWriter writer)
        {
            if (Trials == null)
                writer.Write(-1);
            else
            {
                var len = Trials.Count;
                writer.Write(len);
                for (var i = 0; i < len; i++)
                    writer.Write((Double)Trials[i]);
            }
        }

        /// <summary>
        /// Reads trials in from a BinaryReader.
        /// </summary>
        public void ReadTrials(IEvolutionState state, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            if (len >= 0)
            {
                Trials = new List<double>(len);
                for (var i = 0; i < len; i++)
                    Trials.Add(reader.ReadDouble());
            }
        }

        #endregion // IO
    }
}