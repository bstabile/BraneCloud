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
    /// An ADFArgument is a GPNode which represents an ADF's 
    /// <i>argument terminal</i>, its counterpart which returns argument
    /// values in its associated function tree.  In lil-gp this is called an
    /// ARG node.
    /// 
    /// <p/>Obviously, if you have Argument Terminals in a tree, that tree must
    /// be only callable by ADFs and ADMs, otherwise the Argument Terminals
    /// won't have anything to return.  Furthermore, you must make sure that
    /// you don't have an Argument Terminal in a tree whose number is higher
    /// than the smallest arity (number of arguments) of a calling ADF or ADM.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>arg</tt><br/>
    /// <font size="-1">int &gt;= 0</font></td>
    /// <td valign="top">(The related argument position for the ADF Argument Node in the associated ADF)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// gp.adf-argument
    /// <seealso cref="BraneCloud.Evolution.EC.GP.ADF"/>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.ADFArgument")]
    public class ADFArgument : GPNode
    {
        #region Constants

        public const string P_ADFARGUMENT = "adf-argument";
        public const string P_ARGUMENT = "arg";
        public const string P_FUNCTIONNAME = "name";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return GPDefaults.ParamBase.Push(P_ADFARGUMENT); }
        }

        public int Argument { get; set; }

        public string FunctionName { get; set; }

        public override int ExpectedChildren { get { return 0; } }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            
            var def = DefaultBase;
            
            Argument = state.Parameters.GetInt(paramBase.Push(P_ARGUMENT), def.Push(P_ARGUMENT), 0);
            if (Argument < 0)
                state.Output.Fatal("Argument terminal must have a non-negative argument number.", paramBase.Push(P_ARGUMENT), def.Push(P_ARGUMENT));

            FunctionName = state.Parameters.GetString(paramBase.Push(P_FUNCTIONNAME), def.Push(P_FUNCTIONNAME));
            if (String.IsNullOrEmpty(FunctionName))
            {
                FunctionName = "ARG" + Argument;
                state.Output.Warning("ADFArgument node for argument " + Argument + " has no function name.  Using the name " + FunctionName,
                    paramBase.Push(P_FUNCTIONNAME), def.Push(P_FUNCTIONNAME));
            }
        }

        #endregion // Setup
        #region Evaluation

        public override void Eval(IEvolutionState state, int thread, GPData input, ADFStack stack, GPIndividual individual, IProblem problem)
        {
            // get the current context
            var c = stack.Top(0);
            if (c == null)
                // uh oh
                state.Output.Fatal("No context with which to evaluate ADFArgument terminal " + ToStringForError()
                    + ".  This often happens if you evaluate a tree by hand  which is supposed to only be an ADF's associated tree.");

            c.Evaluate(state, thread, input, stack, individual, problem, Argument);
        }

        #endregion // Evaluation
        #region ToString

        public override string ToString() { return FunctionName; }

        #endregion // ToString
        #region IO

        public override void WriteNode(IEvolutionState state, BinaryWriter dataOutput)
        {
            dataOutput.Write(Argument);
        }
        
        public override void  ReadNode(IEvolutionState state, BinaryReader dataInput)
        {
            Argument = dataInput.ReadInt32();
        }

        #endregion // IO
    }
}