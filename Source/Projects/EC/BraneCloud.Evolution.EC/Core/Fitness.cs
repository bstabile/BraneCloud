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

        /// <inheritdoc />
        public abstract double Value { get; /* protected set; */ } // The actual value should be handled in each derived type!

        /// <inheritdoc />
        public abstract bool IsIdeal { get; }

        /// <inheritdoc />
        public List<double> Trials { get => _trials; set => _trials = value; }

        private List<double> _trials;

        /// <inheritdoc />
        public Individual[] Context { get => _context; set => _context = value; }
        private Individual[] _context;

        #endregion // Properties
        #region Setup

        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            // by default does nothing
        }

        #endregion // Setup
        #region Operations

        /// <inheritdoc />
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

        /// <inheritdoc />
        public Individual[] GetContext()
        {
            return Context;
        }

        #endregion

        #endregion // Operations
        #region Comparison

        /// <inheritdoc />
        public abstract bool EquivalentTo(IFitness other);

        /// <inheritdoc />
        public abstract bool BetterThan(IFitness other);

        /// <inheritdoc />
        public virtual bool ContextIsBetterThan(IFitness other)
        {
            if (other.Trials == null) return true; // I win
            if (Trials == null) return false; // he wins
            return BestTrial(Trials) < BestTrial(other.Trials);
        }

        /// <inheritdoc />
        public virtual int CompareTo(IFitness other)
        {
            // If the following is not valid, the derivation should override
            if (other == null || BetterThan(other)) return 1;
            if (EquivalentTo(other)) return 0;
            return -1;
        }

        int IComparable.CompareTo(object other)
        {
            if (!(other is IFitness))
                throw new ArgumentException(
                    $"Argument is not of the correct type. It must be typeof({GetType().Name}).", nameof(other));
            return CompareTo((IFitness)other);
        }

        /// <summary>
        /// Sets the fitness to be the same value as the best of the provided fitnesses.  This method calls
        /// setToMeanOf(...), so if that method is unimplemented, this method will also fail.
        /// </summary>
        public virtual void SetToBestOf(IEvolutionState state, IFitness[] fitnesses)
        {
            var f2 = (IFitness[])fitnesses.Clone();
            Array.Sort(f2);
            SetToMeanOf(state, new[] { f2[0] });
        }

        /// <summary>
        /// Sets the fitness to be the same value as the mean of the provided fitnesses.  The default
        /// version of this method exits with an "unimplemented" error; you should override this.
        /// </summary>
        public virtual void SetToMeanOf(IEvolutionState state, IFitness[] fitnesses)
        {
            state.Output.Fatal("SetToMeanOf(EvolutionState, IFitness[]) not implemented in " + GetType());
        }

        /// <summary>
        /// Sets the fitness to be the median of the provided fitnesses.  This method calls
        /// SetToMeanOf(...), so if that method is unimplemented, this method will also fail.
        /// </summary>
        public virtual void SetToMedianOf(IEvolutionState state, IFitness[] fitnesses)
        {
            var f2 = (IFitness[])fitnesses.Clone();
            Array.Sort(f2);
            if (f2.Length % 2 == 1)
            {
                SetToMeanOf(state, new[] { f2[f2.Length / 2] });   // for example, 5/2 = 2, and 0, 1, *2*, 3, 4
            }
            else
            {
                SetToMeanOf(state, new[] { f2[f2.Length / 2 - 1], f2[f2.Length / 2] });  // for example, 6/2 = 3, and 0, 1, *2*, *3*, 4, 5
            }
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

        /// <inheritdoc />
        public virtual string FitnessToStringForHumans()
        {
            return ToString();
        }

        /// <inheritdoc />
        public virtual string FitnessToString()
        {
            return ToString();
        }

        #endregion // ToString
        #region IO

        /// <inheritdoc />
        public virtual void PrintFitnessForHumans(IEvolutionState state, int log)
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

        /// <inheritdoc />
        public virtual void PrintFitness(IEvolutionState state, int log)
        {
            state.Output.PrintLn(ToString(), log);
        }

        /// <inheritdoc />
        public virtual void PrintFitness(IEvolutionState state, StreamWriter writer)
        {
            writer.WriteLine(ToString());
        }

        /// <inheritdoc />
        public virtual void ReadFitness(IEvolutionState state, StreamReader reader)
        {
            state.Output.Fatal("readFitness(IEvolutionState, StreamReader)  not implemented in " + GetType().Name);
        }

        /// <inheritdoc />
        public virtual void WriteFitness(IEvolutionState state, BinaryWriter writer)
        {
            state.Output.Fatal("WriteFitness(EvolutionState, DataOutput) not implemented in " + GetType().Name);
        }

        /// <inheritdoc />
        public virtual void ReadFitness(IEvolutionState state, BinaryReader reader)
        {
            state.Output.Fatal("ReadFitness(IEvolutionState, BinaryReader) not implemented in " + GetType());
        }

        /// <inheritdoc />
        public virtual void WriteTrials(IEvolutionState state, BinaryWriter writer)
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

        /// <inheritdoc />
        public virtual void ReadTrials(IEvolutionState state, BinaryReader reader)
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