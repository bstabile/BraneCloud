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
    /// A GPNodeConstraints is a IClique which defines constraint information
    /// common to many different GPNode functions, namely return types,
    /// child types, and number of children. 
    /// GPNodeConstraints have unique names by which
    /// they are identified.
    /// 
    /// <p/>In adding new things to GPNodeConstraints, you should ask yourself
    /// the following questions: first, is this something that takes up too
    /// much memory to store in GPNodes themselves?  second, is this something
    /// that needs to be accessed very rapidly, so cannot be implemented
    /// as a method call in a GPNode?  third, can this be shared among
    /// different GPNodes, even ones representing different functions?
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>size</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(number of node constraints)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>name</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(name of node constraint <i>n</i>)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>returns</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(return type for node constraint <i>n</i>)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>size</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(number of child arguments for node constraint <i>n</i>)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>child.</tt><i>m</i><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(name of type for child argument <i>m</i> of node constraint <i>n</i>)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>prob</tt><br/>
    /// <font size="-1">double &gt;= 0.0</font></td>
    /// <td valign="top">(auxillary probability of selection -- used by ec.gp.build.PTC1 and ec.gp.build.PTC2)</td></tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.GPNodeConstraints")]
    public class GPNodeConstraints : IClique
    {
        #region Constants

        public const int SIZE_OF_BYTE = 256;
        public const string P_NAME = "name";
        public const string P_RETURNS = "returns";
        public const string P_CHILD = "child";
        public const string P_SIZE = "size";
        public const string P_PROBABILITY = "prob";
        public const double DEFAULT_PROBABILITY = 1.0;

        #endregion // Constants
        #region Static

        /// <summary>
        /// You must guarantee that after calling constraintsFor(...) one or
        /// several times, you call state.Output.ExitIfErrors() once. 
        /// </summary>		
        public static GPNodeConstraints ConstraintsFor(string constraintsName, IEvolutionState state)
        {
            var myConstraints = (GPNodeConstraints)(((GPInitializer)state.Initializer).NodeConstraintRepository[constraintsName]);
            if (myConstraints == null)
                state.Output.Error("The GP node constraint \"" + constraintsName + "\" could not be found.");
            return myConstraints;
        }

        #endregion // Static
        #region Properties

        /// <summary>
        /// Probability of selection -- an auxillary measure mostly used by PTC1/PTC2 right now 
        /// </summary>
        public double ProbabilityOfSelection { get; set; }

        /// <summary>
        /// The index of the constraints
        /// </summary>
        public int ConstraintIndex { get; set; }

        /// <summary>
        /// The return type for a GPNode 
        /// </summary>
        public GPType ReturnType { get; set; }

        /// <summary>
        /// The children types for a GPNode 
        /// </summary>
        public GPType[] ChildTypes { get; set; }

        /// <summary>
        /// The name of the GPNodeConstraints object -- this is NOT the name of the GPNode 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A little memory optimization: if GPNodes have no children, they are welcome to
        /// use share this zero-sized array as their children array.
        /// </summary>
        public GPNode[] ZeroChildren
        {
            get => _zeroChildren;
            set => _zeroChildren = value;
        }
        private GPNode[] _zeroChildren = new GPNode[0];

        #endregion // Properties
        #region Setup

        /// <summary>
        /// This must be called <i>after</i> the GPTypes have been set up. 
        /// </summary>
        public void Setup(IEvolutionState state, IParameter paramBase)
        {
            // What's my name?
            Name = state.Parameters.GetString(paramBase.Push(P_NAME), null);
            if (Name == null)
                state.Output.Fatal("No name was given for this node constraints.", paramBase.Push(P_NAME));

            // Register me
            var tempObject = ((GPInitializer)state.Initializer).NodeConstraintRepository[Name];
            ((GPInitializer)state.Initializer).NodeConstraintRepository[Name] = this;
            var oldConstraints = (GPNodeConstraints)(tempObject);
            if (oldConstraints != null)
                state.Output.Fatal("The GP node constraint \"" + Name + "\" has been defined multiple times.", paramBase.Push(P_NAME));

            // What's my return type?
            var s = state.Parameters.GetString(paramBase.Push(P_RETURNS), null);
            if (s == null)
                state.Output.Fatal("No return type given for the GPNodeConstraints " + Name, paramBase.Push(P_RETURNS));
            ReturnType = GPType.TypeFor(s, state);

            // Load probability of selection

            if (state.Parameters.ParameterExists(paramBase.Push(P_PROBABILITY), null))
            {
                var f = state.Parameters.GetDouble(paramBase.Push(P_PROBABILITY), null, 0);
                if (f < 0)
                    state.Output.Fatal("The probability of selection is < 0, which is not valid.", paramBase.Push(P_PROBABILITY), null);
                ProbabilityOfSelection = f;
            }
            else
                ProbabilityOfSelection = DEFAULT_PROBABILITY;

            // How many child types do I have?

            var x = state.Parameters.GetInt(paramBase.Push(P_SIZE), null, 0);
            if (x < 0)
                state.Output.Fatal("The number of children types for the GPNodeConstraints " + Name + " must be >= 0.", paramBase.Push(P_SIZE));

            ChildTypes = new GPType[x];

            var p = paramBase.Push(P_CHILD);

            // Load my children
            for (x = 0; x < ChildTypes.Length; x++)
            {
                s = state.Parameters.GetString(p.Push("" + x), null);
                if (s == null)
                    state.Output.Fatal("Type #" + x + " is not defined for the GPNodeConstraints " + Name + ".", paramBase.Push("" + x));
                ChildTypes[x] = GPType.TypeFor(s, state);
            }
            // ...because I promised when I called typeFor(...)
            state.Output.ExitIfErrors();
        }

        #endregion // Setup
        #region ToString

        public override string ToString()
        {
            return Name;
        }

        #endregion // ToString
    }
}