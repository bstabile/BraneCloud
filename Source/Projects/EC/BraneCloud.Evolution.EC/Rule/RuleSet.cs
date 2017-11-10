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
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Rule
{
    /// <summary> 
    /// RuleSet is a set of Rules, implemented straightforwardly as an arbitrary-length array of Rules.
    /// A RuleIndividual is simply a list of RuleSets.  Most typically, a RuleIndividual contains a
    /// single RuleSet, containing a variety of Rules.
    /// RuleSets contain many useful subsetting and modification functions which you can use
    /// in breeding operators which modify RuleSets and Rules.
    /// 
    /// <p/> Besides the Rules themselves, the only thing else a RuleSet contains is a pointer to a
    /// corresponding RuleSetConstraints object, which holds all of its modification parameters.
    /// See RuleSetConstraints for a description of these parameters.
    /// <p/>In addition to serialization for checkpointing, RuleSets may read and write themselves to streams in three ways.
    /// 
    /// <ul>
    /// <li/><b>writeRuleSet(...,DataOutput)/readRuleSet(...,DataInput)</b>&nbsp;&nbsp;&nbsp;This method
    /// transmits or receives a RuleSet in binary.  It is the most efficient approach to sending
    /// RuleSets over networks, etc.  The default versions of writeRuleSet/readRuleSet reads/writes out the number
    /// of rules, then calls read/writeRule(...) on each Rule.  Override this if you need more functionality.
    /// 
    /// <li/><b>printRuleSet(...,PrintWriter)/readRuleSet(...,LineNumberReader)</b>&nbsp;&nbsp;&nbsp;This
    /// approach transmits or receives a RuleSet in text encoded such that the RuleSet is largely readable
    /// by humans but can be read back in 100% by ECJ as well.  To do this, these methods will typically encode numbers
    /// using the <tt>ec.util.Code</tt> class.  These methods are mostly used to write out populations to
    /// files for inspection, slight modification, then reading back in later on.  <b>readRuleSet</b>
    /// reads in the number of rules, then calls readRule(...) on each new Rule.  <b>printRuleSet</b> writes
    /// out the number of rules, then calls printrule(...) on each new Rule.  Again, override this if you need more
    /// functionality.
    /// 
    /// <li/><b>printRuleSetForHumans(...,PrintWriter)</b>&nbsp;&nbsp;&nbsp;This
    /// approach prints a RuleSet in a fashion intended for human consumption only.
    /// <b>printRuleSetForHumans</b> prints out the number of rules, then calles <b>printRuleForHumans</b>
    /// on each Rule in turn.  You may wish to override this to provide more information instead. 
    /// You should handle one of these methods properly
    /// to ensure RuleSets can be printed by ECJ.
    /// </ul>
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>constraints</tt><br/>
    /// <font size="-1">string</font></td>
    /// <td valign="top">(name of the rule set constraints)</td></tr>
    /// </table>
    /// 
    /// <p/><b>Default Base</b><br/>
    /// rule.ruleset
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.rule.RuleSet")]
    public class RuleSet : IPrototype
    {
        #region Constants

        /// <summary>
        /// The message to appear when printing the rule set
        /// </summary>
        public const string N_RULES = "Num: ";

        public const string P_RULESET = "ruleset";

        /// <summary>
        /// The constraint for the rule set
        /// </summary>
        public const string P_CONSTRAINTS = "constraints";

        #endregion // Constants
        #region Properties

        public virtual IParameter DefaultBase
        {
            get { return RuleDefaults.ParamBase.Push(P_RULESET); }
        }

        /// <summary>
        /// An index to a RuleSetConstraints
        /// </summary>
        public int ConstraintCount { get; set; }

        /// <summary>
        /// The rules in the rule set
        /// </summary>
        public Rule[] Rules
        {
            get { return _rules; }
            set { _rules = value; }
        }
        private Rule[] _rules = new Rule[0];

        /// <summary>
        /// How many rules are there used in the rules array
        /// </summary>
        public int RuleCount
        {
            get { return _ruleCount; }
            set { _ruleCount = value; }
        }
        private int _ruleCount = 0;

        /// <summary>
        /// How many rules are there used in the rules array
        /// </summary>
        public virtual int NumRules
        {
            get { return _ruleCount; }
        }

        #endregion // Properties
        #region Setup

        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            var constraintName = state.Parameters.GetString(paramBase.Push(P_CONSTRAINTS), DefaultBase.Push(P_CONSTRAINTS));
            if (String.IsNullOrEmpty(constraintName))
                state.Output.Fatal("No RuleSetConstraints name given", paramBase.Push(P_CONSTRAINTS), DefaultBase.Push(P_CONSTRAINTS));

            ConstraintCount = RuleSetConstraints.ConstraintsFor(constraintName, state).ConstraintCount;
            state.Output.ExitIfErrors();
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Returns the RuleSet's constraints.  A good JIT compiler should inline this.
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public RuleSetConstraints Constraints(RuleInitializer initializer)
        {
            return initializer.RuleSetConstraints[ConstraintCount];
        }

        /// <summary>
        /// Mutates rules in the RuleSet independently with the given probability.
        /// </summary>
        public virtual void Mutate(IEvolutionState state, int thread)
        {
            var initializer = ((RuleInitializer)state.Initializer);

            for (var i = 0; i < _ruleCount; i++)
            {
                _rules[i].Mutate(state, thread);
            }
            while (state.Random[thread].NextBoolean(Constraints(initializer).p_del) && _ruleCount > Constraints(initializer).MinSize)
            {
                RemoveRandomRule(state, thread);
            }
            while (state.Random[thread].NextBoolean(Constraints(initializer).p_add) && _ruleCount < Constraints(initializer).MaxSize)
            {
                AddRandomRule(state, thread);
            }
            if (state.Random[thread].NextBoolean(Constraints(initializer).p_randorder))
            {
                RandomizeRulesOrder(state, thread);
            }
        }

        /// <summary>
        /// Should be called by pipelines to "fix up" the rulesets before they have been
        /// mutated or crossed over.  Override this method to do so.
        /// </summary>
        public virtual void PreprocessRules(IEvolutionState state, int thread)
        {
        }

        /// <summary>
        /// Should be called by pipelines to "fix up" the rulesets after they have been
        /// mutated or crossed over.  Override this method to do so.
        /// </summary>
        public virtual void PostprocessRules(IEvolutionState state, int thread)
        {
        }

        /// <summary>
        /// Randomizes the order of the rules in the rule set. It is helpful when the
        /// order of rule is important for the conflict resolution.
        /// </summary>
        public virtual void RandomizeRulesOrder(IEvolutionState state, int thread)
        {
            for (var i = _ruleCount - 1; i > 0; i--)
            {
                var j = state.Random[thread].NextInt(i + 1);
                var temp = _rules[i];
                _rules[i] = _rules[j];
                _rules[j] = temp;
            }
        }

        /// <summary>
        /// Add a random rule to the rule set.
        /// </summary>
        public virtual void AddRandomRule(IEvolutionState state, int thread)
        {
            var newRule = (Rule)(Constraints(((RuleInitializer)state.Initializer)).RulePrototype.Clone());
            newRule.Reset(state, thread);
            AddRule(newRule);
        }

        /// <summary>
        /// Add a rule directly to the rule set.  Does not copy the rule.
        /// </summary>
        public virtual void AddRule(Rule rule)
        {
            if (_rules == null  || _ruleCount == _rules.Length)
            {
                Rule[] tempRules;
                if (_rules == null)
                {
                    tempRules = new Rule[2];
                }
                else
                {
                    tempRules = new Rule[(_rules.Length + 1) * 2];
                }
                if (_rules != null)
                    Array.Copy(_rules, 0, tempRules, 0, _rules.Length);
                _rules = tempRules;
            }

            // add the rule and increase the counter
            _rules[_ruleCount++] = rule;
        }

        /// <summary>
        /// Removes a rule from the rule set and returns it.  
        /// If index is out of bounds, then this method returns null.
        /// </summary>
        public virtual Rule RemoveRule(int index)
        {
            if (index >= _ruleCount || index < 0)
                return null;

            var myrule = _rules[index];

            if (index < NumRules - 1)   // if we've chosen to remove the last rule, leave it where it is
                Array.Copy(_rules, index + 1, _rules, index, _ruleCount - (index + 1));

            _ruleCount--;
            return myrule;
        }

        /// <summary>
        /// Removes a randomly-chosen rule from the rule set and returns it.  
        /// If there are no rules to remove, this method returns null.
        /// </summary>
        public virtual Rule RemoveRandomRule(IEvolutionState state, int thread)
        {
            if (_ruleCount <= 0)
                return null;

            return RemoveRule(state.Random[thread].NextInt(_ruleCount));
        }

        /// <summary>
        /// Makes a copy of the rules in another RuleSet and adds the rule copies.
        /// </summary>
        public virtual void Join(RuleSet other)
        {
            // if there's not enough place to store the new rules, increase space
            if (_rules.Length <= _ruleCount + other._ruleCount)
            {
                var tempRules = new Rule[_rules.Length + other._rules.Length];
                Array.Copy(_rules, 0, tempRules, 0, _ruleCount);
                _rules = tempRules;
            }
            // copy in the new rules
            Array.Copy(other._rules, 0, _rules, _ruleCount, other._ruleCount);
            // protoclone the rules
            for (var x = _ruleCount; x < _ruleCount + other._ruleCount; x++)
                _rules[x] = (Rule)_rules[x].Clone();
            _ruleCount += other._ruleCount;
        }

        /// <summary>
        /// Clears out existing rules, and loads the rules from the other ruleset without protocloning them.
        /// Mostly for use if you create temporary rulesets (see for example RuleCrossoverPipeline)
        /// </summary>
        public virtual void CopyNoClone(RuleSet other)
        {
            // if there's not enough place to store the new rules, increase space
            if (_rules.Length <= other._ruleCount)
            {
                _rules = new Rule[other._ruleCount];
            }
            // copy in the new rules
            // out.PrintLn(other.rules);
            Array.Copy(other._rules, 0, _rules, 0, other._ruleCount);
            _ruleCount = other._ruleCount;
        }

        /// <summary>
        /// Splits the rule set into n pieces, according to points, which *must* be sorted.
        /// The rules in each piece are cloned and added to the equivalent set.  Sets must be already allocated.
        /// sets.length must be 1+ points.length.  
        /// Comment: This function appends the split rulesets to the existing rulesets already in <i>sets</i>.
        /// </summary>
        public virtual RuleSet[] Split(int[] points, RuleSet[] sets)
        {
            // Do the first chunk or the whole thing
            for (var i = 0; i < (points.Length > 0 ? points[0] : _rules.Length); i++)
                sets[0].AddRule((Rule)_rules[i].Clone());

            if (points.Length > 0)
            {
                // do the in-between chunks
                for (var p = 1; p < points.Length; p++)
                    for (var i = points[p - 1]; i < points[p]; i++)
                        sets[p].AddRule((Rule)_rules[i].Clone());

                // do the chunk
                for (var i = points[points.Length - 1]; i < _rules.Length; i++)
                    sets[points.Length].AddRule((Rule)_rules[i].Clone());
            }
            return sets;
        }

        /// <summary>
        /// Splits the rule set into a number of disjoint rule sets, copying the rules and adding
        /// them to the sets as appropriate.  Each rule independently
        /// throws a die to determine which ruleset it will go into.  Sets must be already allocated.
        /// Comment: This function appends the split rulesets to the existing rulesets already in <i>sets</i>.
        /// </summary>
        public RuleSet[] Split(IEvolutionState state, int thread, RuleSet[] sets)
        {
            for (var i = 0; i < _ruleCount; i++)
                sets[state.Random[thread].NextInt(sets.Length)].AddRule((Rule)_rules[i].Clone());
            return sets;
        }

        /// <summary>
        /// Splits the rule set into a two disjoint rule sets, copying the rules and adding
        /// them to the sets as appropriate.  The value <i>prob</i> is the probability that an element will
        /// land in the first set.  Sets must be already allocated.
        /// Comment: This function appends the splitted rulesets to the existing rulesets already in <i>sets</i>.
        /// </summary>
        public virtual RuleSet[] SplitIntoTwo(IEvolutionState state, int thread, RuleSet[] sets, double prob)
        {
            for (var i = 0; i < _ruleCount; i++)
                if (state.Random[thread].NextBoolean(prob))
                    sets[0].AddRule((Rule)_rules[i].Clone());
                else
                    sets[1].AddRule((Rule)_rules[i].Clone());
            return sets;
        }

        /// <summary>
        /// A reset method for randomly reinitializing the RuleSet
        /// </summary>
        public virtual void Reset(IEvolutionState state, int thread)
        {
            // reinitialize the array of rules
            var initializer = ((RuleInitializer)state.Initializer);
            _ruleCount = Constraints(initializer).NumRulesForReset(this, state, thread);

            _rules = new Rule[_ruleCount];

            for (var i = 0; i < _rules.Length; i++)
            {
                _rules[i] = (Rule)(Constraints(initializer).RulePrototype.Clone());
                _rules[i].Reset(state, thread);
            }
        }

        #endregion // Operations
        #region Comparison

        /// <summary>
        /// The hash code for the rule set.  This isn't a very good hash code,
        /// but it has the benefit of not being O(n lg n) -- otherwise, we'd have
        /// to do something like sort the rules in the individual first and then
        /// do an ordered hash code of some sort, ick.
        /// </summary>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() + _rules.Where(t => t != null).Sum(t => t.GetHashCode());
        }

        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (!GetType().Equals(other.GetType()))
                // not the same class, I'm conservative that way
                return false;

            var otherRuleSet = (RuleSet)other;
            if (_ruleCount != otherRuleSet._ruleCount)
                return false; // quick and dirty
            if (_ruleCount == 0 && otherRuleSet._ruleCount == 0)
                return true; // quick and dirty

            // we need to sort the rulesets.  First, let's clone
            // the rule arrays

            var srules = (Rule[])(_rules.Clone());
            var orules = (Rule[])(otherRuleSet._rules.Clone());

            Array.Sort(srules);
            Array.Sort(orules);

            // Now march down and see if the rules are the same
            for (var x = 0; x < _ruleCount; x++)
                if (!(srules[x].Equals(orules[x])))
                    return false;

            return true;
        }

        #endregion // Comparison
        #region Cloning

        public virtual object Clone()
        {
            try
            {
                var newRuleSet = (RuleSet)MemberwiseClone();
                // copy the rules over
                if (_rules != null)
                {
                    newRuleSet._rules = (Rule[])_rules.Clone();
                }
                else
                {
                    newRuleSet._rules = null;
                }
                for (var x = 0; x < _ruleCount; x++)
                    newRuleSet._rules[x] = (Rule)_rules[x].Clone();
                return newRuleSet;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Cloning Error!", ex);
            } // never happens
        }

        #endregion // Cloning
        #region IO

        /// <summary>
        /// Prints out the rule set in a readable fashion.
        /// </summary>
        public void PrintRuleSetForHumans(IEvolutionState state, int log)
        {
            state.Output.PrintLn("Ruleset contains " + _ruleCount + " rules", log);
            for (var i = 0; i < _ruleCount; i++)
            {
                state.Output.PrintLn("Rule " + i + ":", log);
                _rules[i].PrintRuleForHumans(state, log);
            }
        }

        /// <summary>
        /// Prints the rule set such that the computer can read it later.
        /// </summary>
        public void PrintRuleSet(IEvolutionState state, int log)
        {
            state.Output.PrintLn(N_RULES + Code.Encode(_ruleCount), log);
            for (var i = 0; i < _ruleCount; i++)
                _rules[i].PrintRule(state, log);
        }

        /// <summary>
        /// Prints the rule set such that the computer can read it later.
        /// </summary>
        public virtual void  PrintRuleSet(IEvolutionState state, StreamWriter writer)
        {
            writer.WriteLine(N_RULES + Code.Encode(_ruleCount));
            for (var i = 0; i < _ruleCount; i++)
                _rules[i].PrintRule(state, writer);
        }
        
        /// <summary>
        /// Reads the rule set.
        /// </summary>
        public virtual void ReadRuleSet(IEvolutionState state, StreamReader reader)
        {
            _ruleCount = Code.ReadIntWithPreamble(N_RULES, state, reader);
            
            _rules = new Rule[_ruleCount];
            for (var x = 0; x < _ruleCount; x++)
            {
                _rules[x] = (Rule) (Constraints(((RuleInitializer) state.Initializer)).RulePrototype.Clone());
                _rules[x].ReadRule(state, reader);
            }
        }
        
        /// <summary>
        /// Writes RuleSets out to a binary stream. 
        /// </summary>
        public virtual void WriteRuleSet(IEvolutionState state, BinaryWriter dataOutput)
        {
            dataOutput.Write(_ruleCount);
            for (var x = 0; x < _ruleCount; x++)
                _rules[x].WriteRule(state, dataOutput);
        }
        
        /// <summary>
        /// Reads RuleSets in from a binary stream. 
        /// </summary>
        public virtual void ReadRuleSet(IEvolutionState state, BinaryReader dataInput)
        {
            var ruleCount = dataInput.ReadInt32();
            if (_rules == null || _rules.Length != ruleCount)
                _rules = new Rule[ruleCount];
            for (var x = 0; x < ruleCount; x++)
            {
                _rules[x] = (Rule) (Constraints((RuleInitializer) state.Initializer).RulePrototype.Clone());
                _rules[x].ReadRule(state, dataInput);
            }
        }

        #endregion // IO
    }
}