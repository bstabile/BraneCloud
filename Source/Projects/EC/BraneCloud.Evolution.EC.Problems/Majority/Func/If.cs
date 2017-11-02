using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.Problems.Majority.Func
{
    [ECConfiguration("ec.problems.majority.func.If")]
    public class If : GPNode
    {
        public override string ToString()
        {
            return "if";
        }

        public override int ExpectedChildren => 3;

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
            Children[0].Eval(state, thread, input, stack, individual, problem);

            var md = (MajorityData) input;
            long y0 = md.Data0;
            long y1 = md.Data1;

            Children[1].Eval(state, thread, input, stack, individual, problem);
            long z0 = md.Data0;
            long z1 = md.Data1;

            Children[2].Eval(state, thread, input, stack, individual, problem);

            // IF Y THEN Z ELSE MD is
            // (Y -> Z) ^ (~Y -> MD)
            // (!Y v Z) ^ (Y v MD)
            md.Data0 = (~y0 | z0) & (y0 | md.Data0);
            md.Data1 = (~y1 | z1) & (y1 | md.Data1);
        }
    }
}


