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

namespace BraneCloud.Evolution.EC.Rule
{
    /// <summary> 
    /// RuleConstraints is a class for constraints applicable to rules.
    /// You can subclass this to add additional constraints information
    /// for different kinds of rules.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>size</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(number of rule constraints)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>name</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(name of rule constraint <i>n</i>)</td></tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.rule.RuleConstraints")]
    public class RuleConstraints : IClique
    {
        #region Constants

        public const int SIZE_OF_BYTE = 256;
        public const string P_NAME = "name";
        public const string P_SIZE = "size";

        #endregion // Constants
        #region Static

        /// <summary>
        /// You must guarantee that after calling constraintsFor(...) one or
        /// several times, you call state.Output.ExitIfErrors() once. 
        /// </summary>
        public static RuleConstraints ConstraintsFor(string constraintsName, IEvolutionState state)
        {
            var myConstraints = (RuleConstraints)(((RuleInitializer)state.Initializer).RuleConstraintRepository[constraintsName]);
            if (myConstraints == null)
                state.Output.Error("The rule constraints \"" + constraintsName + "\" could not be found.");
            return myConstraints;
        }

        #endregion // Static
        #region Properties

        /// <summary>
        /// The name of the RuleConstraints object 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The byte value of the constraints -- we can only have 256 of them 
        /// </summary>
        public int ConstraintCount { get; set; }

        #endregion // Properties
        #region Setup

        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            // What's my name?
            Name = state.Parameters.GetString(paramBase.Push(P_NAME), null);
            if (Name == null)
                state.Output.Fatal("No name was given for this Rule Constraints.", paramBase.Push(P_NAME));

            // Register me
            if (String.IsNullOrEmpty(Name)) throw new InvalidOperationException("RuleConstraints Name cannot be null or empty.");
            var tempObject = ((RuleInitializer)state.Initializer).RuleConstraintRepository[Name];
            ((RuleInitializer)state.Initializer).RuleConstraintRepository[Name] = this;
            var oldConstraints = (RuleConstraints)(tempObject);
            if (oldConstraints != null)
                state.Output.Fatal("The rule constraints \"" + Name + "\" has been defined multiple times.", paramBase.Push(P_NAME));
        }

        #endregion // Setup
        #region ToString

        /// <summary>
        /// Converting the rule to a string ( the name ) 
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        #endregion // ToString
    }
}