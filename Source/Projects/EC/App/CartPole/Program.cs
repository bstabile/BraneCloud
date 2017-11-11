using System;
using System.Reflection;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Runtime;

namespace BraneCloud.Evolution.EC.App.CartPole
{
    class Program
    {
        static void Main(string[] args)
        {
            // This primes the activator so it knows where to look for types that will be created from parameters.
            ECActivator.AddSourceAssemblies(new[]
            {
                Assembly.GetAssembly(typeof (IEvolutionState)),
                Assembly.GetAssembly(typeof (CartPole))
            });

            // Here we are starting up with ant.params
            // But this can also be started with antge.params (which does generational evolution)
            Evolve.Run(new[] { "-file", @"Params/App/CartPole/cartpole.params" });
            Console.WriteLine("\nDone!");
            Console.ReadLine();
        }
    }
}
