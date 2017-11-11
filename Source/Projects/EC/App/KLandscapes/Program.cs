using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Runtime;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.App.KLandscapes
{
    class Program
    {
        static void Main(string[] args)
        {
            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
            {
                Assembly.GetAssembly(typeof(IEvolutionState)),
                Assembly.GetAssembly(typeof(KLandscapes))
            });

            Evolve.Run(new[] { "-file", @"Params\App\KLandscapes\klandscapes.params" });
            Console.WriteLine("\nDone!");
            Console.ReadLine();
        }
    }
}
