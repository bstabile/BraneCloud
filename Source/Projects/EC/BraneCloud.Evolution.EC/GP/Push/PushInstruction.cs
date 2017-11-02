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
using BraneCloud.Evolution.EC.Psh;

namespace BraneCloud.Evolution.EC.GP.Push
{
    /// <summary>
    /// PushInstruction encapsulates a custom Push instruction.  This
    /// class requires that you implement a Psh method called <b><tt>Execute(...)</tt></b>.
    /// You will need to consult Psh to understand what you can do, and how to do it.  But
    /// for some examples, see the <b>Atan.java</b> and <b>Print.java</b> classes in
    /// <b>ec/app/push/</b>.
    ///       
    /// <p/>PushInstruction is a Prototype, so you may with also to override setup() 
    /// to set up your instruction initially.
    ///
    /// <p/><b>Default Base</b><br/>
    /// gp.push.func
    /// </summary>
    [ECConfiguration("ec.gp.push.PushInstruction")]
    public abstract class PushInstruction : Instruction, IPrototype
    {
        public const string P_INSTRUCTION = "func";

        public IParameter DefaultBase => PushDefaults.ParamBase.Push(P_INSTRUCTION);


        public void Setup(IEvolutionState state, IParameter paramBase)
        {
        }

        public override Object Clone()
        {
            // Instruction.Clone() does a shallow MemberwiseClone()
            PushInstruction myobj = (PushInstruction) base.Clone();
            return myobj;
        }

        //public abstract void Execute(Interpreter interpreter);
    }
}