using System;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP.Push;
using BraneCloud.Evolution.EC.Psh;

namespace BraneCloud.Evolution.EC.Problems.Push
{
    [ECConfiguration("ec.problems.push.Atan")]
    public class Atan : PushInstruction
    {
        public override void Execute(Interpreter interpeter)
        {
            FloatStack stack = interpeter.FloatStack();

            if (stack.Size() >= 1)
            {
                // we're good
                float slope = stack.Pop();
                stack.Push((float) Math.Atan(slope));
            }
            else stack.Push(0);
        }
    }
}