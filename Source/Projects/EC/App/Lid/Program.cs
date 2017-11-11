using System;
using System.Reflection;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Runtime;

namespace BraneCloud.Evolution.EC.App.Lid
{
    class Program
    {
        static void Main(string[] args)
        {
            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
            {
                Assembly.GetAssembly(typeof(IEvolutionState)),
                Assembly.GetAssembly(typeof(Lid))
            });

            Evolve.Run(new[] { "-file", @"Params\App\Lid\lid.params" });
            Console.WriteLine("\nDone!");
            Console.ReadLine();
        }
    }
}
