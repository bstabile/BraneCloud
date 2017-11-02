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

namespace BraneCloud.Evolution.EC.GP
{
    /// <summary> 
    /// An ADF is a GPNode which implements an "Automatically Defined Function",
    /// as described in Koza II.  
    /// 
    /// <p/>In this system, the ADF facility consists of several classes: ADF,
    /// ADM, ADFStack, ADFContext, and ADFArgument. ADFs, and their cousins
    /// ADMs ("Automatically Defined Macros [Lee Spector]"), appear as
    /// typical function nodes in a GP tree.  However, they have a special
    /// <i>associated tree</i> in the individual's tree forest which 
    /// they evaluate as a kind of a "subfunction".
    /// 
    /// <p/>When an ADF is evaluated, it first evaluates all of its children
    /// and stores away their results.  It then evaluates its associated tree.
    /// In the associated tree may exist one or more <i>ADF Argument Terminals</i>,
    /// defined by the ADFArgument class.  These terminal nodes are associated
    /// with a single number which represents the "argument" in the original ADF
    /// which evaluated their tree.  When an Argument Terminal is evaluated,
    /// it returns the stored result for that child number in the parent ADF.
    /// Ultimately, when the associated tree completes its evaluation, the ADF
    /// returns that value.
    /// 
    /// <p/>ADMs work slightly differently.  When an ADM is evaluated, it
    /// immediately evaluates its associated tree without first evaluating
    /// any children.  When an Argument Terminal is evaluated, it evaluates
    /// the subtree of the appropriate child number in the parent ADM and returns
    /// that result.  These subtrees can be evaluated many times.  When the
    /// associated tree completes its evaluation, the ADM returns that value.
    /// 
    /// <p/>Obviously, if you have Argument Terminals in a tree, that tree must
    /// be only callable by ADFs and ADMs, otherwise the Argument Terminals
    /// won't have anything to return.  Furthermore, you must make sure that
    /// you don't have an Argument Terminal in a tree whose number is higher
    /// than the smallest arity (number of arguments) of a calling ADF or ADM.
    /// 
    /// <p/>The mechanism behind ADFs and ADMs is complex, requiring two specially-
    /// stored stacks (contained in the ADFStack object) of ADFContexts.  For
    /// information on how this mechanism works, see ADFStack.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>tree</tt><br/>
    /// <font size="-1">int &gt;= 0</font></td>
    /// <td valign="top">(The "associated tree" of the ADF)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>name</tt><br/>
    /// <font size="-1">String, can be undefined</font></td>
    /// <td valign="top">(A simple "name" of the ADF to distinguish it from other ADF functions in your function set.  Use only letters, numbers, hyphens, and underscores.  Lowercase is best.)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// gp.adf
    /// <seealso cref="BraneCloud.Evolution.EC.GP.ADFStack"/>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.ADF")]
    public class ADF : GPNode
    {
        #region Constants

        public const string P_ADF = "adf";
        public const string P_ASSOCIATEDTREE = "tree";
        public const string P_FUNCTIONNAME = "name";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return GPDefaults.ParamBase.Push(P_ADF); }
        }

        /// <summary>
        /// The ADF's associated tree 
        /// </summary>
        public int AssociatedTree { get; set; }

        /// <summary>
        /// The "function name" of the ADF, to distinguish it from other ADF functions you might provide.  
        /// </summary>
        public string FunctionName { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // we don't know our name yet, (used in ToStringForError(),
            // which is used in GPNode's Setup(...) method),
            // so WE load parameters before our parent does.

            var def = DefaultBase;

            AssociatedTree = state.Parameters.GetInt(paramBase.Push(P_ASSOCIATEDTREE), def.Push(P_FUNCTIONNAME), 0);
            if (AssociatedTree < 0)
                state.Output.Fatal("ADF/ADM node must have a positive-numbered associated tree.", paramBase.Push(P_ASSOCIATEDTREE), def.Push(P_FUNCTIONNAME));

            FunctionName = state.Parameters.GetString(paramBase.Push(P_FUNCTIONNAME), def.Push(P_FUNCTIONNAME));
            if (String.IsNullOrEmpty(FunctionName))
            {
                FunctionName = "ADF" + (AssociatedTree - 1);
                state.Output.Warning("ADF/ADM node for Tree " + AssociatedTree + " has no function name.  Using the name " + FunctionName,
                    paramBase.Push(P_FUNCTIONNAME), def.Push(P_FUNCTIONNAME));
            }

            if (FunctionName.Length == 1)
            {
                state.Output.Warning("Using old-style ADF/ADM name.  You should change it to something longer and more descriptive, such as ADF"
                    + FunctionName, paramBase.Push(P_FUNCTIONNAME), def.Push(P_FUNCTIONNAME));
            }

            // now we let our parent set up.  
            base.Setup(state, paramBase);
        }

        /// <summary>
        /// Checks type-compatibility constraints between the ADF, its argument terminals, 
        /// and the tree type of its associated tree, and also checks to make sure the tree exists, 
        /// there aren't invalid argument terminals in it, and there are sufficient argument terminals (a warning).  Whew! 
        /// </summary>
        public override void CheckConstraints(IEvolutionState state, int tree, GPIndividual typicalIndividual, IParameter individualBase)
        {
            base.CheckConstraints(state, tree, typicalIndividual, individualBase);

            // does the associated tree exist?

            if (AssociatedTree < 0 || AssociatedTree >= typicalIndividual.Trees.Length)
            {
                state.Output.Error("The node " + ToStringForError() + " of individual " + individualBase
                            + " must have an associated tree that is >= 0 and < " + typicalIndividual.Trees.Length 
                            + ".  Value provided was: " + AssociatedTree);
            }
            else
            {

                // is the associated tree of the correct type?  Issue an error.
                var initializer = ((GPInitializer)state.Initializer);

                if (!Constraints(initializer).ReturnType.CompatibleWith(initializer, typicalIndividual.Trees[AssociatedTree].Constraints(initializer).TreeType))
                {
                    state.Output.Error("The return type of the node " + ToStringForError() + " of individual " + individualBase
                                        + "is not type-compatible with the tree type of its associated tree.");
                }

                var funcs = typicalIndividual.Trees[AssociatedTree].Constraints(initializer).FunctionSet.Nodes;

                var validArgument = new ADFArgument[Children.Length];

                for (var w = 0; w < funcs.Length; w++)
                {
                    // does the tree's function set have argument terminals 
                    // that are beyond what I can provide?  (issue an error)

                    var gpfi = funcs[w];
                    for (var x = 0; x < gpfi.Length; x++)
                    {
                        if (gpfi[x] is ADFArgument)
                        {
                            var argument = (ADFArgument)(gpfi[x]);
                            var arg = argument.Argument;
                            if (arg >= Children.Length)
                            // uh oh
                            {
                                state.Output.Error("The node " + ToStringForError() + " in individual "
                                                   + individualBase +
                                                   " would call its associated tree, which has an argument terminal with an argument number ("
                                                   + arg + ") >= the ADF/ADM's arity ("
                                                   + Children.Length
                                                   + ").  The argument terminal in question is "
                                                   + gpfi[x].ToStringForError());
                            }
                            else
                            {
                                if (validArgument[arg] != null && validArgument[arg] != argument)
                                // got one already
                                {
                                    state.Output.Warning("There exists more than one Argument terminal for argument #" +
                                                         arg
                                                         + " for the node " + ToStringForError() + " in individual " +
                                                         individualBase);
                                }
                                else
                                    validArgument[arg] = argument;

                                // is the argument terminal of the correct return type?  Issue an error.
                                if (
                                    !gpfi[x].Constraints(initializer).ReturnType.CompatibleWith(initializer,
                                                                                                Constraints(initializer).ChildTypes[arg]))
                                {
                                    state.Output.Error("The node " + ToStringForError() + " in individual " +
                                                       individualBase
                                                       +
                                                       " would call its associated tree, which has an argument terminal which is not type-compatible with the related argument position of the ADF/ADM.  The argument terminal in question is "
                                                       + gpfi[x].ToStringForError());
                                }
                            }
                        }
                    }
                }

                // does the tree's function set have fewer argument terminals
                // than I can provide? (issue a warning)

                for (var x = 0; x < Children.Length; x++)
                {
                    if (validArgument[x] == null)
                    {
                        state.Output.Warning("There is no argument terminal for argument #" + x
                            + " for the node " + ToStringForError() + " in individual " + individualBase);
                    }
                }
            }
        }

        #endregion // Setup
        #region Evaluation

        public override void Eval(IEvolutionState state, int thread, GPData input, ADFStack stack, GPIndividual individual, IProblem problem)
        {
            // get a context and prepare it
            var c = stack.Take();
            c.PrepareADF(this, (GPProblem) problem);

            // evaluate my Arguments and load 'em in 
            for (var x = 0; x < Children.Length; x++)
            {
                input.CopyTo(c.Arguments[x]);
                Children[x].Eval(state, thread, c.Arguments[x], stack, individual, problem);
            }

            // Now push the context onto the stack.
            stack.Push(c);

            // evaluate the top of the associatedTree
            individual.Trees[AssociatedTree].Child.Eval(state, thread, input, stack, individual, problem);

            // pop the context off, and we're done!
            if (stack.Pop(1) != 1)
                state.Output.Fatal("Stack prematurely empty for " + ToStringForError());
        }

        #endregion // Evaluation
        #region Comparison

        /// <summary>
        /// Returns class.hashCode() + functionName.GetHashCode() + associatedTree.  
        /// Hope that's reasonably random. 
        /// </summary>       
        public override int NodeHashCode()
        {
            return (GetType().GetHashCode() + FunctionName.GetHashCode() + AssociatedTree);
        }
        
        /// <summary>
        /// Determines node equality by comparing the class, associated tree, and
        /// function name of the nodes. 
        /// </summary>
        public override bool NodeEquals(GPNode node)
        {
            if (!GetType().Equals(node.GetType()) || Children.Length != node.Children.Length)
                return false;
            var adf = (ADF) node;
            return (AssociatedTree == adf.AssociatedTree && FunctionName.Equals(adf.FunctionName));
        }

        #endregion // Comparison
        #region ToString

        public override string ToString()
        {
            return FunctionName;
        }

        #endregion // ToString
        #region IO

        public override void WriteNode(IEvolutionState state, BinaryWriter writer)
        {
            writer.Write(AssociatedTree);
            writer.Write(FunctionName); // Default encoding of BinaryWriter is UTF-8
        }

        public override void ReadNode(IEvolutionState state, BinaryReader reader)
        {
            AssociatedTree = reader.ReadInt32();
            FunctionName = reader.ReadString(); // Default encoding of BinaryWriter is UTF-8
        }

        #endregion // IO
    }
}