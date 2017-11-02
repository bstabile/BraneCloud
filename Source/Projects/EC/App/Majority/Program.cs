﻿using System;
using System.Reflection;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Runtime;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.App.Majority
{
    class Program
    {
        static void Main(string[] args)
        {
            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
            {
                Assembly.GetAssembly(typeof(EvolutionState)),
                Assembly.GetAssembly(typeof(SimpleEvaluator)),
                Assembly.GetAssembly(typeof(MajorityGP))
            });

            Evolve.Run(new[] { "-file", @"Params\App\Majority\majorityga.params" });
            Console.WriteLine("\nDone!");
            Console.ReadLine();
        }
    }
}
