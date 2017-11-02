using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.App.Majority.Func
{

    [ECConfiguration("ec.app.majority.func.EE")]
    public class EE : GPNode
    {
        public override string ToString()
        {
            return "ee";
        }

        public override int ExpectedChildren => 0;

        public static long X0;
        public static long X1;

        static EE()
        {
            for (int i = 0; i < 64; i++)
            {
                long val = (i >> 1) & 0x1; // east east element
                X0 = X0 | (val << i);
            }

            for (int i = 64; i < 128; i++)
            {
                long val = (i >> 1) & 0x1; // east east element
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


