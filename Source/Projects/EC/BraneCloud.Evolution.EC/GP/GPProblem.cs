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

namespace BraneCloud.Evolution.EC.GP
{	
    /// <summary> 
    /// A GPProblem is a Problem which is meant to efficiently handle GP
    /// evaluation.  GPProblems hold one ADFStack, which is used to 
    /// evaluate a large number of trees without having to be garbage-collected
    /// and reallocated.  Be sure to call Stack.Reset() after each
    /// tree evaluation.
    /// 
    /// <p/>GPProblem also provides a default (empty) version of Describe(...) for
    /// SimpleProblemForm so you don't have to bother with it if you don't want to.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i><tt>.Stack</tt><br/>
    /// <font size="-1">classname, inherits or = ec.ADFStack</font></td>
    /// <td valign="top">(the class for the GPProblem's ADF Stack)</td></tr>
    /// <tr><td valign="top"><i>base</i><tt>.Data</tt><br/>
    /// <font size="-1">classname, inherits and != ec.GPData</font></td>
    /// <td valign="top">(the class for the GPProblem's basic GPData type)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// gp.problem
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"/><i>base</i><tt>.Stack</tt><br/>
    /// <td valign="top">(Stack)</td></tr> 
    /// <tr><td valign="top"/><i>base</i><tt>.Data</tt><br/>
    /// <td valign="top"/>(Data)</tr> 
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.GPProblem")]
    public abstract class GPProblem : Problem, IGPProblem
    {
        #region Constants

        public const string P_GPPROBLEM = "problem";
        public const string P_STACK = "stack";
        public const string P_DATA = "data";

        #endregion // Constants
        #region Properties

        /// <summary>
        /// GPProblem defines a default base so your subclass doesn't absolutely have to. 
        /// </summary>
        public override IParameter DefaultBase
        {
            get { return GPDefaults.ParamBase.Push(P_GPPROBLEM); }
        }

        /// <summary>
        /// The GPProblem's Stack 
        /// </summary>
        public ADFStack Stack { get; set; }

        /// <summary>
        /// The GPProblems' GPData 
        /// </summary>
        public GPData Data { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            var def = DefaultBase;

            // BRS: MUST set up GPData before ADFStack, so that ADFContext has access to state.Evaluator.p_problem.Data
            // This avoids a reference to GPProblem.P_DATA, and thus allows us to use interfaces instead of concrete types
            // Important for decoupling and Inversion of Control (IoC).
            var p = paramBase.Push(P_DATA);
            Data = (GPData)(state.Parameters.GetInstanceForParameter(p, def.Push(P_DATA), typeof(GPData)));
            Data.Setup(state, p);

            p = paramBase.Push(P_STACK);
            Stack = (ADFStack)(state.Parameters.GetInstanceForParameterEq(p, def.Push(P_STACK), typeof(ADFStack)));
            Stack.Setup(state, p);

        }

        #endregion // Setup
        #region Cloning

        public override object Clone()
        {
            var prob = (GPProblem) (base.Clone());
            
            // deep-clone the Stack; it's not shared
            prob.Stack = (ADFStack) (Stack.Clone());
            return prob;
        }

        #endregion // Cloning
    }
}