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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC
{
    // TODO : Decide if this serves any purpose. Or is the interface enough? 
    // ECJ includes an abstract class with abstract methods.
    // That seems rather pointless also, because it has to be made concrete
    // before a client can do anything useful with the interface!

    [ECConfiguration("ec.Finisher")]
    public class Finisher : IFinisher 
    {
        public virtual void Setup(IEvolutionState state, IParameter parm) { /* Default implementation does nothing */ }
        public virtual void FinishPopulation(IEvolutionState state, int result) { /* Default implementation does nothing */ }
    }
}