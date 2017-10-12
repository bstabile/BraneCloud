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

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.GP
{
    /// <summary> 
    /// A GPTreeConstraints is a IClique which defines constraint information
    /// common to many different GPTree trees, namely the tree type,
    /// builder, and function set.  GPTreeConstraints have unique names
    /// by which they are identified.
    /// 
    /// <p/>In adding new things to GPTreeConstraints, you should ask yourself
    /// the following questions: 
    /// 
    /// first, is this something that takes up too much memory to store in GPTrees themseves?  
    /// second, is this something that needs to be accessed very rapidly, so cannot be implemented as
    /// a method call in a GPTree?  
    /// third, can this be shared among different GPTrees?
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>size</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(number of tree constraints)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>name</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(name of tree constraint <i>n</i>)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>init</tt><br/>
    /// <font size="-1">classname, inherits and != ec.gp.GPNodeBuilder</font></td>
    /// <td valign="top">(GP node builder for tree constraint <i>n</i>)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>returns</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(tree type for tree constraint <i>n</i>)</td></tr>
    /// <tr><td valign="top"><i>base.n</i>.<tt>fset</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(function set for tree constraint <i>n</i>)</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.GPTreeConstraints")]
    public class GPTreeConstraints : IClique
    {
        #region Constants

        public const int SIZE_OF_BYTE = 256;
        public const string P_NAME = "name";
        public const string P_SIZE = "size";
        public const string P_INIT = "init";
        public const string P_RETURNS = "returns";
        public const string P_FUNCTIONSET = "fset";

        #endregion // Constants
        #region Static

        /// <summary>
        /// You must guarantee that after calling ConstraintsFor(...) one or
        /// several times, you call state.Output.ExitIfErrors() once. 
        /// </summary>		
        public static GPTreeConstraints ConstraintsFor(string constraintsName, IEvolutionState state)
        {
            var myConstraints = (GPTreeConstraints)(((GPInitializer)state.Initializer).TreeConstraintRepository[constraintsName]);
            if (myConstraints == null)
                state.Output.Error("The GP tree constraint \"" + constraintsName + "\" could not be found.");
            return myConstraints;
        }

        #endregion // Static
        #region Properties

        public string Name { get; set; }

        /// <summary>
        /// The index value of the constraints. 
        /// This is used to locate the constraints within the GPInitializer.TreeConstraints array.
        /// </summary>
        public int ConstraintsIndex { get; set; }

        /// <summary>
        /// The builder for the tree. 
        /// </summary>
        public GPNodeBuilder Init { get; set; }

        /// <summary>
        /// The type of the root of the tree. 
        /// </summary>
        public GPType TreeType { get; set; }

        /// <summary>
        /// The function set for nodes in the tree 
        /// </summary>
        public GPFunctionSet FunctionSet { get; set; }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// This must be called <i>after</i> the GPTypes and GPFunctionSets 
        /// have been set up. 
        /// </summary>
        public void Setup(IEvolutionState state, IParameter paramBase)
        {
            // What's my name?
            Name = state.Parameters.GetString(paramBase.Push(P_NAME), null);
            if (Name == null)
                state.Output.Fatal("No name was given for this function set.", paramBase.Push(P_NAME));

            // Register me
            var tempObject = ((GPInitializer)state.Initializer).TreeConstraintRepository[Name];
            ((GPInitializer)state.Initializer).TreeConstraintRepository[Name] = this;

            var oldConstraints = (GPTreeConstraints)(tempObject);
            if (oldConstraints != null)
                state.Output.Fatal("The GP tree constraint \"" + Name + "\" has been defined multiple times.", paramBase.Push(P_NAME));

            // Load my initializing builder
            Init = (GPNodeBuilder)(state.Parameters.GetInstanceForParameter(paramBase.Push(P_INIT), null, typeof(GPNodeBuilder)));
            Init.Setup(state, paramBase.Push(P_INIT));

            // Load my return type
            var s = state.Parameters.GetString(paramBase.Push(P_RETURNS), null);
            if (s == null)
                state.Output.Fatal("No return type given for the GPTreeConstraints " + Name, paramBase.Push(P_RETURNS));

            TreeType = GPType.TypeFor(s, state);

            // Load my function set
            s = state.Parameters.GetString(paramBase.Push(P_FUNCTIONSET), null);
            if (s == null)
                state.Output.Fatal("No function set given for the GPTreeConstraints " + Name, paramBase.Push(P_RETURNS));

            FunctionSet = GPFunctionSet.FunctionSetFor(s, state);
            state.Output.ExitIfErrors(); // otherwise checkFunctionSetValidity might crash below

            // Determine the validity of the function set
            // the way we do that is by gathering all the types that
            // are transitively used, starting with treetype, as in:
            var typ = Hashtable.Synchronized(new Hashtable());
            CheckFunctionSetValidity(state, typ, TreeType);

            // next we make sure that for every one of these types,
            // there's a terminal with that return type, and *maybe*
            // a nonterminal
            var e = typ.Values.GetEnumerator();
            while (e.MoveNext())
            {
                var t = (GPType)(e.Current);
                var i = FunctionSet.Nodes[t.Type];
                if (i.Length == 0) // yeesh
                {
                    state.Output.Error("In function set " + FunctionSet + " for the GPTreeConstraints "
                        + this + ", no nodes at all are given with the return type "
                        + t + " which is required by other functions in the function set or by the tree's return type."
                        + " This almost certainly indicates a serious typing error.", paramBase);
                }
                else
                {
                    i = FunctionSet.Terminals[t.Type];
                    if (i.Length == 0)
                    // uh oh
                    {
                        state.Output.Warning("In function set " + FunctionSet + " for the GPTreeConstraints "
                            + this + ", no terminals are given with the return type "
                            + t + " which is required by other functions in the function set or by the tree's return type."
                            + " Nearly all tree-builders in ECJ require the ability to add a terminal of any type for which "
                            + " there is a nonterminal, and at any time.  Without terminals, your code may not work."
                            + " One common indication that a tree-builder has failed due to this problem is if you get"
                            + " the MersenneTwister error 'n must be positive'.", paramBase);
                    }
                    i = FunctionSet.Nonterminals[t.Type];
                    if (i.Length == 0)
                    // uh oh
                    {
                        state.Output.Warning("In function set " + FunctionSet + " for the GPTreeConstraints "
                            + this + ", no *nonterminals* are given with the return type "
                            + t + " which is required by other functions in the function set or by the tree's return type."
                            + " This may or may not be a problem for you.", paramBase);
                    }
                }
            }
            state.Output.ExitIfErrors();
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// When completed, done will hold all the types which are needed
        /// in the function set -- you can then check to make sure that
        /// they contain at least one terminal and (hopefully) at least
        /// one nonterminal.
        /// </summary>
        private void CheckFunctionSetValidity(IEvolutionState state, Hashtable done, GPType type)
        {
            // put type in the hashtable -- it's being used
            done[type] = type;

            // Grab the array in nodes
            var i = FunctionSet.Nodes[type.Type];

            // For each argument type in a node in i, if it's not in done,
            // then add it to done and call me on it
            var initializer = ((GPInitializer)state.Initializer);

            foreach (var node in i)
                foreach (var t in node.Constraints(initializer).ChildTypes)
                    if (done[t] == null)
                    {
                        CheckFunctionSetValidity(state, done, t);
                    }
        }

        #endregion // Operations
        #region ToString

        public override string ToString()
        {
            return Name;
        }

        #endregion // ToString
    }
}