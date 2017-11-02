using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.App.Majority.Func
{
    [ECConfiguration("ec.app.majority.func.Not")]
    public class Not : GPNode
    {
        public override string ToString()
        {
            return "not";
        }

        public override int ExpectedChildren => 1;

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
            Children[0].Eval(state, thread, input, stack, individual, problem);

            var md = (MajorityData) input;
            md.Data0 = ~(md.Data0);
            md.Data1 = ~(md.Data1);
        }
    }
}


