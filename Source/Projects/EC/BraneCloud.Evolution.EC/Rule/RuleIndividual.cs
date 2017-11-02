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
using System.IO;
using System.Linq;
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Rule
{
    /// <summary> 
    /// RuleIndividual is an Individual with an array of RuleSets, each of which
    /// is a set of Rules.  RuleIndividuals belong to some subclass of RuleSpecies
    /// (or just RuleSpecies itself).
    /// 
    /// <p/>RuleIndividuals really have basically one parameter: the number
    /// of RuleSets to use.  This is determined by the <tt>num-rulesets</tt>
    /// parameter.
    /// <p/><b>From ec.Individual:</b>  
    /// 
    /// <p/>In addition to serialization for checkpointing, Individuals may read and write themselves to streams in three ways.
    /// 
    /// <ul>
    /// <li/><b>WriteIndividual(...,BinaryWriter) / ReadIndividual(...,BinaryReader)</b>&nbsp;&nbsp;&nbsp;This method
    /// transmits or receives an individual in binary.  It is the most efficient approach to sending
    /// individuals over networks, etc.  These methods write the evaluated flag and the fitness, then
    /// call <b>ReadGenotype/WriteGenotype</b>, which you must implement to write those parts of your 
    /// Individual special to your functions-- the default versions of ReadGenotype/WriteGenotype throw errors.
    /// You don't need to implement them if you don't plan on using Read/WriteIndividual.
    /// 
    /// <li/><b>PrintIndividual(...,StreamWriter) / ReadIndividual(...,StreamReader)</b>&nbsp;&nbsp;&nbsp;This
    /// approach transmits or receives an indivdual in text encoded such that the individual is largely readable
    /// by humans but can be read back in 100% by ECJ as well.  To do this, these methods will encode numbers
    /// using the <tt>ec.util.Code</tt> class.  These methods are mostly used to write out populations to
    /// files for inspection, slight modification, then reading back in later on.  <b>ReadIndividual</b> reads
    /// in the fitness and the evaluation flag, then calls <b>ParseGenotype</b> to read in the remaining individual.
    /// You are responsible for implementing ParseGenotype: the Code class is there to help you.
    /// <b>printIndividual</b> writes out the fitness and evaluation flag, then calls <b>GenotypeToString</b> 
    /// and PrintLns the resultant string. You are responsible for implementing the GenotypeToString method in such
    /// a way that ParseGenotype can read back in the individual PrintLn'd with GenotypeToString.  The default form
    /// of GenotypeToString simply calls <b>ToString</b>, which you may override instead if you like.  The default
    /// form of <b>ParseGenotype</b> throws an error.  You are not required to implement these methods, but without
    /// them you will not be able to write individuals to files in a simultaneously computer- and human-readable fashion.
    /// 
    /// <li/><b>PrintIndividualForHumans(...,StreamWriter)</b>&nbsp;&nbsp;&nbsp;This
    /// approach prints an individual in a fashion intended for human consumption only.
    /// <b>PrintIndividualForHumans</b> writes out the fitness and evaluation flag, then calls <b>GenotypeToStringForHumans</b> 
    /// and PrintLns the resultant string. You are responsible for implementing the GenotypeToStringForHumans method.
    /// The default form of GenotypeToStringForHumans simply calls <b>ToString</b>, which you may override instead if you like
    /// (though note that GenotypeToString's default also calls ToString).  You should handle one of these methods properly
    /// to ensure individuals can be printed by ECJ.
    /// </ul>
    /// <p/>In general, the various readers and writers do three things: they tell the Fitness to read/write itself,
    /// they read/write the evaluated flag, and they read/write the Rulesets.  If you add instance variables to
    /// a RuleIndividual or subclass, you'll need to read/write those variables as well.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>num-rulesets</tt><br/>
    /// <font size="-1">int >= 1</font></td>
    /// <td valign="top">(number of rulesets used)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>ruleset</tt>.<i>n</i><br/>
    /// <font size="-1">Classname, subclass of or = ec.rule.RuleSet</font></td>
    /// <td valign="top">(class of ruleset <i>n</i>)</td></tr>
    /// </table>
    /// 
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"/><i>base</i>.<tt>ruleset</tt>.<i>n</i><br/>
    /// <td>RuleSet <i>n</i></td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// rule.individual
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.rule.RuleIndividual")]
    public class RuleIndividual : Individual
    {
        #region Constants

        private const long SerialVersionUID = 1;

        public const string P_RULESET = "ruleset";
        public const string P_NUMRULESETS = "num-rulesets";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return RuleDefaults.ParamBase.Push(P_INDIVIDUAL); }
        }

        public override long Size
        {
            get
            {
                return Rulesets.Aggregate<RuleSet, long>(0, (current, t) => current + t.NumRules);
            }
        }

        /// <summary>
        /// The individual's rulesets. 
        /// </summary>
        public RuleSet[] Rulesets { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase); // actually unnecessary (Individual.Setup() is empty)

            // I'm the top-level Setup, I guess
            var numrulesets = state.Parameters.GetInt(paramBase.Push(P_NUMRULESETS), DefaultBase.Push(P_NUMRULESETS), 1); // need at least 1 ruleset!
            if (numrulesets == 0)
                state.Output.Fatal("RuleIndividual needs at least one RuleSet!", paramBase.Push(P_NUMRULESETS), DefaultBase.Push(P_NUMRULESETS));

            Rulesets = new RuleSet[numrulesets];

            for (var x = 0; x < numrulesets; x++)
            {
                Rulesets[x] = (RuleSet)(state.Parameters.GetInstanceForParameterEq(
                    paramBase.Push(P_RULESET).Push("" + x),
                    DefaultBase.Push(P_RULESET), typeof(RuleSet)));
                Rulesets[x].Setup(state, paramBase.Push(P_RULESET).Push("" + x));
            }
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Mutates the Individual.  The default implementation simply calls mutate(...) on each of the RuleSets.
        /// </summary>
        public void Mutate(IEvolutionState state, int thread)
        {
            foreach (var t in Rulesets)
                t.Mutate(state, thread);
        }

        public virtual void Reset(IEvolutionState state, int thread)
        {
            foreach (var t in Rulesets)
                t.Reset(state, thread);
        }

        /// <summary>
        /// Called by pipelines before they've modified the individual and
        /// it might need to be "fixed"  -- basically a hook for you to override.
        /// By default, calls validateRules on each ruleset. 
        /// </summary>
        public virtual void PreprocessIndividual(IEvolutionState state, int thread)
        {
            foreach (var t in Rulesets)
                t.PreprocessRules(state, thread);
        }

        /// <summary>
        /// Called by pipelines after they've modified the individual and
        /// it might need to be "fixed"  -- basically a hook for you to override.
        /// By default, calls validateRules on each ruleset. 
        /// </summary>
        public virtual void PostprocessIndividual(IEvolutionState state, int thread)
        {
            foreach (var t in Rulesets)
                t.PostprocessRules(state, thread);
        }

        #endregion // Operations
        #region Comparison

        public override int GetHashCode()
        {
            var hash = GetType().GetHashCode();
            return Rulesets.Aggregate(hash, (current, t) => (current << 1 | BitShifter.URShift(current, 31)) ^ t.GetHashCode());
        }

        public override bool Equals(object ind)
        {
            if (ind == null) return false;
            // My loose definition: ind must be a 
            if (!GetType().Equals(ind.GetType()))
                // not the same class, I'm conservative that way
                return false;

            var other = (RuleIndividual)ind;
            if (Rulesets.Length != other.Rulesets.Length)
                return false;
            return !Rulesets.Where((t, x) => !t.Equals(other.Rulesets[x])).Any();
        }

        #endregion // Comparison
        #region Cloning

        public override object Clone()
        {
            var myobj = (RuleIndividual)(base.Clone());
            myobj.Rulesets = new RuleSet[Rulesets.Length];
            for (var x = 0; x < Rulesets.Length; x++)
                myobj.Rulesets[x] = (RuleSet)(Rulesets[x].Clone());
            return myobj;
        }

        #endregion // Cloning
        #region IO

        /// <summary>
        /// Overridden for the RuleIndividual genotype, writing each ruleset in turn. 
        /// </summary>
        public override void PrintIndividualForHumans(IEvolutionState state, int log)
        {
            state.Output.PrintLn(EVALUATED_PREAMBLE + (Evaluated ? "true" : "false"), log);
            Fitness.PrintFitnessForHumans(state, log);
            for (var x = 0; x < Rulesets.Length; x++)
            {
                state.Output.PrintLn("Ruleset " + x + ":", log);
                Rulesets[x].PrintRuleSetForHumans(state, log);
            }
        }

        /// <summary>
        /// Overridden for the RuleIndividual genotype, writing each ruleset in turn. 
        /// </summary>
        public override void PrintIndividual(IEvolutionState state, int log)
        {
            state.Output.PrintLn(EVALUATED_PREAMBLE + Code.Encode(Evaluated), log);
            Fitness.PrintFitness(state, log);
            for (var x = 0; x < Rulesets.Length; x++)
            {
                state.Output.PrintLn("Ruleset " + x + ":", log);
                Rulesets[x].PrintRuleSet(state, log);
            }
        }

        /// <summary>
        /// Overridden for the RuleIndividual genotype, writing each ruleset in turn. 
        /// </summary>
        public override void PrintIndividual(IEvolutionState state, StreamWriter writer)
        {
            writer.WriteLine(EVALUATED_PREAMBLE + Code.Encode(Evaluated));
            Fitness.PrintFitness(state, writer);
            for (var x = 0; x < Rulesets.Length; x++)
            {
                writer.WriteLine("Ruleset " + x + ":");
                Rulesets[x].PrintRuleSet(state, writer);
            }
        }

        /// <summary>
        /// Overridden for the RuleIndividual genotype. 
        /// </summary>
        public override void ParseGenotype(IEvolutionState state, StreamReader reader)
        {
            // read my ruleset
            foreach (var t in Rulesets)
            {
                reader.ReadLine(); // throw it away -- it's the ruleset# indicator
                t.ReadRuleSet(state, reader);
            }
        }

        /// <summary>
        /// Overridden for the RuleIndividual genotype, writing each ruleset in turn. 
        /// </summary>
        public override void WriteGenotype(IEvolutionState state, BinaryWriter writer)
        {
            writer.Write(Rulesets.Length);
            foreach (var t in Rulesets)
                t.WriteRuleSet(state, writer);
        }

        /// <summary>
        /// Overridden for the RuleIndividual genotype. 
        /// </summary>
        public override void ReadGenotype(IEvolutionState state, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            if (Rulesets == null || Rulesets.Length != len)
                state.Output.Fatal("Number of RuleSets differ in RuleIndividual when reading from ReadGenotype(EvolutionState, DataInput).");
            foreach (var t in Rulesets)
                t.ReadRuleSet(state, reader);
        }

        #endregion // IO

    }
}