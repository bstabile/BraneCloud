using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Runtime;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.App.Lid
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
                Assembly.GetAssembly(typeof(Lid))
            });

            Evolve.Run(new[] { "-file", @"Params\App\Lid\lid.params" });
            Console.WriteLine("\nDone!");
            Console.ReadLine();
        }
    }
}
