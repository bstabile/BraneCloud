using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Runtime;

namespace BraneCloud.Evolution.EC.App.Xor
{
    class Program
    {
        static void Main(string[] args)
        {
            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
            {
                Assembly.GetAssembly(typeof(IEvolutionState)),
                Assembly.GetAssembly(typeof(XOR))
            });

            Evolve.Run(new[] { "-file", @"Params\App\Xor\xor.params" });
            Console.WriteLine("\nDone!");
            Console.ReadLine();
        }
    }
}
