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

using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Rule
{
    /// <summary>
    /// A SimpleInitializer subclass designed to be used with rules.  Basically,
    /// the RuleInitializer sets up the RuleConstraints and RuleSetConstraints cliques
    /// at Setup() time, and does nothing else different from SimpleInitializer. 
    /// The RuleInitializer also specifies the parameter bases for the RuleSetConstraints
    /// and RuleConstraints objects.  
    /// 
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><tt>rule.rsc</tt></td>
    /// <td>RuleSetConstraints</td></tr>
    /// <tr><td valign="top"><tt>rule.rc</tt></td>
    /// <td>RuleConstraints</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.rule.RuleInitializer")]
    public class RuleInitializer : SimpleInitializer
    {
        #region Constants

        private const long SerialVersionUID = 1;

        // BRS : TODO : Change all the sbytes to bytes (conversion was easier with sbyte)
        // used just here, so far as I know :-)
        public const int SIZE_OF_BYTE = 256;
        public const string P_RULESETCONSTRAINTS = "rsc";
        public const string P_RULECONSTRAINTS = "rc";
        public const string P_SIZE = "size";

        #endregion // Constants
        #region Properties

        public Hashtable RuleConstraintRepository { get; set; }
        public RuleConstraints[] RuleConstraints { get; set; }
        public sbyte NumRuleConstraints { get; set; }

        public Hashtable RuleSetConstraintRepository { get; set; }
        public RuleSetConstraints[] RuleSetConstraints { get; set; }
        public sbyte NumRuleSetConstraints { get; set; }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// Sets up the RuleConstraints and RuleSetConstraints cliques. 
        /// </summary>
        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            RuleConstraintRepository = Hashtable.Synchronized(new Hashtable());
            RuleConstraints = new RuleConstraints[SIZE_OF_BYTE];
            NumRuleConstraints = 0;

            RuleSetConstraintRepository = Hashtable.Synchronized(new Hashtable());
            RuleSetConstraints = new RuleSetConstraints[SIZE_OF_BYTE];
            NumRuleSetConstraints = 0;

            // Now let's load our constraints and function sets also.
            // This is done in a very specific order, don't change it or things
            // will break.
            SetupConstraints(state, RuleDefaults.ParamBase.Push(P_RULECONSTRAINTS));
            SetupRuleSetConstraints(state, RuleDefaults.ParamBase.Push(P_RULESETCONSTRAINTS));
        }

        /// <summary>
        /// Sets up all the RuleConstraints, loading them from the parameter
        /// file.  This must be called before anything is called which refers
        /// to a type by name. 
        /// </summary>		
        public virtual void SetupConstraints(IEvolutionState state, IParameter paramBase)
        {
            state.Output.Message("Processing Rule Constraints");

            // How many RuleConstraints do we have?
            var x = state.Parameters.GetInt(paramBase.Push(P_SIZE), null, 1);
            if (x <= 0)
                state.Output.Fatal("The number of rule constraints must be at least 1.", paramBase.Push(P_SIZE));

            // Load our constraints
            for (var y = 0; y < x; y++)
            {
                RuleConstraints c;
                // Figure the constraints class
                if (state.Parameters.ParameterExists(paramBase.Push("" + y), null))
                    c = (RuleConstraints)(state.Parameters.GetInstanceForParameterEq(paramBase.Push("" + y), null, typeof(RuleConstraints)));
                else
                {
                    state.Output.Message("No Rule Constraints specified, assuming the default class: ec.rule.RuleConstraints for " + paramBase.Push("" + y));
                    c = new RuleConstraints();
                }
                c.Setup(state, paramBase.Push("" + y));
            }

            // set our constraints array up
            var e = RuleConstraintRepository.Values.GetEnumerator();
            while (e.MoveNext())
            {
                var c = (RuleConstraints)(e.Current);
                c.ConstraintCount = NumRuleConstraints;
                RuleConstraints[NumRuleConstraints] = c;
                NumRuleConstraints++;
            }
        }

        public virtual void SetupRuleSetConstraints(IEvolutionState state, IParameter paramBase)
        {
            state.Output.Message("Processing Ruleset Constraints");
            // How many RuleSetConstraints do we have?
            var x = state.Parameters.GetInt(paramBase.Push(P_SIZE), null, 1);
            if (x <= 0)
                state.Output.Fatal("The number of RuleSetConstraints must be at least 1.", paramBase.Push(P_SIZE));

            // Load our RuleSetConstraints
            for (var y = 0; y < x; y++)
            {
                RuleSetConstraints c;
                // Figure the RuleSetConstraints class
                if (state.Parameters.ParameterExists(paramBase.Push("" + y), null))
                    c = (RuleSetConstraints)(state.Parameters.GetInstanceForParameterEq(paramBase.Push("" + y), null, typeof(RuleSetConstraints)));
                else
                {
                    state.Output.Message("No RuleSetConstraints specified, assuming default class: ec.gp.RuleSetConstraints for " + paramBase.Push("" + y));
                    c = new RuleSetConstraints();
                }
                c.Setup(state, paramBase.Push("" + y));
            }
            // set our constraints array up
            foreach (RuleSetConstraints c in RuleSetConstraintRepository)
            {
                c.ConstraintCount = NumRuleSetConstraints;
                RuleSetConstraints[NumRuleSetConstraints] = c;
                NumRuleSetConstraints++;
            }
        }

        #endregion // Setup
    }
}