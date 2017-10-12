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
using System.Reflection;
using System.Text;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.Async
{
    class Program
    {
        static void Main(string[] args)
        {
            // NOTE: This applicatin seems to be missing AsynchronousEvolutionState (set in asesum.params).

            //// This primes the activator so it knows where to look for types that will be created from parameters.
            //ECActivator.AddSourceAssemblies(new[] { Assembly.GetAssembly(typeof(IEvolutionState)), Assembly.GetAssembly(typeof(Evaluator)) });
            //// Here we are starting up with ant.params
            //// But this can also be started antge.params (which presumably does generational evolution; need to check)
            //Evolve.Run(new[] { "-file", @"Params\App\Async\asesum.params" });
            //Console.WriteLine("\nDone!");
            //Console.ReadLine();
        }
    }
}