using System;
using System.Reflection;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Runtime;

namespace BraneCloud.Evolution.EC.App.OrderTree
{
    class Program
    {
        static void Main(string[] args)
        {
            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
            {
                Assembly.GetAssembly(typeof(IEvolutionState)),
                Assembly.GetAssembly(typeof(Evaluator)),
                Assembly.GetAssembly(typeof(OrderTree))
            });

            Evolve.Run(new[] { "-file", @"Params\App\OrderTree\ordertree.params" });
            Console.WriteLine("\nDone!");
            Console.ReadLine();
        }
    }
}
