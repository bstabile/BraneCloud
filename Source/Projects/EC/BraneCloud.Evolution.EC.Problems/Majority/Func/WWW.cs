using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.Problems.Majority.Func
{
    [ECConfiguration("ec.problems.majority.func.WWW")]
    public class WWW : GPNode
    {
        public override string ToString()
        {
            return "www";
        }

        public override int ExpectedChildren => 0;

        public static long X0;
        public static long X1;

        static WWW()
        {
            for (int i = 0; i < 64; i++)
            {
                long val = (i >> 6) & 0x1; // west west west element
                X0 = X0 | (val << i);
            }

            for (int i = 64; i < 128; i++)
            {
                long val = (i >> 6) & 0x1; // west west west element
                X1 = X1 | (val << (i - 64));
            }
        }

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
            var md = (MajorityData) input;
            md.Data0 = X0;
            md.Data1 = X1;
        }
    }
}


