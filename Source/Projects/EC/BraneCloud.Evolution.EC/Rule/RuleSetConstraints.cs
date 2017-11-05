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
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.Rule
{
    /// <summary> 
    /// RuleSetConstraints is an basic class for constraints applicable to rulesets.
    /// There are two categories of parameters associated with this class.  First, there are parameters
    /// which guide the initial number of rules to be created when a ruleset is initialized for
    /// the first time, or totally reset.  Second, there are parameters which indicate how rulesets
    /// are to be mutated under the "default" rule mutation operator.
    /// 
    /// <p/>First the initialization parameters.  You need to specify a distribution from which
    /// the system will pick random integer values X.  When a ruleset is to be initialized, a
    /// random value X is picked from this distribution, and the ruleset will be created with X initial rules.
    /// You can specify the distribution in one of two ways.  First, you can specify a minimum and maximum
    /// number of rules; the system will then pick an X uniformly from between the min and the max. 
    /// Second, you can specify a full distribution of size probabilities for more control.  For example,
    /// to specify that the system should make individuals with 0 rules 0.1 of the time, 1 rule 0.2 of the time,
    /// and 2 rules 0.7 of the time, you set <i>reset-num-sizes</i> to 3 (for rule sizes up to but not including 3),
    /// and then set  reset-size.0 to 0.1, reset-size.1 to 0.2, and reset-size.2 to 0.7.
    /// 
    /// <p/>Next the mutation parameters.  The default mutation procedure works as follows.  First, every rule
    /// in the ruleset is mutated.  It is up to the rule to determine by how much or whether it will be mutated
    /// (perhaps by flipping a coin) when its mutate function is called.  Second, the system repeatedly flips
    /// a coin with "p-del" probability of being true, until it comes up false.  The number of times it came up
    /// true is the number of rules to remove from the ruleset; rules to be removed are chosen at random.
    /// Third, the system repeatedly flips
    /// a coin with "p-add" probability of being true, until it comes up false.  The number of times it came up
    /// true is the number of new randomly-generated rules to add to the ruleset; rules are added to the end of the array.
    /// Fourth, with "p-rand-order" probability, the order of rules in the ruleset array is randomized; this last
    /// item might or might not matter to you depending on whether or not your rule interpreter differs depending on rule order.  
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>size</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(number of rule set constraints)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>Name</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(name of rule set constraint <i>n</i>)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>reset-min-size</tt><br/>
    /// <font size="-1">int >= 0 (default=0)</font></td>
    /// <td valign="top">(for rule set constraint <i>n</i>, the minimum number of rules that rulesets may contain upon initialization (resetting), see discussion above)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>reset-max-size</tt><br/>
    /// <font size="-1">int >= <i>base.n</i>.<tt>reset-min-size</tt> (default=0)</font></td>
    /// <td valign="top">(for rule set constraint <i>n</i>, the maximum number of rules that rulesets may contain upon initialization (resetting), see discussion above)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>reset-num-sizes</tt><br/>
    /// <font size="-1">int >= 0</font> (default=unset)</td>
    /// <td valign="top">(for rule set constraint <i>n</i>, the number of sizes in the size distribution for initializtion, see discussion above)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>reset-size</tt>.<i>i</i><br/>
    /// <font size="-1">0.0 &lt;= double &lt;= 1.0</font></td>
    /// <td valign="top">(for rule set constraint <i>n</i>, the probability that <i>i</i> will be chosen as the number of rules upon initialization, see discussion above)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>p-add</tt><br/>
    /// <font size="-1">0.0 &lt;= double &lt;= 1.0</font></td>
    /// <td valign="top">(the probability that a new rule will be added, see discussion)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>p-del</tt><br/>
    /// <font size="-1">0.0 &lt;= double &lt;= 1.0</font></td>
    /// <td valign="top">(the probability that a rule will be deleted, see discussion)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>p-rand-order</tt><br/>
    /// <font size="-1">0.0 &lt;= double &lt;= 1.0</font></td>
    /// <td valign="top">(the probability that the rules' order will be randomized, see discussion)</td></tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.rule.RuleSetConstraints")]
    public class RuleSetConstraints : IClique
    {
        #region Constants

        private const long SerialVersionUID = 1;

        /// <summary>The size of a byte </summary>
        //public const int SIZE_OF_BYTE = 256;

        public const string P_NAME = "name";

        public const string P_RULE = "rule"; // our prototype
        public const string P_RESETMINSIZE = "reset-min-size";
        public const string P_RESETMAXSIZE = "reset-max-size";
        public const string P_NUMSIZES = "reset-num-sizes";
        public const string P_RESETSIZE = "reset-size";
        
        public const string P_MINSIZE = "min-size";
        public const string P_MAXSIZE = "max-size";

        /// <summary>
        /// Probability of adding a random rule to the rule set
        /// </summary>
        public const string P_ADD_PROB = "p-add";
 
        /// <summary>
        /// Probability of removing a random rule from the rule set
        /// </summary>
        public const string P_DEL_PROB = "p-del";

        /// <summary>
        /// Probability of randomizing the rule order in the rule set
        /// </summary>
        public const string P_RAND_ORDER_PROB = "p-rand-order";

        #endregion // Constants
        #region Properties

        /// <summary>
        /// the minimum legal size
        /// </summary>
        public int MinSize { get; set; }

        /// <summary>
        /// the maximum legal size
        /// </summary>
        public int MaxSize { get; set; }

        /// <summary>
        /// The minium possible size -- if unused, it's 0, but 0 is also a valid number, so check SizeDistribution == null
        /// </summary>
        public int ResetMinSize { get; set; }

        /// <summary>
        /// The maximum possible size -- if unused, it's 0, but 0 is also a valid number, so check SizeDistribution==null
        /// </summary>
        public int ResetMaxSize { get; set; }

        public double[] SizeDistribution { get; set; }

        /// <summary>
        /// Probability of adding a random rule to the rule set
        /// </summary>
        public double p_add { get; set; }

        /// <summary>
        /// Probability of removing a random rule from the rule set
        /// </summary>
        public double p_del { get; set; }

        /// <summary>
        /// Probability of randomizing the rule order in the rule set
        /// </summary>
        public double p_randorder { get; set; }

        /// <summary>
        /// The prototype of the Rule that will be used in the RuleSet
        /// (the RuleSet contains only rules with the specified prototype).
        /// </summary>
        public Rule RulePrototype { get; set; }

        /// <summary>
        /// The byte value of the constraints -- we can only have 256 of them. 
        /// </summary>
        public int ConstraintCount { get; set; }

        /// <summary>
        /// The name of the RuleSetConstraints object. 
        /// </summary>
        public string Name { get; set; }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// Sets up all the RuleSetConstraints, loading them from the parameter
        /// file.  This must be called before anything is called which refers
        /// to a type by Name. 
        /// </summary>
        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            // What's my name?
            Name = state.Parameters.GetString(paramBase.Push(P_NAME), null);
            if (Name == null)
                state.Output.Fatal("No name was given for this RuleSetConstraints.", paramBase.Push(P_NAME));

            // Register me
            var tempObject = ((RuleInitializer)state.Initializer).RuleSetConstraintRepository[Name];
            ((RuleInitializer)state.Initializer).RuleSetConstraintRepository[Name] = this;
            var oldConstraints = (RuleSetConstraints)(tempObject);
            if (oldConstraints != null)
                state.Output.Fatal("The rule constraints \"" + Name + "\" has been defined multiple times.", paramBase.Push(P_NAME));

            // load my prototypical Rule
            RulePrototype = (Rule)(state.Parameters.GetInstanceForParameter(paramBase.Push(P_RULE), null, typeof(Rule)));
            RulePrototype.Setup(state, paramBase.Push(P_RULE));

            p_add = state.Parameters.GetDouble(paramBase.Push(P_ADD_PROB), null, 0);
            if (p_add < 0 || p_add > 1)
            {
                state.Output.Fatal("Parameter not found, or its value is outside of allowed range [0..1].", paramBase.Push(P_ADD_PROB));
            }
            p_del = state.Parameters.GetDouble(paramBase.Push(P_DEL_PROB), null, 0);
            if (p_del < 0 || p_del > 1)
            {
                state.Output.Fatal("Parameter not found, or its value is outside of allowed range [0..1].", paramBase.Push(P_DEL_PROB));
            }

            p_randorder = state.Parameters.GetDouble(paramBase.Push(P_RAND_ORDER_PROB), null, 0);
            if (p_randorder < 0 || p_randorder > 1)
            {
                state.Output.Fatal("Parameter not found, or its value is outside of allowed range [0..1].", paramBase.Push(P_RAND_ORDER_PROB));
            }

            // now, we are going to load EITHER min/max size OR a size distribution, or both
            // (the size distribution takes precedence)

            // reset min and max size

            if (state.Parameters.ParameterExists(paramBase.Push(P_RESETMINSIZE), null) ||
                state.Parameters.ParameterExists(paramBase.Push(P_RESETMAXSIZE), null))
            {
                if (!(state.Parameters.ParameterExists(paramBase.Push(P_RESETMAXSIZE), null)))
                    state.Output.Error("This RuleSetConstraints has a " + P_RESETMINSIZE + " but not a " + P_RESETMAXSIZE + ".");

                ResetMinSize = state.Parameters.GetInt(paramBase.Push(P_RESETMINSIZE), null, 0);
                if (ResetMinSize == -1)
                    state.Output.Error("If min&max are defined, RuleSetConstraints must have a min size >= 0.", paramBase.Push(P_RESETMINSIZE), null);

                ResetMaxSize = state.Parameters.GetInt(paramBase.Push(P_RESETMAXSIZE), null, 0);
                if (ResetMaxSize == -1)
                    state.Output.Error("If min&max are defined, RuleSetConstraints must have a max size >= 0.", paramBase.Push(P_RESETMAXSIZE), null);

                if (ResetMinSize > ResetMaxSize)
                    state.Output.Error("If min&max are defined, RuleSetConstraints must have min size <= max size.", paramBase.Push(P_RESETMINSIZE), null);
                state.Output.ExitIfErrors();
            }

            // load SizeDistribution

            if (state.Parameters.ParameterExists(paramBase.Push(P_NUMSIZES), null))
            {
                var siz = state.Parameters.GetInt(paramBase.Push(P_NUMSIZES), null, 1);
                if (siz == 0)
                    state.Output.Fatal("The number of sizes in the RuleSetConstraints's distribution must be >= 1. ");
                SizeDistribution = new double[siz];

                var sum = 0.0;
                for (var x = 0; x < siz; x++)
                {
                    SizeDistribution[x] = state.Parameters.GetDouble(paramBase.Push(P_RESETSIZE).Push("" + x), null, 0.0);
                    if (SizeDistribution[x] < 0.0)
                    {
                        state.Output.Warning("Distribution value #" + x + " negative or not defined, assumed to be 0.0",
                            paramBase.Push(P_RESETSIZE).Push("" + x), null);
                        SizeDistribution[x] = 0.0f;
                    }
                    sum += SizeDistribution[x];
                }
                if (sum > 1.0)
                    state.Output.Warning("Distribution sums to greater than 1.0", paramBase.Push(P_RESETSIZE), null);
                if (sum == 0.0)
                    state.Output.Fatal("Distribution is all 0's", paramBase.Push(P_RESETSIZE), null);

                // normalize and prepare
                RandomChoice.OrganizeDistribution(SizeDistribution);
            }

            MinSize = state.Parameters.ParameterExists(paramBase.Push(P_MINSIZE), null)
                ? state.Parameters.GetInt(paramBase.Push(P_MINSIZE), null, 0) : 0;

            MaxSize = state.Parameters.ParameterExists(paramBase.Push(P_MAXSIZE), null)
                ? state.Parameters.GetInt(paramBase.Push(P_MAXSIZE), null, 0) : int.MaxValue;

            // sanity checks
            if (MinSize > MaxSize)
            {
                state.Output.Fatal("Cannot have min size greater than max size : (" + MinSize + " > " + MaxSize + ")", paramBase.Push(P_MINSIZE), null);
            }

            if (SizeDistribution != null)
            {
                if (MinSize != 0)
                    state.Output.Fatal("Using size distribution, but min size is not 0", paramBase.Push(P_MINSIZE), null);
                if (SizeDistribution.Length - 1 > MaxSize)
                    state.Output.Fatal("Using size distribution whose maximum size is higher than max size", paramBase.Push(P_MAXSIZE), null);
            }
            else
            {
                if (ResetMinSize < MinSize)
                    state.Output.Fatal("Cannot have min size greater than reset min size : ("
                        + MinSize + " > " + ResetMinSize + ")", paramBase.Push(P_MINSIZE), null);

                if (ResetMaxSize > MaxSize)
                    state.Output.Fatal("Cannot have max size less than reset max size : ("
                        + MaxSize + " > " + ResetMaxSize + ")", paramBase.Push(P_MAXSIZE), null);
            }
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Assuming that either ResetMinSize and ResetMaxSize, or SizeDistribution, is defined,
        /// picks a random size from ResetMinSize...ResetMaxSize inclusive, or randomly
        /// from SizeDistribution. 
        /// </summary>
        public virtual int PickSize(IEvolutionState state, int thread)
        {
            if (SizeDistribution != null)
            // pick from distribution
                return RandomChoice.PickFromDistribution(SizeDistribution, state.Random[thread].NextDouble());

            // pick from ResetMinSize...ResetMaxSize
            return state.Random[thread].NextInt(ResetMaxSize - ResetMinSize + 1) + ResetMinSize;
        }

        /// <summary>
        /// Returns a stochastic value picked to specify the number of rules
        /// to generate when calling Reset() on this kind of Rule.  The default
        /// version picks from the min/max or distribution, but you can override
        /// this to do whatever kind of thing you like here.
        /// </summary>
        public virtual int NumRulesForReset(RuleSet ruleset, IEvolutionState state, int thread)
        {
            // the default just uses pickSize
            return PickSize(state, thread);
        }

        /// <summary>
        /// You must guarantee that after calling constraintsFor(...) one or
        /// several times, you call state.Output.ExitIfErrors() once. 
        /// </summary>		
        public static RuleSetConstraints ConstraintsFor(string constraintsName, IEvolutionState state)
        {
            var myConstraints = (RuleSetConstraints)(((RuleInitializer)state.Initializer).RuleSetConstraintRepository[constraintsName]);
            if (myConstraints == null)
                state.Output.Error("The rule constraints \"" + constraintsName + "\" could not be found.");
            return myConstraints;
        }

        #endregion // Operations
        #region IO

        /// <summary>
        /// Converting the rule to a string ( the Name ). 
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        #endregion // IO
    }
}