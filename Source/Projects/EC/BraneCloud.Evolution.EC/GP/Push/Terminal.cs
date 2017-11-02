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


/** 
    Terminal is the leaf node in Push trees and is used to represent Push
    instructions of all types.

    <p>ECJ implements Push's s-expressions as trees of nonterminals
    and terminals.  The nonterminals are all dummy instances of the Nonterminal class.
    Terminals are all instances of the Terminal class.
    
    <p>The nonterminals and terminals aren't actually evaluated.  Instead, the
    tree is printed out as a lisp s-expression and sent to the Push interpreter.
    
    <p>Terminals are implemented as ERCs which hold the actual Push instruction
    or atom as a string ('value').  There are four kinds of instructions at present:
    
    <ol>
    <li> Built-in Push instructions like float.* or integer.swap
    <li> Floating-point ERCs (defined by "float.erc")
    <li> Integer ERCs (defined by "integer.erc")
    <li> Custom Push instructions
    </ol>
    
    <p>You specify your instructions like this:
    
    <tt><pre>
    push.op.size = 7
    push.op.0 = float.erc
    push.op.1 = float.+
    
    # This is a custom instruction
    push.op.2 = float.print
    push.op.2.func = ec.gp.Push.example.MyPushInstruction
    
    push.op.3 = float.%
    push.op.4 = float.-
    push.op.5 = float.dup
    push.op.6 = float.swap
    </pre></tt>
    
    <p>For the (at present) two kinds of ERCs, you can specify a minimum
    and a maximum value.  Here are the defaults:
    
    <tt><pre>
    push.op.float.min = -10
    push.op.float.max = 10
    push.op.int.min = -10
    push.op.int.max = 10
    </tt></pre>
    
    The full list of Psh instructions is:
    
    <p><tt>
    integer.+<br>
    integer.-<br>
    integer./<br>
    integer.\%<br>
    integer.*<br>
    integer.pow<br>
    integer.log<br>
    integer.=<br>
    integer.><br>
    integer.*lt;<br>
    integer.min<br>
    integer.max<br>
    integer.abs<br>
    integer.neg<br>
    integer.ln<br>
    integer.fromfloat<br>
    integer.fromboolean<br>
    integer.rand<br>
    float.+<br>
    float.-<br>
    float./<br>
    float.\%<br>
    float.*<br>
    float.pow<br>
    float.log<br>
    float.=<br>
    float.><br>
    float.&lt;<br>
    float.min<br>
    float.max<br>
    float.sin<br>
    float.cos<br>
    float.tan<br>
    float.exp<br>
    float.abs<br>
    float.neg<br>
    float.ln<br>
    float.frominteger<br>
    float.fromboolean<br>
    float.rand<br>
    boolean.=<br>
    boolean.not<br>
    boolean.and<br>
    boolean.or<br>
    boolean.xor<br>
    boolean.frominteger<br>
    boolean.fromfloat<br>
    boolean.rand<br>
    true<br>
    false<br>
    code.quote<br>
    code.fromboolean<br>
    code.frominteger<br>
    code.fromfloat<br>
    code.noop<br>
    code.do*times<br>
    code.do*count<br>
    code.do*range<br>
    code.=<br>
    code.if<br>
    code.rand<br>
    exec.k<br>
    exec.s<br>
    exec.y<br>
    exec.noop<br>
    exec.do*times<br>
    exec.do*count<br>
    exec.do*range<br>
    exec.=<br>
    exec.if<br>
    exec.rand<br>
    input.index<br>
    input.inall<br>
    input.inallrev<br>
    input.stackdepth<br>
    frame.Push<br>
    frame.pop<br>
    </tt>
    
    
    <p><b>Parameters</b><br>
    <table>
    <tr><td valign=top><i>base</i>.<tt>op.size</tt><br>
    <font size=-1>int >= 1</font></td>
    <td valign=top>(Number of instructions in Push's internal "instruction set")</td></tr>
    <tr><td valign=top><i>base</i>.<tt>op</tt>.<i>i</i><br>
    <font size=-1>String</font></td>
    <td valign=top>(Name of instruction <i>i</i>)</td></tr>
    <tr><td valign=top><i>base</i>.<tt>op</tt>.<i>i</i>.<tt>func</tt><br>
    <font size=-1>classname, inherits and != ec.gp.Push.PushInstruction</font></td>
    <td valign=top>(PushInstruction corresponding to instruction <i>i</i>, if it is a custom instruction)</td></tr>
    <tr><td valign=top><i>base</i>.<tt>op.float.min</tt><br>
    <font size=-1>float</font></td>
    <td valign=top>(Minimum value for a Push floating-point ERC)</td></tr>
    <tr><td valign=top><i>base</i>.<tt>op.float.max</tt><br>
    <font size=-1>float</font></td>
    <td valign=top>(Maximum value for a Push floating-point ERC)</td></tr>
    <tr><td valign=top><i>base</i>.<tt>op.int.min</tt><br>
    <font size=-1>int</font></td>
    <td valign=top>(Minimum value for a Push integer ERC)</td></tr>
    <tr><td valign=top><i>base</i>.<tt>op.int.max</tt><br>
    <font size=-1>int</font></td>
    <td valign=top>(Maximum value for a Push integer ERC)</td></tr>
    </table>

    <p><b>Default Base</b><br>
    gp.Push
*/


using System;
using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.GP.Push;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.GP.Push
{
    [ECConfiguration("ec.gp.push.Terminal")]
    public class Terminal : ERC
    {
        public const String P_INSTRUCTION = "in";
        public const String P_NUM_INSTRUCTIONS = "size";
        public const String P_FUNC = "func";
        public const String P_FLOAT = "erc.float";
        public const String P_INTEGER = "erc.int";
        public const String P_MIN = "min";
        public const String P_MAX = "max";

        public const int FLOAT_ERC = 0; // ultimately this needs to be a special kind of class
        public const int INTEGER_ERC = 1; // ultimately this needs to be a special kind of class
        public static readonly String[] ERC_NAMES = {"float.erc", "integer.erc"};

        public static double MinFloatERC = -10.0; // inclusive
        public static double MaxFloatERC = 10.0; // inclusive
        public static int MinIntegerERC = -10;
        public static int MaxIntegerERC = 10;

        /** Names of all the Push instructions I can be set to.  This includes names for custom PushInstructions. */
        public String[] Instructions { get; set; }

        /** A list of custom PushInstructions I can be set to. */
        public PushInstruction[] CustomInstructions { get; set; }

        /** For each PushInstruction, a pointer into instructions which gives the name of that instruction. 
            Note that some instructions in instructions are built-in Push instructions and will have nothing
            pointing to them. */
        public int[] Indices { get; set; } // point to locations in instructions

        /** The current name of the Push Terminal I am set to. */
        String _value;

        public override String Name => "IN";

        public override int ExpectedChildren => 0;
        

        public override String ToStringForHumans()
        {
            return _value;
        }

        public override IParameter DefaultBase => PushDefaults.ParamBase.Push(P_INSTRUCTION);

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            IParameter def = DefaultBase;

            // Load my standard instructions
            int len = state.Parameters.GetInt(paramBase.Push(P_INSTRUCTION).Push(P_NUM_INSTRUCTIONS),
                def.Push(P_NUM_INSTRUCTIONS), 1);
            if (len < 1)
                state.Output.Fatal("Number of instructions must be >= 1",
                    paramBase.Push(P_INSTRUCTION).Push(P_NUM_INSTRUCTIONS), def.Push(P_NUM_INSTRUCTIONS));

            Instructions = new String[len];
            PushInstruction[] insts = new PushInstruction[len];

            for (int i = 0; i < len; i++)
            {
                Instructions[i] = state.Parameters.GetString(paramBase.Push(P_INSTRUCTION).Push("" + i), def.Push("" + i));
                if (Instructions[i] == null)
                    state.Output.Fatal("Terminal number " + i + " is missing.", paramBase.Push(P_INSTRUCTION).Push("" + i),
                        def.Push("" + i));

                // load Instruction if there is one
                IParameter bb = paramBase.Push(P_INSTRUCTION).Push("" + i).Push(P_FUNC);
                IParameter dd = def.Push("" + i).Push(P_FUNC);
                if (state.Parameters.ParameterExists(bb, dd)) // got one
                {
                    String s = state.Parameters.GetString(bb, dd);
                    state.Output.Message("Adding Instruction " + Instructions[i] + " --> " + s);
                    PushInstruction inst =
                        (PushInstruction) state.Parameters.GetInstanceForParameter(bb, dd, typeof(PushInstruction));
                    if (inst == null) // uh oh
                        state.Output.Fatal("Terminal number " + i + ", named " + Instructions[i] +
                                           ", has an invalid function class: " + s);
                    // load that sucker
                    insts[i] = inst;
                }
            }

            // compress instruction list
            int count = 0;
            for (int i = 0; i < len; i++)
                if (insts[i] != null)
                    count++;
            CustomInstructions = new PushInstruction[count];
            Indices = new int[count];

            count = 0;
            for (int i = 0; i < len; i++)
                if (insts[i] != null)
                {
                    CustomInstructions[count] = insts[i];
                    Indices[count] = i;
                    count++;
                }

            // load float ERC bounds
            IParameter b = paramBase.Push(P_FLOAT).Push(P_MIN);
            IParameter d = PushDefaults.ParamBase.Push(P_FLOAT).Push(P_MIN);

            if (!state.Parameters.ParameterExists(b, d))
                state.Output.Warning("No " + ERC_NAMES[FLOAT_ERC] + " min value provided, using " + MinFloatERC, b, d);
            else
            {
                double min = state.Parameters.GetDoubleWithDefault(b, d, double.NaN);
                if (double.IsNaN(min)) // it's NaN
                    state.Output.Fatal("Malformed " + ERC_NAMES[FLOAT_ERC] + " min value", b, d);
                else MinFloatERC = min;
            }

            b = paramBase.Push(P_FLOAT).Push(P_MAX);
            d = PushDefaults.ParamBase.Push(P_FLOAT).Push(P_MAX);

            if (!state.Parameters.ParameterExists(b, d))
                state.Output.Warning("No " + ERC_NAMES[FLOAT_ERC] + " max value provided, using " + MaxFloatERC, b, d);
            else
            {
                double max = state.Parameters.GetDoubleWithDefault(b, d, double.NaN);
                if (double.IsNaN(max)) // it's NaN
                    state.Output.Fatal("Malformed " + ERC_NAMES[FLOAT_ERC] + " max value", b, d);
                else MaxFloatERC = max;
            }
            if (MinFloatERC > MaxFloatERC) // uh oh
                state.Output.Fatal("" + ERC_NAMES[FLOAT_ERC] + " min value is greater than max value.\nMin: " +
                                   MinFloatERC + "\nMax: " + MaxFloatERC);

            b = paramBase.Push(P_INTEGER).Push(P_MIN);
            d = PushDefaults.ParamBase.Push(P_INTEGER).Push(P_MIN);

            // load integer ERC bounds
            if (!state.Parameters.ParameterExists(b, d))
                state.Output.Warning("No " + ERC_NAMES[INTEGER_ERC] + " min value provided, using " + MinIntegerERC, b,
                    d);
            else
            {
                double min = state.Parameters.GetDoubleWithDefault(b, d, double.NaN);
                if (double.IsNaN(min) || min != (int) min) // it's NaN or invalid
                    state.Output.Fatal("Malformed " + ERC_NAMES[INTEGER_ERC] + " min value", b, d);
                MinIntegerERC = (int) min;
            }

            b = paramBase.Push(P_INTEGER).Push(P_MAX);
            d = PushDefaults.ParamBase.Push(P_INTEGER).Push(P_MAX);

            if (!state.Parameters.ParameterExists(b, d))
                state.Output.Warning("No " + ERC_NAMES[INTEGER_ERC] + " max value provided, using " + MaxIntegerERC, b,
                    d);
            else
            {
                double max = state.Parameters.GetDoubleWithDefault(b, d, double.NaN);
                if (double.IsNaN(max) || (max != (int) max)) // it's NaN or invalid
                    state.Output.Fatal("Malformed " + ERC_NAMES[INTEGER_ERC] + " max value", b, d);
                else MaxIntegerERC = (int) max;
            }
            if (MinIntegerERC > MaxIntegerERC) // uh oh
                state.Output.Fatal("" + ERC_NAMES[INTEGER_ERC] + " min value is greater than max value.\nMin: " +
                                   MinIntegerERC + "\nMax: " + MaxIntegerERC);

        }

        public override bool NodeEquals(GPNode other)
        {
            if (other == null) return false;
            if (!(other is Terminal)) return false;
            var o = (Terminal) other;
            return (o._value == _value);
        }

        public override String Encode()
        {
            return Code.Encode(_value);
        }

        public override bool Decode(DecodeReturn dret)
        {
            Code.Decode(dret);
            if (dret.Type == DecodeReturn.T_STRING)
            {
                _value = dret.S;
                // verify
                foreach (string t in Instructions)
                    if (t.Equals(_value))
                        return true;
            }
            // otherwise, uh oh
            return false;
        }

        public override void ResetNode(IEvolutionState state, int thread)
        {
            int i = state.Random[thread].NextInt(Instructions.Length);
            if (Instructions[i].EndsWith("erc")) // it's an erc
            {
                // we'll assume we don't have a lot of ercs
                for (var j = 0; j < ERC_NAMES.Length; j++)
                {
                    if (Instructions[i].Equals(ERC_NAMES[j]))
                    {
                        switch (j)
                        {
                            case FLOAT_ERC:
                                _value =
                                    "" + (state.Random[thread].NextDouble(true, true) * (MaxFloatERC - MinFloatERC) +
                                          MinFloatERC);
                                break;
                            case INTEGER_ERC:
                                _value = "" + (state.Random[thread].NextInt(MaxIntegerERC - MinIntegerERC + 1) +
                                              MinIntegerERC);
                                break;
                            default:
                                state.Output.Fatal("The following PUSH ERC is unknown: " + Instructions[i]);
                                break;
                        }
                        break; // break from for-loop
                    }
                }
            }
            else // it's an instruction
            {
                _value = Instructions[i];
            }
        }

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
            // do nothing
        }
    }

}

