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
using System.Text;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Psh;

namespace BraneCloud.Evolution.EC.GP.Push
{
    /// <summary>
    /// A PushProblem contains useful methods to help you create an
    /// interpreter, write out the ECJ GP tree to a string, build a Push Program
    /// around this string, load the interpreter with all your custom instructions, 
    /// and run the Push Program on the interpreter.  
    ///     
    /// <p/>Commonly you'd also set up the interpreter's data stacks with some initial
    /// data, then after running the program you might inspect the stacks to determine
    /// the return value. PushProblem also contains some helpful methods to make it easy
    /// for you to set up and modify these stacks.
    /// </summary>
    [ECConfiguration("ec.gp.push.PushProblem")]
    public abstract class PushProblem : GPProblem
    {
        StringBuilder _buffer;

        public override Object Clone()
        {
            PushProblem other = (PushProblem) base.Clone();
            other._buffer = null; // do not share
            return other;
        }

        /** Produces a Push Program from the provided GP Individual's tree. */
        public Program GetProgram(IEvolutionState state, GPIndividual ind)
        {
            if (_buffer == null) _buffer = new StringBuilder();
            else _buffer.Clear();
            try
            {
                String prog = ind.Trees[0].Child.MakeLispTree(_buffer).ToString();
                if (!prog.StartsWith("("))
                    prog = "(" + prog + ")";
                return new Program(prog);
            }
            catch (Exception e)
            {
                // do nothing for the moment
                state.Output.Fatal("Push exception encountered while parsing program from GP Tree:\n" +
                                   ind.Trees[0].Child.MakeLispTree(_buffer) + "\n" + e);
            }
            return null; // unreachable
        }

        /** Builds a Push Interpreter suitable for interpreting the Program given in getProgram(). */
        public Interpreter GetInterpreter(IEvolutionState state, GPIndividual ind, int threadnum)
        {
            // create an Interpreter with an IMersenneTwister RNG
            Interpreter interpreter = new Interpreter(state.Random[threadnum]);

            // Find the function set
            GPFunctionSet set = ind.Trees[0].Constraints((GPInitializer) (state.Initializer)).FunctionSet;
            GPNode[] terminals = set.Terminals[0]; // only one type we assume

            // dump the additional instructions into the interpreter
            foreach (GPNode t in terminals)
                if (t is Terminal) // maybe has some instructions?
                {
                    // This code is here rather than (more appropriately) in Terminal so that we can
                    // free up Terminal from being reliant on the underlying library.
                    Terminal op = (Terminal) (t);
                    PushInstruction[] customInstructions = op.CustomInstructions;
                    int[] indices = op.Indices;
                    String[] instructions = op.Instructions;
                    for (int j = 0; j < customInstructions.Length; j++)
                    {
                        Console.Error.WriteLine(instructions[indices[j]]);
                        interpreter.AddInstruction(instructions[indices[j]],
                            (PushInstruction) customInstructions[j].Clone()); // or should this be DefineInstruction?
                    }
                }

            // all done
            return interpreter;
        }

        /** Executes the given program for up to maxSteps steps. */
        public void ExecuteProgram(Program program, Interpreter interpreter, int maxSteps)
        {
            interpreter.Execute(program, maxSteps);
        }

        /** Clears the Interpreter's stacks so it is ready to execute another program. */
        public void ResetInterpreter(Interpreter interpreter)
        {
            interpreter.ClearStacks();
        }

        /** Pushes a value onto the top of the float stack of the interpreter. */
        public void PushOntoFloatStack(Interpreter interpreter, float val)
        {
            interpreter.FloatStack().Push(val);
        }

        /** Pushes a value onto the top of the int stack of the interpreter. */
        public void PushOntoIntStack(Interpreter interpreter, int val)
        {
            interpreter.IntStack().Push(val);
        }

        /** Tests to see if the interpreter's float stack is empty. */
        public bool IsFloatStackEmpty(Interpreter interpreter)
        {
            return interpreter.FloatStack().Count == 0;
        }

        /** Tests to see if the interpreter's int stack is empty. */
        public bool IsIntStackEmpty(Interpreter interpreter)
        {
            return interpreter.IntStack().Count == 0;
        }

        /** Returns the top of the interpreter's float stack. */
        public float TopOfFloatStack(Interpreter interpreter)
        {
            return interpreter.FloatStack().Top();
        }

        /** Returns the top of the interpreter's int stack. */
        public int TopOfIntStack(Interpreter interpreter)
        {
            return interpreter.IntStack().Top();
        }
    }

}