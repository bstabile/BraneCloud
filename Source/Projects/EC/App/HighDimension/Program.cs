using System;
using System.Reflection;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Runtime;

namespace BraneCloud.Evolution.EC.App.HighDimension
{
    class Program
    {
        static void Main(string[] args)
        {
            throw new NotImplementedException("DOvS is still under construction!");

            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
            {
                Assembly.GetAssembly(typeof(IEvolutionState)),
                Assembly.GetAssembly(typeof(HighDimension))
            });

            Evolve.Run(new[] { "-file", @"Params\App\HighDimension\highdimension.params" });
            Console.WriteLine("\nDone!");
            Console.ReadLine();
        }
    }
}
