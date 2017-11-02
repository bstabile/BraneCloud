

using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.Problems.Lid.Func
{
    [ECConfiguration("ec.problems.lid.func.LidJ")]
    public class LidJ : GPNode
    {
        public override string ToString()
        {
            return "J";
        }

        public override int ExpectedChildren => 2;

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
            // No need to evaluate or look at children. Lid is only
            // about tree shape/size
        }
    }
}
