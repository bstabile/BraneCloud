using System;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP.Push;
using BraneCloud.Evolution.EC.Psh;

namespace BraneCloud.Evolution.EC.Problems.Push
{
    [ECConfiguration("ec.problems.push.Print")]
    public class Print : PushInstruction
    {
        public override void Execute(Interpreter interpeter)
        {
            FloatStack stack = interpeter.FloatStack();

            if (stack.Size() > 0)
            {
                Console.Error.WriteLine(stack.Top());
            }
            else Console.Error.WriteLine("empty");
        }
    }
}