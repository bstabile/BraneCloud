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

using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Logging;

namespace BraneCloud.Evolution.EC.Rule
{
    /// <summary> 
    /// Rule is an abstract class for describing rules. It is abstract
    /// because it is supposed to be extended by different classes
    /// modelling different kinds of rules.
    /// It provides the reset abstract method for randomizing the individual. 
    /// It also provides the mutate function for mutating an individual rule
    /// It also provides the clone function for cloning the rule.
    /// 
    /// <p/>You will need to implement some kind of artificial ordering between
    /// rules in a ruleset using the Comparable interface,
    /// so the ruleset can be sorted in such a way that it can be compared with
    /// another ruleset for equality.  You should also implement hashCode
    /// and equals 
    /// in such a way that they aren't based on pointer information, but on actual
    /// internal features. 
    /// 
    /// <p/>Every rule points to a RuleConstraints which handles information that
    /// Rule shares with all the other Rules in a RuleSet.
    /// <p/>In addition to serialization for checkpointing, Rules may read and write themselves to streams in three ways.
    /// 
    /// <ul>
    /// <li/><b>writeRule(...,DataOutput)/readRule(...,DataInput)</b>&nbsp;&nbsp;&nbsp;This method
    /// transmits or receives a Rule in binary.  It is the most efficient approach to sending
    /// Rules over networks, etc.  The default versions of writeRule/readRule throw errors.
    /// You don't need to implement them if you don't plan on using read/writeRule.
    /// 
    /// <li/><b>printRule(...,PrintWriter)/readRule(...,LineNumberReader)</b>&nbsp;&nbsp;&nbsp;This
    /// approach transmits or receives a Rule in text encoded such that the Rule is largely readable
    /// by humans but can be read back in 100% by ECJ as well.  To do this, these methods will typically encode numbers
    /// using the <tt>ec.util.Code</tt> class.  These methods are mostly used to write out populations to
    /// files for inspection, slight modification, then reading back in later on.  <b>readRule</b>
    /// reads in a line, then calls <b>readRuleFromString</b> on that line.
    /// You are responsible for implementing readRuleFromString: the Code class is there to help you.
    /// The default version throws an error if called.
    /// <b>printRule</b> calls <b>printRuleToString</b>
    /// and PrintLns the resultant string. You are responsible for implementing the printRuleToString method in such
    /// a way that readRuleFromString can read back in the Rule PrintLn'd with printRuleToString.  The default form
    /// of printRuleToString() simply calls <b>ToString()</b> 
    /// by default.  You might override <b>printRuleToString()</b> to provide better information.   You are not required to implement these methods, but without
    /// them you will not be able to write Rules to files in a simultaneously computer- and human-readable fashion.
    /// 
    /// <li/><b>printRuleForHumans(...,PrintWriter)</b>&nbsp;&nbsp;&nbsp;This
    /// approach prints a Rule in a fashion intended for human consumption only.
    /// <b>printRuleForHumans</b> calls <b>printRuleToStringForHumans()</b> 
    /// and PrintLns the resultant string.  The default form of this method just returns the value of
    /// <b>ToString()</b>. You may wish to override this to provide more information instead. 
    /// You should handle one of these methods properly
    /// to ensure Rules can be printed by ECJ.
    /// </ul>
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>constraints</tt><br/>
    /// <font size="-1">string</font></td>
    /// <td valign="top">(name of the rule constraint)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// rule.rule
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.rule.Rule")]
    public abstract class Rule : IPrototype, IComparable
    {
        #region Constants

        public const string P_RULE = "rule";
        public const string P_CONSTRAINTS = "constraints";

        #endregion // Constants
        #region Properties

        public virtual IParameter DefaultBase
        {
            get { return RuleDefaults.ParamBase.Push(P_RULE); }
        }

        /// <summary>
        /// An index to a RuleConstraints
        /// </summary>
        public int ConstraintCount { get; set; }

        #endregion // Properties
        #region Setup

        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            var constraintName = state.Parameters.GetString(paramBase.Push(P_CONSTRAINTS), DefaultBase.Push(P_CONSTRAINTS));
            if (constraintName == null)
                state.Output.Fatal("No RuleConstraints name given", paramBase.Push(P_CONSTRAINTS), DefaultBase.Push(P_CONSTRAINTS));

            ConstraintCount = RuleConstraints.ConstraintsFor(constraintName, state).ConstraintCount;
            state.Output.ExitIfErrors();
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// The reset method randomly reinitializes the rule.
        /// </summary>
        public abstract void Reset(IEvolutionState state, int thread);

        /// <summary>
        /// Mutate the rule.  The default form just resets the rule.
        /// </summary>
        public virtual void Mutate(IEvolutionState state, int thread)
        {
            Reset(state, thread);
        }

        /// <summary>
        /// Returns the Rule's constraints.  A good JIT compiler should inline this.
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public RuleConstraints Constraints(RuleInitializer initializer)
        {
            return initializer.RuleConstraints[ConstraintCount];
        }

        #endregion // Operations
        #region Comparison

        /// <summary>
        /// Rulerates a hash code for this rule -- the rule for this is that the hash code
        /// must be the same for two rules that are equal to each other genetically. 
        /// </summary>
        abstract public override int GetHashCode();
        
        /// <summary>
        /// Unlike the standard form for Java, this function should return true if this
        /// rule is "genetically identical" to the other rule. 
        /// </summary>
        public override bool Equals(object other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// This function replaces the old gt and lt functions that Rule used to require
        /// as it implemented the SortComparator interface.  If you had implemented those
        /// old functions, you can simply implement this function as:
        /// 
        /// <code>
        ///     public abstract int CompareTo(Object o)
        ///     {
        ///         if (gt(this,o)) return 1;
        ///         if (lt(this,o)) return -1;
        ///         return 0;
        ///     }
        /// </code>
        /// </summary>
        public abstract int CompareTo(object o);

        #endregion // Comparison
        #region Cloning

        public virtual object Clone()
        {
            try
            {
                return MemberwiseClone();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Cloning Error!", ex);
            } // never happens
        }

        #endregion // Cloning
        #region IO

        /// <summary>
        /// Nice printing.  
        /// The default form simply calls printRuleToStringForHumans and prints the result,
        /// but you might want to override this.
        /// </summary>
        public virtual void PrintRuleForHumans(IEvolutionState state, int log)
        {
            PrintRuleForHumans(state, log);
        }

        /// <summary>
        /// Nice printing to a string. The default form calls ToString().  
        /// </summary>
        public virtual string PrintRuleToStringForHumans()
        {
            return ToString();
        }

        /// <summary>
        /// Prints the rule to a string in a fashion readable by readRuleFromString.
        /// The default form simply calls ToString() -- you should just override ToString() 
        /// if you don't need the EvolutionState. 
        /// </summary>
        public virtual string PrintRuleToString()
        {
            return ToString();
        }

        /// <summary>
        /// Reads a rule from a string, which may contain a '\n'.
        /// Override this method.  The default form generates an error. 
        /// </summary>
        public virtual void ReadRuleFromString(string ruleText, IEvolutionState state)
        {
            state.Output.Error("readRuleFromString(ruleText,state) unimplemented in " + GetType());
        }

        /// <summary>
        /// Prints the rule in a way that can be read by readRule().  The default form simply
        /// calls printRuleToString(state).   Override this rule to do custom writing to the log,
        /// or just override printRuleToString(...), which is probably easier to do.
        /// </summary>
        public virtual void PrintRule(IEvolutionState state, int log)
        {
            PrintRule(state, log);
        }

        /// <summary>
        /// Prints the rule in a way that can be read by readRule().  The default form simply
        /// calls printRuleToString(state).   Override this rule to do custom writing,
        /// or just override printRuleToString(...), which is probably easier to do.
        /// </summary>
        public virtual void PrintRule(IEvolutionState state, StreamWriter writer)
        {
            writer.WriteLine(PrintRuleToString());
        }

        /// <summary>
        /// Reads a rule printed by printRule(...).  The default form simply reads a line into
        /// a string, and then calls readRuleFromString() on that line.  Override this rule to do
        /// custom reading, or just override readRuleFromString(...), which is probably easier to do.
        /// </summary>
        public virtual void ReadRule(IEvolutionState state, StreamReader reader)
        {
            ReadRuleFromString(reader.ReadLine(), state);
        }

        /// <summary>
        /// Override this if you need to write rules out to a binary stream. 
        /// </summary>
        public virtual void WriteRule(IEvolutionState state, BinaryWriter dataOutput)
        {
            state.Output.Fatal("WriteRule(EvolutionState, DataOutput) not implemented in " + GetType());
        }

        /// <summary>
        /// Override this if you need to read rules in from a binary stream. 
        /// </summary>
        public virtual void ReadRule(IEvolutionState state, BinaryReader dataInput)
        {
            state.Output.Fatal("ReadRule(EvolutionState, DataInput) not implemented in " + GetType());
        }

        #endregion // IO
    }
}