/*
* Copyright 2009-2010 Jon Klein
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*    http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using BraneCloud.Evolution.EC.Randomization;
using SharpenMinimal;

namespace BraneCloud.Evolution.EC.Psh
{
    /// <summary>The Push language interpreter.</summary>
    public class Interpreter
    {

        protected internal Dictionary<string, Instruction> _instructions = new Dictionary<string, Instruction>();

        protected internal Dictionary<string, Interpreter.AtomGenerator> _generators =
            new Dictionary<string, Interpreter.AtomGenerator>();

        protected internal List<Interpreter.AtomGenerator> _randomGenerators = new List<Interpreter.AtomGenerator>();

        protected internal Psh.IntStack _intStack = new Psh.IntStack();

        protected internal Psh.FloatStack _floatStack = new Psh.FloatStack();

        protected internal BooleanStack _boolStack = new BooleanStack();

        protected internal ObjectStack _codeStack = new ObjectStack();

        protected internal GenericStack<string> _nameStack = new GenericStack<string>();

        protected internal ObjectStack _execStack = new ObjectStack();

        protected internal ObjectStack _inputStack = new ObjectStack();

        // This kind of looks like it wants to be a map, but I think the order is more important.
        protected internal List<Tuple<string, Stack>> _stacks = new List<Tuple<string, Stack>>();

        protected internal int _totalStepsTaken;

        protected internal long _evaluationExecutions = 0;

        // These parameters seem weird to have in the interpreter.

        protected internal int _maxRandomInt;

        protected internal int _minRandomInt;

        protected internal int _randomIntResolution;

        protected internal float _maxRandomFloat;

        protected internal float _minRandomFloat;

        protected internal float _randomFloatResolution;

        protected internal int _maxRandomCodeSize;

        protected internal int _maxPointsInProgram;

        protected internal Random Rng = new Random();

        // XXX What is the input stack and pusher really for?
        protected internal InputPusher _inputPusher = new InputPusher();

        // XXX This value needs to be set at the beginning somehow.
        // Or setting it is just more complicated.
        public bool CaseSensitive => false;

        // XXX yield flag?
        public bool Stop { get; set; }

        public Interpreter(IMersenneTwister mersenneTwisterRng)
        {
            Rng = (Random) mersenneTwisterRng;
        }

        public Interpreter()
        {
            // All generators
            // Create the stacks.
            // This arraylist will hold all custom stacks that can be created by the
            // problem classes
            /* Since the _inputStack will not change after initialization, it will not
              * need a frame stack.
            */
            AddStack("exec", _execStack);
            AddStack("code", _codeStack);
            AddStack("integer", _intStack);
            AddStack("float", _floatStack);
            AddStack("boolean", _boolStack);
            AddStack("name", _nameStack);
            AddStack("input", _inputStack);

            // Auto converted Java to C# using Sharpen but it could still use some more love.

            // How it used to be:
            // ------------------
            // DefineInstruction("integer.+", new IntegerAdd());

            // // The IntegerAdd class is defined like so:
            // internal class IntegerAdd : BinaryIntegerInstruction {
            //   internal override int BinaryOperator(int inA, int inB) {
            //     // Test for overflow
            //     if ((Math.Abs(inA) > int.MaxValue / 10) || (Math.Abs(inB) > int.MaxValue / 10)) {
            //       long lA = (long)inA;
            //       long lB = (long)inB;
            //       if (lA + lB != inA + inB) {
            //         if (inA > 0) {
            //           return int.MaxValue;
            //         } else {
            //           return int.MinValue;
            //         }
            //       }
            //     }
            //     return inA + inB;
            //   }
            // }
            // And there are a lot of these classes:
            // DefineInstruction("integer.-", new IntegerSub());
            // DefineInstruction("integer./", new IntegerDiv());
            // DefineInstruction("integer.%", new IntegerMod());
            // DefineInstruction("integer.*", new IntegerMul());
            // Eek.

            // How it is now:
            // --------------
            DefineInstruction<int>("integer.+", (a, b) => unchecked(a + b));
            // Let's use some lambdas and unchecked arithmetic.

            // Sympathy for the Original Author, Jon Klein
            // -------------------------------------------
            //
            // Java has lambdas now, but it didn't when this code was written. Also I
            // don't believe there's a nice way just turning off Java's over- and
            // under-flow checking.

            DefineInstruction<int>("integer.-", (a, b) => unchecked(a - b));
            // DefineInstruction<int>("integer./", (a, b) => { return (b != 0 ? a / b : 0); } );
            DefineInstruction<int>("integer./", (a, b) => unchecked(b != 0 ? (a / b) : 0));
            DefineInstruction<int>("integer.%", (a, b) => unchecked(b != 0 ? (a % b) : 0));
            // DefineInstruction<int>("integer.%", (a, b) => { unchecked { var c = (b != 0 ? (a % b) : 0); return c; } } );
            DefineInstruction<int>("integer.*", (a, b) => unchecked(a * b));
            DefineInstruction<int>("integer.pow", (a, b) => unchecked((int) Math.Pow(a, b)));
            DefineInstruction<int>("integer.log", (a, b) => unchecked((int) Math.Log(a, b)));
            DefineInstruction<int, bool>("integer.=", (a, b) => (a == b));
            DefineInstruction<int, bool>("integer.>", (a, b) => (a > b));
            DefineInstruction<int, bool>("integer.<", (a, b) => (a < b));
            DefineInstruction<int>("integer.min", (a, b) => (a < b ? a : b));
            DefineInstruction<int>("integer.max", (a, b) => (a > b ? a : b));
            DefineInstruction<int>("integer.abs", (a, b) => unchecked(a < 0 ? -a : a));
            DefineInstruction<int>("integer.neg", (a) => unchecked(-a));
            DefineInstruction<int>("integer.log", (a) => unchecked((int) Math.Log(a)));
            DefineInstruction("integer.define", (int i, string name) => { DefineConstant(name, i); });
            // DefineInstruction("integer.ln", new IntegerLn());
            DefineInstruction("integer.fromfloat", (float a) => (int) a);
            DefineInstruction("integer.fromboolean", (bool a) => a ? 1 : 0);
            DefineInstruction("integer.rand", new IntegerRand());

            DefineInstruction<float>("float.+", (a, b) => unchecked(a + b));
            DefineInstruction<float>("float.-", (a, b) => unchecked(a - b));
            DefineInstruction<float>("float./", (a, b) => unchecked(b != 0f ? a / b : 0f));
            DefineInstruction<float>("float.%", (a, b) => unchecked(b != 0f ? a % b : 0f));
            DefineInstruction<float>("float.*", (a, b) => unchecked(a * b));
            DefineInstruction<float>("float.pow", (a, b) => unchecked((float) Math.Pow(a, b)));
            DefineInstruction<float>("float.log", (a, b) => unchecked((float) Math.Log(a, b)));
            DefineInstruction<float, bool>("float.=", (a, b) => unchecked(a == b));
            DefineInstruction<float, bool>("float.>", (a, b) => unchecked(a > b));
            DefineInstruction<float, bool>("float.<", (a, b) => unchecked(a < b));
            DefineInstruction<float>("float.min", (a, b) => unchecked(a < b ? a : b));
            DefineInstruction<float>("float.max", (a, b) => unchecked(a > b ? a : b));
            DefineInstruction<float>("float.sin", (a) => unchecked((float) Math.Sin(a)));
            DefineInstruction<float>("float.cos", (a) => unchecked((float) Math.Cos(a)));
            DefineInstruction<float>("float.tan", (a) => unchecked((float) Math.Tan(a)));
            DefineInstruction<float>("float.exp", (a) => unchecked((float) Math.Exp(a)));
            DefineInstruction<float>("float.abs", (a) => unchecked(a < 0 ? -a : a));
            DefineInstruction<float>("float.neg", (a) => unchecked(-a));
            DefineInstruction<float>("float.ln", (a) => unchecked((float) Math.Log(a)));
            DefineInstruction("float.frominteger", (int a) => (float) a);
            DefineInstruction("float.fromboolean", (bool a) => a ? 1f : 0f);
            DefineInstruction("float.rand", new FloatRand());
            DefineInstruction("float.define", (float i, string name) => { DefineConstant(name, i); });

            DefineInstruction<bool>("boolean.=", (a, b) => a == b);
            DefineInstruction<bool>("boolean.not", a => !a);
            DefineInstruction<bool>("boolean.and", (a, b) => a & b);
            DefineInstruction<bool>("boolean.or", (a, b) => a | b);
            DefineInstruction<bool>("boolean.xor", (a, b) => a ^ b);
            DefineInstruction("boolean.frominteger", (int a) => a != 0);
            DefineInstruction("boolean.fromfloat", (float a) => a != 0f);
            DefineInstruction("boolean.rand", new BoolRand());
            DefineInstruction("boolean.define", (bool b, string name) => { DefineConstant(name, b); });

            DefineInstruction("code.quote", new Quote());
            DefineInstruction("code.fromboolean", new CodeFromBoolean());
            DefineInstruction("code.frominteger", new CodeFromInteger());
            DefineInstruction("code.fromfloat", new CodeFromFloat());
            DefineInstruction("code.noop", new ExecNoop());
            DefineInstruction("exec.k", new ExecK(_execStack));
            DefineInstruction("exec.s", new ExecS(_execStack, _maxPointsInProgram));
            DefineInstruction("exec.y", new ExecY(_execStack));
            DefineInstruction("exec.yield", new ExecYield());
            DefineInstruction("exec.noop", new ExecNoop());
            DefineInstruction("exec.do*times", new ExecDoTimes(this));
            DefineInstruction("code.do*times", new CodeDoTimes(this));
            DefineInstruction("exec.do*count", new ExecDoCount(this));
            DefineInstruction("code.do*count", new CodeDoCount(this));
            DefineInstruction("exec.do*range", new ExecDoRange(this));
            DefineInstruction("code.do*range", new CodeDoRange(this));
            DefineInstruction("code.=", new ObjectEquals(_codeStack));
            DefineInstruction("exec.=", new ObjectEquals(_execStack));
            DefineInstruction("code.if", new IF(_codeStack));
            DefineInstruction("exec.if", new IF(_execStack));
            DefineInstruction("code.rand", new RandomPushCode(_codeStack));
            DefineInstruction("exec.rand", new RandomPushCode(_execStack));
            DefineInstruction("true", new Constant<bool>(true));
            DefineInstruction("false", new Constant<bool>(false));
            DefineInstruction("input.index", new InputIndex(_inputStack));
            DefineInstruction("input.inall", new InputInAll(_inputStack));
            DefineInstruction("input.inallrev", new InputInRev(_inputStack));
            DefineInstruction("input.stackdepth", new Depth(_inputStack));
            _generators[InstructionCase("float.erc")] = new FloatAtomGenerator();
            _generators[InstructionCase("integer.erc")] = new IntAtomGenerator();
        }

        // XXX What are frames?
        // YYY We're removing frames for now.
        /// <summary>
        /// Enables experimental Push "frames"
        /// When frames are enabled, each Push subtree is given a fresh set of stacks
        /// (a "frame") when it executes.
        /// </summary>
        /// <remarks>
        /// Enables experimental Push "frames"
        /// When frames are enabled, each Push subtree is given a fresh set of stacks
        /// (a "frame") when it executes. When a frame is pushed, the top value from
        /// each stack is passed to the new frame, and likewise when the frame pops,
        /// allowing for input arguments and return values.
        /// </remarks>
        // public void SetUseFrames(bool inUseFrames) {
        //   _useFrames = inUseFrames;
        // }

        /// <summary>
        /// Defines the instruction set used for random code generation in this Push
        /// interpreter.
        /// </summary>
        /// <param name="inInstructionList">
        /// A program consisting of a list of string instruction names to
        /// be placed in the instruction set.
        /// </param>
        /// <exception cref="Exception"/>
        public void SetInstructions(Program inInstructionList)
        {
            _randomGenerators.Clear();
            for (int n = 0; n < inInstructionList.Size(); n++)
            {
                object o = inInstructionList.DeepPeek(n);
                string name = null;
                if (o is Instruction)
                {
                    if (_instructions.Keys.Contains(o))
                        break;
                }
                else
                {
                    if (o is string)
                    {
                        name = (string) o;
                    }
                    else
                    {
                        throw new Exception("Instruction list must contain a list of Push instruction names only");
                    }
                }
                // Check for registered
                if (name.IndexOf("registered.") == 0)
                {
                    string registeredType = Runtime.Substring(name, 11);
                    if (!registeredType.Equals("integer")
                        && !registeredType.Equals("float")
                        && !registeredType.Equals("boolean")
                        && !registeredType.Equals("exec")
                        && !registeredType.Equals("code")
                        && !registeredType.Equals("name")
                        && !registeredType.Equals("input")
                        && !registeredType.Equals("frame"))
                    {
                        Console.Error.WriteLine("Unknown instruction \"" + name + "\" in instruction set");
                    }
                    else
                    {
                        // Legal stack type, so add all generators matching
                        // registeredType to _randomGenerators.
                        // object[] keys = SharpenMinimal.Collections.ToArray(_instructions.Keys);
                        // for (int i = 0; i < keys.Length; i++)
                        // {
                        //   string key = (string)keys[i];
                        foreach (string key in _instructions.Keys)
                        {
                            if (key.IndexOf(registeredType) == 0)
                            {
                                Interpreter.AtomGenerator g = _generators[key];
                                _randomGenerators.Add(g);
                            }
                        }
                        if (registeredType.Equals("boolean"))
                        {
                            var t = _generators["true"];
                            _randomGenerators.Add(t);
                            var f = _generators["false"];
                            _randomGenerators.Add(f);
                        }
                        if (registeredType.Equals("integer"))
                        {
                            var g = _generators["integer.erc"];
                            _randomGenerators.Add(g);
                        }
                        if (registeredType.Equals("float"))
                        {
                            var g = _generators["float.erc"];
                            _randomGenerators.Add(g);
                        }
                    }
                }
                else
                {
                    if (name.IndexOf("input.makeinputs") == 0)
                    {
                        string strnum = SharpenMinimal.Runtime.Substring(name, 16);
                        int num = System.Convert.ToInt32(strnum);
                        for (int i = 0; i < num; i++)
                        {
                            DefineInstruction("input.in" + i, new InputInN(i));
                            var g = _generators["input.in" + i];
                            _randomGenerators.Add(g);
                        }
                    }
                    else
                    {
                        var g = _generators[name];
                        if (g == null)
                        {
                            throw new Exception("Unknown instruction \"" + name + "\" in instruction set");
                        }
                        else
                        {
                            _randomGenerators.Add(g);
                        }
                    }
                }
            }
        }

        /*
          Define a new instruction and add it to the random generators.
         */
        public void AddInstruction(string inName, Instruction inInstruction)
        {
            inName = InstructionCase(inName);
            var iag = new Interpreter.InstructionAtomGenerator(inName);
            _instructions[inName] = inInstruction;
            _generators[inName] = iag;
            _randomGenerators.Add(iag);
        }

        public void DefineConstant<T>(string inName, T value)
        {
            DefineInstruction(inName, new Constant<T>(value));
        }

        public void DefineInstruction<T>(string inName, Func<T> f)
        {
            DefineInstruction(inName, new NullaryInstruction<T>(f));
        }

        public void DefineInstruction(string inName, Action f)
        {
            DefineInstruction(inName, new NullaryAction(f));
        }

        public void DefineInstruction<T>(string inName, Func<T, T> f)
        {
            DefineInstruction(inName, new UnaryInstruction<T>(f));
        }

        public void DefineInstruction<T>(string inName, Action<T> f)
        {
            DefineInstruction(inName, new UnaryAction<T>(f));
        }

        public void DefineInstruction<T>(string inName, Func<T, T, T> f)
        {
            DefineInstruction(inName, new BinaryInstruction<T>(f));
        }

        public void DefineInstruction<X, Y, Z>(string inName, Func<X, Y, Z> f)
        {
            DefineInstruction(inName, new BinaryInstruction<X, Y, Z>(f));
        }

        public void DefineInstruction<X, Y>(string inName, Action<X, Y> f)
        {
            DefineInstruction(inName, new BinaryAction<X, Y>(f));
        }

        public void DefineInstruction<T>(string inName, Action<T, T> f)
        {
            DefineInstruction(inName, new BinaryAction<T>(f));
        }

        public void DefineInstruction<inT, outT>(string inName, Func<inT, outT> f)
        {
            DefineInstruction(inName, new UnaryInstruction<inT, outT>(f));
        }

        public void DefineInstruction<inT, outT>(string inName, Func<inT, inT, outT> f)
        {
            DefineInstruction(inName, new BinaryInstruction<inT, outT>(f));
        }

        public void DefineInstruction(string inName, Instruction inInstruction)
        {
            inName = InstructionCase(inName);
            _instructions[inName] = inInstruction;
            _generators[inName] = new Interpreter.InstructionAtomGenerator(inName);
        }

        protected internal void DefineStackInstructions(string inTypeName, Stack inStack)
        {
            DefineInstruction(inTypeName + ".pop", new Pop(inStack));
            DefineInstruction(inTypeName + ".swap", new Swap(inStack));
            DefineInstruction(inTypeName + ".rot", new Rot(inStack));
            DefineInstruction(inTypeName + ".flush", new Flush(inStack));
            DefineInstruction(inTypeName + ".dup", new Dup(inStack));
            DefineInstruction(inTypeName + ".stackdepth", new Depth(inStack));
            DefineInstruction(inTypeName + ".shove", new Shove(inStack));
            DefineInstruction(inTypeName + ".yank", new Yank(inStack));
            DefineInstruction(inTypeName + ".yankdup", new YankDup(inStack));
        }

        /// <summary>Sets the parameters for the ERCs.</summary>
        /// <param name="minRandomInt"/>
        /// <param name="maxRandomInt"/>
        /// <param name="randomIntResolution"/>
        /// <param name="minRandomFloat"/>
        /// <param name="maxRandomFloat"/>
        /// <param name="randomFloatResolution"/>
        /// <param name="maxRandomCodeSize"></param>
        /// <param name="maxPointsInProgram"></param>
        public void SetRandomParameters(int minRandomInt, int maxRandomInt,
            int randomIntResolution,
            float minRandomFloat, float maxRandomFloat,
            float randomFloatResolution,
            int maxRandomCodeSize,
            int maxPointsInProgram)
        {
            _minRandomInt = minRandomInt;
            _maxRandomInt = maxRandomInt;
            _randomIntResolution = randomIntResolution;
            _minRandomFloat = minRandomFloat;
            _maxRandomFloat = maxRandomFloat;
            _randomFloatResolution = randomFloatResolution;
            _maxRandomCodeSize = maxRandomCodeSize;
            _maxPointsInProgram = maxPointsInProgram;
        }

        /// <summary>Executes a Push program with no execution limit.</summary>
        /// <returns>The number of instructions executed.</returns>
        public int Execute(Program inProgram)
        {
            return Execute(inProgram, -1);
        }

        /// <summary>Executes a Push program with a given instruction limit.</summary>
        /// <param name="inProgram"></param>
        /// <param name="inMaxSteps">The maximum number of instructions allowed to be executed.</param>
        /// <returns>The number of instructions executed.</returns>
        public int Execute(Program inProgram, int inMaxSteps)
        {
            _evaluationExecutions++;
            LoadProgram(inProgram);
            // Initializes program
            return Step(inMaxSteps);
        }

        /// <summary>Loads a Push program into the interpreter's exec and code stacks.</summary>
        /// <param name="inProgram">The program to load.</param>
        public void LoadProgram(Program inProgram)
        {
            _codeStack.Push(inProgram);
            _execStack.Push(inProgram);
        }

        /// <summary>Steps a Push interpreter forward with a given instruction limit.</summary>
        /// <remarks>
        /// Steps a Push interpreter forward with a given instruction limit.
        /// This method assumes that the intepreter is already setup with an active
        /// program (typically using \ref Execute).
        /// </remarks>
        /// <param name="inMaxSteps">The maximum number of instructions allowed to be executed.</param>
        /// <returns>The number of instructions executed.</returns>
        public int Step(int inMaxSteps)
        {
            Stop = false;
            int executed = 0;
            while (inMaxSteps != 0 && _execStack.Size() > 0 && !Stop)
            {
                var inst = _execStack.Pop();
                switch (ExecuteInstruction(inst))
                {
                    case 0:
                        break;
                    case -1:
                        throw new Exception("Unable to execute instruction " + inst);
                }
                inMaxSteps--;
                executed++;
            }
            _totalStepsTaken += executed;
            return executed;
        }

        public int ExecuteInstruction(object inObject)
        {
            if (inObject is Program)
            {
                Program p = (Program) inObject;
                p.PushAllReverse(_execStack);
                return 0;
            }
            if (inObject is int)
            {
                _intStack.Push((int) inObject);
                return 0;
            }
            // if (inObject is Number)
            // {
            //   _floatStack.Push(((Number)inObject).FloatValue());
            //   return 0;
            // }
            if (inObject is float)
            {
                _floatStack.Push((float) inObject);
                return 0;
            }
            if (inObject is Instruction)
            {
                ((Instruction) inObject).Execute(this);
                return 0;
            }
            if (inObject is string)
            {
                Instruction i;
                if (_instructions.TryGetValue(InstructionCase((string) inObject), out i))
                {
                    i.Execute(this);
                }
                else
                {
                    _nameStack.Push((string) inObject);
                }
                return 0;
            }
            return -1;
        }

        public GenericStack<T> GetStack<T>()
        {
            return _stacks
                .Select(x => x.Item2)
                .Where(y => y is GenericStack<T>)
                .Cast<GenericStack<T>>()
                .Single(s => s.StackType == typeof(T));
            // if (typeof(T) == typeof(int))
            //   return _intStack as Psh.GenericStack<T>;
            // else if (typeof(T) == typeof(bool))
            //   return _boolStack as Psh.GenericStack<T>;
            // else if (typeof(T) == typeof(float))
            //   return _floatStack as Psh.GenericStack<T>;
            // else
            //   throw new Exception("No stack for type " + typeof(T));
            // XXX Do the good thing!
            // return null;
        }

        /// <summary>Fetch the active integer stack.</summary>
        public Psh.IntStack IntStack()
        {
            return _intStack;
        }

        /// <summary>Fetch the active float stack.</summary>
        public FloatStack FloatStack()
        {
            return _floatStack;
        }

        /// <summary>Fetch the active exec stack.</summary>
        public ObjectStack ExecStack()
        {
            return _execStack;
        }

        /// <summary>Fetch the active code stack.</summary>
        public ObjectStack CodeStack()
        {
            return _codeStack;
        }

        /// <summary>Fetch the active bool stack.</summary>
        public BooleanStack BoolStack()
        {
            return _boolStack;
        }

        /// <summary>Fetch the active name stack.</summary>
        public GenericStack<string> NameStack()
        {
            return _nameStack;
        }

        /// <summary>Fetch the active input stack.</summary>
        public ObjectStack InputStack()
        {
            return _inputStack;
        }

        /// <summary>Fetch the indexed custom stack</summary>
        public Stack GetStack(int inIndex)
        {
            return _stacks[inIndex].Item2;
        }

        public Stack GetStack(string stackName)
        {
            return _stacks.Single(x => x.Item1 == stackName).Item2;
        }

        /// <summary>Add a custom stack, and return that stack's index</summary>
        public int AddStack(string stackName, Stack inStack)
        {
            _stacks.Add(Tuple.Create(stackName, inStack));
            DefineStackInstructions(stackName, inStack);
            return _stacks.Count - 1;
        }

        /// <summary>Prints out the current stack states.</summary>
        public void PrintStacks()
        {
            Console.Out.WriteLine(this);
        }

        /// <summary>Returns a string containing the current Interpreter stack states.</summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var t in _stacks)
            {
                sb.Append(t.Item1);
                sb.Append(" stack: ");
                sb.Append(t.Item2);
                sb.Append("\n");
            }
            return sb.ToString();
        }

        /// <summary>Resets the Push interpreter state by clearing all of the stacks.</summary>
        public void ClearStacks()
        {
            // Clear all custom stacks
            foreach (var t in _stacks)
            {
                t.Item2.Clear();
            }
        }

        /// <summary>Returns a string list of all instructions enabled in the interpreter.</summary>
        public string GetRegisteredInstructionsString()
        {
            return _instructions
                .OrderBy(kv => kv.Key)
                .Select(kv => kv.Key.ToString())
                .Aggregate((current, next) => current + " " + next);
            // object[] keys = SharpenMinimal.Collections.ToArray(_instructions.Keys);
            // Arrays.Sort(keys);
            // string list = string.Empty;
            // for (int n = 0; n < keys.Length; n++)
            // {
            //   list += keys[n] + " ";
            // }
            // return list;
        }

        public string InstructionCase(string instructionName)
        {
            return CaseSensitive ? instructionName : instructionName.ToLower();
        }

        /// <summary>Returns a string of all the instructions used in this run.</summary>
        /// <returns/>
        public string GetInstructionsString()
        {
            // object[] keys = SharpenMinimal.Collections.ToArray(_instructions.Keys);
            // List<string> strings = new List<string>();
            List<string> strings =
                _instructions.Keys.Where(key => _randomGenerators.Contains(_generators[key])).ToList();
            // string str = string.Empty;
            // for (int i = 0; i < keys.Length; i++)
            // {
            //   string key = (string)keys[i];
            //   if (_randomGenerators.Contains(_generators[key))]
            //   {
            //     strings.Add(key);
            //   }
            // }
            if (_randomGenerators.Contains(_generators[InstructionCase("float.erc")]))
            {
                strings.Add(InstructionCase("float.erc"));
            }
            if (_randomGenerators.Contains(_generators[InstructionCase("integer.erc")]))
            {
                strings.Add(InstructionCase("integer.erc"));
            }
            return strings.OrderBy(x => x).Aggregate((current, next) => current + next);
            // strings.Sort();
            // foreach (string s in strings)
            // {
            //   str += s + " ";
            // }
            // return SharpenMinimal.Runtime.Substring(str, 0, str.Length - 1);
        }

        /// <summary>Returns the Instruction whose name is given in instr.</summary>
        /// <param name="instr"/>
        /// <returns>the Instruction or null if no such Instruction.</returns>
        public Instruction GetInstruction(string instr)
        {
            return _instructions[instr];
        }

        /// <summary>Returns the number of evaluation executions so far this run.</summary>
        /// <returns>The number of evaluation executions during this run.</returns>
        public long GetEvaluationExecutions()
        {
            return _evaluationExecutions;
        }

        public InputPusher GetInputPusher()
        {
            return _inputPusher;
        }

        public void SetInputPusher(InputPusher inputPusher)
        {
            _inputPusher = inputPusher;
        }

        /// <summary>
        /// Generates a single random Push atom (instruction name, integer, float,
        /// etc) for use in random code generation algorithms.
        /// </summary>
        /// <returns>
        /// A random atom based on the interpreter's current active
        /// instruction set.
        /// </returns>
        public object RandomAtom()
        {
            int index = Rng.Next(_randomGenerators.Count);
            try
            {
                return _randomGenerators[index].Generate(this);
            }
            catch (Exception e)
            {
                throw new Exception("got bad generator for index " + index + " rg " + _randomGenerators[index], e);
            }
        }

        /// <summary>Generates a random Push program of a given size.</summary>
        /// <param name="inSize">The requested size for the program to be generated.</param>
        /// <returns>A random Push program of the given size.</returns>
        public Program RandomCode(int inSize)
        {
            Program p = new Program();
            IList<int> distribution = RandomCodeDistribution(inSize - 1, inSize - 1);
            foreach (int count in distribution)
            {
                p.Push(count == 1 ? RandomAtom() : RandomCode(count));
            }
            return p;
        }

        /// <summary>
        /// Generates a list specifying a size distribution to be used for random
        /// code.
        /// </summary>
        /// <remarks>
        /// Generates a list specifying a size distribution to be used for random
        /// code.
        /// Note: This method is called "decompose" in the lisp implementation.
        /// </remarks>
        /// <param name="inCount">The desired resulting program size.</param>
        /// <param name="inMaxElements">The maxmimum number of elements at this level.</param>
        /// <returns>A list of integers representing the size distribution.</returns>
        public IList<int> RandomCodeDistribution(int inCount, int inMaxElements)
        {
            List<int> result = new List<int>();
            RandomCodeDistribution(result, inCount, inMaxElements);
            Shuffle(result);
            return result;
        }

        // Fisher-Yates-Durstenfeld shuffle
        //$ cite -ma 1653204 -t excerpt -u \
        // http://stackoverflow.com/questions/1651619/optimal-linq-query-to-get-a-random-sub-collection-shuffle
        public static IEnumerable<T> Shuffle<T>(IEnumerable<T> source, Random rng = null)
        {
            if (rng == null)
                rng = new Random();
            var buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++)
            {
                int j = rng.Next(i, buffer.Count);
                yield return buffer[j];
                buffer[j] = buffer[i];
            }
        }

        /// <summary>The recursive worker function for the public RandomCodeDistribution.</summary>
        /// <param name="ioList">The working list of distribution values to append to.</param>
        /// <param name="inCount">The desired resulting program size.</param>
        /// <param name="inMaxElements">The maxmimum number of elements at this level.</param>
        private void RandomCodeDistribution(IList<int> ioList, int inCount, int inMaxElements)
        {
            if (inCount < 1)
            {
                return;
            }
            int thisSize = inCount < 2 ? 1 : (Rng.Next(inCount) + 1);
            ioList.Add(thisSize);
            RandomCodeDistribution(ioList, inCount - thisSize, inMaxElements - 1);
        }

        public interface AtomGenerator
        {
            object Generate(Interpreter inInterpreter);
        }

        private class InstructionAtomGenerator : Interpreter.AtomGenerator
        {

            internal string _instruction;

            internal InstructionAtomGenerator(string inInstructionName)
            {
                this._instruction = inInstructionName;
            }

            public object Generate(Interpreter inInterpreter)
            {
                return this._instruction;
            }

        }

        private class FloatAtomGenerator : Interpreter.AtomGenerator
        {

            public object Generate(Interpreter inInterpreter)
            {
                float r = (float) inInterpreter.Rng.NextDouble() *
                          (inInterpreter._maxRandomFloat - inInterpreter._minRandomFloat);
                r -= (r % inInterpreter._randomFloatResolution);
                return r + inInterpreter._minRandomFloat;
            }

        }

        private class IntAtomGenerator : Interpreter.AtomGenerator
        {

            public object Generate(Interpreter inInterpreter)
            {
                int r = inInterpreter.Rng.Next(inInterpreter._maxRandomInt - inInterpreter._minRandomInt);
                r -= (r % inInterpreter._randomIntResolution);
                return r + inInterpreter._minRandomInt;
            }
        }
    }
}
